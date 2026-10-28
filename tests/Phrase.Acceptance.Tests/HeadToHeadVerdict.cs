using System.Diagnostics;
using System.IO;

namespace Phrase.Acceptance.Tests;

// Feature: Steve receives one frozen-bar verdict for Phrase versus plain C#
//   As Steve, I want one totals-only verdict page, so that I can decide whether paused vocabulary work continues.
public class HeadToHeadVerdict
{
    [Fact]
    [Trait("Slice", "103")]
    public void The_python_verdict_test_suite_passes()
    {
        // Given invented WIN, LOSS, TIE, INCONCLUSIVE and token-failure benchmark rows
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        start.ArgumentList.Add("tests.test_verdict");

        // When the verdict acceptance suite runs
        using var process = Process.Start(start);
        Assert.NotNull(process);
        process.WaitForExit(120000);

        // Then every frozen-bar scenario produces its expected totals-only verdict
        Assert.Equal(0, process.ExitCode);
    }
}
