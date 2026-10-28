using System.Diagnostics;
using System.IO;

namespace Phrase.Acceptance.Tests;

// Feature: Public documentation states durable product principles
//   As Morgan, I want a mechanical check for personal attribution,
//   so that I can understand and challenge a direction from its rationale alone.
public class DocumentationPrinciples
{
    [Fact]
    [Trait("Slice", "154")]
    public void The_no_personal_names_test_suite_passes()
    {
        // Given examples of product principles, persona stories, and personal attributions
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
        start.ArgumentList.Add("tests.test_no_personal_names");

        // When the repository policy test suite runs
        using var process = Process.Start(start);
        Assert.NotNull(process);
        process.WaitForExit(120000);

        // Then forbidden attribution is rejected while persona contexts remain valid
        Assert.Equal(0, process.ExitCode);
    }
}
