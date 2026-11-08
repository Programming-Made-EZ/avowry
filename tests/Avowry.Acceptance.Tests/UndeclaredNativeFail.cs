using System.Globalization;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

public class UndeclaredNativeFail
{
    private static readonly Regex DiagnosticLine = new(
        """(?m)^(PH\d{3}) \((\d+):(\d+)\) """,
        RegexOptions.CultureInvariant);

    private static readonly Regex PhShape = new(
        """(?m)^PH\d{3} \(""",
        RegexOptions.CultureInvariant);

    [Fact]
    [Trait("Slice", "202")]
    [Trait("Criterion", "202-1")]
    [Trait("Status", "Pending")]
    public async Task Undeclared_native_fail_reports_a_diagnostic()
    {
        // Given each in-card native Fail row as an inline source
        var failures = new List<string>();

        // When avowry build runs on each row
        foreach (var row in Rows())
        {
            var reason = await Check(row);
            if (reason is not null)
                failures.Add($"{row.Id}: {reason}");
        }

        // Then every failing id is PH337 at the Fail line and every keep id still builds
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    }

    private static async Task<string?> Check(Row row)
    {
        using var workspace = TempWorkspace.Create();
        var sourcePath = Path.Combine(workspace.Root, row.Id + ".avowry");
        var text = row.Crlf
            ? row.Source.Replace("\n", "\r\n", StringComparison.Ordinal)
            : row.Source;
        await File.WriteAllTextAsync(sourcePath, text);
        var output = Path.Combine(workspace.Root, "out");
        var result = await PhraseCli.RunAsync("build", sourcePath, "-o", output);
        var combined = result.Text.Replace("\r\n", "\n", StringComparison.Ordinal);
        var reasons = new List<string>();

        if (row.KeepBuilding)
        {
            if (result.ExitCode != 0)
                reasons.Add($"exit {result.ExitCode}");
            if (PhShape.IsMatch(combined))
                reasons.Add("PHnnn (line:col)");
            return reasons.Count == 0 ? null : string.Join("; ", reasons);
        }

        if (result.ExitCode != 1)
            reasons.Add($"exit {result.ExitCode}");
        if (combined.Contains("Unhandled exception", StringComparison.Ordinal))
            reasons.Add("Unhandled exception");

        var match = DiagnosticLine.Match(combined);
        if (!match.Success)
        {
            reasons.Add("no PHnnn (line:col)");
            return string.Join("; ", reasons);
        }

        var code = match.Groups[1].Value;
        var line = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var rest = combined[(match.Index + match.Length)..];
        var cut = rest.IndexOf('\n');
        var message = cut < 0 ? rest : rest[..cut];
        var expectedLine = LineOf(row.Source, row.Marker);

        if (code != "PH337")
            reasons.Add($"code {code} expected PH337");
        if (line != expectedLine)
            reasons.Add($"line {line} expected {expectedLine}");
        if (!message.Contains(row.UndeclaredCode, StringComparison.Ordinal))
            reasons.Add($"message lacks {row.UndeclaredCode}");
        if (!message.Contains(row.Template, StringComparison.Ordinal))
            reasons.Add($"message lacks template {row.Template}");
        if (!message.Contains("fails with", StringComparison.Ordinal))
            reasons.Add("message lacks fails with");
        if (!message.Contains($"line {expectedLine}", StringComparison.Ordinal))
            reasons.Add($"message lacks line {expectedLine}");

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

    private static IEnumerable<Row> Rows()
    {
        yield return new Row("omitted-from-clause", """
            module M
            avow to "reject {n: Int}"
              fails with Declared
              native csharp
                if (n < 0) Fail("X");
            """, Marker: """Fail("X")""", UndeclaredCode: "X", Template: "reject {n: Int}");

        yield return new Row("no-clause", """
            # undeclared-native-failure: native-only Fail with an undeclared code creates no language failure record (a quirk to revisit: GRAMMAR.md says a phrase may only fail with declared codes, but native bodies are not inspected; tracked by card 202, and fixing it flips this fixture)
            module UndeclaredNativeFailure
            avow to "reject natively"
              native csharp
                Fail("NativeOnly");
            """, Marker: """Fail("NativeOnly")""", UndeclaredCode: "NativeOnly", Template: "reject natively");

        yield return new Row("fail-not-first-line", """
            module M
            avow to "reject {n: Int}"
              fails with Declared
              native csharp

                var m = n + 1;
                if (m < 0) Fail("Declared");

                if (n < 0) Fail("X");
            """, Marker: """Fail("X")""", UndeclaredCode: "X", Template: "reject {n: Int}");

        yield return new Row("one-of-two-undeclared", """
            module M
            avow to "reject {n: Int}"
              fails with Declared
              native csharp
                if (n < 0) Fail("Declared");
                if (n > 9) Fail("X");
            """, Marker: """Fail("X")""", UndeclaredCode: "X", Template: "reject {n: Int}");

        yield return new Row("two-undeclared", """
            module M
            avow to "reject {n: Int}"
              native csharp
                if (n < 0) Fail("A");
                if (n > 9) Fail("B");
            """, Marker: """Fail("A")""", UndeclaredCode: "A", Template: "reject {n: Int}");

        yield return new Row("crlf", """
            module M
            avow to "reject {n: Int}"
              fails with Declared
              native csharp
                if (n < 0) Fail("X");
            """, Marker: """Fail("X")""", UndeclaredCode: "X", Template: "reject {n: Int}", Crlf: true);

        yield return new Row("multi-line-call", """
            module M
            avow to "reject {n: Int}"
              native csharp
                Fail(
                  "X");
            """, Marker: "Fail(", UndeclaredCode: "X", Template: "reject {n: Int}");

        yield return new Row("declared-twice", """
            module M
            avow to "reject {n: Int}"
              fails with X
              native csharp
                if (n < 0) Fail("X");
                if (n > 9) Fail("X");
            """, KeepBuilding: true);

        yield return new Row("computed", """
            module M
            avow to "reject {code: Text}"
              native csharp
                Fail(code);
            """, KeepBuilding: true);

        yield return new Row("line-comment", """
            module M
            avow to "reject {n: Int}"
              native csharp
                // Fail("X");
                return;
            """, KeepBuilding: true);

        yield return new Row("block-comment", """
            module M
            avow to "reject {n: Int}"
              native csharp
                /* Fail("X"); */
                return;
            """, KeepBuilding: true);

        yield return new Row("trailing-comment", """
            module M
            avow to "reject {n: Int}"
              fails with Y
              native csharp
                if (n < 0) Fail("Y"); // not Fail("X")
            """, KeepBuilding: true);

        yield return new Row("raw-string", """"
            module M
            avow to "reject {n: Int}"
              native csharp
                var s = """Fail("X")""";
            """", KeepBuilding: true);

        yield return new Row("qualified-call", """
            module M
            avow to "reject {n: Int}"
              native csharp
                Other.Fail("X");
                MyFail("Z");
            """, KeepBuilding: true);

        yield return new Row("other-target", """
            module M
            avow to "reject {n: Int}"
              fails with Y
              native csharp
                if (n < 0) Fail("Y");
              native python
                raise Fail("X")
            """, KeepBuilding: true);
    }

    private sealed record Row(
        string Id,
        string Source,
        bool KeepBuilding = false,
        string Marker = "",
        string UndeclaredCode = "",
        string Template = "",
        bool Crlf = false);
}
