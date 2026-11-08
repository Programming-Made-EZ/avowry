using System.Globalization;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

public class UnsupportedCsharpConstructs
{
    private static readonly Regex DiagnosticLine = new(
        """(?m)^(PH\d{3}) \((\d+):(\d+)\) """,
        RegexOptions.CultureInvariant);

    [Fact]
    [Trait("Slice", "189")]
    [Trait("Criterion", "189-1")]
    public async Task Unsupported_csharp_constructs_report_a_diagnostic()
    {
        // Given each in-card unsupported C# construct as an inline source
        var failures = new List<string>();

        // When avowry build runs on each row
        foreach (var row in Rows())
        {
            var reason = await Check(row);
            if (reason is not null)
                failures.Add($"{row.Id}: {reason}");
        }

        // Then every row is a PH diagnostic at the named source line, not a crash
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

        if (result.ExitCode != 1)
            reasons.Add($"exit {result.ExitCode}");
        if (combined.Contains("Unhandled exception", StringComparison.Ordinal))
            reasons.Add("Unhandled exception");
        if (combined.Contains("NotImplementedException", StringComparison.Ordinal))
            reasons.Add("NotImplementedException");
        if (combined.Split('\n').Any(line => line.StartsWith("   at ", StringComparison.Ordinal)))
            reasons.Add("stack at");

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
        var expectedCode = row.Code;
        if (row.OtherMarker is not null)
        {
            var otherLine = LineOf(row.Source, row.OtherMarker);
            var lineOk = line == expectedLine || line == otherLine;
            var codeOk = code == row.Code || code == row.OtherCode;
            if (!lineOk)
                reasons.Add($"line {line} expected {expectedLine} or {otherLine}");
            if (!codeOk)
                reasons.Add($"code {code} expected {row.Code} or {row.OtherCode}");
            var wordsOk = (code == "PH020" && message.Contains("no implementation for target", StringComparison.Ordinal))
                || (code != "PH020"
                    && message.Contains("does not support", StringComparison.Ordinal)
                    && message.Contains("yet", StringComparison.Ordinal));
            if (!wordsOk)
                reasons.Add($"message {message}");
            return reasons.Count == 0 ? null : string.Join("; ", reasons);
        }

        if (line != expectedLine)
            reasons.Add($"line {line} expected {expectedLine}");
        if (code != expectedCode)
            reasons.Add($"code {code} expected {expectedCode}");
        if (expectedCode == "PH020")
        {
            if (!message.Contains("no implementation for target", StringComparison.Ordinal))
                reasons.Add($"message {message}");
        }
        else if (!message.Contains("does not support", StringComparison.Ordinal)
                 || !message.Contains("yet", StringComparison.Ordinal))
        {
            reasons.Add($"message {message}");
        }

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
        yield return new Row("plain-call", """
            module M

            avow to "reject {n: Int}" -> Int
              fails with Rejected
              native csharp
                if (n < 0) Fail("Rejected");
                return n;

            avow to "inner {n: Int}"
              steps
                let checked = reject n

            avow to "outer {n: Int}"
              steps
                inner n

            endpoint Check : POST /check
              input
                n: Int
              steps
                outer input.n
            """, "    inner n", "PH340");

        yield return new Row("composite-fail", """
            module M

            avow to "check {n: Int}"
              fails with Bad
              steps
                fail with Bad

            endpoint Check : POST /check
              input
                n: Int
              steps
                check input.n
            """, "fail with Bad", "PH340");

        yield return new Row("native-python-only", """
            module M

            avow to "double {n: Int}" -> Int
              native python
                return n * 2

            endpoint Check : POST /check
              input
                n: Int
              output
                r: Int
              steps
                let r = double input.n
                respond with { r: r }
            """, "double {n: Int}", "PH020");

        yield return new Row("two-effects", """
            module M

            avow to "ping {n: Int}"
              effects Clock, Mailer
              native csharp
                return;

            endpoint Check : POST /check
              input
                n: Int
              steps
                ping input.n
            """, "ping {n: Int}", "PH340");

        yield return new Row("let-literal-arg", """
            module M

            avow to "reject {n: Int}" -> Int
              native csharp
                return n;

            avow to "inner {n: Int}"
              steps
                let checked = reject 5

            endpoint Check : POST /check
              input
                n: Int
              steps
                inner input.n
            """, "let checked = reject 5", "PH340");

        yield return new Row("spec-none", """
            module M

            avow to "show {x: Option<Text>}" -> Text
              spec "none"
                given x = none
                when called
                then returns "a"
              native csharp
                return "a";

            endpoint Check : POST /check
              input
                n: Int
              output
                r: Int
              steps
                respond with { r: input.n }
            """, @"spec ""none""", "PH340");

        yield return new Row("let-effectful", """
            module M

            type Item
              id: Id
              name: Text

            avow to "find item {id: Id}" -> Item
              effects ItemStore
              native csharp
                return itemstore.Find(id)!;

            avow to "wrap {id: Id}"
              effects ItemStore
              steps
                let item = find item id

            endpoint Check : POST /check
              input
                id: Id
              steps
                wrap input.id
            """, "let item = find item id", "PH340");

        yield return new Row("seed-int", """
            module M

            type Item
              id: Id
              count: Int

            avow to "find item {id: Id}" -> Item
              effects ItemStore
              fails with NotFound
              native csharp
                var item = itemstore.Find(id);
                if (item is null)
                    Fail("NotFound");
                return item;

            avow to "delete item {id: Id}"
              effects ItemStore
              fails with NotFound
              native csharp
                if (!itemstore.Remove(id))
                    Fail("NotFound");

            endpoint Del : POST /items/delete
              input
                id: Id
              steps
                delete item input.id
              spec "deletes"
                given input { id: "a" }
                when handled
                then succeeds
            """, @"spec ""deletes""", "PH340");

        yield return new Row("unused-composite", """
            module M

            avow to "unused fail {n: Int}"
              fails with Bad
              steps
                fail with Bad

            avow to "fine {n: Int}"
              native csharp
                return;

            endpoint Check : POST /check
              input
                n: Int
              steps
                fine input.n
            """, "fail with Bad", "PH340");

        yield return new Row("crlf-copy", """
            module M

            avow to "reject {n: Int}" -> Int
              fails with Rejected
              native csharp
                if (n < 0) Fail("Rejected");
                return n;

            avow to "inner {n: Int}"
              steps
                let checked = reject n

            avow to "outer {n: Int}"
              steps
                inner n

            endpoint Check : POST /check
              input
                n: Int
              steps
                outer input.n
            """, "    inner n", "PH340", Crlf: true);

        yield return new Row("supported-avow-first", """
            module MultiLevel

            avow to "ok {n: Int}"
              native csharp
                return;

            avow to "reject {n: Int}" -> Int
              fails with Rejected
              native csharp
                if (n < 0) Fail("Rejected");
                return n;

            avow to "inner {n: Int}"
              steps
                let checked = reject n

            avow to "outer {n: Int}"
              steps
                inner n

            endpoint Check : POST /check
              input
                n: Int
              steps
                outer input.n
            """, "    inner n", "PH340");

        yield return new Row("two-problems", """
            module M

            avow to "double {n: Int}" -> Int
              native python
                return n * 2

            avow to "check {n: Int}"
              fails with Bad
              steps
                fail with Bad

            endpoint Check : POST /check
              input
                n: Int
              steps
                check input.n
            """, "double {n: Int}", "PH020",
            OtherMarker: "fail with Bad", OtherCode: "PH340");
    }

    private sealed record Row(
        string Id,
        string Source,
        string Marker,
        string Code,
        bool Crlf = false,
        string? OtherMarker = null,
        string? OtherCode = null);
}
