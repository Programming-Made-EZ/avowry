using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: A guard refuses a new home path at the commit boundary
//   As the sponsor persona, I want the commit hook to refuse a staged home path in any file,
//   so that a path scrubbed once does not come back.
public class PreCommitRefusesAHomePath
{
    private const string Module = "tests.test_home_paths_370.";

    [Fact]
    [Trait("Slice", "370")]
    [Trait("Criterion", "370-3")]
    [Trait("Status", "Pending")]
    public async Task The_hook_names_the_file_that_adds_a_home_path()
    {
        // Given a clone whose staged file (not MOB.md) adds a home path in the plain, escaped and slug forms
        // When check-no-secrets.py runs, as the pre-commit hook does
        // Then it exits 1 naming the file and each line but not the account (G1), the placeholder form exits 0 (G2),
        // and the shared scanner hits every planted form F1 to F9 and none of the neutral samples
        await AssertPythonTestsPass("PreCommitRefusesAHomePath", "ScannerRows");
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

        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(120000));

        Assert.True(result.ExitCode == 0 && !result.TimedOut, result.Stdout + result.Stderr);
    }
}
