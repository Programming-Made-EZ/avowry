using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: An account name with a space is rewritten whole
//   As the sponsor persona, I want a two-word account segment to become the placeholder,
//   so that the second word of a spaced account name is not left in a published file.
public class AccountNameWithASpace
{
    private const string Module = "tests.test_account_name_376.";

    [Fact]
    [Trait("Slice", "376")]
    [Trait("Criterion", "376-3")]
    [Trait("Status", "Pending")]
    public async Task A_two_word_account_becomes_the_placeholder_and_prose_stays()
    {
        // Given planted paths whose account is two words
        // When the rewrite and the scan run
        // Then the whole segment becomes <user> for backslash (S1), slash (S2), doubled backslash (S3),
        // a closing quote or backtick (S4) and %20 (S5), at end of line (S1eol) and in account_root_re (S7),
        // while a path followed by prose keeps the prose (S6)
        await AssertPythonTestsPass("SpacedAccountRows");
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
