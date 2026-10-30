using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Phrase.Acceptance.Tests.Support;

/// <summary>Pending reasons for card 214's red tests. Each turns green in commit 5 and loses its Skip there.</summary>
internal static class Card214
{
    public const string Red1a = "Card 214 red: Done check 1 hazard a (port taken before the API binds), turns green in commit 5";
    public const string Red1b = "Card 214 red: Done check 1 hazard b (foreign listener reported as up), turns green in commit 5";
    public const string Red2 = "Card 214 red: Done check 2 (the harness owns the port and reads the bound address), turns green in commit 5";
    public const string Red3 = "Card 214 red: Done check 3 (no port reuse window left in the source), turns green in commit 5";
}

/// <summary>How a test-owned listener treats a connection.</summary>
public enum HolderMode
{
    AcceptAndClose,
    AcceptAndKeep,
    BacklogNeverAccepts,
}

/// <summary>
/// A listener the test owns on loopback (port 0 = any free port), standing in for a foreign process or for the address a child reports.
/// Always released by Dispose, so one failing test cannot poison the next.
/// </summary>
internal sealed class Holder : IDisposable
{
    private readonly TcpListener _listener;
    private readonly HolderMode _mode;
    private readonly List<TcpClient> _kept = new();
    private readonly CancellationTokenSource _stop = new();
    private int _accepted;

    public Holder(HolderMode mode = HolderMode.AcceptAndKeep, int port = 0)
    {
        _mode = mode;
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start(mode == HolderMode.BacklogNeverAccepts ? 1 : 16);
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        if (mode != HolderMode.BacklogNeverAccepts)
            _ = AcceptLoopAsync();
    }

    public int Port { get; }
    public string Url => $"http://127.0.0.1:{Port}";
    public string ListeningLine => $"Now listening on: {Url}";

    /// <summary>True once anything connected: accepted connections, or one waiting in the backlog.</summary>
    public bool Contacted => _mode == HolderMode.BacklogNeverAccepts ? SafePending() : Volatile.Read(ref _accepted) > 0;

    public int Accepted => Volatile.Read(ref _accepted);

    private bool SafePending()
    {
        try { return _listener.Pending(); }
        catch (InvalidOperationException) { return false; }
    }

    private async Task AcceptLoopAsync()
    {
        try
        {
            while (!_stop.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_stop.Token);
                Interlocked.Increment(ref _accepted);
                if (_mode == HolderMode.AcceptAndClose)
                    client.Dispose();
                else
                    lock (_kept) _kept.Add(client);
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or SocketException)
        {
            // Released by Dispose.
        }
    }

    public void Dispose()
    {
        _stop.Cancel();
        _listener.Stop();
        lock (_kept)
            foreach (var client in _kept)
                client.Dispose();
        _stop.Dispose();
    }
}

internal static class Probes
{
    /// <summary>Options that run QuietChild (never a real API) with these arguments. The hook runs after the arguments are added.</summary>
    public static GeneratedApiStartOptions QuietChild(string[] childArgs, TimeSpan timeout, Action<ProcessStartInfo>? onStart = null) => new()
    {
        PrebuiltDll = GeneratedApiSeams.QuietChildPath(),
        StartupTimeout = timeout,
        BeforeChildStart = psi =>
        {
            foreach (var arg in childArgs)
                psi.ArgumentList.Add(arg);
            onStart?.Invoke(psi);
        },
    };

    /// <summary>
    /// A QuietChild that reports an address a test-owned listener holds. On a harness that still chooses a port, the listener
    /// takes that chosen port, so the same test passes on the old and the new harness; on a harness that owns its port it takes any free one.
    /// </summary>
    public static GeneratedApiStartOptions SelfListening(Action<Holder> made, TimeSpan timeout, params string[] after) => QuietChild([], timeout, psi =>
    {
        var holder = new Holder(HolderMode.AcceptAndKeep, ChosenUri(psi).Port);
        made(holder);
        psi.ArgumentList.Add("--print");
        psi.ArgumentList.Add(holder.ListeningLine);
        foreach (var arg in after)
            psi.ArgumentList.Add(arg);
    });

    /// <summary>The URL in the ASPNETCORE_URLS the harness chose (its port is 0 once the harness owns its port).</summary>
    public static Uri ChosenUri(ProcessStartInfo psi) => new(psi.Environment["ASPNETCORE_URLS"]!);

    public static async Task<(GeneratedApi? Api, Exception? Error)> StartAsync(string project, GeneratedApiStartOptions options)
    {
        try
        {
            return (await GeneratedApi.StartAsync(project, options), null);
        }
        catch (Exception ex)
        {
            return (null, ex);
        }
    }

    public static bool IsDead(int pid)
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
    }

    /// <summary>Builds the Tasks demo once for this test and returns its API project. The workspace is released by the caller.</summary>
    public static async Task<string> BuildTasksApiAsync(TempWorkspace workspace)
    {
        var cli = await PhraseCli.RunAsync("build", RepoPaths.Find("demo", "tasks", "Tasks.avowry"), "-o", workspace.Root);
        Assert.True(cli.ExitCode == 0, cli.Text);
        return workspace.FindSingle("*.Api.csproj");
    }

    public static async Task<bool> AnsweredAsync(string baseUrl)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        try
        {
            using var response = await http.GetAsync(baseUrl);
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }
}
