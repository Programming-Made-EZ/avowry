using System.Text.RegularExpressions;
using Avowry.Compiler.Semantics;

namespace Avowry.Compiler.Tests.Diagnostics;

// Feature: Diagnostic numbers survive the prefix rename (card 361, characterisation, D204)
//   Every trigger source keeps its diagnostic number, line, column and message while the two-letter
//   prefix changes. The snapshot file holds no prefix at all, so this test passes before and after
//   the rename: it is the only evidence that the rewrite kept the numbers (the existing pins are
//   rewritten by the same change and cannot show it).
public class CodesKeepTheirNumbers
{
    private static readonly Regex IdPrefix = new("-[A-Z]{2}(?=[0-9])", RegexOptions.CultureInvariant);
    private static readonly Regex CodeShape = new("^[A-Z]{2}[0-9]{3}$", RegexOptions.CultureInvariant);

    public static TheoryData<string> SourceIds()
    {
        var data = new TheoryData<string>();
        foreach (var id in TriggerSources.Ids)
            data.Add(id);
        return data;
    }

    [Theory]
    [Trait("Slice", "361")]
    [MemberData(nameof(SourceIds))]
    public void Every_trigger_source_reports_the_same_number_position_and_message(string id)
    {
        // Given a trigger source and its prefix-free snapshot rows (ordinal|digits|line|col|message)
        var snapshot = Snapshot();
        var key = IdPrefix.Replace(id, "-");
        Assert.True(snapshot.ContainsKey(key), $"numbers-361.txt has no block for {key}");

        // When it is compiled
        var diagnostics = Resolver.Resolve(TriggerSources.Source(id)).Diagnostics;

        // Then each code is two letters and three digits, and with the two letters removed the rows equal the snapshot
        foreach (var diagnostic in diagnostics)
            Assert.Matches(CodeShape, diagnostic.Code);
        var actual = diagnostics
            .Select((d, i) => $"{i + 1}|{d.Code[2..]}|{d.Line}|{d.Column}|{d.Message}")
            .ToList();
        Assert.Equal(snapshot[key], actual);
    }

    [Fact]
    [Trait("Slice", "361")]
    public void The_snapshot_covers_every_trigger_source_and_holds_no_letters_before_a_number()
    {
        // Given the snapshot and the recorded sources
        var snapshot = Snapshot();

        // When their keys are compared
        // Then there is one block per source (19), each non-empty, and no row starts with a letter
        Assert.Equal(19, TriggerSources.Ids.Count);
        Assert.Equal(
            TriggerSources.Ids.Select(id => IdPrefix.Replace(id, "-")).Order(StringComparer.Ordinal),
            snapshot.Keys.Order(StringComparer.Ordinal));
        foreach (var (key, rows) in snapshot)
        {
            Assert.NotEmpty(rows);
            foreach (var row in rows)
                Assert.Matches("""^[0-9]+\|[0-9]{3}\|[0-9]+\|[0-9]+\|""", row);
        }
    }

    private static Dictionary<string, List<string>> Snapshot()
    {
        var text = File.ReadAllText(Path.Combine(DiagnosticsDirectory(), "numbers-361.txt"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        var blocks = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        List<string>? current = null;
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                current = [];
                blocks.Add(line[4..].Trim(), current);
            }
            else
            {
                current!.Add(line);
            }
        }

        return blocks;
    }

    private static string DiagnosticsDirectory()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Avowry.sln")))
                return Path.Combine(dir.FullName, "tests", "Avowry.Compiler.Tests", "Diagnostics");
        }

        throw new DirectoryNotFoundException("Could not find Avowry.sln above the test output directory.");
    }
}
