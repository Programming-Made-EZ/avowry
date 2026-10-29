using System.Diagnostics;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Process accountability remains enforceable while its checker evolves
//   As Morgan, I want the repository's independent process checks to test their own
//   workflow wiring and audit rules, so that a skipped mob step turns the gate red.
public class MobProcessAccountability
{
    [Fact]
    [Trait("Slice", "D134c")]
    public async Task The_mob_process_checker_proves_its_audit_behaviors()
    {
        // Given the committed mob-process checker and its repository-level tests
        var repository = RepoPaths.Root;
        var python = OperatingSystem.IsWindows() ? "python" : "python3";
        var start = new ProcessStartInfo(python)
        {
            WorkingDirectory = repository,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        start.ArgumentList.Add("tests.test_mob_process");

        // When Morgan runs the process-accountability acceptance check
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start Python.");
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        // Then every audit behavior passes, including workflow self-protection
        Assert.True(process.ExitCode == 0, output + error);
    }
}
