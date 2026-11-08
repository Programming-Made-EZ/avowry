using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: Dana installs a versioned Avowry CLI as a .NET tool from a local feed
//   As Dana, I want to pack, install and run avowry without a global install,
//   so that I can try Avowry without building its source.
public class PackedToolIsVersionedInstallsAndBuildsTheDemo
{
    [Fact]
    [Trait("Slice", "176")]
    [Trait("Status", "Pending")]
    [Trait("Criterion", "176-1")]
    public async Task Packed_tool_is_versioned_installs_and_builds_the_demo()
    {
        // Given a fresh local feed, a scratch tool path, and the CLI project as it is
        using var feed = TempWorkspace.Create();
        using var tools = TempWorkspace.Create();
        using var config = TempWorkspace.Create();
        using var demoOut = TempWorkspace.Create();
        var checks = new Checks();
        string? packageVersion = null;
        string? toolExe = null;
        var project = RepoPaths.Find("src", "Avowry.Cli", "Avowry.Cli.csproj");
        var demo = RepoPaths.Find("demo", "tasks", "Tasks.avowry");

        var beforeGlobal = await DotnetAsync(RepoPaths.Root, "tool", "list", "-g");

        // When Dana packs the CLI, inspects the package, and installs it from that feed only
        var pack = await DotnetAsync(RepoPaths.Root, "pack", project, "-c", "Release", "-o", feed.Root);
        var nupkgs = Directory.GetFiles(feed.Root, "*.nupkg");
        checks.Record("pack", () => AssertPack(pack, nupkgs));
        checks.Record("metadata", () =>
        {
            Assert.True(nupkgs.Length > 0, "blocked: no nupkg to inspect");
            var info = ReadPackage(nupkgs[0]);
            packageVersion = info.Version;
            AssertMetadata(info);
        });
        checks.Record("explicit-version-source", AssertExplicitVersion);

        var nugetConfig = Path.Combine(config.Root, "nuget.config");
        await File.WriteAllTextAsync(nugetConfig, IsolatedFeedConfig(feed.Root));
        var install = await DotnetAsync(
            config.Root,
            "tool", "install", "Avowry",
            "--tool-path", tools.Root,
            "--add-source", feed.Root,
            "--configfile", nugetConfig);
        var afterGlobal = await DotnetAsync(RepoPaths.Root, "tool", "list", "-g");
        var avowry = Path.Combine(tools.Root, OperatingSystem.IsWindows() ? "avowry.exe" : "avowry");
        if (File.Exists(avowry))
            toolExe = avowry;

        checks.Record("install", () => AssertInstall(install, beforeGlobal, afterGlobal, avowry));

        // Then every independent check is reported, including those blocked by a missing package
        if (toolExe is not null)
        {
            var version = await RunToolAsync(toolExe, "--version");
            var built = await RunToolAsync(toolExe, "build", demo, "-o", demoOut.Root);
            checks.Record("version", () => AssertVersion(version, packageVersion));
            checks.Record("demo-build", () => AssertDemoBuild(built, demoOut.Root));
        }
        else
        {
            checks.Record("version", () => Assert.Fail("blocked: tool not installed"));
            checks.Record("demo-build", () => Assert.Fail("blocked: tool not installed"));
        }

        checks.AssertAll();
    }

    private static void AssertPack(ProcessResult pack, string[] nupkgs)
    {
        var problems = new List<string>();
        if (pack.TimedOut)
            problems.Add("dotnet pack timed out");
        if (pack.ExitCode != 0)
            problems.Add("dotnet pack exited " + pack.ExitCode);
        if (nupkgs.Length != 1)
            problems.Add("expected exactly one .nupkg, found " + nupkgs.Length + ": " + Names(nupkgs));
        else if (!NamedForAvowry(nupkgs[0]))
            problems.Add("nupkg is not named for package ID Avowry: " + Path.GetFileName(nupkgs[0]));
        Assert.True(problems.Count == 0, string.Join("; ", problems) + (pack.ExitCode != 0 || pack.TimedOut ? Extra(pack) : ""));
    }

    private static void AssertMetadata(PackageInfo info)
    {
        var problems = new List<string>();
        if (info.Id != "Avowry")
            problems.Add("id is " + (info.Id ?? "<missing>"));
        if (string.IsNullOrWhiteSpace(info.Version) || info.Version == "1.0.0")
            problems.Add("Version is " + (string.IsNullOrWhiteSpace(info.Version) ? "<missing>" : info.Version));
        if (info.Command != "avowry")
            problems.Add("tool command is " + (info.Command ?? "<missing>"));
        Assert.True(problems.Count == 0, string.Join("; ", problems));
    }

    private static void AssertExplicitVersion()
    {
        var version = ExplicitVersion();
        Assert.False(string.IsNullOrWhiteSpace(version), "CLI project and Directory.Build.props set no Version or PackageVersion");
        Assert.NotEqual("1.0.0", version);
    }

    private static void AssertInstall(ProcessResult install, ProcessResult before, ProcessResult after, string avowry)
    {
        var problems = new List<string>();
        if (install.TimedOut)
            problems.Add("dotnet tool install timed out");
        if (install.ExitCode != 0)
            problems.Add("dotnet tool install exited " + install.ExitCode);
        if (Normalize(before.Stdout) != Normalize(after.Stdout))
            problems.Add("global tool list changed");
        if (!File.Exists(avowry))
            problems.Add("installed command missing");
        Assert.True(problems.Count == 0, string.Join("; ", problems) + Extra(install));
    }

    private static void AssertVersion(ProcessResult version, string? packageVersion)
    {
        Assert.False(string.IsNullOrWhiteSpace(packageVersion), "blocked: package version unknown");
        Assert.False(version.TimedOut, "avowry --version timed out");
        Assert.True(version.ExitCode == 0, "avowry --version exited " + version.ExitCode + Extra(version));
        Assert.Equal(packageVersion, version.Stdout.Trim());
    }

    private static void AssertDemoBuild(ProcessResult built, string output)
    {
        Assert.False(built.TimedOut, "avowry build timed out");
        Assert.True(built.ExitCode == 0, "avowry build exited " + built.ExitCode + Extra(built));
        Assert.True(File.Exists(Find(output, "*.Api.csproj")), "generated API project is missing");
        Assert.True(File.Exists(Find(output, "*.Specs.csproj")), "generated specs project is missing");
    }

    private static PackageInfo ReadPackage(string nupkg)
    {
        using var zip = ZipFile.OpenRead(nupkg);
        var nuspec = zip.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
        Assert.True(nuspec is not null, "nupkg has no .nuspec");
        using var stream = nuspec.Open();
        var xml = XDocument.Load(stream);
        var settings = zip.Entries.FirstOrDefault(entry =>
            entry.Name.Equals("DotnetToolSettings.xml", StringComparison.OrdinalIgnoreCase));
        string? command = null;
        if (settings is not null)
        {
            using var settingsStream = settings.Open();
            command = XDocument.Load(settingsStream).Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "Command")
                ?.Attribute("Name")?.Value;
        }
        return new PackageInfo(Value(xml, "id"), Value(xml, "version"), command);
    }

    private static string? ExplicitVersion()
    {
        foreach (var path in new[]
        {
            RepoPaths.Find("src", "Avowry.Cli", "Avowry.Cli.csproj"),
            RepoPaths.Find("Directory.Build.props"),
        })
        {
            foreach (var name in new[] { "Version", "PackageVersion" })
            {
                var value = XDocument.Load(path).Descendants()
                    .FirstOrDefault(element => element.Name.LocalName == name)?.Value?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }
        return null;
    }

    private static string IsolatedFeedConfig(string feed) =>
        new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("configuration",
                new XElement("packageSources",
                    new XElement("clear"),
                    new XElement("add",
                        new XAttribute("key", "local-feed"),
                        new XAttribute("value", feed))))).ToString();

    private static async Task<ProcessResult> DotnetAsync(string workingDirectory, params string[] args) =>
        await RunAsync("dotnet", workingDirectory, args);

    private static async Task<ProcessResult> RunToolAsync(string tool, params string[] args) =>
        await RunAsync(tool, RepoPaths.Root, args);

    private static async Task<ProcessResult> RunAsync(string fileName, string workingDirectory, string[] args)
    {
        var start = new ProcessStartInfo(fileName)
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

    private static bool NamedForAvowry(string path)
    {
        var name = Path.GetFileName(path);
        return name.StartsWith("Avowry.", StringComparison.Ordinal)
            && name.EndsWith(".nupkg", StringComparison.Ordinal)
            && name.Length > 8
            && char.IsDigit(name[7]);
    }

    private static string? Value(XDocument xml, string local) =>
        xml.Descendants().FirstOrDefault(element => element.Name.LocalName == local)?.Value?.Trim();

    private static string Find(string root, string pattern)
    {
        var matches = Directory.GetFiles(root, pattern, SearchOption.AllDirectories);
        return matches.Length == 1 ? matches[0] : "";
    }

    private static string Names(string[] nupkgs) =>
        nupkgs.Length == 0 ? "<none>" : string.Join(", ", nupkgs.Select(Path.GetFileName));

    private static string Extra(ProcessResult result)
    {
        var text = result.Stdout + result.Stderr;
        return string.IsNullOrWhiteSpace(text) ? "" : Environment.NewLine + text;
    }

    private static string Normalize(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);

    private sealed record PackageInfo(string? Id, string? Version, string? Command);

    private sealed class Checks
    {
        private readonly List<string> failures = [];

        public void Record(string name, Action check)
        {
            try
            {
                check();
            }
            catch (Exception ex)
            {
                var text = Normalize(ex.Message);
                var kind = text.Contains("blocked:", StringComparison.Ordinal) ? "blocked" : "failed";
                failures.Add(name + " (" + kind + "): " + text);
            }
        }

        public void AssertAll() =>
            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
}
