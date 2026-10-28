using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: phrase build compiles one file into output projects
//   As Ari, I want phrase build on one .phrase file to write the API and Specs
//   projects to the requested output and exit successfully, so that I can
//   produce a runnable, testable program with one command.
public class PhraseBuildOneFile
{
    [Fact]
    [Trait("Slice", "051")]
    public async Task Orders_phrase_build_writes_api_and_specs_projects_and_exits_zero()
    {
        // Given Ari's orders.phrase fixture and an empty output folder
        var source = RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.phrase");
        using var workspace = TempWorkspace.Create();

        // When phrase build runs on that file with -o
        var result = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);

        // Then the command exits 0 and both project folders exist with their project files
        Assert.True(result.ExitCode == 0, result.Text);
        var api = workspace.FindSingle("*.Api.csproj");
        var specs = workspace.FindSingle("*.Specs.csproj");
        Assert.True(Directory.Exists(Path.GetDirectoryName(api)));
        Assert.True(Directory.Exists(Path.GetDirectoryName(specs)));
    }

    [Fact]
    [Trait("Slice", "051")]
    public async Task Missing_phrase_file_exits_two_and_names_the_path()
    {
        // Given a path to a .phrase file that is not on disk
        using var workspace = TempWorkspace.Create();
        var missing = Path.Combine(workspace.Root, "missing.phrase");
        var output = Path.Combine(workspace.Root, "out");

        // When phrase build is asked to compile that path
        var result = await PhraseCli.RunAsync("build", missing, "-o", output);

        // Then it exits 2 and stderr is one line that names the path
        Assert.True(result.ExitCode == 2, result.Text);
        var lines = result.Stderr.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines);
        Assert.Contains(missing, lines[0], StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "051")]
    public async Task Compile_error_exits_one_and_prints_the_diagnostic()
    {
        // Given a phrase file whose call does not match any phrase
        using var workspace = TempWorkspace.Create();
        var source = Path.Combine(workspace.Root, "broken.phrase");
        await File.WriteAllTextAsync(source, """
            module Test

            phrase "known phrase"
              native csharp
                return;

            endpoint Probe : POST /probe
              input
                request: Text
              steps
                mistyped phrase
            """);
        var output = Path.Combine(workspace.Root, "out");

        // When phrase build compiles it
        var result = await PhraseCli.RunAsync("build", source, "-o", output);

        // Then it exits 1 and prints the current diagnostic text
        Assert.True(result.ExitCode == 1, result.Text);
        Assert.Contains("PH001 (", result.Text, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "051")]
    public async Task Build_output_matches_the_compiler_bytes()
    {
        // Given orders.phrase and the files PhraseCompiler.Compile plus WriteTo produce
        var sourcePath = RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.phrase");
        var expectedOutput = PhraseCompiler.Compile(await File.ReadAllTextAsync(sourcePath), new CSharpProvider(), sourcePath);
        using var expected = TempWorkspace.Create();
        expectedOutput.WriteTo(expected.Root);
        using var actual = TempWorkspace.Create();

        // When the same file is built through the CLI, including an explicit csharp target
        var result = await PhraseCli.RunAsync("build", sourcePath, "-o", actual.Root, "--target", "csharp");

        // Then every output path and its bytes match the compiler
        Assert.True(result.ExitCode == 0, result.Text);
        var expectedFiles = Snapshot(expected.Root);
        var actualFiles = Snapshot(actual.Root);
        Assert.Equal(expectedFiles.Keys.OrderBy(key => key, StringComparer.Ordinal), actualFiles.Keys.OrderBy(key => key, StringComparer.Ordinal));
        foreach (var (relative, bytes) in expectedFiles)
            Assert.Equal(bytes, actualFiles[relative]);
    }

    [Fact]
    [Trait("Slice", "051")]
    public async Task Missing_output_directory_exits_two_with_usage()
    {
        // Given a real phrase file and no -o output directory
        var source = RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.phrase");

        // When phrase build is run without -o
        var result = await PhraseCli.RunAsync("build", source);

        // Then it exits 2 and prints usage
        Assert.True(result.ExitCode == 2, result.Text);
        Assert.Contains("Usage:", result.Stderr, StringComparison.Ordinal);
        Assert.Contains("phrase build", result.Stderr, StringComparison.Ordinal);
        Assert.Contains("-o", result.Stderr, StringComparison.Ordinal);
    }

    private static Dictionary<string, byte[]> Snapshot(string root)
    {
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            files.Add(Path.GetRelativePath(root, file), File.ReadAllBytes(file));
        return files;
    }
}
