using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The sponsor persona reads the owner items as answered, with a trademark search brief
//   As the sponsor persona, I want the pre-publication page to show each owner item as answered,
//   with the trademark search brief, so that I see what is settled and what still waits.
public class PrePublicationOwnerItems
{
    private const string Module = "tests.test_pre_publication_owner_items.PrePublicationOwnerItemsTests.";

    [Fact]
    [Trait("Slice", "371")]
    [Trait("Criterion", "371-1")]
    public async Task Each_of_the_nine_owner_items_shows_its_answer()
    {
        // Given the owner's answers of 2026-11-08
        // When the sponsor persona opens the Owner items section
        // Then exactly nine items each read "answered 2026-11-08" with their answer, and a missing answer is reported by its item
        await AssertPythonTestsPass(
            "test_each_owner_item_shows_its_answer",
            "test_the_owner_items_check_reports_a_missing_answer");
    }

    [Fact]
    [Trait("Slice", "371")]
    [Trait("Criterion", "371-2")]
    public async Task No_page_line_still_calls_an_answered_item_open()
    {
        // Given the answered page
        // When the sponsor persona reads any section
        // Then no line says OPEN or repeats a stale clause, while a planted stale phrase is reported
        await AssertPythonTestsPass(
            "test_no_page_line_still_calls_an_answered_item_open",
            "test_the_stale_check_reports_a_planted_phrase");
    }

    [Fact]
    [Trait("Slice", "371")]
    [Trait("Criterion", "371-3")]
    public async Task The_page_carries_the_trademark_search_brief()
    {
        // Given the commissioned search
        // When the owner hands the brief to a provider
        // Then the brief names the mark Avowry, classes 9 and 42 only, the jurisdictions, every registry result and what was not checked
        await AssertPythonTestsPass("test_the_page_carries_the_trademark_search_brief");
    }

    [Fact]
    [Trait("Slice", "371")]
    public async Task The_owner_items_stay_nine_and_the_stale_check_ignores_the_answered_page()
    {
        // Given the page before and after the answers are written
        // When the owner items are counted and the stale check reads a fully answered page
        // Then there are nine items and the stale check reports nothing
        await AssertPythonTestsPass(
            "test_the_page_has_nine_owner_items",
            "test_the_stale_check_ignores_the_answered_page");
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
