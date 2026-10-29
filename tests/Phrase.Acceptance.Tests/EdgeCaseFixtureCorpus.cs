using System.Text.Json;
using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Compiler edge cases stay pinned as executable fixtures
//   As Morgan, I want each tricky Phrase shape and its expected outcome stored side by side,
//   so that compiler and adapter regressions fail the gate instead of depending on diff review.
public partial class EdgeCaseFixtureCorpus
{
    private static string Corpus => RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "corpus");

    public static IEnumerable<object[]> Fixtures() =>
        Directory.Exists(Corpus)
            ? Directory.EnumerateFiles(Corpus, "*.avowry").Order(StringComparer.Ordinal).Select(path => new object[] { path })
            : [];

    [Theory]
    [MemberData(nameof(Fixtures))]
    [Trait("Slice", "174")]
    public async Task Every_corpus_fixture_matches_its_expected_outcome(string source)
    {
        // Given Morgan's hand-written expected outcome beside one edge-case fixture
        using var expected = JsonDocument.Parse(await File.ReadAllTextAsync(Path.ChangeExtension(source, ".expected.json")));
        using var workspace = TempWorkspace.Create();

        // When the real CLI builds the fixture into an isolated temporary workspace
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);

        // Then the diagnostic or complete source-map projection exactly matches the pinned outcome
        var root = expected.RootElement;
        if (root.GetProperty("outcome").GetString() == "diagnostic")
        {
            Assert.NotEqual(0, build.ExitCode);
            var diagnostic = DiagnosticPattern().Match(build.Text);
            Assert.True(diagnostic.Success, build.Text);
            Assert.Equal(root.GetProperty("code").GetString(), diagnostic.Groups["code"].Value);
            Assert.Equal(root.GetProperty("line").GetInt32(), int.Parse(diagnostic.Groups["line"].Value));
            return;
        }

        Assert.True(build.ExitCode == 0, build.Text);
        using var map = JsonDocument.Parse(await File.ReadAllTextAsync(workspace.FindSingle("*.avowrymap.json")));
        var actual = map.RootElement.GetProperty("records").EnumerateArray().Select(MapRecord.From).ToArray();
        var pinned = root.GetProperty("records").EnumerateArray().Select(MapRecord.From).ToArray();
        Assert.Equal(pinned, actual);

        if (root.GetProperty("runSpecs").GetBoolean())
        {
            var specs = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
            Assert.True(specs.Built, specs.Log);
            Assert.True(specs.Total > 0, "runSpecs fixtures must run at least one generated spec");
            Assert.Equal(0, specs.Failed);
        }
    }

    [Fact]
    [Trait("Slice", "174")]
    public void The_corpus_has_a_header_comment_and_an_expected_file_per_fixture()
    {
        // Given the edge-case fixture corpus
        var sources = Directory.Exists(Corpus) ? Directory.GetFiles(Corpus, "*.avowry") : [];
        var outcomes = Directory.Exists(Corpus) ? Directory.GetFiles(Corpus, "*.expected.json") : [];

        // When Morgan checks its convention-based layout
        var missingOutcomes = sources.Where(path => !File.Exists(Path.ChangeExtension(path, ".expected.json"))).ToArray();
        var orphanOutcomes = outcomes.Where(path => !File.Exists(path[..^".expected.json".Length] + ".avowry")).ToArray();

        // Then every named shape has a header and a paired expected outcome, with no orphans
        Assert.NotEmpty(sources);
        Assert.Empty(missingOutcomes);
        Assert.Empty(orphanOutcomes);
        Assert.All(sources, path => Assert.StartsWith(
            $"# {Path.GetFileNameWithoutExtension(path)}:",
            File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal)));
    }

    private sealed record MapRecord(string Kind, string Name, string Endpoint, int Line)
    {
        public static MapRecord From(JsonElement record) => new(
            record.GetProperty("kind").GetString()!,
            record.GetProperty("name").GetString()!,
            record.GetProperty("endpoint").GetString()!,
            record.GetProperty("line").GetInt32());
    }

    [GeneratedRegex(@"(?<code>PH\d+) \((?<line>\d+):\d+\)")]
    private static partial Regex DiagnosticPattern();
}
