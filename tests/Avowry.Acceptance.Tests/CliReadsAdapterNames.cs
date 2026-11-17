using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;
using Avowry.Adapters.CSharp;
using Avowry.Compiler;

namespace Avowry.Acceptance.Tests;

// Feature: The CLI reads method and type names from the adapter
//   As Morgan, I want the command line to know nothing about the C# layout,
//   so that a second adapter does not need changes in the CLI.
public class CliReadsAdapterNames
{
    private const int MaxLineLength = 120;
    private const int MaxChain = 4;

    private static readonly string[] AdapterLiterals = ["Handler", "Handle", "Avows", "Phrases"];

    private static readonly Regex MethodNameRule = new(@"\bMethodName\b|\bIsHole\b|\bParts\b|\bchar\.ToUpper", RegexOptions.CultureInvariant);

    private static readonly Regex LoopHeader = new(@"\b(?:foreach|for|while)\s*\(", RegexOptions.CultureInvariant);

    private static readonly Regex LoopStart = new(@"^(?:foreach|for|while)\s*\(", RegexOptions.CultureInvariant);

    private static readonly Regex NullForgiving = new(@"(?<=[\w\)\]])!(?!=)", RegexOptions.CultureInvariant);

    private static readonly Regex ChainedCall = new(@"\G\.\s*\w+\s*(?:<[^()]*>)?\s*\(", RegexOptions.CultureInvariant);

    private static readonly Lazy<MethodInfo> CliEntryPoint = new(LoadCliEntryPoint);

    [Fact]
    [Trait("Slice", "171")]
    [Trait("Criterion", "171-1")]
    public void Program_cs_holds_no_adapter_literal_and_no_method_name_rule()
    {
        // Given planted texts that each hold an adapter literal or a copy of the method-name rule
        var planted = new[]
        {
            "var a = $\"{endpoint.Name}Handler.Handle\";",
            "var b = $\"Avows.{name}\";",
            "var c = \"Phrases\";",
            "var d = \"Handle\";",
            "static string MethodName(AvowModel phrase) => \"\";",
            "var e = phrase.Parts.Where(part => !part.IsHole);",
            "var f = char.ToUpperInvariant(text[0]);",
        };

        // When the scanner reads each planted text
        var missed = planted.Where(text => AdapterNameFindings(text).Count == 0).ToList();

        // Then every one is reported, and the same words used as identifiers or comments, or other literals, are not
        Assert.Empty(missed);
        Assert.Empty(AdapterNameFindings("foreach (var phrase in module.Avows)\n{\n    var cs = output.AvowFiles[phrase.Key];\n}"));
        Assert.Empty(AdapterNameFindings("var method = output.AvowMethods[phrase.Key]; // Handler, Handle, Avows, Phrases"));
        Assert.Empty(AdapterNameFindings("var kind = \"avow\"; var id = $\"{kind}:{phrase.Key}\";"));
        Assert.Contains("Handler.Handle", Lex("var a = $\"{endpoint.Name}Handler.Handle\";").Literals);

        // And the real Program.cs holds none
        var program = File.ReadAllText(RepoPaths.Find("src", "Avowry.Cli", "Program.cs"));
        Assert.Empty(AdapterNameFindings(program));
    }

    [Fact]
    [Trait("Slice", "171")]
    [Trait("Criterion", "171-1")]
    public async Task Map_csMethod_values_are_the_names_the_adapter_returned()
    {
        // Given a source with two endpoints and avows, and what the adapter returns for it
        using var workspace = TempWorkspace.Create();
        var source = Path.Combine(workspace.Root, "Names.avowry");
        await File.WriteAllTextAsync(source, FixtureSource);
        await PhraseCli.RunAsync("--version"); // builds the CLI if needed and checks it is fresh
        var returned = AvowryCompiler.Compile(File.ReadAllText(source), new CSharpProvider(), source);
        var module = returned.Module ?? throw new InvalidOperationException("The adapter returned no module.");
        var endpointNames = module.Endpoints.Select(endpoint => endpoint.Name).Order(StringComparer.Ordinal).ToArray();
        var avowKeys = module.Avows.Select(phrase => phrase.Key).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "Alpha", "Beta" }, endpointNames);
        Assert.Equal(3, avowKeys.Length);

        // When the CLI builds the same source and writes the map
        var nameMaps = NameMaps(returned);
        var endpointMethods = nameMaps.SingleOrDefault(map => map.Keys.Order(StringComparer.Ordinal).SequenceEqual(endpointNames));
        var avowMethods = nameMaps.SingleOrDefault(map => map.Keys.Order(StringComparer.Ordinal).SequenceEqual(avowKeys));
        var output = Path.Combine(workspace.Root, "out");
        Directory.CreateDirectory(output);
        var exit = (int)CliEntryPoint.Value.Invoke(null, [new[] { "build", source, "-o", output }])!;
        Assert.True(exit == 0, $"The build exited {exit}.");
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "Names.avowrymap.json")));
        var records = document.RootElement.GetProperty("records").EnumerateArray()
            .Select(record => (
                Kind: record.GetProperty("kind").GetString()!,
                Name: record.GetProperty("name").GetString()!,
                Endpoint: record.GetProperty("endpoint").GetString()!,
                CsMethod: record.GetProperty("csMethod").GetString()!))
            .ToArray();

        // Then the adapter returned one method name per endpoint and one per avow key, and each record's csMethod is that name
        Assert.True(endpointMethods is not null, "The adapter must return a map of method name per endpoint (a new IReadOnlyDictionary<string, string> on CompilationOutput, keyed by endpoint name).");
        Assert.True(avowMethods is not null, "The adapter must return a map of method name per avow key (a new IReadOnlyDictionary<string, string> on CompilationOutput, keyed by avow key).");
        Assert.NotNull(endpointMethods);
        Assert.NotNull(avowMethods);
        Assert.Equal(2, endpointMethods.Values.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(3, avowMethods.Values.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(2, records.Count(record => record.Kind == "endpoint"));
        Assert.Equal(3, records.Count(record => record.Kind == "avow"));
        Assert.Contains(records, record => record.Kind == "step");
        Assert.Contains(records, record => record.Kind == "failure");
        foreach (var record in records)
        {
            var expected = record.Kind == "avow"
                ? avowMethods[module.Avows.Single(phrase => phrase.Template == record.Name).Key]
                : endpointMethods[record.Endpoint];
            Assert.True(record.CsMethod == expected, $"The {record.Kind} record '{record.Name}' has csMethod '{record.CsMethod}', the adapter returned '{expected}'.");
        }
    }

    [Fact]
    [Trait("Slice", "171")]
    [Trait("Criterion", "171-2")]
    public void Program_cs_keeps_one_statement_per_line_short_lines_short_chains_and_no_null_forgiving()
    {
        // Given planted texts that each break one style rule
        var planted = new (string Rule, string Code)[]
        {
            ("statements", "var a = 1; var b = 2;"),
            ("statements", "if (a) { Use(a); Use(b); }"),
            ("loop", "foreach (var a in items)\nforeach (var b in a)\n{\n    Use(b);\n}"),
            ("loop", "foreach (var a in items) Use(a);"),
            ("loop", "while (Next())\nfor (var i = 0; i < 3; i++)\n{\n    Use(i);\n}"),
            ("length", "//" + new string('x', MaxLineLength - 1)),
            ("chain", "var a = items.A().B().C().D().E();"),
            ("chain", "var a = Items()\n    .A()\n    .B(x => x.C().D())\n    .E()\n    .F()\n    .G();"),
            ("null-forgiving", "var a = map!;"),
            ("null-forgiving", "var a = Find(name)!.Value;"),
            ("null-forgiving", "var a = items[0]!;"),
        };

        // When the scanner reads each planted text
        var missed = planted.Where(row => !StyleFindings(row.Code).Any(finding => finding.Rule == row.Rule)).ToList();

        // Then every one is reported under its own rule
        Assert.Empty(missed);

        // And texts at the limit of each rule are not
        string[] clean =
        [
            "//" + new string('x', MaxLineLength - 2),
            "var a = items.A().B().C().D();",
            "var a = Items(x).A().B(x => x.C().D().E().F()).C();",
            "foreach (var a in items)\n{\n    foreach (var b in a)\n    {\n        Use(b);\n    }\n}",
            "for (var i = 0; i < 3; i++)\n{\n    Use(i);\n}",
            "var a = 1;\nvar b = 2;",
            "var a = b != c && !d && e!=f && !(g);",
            "var a = \"x; y; z! (\";",
            "var a = $\"{b} {(c == \"x\" ? \"y; z\" : \"w!\")}\";",
        ];
        foreach (var code in clean)
            Assert.Empty(StyleFindings(code));

        // And the real Program.cs breaks none
        var program = File.ReadAllText(RepoPaths.Find("src", "Avowry.Cli", "Program.cs"));
        Assert.Empty(StyleFindings(program).Select(finding => $"{finding.Rule} at line {finding.Line}"));
    }

    [Fact]
    [Trait("Slice", "171")]
    [Trait("Criterion", "171-3")]
    public async Task Every_map_csMethod_names_a_method_that_exists_in_the_generated_csharp()
    {
        // Given the orders fixture and the names fixture, whose avows have holes; the generated C# of orders is held byte-identical by the
        // Slice 142 hash pin (19fe6d71...) and the Tasks demo's by the Slice 176 golden test, so only the map may change
        using var workspace = TempWorkspace.Create();
        var orders = RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "Fixtures", "orders.avowry");
        var names = Path.Combine(workspace.Root, "Names.avowry");
        await File.WriteAllTextAsync(names, FixtureSource);

        // When the CLI builds each and writes its map
        var ordersOut = Path.Combine(workspace.Root, "orders-out");
        var namesOut = Path.Combine(workspace.Root, "names-out");
        var ordersResult = await PhraseCli.RunAsync("build", orders, "-o", ordersOut);
        var namesResult = await PhraseCli.RunAsync("build", names, "-o", namesOut);
        Assert.True(ordersResult.ExitCode == 0, ordersResult.Text);
        Assert.True(namesResult.ExitCode == 0, namesResult.Text);

        // Then every record's csMethod is Type.Method, the type is a class and the method a public static method in the record's csFile
        var missing = new List<string>();
        var checkedAvows = 0;
        foreach (var (root, map) in new[] { (ordersOut, "Orders.avowrymap.json"), (namesOut, "Names.avowrymap.json") })
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, map)));
            foreach (var record in document.RootElement.GetProperty("records").EnumerateArray())
            {
                var csMethod = record.GetProperty("csMethod").GetString()!;
                var csFile = record.GetProperty("csFile").GetString()!;
                var generated = File.ReadAllText(Path.Combine(root, csFile.Replace('/', Path.DirectorySeparatorChar)));
                var dot = csMethod.LastIndexOf('.');
                var type = dot > 0 ? csMethod[..dot] : "";
                var method = dot > 0 ? csMethod[(dot + 1)..] : csMethod;
                var declared = Regex.IsMatch(generated, $@"\bclass\s+{Regex.Escape(type)}\b", RegexOptions.CultureInvariant)
                    && Regex.IsMatch(generated, $@"public static [^\n(]*\s{Regex.Escape(method)}\(", RegexOptions.CultureInvariant);
                if (!declared)
                    missing.Add($"{map}: {record.GetProperty("kind").GetString()} '{record.GetProperty("name").GetString()}' names {csMethod}, not declared in {csFile}");
                if (record.GetProperty("kind").GetString() == "avow")
                    checkedAvows++;
            }
        }
        Assert.Equal(6, checkedAvows);
        Assert.Empty(missing);
    }

    // The two maps the adapter adds beside EndpointFiles and AvowFiles: any other string map on the output.
    // The names of the new properties are the green's choice, so the test finds them by shape.
    private static List<IReadOnlyDictionary<string, string>> NameMaps(CompilationOutput output) =>
        typeof(CompilationOutput).GetProperties()
            .Where(property => typeof(IReadOnlyDictionary<string, string>).IsAssignableFrom(property.PropertyType))
            .Where(property => property.Name is not ("Files" or "EndpointFiles" or "AvowFiles"))
            .Select(property => (IReadOnlyDictionary<string, string>)property.GetValue(output)!)
            .ToList();

    private static List<string> AdapterNameFindings(string code)
    {
        var lexed = Lex(code);
        var found = new List<string>();
        foreach (var literal in lexed.Literals)
        {
            foreach (var word in AdapterLiterals.Where(word => literal.Contains(word, StringComparison.Ordinal)))
                found.Add($"literal \"{literal}\" holds {word}");
        }
        foreach (Match rule in MethodNameRule.Matches(lexed.Masked))
            found.Add($"method-name rule: {rule.Value}");
        return found;
    }

    private static List<(string Rule, int Line)> StyleFindings(string code)
    {
        var text = code.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var masked = Lex(text).Masked;
        var lines = text.Split('\n');
        var maskedLines = masked.Split('\n');
        var found = new List<(string Rule, int Line)>();
        for (var index = 0; index < lines.Length; index++)
        {
            if (lines[index].Length > MaxLineLength)
                found.Add(("length", index + 1));
            if (TopLevelSemicolons(maskedLines[index]) > 1)
                found.Add(("statements", index + 1));
            if (LoopProblem(maskedLines, index))
                found.Add(("loop", index + 1));
        }
        foreach (Match forgiving in NullForgiving.Matches(masked))
            found.Add(("null-forgiving", LineOf(masked, forgiving.Index)));
        foreach (var chain in LongChainIndexes(masked))
            found.Add(("chain", LineOf(masked, chain)));
        return found;
    }

    private static int LineOf(string text, int index) => text[..index].Count(character => character == '\n') + 1;

    private static int TopLevelSemicolons(string line)
    {
        var depth = 0;
        var count = 0;
        foreach (var character in line)
        {
            if (character == '(')
                depth++;
            else if (character == ')')
                depth--;
            else if (character == ';' && depth == 0)
                count++;
        }
        return count;
    }

    // A loop header with its body on the same line, or with another loop header as its unbraced body.
    private static bool LoopProblem(string[] lines, int index)
    {
        var line = lines[index];
        if (line.TrimStart().StartsWith('}'))
            return false;
        var header = LoopHeader.Match(line);
        if (!header.Success)
            return false;
        var depth = 0;
        for (var i = header.Index + header.Length - 1; i < line.Length; i++)
        {
            depth += line[i] == '(' ? 1 : line[i] == ')' ? -1 : 0;
            if (depth != 0)
                continue;
            var after = line[(i + 1)..].Trim();
            if (after.Length > 0 && after != "{")
                return true;
            var next = lines.Skip(index + 1).FirstOrDefault(candidate => candidate.Trim().Length > 0)?.Trim() ?? "";
            return after.Length == 0 && LoopStart.IsMatch(next);
        }
        return false;
    }

    // Counts the .Name( calls that follow one another on one value, at any nesting depth; a statement ends the chain.
    private static List<int> LongChainIndexes(string masked)
    {
        var found = new List<int>();
        var counts = new Stack<int>();
        counts.Push(0);
        for (var i = 0; i < masked.Length; i++)
        {
            var character = masked[i];
            if (character == ';')
            {
                counts.Clear();
                counts.Push(0);
            }
            else if (character == '.' && ChainedCall.Match(masked, i) is { Success: true } call)
            {
                var count = counts.Pop() + 1;
                counts.Push(count);
                if (count == MaxChain + 1)
                    found.Add(i);
                counts.Push(0);
                i += call.Length - 1;
            }
            else if (character == '(')
            {
                counts.Push(0);
            }
            else if (character == ')' && counts.Count > 1)
            {
                counts.Pop();
                if (!ContinuesChain(masked, i + 1))
                {
                    counts.Pop();
                    counts.Push(0);
                }
            }
        }
        return found;
    }

    private static bool ContinuesChain(string masked, int start)
    {
        var index = start;
        while (index < masked.Length && char.IsWhiteSpace(masked[index]))
            index++;
        if (index >= masked.Length)
            return false;
        return masked[index] == '.' || (masked[index] == '?' && index + 1 < masked.Length && masked[index + 1] == '.');
    }

    private sealed record Lexed(string Masked, IReadOnlyList<string> Literals);

    // Blanks string contents (including nested strings inside interpolation holes), char literals and // comments,
    // keeping the length and the line breaks, and collects the text of each string literal.
    private static Lexed Lex(string code)
    {
        var masked = code.ToCharArray();
        var literals = new List<string>();
        var modes = new Stack<char>();
        var texts = new Stack<StringBuilder>();
        modes.Push('c');
        for (var i = 0; i < code.Length; i++)
        {
            var character = code[i];
            var next = i + 1 < code.Length ? code[i + 1] : '\0';
            var mode = modes.Peek();
            if (mode == 'c')
            {
                if (character == '$' && next == '"')
                {
                    modes.Push('i');
                    texts.Push(new StringBuilder());
                    i++;
                }
                else if (character == '"')
                {
                    modes.Push('s');
                    texts.Push(new StringBuilder());
                }
                else if (character == '\'')
                {
                    i = BlankCharLiteral(code, masked, i);
                }
                else if (character == '/' && next == '/')
                {
                    while (i < code.Length && code[i] != '\n')
                        Blank(masked, i++);
                    i--;
                }
                else if (character == '}' && modes.Count > 1)
                {
                    modes.Pop();
                }
            }
            else if (character == '\\' && i + 1 < code.Length)
            {
                texts.Peek().Append(character).Append(next);
                Blank(masked, i);
                Blank(masked, i + 1);
                i++;
            }
            else if (character == '"')
            {
                literals.Add(texts.Pop().ToString());
                modes.Pop();
            }
            else if (mode == 'i' && character == '{' && next != '{')
            {
                modes.Push('c');
            }
            else if (mode == 'i' && (character == '{' || character == '}') && next == character)
            {
                texts.Peek().Append(character);
                Blank(masked, i);
                Blank(masked, i + 1);
                i++;
            }
            else
            {
                texts.Peek().Append(character);
                Blank(masked, i);
            }
        }
        return new Lexed(new string(masked), literals);
    }

    private static int BlankCharLiteral(string code, char[] masked, int quote)
    {
        if (quote + 1 >= code.Length)
            return quote;
        var closing = Math.Min(code[quote + 1] == '\\' ? quote + 3 : quote + 2, code.Length - 1);
        for (var i = quote + 1; i < closing; i++)
            Blank(masked, i);
        return closing;
    }

    private static void Blank(char[] masked, int index)
    {
        if (masked[index] != '\n' && masked[index] != '\r')
            masked[index] = ' ';
    }

    // The CLI is top-level statements, so its synthesized entry point is run on this thread in this process,
    // the way card 169's MapCultureInvariance runs it; it reads the test project's copies of the compiler assemblies.
    private static MethodInfo LoadCliEntryPoint()
    {
        var configuration = AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";
        var dll = RepoPaths.Find("src", "Avowry.Cli", "bin", configuration, "net8.0", "Avowry.Cli.dll");
        return Assembly.LoadFrom(dll).EntryPoint
            ?? throw new InvalidOperationException($"The CLI assembly has no entry point: {dll}");
    }

    private static readonly string FixtureSource = """
        module Names

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

        """ + Endpoint("Alpha") + Endpoint("Beta");

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
}
