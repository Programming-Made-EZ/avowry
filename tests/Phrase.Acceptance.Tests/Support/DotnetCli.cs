using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Phrase.Acceptance.Tests.Support;

public sealed record TestRunResult(int ExitCode, int? Total, int? Failed, string Log)
{
    /// <summary>The run produced a test summary, which only happens after a successful build.</summary>
    public bool Built => Total is not null;
}

public static partial class DotnetCli
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    public static async Task<TestRunResult> TestAsync(string projectPath)
    {
        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(projectPath)!,
        };
        foreach (var arg in new[] { "test", projectPath, "--nologo", "--disable-build-servers" })
            psi.ArgumentList.Add(arg);

        // The outer test host sets MSBuild variables that would hijack the nested build.
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            psi.Environment.Remove(name);

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start dotnet.");
        var log = new StringBuilder();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        using var cts = new CancellationTokenSource(Timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"dotnet test exceeded {Timeout.TotalMinutes} minutes for {projectPath}");
        }

        log.Append(await stdout).Append(await stderr);
        var text = log.ToString();

        return new TestRunResult(
            process.ExitCode,
            Total: Capture(TotalPattern(), text),
            Failed: Capture(FailedPattern(), text),
            Log: text);
    }

    private static int? Capture(Regex pattern, string text)
    {
        var match = pattern.Match(text);
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

    [GeneratedRegex(@"Total:\s+(\d+)")]
    private static partial Regex TotalPattern();

    [GeneratedRegex(@"Failed:\s+(\d+)")]
    private static partial Regex FailedPattern();
}
