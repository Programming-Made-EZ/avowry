using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Avowry.Acceptance.Tests.Support;

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

        NestedDotnet.Apply(psi);

        var result = await NestedDotnet.RunAsync(psi, Timeout);
        if (result.TimedOut)
            throw new TimeoutException($"dotnet test exceeded {Timeout.TotalMinutes} minutes for {projectPath}");

        var text = result.Stdout + result.Stderr;

        return new TestRunResult(
            result.ExitCode,
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