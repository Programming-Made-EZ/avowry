using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The grammar and the cheat sheet teach the ensure step and the record update (card 162, criterion 1)
//   As the sponsor persona, I want docs/GRAMMAR.md and docs/CHEATSHEET.md to describe the ensure step, the
//   record update and the AV code convention, so that a cold model can use them from those two documents alone.
public class PatternDocs
{
    [Fact]
    [Trait("Slice", "162")]
    [Trait("Criterion", "162-1")]
    [Trait("Status", "Pending")]
    // Pending until green 1 folds the pattern steps into the two documents (ids G1-G8, S1, S2).
    public async Task The_grammar_and_the_cheat_sheet_teach_the_ensure_step_and_the_record_update()
    {
        // Given the working-tree documents and the python needle module
        var repoRoot = RepoPaths.Root;
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        start.ArgumentList.Add("tests.test_pattern_docs");

        // When the pattern documentation needles run
        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(120000));

        // Then every needle holds (the failure text lists the ones that do not)
        Assert.False(result.TimedOut, "tests.test_pattern_docs timed out after 120 s.");
        Assert.True(result.ExitCode == 0, result.Stdout + result.Stderr);
    }
}
