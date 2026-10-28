using Phrase.Compiler.Semantics;
using static Phrase.Compiler.Tests.Semantics.Resolved;

namespace Phrase.Compiler.Tests.Semantics;

// Feature: Endpoint flow and composite phrases
//   An endpoint with an output ends by responding (or failing). Composite phrases
//   use their holes as variables, and their failures and effects propagate to callers.
public class ResolverFlowTests
{
    [Fact]
    [Trait("Slice", "159")]
    public void Ensure_resolves_its_typed_condition_and_failure()
    {
        // Given a phrase with a declared text guard
        var source = Program("""
            phrase "validate {title: Text}"
              fails with EmptyTitle
              steps
                ensure title is not blank else EmptyTitle
            """);

        // When the phrase is resolved
        var phrase = Clean(source).Phrase("validate _");

        // Then the condition is typed and the failure is part of its flow
        var ensure = Assert.IsType<BoundEnsureStep>(Assert.Single(phrase.Steps!));
        Assert.Equal(PType.Text, Assert.IsType<BoundBlankCondition>(ensure.Condition).Value.Type);
        Assert.Equal("EmptyTitle", ensure.Code);
        Assert.Equal(["EmptyTitle"], phrase.Failures);
    }

    [Theory]
    [Trait("Slice", "159")]
    [InlineData("ensure title length at most 200 else Missing", "PH326")]
    [InlineData("ensure count length at most 200 else Invalid", "PH312")]
    [InlineData("ensure missing is not blank else Invalid", "PH310")]
    public void Ensure_rejects_undeclared_failures_wrong_types_and_unknown_names(string step, string code)
    {
        // Given an invalid ensure step
        var source = Program($$"""
            phrase "validate {title: Text} count {count: Int}"
              fails with Invalid
              steps
                {{step}}
            """);

        // When the phrase is resolved
        // Then the specific semantic error is reported
        Assert.Equal([code], Codes(source));
    }
    private const string Validate = """
        phrase "validate {items: List<LineItem>}"
          steps
            ensure items is not empty
            ensure every quantity in items is positive

        """;

    [Fact]
    public void Respond_with_builds_the_typed_output_record()
    {
        // Given an endpoint that computes and responds
        var module = Clean(Program(ProbeWithOutput("let total = total quantity of input.items", "respond with { totalQuantity: total }")));

        // When it is resolved
        var respond = Assert.IsType<BoundRespondStep>(ProbeModel(module).Steps[1]);

        // Then the record is the endpoint's output type
        Assert.Equal("Probe.Output", respond.Value.Type.ToString());
        Assert.Equal(new BoundVariable("total", PType.Int), Assert.Single(respond.Value.Fields).Value);
    }

    [Theory]
    [InlineData("respond with {}", "PH314")]                                  // missing totalQuantity
    [InlineData("respond with { totalQuantity: 1, extra: 2 }", "PH314")]       // field not in output
    [InlineData("respond with { totalQuantity: \"x\" }", "PH312")]             // wrong type
    public void The_response_must_match_the_output_shape(string step, string code)
    {
        // Given a respond step that does not fit the output
        var source = Program(ProbeWithOutput(step));

        // When it is resolved
        // Then the mismatch is reported
        Assert.Equal([code], Codes(source));
    }

    [Fact]
    public void Responding_from_an_endpoint_without_output_is_PH322()
    {
        // Given an endpoint with no output section that responds anyway
        var source = Program(Probe("respond with { totalQuantity: 1 }"));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH322"], Codes(source));
    }

    [Fact]
    public void An_endpoint_with_output_that_never_responds_is_PH323()
    {
        // Given an endpoint with output whose steps just validate
        var source = Program(ProbeWithOutput("ensure input.items is not empty"));

        // When it is resolved
        // Then the missing response is reported
        Assert.Equal(["PH323"], Codes(source));
    }

    [Fact]
    public void A_step_after_respond_is_PH324()
    {
        // Given a step after the response
        var source = Program(ProbeWithOutput("respond with { totalQuantity: 1 }", "ensure input.items is not empty"));

        // When it is resolved
        // Then it is reported as unreachable
        Assert.Equal(["PH324"], Codes(source));
    }

    [Fact]
    public void Ending_with_fail_with_is_a_valid_endpoint()
    {
        // Given an endpoint with output that always fails
        // When it is resolved
        var endpoint = ProbeModel(Clean(Program(ProbeWithOutput("fail with NotBuilt"))));

        // Then it is valid and its failure is recorded
        Assert.Equal(["NotBuilt"], endpoint.Failures);
    }

    [Fact]
    public void Endpoint_failures_collect_from_every_called_phrase_sorted()
    {
        // Given an endpoint that calls two guards and then fails itself
        var endpoint = ProbeModel(Clean(Program(Probe(
            "ensure input.items is not empty",
            "ensure every quantity in input.items is positive",
            "fail with NotBuilt"))));

        // When its failures are read
        // Then they are the union, in ordinal order
        Assert.Equal(["EmptyOrder", "InvalidQuantity", "NotBuilt"], endpoint.Failures);
    }

    [Fact]
    public void A_composite_phrase_uses_its_holes_as_variables()
    {
        // Given a composite phrase and an endpoint that calls it
        var module = Clean(Program(Validate, Probe("validate input.items")));

        // When it is resolved
        var validate = module.Phrase("validate _");

        // Then its steps bind to the prelude guards
        Assert.Equal(
            ["ensure _ is not empty", "ensure every quantity in _ is positive"],
            validate.Steps!.Cast<BoundCallStep>().Select(s => s.Call.PhraseKey));
        Assert.Equal(new BoundVariable("items", PType.ListOf(new PType("LineItem", []))), validate.Steps!.Cast<BoundCallStep>().First().Call.Arguments[0].Value);

        // And its failures, and the endpoint's, include the guards' failures
        Assert.Equal(["EmptyOrder", "InvalidQuantity"], validate.Failures);
        Assert.Equal(["EmptyOrder", "InvalidQuantity"], ProbeModel(module).Failures);
    }

    [Theory]
    [InlineData("", "PH326")]
    [InlineData("  fails with Rejected\n", null)]
    public void A_composite_may_only_fail_with_declared_codes(string clause, string? code)
    {
        // Given a composite phrase that fails directly
        var source = Program($"phrase \"reject {{items: List<LineItem>}}\"\n{clause}  steps\n    fail with Rejected\n");

        // When it is resolved
        // Then the code must be declared in fails with
        Assert.Equal(code is null ? [] : [code], Codes(source));
    }

    [Fact]
    public void Phrases_that_call_each_other_in_a_cycle_are_PH327()
    {
        // Given two composites that call each other
        var source = Program(
            "phrase \"ping {n: Int}\"\n  steps\n    pong n\n",
            "phrase \"pong {n: Int}\"\n  steps\n    ping n\n");

        // When it is resolved
        // Then the cycle is reported
        Assert.Contains("PH327", Codes(source));
    }

    [Theory]
    [InlineData("", "PH328")]
    [InlineData("  effects Db\n", null)]
    public void A_composite_must_declare_the_effects_of_what_it_calls(string clause, string? code)
    {
        // Given an effectful leaf and a composite that calls it
        var source = Program(
            "phrase \"save {item: LineItem}\"\n  effects Db\n  native csharp\n    return;\n",
            $"phrase \"store {{item: LineItem}}\"\n{clause}  steps\n    save item\n");

        // When it is resolved
        // Then the callee's effect must be declared
        Assert.Equal(code is null ? [] : [code], Codes(source));
    }

    [Fact]
    public void A_composite_with_a_return_type_is_PH325()
    {
        // Given a composite that claims to return a value
        var source = Program("phrase \"count {items: List<LineItem>}\" -> Int\n  steps\n    ensure items is not empty\n");

        // When it is resolved
        // Then it is reported, since steps have no way to return a value in v0.1
        Assert.Equal(["PH325"], Codes(source));
    }

    [Fact]
    public void Responding_from_a_composite_is_PH322()
    {
        // Given a composite that tries to respond
        var source = Program("phrase \"answer {n: Int}\"\n  steps\n    respond with { totalQuantity: n }\n");

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH322"], Codes(source));
    }
}
