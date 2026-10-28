using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;

const string Usage = "Usage: phrase build FILE.phrase -o OUT_DIR [--target csharp]\n       phrase trace NAME [--map PATH] [--assembly PATH] [--strict]";
if (args.Length > 0 && args[0] == "trace") return Trace(args[1..]);
if (!ParseBuild(args, out var file, out var output)) { Console.Error.WriteLine(Usage); return 2; }
if (!File.Exists(file)) { Console.Error.WriteLine($"No such file: {file}"); return 2; }
try
{
    var compiled = PhraseCompiler.Compile(File.ReadAllText(file), new CSharpProvider(), file);
    compiled.WriteTo(output);
    WriteMap(compiled, output);
    return 0;
}
catch (CompilationFailedException exception) { Console.Error.WriteLine(exception.Message); return 1; }

static void WriteMap(CompilationOutput output, string root)
{
    var module = output.Module!;
    var records = new List<MapRecord>();
    foreach (var endpoint in module.Endpoints)
    {
        var cs = $"{module.Name}.Api/{endpoint.Name}.cs.cs";
        records.Add(Record("endpoint", endpoint.Name, endpoint.Line, endpoint, endpoint.Steps.FirstOrDefault(), endpoint.Specs.FirstOrDefault(), cs, $"{endpoint.Name}Handler.Handle", output));
    }
    foreach (var phrase in module.Phrases)
    {
        var owner = module.Endpoints.FirstOrDefault(endpoint => endpoint.Steps.Any(step => Calls(step, phrase.Key)));
        var step = owner?.Steps.FirstOrDefault(candidate => Calls(candidate, phrase.Key));
        var spec = phrase.Specs.FirstOrDefault();
        records.Add(new("phrase", phrase.Template, module.PhraseFile, phrase.Line, owner?.Name ?? "", step?.Source ?? "", new(spec?.SourceSpan.FirstLine ?? 0, spec?.SourceSpan.LastLine ?? 0), $"{module.Name}.Api/Phrases.cs", $"Phrases.{MethodName(phrase)}", Hash(output.Files[$"{module.Name}.Api/Phrases.cs"])));
        foreach (var failure in phrase.DeclaredFailures)
            foreach (var endpoint in module.Endpoints.Where(endpoint => endpoint.Failures.Contains(failure.Name, StringComparer.Ordinal)))
            {
                var failureStep = endpoint.Steps.FirstOrDefault(candidate => Failures(module, candidate).Contains(failure.Name, StringComparer.Ordinal));
                var failureSpec = endpoint.Specs.FirstOrDefault(candidate => candidate.Outcome is FailsModel fails && fails.Code == failure.Name);
                records.Add(Record("failure", failure.Name, failure.Line, endpoint, failureStep, failureSpec, $"{module.Name}.Api/{endpoint.Name}.cs.cs", $"{endpoint.Name}Handler.Handle", output));
            }
    }
    var map = new MapFile(1, module.BuildId, records.OrderBy(r => r.Kind).ThenBy(r => r.Name).ThenBy(r => r.Endpoint).ToList());
    var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllText(Path.Combine(root, module.Name + ".phrasemap.json"), json.Replace("\r\n", "\n", StringComparison.Ordinal) + "\n", new UTF8Encoding(false));
}

static MapRecord Record(string kind, string name, int line, EndpointModel endpoint, BoundStep? step, EndpointSpecModel? spec, string cs, string method, CompilationOutput output) =>
    new(kind, name, output.Module!.PhraseFile, line, endpoint.Name, step?.Source ?? "", new(spec?.SourceSpan.FirstLine ?? 0, spec?.SourceSpan.LastLine ?? 0), cs, method, Hash(output.Files[cs]));

static int Trace(string[] values)
{
    if (values.Length == 0) { Console.Error.WriteLine(Usage); return 2; }
    var name = values[0]; string? path = null; string? assemblyPath = null; var strict = false;
    for (var i = 1; i < values.Length; i++)
        if (values[i] == "--map" && i + 1 < values.Length) path = values[++i];
        else if (values[i] == "--assembly" && i + 1 < values.Length) assemblyPath = values[++i];
        else if (values[i] == "--strict") strict = true;
        else { Console.Error.WriteLine(Usage); return 2; }
    if (path is null || !File.Exists(path)) { Console.Error.WriteLine("Source map not found; pass --map PATH."); return 2; }
    var map = JsonSerializer.Deserialize<MapFile>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    var found = map.Records.FirstOrDefault(r => r.Name == name || r.CsMethod == name);
    if (found is null) { Console.Error.WriteLine($"Unknown trace name or frame: {name}"); return 1; }
    var mismatch = false; var check = "map not checked against assembly";
    if (assemblyPath is not null)
    {
        var id = Assembly.LoadFrom(Path.GetFullPath(assemblyPath)).GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "PhraseBuildId")?.Value;
        mismatch = id != map.BuildId; check = mismatch ? "WARNING: map does not match assembly" : "map matches assembly";
    }
    var csPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, found.CsFile.Replace('/', Path.DirectorySeparatorChar));
    if (File.Exists(csPath) && Hash(File.ReadAllText(csPath)) != found.CsHash) { mismatch = true; check = "WARNING: generated C# hash does not match map"; }
    Console.WriteLine($"{found.Name}  {found.Kind}{(found.Kind == "failure" ? "  422" : "")}");
    Console.WriteLine($"  {found.PhraseFile}:{found.Line} endpoint {found.Endpoint}, step {found.Step}");
    Console.WriteLine($"  spec: {found.PhraseFile}:{found.SpecLines.First}-{found.SpecLines.Last}");
    Console.WriteLine($"  C#: {found.CsFile}  {found.CsMethod}");
    Console.WriteLine($"  build: {map.BuildId} ({check})");
    return mismatch && strict ? 1 : 0;
}

static bool Calls(BoundStep step, string key) => step is BoundCallStep c && c.Call.PhraseKey == key || step is BoundLetStep l && l.Call.PhraseKey == key;
static IReadOnlyList<string> Failures(ModuleModel module, BoundStep step) => step switch { BoundCallStep c => module.Phrase(c.Call.PhraseKey).Failures, BoundLetStep l => module.Phrase(l.Call.PhraseKey).Failures, BoundFailStep f => [f.Code], BoundEnsureStep e => [e.Code], _ => [] };
static string MethodName(PhraseModel phrase) => string.Concat(phrase.Parts.Where(p => !p.IsHole).Select(p => char.ToUpperInvariant(p.Text[0]) + p.Text[1..]));
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal)))).ToLowerInvariant();
static bool ParseBuild(string[] values, out string file, out string output)
{
    file = ""; output = ""; if (values.Length == 0 || values[0] != "build") return false;
    for (var i = 1; i < values.Length; i++) if (values[i] == "-o" && i + 1 < values.Length) output = values[++i]; else if (values[i] == "--target" && i + 1 < values.Length && values[++i] == "csharp") { } else if (!values[i].StartsWith('-') && file.Length == 0) file = values[i]; else return false;
    return file.Length > 0 && output.Length > 0;
}

sealed record MapFile(int SchemaVersion, string BuildId, IReadOnlyList<MapRecord> Records);
sealed record MapRecord(string Kind, string Name, string PhraseFile, int Line, string Endpoint, string Step, MapLines SpecLines, string CsFile, string CsMethod, string CsHash);
sealed record MapLines(int First, int Last);
