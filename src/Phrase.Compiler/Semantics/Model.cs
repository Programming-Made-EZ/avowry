namespace Phrase.Compiler.Semantics;

// The bound model: every phrase call resolved to exactly one phrase, every value typed,
// every spec checked. This is the IR that target providers consume. It holds no syntax
// nodes; `Source` strings carry canonical source text for comments and traceability.

public sealed record ResolveResult(ModuleModel Module, IReadOnlyList<Diagnostic> Diagnostics)
{
    /// <summary>Extensions this file loads. Always contains <c>core</c>. The same source always yields the same list.</summary>
    public IReadOnlyList<string> ActiveExtensions { get; init; } = ExtensionKeywords.CoreOnly;
}

public sealed record ModuleModel(
    string Name,
    IReadOnlyList<TypeModel> Types,
    IReadOnlyList<PhraseModel> Phrases,
    IReadOnlyList<EndpointModel> Endpoints)
{
    public PhraseModel Phrase(string key) => Phrases.Single(p => p.Key == key);
}

public sealed record FieldModel(string Name, PType Type);

public sealed record TypeModel(string Name, IReadOnlyList<FieldModel> Fields);

/// <summary>One word or hole of a phrase template, in order. Providers use these to build target names.</summary>
public sealed record PhrasePart(string Text, bool IsHole);

public sealed record PhraseModel(
    string Key,                                   // words lowercased, holes as "_": "ensure _ is not empty"
    string Template,                              // canonical: "ensure {items: List<LineItem>} is not empty"
    IReadOnlyList<PhrasePart> Parts,
    IReadOnlyList<FieldModel> Holes,
    PType? Returns,
    IReadOnlyList<string> Effects,
    IReadOnlyList<string> Failures,               // effective: declared plus everything its steps can fail with
    IReadOnlyDictionary<string, string> Natives,  // target -> code (leaf phrases)
    IReadOnlyList<BoundStep>? Steps,              // composite phrases
    IReadOnlyList<PhraseSpecModel> Specs);

public sealed record EndpointModel(
    string Name,
    string Verb,
    string Route,
    PType InputType,
    IReadOnlyList<FieldModel> Input,
    PType? OutputType,
    IReadOnlyList<FieldModel> Output,
    IReadOnlyList<BoundStep> Steps,
    IReadOnlyList<string> Failures,               // everything its steps can fail with
    IReadOnlyList<EndpointSpecModel> Specs);

// ---------- steps ----------

public abstract record BoundStep(string Source);
public sealed record BoundCallStep(BoundCall Call, string Source) : BoundStep(Source);
public sealed record BoundLetStep(string Name, PType Type, BoundCall Call, string Source) : BoundStep(Source);
public sealed record BoundLetUpdateStep(string Name, BoundExpr Record, IReadOnlyList<BoundRecordField> Fields, PType Type, string Source) : BoundStep(Source);
public sealed record BoundRespondStep(BoundRecord Value, string Source) : BoundStep(Source);
public sealed record BoundFailStep(string Code, string Source) : BoundStep(Source);
public sealed record BoundEnsureStep(BoundCondition Condition, string Code, string Source) : BoundStep(Source);

public abstract record BoundCondition;
public sealed record BoundBlankCondition(BoundExpr Value, bool IsNotBlank) : BoundCondition;
public sealed record BoundLengthAtMostCondition(BoundExpr Value, int Maximum) : BoundCondition;
public sealed record BoundComparisonCondition(BoundExpr Left, string Operator, BoundExpr Right) : BoundCondition;

/// <summary>A call to the phrase with this key; one argument per hole, in hole order.</summary>
public sealed record BoundCall(string PhraseKey, IReadOnlyList<BoundArgument> Arguments);

public sealed record BoundArgument(FieldModel Hole, BoundExpr Value);

// ---------- values ----------

public abstract record BoundExpr(PType Type);
public sealed record BoundVariable(string Name, PType Type) : BoundExpr(Type);
public sealed record BoundField(BoundExpr Target, string Name, PType Type) : BoundExpr(Type);
public sealed record BoundText(string Value, PType Type) : BoundExpr(Type);     // Text or Id
public sealed record BoundNumber(string Text, PType Type) : BoundExpr(Type);    // Int or Decimal
public sealed record BoundBool(bool Value) : BoundExpr(PType.Bool);
public sealed record BoundNone(PType Type) : BoundExpr(Type);                  // an Option<T>
public sealed record BoundRecord(IReadOnlyList<BoundRecordField> Fields, PType Type) : BoundExpr(Type); // fields in declaration order
public sealed record BoundRecordField(string Name, BoundExpr Value);
public sealed record BoundList(IReadOnlyList<BoundExpr> Items, PType Type) : BoundExpr(Type);

// ---------- specs ----------

/// <summary>A checked phrase spec. <see cref="Lines"/> are the canonical given/when/then lines.</summary>
public sealed record PhraseSpecModel(
    string Description,
    IReadOnlyList<BoundArgument> Givens,          // one per hole, in hole order
    SpecOutcomeModel Outcome,
    IReadOnlyList<string> Lines);

public sealed record EndpointSpecModel(
    string Description,
    BoundRecord Input,
    SpecOutcomeModel Outcome,
    IReadOnlyList<string> Lines);

public abstract record SpecOutcomeModel;
public sealed record ReturnsModel(BoundExpr Value) : SpecOutcomeModel;
public sealed record RespondsModel(BoundRecord Value) : SpecOutcomeModel;
public sealed record FailsModel(string Code) : SpecOutcomeModel;
public sealed record SucceedsModel : SpecOutcomeModel;
