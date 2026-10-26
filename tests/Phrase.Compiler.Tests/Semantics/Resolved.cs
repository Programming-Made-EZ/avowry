using Phrase.Compiler.Semantics;

namespace Phrase.Compiler.Tests.Semantics;

/// <summary>
/// Test helpers. <see cref="Prelude"/> is a small phrase library shared by most tests;
/// tests add declarations after it with <see cref="Program"/>.
/// </summary>
internal static class Resolved
{
    public const string Prelude = """
        module Test

        type LineItem
          sku: Text
          quantity: Int

        phrase "ensure {items: List<LineItem>} is not empty"
          fails with EmptyOrder
          native csharp
            return;

        phrase "ensure every quantity in {items: List<LineItem>} is positive"
          fails with InvalidQuantity
          native csharp
            return;

        phrase "total quantity of {items: List<LineItem>}" -> Int
          native csharp
            return 0;

        phrase "ensure {n: Int} is positive"
          fails with NotPositive
          native csharp
            return;

        """;

    public static string Program(params string[] declarations) => Prelude + string.Join("\n", declarations);

    /// <summary>An endpoint with input { customerId: Id, items: List&lt;LineItem&gt; } and no output.</summary>
    public static string Probe(params string[] steps) => Endpoint(output: false, steps, specs: "");

    /// <summary>Same as <see cref="Probe"/> plus output { totalQuantity: Int }.</summary>
    public static string ProbeWithOutput(params string[] steps) => Endpoint(output: true, steps, specs: "");

    public static string ProbeWithSpec(string specs) =>
        Endpoint(output: true,
            ["let total = total quantity of input.items", "respond with { totalQuantity: total }"],
            specs);

    public static string Endpoint(bool output, IEnumerable<string> steps, string specs) =>
        "endpoint Probe : POST /probe\n" +
        "  input\n" +
        "    customerId: Id\n" +
        "    items: List<LineItem>\n" +
        (output ? "  output\n    totalQuantity: Int\n" : "") +
        "  steps\n" +
        string.Concat(steps.Select(s => $"    {s}\n")) +
        specs;

    /// <summary>A spec block indented for a phrase or endpoint body.</summary>
    public static string Spec(string description, params string[] lines) =>
        $"  spec \"{description}\"\n" + string.Concat(lines.Select(l => $"    {l}\n"));

    public static ModuleModel Clean(string source)
    {
        var result = Resolver.Resolve(source);
        Assert.True(result.Diagnostics.Count == 0, string.Join("\n", result.Diagnostics));
        return result.Module;
    }

    public static IReadOnlyList<string> Codes(string source) =>
        Resolver.Resolve(source).Diagnostics.Select(d => d.Code).ToList();

    /// <summary>Asserts exactly one diagnostic and returns it.</summary>
    public static Diagnostic Only(string source) => Assert.Single(Resolver.Resolve(source).Diagnostics);

    public static EndpointModel ProbeModel(ModuleModel module) => module.Endpoints.Single(e => e.Name == "Probe");
}
