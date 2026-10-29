using Phrase.Compiler.Syntax;

namespace Phrase.Compiler.Tests.Syntax;

public class AvowDeclarationTests
{
    [Theory]
    [InlineData("avow to \"double {n: Int}\"")]
    [InlineData("avow that \"double {n: Int}\"")]
    [InlineData("avow it \"double {n: Int}\"")]
    [InlineData("avow \"double {n: Int}\"")]
    [Trait("Slice", "199")]
    public void Each_supported_filler_and_no_filler_declares_an_avow(string declaration)
    {
        // Given Dana writes an avow with one supported filler, or no filler
        var source = $"module Numbers\n\n{declaration} -> Int\n  native csharp\n    return n * 2;\n";

        // When the declaration is parsed
        var result = Parser.Parse(source);

        // Then it has the quoted template, independent of the filler
        Assert.Empty(result.Diagnostics);
        Assert.Equal("double {n: Int}", SourceFormatter.Format(Assert.Single(result.File.Phrases).Template));
    }

    [Fact]
    [Trait("Slice", "199")]
    public void A_filler_is_not_part_of_the_name_used_to_resolve_a_call()
    {
        // Given an avow whose filler is also the first word of its quoted name
        const string source = """
            module Tasks

            avow to "to do list"
              native csharp
                return;

            avow to "run list"
              steps
                to do list
            """;

        // When the module is compiled
        var result = Phrase.Compiler.Semantics.Resolver.Resolve(source);

        // Then the unchanged call resolves to the name inside the quotes
        Assert.Empty(result.Diagnostics);
        var caller = result.Module!.Phrases.Single(phrase => phrase.Template == "run list");
        Assert.Equal("to do list", Assert.IsType<Phrase.Compiler.Semantics.BoundCallStep>(Assert.Single(caller.Steps!)).Call.PhraseKey);
    }

    [Theory]
    [InlineData("avow toward \"work\"", "PH212")]
    [InlineData("avow to that \"work\"", "PH213")]
    [Trait("Slice", "199")]
    public void Invalid_words_before_the_template_report_the_filler_rule(string declaration, string code)
    {
        // Given an avow with an unknown word or a second filler before its template
        var source = $"module Work\n\n{declaration}\n  native csharp\n    return;\n";

        // When it is parsed
        var result = Parser.Parse(source);

        // Then a dedicated diagnostic names the closed set and canonical form
        var diagnostic = Assert.Single(result.Diagnostics, diagnostic => diagnostic.Code == code);
        Assert.Contains("to, that, it", diagnostic.Message, StringComparison.Ordinal);
        Assert.Contains("avow to", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "199")]
    public void The_old_phrase_keyword_is_an_unknown_top_level_declaration()
    {
        // Given a declaration starts with the removed keyword
        const string source = "module Work\n\nphrase \"work\"\n  native csharp\n    return;\n";

        // When it is parsed
        var result = Parser.Parse(source);

        // Then the existing unknown declaration diagnostic points authors to avow
        var diagnostic = Assert.Single(result.Diagnostics, diagnostic => diagnostic.Code == "PH201");
        Assert.Contains("avow", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "199")]
    public void Formatting_an_avow_always_writes_the_canonical_filler()
    {
        // Given an avow uses a non-canonical accepted filler
        var parsed = Parser.Parse("module Work\n\navow it \"finish work\"\n  native csharp\n    return;\n");

        // When its declaration is formatted
        var formatted = SourceFormatter.Format(Assert.Single(parsed.File.Phrases));

        // Then the canonical source uses avow to
        Assert.Equal("avow to \"finish work\"", formatted);
    }
}
