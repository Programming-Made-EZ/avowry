using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Card 214 Done check 1: the two root-cause hazards of F-056, committed red before the fix (commit 2),
// and the born-green guards that keep them honest (guards 3, 4 and 5).
// Layers, outermost first: L2 StartAsync orchestration -> L3 readiness -> L5 the process (a real generated API, or QuietChild).
// Every foreign listener is a Holder the test owns and releases in a finally.
public class GeneratedApiHazards
{
    [Fact]
    [Trait("Slice", "214")]
    public async Task Hazard_a_a_port_taken_before_the_api_binds_does_not_stop_a_real_api()
    {
        // Doubles: S1 BeforeChildStart (reads the chosen URL, binds that port itself); a test-owned Holder; the real generated API, no other double.
        // Given a real generated API and a hook that takes the port the harness chose before the child binds
        using var workspace = TempWorkspace.Create();
        var project = await Probes.BuildTasksApiAsync(workspace);
        Holder? held = null;
        var hookCalls = 0;
        var options = new GeneratedApiStartOptions
        {
            BeforeChildStart = psi =>
            {
                hookCalls++;
                var port = Probes.ChosenUri(psi).Port;
                if (port != 0)
                    held = new Holder(HolderMode.AcceptAndClose, port);
            },
        };
        GeneratedApi? api = null;
        try
        {
            // When the harness starts the API
            var (started, error) = await Probes.StartAsync(project, options);
            api = started;

            // Then the hook ran once, StartAsync did not fail, and the BaseUrl answers an HTTP request from the API itself
            Assert.Equal(1, hookCalls);
            Assert.True(error is null, $"StartAsync failed after the port was taken: {error}");
            var answered = await Probes.AnsweredAsync(api!.BaseUrl);
            Assert.True(answered, $"{api.BaseUrl} did not answer an HTTP request; the port belongs to someone else.{Environment.NewLine}{api.Log}");
            Assert.DoesNotContain("address already in use", api.Log);
        }
        finally
        {
            if (api is not null)
                await api.DisposeAsync();
            held?.Dispose();
        }
    }

    [Theory]
    [Trait("Slice", "214")]
    [InlineData(HolderMode.AcceptAndClose)]
    [InlineData(HolderMode.AcceptAndKeep)]
    [InlineData(HolderMode.BacklogNeverAccepts)]
    public async Task Hazard_b_a_foreign_listener_on_the_waited_port_is_not_reported_as_up(HolderMode mode)
    {
        // Doubles: S1 (binds the chosen port with a Holder in one of three states), S2 (1 second), S3 and S4 (a silent QuietChild that never listens).
        // Given a child that stays alive and never listens, and a foreign listener on the port the harness chose
        Holder? foreign = null;
        var hookCalls = 0;
        var options = Probes.QuietChild([], TimeSpan.FromSeconds(1), psi =>
        {
            hookCalls++;
            var port = Probes.ChosenUri(psi).Port;
            if (port != 0)
                foreign = new Holder(mode, port);
        });
        GeneratedApi? api = null;
        try
        {
            // When the harness starts it
            var (started, error) = await Probes.StartAsync("unused.csproj", options);
            api = started;

            // Then it is not reported as up: the start times out, and the foreign listener was never contacted
            Assert.Equal(1, hookCalls);
            Assert.True(error is TimeoutException,
                $"Expected a TimeoutException for a child that never listens, got {(error is null ? $"an api at {api!.BaseUrl}" : error.GetType().Name)}; foreign listener ({mode}) contacted: {foreign?.Contacted}.");
            Assert.False(foreign?.Contacted ?? false, $"The harness connected to a foreign listener ({mode}).");
        }
        finally
        {
            if (api is not null)
                await api.DisposeAsync();
            foreign?.Dispose();
        }
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public async Task Guard_two_parallel_starts_get_distinct_base_urls()
    {
        // Doubles: S1 (a Holder per start on the chosen or any free port), S3 and S4 (a QuietChild reporting that address).
        // Given two starts under Task.WhenAll, each with its own listener standing in for its API
        var holders = new List<Holder>();
        var gate = new object();
        GeneratedApiStartOptions Options() => Probes.SelfListening(h => { lock (gate) holders.Add(h); }, TimeSpan.FromSeconds(30));
        GeneratedApi[] apis = [];
        try
        {
            // When both start together
            apis = await Task.WhenAll(GeneratedApi.StartAsync("unused.csproj", Options()), GeneratedApi.StartAsync("unused.csproj", Options()));

            // Then the BaseUrl values differ and each is the address of one of the two listeners
            Assert.NotEqual(apis[0].BaseUrl, apis[1].BaseUrl);
            Assert.Equal(holders.Select(h => h.Url).Order(), apis.Select(a => a.BaseUrl).Order());
        }
        finally
        {
            foreach (var api in apis)
                await api.DisposeAsync();
            foreach (var holder in holders)
                holder.Dispose();
        }
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public async Task Guard_child_exit_code_names_pid_code_and_log()
    {
        // Doubles: S3 and S4 (QuietChild writes a log line, waits for a gate file, then exits 3 before it binds), S2 (30 seconds, never reached).
        // The gate gives the output pump time to read the log line before the exit is seen (otherwise a loaded machine can see the exit first).
        // Given a child that exits 3 before it listens
        var gate = Path.Combine(Path.GetTempPath(), $"quietchild-gate-{Guid.NewGuid():N}");
        var options = Probes.QuietChild(["--eprint", "log-line-of-the-child", "--wait-file", gate, "--exit", "3"], TimeSpan.FromSeconds(30));

        // When the harness starts it and the gate opens a moment later
        var release = Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ => File.WriteAllText(gate, "go"));
        InvalidOperationException error;
        try
        {
            error = await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratedApi.StartAsync("unused.csproj", options));
        }
        finally
        {
            await release;
            File.Delete(gate);
        }

        // Then the message names the PID, exit code 3 and a line of the log, and that PID is dead; this test process is not
        var match = Regex.Match(error.Message, @"process (\d+) exited with code 3");
        Assert.True(match.Success, error.Message);
        Assert.Contains("log-line-of-the-child", error.Message);
        var pid = int.Parse(match.Groups[1].Value);
        Assert.NotEqual(Environment.ProcessId, pid);
        Assert.True(Probes.IsDead(pid), $"Process {pid} is still running after the failed start.");
        Assert.False(Probes.IsDead(Environment.ProcessId));
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public async Task Guard_hook_exception_propagates_and_no_child_runs()
    {
        // Doubles: S1 (throws), S3 and S4 (the QuietChild dll, which must never be started), S2 (30 seconds so a swallowed hook shows as a long start, not a hang).
        // Given a hook that throws its own exception
        var calls = 0;
        var thrown = new InvalidOperationException("hook says stop");
        var options = Probes.QuietChild([], TimeSpan.FromSeconds(30), _ =>
        {
            calls++;
            throw thrown;
        });

        // When the harness starts, with a limit that a swallowed hook (a running silent child) would exceed
        var error = await Record.ExceptionAsync(() => GeneratedApi.StartAsync("unused.csproj", options).WaitAsync(TimeSpan.FromSeconds(10)));

        // Then the very same exception comes out, once
        Assert.Same(thrown, error);
        Assert.Equal(1, calls);
    }
}
