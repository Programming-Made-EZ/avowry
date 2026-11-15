using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Avowry.Acceptance.Tests;

// Card 422 helper ports from the reference checks (reference-refcheck.py.txt):
// classify_licence_text, licence_value_failures and restore_outcome.

/// <summary>The nuspec facts of one restored package: its licence expression, its licenseUrl, and the head text of each packaged licence file.</summary>
internal sealed record PackageEvidence(string? Expression, string? LicenceUrl, IReadOnlyDictionary<string, string> LicenceFiles);

/// <summary>One restored package: its folder, nuspec state (missing, broken or ok), evidence (when the nuspec parses) and whether it asks for licence acceptance.</summary>
internal sealed record RestoredPackage(string Id, string Version, bool FolderFound, string NuspecState, PackageEvidence? Evidence, bool RequiresAcceptance);

internal static class RestoredLicenceChecks
{
    private static readonly Dictionary<(string Id, string Version), (string Url, string Licence)> ReviewedUrls = new()
    {
        [("xunit.abstractions", "2.0.3")] = ("https://raw.githubusercontent.com/xunit/xunit/master/license.txt", "Apache-2.0"),
    };

    private static readonly string[] NetworkMarkers =
    [
        "NU1301",
        "Unable to load the service index",
        "No such host is known",
        "Name or service not known",
        "nodename nor servname provided",
        "The remote name could not be resolved",
    ];

    private static string Msg(string id, string version, string what) =>
        $"restored-package-licences: {id} {version}: {what}";

    private static string? ClassifyLicenceText(string? text)
    {
        var raw = text ?? string.Empty;
        var head = Regex.Replace(raw[..Math.Min(600, raw.Length)], @"\s+", " ").Trim();
        if (Regex.IsMatch(head, @"\bMIT License\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            return "MIT";
        if (Regex.IsMatch(head, @"\bApache License\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
            && Regex.IsMatch(head, @"\bVersion 2\.0\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            return "Apache-2.0";
        var bsd = Regex.Match(head, @"\bBSD ([23])-Clause\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return bsd.Success ? $"BSD-{bsd.Groups[1].Value}-Clause" : null;
    }

    /// <summary>[E] Licence value failures for the page rows (cells: id, version, kind, licence, licence source, where used, review). Empty when every licence value is right.</summary>
    public static IReadOnlyList<string> LicenceValueFailures(
        IReadOnlyList<string[]> rows,
        IReadOnlyDictionary<string, PackageEvidence> evidence)
    {
        var failures = new List<string>();
        foreach (var row in rows)
        {
            if (row.Length < 7)
                continue;

            var id = row[0];
            var version = row[1];
            var licence = row[3];
            var source = row[4];

            if (!evidence.TryGetValue(id + " " + version, out var packageEvidence))
            {
                failures.Add(Msg(id, version, "no restored package to read the licence from"));
                continue;
            }

            if (source == "nuspec licence expression")
            {
                if (!string.Equals(packageEvidence.Expression, licence, StringComparison.Ordinal))
                    failures.Add(Msg(id, version, $"licence {licence} but the nuspec expression is {packageEvidence.Expression}"));
                continue;
            }

            if (source == "packaged licence file")
            {
                var found = packageEvidence.LicenceFiles.Values
                    .Select(ClassifyLicenceText)
                    .Where(value => !string.IsNullOrEmpty(value))
                    .Select(value => value!)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList();
                if (found.Count == 0)
                    failures.Add(Msg(id, version, $"licence {licence} but no packaged licence file names a known licence"));
                else if (!found.Contains(licence, StringComparer.Ordinal))
                    failures.Add(Msg(id, version, $"licence {licence} but the packaged licence file says {string.Join(" and ", found)}"));
                continue;
            }

            if (source == "licence URL")
            {
                var url = packageEvidence.LicenceUrl?.Trim() ?? string.Empty;
                if (!ReviewedUrls.TryGetValue((id, version), out var reviewed) || !string.Equals(reviewed.Url, url, StringComparison.Ordinal))
                {
                    failures.Add(Msg(id, version, $"licence {licence} from licence URL {(url.Length == 0 ? "-" : url)}, which is not a reviewed URL"));
                }
                else if (!string.Equals(reviewed.Licence, licence, StringComparison.Ordinal))
                {
                    failures.Add(Msg(id, version, $"licence {licence} but the reviewed licence URL says {reviewed.Licence}"));
                }
            }
        }

        return failures;
    }

    /// <summary>[O] pass, mismatch, network-missing or fail, from a locked restore's exit code, time-out, text and restored package count.</summary>
    public static string RestoreOutcome(int exitCode, bool timedOut, string text, int restoredCount)
    {
        if (timedOut)
            return "fail";
        if (exitCode == 0)
            return "pass";
        if (Regex.IsMatch(text ?? string.Empty, @"\bNU1004\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            return "mismatch";

        var output = text ?? string.Empty;
        if (NetworkMarkers.Any(marker => output.Contains(marker, StringComparison.OrdinalIgnoreCase)) && restoredCount == 0)
            return "network-missing";

        if (exitCode == 0)
            return "pass";
        return "fail";
    }

    /// <summary>One restored package as the networked F6 reads it: the folder, the nuspec state (missing, broken or ok), its evidence when the nuspec parses, and whether it asks for licence acceptance.</summary>
    public static RestoredPackage RestoredPackageFrom(string id, string version, bool folderFound, string? nuspecXml, IReadOnlyList<string> relativePaths, Func<string, string> readHead)
    {
        if (nuspecXml is null)
            return new RestoredPackage(id, version, folderFound, "missing", null, false);

        XDocument doc;
        try
        {
            doc = XDocument.Parse(nuspecXml);
        }
        catch (XmlException)
        {
            return new RestoredPackage(id, version, folderFound, "broken", null, false);
        }

        var license = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "license");
        var expression = license is not null && string.Equals((string?)license.Attribute("type"), "expression", StringComparison.OrdinalIgnoreCase)
            ? license.Value.Trim()
            : null;
        var url = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "licenseUrl")?.Value.Trim();
        var acceptance = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "requireLicenseAcceptance");
        var requiresAcceptance = acceptance is not null && string.Equals(acceptance.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase);

        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in relativePaths)
        {
            var name = path.Split('/', '\\')[^1].ToLowerInvariant();
            if (name.StartsWith("license", StringComparison.Ordinal)
                || name.StartsWith("licence", StringComparison.Ordinal)
                || name.StartsWith("copying", StringComparison.Ordinal))
                files[path] = readHead(path);
        }

        return new RestoredPackage(id, version, folderFound, "ok", new PackageEvidence(expression, url, files), requiresAcceptance);
    }

    /// <summary>[E] The failures of the networked F6 for the page rows and the restored packages, other than the acceptance set. Checks that each page row and restored package match as a set, and passes each checked row and its evidence to LicenceValueFailures, so a wrong or blank licence or source label fails too.</summary>
    public static IReadOnlyList<string> F6Failures(IReadOnlyList<string[]> pageRows, IReadOnlyList<RestoredPackage> restored)
    {
        var failures = new List<string>();
        var rows = pageRows.Where(row => row.Length >= 7).ToList();
        var pageSet = rows.Select(row => (Id: row[0], Version: row[1])).ToHashSet();
        var restoredSet = restored.Select(p => (p.Id, p.Version)).ToHashSet();
        if (!restoredSet.SetEquals(pageSet))
            failures.Add(DescribeSet("locked restore vs page", pageSet, restoredSet));

        var byKey = restored.GroupBy(p => (p.Id, p.Version)).ToDictionary(g => g.Key, g => g.First());
        var checkedRows = new List<string[]>();
        var evidence = new Dictionary<string, PackageEvidence>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            if (!byKey.TryGetValue((row[0], row[1]), out var package))
                continue;

            var id = row[0];
            var version = row[1];
            var licence = row[3];
            var source = row[4];
            if (!package.FolderFound)
            {
                failures.Add(Msg(id, version, "package folder missing after restore"));
                continue;
            }

            if (package.NuspecState != "ok")
            {
                failures.Add(Msg(id, version, package.NuspecState == "missing" ? "nuspec missing after restore" : "nuspec does not parse"));
                continue;
            }

            var packageEvidence = package.Evidence!;
            if (source == "nuspec licence expression")
            {
                if (!string.Equals(packageEvidence.Expression, licence, StringComparison.Ordinal))
                    failures.Add(Msg(id, version, $"licence {licence} but the nuspec expression is {packageEvidence.Expression}"));
            }
            else if (source == "packaged licence file")
            {
                if (packageEvidence.LicenceFiles.Count == 0)
                    failures.Add(Msg(id, version, $"licence {licence} but no packaged licence file names a known licence"));
            }
            else if (source == "licence URL")
            {
                if (string.IsNullOrEmpty(packageEvidence.LicenceUrl))
                    failures.Add(Msg(id, version, $"licence {licence} from licence URL -, which is not a reviewed URL"));
            }
            else
            {
                failures.Add(Msg(id, version, "licence source is not one of the three labels"));
                continue;
            }

            checkedRows.Add(row);
            evidence[id + " " + version] = packageEvidence;
        }

        failures.AddRange(LicenceValueFailures(checkedRows, evidence));
        return failures;
    }

    /// <summary>The acceptance set of the networked F6: the packages whose nuspec asks for acceptance equal the packages the page's acceptance lines name.</summary>
    public static IReadOnlyList<string> F6AcceptanceFailures(string restoredBody, IReadOnlyList<string[]> pageRows, IReadOnlyList<RestoredPackage> restored)
    {
        var byKey = restored.GroupBy(p => (p.Id, p.Version)).ToDictionary(g => g.Key, g => g.First());
        var want = pageRows
            .Where(row => row.Length >= 7 && byKey.TryGetValue((row[0], row[1]), out var package) && package.RequiresAcceptance)
            .Select(row => (Id: row[0], Version: row[1]))
            .ToHashSet();
        var named = new HashSet<(string Id, string Version)>();
        foreach (var line in restoredBody.Split('\n'))
        {
            if (line.IndexOf("acceptance", StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            foreach (var package in restored)
            {
                if (line.Contains(package.Id, StringComparison.Ordinal) && line.Contains(package.Version, StringComparison.Ordinal))
                    named.Add((package.Id, package.Version));
            }
        }

        return named.SetEquals(want) ? [] : [DescribeSet("acceptance set", want, named)];
    }

    /// <summary>The package libraries (type package) in a project.assets.json text; 0 for no text. Malformed JSON throws, which fails closed.</summary>
    public static int RestoredCount(string? assetsJson)
    {
        if (assetsJson is null)
            return 0;
        using var doc = JsonDocument.Parse(assetsJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Object
            || !doc.RootElement.TryGetProperty("libraries", out var libraries)
            || libraries.ValueKind != JsonValueKind.Object)
            return 0;
        return libraries.EnumerateObject().Count(lib =>
            lib.Value.ValueKind == JsonValueKind.Object
            && lib.Value.TryGetProperty("type", out var type)
            && type.ValueKind == JsonValueKind.String
            && type.GetString() == "package");
    }

    /// <summary>The outcome F5's planted step and RestoreLockedOrFail act on: fail, network-missing, pass or mismatch. Returns RestoreOutcome for the same arguments; it decides nothing itself.</summary>
    public static string ClassifyRestore(int exitCode, bool timedOut, string text, int restoredCount)
        => RestoreOutcome(exitCode, timedOut, text, restoredCount);

    public static string DescribeSet(string label, HashSet<(string Id, string Version)> expected, HashSet<(string Id, string Version)> actual)
    {
        var missing = string.Join(", ", expected.Except(actual).OrderBy(p => p.Id, StringComparer.Ordinal).ThenBy(p => p.Version, StringComparer.Ordinal).Select(p => p.Id + " " + p.Version));
        var extra = string.Join(", ", actual.Except(expected).OrderBy(p => p.Id, StringComparer.Ordinal).ThenBy(p => p.Version, StringComparer.Ordinal).Select(p => p.Id + " " + p.Version));
        return label + " missing: " + missing + "; extra: " + extra;
    }
}
