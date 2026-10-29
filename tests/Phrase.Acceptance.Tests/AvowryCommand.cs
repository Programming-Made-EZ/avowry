using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: The command and its source map use the public Avowry name
//   As Dana, I want to type avowry and see Avowry in every message the tool prints,
//   so that nothing teaches me or a model the old name.
public class AvowryCommand
{
    [Fact]
    [Trait("Slice", "199")]
    public async Task Build_rejects_the_removed_phrase_extension_and_names_avowry()
    {
        // Given Dana has a source path with the removed extension
        using var workspace = TempWorkspace.Create();
        var source = Path.Combine(workspace.Root, "Work.phrase");
        await File.WriteAllTextAsync(source, "module Work\n");

        // When she asks the CLI to build it
        var result = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);

        // Then the command rejects it and names the only accepted extension
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains(".avowry", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "191")]
    public async Task Help_version_build_map_and_trace_use_only_the_Avowry_name()
    {
        // Given a valid Avowry source in a clean output workspace
        using var workspace = TempWorkspace.Create();
        var source = RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.avowry");

        // When Dana reads help and version, builds it, and traces from the resulting map
        var help = await PhraseCli.RunAsync("--help");
        var version = await PhraseCli.RunAsync("--version");
        var invalid = await PhraseCli.RunAsync("--bogus");
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        var map = Path.Combine(workspace.Root, "Orders.avowrymap.json");
        var trace = await PhraseCli.RunAsync("trace", "CreateOrder", "--map", map);

        // Then every visible command name is Avowry, only the Avowry map exists, and trace reads it
        Assert.Equal(0, help.ExitCode);
        Assert.Contains("Usage: avowry build", help.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("phrase build", help.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, version.ExitCode);
        Assert.StartsWith("Avowry ", version.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("Phrase", version.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, invalid.ExitCode);
        Assert.StartsWith("Usage: avowry build", invalid.Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("phrase:", invalid.Stderr, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, build.ExitCode);
        Assert.True(File.Exists(map), $"Expected map: {map}");
        Assert.False(File.Exists(Path.Combine(workspace.Root, "Orders.phrasemap.json")));
        Assert.Equal(0, trace.ExitCode);
        Assert.Contains("CreateOrder", trace.Stdout, StringComparison.Ordinal);

        // And the failure paths of build and trace also speak as avowry
        var missingSource = await PhraseCli.RunAsync("build", Path.Combine(workspace.Root, "Missing.avowry"), "-o", workspace.Root);
        var missingMap = await PhraseCli.RunAsync("trace", "CreateOrder", "--map", Path.Combine(workspace.Root, "Nothing.avowrymap.json"));
        var unknownName = await PhraseCli.RunAsync("trace", "NoSuchThing", "--map", map);
        Assert.Equal(2, missingSource.ExitCode);
        Assert.StartsWith("avowry: No such file", missingSource.Stderr, StringComparison.Ordinal);
        Assert.Equal(2, missingMap.ExitCode);
        Assert.StartsWith("avowry: Source map not found", missingMap.Stderr, StringComparison.Ordinal);
        Assert.Equal(1, unknownName.ExitCode);
        Assert.StartsWith("avowry: Unknown trace name", unknownName.Stderr, StringComparison.Ordinal);
    }
}
