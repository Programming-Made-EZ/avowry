using Phrase.Compiler.Semantics;

namespace Phrase.Compiler;

/// <summary>
/// A target adapter turns the resolved module into files for one target
/// (C#, TypeScript, Python).
/// </summary>
public interface ITargetAdapter
{
    /// <summary>The name used in `native &lt;target&gt;` blocks, e.g. "csharp".</summary>
    string Target { get; }

    /// <summary>Turns the resolved module into files for this target.</summary>
    CompilationOutput Emit(ModuleModel module);
}
