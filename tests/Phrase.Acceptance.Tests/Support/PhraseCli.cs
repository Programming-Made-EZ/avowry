using System.Diagnostics;
using System.Text;

namespace Phrase.Acceptance.Tests.Support;

public sealed record CliRun(int ExitCode, string Stdout, string Stderr)
{
    public string Text => Stdout + Stderr;
}

/// <summary>
/// Runs the real phrase executable (the built phrase.dll) the way a user would,
/// with the same dotnet host settings the nested builds already use.
/// </summary>
public static class PhraseCli
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan FreshnessTolerance = TimeSpan.FromSeconds(2);
    private static readonly SemaphoreSlim BuildGate = new(1, 1);
    private static readonly Lazy<bool> FreshnessChecked = new(CheckProductionFreshness);

    public static void EnsureFresh(
        string dllPath,
        IEnumerable<string> sourceFiles,
        Func<string, DateTime> lastWriteUtc,
        TimeSpan tolerance)
    {
        var dllWriteTime = lastWriteUtc(dllPath);
        if (sourceFiles.Any(source => lastWriteUtc(source) > dllWriteTime + tolerance))
            throw new InvalidOperationException("phrase CLI is stale, run dotnet build src/Phrase.Cli");
    }

    public static void EnsureFresh() => _ = FreshnessChecked.Value;

    public static async Task<CliRun> RunAsync(params string[] args)
    {
        var dll = await EnsureBuiltAsync();
        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = RepoPaths.Root,
        };
        psi.ArgumentList.Add(dll);
        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        ApplyHostEnvironment(psi);

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start the phrase CLI.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        using var cts = new CancellationTokenSource(Timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            var id = process.Id;
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            throw new TimeoutException($"phrase CLI exceeded {Timeout.TotalMinutes} minutes (pid {id}).");
        }

        return new CliRun(process.ExitCode, await stdout, await stderr);
    }

    private static async Task<string> EnsureBuiltAsync()
    {
        var dll = DllPath();
        if (File.Exists(dll))
        {
            EnsureFresh();
            return dll;
        }

        await BuildGate.WaitAsync();
        try
        {
            if (File.Exists(dll))
                return dll;

            var project = RepoPaths.Find("src", "Phrase.Cli", "Phrase.Cli.csproj");
            if (!File.Exists(project))
                throw new FileNotFoundException($"The phrase CLI project is not in the repo yet: {project}");

            var psi = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = RepoPaths.Root,
            };
            foreach (var arg in new[] { "build", project, "--nologo", "--disable-build-servers", "--configuration", Configuration() })
                psi.ArgumentList.Add(arg);
            ApplyHostEnvironment(psi);

            using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start dotnet build for the phrase CLI.");
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var cts = new CancellationTokenSource(Timeout);
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                var id = process.Id;
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
                throw new TimeoutException($"dotnet build of the phrase CLI exceeded {Timeout.TotalMinutes} minutes (pid {id}).");
            }

            var log = new StringBuilder().Append(await stdout).Append(await stderr).ToString();
            if (process.ExitCode != 0 || !File.Exists(dll))
                throw new InvalidOperationException($"phrase CLI did not build ({dll}).{Environment.NewLine}{log}");
            EnsureFresh();
            return dll;
        }
        finally
        {
            BuildGate.Release();
        }
    }

    private static string DllPath() =>
        RepoPaths.Find("src", "Phrase.Cli", "bin", Configuration(), "net8.0", "phrase.dll");

    private static string Configuration() =>
        AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";

    private static bool CheckProductionFreshness()
    {
        var sources = new[]
        {
            RepoPaths.Find("src", "Phrase.Cli"),
            RepoPaths.Find("src", "Phrase.Compiler"),
            RepoPaths.Find("src", "Phrase.Adapters.CSharp"),
        }.SelectMany(SourceFiles);
        EnsureFresh(DllPath(), sources, File.GetLastWriteTimeUtc, FreshnessTolerance);
        return true;
    }

    private static IEnumerable<string> SourceFiles(string projectDirectory) =>
        Directory.EnumerateFiles(projectDirectory, "*", SearchOption.AllDirectories)
            .Where(path =>
                (path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".csproj", StringComparison.Ordinal)) &&
                !Path.GetRelativePath(projectDirectory, path).Split(Path.DirectorySeparatorChar)
                    .Any(part => part.Equals("bin", StringComparison.Ordinal) || part.Equals("obj", StringComparison.Ordinal)));

    private static void ApplyHostEnvironment(ProcessStartInfo psi)
    {
        // A newer system host may have no matching ASP.NET Core 8 runtime. Match DotnetCli.
        psi.Environment["DOTNET_ROLL_FORWARD"] = "Major";

        // A reused MSBuild node outlives the build and keeps the redirected output pipe open.
        psi.Environment["MSBUILDDISABLENODEREUSE"] = "1";

        // The outer test host sets MSBuild variables that would hijack a nested build.
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            psi.Environment.Remove(name);
    }
}
