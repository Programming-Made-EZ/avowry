using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: trace never trusts a stale or unsafe map record
//   As Morgan, I want trace to report a generated file that has gone missing and to refuse a csFile
//   that leaves the output folder, so that a stale map is never trusted quietly.
// Card 172 criterion 1. Each test plants a small map in a temporary folder and runs the real CLI (as SourceMapTraceCommand does).
public class TraceGuards172
{
    private const string RecordName = "PlantedStep";
    private const string GeneratedText = "// planted generated file\nnamespace Planted;\n";

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-1")]
    public async Task Trace_of_a_deleted_generated_file_warns_that_it_is_missing()
    {
        // Given a map record whose generated file no longer exists
        using var workspace = TempWorkspace.Create();
        var map = Plant(workspace, "Gen/Planted.cs", create: false);

        // When Morgan traces the record
        var trace = await PhraseCli.RunAsync("trace", RecordName, "--map", map);

        // Then trace warns, naming the missing file, instead of treating the hash as matching
        Assert.True(trace.ExitCode == 0, trace.Text);
        Assert.Contains("WARNING", trace.Text, StringComparison.Ordinal);
        Assert.Contains("missing", trace.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Gen/Planted.cs", trace.Text, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-1")]
    public async Task Strict_trace_of_a_deleted_generated_file_exits_one()
    {
        // Given a map record whose generated file no longer exists
        using var workspace = TempWorkspace.Create();
        var map = Plant(workspace, "Gen/Planted.cs", create: false);

        // When Morgan traces the record strictly
        var trace = await PhraseCli.RunAsync("trace", RecordName, "--map", map, "--strict");

        // Then the missing file is still named and the command fails like a hash mismatch does
        Assert.Equal(1, trace.ExitCode);
        Assert.Contains("missing", trace.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Gen/Planted.cs", trace.Text, StringComparison.Ordinal);
    }

    [Theory]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-1")]
    [InlineData("dotdot")]
    [InlineData("nested-dotdot")]
    [InlineData("backslash-dotdot")]
    [InlineData("absolute")]
    [InlineData("leading-slash")]
    [InlineData("leading-backslash")]
    [InlineData("drive-letter")]
    [InlineData("other-record")]
    public async Task Trace_refuses_a_csFile_that_leaves_the_output_folder(string form)
    {
        // Given a map whose record points outside the folder that holds the map; the outside file really exists
        // with a matching hash for the forms that can resolve, so only a guard before any read can refuse it
        using var workspace = TempWorkspace.Create();
        var outside = Path.Combine(workspace.Root, "outside.cs");
        await File.WriteAllTextAsync(outside, GeneratedText);
        var csFile = form switch
        {
            "dotdot" => "../outside.cs",
            "nested-dotdot" => "Gen/../../outside.cs",
            "backslash-dotdot" => "..\\outside.cs",
            "absolute" => outside,
            "leading-slash" => "/outside.cs",
            "leading-backslash" => "\\outside.cs",
            "drive-letter" => "C:\\outside.cs",
            _ => "../outside.cs",
        };
        var otherBad = form == "other-record";
        var map = Plant(workspace, csFile, create: false, mapFolder: "maps", otherBadRecord: otherBad);

        // When Morgan traces the record, with and without --strict
        var trace = await PhraseCli.RunAsync("trace", RecordName, "--map", map);
        var strict = await PhraseCli.RunAsync("trace", RecordName, "--map", map, "--strict");

        // Then trace refuses with exit 2, names the record and the csFile problem, and prints no record
        foreach (var run in new[] { trace, strict })
        {
            Assert.Equal(2, run.ExitCode);
            Assert.Contains(otherBad ? "OtherStep" : RecordName, run.Stderr, StringComparison.Ordinal);
            Assert.Contains("csFile", run.Stderr, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Unhandled exception", run.Text, StringComparison.OrdinalIgnoreCase);
            Assert.True(string.IsNullOrWhiteSpace(run.Stdout), run.Stdout);
        }
    }

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-1")]
    public async Task Trace_still_accepts_a_present_unchanged_file_and_a_name_that_only_contains_two_dots()
    {
        // Given two maps whose generated files exist and match: one plain, one with two dots inside folder and file names
        using var plain = TempWorkspace.Create();
        using var dotted = TempWorkspace.Create();
        var plainMap = Plant(plain, "Gen/Planted.cs", create: true);
        var dottedMap = Plant(dotted, "Gen..v2/Planted..cs", create: true);

        // When Morgan traces each strictly
        var first = await PhraseCli.RunAsync("trace", RecordName, "--map", plainMap, "--strict");
        var second = await PhraseCli.RunAsync("trace", RecordName, "--map", dottedMap, "--strict");

        // Then neither is refused or warned about (the .. test is on whole path segments, not on substrings)
        foreach (var run in new[] { first, second })
        {
            Assert.True(run.ExitCode == 0, run.Text);
            Assert.DoesNotContain("WARNING: generated C#", run.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("missing", run.Text, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string Plant(
        TempWorkspace workspace, string csFile, bool create, string mapFolder = "", bool otherBadRecord = false)
    {
        var folder = Path.Combine(workspace.Root, mapFolder);
        Directory.CreateDirectory(folder);
        if (create)
        {
            var generated = Path.Combine(folder, csFile.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(generated)!);
            File.WriteAllText(generated, GeneratedText);
        }
        var records = new JsonArray { Record(RecordName, otherBadRecord ? "Gen/Planted.cs" : csFile) };
        if (otherBadRecord)
            records.Add(Record("OtherStep", csFile));
        var root = new JsonObject { ["schemaVersion"] = 1, ["buildId"] = "planted", ["records"] = records };
        var map = Path.Combine(folder, "Planted.avowrymap.json");
        File.WriteAllText(map, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return map;
    }

    private static JsonObject Record(string name, string csFile) => new()
    {
        ["stepId"] = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..12].ToLowerInvariant(),
        ["kind"] = "failure",
        ["name"] = name,
        ["sourceFile"] = "Planted.avowry",
        ["line"] = 1,
        ["endpoint"] = "Get",
        ["step"] = "fail Planted",
        ["specLines"] = new JsonObject { ["first"] = 1, ["last"] = 2 },
        ["csFile"] = csFile,
        ["csMethod"] = "Planted.Run",
        ["csHash"] = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(GeneratedText))).ToLowerInvariant(),
    };
}
