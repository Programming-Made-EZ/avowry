using System.Diagnostics;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;

namespace Phrase.Acceptance.Tests.Support;

public sealed record MutationPhraseResult(string Phrase, int Killed, int Buildable, int Excluded);

public sealed record MutationSurvivor(string Phrase, string Operator);

public sealed record MutationTotals(int Killed, int Buildable, int Excluded);

public sealed record MutationCheckResult(
    IReadOnlyList<MutationPhraseResult> Phrases,
    MutationTotals Total,
    IReadOnlyList<MutationSurvivor> Survivors,
    TimeSpan RunTime,
    string DeterministicSummary,
    string Report)
{
    public double KillRate => Total.Buildable == 0 ? 0 : (double)Total.Killed / Total.Buildable;
}

public static class CSharpMutationCheck
{
    private sealed record Mutation(string Phrase, string Operator, string OriginalBody, string MutatedBody);

    private static readonly Mutation[] Mutations =
    [
        new("ensure {items: List<LineItem>} is not empty", "flip comparison",
            "if (items.Count == 0) Fail(\"EmptyOrder\");", "if (items.Count != 0) Fail(\"EmptyOrder\");"),
        new("ensure {items: List<LineItem>} is not empty", "drop Fail call",
            "if (items.Count == 0) Fail(\"EmptyOrder\");", "if (items.Count == 0) { }"),
        new("ensure {items: List<LineItem>} is not empty", "change integer constant by one",
            "if (items.Count == 0) Fail(\"EmptyOrder\");", "if (items.Count == 1) Fail(\"EmptyOrder\");"),
        new("ensure {items: List<LineItem>} is not empty", "return early",
            "if (items.Count == 0) Fail(\"EmptyOrder\");", "return;"),
        new("ensure every quantity in {items: List<LineItem>} is positive", "flip comparison",
            "if (items.Any(i => i.Quantity <= 0)) Fail(\"InvalidQuantity\");", "if (items.Any(i => i.Quantity > 0)) Fail(\"InvalidQuantity\");"),
        new("ensure every quantity in {items: List<LineItem>} is positive", "drop Fail call",
            "if (items.Any(i => i.Quantity <= 0)) Fail(\"InvalidQuantity\");", "if (items.Any(i => i.Quantity <= 0)) { }"),
        new("ensure every quantity in {items: List<LineItem>} is positive", "change integer constant by one",
            "if (items.Any(i => i.Quantity <= 0)) Fail(\"InvalidQuantity\");", "if (items.Any(i => i.Quantity <= 1)) Fail(\"InvalidQuantity\");"),
        new("ensure every quantity in {items: List<LineItem>} is positive", "return early",
            "if (items.Any(i => i.Quantity <= 0)) Fail(\"InvalidQuantity\");", "return;"),
        new("total quantity of {items: List<LineItem>}", "return early",
            "return items.Sum(i => i.Quantity);", "return 0;"),
    ];

    public static async Task<MutationCheckResult> RunAsync(string source)
    {
        var started = Stopwatch.StartNew();
        var sourceWithBoundarySpecs = AddBoundarySpecsToCopy(source);
        var outcomes = new List<(Mutation Mutation, bool Built, bool Killed)>();

        foreach (var mutation in Mutations)
        {
            var mutant = ReplaceExactlyOnce(sourceWithBoundarySpecs, mutation.OriginalBody, mutation.MutatedBody);
            using var workspace = TempWorkspace.Create();
            PhraseCompiler.Compile(mutant, new CSharpProvider()).WriteTo(workspace.Root);
            var run = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
            outcomes.Add((mutation, run.Built, run.Built && run.Failed > 0));
        }

        started.Stop();
        var phrases = Mutations.Select(mutation => mutation.Phrase).Distinct(StringComparer.Ordinal)
            .Select(phrase =>
            {
                var results = outcomes.Where(outcome => outcome.Mutation.Phrase == phrase).ToArray();
                return new MutationPhraseResult(
                    phrase,
                    results.Count(result => result.Killed),
                    results.Count(result => result.Built),
                    results.Count(result => !result.Built));
            }).ToArray();
        var survivors = outcomes
            .Where(outcome => outcome.Built && !outcome.Killed)
            .Select(outcome => new MutationSurvivor(outcome.Mutation.Phrase, outcome.Mutation.Operator))
            .ToArray();
        var total = new MutationTotals(
            phrases.Sum(phrase => phrase.Killed),
            phrases.Sum(phrase => phrase.Buildable),
            phrases.Sum(phrase => phrase.Excluded));
        var summary = FormatSummary(phrases, total, survivors);
        var report = summary + Environment.NewLine + $"Run time: {started.Elapsed.TotalSeconds:F1} seconds";
        return new MutationCheckResult(phrases, total, survivors, started.Elapsed, summary, report);
    }

    private static string AddBoundarySpecsToCopy(string source) => source
        .Replace(
            "  native csharp\n    if (items.Count == 0) Fail(\"EmptyOrder\");",
            "  spec \"accepts a non-empty list\"\n    given items = [{ sku: \"A\", quantity: 1 }]\n    when called\n    then succeeds\n  native csharp\n    if (items.Count == 0) Fail(\"EmptyOrder\");",
            StringComparison.Ordinal)
        .Replace(
            "  native csharp\n    if (items.Any(i => i.Quantity <= 0)) Fail(\"InvalidQuantity\");",
            "  spec \"accepts a positive quantity\"\n    given items = [{ sku: \"A\", quantity: 1 }]\n    when called\n    then succeeds\n  native csharp\n    if (items.Any(i => i.Quantity <= 0)) Fail(\"InvalidQuantity\");",
            StringComparison.Ordinal);

    private static string ReplaceExactlyOnce(string source, string original, string replacement)
    {
        var first = source.IndexOf(original, StringComparison.Ordinal);
        var last = source.LastIndexOf(original, StringComparison.Ordinal);
        if (first < 0 || first != last)
            throw new InvalidOperationException($"Mutation target must occur exactly once: {original}");
        return source[..first] + replacement + source[(first + original.Length)..];
    }

    private static string FormatSummary(
        IReadOnlyList<MutationPhraseResult> phrases,
        MutationTotals total,
        IReadOnlyList<MutationSurvivor> survivors)
    {
        var lines = phrases.Select(phrase =>
            $"{phrase.Phrase}: {phrase.Killed} / {phrase.Buildable} killed; {phrase.Excluded} excluded").ToList();
        lines.Add($"Total: {total.Killed} / {total.Buildable} killed; {total.Excluded} excluded");
        lines.Add(survivors.Count == 0
            ? "Survivors: none"
            : "Survivors: " + string.Join(", ", survivors.Select(survivor => $"{survivor.Phrase} [{survivor.Operator}]")));
        return string.Join(Environment.NewLine, lines);
    }
}
