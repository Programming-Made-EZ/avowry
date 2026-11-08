using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: packaging keeps the Tasks demo's generated C#
//   As Dana, I want the packed tool to emit the same C# the demo already emits,
//   so that installing from a feed does not change the compiled program.
public class TasksDemoGeneratedCsharp
{
    [Fact]
    [Trait("Slice", "176")]
    [Trait("Criterion", "3")]
    public async Task Tasks_demo_generated_csharp_matches_the_golden_file()
    {
        // Given the Tasks demo and the committed golden snapshot of its generated C#
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var goldenPath = RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "TasksDemoGeneratedCsharp.golden");
        var golden = Normalize(await File.ReadAllTextAsync(goldenPath));
        using var workspace = TempWorkspace.Create();

        // When avowry build compiles the demo
        var result = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);

        // Then every generated C# file matches the golden bytes
        Assert.True(result.ExitCode == 0, result.Text);
        Assert.Equal(golden, Snapshot(workspace.Root));
    }

    private static string Snapshot(string root)
    {
        var parts = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => (path, rel: Path.GetRelativePath(root, path).Replace('\\', '/')))
            .Where(file => file.rel.EndsWith(".cs", StringComparison.Ordinal)
                && !file.rel.Split('/').Any(part => part is "bin" or "obj"))
            .OrderBy(file => file.rel, StringComparer.Ordinal)
            .Select(file =>
            {
                var text = Normalize(File.ReadAllText(file.path));
                if (!text.EndsWith('\n'))
                    text += "\n";
                return "### " + file.rel + "\n" + text;
            });
        return string.Concat(parts);
    }

    private static string Normalize(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
}
