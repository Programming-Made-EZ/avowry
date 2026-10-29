using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;

const string Usage = "Usage: avowry build FILE.phrase -o OUT_DIR [--target csharp]\n       avowry trace NAME [--map PATH] [--assembly PATH] [--strict]";
if (args is ["--help"])
{
    Console.WriteLine(Usage);
    return 0;
}
if (args is ["--version"])
{
    Console.WriteLine($"Avowry {Assembly.GetExecutingAssembly().GetName().Version}");
    return 0;
}
if (args.Length > 0 && args[0] == "trace")
    return Trace(args[1..]);
if (!ParseBuild(args, out var file, out var output))
{
    Console.Error.WriteLine(Usage);
    return 2;
}
if (!File.Exists(file))
{
    Error($"No such file: {file}");
    return 2;
}
try
{
    var compiled = PhraseCompiler.Compile(File.ReadAllText(file), new CSharpProvider(), file);
    compiled.WriteTo(output);
    WriteMap(compiled, output);
    return 0;
}
catch (CompilationFailedException exception)
{
    Error(exception.Message);
    return 1;
}

static void WriteMap(CompilationOutput output, string root)
{
    if (output.Module is not { } module)
        throw new InvalidOperationException("The adapter did not return a module for the source map.");
    var records = Records(output, module).OrderBy(record => record.Kind, StringComparer.Ordinal)
        .ThenBy(record => record.Name, StringComparer.Ordinal)
        .ThenBy(record => record.Endpoint, StringComparer.Ordinal)
        .ThenBy(record => record.Line).ToList();
    var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    var json = JsonSerializer.Serialize(new MapFile(1, module.BuildId, records), options)
        .Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    File.WriteAllText(Path.Combine(root, module.Name + ".avowrymap.json"), json, new UTF8Encoding(false));
}

static IEnumerable<MapRecord> Records(CompilationOutput output, ModuleModel module)
{
    foreach (var endpoint in module.Endpoints)
    {
        var cs = output.EndpointFiles[endpoint.Name];
        yield return Record(output, module, "endpoint", endpoint.Name, endpoint.Line, endpoint, null, endpoint.Specs.FirstOrDefault(), cs);
        foreach (var step in endpoint.Steps)
            yield return Record(output, module, "step", step.Source, step.Line, endpoint, step, SpecFor(endpoint, step), cs);
    }
    foreach (var phrase in module.Phrases)
    {
        var owner = module.Endpoints.FirstOrDefault(endpoint => endpoint.Steps.Any(step => Calls(step, phrase.Key)));
        var step = owner?.Steps.FirstOrDefault(candidate => Calls(candidate, phrase.Key));
        var cs = output.PhraseFiles[phrase.Key];
        var spec = phrase.Specs.FirstOrDefault();
        yield return new(StepId("phrase", phrase.Line, phrase.Template), "phrase", phrase.Template, module.PhraseFile,
            phrase.Line, owner?.Name ?? "", step?.Source ?? "", Lines(spec?.SourceSpan), cs,
            $"Phrases.{MethodName(phrase)}", Hash(output.Files[cs]));
    }
    foreach (var endpoint in module.Endpoints)
    foreach (var failure in endpoint.Failures.Distinct(StringComparer.Ordinal))
    {
        var failureStep = endpoint.Steps.FirstOrDefault(candidate => FailureDeclaration(module, candidate, failure) is not null);
        var declaration = failureStep is null ? null : FailureDeclaration(module, failureStep, failure);
        if (declaration is not null)
        {
            var failureSpec = endpoint.Specs.FirstOrDefault(candidate => candidate.Outcome is FailsModel fails && fails.Code == failure);
            var cs = output.EndpointFiles[endpoint.Name];
            yield return Record(output, module, "failure", failure, declaration.Line, endpoint, failureStep, failureSpec, cs);
        }
    }
}

static EndpointSpecModel? SpecFor(EndpointModel endpoint, BoundStep step) =>
    endpoint.Specs.FirstOrDefault(spec => spec.Outcome is FailsModel fails && StepFails(step, fails.Code))
    ?? endpoint.Specs.FirstOrDefault();
static bool StepFails(BoundStep step, string code) => step is BoundFailStep fail && fail.Code == code || step is BoundEnsureStep ensure && ensure.Code == code;
static MapLines Lines(SourceSpan? span) => new(span?.FirstLine ?? 0, span?.LastLine ?? 0);
static MapRecord Record(CompilationOutput output, ModuleModel module, string kind, string name, int line, EndpointModel endpoint, BoundStep? step, EndpointSpecModel? spec, string cs) =>
    new(StepId(kind, line, name, endpoint.Name), kind, name, module.PhraseFile, line, endpoint.Name, step?.Source ?? "", Lines(spec?.SourceSpan), cs, $"{endpoint.Name}Handler.Handle", Hash(output.Files[cs]));
static string StepId(string kind, int line, string name, string endpoint = "")
{
    var normalizedName = name.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
    var input = $"{kind}\n{line}\n{normalizedName}" + (kind == "failure" ? $"\n{endpoint}" : "");
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)))[..12].ToLowerInvariant();
}

static int Trace(string[] values)
{
    try
    {
        return TraceCore(values);
    }
    catch (JsonException exception)
    {
        Error($"Could not trace: invalid JSON source map: {exception.Message}");
        return 2;
    }
    catch (BadImageFormatException exception)
    {
        Error($"Could not trace: assembly is not valid: {exception.Message}");
        return 2;
    }
    catch (Exception exception)
    {
        Error($"Could not trace: {exception.Message}");
        return 2;
    }
}

static int TraceCore(string[] values)
{
    if (!ParseTrace(values, out var name, out var path, out var assemblyPath, out var strict))
    {
        Console.Error.WriteLine(Usage);
        return 2;
    }
    if (path is null || !File.Exists(path))
    {
        Error("Source map not found; pass --map PATH.");
        return 2;
    }
    var parsed = JsonSerializer.Deserialize<MapFile>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    var problem = Validate(parsed);
    if (problem is not null)
    {
        Error($"Source map is malformed: {problem}");
        return 2;
    }
    var map = parsed!;
    var found = map.Records.Where(record => record.Name == name || record.CsMethod == name).ToList();
    if (found.Count == 0)
    {
        Error($"Unknown trace name or frame: {name}");
        return 1;
    }
    var mismatch = CheckAssembly(map, assemblyPath, out var check);
    foreach (var record in found)
    {
        var hashMismatch = CheckHash(path, record);
        mismatch |= hashMismatch;
        Print(map, record, hashMismatch ? "WARNING: generated C# hash does not match map" : check);
    }
    return mismatch && strict ? 1 : 0;
}

static bool CheckAssembly(MapFile map, string? path, out string check)
{
    check = "map not checked against assembly";
    if (path is null)
        return false;
    if (!File.Exists(path))
        throw new FileNotFoundException("Assembly not found.", path);
    var id = AssemblyBuildId(path);
    var mismatch = id != map.BuildId;
    check = mismatch ? "WARNING: map does not match assembly" : "map matches assembly";
    return mismatch;
}

static string? AssemblyBuildId(string path)
{
    using var stream = File.OpenRead(path);
    using var pe = new PEReader(stream);
    if (!pe.HasMetadata)
        throw new BadImageFormatException("The assembly has no .NET metadata.", path);
    var metadata = pe.GetMetadataReader();
    foreach (var handle in metadata.GetAssemblyDefinition().GetCustomAttributes())
    {
        var attribute = metadata.GetCustomAttribute(handle);
        if (!IsAssemblyMetadataAttribute(metadata, attribute.Constructor))
            continue;
        var value = metadata.GetBlobReader(attribute.Value);
        if (value.ReadUInt16() != 1)
            throw new BadImageFormatException("The assembly metadata is malformed.", path);
        if (value.ReadSerializedString() == "PhraseBuildId")
            return value.ReadSerializedString();
    }
    return null;
}

static bool IsAssemblyMetadataAttribute(MetadataReader metadata, EntityHandle constructor)
{
    if (constructor.Kind != HandleKind.MemberReference)
        return false;
    var member = metadata.GetMemberReference((MemberReferenceHandle)constructor);
    if (member.Parent.Kind != HandleKind.TypeReference)
        return false;
    var type = metadata.GetTypeReference((TypeReferenceHandle)member.Parent);
    return metadata.GetString(type.Namespace) == "System.Reflection"
        && metadata.GetString(type.Name) == "AssemblyMetadataAttribute";
}

static string? Validate(MapFile? map)
{
    if (map is null)
        return "the JSON value is null";
    if (map.Records is null)
        return "records is missing";
    for (var index = 0; index < map.Records.Count; index++)
    {
        var record = map.Records[index];
        if (record is null)
            return $"records[{index}] is null";
        if (record.SpecLines is null)
            return $"records[{index}].specLines is missing";
        if (string.IsNullOrWhiteSpace(record.CsFile))
            return $"records[{index}].csFile is missing";
    }
    return null;
}

static bool CheckHash(string mapPath, MapRecord record)
{
    var root = Path.GetDirectoryName(Path.GetFullPath(mapPath))!;
    var path = Path.Combine(root, record.CsFile.Replace('/', Path.DirectorySeparatorChar));
    return File.Exists(path) && Hash(File.ReadAllText(path)) != record.CsHash;
}

static void Print(MapFile map, MapRecord record, string check)
{
    Console.WriteLine($"{record.Name}  {record.Kind}{(record.Kind == "failure" ? "  422" : "")}  stepId {record.StepId}");
    Console.WriteLine($"  {record.PhraseFile}:{record.Line} endpoint {record.Endpoint}, step {record.Step}");
    Console.WriteLine($"  spec: {record.PhraseFile}:{record.SpecLines.First}-{record.SpecLines.Last}");
    Console.WriteLine($"  C#: {record.CsFile}  {record.CsMethod}");
    Console.WriteLine($"  build: {map.BuildId} ({check})");
}

static bool ParseTrace(string[] values, out string name, out string? path, out string? assemblyPath, out bool strict)
{
    name = values.Length > 0 ? values[0] : "";
    path = null;
    assemblyPath = null;
    strict = false;
    if (values.Length == 0)
        return false;
    for (var i = 1; i < values.Length; i++)
    {
        if (values[i] == "--map" && i + 1 < values.Length)
            path = values[++i];
        else if (values[i] == "--assembly" && i + 1 < values.Length)
            assemblyPath = values[++i];
        else if (values[i] == "--strict")
            strict = true;
        else
            return false;
    }
    return true;
}

static bool Calls(BoundStep step, string key) => step is BoundCallStep call && call.Call.PhraseKey == key || step is BoundLetStep let && let.Call.PhraseKey == key;
static DeclaredFailureModel? FailureDeclaration(ModuleModel module, BoundStep step, string failure) => step switch
{
    BoundCallStep call => PhraseFailureDeclaration(module, module.Phrase(call.Call.PhraseKey), failure),
    BoundLetStep let => PhraseFailureDeclaration(module, module.Phrase(let.Call.PhraseKey), failure),
    _ => null,
};
static DeclaredFailureModel? PhraseFailureDeclaration(ModuleModel module, PhraseModel phrase, string failure) =>
    phrase.DeclaredFailures.FirstOrDefault(candidate => candidate.Name == failure)
    ?? phrase.Steps?.Select(step => FailureDeclaration(module, step, failure)).FirstOrDefault(declaration => declaration is not null);
static string MethodName(PhraseModel phrase) => string.Concat(phrase.Parts.Where(part => !part.IsHole).Select(part => char.ToUpperInvariant(part.Text[0]) + part.Text[1..]));
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n')))).ToLowerInvariant();
static void Error(string message) => Console.Error.WriteLine($"avowry: {message}");

static bool ParseBuild(string[] values, out string file, out string output)
{
    file = "";
    output = "";
    if (values.Length == 0 || values[0] != "build")
        return false;
    for (var i = 1; i < values.Length; i++)
    {
        if (values[i] == "-o" && i + 1 < values.Length)
            output = values[++i];
        else if (values[i] == "--target" && i + 1 < values.Length && values[++i] == "csharp")
        {
        }
        else if (!values[i].StartsWith('-') && file.Length == 0)
            file = values[i];
        else
            return false;
    }
    return file.Length > 0 && output.Length > 0;
}

sealed record MapFile(int SchemaVersion, string BuildId, IReadOnlyList<MapRecord> Records);
sealed record MapRecord(string StepId, string Kind, string Name, string PhraseFile, int Line, string Endpoint, string Step, MapLines SpecLines, string CsFile, string CsMethod, string CsHash);
sealed record MapLines(int First, int Last);
