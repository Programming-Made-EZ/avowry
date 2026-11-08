using System.Diagnostics;
using System.Xml.Linq;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Scenario [E]: the package is absent from the local feed; install must not fall through to a public source.
public class InstallFromAnEmptyFeedFailsWithoutReachingAPublicSource
{
    [Fact]
    [Trait("Slice", "176")]
    [Trait("Criterion", "176-E")]
    public async Task Install_from_an_empty_feed_fails_without_reaching_a_public_source()
    {
        // Given an empty local folder feed and a nuget.config that clears all sources and adds only that feed
        using var feed = TempWorkspace.Create();
        using var tools = TempWorkspace.Create();
        using var config = TempWorkspace.Create();
        var nugetConfig = Path.Combine(config.Root, "nuget.config");
        await File.WriteAllTextAsync(nugetConfig, IsolatedFeedConfig(feed.Root));
        var avowry = Path.Combine(tools.Root, OperatingSystem.IsWindows() ? "avowry.exe" : "avowry");
        var beforeGlobal = await DotnetAsync(RepoPaths.Root, "tool", "list", "-g");

        // When installation is attempted from that config only
        var install = await DotnetAsync(
            config.Root,
            "tool", "install", "Avowry",
            "--tool-path", tools.Root,
            "--add-source", feed.Root,
            "--configfile", nugetConfig);
        var afterGlobal = await DotnetAsync(RepoPaths.Root, "tool", "list", "-g");
        var output = install.Stdout + install.Stderr;

        // Then installation fails without falling back to a public feed
        Assert.False(install.TimedOut, output);
        Assert.NotEqual(0, install.ExitCode);
        Assert.Contains("Avowry", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not found in NuGet feeds", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(feed.Root, output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("nuget.org", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", output, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(avowry));
        Assert.Equal(Normalize(beforeGlobal.Stdout), Normalize(afterGlobal.Stdout));
    }

    private static string IsolatedFeedConfig(string feed) =>
        new XDocument(
            new XElement("configuration",
                new XElement("packageSources",
                    new XElement("clear"),
                    new XElement("add",
                        new XAttribute("key", "local-feed"),
                        new XAttribute("value", feed))))).ToString();

    private static async Task<ProcessResult> DotnetAsync(string workingDirectory, params string[] args)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory,
        };
        foreach (var arg in args)
            start.ArgumentList.Add(arg);
        start.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            start.Environment.Remove(name);
        return await ProcessRunner.RunAsync(start, TimeSpan.FromMinutes(5));
    }

    private static string Normalize(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
}
