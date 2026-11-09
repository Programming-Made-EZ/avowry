using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: CLI diagnostic shape (card 324 criterion 2)
//   avowry build still exits 1 with the AV line on a trigger, and 0 with empty
//   stderr on a clean source; only the unit word in the AV001 line may change.
public class CliKeepsDiagnosticShape
{
    [Theory]
    [Trait("Criterion", "2")]
    [InlineData("M13-AV001")]
    [InlineData("M08-AV203")]
    [InlineData("clean")]
    public async Task The_CLI_exits_1_with_the_diagnostic_line_and_0_when_clean(string id)
    {
        // Given a source file in a fresh output folder
        using var workspace = TempWorkspace.Create();
        var sourcePath = Path.Combine(workspace.Root, "probe.avowry");
        await File.WriteAllTextAsync(sourcePath, SourceFor(id));
        var output = Path.Combine(workspace.Root, "out");

        // When avowry build runs
        var result = await PhraseCli.RunAsync("build", sourcePath, "-o", output);

        // Then the exit code and stderr keep the current CLI shape
        if (id == "clean")
        {
            Assert.Equal(0, result.ExitCode);
            Assert.Equal("", result.Stderr);
            return;
        }

        Assert.Equal(1, result.ExitCode);
        if (id != "M13-AV001")
            return;

        var lines = result.Stderr.Replace("\r\n", "\n", StringComparison.Ordinal)
            .TrimEnd('\n')
            .Split('\n');
        Assert.Equal("avowry: Compilation failed:", lines[0]);
        Assert.Matches(
            """^AV001 \(11:5\) No (phrase|avow) matches this step\. Closest: "do nothing"\.$""",
            lines[1]);
    }

    private static string SourceFor(string id) => id switch
    {
        "M13-AV001" => """
            module Test

            avow to "do nothing"
              native csharp
                return;

            endpoint Probe : POST /probe
              input
                n: Int
              steps
                frobnicate widgets
            """,
        "M08-AV203" => """
            module Test

            avow to "do nothing"
            """,
        "clean" => """
            module Test

            avow to "do nothing"
              native csharp
                return;

            endpoint Probe : POST /probe
              input
                n: Int
              steps
                do nothing
            """,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown CLI characterisation row."),
    };
}
