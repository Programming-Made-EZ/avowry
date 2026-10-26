using System.Security.Cryptography;
using System.Text.Json;
using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;
using Phrase.Compiler.Syntax;

namespace Phrase.Acceptance.Tests;

// Milestone: Usable v1 (board/MILESTONE-usable-v1.md)
//   As Steve
//   I want one automated check that says whether Phrase can build an application
//   So that "usable" is detected, not guessed
//
// One test per bar, each checking what the WRITTEN bar says (never a file-existence probe). A bar that is not wired
// yet counts as NOT MET. The tests are Pending until card 012, so the normal gate skips them; the CI job
// `usable-v1` runs them through scripts/usable-v1-report.py, which stays green while the milestone is pending
// ("Usable v1 not reached: X of 6 criteria") and goes red only if a bar listed in board/usable-v1-ratchet.json regresses.
public class UsableV1
{
    private static string DemoApp => RepoPaths.Find("demo", "tasks", "Tasks.phrase");

    [Fact]
    [Trait("Status", "Pending")]
    [Trait("Milestone", "UsableV1")]
    public void Bar1_a_real_app_written_entirely_in_phrase()
    {
        // Given demo/tasks/Tasks.phrase, a CRUD API beyond the orders fixture
        Assert.True(File.Exists(DemoApp), "Bar 1 not met: demo/tasks/Tasks.phrase does not exist (card 009).");

        // When it goes through the real compiler front end
        var parsed = Parser.Parse(File.ReadAllText(DemoApp));
        Assert.True(parsed.Diagnostics.Count == 0, "Bar 1 not met: parse diagnostics: " + string.Join("; ", parsed.Diagnostics));
        var resolved = Resolver.Resolve(parsed.File);
        Assert.True(resolved.Diagnostics.Count == 0, "Bar 1 not met: resolver diagnostics: " + string.Join("; ", resolved.Diagnostics));

        // Then it has 3 to 5 endpoints, a validation, and a spec expecting a failure
        var endpoints = resolved.Module.Endpoints;
        Assert.True(endpoints.Count is >= 3 and <= 5, $"Bar 1 not met: {endpoints.Count} endpoints, expected 3 to 5.");
        Assert.True(endpoints.Any(e => e.Failures.Count > 0), "Bar 1 not met: no endpoint has a validation that can fail.");
        Assert.True(endpoints.Any(e => e.Specs.Any(s => s.Outcome is FailsModel)), "Bar 1 not met: no spec expects a failure.");

        // And it is written entirely in .phrase: no hand-written C# beside it
        var csharp = Directory.GetFiles(Path.GetDirectoryName(DemoApp)!, "*.cs", SearchOption.AllDirectories);
        Assert.True(csharp.Length == 0, "Bar 1 not met: hand-written C# found next to the app: " + string.Join(", ", csharp));
    }

    [Fact]
    [Trait("Status", "Pending")]
    [Trait("Milestone", "UsableV1")]
    public void Bar2_one_cli_command_compiles_it_to_csharp_and_fastendpoints()
    {
        // Not wired until card 007 (CLI) and the emitter cards. When wired: `phrase build demo/tasks/Tasks.phrase -o <dir>`
        // on the real demo app must produce the API and Specs projects, which bars 3 to 5 then build, test and call.
        Assert.Fail("Bar 2 not met: not wired yet (cards 003-007: the C# adapter and `phrase build`).");
    }

    [Fact]
    [Trait("Status", "Pending")]
    [Trait("Milestone", "UsableV1")]
    public void Bar3_dotnet_build_of_the_generated_projects_has_zero_warnings()
    {
        Assert.Fail("Bar 3 not met: not wired yet (needs bar 2's generated projects; card 012).");
    }

    [Fact]
    [Trait("Status", "Pending")]
    [Trait("Milestone", "UsableV1")]
    public void Bar4_the_generated_xunit_specs_all_pass_one_test_per_spec()
    {
        Assert.Fail("Bar 4 not met: not wired yet (needs the generated specs project; cards 006 and 012).");
    }

    [Fact]
    [Trait("Status", "Pending")]
    [Trait("Milestone", "UsableV1")]
    public void Bar5_the_app_runs_and_live_http_matches_the_specs()
    {
        Assert.Fail("Bar 5 not met: not wired yet (card 010: end-to-end over real HTTP).");
    }

    [Fact]
    [Trait("Status", "Pending")]
    [Trait("Milestone", "UsableV1")]
    public void Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone()
    {
        // Given the evidence the LLM-customer check records (card 011)
        var evidence = RepoPaths.Find("docs", "usable-v1", "llm-check.json");
        Assert.True(File.Exists(evidence), "Bar 6 not met: docs/usable-v1/llm-check.json does not exist (card 011).");

        // Then it says the check passed, and was made against the CURRENT GRAMMAR.md
        using var json = JsonDocument.Parse(File.ReadAllText(evidence));
        Assert.True(json.RootElement.TryGetProperty("passed", out var passed) && passed.ValueKind == JsonValueKind.True,
            "Bar 6 not met: the recorded LLM check did not pass.");
        var grammarHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(RepoPaths.Find("docs", "GRAMMAR.md")))).ToLowerInvariant();
        Assert.True(json.RootElement.TryGetProperty("grammar_sha256", out var recorded)
                    && string.Equals(recorded.GetString(), grammarHash, StringComparison.OrdinalIgnoreCase),
            "Bar 6 not met: the LLM check was made against an older GRAMMAR.md; re-run it.");
    }
}
