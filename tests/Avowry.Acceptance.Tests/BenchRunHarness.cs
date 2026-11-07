using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Card 264 red 2 (D157 Criterion 2): the harness that runs the Phrase.Bench child must preserve the
// child's real result (exit code, stdout, stderr) and the history path BEFORE it reads history, so an early
// child exit is reported as itself and not masked by DirectoryNotFoundException on results/history.jsonl.
//
// Proposed seam (the green creates it): tests/Avowry.Acceptance.Tests/Support/BenchChild.cs
//   internal static class BenchChild
//     Task<BenchChildResult> RunAsync(ProcessStartInfo child, string historyPath, params int[] acceptedExitCodes)
//   internal sealed record BenchChildResult(int ExitCode, string Stdout, string Stderr, string[] HistoryLines)
//   internal sealed class BenchChildException : Exception   // message carries exit code, stdout, stderr, historyPath
// RunAsync starts the child, awaits it, then: exit code not in acceptedExitCodes -> BenchChildException;
// history missing or empty -> BenchChildException; otherwise returns the result. It never lets a
// DirectoryNotFoundException escape. The existing private Run in PhraseExecutedOutcomesAcceptance becomes a
// thin caller of it.
public class BenchRunHarness
{
    private static readonly int[] Accepted = [0, 3];

    [Fact]
    [Trait("Slice", "264")]
    public async Task Early_child_exit_42_before_any_results_directory_is_reported_with_its_own_result()
    {
        using var workspace = TempWorkspace.Create();
        var history = HistoryPath(workspace);
        var child = FakeChild(workspace, stdout: "child-says-out", stderr: "child-says-err", exitCode: 42, historyLine: null);

        var failure = await Record.ExceptionAsync(() => BenchChild.RunAsync(child, history, Accepted));

        Assert.NotNull(failure);
        Assert.IsNotType<DirectoryNotFoundException>(failure);
        Assert.IsType<BenchChildException>(failure);
        Assert.Contains("42", failure.Message);
        Assert.Contains("child-says-out", failure.Message);
        Assert.Contains("child-says-err", failure.Message);
        Assert.Contains(history, failure.Message);
    }

    [Fact]
    [Trait("Slice", "264")]
    public async Task Child_that_exits_zero_and_writes_history_returns_its_result()
    {
        using var workspace = TempWorkspace.Create();
        var history = HistoryPath(workspace);
        var child = FakeChild(workspace, "ok-out", "", 0, "{\"task\":\"004-read-phrase\"}");

        var result = await BenchChild.RunAsync(child, history, Accepted);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("ok-out", result.Stdout);
        Assert.Single(result.HistoryLines);
    }

    [Fact]
    [Trait("Slice", "264")]
    public async Task Legitimate_scoring_exit_3_that_wrote_history_is_not_an_infrastructure_failure()
    {
        using var workspace = TempWorkspace.Create();
        var history = HistoryPath(workspace);
        var child = FakeChild(workspace, "scored", "", 3, "{\"task\":\"005-read-failure\"}");

        var result = await BenchChild.RunAsync(child, history, Accepted);

        Assert.Equal(3, result.ExitCode);
        Assert.Single(result.HistoryLines);
    }

    [Fact]
    [Trait("Slice", "264")]
    public async Task Exit_zero_with_no_results_file_at_all_fails_with_the_history_path_not_a_missing_directory()
    {
        using var workspace = TempWorkspace.Create();
        var history = HistoryPath(workspace);
        var child = FakeChild(workspace, "quiet-out", "quiet-err", 0, historyLine: null);

        var failure = await Record.ExceptionAsync(() => BenchChild.RunAsync(child, history, Accepted));

        Assert.IsType<BenchChildException>(failure);
        Assert.Contains(history, failure.Message);
        Assert.Contains("quiet-out", failure.Message);
    }

    [Fact]
    [Trait("Slice", "264")]
    public async Task History_present_but_empty_fails_with_the_exit_code_and_path()
    {
        using var workspace = TempWorkspace.Create();
        var history = HistoryPath(workspace);
        var child = FakeChild(workspace, "empty-out", "", 0, historyLine: "");

        var failure = await Record.ExceptionAsync(() => BenchChild.RunAsync(child, history, Accepted));

        Assert.IsType<BenchChildException>(failure);
        Assert.Contains(history, failure.Message);
        Assert.Contains("empty", failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Slice", "264")]
    public async Task Failed_child_build_keeps_its_diagnostics_in_the_failure()
    {
        using var workspace = TempWorkspace.Create();
        var history = HistoryPath(workspace);
        var child = FakeChild(workspace, "Build FAILED.", "error CS1002: ; expected", 1, historyLine: null);

        var failure = await Record.ExceptionAsync(() => BenchChild.RunAsync(child, history, Accepted));

        Assert.IsType<BenchChildException>(failure);
        Assert.Contains("Build FAILED.", failure.Message);
        Assert.Contains("error CS1002: ; expected", failure.Message);
        Assert.Contains(history, failure.Message);
    }

    [Fact]
    [Trait("Slice", "264")]
    public async Task Unexpected_non_zero_exit_that_still_wrote_history_is_a_failure_with_the_exit_code()
    {
        using var workspace = TempWorkspace.Create();
        var history = HistoryPath(workspace);
        var child = FakeChild(workspace, "crashed", "boom", 42, "{\"task\":\"x\"}");

        var failure = await Record.ExceptionAsync(() => BenchChild.RunAsync(child, history, Accepted));

        Assert.IsType<BenchChildException>(failure);
        Assert.Contains("42", failure.Message);
        Assert.Contains("boom", failure.Message);
    }

    private static string HistoryPath(TempWorkspace workspace) =>
        Path.Combine(workspace.Root, "suite", "results", "history.jsonl");

    // A tiny deterministic child: a script that prints, optionally writes the history file, and exits.
    // historyLine null = create nothing (not even the results directory); "" = create an empty file.
    private static ProcessStartInfo FakeChild(
        TempWorkspace workspace, string stdout, string stderr, int exitCode, string? historyLine)
    {
        var history = HistoryPath(workspace);
        var windows = OperatingSystem.IsWindows();
        var script = Path.Combine(workspace.Root, windows ? "child.cmd" : "child.sh");
        var lines = new List<string>();
        if (windows)
        {
            lines.Add("@echo off");
            if (stdout.Length > 0) lines.Add($"echo {stdout}");
            if (stderr.Length > 0) lines.Add($"echo {stderr} 1>&2");
            if (historyLine is not null)
            {
                lines.Add($"mkdir \"{Path.GetDirectoryName(history)}\"");
                lines.Add(historyLine.Length > 0 ? $"echo {historyLine}> \"{history}\"" : $"type nul> \"{history}\"");
            }
            lines.Add($"exit /b {exitCode}");
        }
        else
        {
            if (stdout.Length > 0) lines.Add($"echo '{stdout}'");
            if (stderr.Length > 0) lines.Add($"echo '{stderr}' >&2");
            if (historyLine is not null)
            {
                lines.Add($"mkdir -p \"{Path.GetDirectoryName(history)}\"");
                lines.Add($"printf '%s' '{historyLine}' > \"{history}\"");
            }
            lines.Add($"exit {exitCode}");
        }
        File.WriteAllLines(script, lines);

        var info = new ProcessStartInfo(windows ? "cmd.exe" : "sh")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        if (windows) { info.ArgumentList.Add("/d"); info.ArgumentList.Add("/c"); }
        info.ArgumentList.Add(script);
        return info;
    }
}
