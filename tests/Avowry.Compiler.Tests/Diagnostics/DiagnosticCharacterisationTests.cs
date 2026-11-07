using Avowry.Compiler.Semantics;

namespace Avowry.Compiler.Tests.Diagnostics;

// Feature: Diagnostic characterisation (card 324 criterion 2)
//   Every recorded trigger keeps its diagnostic code, position and tail; only the
//   unit word may change, and only through the adopted a/an fragments.
public class DiagnosticCharacterisationTests
{
    public static TheoryData<string> SourceIds()
    {
        var data = new TheoryData<string>();
        foreach (var id in TriggerSources.Ids)
            data.Add(id);
        return data;
    }

    [Theory]
    [Trait("Criterion", "2")]
    [MemberData(nameof(SourceIds))]
    public void Diagnostic_codes_positions_and_tails_are_unchanged(string id)
    {
        // Given a trigger source whose diagnostics are recorded in the baseline
        var source = TriggerSources.Source(id);

        // When it is compiled
        var actual = Resolver.Resolve(source).Diagnostics
            .Select(diagnostic => $"{diagnostic.Code}|{diagnostic.Line}|{diagnostic.Column}|{TriggerSources.ToOld(diagnostic.Message)}")
            .ToList();

        // Then the whole list matches the baseline rows after ToOld
        var expected = TriggerSources.Baseline(id).Select(row => row.LineText).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("Criterion", "2")]
    public void The_baseline_covers_every_source_and_each_source_triggers()
    {
        // Given the recorded trigger sources
        var ids = TriggerSources.Ids;

        // When each source is compiled
        // Then there are 19 ids, each with a baseline row, and each yields a diagnostic
        Assert.Equal(19, ids.Count);
        foreach (var id in ids)
        {
            Assert.NotEmpty(TriggerSources.Baseline(id));
            Assert.NotEmpty(Resolver.Resolve(TriggerSources.Source(id)).Diagnostics);
        }
    }

    [Fact]
    [Trait("Criterion", "2")]
    public void Normalisation_does_not_hide_wording_drift()
    {
        // Given nine wording drifts that are not the adopted new fragments
        string[] samples =
        [
            "Avow  'ping {m: Int}' has the same wording as 'ping {n: Int}'.",
            "an avow 'count things' returns Int; use 'let' to keep it.",
            "Expected a avow clause (effects, fails with, spec, native or steps), found 'returns'.",
            "No avows match this step. Closest: \"do nothing\".",
            "Composite Avow 'count {n: Int}' cannot return a value.",
            "Avow' ping {m: Int}' has the same wording as 'ping {n: Int}'.",
            "avow 'do nothing' returns no value and cannot be used with 'let'.",
            "Phrase 'not-a-trigger' has drifted.",
            "An avow  is implemented by native blocks or by steps, not both.",
        ];
        var oldTexts = TriggerSources.BaselineTexts().ToHashSet(StringComparer.Ordinal);

        // When each drift is run through ToOld
        // Then none equals a legitimate baseline old text
        foreach (var sample in samples)
            Assert.False(oldTexts.Contains(TriggerSources.ToOld(sample)), sample);
    }
}
