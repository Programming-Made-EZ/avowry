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
    // The names differ only in letters and case. Ab/AC and Ia/IB order differently by culture than by ordinal.
    // Aib/AIc is the pair that tr-TR orders differently from the invariant culture (the dotless I is a letter
    // before the dotted i, so AIc comes before Aib there, and Aib before AIc under the invariant culture); it
    // stands in for the ruling's Ib/ia, because an endpoint name must start with an upper-case ASCII letter.
    // No two names differ by case alone, so the generated files stay distinct on a case-insensitive file system.
    private static readonly string[] EndpointNames = ["Ab", "AC", "Ia", "IB", "Zy", "ZX", "Aib", "AIc"];

    private static readonly string[] TurkishPair = ["Aib", "AIc"];

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
        var invariantPair = TurkishPair.Order(StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase: false)).ToArray();
        var turkishPair = TurkishPair.Order(StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), ignoreCase: false)).ToArray();
        Assert.False(invariantPair.SequenceEqual(turkishPair), "The pair must sort differently under tr-TR than under the invariant culture, or a culture-aware sort could pass.");
        using var workspace = TempWorkspace.Create();
        var source = Path.Combine(workspace.Root, "Cultures.avowry");
        await File.WriteAllTextAsync(source, SourceWithEndpoints(EndpointNames));
        await PhraseCli.RunAsync("--version"); // builds the CLI if needed and checks it is fresh

        // When the CLI writes the map under the invariant culture, tr-TR and sv-SE
        var invariant = BuildMap(workspace, source, CultureInfo.InvariantCulture);
        var maps = Cultures.ToDictionary(name => name, name => BuildMap(workspace, source, CultureInfo.GetCultureInfo(name)));

        // Then the invariant map lists every record in ordinal (kind, name, endpoint, line) order, and each culture writes the same bytes
        using var document = JsonDocument.Parse(invariant);
        var records = document.RootElement.GetProperty("records").EnumerateArray()
            .Select(record => (
                Kind: record.GetProperty("kind").GetString()!,
                Name: record.GetProperty("name").GetString()!,
                Endpoint: record.GetProperty("endpoint").GetString()!,
                Line: record.GetProperty("line").GetInt32()))
            .ToArray();
        Assert.Equal(new[] { "avow", "endpoint", "failure", "step" }, records.Select(record => record.Kind).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
        Assert.Equal(
            records.OrderBy(record => record.Kind, StringComparer.Ordinal)
                .ThenBy(record => record.Name, StringComparer.Ordinal)
                .ThenBy(record => record.Endpoint, StringComparer.Ordinal)
                .ThenBy(record => record.Line).ToArray(),
            records);
        Assert.Equal(ordinal, records.Where(record => record.Kind == "endpoint").Select(record => record.Name).ToArray());
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
            "var o = names.Order();",
            "var p = names.OrderDescending();",
            "var q = names.Min();",
            "var r = names.Max();",
            "var s = records.MinBy(record => record.Name);",
            "var t = records.MaxBy(record => record.Name);",
            "var u = names.ToLookup(name => name);",
            "var v = names.BinarySearch(\"x\");",
            "var w = from record in records orderby record.Name select record;",
            "var x = left.ToUpper();",
            "var y = char.ToUpper(letter);",
            "var z = char.ToLower(letter);",
            "var aa = left.ToLower(CultureInfo.CurrentCulture);",
        };

        // When the scanner reads each planted text
        var missed = planted.Where(text => FindNonOrdinalCalls(text).Count == 0).ToList();

        // Then every one is reported, and an ordinal text with a non-string key is not
        Assert.Empty(missed);
        Assert.Empty(FindNonOrdinalCalls(
            "var a = list.OrderBy(record => record.Kind, StringComparer.Ordinal).ThenBy(record => record.Name, StringComparer.Ordinal).ThenBy(record => record.Line);"));
        Assert.Empty(FindNonOrdinalCalls(
            "var b = names.Order(StringComparer.Ordinal).Select(name => name.ToLower(CultureInfo.InvariantCulture) + char.ToUpperInvariant(name[0]) + name.ToUpperInvariant());"));

        // And a planted call in each map helper that stays (found by its declaration, past the trace code) is reported too
        foreach (var dirty in MapHelpers)
            Assert.NotEmpty(MapWriterFindings(PlantedProgram(dirty)));
        Assert.Empty(MapWriterFindings(PlantedProgram(null)));

        // And the real map writer, from WriteMap to the end of the map helpers, holds no non-ordinal call
        var program = File.ReadAllText(RepoPaths.Find("src", "Avowry.Cli", "Program.cs"));
        Assert.Empty(MapWriterFindings(program));
        Assert.Contains(".OrderBy(", program, StringComparison.Ordinal);
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
    // is run on this thread, with the thread's culture set: the CLI's build path (it runs against the test project's copies of the compiler assemblies), no product seam, and
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

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    [Trait("Kind", "BornGreen")]
    public void The_shared_step_id_recipe_in_the_compiler_holds_no_non_ordinal_call()
    {
        // Card 165, R165-3 (F6): the step id recipe moved from the CLI's map writer into src/Avowry.Compiler/SourceMapIds.cs, so that file is scanned too.
        // Given planted recipes, one with a culture-aware case change and one clean
        var dirty = "public static class SourceMapIds { public static string StepId(string name) => name.ToLower(); }";
        var clean = "public static class SourceMapIds { public static string StepId(string name) => name.Replace(\"a\", \"b\", StringComparison.Ordinal).ToLowerInvariant(); }";
        var sorted = "public static class SourceMapIds { public static int F(string[] names) => names.Order().Count(); }";

        // Then the scanner reports the dirty ones and accepts the clean one
        Assert.NotEmpty(FindNonOrdinalCalls(dirty));
        Assert.NotEmpty(FindNonOrdinalCalls(sorted));
        Assert.Empty(FindNonOrdinalCalls(clean));

        // And the real recipe file holds no non-ordinal call
        var recipe = File.ReadAllText(RepoPaths.Find("src", "Avowry.Compiler", "SourceMapIds.cs"));
        Assert.Contains("static string StepId(", recipe, StringComparison.Ordinal);
        Assert.Empty(FindNonOrdinalCalls(recipe));
    }

    // The map writer is WriteMap up to Trace (WriteMap, Records, the record builders, StepId), plus these map
    // helpers that feed the records, each found by its declaration (never by line number).
    private static readonly string[] MapHelpers = ["Calls", "FailureDeclaration", "AvowFailureDeclaration", "Hash"];

    // A program of the right shape whose map helpers are clean, except the one named (if any), which calls a culture-aware method.
    // Card 171 moves MethodName out of Program.cs, so the helper list no longer requires it; each helper that stays has its own proof.
    private static string PlantedProgram(string? dirtyHelper)
    {
        var helpers = string.Concat(MapHelpers.Select(helper => $"static int {helper}(int a) => {(helper == dirtyHelper ? "char.ToUpper(a)" : "a")};\n"));
        return "static void WriteMap() { }\nstatic string StepId() => \"\";\nstatic int Trace(string[] values) { }\n" + helpers + "static void Error(string m) { }\n";
    }

    private static List<string> MapWriterFindings(string program)
    {
        var start = program.IndexOf("static void WriteMap(", StringComparison.Ordinal);
        var end = program.IndexOf("static int Trace(", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "The map writer region (WriteMap up to Trace) was not found in Program.cs.");
        var writer = program[start..end];
        Assert.Contains("static string StepId(", writer, StringComparison.Ordinal);
        var findings = FindNonOrdinalCalls(writer);
        foreach (var helper in MapHelpers)
        {
            var declaration = Regex.Match(program, $@"(?m)^static [^\r\n]*\b{helper}\(", RegexOptions.CultureInvariant);
            Assert.True(declaration.Success, $"The map helper {helper} was not found in Program.cs.");
            var after = declaration.Index + declaration.Length;
            var next = Regex.Match(program[after..], @"(?m)^(?:static|sealed) ", RegexOptions.CultureInvariant);
            findings.AddRange(FindNonOrdinalCalls(program[declaration.Index..(next.Success ? after + next.Index : program.Length)]));
        }
        return findings;
    }

    private static readonly string[] CheckedMethods =
    [
        "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "Order", "OrderDescending", "Sort", "Distinct",
        "GroupBy", "ToDictionary", "ToHashSet", "ToLookup", "Min", "Max", "MinBy", "MaxBy", "BinarySearch",
        "Compare", "CompareTo", "StartsWith", "EndsWith", "IndexOf", "LastIndexOf",
    ];

    // A case change is fine only with an invariant culture argument (the ...Invariant methods never match CaseChange).
    private static readonly Regex CaseChange = new(@"\.To(?:Lower|Upper)\s*\(", RegexOptions.CultureInvariant);
    private static readonly Regex InvariantArgument = new(@"\bCultureInfo\.InvariantCulture\b", RegexOptions.CultureInvariant);

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
        foreach (Match change in CaseChange.Matches(code))
        {
            var text = code[change.Index..ArgumentsEnd(code, change.Index + change.Length)];
            if (!InvariantArgument.IsMatch(text))
                found.Add(Regex.Replace(text, @"\s+", " ").TrimStart('.'));
        }
        foreach (Match culture in Regex.Matches(code, @"\borderby\b|\bString(?:Comparer|Comparison)\.(?!Ordinal\b)\w+|\bComparer(?:<[^>]*>)?\.Default\b", RegexOptions.CultureInvariant))
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
