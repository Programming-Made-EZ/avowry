using System.Diagnostics;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Card 217 red 1 (D157 Criterion 1): the deterministic, isolated thread-pool starvation probe.
//
// The probe is a child process (tests/Fixtures/StarvationProbe, built through a ProjectReference like QuietChild). It does not
// reference this assembly, so the 128-thread ThreadPoolFloor module initializer is never loaded in it. Admission protocol:
// the child records the initial worker minimum m, clamps the worker maximum to m, starts N blockers, counts barrier entries up
// to m only, then queues K readiness continuations and reports how many ran inside a fixed window. The synchronous seam
// (a pool worker blocked on a gate, like ReadToEnd/WaitForExit) must MISS the window with N >= m; the asynchronous seam
// (an awaited gate, no worker held) must MEET the very same window. Only --seam differs between the red and green runs.
//
// Proposed seam (the green creates it): tests/Phrase.Acceptance.Tests/Support/ProcessRunner.cs, the shared runner of
// design decision 1:
//   internal static class ProcessRunner
//     static Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, TimeSpan? timeout = null)
//       Process.Start, then stdout and stderr ReadToEndAsync started right after Start, exit awaited asynchronously
//       (WaitForExitAsync, never a blocking wait), then both drains awaited; returns an immutable result.
//       With a timeout: on expiry the process tree is killed, termination and both drains are awaited, TimedOut is true.
//   internal sealed record ProcessResult(int ExitCode, string Stdout, string Stderr, bool TimedOut)
// This file does not compile until that type exists: the first red is that compile failure.
public class StarvationProbe
{
    private const int WindowMs = 3000;
    private const int Waiters = 4;
    private static readonly TimeSpan Watchdog = TimeSpan.FromSeconds(90);

    [Fact]
    [Trait("Slice", "217")]
    public async Task Zero_blockers_meet_the_readiness_window_on_both_seams()
    {
        foreach (var seam in new[] { "sync", "async" })
        {
            var run = await Probe(seam, blockers: "0");

            Assert.Equal("ok", run["barrier"]);
            Assert.Equal("0", run["entered"]);
            Assert.Equal("true", run["readiness-met"]);
            Assert.Equal(Waiters.ToString(), run["readiness-met-count"]);
        }
    }

    [Theory]
    [InlineData("m")]
    [InlineData("m+1")]
    [Trait("Slice", "217")]
    public async Task Synchronous_blockers_fill_every_worker_and_readiness_misses_its_window(string blockers)
    {
        var run = await Probe("sync", blockers);

        var m = int.Parse(run["initial-worker-min"]);
        Assert.Equal(m.ToString(), run["clamped-worker-max"]);
        Assert.Equal("ok", run["barrier"]);
        Assert.Equal(m.ToString(), run["entered"]);
        Assert.Equal("false", run["readiness-met"]);
        Assert.Equal("0", run["readiness-met-count"]);
    }

    [Fact]
    [Trait("Slice", "217")]
    public async Task One_more_synchronous_blocker_than_workers_is_observed_queued_not_entered()
    {
        var atMin = await Probe("sync", "m");
        var overMin = await Probe("sync", "m+1");

        Assert.Equal("false", atMin["extra-queued"]);
        Assert.Equal("true", overMin["extra-queued"]);
        Assert.Equal(atMin["entered"], overMin["entered"]);
        Assert.Equal(int.Parse(atMin["blockers"]) + 1, int.Parse(overMin["blockers"]));
    }

    [Fact]
    [Trait("Slice", "217")]
    public async Task One_spare_worker_keeps_synchronous_readiness_inside_the_window()
    {
        var run = await Probe("sync", "m-1");

        Assert.Equal("ok", run["barrier"]);
        Assert.Equal("true", run["readiness-met"]);
    }

    [Theory]
    [InlineData("m")]
    [InlineData("m+1")]
    [Trait("Slice", "217")]
    public async Task Asynchronous_blockers_leave_the_workers_free_and_readiness_meets_the_same_window(string blockers)
    {
        var run = await Probe("async", blockers);

        Assert.Equal("ok", run["barrier"]);
        Assert.Equal("true", run["readiness-met"]);
        Assert.Equal(Waiters.ToString(), run["readiness-met-count"]);
        Assert.Equal(WindowMs.ToString(), run["window-ms"]);
        Assert.Equal("false", run["extra-queued"]);
    }

    [Fact]
    [Trait("Slice", "217")]
    public async Task Only_the_seam_differs_between_the_red_and_green_runs()
    {
        var red = await Probe("sync", "m+1");
        var green = await Probe("async", "m+1");

        foreach (var key in new[] { "initial-worker-min", "initial-worker-max", "initial-io-min", "initial-io-max", "clamped-worker-max", "blockers", "waiters", "window-ms", "barrier-target" })
            Assert.Equal(red[key], green[key]);
        Assert.NotEqual(red["seam"], green["seam"]);
        Assert.NotEqual(red["readiness-met"], green["readiness-met"]);
    }

    [Fact]
    [Trait("Slice", "217")]
    public async Task The_child_prints_its_initial_limits_and_never_loads_the_acceptance_assembly_or_its_floor()
    {
        var run = await Probe("async", "0");

        var workerMin = int.Parse(run["initial-worker-min"]);
        Assert.True(workerMin > 0 && workerMin <= int.Parse(run["initial-worker-max"]), $"worker min {workerMin}, max {run["initial-worker-max"]}");
        Assert.True(int.Parse(run["initial-io-min"]) > 0 && int.Parse(run["initial-io-min"]) <= int.Parse(run["initial-io-max"]));
        Assert.Equal("false", run["acceptance-assembly-loaded"]);
        // The default minimum is the processor count; the floor would have made it at least 128.
        if (Environment.ProcessorCount < 128 && Environment.GetEnvironmentVariable("DOTNET_ThreadPool_MinThreads") is null)
            Assert.Equal(Environment.ProcessorCount, workerMin);
    }

    [Fact]
    [Trait("Slice", "217")]
    public async Task A_barrier_that_cannot_complete_fails_with_its_entry_count_and_does_not_hang()
    {
        var info = StartInfo("sync", "m", barrierTarget: "m+1", barrierTimeoutMs: 1500);

        var result = await ProcessRunner.RunAsync(info, Watchdog);

        Assert.False(result.TimedOut, "the child hung past the watchdog:" + Environment.NewLine + result.Stdout + result.Stderr);
        Assert.Equal(3, result.ExitCode);
        var values = Parse(result.Stdout);
        Assert.Equal("failed", values["barrier"]);
        Assert.Equal(values["initial-worker-min"], values["entered"]);
        Assert.Contains($"barrier failed: {values["entered"]} of {int.Parse(values["entered"]) + 1} blockers entered", result.Stderr);
    }

    private static async Task<Dictionary<string, string>> Probe(string seam, string blockers)
    {
        var result = await ProcessRunner.RunAsync(StartInfo(seam, blockers), Watchdog);

        Assert.False(result.TimedOut, "the probe child hit the watchdog:" + Environment.NewLine + result.Stdout + result.Stderr);
        Assert.True(result.ExitCode == 0, $"probe child exited {result.ExitCode}:{Environment.NewLine}{result.Stdout}{Environment.NewLine}{result.Stderr}");
        return Parse(result.Stdout);
    }

    private static ProcessStartInfo StartInfo(string seam, string blockers, string? barrierTarget = null, int? barrierTimeoutMs = null)
    {
        var dll = Path.Combine(AppContext.BaseDirectory, "StarvationProbe.dll");
        Assert.True(File.Exists(dll), $"StarvationProbe.dll was not found at {dll}.");

        var info = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        // The child is a net8 build; a host with only a newer runtime still runs it.
        info.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        foreach (var argument in new[] { dll, "--seam", seam, "--blockers", blockers, "--waiters", Waiters.ToString(), "--window-ms", WindowMs.ToString() })
            info.ArgumentList.Add(argument);
        if (barrierTarget is not null)
        {
            info.ArgumentList.Add("--barrier-target");
            info.ArgumentList.Add(barrierTarget);
        }
        if (barrierTimeoutMs is not null)
        {
            info.ArgumentList.Add("--barrier-timeout-ms");
            info.ArgumentList.Add(barrierTimeoutMs.Value.ToString());
        }
        return info;
    }

    private static Dictionary<string, string> Parse(string stdout) =>
        stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Contains('='))
            .ToDictionary(line => line[..line.IndexOf('=')], line => line[(line.IndexOf('=') + 1)..]);
}
