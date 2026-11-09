using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: Usable v1 closes with no suspended bar and a recorded re-freeze (card 162, criterion 3)
//   As the sponsor persona, I want bar 6 enforced again and the re-freeze written down, so that "usable" is
//   detected against the documents a model actually reads.
// All three rows are Pending until green 2 (Z1, Z2) and the re-freeze record commit (R1). None has a Milestone
// trait, so the usable-v1 job never selects them.
public class UsableV1Closure
{
    private const string Marker = "<a id=\"refreeze-162\"></a>";

    [Fact]
    [Trait("Slice", "162")]
    [Trait("Criterion", "162-3")]
    [Trait("Status", "Pending")]
    // Pending until green 2 deletes the suspension.
    public void Z1_the_ratchet_has_no_suspended_bar()
    {
        // Given the usable-v1 ratchet
        using var json = JsonDocument.Parse(File.ReadAllText(RepoPaths.Find("board", "usable-v1-ratchet.json")));

        // When its suspended entry is read
        var suspended = json.RootElement.TryGetProperty("suspended", out var value) ? value : (JsonElement?)null;

        // Then no bar is suspended
        var names = suspended is { ValueKind: JsonValueKind.Object } entries ? entries.EnumerateObject().Select(p => p.Name).ToList() : [];
        Assert.True(suspended is null || names.Count == 0, "board/usable-v1-ratchet.json still suspends bar(s): " + string.Join(", ", names));
    }

    [Fact]
    [Trait("Slice", "162")]
    [Trait("Criterion", "162-3")]
    [Trait("Status", "Pending")]
    // Pending until green 2 removes the Pending trait from the Bar 6 test.
    public void Z2_the_bar_6_test_is_not_pending()
    {
        // Given the Bar 6 test method
        var method = typeof(UsableV1).GetMethods().Single(m => m.Name.StartsWith("Bar6_", StringComparison.Ordinal));

        // When its traits are read
        var pending = method.GetCustomAttributesData().Any(attr => attr.AttributeType.Name == "TraitAttribute"
            && attr.ConstructorArguments.Count == 2
            && attr.ConstructorArguments[0].Value as string == "Status"
            && attr.ConstructorArguments[1].Value as string == "Pending");

        // Then it is not Pending, so the gate enforces it
        Assert.False(pending, method.Name + " is still marked Status=Pending.");
    }

    // Seven files are bound by the record; Program.cs and the runner are listed for the launcher guard only (informational).
    private static readonly (string Name, string Path)[] Bound =
    [
        ("grammar", "docs/GRAMMAR.md"),
        ("sheet", "docs/CHEATSHEET.md"),
        ("prompt", "bench/usable-v1-llm-check/tasks/U1-add-endpoint/prompt.md"),
        ("outcomes", "bench/usable-v1-llm-check/tasks/U1-add-endpoint/outcomes.json"),
        ("suite", "bench/usable-v1-llm-check/suite.json"),
        ("tasks", "demo/tasks/Tasks.avowry"),
        ("orders", "tests/Avowry.Acceptance.Tests/Fixtures/orders.avowry"),
    ];

    [Fact]
    [Trait("Slice", "162")]
    [Trait("Criterion", "162-3")]
    [Trait("Status", "Pending")]
    // Pending until the re-freeze record commit, which un-pends it so the gate enforces the freeze window.
    // Record format (docs/decisions/language-and-grammar.md, one marker, then machine lines):
    //   <a id="refreeze-162"></a>
    //   - refreeze-162 <repo-relative path> <old sha256 or -> <new sha256>
    //   - refreeze-162-tokens sheet <estimated tokens>
    public void R1_the_refreeze_record_matches_the_files_the_check_runs_against()
    {
        // Given the language and grammar decisions file
        var record = File.ReadAllText(RepoPaths.Find("docs", "decisions", "language-and-grammar.md"));

        // When the refreeze-162 marker and its machine lines are read
        var markerCount = Regex.Matches(record, Regex.Escape(Marker)).Count;
        Assert.True(markerCount == 1, $"docs/decisions/language-and-grammar.md must hold the {Marker} marker exactly once, found {markerCount}.");
        var section = record[(record.IndexOf(Marker, StringComparison.Ordinal) + Marker.Length)..];
        var next = section.IndexOf("<a id=", StringComparison.Ordinal);
        if (next >= 0) section = section[..next];
        var lines = Regex.Matches(section, @"(?m)^- refreeze-162 (?<path>\S+) (?<old>-|[0-9a-f]{64}) (?<new>[0-9a-f]{64})\s*$")
            .ToDictionary(m => m.Groups["path"].Value, m => (Old: m.Groups["old"].Value, New: m.Groups["new"].Value));

        // Then every bound file has a line whose new hash is the working file's sha256
        var failures = new List<string>();
        foreach (var (name, path) in Bound)
        {
            if (!lines.TryGetValue(path, out var hashes)) { failures.Add($"{name}: no '- refreeze-162 {path} <old|-> <new>' line."); continue; }
            var actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(RepoPaths.Find(path.Split('/'))))).ToLowerInvariant();
            if (hashes.New != actual) failures.Add($"{name}: recorded new hash {hashes.New} is not the working file's {actual}.");
        }

        // And each evidence file pins the recorded old or the recorded new GRAMMAR and sheet hash (the pins move when a model reruns)
        foreach (var model in new[] { "codex", "grok" })
        {
            using var evidence = JsonDocument.Parse(File.ReadAllText(RepoPaths.Find("docs", "usable-v1", $"llm-check-{model}.json")));
            foreach (var (field, path) in new[] { ("grammar_sha256", "docs/GRAMMAR.md"), ("cheatsheet_sha256", "docs/CHEATSHEET.md") })
            {
                if (!lines.TryGetValue(path, out var hashes)) continue;
                var pinned = evidence.RootElement.TryGetProperty(field, out var pin) ? pin.GetString() : null;
                if (pinned != hashes.Old && pinned != hashes.New)
                    failures.Add($"llm-check-{model}.json {field} {pinned} is neither the recorded old {hashes.Old} nor the recorded new {hashes.New}.");
            }
        }

        // And the cheat sheet is at most 700 estimated tokens and equals the recorded figure
        var sheetTokens = Math.Max(1, (int)Math.Ceiling(File.ReadAllText(RepoPaths.Find("docs", "CHEATSHEET.md")).Length / 4d));
        var token = Regex.Match(section, @"(?m)^- refreeze-162-tokens sheet (?<n>\d+)\s*$");
        if (!token.Success) failures.Add("no '- refreeze-162-tokens sheet <n>' line.");
        else if (int.Parse(token.Groups["n"].Value) != sheetTokens) failures.Add($"recorded sheet tokens {token.Groups["n"].Value} is not the working file's {sheetTokens}.");
        if (sheetTokens > 700) failures.Add($"the cheat sheet is {sheetTokens} estimated tokens, above 700.");

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
}
