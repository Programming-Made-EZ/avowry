using System.Diagnostics;

namespace Phrase.Acceptance.Tests.Support;

/// <summary>
/// Card 217 (D157 Criterion 1): the shared asynchronous process runner. Both output streams are drained from the moment
/// the process starts (no pipe deadlock), exit is awaited without holding a thread-pool worker, and the result is only
/// returned once both drains finished. With a timeout, expiry kills the process tree, awaits termination and both drains,
/// and reports TimedOut instead of throwing.
/// </summary>
internal static class ProcessRunner
{
    internal static async Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, TimeSpan? timeout = null)
    {
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Process did not start: {startInfo.FileName}");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        var timedOut = false;

        using var cts = timeout is { } limit ? new CancellationTokenSource(limit) : new CancellationTokenSource();
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            timedOut = true;
            KillTreeIfRunning(process);
            await process.WaitForExitAsync();
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return new ProcessResult(process.ExitCode, stdout, stderr, timedOut);
    }

    private static void KillTreeIfRunning(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // Already exited between the check and the kill.
        }
    }
}

internal sealed record ProcessResult(int ExitCode, string Stdout, string Stderr, bool TimedOut);
