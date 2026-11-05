using System.Globalization;

// Card 217 (D157 Criterion 1): the isolated thread-pool starvation probe child. It does NOT reference the acceptance assembly,
// so the acceptance ThreadPoolFloor module initializer (a 128-thread floor) is never loaded here and the pool keeps its own defaults.
//
// Admission protocol (adopted in the 217 slice window):
//   1. record the initial worker/IO minimum and maximum, print them;
//   2. clamp the worker maximum to the initial worker minimum m (the pool can never grow past m workers);
//   3. start N blockers, through ONE of two seams: "sync" blocks a pool thread (a ManualResetEventSlim.Wait, the way
//      ReadToEnd/WaitForExit do), "async" awaits a TaskCompletionSource and holds no worker;
//   4. the barrier counts entries up to min(N, m) only (never waits for more than m), and gives up after --barrier-timeout-ms,
//      printing the entry count it reached and exiting 3;
//   5. N = m + 1 on the sync seam: m enter, the extra one stays queued; the probe observes that after a short settle;
//   6. after the barrier, queue K readiness continuations and measure how many run inside --window-ms;
//   7. blockers are released in a finally.
// Arguments: --seam sync|async --blockers N|m|m+1|m-1 --waiters K --window-ms W [--barrier-timeout-ms T] [--barrier-target X|m+1]
// Output: one key=value per line on stdout. Exit 0 = the probe ran (the verdict is readiness-met), 3 = barrier failed, 2 = bad usage.
var options = Options.Parse(args);
if (options is null)
{
    Console.Error.WriteLine("usage: --seam sync|async --blockers N|m|m+1|m-1 --waiters K --window-ms W [--barrier-timeout-ms T] [--barrier-target X|m+1]");
    return 2;
}

ThreadPool.GetMinThreads(out var workerMin, out var ioMin);
ThreadPool.GetMaxThreads(out var workerMax, out var ioMax);
Print("initial-worker-min", workerMin);
Print("initial-worker-max", workerMax);
Print("initial-io-min", ioMin);
Print("initial-io-max", ioMax);
Print("acceptance-assembly-loaded", AppDomain.CurrentDomain.GetAssemblies().Any(a => (a.GetName().Name ?? "").StartsWith("Phrase.Acceptance", StringComparison.Ordinal)));

var m = workerMin;
if (!ThreadPool.SetMaxThreads(m, ioMax))
{
    Console.Error.WriteLine($"could not clamp the worker maximum to {m}");
    return 4;
}

ThreadPool.GetMaxThreads(out var clamped, out _);
Print("clamped-worker-max", clamped);
Print("seam", options.Seam);
Print("waiters", options.Waiters);
Print("window-ms", options.WindowMs);

var blockerCount = Count.Resolve(options.Blockers, m);
var target = options.BarrierTarget is { } forced ? Count.Resolve(forced, m) : Math.Min(blockerCount, m);
Print("blockers", blockerCount);
Print("barrier-target", target);

var entered = 0;
using var barrier = new ManualResetEventSlim(target == 0);
using var gate = new ManualResetEventSlim(false);
var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var blockerTasks = new List<Task>();

void Enter()
{
    if (Interlocked.Increment(ref entered) >= target)
        barrier.Set();
}

try
{
    for (var i = 0; i < blockerCount; i++)
    {
        if (options.Seam == "sync")
        {
            // Blocks a pool worker for as long as the gate is closed, as a synchronous ReadToEnd or WaitForExit does.
            blockerTasks.Add(Task.Factory.StartNew(() => { Enter(); gate.Wait(); }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default));
        }
        else
        {
            // Awaits the same gate without holding a worker.
            blockerTasks.Add(Task.Run(async () => { Enter(); await release.Task; }));
        }
    }

    if (!barrier.Wait(options.BarrierTimeoutMs))
    {
        Print("barrier", "failed");
        Print("entered", Volatile.Read(ref entered));
        Console.Error.WriteLine($"barrier failed: {Volatile.Read(ref entered)} of {target} blockers entered within {options.BarrierTimeoutMs} ms");
        return 3;
    }

    Print("barrier", "ok");
    Thread.Sleep(300); // settle: an extra blocker beyond m must still be queued, not entered
    var enteredNow = Volatile.Read(ref entered);
    Print("entered", enteredNow);
    Print("extra-queued", blockerCount > m && enteredNow == m);

    var met = 0;
    using var allMet = new ManualResetEventSlim(options.Waiters == 0);
    for (var i = 0; i < options.Waiters; i++)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (Interlocked.Increment(ref met) >= options.Waiters)
                allMet.Set();
        });
    }

    allMet.Wait(options.WindowMs);
    var metInWindow = Volatile.Read(ref met);
    Print("readiness-met-count", metInWindow);
    Print("readiness-met", metInWindow == options.Waiters);
    return 0;
}
finally
{
    gate.Set();
    release.TrySetResult();
    Task.WaitAll(blockerTasks.ToArray(), TimeSpan.FromSeconds(10));
}

static void Print(string key, object value) =>
    Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{key}={(value is bool b ? (b ? "true" : "false") : value)}"));

// A count is a number ("0", "5") or relative to the initial worker minimum m ("m", "m+1", "m-1").
internal static class Count
{
    internal static bool IsValid(string spec) => TryResolve(spec, 0, out _);

    internal static int Resolve(string spec, int m) => TryResolve(spec, m, out var value) ? value : throw new FormatException(spec);

    private static bool TryResolve(string spec, int m, out int value)
    {
        value = 0;
        if (spec == "m") { value = m; return true; }
        if (spec.StartsWith("m+", StringComparison.Ordinal) && int.TryParse(spec[2..], NumberStyles.None, CultureInfo.InvariantCulture, out var up)) { value = m + up; return true; }
        if (spec.StartsWith("m-", StringComparison.Ordinal) && int.TryParse(spec[2..], NumberStyles.None, CultureInfo.InvariantCulture, out var down)) { value = Math.Max(0, m - down); return true; }
        return int.TryParse(spec, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}

internal sealed record Options(string Seam, string Blockers, int Waiters, int WindowMs, int BarrierTimeoutMs, string? BarrierTarget)
{
    internal static Options? Parse(string[] args)
    {
        string? seam = null;
        string? blockers = null, target = null;
        int? waiters = null, window = null;
        var timeout = 20000;
        for (var i = 0; i + 1 < args.Length; i += 2)
        {
            _ = int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var number);
            var isCount = args[i] is "--blockers" or "--barrier-target";
            if (args[i] != "--seam" && (isCount ? !Count.IsValid(args[i + 1]) : number == 0 && args[i + 1] != "0"))
                return null;
            switch (args[i])
            {
                case "--seam": seam = args[i + 1]; break;
                case "--blockers": blockers = args[i + 1]; break;
                case "--waiters": waiters = number; break;
                case "--window-ms": window = number; break;
                case "--barrier-timeout-ms": timeout = number; break;
                case "--barrier-target": target = args[i + 1]; break;
                default: return null;
            }
        }

        if (seam is not ("sync" or "async") || blockers is null || waiters is null || window is null)
            return null;
        return new Options(seam, blockers, waiters.Value, window.Value, timeout, target);
    }
}
