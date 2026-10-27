using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Phrase.Acceptance.Tests.Support;

/// <summary>
/// Builds one generated API and runs it on a free 127.0.0.1 port.
/// The recorded PID is the single <c>dotnet</c> process hosting the app.
/// Dispose stops that PID.
/// </summary>
public sealed class GeneratedApi : IAsyncDisposable
{
    private static readonly TimeSpan BuildTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(2);

    private readonly Process _process;
    private readonly StringBuilder _log = new();
    private readonly object _logGate = new();
    private readonly Task _stdout;
    private readonly Task _stderr;
    private int _stopped;

    public int Pid { get; }
    public string BaseUrl { get; }

    public string Log
    {
        get
        {
            lock (_logGate)
                return _log.ToString();
        }
    }

    private GeneratedApi(Process process, string baseUrl)
    {
        _process = process;
        Pid = process.Id;
        BaseUrl = baseUrl;
        _stdout = PumpAsync(process.StandardOutput);
        _stderr = PumpAsync(process.StandardError);
    }

    public static async Task<GeneratedApi> StartAsync(string projectPath)
    {
        var projectDir = Path.GetDirectoryName(projectPath)
            ?? throw new InvalidOperationException($"API project has no directory: {projectPath}");
        var build = await RunToEndAsync(
            projectDir,
            ["build", projectPath, "--nologo", "--disable-build-servers", "-v", "m"],
            BuildTimeout);
        if (build.ExitCode != 0)
            throw new InvalidOperationException($"dotnet build of {projectPath} exited {build.ExitCode}.{Environment.NewLine}{build.Text}");

        var dll = Path.Combine(projectDir, "bin", "Debug", "net8.0", Path.GetFileNameWithoutExtension(projectPath) + ".dll");
        if (!File.Exists(dll))
            throw new InvalidOperationException($"Built API assembly was not found at {dll}.{Environment.NewLine}{build.Text}");

        var port = FreeLoopbackPort();
        var baseUrl = $"http://127.0.0.1:{port}";
        var psi = Prepare("dotnet", projectDir);
        psi.ArgumentList.Add(dll);
        psi.Environment["ASPNETCORE_URLS"] = baseUrl;

        GeneratedApi? api = null;
        try
        {
            var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start dotnet.");
            api = new GeneratedApi(process, baseUrl);
            await api.WaitUntilListeningAsync(port);
            return api;
        }
        catch
        {
            if (api is not null)
                await api.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _stopped, 1) == 1)
            return;

        try
        {
            if (!_process.HasExited && _process.Id == Pid)
                _process.Kill();
        }
        catch (InvalidOperationException)
        {
            // Exited between the check and the kill.
        }

        if (!_process.HasExited)
        {
            try
            {
                await _process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
            }
            catch (TimeoutException)
            {
                throw new InvalidOperationException($"Generated API process {Pid} was still running after it was stopped.");
            }
        }

        try
        {
            await Task.WhenAll(_stdout, _stderr).WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException)
        {
            // The pipes closed late. The recorded PID has already exited.
        }
        finally
        {
            _process.Dispose();
        }
    }

    private async Task WaitUntilListeningAsync(int port)
    {
        using var timeout = new CancellationTokenSource(StartupTimeout);
        while (true)
        {
            if (_process.HasExited)
            {
                throw new InvalidOperationException(
                    $"Generated API process {Pid} exited with code {_process.ExitCode} before listening on {BaseUrl}.{Environment.NewLine}{Log}");
            }

            try
            {
                if (await PortIsOpenAsync(port, timeout.Token))
                    return;
                await Task.Delay(200, timeout.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException(
                    $"Generated API process {Pid} did not listen on {BaseUrl} within {StartupTimeout.TotalSeconds:0} seconds.{Environment.NewLine}{Log}");
            }
        }
    }

    private async Task PumpAsync(StreamReader reader)
    {
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
                return;
            lock (_logGate)
                _log.AppendLine(line);
        }
    }

    private static async Task<(int ExitCode, string Text)> RunToEndAsync(string workingDirectory, IReadOnlyList<string> args, TimeSpan timeout)
    {
        var psi = Prepare("dotnet", workingDirectory);
        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start dotnet.");
        var pid = process.Id;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        try
        {
            await process.WaitForExitAsync().WaitAsync(timeout);
        }
        catch (TimeoutException)
        {
            if (!process.HasExited && process.Id == pid)
                process.Kill();
            throw new TimeoutException($"dotnet process {pid} exceeded {timeout.TotalMinutes:0} minutes ({string.Join(' ', args)}).");
        }

        var text = await stdout + await stderr;
        return (process.ExitCode, text);
    }

    private static ProcessStartInfo Prepare(string fileName, string workingDirectory)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
        };

        // A newer host may be the one on PATH. The generated project stays net8.0.
        psi.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        psi.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        psi.Environment.Remove("ASPNETCORE_HTTPS_PORT");
        foreach (var name in new[]
        {
            "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly",
            "OutputPath", "OutDir", "BaseOutputPath", "BaseIntermediateOutputPath", "IntermediateOutputPath",
            "Configuration", "TargetFramework", "UseAppHost",
        })
            psi.Environment.Remove(name);
        return psi;
    }

    private static async Task<bool> PortIsOpenAsync(int port, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        try
        {
            using var attempt = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            attempt.CancelAfter(TimeSpan.FromMilliseconds(500));
            await client.ConnectAsync(IPAddress.Loopback, port, attempt.Token);
            return true;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private static int FreeLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
