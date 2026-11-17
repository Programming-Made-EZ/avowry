using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The map bytes do not depend on the culture
//   As Morgan, I want the same source to give the same map bytes under any culture,
//   so that a map made on one machine matches a map made on another.
public class MapCultureInvariance
{
    // The names differ only in letters and case, and every pair that a culture orders differently from
    // an ordinal comparison appears here (for example Ab/AC, Ia/IB, Zy/ZX). No two differ by case alone,
    // so the generated files stay distinct on a case-insensitive file system.
    private static readonly string[] EndpointNames = ["Ab", "AC", "Ia", "IB", "Zy", "ZX"];

    private static readonly string[] Cultures = ["tr-TR", "sv-SE"];

    private static readonly Lazy<MethodInfo> CliEntryPoint = new(LoadCliEntryPoint);

    [Fact]
    [Trait("Slice", "169")]
    [Trait("Criterion", "169-1")]
    public async Task Map_bytes_for_one_source_equal_the_invariant_bytes_under_tr_TR_and_sv_SE()
    {
        // Given one source whose endpoint names order differently under tr-TR, sv-SE and by ordinal comparison
        var ordinal = EndpointNames.Order(StringComparer.Ordinal).ToArray();
        foreach (var name in Cultures)
        {
            var byCulture = EndpointNames.Order(StringComparer.Create(CultureInfo.GetCultureInfo(name), ignoreCase: false)).ToArray();
            Assert.False(ordinal.SequenceEqual(byCulture), $"The sample must sort differently under {name}, or the test proves nothing.");
        }
        using var workspace = TempWorkspace.Create();
        var source = Path.Combine(workspace.Root, "Cultures.avowry");
        await File.WriteAllTextAsync(source, SourceWithEndpoints(EndpointNames));
        await PhraseCli.RunAsync("--version"); // builds the CLI if needed and checks it is fresh

        // When the CLI writes the map under the invariant culture, tr-TR and sv-SE
        var invariant = BuildMap(workspace, source, CultureInfo.InvariantCulture);
        var maps = Cultures.ToDictionary(name => name, name => BuildMap(workspace, source, CultureInfo.GetCultureInfo(name)));

        // Then the invariant map lists the endpoints in ordinal order, and each culture writes the same bytes
        using var document = JsonDocument.Parse(invariant);
        var endpointNames = document.RootElement.GetProperty("records").EnumerateArray()
            .Where(record => record.GetProperty("kind").GetString() == "endpoint")
            .Select(record => record.GetProperty("name").GetString()!)
            .ToArray();
        Assert.Equal(ordinal, endpointNames);
        foreach (var (name, bytes) in maps)
            Assert.True(invariant.SequenceEqual(bytes), $"The map written under {name} differs from the invariant map.");
    }

    [Fact]
    [Trait("Slice", "169")]
    [Trait("Criterion", "169-2")]
    public void Every_sort_in_the_map_writer_uses_an_ordinal_comparison()
    {
        // Given planted map-writer texts that each hold one non-ordinal comparison
        var planted = new[]
        {
            "var a = list.OrderBy(record => record.Name, StringComparer.CurrentCulture);",
            "var b = list.ThenBy(record => record.Name);",
            "var c = list.OrderByDescending(record => record.Name, StringComparer.InvariantCulture);",
            "var d = names.Distinct();",
            "var e = names.GroupBy(name => name);",
            "var f = names.ToDictionary(name => name, name => 1);",
            "names.Sort();",
            "names.Sort(StringComparer.OrdinalIgnoreCase);",
            "var g = string.Compare(left, right);",
            "var h = left.CompareTo(right);",
            "var i = left.StartsWith(\"x\");",
            "var j = left.ToLower();",
            "var k = new HashSet<string>();",
            "var l = new Dictionary<string, List<int>>();",
            "var m = Comparer<string>.Default;",
            "var n = left.IndexOf(\"x\", StringComparison.CurrentCulture);",
        };

        // When the scanner reads each planted text
        var missed = planted.Where(text => FindNonOrdinalCalls(text).Count == 0).ToList();

        // Then every one is reported, and an ordinal text with a non-string key is not
        Assert.Empty(missed);
        Assert.Empty(FindNonOrdinalCalls(
            "var a = list.OrderBy(record => record.Kind, StringComparer.Ordinal).ThenBy(record => record.Name, StringComparer.Ordinal).ThenBy(record => record.Line);"));

        // And the real map writer, from WriteMap to the end of the map helpers, holds no non-ordinal call
        var program = File.ReadAllText(RepoPaths.Find("src", "Avowry.Cli", "Program.cs"));
        var start = program.IndexOf("static void WriteMap(", StringComparison.Ordinal);
        var end = program.IndexOf("static int Trace(", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "The map writer region (WriteMap up to Trace) was not found in Program.cs.");
        var writer = program[start..end];
        Assert.Contains("static string StepId(", writer, StringComparison.Ordinal);
        Assert.Contains(".OrderBy(", writer, StringComparison.Ordinal);
        Assert.Empty(FindNonOrdinalCalls(writer));
    }

    [Fact]
    [Trait("Slice", "169")]
    [Trait("Criterion", "169-3")]
    public void Traceability_names_the_culture_invariance_tests()
    {
        // Given docs/TRACEABILITY.md
        var docs = File.ReadAllText(RepoPaths.Find("docs", "TRACEABILITY.md"));

        // When Morgan looks for the tests that prove the map does not depend on the culture
        string[] named =
        [
            $"`{nameof(MapCultureInvariance)}.{nameof(Map_bytes_for_one_source_equal_the_invariant_bytes_under_tr_TR_and_sv_SE)}`",
            $"`{nameof(MapCultureInvariance)}.{nameof(Every_sort_in_the_map_writer_uses_an_ordinal_comparison)}`",
        ];

        // Then the document names both, as backticked Class.Method
        foreach (var name in named)
            Assert.Contains(name, docs, StringComparison.Ordinal);
    }

    // The CLI is top-level statements and no environment variable sets a .NET culture, so a child process
    // cannot be forced into tr-TR on every platform. The built CLI is loaded and its synthesized entry point
    // is run on this thread, with the thread's culture set: the real build path, no product seam, and
    // an exit code of 0 is required. The culture is restored before the thread is used for anything else.
    private static byte[] BuildMap(TempWorkspace workspace, string source, CultureInfo culture)
    {
        var output = Path.Combine(workspace.Root, "out-" + culture.Name.Replace('-', '_') + "-" + (culture.Name.Length == 0 ? "invariant" : "culture"));
        Directory.CreateDirectory(output);
        var previous = (Culture: CultureInfo.CurrentCulture, Ui: CultureInfo.CurrentUICulture);
        int exit;
        try
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            exit = (int)CliEntryPoint.Value.Invoke(null, [new[] { "build", source, "-o", output }])!;
        }
        finally
        {
            CultureInfo.CurrentCulture = previous.Culture;
            CultureInfo.CurrentUICulture = previous.Ui;
        }
        Assert.True(exit == 0, $"The build exited {exit} under '{culture.Name}'.");
        return File.ReadAllBytes(Path.Combine(output, "Cultures.avowrymap.json"));
    }

    private static MethodInfo LoadCliEntryPoint()
    {
        var configuration = AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";
        var dll = RepoPaths.Find("src", "Avowry.Cli", "bin", configuration, "net8.0", "Avowry.Cli.dll");
        return Assembly.LoadFrom(dll).EntryPoint
            ?? throw new InvalidOperationException($"The CLI assembly has no entry point: {dll}");
    }

    private static string SourceWithEndpoints(IEnumerable<string> names) =>
        Header + string.Concat(names.Select(name => Endpoint(name)));

    private const string Header = """
        module Cultures

        type LineItem
          sku: Text
          quantity: Int

        avow to "ensure {items: List<LineItem>} is not empty"
          fails with EmptyOrder
          spec "rejects an empty list"
            given items = []
            when called
            then fails with EmptyOrder
          native csharp
            if (items.Count == 0) Fail("EmptyOrder");

        avow to "ensure every quantity in {items: List<LineItem>} is positive"
          fails with InvalidQuantity
          spec "rejects a zero quantity"
            given items = [{ sku: "A", quantity: 0 }]
            when called
            then fails with InvalidQuantity
          native csharp
            if (items.Any(i => i.Quantity <= 0)) Fail("InvalidQuantity");

        avow to "total quantity of {items: List<LineItem>}" -> Int
          spec "sums quantities across line items"
            given items = [{ sku: "A", quantity: 2 }, { sku: "B", quantity: 3 }]
            when called
            then returns 5
          native csharp
            return items.Sum(i => i.Quantity);

        """;

    private static string Endpoint(string name) => $$"""
        endpoint {{name}} : POST /{{name.ToLowerInvariant()}}
          input
            customerId: Id
            items: List<LineItem>
          output
            totalQuantity: Int
          steps
            ensure input.items is not empty
            ensure every quantity in input.items is positive
            let total = total quantity of input.items
            respond with { totalQuantity: total }

          spec "accepts a valid order"
            given input { customerId: "c-1", items: [{ sku: "A", quantity: 2 }, { sku: "B", quantity: 3 }] }
            when handled
            then responds { totalQuantity: 5 }

          spec "rejects an order with no items"
            given input { customerId: "c-1", items: [] }
            when handled
            then fails with EmptyOrder


        """;

    private static readonly string[] CheckedMethods =
    [
        "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "Sort", "Distinct", "GroupBy", "ToDictionary",
        "ToHashSet", "Compare", "CompareTo", "StartsWith", "EndsWith", "IndexOf", "LastIndexOf",
    ];

    // A call on a non-string key has no comparer to name; each such call is listed here in full, so a new
    // call, or a string key swapped in, is still reported.
    private static readonly string[] NonStringKeyCalls = ["ThenBy(record => record.Line)"];

    private static readonly Regex Ordinal = new(@"\bString(Comparer|Comparison)\.Ordinal\b", RegexOptions.CultureInvariant);

    private static List<string> FindNonOrdinalCalls(string code)
    {
        var found = new List<string>();
        var methods = string.Join("|", CheckedMethods);
        var calls = new Regex($@"\.(?:{methods})\s*\(|new\s+(?:HashSet|Dictionary|SortedSet|SortedDictionary|SortedList)<[^(;]*?\(|(?:string|String)\.Compare\s*\(",
            RegexOptions.CultureInvariant);
        foreach (Match call in calls.Matches(code))
        {
            var text = code[call.Index..ArgumentsEnd(code, call.Index + call.Length)];
            var normalised = Regex.Replace(text, @"\s+", " ").TrimStart('.');
            if (!Ordinal.IsMatch(text) && !NonStringKeyCalls.Contains(normalised))
                found.Add(normalised);
        }
        foreach (Match culture in Regex.Matches(code, @"\.To(?:Lower|Upper)\s*\(|\bString(?:Comparer|Comparison)\.(?!Ordinal\b)\w+|\bComparer(?:<[^>]*>)?\.Default\b", RegexOptions.CultureInvariant))
            found.Add(culture.Value);
        return found;
    }

    private static int ArgumentsEnd(string code, int afterOpenParen)
    {
        var depth = 1;
        var inString = false;
        for (var i = afterOpenParen; i < code.Length; i++)
        {
            var c = code[i];
            if (inString)
            {
                if (c == '\\') i++;
                else if (c == '"') inString = false;
            }
            else if (c == '"') inString = true;
            else if (c == '(') depth++;
            else if (c == ')' && --depth == 0) return i + 1;
        }
        return code.Length;
    }
}
