using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: a locked restore of the reference projects matches the committed lockfiles and the page
//   As Dana, I want the listed licences to match a locked restore, so that a stale lockfile or page cannot pass.
[Collection("Bench processes")] // the two classes that start the bench tools under a 20 second limit never overlap (card 368: the extra launches made the timing test flaky under the full run)
public class RestoredPackageLicencesLockedRestore
{
    private static readonly TimeSpan RestoreLimit = TimeSpan.FromMinutes(5);
    private const string ApiLockRel = "reference/csharp/Orders.Api/packages.lock.json";
    private const string SpecsLockRel = "reference/csharp/Orders.Specs/packages.lock.json";

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-3")]
    [Trait("Status", "Pending")] // green 1 (lockfiles and props) removes this Pending trait
    public async Task F5_locked_restore_matches_the_lockfiles_and_a_planted_mismatch_is_NU1004()
    {
        // Given committed lockfiles for the two reference projects
        var apiLock = RepoPaths.Find("reference", "csharp", "Orders.Api", "packages.lock.json");
        var specsLock = RepoPaths.Find("reference", "csharp", "Orders.Specs", "packages.lock.json");
        Assert.True(File.Exists(apiLock), "lockfile missing: " + ApiLockRel);
        Assert.True(File.Exists(specsLock), "lockfile missing: " + SpecsLockRel);

        // When a locked restore runs in a temp copy of reference/csharp
        using var match = TempWorkspace.Create();
        match.CopyFrom(RepoPaths.Find("reference", "csharp"));
        var apiProject = Path.Combine(match.Root, "Orders.Api", "Orders.Api.csproj");
        var specsProject = Path.Combine(match.Root, "Orders.Specs", "Orders.Specs.csproj");
        await RestoreLockedOrFail(apiProject);
        await RestoreLockedOrFail(specsProject);

        // Then the restored package set equals the union of the two lockfiles
        var restored = AssetsPackages(Path.Combine(match.Root, "Orders.Specs", "obj", "project.assets.json"));
        var locked = LockfilePackages(apiLock);
        locked.UnionWith(LockfilePackages(specsLock));
        Assert.True(restored.SetEquals(locked), DescribeSet("locked restore vs lockfiles", locked, restored));

        // And a planted lockfile mismatch restores with a non-zero exit and NU1004
        using var drift = TempWorkspace.Create();
        drift.CopyFrom(RepoPaths.Find("reference", "csharp"));
        var plantedLock = Path.Combine(drift.Root, "Orders.Api", "packages.lock.json");
        PlantLockfileMismatch(plantedLock);
        var planted = await RestoreLockedAsync(Path.Combine(drift.Root, "Orders.Api", "Orders.Api.csproj"));
        var plantedText = PublicText(planted.Stdout + planted.Stderr);
        Assert.False(planted.TimedOut, "restore exceeded 5 minutes");
        if (NetworkMissing(plantedText))
            Assert.Fail("NETWORK MISSING " + FirstLine(plantedText));
        Assert.NotEqual(0, planted.ExitCode);
        Assert.Contains("NU1004", plantedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-1")]
    [Trait("Status", "Pending")] // green 2 (the page) removes this Pending trait
    public async Task F6_restore_set_matches_the_page_and_the_packaged_licences()
    {
        // Given committed lockfiles and the restored-package licences page
        var apiLock = RepoPaths.Find("reference", "csharp", "Orders.Api", "packages.lock.json");
        var specsLock = RepoPaths.Find("reference", "csharp", "Orders.Specs", "packages.lock.json");
        Assert.True(File.Exists(apiLock), "lockfile missing: " + ApiLockRel);
        Assert.True(File.Exists(specsLock), "lockfile missing: " + SpecsLockRel);

        // When a locked restore runs and the page rows are read
        using var workspace = TempWorkspace.Create();
        workspace.CopyFrom(RepoPaths.Find("reference", "csharp"));
        await RestoreLockedOrFail(Path.Combine(workspace.Root, "Orders.Api", "Orders.Api.csproj"));
        await RestoreLockedOrFail(Path.Combine(workspace.Root, "Orders.Specs", "Orders.Specs.csproj"));
        var assetsPath = Path.Combine(workspace.Root, "Orders.Specs", "obj", "project.assets.json");
        Assert.True(File.Exists(assetsPath), "project.assets.json missing after locked restore");
        using var assets = JsonDocument.Parse(await File.ReadAllTextAsync(assetsPath));
        var restored = AssetsPackages(assets.RootElement);
        var pageText = (await File.ReadAllTextAsync(RepoPaths.Find("docs", "release", "pre-publication-checks.md")))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        var pageRows = ReadRestoredRows(pageText);

        // Then the restore set equals the page rows, and nuspec licences, licence files and the acceptance set match
        var pageSet = pageRows.Select(r => (r.Id, r.Version)).ToHashSet();
        Assert.True(restored.SetEquals(pageSet), DescribeSet("locked restore vs page", pageSet, restored));
        var acceptanceTrue = new HashSet<(string Id, string Version)>();
        foreach (var row in pageRows)
        {
            var dir = FindPackageDir(assets.RootElement, row.Id, row.Version);
            Assert.True(dir is not null, row.Id + " " + row.Version + ": package folder missing after restore");
            var nuspec = LoadNuspec(dir!);
            Assert.True(nuspec is not null, row.Id + " " + row.Version + ": nuspec missing after restore");
            if (RequiresAcceptance(nuspec!))
                acceptanceTrue.Add((row.Id, row.Version));
            if (row.LicenceSource == "nuspec licence expression")
            {
                var expression = LicenseExpression(nuspec!);
                Assert.Equal(expression, row.Licence);
            }
            else if (row.LicenceSource == "packaged licence file")
            {
                Assert.True(HasLicenceFile(dir!), row.Id + " " + row.Version + ": licence source is packaged licence file but no licence file is in the package");
            }
            else if (row.LicenceSource == "licence URL")
            {
                Assert.False(string.IsNullOrEmpty(LicenseUrl(nuspec!)), row.Id + " " + row.Version + ": licence source is licence URL but the nuspec has no licenseUrl");
            }
            else
            {
                Assert.Fail(row.Id + " " + row.Version + ": licence source is not one of the three labels");
            }
        }

        var body = RestoredBody(pageText);
        var namedAcceptance = new HashSet<(string Id, string Version)>();
        foreach (var line in body.Split('\n'))
        {
            if (line.IndexOf("acceptance", StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            foreach (var pkg in restored)
            {
                if (line.Contains(pkg.Id, StringComparison.Ordinal) && line.Contains(pkg.Version, StringComparison.Ordinal))
                    namedAcceptance.Add(pkg);
            }
        }
        Assert.True(namedAcceptance.SetEquals(acceptanceTrue), DescribeSet("acceptance set", acceptanceTrue, namedAcceptance));
    }

    private static async Task RestoreLockedOrFail(string projectPath)
    {
        var result = await RestoreLockedAsync(projectPath);
        var text = PublicText(result.Stdout + result.Stderr);
        Assert.False(result.TimedOut, "restore exceeded 5 minutes");
        if (NetworkMissing(text))
            Assert.Fail("NETWORK MISSING " + FirstLine(text));
        Assert.True(result.ExitCode == 0, text);
    }

    private static async Task<ProcessResult> RestoreLockedAsync(string projectPath)
    {
        var result = await RestoreAsync(projectPath, disableBuildServers: true);
        var text = result.Stdout + result.Stderr;
        if (result.ExitCode != 0
            && text.Contains("disable-build-servers", StringComparison.OrdinalIgnoreCase)
            && (text.Contains("unknown", StringComparison.OrdinalIgnoreCase)
                || text.Contains("Unrecognized", StringComparison.OrdinalIgnoreCase)))
        {
            result = await RestoreAsync(projectPath, disableBuildServers: false);
        }
        return result;
    }

    private static async Task<ProcessResult> RestoreAsync(string projectPath, bool disableBuildServers)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(projectPath)!,
        };
        foreach (var arg in new[] { "restore", projectPath, "--locked-mode", "--nologo" })
            start.ArgumentList.Add(arg);
        if (disableBuildServers)
            start.ArgumentList.Add("--disable-build-servers");
        start.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            start.Environment.Remove(name);
        return await ProcessRunner.RunAsync(start, RestoreLimit);
    }

    private static void PlantLockfileMismatch(string lockPath)
    {
        var node = JsonNode.Parse(File.ReadAllText(lockPath))
            ?? throw new InvalidOperationException("lockfile does not parse");
        var deps = node["dependencies"]?["net8.0"] as JsonObject
            ?? throw new InvalidOperationException("lockfile dependencies[net8.0] missing");
        if (!deps.Remove("FluentValidation"))
        {
            var victim = deps.FirstOrDefault(p => p.Value?["type"]?.GetValue<string>() == "Transitive").Key;
            Assert.False(string.IsNullOrEmpty(victim), "could not plant a lockfile mismatch");
            deps.Remove(victim);
        }
        File.WriteAllText(lockPath, node.ToJsonString());
    }

    private static HashSet<(string Id, string Version)> LockfilePackages(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var set = new HashSet<(string, string)>();
        if (!doc.RootElement.TryGetProperty("dependencies", out var deps))
            return set;
        if (!deps.TryGetProperty("net8.0", out var tfm))
            return set;
        foreach (var pkg in tfm.EnumerateObject())
        {
            if (pkg.Value.TryGetProperty("type", out var type) && type.GetString() == "Project")
                continue;
            if (!pkg.Value.TryGetProperty("resolved", out var resolved) || resolved.GetString() is not { } version)
                continue;
            set.Add((pkg.Name, version));
        }
        return set;
    }

    private static HashSet<(string Id, string Version)> AssetsPackages(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return AssetsPackages(doc.RootElement);
    }

    private static HashSet<(string Id, string Version)> AssetsPackages(JsonElement assets)
    {
        var set = new HashSet<(string, string)>();
        if (!assets.TryGetProperty("libraries", out var libraries))
            return set;
        foreach (var lib in libraries.EnumerateObject())
        {
            if (!lib.Value.TryGetProperty("type", out var type) || type.GetString() != "package")
                continue;
            var slash = lib.Name.IndexOf('/');
            if (slash <= 0)
                continue;
            set.Add((lib.Name[..slash], lib.Name[(slash + 1)..]));
        }
        return set;
    }

    private sealed record PageRow(string Id, string Version, string Kind, string Licence, string LicenceSource, string WhereUsed, string Review);

    private static string RestoredBody(string page)
    {
        const string title = "## Restored package licences\n";
        var at = page.IndexOf(title, StringComparison.Ordinal);
        Assert.True(at >= 0, "section missing: Restored package licences");
        var start = at + title.Length;
        var nxt = page.IndexOf("\n## ", start, StringComparison.Ordinal);
        return nxt < 0 ? page[start..] : page[start..nxt];
    }

    private static List<PageRow> ReadRestoredRows(string page)
    {
        var rows = new List<PageRow>();
        foreach (var line in RestoredBody(page).Split('\n'))
        {
            if (!line.StartsWith('|'))
                continue;
            var cells = line.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray();
            if (cells.Length < 7)
                continue;
            if (cells[0].Length == 0 || cells[0].All(ch => ch is '-' or ':') || cells[0].Equals("id", StringComparison.OrdinalIgnoreCase))
                continue;
            rows.Add(new PageRow(cells[0], cells[1], cells[2], cells[3], cells[4], cells[5], cells[6]));
        }
        return rows;
    }

    private static string? FindPackageDir(JsonElement assets, string id, string version)
    {
        if (!assets.TryGetProperty("packageFolders", out var folders))
            return null;
        foreach (var folder in folders.EnumerateObject())
        {
            var dir = Path.Combine(folder.Name, id.ToLowerInvariant(), version);
            if (Directory.Exists(dir))
                return dir;
        }
        return null;
    }

    private static XDocument? LoadNuspec(string packageDir)
    {
        var files = Directory.GetFiles(packageDir, "*.nuspec", SearchOption.TopDirectoryOnly);
        return files.Length == 0 ? null : XDocument.Load(files[0]);
    }

    private static string? LicenseExpression(XDocument doc)
    {
        var el = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "license");
        if (el is null)
            return null;
        var type = (string?)el.Attribute("type");
        return string.Equals(type, "expression", StringComparison.OrdinalIgnoreCase) ? el.Value.Trim() : null;
    }

    private static string? LicenseUrl(XDocument doc) =>
        doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "licenseUrl")?.Value.Trim();

    private static bool RequiresAcceptance(XDocument doc)
    {
        var el = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "requireLicenseAcceptance");
        return el is not null && string.Equals(el.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasLicenceFile(string packageDir)
    {
        foreach (var file in Directory.EnumerateFiles(packageDir, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(file);
            if (name.StartsWith("license", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("licence", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("copying", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static bool NetworkMissing(string text) =>
        text.Contains("NU1301", StringComparison.OrdinalIgnoreCase)
        || text.Contains("Unable to load the service index", StringComparison.OrdinalIgnoreCase)
        || text.Contains("No such host is known", StringComparison.OrdinalIgnoreCase)
        || text.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
        || text.Contains("nodename nor servname provided", StringComparison.OrdinalIgnoreCase)
        || text.Contains("The remote name could not be resolved", StringComparison.OrdinalIgnoreCase);

    private static string DescribeSet(string label, HashSet<(string Id, string Version)> expected, HashSet<(string Id, string Version)> actual)
    {
        var missing = string.Join(", ", expected.Except(actual).OrderBy(p => p.Id).Select(p => p.Id + " " + p.Version));
        var extra = string.Join(", ", actual.Except(expected).OrderBy(p => p.Id).Select(p => p.Id + " " + p.Version));
        return label + " missing: " + missing + "; extra: " + extra;
    }

    private static string PublicText(string text)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(home))
            text = text.Replace(home, "<home>", StringComparison.OrdinalIgnoreCase);
        var temp = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!string.IsNullOrEmpty(temp))
            text = text.Replace(temp, "<temp>", StringComparison.OrdinalIgnoreCase);
        return text;
    }

    private static string FirstLine(string text)
    {
        var line = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').FirstOrDefault(l => l.Trim().Length > 0);
        return line ?? "";
    }
}
