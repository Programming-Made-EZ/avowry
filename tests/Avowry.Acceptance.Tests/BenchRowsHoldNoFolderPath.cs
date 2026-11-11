using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

[Collection("Bench processes")] // a test class that starts an external process under a hard time limit never overlaps another such class (D247)
public sealed class BenchRowsHoldNoFolderPath
{
    private const string ShortForm = "Candidate.cs(7,1): error CS1529: planted [Verification.csproj]";
    private const string PassingPhrase = """
        module Greeting

        avow to "say hello"
          native csharp
            return;
        """;
    private const string PassingCSharp = """
        // FILE: Greeter.cs
        namespace Greeting; public static class Greeter { public static string SayHello() => "hello"; }
        // FILE: GreeterTests.cs
        using Greeting;
        public sealed class GreeterTests { [Xunit.Fact] public void Greeter_returns_hello() => Xunit.Assert.Equal("hello", Greeter.SayHello()); }
        """;
    private const string BrokenCSharp = "namespace Greeting; public class Broken { [Xunit.Fact] public void test() { nope } }";
    private const string OneCase = "[{\"name\":\"returns hello\",\"given\":{},\"when\":\"called\",\"then\":{\"returns\":\"hello\"}}]";
    private const string EndpointOutcomes = "[{\"name\":\"greets Ada\",\"given\":{\"name\":\"Ada\"},\"when\":\"called\",\"then\":{\"responds\":{\"message\":\"hello\",\"name\":\"Ada\"},\"status\":200}},{\"name\":\"rejects blank name\",\"given\":{\"name\":\"\"},\"when\":\"called\",\"then\":{\"fails_with\":\"MissingName\"}}]";
    private const string CorrectEndpointReply = """
        using FastEndpoints;
        namespace Greetings;
        public sealed record GreetingRequest(string Name);
        public sealed record GreetingResponse(string Message, string Name);
        public sealed class GreetingsEndpoint : Endpoint<GreetingRequest, GreetingResponse>
        {
            public override void Configure() => Post("/greetings");
            public override async Task HandleAsync(GreetingRequest req, CancellationToken ct)
            {
                if (string.IsNullOrWhiteSpace(req.Name))
                {
                    AddError("MissingName");
                    await Send.ErrorsAsync(422, ct);
                    return;
                }

                await Send.OkAsync(new GreetingResponse("hello", req.Name), ct);
            }
        }
        """;
    private const string PhraseEndpointCorrect = """
        module Greetings

        avow to "ensure {name: Text} is present"
          fails with MissingName
          spec "rejects blank name"
            given name = ""
            when called
            then fails with MissingName
          native csharp
            if (string.IsNullOrWhiteSpace(name)) Fail("MissingName");

        endpoint Greetings : POST /greetings
          input
            name: Text
          output
            message: Text
            name: Text
          steps
            ensure input.name is present
            respond with { message: "hello", name: input.name }

          spec "greets Ada"
            given input { name: "Ada" }
            when handled
            then responds { message: "hello", name: "Ada" }
        """;
    private const string PhraseEndpointBroken = """
        module Greetings

        avow to "ensure {name: Text} is present"
          fails with MissingName
          spec "rejects blank name"
            given name = ""
            when called
            then fails with MissingName
          native csharp
            if (nope) Fail("MissingName");

        endpoint Greetings : POST /greetings
          input
            name: Text
          output
            message: Text
            name: Text
          steps
            ensure input.name is present
            respond with { message: "hello", name: input.name }

          spec "greets Ada"
            given input { name: "Ada" }
            when handled
            then responds { message: "hello", name: "Ada" }
        """;
    private const string PassingTrx = """
        <?xml version="1.0" encoding="utf-8"?>
        <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
          <Results>
            <UnitTestResult testName="GeneratedOutcomeTests.x" outcome="Passed" />
            <UnitTestResult testName="GreeterTests.Greeter_returns_hello" outcome="Passed" />
          </Results>
        </TestRun>
        """;
    private const string EmptyTrx = """
        <?xml version="1.0" encoding="utf-8"?>
        <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results /></TestRun>
        """;

    private static readonly HashSet<string> GenericAccounts = new(StringComparer.OrdinalIgnoreCase)
    {
        "runner", "runneradmin", "user", "admin", "ubuntu", "root", "default", "guest", "public", "vsts",
        "administrator", "x", "...", "\u2026",
    };

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-1")]
    [Trait("Status", "Pending")] // green 2 (the C# tool and its project link) un-pends this
    public async Task Id_418_1a_csharp_planted_build_failure_stores_short_file_names()
    {
        // Given the C# baseline bench, a non-endpoint suite, and a stand-in dotnet that prints the planted Windows and Linux paths past 600 characters and exits 1 without a trx
        using var workspace = TempWorkspace.Create();
        var suite = WriteCSharpSuite(workspace.Root);
        var reply = WriteReply(workspace.Root, "pass.cs", PassingCSharp);
        var plant = WritePlant(workspace.Root);
        var fakeDotnet = WriteStandInDotnet(workspace.Root, plant, trxToCopy: null, exitCode: 1);

        // When the row is saved
        var run = await RunArm("Avowry.BenchCs", suite, "418-1a", PrintFileCommand(reply),
            environment: DotnetStandIn(fakeDotnet), timeoutMs: 180000);

        // Then every line of every diagnostic equals the short form, each entry is at most 600 characters, and both stored files pass the residue check
        var row = AssertStored(suite, "418-1a", run);
        AssertEveryLineIsShortForm(DiagnosticsOf(row));
        AssertNoResidue(suite, "418-1a");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-1")]
    [Trait("Status", "Pending")] // green 2 (the C# tool and its project link) un-pends this
    public async Task Id_418_1b_csharp_tests_did_not_run_stores_short_file_names()
    {
        // Given the C# baseline bench, an endpoint suite, and a stand-in that copies an empty trx, prints the plant, and exits 1
        using var workspace = TempWorkspace.Create();
        var suite = WriteEndpointSuite(workspace.Root);
        var reply = WriteReply(workspace.Root, "endpoint.cs", CorrectEndpointReply);
        var plant = WritePlant(workspace.Root);
        var emptyTrx = WriteReply(workspace.Root, "empty.trx", EmptyTrx);
        var fakeDotnet = WriteStandInDotnet(workspace.Root, plant, trxToCopy: emptyTrx, exitCode: 1);

        // When the row is saved
        var run = await RunArm("Avowry.BenchCs", suite, "418-1b", PrintFileCommand(reply),
            environment: DotnetStandIn(fakeDotnet), timeoutMs: 180000);

        // Then each diagnostic begins tests_did_not_run: and each following non-empty line equals the short form, and both stored files pass the residue check
        var row = AssertStored(suite, "418-1b", run);
        var diagnostics = DiagnosticsOf(row);
        Assert.NotEmpty(diagnostics);
        Assert.True(diagnostics.Length <= 3, "expected at most 3 attempt entries, got " + diagnostics.Length);
        foreach (var entry in diagnostics)
        {
            Assert.StartsWith("tests_did_not_run:", entry, StringComparison.Ordinal);
            var rest = entry["tests_did_not_run:".Length..].TrimStart();
            foreach (var line in Regex.Split(rest, @"\r?\n"))
            {
                if (line.Length == 0) continue;
                Assert.Equal(ShortForm, line);
            }
        }
        AssertNoResidue(suite, "418-1b");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-1")]
    public async Task Id_418_1c_build_failed_without_a_path_is_stored_unchanged()
    {
        // Given the C# baseline bench and a stand-in that prints only Build FAILED.
        using var workspace = TempWorkspace.Create();
        var suite = WriteCSharpSuite(workspace.Root);
        var reply = WriteReply(workspace.Root, "pass.cs", PassingCSharp);
        var fakeDotnet = WriteEchoStandIn(workspace.Root, "Build FAILED.");

        // When the row is saved
        var run = await RunArm("Avowry.BenchCs", suite, "418-1c", PrintFileCommand(reply),
            environment: DotnetStandIn(fakeDotnet), timeoutMs: 180000);

        // Then every diagnostic reads exactly Build FAILED.
        var row = AssertStored(suite, "418-1c", run);
        var diagnostics = DiagnosticsOf(row);
        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, entry => Assert.Equal("Build FAILED.", entry));
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-1")]
    [Trait("Status", "Pending")] // green 2 (the C# tool and its project link) un-pends this
    public async Task Id_418_1d_csharp_real_syntax_error_holds_no_folder()
    {
        // Given the C# baseline bench and a syntax-error reply scored by the real dotnet
        using var workspace = TempWorkspace.Create();
        var suite = WriteCSharpSuite(workspace.Root);
        var reply = WriteReply(workspace.Root, "broken.cs", BrokenCSharp);

        // When the row is saved
        var run = await RunArm("Avowry.BenchCs", suite, "418-1d", PrintFileCommand(reply), timeoutMs: 180000);

        // Then diagnostics[0] contains Candidate.cs( and error CS, and both stored files pass the residue check including the real folder
        var row = AssertStored(suite, "418-1d", run);
        var diagnostics = DiagnosticsOf(row);
        Assert.NotEmpty(diagnostics);
        Assert.Contains("Candidate.cs(", diagnostics[0], StringComparison.Ordinal);
        Assert.Contains("error CS", diagnostics[0], StringComparison.Ordinal);
        AssertNoResidue(suite, "418-1d");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-2")]
    [Trait("Status", "Pending")] // green 1 (the Phrase tool and the PathScrub.cs file) un-pends this
    public async Task Id_418_2a_phrase_model_exit_then_pass_scrubs_diagnostics()
    {
        // Given the Phrase bench, a model that writes the plant to stderr and exits 1 on attempt 1, then answers a passing Phrase
        using var workspace = TempWorkspace.Create();
        var suite = WritePhraseSuite(workspace.Root);
        var plant = WritePlant(workspace.Root);
        var pass = WriteReply(workspace.Root, "pass.avowry", PassingPhrase);
        var command = WriteExitThenReplyCommand(workspace.Root, plant, pass, toStderr: true, exitCode: 1);

        // When the row is saved
        var run = await RunArm("Avowry.Bench", suite, "418-2a", command);

        // Then the row is completed and passed in 2 attempts, diagnostics has one entry starting Model '<m>' exited 1: with the short name, and both stored files pass the residue check
        var row = AssertStored(suite, "418-2a", run);
        Assert.Equal("completed", row.GetProperty("status").GetString());
        Assert.True(row.GetProperty("passed").GetBoolean());
        Assert.Equal(2, row.GetProperty("attempts").GetInt32());
        var entry = Assert.Single(DiagnosticsOf(row));
        Assert.StartsWith("Model '418-2a' exited 1: ", entry, StringComparison.Ordinal);
        Assert.Contains("Candidate.cs", entry, StringComparison.Ordinal);
        AssertNoResidue(suite, "418-2a");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-2")]
    [Trait("Status", "Pending")] // green 1 (the Phrase tool and the PathScrub.cs file) un-pends this
    public async Task Id_418_2b_phrase_model_exit_on_every_attempt_scrubs_error()
    {
        // Given the Phrase bench and a model that writes the plant to stderr and exits 1 on every attempt
        using var workspace = TempWorkspace.Create();
        var suite = WritePhraseSuite(workspace.Root);
        var plant = WritePlant(workspace.Root);
        var command = WriteStderrExitCommand(workspace.Root, plant, 1);

        // When the row is saved
        var run = await RunArm("Avowry.Bench", suite, "418-2b", command);

        // Then the row is unavailable, error contains exited 1 and the short name, diagnostics is empty, and both stored files pass the residue check
        var row = AssertStored(suite, "418-2b", run);
        Assert.Equal("unavailable", row.GetProperty("status").GetString());
        var error = row.GetProperty("error").GetString() ?? "";
        Assert.Contains("exited 1", error, StringComparison.Ordinal);
        Assert.Contains("Candidate.cs", error, StringComparison.Ordinal);
        Assert.Empty(DiagnosticsOf(row));
        AssertNoResidue(suite, "418-2b");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-2")]
    [Trait("Status", "Pending")] // green 1 (the Phrase tool and the PathScrub.cs file) un-pends this
    public async Task Id_418_2c_phrase_executed_real_build_holds_no_folder()
    {
        // Given the Phrase bench in executed scoring and an endpoint reply whose native block names an undefined identifier
        using var workspace = TempWorkspace.Create();
        var suite = WritePhraseEndpointSuite(workspace.Root);
        var reply = WriteReply(workspace.Root, "broken.avowry", PhraseEndpointBroken);

        // When the row is saved
        var run = await RunArm("Avowry.Bench", suite, "418-2c", PrintFileCommand(reply),
            extraBenchArguments: ["--scoring", "executed"], disableBuildServers: true, timeoutMs: 180000);

        // Then the row is not passed, a diagnostic contains error CS0103 and the generated file's short name, and both stored files pass the residue check including the real folder
        var row = AssertStored(suite, "418-2c", run);
        Assert.False(row.GetProperty("passed").GetBoolean());
        var diagnostics = DiagnosticsOf(row);
        Assert.Contains(diagnostics, text => text.Contains("error CS0103", StringComparison.Ordinal));
        Assert.Contains(diagnostics, text => text.Contains("Avows.cs", StringComparison.Ordinal));
        AssertNoResidue(suite, "418-2c");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-2")]
    [Trait("Status", "Pending")] // green 2 (the C# tool and its project link) un-pends this
    public async Task Id_418_2d_csharp_start_failure_stores_the_missing_file_short_name()
    {
        // Given the C# baseline bench and PHRASE_BENCHCS_DOTNET pointing to a missing file inside <workspace>/Users/<user>/
        using var workspace = TempWorkspace.Create();
        var suite = WriteCSharpSuite(workspace.Root);
        var reply = WriteReply(workspace.Root, "pass.cs", PassingCSharp);
        var missing = Path.Combine(workspace.Root, "Users", "<user>", "missing-dotnet");

        // When the row is saved
        var run = await RunArm("Avowry.BenchCs", suite, "418-2d", PrintFileCommand(reply),
            environment: DotnetStandIn(missing), timeoutMs: 180000);

        // Then the row is unavailable, error contains An error occurred trying to start process and the missing file's short name, and both stored files pass the residue check including the real temp folder
        var row = AssertStored(suite, "418-2d", run);
        Assert.Equal("unavailable", row.GetProperty("status").GetString());
        var error = row.GetProperty("error").GetString() ?? "";
        Assert.Contains("An error occurred trying to start process", error, StringComparison.Ordinal);
        Assert.Contains("missing-dotnet", error, StringComparison.Ordinal);
        AssertNoResidue(suite, "418-2d");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-2")]
    [Trait("Status", "Pending")] // green 2 (the C# tool and its project link) un-pends this
    public async Task Id_418_2e_csharp_model_exit_scrubs_error()
    {
        // Given the C# baseline bench and a model that writes the plant to stderr and exits 1
        using var workspace = TempWorkspace.Create();
        var suite = WriteCSharpSuite(workspace.Root);
        var plant = WritePlant(workspace.Root);
        var command = WriteStderrExitCommand(workspace.Root, plant, 1);

        // When the row is saved
        var run = await RunArm("Avowry.BenchCs", suite, "418-2e", command, timeoutMs: 180000);

        // Then the row is unavailable, error contains exited 1 and the short name, and both stored files pass the residue check
        var row = AssertStored(suite, "418-2e", run);
        Assert.Equal("unavailable", row.GetProperty("status").GetString());
        var error = row.GetProperty("error").GetString() ?? "";
        Assert.Contains("exited 1", error, StringComparison.Ordinal);
        Assert.Contains("Candidate.cs", error, StringComparison.Ordinal);
        AssertNoResidue(suite, "418-2e");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-3")]
    [Trait("Status", "Pending")] // green 2 (the C# tool and its project link) un-pends this
    public async Task Id_418_3a_csharp_retry_prompt_carries_the_scrubbed_text()
    {
        // Given the C# baseline bench, a model that records every stdin, and a stand-in that prints the plant and exits 1 on call 1 then copies a passing trx and exits 0 on call 2
        using var workspace = TempWorkspace.Create();
        var suite = WriteCSharpSuite(workspace.Root);
        var reply = WriteReply(workspace.Root, "pass.cs", PassingCSharp);
        var plant = WritePlant(workspace.Root);
        var trx = WriteReply(workspace.Root, "pass.trx", PassingTrx);
        var command = WriteRecordingModel(workspace.Root, reply);
        var fakeDotnet = WriteTwoCallStandIn(workspace.Root, plant, trx);

        // When the C# baseline bench retries
        var run = await RunArm("Avowry.BenchCs", suite, "418-3a", command,
            environment: DotnetStandIn(fakeDotnet), timeoutMs: 180000);

        // Then the second recorded prompt contains Compiler or test diagnostics: then the short-form lines; both prompts pass the residue check; the row is passed in 2 attempts, error is null, and diagnostics is exactly the one scrubbed entry
        var row = AssertStored(suite, "418-3a", run);
        Assert.True(row.GetProperty("passed").GetBoolean());
        Assert.Equal(2, row.GetProperty("attempts").GetInt32());
        Assert.Equal(JsonValueKind.Null, row.GetProperty("error").ValueKind);
        var storedDiagnostics = DiagnosticsOf(row);
        Assert.Single(storedDiagnostics);
        AssertEveryLineIsShortForm(storedDiagnostics);
        var prompt1 = File.ReadAllText(Path.Combine(workspace.Root, "prompt-1.txt"));
        var prompt2 = File.ReadAllText(Path.Combine(workspace.Root, "prompt-2.txt"));
        var normalised = prompt2.Replace("\r\n", "\n", StringComparison.Ordinal);
        const string header = "Compiler or test diagnostics:";
        var at = normalised.IndexOf(header, StringComparison.Ordinal);
        Assert.True(at >= 0, "second prompt lacks Compiler or test diagnostics:");
        var block = normalised[(at + header.Length)..].TrimStart('\n', '\r');
        foreach (var line in block.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (trimmed.Length == 0) continue;
            Assert.Equal(ShortForm, trimmed);
        }
        AssertNoResidueOnText(prompt1.Replace("\r\n", "\n", StringComparison.Ordinal), prompt1, "prompt-1");
        AssertNoResidueOnText(normalised, prompt2, "prompt-2");
        AssertNoResidue(suite, "418-3a");
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-3")]
    [Trait("Status", "Pending")] // green 1 (the Phrase tool and the PathScrub.cs file) un-pends this
    public async Task Id_418_3b_phrase_retry_prompt_carries_the_scrubbed_text()
    {
        // Given a real Phrase broken build then a correct reply, with every prompt recorded
        using var workspace = TempWorkspace.Create();
        var suite = WritePhraseEndpointSuite(workspace.Root);
        var broken = WriteReply(workspace.Root, "broken.avowry", PhraseEndpointBroken);
        var correct = WriteReply(workspace.Root, "correct.avowry", PhraseEndpointCorrect);
        var command = WriteRecordingSwitchModel(workspace.Root, broken, correct);

        // When the Phrase bench retries
        var run = await RunArm("Avowry.Bench", suite, "418-3b", command,
            extraBenchArguments: ["--scoring", "executed"], disableBuildServers: true, timeoutMs: 180000);

        // Then the second prompt's Compiler diagnostics: block holds the generated file's short name and no residue
        _ = AssertStored(suite, "418-3b", run);
        var prompt2 = File.ReadAllText(Path.Combine(workspace.Root, "prompt-2.txt"));
        var normalised = prompt2.Replace("\r\n", "\n", StringComparison.Ordinal);
        const string header = "Compiler diagnostics:";
        var at = normalised.IndexOf(header, StringComparison.Ordinal);
        Assert.True(at >= 0, "second prompt lacks Compiler diagnostics:");
        var block = normalised[(at + header.Length)..];
        Assert.Contains("Avows.cs", block, StringComparison.Ordinal);
        AssertNoResidueOnText(block, block, "prompt-2 Compiler diagnostics");
    }

    private static JsonElement AssertStored(string suite, string model, (int ExitCode, string Stdout, string Stderr) run)
    {
        var path = ModelPath(suite, model);
        Assert.True(File.Exists(path), ScrubObserved(run.Stdout + Environment.NewLine + run.Stderr));
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }

    private static string[] DiagnosticsOf(JsonElement row) =>
        row.TryGetProperty("diagnostics", out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Select(item => item.GetString() ?? "").ToArray()
            : [];

    private static void AssertEveryLineIsShortForm(IReadOnlyList<string> diagnostics)
    {
        Assert.NotEmpty(diagnostics);
        Assert.True(diagnostics.Count <= 3, "expected at most 3 attempt entries, got " + diagnostics.Count);
        foreach (var entry in diagnostics)
        {
            Assert.True(entry.Length <= 600, "diagnostic longer than 600");
            var lines = Regex.Split(entry, @"\r?\n");
            var any = false;
            foreach (var line in lines)
            {
                if (line.Length == 0) continue;
                any = true;
                Assert.Equal(ShortForm, line);
            }
            Assert.True(any, "diagnostic had no lines");
        }
    }

    private static void AssertNoResidue(string suite, string model)
    {
        var modelRaw = File.ReadAllText(ModelPath(suite, model));
        var historyRaw = Assert.Single(File.ReadAllLines(HistoryPath(suite)), line =>
            line.Contains("\"model\":\"" + model + "\"", StringComparison.Ordinal));
        AssertNoResidueOnRow(modelRaw, "results/" + model + ".json");
        AssertNoResidueOnRow(historyRaw, "history.jsonl");
    }

    private static void AssertNoResidueOnRow(string raw, string label)
    {
        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;
        var hits = new List<string>();
        if (root.TryGetProperty("diagnostics", out var diagnostics) && diagnostics.ValueKind == JsonValueKind.Array)
        {
            var i = 0;
            foreach (var item in diagnostics.EnumerateArray())
            {
                hits.AddRange(ResidueIn(item.GetString() ?? "", $"{label} diagnostics[{i}]"));
                i++;
            }
        }
        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
            hits.AddRange(ResidueIn(error.GetString() ?? "", $"{label} error"));
        hits.AddRange(RawResidue(raw, label));
        hits.AddRange(RealFolderResidue(DecodedPayload(root), raw, label));
        Assert.True(hits.Count == 0, string.Join("; ", hits));
    }

    private static void AssertNoResidueOnText(string decoded, string raw, string label)
    {
        var hits = new List<string>();
        hits.AddRange(ResidueIn(decoded, label));
        hits.AddRange(RawResidue(raw, label));
        hits.AddRange(RealFolderResidue(decoded, raw, label));
        Assert.True(hits.Count == 0, string.Join("; ", hits));
    }

    private static string DecodedPayload(JsonElement root)
    {
        var parts = new List<string>();
        if (root.TryGetProperty("diagnostics", out var diagnostics) && diagnostics.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in diagnostics.EnumerateArray())
                parts.Add(item.GetString() ?? "");
        }
        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
            parts.Add(error.GetString() ?? "");
        return string.Join("\n", parts);
    }

    private static IEnumerable<string> ResidueIn(string text, string where)
    {
        if (Regex.IsMatch(text, @"(?<![A-Za-z0-9_])[A-Za-z]:[\\/]"))
            yield return where + " holds a drive path";
        if (Regex.IsMatch(text, @"\\\\[A-Za-z0-9._-]+\\"))
            yield return where + " holds a UNC path";
        if (Regex.IsMatch(text, @"(?<![\w.])~[\\/]"))
            yield return where + " holds a tilde path";
        if (Regex.IsMatch(text, @"(?<![A-Za-z0-9_])/home/"))
            yield return where + " holds /home/";
        if (Regex.IsMatch(text, @"(?<![A-Za-z0-9_])/Users/"))
            yield return where + " holds /Users/";
        if (Regex.IsMatch(text, @"(?<![A-Za-z0-9_])/root/"))
            yield return where + " holds /root/";
        if (Regex.IsMatch(text, @"(?<![A-Za-z0-9_])/tmp/"))
            yield return where + " holds /tmp/";
        if (Regex.IsMatch(text, @"(?<![A-Za-z0-9_])/var/folders/"))
            yield return where + " holds /var/folders/";
        if (text.Contains("<user>", StringComparison.Ordinal))
            yield return where + " holds the <user> placeholder";
        if (text.Contains(@"Users\", StringComparison.Ordinal))
            yield return where + " holds Users\\";
        if (text.Contains("AppData", StringComparison.Ordinal))
            yield return where + " holds AppData";
        if (text.Contains("phrase-bench", StringComparison.OrdinalIgnoreCase))
            yield return where + " holds phrase-bench";
    }

    private static IEnumerable<string> RawResidue(string raw, string where)
    {
        if (raw.Contains("\\u003Cuser\\u003E", StringComparison.Ordinal))
            yield return where + " raw holds \\u003Cuser\\u003E";
        if (raw.Contains("Users\\\\", StringComparison.Ordinal))
            yield return where + " raw holds a backslash-doubled Users path";
        if (raw.Contains("phrase-bench", StringComparison.OrdinalIgnoreCase))
            yield return where + " raw holds phrase-bench";
        if (raw.Contains("AppData", StringComparison.Ordinal))
            yield return where + " raw holds AppData";
    }

    private static IEnumerable<string> RealFolderResidue(string decoded, string raw, string where)
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var temp = Path.GetTempPath();
        foreach (var (label, folder) in new[] { ("profile", profile), ("temp", temp) })
        {
            if (string.IsNullOrEmpty(folder)) continue;
            foreach (var form in FolderForms(folder))
            {
                if (form.Length < 3) continue;
                if (decoded.Contains(form, StringComparison.OrdinalIgnoreCase))
                    yield return where + " decoded holds the " + label + " folder";
                if (raw.Contains(form, StringComparison.OrdinalIgnoreCase))
                    yield return where + " raw holds the " + label + " folder";
                var jsonEscaped = form.Replace("\\", "\\\\", StringComparison.Ordinal);
                if (jsonEscaped != form && raw.Contains(jsonEscaped, StringComparison.OrdinalIgnoreCase))
                    yield return where + " raw holds the JSON-escaped " + label + " folder";
            }
        }

        var last = Path.GetFileName(profile.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (last.Length >= 3 && !GenericAccounts.Contains(last))
        {
            if (ContainsWholeWord(decoded, last) || ContainsWholeWord(raw, last))
                yield return where + " holds the profile folder's last segment";
        }
    }

    private static string[] FolderForms(string folder)
    {
        var trimmed = folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return
        [
            trimmed,
            trimmed.Replace('\\', '/'),
            trimmed.Replace('/', '\\'),
        ];
    }

    private static bool ContainsWholeWord(string text, string word)
    {
        var start = 0;
        while (true)
        {
            var i = text.IndexOf(word, start, StringComparison.OrdinalIgnoreCase);
            if (i < 0) return false;
            var before = i == 0 || !IsWordChar(text[i - 1]);
            var after = i + word.Length >= text.Length || !IsWordChar(text[i + word.Length]);
            if (before && after) return true;
            start = i + 1;
        }
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static string ScrubObserved(string text)
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var temp = Path.GetTempPath();
        var result = text;
        foreach (var (folder, token) in new[] { (profile, "<home>"), (temp, "<temp>") })
        {
            if (string.IsNullOrEmpty(folder)) continue;
            foreach (var form in FolderForms(folder))
            {
                if (form.Length == 0) continue;
                result = result.Replace(form, token, StringComparison.OrdinalIgnoreCase);
            }
        }
        return result;
    }

    private static string WritePlant(string root)
    {
        var winRoot = @"C:\Users\<user>\AppData\Local\Temp\phrase-bench-cs\a\verification-b";
        var unixRoot = "/home/<user>/tmp/phrase-bench-cs/a/verification-b";
        var win = winRoot + @"\Candidate.cs(7,1): error CS1529: planted [" + winRoot + @"\Verification.csproj]";
        var unix = unixRoot + "/Candidate.cs(7,1): error CS1529: planted [" + unixRoot + "/Verification.csproj]";
        var pair = win + "\n" + unix + "\n";
        var builder = new StringBuilder();
        while (builder.Length < 700)
            builder.Append(pair);
        return WriteReply(root, "plant.txt", builder.ToString());
    }

    private static Dictionary<string, string> DotnetStandIn(string command) =>
        new() { ["PHRASE_BENCHCS_DOTNET"] = command };

    private static string WriteStandInDotnet(string root, string plantPath, string? trxToCopy, int exitCode)
    {
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, "fake-dotnet.cmd");
            var copy = trxToCopy is null
                ? ""
                : "if not exist TestResults mkdir TestResults\r\ncopy /y \"" + trxToCopy + "\" TestResults\\results.trx > nul\r\n";
            File.WriteAllText(script, "@echo off\r\n" + copy + "type \"" + plantPath + "\"\r\nexit /b " + exitCode + "\r\n");
            return script;
        }

        var shell = Path.Combine(root, "fake-dotnet.sh");
        var unixCopy = trxToCopy is null
            ? ""
            : "mkdir -p TestResults\ncp '" + trxToCopy + "' TestResults/results.trx\n";
        File.WriteAllText(shell, "#!/bin/sh\n" + unixCopy + "cat '" + plantPath + "'\nexit " + exitCode + "\n");
        File.SetUnixFileMode(shell, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return shell;
    }

    private static string WriteEchoStandIn(string root, string text)
    {
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, "fake-dotnet.cmd");
            File.WriteAllText(script, "@echo off\r\necho " + text + "\r\nexit /b 1\r\n");
            return script;
        }

        var shell = Path.Combine(root, "fake-dotnet.sh");
        File.WriteAllText(shell, "#!/bin/sh\necho '" + text + "'\nexit 1\n");
        File.SetUnixFileMode(shell, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return shell;
    }

    private static string WriteTwoCallStandIn(string root, string plantPath, string trxPath)
    {
        var counter = Path.Combine(root, "dotnet-counter.txt");
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, "fake-dotnet.cmd");
            File.WriteAllText(script, "@echo off\r\n" +
                "if exist \"" + counter + "\" goto later\r\n" +
                "echo 1>\"" + counter + "\"\r\n" +
                "type \"" + plantPath + "\"\r\n" +
                "exit /b 1\r\n" +
                ":later\r\n" +
                "if not exist TestResults mkdir TestResults\r\n" +
                "copy /y \"" + trxPath + "\" TestResults\\results.trx > nul\r\n" +
                "exit /b 0\r\n");
            return script;
        }

        var shell = Path.Combine(root, "fake-dotnet.sh");
        File.WriteAllText(shell, "#!/bin/sh\n" +
            "if [ -f '" + counter + "' ]; then\n" +
            "  mkdir -p TestResults\n" +
            "  cp '" + trxPath + "' TestResults/results.trx\n" +
            "  exit 0\n" +
            "fi\n" +
            "touch '" + counter + "'\n" +
            "cat '" + plantPath + "'\n" +
            "exit 1\n");
        File.SetUnixFileMode(shell, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return shell;
    }

    private static string WriteStderrExitCommand(string root, string plantPath, int exitCode)
    {
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, "stderr-exit.cmd");
            File.WriteAllText(script, "@echo off\r\nmore > nul\r\ntype \"" + plantPath + "\" 1>&2\r\nexit /b " + exitCode + "\r\n");
            return "call \"" + script + "\"";
        }

        var shell = Path.Combine(root, "stderr-exit.sh");
        File.WriteAllText(shell, "#!/bin/sh\ncat >/dev/null\ncat '" + plantPath + "' >&2\nexit " + exitCode + "\n");
        return "sh '" + shell + "'";
    }

    private static string WriteExitThenReplyCommand(string root, string plantPath, string replyPath, bool toStderr, int exitCode)
    {
        var counter = Path.Combine(root, "model-counter.txt");
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, "exit-then-reply.cmd");
            var plantLine = toStderr ? "type \"" + plantPath + "\" 1>&2\r\n" : "type \"" + plantPath + "\"\r\n";
            File.WriteAllText(script, "@echo off\r\nmore > nul\r\n" +
                "if exist \"" + counter + "\" goto later\r\n" +
                "echo 1>\"" + counter + "\"\r\n" +
                plantLine +
                "exit /b " + exitCode + "\r\n" +
                ":later\r\n" +
                "type \"" + replyPath + "\"\r\n");
            return "call \"" + script + "\"";
        }

        var shell = Path.Combine(root, "exit-then-reply.sh");
        var unixPlant = toStderr ? "cat '" + plantPath + "' >&2\n" : "cat '" + plantPath + "'\n";
        File.WriteAllText(shell, "#!/bin/sh\ncat >/dev/null\n" +
            "if [ -f '" + counter + "' ]; then cat '" + replyPath + "'; exit 0; fi\n" +
            "touch '" + counter + "'\n" +
            unixPlant +
            "exit " + exitCode + "\n");
        return "sh '" + shell + "'";
    }

    private static string WriteRecordingModel(string root, string replyPath)
    {
        var script = Path.Combine(root, "record-model.py");
        File.WriteAllText(script, """
            import pathlib, sys
            root = pathlib.Path(sys.argv[1])
            reply = pathlib.Path(sys.argv[2])
            n = 1
            while (root / f"prompt-{n}.txt").exists():
                n += 1
            (root / f"prompt-{n}.txt").write_bytes(sys.stdin.buffer.read())
            sys.stdout.buffer.write(reply.read_bytes())
            """);
        return "python \"" + script + "\" \"" + root + "\" \"" + replyPath + "\"";
    }

    private static string WriteRecordingSwitchModel(string root, string firstPath, string laterPath)
    {
        var script = Path.Combine(root, "record-switch.py");
        File.WriteAllText(script, """
            import pathlib, sys
            root = pathlib.Path(sys.argv[1])
            first = pathlib.Path(sys.argv[2])
            later = pathlib.Path(sys.argv[3])
            n = 1
            while (root / f"prompt-{n}.txt").exists():
                n += 1
            (root / f"prompt-{n}.txt").write_bytes(sys.stdin.buffer.read())
            sys.stdout.buffer.write((first if n == 1 else later).read_bytes())
            """);
        return "python \"" + script + "\" \"" + root + "\" \"" + firstPath + "\" \"" + laterPath + "\"";
    }

    private static string WriteCSharpSuite(string root)
    {
        var suiteRoot = Path.Combine(root, "csharp-baseline");
        var taskRoot = Path.Combine(suiteRoot, "tasks", "001-write-greeting");
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), "Return only C# source for namespace Greeting, static class Greeter, static string SayHello().");
        File.WriteAllText(Path.Combine(taskRoot, "outcomes.json"), OneCase);
        var suite = Path.Combine(suiteRoot, "suite.json");
        File.WriteAllText(suite, """
            {"version":1,"tasks":[{"id":"001-write-greeting","kind":"write","prompt":"tasks/001-write-greeting/prompt.md","outcomes":"tasks/001-write-greeting/outcomes.json","holdout":false}]}
            """);
        return suite;
    }

    private static string WriteEndpointSuite(string root)
    {
        var suiteRoot = Path.Combine(root, "csharp-baseline");
        var taskRoot = Path.Combine(suiteRoot, "tasks", "020-endpoint-greetings");
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), "Return only a FastEndpoints endpoint for POST /greetings.");
        File.WriteAllText(Path.Combine(taskRoot, "outcomes.json"), EndpointOutcomes);
        var suite = Path.Combine(suiteRoot, "suite.json");
        File.WriteAllText(suite, """
            {"version":1,"tasks":[{"id":"020-endpoint-greetings","kind":"endpoint","prompt":"tasks/020-endpoint-greetings/prompt.md","outcomes":"tasks/020-endpoint-greetings/outcomes.json","holdout":false}]}
            """);
        return suite;
    }

    private static string WritePhraseSuite(string root)
    {
        var suiteDirectory = Path.Combine(root, "bench");
        var taskDirectory = Path.Combine(suiteDirectory, "tasks", "001-write-greeting");
        Directory.CreateDirectory(taskDirectory);
        File.WriteAllText(Path.Combine(suiteDirectory, "suite.json"), """
            {
              "version": 1,
              "tasks": [
                {
                  "id": "001-write-greeting",
                  "kind": "write",
                  "prompt": "tasks/001-write-greeting/prompt.md",
                  "holdout": false
                }
              ]
            }
            """);
        File.Copy(
            RepoPaths.Find("bench", "tasks", "001-write-greeting", "prompt.md"),
            Path.Combine(taskDirectory, "prompt.md"));
        return Path.Combine(suiteDirectory, "suite.json");
    }

    private static string WritePhraseEndpointSuite(string root)
    {
        var suiteRoot = Path.Combine(root, "bench");
        var taskRoot = Path.Combine(suiteRoot, "tasks", "020-endpoint-greetings");
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), "Return only a Phrase endpoint module for POST /greetings.");
        File.WriteAllText(Path.Combine(taskRoot, "outcomes.json"), EndpointOutcomes);
        var suite = Path.Combine(suiteRoot, "suite.json");
        File.WriteAllText(suite, """
            {"version":1,"tasks":[{"id":"020-endpoint-greetings","kind":"endpoint","prompt":"tasks/020-endpoint-greetings/prompt.md","holdout":false}]}
            """);
        return suite;
    }

    private static string WriteReply(string root, string name, string contents)
    {
        var path = Path.Combine(root, name);
        File.WriteAllText(path, contents);
        return path;
    }

    private static string PrintFileCommand(string path) => OperatingSystem.IsWindows()
        ? "more > nul && type \"" + path + "\""
        : "cat > /dev/null && cat \"" + path + "\"";

    private static string HistoryPath(string suite) =>
        Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl");

    private static string ModelPath(string suite, string model) =>
        Path.Combine(Path.GetDirectoryName(suite)!, "results", model + ".json");

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunArm(
        string project,
        string suite,
        string model,
        string command,
        IReadOnlyDictionary<string, string>? environment = null,
        IReadOnlyList<string>? extraBenchArguments = null,
        bool disableBuildServers = false,
        int timeoutMs = 60000)
    {
        var arguments = new List<string>
        {
            "run", "--suite", suite, "--model", model, "--model-command", command,
            "--timeout-ms", timeoutMs.ToString(),
            "--private-results", HistoryPath(suite),
        };
        if (extraBenchArguments is not null)
            arguments.AddRange(extraBenchArguments);

        var runOptions = project == "Avowry.BenchCs" || disableBuildServers
            ? new[] { "--disable-build-servers" }
            : Array.Empty<string>();
        var info = BenchLauncher.CreateStartInfoFor(project, arguments, runOptions);
        info.WorkingDirectory = RepoPaths.Root;
        info.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        info.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            info.Environment.Remove(name);
        if (environment is not null)
        {
            foreach (var pair in environment)
                info.Environment[pair.Key] = pair.Value;
        }

        var wait = project == "Avowry.BenchCs" || disableBuildServers ? TimeSpan.FromMinutes(6) : TimeSpan.FromMinutes(1);
        var result = await ProcessRunner.RunAsync(info, wait);
        Assert.False(result.TimedOut, project + " " + model + " timed out.");
        return (result.ExitCode, result.Stdout, result.Stderr);
    }
}
