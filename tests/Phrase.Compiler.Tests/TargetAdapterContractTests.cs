using Avowry.Compiler;
using Avowry.Compiler.Semantics;

namespace Phrase.Compiler.Tests;

// Feature: Handing a resolved phrase model to a target adapter
//   AvowryCompiler owns parsing and resolution without knowing any target emitter.
public class TargetAdapterContractTests
{
    [Fact]
    [Trait("Slice", "040")]
    public void Resolved_model_is_handed_to_the_selected_adapter_without_CSharp_knowledge()
    {
        // Given a substitute adapter and a valid one-number phrase
        var expectedOutput = new CompilationOutput(new Dictionary<string, string>
        {
            ["adapter-result.txt"] = "received",
        });
        var adapter = new SubstituteAdapter(expectedOutput);
        const string source = """
            module Numbers

            avow to "double {n: Int}" -> Int
              native csharp
                return n * 2;
            """;

        // When AvowryCompiler compiles for that adapter
        var output = AvowryCompiler.Compile(source, adapter);

        // Then it hands over the resolved model and returns the adapter's output
        Assert.Same(expectedOutput, output);
        var module = Assert.IsType<ModuleModel>(adapter.ReceivedModule);
        Assert.Equal("Numbers", module.Name);
        Assert.Equal("double {n: Int}", Assert.Single(module.Avows).Template);

        // And the compiler project has no C# adapter or emitter reference
        var project = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Avowry.Compiler.csproj"));
        Assert.DoesNotContain("Phrase.Adapters.CSharp", project, StringComparison.Ordinal);
        Assert.DoesNotContain("CSharpEmitter", project, StringComparison.Ordinal);
    }

    private sealed class SubstituteAdapter(CompilationOutput output) : ITargetAdapter
    {
        public string Target => "substitute";

        public ModuleModel? ReceivedModule { get; private set; }

        public CompilationOutput Emit(ModuleModel module)
        {
            ReceivedModule = module;
            return output;
        }
    }
}
