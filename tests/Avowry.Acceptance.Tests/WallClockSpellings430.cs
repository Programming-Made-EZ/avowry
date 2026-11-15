using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Card 430 red: the D263 and D265 real-shape corpus (.mob/records/430/corpus.txt, embedded below row for row; a completeness
// fact reads the file and checks that every row is embedded). Second part of the guard class (card 417) so these rows reach
// its private Find, Src, Describe and LinesOf. Row ids are <family>-<NN>/<corpus id>; NN counts within the family in corpus order.
// Criteria: 430-1 [Z][O] (a clean class; the Elapsed and CancellationTokenSource shapes of real tests); 430-2 [M] (the new
// spellings, their neighbours that stay unnamed, the encodings of each); 430-3 [B] (near misses and collection rows whose
// outcome does not change); 430-4 [I] (test-type shapes); 430-5 [E] (the nine real classes carry the isolated collection).
// Pending theories hold the rows the card 417 guard gets wrong; the Characterised theories hold the rows it already gets right.
// No wall-clock spelling is written in code here: every row is a string, so the live-tree fact does not name this class.
public partial class WallClockTestsRunIsolated
{
    private static IEnumerable<object[]> Embed((string Id, string Text, string Expected)[] rows) =>
        rows.Select(r => new object[] { r.Id, r.Text, r.Expected });

    private static readonly (string Id, string Text, string Expected)[] Rows430_1_elapsed_Characterised =
    [
        ("elapsed-01/Z1",
         "public class Clean\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(true);\n    }\n}\n",
         "-"),
        ("elapsed-03/E-real",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        var stopwatch = Stopwatch.StartNew();\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "Clocked"),
    ];

    public static IEnumerable<object[]> Rows430_1_elapsed_Characterised_Data() => Embed(Rows430_1_elapsed_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_1_elapsed_Pending =
    [
        ("elapsed-02/O1",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        var stopwatch = Stopwatch.StartNew();\n        Assert.True(stopwatch.ElapsedMilliseconds < 20000, $\"took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "Clocked"),
    ];

    public static IEnumerable<object[]> Rows430_1_elapsed_Pending_Data() => Embed(Rows430_1_elapsed_Pending);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_elapsed_Pending =
    [
        ("elapsed-04/M-ms-var",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.ElapsedMilliseconds < limitMs, $\"took {stopwatch.ElapsedMilliseconds} ms\");\n    }\n}\n",
         "Clocked"),
        ("elapsed-06/M-inrange-ms",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.InRange(stopwatch.ElapsedMilliseconds, 0, 20000);\n    }\n}\n",
         "Clocked"),
        ("elapsed-07/M-inrange-span",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.InRange(stopwatch.Elapsed, TimeSpan.Zero, limit);\n    }\n}\n",
         "Clocked"),
        ("elapsed-08/M-inrange-generic",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.InRange<long>(clock.ElapsedMilliseconds, 0L, 5000L);\n    }\n}\n",
         "Clocked"),
        ("elapsed-09/M-reversed",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(limit > stopwatch.Elapsed);\n    }\n}\n",
         "Clocked"),
        ("elapsed-10/M-reversed-ge",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(20000 >= stopwatch.ElapsedMilliseconds);\n    }\n}\n",
         "Clocked"),
        ("elapsed-11/M-nospace",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed<limit);\n    }\n}\n",
         "Clocked"),
        ("elapsed-12/M-nospace-span",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed<TimeSpan.FromSeconds(20));\n    }\n}\n",
         "Clocked"),
        ("elapsed-13/M-le",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.ElapsedMilliseconds <= 20000);\n    }\n}\n",
         "Clocked"),
        ("elapsed-14/M-ticks",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.ElapsedTicks < limit.Ticks);\n    }\n}\n",
         "Clocked"),
        ("elapsed-15/M-total-minutes",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True((DateTime.UtcNow - started).TotalMinutes < 2);\n    }\n}\n",
         "Clocked"),
        ("elapsed-23/X-elapsed-code",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.ElapsedMilliseconds < limitMs);\n    }\n}\n",
         "Enc"),
        ("elapsed-30/X-elapsed-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"took {(stopwatch.ElapsedMilliseconds < limitMs ? \"in\" : \"over\")}\";\n    }\n}\n",
         "Enc"),
        ("elapsed-33/X-elapsed-raw-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"took {{(stopwatch.ElapsedMilliseconds < limitMs ? \"in\" : \"over\")}}\"\"\";\n    }\n}\n",
         "Enc"),
    ];

    public static IEnumerable<object[]> Rows430_2_elapsed_Pending_Data() => Embed(Rows430_2_elapsed_Pending);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_elapsed_Characterised =
    [
        ("elapsed-05/M-total-var",
         "public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed.TotalSeconds < limitSeconds);\n    }\n}\n",
         "Clocked"),
        ("elapsed-16/N-e-writeline",
         "public class Timed\n{\n    [Fact]\n    public async Task T()\n    {\n        var clock = Stopwatch.StartNew();\n        output.WriteLine($\"shape={unit} length={text.Length} first_pass_ms={clock.Elapsed.TotalMilliseconds:F1}\");\n    }\n}\n",
         "-"),
        ("elapsed-17/N-e-argument",
         "public class Timed\n{\n    [Fact]\n    public async Task T()\n    {\n        return new MutationCheckResult(phrases, total, survivors, started.Elapsed, summary, report);\n    }\n}\n",
         "-"),
        ("elapsed-18/N-e-lambda",
         "public class Timed\n{\n    [Fact]\n    public async Task T()\n    {\n        Func<TimeSpan> took = () => started.Elapsed;\n        Assert.NotNull(took);\n    }\n}\n",
         "-"),
        ("elapsed-19/N-e-minutes-hole",
         "public class Timed\n{\n    [Fact]\n    public async Task T()\n    {\n        throw new TimeoutException($\"dotnet publish process {pid} exceeded {PublishTimeout.TotalMinutes:0} minutes.\");\n    }\n}\n",
         "-"),
        ("elapsed-20/N-e-inrange-count",
         "public class Counted\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.InRange(model.Module.Endpoints.Count, 3, 5);\n    }\n}\n",
         "-"),
        ("elapsed-21/N-e-generic",
         "public class Timed\n{\n    [Fact]\n    public async Task T()\n    {\n        var t = Probe.Elapsed<TimeSpan>(run);\n        Assert.True(t > TimeSpan.Zero);\n    }\n}\n",
         "-"),
        ("elapsed-22/N-e-greater",
         "public class Timed\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed > TimeSpan.FromMilliseconds(100));\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_2_elapsed_Characterised_Data() => Embed(Rows430_2_elapsed_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_1_cts_Characterised =
    [
        ("cts-01/C-real",
         "public class Capped\n{\n    [Fact]\n    public async Task T()\n    {\n        using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(20));\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Capped"),
    ];

    public static IEnumerable<object[]> Rows430_1_cts_Characterised_Data() => Embed(Rows430_1_cts_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_cts_Pending =
    [
        ("cts-02/M-cts-minutes",
         "public class Capped\n{\n    [Fact]\n    public async Task T()\n    {\n        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Capped"),
        ("cts-03/M-cts-variable",
         "public class Capped\n{\n    [Fact]\n    public async Task T()\n    {\n        using var cts = new CancellationTokenSource(Timeout);\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Capped"),
        ("cts-04/M-cts-target",
         "public class Capped\n{\n    [Fact]\n    public async Task T()\n    {\n        CancellationTokenSource limit = new(TimeSpan.FromSeconds(20));\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Capped"),
        ("cts-05/M-cts-target-using",
         "public class Capped\n{\n    [Fact]\n    public async Task T()\n    {\n        using CancellationTokenSource limit = new (ParallelTimeout);\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Capped"),
        ("cts-06/M-cancelafter",
         "public class Capped\n{\n    [Fact]\n    public async Task T()\n    {\n        using var attempt = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);\n        attempt.CancelAfter(TimeSpan.FromMilliseconds(500));\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Capped"),
        ("cts-12/X-cts-code",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        using CancellationTokenSource limit = new(ParallelTimeout);\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Enc"),
        ("cts-19/X-cts-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"took {new CancellationTokenSource(ParallelTimeout).Token}\";\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Enc"),
        ("cts-22/X-cts-raw-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"took {{new CancellationTokenSource(ParallelTimeout).Token}}\"\"\";\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "Enc"),
    ];

    public static IEnumerable<object[]> Rows430_2_cts_Pending_Data() => Embed(Rows430_2_cts_Pending);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_cts_Characterised =
    [
        ("cts-07/N-c-no-message",
         "public class Docs\n{\n    [Fact]\n    public async Task T()\n    {\n        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));\n        var result = await ProcessRunner.RunAsync(start, timeout.Token);\n    }\n}\n",
         "-"),
        ("cts-08/N-c-seconds-no-message",
         "public class Docs\n{\n    [Fact]\n    public async Task T()\n    {\n        using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(20));\n        await process.WaitForExitAsync(limit.Token);\n    }\n}\n",
         "-"),
        ("cts-09/N-c-empty",
         "public class Stopper\n{\n    [Fact]\n    public async Task T()\n    {\n        using var stop = new CancellationTokenSource();\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-10/N-c-target-empty",
         "public class Stopper\n{\n    [Fact]\n    public async Task T()\n    {\n        CancellationTokenSource _stop = new();\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-11/N-c-linked",
         "public class Linked\n{\n    [Fact]\n    public async Task T()\n    {\n        using var attempt = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_2_cts_Characterised_Data() => Embed(Rows430_2_cts_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_waitasync_Pending =
    [
        ("waitasync-01/W-real-var",
         "public class Parallel\n{\n    [Fact]\n    public async Task T()\n    {\n        var created = await Task.WhenAll(ids.Select(id => SendAsync(http, HttpMethod.Post, create.Route, body))).WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Parallel"),
        ("waitasync-02/W-real-span",
         "public class Hazards\n{\n    [Fact]\n    public async Task T()\n    {\n        var error = await Record.ExceptionAsync(() => GeneratedApi.StartAsync(\"unused.csproj\", options).WaitAsync(TimeSpan.FromSeconds(10)));\n    }\n}\n",
         "Hazards"),
        ("waitasync-03/W-real-exit",
         "public class Exits\n{\n    [Fact]\n    public async Task T()\n    {\n        await _process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));\n    }\n}\n",
         "Exits"),
        ("waitasync-04/W-span-and-token",
         "public class Settles\n{\n    [Fact]\n    public async Task T()\n    {\n        await exited.WaitAsync(ExitSettle, timeout.Token);\n    }\n}\n",
         "Settles"),
        ("waitasync-13/X-waitasync-code",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Enc"),
        ("waitasync-20/X-waitasync-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"took {await work.WaitAsync(ParallelTimeout)}\";\n    }\n}\n",
         "Enc"),
        ("waitasync-23/X-waitasync-raw-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"took {{await work.WaitAsync(ParallelTimeout)}}\"\"\";\n    }\n}\n",
         "Enc"),
    ];

    public static IEnumerable<object[]> Rows430_2_waitasync_Pending_Data() => Embed(Rows430_2_waitasync_Pending);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_waitasync_Characterised =
    [
        ("waitasync-05/N-w-empty",
         "public class Gate\n{\n    [Fact]\n    public async Task T()\n    {\n        await BuildGate.WaitAsync();\n    }\n}\n",
         "-"),
        ("waitasync-06/N-w-token",
         "public class Gate\n{\n    [Fact]\n    public async Task T()\n    {\n        var (address, rejected) = await _reported.Task.WaitAsync(timeout.Token);\n    }\n}\n",
         "-"),
        ("waitasync-07/N-w-cancellation-token",
         "public class Gate\n{\n    [Fact]\n    public async Task T()\n    {\n        await gate.WaitAsync(cancellationToken);\n    }\n}\n",
         "-"),
        ("waitasync-08/N-w-ct",
         "public class Gate\n{\n    [Fact]\n    public async Task T()\n    {\n        await gate.WaitAsync(ct);\n    }\n}\n",
         "-"),
        ("waitasync-09/N-w-none",
         "public class Gate\n{\n    [Fact]\n    public async Task T()\n    {\n        await gate.WaitAsync(CancellationToken.None);\n    }\n}\n",
         "-"),
        ("waitasync-10/N-w-context-token",
         "public class Gate\n{\n    [Fact]\n    public async Task T()\n    {\n        await gate.WaitAsync(TestContext.Current.CancellationToken);\n    }\n}\n",
         "-"),
        ("waitasync-11/N-w-inlinedata",
         "public class Waits\n{\n    [Theory]\n    [InlineData(\"await task.WaitAsync(TimeSpan.FromSeconds(1));\", 0)]\n    public void T(string line, int hits) { Assert.Equal(hits, Scan(line)); }\n}\n",
         "-"),
        ("waitasync-12/N-w-regex-verbatim",
         "public class Source\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.Single(Regex.Matches(source, @\"WaitForExitAsync\\(\\)\\.WaitAsync\\(TimeSpan\\.FromSeconds\\(15\\)\\)\"));\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_2_waitasync_Characterised_Data() => Embed(Rows430_2_waitasync_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_whenany_Pending =
    [
        ("whenany-01/A-real",
         "public class Bound\n{\n    [Fact]\n    public async Task T()\n    {\n        var early = await Task.WhenAny(start, Task.Delay(TimeSpan.FromSeconds(1.5)));\n        Assert.False(early == start, \"StartAsync returned before the child reported an address.\");\n    }\n}\n",
         "Bound"),
        ("whenany-02/M-whenany-var",
         "public class Raced\n{\n    [Fact]\n    public async Task T()\n    {\n        var delay = Task.Delay(Limit);\n        var first = await Task.WhenAny(work, delay);\n        Assert.True(first == work, \"work did not finish within the limit\");\n    }\n}\n",
         "Raced"),
        ("whenany-06/X-whenany-code",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var first = await Task.WhenAny(work, Task.Delay(Limit));\n    }\n}\n",
         "Enc"),
        ("whenany-13/X-whenany-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"took {await Task.WhenAny(work, Task.Delay(Limit))}\";\n    }\n}\n",
         "Enc"),
        ("whenany-16/X-whenany-raw-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"took {{await Task.WhenAny(work, Task.Delay(Limit))}}\"\"\";\n    }\n}\n",
         "Enc"),
    ];

    public static IEnumerable<object[]> Rows430_2_whenany_Pending_Data() => Embed(Rows430_2_whenany_Pending);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_whenany_Characterised =
    [
        ("whenany-03/N-a-delay-alone",
         "public class Released\n{\n    [Fact]\n    public async Task T()\n    {\n        var release = Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ => File.WriteAllText(gate, \"go\"));\n    }\n}\n",
         "-"),
        ("whenany-04/N-a-whenany-alone",
         "public class Raced\n{\n    [Fact]\n    public async Task T()\n    {\n        var first = await Task.WhenAny(_reported.Task, exited);\n    }\n}\n",
         "-"),
        ("whenany-05/N-a-poll",
         "public class Polled\n{\n    [Fact]\n    public async Task T()\n    {\n        while (!ready)\n            await Task.Delay(250, timeout.Token);\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_2_whenany_Characterised_Data() => Embed(Rows430_2_whenany_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_httpclient_Pending =
    [
        ("httpclient-01/H-real",
         "public class Mapped\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = TimeSpan.FromSeconds(30) };\n    }\n}\n",
         "Mapped"),
        ("httpclient-02/H-real-var",
         "public class Spec\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = SpecTimeout };\n    }\n}\n",
         "Spec"),
        ("httpclient-03/H-real-only",
         "public class Probe\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };\n    }\n}\n",
         "Probe"),
        ("httpclient-04/M-http-handler",
         "public class Probe\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient(handler, disposeHandler: true) { Timeout = TimeSpan.FromSeconds(10) };\n    }\n}\n",
         "Probe"),
        ("httpclient-05/M-http-target",
         "public class Probe\n{\n    [Fact]\n    public async Task T()\n    {\n        using HttpClient http = new() { Timeout = TimeSpan.FromSeconds(10) };\n    }\n}\n",
         "Probe"),
        ("httpclient-06/M-http-assign",
         "public class Probe\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient();\n        http.Timeout = TimeSpan.FromSeconds(10);\n    }\n}\n",
         "Probe"),
        ("httpclient-13/X-httpclient-code",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient { Timeout = SpecTimeout };\n    }\n}\n",
         "Enc"),
        ("httpclient-20/X-httpclient-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"took {new HttpClient { Timeout = SpecTimeout }.Timeout}\";\n    }\n}\n",
         "Enc"),
        ("httpclient-23/X-httpclient-raw-interp-hole",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"took {{new HttpClient { Timeout = SpecTimeout }.Timeout}}\"\"\";\n    }\n}\n",
         "Enc"),
    ];

    public static IEnumerable<object[]> Rows430_2_httpclient_Pending_Data() => Embed(Rows430_2_httpclient_Pending);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_httpclient_Characterised =
    [
        ("httpclient-07/N-h-no-timeout",
         "public class Plain\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl) };\n    }\n}\n",
         "-"),
        ("httpclient-08/N-h-infinite",
         "public class Plain\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };\n    }\n}\n",
         "-"),
        ("httpclient-09/N-h-infinite-assign",
         "public class Plain\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient();\n        http.Timeout = System.Threading.Timeout.InfiniteTimeSpan;\n    }\n}\n",
         "-"),
        ("httpclient-10/N-h-field",
         "public class Fielded\n{\n    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient();\n        var text = await http.GetStringAsync(url);\n    }\n}\n",
         "-"),
        ("httpclient-11/N-h-startup",
         "public class Seams\n{\n    [Fact]\n    public async Task T()\n    {\n        var options = new GeneratedApiStartOptions\n        {\n            StartupTimeout = TimeSpan.FromSeconds(1),\n        };\n    }\n}\n",
         "-"),
        ("httpclient-12/N-h-compare",
         "public class Plain\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient();\n        Assert.True(http.Timeout == TimeSpan.FromSeconds(100));\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_2_httpclient_Characterised_Data() => Embed(Rows430_2_httpclient_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_3_process_Characterised =
    [
        ("process-01/B-timeout-ms-argument",
         "public class Near\n{\n    [Fact]\n    public async Task T()\n    {\n        var a = new[] { \"--timeout-ms\", \"20000\" };\n    }\n}\n",
         "-"),
        ("process-02/B-wait-for-exit-limit",
         "public class Near\n{\n    [Fact]\n    public async Task T()\n    {\n        await process.WaitForExitAsync(TimeSpan.FromSeconds(5));\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_3_process_Characterised_Data() => Embed(Rows430_3_process_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_2_process_Characterised =
    [
        ("process-03/N-p-runner",
         "public class Runner\n{\n    [Fact]\n    public async Task T()\n    {\n        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(120000));\n    }\n}\n",
         "-"),
        ("process-04/N-p-quietchild",
         "public class Child\n{\n    [Fact]\n    public async Task T()\n    {\n        var options = Probes.QuietChild([\"--eprint\", \"z-log-line\"], TimeSpan.FromSeconds(5));\n    }\n}\n",
         "-"),
        ("process-05/N-p-waitall",
         "public class Starve\n{\n    [Fact]\n    public async Task T()\n    {\n        Task.WaitAll(blockerTasks.ToArray(), TimeSpan.FromSeconds(10));\n    }\n}\n",
         "-"),
        ("process-06/N-p-fresh",
         "public class Fresh\n{\n    [Fact]\n    public async Task T()\n    {\n        PhraseCli.EnsureFresh(\"Avowry.Cli.dll\", [\"newer.cs\"], LastWriteUtc, TimeSpan.FromSeconds(2));\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_2_process_Characterised_Data() => Embed(Rows430_2_process_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_3_collection_Characterised =
    [
        ("collection-01/O3-above",
         "[Collection(\"Isolated timing\")]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "-"),
        ("collection-02/O3-same-line",
         "[Collection(\"Isolated timing\")] public class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "-"),
        ("collection-03/O3-comma-list",
         "[Collection(\"Isolated timing\"), Trait(\"Slice\", \"1\")]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "-"),
        ("collection-04/O3-after-a-comment",
         "// keep the attribute\n[Collection(\"Isolated timing\")]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "-"),
        ("collection-05/O3-xunit-qualified",
         "[Xunit.Collection(\"Isolated timing\")]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "-"),
        ("collection-06/B-wrong-case-collection",
         "[Collection(\"Isolated Timing\")]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "Clocked"),
        ("collection-07/B-bench-processes",
         "[Collection(\"Bench processes\")]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "Clocked"),
        ("collection-08/B-collection-attribute",
         "[CollectionAttribute(\"Isolated timing\")]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "Clocked"),
        ("collection-09/B-constant",
         "const string N = \"Isolated timing\";\n[Collection(N)]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "Clocked"),
        ("collection-10/B-commented-out",
         "// [Collection(\"Isolated timing\")]\npublic class Clocked\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $\"{arm.Project} took {stopwatch.Elapsed}.\");\n    }\n}\n",
         "Clocked"),
        ("collection-11/K-new-spelling-isolated",
         "[Collection(\"Isolated timing\")]\npublic class Spec\n{\n    [Fact]\n    public async Task T()\n    {\n        using var http = new HttpClient { Timeout = SpecTimeout };\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_3_collection_Characterised_Data() => Embed(Rows430_3_collection_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_3_elapsed_Characterised =
    [
        ("elapsed-24/X-elapsed-line-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        // stopwatch.ElapsedMilliseconds < limitMs\n    }\n}\n",
         "-"),
        ("elapsed-25/X-elapsed-block-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /* stopwatch.ElapsedMilliseconds < limitMs */\n    }\n}\n",
         "-"),
        ("elapsed-26/X-elapsed-doc-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /// <summary>stopwatch.ElapsedMilliseconds < limitMs</summary>\n    }\n}\n",
         "-"),
        ("elapsed-27/X-elapsed-string",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"stopwatch.ElapsedMilliseconds < limitMs\";\n    }\n}\n",
         "-"),
        ("elapsed-28/X-elapsed-verbatim",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = @\"stopwatch.ElapsedMilliseconds < limitMs\";\n    }\n}\n",
         "-"),
        ("elapsed-29/X-elapsed-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"x={x} stopwatch.ElapsedMilliseconds < limitMs\";\n    }\n}\n",
         "-"),
        ("elapsed-31/X-elapsed-raw",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"\"\"\n            stopwatch.ElapsedMilliseconds < limitMs\n            \"\"\";\n    }\n}\n",
         "-"),
        ("elapsed-32/X-elapsed-raw-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"x={{x}} stopwatch.ElapsedMilliseconds < limitMs\"\"\";\n    }\n}\n",
         "-"),
        ("elapsed-34/X-elapsed-char",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var c = '<';\n        var d = '>';\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_3_elapsed_Characterised_Data() => Embed(Rows430_3_elapsed_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_3_cts_Characterised =
    [
        ("cts-13/X-cts-line-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        // CancellationTokenSource limit = new(ParallelTimeout)\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-14/X-cts-block-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /* CancellationTokenSource limit = new(ParallelTimeout) */\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-15/X-cts-doc-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /// <summary>CancellationTokenSource limit = new(ParallelTimeout)</summary>\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-16/X-cts-string",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"CancellationTokenSource limit = new(ParallelTimeout)\";\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-17/X-cts-verbatim",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = @\"CancellationTokenSource limit = new(ParallelTimeout)\";\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-18/X-cts-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"x={x} CancellationTokenSource limit = new(ParallelTimeout)\";\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-20/X-cts-raw",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"\"\"\n            CancellationTokenSource limit = new(ParallelTimeout)\n            \"\"\";\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-21/X-cts-raw-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"x={{x}} CancellationTokenSource limit = new(ParallelTimeout)\"\"\";\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
        ("cts-23/X-cts-char",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var c = '<';\n        var d = '>';\n        throw new Xunit.Sdk.XunitException($\"{projectName} did not finish within 20 seconds.\");\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_3_cts_Characterised_Data() => Embed(Rows430_3_cts_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_3_waitasync_Characterised =
    [
        ("waitasync-14/X-waitasync-line-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        // work.WaitAsync(ParallelTimeout)\n    }\n}\n",
         "-"),
        ("waitasync-15/X-waitasync-block-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /* work.WaitAsync(ParallelTimeout) */\n    }\n}\n",
         "-"),
        ("waitasync-16/X-waitasync-doc-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /// <summary>work.WaitAsync(ParallelTimeout)</summary>\n    }\n}\n",
         "-"),
        ("waitasync-17/X-waitasync-string",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"work.WaitAsync(ParallelTimeout)\";\n    }\n}\n",
         "-"),
        ("waitasync-18/X-waitasync-verbatim",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = @\"work.WaitAsync(ParallelTimeout)\";\n    }\n}\n",
         "-"),
        ("waitasync-19/X-waitasync-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"x={x} work.WaitAsync(ParallelTimeout)\";\n    }\n}\n",
         "-"),
        ("waitasync-21/X-waitasync-raw",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"\"\"\n            work.WaitAsync(ParallelTimeout)\n            \"\"\";\n    }\n}\n",
         "-"),
        ("waitasync-22/X-waitasync-raw-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"x={{x}} work.WaitAsync(ParallelTimeout)\"\"\";\n    }\n}\n",
         "-"),
        ("waitasync-24/X-waitasync-char",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var c = '<';\n        var d = '>';\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_3_waitasync_Characterised_Data() => Embed(Rows430_3_waitasync_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_3_whenany_Characterised =
    [
        ("whenany-07/X-whenany-line-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        // Task.WhenAny(work, Task.Delay(Limit))\n    }\n}\n",
         "-"),
        ("whenany-08/X-whenany-block-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /* Task.WhenAny(work, Task.Delay(Limit)) */\n    }\n}\n",
         "-"),
        ("whenany-09/X-whenany-doc-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /// <summary>Task.WhenAny(work, Task.Delay(Limit))</summary>\n    }\n}\n",
         "-"),
        ("whenany-10/X-whenany-string",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"Task.WhenAny(work, Task.Delay(Limit))\";\n    }\n}\n",
         "-"),
        ("whenany-11/X-whenany-verbatim",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = @\"Task.WhenAny(work, Task.Delay(Limit))\";\n    }\n}\n",
         "-"),
        ("whenany-12/X-whenany-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"x={x} Task.WhenAny(work, Task.Delay(Limit))\";\n    }\n}\n",
         "-"),
        ("whenany-14/X-whenany-raw",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"\"\"\n            Task.WhenAny(work, Task.Delay(Limit))\n            \"\"\";\n    }\n}\n",
         "-"),
        ("whenany-15/X-whenany-raw-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"x={{x}} Task.WhenAny(work, Task.Delay(Limit))\"\"\";\n    }\n}\n",
         "-"),
        ("whenany-17/X-whenany-char",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var c = '<';\n        var d = '>';\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_3_whenany_Characterised_Data() => Embed(Rows430_3_whenany_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_3_httpclient_Characterised =
    [
        ("httpclient-14/X-httpclient-line-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        // new HttpClient { Timeout = SpecTimeout }\n    }\n}\n",
         "-"),
        ("httpclient-15/X-httpclient-block-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /* new HttpClient { Timeout = SpecTimeout } */\n    }\n}\n",
         "-"),
        ("httpclient-16/X-httpclient-doc-comment",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        /// <summary>new HttpClient { Timeout = SpecTimeout }</summary>\n    }\n}\n",
         "-"),
        ("httpclient-17/X-httpclient-string",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"new HttpClient { Timeout = SpecTimeout }\";\n    }\n}\n",
         "-"),
        ("httpclient-18/X-httpclient-verbatim",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = @\"new HttpClient { Timeout = SpecTimeout }\";\n    }\n}\n",
         "-"),
        ("httpclient-19/X-httpclient-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $\"x={x} new HttpClient {{ Timeout = SpecTimeout }}\";\n    }\n}\n",
         "-"),
        ("httpclient-21/X-httpclient-raw",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = \"\"\"\n            new HttpClient { Timeout = SpecTimeout }\n            \"\"\";\n    }\n}\n",
         "-"),
        ("httpclient-22/X-httpclient-raw-interp-text",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var s = $$\"\"\"x={{x}} new HttpClient { Timeout = SpecTimeout }\"\"\";\n    }\n}\n",
         "-"),
        ("httpclient-24/X-httpclient-char",
         "public class Enc\n{\n    [Fact]\n    public async Task T()\n    {\n        var c = '<';\n        var d = '>';\n    }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_3_httpclient_Characterised_Data() => Embed(Rows430_3_httpclient_Characterised);

    private static readonly (string Id, string Text, string Expected)[] Rows430_4_testtype_Pending =
    [
        ("testtype-01/I-skippable-fact",
         "public class Skips\n{\n    [SkippableFact]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Skips"),
        ("testtype-02/I-skippable-theory",
         "public class Skips\n{\n    [SkippableTheory]\n    [InlineData(1)]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Skips"),
        ("testtype-04/I-attr-list-trait-first",
         "public class Listed\n{\n    [Trait(\"Slice\", \"430\"), Fact]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Listed"),
        ("testtype-05/I-attr-list-fact-first",
         "public class Listed\n{\n    [Fact, Trait(\"Slice\", \"430\")]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Listed"),
        ("testtype-06/I-fact-attribute",
         "public class Listed\n{\n    [FactAttribute]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Listed"),
        ("testtype-07/I-fact-skip",
         "public class Listed\n{\n    [Fact(Skip = \"slow\")]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Listed"),
        ("testtype-08/I-record",
         "public record Recorded\n{\n    [Fact]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Recorded"),
        ("testtype-09/I-record-class",
         "public sealed record class Recorded\n{\n    [Fact]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Recorded"),
        ("testtype-10/I-record-positional",
         "public record Recorded(ITestOutputHelper output)\n{\n    [Fact]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "Recorded"),
        ("testtype-15/I-nested-under-plain",
         "public class Outer\n{\n    public class Inner\n    {\n        [SkippableFact]\n        public async Task T() { await work.WaitAsync(ParallelTimeout); }\n    }\n}\n",
         "Outer"),
        ("testtype-17/I-nested-attribute-on-child",
         "public class Outer\n{\n    [Collection(\"Isolated timing\")]\n    public class Inner\n    {\n        [Fact]\n        public async Task T() { await work.WaitAsync(ParallelTimeout); }\n    }\n}\n",
         "Outer"),
        ("testtype-19/I-partial-plain",
         "public partial class Split\n{\n    [SkippableFact] public async Task T() { }\n}\n|||public partial class Split\n{\n    private static async Task Help() { using var http = new HttpClient { Timeout = SpecTimeout }; }\n}\n",
         "Split"),
        ("testtype-20/I-partial-record",
         "public partial record Split\n{\n    [Fact] public async Task T() { await work.WaitAsync(ParallelTimeout); }\n}\n",
         "Split"),
    ];

    public static IEnumerable<object[]> Rows430_4_testtype_Pending_Data() => Embed(Rows430_4_testtype_Pending);

    private static readonly (string Id, string Text, string Expected)[] Rows430_4_testtype_Characterised =
    [
        ("testtype-03/I-skippable-fact-isolated",
         "[Collection(\"Isolated timing\")]\npublic class Skips\n{\n    [SkippableFact]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "-"),
        ("testtype-11/I-record-isolated",
         "[Collection(\"Isolated timing\")]\npublic record Recorded\n{\n    [Fact]\n    public async Task T()\n    {\n        await work.WaitAsync(ParallelTimeout);\n    }\n}\n",
         "-"),
        ("testtype-12/I-record-decl-only",
         "public sealed record GreetingRequest(string Name);\npublic class Clean\n{\n    [Fact]\n    public async Task T()\n    {\n        Assert.True(true);\n    }\n}\n",
         "-"),
        ("testtype-13/I-record-local-word",
         "public class Clean\n{\n    [Fact]\n    public async Task T()\n    {\n        foreach (var record in records)\n        {\n            Assert.NotNull(record);\n        }\n        Assert.All(rows, record => Assert.NotNull(record));\n    }\n}\n",
         "-"),
        ("testtype-14/I-nested-under-isolated",
         "[Collection(\"Isolated timing\")]\npublic class Outer\n{\n    public class Inner\n    {\n        [Fact]\n        public async Task T() { await work.WaitAsync(ParallelTimeout); }\n    }\n}\n",
         "-"),
        ("testtype-16/I-nested-record-under-isolated",
         "[Collection(\"Isolated timing\")]\npublic class Outer\n{\n    public record Inner\n    {\n        [Fact]\n        public async Task T() { await work.WaitAsync(ParallelTimeout); }\n    }\n}\n",
         "-"),
        ("testtype-18/I-partial-split",
         "public partial class Split\n{\n    [Fact] public async Task T() { await work.WaitAsync(ParallelTimeout); }\n}\n|||[Collection(\"Isolated timing\")]\npublic partial class Split\n{\n}\n",
         "-"),
        ("testtype-21/I-no-test",
         "internal sealed class Helper\n{\n    public async Task Stop() { await work.WaitAsync(ParallelTimeout); }\n}\n",
         "-"),
    ];

    public static IEnumerable<object[]> Rows430_4_testtype_Characterised_Data() => Embed(Rows430_4_testtype_Characterised);

    private static IEnumerable<(string Id, string Text, string Expected)> AllRows430() =>
        Rows430_1_elapsed_Characterised
            .Concat(Rows430_1_elapsed_Pending)
            .Concat(Rows430_2_elapsed_Pending)
            .Concat(Rows430_2_elapsed_Characterised)
            .Concat(Rows430_1_cts_Characterised)
            .Concat(Rows430_2_cts_Pending)
            .Concat(Rows430_2_cts_Characterised)
            .Concat(Rows430_2_waitasync_Pending)
            .Concat(Rows430_2_waitasync_Characterised)
            .Concat(Rows430_2_whenany_Pending)
            .Concat(Rows430_2_whenany_Characterised)
            .Concat(Rows430_2_httpclient_Pending)
            .Concat(Rows430_2_httpclient_Characterised)
            .Concat(Rows430_3_process_Characterised)
            .Concat(Rows430_2_process_Characterised)
            .Concat(Rows430_3_collection_Characterised)
            .Concat(Rows430_3_elapsed_Characterised)
            .Concat(Rows430_3_cts_Characterised)
            .Concat(Rows430_3_waitasync_Characterised)
            .Concat(Rows430_3_whenany_Characterised)
            .Concat(Rows430_3_httpclient_Characterised)
            .Concat(Rows430_4_testtype_Pending)
            .Concat(Rows430_4_testtype_Characterised);

    [Theory]
    [MemberData(nameof(Rows430_1_elapsed_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-1")]
    public void Row_430_1_elapsed_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the elapsed family (a clean class, and the Elapsed or CancellationTokenSource shape of one real test)
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_1_elapsed_Pending_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-1")]
    public void Row_430_1_elapsed_pending(string id, string text, string expected)
    {
        // Given a corpus row of the elapsed family (a clean class, and the Elapsed or CancellationTokenSource shape of one real test)
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_elapsed_Pending_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_elapsed_pending(string id, string text, string expected)
    {
        // Given a corpus row of the elapsed family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_elapsed_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_elapsed_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the elapsed family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_1_cts_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-1")]
    public void Row_430_1_cts_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the cts family (a clean class, and the Elapsed or CancellationTokenSource shape of one real test)
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_cts_Pending_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_cts_pending(string id, string text, string expected)
    {
        // Given a corpus row of the cts family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_cts_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_cts_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the cts family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_waitasync_Pending_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_waitasync_pending(string id, string text, string expected)
    {
        // Given a corpus row of the waitasync family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_waitasync_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_waitasync_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the waitasync family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_whenany_Pending_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_whenany_pending(string id, string text, string expected)
    {
        // Given a corpus row of the whenany family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_whenany_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_whenany_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the whenany family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_httpclient_Pending_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_httpclient_pending(string id, string text, string expected)
    {
        // Given a corpus row of the httpclient family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_httpclient_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_httpclient_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the httpclient family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_3_process_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-3")]
    public void Row_430_3_process_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the process family (the near misses and the collection attribute rows (outcomes unchanged by this card))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_2_process_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-2")]
    public void Row_430_2_process_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the process family (the spellings of the card [M] (positive rows) and their neighbours that must stay unnamed (N- rows, encodings))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_3_collection_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-3")]
    public void Row_430_3_collection_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the collection family (the near misses and the collection attribute rows (outcomes unchanged by this card))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_3_elapsed_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-3")]
    public void Row_430_3_elapsed_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the elapsed family (the near misses and the collection attribute rows (outcomes unchanged by this card))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_3_cts_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-3")]
    public void Row_430_3_cts_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the cts family (the near misses and the collection attribute rows (outcomes unchanged by this card))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_3_waitasync_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-3")]
    public void Row_430_3_waitasync_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the waitasync family (the near misses and the collection attribute rows (outcomes unchanged by this card))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_3_whenany_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-3")]
    public void Row_430_3_whenany_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the whenany family (the near misses and the collection attribute rows (outcomes unchanged by this card))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_3_httpclient_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-3")]
    public void Row_430_3_httpclient_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the httpclient family (the near misses and the collection attribute rows (outcomes unchanged by this card))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_4_testtype_Pending_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-4")]
    public void Row_430_4_testtype_pending(string id, string text, string expected)
    {
        // Given a corpus row of the testtype family (the test-type shapes of [I] (SkippableFact, records, nested, partial))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Theory]
    [MemberData(nameof(Rows430_4_testtype_Characterised_Data))]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-4")]
    public void Row_430_4_testtype_characterised(string id, string text, string expected)
    {
        // Given a corpus row of the testtype family (the test-type shapes of [I] (SkippableFact, records, nested, partial))
        // When the guard runs on its source files
        // Then it names exactly the classes the corpus expects, or nothing
        AssertCorpusRow430(id, text, expected);
    }

    [Fact]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-1")]
    public void Id_430_1_the_embedded_rows_are_the_corpus_file()
    {
        // Given the two corpus files of the slice (corpus.txt and the fix round's corpus-fix.txt) and the rows embedded in this class
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, ".mob", "records", "430", "corpus.txt")))
            dir = dir.Parent;
        Assert.True(dir is not null, "corpus.txt was not found above the test binaries.");
        var file = new[] { "corpus.txt", "corpus-fix.txt" }
            .SelectMany(name => File.ReadAllLines(Path.Combine(dir!.FullName, ".mob", "records", "430", name)))
            .Where(l => l.Length > 0 && !l.StartsWith('#'))
            .Select(l => l.Split('\t'))
            .Select(p => (Id: p[0], Text: DecodeCorpus430(p[5]), Expected: p[3]))
            .ToList();

        // When every file row is looked up among the embedded rows
        var embedded = AllRows430().Concat(AllFixRows430()).Select(r => (Id: r.Id[(r.Id.IndexOf('/') + 1)..], r.Text, r.Expected)).ToList();
        var missing = file.Where(r => !embedded.Contains(r)).Select(r => r.Id).ToList();

        // Then none is missing, and no row is embedded twice or that the files do not hold
        Assert.True(missing.Count == 0, "missing rows: " + string.Join(", ", missing));
        Assert.Equal(file.Count, embedded.Count);
        Assert.Equal(file.Count, embedded.Select(r => r.Id).Distinct(StringComparer.Ordinal).Count());
    }

    private static readonly string[] IsolatedRealClasses430 =
    [
        "TasksOverHttp",
        "GeneratedApiHazards",
        "EndpointFailureMapsToHttpStatus",
        "GetEndpointsCompile",
        "PutAndDeleteEndpointsCompile",
        "GeneratedApiBoundAddress",
        "SourceMapDeployment",
        "GeneratedApiBoundAddressEdges",
        "StarvationProbe",
    ];

    [Fact]
    [Trait("Slice", "430")]
    [Trait("Criterion", "430-5")]
    [Trait("Status", "Pending")]
    public void E_the_nine_real_classes_with_short_hard_limits_carry_the_isolated_collection()
    {
        // Given the real source files of the nine classes that assert a short hard limit (the card's five, the two of ruling 2 and the two of R1-1)
        var project = RepoPaths.Find("tests", "Avowry.Acceptance.Tests");
        var attribute = new Regex(
            @"[\[,]\s*(?:Xunit\s*\.\s*)?Collection\s*\(\s*""" + Regex.Escape(IsolatedName) + @"""\s*\)",
            RegexOptions.CultureInvariant);

        // When the attributes written directly above each class declaration are read from the source text
        var without = new List<string>();
        foreach (var name in IsolatedRealClasses430)
        {
            var text = File.ReadAllText(Path.Combine(project, name + ".cs")).Replace("\r\n", "\n");
            var declaration = Regex.Match(
                text,
                @"(?m)^(?<above>(?:[ \t]*\[[^\n]*\][ \t]*\n|[ \t]*//[^\n]*\n)*)[ \t]*(?:(?:public|internal|sealed|static|abstract|partial)\s+)*class\s+" + name + @"\b",
                RegexOptions.CultureInvariant);
            Assert.True(declaration.Success, name + ": the class declaration was not found in " + name + ".cs");
            var attributes = declaration.Groups["above"].Value
                .Split('\n')
                .Where(l => l.TrimStart().StartsWith('['))
                .ToList();
            if (!attributes.Any(l => attribute.IsMatch(l.TrimStart())))
                without.Add(name);
        }

        // Then every one carries Collection("Isolated timing")
        Assert.True(without.Count == 0, "no Collection(\"" + IsolatedName + "\") above: " + string.Join(", ", without));
    }

    private static void AssertCorpusRow430(string id, string text, string expected)
    {
        var corpusId = id[(id.IndexOf('/') + 1)..];
        var files = text.Split("|||");
        var sources = files
            .Select((t, k) => Src(corpusId + (files.Length == 1 ? "" : "-" + (k + 1)) + ".cs", t))
            .ToArray();
        var firstFile = sources.Select(s => s.Relative).Order(StringComparer.Ordinal).First();
        var want = expected == "-"
            ? Array.Empty<string>()
            : expected.Split(',').Order(StringComparer.Ordinal).Select(c => firstFile + ": " + c).ToArray();

        var found = Find(sources);

        Assert.True(
            want.SequenceEqual(LinesOf(found)),
            id + ": expected " + (want.Length == 0 ? "no name" : string.Join(" | ", want)) + ", got:\n" + Describe(found));
    }

    private static string DecodeCorpus430(string text)
    {
        var sb = new System.Text.StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\' && i + 1 < text.Length)
            {
                var c = text[++i];
                sb.Append(c == 'n' ? '\n' : c == 't' ? '\t' : c);
            }
            else
            {
                sb.Append(text[i]);
            }
        }

        return sb.ToString();
    }

}
