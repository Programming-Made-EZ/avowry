using Phrase.Compiler;
using Phrase.Compiler.Semantics;
using Phrase.Compiler.Syntax;

namespace Phrase.Compiler.Tests;

// Feature: The compiler partitions core and web
//   As Dana, I want the compiler to know which keywords are core and which
//   belong to the web extension, so that web concepts stay out of the core
//   while every existing app keeps working.
public class ExtensionBoundaryTests
{
    private const string CorePhrase = """
        module Numbers

        avow to "double {n: Int}" -> Int
          native csharp
            return n * 2;
        """;

    // Web words inside the template are ordinary words. Nothing here is an endpoint, a verb or a route.
    private const string PhraseWithWebWords = """
        module Catalog

        avow to "publish the endpoint input when the caller responds {sku: Text}" -> Text
          native csharp
            return sku;
        """;

    private const string EndpointProgram = """
        module Orders

        avow to "double {n: Int}" -> Int
          native csharp
            return n * 2;

        endpoint Echo : POST /echo/{id}
          input
            n: Int
          output
            n: Int
          steps
            let doubled = double input.n
            respond with { n: doubled }
          spec "echoes the double"
            given input { n: 2 }
            when handled
            then responds { n: 4 }
        """;

    private const string ExplicitWebOnly = """
        uses web
        module Numbers

        avow to "double {n: Int}" -> Int
          native csharp
            return n * 2;
        """;

    [Fact]
    [Trait("Slice", "142")]
    public void Keywords_are_partitioned_into_core_and_web()
    {
        // Given the keyword lists for the two parts
        // steps stays core because a composite phrase uses it; an endpoint's steps block is the web construct endpoint-steps.
        string[] core =
        [
            "and", "called", "effects", "ensure", "fail with", "fails with", "given", "let", "module", "native",
            "avow", "returns", "spec", "steps", "succeeds", "then", "type", "uses", "when",
        ];
        string[] web =
        [
            "DELETE", "GET", "PATCH", "POST", "PUT", "endpoint", "handled", "input", "output",
            "respond with", "responds",
        ];

        // When the registry is read
        var registeredCore = ExtensionKeywords.Keywords(ExtensionPart.Core);
        var registeredWeb = ExtensionKeywords.Keywords(ExtensionPart.Web);

        // Then each keyword is in exactly one part, and a web keyword is not registered as core
        Assert.Equal(core.OrderBy(keyword => keyword, StringComparer.Ordinal), registeredCore);
        Assert.Equal(web.OrderBy(keyword => keyword, StringComparer.Ordinal), registeredWeb);
        Assert.Empty(registeredCore.Intersect(registeredWeb, StringComparer.Ordinal));
        foreach (var keyword in web)
            Assert.Equal(ExtensionPart.Web, ExtensionKeywords.PartOf(keyword));
        foreach (var keyword in core)
            Assert.Equal(ExtensionPart.Core, ExtensionKeywords.PartOf(keyword));

        // And the web constructs, including routes and status mapping, are not core
        Assert.Equal(ExtensionPart.Web, ExtensionKeywords.Construct("endpoint"));
        Assert.Equal(ExtensionPart.Web, ExtensionKeywords.Construct("endpoint-steps"));
        Assert.Equal(ExtensionPart.Web, ExtensionKeywords.Construct("route"));
        Assert.Equal(ExtensionPart.Web, ExtensionKeywords.Construct("route-parameter"));
        Assert.Equal(ExtensionPart.Web, ExtensionKeywords.Construct("respond-with"));
        Assert.Equal(ExtensionPart.Web, ExtensionKeywords.Construct("when-handled"));
        Assert.Equal(ExtensionPart.Web, ExtensionKeywords.Construct("responds"));
        Assert.Equal(ExtensionPart.Web, ExtensionKeywords.Construct("status-mapping"));
        Assert.Equal(ExtensionPart.Core, ExtensionKeywords.Construct("phrase-steps"));
        Assert.Equal(ExtensionPart.Core, ExtensionKeywords.Construct("ensure-step"));
        Assert.Equal(ExtensionPart.Core, ExtensionKeywords.Construct("record-update"));

        // And the web diagnostics stay web, including only the endpoint arm of the shared codes
        foreach (var code in new[] { "PH209", "PH304", "PH322", "PH323" })
            Assert.Equal(ExtensionPart.Web, ExtensionKeywords.DiagnosticPart(code));
        foreach (var code in new[] { "PH330", "PH331", "PH333", "PH334" })
        {
            Assert.Equal(ExtensionPart.Core, ExtensionKeywords.DiagnosticPart(code, "phrase"));
            Assert.Equal(ExtensionPart.Web, ExtensionKeywords.DiagnosticPart(code, "endpoint"));
        }

        // And the verb list the parser prints is still the web verbs, in the same order
        Assert.Equal(new[] { "GET", "POST", "PUT", "PATCH", "DELETE" }, Parser.HttpVerbs);
    }

    [Fact]
    [Trait("Slice", "142")]
    public void The_active_extension_set_is_deterministic()
    {
        // Given one core file, one implicit web file and one explicit web file
        string[] sources = [CorePhrase, EndpointProgram, WithExplicitWeb(EndpointProgram), ExplicitWebOnly];

        foreach (var source in sources)
        {
            // When the active set is read twice from equal inputs
            var first = ExtensionLoad.Active(source);
            var second = ExtensionLoad.Active(new string(source.ToCharArray()));

            // Then both reads are the same set, and resolution reports that set
            Assert.Equal(first, second);
            Assert.Equal(first, Resolver.Resolve(source).ActiveExtensions);
        }

        // And an endpoint, a verb or a route loads web, while the same text loads it again
        Assert.Equal(new[] { "core", "web" }, ExtensionLoad.Active(EndpointProgram));
        Assert.Equal(new[] { "core" }, ExtensionLoad.Active(CorePhrase));
    }

    [Fact]
    [Trait("Slice", "142")]
    public void An_explicit_uses_web_line_is_accepted_and_equivalent()
    {
        // Given the same endpoint program with and without an explicit uses web line
        var implied = EndpointProgram;
        var explicitWeb = WithExplicitWeb(implied);

        // When both are parsed and resolved
        var impliedParse = Parser.Parse(implied);
        var explicitParse = Parser.Parse(explicitWeb);
        var impliedResolved = Resolver.Resolve(implied);
        var explicitResolved = Resolver.Resolve(explicitWeb);

        // Then the explicit line is accepted and both files load the same web set
        Assert.DoesNotContain(explicitParse.Diagnostics, diagnostic => diagnostic.Code == "PH201");
        Assert.Empty(impliedParse.Diagnostics);
        Assert.Empty(explicitParse.Diagnostics);
        Assert.Equal(new[] { "core", "web" }, ExtensionLoad.Active(implied));
        Assert.Equal(ExtensionLoad.Active(implied), ExtensionLoad.Active(explicitWeb));
        Assert.Equal(impliedResolved.Diagnostics.Select(diagnostic => diagnostic.Code), explicitResolved.Diagnostics.Select(diagnostic => diagnostic.Code));

        // And the endpoint shape is unchanged
        var impliedEndpoint = Assert.Single(impliedResolved.Module.Endpoints);
        var explicitEndpoint = Assert.Single(explicitResolved.Module.Endpoints);
        Assert.Equal(
            (impliedEndpoint.Name, impliedEndpoint.Verb, impliedEndpoint.Route),
            (explicitEndpoint.Name, explicitEndpoint.Verb, explicitEndpoint.Route));

        // And uses web on its own is enough to load web, with no unknown-declaration diagnostic
        var onlyUses = Parser.Parse(ExplicitWebOnly);
        Assert.DoesNotContain(onlyUses.Diagnostics, diagnostic => diagnostic.Code == "PH201");
        Assert.Empty(onlyUses.Diagnostics);
        Assert.Equal(new[] { "core", "web" }, ExtensionLoad.Active(ExplicitWebOnly));
    }

    [Fact]
    [Trait("Slice", "142")]
    public void A_file_with_no_web_construct_leaves_the_web_extension_inactive()
    {
        // Given a phrase whose template uses web words and no endpoint, verb or route
        var source = PhraseWithWebWords;

        // When the file is parsed and its active set is read
        var parsed = Parser.Parse(source);
        var active = ExtensionLoad.Active(source);

        // Then the web extension is inactive and those words stay ordinary template words
        Assert.Empty(parsed.Diagnostics);
        Assert.Equal(new[] { "core" }, active);
        Assert.DoesNotContain("web", active);
        var words = Assert.Single(parsed.File.Phrases).Template.Parts.OfType<TemplateWord>().Select(word => word.Text).ToList();
        Assert.Contains("endpoint", words);
        Assert.Contains("input", words);
        Assert.Contains("responds", words);
        Assert.Empty(parsed.File.Endpoints);
    }

    private static string WithExplicitWeb(string source) =>
        source.Replace("module Orders\n", "module Orders\nuses web\n", StringComparison.Ordinal);
}
