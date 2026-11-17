using System.Diagnostics;
using System.Reflection;

namespace Avowry.Acceptance.Tests.Support;

/// <summary>
/// Card 172 (criterion 2): the red tests reach the shared helper by name, through reflection, so that its absence fails
/// only the tests that need it instead of stopping the whole test project from compiling. The seam the green must add:
/// <c>Avowry.Acceptance.Tests.Support.NestedDotnet</c> (static, in Support/NestedDotnet.cs) with
/// <c>Apply(ProcessStartInfo)</c> (environment scrub plus node-reuse and roll-forward settings) and
/// <c>RunAsync(ProcessStartInfo, TimeSpan)</c> (start, drain both streams, wait, and on timeout kill the whole tree and wait).
/// On timeout RunAsync may either throw TimeoutException or return a result with a bool property TimedOut set.
/// </summary>
internal static class NestedDotnetSeam172
{
    internal const string TypeName = "Avowry.Acceptance.Tests.Support.NestedDotnet";
    private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    internal static Type Require() =>
        typeof(NestedDotnetSeam172).Assembly.GetType(TypeName)
        ?? throw new Xunit.Sdk.XunitException(
            $"The shared nested-dotnet helper {TypeName} does not exist yet (add tests/Avowry.Acceptance.Tests/Support/NestedDotnet.cs).");

    internal static void Apply(ProcessStartInfo info)
    {
        var method = Require().GetMethod("Apply", All, [typeof(ProcessStartInfo)])
            ?? throw new Xunit.Sdk.XunitException($"{TypeName}.Apply(ProcessStartInfo) does not exist.");
        method.Invoke(null, [info]);
    }

    /// <summary>Runs through the helper; true means the helper timed the child out (and so killed and awaited its tree).</summary>
    internal static async Task<bool> RunAsync(ProcessStartInfo info, TimeSpan timeout)
    {
        var method = Require().GetMethods(All).FirstOrDefault(candidate =>
            candidate.Name == "RunAsync"
            && candidate.GetParameters() is [{ } first, { } second]
            && first.ParameterType == typeof(ProcessStartInfo)
            && (second.ParameterType == typeof(TimeSpan) || second.ParameterType == typeof(TimeSpan?)))
            ?? throw new Xunit.Sdk.XunitException($"{TypeName}.RunAsync(ProcessStartInfo, TimeSpan) does not exist.");
        try
        {
            var task = (Task)method.Invoke(null, [info, timeout])!;
            await task;
            var result = task.GetType().GetProperty("Result")?.GetValue(task);
            return result?.GetType().GetProperty("TimedOut")?.GetValue(result) is true;
        }
        catch (TimeoutException)
        {
            return true;
        }
        catch (TargetInvocationException exception) when (exception.InnerException is TimeoutException)
        {
            return true;
        }
    }
}
