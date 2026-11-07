using System.Text;
using System.Text.RegularExpressions;

namespace Avowry.Acceptance.Tests.Support;

internal static class CSharpLiteralScanner
{
    internal sealed record Violation(string File, int Line, string Text);
    internal sealed record ScanResult(IReadOnlyList<Violation> Violations, int ExceptionsMatched, int FilesScanned);

    private static readonly Regex Needle = new("phrase", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly (string File, string Body, int Count)[] Pins =
    [
        ("src/Avowry.Compiler/ExtensionKeywords.cs", "phrase-steps", 1),
        ("src/Avowry.Compiler/ExtensionKeywords.cs", "phrase", 5),
    ];

    public static string RepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "Avowry.sln")))
                return dir.FullName;
        throw new DirectoryNotFoundException("Could not find Avowry.sln above the test output directory.");
    }

    public static IReadOnlyList<(string RelativePath, string Text)> TrackedSourceFiles(string root)
    {
        var src = Path.Combine(root, "src");
        if (!Directory.Exists(src))
            return [];
        var files = new List<(string RelativePath, string Text)>();
        foreach (var path in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
            if (relative.Split('/').Any(part => part is "obj" or "bin"))
                continue;
            files.Add((relative, File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n')));
        }
        files.Sort((left, right) => string.CompareOrdinal(left.RelativePath, right.RelativePath));
        return files;
    }

    public static ScanResult Scan(IEnumerable<(string RelativePath, string Text)> files)
    {
        var list = files.ToList();
        var hits = new List<(string File, int Line, string Body)>();
        foreach (var (path, text) in list)
        {
            var file = path.Replace('\\', '/');
            foreach (var (line, body) in Literals(text))
            {
                if (Needle.IsMatch(body))
                    hits.Add((file, line, body));
            }
        }

        var scanned = list.Select(file => file.RelativePath.Replace('\\', '/')).ToHashSet(StringComparer.Ordinal);
        var consumed = new bool[hits.Count];
        var matched = 0;
        var violations = new List<Violation>();
        foreach (var pin in Pins)
        {
            if (!scanned.Contains(pin.File))
                continue;
            var indices = new List<int>();
            for (var i = 0; i < hits.Count; i++)
            {
                if (!consumed[i] && hits[i].File == pin.File && hits[i].Body == pin.Body)
                    indices.Add(i);
            }
            if (indices.Count < pin.Count)
                violations.Add(new Violation(pin.File, 0, pin.Body));
            var take = Math.Min(indices.Count, pin.Count);
            matched += take;
            for (var k = 0; k < take; k++)
                consumed[indices[k]] = true;
        }
        for (var i = 0; i < hits.Count; i++)
        {
            if (!consumed[i])
                violations.Add(new Violation(hits[i].File, hits[i].Line, hits[i].Body));
        }
        violations.Sort((left, right) =>
            string.CompareOrdinal(left.File, right.File) is var file && file != 0 ? file : left.Line.CompareTo(right.Line));
        return new ScanResult(violations, matched, list.Count);
    }

    private static List<(int Line, string Body)> Literals(string src)
    {
        src = src.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var found = new List<(int Line, string Body)>();
        var i = 0;
        var line = 1;
        var n = src.Length;
        while (i < n)
        {
            if (src[i] == '\n') { line++; i++; continue; }
            if (SkipComment(src, ref i, ref line, n)) continue;
            if (src[i] == '\'') { SkipChar(src, ref i, n); continue; }
            if (TryReadString(src, ref i, ref line, n, out var startLine, out var body))
            {
                found.Add((startLine, body));
                continue;
            }
            i++;
        }
        return found;
    }

    private static bool TryReadString(string src, ref int i, ref int line, int n, out int startLine, out string body)
    {
        startLine = line;
        body = "";
        var p = i;
        var dollars = 0;
        var verbatim = false;
        if (p < n && src[p] == '@')
        {
            verbatim = true;
            p++;
            while (p < n && src[p] == '$') { dollars++; p++; }
        }
        else
        {
            while (p < n && src[p] == '$') { dollars++; p++; }
            if (p < n && src[p] == '@') { verbatim = true; p++; }
        }
        if (p >= n || src[p] != '"')
            return false;
        var quotes = 0;
        var q = p;
        while (q < n && src[q] == '"') { quotes++; q++; }
        startLine = line;
        if (!verbatim && quotes >= 3)
        {
            i = q;
            body = ReadRaw(src, ref i, ref line, n, quotes, dollars);
            return true;
        }
        i = p + 1;
        body = ReadQuoted(src, ref i, ref line, n, verbatim, dollars > 0);
        return true;
    }

    private static string ReadQuoted(string src, ref int i, ref int line, int n, bool verbatim, bool interp)
    {
        var body = new StringBuilder();
        while (i < n)
        {
            var ch = src[i];
            if (interp && ch == '{')
            {
                if (i + 1 < n && src[i + 1] == '{') { body.Append('{'); i += 2; continue; }
                SkipHole(src, ref i, ref line, n);
                continue;
            }
            if (verbatim)
            {
                if (ch == '"' && i + 1 < n && src[i + 1] == '"') { body.Append('"'); i += 2; continue; }
                if (ch == '"') { i++; return body.ToString(); }
            }
            else
            {
                if (ch == '\\' && i + 1 < n) { body.Append(src, i, 2); i += 2; continue; }
                if (ch == '"') { i++; return body.ToString(); }
            }
            if (ch == '\n') line++;
            body.Append(ch);
            i++;
        }
        return body.ToString();
    }

    private static string ReadRaw(string src, ref int i, ref int line, int n, int quotes, int dollars)
    {
        var close = new string('"', quotes);
        var body = new StringBuilder();
        if (i < n && src[i] == '\n') { i++; line++; }
        while (i < n)
        {
            if (src.AsSpan(i).StartsWith(close)) { i += quotes; return body.ToString(); }
            if (dollars > 0 && src[i] == '{')
            {
                var count = 0;
                while (i + count < n && src[i + count] == '{') count++;
                if (count == dollars) { SkipHole(src, ref i, ref line, n); continue; }
            }
            if (src[i] == '\n') line++;
            body.Append(src[i]);
            i++;
        }
        return body.ToString();
    }

    private static void SkipHole(string src, ref int i, ref int line, int n)
    {
        i++;
        var depth = 1;
        while (i < n && depth > 0)
        {
            if (src[i] == '\n') { line++; i++; continue; }
            if (SkipComment(src, ref i, ref line, n)) continue;
            if (src[i] == '\'') { SkipChar(src, ref i, n); continue; }
            if (TryReadString(src, ref i, ref line, n, out _, out _)) continue;
            if (src[i] == '{') depth++;
            else if (src[i] == '}') depth--;
            i++;
        }
    }

    private static bool SkipComment(string src, ref int i, ref int line, int n)
    {
        if (i + 1 >= n || src[i] != '/')
            return false;
        if (src[i + 1] == '/')
        {
            i += 2;
            while (i < n && src[i] != '\n') i++;
            return true;
        }
        if (src[i + 1] != '*')
            return false;
        i += 2;
        while (i + 1 < n && !(src[i] == '*' && src[i + 1] == '/'))
        {
            if (src[i] == '\n') line++;
            i++;
        }
        i = Math.Min(i + 2, n);
        return true;
    }

    private static void SkipChar(string src, ref int i, int n)
    {
        i++;
        while (i < n && src[i] != '\'')
            i += src[i] == '\\' && i + 1 < n ? 2 : 1;
        if (i < n) i++;
    }
}
