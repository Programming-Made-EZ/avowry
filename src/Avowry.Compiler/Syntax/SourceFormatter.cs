namespace Avowry.Compiler.Syntax;

/// <summary>
/// Renders syntax back to canonical Phrase source. Used for diagnostics, for comments in
/// generated code, and later for a `phrase fmt` command.
/// </summary>
public static class SourceFormatter
{
    public static string Format(AvowDecl phrase) =>
        $"avow to {Quote(Format(phrase.Template))}" + (phrase.Returns is null ? "" : $" -> {Format(phrase.Returns)}");

    public static string Format(Template template) =>
        string.Join(" ", template.Parts.Select(p => p switch
        {
            TemplateWord w => w.Text,
            TemplateHole h => $"{{{h.Name}: {Format(h.Type)}}}",
            _ => throw Unknown(p),
        }));

    public static string Format(TypeRef type) =>
        type.Arguments.Count == 0 ? type.Name : $"{type.Name}<{string.Join(", ", type.Arguments.Select(Format))}>";

    public static string Format(Step step) => step switch
    {
        LetStep l => $"let {l.Name} = {Format(l.Call)}",
        LetUpdateStep u => $"let {u.Name} = {Format(u.Source)} with {Format(u.Fields)}",
        RespondStep r => $"respond with {Format(r.Value)}",
        FailStep f => $"fail with {f.Code}",
        EnsureStep e => $"ensure {Format(e.Condition)} else {e.Code}",
        AvowCall c => string.Join(" ", c.Parts.Select(p => p switch
        {
            CallWord w => w.Text,
            CallArgument a => Format(a.Value),
            _ => throw Unknown(p),
        })),
        _ => throw Unknown(step),
    };

    public static string Format(Condition condition) => condition switch
    {
        BlankCondition b => $"{Format(b.Value)} is {(b.IsNotBlank ? "not " : "")}blank",
        LengthAtMostCondition l => $"{Format(l.Value)} length at most {l.Maximum}",
        ComparisonCondition c => $"{Format(c.Left)} {c.Operator} {Format(c.Right)}",
        _ => throw Unknown(condition),
    };

    public static string Format(Expr expr) => expr switch
    {
        PathExpr p => string.Join(".", p.Segments),
        StringLit s => Quote(s.Value),
        NumberLit n => n.Text,
        BoolLit b => b.Value ? "true" : "false",
        NoneLit => "none",
        RecordExpr r when r.Fields.Count == 0 => "{}",
        RecordExpr r => "{ " + string.Join(", ", r.Fields.Select(f => $"{f.Name}: {Format(f.Value)}")) + " }",
        ListExpr l => "[" + string.Join(", ", l.Items.Select(Format)) + "]",
        _ => throw Unknown(expr),
    };

    /// <summary>The spec's clauses as canonical lines: given/and, when, then/and.</summary>
    public static IReadOnlyList<string> Lines(SpecDecl spec)
    {
        var lines = new List<string>();
        for (var i = 0; i < spec.Givens.Count; i++)
            lines.Add((i == 0 ? "given " : "and ") + Format(spec.Givens[i]));
        if (spec.When is { } when)
            lines.Add("when " + (when == SpecTrigger.Called ? "called" : "handled"));
        for (var i = 0; i < spec.Thens.Count; i++)
            lines.Add((i == 0 ? "then " : "and ") + Format(spec.Thens[i]));
        return lines;
    }

    public static string Format(Given given) => given switch
    {
        GivenBinding b => $"{b.Name} = {Format(b.Value)}",
        GivenInput i => $"input {Format(i.Value)}",
        _ => throw Unknown(given),
    };

    public static string Format(Outcome outcome) => outcome switch
    {
        ReturnsOutcome r => $"returns {Format(r.Value)}",
        RespondsOutcome r => $"responds {Format(r.Value)}",
        FailsWithOutcome f => $"fails with {f.Code}",
        SucceedsOutcome => "succeeds",
        _ => throw Unknown(outcome),
    };

    private static string Quote(string value) =>
        "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    private static ArgumentException Unknown(object node) => new($"Cannot format {node.GetType().Name}.");
}
