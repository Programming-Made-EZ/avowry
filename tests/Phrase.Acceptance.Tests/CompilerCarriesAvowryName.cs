using Avowry.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: The compiler carries the Avowry product name
//   As Morgan, I want the compiler assembly and namespace to carry the product name,
//   so that the packed tool shows one name.
public class CompilerCarriesAvowryName
{
    [Fact]
    [Trait("Slice", "194")]
    public void Compiler_assembly_and_namespace_carry_the_Avowry_name()
    {
        // Given a public compiler type
        var compilerType = typeof(AvowryCompiler);

        // When its assembly and namespace identify their owners
        var assemblyName = compilerType.Assembly.GetName().Name;
        var compilerNamespace = compilerType.Namespace;

        // Then both identify the compiler as Avowry
        Assert.Equal("Avowry.Compiler", assemblyName);
        Assert.StartsWith("Avowry.Compiler", compilerNamespace, StringComparison.Ordinal);
    }
}
