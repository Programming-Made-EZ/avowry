using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: No old-prefix diagnostic code remains in the live tree (card 361, criterion 2)
//   As the sponsor persona, I want every tracked live file to use the AV prefix, so that the codes I read,
//   paste and search are Avowry's. History (records, decisions, retros, bench rows, the generated board and
//   MOB.md) keeps the codes it was written with.
//
// Self-reference trap: this file must itself be clean under the scanner it defines. Every sample is built from
// parts (the two-letter old prefix is never written as one token), and the pattern uses a character class.
// The scanner is a pure function over (path, text) pairs; only the tracked-file list comes from git.
public class NoPhCodesInTheLiveTree
{
    // The old two-letter prefix, built so that this file never contains it as one token.
    private static readonly string Old = "P" + "H";

    // ---- Criterion 2, rows L1-L6 (NEW-STATE, Pending until the green) --------------------------------------

    [Fact]
    [Trait("Slice", "361")]
    [Trait("Criterion", "361-2")]
    public async Task Tracked_live_files_contain_no_PH_diagnostic_code()
    {
        // Given every tracked file outside the history allow-list (git ls-files -z, content read from disk)
        var files = await TrackedFilesAsync();

        // When the scanner looks for the old prefix followed by a digit, nnn, a hash run, a backslash-d, a quote or a word edge
        var hits = LiveTreeScanner.Scan(files);

        // Then each row has zero hits: L1 src, L2 tests, L3 live docs, L4 scripts, L5 working agreement, L6 matcher literals
        var codeHits = hits.Where(hit => hit.IsCode).ToList();
        var rows = new (string Id, string Name, List<Hit> Hits)[]
        {
            ("L1", "src", codeHits.Where(hit => LiveTreeScanner.AreaOf(hit.Path) == "src").ToList()),
            ("L2", "tests", codeHits.Where(hit => LiveTreeScanner.AreaOf(hit.Path) == "tests").ToList()),
            ("L3", "live-docs", codeHits.Where(hit => LiveTreeScanner.AreaOf(hit.Path) == "live-docs").ToList()),
            ("L4", "scripts", codeHits.Where(hit => LiveTreeScanner.AreaOf(hit.Path) == "scripts").ToList()),
            ("L5", "working-agreement", codeHits.Where(hit => LiveTreeScanner.AreaOf(hit.Path) == "working-agreement").ToList()),
            ("L6", "matcher-literals", hits.Where(hit => !hit.IsCode).ToList()),
            ("L7", "other-live-paths", codeHits.Where(hit => LiveTreeScanner.AreaOf(hit.Path) == "other").ToList()),
        };
        var failures = rows.Where(row => row.Hits.Count > 0).Select(row => Describe(row.Id, row.Name, row.Hits)).ToList();
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    }

    private static string Describe(string id, string name, List<Hit> hits)
    {
        var sample = string.Join(", ", hits.Take(3).Select(hit => $"{hit.Path}:{hit.Line}"));
        return $"{id} {name}: {hits.Count} hits in {hits.Select(hit => hit.Path).Distinct().Count()} files (first: {sample})";
    }

    // ---- Characterisation (D204): passes before and after the green --------------------------------------

    private static readonly (string Id, string Prefix, string Reason)[] History =
    [
        ("H1", ".mob/", "mob records are dated history of what was decided and run; rewriting them would falsify the audit trail"),
        ("H2", "docs/decisions/", "decision records are append-only history; the owner's rename ruling is appended there, never applied back"),
        ("H3", "docs/retros/", "retrospectives are dated history of what the team saw at the time"),
        ("H4", "bench/results/", "pushed benchmark rows are schema: a row written with a code stays as pushed"),
        ("H5", "board/", "the board files are generated from the issues and never hand-edited"),
        ("H6", "reference/csharp/Orders.Api/packages.lock.json", "a generated lockfile: its base64 content hashes can contain the old prefix by chance and are never hand-edited (card 369)"),
        ("H7", "reference/csharp/Orders.Specs/packages.lock.json", "a generated lockfile: its base64 content hashes can contain the old prefix by chance and are never hand-edited (card 369)"),
    ];

    [Fact]
    [Trait("Slice", "361")]
    public async Task Every_history_allow_list_entry_still_holds_a_PH_code()
    {
        // Given the seven allow-listed history areas (ids H1 to H5, and H6 and H7 for the two generated lockfiles of card 369; the earlier H6, MOB.md, was removed on 2026-11-10 when its last old-prefix text rotated out of the generated file), each with its reason
        Assert.Equal(["H1", "H2", "H3", "H4", "H5", "H6", "H7"], History.Select(entry => entry.Id));
        var files = await TrackedFilesAsync();

        // When each area is scanned for the old prefix without the allow-list
        var dead = new List<string>();
        foreach (var (id, prefix, reason) in History)
        {
            var count = files
                .Where(file => file.Path == prefix || file.Path.StartsWith(prefix, StringComparison.Ordinal))
                .Sum(file => LiveTreeScanner.HitsIn(file.Path, file.Text).Count);
            if (count == 0)
                dead.Add($"{id} {prefix}: no old-prefix code left ({reason})");
        }

        // Then every area still holds at least one, so no entry is dead and history was not scrubbed
        Assert.True(dead.Count == 0, string.Join("\n", dead));
    }

    public static TheoryData<string, string, string, bool> PlantedRows()
    {
        var data = new TheoryData<string, string, string, bool>();
        // Must hit (a planted live file with each form of the old prefix)
        data.Add("hit-code", "src/A.cs", "var code = \"" + Old + "325\";", true);
        data.Add("hit-fixture-id", "tests/A.cs", "[InlineData(\"M01-" + Old + "010\")]", true);
        data.Add("hit-no-left-boundary", "tests/CompositeReturnTypeStays" + Old + "325.cs", "class CompositeReturnTypeStays" + Old + "325 { }", true);
        data.Add("hit-two-digit-prefix-literal", "tests/B.cs", "text.StartsWith(\"" + Old + "10\")", true);
        data.Add("hit-backslash-d", "tests/C.cs", "new Regex(\"" + Old + "\\\\d{3}\")", true);
        data.Add("hit-quoted-bare", "tests/D.cs", "line.StartsWith(\"" + Old + "\", StringComparison.Ordinal)", true);
        data.Add("hit-nnn", "docs/GUIDE.md", "keep the " + Old + "nnn style", true);
        data.Add("hit-hash-run", "src/Diagnostic.cs", "/// code such as " + Old + "###", true);
        data.Add("hit-bare-word", "tests/E.cs", "builds without " + Old + " diagnostics", true);
        data.Add("hit-working-agreement", "TEAM_WORKING_AGREEMENT.md", "ledger row " + Old + "312", true);
        // Must not hit (lookalikes and case differences)
        data.Add("miss-phrase", "src/F.cs", "Phrase is a word", false);
        data.Add("miss-upper-phrase", "tools/Program.cs", Old + "RASE_BENCHCS_DOTNET", false);
        data.Add("miss-underscore", "tests/G.cs", Old + "RASE_X", false);
        data.Add("miss-orphans", "TEAM_WORKING_AGREEMENT.md", "OR" + Old + "ANS are handled", false);
        data.Add("miss-lowercase", "tests/H.cs", "ph325 and Ph325", false);
        data.Add("miss-new-prefix", "src/I.cs", "AV325 and AVnnn", false);
        // Must not hit (anything under an allow-listed path)
        foreach (var (id, prefix, _) in History)
            data.Add("allow-listed-" + id, !prefix.EndsWith("/") ? prefix : prefix + "x/y.md", Old + "325 and " + Old + "nnn", false);
        return data;
    }

    [Theory]
    [Trait("Slice", "361")]
    [MemberData(nameof(PlantedRows))]
    public void Scanner_planted_rows(string id, string path, string text, bool mustHit)
    {
        // Given a planted (path, text) pair
        // When the pure scanner reads it
        var hits = LiveTreeScanner.Scan([(path, text)]);

        // Then it hits exactly when the row says so
        Assert.True(mustHit == (hits.Count > 0), $"{id}: expected {(mustHit ? "a hit" : "no hit")} for {path}, got {hits.Count}");
    }

    [Fact]
    [Trait("Slice", "361")]
    public void Scanner_reports_the_line_and_whether_the_hit_is_a_three_digit_code()
    {
        // Given a text with a code on line 2, a literal on line 3 and a two-digit prefix literal on line 4
        var text = "a\nx " + Old + "341 y\nz " + Old + "nnn\nq \"" + Old + "10\"";

        // When it is scanned
        var hits = LiveTreeScanner.Scan([("src/A.cs", text)]);

        // Then the lines are 2, 3 and 4, and only the first is a code
        Assert.Equal([2, 3, 4], hits.Select(hit => hit.Line));
        Assert.Equal([true, false, false], hits.Select(hit => hit.IsCode));
    }

    // ---- The tracked-file list: git ls-files -z through ProcessRunner, content read from disk -------------

    private static async Task<List<(string Path, string Text)>> TrackedFilesAsync()
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("ls-files");
        start.ArgumentList.Add("-z");
        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromSeconds(60));
        Assert.True(result.ExitCode == 0 && !result.TimedOut, $"git ls-files failed: exit {result.ExitCode}, {result.Stderr}");

        var files = new List<(string Path, string Text)>();
        foreach (var path in result.Stdout.Split('\0', StringSplitOptions.RemoveEmptyEntries))
        {
            var full = Path.Combine(RepoPaths.Root, path.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(full))
                continue; // tracked but removed in the working tree (a move in progress)
            var bytes = File.ReadAllBytes(full);
            if (Array.IndexOf(bytes, (byte)0) >= 0)
                continue; // binary content has no source text to scan
            files.Add((path, Encoding.UTF8.GetString(bytes)));
        }

        Assert.True(files.Count > 500, $"git ls-files returned only {files.Count} readable files");
        return files;
    }
}

internal sealed record Hit(string Path, int Line, bool IsCode);

internal static class LiveTreeScanner
{
    // History allow-list: files under these paths are never live. The red's own files are not listed; they are clean.
    private static readonly string[] HistoryPrefixes =
        [".mob/", "docs/decisions/", "docs/retros/", "bench/results/", "board/", "MOB.md",
         "reference/csharp/Orders.Api/packages.lock.json", "reference/csharp/Orders.Specs/packages.lock.json"];

    // Case-sensitive, no left anchor (a class name may end in the old prefix and a number). The character class keeps this line clean.
    private static readonly Regex Pattern = new("""P[H](?:[0-9]|n{3}|\b)""", RegexOptions.CultureInvariant);

    private static readonly Regex ThreeDigitCode = new("""\GP[H][0-9]{3}(?![0-9])""", RegexOptions.CultureInvariant);

    public static bool IsHistory(string path)
    {
        var normal = path.Replace('\\', '/');
        return HistoryPrefixes.Any(prefix => normal == prefix || normal.StartsWith(prefix, StringComparison.Ordinal));
    }

    public static string AreaOf(string path)
    {
        var normal = path.Replace('\\', '/');
        if (normal.StartsWith("src/", StringComparison.Ordinal)) return "src";
        if (normal.StartsWith("tests/", StringComparison.Ordinal)) return "tests";
        if (normal.StartsWith("docs/", StringComparison.Ordinal)) return "live-docs";
        if (normal.StartsWith("scripts/", StringComparison.Ordinal)) return "scripts";
        if (normal == "TEAM_WORKING_AGREEMENT.md") return "working-agreement";
        return "other";
    }

    public static IReadOnlyList<Hit> Scan(IEnumerable<(string Path, string Text)> files) =>
        files.Where(file => !IsHistory(file.Path)).SelectMany(file => HitsIn(file.Path, file.Text)).ToList();

    public static IReadOnlyList<Hit> HitsIn(string path, string text)
    {
        var hits = new List<Hit>();
        foreach (Match match in Pattern.Matches(text))
        {
            var line = 1 + text.AsSpan(0, match.Index).Count((char)10);
            hits.Add(new Hit(path, line, ThreeDigitCode.Match(text, match.Index).Success));
        }

        return hits;
    }
}
