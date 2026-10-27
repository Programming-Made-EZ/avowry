using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;
using Phrase.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: A declared effect becomes an injectable dependency
//   As Dana, I want a declared Db effect to become an injectable C# dependency
//   that a generated spec replaces, so that I can verify effectful service behavior
//   without calling real infrastructure.
public class DeclaredEffectDependencyCompiles
{
    [Fact]
    [Trait("Slice", "047")]
    public async Task Declared_effect_is_injected_and_its_spec_supplies_a_double()
    {
        // Given a phrase that declares one effect and observes the supplied dependency
        const string source = """
            module Orders

            phrase "database is available" -> Bool
              effects Db
              spec "uses a database double"
                when called
                then returns true
              native csharp
                return db is not null;
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the API exposes the declared effect as an interface dependency
        var phrases = File.ReadAllText(workspace.FindSingle("Phrases.cs"));
        Assert.Contains("public interface IDb", phrases, StringComparison.Ordinal);
        Assert.Contains("DatabaseIsAvailable(IDb db)", phrases, StringComparison.Ordinal);

        // And the generated spec supplies a test double and passes
        var specs = File.ReadAllText(workspace.FindSingle("PhraseSpecs.cs"));
        Assert.Contains("sealed class DbDouble : IDb", specs, StringComparison.Ordinal);
        Assert.Contains("Phrases.DatabaseIsAvailable(new DbDouble())", specs, StringComparison.Ordinal);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
