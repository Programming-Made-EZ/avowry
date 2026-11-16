using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The four model vendors' published terms are read for publishing model output
//   As Dana, I want a cited summary per vendor and flagged blockers,
//   so that I can judge whether the bench reply prefixes and seat replies can be public.
public class VendorTermsRead
{
    private const string Module = "tests.test_vendor_terms.VendorTermsTests.";

    [Fact]
    [Trait("Slice", "367")]
    [Trait("Criterion", "367-1")]
    public async Task Every_vendor_has_a_cited_dated_row_naming_both_outputs()
    {
        // Given the four vendors the mob uses
        // When a seat reads each vendor's current published terms
        // Then a page cites each URL with the date read and summarises publishing model output, naming reply_head and .mob/records
        await AssertPythonTestsPass(
            "test_every_vendor_has_a_cited_dated_row_naming_both_outputs",
            "test_the_vendor_terms_check_reports_a_planted_row_without_url_or_date",
            "test_the_vendor_terms_check_reports_a_page_missing_the_not_legal_advice_line",
            "test_the_vendor_terms_check_reports_a_page_missing_the_how_this_was_read_section");
    }

    [Fact]
    [Trait("Slice", "367")]
    [Trait("Criterion", "367-2")]
    public async Task Blockers_agree_with_the_blocks_publication_column()
    {
        // Given the summaries
        // When any term would block publication
        // Then the page names it with the quoted URL and a recommendation; when none does, it says so
        await AssertPythonTestsPass(
            "test_blockers_agree_with_the_blocks_publication_column",
            "test_the_vendor_terms_check_reports_a_planted_yes_row_missing_from_blockers",
            "test_the_vendor_terms_check_pins_empty_page_and_invalid_blocks_publication_per_criterion");
    }

    [Fact]
    [Trait("Slice", "367")]
    [Trait("Criterion", "367-3")]
    public async Task Pre_publication_status_matches_each_vendor_read_on_date()
    {
        // Given the four vendors listed in the pre-publication page
        // When the page is checked
        // Then each pre-publication terms status reads read <date> (vendor-terms.md) matching that vendor's read on date
        await AssertPythonTestsPass(
            "test_pre_publication_status_matches_each_vendor_read_on_date",
            "test_the_vendor_terms_check_reports_a_vendor_left_not_reviewed");
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
