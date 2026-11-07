using System.Text.RegularExpressions;

namespace Avowry.Compiler.Tests.Diagnostics;

/// <summary>
/// Loads the 19 trigger sources and their pre-change diagnostic rows, and maps
/// the 13 adopted new fragments back to the old wording (and the reverse).
/// </summary>
internal static class TriggerSources
{
    private static readonly (string Old, string New)[] Fragments =
    [
        ("Phrase '", "Avow '"),
        ("Phrase specs cannot", "Avow specs cannot"),
        ("Phrase spec for '", "Avow spec for '"),
        ("Phrase has no implementation", "Avow has no implementation"),
        ("Composite phrase '", "Composite avow '"),
        (" on phrase '", " on avow '"),
        ("for a phrase that returns", "for an avow that returns"),
        ("No phrase matches", "No avow matches"),
        ("A phrase is implemented", "An avow is implemented"),
        ("a quoted phrase template", "a quoted avow template"),
        ("a phrase clause", "an avow clause"),
        ("a phrase call", "an avow call"),
        ("(phrase spec)", "(avow spec)"),
    ];

    private static readonly Dictionary<string, string> NewToOld =
        Fragments.ToDictionary(pair => pair.New, pair => pair.Old, StringComparer.Ordinal);

    private static readonly Dictionary<string, string> OldToNew =
        Fragments.ToDictionary(pair => pair.Old, pair => pair.New, StringComparer.Ordinal);

    private static readonly Regex NewFragments = Alternation(Fragments.Select(pair => pair.New));
    private static readonly Regex OldFragments = Alternation(Fragments.Select(pair => pair.Old));

    private static readonly IReadOnlyDictionary<string, string> SourceById;
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<BaselineRow>> BaselineById;

    static TriggerSources()
    {
        var directory = DiagnosticsDirectory();
        SourceById = ParseSources(ReadLf(Path.Combine(directory, "sources.txt")));
        BaselineById = ParseBaseline(ReadLf(Path.Combine(directory, "baseline.txt")));
        Ids = [.. SourceById.Keys];
    }

    public static IReadOnlyList<string> Ids { get; }

    public static string Source(string id) => SourceById[id];

    public static IReadOnlyList<BaselineRow> Baseline(string id) => BaselineById[id];

    public static IEnumerable<string> BaselineTexts() =>
        Ids.SelectMany(Baseline).Select(row => row.Text);

    public static string ToOld(string text) =>
        NewFragments.Replace(text, match => NewToOld[match.Value]);

    public static string ToNew(string text) =>
        OldFragments.Replace(text, match => OldToNew[match.Value]);

    internal sealed record BaselineRow(string Code, int Line, int Column, string Text)
    {
        public string LineText => $"{Code}|{Line}|{Column}|{Text}";
    }

    private static Regex Alternation(IEnumerable<string> fragments)
    {
        var escaped = fragments
            .OrderByDescending(fragment => fragment.Length)
            .Select(Regex.Escape);
        return new Regex(string.Join("|", escaped), RegexOptions.CultureInvariant);
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

    private static string ReadLf(string path) =>
        File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private static Dictionary<string, string> ParseSources(string text)
    {
        var sources = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var block in text.Split("### ", StringSplitOptions.RemoveEmptyEntries))
        {
            var (id, body) = SplitId(block);
            sources.Add(id, body.TrimEnd('\n'));
        }

        return sources;
    }

    private static Dictionary<string, IReadOnlyList<BaselineRow>> ParseBaseline(string text)
    {
        var baselines = new Dictionary<string, IReadOnlyList<BaselineRow>>(StringComparer.Ordinal);
        foreach (var block in text.Split("### ", StringSplitOptions.RemoveEmptyEntries))
        {
            var (id, body) = SplitId(block);
            var rows = new List<BaselineRow>();
            foreach (var line in body.Split('\n'))
            {
                if (!line.StartsWith("PH", StringComparison.Ordinal))
                    continue;
                var parts = line.Split('|', 4);
                rows.Add(new BaselineRow(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), parts[3]));
            }

            baselines.Add(id, rows);
        }

        return baselines;
    }

    private static (string Id, string Body) SplitId(string block)
    {
        var end = block.IndexOf('\n');
        var id = (end < 0 ? block : block[..end]).Trim();
        var body = end < 0 ? "" : block[(end + 1)..];
        return (id, body);
    }
}
