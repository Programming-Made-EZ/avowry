namespace Avowry.Acceptance.Tests;

// CARD 422 RED STUB. The green replaces the bodies of this file (and may move the two helpers into
// RestoredPackageLicencesLockedRestore.cs, deleting this file). Until then every row that calls them fails
// with NotImplementedException, which is the Pending state. The reference is refcheck.py in the card 422 probe:
// licence_value_failures(table, evidence) and restore_outcome(exit_code, timed_out, text, restored_count).

/// <summary>The nuspec facts of one restored package: its licence expression, its licenseUrl, and the head text of each packaged licence file.</summary>
internal sealed record PackageEvidence(string? Expression, string? LicenceUrl, IReadOnlyDictionary<string, string> LicenceFiles);

internal static class RestoredLicenceChecks
{
    /// <summary>[E] Licence value failures for the page rows (cells: id, version, kind, licence, licence source, where used, review). Empty when every licence value is right.</summary>
    public static IReadOnlyList<string> LicenceValueFailures(
        IReadOnlyList<string[]> rows,
        IReadOnlyDictionary<string, PackageEvidence> evidence) =>
        throw new NotImplementedException("card 422 green: port licence_value_failures from the reference");

    /// <summary>[O] pass, mismatch, network-missing or fail, from a locked restore's exit code, time-out, text and restored package count.</summary>
    public static string RestoreOutcome(int exitCode, bool timedOut, string text, int restoredCount) =>
        throw new NotImplementedException("card 422 green: port restore_outcome from the reference");
}
