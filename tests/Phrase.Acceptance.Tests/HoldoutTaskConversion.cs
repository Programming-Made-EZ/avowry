using System.Diagnostics;
using System.IO;

namespace Phrase.Acceptance.Tests;

// Feature: A generated holdout task is fair across both arms
//   As Riley, I want one script that converts a holdout file into the Phrase and C# task folders
//   with the same wrapper, so that neither arm gets a more specific prompt or richer starting code.
public class HoldoutTaskConversion
{
    [Fact]
    [Trait("Slice", "112")]
    public void The_python_holdout_task_test_suite_passes()
    {
        // Given an invented holdout task file and a temp output directory outside the repo
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
        start.ArgumentList.Add("tests.test_make_holdout_tasks");

        // When the task test suite runs
        using var process = Process.Start(start);
        Assert.NotNull(process);
        process.WaitForExit(120000);

        // Then it exits cleanly and proves the generator matches the fair-wrapper contract
        Assert.Equal(0, process.ExitCode);
    }
}
