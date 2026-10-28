using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: Generated C# points back to its Phrase source
//   As Morgan, I want generated files and methods to name their Phrase declaration,
//   so that I can trace production behavior without reverse-engineering generated C#.
public class SourceTraceabilityCompiles
{
    [Fact]
    [Trait("Slice", "161")]
    public void In_memory_source_uses_the_module_filename_and_declaration_lines()
    {
        // Given Phrase source compiled from memory without a filesystem path
        const string source = """
            module Notes

            phrase "record {value: Int}"
              native csharp
                return;
            """;

        // When the C# adapter emits the module
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var phrases = File.ReadAllText(workspace.FindSingle("Phrases.cs"));

        // Then the module name is the portable fallback and the phrase line is retained
        Assert.StartsWith("// phrase: Notes.phrase:1 module Notes", phrases, StringComparison.Ordinal);
        Assert.Contains("// phrase: Notes.phrase:3 phrase record {value: Int}", phrases, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Generated_files_and_methods_carry_stable_source_comments()
    {
        // Given the Tasks demo at its named source path
        var sourcePath = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        using var first = TempWorkspace.Create();
        using var second = TempWorkspace.Create();

        // When the unchanged source is built twice
        var firstBuild = await PhraseCli.RunAsync("build", sourcePath, "-o", first.Root);
        var secondBuild = await PhraseCli.RunAsync("build", sourcePath, "-o", second.Root);

        // Then every generated C# file names the portable Phrase source and declaration line
        Assert.True(firstBuild.ExitCode == 0, firstBuild.Text);
        Assert.True(secondBuild.ExitCode == 0, secondBuild.Text);
        var firstFiles = Directory.GetFiles(first.Root, "*.cs", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(first.Root, path), File.ReadAllText, StringComparer.Ordinal);
        var secondFiles = Directory.GetFiles(second.Root, "*.cs", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(second.Root, path), File.ReadAllText, StringComparer.Ordinal);
        Assert.NotEmpty(firstFiles);
        Assert.All(firstFiles.Values, text => Assert.Contains("// phrase: Tasks.phrase:", text, StringComparison.Ordinal));
        Assert.Equal(firstFiles, secondFiles);

        // And endpoint and phrase methods point to their own declarations
        var phrases = firstFiles.Single(file => file.Key.EndsWith("Phrases.cs", StringComparison.Ordinal)).Value;
        var create = firstFiles.Single(file => file.Key.EndsWith("CreateTask.cs.cs", StringComparison.Ordinal)).Value;
        Assert.Contains("// phrase: Tasks.phrase:17 phrase validate task title {title: Text}", phrases, StringComparison.Ordinal);
        Assert.Contains("// phrase: Tasks.phrase:52 endpoint CreateTask", create, StringComparison.Ordinal);
        Assert.Equal(2, CountOf(create, "// phrase: Tasks.phrase:52 endpoint CreateTask"));
    }

    private static int CountOf(string text, string value) =>
        (text.Length - text.Replace(value, string.Empty, StringComparison.Ordinal).Length) / value.Length;
}
