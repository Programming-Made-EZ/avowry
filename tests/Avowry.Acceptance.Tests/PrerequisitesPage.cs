using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: Dana reads one prerequisites page that matches what CI tests
//   As Dana, I want the supported .NET SDK version, operating systems and architecture
//   stated in one place, so that I know before installing whether it will work for me.
public class PrerequisitesPage
{
    [Fact]
    [Trait("Slice", "178")]
    [Trait("Criterion", "178-1")]
    [Trait("Status", "Pending")]
    public async Task Prerequisites_page_matches_what_CI_uses()
    {
        // Given the CI workflows and docs/PREREQUISITES.md as they are on the working tree
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        start.ArgumentList.Add("tests.test_prerequisites");
        using var process = Process.Start(start);
        Assert.NotNull(process);
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await process.WaitForExitAsync(timeout.Token);
        var text = await stdout + await stderr;
        Assert.True(process.ExitCode == 0, text);
    }
}
