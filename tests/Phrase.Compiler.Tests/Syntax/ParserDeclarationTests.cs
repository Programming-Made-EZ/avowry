using Avowry.Compiler.Syntax;
using static Phrase.Compiler.Tests.Syntax.Parsed;

namespace Phrase.Compiler.Tests.Syntax;

// Feature: Module and type declarations
//   A file is one module followed by types, phrases and endpoints in any order.
public class ParserDeclarationTests
{
    [Fact]
    public void A_file_starts_with_its_module_name()
    {
        // Given a file with only a module declaration
        const string source = "module Orders\n";

        // When it is parsed
        var file = Clean(source);

        // Then the module is named and positioned
        Assert.Equal(new ModuleDecl("Orders", new Position(1, 1)), file.Module);
    }

    [Fact]
    public void A_missing_module_declaration_is_PH202()
    {
        // Given a type with no module above it
        const string source = "type LineItem\n  sku: Text\n";

        // When it is parsed
        var result = Parser.Parse(source);

        // Then the missing module is reported at the top of the file
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(("PH202", 1, 1), (diagnostic.Code, diagnostic.Line, diagnostic.Column));

        // And the type is still parsed
        Assert.Single(result.File.Types);
    }

    [Fact]
    public void A_second_module_declaration_is_PH202()
    {
        // Given two module lines
        const string source = "module Orders\nmodule Billing\n";

        // When it is parsed
        var codes = Codes(source);

        // Then the second is reported
        Assert.Equal(["PH202"], codes);
    }

    [Fact]
    public void A_type_lists_its_fields_in_order()
    {
        // Given a record type
        var source = InModule("""
            type LineItem
              sku: Text
              quantity: Int
            """);

        // When it is parsed
        var type = Assert.Single(Clean(source).Types);

        // Then its fields keep source order
        Assert.Equal("(type LineItem (sku: Text) (quantity: Int))", Print(type));
    }

    [Fact]
    public void Generic_type_references_nest()
    {
        // Given a field with a nested generic type and a two-argument type
        var source = InModule("""
            type Basket
              lines: List<Option<LineItem>>
              prices: Map<Text, Decimal>
            """);

        // When it is parsed
        var type = Assert.Single(Clean(source).Types);

        // Then the type arguments nest
        Assert.Equal("(type Basket (lines: List<Option<LineItem>>) (prices: Map<Text, Decimal>))", Print(type));
    }

    [Fact]
    public void A_type_with_no_fields_is_PH203()
    {
        // Given a type with nothing under it
        var source = InModule("type Empty\n");

        // When it is parsed
        var codes = Codes(source);

        // Then the missing fields are reported
        Assert.Equal(["PH203"], codes);
    }

    [Fact]
    public void An_unknown_top_level_line_is_PH201()
    {
        // Given a line that starts with no declaration keyword
        var source = InModule("function foo\n");

        // When it is parsed
        var result = Parser.Parse(source);

        // Then it is reported at its first token
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(("PH201", 2, 1), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }

    [Fact]
    public void A_malformed_field_is_PH200_and_the_other_fields_survive()
    {
        // Given a type where one field is missing its colon
        var source = InModule("""
            type LineItem
              sku Text
              quantity: Int
            """);

        // When it is parsed
        var result = Parser.Parse(source);

        // Then the bad field is reported
        Assert.Equal(["PH200"], result.Diagnostics.Select(d => d.Code));

        // And the good field is kept
        Assert.Equal("(type LineItem (quantity: Int))", Print(Assert.Single(result.File.Types)));
    }

    [Fact]
    public void An_unexpected_indented_block_is_PH211()
    {
        // Given a module declaration with lines indented under it
        const string source = "module Orders\n  stray\n";

        // When it is parsed
        var codes = Codes(source);

        // Then the block is reported
        Assert.Equal(["PH211"], codes);
    }

    [Fact]
    public void Lexer_diagnostics_are_included_in_the_parse_result()
    {
        // Given a file with a tab in its indentation
        var source = InModule("type LineItem\n\tsku: Text\n");

        // When it is parsed
        var codes = Codes(source);

        // Then the lexer's PH100 comes through
        Assert.Contains("PH100", codes);
    }
}
