using Avowry.Compiler.Semantics;
using static Phrase.Compiler.Tests.Semantics.Resolved;

namespace Phrase.Compiler.Tests.Semantics;

// Feature: Resolving phrase calls
//   Each step must match exactly one phrase template. Holes match one argument, or
//   one bare word naming a variable in scope. Arguments are type-checked against holes.
public class ResolverCallTests
{
    private static BoundCall CallAt(ModuleModel module, int step) => ProbeModel(module).Steps[step] switch
    {
        BoundCallStep c => c.Call,
        BoundLetStep l => l.Call,
        var other => throw new InvalidOperationException($"Step {step} is a {other.GetType().Name}"),
    };

    [Fact]
    public void A_step_binds_to_its_one_matching_phrase()
    {
        // Given a step that reads like a prelude phrase
        var module = Clean(Program(Probe("ensure input.items is not empty")));

        // When it is resolved
        var step = Assert.IsType<BoundCallStep>(Assert.Single(ProbeModel(module).Steps));

        // Then it calls that phrase with the path bound to the hole
        Assert.Equal("ensure _ is not empty", step.Call.AvowKey);
        var argument = Assert.Single(step.Call.Arguments);
        Assert.Equal("items", argument.Hole.Name);
        var field = Assert.IsType<BoundField>(argument.Value);
        Assert.Equal(("input", "items", "List<LineItem>"), (Assert.IsType<BoundVariable>(field.Target).Name, field.Name, field.Type.ToString()));

        // And it keeps its canonical source text
        Assert.Equal("ensure input.items is not empty", step.Source);
    }

    [Fact]
    public void An_unknown_phrase_is_PH001_and_names_the_closest_templates()
    {
        // Given a step with a typo
        var source = Program(Probe("ensure input.items isnt empty"));

        // When it is resolved
        var diagnostic = Only(source);

        // Then the nearest template is suggested
        Assert.Equal("PH001", diagnostic.Code);
        Assert.Contains("\"ensure {items: List<LineItem>} is not empty\"", diagnostic.Message);
    }

    [Fact]
    public void A_bare_word_where_a_value_belongs_gets_a_did_you_mean()
    {
        // Given a step that names a field of input without the input prefix
        var source = Program(Probe("ensure items is not empty"));

        // When it is resolved
        var diagnostic = Only(source);

        // Then the hint points at the path that would work
        Assert.Equal("PH001", diagnostic.Code);
        Assert.Contains("input.items", diagnostic.Message);
    }

    [Fact]
    public void A_step_matching_two_phrases_is_PH002()
    {
        // Given a hole-taking phrase and a fixed phrase that read the same when a variable is named ready
        var source = Program(
            "avow to \"readiness of {items: List<LineItem>}\" -> Bool\n  native csharp\n    return true;\n",
            "avow to \"ensure {flag: Bool} holds\"\n  native csharp\n    return;\n",
            "avow to \"ensure ready holds\"\n  native csharp\n    return;\n",
            Probe("let ready = readiness of input.items", "ensure ready holds"));

        // When it is resolved
        var diagnostic = Only(source);

        // Then both candidates are listed
        Assert.Equal("PH002", diagnostic.Code);
        Assert.Contains("ensure {flag: Bool} holds", diagnostic.Message);
        Assert.Contains("ensure ready holds", diagnostic.Message);
    }

    [Fact]
    public void A_let_variable_named_by_a_bare_word_fills_a_hole()
    {
        // Given a let followed by a step that uses it by name
        var module = Clean(Program(Probe("let total = total quantity of input.items", "ensure total is positive")));

        // When it is resolved
        var let = Assert.IsType<BoundLetStep>(ProbeModel(module).Steps[0]);
        var call = CallAt(module, 1);

        // Then the let is typed by the phrase's return type and the word binds to it
        Assert.Equal(("total", "Int"), (let.Name, let.Type.ToString()));
        Assert.Equal("ensure _ is positive", call.AvowKey);
        Assert.Equal(new BoundVariable("total", PType.Int), Assert.Single(call.Arguments).Value);
    }

    [Fact]
    public void An_argument_of_the_wrong_type_is_PH312()
    {
        // Given a list passed where an Int is expected
        var diagnostic = Only(Program(Probe("ensure input.items is positive")));

        // When it is resolved
        // Then both types are named
        Assert.Equal("PH312", diagnostic.Code);
        Assert.Contains("Int", diagnostic.Message);
        Assert.Contains("List<LineItem>", diagnostic.Message);
    }

    [Fact]
    public void An_unknown_variable_is_PH310()
    {
        // Given a path rooted at a name that is not in scope
        var diagnostic = Only(Program(Probe("ensure basket.items is not empty")));

        // When it is resolved
        // Then the variable is reported with what is in scope
        Assert.Equal("PH310", diagnostic.Code);
        Assert.Contains("input", diagnostic.Message);
    }

    [Fact]
    public void An_unknown_field_is_PH311_and_lists_the_real_fields()
    {
        // Given a path to a field input does not have
        var diagnostic = Only(Program(Probe("ensure input.lines is not empty")));

        // When it is resolved
        // Then the available fields are listed
        Assert.Equal("PH311", diagnostic.Code);
        Assert.Contains("customerId, items", diagnostic.Message);
    }

    [Fact]
    public void A_field_of_a_non_record_is_PH311()
    {
        // Given a path that reads a field of a list
        var source = Program(
            "avow to \"check sku {sku: Text}\"\n  native csharp\n    return;\n",
            Probe("check sku input.items.sku"));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH311"], Codes(source));
    }

    [Fact]
    public void Letting_a_phrase_that_returns_nothing_is_PH320()
    {
        // Given a let around a guard
        var source = Program(Probe("let ok = ensure input.items is not empty"));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH320"], Codes(source));
    }

    [Fact]
    public void Discarding_a_returned_value_is_PH321()
    {
        // Given a value-returning phrase used as a statement
        var source = Program(Probe("total quantity of input.items"));

        // When it is resolved
        // Then it is reported, because the result is silently lost
        Assert.Equal(["PH321"], Codes(source));
    }

    [Theory]
    [InlineData("let total = total quantity of input.items", "let total = total quantity of input.items")]
    [InlineData("let input = total quantity of input.items", "fail with NotBuilt")]
    public void Redefining_a_variable_is_PH313(string first, string second)
    {
        // Given a let that reuses a name already in scope
        var source = Program(Probe(first, second));

        // When it is resolved
        // Then it is reported
        Assert.Contains("PH313", Codes(source));
    }

    [Theory]
    [InlineData("retry \"sync\" up to 3 times", null)]
    [InlineData("retry \"sync\" up to 2.5 times", "PH312")]
    [InlineData("retry 3 up to 3 times", "PH312")]
    [InlineData("retry true up to 3 times", "PH312")]
    public void Literal_arguments_are_checked_against_hole_types(string step, string? code)
    {
        // Given a phrase with Text and Int holes
        var source = Program(
            "avow to \"retry {name: Text} up to {n: Int} times\"\n  native csharp\n    return;\n",
            Probe(step));

        // When it is resolved
        // Then only well-typed literals are accepted
        Assert.Equal(code is null ? [] : [code], Codes(source));
    }

    [Fact]
    public void Record_and_list_literals_are_checked_field_by_field()
    {
        // Given a list-of-records literal passed to a list hole
        var module = Clean(Program(Probe("ensure [{ sku: \"A\", quantity: 1 }] is not empty")));

        // When it is resolved
        var value = Assert.Single(CallAt(module, 0).Arguments).Value;

        // Then the literal is typed all the way down
        var list = Assert.IsType<BoundList>(value);
        Assert.Equal("List<LineItem>", list.Type.ToString());
        var record = Assert.IsType<BoundRecord>(Assert.Single(list.Items));
        Assert.Equal(["sku: Text", "quantity: Int"], record.Fields.Select(f => $"{f.Name}: {f.Value.Type}"));
    }
}
