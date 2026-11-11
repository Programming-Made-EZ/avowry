using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Slice 417: a test class that asserts a wall-clock limit must run in Collection("Isolated timing").
// Offline text scan of this project (no build, no process). The Code variant blanks comments, string
// contents and char literals; the Text variant blanks comments only. Newlines are kept. Nested types
// count toward their parent. Partial classes merge by name. Fixtures are in-memory strings.
public class WallClockTestsRunIsolated
{
    private const string ThisFile = "WallClockTestsRunIsolated.cs";
    private const string IsolatedName = "Isolated timing";
    private static readonly string[] ExcludedDirectories = ["bin", "obj", ".git", "node_modules", "TestResults"];
    private static readonly string Ela = "Ela" + "psed";
    private static readonly string CtsType = "Cancellation" + "TokenSource";
    private static readonly string DidNotFinish = "did not finish within";
    private static readonly string IsolatedAttr = "Collection(\"" + IsolatedName + "\")";
    private static readonly string XunitIsolatedAttr = "Xunit." + IsolatedAttr;

    private static readonly Regex ClassDecl = new(@"(?<![:,]\s*)\bclass\s+(\w+)", RegexOptions.CultureInvariant);
    private static readonly Regex FactOrTheory = new(@"\[\s*(?:Xunit\s*\.\s*)?(?:Fact|Theory)\b", RegexOptions.CultureInvariant);
    private static readonly Regex IsolatedCollection = new(
        @"(?:^\[|,)\s*(?:Xunit\s*\.\s*)?Collection\s*\(\s*""" + IsolatedName + @"""\s*\)",
        RegexOptions.CultureInvariant);
    private static readonly Regex ElapsedCompare = new(
        @"\b(?:" + Ela + @"|TotalSeconds|TotalMilliseconds)\b(?:\s*\.\s*(?:TotalSeconds|TotalMilliseconds|" + Ela + @")\b)*\s*(?:<=|<(?![<=A-Za-z]))",
        RegexOptions.CultureInvariant);
    private static readonly Regex CtsFromSpan = new(
        @"new\s+" + CtsType + @"\s*\(\s*TimeSpan\s*\.\s*From(?:Seconds|Milliseconds)\s*\(",
        RegexOptions.CultureInvariant);
    private static readonly HashSet<string> Modifiers = new(StringComparer.Ordinal)
    {
        "public", "private", "protected", "internal", "file", "sealed", "static", "abstract", "partial",
    };

    private sealed record Source(string Relative, string Text);
    private sealed record Clocked(string File, string Class, bool Isolated);
    private sealed record Blanked(string Code, string Text);
    private sealed record ClassPart(
        string File, string Name, bool Isolated, bool IsPartial, bool IsTest, bool AssertsLimit);

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void Z1_no_class_asserts_a_wall_clock_limit()
    {
        // Given sources where no test class asserts a wall-clock limit
        var text = "public class Helper { public void T() { " + LimitCompare() + " } }\n" +
                   TestClass("Clean", "Assert.True(true);");

        // When the guard runs
        var found = Find([Src("Z1.cs", text)]);

        // Then it lists nothing
        Assert.Empty(found);
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void O1_elapsed_comparison_without_the_collection_is_named()
    {
        // Given one class that compares Elapsed with a limit and has no Isolated timing attribute
        var sources = new[] { Src("O1.cs", TestClass("Clocked", LimitCompare())) };

        // When Dana runs the guard
        var found = Find(sources);
        var message = Describe(found);

        // Then it names that file and class, and the header line holds no class name
        Assert.Equal(["tests/Avowry.Acceptance.Tests/O1.cs: Clocked"], LinesOf(found));
        Assert.Contains("tests/Avowry.Acceptance.Tests/O1.cs: Clocked", message, StringComparison.Ordinal);
        Assert.DoesNotContain("Clocked", message.Split('\n')[0], StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void O2_cancellation_token_limit_without_the_collection_is_named()
    {
        // Given one class that creates a CancellationTokenSource with TimeSpan.FromSeconds and fails with did not finish within
        var sources = new[] { Src("O2.cs", TestClass("Capped", CtsBody())) };

        // When Dana runs the guard
        var found = Find(sources);

        // Then it names that file and class
        Assert.Equal(["tests/Avowry.Acceptance.Tests/O2.cs: Capped"], LinesOf(found));
    }

    public static TheoryData<string, string> O3Rows() => new()
    {
        { "above", "[" + IsolatedAttr + "]\n" },
        { "same-line", "[" + IsolatedAttr + "] " },
        { "comma-list", "[" + IsolatedAttr + ", Trait(\"Slice\", \"1\")]\n" },
        { "after-a-comment", "// keep the attribute\n[" + IsolatedAttr + "]\n" },
        { "xunit-qualified", "[" + XunitIsolatedAttr + "]\n" },
    };

    [Theory]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    [MemberData(nameof(O3Rows))]
    public void O3_accepted_collection_spellings_and_placements_are_not_named(string id, string prefix)
    {
        // Given a wall-clock class whose Isolated timing attribute is placed as this row
        var sources = new[] { Src("O3-" + id + ".cs", TestClass("Clocked", LimitCompare(), prefix)) };

        // When the guard runs
        var found = Find(sources);

        // Then the class is not named
        Assert.True(found.Count == 0, id + " was named:\n" + Describe(found));
    }

    public static TheoryData<string, string, bool> BRows() => new()
    {
        { "comment", TestClass("Near", "// sw." + Ela + " < TimeSpan.FromSeconds(1)\n"), false },
        { "string", TestClass("Near", "var s = \"sw." + Ela + " < TimeSpan.FromSeconds(1)\";\n"), false },
        { "timeout-ms-argument", TestClass("Near", "var a = new[] { \"--timeout-ms\", \"20000\" };\n"), false },
        { "wait-for-exit-limit", TestClass("Near", "await process.WaitForExitAsync(TimeSpan.FromSeconds(5));\n"), false },
        { "wrong-case-collection", TestClass("Clocked", LimitCompare(), "[Collection(\"Isolated Timing\")]\n"), true },
        { "bench-processes", TestClass("Clocked", LimitCompare(), "[Collection(\"Bench processes\")]\n"), true },
        { "collection-attribute", TestClass("Clocked", LimitCompare(), "[CollectionAttribute(\"Isolated timing\")]\n"), true },
        { "constant", "const string N = \"Isolated timing\";\n[Collection(N)]\n" + TestClass("Clocked", LimitCompare()), true },
        { "commented-out", "// [" + IsolatedAttr + "]\n" + TestClass("Clocked", LimitCompare()), true },
    };

    [Theory]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    [MemberData(nameof(BRows))]
    public void B_near_misses_are_named_only_when_the_collection_is_wrong(string id, string text, bool named)
    {
        // Given a class that mentions Elapsed only in a comment or a string, uses a TimeSpan as a process timeout, or carries a different collection name
        var sources = new[] { Src("B-" + id + ".cs", text) };

        // When the guard runs
        var found = Find(sources);

        // Then the first two kinds are not named and a different collection name is
        Assert.True(named == found.Count > 0, id + ": expected " + (named ? "a name" : "no name") + ", got:\n" + Describe(found));
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void I_string_literal_forms_and_a_class_inside_a_raw_string_are_not_false_hits()
    {
        // Given every string literal form holding Elapsed, a class declaration inside a raw string, and one real wall-clock class
        var q3 = new string('"', 3);
        var phantom = "var s = " + q3 + "\npublic class Phantom\n{\n    [Fact] public void T() { " + LimitCompare() + " }\n}\n" + q3 + ";\n";
        var text = StringFormsClass() + phantom + TestClass("Clocked", LimitCompare());

        // When the guard runs
        var found = Find([Src("I-strings.cs", text)]);

        // Then only the real class is named
        Assert.Equal(["tests/Avowry.Acceptance.Tests/I-strings.cs: Clocked"], LinesOf(found));
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void I_several_classes_in_one_file_are_judged_separately()
    {
        // Given several classes in one file, two of which assert a wall-clock limit
        var text = TestClass("Alpha", LimitCompare()) + TestClass("Beta", "Assert.True(true);") + TestClass("Gamma", LimitCompare());

        // When the guard runs
        var found = Find([Src("Many.cs", text)]);

        // Then each offender is named on its own line, sorted by class
        Assert.Equal(
            [
                "tests/Avowry.Acceptance.Tests/Many.cs: Alpha",
                "tests/Avowry.Acceptance.Tests/Many.cs: Gamma",
            ],
            LinesOf(found));
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void I_a_partial_class_is_merged_across_files()
    {
        // Given a partial class split across files, one part carrying Isolated timing, and another partial with no attribute
        var withAttr = "[" + IsolatedAttr + "]\npublic partial class Split\n{\n}\n";
        var withLimit = "public partial class Split\n{\n    [Fact] public void T() { " + LimitCompare() + " }\n}\n";
        var other = "public partial class Other\n{\n    [Fact] public void T() { " + LimitCompare() + " }\n}\n";
        var otherTail = "public partial class Other\n{\n}\n";

        // When the guard runs
        var accepted = Find([Src("SplitA.cs", withLimit), Src("SplitB.cs", withAttr)]);
        var named = Find([Src("ZOther.cs", otherTail), Src("AOther.cs", other)]);

        // Then the attributed partial is not named, and the other is reported once at the first part in ordinal path order
        Assert.Empty(accepted);
        Assert.Equal(["tests/Avowry.Acceptance.Tests/AOther.cs: Other"], LinesOf(named));
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void I_bin_and_obj_copies_are_skipped()
    {
        // Given wall-clock classes under bin, obj and TestResults, and two real sources beside them
        var body = TestClass("Copy", LimitCompare());
        var sources = new[]
        {
            new Source("tests/Avowry.Acceptance.Tests/bin/Debug/net8.0/Gen.cs", body),
            new Source("tests/Avowry.Acceptance.Tests/obj/Gen.cs", body),
            new Source("tests/Avowry.Acceptance.Tests/TestResults/Gen.cs", body),
            Src("Real.cs", TestClass("Real", LimitCompare())),
            new Source("tests/Avowry.Acceptance.Tests/Support/Kept.cs", TestClass("Kept", LimitCompare())),
        };

        // When the guard runs
        var found = Find(sources);

        // Then only the real sources are named
        Assert.Equal(
            [
                "tests/Avowry.Acceptance.Tests/Real.cs: Real",
                "tests/Avowry.Acceptance.Tests/Support/Kept.cs: Kept",
            ],
            LinesOf(found));
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void I_the_guard_file_is_not_named()
    {
        // Given the guard's own file
        var path = Path.Combine(RepoPaths.Find("tests", "Avowry.Acceptance.Tests"), ThisFile);

        // When the guard scans it
        var found = Find([new Source("tests/Avowry.Acceptance.Tests/" + ThisFile, File.ReadAllText(path))]);

        // Then the guard's own file is not named
        Assert.Empty(found);
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-1")]
    [Trait("Kind", "BornGreen")]
    public void L2_live_tree_detected_set_contains_BenchTimeoutIsolation()
    {
        // Given the repository as it is today
        // When the guard detects classes that assert a wall-clock limit
        var detected = Detect(ReadAcceptanceSources());

        // Then the set contains BenchTimeoutIsolation (the class that holds the timing test after the green; before it, BenchmarkHarnessAcceptance), so a scanner that never matches cannot pass L1 forever
        Assert.Contains("BenchTimeoutIsolation", detected.Select(c => c.Class));
    }

    [Fact]
    [Trait("Slice", "417")]
    [Trait("Criterion", "417-2")]
    public void L1_live_tree_has_no_wall_clock_class_outside_isolated_timing()
    {
        // Given the repository as it is today
        // When the guard runs
        var found = Find(ReadAcceptanceSources());

        // Then every class that asserts a wall-clock limit carries Collection("Isolated timing")
        Assert.True(found.Count == 0, Describe(found));
    }

    private static string LimitCompare() =>
        "Assert.True(sw." + Ela + " < TimeSpan.FromSeconds(20));";

    private static string CtsBody() =>
        "using var limit = new " + CtsType + "(TimeSpan.FromSeconds(20));\n        throw new Exception(\"" + DidNotFinish + " 20 seconds\");";

    private static string TestClass(string name, string statements, string prefix = "") =>
        prefix + "public class " + name + "\n{\n    [Fact]\n    public void T()\n    {\n        " + statements + "\n    }\n}\n";

    private static string StringFormsClass()
    {
        var q3 = new string('"', 3);
        return "public class Strings\n{\n    [Fact] public void T()\n    {\n" +
               "        var a = \"sw." + Ela + " < 1\";\n" +
               "        var b = \"quote \\\" sw." + Ela + " < 1\";\n" +
               "        var c = @\"sw." + Ela + " < 1\";\n" +
               "        var d = $\"sw." + Ela + " < 1\";\n" +
               "        var e = $@\"sw." + Ela + " < 1\";\n" +
               "        var f = @$\"sw." + Ela + " < 1\";\n" +
               "        var g = " + q3 + "sw." + Ela + " < 1" + q3 + ";\n" +
               "        var h = $$" + q3 + "sw." + Ela + " < 1" + q3 + ";\n" +
               "        var i = '<';\n" +
               "        var j = $\"n={n}\";\n" +
               "    }\n}\n";
    }

    private static Source Src(string file, string text) =>
        new("tests/Avowry.Acceptance.Tests/" + file, text);

    private static string[] LinesOf(IEnumerable<Clocked> found) =>
        found.Select(c => c.File + ": " + c.Class).ToArray();

    private static string Describe(IEnumerable<Clocked> found)
    {
        var lines = new List<string> { "a test class that asserts a wall-clock limit must carry Collection(\"" + IsolatedName + "\"):" };
        foreach (var item in found)
            lines.Add("  " + item.File + ": " + item.Class);
        return string.Join("\n", lines);
    }

    private static List<Clocked> Find(IEnumerable<Source> sources) =>
        Detect(sources).Where(c => !c.Isolated).ToList();

    private static List<Clocked> Detect(IEnumerable<Source> sources)
    {
        var parts = new List<ClassPart>();
        foreach (var source in sources)
        {
            if (!source.Relative.EndsWith(".cs", StringComparison.Ordinal))
                continue;
            var segments = source.Relative.Replace('\\', '/').Split('/');
            if (segments.SkipLast(1).Any(d => ExcludedDirectories.Contains(d, StringComparer.OrdinalIgnoreCase)))
                continue;
            parts.AddRange(Parse(source));
        }

        var clocked = new List<Clocked>();
        foreach (var group in parts.GroupBy(p => p.Name))
        {
            if (group.Any(p => p.IsPartial))
            {
                var ordered = group.OrderBy(p => p.File, StringComparer.Ordinal).ToList();
                if (ordered.Any(p => p.IsTest) && ordered.Any(p => p.AssertsLimit))
                    clocked.Add(new Clocked(ordered[0].File, group.Key, ordered.Any(p => p.Isolated)));
            }
            else
            {
                foreach (var part in group)
                {
                    if (part.IsTest && part.AssertsLimit)
                        clocked.Add(new Clocked(part.File, part.Name, part.Isolated));
                }
            }
        }

        return clocked
            .OrderBy(c => c.File, StringComparer.Ordinal)
            .ThenBy(c => c.Class, StringComparer.Ordinal)
            .ToList();
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

    private static List<ClassPart> Parse(Source source)
    {
        var blanked = Blank(source.Text);
        var matches = ClassDecl.Matches(blanked.Code);
        var spans = new List<(int Start, int Open, int Close, string Name)>();
        foreach (Match match in matches)
        {
            var open = IndexOfBody(blanked.Code, match.Index + match.Length);
            if (open < 0)
                continue;
            var close = EndOfBrace(blanked.Code, open);
            if (close < 0)
                continue;
            spans.Add((match.Index, open, close, match.Groups[1].Value));
        }

        var top = spans.Where(s => !spans.Any(p => p.Start < s.Start && p.Close > s.Close)).ToList();
        var parts = new List<ClassPart>();
        foreach (var span in top)
        {
            var (isolated, partial) = ReadAttributes(blanked.Code, blanked.Text, span.Start);
            var codeBody = blanked.Code[span.Open..(span.Close + 1)];
            var textBody = blanked.Text[span.Open..(span.Close + 1)];
            parts.Add(new ClassPart(
                source.Relative,
                span.Name,
                isolated,
                partial,
                FactOrTheory.IsMatch(codeBody),
                AssertsLimit(codeBody, textBody)));
        }

        return parts;
    }

    private static bool AssertsLimit(string codeBody, string textBody) =>
        ElapsedCompare.IsMatch(codeBody) ||
        (CtsFromSpan.IsMatch(codeBody) && textBody.Contains(DidNotFinish, StringComparison.Ordinal));

    private static (bool Isolated, bool Partial) ReadAttributes(string code, string text, int classIndex)
    {
        var isolated = false;
        var partial = false;
        var i = classIndex;
        while (i > 0)
        {
            var j = i;
            while (j > 0 && char.IsWhiteSpace(code[j - 1]))
                j--;
            if (j == 0)
                break;
            if (code[j - 1] == ']')
            {
                var depth = 1;
                var k = j - 2;
                while (k >= 0 && depth > 0)
                {
                    if (code[k] == ']')
                        depth++;
                    else if (code[k] == '[')
                        depth--;
                    k--;
                }

                var start = k + 1;
                if (IsolatedCollection.IsMatch(text[start..j]))
                    isolated = true;
                i = start;
                continue;
            }

            if (char.IsLetter(code[j - 1]))
            {
                var k = j - 1;
                while (k >= 0 && char.IsLetter(code[k]))
                    k--;
                var word = code[(k + 1)..j];
                if (word == "partial")
                    partial = true;
                if (Modifiers.Contains(word))
                {
                    i = k + 1;
                    continue;
                }
            }

            break;
        }

        return (isolated, partial);
    }

    private static int IndexOfBody(string code, int from)
    {
        var angle = 0;
        var paren = 0;
        for (var i = from; i < code.Length; i++)
        {
            var c = code[i];
            if (c == '{' && angle == 0 && paren == 0)
                return i;
            if (c == '<')
                angle++;
            else if (c == '>' && angle > 0)
                angle--;
            else if (c == '(')
                paren++;
            else if (c == ')' && paren > 0)
                paren--;
            else if (c == ';' && angle == 0 && paren == 0)
                return -1;
        }

        return -1;
    }

    private static int EndOfBrace(string code, int open)
    {
        var depth = 0;
        for (var i = open; i < code.Length; i++)
        {
            if (code[i] == '{')
                depth++;
            else if (code[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private static Blanked Blank(string source)
    {
        source = source.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var code = source.ToCharArray();
        var text = source.ToCharArray();
        var i = 0;
        var n = source.Length;
        while (i < n)
        {
            if (TryComment(source, code, text, ref i, n))
                continue;
            if (TryChar(source, code, ref i, n))
                continue;
            if (TryString(source, code, text, ref i, n))
                continue;
            i++;
        }

        return new Blanked(new string(code), new string(text));
    }

    private static void Clear(char[] dest, int from, int to)
    {
        for (var k = from; k < to && k < dest.Length; k++)
        {
            if (dest[k] != '\n')
                dest[k] = ' ';
        }
    }

    private static bool TryComment(string src, char[] code, char[] text, ref int i, int n)
    {
        if (i + 1 >= n || src[i] != '/')
            return false;
        if (src[i + 1] == '/')
        {
            var start = i;
            i += 2;
            while (i < n && src[i] != '\n')
                i++;
            Clear(code, start, i);
            Clear(text, start, i);
            return true;
        }

        if (src[i + 1] != '*')
            return false;
        var from = i;
        i += 2;
        while (i + 1 < n && !(src[i] == '*' && src[i + 1] == '/'))
            i++;
        i = Math.Min(i + 2, n);
        Clear(code, from, i);
        Clear(text, from, i);
        return true;
    }

    private static bool TryChar(string src, char[] code, ref int i, int n)
    {
        if (src[i] != '\'')
            return false;
        var start = i;
        i++;
        while (i < n && src[i] != '\'' && src[i] != '\n')
            i += src[i] == '\\' && i + 1 < n ? 2 : 1;
        if (i < n && src[i] == '\'')
            i++;
        Clear(code, start, i);
        return true;
    }

    private static bool TryString(string src, char[] code, char[] text, ref int i, int n)
    {
        var p = i;
        var dollars = 0;
        var verbatim = false;
        if (p < n && src[p] == '@')
        {
            verbatim = true;
            p++;
            while (p < n && src[p] == '$')
            {
                dollars++;
                p++;
            }
        }
        else
        {
            while (p < n && src[p] == '$')
            {
                dollars++;
                p++;
            }

            if (p < n && src[p] == '@')
            {
                verbatim = true;
                p++;
            }
        }

        if (p >= n || src[p] != '"')
            return false;

        var quotes = 0;
        var q = p;
        while (q < n && src[q] == '"')
        {
            quotes++;
            q++;
        }

        if (!verbatim && quotes >= 3)
        {
            i = q;
            ReadRaw(src, code, text, ref i, n, quotes, dollars);
            return true;
        }

        i = p + 1;
        ReadQuoted(src, code, text, ref i, n, verbatim, dollars > 0);
        return true;
    }

    private static void ReadQuoted(string src, char[] code, char[] text, ref int i, int n, bool verbatim, bool interp)
    {
        var contentStart = i;
        while (i < n)
        {
            var ch = src[i];
            if (interp && ch == '{')
            {
                if (i + 1 < n && src[i + 1] == '{')
                {
                    i += 2;
                    continue;
                }

                Clear(code, contentStart, i);
                SkipHole(src, code, text, ref i, n);
                contentStart = i;
                continue;
            }

            if (verbatim)
            {
                if (ch == '"' && i + 1 < n && src[i + 1] == '"')
                {
                    i += 2;
                    continue;
                }

                if (ch == '"')
                {
                    Clear(code, contentStart, i);
                    i++;
                    return;
                }
            }
            else
            {
                if (ch == '\\' && i + 1 < n)
                {
                    i += 2;
                    continue;
                }

                if (ch == '"' || ch == '\n')
                {
                    Clear(code, contentStart, i);
                    if (ch == '"')
                        i++;
                    return;
                }
            }

            i++;
        }

        Clear(code, contentStart, i);
    }

    private static void ReadRaw(string src, char[] code, char[] text, ref int i, int n, int quotes, int dollars)
    {
        var close = new string('"', quotes);
        var contentStart = i;
        while (i < n)
        {
            if (src.AsSpan(i).StartsWith(close))
            {
                Clear(code, contentStart, i);
                i += quotes;
                return;
            }

            if (dollars > 0 && src[i] == '{')
            {
                var count = 0;
                while (i + count < n && src[i + count] == '{')
                    count++;
                if (count == dollars)
                {
                    Clear(code, contentStart, i);
                    SkipHole(src, code, text, ref i, n);
                    contentStart = i;
                    continue;
                }
            }

            i++;
        }

        Clear(code, contentStart, i);
    }

    private static void SkipHole(string src, char[] code, char[] text, ref int i, int n)
    {
        i++;
        var depth = 1;
        while (i < n && depth > 0)
        {
            if (src[i] == '{')
            {
                depth++;
                i++;
                continue;
            }

            if (src[i] == '}')
            {
                depth--;
                i++;
                continue;
            }

            if (TryComment(src, code, text, ref i, n))
                continue;
            if (TryChar(src, code, ref i, n))
                continue;
            if (TryString(src, code, text, ref i, n))
                continue;
            i++;
        }
    }
}
