using Phrase.Compiler.Semantics;

namespace Phrase.Compiler;

/// <summary>Which part of the language a keyword, construct or diagnostic belongs to.</summary>
public enum ExtensionPart
{
    Core,
    Web,
}

/// <summary>
/// Keywords and constructs of the core and the web extension.
/// Later web cards add HTTP verbs, routes and endpoint forms here.
/// <c>steps</c> stays a core keyword because composite phrases use it;
/// an endpoint's steps block is the web construct <c>endpoint-steps</c>.
/// </summary>
public static class ExtensionKeywords
{
    public static readonly IReadOnlyList<string> CoreOnly = ["core"];
    public static readonly IReadOnlyList<string> CoreAndWeb = ["core", "web"];

    private static readonly string[] VerbNames = ["GET", "POST", "PUT", "PATCH", "DELETE"];

    /// <summary>The HTTP verbs, in the order PH209 prints them.</summary>
    public static readonly IReadOnlySet<string> HttpVerbs =
        new HashSet<string>(VerbNames, StringComparer.Ordinal);

    private static readonly Dictionary<string, ExtensionPart> KeywordParts = new(StringComparer.Ordinal)
    {
        ["module"] = ExtensionPart.Core,
        ["type"] = ExtensionPart.Core,
        ["phrase"] = ExtensionPart.Core,
        ["steps"] = ExtensionPart.Core,
        ["let"] = ExtensionPart.Core,
        ["fail with"] = ExtensionPart.Core,
        ["fails with"] = ExtensionPart.Core,
        ["effects"] = ExtensionPart.Core,
        ["spec"] = ExtensionPart.Core,
        ["given"] = ExtensionPart.Core,
        ["when"] = ExtensionPart.Core,
        ["then"] = ExtensionPart.Core,
        ["and"] = ExtensionPart.Core,
        ["native"] = ExtensionPart.Core,
        ["called"] = ExtensionPart.Core,
        ["returns"] = ExtensionPart.Core,
        ["succeeds"] = ExtensionPart.Core,
        ["uses"] = ExtensionPart.Core,

        ["endpoint"] = ExtensionPart.Web,
        ["input"] = ExtensionPart.Web,
        ["output"] = ExtensionPart.Web,
        ["respond with"] = ExtensionPart.Web,
        ["handled"] = ExtensionPart.Web,
        ["responds"] = ExtensionPart.Web,
        ["GET"] = ExtensionPart.Web,
        ["POST"] = ExtensionPart.Web,
        ["PUT"] = ExtensionPart.Web,
        ["PATCH"] = ExtensionPart.Web,
        ["DELETE"] = ExtensionPart.Web,
    };

    private static readonly Dictionary<string, ExtensionPart> Constructs = new(StringComparer.Ordinal)
    {
        ["phrase-steps"] = ExtensionPart.Core,
        ["endpoint"] = ExtensionPart.Web,
        ["endpoint-steps"] = ExtensionPart.Web,
        ["route"] = ExtensionPart.Web,
        ["route-parameter"] = ExtensionPart.Web,
        ["respond-with"] = ExtensionPart.Web,
        ["when-handled"] = ExtensionPart.Web,
        ["responds"] = ExtensionPart.Web,
        // The 422 status mapping stays in the C# adapter. It is web, and this slice does not move it.
        ["status-mapping"] = ExtensionPart.Web,
    };

    private readonly record struct DiagnosticHome(ExtensionPart Part, string? Arm);

    private static readonly Dictionary<string, DiagnosticHome[]> Diagnostics = new(StringComparer.Ordinal)
    {
        ["PH209"] = [new(ExtensionPart.Web, null)],
        ["PH304"] = [new(ExtensionPart.Web, null)],
        ["PH322"] = [new(ExtensionPart.Web, null)],
        ["PH323"] = [new(ExtensionPart.Web, null)],
        ["PH330"] = [new(ExtensionPart.Core, "phrase"), new(ExtensionPart.Web, "endpoint")],
        ["PH331"] = [new(ExtensionPart.Core, "phrase"), new(ExtensionPart.Web, "endpoint")],
        ["PH333"] = [new(ExtensionPart.Core, "phrase"), new(ExtensionPart.Web, "endpoint")],
        ["PH334"] = [new(ExtensionPart.Core, "phrase"), new(ExtensionPart.Web, "endpoint")],
    };

    static ExtensionKeywords()
    {
        foreach (var verb in VerbNames)
        {
            if (!Is(ExtensionPart.Web, verb))
                throw new InvalidOperationException($"HTTP verb '{verb}' must be registered as a web keyword.");
        }
    }

    public static IReadOnlyList<string> Keywords(ExtensionPart part) =>
        KeywordParts.Where(pair => pair.Value == part)
            .Select(pair => pair.Key)
            .OrderBy(keyword => keyword, StringComparer.Ordinal)
            .ToArray();

    public static ExtensionPart PartOf(string keyword) =>
        KeywordParts.TryGetValue(keyword, out var part)
            ? part
            : throw new ArgumentException($"'{keyword}' is not a registered keyword.", nameof(keyword));

    public static bool Is(ExtensionPart part, string keyword) =>
        KeywordParts.TryGetValue(keyword, out var home) && home == part;

    public static bool IsHttpVerb(string text) => HttpVerbs.Contains(text) && Is(ExtensionPart.Web, text);

    public static ExtensionPart Construct(string name) =>
        Constructs.TryGetValue(name, out var part)
            ? part
            : throw new ArgumentException($"'{name}' is not a registered construct.", nameof(name));

    /// <summary>
    /// Part that owns a diagnostic. Shared codes pass <paramref name="arm"/>
    /// <c>phrase</c> or <c>endpoint</c>. Whole-code web diagnostics omit the arm.
    /// </summary>
    public static ExtensionPart DiagnosticPart(string code, string? arm = null)
    {
        if (!Diagnostics.TryGetValue(code, out var homes))
            throw new ArgumentException($"'{code}' is not a registered diagnostic.", nameof(code));
        foreach (var home in homes)
        {
            if (home.Arm == arm)
                return home.Part;
        }

        throw new ArgumentException($"'{code}' has no {(arm ?? "whole")} arm.", nameof(arm));
    }
}

/// <summary>Diagnostic codes as the extension registry classifies them. The returned text is the code unchanged.</summary>
public static class ExtensionDiagnostics
{
    public static string Web(string code) => Require(code, ExtensionPart.Web, arm: null);

    public static string WebArm(string code) => Require(code, ExtensionPart.Web, arm: "endpoint");

    public static string CoreArm(string code) => Require(code, ExtensionPart.Core, arm: "phrase");

    private static string Require(string code, ExtensionPart part, string? arm)
    {
        if (ExtensionKeywords.DiagnosticPart(code, arm) != part)
            throw new InvalidOperationException($"Diagnostic {code} ({arm ?? "whole"}) is not registered as {part}.");
        return code;
    }
}

/// <summary>The extensions a source file loads. The same text always yields the same set.</summary>
public static class ExtensionLoad
{
    public static IReadOnlyList<string> Active(string source) =>
        Resolver.Resolve(source).ActiveExtensions;
}
