using System.Reflection;
using System.Text.Json;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: The adapter, CLI, and source map carry the Avowry product name
//   As Morgan, I want the tool artifact to contain no Phrase-named production unit,
//   so that the package presents one product name.
public class ProductNamedToolBoundary
{
    [Fact]
    [Trait("Slice", "195")]
    public async Task Built_tool_uses_Avowry_assembly_and_source_map_names()
    {
        // Given a valid Avowry source and an empty output directory
        using var workspace = TempWorkspace.Create();
        var source = RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.avowry");

        // When Morgan builds it through the real tool
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);

        // Then the production assemblies and emitted map use only the Avowry names
        Assert.True(build.ExitCode == 0, build.Text);
        var configuration = AppContext.BaseDirectory.Contains(
            $"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";
        var cliAssembly = RepoPaths.Find("src", "Avowry.Cli", "bin", configuration, "net8.0", "Avowry.Cli.dll");
        var adapterAssembly = RepoPaths.Find("src", "Avowry.Cli", "bin", configuration, "net8.0", "Avowry.Adapters.CSharp.dll");
        Assert.True(File.Exists(cliAssembly), $"Missing {cliAssembly}");
        Assert.True(File.Exists(adapterAssembly), $"Missing {adapterAssembly}");
        Assert.Equal("Avowry.Cli", AssemblyName.GetAssemblyName(cliAssembly).Name);
        Assert.Equal("Avowry.Adapters.CSharp", AssemblyName.GetAssemblyName(adapterAssembly).Name);

        using var map = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace.Root, "Orders.avowrymap.json")));
        var records = map.RootElement.GetProperty("records").EnumerateArray().ToArray();
        Assert.All(records, record =>
        {
            Assert.True(record.TryGetProperty("sourceFile", out _));
            Assert.False(record.TryGetProperty("phraseFile", out _));
        });
        Assert.Contains(records, record => record.GetProperty("kind").GetString() == "avow");
        Assert.DoesNotContain(records, record => record.GetProperty("kind").GetString() == "phrase");
    }
}
