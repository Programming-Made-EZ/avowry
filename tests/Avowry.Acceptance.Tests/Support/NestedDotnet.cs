using System.Diagnostics;

namespace Avowry.Acceptance.Tests.Support;

/// <summary>
/// Card 172: one place for nested-dotnet environment scrub, node-reuse settings, timeouts and process-tree kill.
/// </summary>
internal static class NestedDotnet
{
    private const string RollForwardVariable = "DOTNET_ROLL_FORWARD";
    private const string RollForwardValue = "Major";
    private const string DisableNodeReuseVariable = "MSBUILDDISABLENODEREUSE";
    private const string DisableNodeReuseValue = "1";

    private static readonly string[] ScrubbedVariables =
    [
        "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly",
        "OutputPath", "OutDir", "BaseOutputPath", "BaseIntermediateOutputPath", "IntermediateOutputPath",
        "Configuration", "TargetFramework", "UseAppHost",
    ];

    internal static void Apply(ProcessStartInfo startInfo)
    {
        startInfo.Environment[RollForwardVariable] = RollForwardValue;
        startInfo.Environment[DisableNodeReuseVariable] = DisableNodeReuseValue;
        foreach (var name in ScrubbedVariables)
            startInfo.Environment.Remove(name);
    }

    internal static void ApplyProcessWideNodeReuseDisable() =>
        Environment.SetEnvironmentVariable(DisableNodeReuseVariable, DisableNodeReuseValue);

    internal static async Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, TimeSpan timeout)
    {
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Process did not start: {startInfo.FileName}");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        var timedOut = false;

        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            timedOut = true;
            KillEntireTreeIfRunning(process);
            await process.WaitForExitAsync();
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return new ProcessResult(process.ExitCode, stdout, stderr, timedOut);
    }

    internal static void KillEntireTreeIfRunning(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // Already exited between the check and the kill.
        }
    }
}