using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The tree scan and the commit hook gate by exit code
//   As the sponsor persona, I want the tree scan and the hook to refuse a home path or the account name,
//   so that a name scrubbed once does not come back unnoticed.
public class TreeScanGatesByExitCode
{
    private const string Module = "tests.test_account_name_376.";

    [Fact]
    [Trait("Slice", "376")]
    [Trait("Criterion", "376-2")]
    public async Task The_tree_scan_and_the_hook_gate_by_exit_code()
    {
        // Given a temporary repository holding a planted home path or the planted account name
        // When check-no-secrets.py --tree runs, or the staged guard checks an added line
        // Then --tree exits 1 naming path:line:form (T1, T2), exits 0 on the neutral file (T3),
        // prints no planted text and caps its hit list (T4), and the hook refuses the name (G3) but not <user> (G4)
        await AssertPythonTestsPass("TreeScanGatesByExitCode");
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
