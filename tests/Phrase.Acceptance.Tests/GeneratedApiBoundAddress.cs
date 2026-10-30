using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Card 214 Done check 2, part 1 (commit 3): the harness owns the port and learns the bound address from the process (Z, O, M, I),
// plus guards 6 and 8. The two check 1 tests are the green proof for the other half and are re-run unchanged.
// Layers, outermost first: L2 StartAsync -> L3 readiness -> L4 pump and parser -> L5 process (a real generated API, or QuietChild).
public class GeneratedApiBoundAddress
{
    [Fact]
    [Trait("Slice", "214")]
    public async Task Z_a_child_that_never_reports_an_address_times_out_naming_pid_url_0_and_log()
    {
        // Doubles: S2 (1 second), S3 and S4 (a QuietChild that writes one log line and stays silent otherwise).
        // Given a child that never prints a "Now listening on:" line
        var options = Probes.QuietChild(["--eprint", "z-log-line"], TimeSpan.FromSeconds(1));

        // When the harness starts it
        var (api, error) = await Probes.StartAsync("unused.csproj", options);
        if (api is not null)
            await api.DisposeAsync();

        // Then a TimeoutException names the PID, the URL asked for (port 0) and the log; no guessed port was polled into success
        Assert.True(error is TimeoutException, $"Expected a TimeoutException, got {error?.GetType().Name ?? "an api at " + api?.BaseUrl}.");
        Assert.Matches(@"process \d+ did not listen on http://127\.0\.0\.1:0 within 1 seconds", error!.Message);
        Assert.Contains("z-log-line", error.Message);
    }

    [Fact]
    [Trait("Slice", "214")]
    public async Task O_one_real_api_is_started_on_port_0_and_its_base_url_is_the_address_it_reported()
    {
        // Doubles: S1 (reads the URL the harness passes to the child, changes nothing); the real generated API, no other double.
        // Given a real generated API and a hook that only reads the URL
        using var workspace = TempWorkspace.Create();
        var project = await Probes.BuildTasksApiAsync(workspace);
        string? urls = null;
        var options = new GeneratedApiStartOptions { BeforeChildStart = psi => urls = psi.Environment["ASPNETCORE_URLS"] };

        // When the harness starts it
        await using var api = await GeneratedApi.StartAsync(project, options);

        // Then the URL asked for was exactly port 0, BaseUrl has a nonzero port, equals the child's own listening line, and answers
        Assert.Equal("http://127.0.0.1:0", urls);
        Assert.NotEqual(0, new Uri(api.BaseUrl).Port);
        var reported = Regex.Match(api.Log, @"Now listening on:\s*(\S+)");
        Assert.True(reported.Success, $"The child's log has no listening line.{Environment.NewLine}{api.Log}");
        Assert.Equal(reported.Groups[1].Value, api.BaseUrl);
        Assert.True(await Probes.AnsweredAsync(api.BaseUrl), $"{api.BaseUrl} did not answer.{Environment.NewLine}{api.Log}");
    }

    [Fact]
    [Trait("Slice", "214")]
    public async Task M_an_unrelated_line_is_ignored_and_the_reported_address_is_taken_without_touching_another_listener()
    {
        // Doubles: S3 and S4 (QuietChild prints an unrelated line, then the listening line), two test-owned Holders (the reported one, and another that must stay untouched).
        // Given a child that prints an unrelated line and then the address of a listener the test owns, and a second listener elsewhere
        using var reported = new Holder();
        using var other = new Holder();
        var options = Probes.QuietChild(["--print", $"unrelated: listening soon, not on {other.Url}", "--print", reported.ListeningLine], TimeSpan.FromSeconds(10));

        // When the harness starts it
        var (api, error) = await Probes.StartAsync("unused.csproj", options);

        try
        {
            // Then BaseUrl is the reported address, and the second listener was never contacted
            Assert.True(error is null, $"StartAsync failed: {error}");
            Assert.Equal(reported.Url, api!.BaseUrl);
            Assert.Equal(0, other.Accepted);
        }
        finally
        {
            if (api is not null)
                await api.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Slice", "214")]
    public async Task I_readiness_comes_from_the_childs_own_output_before_any_connection_and_not_before_the_gate()
    {
        // Doubles: S3 and S4 (QuietChild with --wait-file: prints the listening line only after the test releases the gate), a counting Holder as the reported address.
        // Given a child that reports an address only after a gate file exists
        using var reported = new Holder();
        var gate = Path.Combine(Path.GetTempPath(), $"quietchild-gate-{Guid.NewGuid():N}");
        var options = Probes.QuietChild(["--wait-file", gate, "--print", reported.ListeningLine], TimeSpan.FromSeconds(15));
        Task<(GeneratedApi? Api, Exception? Error)>? start = null;
        GeneratedApi? api = null;
        try
        {
            // When the harness starts and the gate is still closed
            start = Probes.StartAsync("unused.csproj", options);
            var early = await Task.WhenAny(start, Task.Delay(TimeSpan.FromSeconds(1.5)));

            // Then it has not returned and made 0 connections to the printed address
            Assert.False(early == start, "StartAsync returned before the child reported an address.");
            Assert.False(reported.Contacted, "The harness connected to the address before the child printed it.");

            // And once the gate is released it returns that address
            File.WriteAllText(gate, "go");
            var (started, error) = await start;
            api = started;
            Assert.True(error is null, $"StartAsync failed after the gate was released: {error}");
            Assert.Equal(reported.Url, api!.BaseUrl);
        }
        finally
        {
            File.WriteAllText(gate, "go");
            if (api is null && start is not null)
                api = (await start).Api;
            if (api is not null)
                await api.DisposeAsync();
            File.Delete(gate);
        }
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public async Task Guard_exit_after_reporting_names_pid_code_and_address()
    {
        // Doubles: S3 and S4 (QuietChild reports the address of a test-owned Holder, then exits 4), S2 (30 seconds, never reached).
        // Given a child that reports an address and exits before the harness has made a connection
        // The listener is on a free port of its own, not the harness-chosen one, so a harness that still polls its own port cannot be satisfied by it.
        using var reported = new Holder();
        var options = Probes.QuietChild(["--print", reported.ListeningLine, "--exit", "4"], TimeSpan.FromSeconds(30));
        var (api, error) = await Probes.StartAsync("unused.csproj", options);
        if (api is not null)
            await api.DisposeAsync();

        // Then the failure names the PID, exit code 4 and an address, instead of hanging or returning
        Assert.True(error is InvalidOperationException, $"Expected an InvalidOperationException, got {error?.GetType().Name ?? "an api at " + api?.BaseUrl}.");
        Assert.Matches(@"process \d+ exited with code 4 .*http://127\.0\.0\.1:\d+", error!.Message.Replace('\n', ' '));
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public void Guard_default_timeouts_are_2_minutes_and_15_seconds()
    {
        // Doubles: none. The start default is read back from the harness; the stop wait is a literal in DisposeAsync, so its source line is pinned.
        Assert.Equal(TimeSpan.FromMinutes(2), GeneratedApi.DefaultStartupTimeout);
        var source = File.ReadAllText(RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Support", "GeneratedApi.cs"));
        Assert.Single(Regex.Matches(source, @"WaitForExitAsync\(\)\.WaitAsync\(TimeSpan\.FromSeconds\(15\)\)"));
    }
}
