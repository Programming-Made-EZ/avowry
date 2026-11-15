using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;
using Xunit.Abstractions;

namespace Avowry.Acceptance.Tests;

// Feature: Dana reads every package the generated service and specs restore, with its licence
//   As Dana, I want every restored package listed with version, licence, source and where-used,
//   so that before I adopt the preview I can see whether anything it pulls needs a licence review.
[Collection("Bench processes")] // D247: a test class that starts an external process (here python or dotnet restore) under a hard time limit never overlaps another such class
public class RestoredPackageLicences
{
    private const string Module = "tests.test_restored_package_licences.RestoredLicencesOffline.";

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-1")]
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
    public async Task F4_a_stale_page_row_is_reported()
    {
        // Given the page and the two committed lockfiles
        // When a lockfile package is missing from the page, listed at another version, or a page row is in neither lockfile
        // Then each drift is reported by id and version
        await AssertPythonTestsPass(
            "test_E1_E2_E3_planted_drift_is_reported",
            "test_L2_union_of_lockfile_packages_matches_page_rows");
    }


    private readonly ITestOutputHelper output;

    public RestoredPackageLicences(ITestOutputHelper output) => this.output = output;

    // Ruling 4: the Python output is written to the test log, so a passing run shows the shallow-checkout skip note.
    private async Task AssertPython422(string className, params string[] testNames)
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
            start.ArgumentList.Add("tests.test_restored_package_licences." + className + "." + name);

        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(120000));

        output.WriteLine(result.Stdout + result.Stderr);
        Assert.True(result.ExitCode == 0 && !result.TimedOut, result.Stdout + result.Stderr);
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-1")]
    public async Task F7_422_1_rows_that_hold_today()
    {
        // 422-1: [Z] the committed page, tool projects and props pass
        await AssertPython422("Test422Characterisation",
            "test_422_Z_tools",
            "test_422_Z_props",
            "test_422_every_corpus_row_is_embedded");
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-1")]
    [Trait("Status", "Pending")]
    public async Task F8_422_1_pending_rows_turn_green()
    {
        // 422-1: [Z] the committed page, tool projects and props pass
        await AssertPython422("Test422Pending",
            "test_422_Z_page");
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-2")]
    [Trait("Status", "Pending")]
    public async Task F9_422_2_pending_rows_turn_green()
    {
        // 422-2: [E] the offline half: one-way and swapped licence changes against the figures
        await AssertPython422("Test422Pending",
            "test_422_E_xa_mit_off",
            "test_422_E_swap_off");
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-3")]
    public async Task F10_422_3_rows_that_hold_today()
    {
        // 422-3: [M][B] page rows (indented rows, wrapped rows) and the prose figures
        await AssertPython422("Test422Characterisation",
            "test_422_L_gpl",
            "test_422_L_bold",
            "test_422_L_code",
            "test_422_L_inner_ws",
            "test_422_L_source_label",
            "test_422_L_indent4",
            "test_422_L_row_wrapped",
            "test_422_L_E1",
            "test_422_L_E2",
            "test_422_L_E3",
            "test_422_L_kind",
            "test_422_L_crlf");
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-3")]
    [Trait("Status", "Pending")]
    public async Task F11_422_3_pending_rows_turn_green()
    {
        // 422-3: [M][B] page rows (indented rows, wrapped rows) and the prose figures
        await AssertPython422("Test422Pending",
            "test_422_L_gpl_flagged",
            "test_422_L_indent_dup",
            "test_422_L_indent3_dup",
            "test_422_M_add",
            "test_422_B_total",
            "test_422_B_api",
            "test_422_B_specs",
            "test_422_B_direct",
            "test_422_B_trans",
            "test_422_B_mit",
            "test_422_B_apache",
            "test_422_B_notice",
            "test_422_B_noexpr",
            "test_422_B_withexpr",
            "test_422_B_acc",
            "test_422_B_acc_word",
            "test_422_B_group",
            "test_422_B_rec_total",
            "test_422_B_rec_mit",
            "test_422_B_rec_apache",
            "test_422_B_acc_named",
            "test_422_W_words",
            "test_422_W_words_hyphen",
            "test_422_W_words_wrong",
            "test_422_W_bold",
            "test_422_W_bold_wrong",
            "test_422_W_code",
            "test_422_W_code_wrong",
            "test_422_W_wrapped",
            "test_422_W_wrapped_wrong",
            "test_422_W_inner",
            "test_422_W_leading",
            "test_422_W_gone",
            "test_422_W_dup_wrong",
            "test_422_W_notice_unknown",
            "test_422_W_url");
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-4")]
    [Trait("Status", "Pending")]
    public async Task F12_422_4_pending_rows_turn_green()
    {
        // 422-4: [I] tool PackageReference forms and the files the tool reads
        await AssertPython422("Test422Pending",
            "test_422_I_E5",
            "test_422_I_order",
            "test_422_I_child",
            "test_422_I_child_cdata",
            "test_422_I_entity",
            "test_422_I_single",
            "test_422_I_update",
            "test_422_I_update_child",
            "test_422_I_list",
            "test_422_I_lower",
            "test_422_I_multiline",
            "test_422_I_inner_ws",
            "test_422_I_value_ws",
            "test_422_I_tab",
            "test_422_I_comment",
            "test_422_I_cdata_text",
            "test_422_I_remove",
            "test_422_I_noversion",
            "test_422_I_override",
            "test_422_I_cond_false",
            "test_422_I_download",
            "test_422_I_root_props",
            "test_422_I_root_targets",
            "test_422_I_src_props",
            "test_422_I_global",
            "test_422_I_pkgversion",
            "test_422_I_reference_props",
            "test_422_I_projref_single",
            "test_422_I_projref_backslash",
            "test_422_I_projref_order",
            "test_422_I_projref_plain",
            "test_422_I_import",
            "test_422_I_import_thisdir",
            "test_422_I_import_unknown",
            "test_422_I_import_sdk",
            "test_422_I_broken",
            "test_422_I_framework");
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-5")]
    public async Task F13_422_5_rows_that_hold_today()
    {
        // 422-5: [S] the two lockfile properties under Conditions
        await AssertPython422("Test422Characterisation",
            "test_422_S_true",
            "test_422_S_true_compare",
            "test_422_S_true_wrapped",
            "test_422_S_empty_cond",
            "test_422_S_plain",
            "test_422_S_both",
            "test_422_S_override_false",
            "test_422_S_import_before",
            "test_422_S_import_sdk",
            "test_422_S_case",
            "test_422_S_value_ws",
            "test_422_S_value_prop",
            "test_422_S_cdata",
            "test_422_S_entity",
            "test_422_S_broken");
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-5")]
    [Trait("Status", "Pending")]
    public async Task F14_422_5_pending_rows_turn_green()
    {
        // 422-5: [S] the two lockfile properties under Conditions
        await AssertPython422("Test422Pending",
            "test_422_S_false",
            "test_422_S_false_compare",
            "test_422_S_false_ne",
            "test_422_S_false_single",
            "test_422_S_unknown",
            "test_422_S_unknown_and",
            "test_422_S_elem_false",
            "test_422_S_override",
            "test_422_S_override_unknown",
            "test_422_S_target",
            "test_422_S_choose",
            "test_422_S_import_after",
            "test_422_S_comment",
            "test_422_S_cond_entity",
            "test_422_S_missing");
    }

    [Fact]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-6")]
    [Trait("Status", "Pending")]
    public async Task F15_422_6_pending_rows_turn_green()
    {
        // 422-6: [O] the shallow-checkout note
        await AssertPython422("Test422Pending",
            "test_422_O_shallow",
            "test_422_O_deep",
            "test_422_O_shallow_nohash");
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
