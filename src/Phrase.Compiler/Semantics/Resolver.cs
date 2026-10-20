using Phrase.Compiler.Syntax;

namespace Phrase.Compiler.Semantics;

/// <summary>
/// Binds the syntax tree into the <see cref="ModuleModel"/>: every phrase call matched to
/// exactly one template, every value typed, every spec checked.
///
/// Not built yet. This is the mob's starting point: see HANDOFF.md, and make the
/// tests in tests/Phrase.Compiler.Tests/Semantics pass one at a time.
/// </summary>
public static class Resolver
{
    /// <summary>Parses then resolves. Parse diagnostics come first in the result.</summary>
    public static ResolveResult Resolve(string source) =>
        throw new NotImplementedException("Resolver not built yet. See HANDOFF.md.");

    /// <summary>Resolves an already-parsed file. Must never throw, even on a tree with syntax errors.</summary>
    public static ResolveResult Resolve(SourceFile file) =>
        throw new NotImplementedException("Resolver not built yet. See HANDOFF.md.");
}
