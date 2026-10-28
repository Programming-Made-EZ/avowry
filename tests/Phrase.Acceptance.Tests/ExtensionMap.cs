using System.Diagnostics;
using System.IO;

namespace Phrase.Acceptance.Tests;

// Feature: The extension map separates the core from domain libraries
//   As Dana, I want one written map of what is core and what is an extension
//   for each application domain, with an audit of the current core,
//   so that the language grows by clean breaks instead of by accretion.
public class ExtensionMap
{
    [Fact]
    [Trait("Slice", "141")]
    public void The_extension_map_check_passes()
    {
        // Given the extension-map ADR and the checker that reads it
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
        start.ArgumentList.Add("tests.test_extension_map");

        // When the extension-map tests run
        using var process = Process.Start(start);
        Assert.NotNull(process);
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(120000);

        // Then the ADR defines the core, the extensions, the audit and the domains
        Assert.True(process.ExitCode == 0, stdout + stderr);
    }
}
