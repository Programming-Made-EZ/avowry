using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: Dana reads every package the generated service and specs restore, with its licence
//   As Dana, I want every restored package listed with version, licence, source and where-used,
//   so that before I adopt the preview I can see whether anything it pulls needs a licence review.
[Collection("Bench processes")] // the two classes that start the bench tools under a 20 second limit never overlap (card 368: the extra launches made the timing test flaky under the full run)
public class RestoredPackageLicences
{
    private const string Module = "tests.test_restored_package_licences.RestoredLicencesOffline.";

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-1")]
    [Trait("Status", "Pending")] // green 2 (the page) removes this Pending trait
    public async Task F1_lockfiles_parse_and_every_package_is_listed()
    {
        // Given committed lockfiles for the reference API and specs projects
        // When Dana reads the restored-package licences on the pre-publication page
        // Then each lockfile package appears once with version, kind, licence, source and where-used
        await AssertPythonTestsPass(
            "test_L1_both_lockfiles_exist_parse_and_have_net8_0",
            "test_L2_union_of_lockfile_packages_matches_page_rows",
            "test_L3_direct_kind_and_where_used",
            "test_L4_every_row_has_licence_source_and_where_used",
            "test_L5_section_names_lockfiles_restore_locked_mode_and_commit",
            "test_L6_reference_props_enable_lockfile_restore");
    }

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-2")]
    [Trait("Status", "Pending")] // green 2 (the page) removes this Pending trait
    public async Task F2_empty_and_borderline_cases_are_stated()
    {
        // Given the Avowry tool projects and the packages with no licence expression or licence acceptance
        // When Dana reads the page
        // Then the tool restores no packages, the borderline packages are named with a recommendation, and Package licences no longer says not reviewed
        await AssertPythonTestsPass(
            "test_Z1_the_avowry_tool_restores_no_packages",
            "test_B1_no_licence_expression_packages_are_named",
            "test_B2_licence_acceptance_names_microsoft_codecoverage",
            "test_B3_outside_allowed_list_or_the_none_line",
            "test_B4_one_recommendation_and_package_licences_points_here");
    }

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-3")]
    [Trait("Status", "Pending")] // green 1 (lockfiles and props) removes this Pending trait
    public async Task F3_lockfiles_match_the_provider_and_the_tool_gains_no_package()
    {
        // Given the lockfiles, the C# provider and the three tool projects
        // When Direct entries are compared with the provider and a planted PackageReference is read
        // Then the Direct sets match, a planted PackageReference is reported, and the lockfiles and props exist
        await AssertPythonTestsPass(
            "test_E4_lockfile_direct_set_equals_the_provider",
            "test_E5_planted_tool_packagereference_is_reported",
            "test_L1_both_lockfiles_exist_parse_and_have_net8_0",
            "test_L6_reference_props_enable_lockfile_restore");
    }

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-3")]
    [Trait("Status", "Pending")] // green 2 (the page) removes this Pending trait
    public async Task F4_a_stale_page_row_is_reported()
    {
        // Given the page and the two committed lockfiles
        // When a lockfile package is missing from the page, listed at another version, or a page row is in neither lockfile
        // Then each drift is reported by id and version
        await AssertPythonTestsPass(
            "test_E1_E2_E3_planted_drift_is_reported",
            "test_L2_union_of_lockfile_packages_matches_page_rows");
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
            start.ArgumentList.Add(Module + name);

        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(120000));

        Assert.True(result.ExitCode == 0 && !result.TimedOut, result.Stdout + result.Stderr);
    }
}
