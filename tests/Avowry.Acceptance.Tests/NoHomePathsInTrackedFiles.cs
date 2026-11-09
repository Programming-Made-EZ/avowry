using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The sponsor persona publishes the repository without the maintainer's home path
//   As the sponsor persona, I want no tracked file to carry a home path of a local account,
//   so that publishing the repository does not publish where its maintainer works.
public class NoHomePathsInTrackedFiles
{
    private const string Module = "tests.test_home_paths_370.";

    [Fact]
    [Trait("Slice", "370")]
    [Trait("Criterion", "370-1")]
    [Trait("Status", "Pending")]
    public async Task Tracked_files_carry_no_home_path_of_a_local_account()
    {
        // Given every file git ls-files lists, in every area (A1 .mob/ to A8 other)
        // When the rewrite rule and the shared scanner read each line, and the board generator renders a card
        // Then no line holds a home path outside the allow-list, and a generated card keeps its placeholders
        await AssertPythonTestsPass("TreeScan", "GeneratorRow");
    }

    [Fact]
    [Trait("Slice", "370")]
    public async Task The_rewrite_changes_only_account_segments_and_history_stays()
    {
        // Given the base commit recorded in .mob/records/370/scrub-commits.txt
        // When each tracked file is compared with the rule applied to its base blob
        // Then every difference is an account segment that became the placeholder (N1), no file is deleted (N2),
        // untracked files stay byte-identical (N3), the base blob keeps its path and is an ancestor (N4),
        // and the MIGRATION.txt pin is the proof over the rule-applied records
        await AssertPythonTestsPass("ScrubChangesOnlyAccountSegments");
    }

    private static async Task AssertPythonTestsPass(params string[] testNames)
    {
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        foreach (var name in testNames)
        {
            start.ArgumentList.Add(Module + name);
        }

        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(300000));

        Assert.True(result.ExitCode == 0 && !result.TimedOut, result.Stdout + result.Stderr);
    }
}
