using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: a locked restore of the reference projects matches the committed lockfiles and the page
//   As Dana, I want the listed licences to match a locked restore, so that a stale lockfile or page cannot pass.
[Collection("Bench processes")] // D247: a test class that starts an external process (here python or dotnet restore) under a hard time limit never overlaps another such class
public class RestoredPackageLicencesLockedRestore
{
    private static readonly TimeSpan RestoreLimit = TimeSpan.FromMinutes(5);
    private const string ApiLockRel = "reference/csharp/Orders.Api/packages.lock.json";
    private const string SpecsLockRel = "reference/csharp/Orders.Specs/packages.lock.json";

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-3")]
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
        Assert.True(restored.SetEquals(locked), RestoredLicenceChecks.DescribeSet("locked restore vs lockfiles", locked, restored));

        // And a planted lockfile mismatch restores with a non-zero exit and NU1004
        using var drift = TempWorkspace.Create();
        drift.CopyFrom(RepoPaths.Find("reference", "csharp"));
        var plantedLock = Path.Combine(drift.Root, "Orders.Api", "packages.lock.json");
        PlantLockfileMismatch(plantedLock);
        var plantedProject = Path.Combine(drift.Root, "Orders.Api", "Orders.Api.csproj");
        var planted = await RestoreLockedAsync(plantedProject);
        var plantedText = PublicText(planted.Stdout + planted.Stderr);
        var plantedOutcome = RestoredLicenceChecks.ClassifyRestore(planted.ExitCode, planted.TimedOut, plantedText, RestoredLicenceChecks.RestoredCount(ReadAssets(plantedProject)));
        if (plantedOutcome == "network-missing")
            Assert.Fail("NETWORK MISSING " + FirstLine(plantedText));
        Assert.True(plantedOutcome == "mismatch", "planted restore: " + plantedOutcome + ": " + plantedText);
    }

    [Fact]
    [Trait("Slice", "369")]
    [Trait("Criterion", "369-1")]
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
        var restored = ReadRestoredPackages(assets.RootElement);
        var pageText = (await File.ReadAllTextAsync(RepoPaths.Find("docs", "release", "pre-publication-checks.md")))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        var pageRows = ReadRestoredRows(pageText).Select(r => new[] { r.Id, r.Version, r.Kind, r.Licence, r.LicenceSource, r.WhereUsed, r.Review }).ToList();

        // Then the restore set equals the page rows, and nuspec licences, licence files and the acceptance set match
        var failures = RestoredLicenceChecks.F6Failures(pageRows, restored)
            .Concat(RestoredLicenceChecks.F6AcceptanceFailures(RestoredBody(pageText), pageRows, restored))
            .ToList();
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    }

    // ---- Card 422 red (task-force role: red) --------------------------------------------------------------------
    // Offline rows for [E] (the licence VALUES) and [O] (the locked-restore outcome). They read the real page and fixture
    // evidence, never the network. They call two pure helpers that do not exist yet: RestoredLicenceChecks.LicenceValueFailures
    // and RestoredLicenceChecks.RestoreOutcome, stubbed in RestoredPackageLicences422Stubs.cs (they throw NotImplementedException
    // until the green replaces them). Every row below is Pending for that reason.

    private sealed record LicenceRow422((string Old, string New)[] Edits, (string Id, string Version, string Field, string Value)[] Evidence, string Expected);

    private sealed record RestoreRow422(string Kind, int ExitCode, int RestoredCount, bool TimedOut, string Output, string Expected);

    // The nuspec facts of the 30 restored packages (expression, licenseUrl, licence files), read once from the local package folder.
    private static Dictionary<string, PackageEvidence> Evidence422() => new()
    {
            ["FastEndpoints 8.3.0"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["FastEndpoints.Attributes 8.3.0"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["FastEndpoints.Core 8.3.0"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["FastEndpoints.JobQueues 8.3.0"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["FastEndpoints.Messaging 8.3.0"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["FastEndpoints.Messaging.Core 8.3.0"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["FluentValidation 12.1.1"] = new("Apache-2.0", "https://licenses.nuget.org/Apache-2.0", new Dictionary<string, string> {  }),
            ["Microsoft.CodeCoverage 17.11.1"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.Extensions.Configuration.Abstractions 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.Extensions.DependencyInjection.Abstractions 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.Extensions.Diagnostics.Abstractions 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.Extensions.FileProviders.Abstractions 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.Extensions.Hosting.Abstractions 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.Extensions.Logging.Abstractions 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.Extensions.Options 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.Extensions.Primitives 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.NET.Test.Sdk 17.11.1"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.TestPlatform.ObjectModel 17.11.1"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Microsoft.TestPlatform.TestHost 17.11.1"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["Newtonsoft.Json 13.0.1"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> { ["LICENSE.md"] = "The MIT License (MIT)\n\nCopyright (c) 2007 James Newton-King\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of\nthis software and associated documentation files (the \"Software\"), to deal in\nthe Software without restriction, including without limitation the rights to\nuse, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of\nthe Software, and to permit persons to whom the Software is furnished to do so,\nsubject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all\ncopies or substantial port" }),
            ["System.Diagnostics.DiagnosticSource 10.0.11"] = new("MIT", "https://licenses.nuget.org/MIT", new Dictionary<string, string> {  }),
            ["System.Reflection.Metadata 1.6.0"] = new(null, "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT", new Dictionary<string, string> { ["LICENSE.TXT"] = "The MIT License (MIT)\n\nCopyright (c) .NET Foundation and Contributors\n\nAll rights reserved.\n\nPermission is hereby granted, free of charge, to any person obtaining a copy\nof this software and associated documentation files (the \"Software\"), to deal\nin the Software without restriction, including without limitation the rights\nto use, copy, modify, merge, publish, distribute, sublicense, and/or sell\ncopies of the Software, and to permit persons to whom the Software is\nfurnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included i" }),
            ["xunit 2.9.2"] = new("Apache-2.0", "https://licenses.nuget.org/Apache-2.0", new Dictionary<string, string> {  }),
            ["xunit.abstractions 2.0.3"] = new(null, "https://raw.githubusercontent.com/xunit/xunit/master/license.txt", new Dictionary<string, string> {  }),
            ["xunit.analyzers 1.16.0"] = new("Apache-2.0", "https://licenses.nuget.org/Apache-2.0", new Dictionary<string, string> {  }),
            ["xunit.assert 2.9.2"] = new("Apache-2.0", "https://licenses.nuget.org/Apache-2.0", new Dictionary<string, string> {  }),
            ["xunit.core 2.9.2"] = new("Apache-2.0", "https://licenses.nuget.org/Apache-2.0", new Dictionary<string, string> {  }),
            ["xunit.extensibility.core 2.9.2"] = new("Apache-2.0", "https://licenses.nuget.org/Apache-2.0", new Dictionary<string, string> {  }),
            ["xunit.extensibility.execution 2.9.2"] = new("Apache-2.0", "https://licenses.nuget.org/Apache-2.0", new Dictionary<string, string> {  }),
            ["xunit.runner.visualstudio 2.8.2"] = new("Apache-2.0", "https://licenses.nuget.org/Apache-2.0", new Dictionary<string, string> {  }),
    };

    private static readonly Dictionary<string, LicenceRow422> LicenceRows422 = new()
    {
        ["Z-licence"] = new(
            [],
            [],
            "pass"),
        ["E-xa-mit"] = new(
            [("| xunit.abstractions | 2.0.3 | Transitive | Apache-2.0 | licence URL | generated specs (test-only) | ok |", "| xunit.abstractions | 2.0.3 | Transitive | MIT | licence URL | generated specs (test-only) | ok |")],
            [],
            "fail:xunit.abstractions 2.0.3: licence MIT but the reviewed licence URL says Apache-2.0"),
        ["E-srm-apache"] = new(
            [("| System.Reflection.Metadata | 1.6.0 | Transitive | MIT | packaged licence file | generated specs (test-only) | ok |", "| System.Reflection.Metadata | 1.6.0 | Transitive | Apache-2.0 | packaged licence file | generated specs (test-only) | ok |")],
            [],
            "fail:System.Reflection.Metadata 1.6.0: licence Apache-2.0 but the packaged licence file says MIT"),
        ["E-swap"] = new(
            [("| xunit.abstractions | 2.0.3 | Transitive | Apache-2.0 | licence URL | generated specs (test-only) | ok |", "| xunit.abstractions | 2.0.3 | Transitive | MIT | licence URL | generated specs (test-only) | ok |"), ("| System.Reflection.Metadata | 1.6.0 | Transitive | MIT | packaged licence file | generated specs (test-only) | ok |", "| System.Reflection.Metadata | 1.6.0 | Transitive | Apache-2.0 | packaged licence file | generated specs (test-only) | ok |")],
            [],
            "fail:xunit.abstractions 2.0.3: licence MIT but && System.Reflection.Metadata 1.6.0: licence Apache-2.0 but"),
        ["E-expr-swap"] = new(
            [("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Direct | Apache-2.0 | nuspec licence expression | generated service and generated specs | ok |"), ("| FluentValidation | 12.1.1 | Transitive | Apache-2.0 | nuspec licence expression | generated service and generated specs | ok |", "| FluentValidation | 12.1.1 | Transitive | MIT | nuspec licence expression | generated service and generated specs | ok |")],
            [],
            "fail:FastEndpoints 8.3.0: licence Apache-2.0 but the nuspec expression is MIT && FluentValidation 12.1.1: licence MIT but the nuspec expression is Apache-2.0"),
        ["E-url-unreviewed"] = new(
            [],
            [("xunit.abstractions", "2.0.3", "licence_url", "https://example.invalid/license.txt")],
            "fail:xunit.abstractions 2.0.3: licence Apache-2.0 from licence URL https://example.invalid/license.txt, which is not a reviewed URL"),
        ["E-url-missing"] = new(
            [],
            [("xunit.abstractions", "2.0.3", "licence_url", "")],
            "fail:xunit.abstractions 2.0.3: licence Apache-2.0 from licence URL -, which is not a reviewed URL"),
        ["E-file-apache"] = new(
            [],
            [("System.Reflection.Metadata", "1.6.0", "licence_files", "LICENSE.TXT=                                 Apache License\n                           Version 2.0, January 2004")],
            "fail:System.Reflection.Metadata 1.6.0: licence MIT but the packaged licence file says Apache-2.0"),
        ["E-file-none"] = new(
            [],
            [("System.Reflection.Metadata", "1.6.0", "licence_files", "")],
            "fail:System.Reflection.Metadata 1.6.0: licence MIT but no packaged licence file names a known licence"),
        ["E-file-wrapped"] = new(
            [],
            [("System.Reflection.Metadata", "1.6.0", "licence_files", "LICENSE.TXT=The MIT\nLicense (MIT)\n\nCopyright (c) .NET Foundation")],
            "pass"),
        ["E-file-leading"] = new(
            [],
            [("System.Reflection.Metadata", "1.6.0", "licence_files", "LICENSE.TXT=   \n\t  The MIT License (MIT)")],
            "pass"),
        ["E-file-newtonsoft"] = new(
            [],
            [("Newtonsoft.Json", "13.0.1", "licence_files", "LICENSE.md=Apache License Version 2.0")],
            "pass"),
    };

    private static readonly Dictionary<string, RestoreRow422> RestoreRows422 = new()
    {
        ["Z-restore"] = new("restore", 0, 30, false,
            "  Determining projects to restore...\n  Restored <temp>/avowry-x/Orders.Api/Orders.Api.csproj (in 1.21 sec).\n",
            "pass"),
        ["O-warm-warning"] = new("restore", 0, 30, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : warning NU1801: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n  Determining projects to restore...\n  Restored <temp>/avowry-x/Orders.Api/Orders.Api.csproj (in 1.21 sec).\n",
            "pass"),
        ["O-warm-nopkg"] = new("restore", 0, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : warning NU1801: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n  All projects are up-to-date for restore.\n",
            "pass"),
        ["O-cold"] = new("restore", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n",
            "network-missing"),
        ["O-cold-host"] = new("restore", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n  No such host is known. (api.nuget.org:443)\n",
            "network-missing"),
        ["O-partial"] = new("restore", 1, 12, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n",
            "fail"),
        ["O-nu1102"] = new("restore", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1102: Unable to find package FluentValidation with version (>= 12.1.1)\n  - Found 192 version(s) in nuget.org [ Nearest version: 12.1.1 ]\n",
            "fail"),
        ["O-other"] = new("restore", 1, 0, false,
            "  error MSB1009: Project file does not exist.\n",
            "fail"),
        ["O-timeout"] = new("restore", -1, 0, true,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n",
            "fail"),
        ["O-crlf"] = new("restore", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.\r\n",
            "network-missing"),
        ["O-leading"] = new("restore", 1, 0, false,
            "        <temp>/avowry-x/Orders.Api/Orders.Api.csproj : error nu1301: unable to load the service index for source https://api.nuget.org/v3/index.json.\n",
            "network-missing"),
        ["O-planted"] = new("restore-planted", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1004: The package reference FastEndpoints version has changed from [8.2.0, ) to [8.3.0, ).The packages lock file is inconsistent with the project dependencies so restore can't be run in locked mode. Disable the RestoreLockedMode MSBuild property or pass an explicit --force-evaluate option to run restore to update the lock file.\n",
            "mismatch"),
        ["O-planted-offline"] = new("restore-planted", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1004: The package reference FastEndpoints version has changed from [8.2.0, ) to [8.3.0, ).The packages lock file is inconsistent with the project dependencies so restore can't be run in locked mode. Disable the RestoreLockedMode MSBuild property or pass an explicit --force-evaluate option to run restore to update the lock file.\n",
            "mismatch"),
        ["O-planted-lower"] = new("restore-planted", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error nu1004: The package reference FastEndpoints version has changed from [8.2.0, ) to [8.3.0, ).The packages lock file is inconsistent with the project dependencies so restore can't be run in locked mode. Disable the RestoreLockedMode MSBuild property or pass an explicit --force-evaluate option to run restore to update the lock file.\n",
            "mismatch"),
        ["O-planted-wrapped"] = new("restore-planted", 1, 0, false,
            "error\nNU1004:\n  The package reference FastEndpoints version has changed",
            "mismatch"),
        ["O-planted-nu1102"] = new("restore-planted", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1102: Unable to find package FluentValidation with version (>= 12.1.1)\n  - Found 192 version(s) in nuget.org [ Nearest version: 12.1.1 ]\n",
            "fail"),
        ["O-planted-cold"] = new("restore-planted", 1, 0, false,
            "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n",
            "network-missing"),
        ["O-planted-nu10041"] = new("restore-planted", 1, 0, false,
            "error NU10041: something else\n",
            "fail"),
    };

    public static TheoryData<string> LicenceRowIds422()
    {
        var data = new TheoryData<string>();
        foreach (var id in LicenceRows422.Keys)
            data.Add(id);
        return data;
    }

    public static TheoryData<string> RestoreRowIds422()
    {
        var data = new TheoryData<string>();
        foreach (var id in RestoreRows422.Keys)
            data.Add(id);
        return data;
    }

    private static bool OutcomeMatches422(string expected, IReadOnlyList<string> failures)
    {
        if (expected == "pass")
            return failures.Count == 0;
        var joined = string.Join("\n", failures);
        return failures.Count > 0 && expected["fail:".Length..].Split(" && ").All(part => joined.Contains(part, StringComparison.Ordinal));
    }

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-2")]
    [MemberData(nameof(LicenceRowIds422))]
    public void Licence_value_rows_422(string id)
    {
        // Given the real page with the row's edit and the restored packages' nuspec facts (with the row's evidence edit)
        var row = LicenceRows422[id];
        var page = File.ReadAllText(RepoPaths.Find("docs", "release", "pre-publication-checks.md")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var sectionAt = page.IndexOf("## Restored package licences", StringComparison.Ordinal);
        Assert.True(sectionAt >= 0, id + ": section missing");
        foreach (var (old, replacement) in row.Edits)
        {
            var at = page.IndexOf(old, sectionAt, StringComparison.Ordinal);
            Assert.True(at >= 0, id + ": edit not found: " + old);
            page = page[..at] + replacement + page[(at + old.Length)..];
        }

        var evidence = Evidence422();
        foreach (var (pid, version, field, value) in row.Evidence)
        {
            var key = pid + " " + version;
            evidence[key] = field switch
            {
                "licence_url" => evidence[key] with { LicenceUrl = value },
                "expression" => evidence[key] with { Expression = value },
                "licence_files" => evidence[key] with { LicenceFiles = value.Length == 0 ? new Dictionary<string, string>() : new Dictionary<string, string> { [value[..value.IndexOf('=')]] = value[(value.IndexOf('=') + 1)..] } },
                _ => throw new InvalidOperationException(id + ": unknown evidence field " + field),
            };
        }

        // When the licence values of the page rows are compared with the evidence
        var table = ReadRestoredRows(page).Select(r => new[] { r.Id, r.Version, r.Kind, r.Licence, r.LicenceSource, r.WhereUsed, r.Review }).ToList();
        var failures = RestoredLicenceChecks.LicenceValueFailures(table, evidence);

        // Then the outcome is the row's: no failure, or failures that name the package and both licences
        Assert.True(OutcomeMatches422(row.Expected, failures), id + ": expected " + row.Expected + ", got " + string.Join(" | ", failures));
    }

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-6")]
    [MemberData(nameof(RestoreRowIds422))]
    public void Restore_outcome_rows_422(string id)
    {
        // Given the output of a locked restore (exit code, time-out, text, and the package libraries in the new project.assets.json)
        var row = RestoreRows422[id];

        // When the outcome is classified
        var outcome = RestoredLicenceChecks.RestoreOutcome(row.ExitCode, row.TimedOut, row.Output, row.RestoredCount);

        // Then a warm cache is no network miss, a lock mismatch is never a network miss, and a miss counts only when nothing was restored
        Assert.Equal(row.Expected, outcome);
    }

    // ---- Card 422 fix round 1 red (task-force role: red) -------------------------------------------------------------
    // Offline rows (corpus-fix.txt, ids FW-*) for the pure functions the networked F5, F6 and RestoreLockedOrFail call:
    // F6Failures and F6AcceptanceFailures (422-2), ClassifyRestore and RestoredCount (422-6), and a source scan that the three
    // networked tests call them (422-2 and 422-6). A row that fails on the refactored code runs in a Theory marked Pending.

    private sealed record FixRow422(string Expected, string Text);

    private sealed record ClassifyFix422(string Kind, int ExitCode, int Count, bool TimedOut, string Text, string Expected);

    private const string NuspecNs422 = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd";
    private const string XaUrl422 = "https://raw.githubusercontent.com/xunit/xunit/master/license.txt";
    private const string SrmUrl422 = "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT";
    private const string MitHead422 = "The MIT License (MIT)\n\nCopyright (c) .NET Foundation and Contributors\n\nAll rights reserved.";
    private const string ApacheHead422 = "                                 Apache License\n                           Version 2.0, January 2004\n                        http://www.apache.org/licenses/";
    private const string Nu1301 = "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n";
    private const string Nu1801 = "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : warning NU1801: Unable to load the service index for source https://api.nuget.org/v3/index.json.\n";
    private const string Nu1004 = "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : error NU1004: The package reference FastEndpoints version has changed from [8.2.0, ) to [8.3.0, ).The packages lock file is inconsistent with the project dependencies so restore can't be run in locked mode.\n";
    private const string RestoredText = "  Determining projects to restore...\n  Restored <temp>/avowry-x/Orders.Api/Orders.Api.csproj (in 1.21 sec).\n";
    // The localised texts keep the accented letters as code points (u-umlaut 0xFC, a-umlaut 0xE4, e-acute 0xE9, a-grave 0xE0) so the file stays ASCII.
    private static readonly string De1301 = "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : Fehler NU1301: Der Dienstindex f" + (char)0xFC + "r die Quelle \"https://api.nuget.org/v3/index.json\" konnte nicht geladen werden.\n";
    private static readonly string De1801 = "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : Warnung NU1801: Der Dienstindex f" + (char)0xFC + "r die Quelle \"https://api.nuget.org/v3/index.json\" konnte nicht geladen werden.\n";
    private static readonly string De1004 = "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : Fehler NU1004: Der Paketverweis FastEndpoints wurde von [8.2.0, ) in [8.3.0, ) ge" + (char)0xE4 + "ndert.\n";
    private static readonly string Fr1004 = "<temp>/avowry-x/Orders.Api/Orders.Api.csproj : erreur NU1004: La r" + (char)0xE9 + "f" + (char)0xE9 + "rence de package FastEndpoints a chang" + (char)0xE9 + " de [8.2.0, ) " + (char)0xE0 + " [8.3.0, ).\n";

    // The four packages whose nuspec sets requireLicenseAcceptance to true.
    private static readonly string[] Acceptance422 =
    [
        "Microsoft.CodeCoverage 17.11.1",
        "Microsoft.NET.Test.Sdk 17.11.1",
        "Microsoft.TestPlatform.ObjectModel 17.11.1",
        "Microsoft.TestPlatform.TestHost 17.11.1",
    ];

    // The notice files some packages carry (no licence file: the name rule must ignore them).
    private static readonly Dictionary<string, string[]> Notices422 = new()
    {
        ["Microsoft.CodeCoverage 17.11.1"] = ["ThirdPartyNotices.txt", "build/netstandard2.0/ThirdPartyNotices.txt"],
        ["Microsoft.TestPlatform.TestHost 17.11.1"] = ["ThirdPartyNotices.txt"],
        ["Microsoft.Extensions.Configuration.Abstractions 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["Microsoft.Extensions.DependencyInjection.Abstractions 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["Microsoft.Extensions.Diagnostics.Abstractions 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["Microsoft.Extensions.FileProviders.Abstractions 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["Microsoft.Extensions.Hosting.Abstractions 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["Microsoft.Extensions.Logging.Abstractions 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["Microsoft.Extensions.Options 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["Microsoft.Extensions.Primitives 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["System.Diagnostics.DiagnosticSource 10.0.11"] = ["THIRD-PARTY-NOTICES.TXT"],
        ["System.Reflection.Metadata 1.6.0"] = ["THIRD-PARTY-NOTICES.TXT"],
    };

    // The rows that fail on the refactored code (today's decision is wrong): they run in the Pending Theories.
    private static readonly HashSet<string> F6Pending422 =
    [
        "FW-E-swap", "FW-E-xa-mit", "FW-E-srm-apache", "FW-ev-file-apache", "FW-ev-file-unknown", "FW-ev-url-mit",
        "FW-both-url-source", "FW-pkg-url-case", "FW-pkg-url-http", "FW-pkg-notice-apache",
    ];

    private static readonly HashSet<string> ClassifyPending422 =
    [
        "FW-O-planted-nu10041", "FW-O-nu10045", "FW-O-xnu1004", "FW-O-warm-warning", "FW-O-warm-crlf", "FW-O-net-exit0-count0",
        "FW-O-partial", "FW-O-net-count30", "FW-O-de-partial", "FW-O-planted-offline", "FW-O-de-planted-offline", "FW-O-nu1004-lower-net",
    ];

    private static readonly HashSet<string> WirePending422 = ["FW-src-f6failures-value", "FW-src-classify-outcome"];

    private static string Nuspec422(string id, string version, string inner, bool ns = true) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<package" + (ns ? " xmlns=\"" + NuspecNs422 + "\"" : "") + ">\n  <metadata>\n    <id>" + id + "</id>\n    <version>" + version + "</version>\n" + inner + "  </metadata>\n</package>\n";

    // A page edit that changes one cell (column 3 licence, column 4 licence source) of the row of one package.
    private static string Cell422(string id, string version, int column, string value)
    {
        var body = RestoredBody(File.ReadAllText(RepoPaths.Find("docs", "release", "pre-publication-checks.md")).Replace("\r\n", "\n", StringComparison.Ordinal));
        var old = body.Split('\n').First(line => line.StartsWith("| " + id + " | " + version + " |", StringComparison.Ordinal));
        var cells = old.Trim().Trim('|').Split('|');
        cells[column] = " " + value + " ";
        return old + "|=>|" + "|" + string.Join("|", cells) + "|";
    }

    private static Dictionary<string, FixRow422> F6Rows422() => new()
    {
        ["FW-f6-real"] = new("pass", ""),
        ["FW-E-swap"] = new("fail:xunit.abstractions 2.0.3: licence MIT but the reviewed licence URL says Apache-2.0 && System.Reflection.Metadata 1.6.0: licence Apache-2.0 but the packaged licence file says MIT",
            Cell422("xunit.abstractions", "2.0.3", 3, "MIT") + "|;|" + Cell422("System.Reflection.Metadata", "1.6.0", 3, "Apache-2.0")),
        ["FW-E-xa-mit"] = new("fail:xunit.abstractions 2.0.3: licence MIT but the reviewed licence URL says Apache-2.0", Cell422("xunit.abstractions", "2.0.3", 3, "MIT")),
        ["FW-E-srm-apache"] = new("fail:System.Reflection.Metadata 1.6.0: licence Apache-2.0 but the packaged licence file says MIT", Cell422("System.Reflection.Metadata", "1.6.0", 3, "Apache-2.0")),
        ["FW-E-expr-mit-apache"] = new("fail:FastEndpoints 8.3.0: licence Apache-2.0 but the nuspec expression is MIT", Cell422("FastEndpoints", "8.3.0", 3, "Apache-2.0")),
        ["FW-E-expr-apache-mit"] = new("fail:xunit 2.9.2: licence MIT but the nuspec expression is Apache-2.0", Cell422("xunit", "2.9.2", 3, "MIT")),
        ["FW-ev-file-apache"] = new("fail:System.Reflection.Metadata 1.6.0: licence MIT but the packaged licence file says Apache-2.0", "@ev System.Reflection.Metadata 1.6.0 licence_files LICENSE.TXT=" + ApacheHead422),
        ["FW-ev-file-apache-page-apache"] = new("pass", Cell422("System.Reflection.Metadata", "1.6.0", 3, "Apache-2.0") + "|;|@ev System.Reflection.Metadata 1.6.0 licence_files LICENSE.TXT=" + ApacheHead422),
        ["FW-ev-file-none"] = new("fail:System.Reflection.Metadata 1.6.0: licence MIT but no packaged licence file names a known licence", "@ev System.Reflection.Metadata 1.6.0 licence_files "),
        ["FW-ev-file-unknown"] = new("fail:System.Reflection.Metadata 1.6.0: licence MIT but no packaged licence file names a known licence", "@ev System.Reflection.Metadata 1.6.0 licence_files LICENSE.TXT=Copyright (c) Example. All rights reserved."),
        ["FW-ev-url-mit"] = new("fail:xunit.abstractions 2.0.3: licence Apache-2.0 from licence URL https://licenses.nuget.org/MIT, which is not a reviewed URL", "@ev xunit.abstractions 2.0.3 licence_url https://licenses.nuget.org/MIT"),
        ["FW-ev-url-none"] = new("fail:xunit.abstractions 2.0.3: licence Apache-2.0 from licence URL -, which is not a reviewed URL", "@ev xunit.abstractions 2.0.3 licence_url "),
        ["FW-both-url-source"] = new("fail:System.Reflection.Metadata 1.6.0: licence MIT from licence URL " + SrmUrl422 + ", which is not a reviewed URL", Cell422("System.Reflection.Metadata", "1.6.0", 4, "licence URL")),
        ["FW-both-file-wins"] = new("pass", "@ev System.Reflection.Metadata 1.6.0 licence_url https://licenses.nuget.org/Apache-2.0"),
        ["FW-url-file-ignored"] = new("pass", "@ev xunit.abstractions 2.0.3 licence_files LICENSE.txt=" + MitHead422),
        ["FW-file-source-no-file"] = new("fail:xunit.abstractions 2.0.3: licence Apache-2.0 but no packaged licence file names a known licence", Cell422("xunit.abstractions", "2.0.3", 4, "packaged licence file")),
        ["FW-page-not-restored"] = new("fail:locked restore vs page missing: xunit.abstractions 2.0.3; extra: ", "@drop xunit.abstractions 2.0.3"),
        ["FW-restored-not-on-page"] = new("fail:locked restore vs page missing: ; extra: Evil 1.0.0", "@extra Evil 1.0.0"),
        ["FW-no-folder"] = new("fail:FluentValidation 12.1.1: package folder missing after restore", "@nofolder FluentValidation 12.1.1"),
        ["FW-no-nuspec"] = new("fail:FluentValidation 12.1.1: nuspec missing after restore", "@nuspec FluentValidation 12.1.1 @none"),
        ["FW-label"] = new("fail:xunit.abstractions 2.0.3: licence source is not one of the three labels", Cell422("xunit.abstractions", "2.0.3", 4, "licence-url")),
        ["FW-acc-flag"] = new("fail:acceptance set missing: ; extra: Microsoft.CodeCoverage 17.11.1", "@acc Microsoft.CodeCoverage 17.11.1 false"),
        ["FW-pkg-ns-url"] = new("pass", "@nuspec xunit.abstractions 2.0.3 " + Nuspec422("xunit.abstractions", "2.0.3", "    <licenseUrl>" + XaUrl422 + "</licenseUrl>\n")),
        ["FW-pkg-url-ws"] = new("pass", "@nuspec xunit.abstractions 2.0.3 " + Nuspec422("xunit.abstractions", "2.0.3", "    <licenseUrl>\n      " + XaUrl422 + "  \n    </licenseUrl>\n")),
        ["FW-pkg-url-case"] = new("fail:xunit.abstractions 2.0.3: licence Apache-2.0 from licence URL " + XaUrl422.Replace("license.txt", "LICENSE.txt") + ", which is not a reviewed URL", "@nuspec xunit.abstractions 2.0.3 " + Nuspec422("xunit.abstractions", "2.0.3", "    <licenseUrl>" + XaUrl422.Replace("license.txt", "LICENSE.txt") + "</licenseUrl>\n")),
        ["FW-pkg-url-http"] = new("fail:xunit.abstractions 2.0.3: licence Apache-2.0 from licence URL " + XaUrl422.Replace("https", "http") + ", which is not a reviewed URL", "@nuspec xunit.abstractions 2.0.3 " + Nuspec422("xunit.abstractions", "2.0.3", "    <licenseUrl>" + XaUrl422.Replace("https", "http") + "</licenseUrl>\n")),
        ["FW-pkg-expr-single"] = new("pass", "@nuspec FastEndpoints 8.3.0 " + Nuspec422("FastEndpoints", "8.3.0", "    <license type='expression'>MIT</license>\n", ns: false)),
        ["FW-pkg-expr-ws"] = new("pass", "@nuspec FastEndpoints 8.3.0 " + Nuspec422("FastEndpoints", "8.3.0", "    <license type=\"expression\">\n      MIT\n    </license>\n")),
        ["FW-pkg-type-case"] = new("pass", "@nuspec FastEndpoints 8.3.0 " + Nuspec422("FastEndpoints", "8.3.0", "    <license type=\"Expression\">MIT</license>\n")),
        // The reference prints a missing expression as None; C# prints it as nothing, so the row names the part both agree on.
        ["FW-pkg-type-file"] = new("fail:FastEndpoints 8.3.0: licence MIT but the nuspec expression is", "@nuspec FastEndpoints 8.3.0 " + Nuspec422("FastEndpoints", "8.3.0", "    <license type=\"file\">LICENSE.txt</license>\n")),
        ["FW-pkg-broken"] = new("fail:FluentValidation 12.1.1: nuspec does not parse", "@nuspec FluentValidation 12.1.1 <package><metadata>"),
        ["FW-pkg-acc-true-ws"] = new("pass", "@nuspec Microsoft.CodeCoverage 17.11.1 " + Nuspec422("Microsoft.CodeCoverage", "17.11.1", "    <license type=\"expression\">MIT</license>\n    <requireLicenseAcceptance> True </requireLicenseAcceptance>\n")),
        ["FW-pkg-acc-false"] = new("fail:acceptance set missing: ; extra: Microsoft.NET.Test.Sdk 17.11.1", "@nuspec Microsoft.NET.Test.Sdk 17.11.1 " + Nuspec422("Microsoft.NET.Test.Sdk", "17.11.1", "    <license type=\"expression\">MIT</license>\n    <requireLicenseAcceptance>false</requireLicenseAcceptance>\n")),
        ["FW-pkg-file-crlf"] = new("pass", "@nofiles System.Reflection.Metadata 1.6.0|;|@file System.Reflection.Metadata 1.6.0 LICENSE.TXT=" + MitHead422.Replace("\n", "\r\n")),
        ["FW-pkg-file-nested"] = new("pass", "@nofiles System.Reflection.Metadata 1.6.0|;|@file System.Reflection.Metadata 1.6.0 build/LICENSE.TXT=" + MitHead422),
        ["FW-pkg-file-backslash"] = new("pass", "@nofiles System.Reflection.Metadata 1.6.0|;|@file System.Reflection.Metadata 1.6.0 build\\LICENSE.TXT=" + MitHead422),
        ["FW-pkg-file-licence"] = new("pass", "@nofiles System.Reflection.Metadata 1.6.0|;|@file System.Reflection.Metadata 1.6.0 licence.md=" + MitHead422),
        ["FW-pkg-file-copying"] = new("pass", "@nofiles System.Reflection.Metadata 1.6.0|;|@file System.Reflection.Metadata 1.6.0 COPYING=" + MitHead422),
        ["FW-pkg-file-upper"] = new("pass", "@nofiles System.Reflection.Metadata 1.6.0|;|@file System.Reflection.Metadata 1.6.0 LICENSE.TXT=" + MitHead422),
        ["FW-pkg-notice-apache"] = new("fail:System.Reflection.Metadata 1.6.0: licence Apache-2.0 but the packaged licence file says MIT", Cell422("System.Reflection.Metadata", "1.6.0", 3, "Apache-2.0") + "|;|@file System.Reflection.Metadata 1.6.0 THIRD-PARTY-NOTICES.TXT=" + ApacheHead422),
        ["FW-pkg-dual"] = new("pass", Cell422("System.Reflection.Metadata", "1.6.0", 3, "Apache-2.0") + "|;|@file System.Reflection.Metadata 1.6.0 LICENSE-APACHE.txt=" + ApacheHead422),
    };

    private static Dictionary<string, ClassifyFix422> ClassifyRows422() => new()
    {
        ["FW-O-planted-nu10041"] = new("classify-planted", 1, 0, false, "error NU10041: something else\n", "fail"),
        ["FW-O-warm-warning"] = new("classify", 0, 30, false, Nu1801 + RestoredText, "pass"),
        ["FW-O-partial"] = new("classify", 1, 12, false, Nu1301, "fail"),
        ["FW-O-planted-offline"] = new("classify-planted", 1, 0, false, Nu1301 + Nu1004, "mismatch"),
        ["FW-O-cold"] = new("classify", 1, 0, false, Nu1301, "network-missing"),
        ["FW-O-net-count30"] = new("classify", 1, 30, false, Nu1301, "fail"),
        ["FW-O-net-exit0-count0"] = new("classify", 0, 0, false, Nu1301 + "  All projects are up-to-date for restore.\n", "pass"),
        ["FW-O-planted"] = new("classify-planted", 1, 0, false, Nu1004, "mismatch"),
        ["FW-O-planted-crlf"] = new("classify-planted", 1, 0, false, Nu1004.Replace("\n", "\r\n"), "mismatch"),
        ["FW-O-warm-crlf"] = new("classify", 0, 30, false, (Nu1801 + RestoredText).Replace("\n", "\r\n"), "pass"),
        ["FW-O-cold-crlf"] = new("classify", 1, 0, false, Nu1301.Replace("\n", "\r\n"), "network-missing"),
        ["FW-O-de-cold"] = new("classify", 1, 0, false, De1301, "network-missing"),
        ["FW-O-de-warm"] = new("classify", 0, 30, false, De1801 + RestoredText, "pass"),
        ["FW-O-de-planted-offline"] = new("classify-planted", 1, 0, false, De1301 + De1004, "mismatch"),
        ["FW-O-de-partial"] = new("classify", 1, 12, false, De1301, "fail"),
        ["FW-O-fr-planted"] = new("classify-planted", 1, 0, false, Fr1004, "mismatch"),
        ["FW-O-nu10045"] = new("classify-planted", 1, 0, false, "<temp>/x.csproj : error NU10045: something else\n", "fail"),
        ["FW-O-xnu1004"] = new("classify-planted", 1, 0, false, "<temp>/x.csproj : error XNU1004: something else\n", "fail"),
        ["FW-O-nu1004-paren"] = new("classify-planted", 1, 0, false, "restore failed (NU1004)\n", "mismatch"),
        ["FW-O-nu1004-lower-net"] = new("classify-planted", 1, 0, false, Nu1301.ToLowerInvariant() + Nu1004.ToLowerInvariant(), "mismatch"),
        ["FW-O-nu1004-locked"] = new("classify", 1, 0, false, Nu1004, "mismatch"),
        ["FW-O-timeout-nu1004"] = new("classify-planted", -1, 0, true, Nu1004, "fail"),
        ["FW-O-host-only"] = new("classify", 1, 0, false, "  No such host is known. (api.nuget.org:443)\n", "network-missing"),
    };

    // id, expected count, assets text (null: no assets file)
    private static readonly Dictionary<string, (int Expected, string? Assets)> CountRows422 = new()
    {
        ["FW-count-shape"] = (2, "{\"version\": 3, \"libraries\": {\"FastEndpoints/8.3.0\": {\"type\": \"package\"}, \"xunit/2.9.2\": {\"type\": \"package\"}, \"Orders.Api/1.0.0\": {\"type\": \"project\"}}}"),
        ["FW-count-project-only"] = (0, "{\"version\": 3, \"libraries\": {\"Orders.Api/1.0.0\": {\"type\": \"project\"}}}"),
        ["FW-count-none"] = (0, null),
        ["FW-count-empty"] = (0, "{\"version\": 3}"),
    };

    // id -> (rule, criterion)
    private static readonly Dictionary<string, (string Rule, string Criterion)> WireRows422 = new()
    {
        ["FW-src-f6-calls"] = ("f6-calls", "422-2"),
        ["FW-src-read-packages"] = ("read-packages", "422-2"),
        ["FW-src-f6failures-value"] = ("f6failures-value", "422-2"),
        ["FW-src-f5-classify"] = ("f5-classify", "422-6"),
        ["FW-src-locked-classify"] = ("locked-classify", "422-6"),
        ["FW-src-classify-outcome"] = ("classify-outcome", "422-6"),
        ["FW-src-no-networkmissing"] = ("no-networkmissing", "422-6"),
    };

    private static TheoryData<string> Ids422(IEnumerable<string> ids)
    {
        var data = new TheoryData<string>();
        foreach (var id in ids)
            data.Add(id);
        return data;
    }

    public static TheoryData<string> F6HoldingIds422() => Ids422(F6Rows422().Keys.Where(id => !F6Pending422.Contains(id)));

    public static TheoryData<string> F6PendingIds422() => Ids422(F6Rows422().Keys.Where(F6Pending422.Contains));

    public static TheoryData<string> ClassifyHoldingIds422() => Ids422(ClassifyRows422().Keys.Where(id => !ClassifyPending422.Contains(id)));

    public static TheoryData<string> ClassifyPendingIds422() => Ids422(ClassifyRows422().Keys.Where(ClassifyPending422.Contains));

    public static TheoryData<string> CountIds422() => Ids422(CountRows422.Keys);

    public static TheoryData<string> WireHolding2Ids422() => Ids422(WireRows422.Where(r => r.Value.Criterion == "422-2" && !WirePending422.Contains(r.Key)).Select(r => r.Key));

    public static TheoryData<string> WirePending2Ids422() => Ids422(WireRows422.Where(r => r.Value.Criterion == "422-2" && WirePending422.Contains(r.Key)).Select(r => r.Key));

    public static TheoryData<string> WireHolding6Ids422() => Ids422(WireRows422.Where(r => r.Value.Criterion == "422-6" && !WirePending422.Contains(r.Key)).Select(r => r.Key));

    public static TheoryData<string> WirePending6Ids422() => Ids422(WireRows422.Where(r => r.Value.Criterion == "422-6" && WirePending422.Contains(r.Key)).Select(r => r.Key));

    private sealed class FixPackage422
    {
        public string Id = "";
        public string Version = "";
        public bool Folder = true;
        public string? Expression;
        public string? Url;
        public bool Acceptance;
        public bool NuspecSet;
        public string? Nuspec;
        public List<(string Path, string Text)> Files = [];
    }

    private static List<FixPackage422> BasePackages422()
    {
        var packages = new List<FixPackage422>();
        foreach (var (key, evidence) in Evidence422())
        {
            var parts = key.Split(' ');
            var package = new FixPackage422 { Id = parts[0], Version = parts[1], Expression = evidence.Expression, Url = evidence.LicenceUrl, Acceptance = Acceptance422.Contains(key) };
            package.Files.AddRange(evidence.LicenceFiles.Select(file => (file.Key, file.Value)));
            if (Notices422.TryGetValue(key, out var notices))
                package.Files.AddRange(notices.Select(path => (path, "")));
            packages.Add(package);
        }

        return packages;
    }

    private static string? NuspecOf422(FixPackage422 package)
    {
        if (package.NuspecSet)
            return package.Nuspec;
        var inner = (package.Expression is null ? "" : "    <license type=\"expression\">" + package.Expression + "</license>\n")
            + (package.Url is null ? "" : "    <licenseUrl>" + package.Url + "</licenseUrl>\n")
            + (package.Acceptance ? "    <requireLicenseAcceptance>true</requireLicenseAcceptance>\n" : "");
        return Nuspec422(package.Id, package.Version, inner);
    }

    private static void SettleF6Row422(string id)
    {
        // Given the real page with the row's edits and the 30 restored packages built from their nuspec facts and file lists (with the row's edits)
        var row = F6Rows422()[id];
        var page = File.ReadAllText(RepoPaths.Find("docs", "release", "pre-publication-checks.md")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var sectionAt = page.IndexOf("## Restored package licences", StringComparison.Ordinal);
        Assert.True(sectionAt >= 0, id + ": section missing");
        var packages = BasePackages422();
        FixPackage422 Find(string pid, string version) => packages.First(p => p.Id == pid && p.Version == version);
        foreach (var edit in row.Text.Split("|;|").Where(e => e.Length > 0))
        {
            if (edit.StartsWith("@ev ", StringComparison.Ordinal))
            {
                var parts = edit.Split(' ', 5);
                var package = Find(parts[1], parts[2]);
                if (parts[3] == "licence_files")
                {
                    package.Files = parts[4].Length == 0 ? [] : [(parts[4][..parts[4].IndexOf('=')], parts[4][(parts[4].IndexOf('=') + 1)..])];
                }
                else if (parts[3] == "licence_url")
                {
                    package.Url = parts[4];
                }
                else
                {
                    throw new InvalidOperationException(id + ": unknown evidence field " + parts[3]);
                }
            }
            else if (edit.StartsWith("@drop ", StringComparison.Ordinal))
            {
                var parts = edit.Split(' ');
                packages.Remove(Find(parts[1], parts[2]));
            }
            else if (edit.StartsWith("@extra ", StringComparison.Ordinal))
            {
                var parts = edit.Split(' ');
                packages.Add(new FixPackage422 { Id = parts[1], Version = parts[2], Expression = "MIT" });
            }
            else if (edit.StartsWith("@nofolder ", StringComparison.Ordinal))
            {
                var parts = edit.Split(' ');
                Find(parts[1], parts[2]).Folder = false;
            }
            else if (edit.StartsWith("@acc ", StringComparison.Ordinal))
            {
                var parts = edit.Split(' ');
                Find(parts[1], parts[2]).Acceptance = parts[3] == "true";
            }
            else if (edit.StartsWith("@nuspec ", StringComparison.Ordinal))
            {
                var parts = edit.Split(' ', 4);
                var package = Find(parts[1], parts[2]);
                package.NuspecSet = true;
                package.Nuspec = parts[3] == "@none" ? null : parts[3];
            }
            else if (edit.StartsWith("@nofiles ", StringComparison.Ordinal))
            {
                var parts = edit.Split(' ');
                Find(parts[1], parts[2]).Files = [];
            }
            else if (edit.StartsWith("@file ", StringComparison.Ordinal))
            {
                var parts = edit.Split(' ', 4);
                var at = parts[3].IndexOf('=');
                Find(parts[1], parts[2]).Files.Add((parts[3][..at], parts[3][(at + 1)..]));
            }
            else
            {
                var pair = edit.Split("|=>|");
                Assert.True(pair.Length == 2, id + ": edit has no replacement: " + edit);
                var at = page.IndexOf(pair[0], sectionAt, StringComparison.Ordinal);
                Assert.True(at >= 0, id + ": edit not found: " + pair[0]);
                page = page[..at] + pair[1] + page[(at + pair[0].Length)..];
            }
        }

        var restored = packages
            .Select(p => RestoredLicenceChecks.RestoredPackageFrom(
                p.Id,
                p.Version,
                p.Folder,
                NuspecOf422(p),
                p.Files.Select(file => file.Path).ToList(),
                rel => { var text = p.Files.First(file => file.Path == rel).Text; return text[..Math.Min(600, text.Length)]; }))
            .ToList();
        var tableRows = ReadRestoredRows(page).Select(r => new[] { r.Id, r.Version, r.Kind, r.Licence, r.LicenceSource, r.WhereUsed, r.Review }).ToList();

        // When F6's two decisions run
        var failures = RestoredLicenceChecks.F6Failures(tableRows, restored)
            .Concat(RestoredLicenceChecks.F6AcceptanceFailures(RestoredBody(page), tableRows, restored))
            .ToList();

        // Then the outcome is the row's
        Assert.True(OutcomeMatches422(row.Expected, failures), id + ": expected " + row.Expected + ", got " + string.Join(" | ", failures));
    }

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-2")]
    [MemberData(nameof(F6HoldingIds422))]
    public void F6_rows_422fix(string id) => SettleF6Row422(id);

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-2")]
    [MemberData(nameof(F6PendingIds422))]
    public void F6_rows_422fix_pending(string id) => SettleF6Row422(id);

    private static void SettleClassifyRow422(string id)
    {
        // Given the output of a locked restore (exit code, time-out, text and the package libraries counted from project.assets.json)
        var row = ClassifyRows422()[id];

        // When ClassifyRestore reads it (RestoreLockedOrFail needs pass; F5's planted step needs mismatch)
        var outcome = RestoredLicenceChecks.ClassifyRestore(row.ExitCode, row.TimedOut, row.Text, row.Count);

        // Then the outcome is the row's
        Assert.True(row.Expected == outcome, id + " (" + row.Kind + "): expected " + row.Expected + ", got " + outcome);
    }

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-6")]
    [MemberData(nameof(ClassifyHoldingIds422))]
    public void Classify_rows_422fix(string id) => SettleClassifyRow422(id);

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-6")]
    [MemberData(nameof(ClassifyPendingIds422))]
    public void Classify_rows_422fix_pending(string id) => SettleClassifyRow422(id);

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-6")]
    [MemberData(nameof(CountIds422))]
    public void Count_rows_422fix(string id)
    {
        // Given the text of a project.assets.json (or none)
        var (expected, assets) = CountRows422[id];

        // When the restored package libraries are counted
        var count = RestoredLicenceChecks.RestoredCount(assets);

        // Then only libraries of type package count
        Assert.True(expected == count, id + ": expected " + expected + ", got " + count);
    }

    // The scan reads a method by its signature: up to the matching brace, or to the semicolon of an expression body.
    private static string? MethodBody422(string source, string signature)
    {
        var at = source.IndexOf(signature, StringComparison.Ordinal);
        if (at < 0)
            return null;
        var start = source.IndexOf('{', at);
        var semicolon = source.IndexOf(';', at);
        if (start < 0 || (source[at..start].Contains("=>", StringComparison.Ordinal) && semicolon >= 0 && semicolon < start))
            return semicolon < 0 ? null : source[at..(semicolon + 1)];
        var depth = 0;
        for (var i = start; i < source.Length; i++)
        {
            if (source[i] == '{')
                depth++;
            else if (source[i] == '}' && --depth == 0)
                return source[at..(i + 1)];
        }

        return null;
    }

    private static void SettleWireRow422(string id)
    {
        // Given the two C# files the networked tests and the pure functions live in
        var rule = WireRows422[id].Rule;
        var testSource = File.ReadAllText(RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "RestoredPackageLicencesLockedRestore.cs"));
        var checksSource = File.ReadAllText(RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "RestoredLicenceChecks.cs"));
        var f5 = MethodBody422(testSource, "Task F5_locked_restore_matches_the_lockfiles_and_a_planted_mismatch_is_" + "NU1004(");
        var f6 = MethodBody422(testSource, "Task F6_restore_set_matches_the_page_and_the_" + "packaged_licences(");
        var locked = MethodBody422(testSource, "static async Task " + "RestoreLockedOrFail(");
        var read = MethodBody422(testSource, "static List<RestoredPackage> " + "ReadRestoredPackages(");
        var f6Failures = MethodBody422(checksSource, "IReadOnlyList<string> " + "F6Failures(");
        var classify = MethodBody422(checksSource, "static string " + "ClassifyRestore(");
        static bool Has(string? body, params string[] needles) => body is not null && needles.All(n => body.Contains(n, StringComparison.Ordinal));
        static bool Lacks(string? body, params string[] needles) => body is not null && !needles.Any(n => body.Contains(n, StringComparison.Ordinal));

        // When the rule for the row is applied
        var held = rule switch
        {
            "f6-calls" => Has(f6, "RestoredLicenceChecks.F6Failures(", "RestoredLicenceChecks.F6AcceptanceFailures(", "ReadRestoredPackages(")
                && Lacks(f6, "HasLicenceFile(", "LicenseExpression(", "LicenseUrl(", "RequiresAcceptance(", "Assert.Equal("),
            "read-packages" => Has(read, "RestoredLicenceChecks.RestoredPackageFrom("),
            "f6failures-value" => Has(f6Failures, "LicenceValueFailures("),
            "f5-classify" => Has(f5, "RestoredLicenceChecks.ClassifyRestore(", "\"mismatch\"", "RestoredCount(")
                && Lacks(f5, "Network" + "Missing(", "\"NU1004\""),
            "locked-classify" => Has(locked, "RestoredLicenceChecks.ClassifyRestore(", "\"pass\"", "RestoredCount(")
                && Lacks(locked, "Network" + "Missing("),
            "classify-outcome" => Has(classify, "RestoreOutcome("),
            "no-networkmissing" => !testSource.Contains("Network" + "Missing", StringComparison.Ordinal),
            _ => throw new InvalidOperationException(id + ": unknown rule " + rule),
        };

        // Then the wiring holds
        Assert.True(held, id + ": wiring " + rule + " is not in place");
    }

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-2")]
    [MemberData(nameof(WireHolding2Ids422))]
    public void Wiring_rows_422fix_f6(string id) => SettleWireRow422(id);

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-2")]
    [MemberData(nameof(WirePending2Ids422))]
    public void Wiring_rows_422fix_f6_pending(string id) => SettleWireRow422(id);

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-6")]
    [MemberData(nameof(WireHolding6Ids422))]
    public void Wiring_rows_422fix_restore(string id) => SettleWireRow422(id);

    [Theory]
    [Trait("Slice", "422")]
    [Trait("Criterion", "422-6")]
    [MemberData(nameof(WirePending6Ids422))]
    public void Wiring_rows_422fix_restore_pending(string id) => SettleWireRow422(id);

    private static async Task RestoreLockedOrFail(string projectPath)
    {
        var result = await RestoreLockedAsync(projectPath);
        var text = PublicText(result.Stdout + result.Stderr);
        var outcome = RestoredLicenceChecks.ClassifyRestore(result.ExitCode, result.TimedOut, text, RestoredLicenceChecks.RestoredCount(ReadAssets(projectPath)));
        if (outcome == "network-missing")
            Assert.Fail("NETWORK MISSING " + FirstLine(text));
        Assert.True(outcome == "pass", "locked restore: " + outcome + ": " + text);
    }

    // The project's project.assets.json text, or null when the restore wrote none (TempWorkspace.CopyFrom skips obj, so it is never stale).
    private static string? ReadAssets(string projectPath)
    {
        var path = Path.Combine(Path.GetDirectoryName(projectPath)!, "obj", "project.assets.json");
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    private static List<RestoredPackage> ReadRestoredPackages(JsonElement assets)
    {
        var packages = new List<RestoredPackage>();
        foreach (var (id, version) in AssetsPackages(assets))
        {
            var dir = FindPackageDir(assets, id, version);
            string? nuspecText = null;
            var paths = new List<string>();
            if (dir is not null)
            {
                var nuspecs = Directory.GetFiles(dir, "*.nuspec", SearchOption.TopDirectoryOnly);
                nuspecText = nuspecs.Length == 0 ? null : File.ReadAllText(nuspecs[0]);
                paths.AddRange(Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Select(file => Path.GetRelativePath(dir, file)));
            }

            packages.Add(RestoredLicenceChecks.RestoredPackageFrom(id, version, dir is not null, nuspecText, paths, rel => ReadHead(Path.Combine(dir!, rel))));
        }

        return packages;
    }

    private static string ReadHead(string path)
    {
        using var reader = new StreamReader(path, detectEncodingFromByteOrderMarks: true);
        var buffer = new char[600];
        var read = reader.ReadBlock(buffer, 0, buffer.Length);
        return new string(buffer, 0, read);
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
        // The project asks for FastEndpoints directly, so a lockfile without that DIRECT entry disagrees with the project: NU1004.
        // (A missing transitive entry fails the restore too, but with NU1102, not NU1004: found by the green 1 seat on SDK 10.0.403.)
        Assert.True(deps.Remove("FastEndpoints"), "could not plant a lockfile mismatch: no direct FastEndpoints entry");
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
            var normalized = line;
            var leadingSpaces = normalized.TakeWhile(ch => ch == ' ').Count();
            if (leadingSpaces is > 0 and <= 3)
                normalized = normalized[leadingSpaces..];
            if (!normalized.StartsWith('|'))
                continue;
            var cells = normalized.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray();
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
