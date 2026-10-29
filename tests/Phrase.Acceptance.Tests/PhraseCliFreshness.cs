using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Test runs reject a stale Phrase CLI
//   As Morgan, I want a test run against an out-of-date CLI to stop with a clear message,
//   so that no gate result rests on a binary that does not contain the change under test.
public class PhraseCliFreshness
{
    [Fact]
    [Trait("Slice", "173")]
    public void EnsureFresh_fails_with_the_rebuild_message_only_when_a_source_is_newer_than_the_dll_beyond_the_tolerance()
    {
        // Given a CLI timestamp and injected source timestamps around the two-second tolerance
        var dllTime = new DateTime(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);
        var timestamps = new Dictionary<string, DateTime>(StringComparer.Ordinal)
        {
            ["phrase.dll"] = dllTime,
            ["newer.cs"] = dllTime.AddSeconds(3),
            ["boundary.cs"] = dllTime.AddSeconds(2),
            ["nearer.csproj"] = dllTime.AddSeconds(1),
            ["equal.cs"] = dllTime,
            ["older.cs"] = dllTime.AddSeconds(-1),
        };
        DateTime LastWriteUtc(string path) => timestamps[path];

        // When freshness is checked with newer, boundary, older, empty, and several sources
        var stale = Assert.Throws<InvalidOperationException>(() =>
            PhraseCli.EnsureFresh("phrase.dll", ["newer.cs"], LastWriteUtc, TimeSpan.FromSeconds(2)));
        var boundary = Record.Exception(() =>
            PhraseCli.EnsureFresh("phrase.dll", ["boundary.cs", "nearer.csproj"], LastWriteUtc, TimeSpan.FromSeconds(2)));
        var olderOrEqual = Record.Exception(() =>
            PhraseCli.EnsureFresh("phrase.dll", ["equal.cs", "older.cs"], LastWriteUtc, TimeSpan.FromSeconds(2)));
        var noSources = Record.Exception(() =>
            PhraseCli.EnsureFresh("phrase.dll", [], LastWriteUtc, TimeSpan.FromSeconds(2)));
        var newestDecides = Assert.Throws<InvalidOperationException>(() =>
            PhraseCli.EnsureFresh("phrase.dll", ["older.cs", "newer.cs", "equal.cs"], LastWriteUtc, TimeSpan.FromSeconds(2)));

        // Then only a source beyond the tolerance fails, with the exact rebuild instruction
        Assert.Equal("phrase CLI is stale, run dotnet build src/Phrase.Cli", stale.Message);
        Assert.Null(boundary);
        Assert.Null(olderOrEqual);
        Assert.Null(noSources);
        Assert.Equal("phrase CLI is stale, run dotnet build src/Phrase.Cli", newestDecides.Message);
    }

    [Fact]
    [Trait("Slice", "173")]
    public void EnsureFresh_passes_on_the_real_tree_after_a_build()
    {
        // Given the CLI and its source projects have been built in repository gate order

        // When the production freshness wiring checks the real repository tree
        var exception = Record.Exception(PhraseCli.EnsureFresh);

        // Then the built CLI contains every current source change
        Assert.Null(exception);
    }
}
