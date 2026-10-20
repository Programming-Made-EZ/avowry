using System.Text;

namespace Phrase.Compiler.Lexing;

/// <summary>
/// Turns Phrase source into tokens. Works one physical line at a time:
/// <list type="bullet">
///   <item>At the start of a logical line, leading spaces become INDENT/DEDENT (two spaces per level).</item>
///   <item>Blank and comment-only lines are skipped and never change layout.</item>
///   <item>Inside <c>{ }</c> or <c>[ ]</c>, newlines and indentation are ignored (implicit line joining).</item>
///   <item>A logical line that is exactly <c>native &lt;target&gt;</c> captures the indented lines under it as one RAW token.</item>
/// </list>
/// Errors are reported as PH1xx diagnostics and lexing continues.
/// </summary>
public static class Lexer
{
    public static LexResult Lex(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new Scanner(source).Run();
    }

    private sealed class Scanner(string source)
    {
        private const int IndentWidth = 2;

        private readonly string[] _lines = source.Replace("\r\n", "\n").Split('\n');
        private readonly List<Token> _tokens = [];
        private readonly List<Diagnostic> _diagnostics = [];
        private readonly Stack<int> _indents = new([0]);
        private readonly Stack<(char Open, int Line, int Column)> _brackets = new();

        private int _logicalStart;   // index in _tokens where the current logical line began
        private int _logicalIndent;  // indentation of the current logical line

        public LexResult Run()
        {
            var i = 0;
            while (i < _lines.Length)
            {
                var line = _lines[i];
                var lineNo = i + 1;

                if (_brackets.Count == 0)
                {
                    var (indent, contentStart) = MeasureIndent(line, lineNo);
                    if (IsBlankOrComment(line, contentStart)) { i++; continue; }

                    ApplyIndent(indent, lineNo);
                    _logicalStart = _tokens.Count;
                    _logicalIndent = indent;
                    ScanLine(line, lineNo, contentStart);
                }
                else
                {
                    ScanLine(line, lineNo, 0); // continuation inside brackets
                }

                if (_brackets.Count == 0 && _tokens.Count > _logicalStart)
                {
                    _tokens.Add(new Token(TokenKind.Newline, "", lineNo, line.Length + 1));

                    if (IsNativeHeader())
                    {
                        i = CaptureRaw(i + 1, headerLine: lineNo);
                        continue;
                    }
                }

                i++;
            }

            return Finish();
        }

        // ---------- layout ----------

        private (int Indent, int ContentStart) MeasureIndent(string line, int lineNo)
        {
            var indent = 0;
            var p = 0;
            for (; p < line.Length && line[p] is ' ' or '\t'; p++)
            {
                if (line[p] == '\t')
                {
                    if (!IsBlankOrComment(line, p))
                        Report("PH100", "Indentation must use spaces, not tabs.", lineNo, p + 1);
                    indent += IndentWidth; // recover as one level
                }
                else
                {
                    indent++;
                }
            }
            return (indent, p);
        }

        private void ApplyIndent(int indent, int lineNo)
        {
            var top = _indents.Peek();

            if (indent > top)
            {
                if (indent != top + IndentWidth)
                    Report("PH102", $"Indent by exactly {IndentWidth} spaces per level (found {indent - top}).", lineNo, 1);
                _indents.Push(indent);
                _tokens.Add(new Token(TokenKind.Indent, "", lineNo, 1));
                return;
            }

            while (indent < _indents.Peek())
            {
                _indents.Pop();
                _tokens.Add(new Token(TokenKind.Dedent, "", lineNo, 1));
            }

            if (indent != _indents.Peek())
                Report("PH101", $"Dedent to {indent} spaces does not match any open block (expected {_indents.Peek()}).", lineNo, 1);
        }

        private static bool IsBlankOrComment(string line, int from)
        {
            for (var p = from; p < line.Length; p++)
            {
                if (line[p] is ' ' or '\t') continue;
                return line[p] == '#';
            }
            return true;
        }

        // ---------- native blocks ----------

        private bool IsNativeHeader() =>
            _tokens.Count - _logicalStart == 3 // native, target, NL
            && _tokens[_logicalStart] is { Kind: TokenKind.Name, Text: "native" }
            && _tokens[_logicalStart + 1].Kind == TokenKind.Name;

        /// <summary>
        /// Captures every following line indented deeper than the header (blank lines included)
        /// as one RAW token, stripping the block's base indentation. Returns the next line index.
        /// </summary>
        private int CaptureRaw(int start, int headerLine)
        {
            var headerIndent = _logicalIndent;
            var baseIndent = headerIndent + IndentWidth;
            var body = new List<string>();
            int? firstLine = null;
            var lastLine = 0;
            var lastCodeIndex = -1;

            var j = start;
            for (; j < _lines.Length; j++)
            {
                var line = _lines[j];
                if (line.All(c => c is ' ' or '\t')) { body.Add(""); continue; }

                var spaces = line.TakeWhile(c => c == ' ').Count();
                if (spaces <= headerIndent) break;

                if (spaces < baseIndent)
                {
                    Report("PH105", $"Native block lines must be indented at least {baseIndent} spaces.", j + 1, 1);
                    body.Add(line[spaces..]);
                }
                else
                {
                    body.Add(line[baseIndent..]);
                }

                firstLine ??= j + 1;
                lastLine = j + 1;
                lastCodeIndex = body.Count - 1;
            }

            if (firstLine is null)
            {
                Report("PH106", "Native block has no body.", headerLine, headerIndent + 1);
                return j;
            }

            var text = string.Join("\n", body.Take(lastCodeIndex + 1).SkipWhile(l => l.Length == 0));
            _tokens.Add(new Token(TokenKind.Raw, text, firstLine.Value, baseIndent + 1));
            _tokens.Add(new Token(TokenKind.Newline, "", lastLine, _lines[lastLine - 1].Length + 1));
            return j;
        }

        // ---------- tokens on a line ----------

        private void ScanLine(string line, int lineNo, int p)
        {
            while (p < line.Length)
            {
                var c = line[p];
                var column = p + 1;
                var next = p + 1 < line.Length ? line[p + 1] : '\0';

                if (c is ' ' or '\t') { p++; continue; }
                if (c == '#') return;

                if (c == '"') { p = ScanString(line, lineNo, p); continue; }
                if (char.IsAsciiLetterLower(c)) { p = ScanName(line, lineNo, p); continue; }
                if (char.IsAsciiLetterUpper(c)) { p = ScanTypeName(line, lineNo, p); continue; }
                if (char.IsAsciiDigit(c) || (c == '-' && char.IsAsciiDigit(next))) { p = ScanNumber(line, lineNo, p); continue; }

                if (c == '-' && next == '>')
                {
                    Add(TokenKind.Symbol, "->", lineNo, column);
                    p += 2;
                    continue;
                }

                switch (c)
                {
                    case '{' or '[':
                        _brackets.Push((c, lineNo, column));
                        Add(TokenKind.Symbol, c.ToString(), lineNo, column);
                        break;
                    case '}' or ']':
                        CloseBracket(c, lineNo, column);
                        break;
                    case ':' or ',' or '.' or '=' or '<' or '>' or '/':
                        Add(TokenKind.Symbol, c.ToString(), lineNo, column);
                        break;
                    default:
                        Report("PH104", $"Unexpected character '{c}'.", lineNo, column);
                        break;
                }
                p++;
            }
        }

        private void CloseBracket(char close, int lineNo, int column)
        {
            var expectedOpen = close == '}' ? '{' : '[';

            if (_brackets.Count == 0)
            {
                Report("PH109", $"'{close}' has no matching opener.", lineNo, column);
                return;
            }

            var (open, _, _) = _brackets.Pop();
            if (open != expectedOpen)
                Report("PH109", $"'{close}' does not match '{open}'.", lineNo, column);

            Add(TokenKind.Symbol, close.ToString(), lineNo, column);
        }

        private int ScanName(string line, int lineNo, int start)
        {
            var p = start + 1;
            while (p < line.Length)
            {
                var c = line[p];
                if (char.IsAsciiLetterOrDigit(c)) { p++; continue; }
                // Hyphens and apostrophes join a word only when a letter or digit follows ("first-class", "don't", but not "x->").
                if (c is '-' or '\'' && p + 1 < line.Length && char.IsAsciiLetterOrDigit(line[p + 1])) { p++; continue; }
                break;
            }
            Add(TokenKind.Name, line[start..p], lineNo, start + 1);
            return p;
        }

        private int ScanTypeName(string line, int lineNo, int start)
        {
            var p = start + 1;
            while (p < line.Length && char.IsAsciiLetterOrDigit(line[p])) p++;
            Add(TokenKind.TypeName, line[start..p], lineNo, start + 1);
            return p;
        }

        private int ScanNumber(string line, int lineNo, int start)
        {
            var p = start;
            if (line[p] == '-') p++;
            while (p < line.Length && char.IsAsciiDigit(line[p])) p++;
            if (p + 1 < line.Length && line[p] == '.' && char.IsAsciiDigit(line[p + 1]))
            {
                p++;
                while (p < line.Length && char.IsAsciiDigit(line[p])) p++;
            }
            Add(TokenKind.Number, line[start..p], lineNo, start + 1);
            return p;
        }

        private int ScanString(string line, int lineNo, int start)
        {
            var value = new StringBuilder();
            var p = start + 1;

            while (p < line.Length)
            {
                var c = line[p];
                if (c == '"')
                {
                    Add(TokenKind.String, value.ToString(), lineNo, start + 1);
                    return p + 1;
                }

                if (c == '\\' && p + 1 < line.Length)
                {
                    var escaped = line[p + 1];
                    if (escaped is '"' or '\\')
                    {
                        value.Append(escaped);
                    }
                    else
                    {
                        Report("PH107", $"Unknown escape '\\{escaped}'. Only \\\" and \\\\ are defined.", lineNo, p + 1);
                        value.Append(c).Append(escaped);
                    }
                    p += 2;
                    continue;
                }

                value.Append(c);
                p++;
            }

            Report("PH103", "String is not closed before the end of the line.", lineNo, start + 1);
            Add(TokenKind.String, value.ToString(), lineNo, start + 1);
            return p;
        }

        // ---------- end of input ----------

        private LexResult Finish()
        {
            var eofLine = _lines.Length + 1;

            if (_brackets.Count > 0)
            {
                var (open, line, column) = _brackets.Last(); // outermost unclosed
                Report("PH108", $"'{open}' is never closed.", line, column);
                _brackets.Clear();
                if (_tokens.Count > _logicalStart)
                    _tokens.Add(new Token(TokenKind.Newline, "", eofLine, 1));
            }

            while (_indents.Peek() > 0)
            {
                _indents.Pop();
                _tokens.Add(new Token(TokenKind.Dedent, "", eofLine, 1));
            }

            _tokens.Add(new Token(TokenKind.EndOfFile, "", eofLine, 1));
            return new LexResult(_tokens, _diagnostics);
        }

        private void Add(TokenKind kind, string text, int line, int column) =>
            _tokens.Add(new Token(kind, text, line, column));

        private void Report(string code, string message, int line, int column) =>
            _diagnostics.Add(new Diagnostic(code, message, line, column));
    }
}
