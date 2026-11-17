using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: a timed-out child never leaves processes behind
//   As Morgan, I want the shared helper to kill the whole process tree on timeout,
//   so that a hung nested build or server cannot outlive its test.
// Card 172 criterion 2. A child starts a grandchild and records both process ids; the helper times the child out;
// both ids must then be gone (polled with a bounded wait).
[Collection("Isolated timing")] // a test that waits on a real timeout runs alone, after every parallel collection has finished
public class NestedDotnetTimeout172
{
    private static readonly TimeSpan ChildTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan HelperBound = TimeSpan.FromSeconds(120);

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-2")]
    public async Task A_timed_out_child_and_its_grandchild_are_both_gone()
    {
        // Given a child that starts a grandchild, records both process ids, and then sleeps far past the timeout
        using var workspace = TempWorkspace.Create();
        var pidFile = Path.Combine(workspace.Root, "pids.txt");
        var info = SleepingTree(pidFile);
        using var stopWatching = new CancellationTokenSource();
        var watcher = WatchAsync(pidFile, stopWatching.Token);
        try
        {
            // When the shared helper runs it with a short timeout
            var timedOut = await NestedDotnetSeam172.RunAsync(info, ChildTimeout).WaitAsync(HelperBound);

            // Then the helper reports the timeout, and both processes are gone
            Assert.True(timedOut, "The helper did not report a timeout for a child that sleeps for ten minutes.");
            Assert.True(File.Exists(pidFile), "The child never recorded its process ids (it was killed before it started its grandchild?).");
            var pids = ReadPids(pidFile);
            Assert.Equal(2, pids.Count);
            Assert.NotEqual(pids[0], pids[1]);
            foreach (var pid in pids)
                Assert.True(await GoneAsync(pid), $"Process {pid} is still running after the helper timed its child out.");
        }
        finally
        {
            // Leave nothing behind even when the helper is still red or the test failed early: the pid file is read here,
            // and a process is killed only when its start time equals the one recorded while the pid file was being written
            // (a pid reused by an unrelated process has another start time and is never touched).
            stopWatching.Cancel();
            var recorded = await watcher;
            foreach (var pid in ReadPids(pidFile))
                if (recorded.TryGetValue(pid, out var started))
                    KillIfRunning(pid, started);
        }
    }

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-2")]
    public async Task A_child_that_finishes_in_time_is_not_reported_as_timed_out()
    {
        // Given a child that exits at once
        var info = OperatingSystem.IsWindows()
            ? new ProcessStartInfo("cmd.exe")
            : new ProcessStartInfo("sh");
        info.ArgumentList.Add(OperatingSystem.IsWindows() ? "/c" : "-c");
        info.ArgumentList.Add("echo done");
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.UseShellExecute = false;

        // When the shared helper runs it
        var timedOut = await NestedDotnetSeam172.RunAsync(info, ChildTimeout).WaitAsync(HelperBound);

        // Then it is not a timeout
        Assert.False(timedOut);
    }

    private static ProcessStartInfo SleepingTree(string pidFile)
    {
        ProcessStartInfo info;
        if (OperatingSystem.IsWindows())
        {
            info = new ProcessStartInfo("powershell.exe");
            foreach (var argument in new[]
            {
                "-NoProfile", "-NonInteractive", "-Command",
                "$p = Start-Process -FilePath ping -ArgumentList '-n','600','127.0.0.1' -PassThru -WindowStyle Hidden; "
                + "Set-Content -LiteralPath $env:TREE_PID_FILE -Value \"$PID $($p.Id)\"; Start-Sleep -Seconds 600",
            })
                info.ArgumentList.Add(argument);
        }
        else
        {
            info = new ProcessStartInfo("sh");
            info.ArgumentList.Add("-c");
            info.ArgumentList.Add("sleep 600 & echo \"$$ $!\" > \"$TREE_PID_FILE\"; wait");
        }
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.UseShellExecute = false;
        info.Environment["TREE_PID_FILE"] = pidFile;
        return info;
    }

    private static async Task<bool> GoneAsync(int pid)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            if (IsGone(pid))
                return true;
            await Task.Delay(200);
        }
        return IsGone(pid);
    }

    private static bool IsGone(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            return process.HasExited;
        }
        catch (ArgumentException)
        {
            return true;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    private static List<int> ReadPids(string pidFile)
    {
        try
        {
            return File.ReadAllText(pidFile).Split([' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(text => int.TryParse(text, out var pid) ? pid : -1).Where(pid => pid > 0).ToList();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    /// <summary>
    /// Polls from before the child starts: as soon as the pid file lists processes, records each one's start time while it is alive.
    /// </summary>
    private static async Task<Dictionary<int, DateTime>> WatchAsync(string pidFile, CancellationToken stop)
    {
        var recorded = new Dictionary<int, DateTime>();
        try
        {
            while (!stop.IsCancellationRequested)
            {
                var pids = ReadPids(pidFile);
                if (pids.Count >= 2)
                {
                    foreach (var pid in pids)
                        if (StartTimeOf(pid) is { } started)
                            recorded[pid] = started;
                    if (recorded.Count >= 2)
                        break;
                }
                await Task.Delay(100, stop);
            }
        }
        catch (OperationCanceledException)
        {
            // The test finished.
        }
        return recorded;
    }

    private static DateTime? StartTimeOf(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            return process.HasExited ? null : process.StartTime;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    private static void KillIfRunning(int pid, DateTime recordedStart)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            if (!process.HasExited && (process.StartTime - recordedStart).Duration() < TimeSpan.FromSeconds(1))
                process.Kill(entireProcessTree: true);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            // Already gone.
        }
    }
}
