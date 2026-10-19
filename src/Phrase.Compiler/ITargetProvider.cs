namespace Phrase.Compiler;

/// <summary>
/// A target provider turns the language-neutral IR into files for one target
/// (C#, TypeScript, Python). Deliberately thin for now: the Emit(IR) member is
/// added once the IR shape has been driven out by tests.
/// </summary>
public interface ITargetProvider
{
    /// <summary>The name used in `native &lt;target&gt;` blocks, e.g. "csharp".</summary>
    string Target { get; }
}
