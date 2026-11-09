using System.Globalization;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: The CLI prints diagnostic codes with the AV prefix and the same numbers (card 361, criterion 1)
//   As the sponsor persona, I want every diagnostic code to use the AV prefix, so that the codes I read,
//   paste and search are Avowry's before the preview publishes them.
//
// Self-reference trap: this file is clean under the live-tree scanner (the old prefix is only ever matched through
// a character class, never written as one token). Expected output is the line 2 of stderr after
// "avowry: Compilation failed:", pinned by CliKeepsDiagnosticShape.
public class AvPrefixedDiagnostics
{
    private static readonly Regex DiagnosticLine = new(
        """^([A-Z]{2})([0-9]{3}) \(([0-9]+):([0-9]+)\) (.+)$""",
        RegexOptions.CultureInvariant);

    private static readonly Regex OldPrefixedCode = new("""P[H][0-9]{3}""", RegexOptions.CultureInvariant);

    [Fact]
    [Trait("Slice", "361")]
    [Trait("Criterion", "361-1")]
    [Trait("Status", "Pending")]
    public async Task The_CLI_prints_AV_codes_with_unchanged_numbers()
    {
        // Given one failing source per emitting site (ids C1 to C9; C5 has a phrase arm and an endpoint arm)
        var failures = new List<string>();

        // When avowry build runs on each row
        foreach (var row in Rows())
        {
            var reason = await Check(row);
            if (reason is not null)
                failures.Add($"{row.Id}: {reason}");
        }

        // Then stderr line 2 is "AVnnn (line:col) message" with the same number as today, and stderr names no old-prefix code
        Assert.True(failures.Count == 0, string.Join("\n", failures));
    }

    private static async Task<string?> Check(Row row)
    {
        using var workspace = TempWorkspace.Create();
        var sourcePath = Path.Combine(workspace.Root, "probe.avowry");
        await File.WriteAllTextAsync(sourcePath, row.Source);
        var result = await PhraseCli.RunAsync("build", sourcePath, "-o", Path.Combine(workspace.Root, "out"));
        var lines = result.Stderr.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n').Split('\n');
        var reasons = new List<string>();

        if (result.ExitCode != 1)
            reasons.Add($"exit {result.ExitCode}");
        if (lines[0] != "avowry: Compilation failed:")
            reasons.Add($"line 1 is '{lines[0]}'");

        var match = lines.Length > 1 ? DiagnosticLine.Match(lines[1]) : Match.Empty;
        if (!match.Success)
        {
            reasons.Add($"stderr line 2 is not 'AVnnn (line:col) message': '{(lines.Length > 1 ? lines[1] : "")}'");
        }
        else
        {
            var prefix = match.Groups[1].Value;
            var digits = match.Groups[2].Value;
            if (prefix != "AV")
                reasons.Add($"expected AV{row.Digits}, actual {prefix}{digits}");
            else if (digits != row.Digits)
                reasons.Add($"expected AV{row.Digits}, actual AV{digits} (the number changed)");

            var line = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
            if (line != row.Line)
                reasons.Add($"line {line} expected {row.Line}");
        }

        var oldCodes = OldPrefixedCode.Matches(result.Stderr).Select(code => code.Value).Distinct().ToList();
        if (oldCodes.Count > 0)
            reasons.Add($"stderr still names {string.Join(", ", oldCodes)}");

        return reasons.Count == 0 ? null : string.Join("; ", reasons);
    }

    private sealed record Row(string Id, string Source, string Digits, int Line);

    private static IEnumerable<Row> Rows()
    {
        // C1 lexer: a tab in indentation
        yield return new Row("C1 lexer-100", "module M\n\ntype LineItem\n\tsku: Text\n", "100", 4);

        // C2 parser: an avow with no implementation (card 324 row M08)
        yield return new Row("C2 parser-203", """
            module Test

            avow to "do nothing"
            """, "203", 3);

        // C3 resolver: a step that matches no avow (card 324 row M13)
        yield return new Row("C3 resolver-001", """
            module Test

            avow to "do nothing"
              native csharp
                return;

            endpoint Probe : POST /probe
              input
                n: Int
              steps
                frobnicate widgets
            """, "001", 11);

        // C4 resolver: two avows with the same wording (card 324 row M01)
        yield return new Row("C4 resolver-010", """
            module Test

            avow to "ping {n: Int}"
              native csharp
                return;

            avow to "ping {m: Int}"
              native csharp
                return;
            """, "010", 7);

        // C5 resolver, through the extension lookup: the phrase arm (CoreArm) and the endpoint arm (WebArm)
        yield return new Row("C5a resolver-ext-330-phrase-arm", """
            module Test

            avow to "add {a: Int} and {b: Int}" -> Int
              native csharp
                return a + b;
              spec "wrong trigger"
                given a = 1
                and b = 2
                when handled
                then returns 3
            """, "330", 6);

        yield return new Row("C5b resolver-ext-330-endpoint-arm", """
            module M

            avow to "ok {n: Int}"
              native csharp
                return;

            endpoint Check : POST /check
              input
                n: Int
              steps
                ok input.n
              spec "wrong trigger"
                given input { n: 1 }
                when called
                then succeeds
            """, "330", 12);

        // C6 adapter: no implementation for the csharp target (card 189 row at line 160)
        yield return new Row("C6 adapter-020", """
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
            """, "020", 3);

        // C7 resolver: an undeclared native Fail (card 202 first failing row)
        yield return new Row("C7 resolver-337", """
            module M
            avow to "reject {n: Int}"
              fails with Declared
              native csharp
                if (n < 0) Fail("X");
            """, "337", 5);

        // C8 adapter: two effects on one avow (card 189 row two-effects)
        yield return new Row("C8 adapter-340", """
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
            """, "340", 3);

        // C9 adapter: a type named like a generated attribute (card 203 first row)
        yield return new Row("C9 adapter-341", "module M\n\ntype AvowAttribute\n  n: Int\n", "341", 3);
    }
}
