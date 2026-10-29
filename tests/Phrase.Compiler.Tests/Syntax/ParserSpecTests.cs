using Phrase.Compiler.Syntax;
using static Phrase.Compiler.Tests.Syntax.Parsed;

namespace Phrase.Compiler.Tests.Syntax;

// Feature: Specs
//   A spec is given, when, then. `and` continues whichever clause came before it.
public class ParserSpecTests
{
    private static string PhraseWithSpec(string spec) =>
        InModule("avow to \"total of {items: List<LineItem>}\" -> Int\n" + spec + "  native csharp\n    return 0;\n");

    private static SpecDecl SingleSpec(string spec) =>
        Assert.Single(Assert.Single(Clean(PhraseWithSpec(spec)).Phrases).Specs);

    [Fact]
    public void A_phrase_spec_binds_holes_and_expects_a_value()
    {
        // Given a spec on a phrase
        var spec = SingleSpec("""
              spec "sums quantities"
                given items = [{ sku: "A", quantity: 2 }, { sku: "B", quantity: 3 }]
                when called
                then returns 5

            """);

        // When it is parsed
        // Then each clause is captured
        Assert.Equal(
            "(spec \"sums quantities\" (given items = [{sku: \"A\", quantity: 2}, {sku: \"B\", quantity: 3}]) (when called) (then returns 5))",
            Print(spec));
    }

    [Fact]
    public void An_endpoint_spec_gives_input_and_expects_a_response()
    {
        // Given a spec on an endpoint
        var source = InModule("""
            endpoint CreateOrder : POST /orders
              input
                items: List<LineItem>
              steps
                fail with NotBuilt
              spec "accepts a valid order"
                given input { customerId: "c-1", items: [] }
                when handled
                then responds { totalQuantity: 0 }
            """);

        // When it is parsed
        var spec = Assert.Single(Assert.Single(Clean(source).Endpoints).Specs);

        // Then input and response are records
        Assert.Equal(
            "(spec \"accepts a valid order\" (given input {customerId: \"c-1\", items: []}) (when handled) (then responds {totalQuantity: 0}))",
            Print(spec));
    }

    [Fact]
    public void And_continues_the_clause_before_it()
    {
        // Given a spec with extra givens and extra outcomes
        var spec = SingleSpec("""
              spec "several things"
                given a = 1
                and b = none
                when called
                then returns false
                and fails with Boom

            """);

        // When it is parsed
        // Then each and joins the right clause
        Assert.Equal(
            "(spec \"several things\" (given a = 1) (given b = none) (when called) (then returns false) (then fails Boom))",
            Print(spec));
    }

    [Fact]
    public void A_spec_may_have_no_givens_and_may_just_succeed()
    {
        // Given a spec for a phrase with no holes
        var spec = SingleSpec("""
              spec "runs"
                when called
                then succeeds

            """);

        // When it is parsed
        // Then it is valid
        Assert.Equal("(spec \"runs\" (when called) (then succeeds))", Print(spec));
    }

    [Fact]
    public void A_multi_line_given_reads_as_one_clause()
    {
        // Given a long input record wrapped over lines
        var spec = SingleSpec("""
              spec "wrapped"
                given items = [
                  { sku: "A", quantity: 2 },
                  { sku: "B", quantity: 3 },
                ]
                when called
                then returns 5

            """);

        // When it is parsed
        // Then the trailing comma is accepted and the list has two items
        var given = Assert.IsType<GivenBinding>(Assert.Single(spec.Givens));
        Assert.Equal(2, Assert.IsType<ListExpr>(given.Value).Items.Count);
    }

    [Fact]
    public void A_wrapped_record_may_end_with_a_trailing_comma()
    {
        // Given an endpoint input record wrapped over lines with a trailing comma
        var source = InModule("""
            endpoint CreateOrder : POST /orders
              input
                customerId: Id
              steps
                fail with NotBuilt
              spec "wrapped input"
                given input {
                  customerId: "c-1",
                  items: [],
                }
                when handled
                then fails with NotBuilt
            """);

        // When it is parsed
        var spec = Assert.Single(Assert.Single(Clean(source).Endpoints).Specs);

        // Then the record has both fields
        var input = Assert.IsType<GivenInput>(Assert.Single(spec.Givens));
        Assert.Equal(["customerId", "items"], input.Value.Fields.Select(f => f.Name));
    }

    [Fact]
    public void Missing_when_and_missing_then_are_PH203()
    {
        // Given a spec with only a given
        var source = PhraseWithSpec("  spec \"incomplete\"\n    given items = []\n");

        // When it is parsed
        // Then both missing clauses are reported
        Assert.Equal(["PH203", "PH203"], Codes(source));
    }

    [Fact]
    public void Clauses_out_of_order_are_PH210()
    {
        // Given a spec whose then comes before its when
        var source = PhraseWithSpec("""
              spec "backwards"
                given items = []
                then returns 0
                when called

            """);

        // When it is parsed
        // Then the order is reported
        Assert.Contains("PH210", Codes(source));
    }

    [Fact]
    public void And_with_nothing_to_continue_is_PH210()
    {
        // Given a spec that starts with and
        var source = PhraseWithSpec("""
              spec "dangling"
                and items = []
                when called
                then returns 0

            """);

        // When it is parsed
        // Then the and is reported
        Assert.Equal(["PH210"], Codes(source));
    }

    [Fact]
    public void A_second_when_is_PH204()
    {
        // Given a spec with two triggers
        var source = PhraseWithSpec("""
              spec "twice"
                when called
                when called
                then returns 0

            """);

        // When it is parsed
        // Then the second is reported
        Assert.Contains("PH204", Codes(source));
    }

    [Theory]
    [InlineData("when invoked")]          // only called or handled
    [InlineData("then returns")]          // returns needs a value
    [InlineData("then responds 5")]       // responds needs a record
    [InlineData("then explodes")]         // unknown outcome
    [InlineData("given items")]           // binding needs = value
    [InlineData("given items = [1, }")]   // broken list (lexer also reports PH109)
    public void Malformed_spec_lines_are_PH200(string line)
    {
        // Given a spec with one malformed line next to valid ones
        var lines = new List<string> { "when called", "then returns 0" };
        if (line.StartsWith("given")) lines.Insert(0, line);
        else if (line.StartsWith("when")) lines[0] = line;
        else lines[1] = line;
        var source = PhraseWithSpec("  spec \"bad\"\n" + string.Concat(lines.Select(l => $"    {l}\n")));

        // When it is parsed
        // Then a syntax error is reported
        Assert.Contains("PH200", Codes(source));
    }
}
