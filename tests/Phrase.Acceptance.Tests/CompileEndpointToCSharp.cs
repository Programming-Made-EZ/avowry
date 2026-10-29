using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;
using Phrase.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: Compile a .avowry endpoint to C#
//   As an engineer (or an LLM) writing behavior as phrases
//   I want the C# provider to emit a FastEndpoints API and xUnit specs
//   So that every spec in the .avowry source runs as a real test in the target
//
// This is the end-to-end acceptance test for the C# emitter: it compiles the
// orders.avowry fixture, writes both generated projects, and verifies every
// generated spec passes in the target framework.
public class CompileEndpointToCSharp
{
    [Fact]
    [Trait("Slice", "048")]
    public async Task Generated_post_endpoint_binds_its_request_and_returns_its_success_response()
    {
        // Given a .avowry file describing one POST endpoint, its request record,
        // and a success response with a passing handler spec
        var source = Fixture.Read("orders.avowry");
        var expectedSpecCount = Fixture.CountSpecs(source);

        // When it is compiled with the C# provider and written to disk
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the generated API project and the generated specs project build
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);

        // And every source spec runs as a real xUnit test
        Assert.Equal(expectedSpecCount, result.Total);

        // And the success response is served in process by the generated handler
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "048")]
    public async Task Zero_route_module_still_emits_a_buildable_program()
    {
        // Given a module with no endpoint declarations but with phrase specs
        const string source = """
            module Numbers

            avow to "double {n: Int}" -> Int
              spec "doubles two"
                given n = 2
                when called
                then returns 4
              native csharp
                return n * 2;
            """;

        // When it is compiled with the C# provider and written to disk
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the API project still contains the startup Program.cs and the generated specs build
        Assert.Contains("Program.cs", Directory.GetFiles(workspace.Root, "Program.cs", SearchOption.AllDirectories).Select(Path.GetFileName));
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
