using Phrase.Compiler;

namespace Phrase.Providers.CSharp;

/// <summary>
/// Emits a FastEndpoints API project plus an xUnit specs project.
/// The target output for orders.phrase is hand-written in reference/csharp.
/// </summary>
public sealed class CSharpProvider : ITargetProvider
{
    public string Target => "csharp";
}
