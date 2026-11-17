using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;
using Avowry.Adapters.CSharp;
using Avowry.Compiler;

namespace Avowry.Acceptance.Tests;

// Card 165 red, offline half: what the generated C# declares, registers and builds, the one step-id rule, the reserved names,
// and the pins and docs the card re-records. No generated API is started here (FailureObserverLive165 does that).
//
// The slice leaves some forms open; these tests fix the least that the behaviour needs, and each is named in the red record:
//  - the interface, event and no-op live in the module namespace, all in one generated file of the API project;
//  - the host registers the no-op with TryAddSingleton in the generated Program.cs;
//  - each endpoint that catches AvowryFailure takes IAvowryFailureObserver before HandleAsync and builds `new AvowryFailureEvent(` with five
//    arguments after the 422 is sent, in that catch region;
//  - only the failure records' step ids (kind failure) appear in the generated C#, never the ids of endpoint, step or avow records.
public class FailureObserverGenerated165 : IClassFixture<PristineTasks165>
{
    // The Slice 142 pin before card 165, and the golden pieces that must not move (taken from TasksDemoGeneratedCsharp.golden before this card).
    private const string OldOrdersHash = "19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765";
    private const string DemoSpecsHashBeforeCard165 = "8ed00cd91a979bc57d9917966bd4234380813a9b73cc68dcbb1c53d2b7bbba4f";

    private static readonly Regex EventConstruction = new(@"new\s+AvowryFailureEvent\s*\(", RegexOptions.CultureInvariant);

    // Request, exception and source-position data. The only thing read from the caught exception is its declared code.
    private static readonly Regex ForbiddenInEventArguments = new(
        @"\b(?:req|input|request|HttpContext|Query|Headers|Body|Route|ex|exception)\b"
        + @"|\.(?:Message|StackTrace|Data|InnerException|Source|TargetSite)\b"
        + @"|ToString\s*\("
        + @"|\bfailure\s*\.\s*(?!Code\b)",
        RegexOptions.CultureInvariant);

    private readonly PristineTasks165 tasks;

    public FailureObserverGenerated165(PristineTasks165 tasks) => this.tasks = tasks;

    [Theory]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    [InlineData("tasks")]
    [InlineData("orders")]
    public void The_generated_runtime_declares_the_observer_interface_the_five_field_event_and_a_no_op(string which)
    {
        // Given the generated files of a module
        var output = Compile(which);
        var module = output.Module!.Name;

        // When the file that declares the observer interface is looked for
        var holders = output.Files.Where(file => file.Value.Contains("interface IAvowryFailureObserver", StringComparison.Ordinal)).ToArray();

        // Then exactly one API file declares the interface, the event record and the no-op, in the module namespace
        Assert.True(holders.Length == 1, $"Expected one generated file declaring IAvowryFailureObserver, found {holders.Length}.");
        var (path, text) = holders[0];
        Assert.StartsWith(module + ".Api/", path, StringComparison.Ordinal);
        var code = Flatten(StripComments(text));
        Assert.Matches("namespace " + Regex.Escape(module) + " ?[;{]", code);
        Assert.Matches(@"public interface IAvowryFailureObserver ?\{ ?void OnFailure\(AvowryFailureEvent \w+\); ?\}", code);
        Assert.Matches(
            @"public sealed record AvowryFailureEvent\(int SchemaVersion, string Code, string Endpoint, string StepId, string BuildId\);",
            code);
        Assert.Matches(
            @"public sealed class NoOpAvowryFailureObserver : IAvowryFailureObserver ?\{ ?public void OnFailure\(AvowryFailureEvent \w+\) ?\{ ?\} ?\}",
            code);

        // And the runtime file knows nothing of the authoring layer or the map
        Assert.DoesNotContain(".avowry", text, StringComparison.Ordinal);
        Assert.DoesNotContain("avowrymap", text, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    [InlineData("tasks")]
    [InlineData("orders")]
    public void The_generated_host_registers_the_no_op_with_TryAddSingleton_so_a_host_can_register_first(string which)
    {
        // Given the generated Program.cs of a module
        var output = Compile(which);
        var program = output.Files[output.Module!.Name + ".Api/Program.cs"];

        // Then the no-op is registered with TryAddSingleton for the observer interface, and never with a plain AddSingleton that would replace a host's own
        Assert.Matches(@"TryAddSingleton<\s*(?:\w+\.)*IAvowryFailureObserver\s*,\s*(?:\w+\.)*NoOpAvowryFailureObserver\s*>\s*\(\s*\)", program);
        Assert.DoesNotMatch(@"(?<!Try)AddSingleton<\s*(?:\w+\.)*IAvowryFailureObserver", program);
        Assert.DoesNotMatch(@"\bReplace\s*\(", program);
    }

    [Theory]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    [InlineData("tasks", 5)]
    [InlineData("orders", 1)]
    public void Each_endpoint_that_catches_a_declared_failure_takes_the_observer_and_builds_the_event_after_the_422(string which, int minimum)
    {
        // Given the generated endpoint files that catch AvowryFailure
        var output = Compile(which);
        var module = output.Module!.Name;
        var endpointFiles = output.Files
            .Where(file => file.Key.StartsWith(module + ".Api/", StringComparison.Ordinal) && file.Value.Contains("catch (AvowryFailure", StringComparison.Ordinal))
            .ToArray();
        Assert.True(endpointFiles.Length >= minimum, $"Expected at least {minimum} endpoint files with a failure catch, found {endpointFiles.Length}.");

        foreach (var (path, text) in endpointFiles)
        {
            // Then the endpoint receives the observer before HandleAsync (constructor injection)
            var handle = text.IndexOf("HandleAsync(", StringComparison.Ordinal);
            var observer = text.IndexOf("IAvowryFailureObserver", StringComparison.Ordinal);
            Assert.True(observer >= 0 && observer < handle, $"{path}: the endpoint does not receive IAvowryFailureObserver before HandleAsync.");

            // And the event is built in the catch, after the 422 is sent and after the declared error is added
            var caught = text.IndexOf("catch (AvowryFailure", StringComparison.Ordinal);
            var added = text.IndexOf("AddError(", caught, StringComparison.Ordinal);
            var sent = text.IndexOf("Send.ErrorsAsync(422", caught, StringComparison.Ordinal);
            var built = EventConstruction.Match(text, caught);
            Assert.True(added > caught && sent > added, $"{path}: the catch no longer adds the error and sends the 422.");
            Assert.True(built.Success && built.Index > sent, $"{path}: no `new AvowryFailureEvent(` after Send.ErrorsAsync(422 in the catch.");
        }
    }

    [Theory]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    [InlineData("tasks")]
    [InlineData("orders")]
    public void The_event_is_built_from_five_arguments_with_no_request_exception_or_source_data(string which)
    {
        // Given the catch region of every generated endpoint that builds the event
        var output = Compile(which);
        var module = output.Module!.Name;
        var regions = output.Files
            .Where(file => file.Key.StartsWith(module + ".Api/", StringComparison.Ordinal) && file.Value.Contains("catch (AvowryFailure", StringComparison.Ordinal))
            .Select(file => (file.Key, Region: file.Value[file.Value.IndexOf("catch (AvowryFailure", StringComparison.Ordinal)..]))
            .ToArray();
        Assert.NotEmpty(regions);

        foreach (var (path, region) in regions)
        {
            // Then the event takes schema version, code, endpoint, step id and build id, and nothing it reads is request, exception or source data
            var built = EventConstruction.Match(region);
            Assert.True(built.Success, $"{path}: the catch does not build an AvowryFailureEvent.");
            var arguments = BalancedArguments(region, built.Index + built.Length);
            Assert.True(arguments is not null, $"{path}: the AvowryFailureEvent arguments are not balanced.");
            Assert.Equal(5, TopLevelArgumentCount(arguments!));
            var forbidden = ForbiddenInEventArguments.Match(arguments!);
            Assert.False(forbidden.Success, $"{path}: the event arguments read '{forbidden.Value}': {arguments}");

            // And the catch region names no Phrase file and reads nothing from the exception except its code
            Assert.DoesNotContain(".avowry", region, StringComparison.Ordinal);
            Assert.DoesNotMatch(@"\bfailure\s*\.\s*(?!Code\b)", region);
        }
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    public void Only_the_failure_step_ids_of_the_map_are_in_the_generated_csharp_on_lines_that_hold_no_source_knowledge()
    {
        // Given the Tasks map written by the CLI and the generated C# beside it
        var failureIds = tasks.Records.Where(record => record.Kind == "failure").Select(record => record.StepId).Distinct(StringComparer.Ordinal).ToArray();
        var otherIds = tasks.Records.Where(record => record.Kind != "failure").Select(record => record.StepId)
            .Except(failureIds, StringComparer.Ordinal).ToArray();
        var specTexts = tasks.SourceLines.Select(line => line.Trim()).Where(line => line.StartsWith("spec \"", StringComparison.Ordinal))
            .Select(line => line["spec \"".Length..line.LastIndexOf('"')]).ToArray();
        Assert.Equal(6, failureIds.Length);
        Assert.Equal(11, specTexts.Length);

        // Then every failure id is in the API C# (the compile-time lookup), and only there
        foreach (var id in failureIds)
        {
            var holders = tasks.Generated.Where(file => file.Value.Contains(id, StringComparison.Ordinal)).ToArray();
            Assert.True(holders.Length > 0, $"Failure step id {id} is not in any generated C# file.");
            foreach (var (path, text) in holders)
            {
                Assert.StartsWith("Tasks.Api/", path, StringComparison.Ordinal);

                // And no line carrying an id names the Phrase file, a line number or a spec's text
                foreach (var line in text.Split('\n').Where(candidate => candidate.Contains(id, StringComparison.Ordinal)))
                {
                    Assert.DoesNotContain(".avowry", line, StringComparison.Ordinal);
                    Assert.DoesNotMatch(@"(?i)\bline\s*[:=]?\s*\d", line);
                    foreach (var spec in specTexts)
                        Assert.DoesNotContain(spec, line, StringComparison.Ordinal);
                }
            }
        }

        // And the ids of endpoint, step and avow records are not in the generated C# at all
        foreach (var id in otherIds)
        {
            var leaked = tasks.Generated.Where(file => file.Value.Contains(id, StringComparison.Ordinal)).Select(file => file.Key).ToArray();
            Assert.True(leaked.Length == 0, $"Step id {id} of a non-failure record is in: {string.Join(", ", leaked)}");
        }
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    public void One_public_step_id_rule_lives_in_the_compiler_and_both_the_cli_and_the_adapter_call_it()
    {
        // Given the compiler assembly: a public static StepId(string kind, int line, string name, string endpoint)
        var methods = typeof(AvowryCompiler).Assembly.GetExportedTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(method => method.Name == "StepId"
                && method.GetParameters().Select(parameter => parameter.ParameterType)
                    .SequenceEqual(new[] { typeof(string), typeof(int), typeof(string), typeof(string) }))
            .ToArray();
        Assert.True(methods.Length == 1, $"Expected one public static StepId(string, int, string, string) in Avowry.Compiler, found {methods.Length}.");

        // When it is asked for every record of the Tasks map the CLI wrote
        // Then it returns the id the CLI wrote, for all four kinds
        foreach (var record in tasks.Records)
        {
            var actual = (string?)methods[0].Invoke(null, new object[] { record.Kind, record.Line, record.Name, record.Endpoint });
            Assert.Equal(record.StepId, actual);
        }

        // And the CLI no longer holds its own hash recipe (a one-line forwarder is fine) and the adapter calls the shared rule
        var cli = File.ReadAllText(RepoPaths.Find("src", "Avowry.Cli", "Program.cs"));
        var adapter = File.ReadAllText(RepoPaths.Find("src", "Avowry.Adapters.CSharp", "CSharpProvider.cs"));
        Assert.DoesNotContain("normalizedName", cli, StringComparison.Ordinal);
        Assert.Matches(@"\b[A-Za-z_]\w*\.StepId\s*\(", cli);
        Assert.Matches(@"\b[A-Za-z_]\w*\.StepId\s*\(", adapter);
    }

    [Theory]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    [InlineData("IAvowryFailureObserver")]
    [InlineData("AvowryFailureEvent")]
    [InlineData("NoOpAvowryFailureObserver")]
    public void A_type_named_like_a_generated_observer_type_is_a_reserved_name_diagnostic(string name)
    {
        // Given a module whose own type takes a name the generated runtime now declares
        var source = "module M\n\ntype " + name + "\n  n: Int\n";

        // When it is compiled with the C# provider
        var failure = Assert.Throws<CompilationFailedException>(() => AvowryCompiler.Compile(source, new CSharpProvider(), "M.avowry"));

        // Then it is the reserved-name diagnostic AV341 naming the type
        var diagnostic = Assert.Single(failure.Diagnostics);
        Assert.Equal("AV341", diagnostic.Code);
        Assert.Contains($"'{name}'", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-3")]
    [Trait("Kind", "BornGreen")]
    public void The_eleven_demo_spec_files_are_byte_identical_apart_from_how_the_endpoint_is_constructed()
    {
        // Given the generated spec files of the Tasks demo, as the CLI wrote them
        var snapshot = new StringBuilder();
        foreach (var (path, text) in tasks.Generated.Where(file => file.Key.StartsWith("Tasks.Specs/", StringComparison.Ordinal)).OrderBy(file => file.Key, StringComparer.Ordinal))
        {
            snapshot.Append("### ").Append(path).Append('\n').Append(text.EndsWith('\n') ? text : text + "\n");
        }

        // When the lines that build the endpoint under test are set aside (the observer is a new constructor dependency)
        var all = snapshot.ToString().Split('\n');
        var lines = all[^1].Length == 0 ? all[..^1] : all;
        var construction = lines.Where(line => line.Contains("Factory.Create<", StringComparison.Ordinal)).ToArray();
        var kept = lines.Where(line => !line.Contains("Factory.Create<", StringComparison.Ordinal)
            && !line.Contains("AvowryFailureObserver", StringComparison.Ordinal));

        // Then there are still eleven of them, and every other byte equals the golden recorded before this card
        Assert.Equal(11, construction.Length);
        Assert.Equal(11, lines.Count(line => line.Contains("[Fact(DisplayName", StringComparison.Ordinal)));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Concat(kept.Select(line => line + "\n"))))).ToLowerInvariant();
        Assert.True(hash == DemoSpecsHashBeforeCard165, "The demo spec files changed beyond the endpoint construction lines. Hash " + hash);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-3")]
    public void The_orders_pin_is_re_recorded_with_a_comment_naming_the_observer()
    {
        // Given the Slice 142 pin
        var text = FailureObserver165.Normalize(File.ReadAllText(RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "WebExtensionBoundary.cs")));
        var pin = Regex.Match(text, "private const string OrdersOutputHash = \"([0-9a-f]{64})\";");
        Assert.True(pin.Success, "OrdersOutputHash was not found in WebExtensionBoundary.cs.");

        // Then the hash is no longer the one recorded before the card (the runtime change is in the orders output too)
        Assert.NotEqual(OldOrdersHash, pin.Groups[1].Value);

        // And the comment block directly above the pin names card 165 and the observer
        var lines = text.Split('\n');
        var at = Array.FindIndex(lines, line => line.Contains("OrdersOutputHash = \"", StringComparison.Ordinal));
        var comment = new List<string>();
        for (var index = at - 1; index >= 0 && lines[index].TrimStart().StartsWith("//", StringComparison.Ordinal); index--)
            comment.Add(lines[index]);
        var block = string.Join("\n", comment);
        Assert.Contains("165", block, StringComparison.Ordinal);
        Assert.Contains("observer", block, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-3")]
    public void The_demo_golden_is_re_recorded_with_the_observer_and_a_comment_naming_it()
    {
        // Given the Slice 176 golden and its test
        var golden = FailureObserver165.Normalize(File.ReadAllText(RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "TasksDemoGeneratedCsharp.golden")));
        var test = FailureObserver165.Normalize(File.ReadAllText(RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "TasksDemoGeneratedCsharp.cs")));

        // Then the golden holds the observer runtime and the no-op registration
        Assert.Contains("public interface IAvowryFailureObserver", golden, StringComparison.Ordinal);
        Assert.Contains("TryAddSingleton", golden, StringComparison.Ordinal);

        // And a comment in the golden's test names card 165 and the observer as the reason it was re-recorded
        var named = test.Split('\n')
            .Where(line => line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                && line.Contains("165", StringComparison.Ordinal)
                && line.Contains("observer", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        Assert.NotEmpty(named);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-2")]
    public void The_traceability_doc_shows_the_observer_the_production_log_line_and_tracing_it_by_step_id()
    {
        // Given docs/TRACEABILITY.md
        var docs = File.ReadAllText(RepoPaths.Find("docs", "TRACEABILITY.md"));

        // Then it names the observer interface, shows the plain logging line and says how a step id resolves
        Assert.Contains("IAvowryFailureObserver", docs, StringComparison.Ordinal);
        Assert.Contains("avowry-failure v1 code=", docs, StringComparison.Ordinal);
        Assert.Contains("stepId=", docs, StringComparison.Ordinal);
        Assert.Matches(@"(?i)avowry trace\s+(?:<?step ?id>?|[0-9a-f]{12})\b", docs);
    }

    private static CompilationOutput Compile(string which) => which == "tasks"
        ? AvowryCompiler.Compile(File.ReadAllText(RepoPaths.Find("demo", "tasks", "Tasks.avowry")), new CSharpProvider(), "Tasks.avowry")
        : AvowryCompiler.Compile(Fixture.Read("orders.avowry"), new CSharpProvider(), "orders.avowry");

    private static string StripComments(string text) =>
        Regex.Replace(Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\n]*", "");

    private static string Flatten(string text) => Regex.Replace(text, @"\s+", " ");

    // The text between the opening parenthesis (already consumed, at `start`) and its matching closing one.
    private static string? BalancedArguments(string code, int start)
    {
        var depth = 1;
        for (var index = start; index < code.Length; index++)
        {
            if (code[index] == '(')
                depth++;
            else if (code[index] == ')' && --depth == 0)
                return code[start..index];
        }

        return null;
    }

    private static int TopLevelArgumentCount(string arguments)
    {
        var depth = 0;
        var count = 1;
        var inString = false;
        for (var index = 0; index < arguments.Length; index++)
        {
            var c = arguments[index];
            if (inString)
            {
                if (c == '\\')
                    index++;
                else if (c == '"')
                    inString = false;
                continue;
            }

            if (c == '"')
                inString = true;
            else if (c is '(' or '[' or '{')
                depth++;
            else if (c is ')' or ']' or '}')
                depth--;
            else if (c == ',' && depth == 0)
                count++;
        }

        return count;
    }
}
