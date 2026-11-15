using System.Text.RegularExpressions;

namespace Avowry.Acceptance.Tests;

// Card 422 helper ports from the reference checks (reference-refcheck.py.txt):
// classify_licence_text, licence_value_failures and restore_outcome.

/// <summary>The nuspec facts of one restored package: its licence expression, its licenseUrl, and the head text of each packaged licence file.</summary>
internal sealed record PackageEvidence(string? Expression, string? LicenceUrl, IReadOnlyDictionary<string, string> LicenceFiles);

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
}
