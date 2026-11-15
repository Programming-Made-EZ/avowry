using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
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
        Assert.True(restored.SetEquals(locked), DescribeSet("locked restore vs lockfiles", locked, restored));

        // And a planted lockfile mismatch restores with a non-zero exit and NU1004
        using var drift = TempWorkspace.Create();
        drift.CopyFrom(RepoPaths.Find("reference", "csharp"));
        var plantedLock = Path.Combine(drift.Root, "Orders.Api", "packages.lock.json");
        PlantLockfileMismatch(plantedLock);
        var planted = await RestoreLockedAsync(Path.Combine(drift.Root, "Orders.Api", "Orders.Api.csproj"));
        var plantedText = PublicText(planted.Stdout + planted.Stderr);
        Assert.False(planted.TimedOut, "restore exceeded 5 minutes");
        if (NetworkMissing(plantedText))
            Assert.Fail("NETWORK MISSING " + FirstLine(plantedText));
        Assert.NotEqual(0, planted.ExitCode);
        Assert.Contains("NU1004", plantedText, StringComparison.OrdinalIgnoreCase);
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
        var restored = AssetsPackages(assets.RootElement);
        var pageText = (await File.ReadAllTextAsync(RepoPaths.Find("docs", "release", "pre-publication-checks.md")))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        var pageRows = ReadRestoredRows(pageText);

        // Then the restore set equals the page rows, and nuspec licences, licence files and the acceptance set match
        var pageSet = pageRows.Select(r => (r.Id, r.Version)).ToHashSet();
        Assert.True(restored.SetEquals(pageSet), DescribeSet("locked restore vs page", pageSet, restored));
        var acceptanceTrue = new HashSet<(string Id, string Version)>();
        foreach (var row in pageRows)
        {
            var dir = FindPackageDir(assets.RootElement, row.Id, row.Version);
            Assert.True(dir is not null, row.Id + " " + row.Version + ": package folder missing after restore");
            var nuspec = LoadNuspec(dir!);
            Assert.True(nuspec is not null, row.Id + " " + row.Version + ": nuspec missing after restore");
            if (RequiresAcceptance(nuspec!))
                acceptanceTrue.Add((row.Id, row.Version));
            if (row.LicenceSource == "nuspec licence expression")
            {
                var expression = LicenseExpression(nuspec!);
                Assert.Equal(expression, row.Licence);
            }
            else if (row.LicenceSource == "packaged licence file")
            {
                Assert.True(HasLicenceFile(dir!), row.Id + " " + row.Version + ": licence source is packaged licence file but no licence file is in the package");
            }
            else if (row.LicenceSource == "licence URL")
            {
                Assert.False(string.IsNullOrEmpty(LicenseUrl(nuspec!)), row.Id + " " + row.Version + ": licence source is licence URL but the nuspec has no licenseUrl");
            }
            else
            {
                Assert.Fail(row.Id + " " + row.Version + ": licence source is not one of the three labels");
            }
        }

        var body = RestoredBody(pageText);
        var namedAcceptance = new HashSet<(string Id, string Version)>();
        foreach (var line in body.Split('\n'))
        {
            if (line.IndexOf("acceptance", StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            foreach (var pkg in restored)
            {
                if (line.Contains(pkg.Id, StringComparison.Ordinal) && line.Contains(pkg.Version, StringComparison.Ordinal))
                    namedAcceptance.Add(pkg);
            }
        }
        Assert.True(namedAcceptance.SetEquals(acceptanceTrue), DescribeSet("acceptance set", acceptanceTrue, namedAcceptance));
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

    private static async Task RestoreLockedOrFail(string projectPath)
    {
        var result = await RestoreLockedAsync(projectPath);
        var text = PublicText(result.Stdout + result.Stderr);
        Assert.False(result.TimedOut, "restore exceeded 5 minutes");
        if (NetworkMissing(text))
            Assert.Fail("NETWORK MISSING " + FirstLine(text));
        Assert.True(result.ExitCode == 0, text);
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

    private static XDocument? LoadNuspec(string packageDir)
    {
        var files = Directory.GetFiles(packageDir, "*.nuspec", SearchOption.TopDirectoryOnly);
        return files.Length == 0 ? null : XDocument.Load(files[0]);
    }

    private static string? LicenseExpression(XDocument doc)
    {
        var el = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "license");
        if (el is null)
            return null;
        var type = (string?)el.Attribute("type");
        return string.Equals(type, "expression", StringComparison.OrdinalIgnoreCase) ? el.Value.Trim() : null;
    }

    private static string? LicenseUrl(XDocument doc) =>
        doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "licenseUrl")?.Value.Trim();

    private static bool RequiresAcceptance(XDocument doc)
    {
        var el = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "requireLicenseAcceptance");
        return el is not null && string.Equals(el.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasLicenceFile(string packageDir)
    {
        foreach (var file in Directory.EnumerateFiles(packageDir, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(file);
            if (name.StartsWith("license", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("licence", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("copying", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static bool NetworkMissing(string text) =>
        text.Contains("NU1301", StringComparison.OrdinalIgnoreCase)
        || text.Contains("Unable to load the service index", StringComparison.OrdinalIgnoreCase)
        || text.Contains("No such host is known", StringComparison.OrdinalIgnoreCase)
        || text.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
        || text.Contains("nodename nor servname provided", StringComparison.OrdinalIgnoreCase)
        || text.Contains("The remote name could not be resolved", StringComparison.OrdinalIgnoreCase);

    private static string DescribeSet(string label, HashSet<(string Id, string Version)> expected, HashSet<(string Id, string Version)> actual)
    {
        var missing = string.Join(", ", expected.Except(actual).OrderBy(p => p.Id).Select(p => p.Id + " " + p.Version));
        var extra = string.Join(", ", actual.Except(expected).OrderBy(p => p.Id).Select(p => p.Id + " " + p.Version));
        return label + " missing: " + missing + "; extra: " + extra;
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
