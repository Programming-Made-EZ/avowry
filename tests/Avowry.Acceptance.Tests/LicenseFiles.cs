using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The sponsor persona sees who holds the Avowry name
//   As the sponsor persona, I want NOTICE and TRADEMARKS.md to name the holder and the year,
//   so that I can see who owns the Avowry name before I adopt it.
public class LicenseFiles
{
    private const string Module = "tests.test_license_files.LicenseFilesTests.";

    [Fact]
    [Trait("Slice", "366")]
    [Trait("Criterion", "366-1")]
    public async Task Notice_and_trademarks_name_the_holder_and_the_year()
    {
        // Given the owner's answer: holder "Programming Made EZ", year 2026
        // When the sponsor persona opens NOTICE and TRADEMARKS.md
        // Then NOTICE line 2 and TRADEMARKS.md line 5 name them, and NOTICE has one copyright line
        await AssertPythonTestsPass("test_notice_and_trademarks_name_the_holder_and_the_year");
    }

    [Fact]
    [Trait("Slice", "366")]
    [Trait("Criterion", "366-2")]
    public async Task Notice_and_trademarks_hold_no_placeholder_and_the_scanner_catches_one()
    {
        // Given NOTICE and TRADEMARKS.md
        // When the placeholder scanner reads them
        // Then it finds nothing, while a planted placeholder is reported and a markdown link is not
        await AssertPythonTestsPass(
            "test_notice_and_trademarks_hold_no_placeholder",
            "test_the_placeholder_scanner_reports_planted_placeholders",
            "test_the_placeholder_scanner_ignores_links_and_the_filled_holder");
    }

    [Fact]
    [Trait("Slice", "366")]
    [Trait("Criterion", "366-3")]
    public async Task The_licensing_page_names_the_holder_and_the_year_assumption()
    {
        // Given docs/LICENSING.md
        // When the holder is filled
        // Then the page names "Programming Made EZ", says 2026 is an assumption, and no longer calls the holder a placeholder
        await AssertPythonTestsPass("test_the_licensing_page_names_the_holder_and_the_year_assumption");
    }

    [Fact]
    [Trait("Slice", "366")]
    public async Task The_license_and_standing_terms_are_unchanged()
    {
        // Given LICENSE and the standing terms in README.md, TRADEMARKS.md and docs/LICENSING.md
        // When the holder is filled
        // Then LICENSE still hashes to the Apache-2.0 text and the standing-terms checks still pass
        await AssertPythonTestsPass(
            "test_license_is_the_standard_apache_2_0_text_byte_for_byte",
            "test_reader_facing_legal_files_use_the_avowry_name",
            "test_the_readme_and_the_licensing_page_say_generated_code_belongs_to_the_user",
            "test_the_trademark_policy_covers_redistribution_references_forks_and_the_rationale",
            "test_the_licensing_page_states_the_apache_licence_and_links_the_trademark_policy");
    }

    private static async Task AssertPythonTestsPass(params string[] testNames)
    {
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        foreach (var name in testNames)
        {
            start.ArgumentList.Add(Module + name);
        }

        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(120000));

        Assert.True(result.ExitCode == 0 && !result.TimedOut, result.Stdout + result.Stderr);
    }
}
