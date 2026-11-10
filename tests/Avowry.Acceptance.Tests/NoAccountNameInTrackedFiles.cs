using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The sponsor persona publishes the repository without the maintainer's account name
//   As the sponsor persona, I want no tracked file to carry the local account name standing alone,
//   so that publishing the repository does not publish the maintainer's account name.
public class NoAccountNameInTrackedFiles
{
    private const string Module376 = "tests.test_account_name_376.";
    private const string ModuleBench = "tests.test_bench_history_clean";

    [Fact]
    [Trait("Slice", "376")]
    [Trait("Criterion", "376-1")]
    public async Task The_account_name_is_gone_from_tracked_files_and_the_bench_checker_still_refuses_it()
    {
        // Given every tracked file and the local account name read from the environment
        // When the whole-word scan reads each area (W-A1 to W-A8), local_accounts filters candidates (U1),
        // and the bench checker reads planted rows (B1 to B6)
        // Then the scan finds no hit, and the bench checker refuses the planted name without printing it
        await AssertPythonTestsPass(300000, Module376 + "WholeWordTreeScan", Module376 + "LocalAccountRows", ModuleBench);
    }

    [Fact]
    [Trait("Slice", "376")]
    public async Task The_space_rule_changes_no_tracked_file_and_the_records_are_the_rule_applied()
    {
        // Given today's tracked tree and the record files at the recorded base commit
        // When the rewrite rule reads the tree (C1) and each record is compared with its base blob (R1)
        // Then the space rule changes nothing today, and each record is its base blob or the rule applied to it
        await AssertPythonTestsPass(300000, Module376 + "SpaceRuleChangesNothingToday", Module376 + "RecordRewriteIsTheRule");
    }

    private static async Task AssertPythonTestsPass(int timeoutMilliseconds, params string[] testNames)
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
            start.ArgumentList.Add(name);
        }

        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(timeoutMilliseconds));

        Assert.True(result.ExitCode == 0 && !result.TimedOut, result.Stdout + result.Stderr);
    }
}
