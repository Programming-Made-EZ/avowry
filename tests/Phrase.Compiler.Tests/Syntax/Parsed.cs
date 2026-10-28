using Phrase.Compiler.Syntax;

namespace Phrase.Compiler.Tests.Syntax;

/// <summary>
/// Test helpers. <see cref="Print(object)"/> renders AST nodes compactly so expectations
/// read like source. In phrase calls, arguments are wrapped in &lt; &gt; to tell them apart from words.
/// </summary>
internal static class Parsed
{
    /// <summary>Parses and asserts there were no diagnostics.</summary>
    public static SourceFile Clean(string source)
    {
        var result = Parser.Parse(source);
        Assert.True(result.Diagnostics.Count == 0, string.Join("\n", result.Diagnostics));
        return result.File;
    }

    public static IReadOnlyList<string> Codes(string source) =>
        Parser.Parse(source).Diagnostics.Select(d => d.Code).ToList();

    /// <summary>Wraps lines in a module header so a test can focus on one declaration.</summary>
    public static string InModule(string body) => "module Test\n" + body;

    public static string Print(object node) => node switch
    {
        TypeRef t when t.Arguments.Count == 0 => t.Name,
        TypeRef t => $"{t.Name}<{string.Join(", ", t.Arguments.Select(Print))}>",
        FieldDecl f => $"{f.Name}: {Print(f.Type)}",
        TypeDecl t => $"(type {t.Name} {string.Join(" ", t.Fields.Select(f => $"({Print(f)})"))})",

        Template t => string.Join(" ", t.Parts.Select(Print)),
        TemplateWord w => w.Text,
        TemplateHole h => $"{{{h.Name}: {Print(h.Type)}}}",

        LetStep s => $"(let {s.Name} {Print(s.Call)})",
        LetUpdateStep s => $"(let {s.Name} {Print(s.Source)} with {Print(s.Fields)})",
        RespondStep s => $"(respond {Print(s.Value)})",
        FailStep s => $"(fail {s.Code})",
        EnsureStep s => $"(ensure {SourceFormatter.Format(s.Condition)} else {s.Code})",
        PhraseCall c => $"(call {string.Join(" ", c.Parts.Select(Print))})",
        CallWord w => w.Text,
        CallArgument a => $"<{Print(a.Value)}>",

        PathExpr p => string.Join(".", p.Segments),
        StringLit s => $"\"{s.Value}\"",
        NumberLit n => n.Text,
        BoolLit b => b.Value ? "true" : "false",
        NoneLit => "none",
        RecordExpr r => $"{{{string.Join(", ", r.Fields.Select(f => $"{f.Name}: {Print(f.Value)}"))}}}",
        ListExpr l => $"[{string.Join(", ", l.Items.Select(Print))}]",

        SpecDecl s => $"(spec \"{s.Description}\""
                      + string.Concat(s.Givens.Select(g => " " + Print(g)))
                      + (s.When is { } w ? $" (when {w.ToString().ToLowerInvariant()})" : "")
                      + string.Concat(s.Thens.Select(t => " " + Print(t)))
                      + ")",
        GivenBinding g => $"(given {g.Name} = {Print(g.Value)})",
        GivenInput g => $"(given input {Print(g.Value)})",
        ReturnsOutcome o => $"(then returns {Print(o.Value)})",
        RespondsOutcome o => $"(then responds {Print(o.Value)})",
        FailsWithOutcome o => $"(then fails {o.Code})",
        SucceedsOutcome => "(then succeeds)",

        _ => throw new ArgumentException($"No printer for {node.GetType().Name}"),
    };

    public static string Print(IEnumerable<object> nodes) => string.Join(" ", nodes.Select(Print));
}
