namespace Phrase.Compiler.Syntax;

// The syntax tree. Purely structural: phrase calls are NOT resolved here.
// A step keeps its words and arguments in source order; the resolver later
// matches them against phrase templates (GRAMMAR.md section 4).

/// <summary>1-based source position of a node's first token.</summary>
public readonly record struct Position(int Line, int Column);

public sealed record SourceFile(
    ModuleDecl? Module,
    IReadOnlyList<TypeDecl> Types,
    IReadOnlyList<PhraseDecl> Phrases,
    IReadOnlyList<EndpointDecl> Endpoints);

public sealed record ParseResult(SourceFile File, IReadOnlyList<Diagnostic> Diagnostics)
{
    /// <summary>Extensions this file loads. Always contains <c>core</c>. The same source always yields the same list.</summary>
    public IReadOnlyList<string> ActiveExtensions { get; init; } = ExtensionKeywords.CoreOnly;
}

// ---------- declarations ----------

public sealed record ModuleDecl(string Name, Position At);

public sealed record TypeDecl(string Name, IReadOnlyList<FieldDecl> Fields, Position At);

public sealed record FieldDecl(string Name, TypeRef Type, Position At);

/// <summary>A type reference such as <c>Int</c> or <c>List&lt;LineItem&gt;</c>.</summary>
public sealed record TypeRef(string Name, IReadOnlyList<TypeRef> Arguments, Position At);

public sealed record PhraseDecl(
    Template Template,
    TypeRef? Returns,
    IReadOnlyList<string> Effects,      // empty = `effects none` or not declared
    IReadOnlyList<string> Failures,     // from `fails with A, B`
    IReadOnlyList<SpecDecl> Specs,
    IReadOnlyList<NativeBlock> Natives, // leaf phrase: one per target
    IReadOnlyList<Step>? Steps,         // composite phrase: non-null instead of natives
    Position At);

/// <summary>A phrase name: words and typed holes, e.g. <c>total quantity of {items: List&lt;LineItem&gt;}</c>.</summary>
public sealed record Template(IReadOnlyList<TemplatePart> Parts, Position At);

public abstract record TemplatePart(Position At);
public sealed record TemplateWord(string Text, Position At) : TemplatePart(At);
public sealed record TemplateHole(string Name, TypeRef Type, Position At) : TemplatePart(At);

public sealed record NativeBlock(string Target, string Code, Position At);

public sealed record EndpointDecl(
    string Name,
    string Verb,
    string Route,
    IReadOnlyList<FieldDecl> Input,
    IReadOnlyList<FieldDecl> Output,
    IReadOnlyList<Step> Steps,
    IReadOnlyList<SpecDecl> Specs,
    Position At);

// ---------- steps ----------

public abstract record Step(Position At);

/// <summary><c>let name = &lt;phrase call&gt;</c></summary>
public sealed record LetStep(string Name, PhraseCall Call, Position At) : Step(At);

/// <summary><c>respond with { ... }</c></summary>
public sealed record RespondStep(RecordExpr Value, Position At) : Step(At);

/// <summary><c>fail with Code</c></summary>
public sealed record FailStep(string Code, Position At) : Step(At);

/// <summary><c>ensure &lt;condition&gt; else Code</c></summary>
public sealed record EnsureStep(Condition Condition, string Code, Position At) : Step(At);

public abstract record Condition(Position At);
public sealed record BlankCondition(PathExpr Value, bool IsNotBlank, Position At) : Condition(At);
public sealed record LengthAtMostCondition(PathExpr Value, int Maximum, Position At) : Condition(At);
public sealed record ComparisonCondition(PathExpr Left, string Operator, Expr Right, Position At) : Condition(At);

/// <summary>An unresolved phrase call: words and arguments in source order.</summary>
public sealed record PhraseCall(IReadOnlyList<CallPart> Parts, Position At) : Step(At);

public abstract record CallPart(Position At);

/// <summary>
/// A bare lowercase name. Usually a template word, but the resolver may bind it to a
/// variable (e.g. a <c>let</c>) when it lines up with a hole.
/// </summary>
public sealed record CallWord(string Text, Position At) : CallPart(At);

/// <summary>Anything that can only be an argument: a dotted path, literal, record or list.</summary>
public sealed record CallArgument(Expr Value, Position At) : CallPart(At);

// ---------- expressions ----------

public abstract record Expr(Position At);
public sealed record PathExpr(IReadOnlyList<string> Segments, Position At) : Expr(At);
public sealed record StringLit(string Value, Position At) : Expr(At);
public sealed record NumberLit(string Text, Position At) : Expr(At);
public sealed record BoolLit(bool Value, Position At) : Expr(At);
public sealed record NoneLit(Position At) : Expr(At);
public sealed record RecordExpr(IReadOnlyList<RecordField> Fields, Position At) : Expr(At);
public sealed record RecordField(string Name, Expr Value, Position At);
public sealed record ListExpr(IReadOnlyList<Expr> Items, Position At) : Expr(At);

// ---------- specs ----------

public enum SpecTrigger { Called, Handled }

public sealed record SpecDecl(
    string Description,
    IReadOnlyList<Given> Givens,
    SpecTrigger? When,                  // null only when a diagnostic was reported
    IReadOnlyList<Outcome> Thens,
    Position At);

public abstract record Given(Position At);
/// <summary><c>given items = [...]</c>: binds a phrase hole.</summary>
public sealed record GivenBinding(string Name, Expr Value, Position At) : Given(At);
/// <summary><c>given input { ... }</c>: an endpoint request.</summary>
public sealed record GivenInput(RecordExpr Value, Position At) : Given(At);

public abstract record Outcome(Position At);
public sealed record ReturnsOutcome(Expr Value, Position At) : Outcome(At);
public sealed record RespondsOutcome(RecordExpr Value, Position At) : Outcome(At);
public sealed record FailsWithOutcome(string Code, Position At) : Outcome(At);
public sealed record SucceedsOutcome(Position At) : Outcome(At);
