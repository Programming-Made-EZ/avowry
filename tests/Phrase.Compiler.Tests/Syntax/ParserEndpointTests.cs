using Avowry.Compiler.Syntax;
using static Phrase.Compiler.Tests.Syntax.Parsed;

namespace Phrase.Compiler.Tests.Syntax;

// Feature: Endpoints and steps
//   An endpoint is an HTTP binding plus steps. Steps are let, respond with,
//   fail with, or a phrase call kept as words and arguments for the resolver.
public class ParserEndpointTests
{
    private const string MinimalBody = """
          input
            customerId: Id
          steps
            fail with NotBuilt
        """;

    private static EndpointDecl SingleEndpoint(string source) => Assert.Single(Clean(InModule(source)).Endpoints);

    [Fact]
    public void The_header_names_the_endpoint_verb_and_route()
    {
        // Given an endpoint header
        var endpoint = SingleEndpoint("endpoint CreateOrder : POST /orders\n" + MinimalBody);

        // When it is parsed
        // Then name, verb and route are captured
        Assert.Equal(("CreateOrder", "POST", "/orders"), (endpoint.Name, endpoint.Verb, endpoint.Route));
    }

    [Theory]
    [InlineData("/", "/")]
    [InlineData("/orders/{orderId}/lines", "/orders/{orderId}/lines")]
    [InlineData("/v2/order-lines", "/v2/order-lines")]
    public void Routes_are_rebuilt_from_their_segments(string written, string expected)
    {
        // Given an endpoint with a multi-segment route
        var endpoint = SingleEndpoint($"endpoint GetThing : GET {written}\n" + MinimalBody);

        // When it is parsed
        // Then the route text is canonical
        Assert.Equal(expected, endpoint.Route);
    }

    [Fact]
    public void An_unknown_http_verb_is_PH209()
    {
        // Given an endpoint with a made-up verb
        var source = InModule("endpoint CreateOrder : CREATE /orders\n" + MinimalBody);

        // When it is parsed
        // Then the verb is reported
        Assert.Equal(["PH209"], Codes(source));
    }

    [Fact]
    public void Input_and_output_are_field_lists()
    {
        // Given an endpoint with request and response shapes
        var endpoint = SingleEndpoint("""
            endpoint CreateOrder : POST /orders
              input
                customerId: Id
                items: List<LineItem>
              output
                totalQuantity: Int
              steps
                fail with NotBuilt
            """);

        // When it is parsed
        // Then both shapes are captured
        Assert.Equal(["customerId: Id", "items: List<LineItem>"], endpoint.Input.Select(Print));
        Assert.Equal(["totalQuantity: Int"], endpoint.Output.Select(Print));
    }

    [Fact]
    public void Steps_keep_words_and_arguments_in_order()
    {
        // Given the orders endpoint steps
        var endpoint = SingleEndpoint("""
            endpoint CreateOrder : POST /orders
              input
                items: List<LineItem>
              steps
                ensure input.items is not empty
                ensure every quantity in input.items is positive
                let total = total quantity of input.items
                respond with { totalQuantity: total }
            """);

        // When they are parsed
        // Then dotted paths are arguments and bare names are words
        Assert.Equal(
            [
                "(call ensure <input.items> is not empty)",
                "(call ensure every quantity in <input.items> is positive)",
                "(let total (call total quantity of <input.items>))",
                "(respond {totalQuantity: total})",
            ],
            endpoint.Steps.Select(Print));
    }

    [Fact]
    public void Literals_records_and_lists_in_a_call_are_arguments()
    {
        // Given a step passing several literal arguments
        var endpoint = SingleEndpoint("""
            endpoint Seed : POST /seed
              input
                n: Int
              steps
                retry "sync" up to 3 times with [1, 2.5] and { fast: true }
            """);

        // When it is parsed
        // Then every literal is an argument
        Assert.Equal(
            "(call retry <\"sync\"> up to <3> times with <[1, 2.5]> and <{fast: true}>)",
            Print(Assert.Single(endpoint.Steps)));
    }

    [Fact]
    public void Fail_with_names_a_failure_code()
    {
        // Given a step that fails directly
        var endpoint = SingleEndpoint("endpoint Stub : GET /stub\n" + MinimalBody);

        // When it is parsed
        // Then the code is captured
        Assert.Equal("(fail NotBuilt)", Print(Assert.Single(endpoint.Steps)));
    }

    [Theory]
    [InlineData("let = total of input.items")]   // missing variable name
    [InlineData("let total total of input.items")] // missing '='
    [InlineData("let total =")]                  // nothing to call
    [InlineData("respond with total")]           // respond needs a record
    [InlineData("fail with oops")]               // codes are capitalized
    [InlineData("ensure Customer exists")]       // type names are not call parts in v0.1
    public void Malformed_steps_are_PH200(string step)
    {
        // Given an endpoint with one malformed step
        var source = InModule($"endpoint Stub : GET /stub\n  input\n    n: Int\n  steps\n    {step}\n");

        // When it is parsed
        // Then it is reported as a syntax error
        Assert.Equal(["PH200"], Codes(source));
    }

    [Fact]
    public void Missing_input_or_steps_is_PH203()
    {
        // Given an endpoint with neither section
        var source = InModule("""
            endpoint Stub : GET /stub
              output
                n: Int
            """);

        // When it is parsed
        // Then each missing section is reported
        Assert.Equal(["PH203", "PH203"], Codes(source));
    }

    [Fact]
    public void A_repeated_section_is_PH204()
    {
        // Given an endpoint with two input sections
        var source = InModule("""
            endpoint Stub : GET /stub
              input
                a: Int
              input
                b: Int
              steps
                fail with NotBuilt
            """);

        // When it is parsed
        // Then the second section is reported
        Assert.Equal(["PH204"], Codes(source));
    }
}
