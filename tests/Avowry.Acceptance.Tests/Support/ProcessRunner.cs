using System.Diagnostics;

namespace Avowry.Acceptance.Tests.Support;

/// <summary>
/// Card 217 (D157 Criterion 1): the shared asynchronous process runner. Both output streams are drained from the moment
/// the process starts (no pipe deadlock), exit is awaited without holding a thread-pool worker, and the result is only
/// returned once both drains finished. With a timeout, expiry kills the process tree, awaits termination and both drains,
/// and throws TimeoutException naming the command and the limit. With no timeout, the run is not time-limited.
/// </summary>
internal static class ProcessRunner
{
    internal static async Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, TimeSpan? timeout = null)
    {
        var result = await NestedDotnet.RunAsync(startInfo, timeout);
        if (result.TimedOut && timeout is { } limit)
        {
            var command = string.IsNullOrEmpty(startInfo.Arguments)
                ? startInfo.FileName
                : $"{startInfo.FileName} {startInfo.Arguments}";
            throw new TimeoutException($"{command} exceeded {limit}.");
        }

        return result;
    }
}

internal sealed record ProcessResult(int ExitCode, string Stdout, string Stderr, bool TimedOut);