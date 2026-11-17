using System.Diagnostics;

namespace Avowry.Acceptance.Tests.Support;

/// <summary>
/// Card 217 (D157 Criterion 1): the shared asynchronous process runner. Both output streams are drained from the moment
/// the process starts (no pipe deadlock), exit is awaited without holding a thread-pool worker, and the result is only
/// returned once both drains finished. With a timeout, expiry kills the process tree, awaits termination and both drains,
/// and reports TimedOut instead of throwing.
/// </summary>
internal static class ProcessRunner
{
    internal static Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, TimeSpan? timeout = null) =>
        NestedDotnet.RunAsync(startInfo, timeout ?? TimeSpan.FromMinutes(5));
}

internal sealed record ProcessResult(int ExitCode, string Stdout, string Stderr, bool TimedOut);