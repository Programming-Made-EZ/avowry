using System.Diagnostics;

namespace Phrase.Acceptance.Tests.Support;

/// <summary>
/// Card 264 (D157 Criterion 2, cycle 2b): the one place that builds the start of a Phrase.Bench child. The child is
/// `dotnet run --no-build -c &lt;configuration of the running test assembly&gt;`: the outer test build already built
/// Phrase.Bench through a ProjectReference, so a child never builds (concurrent children that each built the same obj
/// output collided and one exited 1 with no history). Missing or stale output throws BenchLauncherException before any
/// process exists; this class never starts a build and never drops --no-build for Phrase.Bench.
/// </summary>
internal static class BenchLauncher
{
    private const string BenchProject = "Phrase.Bench";
    private static readonly char[] Separators = [(char)92, '/'];

    internal static ProcessStartInfo CreateStartInfo(IEnumerable<string> benchArguments, IEnumerable<string>? runOptions = null) =>
        CreateStartInfo(RepoPaths.Root, AppContext.BaseDirectory, benchArguments, runOptions);

    internal static ProcessStartInfo CreateStartInfo(
        string repoRoot, string testBaseDirectory, IEnumerable<string> benchArguments, IEnumerable<string>? runOptions = null)
    {
        var configuration = ConfigurationOf(testBaseDirectory);
        EnsureFreshOutput(repoRoot, configuration);

        var info = NewInfo();
        info.ArgumentList.Add("run");
        info.ArgumentList.Add("--no-build");
        info.ArgumentList.Add("-c");
        info.ArgumentList.Add(configuration);
        foreach (var option in runOptions ?? [])
            info.ArgumentList.Add(option);
        info.ArgumentList.Add("--project");
        info.ArgumentList.Add(Path.Combine(repoRoot, "tools", BenchProject, BenchProject + ".csproj"));
        info.ArgumentList.Add("--");
        foreach (var argument in benchArguments)
            info.ArgumentList.Add(argument);
        return info;
    }

    /// <summary>
    /// For a site shared with another benchmark project (the C# baseline arm): Phrase.Bench goes through the no-build
    /// launcher; any other project keeps its existing build-capable `dotnet run` (it is not built by the test assembly).
    /// </summary>
    internal static ProcessStartInfo CreateStartInfoFor(string projectName, IEnumerable<string> benchArguments, IEnumerable<string>? runOptions = null)
    {
        if (projectName == BenchProject)
            return CreateStartInfo(benchArguments, runOptions);

        var info = NewInfo();
        info.ArgumentList.Add("run");
        foreach (var option in runOptions ?? [])
            info.ArgumentList.Add(option);
        info.ArgumentList.Add("--project");
        info.ArgumentList.Add(RepoPaths.Find("tools", projectName, projectName + ".csproj"));
        info.ArgumentList.Add("--");
        foreach (var argument in benchArguments)
            info.ArgumentList.Add(argument);
        return info;
    }

    /// <summary>"Debug" or "Release": the last path segment of the test assembly directory that names one (case-insensitive).</summary>
    internal static string ConfigurationOf(string testBaseDirectory)
    {
        var segments = testBaseDirectory.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        for (var i = segments.Length - 1; i >= 0; i--)
        {
            if (segments[i].Equals("Release", StringComparison.OrdinalIgnoreCase)) return "Release";
            if (segments[i].Equals("Debug", StringComparison.OrdinalIgnoreCase)) return "Debug";
        }

        throw new BenchLauncherException(
            $"Cannot tell the build configuration (Debug or Release) from the test assembly directory '{testBaseDirectory}'.");
    }

    private static ProcessStartInfo NewInfo() => new("dotnet")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };

    private static void EnsureFreshOutput(string repoRoot, string configuration)
    {
        var benchDirectory = Path.Combine(repoRoot, "tools", BenchProject);
        var outputDirectory = Path.Combine(benchDirectory, "bin", configuration);
        var remedy = $"Build it first with `dotnet build Phrase.sln -c {configuration}`; the launcher never builds.";

        var dlls = Directory.Exists(outputDirectory)
            ? Directory.GetFiles(outputDirectory, BenchProject + ".dll", SearchOption.AllDirectories)
            : [];
        if (dlls.Length == 0)
            throw new BenchLauncherException(
                $"{BenchProject} {configuration} output is missing: no {BenchProject}.dll under {outputDirectory}. {remedy}");

        var outputTime = dlls.Max(File.GetLastWriteTimeUtc);
        var newestSource = Directory.EnumerateFiles(benchDirectory, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(BenchProject + ".csproj", StringComparison.OrdinalIgnoreCase))
            .Where(path => !IsUnderBinOrObj(benchDirectory, path))
            .Select(path => (Path: path, Time: File.GetLastWriteTimeUtc(path)))
            .OrderByDescending(file => file.Time)
            .FirstOrDefault();
        if (newestSource.Path is not null && outputTime < newestSource.Time)
            throw new BenchLauncherException(
                $"{BenchProject} {configuration} output is stale: {BenchProject}.dll under {outputDirectory} " +
                $"({outputTime:O}) is older than {newestSource.Path} ({newestSource.Time:O}). {remedy}");
    }

    private static bool IsUnderBinOrObj(string benchDirectory, string path)
    {
        var first = Path.GetRelativePath(benchDirectory, path).Split(Separators)[0];
        return first.Equals("bin", StringComparison.OrdinalIgnoreCase) || first.Equals("obj", StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed class BenchLauncherException(string message) : Exception(message);
