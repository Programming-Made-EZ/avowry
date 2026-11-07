using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Card 217 Criterion 2 (D157): no synchronous process wait is left in the acceptance sources. A blocked test thread starves the thread pool, so every wait goes through
// Support/ProcessRunner (asynchronous). Forbidden, in .cs files of this project: .ReadToEnd(  .WaitForExit( with ANY arguments (a bounded timeout still blocks)  .Result
// .Wait(  and GetAwaiter().GetResult(. The async forms (ReadToEndAsync, WaitForExitAsync, WaitAsync) are allowed.
// Comments and string literals: comments (// and /* */) and the contents of string and char literals (regular, verbatim, interpolated, raw) are BLANKED before matching, with
// newlines kept so line numbers stay true. A mention of a forbidden call in prose or in a string is therefore not a hit, which is also why this file, whose fixtures are strings,
// does not flag itself. Code inside an interpolation hole is blanked with its string (a known gap, stated rather than hidden).
// Exclusions: only generated/build output directories (bin, obj, .git, node_modules, TestResults), as in GeneratedApiSourceScan. Nothing else is exempt.
public class NoBlockingProcessWaits
{
    private static readonly string[] ExcludedDirectories = ["bin", "obj", ".git", "node_modules", "TestResults"];

    // Leading dot or GetAwaiter()., then the exact name ended by a word boundary and an opening parenthesis, so ReadToEndAsync and WaitForExitAsync cannot match.
    private static readonly (string Name, Regex Pattern)[] Forbidden =
    [
        (".ReadToEnd(", new Regex(@"\.\s*ReadToEnd\s*\(")),
        (".WaitForExit(", new Regex(@"\.\s*WaitForExit\s*\(")),
        (".Result", new Regex(@"\.\s*Result\b")),
        (".Wait(", new Regex(@"\.\s*Wait\s*\(")),
        ("GetAwaiter().GetResult(", new Regex(@"GetAwaiter\s*\(\s*\)\s*\.\s*GetResult\s*\(")),
    ];

    private sealed record Source(string Relative, string Text);

    private sealed record Hit(string File, int Line, string Form, string Text);

    [Fact]
    [Trait("Slice", "217")]
    public void Z_a_clean_source_yields_no_diagnostic()
    {
        var text = "class C\n{\n    async Task M(Process p)\n    {\n        var o = await p.StandardOutput.ReadToEndAsync();\n        await p.WaitForExitAsync();\n    }\n}\n";
        Assert.Empty(Find(new Source("tests/Avowry.Acceptance.Tests/Clean.cs", text)));
    }

    [Theory]
    [Trait("Slice", "217")]
    [InlineData("var s = p.StandardOutput.ReadToEnd();", ".ReadToEnd(")]
    [InlineData("p.WaitForExit();", ".WaitForExit(")]
    [InlineData("var r = task.Result;", ".Result")]
    [InlineData("task.Wait();", ".Wait(")]
    [InlineData("task.GetAwaiter().GetResult();", "GetAwaiter().GetResult(")]
    public void O_each_single_forbidden_form_is_caught_with_its_file_and_line(string statement, string form)
    {
        var text = "class C\n{\n    void M()\n    {\n        " + statement + "\n    }\n}\n";
        var found = Find(new Source("tests/Avowry.Acceptance.Tests/One.cs", text));
        var hit = found.First(h => h.Form == form);
        Assert.Equal("tests/Avowry.Acceptance.Tests/One.cs", hit.File);
        Assert.Equal(5, hit.Line);
    }

    [Fact]
    [Trait("Slice", "217")]
    public void M_several_forbidden_forms_in_one_file_are_all_reported_by_line()
    {
        var text = "a\nvar o = p.StandardOutput.ReadToEnd();\nvar e = p.StandardError.ReadToEnd();\nb\np.WaitForExit(5);\nt.Wait();\nvar x = t.Result;\nt.GetAwaiter().GetResult();\n";
        var hits = Find(new Source("tests/Avowry.Acceptance.Tests/Many.cs", text));
        Assert.Equal([2, 3, 5, 6, 7, 8], hits.Select(h => h.Line).Order().Distinct().ToArray());
        var message = Describe(hits);
        Assert.Contains("tests/Avowry.Acceptance.Tests/Many.cs:5", message);
        Assert.Contains("tests/Avowry.Acceptance.Tests/Many.cs:8", message);
    }

    [Theory]
    [Trait("Slice", "217")]
    [InlineData("process.WaitForExit(120000);", 1)]
    [InlineData("process.WaitForExit(180000);", 1)]
    [InlineData("process.WaitForExit(TimeSpan.FromSeconds(5));", 1)]
    [InlineData("process.WaitForExit ( 10 );", 1)]
    [InlineData("var s = await process.StandardOutput.ReadToEndAsync();", 0)]
    [InlineData("await process.WaitForExitAsync();", 0)]
    [InlineData("await process.WaitForExitAsync(cts.Token);", 0)]
    [InlineData("await task.WaitAsync(TimeSpan.FromSeconds(1));", 0)]
    [InlineData("var ResultCount = 1; var r = Results.Ok(); var q = x.ResultSet;", 0)]
    [InlineData("var n = new ReadToEndHelper(); WaitForExit(); Wait();", 0)]
    public void B_bounded_wait_is_forbidden_and_the_async_forms_are_allowed(string statement, int expectedHits)
    {
        Assert.Equal(expectedHits, Find(new Source("tests/Avowry.Acceptance.Tests/B.cs", "void M()\n{\n" + statement + "\n}\n")).Count);
    }

    [Theory]
    [Trait("Slice", "217")]
    [InlineData("// p.WaitForExit(); and x.ReadToEnd();\nvar ok = 1;\n")]
    [InlineData("/* t.Wait();\n   t.Result; */\nvar ok = 1;\n")]
    [InlineData("var s = \"p.WaitForExit(1)\";\n")]
    [InlineData("var s = @\"line one\np.ReadToEnd();\";\n")]
    [InlineData("var s = $\"{a} t.GetAwaiter().GetResult()\";\n")]
    [InlineData("var s = \"quote \\\" p.WaitForExit();\";\n")]
    [InlineData("var c = '\"'; var s = \"t.Wait()\";\n")]
    public void I_forbidden_names_in_comments_and_string_literals_are_not_hits(string text)
    {
        Assert.Empty(Find(new Source("tests/Avowry.Acceptance.Tests/Insurance.cs", text)));
    }

    [Fact]
    [Trait("Slice", "217")]
    public void I_a_comment_or_string_does_not_hide_a_real_wait_that_follows_it_and_lines_stay_true()
    {
        var text = "var s = \"a\\\\\"; // note\n/* c\n c */ p.WaitForExit(1);\nvar t = @\"x\n\"\"y\"; q.Wait();\n";
        var hits = Find(new Source("tests/Avowry.Acceptance.Tests/After.cs", text));
        Assert.Equal([3, 5], hits.Select(h => h.Line).Order().ToArray());
    }

    [Fact]
    [Trait("Slice", "217")]
    public void I_bin_and_obj_output_is_not_scanned_but_a_source_beside_it_is()
    {
        var bad = "void M() { p.WaitForExit(); }\n";
        var hits = Violations(
        [
            new Source("tests/Avowry.Acceptance.Tests/bin/Debug/net10.0/Gen.cs", bad),
            new Source("tests/Avowry.Acceptance.Tests/obj/Gen.cs", bad),
            new Source("tests/Avowry.Acceptance.Tests/TestResults/Gen.cs", bad),
            new Source("tests/Avowry.Acceptance.Tests/Real.cs", bad),
            new Source("tests/Avowry.Acceptance.Tests/Support/Binary.cs", bad),
        ]).ToList();
        Assert.Equal(["tests/Avowry.Acceptance.Tests/Real.cs", "tests/Avowry.Acceptance.Tests/Support/Binary.cs"], hits.Select(h => h.File).Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    [Trait("Slice", "217")]
    public void E_the_guard_does_not_flag_its_own_matcher_and_fixture_text()
    {
        var path = Path.Combine(RepoPaths.Find("tests", "Avowry.Acceptance.Tests"), "NoBlockingProcessWaits.cs");
        Assert.Empty(Find(new Source("tests/Avowry.Acceptance.Tests/NoBlockingProcessWaits.cs", File.ReadAllText(path))));
    }

    [Fact]
    [Trait("Slice", "217")]
    public void M_no_synchronous_process_wait_is_left_in_the_acceptance_sources()
    {
        var hits = Violations(ReadAcceptanceSources()).ToList();
        Assert.True(hits.Count == 0, "synchronous process waits (use Support/ProcessRunner and await):" + Environment.NewLine + Describe(hits));
    }

    private static string Describe(IEnumerable<Hit> hits) =>
        string.Join(Environment.NewLine, hits.OrderBy(h => h.File, StringComparer.Ordinal).ThenBy(h => h.Line).Select(h => $"  {h.File}:{h.Line} [{h.Form}] {h.Text}"));

    private static IEnumerable<Hit> Violations(IEnumerable<Source> sources) =>
        sources.Where(s => s.Relative.EndsWith(".cs", StringComparison.Ordinal) && !s.Relative.Split('/').SkipLast(1).Any(d => ExcludedDirectories.Contains(d, StringComparer.OrdinalIgnoreCase)))
            .SelectMany(Find);

    private static List<Hit> Find(Source source)
    {
        var blanked = Blank(source.Text);
        var original = source.Text.Split('\n');
        var hits = new List<Hit>();
        foreach (var (name, pattern) in Forbidden)
        {
            foreach (Match match in pattern.Matches(blanked))
            {
                var line = blanked.AsSpan(0, match.Index).Count('\n') + 1;
                hits.Add(new Hit(source.Relative, line, name, original[line - 1].Trim()));
            }
        }
        return hits.OrderBy(h => h.Line).ToList();
    }

    private static IEnumerable<Source> ReadAcceptanceSources()
    {
        var project = RepoPaths.Find("tests", "Avowry.Acceptance.Tests");
        var pending = new Stack<string>([project]);
        while (pending.Count > 0)
        {
            var dir = pending.Pop();
            foreach (var sub in Directory.EnumerateDirectories(dir).Where(d => !ExcludedDirectories.Contains(Path.GetFileName(d), StringComparer.OrdinalIgnoreCase)))
                pending.Push(sub);
            foreach (var file in Directory.EnumerateFiles(dir, "*.cs"))
                yield return new Source(Path.GetRelativePath(RepoPaths.Root, file).Replace('\\', '/'), File.ReadAllText(file));
        }
    }

    // Replaces comment text and string/char literal contents with spaces (newlines kept).
    private static string Blank(string t)
    {
        var o = t.ToCharArray();
        void Clear(int from, int to)
        {
            for (var k = from; k < to && k < o.Length; k++)
                if (o[k] != '\n' && o[k] != '\r')
                    o[k] = ' ';
        }

        var i = 0;
        while (i < t.Length)
        {
            var c = t[i];
            if (c == '/' && i + 1 < t.Length && t[i + 1] == '/')
            {
                var end = t.IndexOf('\n', i);
                end = end < 0 ? t.Length : end;
                Clear(i, end);
                i = end;
            }
            else if (c == '/' && i + 1 < t.Length && t[i + 1] == '*')
            {
                var end = t.IndexOf("*/", i + 2, StringComparison.Ordinal);
                end = end < 0 ? t.Length : end + 2;
                Clear(i, end);
                i = end;
            }
            else if (c == '"' && i + 2 < t.Length && t[i + 1] == '"' && t[i + 2] == '"')
            {
                var end = t.IndexOf("\"\"\"", i + 3, StringComparison.Ordinal);
                end = end < 0 ? t.Length : end + 3;
                Clear(i, end);
                i = end;
            }
            else if (c == '"')
            {
                var verbatim = (i >= 1 && t[i - 1] == '@') || (i >= 2 && t[i - 1] == '$' && t[i - 2] == '@');
                var j = i + 1;
                while (j < t.Length)
                {
                    if (verbatim && t[j] == '"' && j + 1 < t.Length && t[j + 1] == '"') { j += 2; continue; }
                    if (!verbatim && t[j] == '\\') { j += 2; continue; }
                    if (t[j] == '"' || (!verbatim && t[j] == '\n')) break;
                    j++;
                }
                j = Math.Min(j + 1, t.Length);
                Clear(i, j);
                i = j;
            }
            else if (c == '\'')
            {
                var j = i + 1;
                while (j < t.Length && t[j] != '\'' && t[j] != '\n')
                    j += t[j] == '\\' ? 2 : 1;
                j = Math.Min(j + 1, t.Length);
                Clear(i, j);
                i = j;
            }
            else
            {
                i++;
            }
        }
        return new string(o);
    }
}
