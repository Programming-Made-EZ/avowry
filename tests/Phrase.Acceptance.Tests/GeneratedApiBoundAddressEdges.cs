using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Card 214 Done check 2, part 2 (commit 4): the pure parser boundaries (B), the stream and format edges (B, E), and guards 7 and 9.
// Layers: L4 only for the parser (plain strings, no process); L2 -> L3 -> L4 -> L5 (QuietChild) for the rest.
public class GeneratedApiBoundAddressEdges
{
    [Fact]
    [Trait("Slice", "214")]
    public void B_parser_accepts_only_an_http_127_0_0_1_address_with_a_port_from_1_to_65535()
    {
        // Doubles: none, plain strings into the internal pure parser.
        var accepted = new[]
        {
            ("      Now listening on: http://127.0.0.1:1", 1),
            ("      Now listening on: http://127.0.0.1:65535", 65535),
            ("Now listening on: http://127.0.0.1:34727", 34727),
            ("info: x Now listening on:   http://127.0.0.1:5000", 5000),
        };
        var rejected = new[]
        {
            "Now listening on: http://127.0.0.1:0",
            "Now listening on: http://127.0.0.1:65536",
            "Now listening on: http://127.0.0.1:",
            "Now listening on: http://127.0.0.1:abc",
            "Now listening on: http://[::1]:5000",
            "Now listening on: http://localhost:5000",
            "Now listening on: https://127.0.0.1:5000",
            "Now listening on:",
            "info: Microsoft.Hosting.Lifetime[14]",
            "",
        };

        var wrong = new List<string>();
        foreach (var (line, port) in accepted)
            if (!GeneratedApi.TryParseListeningAddress(line, out var address) || address.Port != port)
                wrong.Add($"should accept with port {port}: \"{line}\"");
        foreach (var line in rejected)
            if (GeneratedApi.TryParseListeningAddress(line, out _))
                wrong.Add($"should reject: \"{line}\"");

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("Slice", "214")]
    public async Task B_a_line_on_stderr_is_found()
    {
        // Doubles: S3 and S4 (QuietChild with --eprint), a test-owned Holder as the reported address.
        using var reported = new Holder();
        var (api, error) = await Probes.StartAsync("unused.csproj", Probes.QuietChild(["--eprint", reported.ListeningLine], TimeSpan.FromSeconds(10)));
        try
        {
            Assert.True(error is null, $"StartAsync failed: {error}");
            Assert.Equal(reported.Url, api!.BaseUrl);
        }
        finally
        {
            if (api is not null)
                await api.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Slice", "214")]
    public async Task B_a_line_after_5000_earlier_log_lines_is_found()
    {
        // Doubles: S3 and S4 (one --print argument of 5000 lines, then the listening line), a Holder as the reported address.
        using var reported = new Holder();
        var noise = string.Join('\n', Enumerable.Range(0, 5000).Select(_ => "x"));
        var (api, error) = await Probes.StartAsync("unused.csproj", Probes.QuietChild(["--print", noise, "--print", reported.ListeningLine], TimeSpan.FromSeconds(20)));
        try
        {
            Assert.True(error is null, $"StartAsync failed: {error}");
            Assert.Equal(reported.Url, api!.BaseUrl);
        }
        finally
        {
            if (api is not null)
                await api.DisposeAsync();
        }
    }

    [Theory]
    [Trait("Slice", "214")]
    [InlineData("http://localhost:")]
    [InlineData("http://[::1]:")]
    [InlineData("https://127.0.0.1:")]
    public async Task B_a_reported_host_or_scheme_that_is_not_http_127_0_0_1_fails_naming_pid_line_and_log(string prefix)
    {
        // Doubles: S3 and S4 (QuietChild writes a log line, then a listening line with the wrong host or scheme), a Holder for the port.
        using var holder = new Holder();
        var line = $"Now listening on: {prefix}{holder.Port}";
        var (api, error) = await Probes.StartAsync("unused.csproj", Probes.QuietChild(["--eprint", "b-log-line", "--print", line], TimeSpan.FromSeconds(3)));
        if (api is not null)
            await api.DisposeAsync();

        Assert.True(error is not null, $"StartAsync accepted \"{line}\" and returned an api at {api?.BaseUrl}.");
        // The first line of the message is the harness's own text (the log follows it, and the log holds the printed line too).
        var header = error!.Message.Split((char)10)[0];
        Assert.Matches(@"process \d+", header);
        Assert.Contains(line, header);
        Assert.Contains("b-log-line", error.Message);
    }

    [Fact]
    [Trait("Slice", "214")]
    public async Task E_an_address_nothing_listens_on_is_not_ready_after_the_timeout_and_names_that_url()
    {
        // Doubles: S2 (2 seconds), S3 and S4 (QuietChild reports the address of a listener the test closed first).
        int port;
        using (var closed = new Holder())
            port = closed.Port;
        var (api, error) = await Probes.StartAsync("unused.csproj", Probes.QuietChild(["--print", $"Now listening on: http://127.0.0.1:{port}"], TimeSpan.FromSeconds(2)));
        if (api is not null)
            await api.DisposeAsync();

        Assert.True(error is TimeoutException, $"Expected a TimeoutException, got {error?.GetType().Name ?? "an api at " + api?.BaseUrl}.");
        Assert.Contains($"http://127.0.0.1:{port}", error!.Message.Split((char)10)[0]);
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public async Task Guard_squatter_on_reported_address_is_out_of_scope_and_the_pid_held_is_the_one_that_started()
    {
        // Out of scope by design: a foreign process squatting on the exact address the child reported cannot be told apart from the child.
        // This test documents the limit: the Holder is that squatter, the harness returns, and the Pid it holds is the started child, not this process.
        // Doubles: S3 and S4 (QuietChild reports the address the Holder owns).
        Holder? squatter = null;
        var (api, error) = await Probes.StartAsync("unused.csproj", Probes.SelfListening(h => squatter = h, TimeSpan.FromSeconds(30)));
        try
        {
            Assert.True(error is null, $"StartAsync failed: {error}");
            Assert.NotEqual(Environment.ProcessId, api!.Pid);
            Assert.False(Probes.IsDead(api.Pid), "The held Pid is not a running process.");
        }
        finally
        {
            if (api is not null)
                await api.DisposeAsync();
            squatter?.Dispose();
        }
        Assert.True(Probes.IsDead(api!.Pid), "The held Pid is still running after dispose.");
        Assert.False(Probes.IsDead(Environment.ProcessId));
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public async Task Guard_bind_failure_starts_exactly_one_process()
    {
        // Doubles: S1 (counts starts), S3 and S4 (QuietChild writes the bind-failure text and exits 1).
        var starts = 0;
        var options = Probes.QuietChild(
            ["--eprint", "Failed to bind to address http://127.0.0.1:1: address already in use.", "--exit", "1"],
            TimeSpan.FromSeconds(30),
            _ => starts++);

        await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratedApi.StartAsync("unused.csproj", options));

        Assert.Equal(1, starts);
    }
}
