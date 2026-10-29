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
            throw new InvalidOperationException("Avowry CLI is stale, run dotnet build src/Phrase.Cli");
    }

    /// <summary>
    /// Each project is compared with its own built assembly: an implementation-only edit in a referenced project
    /// refreshes that project's assembly but not phrase.dll, so one shared dll would report a false stale.
    /// </summary>
    public static void EnsureAllFresh(
        IEnumerable<(string DllPath, IEnumerable<string> SourceFiles)> projects,
        Func<string, DateTime> lastWriteUtc,
        TimeSpan tolerance)
    {
        foreach (var (dllPath, sourceFiles) in projects)
            EnsureFresh(dllPath, sourceFiles, lastWriteUtc, tolerance);
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

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start the Avowry CLI.");
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
            throw new TimeoutException($"Avowry CLI exceeded {Timeout.TotalMinutes} minutes (pid {id}).");
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
                throw new FileNotFoundException($"The Avowry CLI project is not in the repo yet: {project}");

            var psi = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = RepoPaths.Root,
            };
            foreach (var arg in new[] { "build", project, "--nologo", "--disable-build-servers", "--configuration", Configuration() })
                psi.ArgumentList.Add(arg);
            ApplyHostEnvironment(psi);

            using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start dotnet build for the Avowry CLI.");
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
                throw new TimeoutException($"dotnet build of the Avowry CLI exceeded {Timeout.TotalMinutes} minutes (pid {id}).");
            }

            var log = new StringBuilder().Append(await stdout).Append(await stderr).ToString();
            if (process.ExitCode != 0 || !File.Exists(dll))
                throw new InvalidOperationException($"Avowry CLI did not build ({dll}).{Environment.NewLine}{log}");
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
        var output = Path.GetDirectoryName(DllPath())!;
        var shared = new[] { RepoPaths.Find("Directory.Build.props") }.Where(File.Exists).ToArray();
        var projects = new[]
        {
            ("Phrase.Cli", "phrase.dll"),
            ("Avowry.Compiler", "Avowry.Compiler.dll"),
            ("Phrase.Adapters.CSharp", "Phrase.Adapters.CSharp.dll"),
        }.Select(project => (
            DllPath: Path.Combine(output, project.Item2),
            SourceFiles: SourceFiles(RepoPaths.Find("src", project.Item1)).Concat(shared)));
        EnsureAllFresh(projects, File.GetLastWriteTimeUtc, FreshnessTolerance);
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
