using System.Text;
using Phrase.Compiler.Lexing;

namespace Phrase.Compiler.Syntax;

/// <summary>
/// Builds the syntax tree from tokens. Two passes:
/// <list type="number">
///   <item>Group tokens into a tree of logical lines (INDENT/DEDENT become children; a RAW token attaches to the <c>native</c> line above it).</item>
///   <item>Parse each line by its first word. Keywords are recognised only there, so phrase sentences may use any word.</item>
/// </list>
/// Errors are PH2xx diagnostics. A bad line is skipped along with its children; the rest of the file still parses.
/// </summary>
public static class Parser
{
    public static readonly IReadOnlySet<string> Targets = new HashSet<string>(StringComparer.Ordinal) { "csharp", "typescript", "python" };
    public static readonly IReadOnlySet<string> HttpVerbs = ExtensionKeywords.HttpVerbs;

    public static ParseResult Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return Parse(Lexer.Lex(source));
    }

    public static ParseResult Parse(LexResult lexed)
    {
        ArgumentNullException.ThrowIfNull(lexed);
        return new Impl(lexed).Run();
    }

    // ===================================================================== line tree

    private sealed class Line
    {
        public required Position Start { get; init; }
        public List<Token> Tokens { get; } = [];
        public List<Line> Children { get; set; } = [];
        public Token? Raw { get; set; }
    }

    private static List<Line> BuildLines(IReadOnlyList<Token> tokens)
    {
        var i = 0;
        return ReadBlock();

        List<Line> ReadBlock()
        {
            var lines = new List<Line>();
            while (i < tokens.Count)
            {
                var t = tokens[i];
                switch (t.Kind)
                {
                    case TokenKind.EndOfFile:
                        return lines;
                    case TokenKind.Dedent:
                        i++;
                        return lines;
                    case TokenKind.Indent: // indented with nothing above it (e.g. first line of file)
                        i++;
                        lines.Add(new Line { Start = Pos(t), Children = ReadBlock() });
                        continue;
                }

                var line = new Line { Start = Pos(t) };
                while (i < tokens.Count && tokens[i].Kind is not (TokenKind.Newline or TokenKind.EndOfFile or TokenKind.Indent or TokenKind.Dedent))
                    line.Tokens.Add(tokens[i++]);
                if (i < tokens.Count && tokens[i].Kind == TokenKind.Newline) i++;

                if (line.Tokens is [{ Kind: TokenKind.Raw } raw])
                {
                    if (lines.Count > 0) lines[^1].Raw = raw;
                    continue;
                }

                if (i < tokens.Count && tokens[i].Kind == TokenKind.Indent)
                {
                    i++;
                    line.Children = ReadBlock();
                }
                lines.Add(line);
            }
            return lines;
        }
    }

    // ===================================================================== cursor

    private sealed class SyntaxError(Diagnostic diagnostic) : Exception(diagnostic.Message)
    {
        public Diagnostic Diagnostic { get; } = diagnostic;
    }

    /// <summary>Reads one logical line. Errors throw <see cref="SyntaxError"/> with this cursor's code.</summary>
    private sealed class Cursor(IReadOnlyList<Token> tokens, Position end, string code = "PH200")
    {
        private int _i;

        public bool AtEnd => _i >= tokens.Count;
        public Token? Peek(int k = 0) => _i + k < tokens.Count ? tokens[_i + k] : null;
        public bool Is(TokenKind kind, int k = 0) => Peek(k)?.Kind == kind;
        public bool IsName(string text, int k = 0) => Peek(k) is { Kind: TokenKind.Name } t && t.Text == text;
        public bool IsSymbol(string text, int k = 0) => Peek(k) is { Kind: TokenKind.Symbol } t && t.Text == text;
        public Position Here => Peek() is { } t ? Pos(t) : end;

        public Token Next() => AtEnd ? throw Error("more input") : tokens[_i++];

        public bool TryConsume(string symbol)
        {
            if (!IsSymbol(symbol)) return false;
            _i++;
            return true;
        }

        public Token Expect(TokenKind kind, string what) =>
            Is(kind) ? tokens[_i++] : throw Error(what);

        public Token ExpectSymbol(string symbol) =>
            IsSymbol(symbol) ? tokens[_i++] : throw Error($"'{symbol}'");

        public void ExpectEnd()
        {
            if (!AtEnd) throw new SyntaxError(new Diagnostic(code, $"Unexpected {Describe(Peek())}; expected end of line.", Here.Line, Here.Column));
        }

        public SyntaxError Error(string expected) =>
            new(new Diagnostic(code, $"Expected {expected}, found {Describe(Peek())}.", Here.Line, Here.Column));

        private static string Describe(Token? t) => t switch
        {
            null => "end of line",
            { Kind: TokenKind.String } => $"string \"{t.Text}\"",
            { Kind: TokenKind.Raw } => "a native block",
            _ => $"'{t.Text}'",
        };
    }

    private static Position Pos(Token t) => new(t.Line, t.Column);

    private static Position EndOf(Line line)
    {
        if (line.Tokens.Count == 0) return line.Start;
        var last = line.Tokens[^1];
        var width = last.Kind == TokenKind.String ? last.Text.Length + 2 : last.Text.Length;
        return new Position(last.Line, last.Column + Math.Max(1, width));
    }

    // ===================================================================== parser

    private sealed class Impl(LexResult lexed)
    {
        private readonly List<Diagnostic> _diagnostics = [.. lexed.Diagnostics];
        private bool _web;

        public ParseResult Run()
        {
            ModuleDecl? module = null;
            var types = new List<TypeDecl>();
            var phrases = new List<PhraseDecl>();
            var endpoints = new List<EndpointDecl>();
            var sawDeclaration = false;

            foreach (var line in BuildLines(lexed.Tokens))
            {
                if (line.Tokens.Count == 0)
                {
                    NoChildren(line);
                    continue;
                }

                var head = line.Tokens[0];
                var keyword = head.Kind == TokenKind.Name ? head.Text : null;
                if (keyword == "uses" && IsUsesWeb(line))
                {
                    if (!ExtensionKeywords.Is(ExtensionPart.Core, "uses"))
                        throw new InvalidOperationException("The explicit extension load 'uses' is a core keyword.");
                    _web = true;
                    NoChildren(line);
                    continue;
                }

                switch (keyword)
                {
                    case "module":
                        if (Try(() => ParseModule(line)) is not { } m) break;
                        if (module is not null) Report("PH202", "A file has exactly one module declaration.", m.At);
                        else if (sawDeclaration) Report("PH202", "The module declaration must come before every other declaration.", m.At);
                        module ??= m;
                        break;
                    case "type":
                        sawDeclaration = true;
                        AddIfParsed(types, () => ParseType(line));
                        break;
                    case "avow":
                        sawDeclaration = true;
                        AddIfParsed(phrases, () => ParsePhrase(line));
                        break;
                    case "endpoint" when ExtensionKeywords.Is(ExtensionPart.Web, "endpoint"):
                        _web = true;
                        sawDeclaration = true;
                        AddIfParsed(endpoints, () => ParseEndpoint(line));
                        break;
                    default:
                        Report("PH201", $"Expected a declaration (module, type, avow or endpoint), found '{head.Text}'.", Pos(head));
                        break;
                }
            }

            if (module is null && !_diagnostics.Any(d => d.Code == "PH202"))
                Report("PH202", "A file must start with `module <Name>`.", new Position(1, 1));

            return new ParseResult(new SourceFile(module, types, phrases, endpoints), _diagnostics)
            {
                ActiveExtensions = _web ? ExtensionKeywords.CoreAndWeb : ExtensionKeywords.CoreOnly,
            };
        }

        private static bool IsUsesWeb(Line line) =>
            line.Tokens.Count == 2 && line.Tokens[1] is { Kind: TokenKind.Name, Text: "web" };

        // ------------------------------------------------------------- declarations

        private ModuleDecl ParseModule(Line line)
        {
            var c = CursorFor(line);
            var at = Pos(c.Next());
            var name = c.Expect(TokenKind.TypeName, "a module name (Capitalized)");
            c.ExpectEnd();
            NoChildren(line);
            return new ModuleDecl(name.Text, at);
        }

        private TypeDecl ParseType(Line line)
        {
            var c = CursorFor(line);
            var at = Pos(c.Next());
            var name = c.Expect(TokenKind.TypeName, "a type name (Capitalized)");
            c.ExpectEnd();
            return new TypeDecl(name.Text, ParseFieldBlock(line, $"type {name.Text}"), at);
        }

        private List<FieldDecl> ParseFieldBlock(Line header, string owner)
        {
            if (header.Children.Count == 0)
                Report("PH203", $"{owner} has no fields; indent `name: Type` lines under it.", header.Start);

            var fields = new List<FieldDecl>();
            foreach (var child in header.Children)
                AddIfParsed(fields, () => ParseField(child));
            return fields;
        }

        private FieldDecl ParseField(Line line)
        {
            var c = CursorFor(line);
            var name = c.Expect(TokenKind.Name, "a field name (lowercase)");
            c.ExpectSymbol(":");
            var type = ParseTypeRef(c);
            c.ExpectEnd();
            NoChildren(line);
            return new FieldDecl(name.Text, type, Pos(name));
        }

        private static TypeRef ParseTypeRef(Cursor c)
        {
            var name = c.Expect(TokenKind.TypeName, "a type name");
            var args = new List<TypeRef>();
            if (c.TryConsume("<"))
            {
                do args.Add(ParseTypeRef(c));
                while (c.TryConsume(","));
                c.ExpectSymbol(">");
            }
            return new TypeRef(name.Text, args, Pos(name));
        }

        // ------------------------------------------------------------- phrases

        private PhraseDecl ParsePhrase(Line line)
        {
            var c = CursorFor(line);
            var at = Pos(c.Next());
            if (c.Is(TokenKind.Name))
            {
                var filler = c.Next();
                if (filler.Text is not ("to" or "that" or "it"))
                    throw FillerError("PH212", filler);
                if (c.Is(TokenKind.Name))
                    throw FillerError("PH213", c.Peek()!);
            }
            var templateToken = c.Expect(TokenKind.String, "a quoted phrase template");
            TypeRef? returns = null;
            if (c.TryConsume("->")) returns = ParseTypeRef(c);
            c.ExpectEnd();

            var template = ParseTemplate(templateToken);
            var effects = new List<string>();
            var failures = new List<string>();
            var failureLine = 0;
            var specs = new List<SpecDecl>();
            var natives = new List<NativeBlock>();
            List<Step>? steps = null;
            bool sawEffects = false, sawFailures = false, sawNative = false, sawSteps = false;

            foreach (var child in line.Children)
            {
                Try(() =>
                {
                    var cc = CursorFor(child);
                    var clauseAt = cc.Here;

                    if (cc.IsName("effects"))
                    {
                        cc.Next();
                        if (sawEffects) { Report("PH204", "Only one `effects` clause is allowed.", clauseAt); return; }
                        sawEffects = true;
                        if (cc.IsName("none")) { cc.Next(); cc.ExpectEnd(); }
                        else effects.AddRange(ParseTypeNameList(cc, "an effect name"));
                        NoChildren(child);
                    }
                    else if (cc.IsName("fails") && cc.IsName("with", 1))
                    {
                        cc.Next(); cc.Next();
                        if (sawFailures) { Report("PH204", "List every failure in one `fails with A, B` clause.", clauseAt); return; }
                        sawFailures = true;
                        failureLine = clauseAt.Line;
                        failures.AddRange(ParseTypeNameList(cc, "a failure code"));
                        NoChildren(child);
                    }
                    else if (cc.IsName("spec"))
                    {
                        specs.Add(ParseSpec(child, cc));
                    }
                    else if (cc.IsName("native"))
                    {
                        cc.Next();
                        var target = cc.Expect(TokenKind.Name, "a target name (csharp, typescript or python)");
                        cc.ExpectEnd();
                        sawNative = true;

                        if (sawSteps) Report("PH208", "A phrase is implemented by native blocks or by steps, not both.", clauseAt);
                        else if (!Targets.Contains(target.Text)) Report("PH207", $"Unknown native target '{target.Text}'. Known targets: {string.Join(", ", Targets)}.", Pos(target));
                        else if (natives.Any(n => n.Target == target.Text)) Report("PH204", $"A second `native {target.Text}` block.", clauseAt);
                        else if (child.Raw is { } raw) natives.Add(new NativeBlock(target.Text, raw.Text, clauseAt));
                        // No RAW token: the lexer has already reported PH106.
                    }
                    else if (cc.IsName("steps"))
                    {
                        cc.Next();
                        cc.ExpectEnd();
                        if (sawSteps) { Report("PH204", "Only one `steps` block is allowed.", clauseAt); return; }
                        sawSteps = true;
                        if (sawNative) { Report("PH208", "A phrase is implemented by native blocks or by steps, not both.", clauseAt); return; }
                        steps = ParseSteps(child, template.Parts.OfType<TemplateHole>().Select(hole => hole.Name));
                    }
                    else
                    {
                        throw cc.Error("a phrase clause (effects, fails with, spec, native or steps)");
                    }
                });
            }

            if (!sawNative && !sawSteps)
                Report("PH203", "Phrase has no implementation; add `native <target>` or `steps`.", at);

            return new PhraseDecl(template, returns, effects, failures, specs, natives, steps, at) { FailureLine = failureLine };
        }

        private static SyntaxError FillerError(string code, Token token) =>
            new(new Diagnostic(code,
                "An avow may have at most one filler before its template: to, that, it. Canonical form: avow to \"...\".",
                token.Line, token.Column));

        private static List<string> ParseTypeNameList(Cursor c, string what)
        {
            var names = new List<string>();
            do names.Add(c.Expect(TokenKind.TypeName, $"{what} (Capitalized)").Text);
            while (c.TryConsume(","));
            c.ExpectEnd();
            return names;
        }

        /// <summary>
        /// Splits a template string into words and holes by re-lexing its contents.
        /// Positions are mapped back into the string so diagnostics point at the right column.
        /// </summary>
        private Template ParseTemplate(Token str)
        {
            var at = Pos(str);
            var parts = new List<TemplatePart>();
            var content = str.Text;
            var trimmed = content.TrimStart();
            var startColumn = str.Column + 1 + (content.Length - trimmed.Length);
            Position Map(int line, int column) => new(str.Line, startColumn + column - 1);

            if (content.Contains('#'))
            {
                Report("PH205", "A template may not contain '#'.", Map(1, content.IndexOf('#') + 1));
                return new Template(parts, at);
            }

            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var reserved = words switch
            {
                ["let" or "native", ..] => words[0],
                ["respond" or "fail", "with", ..] => $"{words[0]} with",
                _ => null,
            };
            if (reserved is not null)
            {
                Report("PH205", $"A template may not start with the step keyword `{reserved}`; no step could call it.", Map(1, 1));
                return new Template(parts, at);
            }

            var inner = Lexer.Lex(trimmed);
            if (inner.Diagnostics.Count > 0)
            {
                foreach (var d in inner.Diagnostics)
                    Report("PH205", $"Invalid template: {d.Message}", Map(d.Line, d.Column));
                return new Template(parts, at);
            }

            var tokens = inner.Tokens
                .Where(t => t.Kind is not (TokenKind.Newline or TokenKind.Indent or TokenKind.Dedent or TokenKind.EndOfFile))
                .Select(t => t with { Line = str.Line, Column = startColumn + t.Column - 1 })
                .ToList();
            var c = new Cursor(tokens, new Position(str.Line, str.Column + content.Length + 1), "PH205");
            var holes = new HashSet<string>(StringComparer.Ordinal);

            try
            {
                while (!c.AtEnd)
                {
                    if (c.Is(TokenKind.Name))
                    {
                        var w = c.Next();
                        parts.Add(new TemplateWord(w.Text, Pos(w)));
                    }
                    else if (c.IsSymbol("{"))
                    {
                        var open = c.Next();
                        var name = c.Expect(TokenKind.Name, "a lowercase hole name");
                        c.ExpectSymbol(":");
                        var type = ParseTypeRef(c);
                        c.ExpectSymbol("}");
                        if (!holes.Add(name.Text))
                            Report("PH206", $"Hole name '{name.Text}' is used twice in this template.", Pos(open));
                        parts.Add(new TemplateHole(name.Text, type, Pos(open)));
                    }
                    else
                    {
                        throw c.Error("a word or a {name: Type} hole");
                    }
                }
            }
            catch (SyntaxError e)
            {
                _diagnostics.Add(e.Diagnostic);
                return new Template(parts, at);
            }

            if (!parts.OfType<TemplateWord>().Any())
                Report("PH205", "A template needs at least one word, or every call would match it.", at);

            return new Template(parts, at);
        }

        // ------------------------------------------------------------- endpoints

        private EndpointDecl ParseEndpoint(Line line)
        {
            var c = CursorFor(line);
            var at = Pos(c.Next());
            var name = c.Expect(TokenKind.TypeName, "an endpoint name (Capitalized)");
            c.ExpectSymbol(":");
            var verb = c.Expect(TokenKind.TypeName, "an HTTP verb");
            var route = ParseRoute(c);
            _web = true;
            c.ExpectEnd();

            if (!ExtensionKeywords.IsHttpVerb(verb.Text))
                Report(ExtensionDiagnostics.Web("PH209"), $"Unknown HTTP verb '{verb.Text}'. Use one of: {string.Join(", ", HttpVerbs)}.", Pos(verb));
            else
                _web = true;

            List<FieldDecl>? input = null, output = null;
            List<Step>? steps = null;
            var specs = new List<SpecDecl>();

            foreach (var child in line.Children)
            {
                Try(() =>
                {
                    var cc = CursorFor(child);
                    var sectionAt = cc.Here;
                    var section = cc.Peek() is { Kind: TokenKind.Name } t ? t.Text : null;

                    switch (section)
                    {
                        case "input" when ExtensionKeywords.Is(ExtensionPart.Web, "input"):
                        case "output" when ExtensionKeywords.Is(ExtensionPart.Web, "output"):
                        case "steps" when ExtensionKeywords.Is(ExtensionPart.Core, "steps")
                            && ExtensionKeywords.Construct("endpoint-steps") == ExtensionPart.Web:
                            cc.Next();
                            cc.ExpectEnd();
                            var seen = section switch { "input" => input is not null, "output" => output is not null, _ => steps is not null };
                            if (seen) { Report("PH204", $"Only one `{section}` section is allowed.", sectionAt); return; }
                            if (section == "input") input = ParseFieldBlock(child, "input");
                            else if (section == "output") output = ParseFieldBlock(child, "output");
                            else steps = ParseSteps(child, ["input"]);
                            break;
                        case "spec":
                            specs.Add(ParseSpec(child, cc));
                            break;
                        default:
                            throw cc.Error("an endpoint section (input, output, steps or spec)");
                    }
                });
            }

            if (input is null) Report("PH203", $"Endpoint {name.Text} has no `input` section.", at);
            if (steps is null) Report("PH203", $"Endpoint {name.Text} has no `steps` section.", at);

            return new EndpointDecl(name.Text, verb.Text, route, input ?? [], output ?? [], steps ?? [], specs, at);
        }

        private static string ParseRoute(Cursor c)
        {
            if (ExtensionKeywords.Construct("route") != ExtensionPart.Web)
                throw new InvalidOperationException("Routes belong to the web extension.");

            var route = new StringBuilder();
            if (!c.IsSymbol("/")) throw c.Error("a route starting with '/'");
            c.Next();
            route.Append('/');

            while (!c.AtEnd)
            {
                if (c.Is(TokenKind.Name) || c.Is(TokenKind.Number))
                {
                    route.Append(c.Next().Text);
                }
                else if (c.TryConsume("{"))
                {
                    if (ExtensionKeywords.Construct("route-parameter") != ExtensionPart.Web)
                        throw new InvalidOperationException("Route parameters belong to the web extension.");
                    var parameter = c.Expect(TokenKind.Name, "a route parameter name");
                    c.ExpectSymbol("}");
                    route.Append('{').Append(parameter.Text).Append('}');
                }
                else
                {
                    throw c.Error("a route segment");
                }

                if (!c.TryConsume("/")) break;
                route.Append('/');
            }
            return route.ToString();
        }

        // ------------------------------------------------------------- steps

        private List<Step> ParseSteps(Line header, IEnumerable<string> initialNames)
        {
            if (header.Children.Count == 0)
                Report("PH203", "`steps` has no steps; indent one step per line under it.", header.Start);

            var steps = new List<Step>();
            var namesInScope = initialNames.ToHashSet(StringComparer.Ordinal);
            foreach (var child in header.Children)
            {
                if (Try(() => ParseStep(child, namesInScope)) is not { } step) continue;
                steps.Add(step);
                if (step is LetStep let) namesInScope.Add(let.Name);
                if (step is LetUpdateStep update) namesInScope.Add(update.Name);
            }
            return steps;
        }

        private Step ParseStep(Line line, IReadOnlySet<string> namesInScope)
        {
            var c = CursorFor(line);
            var at = c.Here;
            NoChildren(line);

            if (c.IsName("let"))
            {
                c.Next();
                var name = c.Expect(TokenKind.Name, "a variable name after `let`");
                c.ExpectSymbol("=");
                if (LooksLikeRecordUpdate(c, namesInScope))
                {
                    var source = ParsePath(c);
                    c.Next(); // with
                    var fields = ParseRecord(c, requireFields: true, allowTrailingComma: false);
                    c.ExpectEnd();
                    return new LetUpdateStep(name.Text, source, fields, at);
                }
                return new LetStep(name.Text, ParseCall(c), at);
            }

            if (c.IsName("respond") && c.IsName("with", 1)
                && ExtensionKeywords.Is(ExtensionPart.Web, "respond with")
                && ExtensionKeywords.Construct("respond-with") == ExtensionPart.Web)
            {
                c.Next(); c.Next();
                var value = ParseRecord(c);
                c.ExpectEnd();
                return new RespondStep(value, at);
            }

            if (c.IsName("fail") && c.IsName("with", 1))
            {
                c.Next(); c.Next();
                var code = c.Expect(TokenKind.TypeName, "a failure code (Capitalized)");
                c.ExpectEnd();
                return new FailStep(code.Text, at);
            }

            if (c.IsName("ensure") && line.Tokens.Any(token => token is { Kind: TokenKind.Name, Text: "else" }))
            {
                c.Next();
                var value = ParsePath(c);
                Condition condition;
                if (c.IsName("is") && (c.IsName("blank", 1) || c.IsName("not", 1) && c.IsName("blank", 2)))
                {
                    c.Next();
                    var isNotBlank = c.IsName("not");
                    if (isNotBlank) c.Next();
                    c.Next();
                    condition = new BlankCondition(value, isNotBlank, at);
                }
                else if (c.IsName("length") && c.IsName("at", 1) && c.IsName("most", 2))
                {
                    c.Next(); c.Next(); c.Next();
                    var maximum = c.Expect(TokenKind.Number, "an integer maximum");
                    if (!int.TryParse(maximum.Text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsedMaximum))
                        throw new SyntaxError(new Diagnostic("PH200", "Length maximum must be a non-negative 32-bit integer.", maximum.Line, maximum.Column));
                    condition = new LengthAtMostCondition(value, parsedMaximum, at);
                }
                else
                {
                    var op = c.Next();
                    if (op.Kind != TokenKind.Symbol || op.Text is not ("<" or "<=" or ">" or ">=" or "==" or "!="))
                        throw c.Error("a comparison operator (<, <=, >, >=, == or !=)");
                    condition = new ComparisonCondition(value, op.Text, ParseExpr(c), at);
                }
                if (!c.IsName("else")) throw c.Error("`else` after the ensure condition");
                c.Next();
                var code = c.Expect(TokenKind.TypeName, "a failure code (Capitalized)");
                c.ExpectEnd();
                return new EnsureStep(condition, code.Text, at);
            }

            return ParseCall(c);
        }

        /// <summary><c>path with { ... }</c> is a record update, not a phrase call.</summary>
        private static bool LooksLikeRecordUpdate(Cursor c, IReadOnlySet<string> namesInScope)
        {
            if (!c.Is(TokenKind.Name)) return false;
            var root = c.Peek()!.Text;
            if (!namesInScope.Contains(root)) return false;
            var k = 1;
            while (c.IsSymbol(".", k) && c.Is(TokenKind.Name, k + 1))
                k += 2;
            return c.IsName("with", k) && c.IsSymbol("{", k + 1);
        }

        /// <summary>Bare names are words; dotted paths, literals, records and lists are arguments.</summary>
        private static PhraseCall ParseCall(Cursor c)
        {
            var at = c.Here;
            var parts = new List<CallPart>();

            while (!c.AtEnd)
            {
                var t = c.Peek()!;
                if (t.Kind == TokenKind.Name && !c.IsSymbol(".", 1))
                {
                    c.Next();
                    parts.Add(new CallWord(t.Text, Pos(t)));
                }
                else if (t.Kind is TokenKind.Name or TokenKind.String or TokenKind.Number || c.IsSymbol("{") || c.IsSymbol("["))
                {
                    parts.Add(new CallArgument(ParseExpr(c), Pos(t)));
                }
                else if (t.Kind == TokenKind.TypeName)
                {
                    throw c.Error("a word or a value (type names cannot appear in a step)");
                }
                else
                {
                    throw c.Error("a word or a value");
                }
            }

            if (parts.Count == 0) throw c.Error("a phrase call");
            return new PhraseCall(parts, at);
        }

        // ------------------------------------------------------------- expressions

        private static Expr ParseExpr(Cursor c)
        {
            var t = c.Peek() ?? throw c.Error("a value");
            var at = Pos(t);

            switch (t.Kind)
            {
                case TokenKind.String: c.Next(); return new StringLit(t.Text, at);
                case TokenKind.Number: c.Next(); return new NumberLit(t.Text, at);
                case TokenKind.Name when t.Text == "true": c.Next(); return new BoolLit(true, at);
                case TokenKind.Name when t.Text == "false": c.Next(); return new BoolLit(false, at);
                case TokenKind.Name when t.Text == "none": c.Next(); return new NoneLit(at);
                case TokenKind.Name: return ParsePath(c);
            }

            if (c.IsSymbol("{")) return ParseRecord(c);
            if (c.IsSymbol("[")) return ParseList(c);
            throw c.Error("a value");
        }

        private static PathExpr ParsePath(Cursor c)
        {
            var first = c.Expect(TokenKind.Name, "a name");
            var segments = new List<string> { first.Text };
            while (c.TryConsume("."))
                segments.Add(c.Expect(TokenKind.Name, "a field name after '.'").Text);
            return new PathExpr(segments, Pos(first));
        }

        private static RecordExpr ParseRecord(Cursor c, bool requireFields = false, bool allowTrailingComma = true)
        {
            if (!c.IsSymbol("{")) throw c.Error("a record { field: value, ... }");
            var open = c.Next();
            var fields = new List<RecordField>();

            if (requireFields && c.IsSymbol("}"))
                throw c.Error("at least one field in a record update");

            while (!c.IsSymbol("}"))
            {
                var name = c.Expect(TokenKind.Name, "a field name");
                c.ExpectSymbol(":");
                fields.Add(new RecordField(name.Text, ParseExpr(c), Pos(name)));
                if (!c.TryConsume(",")) break;
                if (!allowTrailingComma && c.IsSymbol("}"))
                    throw c.Error("a field after ',' in a record update");
            }

            c.ExpectSymbol("}");
            return new RecordExpr(fields, Pos(open));
        }

        private static ListExpr ParseList(Cursor c)
        {
            var open = c.ExpectSymbol("[");
            var items = new List<Expr>();

            while (!c.IsSymbol("]"))
            {
                items.Add(ParseExpr(c));
                if (!c.TryConsume(",")) break; // a trailing comma before ']' is allowed
            }

            c.ExpectSymbol("]");
            return new ListExpr(items, Pos(open));
        }

        // ------------------------------------------------------------- specs

        private enum Clause { None, Given, When, Then }

        private SpecDecl ParseSpec(Line line, Cursor c)
        {
            var at = Pos(c.Next());
            var description = c.Expect(TokenKind.String, "a quoted spec description");
            c.ExpectEnd();

            var givens = new List<Given>();
            var thens = new List<Outcome>();
            SpecTrigger? when = null;
            bool sawWhen = false, sawThen = false;
            var last = Clause.None;   // what an `and` continues
            var phase = Clause.None;  // furthest clause reached

            foreach (var child in line.Children)
            {
                Try(() =>
                {
                    var cc = CursorFor(child);
                    var clauseAt = cc.Here;
                    NoChildren(child);
                    var keyword = cc.Peek() is { Kind: TokenKind.Name } t ? t.Text : null;

                    switch (keyword)
                    {
                        case "given":
                            cc.Next();
                            if (phase > Clause.Given) Report("PH210", "`given` must come before `when` and `then`.", clauseAt);
                            givens.Add(ParseGiven(cc, clauseAt));
                            last = Clause.Given;
                            if (phase < Clause.Given) phase = Clause.Given;
                            break;

                        case "when":
                            cc.Next();
                            sawWhen = true;
                            if (when is not null) { Report("PH204", "A spec has exactly one `when`.", clauseAt); return; }
                            if (phase > Clause.When) Report("PH210", "`when` must come before `then`.", clauseAt);
                            when = ParseTrigger(cc);
                            last = Clause.When;
                            if (phase < Clause.When) phase = Clause.When;
                            break;

                        case "then":
                            cc.Next();
                            sawThen = true;
                            thens.Add(ParseOutcome(cc, clauseAt));
                            last = Clause.Then;
                            phase = Clause.Then;
                            break;

                        case "and":
                            cc.Next();
                            switch (last)
                            {
                                case Clause.Given: givens.Add(ParseGiven(cc, clauseAt)); break;
                                case Clause.Then: thens.Add(ParseOutcome(cc, clauseAt)); break;
                                default: Report("PH210", "`and` must follow a `given` or a `then`.", clauseAt); break;
                            }
                            break;

                        default:
                            throw cc.Error("a spec clause (given, when, then or and)");
                    }
                });
            }

            if (!sawWhen) Report("PH203", $"Spec \"{description.Text}\" has no `when` line.", at);
            if (!sawThen) Report("PH203", $"Spec \"{description.Text}\" has no `then` line.", at);

            return new SpecDecl(description.Text, givens, when, thens, at);
        }

        private static Given ParseGiven(Cursor c, Position at)
        {
            if (c.IsName("input") && c.IsSymbol("{", 1) && ExtensionKeywords.Is(ExtensionPart.Web, "input"))
            {
                c.Next();
                var input = ParseRecord(c);
                c.ExpectEnd();
                return new GivenInput(input, at);
            }

            var name = c.Expect(TokenKind.Name, "a hole name or `input`");
            c.ExpectSymbol("=");
            var value = ParseExpr(c);
            c.ExpectEnd();
            return new GivenBinding(name.Text, value, at);
        }

        private static SpecTrigger ParseTrigger(Cursor c)
        {
            SpecTrigger trigger;
            if (c.IsName("called")) trigger = SpecTrigger.Called;
            else if (c.IsName("handled")
                && ExtensionKeywords.Is(ExtensionPart.Web, "handled")
                && ExtensionKeywords.Construct("when-handled") == ExtensionPart.Web)
                trigger = SpecTrigger.Handled;
            else throw c.Error("`called` (phrase spec) or `handled` (endpoint spec)");
            c.Next();
            c.ExpectEnd();
            return trigger;
        }

        private static Outcome ParseOutcome(Cursor c, Position at)
        {
            Outcome outcome;
            if (c.IsName("returns")) { c.Next(); outcome = new ReturnsOutcome(ParseExpr(c), at); }
            else if (c.IsName("responds")
                && ExtensionKeywords.Is(ExtensionPart.Web, "responds")
                && ExtensionKeywords.Construct("responds") == ExtensionPart.Web)
            { c.Next(); outcome = new RespondsOutcome(ParseRecord(c), at); }
            else if (c.IsName("fails") && c.IsName("with", 1)) { c.Next(); c.Next(); outcome = new FailsWithOutcome(c.Expect(TokenKind.TypeName, "a failure code (Capitalized)").Text, at); }
            else if (c.IsName("succeeds")) { c.Next(); outcome = new SucceedsOutcome(at); }
            else throw c.Error("an outcome (returns, responds, fails with or succeeds)");
            c.ExpectEnd();
            return outcome;
        }

        // ------------------------------------------------------------- helpers

        private static Cursor CursorFor(Line line) => new(line.Tokens, EndOf(line));

        private void NoChildren(Line line)
        {
            if (line.Children.Count > 0)
                Report("PH211", "Unexpected indented block.", line.Children[0].Start);
        }

        private T? Try<T>(Func<T> parse) where T : class
        {
            try { return parse(); }
            catch (SyntaxError e) { _diagnostics.Add(e.Diagnostic); return null; }
        }

        private void Try(Action parse)
        {
            try { parse(); }
            catch (SyntaxError e) { _diagnostics.Add(e.Diagnostic); }
        }

        private void AddIfParsed<T>(List<T> into, Func<T> parse) where T : class
        {
            if (Try(parse) is { } node) into.Add(node);
        }

        private void Report(string code, string message, Position at) =>
            _diagnostics.Add(new Diagnostic(code, message, at.Line, at.Column));
    }
}
