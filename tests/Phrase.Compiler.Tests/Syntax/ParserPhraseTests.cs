using Phrase.Compiler.Syntax;
using static Phrase.Compiler.Tests.Syntax.Parsed;

namespace Phrase.Compiler.Tests.Syntax;

// Feature: Phrase declarations
//   A phrase is a template (its name), an optional return type, its contract,
//   its specs, and an implementation: native blocks (leaf) or steps (composite).
public class ParserPhraseTests
{
    [Theory]
    [Trait("Slice", "159")]
    [InlineData("200.5")]
    [InlineData("-1")]
    [InlineData("99999999999")]
    [InlineData("-99999999999")]
    public void Length_maximum_must_be_a_non_negative_32_bit_integer(string maximum)
    {
        // Given an ensure length condition whose maximum is not a supported integer
        var source = InModule($$"""
            phrase "validate {title: Text}"
              fails with Invalid
              steps
                ensure title length at most {{maximum}} else Invalid
            """);

        // When the phrase is parsed
        var result = Parser.Parse(source);

        // Then parsing returns a normal, clear diagnostic rather than throwing
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("PH200", diagnostic.Code);
        Assert.Contains("non-negative 32-bit integer", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "159")]
    public void Ensure_is_only_a_step_keyword_at_the_start_of_a_step_line()
    {
        // Given ensure appears in native code, a spec string, and an identifier
        var source = InModule("""
            phrase "preserve {ensureValue: Int}" -> Int
              spec "ensure remains ordinary text"
                given ensureValue = 1
                when called
                then returns 1
              native csharp
                var ensure = ensureValue;
                return ensure;
            """);

        // When it is parsed
        var phrase = Assert.Single(Clean(source).Phrases);

        // Then those occurrences remain their original text and are not ensure steps
        Assert.Null(phrase.Steps);
        Assert.Equal("ensure remains ordinary text", Assert.Single(phrase.Specs).Description);
        Assert.Contains("var ensure = ensureValue;", Assert.Single(phrase.Natives).Code, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "159")]
    public void Ensure_conditions_are_parsed_as_steps()
    {
        // Given a phrase with each ensure condition shape
        var source = InModule("""
            phrase "validate {title: Text} count {count: Int} limit {limit: Int}"
              fails with Invalid
              steps
                ensure title is not blank else Invalid
                ensure title length at most 200 else Invalid
                ensure count <= limit else Invalid
            """);

        // When the phrase is parsed
        var phrase = Assert.Single(Clean(source).Phrases);

        // Then each condition and its declared failure are preserved in the AST
        Assert.Equal([
            "(ensure title is not blank else Invalid)",
            "(ensure title length at most 200 else Invalid)",
            "(ensure count <= limit else Invalid)"], phrase.Steps!.Select(Print));
    }

    [Fact]
    [Trait("Slice", "160")]
    public void Record_update_form_is_parsed_as_a_let_step()
    {
        // Given a phrase that copies a record with one named field replaced
        var source = InModule("""
            phrase "complete {task: Task}"
              steps
                let completed = task with { done: true }
            """);

        // When the phrase is parsed
        var phrase = Assert.Single(Clean(source).Phrases);

        // Then the update keeps the source record, the replaced field, and the new value
        Assert.Equal(["(let completed task with {done: true})"], phrase.Steps!.Select(Print));
        var update = Assert.IsType<LetUpdateStep>(Assert.Single(phrase.Steps!));
        Assert.Equal("completed", update.Name);
        Assert.Equal(["task"], update.Source.Segments);
        var field = Assert.Single(update.Fields.Fields);
        Assert.Equal("done", field.Name);
        Assert.True(Assert.IsType<BoolLit>(field.Value).Value);
    }

    [Theory]
    [Trait("Slice", "160")]
    [InlineData("let completed = task with { done: true", "PH200")]
    [InlineData("let completed = task with { }", "PH200")]
    [InlineData("let completed = task with { done: true, }", "PH200")]
    [InlineData("let completed = task with { owner: owner with { name: \"x\" } }", "PH200")]
    [InlineData("let completed = task with { done true }", "PH200")]
    [InlineData("let completed = task with { done: }", "PH200")]
    public void Malformed_record_updates_report_a_normal_diagnostic(string step, string code)
    {
        // Given a malformed update in a phrase whose source is in scope
        var source = InModule($$"""
            type Task
              done: Bool

            phrase "complete {task: Task} and {owner: Task}"
              steps
                {{step}}
            """);

        // When the malformed source is parsed
        var result = Parser.Parse(source);

        // Then parsing does not throw and reports the ordinary syntax diagnostic
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == code);
    }

    [Fact]
    [Trait("Slice", "160")]
    public void A_phrase_call_containing_with_and_a_record_is_not_reserved_as_an_update()
    {
        // Given a phrase whose wording contains "with" before a record argument
        var source = InModule("""
            type Options
              urgent: Bool

            phrase "send with {options: Options}" -> Bool
              native csharp
                return options.Urgent;

            phrase "dispatch"
              steps
                let sent = send with { urgent: true }
            """);

        // When the caller is parsed
        var caller = Clean(source).Phrases.Single(phrase => phrase.Template.Parts.OfType<TemplateWord>().Any(word => word.Text == "dispatch"));

        // Then it remains a phrase call because "send" is not an in-scope update source
        var let = Assert.IsType<LetStep>(Assert.Single(caller.Steps!));
        Assert.Equal("(call send with <{urgent: true}>)", Print(let.Call));
    }

    private static PhraseDecl SinglePhrase(string body) => Assert.Single(Clean(InModule(body)).Phrases);

    [Fact]
    public void A_template_splits_into_words_and_typed_holes()
    {
        // Given a leaf phrase with one hole
        var phrase = SinglePhrase("""
            phrase "ensure {items: List<LineItem>} is not empty"
              native csharp
                if (items.Count == 0) Fail("EmptyOrder");
            """);

        // When its template is read
        var parts = phrase.Template.Parts;

        // Then it is a word, a hole, and three words
        Assert.Equal("ensure {items: List<LineItem>} is not empty", Print(phrase.Template));
        Assert.IsType<TemplateWord>(parts[0]);
        Assert.Equal("items", Assert.IsType<TemplateHole>(parts[1]).Name);
    }

    [Fact]
    public void Template_parts_point_into_the_string()
    {
        // Given a phrase whose template starts at column 8
        var phrase = SinglePhrase("""
            phrase "total of {items: List<LineItem>}"
              native csharp
                return 0;
            """);

        // When the parts are read
        var parts = phrase.Template.Parts;

        // Then positions are inside the quotes
        Assert.Equal(new Position(2, 9), parts[0].At);  // "total"
        Assert.Equal(new Position(2, 18), parts[2].At); // "{items..."
    }

    [Fact]
    public void A_return_type_follows_the_arrow()
    {
        // Given a phrase that returns a value
        var phrase = SinglePhrase("""
            phrase "total quantity of {items: List<LineItem>}" -> Int
              native csharp
                return items.Sum(i => i.Quantity);
            """);

        // When it is parsed
        // Then the return type is captured
        Assert.Equal("Int", Print(phrase.Returns!));
    }

    [Fact]
    public void A_phrase_without_an_arrow_returns_nothing()
    {
        // Given a guard phrase
        var phrase = SinglePhrase("""
            phrase "ensure {items: List<LineItem>} is not empty"
              native csharp
                return;
            """);

        // When it is parsed
        // Then there is no return type
        Assert.Null(phrase.Returns);
    }

    [Fact]
    public void Failures_and_effects_are_lists_of_type_names()
    {
        // Given a phrase declaring two failures and two effects
        var phrase = SinglePhrase("""
            phrase "save {order: Order}"
              effects Db, Bus
              fails with Conflict, Timeout
              native csharp
                return;
            """);

        // When it is parsed
        // Then both lists are captured in order
        Assert.Equal(["Db", "Bus"], phrase.Effects);
        Assert.Equal(["Conflict", "Timeout"], phrase.Failures);
    }

    [Fact]
    public void Effects_none_is_an_empty_list()
    {
        // Given a phrase that declares it has no effects
        var phrase = SinglePhrase("""
            phrase "double {n: Int}" -> Int
              effects none
              native csharp
                return n * 2;
            """);

        // When it is parsed
        // Then it has no effects
        Assert.Empty(phrase.Effects);
    }

    [Fact]
    public void Each_native_block_records_its_target_and_code()
    {
        // Given a phrase implemented for two targets
        var phrase = SinglePhrase("""
            phrase "double {n: Int}" -> Int
              native csharp
                return n * 2;
              native typescript
                return n * 2
            """);

        // When it is parsed
        // Then both bodies are kept, keyed by target
        Assert.Equal(
            [("csharp", "return n * 2;"), ("typescript", "return n * 2")],
            phrase.Natives.Select(n => (n.Target, n.Code)));
        Assert.Null(phrase.Steps);
    }

    [Fact]
    public void A_composite_phrase_has_steps_instead_of_native_code()
    {
        // Given a phrase built from other phrases
        var phrase = SinglePhrase("""
            phrase "validate {items: List<LineItem>}"
              steps
                ensure items is not empty
                ensure every quantity in items is positive
            """);

        // When it is parsed
        // Then its steps are kept and it has no natives
        Assert.Empty(phrase.Natives);
        Assert.Equal(
            "(call ensure items is not empty) (call ensure every quantity in items is positive)",
            Print(phrase.Steps!));
    }

    [Fact]
    public void A_phrase_with_no_implementation_is_PH203()
    {
        // Given a phrase with only a contract
        var source = InModule("""
            phrase "ensure {items: List<LineItem>} is not empty"
              fails with EmptyOrder
            """);

        // When it is parsed
        // Then the missing implementation is reported
        Assert.Equal(["PH203"], Codes(source));
    }

    [Fact]
    public void A_phrase_with_both_native_code_and_steps_is_PH208()
    {
        // Given a phrase with both kinds of implementation
        var source = InModule("""
            phrase "double {n: Int}" -> Int
              native csharp
                return n * 2;
              steps
                fail with Nope
            """);

        // When it is parsed
        // Then the conflict is reported
        Assert.Equal(["PH208"], Codes(source));
    }

    [Fact]
    public void Two_native_blocks_for_the_same_target_is_PH204()
    {
        // Given two csharp bodies
        var source = InModule("""
            phrase "double {n: Int}" -> Int
              native csharp
                return n * 2;
              native csharp
                return n + n;
            """);

        // When it is parsed
        // Then the duplicate is reported
        Assert.Equal(["PH204"], Codes(source));
    }

    [Fact]
    public void An_unknown_native_target_is_PH207()
    {
        // Given a body for a target with no provider
        var source = InModule("""
            phrase "double {n: Int}" -> Int
              native cobol
                MULTIPLY N BY 2.
            """);

        // When it is parsed
        // Then the target is reported
        Assert.Equal(["PH207"], Codes(source));
    }

    [Fact]
    public void A_second_fails_with_clause_is_PH204()
    {
        // Given a phrase that splits its failures over two lines
        var source = InModule("""
            phrase "save {order: Order}"
              fails with Conflict
              fails with Timeout
              native csharp
                return;
            """);

        // When it is parsed
        // Then the second clause is reported
        Assert.Equal(["PH204"], Codes(source));
    }

    [Theory]
    [InlineData("{a: Int} {b: Int}", "PH205")]                 // no words: every call would match
    [InlineData("ensure {items List<LineItem>} ok", "PH205")]  // hole missing its colon
    [InlineData("ensure {items: List<LineItem> ok", "PH205")]  // hole never closed
    [InlineData("ensure {Items: Int} ok", "PH205")]            // hole names are lowercase
    [InlineData("total of #1", "PH205")]                       // '#' would be read as a comment
    [InlineData("let total be {n: Int}", "PH205")]             // starts with a step keyword
    [InlineData("respond with {n: Int}", "PH205")]
    [InlineData("fail with {n: Int}", "PH205")]
    [InlineData("native {n: Int}", "PH205")]
    [InlineData("add {n: Int} to {n: Int}", "PH206")]          // duplicate hole name
    public void Invalid_templates_are_reported(string template, string code)
    {
        // Given a phrase with a malformed template
        var source = InModule($"phrase \"{template}\"\n  native csharp\n    return;\n");

        // When it is parsed
        // Then the template is rejected with a specific code
        Assert.Equal([code], Codes(source));
    }

    [Fact]
    public void An_unknown_clause_in_a_phrase_is_PH200()
    {
        // Given a phrase with a line that is not a phrase clause
        var source = InModule("""
            phrase "double {n: Int}" -> Int
              returns twice n
              native csharp
                return n * 2;
            """);

        // When it is parsed
        var result = Parser.Parse(source);

        // Then the line is reported
        Assert.Equal(["PH200"], result.Diagnostics.Select(d => d.Code));

        // And the phrase is still kept with its native block
        Assert.Single(Assert.Single(result.File.Phrases).Natives);
    }
}
