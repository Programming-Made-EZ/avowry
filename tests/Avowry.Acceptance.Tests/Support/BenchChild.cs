using System.Diagnostics;

namespace Avowry.Acceptance.Tests.Support;

/// <summary>
/// Card 264 (D157 Criterion 2): runs a Avowry.Bench child and keeps its real result (exit code, stdout, stderr) and the
/// history path before anything reads history, so an early child exit or a failed build is reported as itself and is not
/// masked by a DirectoryNotFoundException on results/history.jsonl.
/// </summary>
internal static class BenchChild
{
    internal static async Task<BenchChildResult> RunAsync(ProcessStartInfo child, string historyPath, params int[] acceptedExitCodes)
    {
        Process process;
        try
        {
            process = Process.Start(child)
                ?? throw new BenchChildException($"Bench child did not start. History path: {historyPath}");
        }
        catch (Exception error) when (error is not BenchChildException)
        {
            throw new BenchChildException($"Bench child could not start: {error.Message}. History path: {historyPath}");
        }

        using (process)
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var exitCode = process.ExitCode;
            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (!acceptedExitCodes.Contains(exitCode))
                throw Failure($"Bench child exited with unexpected exit code {exitCode} (accepted: {string.Join(", ", acceptedExitCodes)})", exitCode, stdout, stderr, historyPath);

            var lines = ReadHistoryLines(historyPath, out var problem);
            if (problem is not null)
                throw Failure($"Bench child exit code {exitCode}: {problem}", exitCode, stdout, stderr, historyPath);

            return new BenchChildResult(exitCode, stdout, stderr, lines);
        }
    }

    /// <summary>Reads a history file that a completed child must have written; a missing or empty file is a BenchChildException.</summary>
    internal static string[] ReadHistoryLines(string historyPath)
    {
        var lines = ReadHistoryLines(historyPath, out var problem);
        return problem is null ? lines : throw Failure(problem, null, "", "", historyPath);
    }

    private static string[] ReadHistoryLines(string historyPath, out string? problem)
    {
        problem = null;
        if (!File.Exists(historyPath))
        {
            problem = "history file is missing";
            return [];
        }

        var lines = File.ReadAllLines(historyPath).Where(line => line.Trim().Length > 0).ToArray();
        if (lines.Length == 0)
            problem = "history file is empty";
        return lines;
    }

    private static BenchChildException Failure(string what, int? exitCode, string stdout, string stderr, string historyPath) =>
        new($"{what}. Exit code: {(exitCode?.ToString() ?? "n/a")}. History path: {historyPath}" +
            $"{Environment.NewLine}--- stdout ---{Environment.NewLine}{stdout}" +
            $"{Environment.NewLine}--- stderr ---{Environment.NewLine}{stderr}");
}

internal sealed record BenchChildResult(int ExitCode, string Stdout, string Stderr, string[] HistoryLines);

internal sealed class BenchChildException(string message) : Exception(message);
