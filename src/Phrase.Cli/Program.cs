using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;

const string Usage = "Usage: phrase build FILE.phrase -o OUT_DIR [--target csharp]\n       phrase trace NAME [--map PATH] [--assembly PATH] [--strict]";
if (args.Length > 0 && args[0] == "trace")
    return Trace(args[1..]);
if (!ParseBuild(args, out var file, out var output))
{
    Console.Error.WriteLine(Usage);
    return 2;
}
if (!File.Exists(file))
{
    Console.Error.WriteLine($"No such file: {file}");
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
    Console.Error.WriteLine(exception.Message);
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
    File.WriteAllText(Path.Combine(root, module.Name + ".phrasemap.json"), json, new UTF8Encoding(false));
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
        foreach (var failure in phrase.DeclaredFailures)
        foreach (var endpoint in module.Endpoints.Where(endpoint => endpoint.Failures.Contains(failure.Name, StringComparer.Ordinal)))
        {
            var failureStep = endpoint.Steps.FirstOrDefault(candidate => Failures(module, candidate).Contains(failure.Name, StringComparer.Ordinal));
            var failureSpec = endpoint.Specs.FirstOrDefault(candidate => candidate.Outcome is FailsModel fails && fails.Code == failure.Name);
            cs = output.EndpointFiles[endpoint.Name];
            yield return Record(output, module, "failure", failure.Name, failure.Line, endpoint, failureStep, failureSpec, cs);
        }
    }
}

static EndpointSpecModel? SpecFor(EndpointModel endpoint, BoundStep step) =>
    endpoint.Specs.FirstOrDefault(spec => spec.Outcome is FailsModel fails && StepFails(step, fails.Code))
    ?? endpoint.Specs.FirstOrDefault();
static bool StepFails(BoundStep step, string code) => step is BoundFailStep fail && fail.Code == code || step is BoundEnsureStep ensure && ensure.Code == code;
static MapLines Lines(SourceSpan? span) => new(span?.FirstLine ?? 0, span?.LastLine ?? 0);
static MapRecord Record(CompilationOutput output, ModuleModel module, string kind, string name, int line, EndpointModel endpoint, BoundStep? step, EndpointSpecModel? spec, string cs) =>
    new(StepId(kind, line, name), kind, name, module.PhraseFile, line, endpoint.Name, step?.Source ?? "", Lines(spec?.SourceSpan), cs, $"{endpoint.Name}Handler.Handle", Hash(output.Files[cs]));
static string StepId(string kind, int line, string name) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{kind}\n{line}\n{name.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n')}")))[..12].ToLowerInvariant();

static int Trace(string[] values)
{
    try
    {
        return TraceCore(values);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or BadImageFormatException or FileLoadException)
    {
        Console.Error.WriteLine($"Could not trace: {exception.Message}");
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
        Console.Error.WriteLine("Source map not found; pass --map PATH.");
        return 2;
    }
    var map = JsonSerializer.Deserialize<MapFile>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (map?.Records is null)
    {
        Console.Error.WriteLine("Source map is malformed.");
        return 2;
    }
    var found = map.Records.Where(record => record.Name == name || record.CsMethod == name).ToList();
    if (found.Count == 0)
    {
        Console.Error.WriteLine($"Unknown trace name or frame: {name}");
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
    var id = Assembly.LoadFrom(Path.GetFullPath(path)).GetCustomAttributes<AssemblyMetadataAttribute>()
        .FirstOrDefault(attribute => attribute.Key == "PhraseBuildId")?.Value;
    var mismatch = id != map.BuildId;
    check = mismatch ? "WARNING: map does not match assembly" : "map matches assembly";
    return mismatch;
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
static IReadOnlyList<string> Failures(ModuleModel module, BoundStep step) => step switch
{
    BoundCallStep call => module.Phrase(call.Call.PhraseKey).Failures,
    BoundLetStep let => module.Phrase(let.Call.PhraseKey).Failures,
    BoundFailStep fail => [fail.Code],
    BoundEnsureStep ensure => [ensure.Code],
    _ => [],
};
static string MethodName(PhraseModel phrase) => string.Concat(phrase.Parts.Where(part => !part.IsHole).Select(part => char.ToUpperInvariant(part.Text[0]) + part.Text[1..]));
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n')))).ToLowerInvariant();

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
