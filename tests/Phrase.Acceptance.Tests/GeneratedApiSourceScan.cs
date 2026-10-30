using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Card 214 Done check 3 (commit 4): no port reuse window is left in the source. The scan reads the files of the working tree at the commit under test
// (no network), so a stale copy fails like CI. Layer: L6 only, real files through RepoPaths.Find, no doubles.
// Self-reference trap: this file and the red tests name what they forbid. Needles are built by concatenation and this file is excluded by name.
// A mention in a comment is NOT exempt: nothing is exempted except the explicit allowlist below.
public class GeneratedApiSourceScan
{
    private const string ThisFile = "GeneratedApiSourceScan.cs";
    private static readonly string Helper = "FreeLoopback" + "Port";
    private static readonly string NewListener = "new Tcp" + "Listener(";
    private static readonly string ProcessStart = "Process" + ".Start(";
    private static readonly string SetsUrls = "\\[\"ASPNETCORE" + "_URLS\"\\]\\s*=(?!=)";

    // The card's own test file that owns the listeners that stand in for foreign processes.
    private static readonly string[] ListenerAllowlist = ["tests/Phrase.Acceptance.Tests/Support/HarnessProbes.cs"];

    // The Process.Start callers that run the CLI and dotnet (build, run --project, tests) without choosing a port, plus the harness itself.
    private static readonly string[] ProcessStartCallers =
    [
        "BenchmarkHarnessAcceptance.cs", "CSharpBaselineAcceptance.cs", "DocumentationPrinciples.cs", "EnsureStepCompiles.cs", "ExtensionMap.cs",
        "HeadToHeadVerdict.cs", "HoldoutTaskConversion.cs", "LibraryBindingLayering.cs", "LlmCustomerCheck.cs", "MatchingCodebases.cs",
        "MobProcessAccountability.cs", "PhraseCheatSheet.cs", "PhraseEndpointExecutedOutcomes.cs", "PhraseExecutedOutcomesAcceptance.cs",
        "PrivateHoldoutResults.cs", "RecordUpdateFormCompiles.cs", "SourceMapDeployment.cs", "SourceMapTraceCommand.cs",
        "Support/DotnetCli.cs", "Support/GeneratedApi.cs", "Support/PhraseCli.cs", "TasksDemoApp.cs", "TasksDemoReadme.cs", "UsableV1.cs",
    ];

    [Fact]
    [Trait("Slice", "214")]
    public void Z_the_name_of_the_old_port_chooser_is_gone_from_cs_py_and_yml_files()
    {
        var hits = Scan(RepoPaths.Root, [".cs", ".py", ".yml"], skipDocs: true)
            .SelectMany(file => Matches(file, Regex.Escape(Helper), Helper)).ToList();
        Assert.True(hits.Count == 0, Describe("the old port chooser is still named", hits));
    }

    [Fact]
    [Trait("Slice", "214")]
    public void O_exactly_one_place_sets_the_url_and_its_value_is_the_literal_port_0()
    {
        var hits = TestSources().SelectMany(file => Matches(file, SetsUrls, "sets the URL environment variable")).ToList();

        Assert.True(hits.Count == 1, Describe("expected exactly one place that sets the URL environment variable", hits));
        Assert.EndsWith("Support/GeneratedApi.cs", hits[0].File);
        Assert.True(hits[0].Text.Contains("= \"http://127.0.0.1:0\";", StringComparison.Ordinal), Describe("the value must be the literal http://127.0.0.1:0", hits));
    }

    [Fact]
    [Trait("Slice", "214")]
    public void M_no_listener_is_created_outside_the_cards_own_test_file()
    {
        var hits = TestSources().Where(f => !ListenerAllowlist.Contains(f.Relative))
            .SelectMany(file => Matches(file, Regex.Escape(NewListener), "a listener created outside the allowlist")).ToList();
        Assert.True(hits.Count == 0, Describe("a listener is created outside the allowlist", hits));
    }

    [Fact]
    [Trait("Slice", "214")]
    public void M_no_listener_is_started_then_stopped_to_read_its_port()
    {
        // A Start, then LocalEndpoint, then Stop or Dispose, with no block closed in between.
        var hits = TestSources().SelectMany(file => Matches(file, @"\.Start\([^)]*\)\s*;[^}]{0,400}?LocalEndpoint[^}]{0,300}?\.(Stop|Dispose)\(", "start, read the port, release it")).ToList();
        Assert.True(hits.Count == 0, Describe("a port is chosen by binding and releasing a listener", hits));
    }

    [Fact]
    [Trait("Slice", "214")]
    public void M_no_helper_returning_an_int_is_named_port_free_unused_or_available()
    {
        var hits = TestSources().SelectMany(file => Matches(file, @"static\s+int\s+\w*(Port|Free|Unused|Available)\w*\s*\(", "an int helper named Port, Free, Unused or Available")).ToList();
        Assert.True(hits.Count == 0, Describe("a helper that could choose a port", hits));
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public void Guard_checked_set_of_process_start_callers()
    {
        var root = RepoPaths.Find("tests", "Phrase.Acceptance.Tests");
        var callers = Scan(root, [".cs"], skipDocs: false)
            .Where(file => file.Text.Contains(ProcessStart, StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(root, file.Path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal).ToList();

        Assert.Equal(ProcessStartCallers.Order(StringComparer.Ordinal), callers);
    }

    private sealed record Source(string Path, string Relative, string Text);

    private sealed record Hit(string File, int Line, string Pattern, string Text);

    private static IEnumerable<Source> TestSources() => Scan(RepoPaths.Find("tests"), [".cs"], skipDocs: false);

    private static IEnumerable<Source> Scan(string start, string[] extensions, bool skipDocs)
    {
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj", ".git", "node_modules", "TestResults" };
        if (skipDocs)
            excluded.UnionWith(["docs", "board", ".mob"]);
        var pending = new Stack<string>([start]);
        while (pending.Count > 0)
        {
            var dir = pending.Pop();
            foreach (var sub in Directory.EnumerateDirectories(dir).Where(d => !excluded.Contains(Path.GetFileName(d))))
                pending.Push(sub);
            foreach (var file in Directory.EnumerateFiles(dir).Where(f => extensions.Contains(Path.GetExtension(f)) && Path.GetFileName(f) != ThisFile))
                yield return new Source(file, Path.GetRelativePath(RepoPaths.Root, file).Replace('\\', '/'), File.ReadAllText(file));
        }
    }

    private static IEnumerable<Hit> Matches(Source file, string pattern, string label)
    {
        foreach (Match match in Regex.Matches(file.Text, pattern))
        {
            var line = file.Text.AsSpan(0, match.Index).Count('\n') + 1;
            yield return new Hit(file.Relative, line, label, file.Text.Split('\n')[line - 1].Trim());
        }
    }

    private static string Describe(string what, List<Hit> hits) =>
        $"{what}:{Environment.NewLine}" + string.Join(Environment.NewLine, hits.Select(h => $"  {h.File}:{h.Line} [{h.Pattern}] {h.Text}"));
}
