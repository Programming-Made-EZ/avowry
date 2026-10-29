using Phrase.Acceptance.Tests.Support;
using Avowry.Compiler;
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
    public async Task Two_phrases_sharing_an_effect_each_pass_with_one_generated_double()
    {
        // Given two effectful phrases whose specs share the declared Db effect
        const string source = """
            module Orders

            avow to "database is available" -> Bool
              effects Db
              spec "uses a database double"
                when called
                then returns true
              native csharp
                return db is not null;

            avow to "database is connected" -> Bool
              effects Db
              spec "also uses the database double"
                when called
                then returns true
              native csharp
                return db is not null;
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the shared effect has one double and both generated specs pass
        var specs = File.ReadAllText(workspace.FindSingle("AvowSpecs.cs"));
        Assert.Equal(1, specs.Split("sealed class DbDouble : IDb", StringSplitOptions.None).Length - 1);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(2, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "047")]
    public async Task Two_specs_sharing_an_effect_each_pass_with_one_generated_double()
    {
        // Given one effectful phrase with two specs that share the declared Db effect
        const string source = """
            module Orders

            avow to "database is available" -> Bool
              effects Db
              spec "uses a database double"
                when called
                then returns true
              spec "also uses the database double"
                when called
                then returns true
              native csharp
                return db is not null;
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the generated specs project builds and both specs pass
        var specs = File.ReadAllText(workspace.FindSingle("AvowSpecs.cs"));
        Assert.Equal(1, specs.Split("sealed class DbDouble : IDb", StringSplitOptions.None).Length - 1);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(2, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "047")]
    public async Task Declared_effect_is_injected_and_its_spec_supplies_a_double()
    {
        // Given a phrase that declares one effect and observes the supplied dependency
        const string source = """
            module Orders

            avow to "database is available" -> Bool
              effects Db
              spec "uses a database double"
                when called
                then returns true
              native csharp
                return db is not null;
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the API exposes the declared effect as an interface dependency
        var phrases = File.ReadAllText(workspace.FindSingle("Avows.cs"));
        Assert.Contains("public interface IDb", phrases, StringComparison.Ordinal);
        Assert.Contains("DatabaseIsAvailable(IDb db)", phrases, StringComparison.Ordinal);

        // And the generated spec supplies a test double and passes
        var specs = File.ReadAllText(workspace.FindSingle("AvowSpecs.cs"));
        Assert.Contains("sealed class DbDouble : IDb", specs, StringComparison.Ordinal);
        Assert.Contains("Avows.DatabaseIsAvailable(new DbDouble())", specs, StringComparison.Ordinal);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
