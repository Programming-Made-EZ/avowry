using System.Globalization;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: A name that the generated C# reserves is a language diagnostic
//   As Dana, I want a clear diagnostic when a name I chose collides with a name the generated C# needs,
//   so that I do not meet an unexplained C# error or a silently dropped file.
public class ReservedGeneratedNames
{
    private static readonly Regex DiagnosticLine = new(
        """(?m)^(PH\d{3}) \((\d+):(\d+)\) (.*)$""",
        RegexOptions.CultureInvariant);

    [Fact]
    [Trait("Slice", "203")]
    [Trait("Criterion", "203-1")]
    [Trait("Status", "Pending")]
    public async Task Names_reserved_by_generated_csharp_report_a_diagnostic()
    {
        // Given one inline source per reserved-name family
        var failures = new List<string>();

        // When avowry build runs on each row
        foreach (var row in Rows())
        {
            var reason = await Check(row);
            if (reason is not null)
                failures.Add($"{row.Id}: {reason}");
        }

        // Then every row is exit 1 with PH341 at its declaration line and nothing written
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    }

    private static async Task<string?> Check(Row row)
    {
        using var workspace = TempWorkspace.Create();
        var sourcePath = Path.Combine(workspace.Root, row.Id + ".avowry");
        await File.WriteAllTextAsync(sourcePath, row.Source);
        var output = Path.Combine(workspace.Root, "out");
        var result = await PhraseCli.RunAsync("build", sourcePath, "-o", output);
        var combined = result.Text.Replace("\r\n", "\n", StringComparison.Ordinal);
        var reasons = new List<string>();

        if (result.ExitCode != 1)
            reasons.Add($"exit {result.ExitCode}");
        if (combined.Contains("Unhandled exception", StringComparison.Ordinal))
            reasons.Add("Unhandled exception");
        if (combined.Split('\n').Any(line => line.StartsWith("   at ", StringComparison.Ordinal)))
            reasons.Add("stack at");

        var match = DiagnosticLine.Match(combined);
        if (!match.Success || match.Groups[1].Value != "PH341")
        {
            reasons.Add("no PH341 (line:col)");
        }
        else
        {
            var line = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            var message = match.Groups[4].Value;
            var expectedLine = LineOf(row.Source, row.Marker);
            if (line != expectedLine)
                reasons.Add($"line {line} expected {expectedLine}");
            if (!message.Contains($"'{row.Name}'", StringComparison.Ordinal))
                reasons.Add($"message lacks '{row.Name}': {message}");
            if (!message.Contains(row.KindWord, StringComparison.Ordinal))
                reasons.Add($"message lacks {row.KindWord}: {message}");
            if (!message.Contains($"line {expectedLine}", StringComparison.Ordinal))
                reasons.Add($"message lacks line {expectedLine}: {message}");
        }

        var written = Directory.Exists(output)
            ? Directory.GetFiles(output, "*.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(output, "*.avowrymap.json", SearchOption.AllDirectories))
                .Select(path => Path.GetRelativePath(output, path))
                .Order(StringComparer.Ordinal)
                .ToArray()
            : [];
        if (written.Length > 0)
            reasons.Add($"{written.Length} files present in out (.cs or source map), first {written[0]}");

        return reasons.Count == 0 ? null : string.Join("; ", reasons);
    }

    private static int LineOf(string source, string fragment)
    {
        var lines = source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(fragment, StringComparison.Ordinal))
                return i + 1;
        }

        throw new InvalidOperationException($"source has no '{fragment}'");
    }

    private static string TypeRow(string typeName) =>
        "module M\n\ntype " + typeName + "\n  n: Int\n";

    private static string EndpointRow(string endpointName) =>
        "module M\n\n"
        + "avow to \"ok {n: Int}\"\n  native csharp\n    return;\n\n"
        + "endpoint " + endpointName + " : POST /ping\n  input\n    n: Int\n  steps\n    ok input.n\n";

    private static IEnumerable<Row> Rows()
    {
        yield return new Row("type-avowattribute", TypeRow("AvowAttribute"),
            "type AvowAttribute", "AvowAttribute", "type");
        yield return new Row("type-avowryfailure", TypeRow("AvowryFailure"),
            "type AvowryFailure", "AvowryFailure", "type");
        yield return new Row("type-avows", TypeRow("Avows"),
            "type Avows", "Avows", "type");

        yield return new Row("type-avowspecs", """
            module M

            type AvowSpecs
              n: Int

            avow to "use {s: AvowSpecs}" -> Int
              spec "reads"
                given s = { n: 1 }
                when called
                then returns 1
              native csharp
                return 1;
            """, "type AvowSpecs", "AvowSpecs", "type");

        yield return new Row("endpoint-avow", EndpointRow("Avow"),
            "endpoint Avow ", "AvowSpecs", "endpoint");

        yield return new Row("module-program", """
            module Program

            avow to "ok {n: Int}"
              native csharp
                return;
            """, "module Program", "Program", "module");

        yield return new Row("avow-method-avows", """
            module M

            avow to "avows"
              native csharp
                return;
            """, "avow to \"avows\"", "Avows", "avow");

        yield return new Row("avow-method-avowryfailure", """
            module M

            avow to "avowry failure"
              native csharp
                return;

            avow to "check {n: Int}"
              fails with Bad
              native csharp
                if (n < 0) Fail("Bad");
                return;
            """, "avow to \"avowry failure\"", "AvowryFailure", "avow");

        yield return new Row("field-named-like-its-type", """
            module M

            type Email
              email: Text
            """, "type Email", "Email", "field");

        yield return new Row("endpoint-types", EndpointRow("Types"),
            "endpoint Types ", "Types", "endpoint");
        yield return new Row("endpoint-avows", EndpointRow("Avows"),
            "endpoint Avows ", "Avows", "endpoint");
        yield return new Row("endpoint-program", EndpointRow("Program"),
            "endpoint Program ", "Program", "endpoint");
        yield return new Row("endpoint-avowryruntime", EndpointRow("AvowryRuntime"),
            "endpoint AvowryRuntime ", "AvowryRuntime", "endpoint");
        yield return new Row("endpoint-avowrybuild", EndpointRow("AvowryBuild"),
            "endpoint AvowryBuild ", "AvowryBuild", "endpoint");
    }

    private sealed record Row(string Id, string Source, string Marker, string Name, string KindWord);
}
