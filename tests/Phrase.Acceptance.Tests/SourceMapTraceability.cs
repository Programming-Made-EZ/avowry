using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler.Semantics;

namespace Phrase.Acceptance.Tests;

// Feature: generated failures point back to the Phrase source without leaking it at runtime
//   As Morgan, I want a separate deterministic source map and a trace command,
//   so that I can find the owning spec without reading or exposing generated implementation details.
public class SourceMapTraceability
{
    [Fact]
    [Trait("Slice", "170")]
    public async Task Failures_inherited_through_a_composite_phrase_still_get_a_failure_record()
    {
        // Given Morgan has an endpoint whose composite phrase inherits a failure declared by its leaf
        const string sourceText = """
            module CompositeFailures

            avow to "check invalid value" -> Int
              fails with InvalidValue
              native csharp
                Fail("InvalidValue");
                return 1;

            avow to "validate value"
              steps
                let result = check invalid value

            endpoint Validate : POST /validate
              input
                value: Text
              steps
                validate value
              spec "invalid values are rejected"
                given input { value: "invalid" }
                when handled
                then fails with InvalidValue
            """;
        var sourceLines = sourceText.Split('\n');
        var declarationLine = LineOf(sourceLines, "  fails with InvalidValue");
        using var workspace = TempWorkspace.Create();
        var source = Path.Combine(workspace.Root, "CompositeFailures.avowry");
        await File.WriteAllTextAsync(source, sourceText);

        // When Morgan builds the source through the CLI and reads its external map
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace.Root, "CompositeFailures.avowrymap.json")));

        // Then the endpoint failure record points to the leaf phrase's declaration
        var failure = document.RootElement.GetProperty("records").EnumerateArray()
            .Single(record => Text(record, "kind") == "failure" && Text(record, "name") == "InvalidValue" && Text(record, "endpoint") == "Validate");
        Assert.Equal(declarationLine, failure.GetProperty("line").GetInt32());
    }

    [Fact]
    [Trait("Slice", "170")]
    public async Task A_failure_record_is_found_when_an_earlier_step_can_raise_the_same_name()
    {
        // Given Morgan has an endpoint ensure before a phrase that declares the same failure
        const string sourceText = """
            module RepeatedFailures

            avow to "check invalid value" -> Int
              fails with InvalidValue
              native csharp
                Fail("InvalidValue");
                return 1;

            endpoint Validate : POST /validate
              input
                value: Text
              steps
                ensure input.value is not blank else InvalidValue
                let result = check invalid value
              spec "invalid values are rejected"
                given input { value: "invalid" }
                when handled
                then fails with InvalidValue
            """;
        var sourceLines = sourceText.Split('\n');
        var declarationLine = LineOf(sourceLines, "  fails with InvalidValue");
        using var workspace = TempWorkspace.Create();
        var source = Path.Combine(workspace.Root, "RepeatedFailures.avowry");
        await File.WriteAllTextAsync(source, sourceText);

        // When Morgan builds the source through the CLI and reads its external map
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace.Root, "RepeatedFailures.avowrymap.json")));

        // Then the failure record reaches the declaring phrase after the earlier matching step
        var failure = document.RootElement.GetProperty("records").EnumerateArray()
            .Single(record => Text(record, "kind") == "failure" && Text(record, "name") == "InvalidValue" && Text(record, "endpoint") == "Validate");
        Assert.Equal(declarationLine, failure.GetProperty("line").GetInt32());
    }

    [Fact]
    [Trait("Slice", "170")]
    public async Task Step_ids_equal_an_independent_recompute_from_the_source()
    {
        // Given Morgan has the Tasks source and its CLI-built external map
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var sourceLines = await File.ReadAllLinesAsync(source);
        using var workspace = TempWorkspace.Create();
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);

        // When every id is recomputed from the documented UTF-8 hash input and its Phrase source line
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace.Root, "Tasks.avowrymap.json")));
        var recomputed = document.RootElement.GetProperty("records").EnumerateArray()
            .Select(record => (Actual: Text(record, "stepId"), Expected: RecomputeStepId(record, sourceLines))).ToArray();

        // Then every independently recomputed id equals the id written by the CLI
        Assert.All(recomputed, item => Assert.Equal(item.Expected, item.Actual));
    }

    [Fact]
    [Trait("Slice", "170")]
    public async Task A_failure_used_by_two_endpoints_has_one_id_per_endpoint()
    {
        // Given Morgan's Tasks source uses its declared NotFound failure from three endpoints
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        using var workspace = TempWorkspace.Create();

        // When the CLI writes the source map
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace.Root, "Tasks.avowrymap.json")));
        var records = document.RootElement.GetProperty("records").EnumerateArray().ToArray();
        var notFound = records.Where(record => Text(record, "kind") == "failure" && Text(record, "name") == "NotFound").ToArray();

        // Then every map record is unambiguous and each NotFound endpoint use has its own id
        Assert.Equal(records.Length, records.Select(record => Text(record, "stepId")).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(new[] { "CompleteTask", "DeleteTask", "GetTask" }, notFound.Select(record => Text(record, "endpoint")).Order(StringComparer.Ordinal));
        Assert.Equal(3, notFound.Select(record => Text(record, "stepId")).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Tasks_build_writes_an_external_map_and_trace_resolves_a_declared_failure()
    {
        // Given Morgan's Tasks source and an empty build directory
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        using var workspace = TempWorkspace.Create();

        // When the source is built and Morgan traces EmptyTitle
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        var map = Path.Combine(workspace.Root, "Tasks.avowrymap.json");
        var trace = await PhraseCli.RunAsync("trace", "EmptyTitle", "--map", map);

        // Then the external map is complete, deterministic, and the trace reaches the owning spec
        Assert.True(build.ExitCode == 0, build.Text);
        Assert.True(File.Exists(map), $"Missing {map}");
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(map));
        var sourceLines = await File.ReadAllLinesAsync(source);
        var failureLine = Array.FindIndex(sourceLines, line => line.TrimStart().StartsWith("fails with EmptyTitle", StringComparison.Ordinal)) + 1;
        var specLine = Array.FindIndex(sourceLines, line => line.Contains("spec \"an empty title is rejected\"", StringComparison.Ordinal)) + 1;
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.NotEmpty(document.RootElement.GetProperty("buildId").GetString()!);
        var failure = document.RootElement.GetProperty("records").EnumerateArray()
            .Single(record => record.GetProperty("kind").GetString() == "failure" && record.GetProperty("name").GetString() == "EmptyTitle");
        Assert.Equal("Tasks.avowry", failure.GetProperty("phraseFile").GetString());
        Assert.Equal(failureLine, failure.GetProperty("line").GetInt32());
        Assert.All(document.RootElement.GetProperty("records").EnumerateArray()
            .Where(record => record.GetProperty("kind").GetString() is "endpoint" or "step" or "failure"),
            record => Assert.Matches("^[0-9a-f]{12}$", record.GetProperty("stepId").GetString()!));
        Assert.Equal("CreateTask", failure.GetProperty("endpoint").GetString());
        Assert.True(trace.ExitCode == 0, trace.Text);
        Assert.Contains("EmptyTitle  failure  422", trace.Stdout);
        Assert.Contains($"Tasks.avowry:{failureLine} endpoint CreateTask", trace.Stdout);
        Assert.Contains($"spec: Tasks.avowry:{specLine}-{specLine + 3}", trace.Stdout);
        Assert.Contains("stepId", trace.Stdout);

        // And this card adds no source file name with a line number, no trace header and no map reference
        // to the generated C# (the older [Phrase] attributes and step comments are a separate decision)
        var generated = string.Join("\n", Directory.GetFiles(workspace.Root, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
        AssertNoPhraseKnowledge(generated);
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Lf_and_crlf_sources_produce_the_same_map_lines_and_ids()
    {
        // Given Morgan has equivalent LF and CRLF copies of the Tasks source
        var text = await File.ReadAllTextAsync(RepoPaths.Find("demo", "tasks", "Tasks.avowry"));
        using var first = TempWorkspace.Create();
        using var second = TempWorkspace.Create();
        var lf = Path.Combine(first.Root, "Tasks.avowry");
        var crlf = Path.Combine(second.Root, "Tasks.avowry");
        await File.WriteAllTextAsync(lf, text.Replace("\r\n", "\n", StringComparison.Ordinal));
        await File.WriteAllTextAsync(crlf, text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\r\n", StringComparison.Ordinal));

        // When both copies are built
        var one = await PhraseCli.RunAsync("build", lf, "-o", first.Root);
        var two = await PhraseCli.RunAsync("build", crlf, "-o", second.Root);

        // Then normalization makes the complete maps byte-identical
        Assert.True(one.ExitCode == 0, one.Text);
        Assert.True(two.ExitCode == 0, two.Text);
        Assert.Equal(await File.ReadAllBytesAsync(Path.Combine(first.Root, "Tasks.avowrymap.json")),
            await File.ReadAllBytesAsync(Path.Combine(second.Root, "Tasks.avowrymap.json")));
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Repeated_builds_from_different_paths_are_byte_identical()
    {
        // Given the same LF-normalised Tasks source through absolute and backslash paths
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        using var first = TempWorkspace.Create();
        using var second = TempWorkspace.Create();

        // When Morgan builds both copies
        var one = await PhraseCli.RunAsync("build", source, "-o", first.Root);
        // The second path is a copy in another directory; on Windows it is written with backslashes
        using var copy = TempWorkspace.Create();
        var copied = Path.Combine(copy.Root, "Tasks.avowry");
        File.Copy(source, copied);
        var secondPath = OperatingSystem.IsWindows() ? copied.Replace('/', '\\') : copied;
        var two = await PhraseCli.RunAsync("build", secondPath, "-o", second.Root);

        // Then the external map and every generated C# file are byte-identical
        Assert.True(one.ExitCode == 0, one.Text);
        Assert.True(two.ExitCode == 0, two.Text);
        Assert.Equal(await File.ReadAllBytesAsync(Path.Combine(first.Root, "Tasks.avowrymap.json")), await File.ReadAllBytesAsync(Path.Combine(second.Root, "Tasks.avowrymap.json")));
        foreach (var path in Directory.GetFiles(first.Root, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(first.Root, path);
            Assert.Equal(await File.ReadAllBytesAsync(path), await File.ReadAllBytesAsync(Path.Combine(second.Root, relative)));
        }
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Unknown_trace_name_exits_nonzero_with_a_clear_message()
    {
        // Given Morgan has built the Tasks map
        using var workspace = TempWorkspace.Create();
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);

        // When a name absent from that map is traced
        var trace = await PhraseCli.RunAsync("trace", "MissingFailure", "--map", Path.Combine(workspace.Root, "Tasks.avowrymap.json"));

        // Then the command refuses the unknown name clearly
        Assert.True(build.ExitCode == 0, build.Text);
        Assert.NotEqual(0, trace.ExitCode);
        Assert.Contains("Unknown trace name or frame: MissingFailure", trace.Stderr);
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Live_failures_and_endpoints_resolve_in_the_source_map()
    {
        // Given Morgan's Tasks source is built by the CLI into an empty temporary workspace
        var sourcePath = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var sourceLines = await File.ReadAllLinesAsync(sourcePath);
        var resolved = Resolver.Resolve(string.Join('\n', sourceLines));
        Assert.Empty(resolved.Diagnostics);
        using var workspace = TempWorkspace.Create();
        var build = await PhraseCli.RunAsync("build", sourcePath, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);

        // When every declared failure is observed through the running API over real HTTP
        await TasksOverHttp.AssertEverySpecMatchesAsync(workspace.FindSingle("*.Api.csproj"), resolved.Module);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace.Root, "Tasks.avowrymap.json")));
        var records = document.RootElement.GetProperty("records").EnumerateArray().ToArray();

        // Then every endpoint and all four failures resolve to their exact declarations and owning endpoints
        var endpointRecords = records.Where(record => Text(record, "kind") == "endpoint").ToArray();
        Assert.Equal(resolved.Module.Endpoints.Select(endpoint => endpoint.Name).Order(), endpointRecords.Select(record => Text(record, "name")).Order());
        foreach (var endpoint in resolved.Module.Endpoints)
        {
            var record = endpointRecords.Single(candidate => Text(candidate, "name") == endpoint.Name);
            Assert.Equal(LineOf(sourceLines, $"endpoint {endpoint.Name} :"), record.GetProperty("line").GetInt32());
            Assert.Equal(endpoint.Name, Text(record, "endpoint"));
        }

        var expectedFailures = new[] { "EmptyTitle", "TitleTooLong", "NotFound", "StoreUnavailable" };
        var failureRecords = records.Where(record => Text(record, "kind") == "failure").ToArray();
        Assert.Equal(expectedFailures.Order(), failureRecords.Select(record => Text(record, "name")).Distinct().Order());

        // The set of (failure, endpoint) pairs in the map equals the pairs the endpoint specs assert, no more and no fewer
        var expectedPairs = resolved.Module.Endpoints
            .SelectMany(endpoint => endpoint.Specs.Where(spec => spec.Outcome is FailsModel).Select(spec => ((FailsModel)spec.Outcome).Code + "@" + endpoint.Name))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var actualPairs = failureRecords.Select(record => Text(record, "name") + "@" + Text(record, "endpoint")).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expectedPairs, actualPairs);

        foreach (var record in failureRecords)
        {
            var failure = Text(record, "name");
            var endpoint = Text(record, "endpoint");

            // The record points at a line that declares the failure, and its spec range contains the spec asserting it
            Assert.Contains(record.GetProperty("line").GetInt32(), LinesContainingFailure(sourceLines, failure));
            var specRange = record.GetProperty("specLines");
            var first = specRange.GetProperty("first").GetInt32();
            var last = specRange.GetProperty("last").GetInt32();
            var endpointLine = LineOf(sourceLines, $"endpoint {endpoint} :");
            var assertingLines = Enumerable.Range(first, last - first + 1)
                .Where(number => sourceLines[number - 1].Trim() == $"then fails with {failure}").ToArray();
            Assert.NotEmpty(assertingLines);
            Assert.All(assertingLines, number => Assert.True(number > endpointLine, $"{failure} spec at line {number} is not inside endpoint {endpoint}"));
            Assert.True(first > endpointLine, $"{failure} spec range does not start inside endpoint {endpoint}");
        }
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task No_phrase_knowledge_assertions_fail_on_a_reintroduced_leak()
    {
        // Given each forbidden form of Phrase source knowledge is reintroduced independently
        var leaks = new[]
        {
            "Tasks.avowry:12", "// phrase: Tasks.avowry", "X-Phrase-Source", "Tasks.avowrymap.json",
            "PhraseRuntime", "PhraseFailure", "PhraseBuildId", "[Phrase(\"an avow\")]", "Phrases.Run()",
        };

        // When the shared generated-output assertions inspect each mutation
        foreach (var leak in leaks)
            Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => AssertNoPhraseKnowledge(leak));

        // Then those same assertions accept the real CLI-generated C# output
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        using var workspace = TempWorkspace.Create();
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        var generated = string.Join("\n", Directory.GetFiles(workspace.Root, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
        AssertNoPhraseKnowledge(generated);
    }

    private static void AssertNoPhraseKnowledge(string generated)
    {
        Assert.DoesNotMatch(@"\.avowry:\d", generated);
        Assert.DoesNotContain("// phrase:", generated, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("X-Phrase-Source", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("avowrymap", generated, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("phrasemap", generated, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PhraseRuntime", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("PhraseFailure", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("PhraseBuildId", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("[Phrase(", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("Phrases.", generated, StringComparison.Ordinal);
    }

    private static int LineOf(IReadOnlyList<string> lines, string declaration) =>
        Array.FindIndex(lines.ToArray(), line => line.StartsWith(declaration, StringComparison.Ordinal)) + 1;

    private static int[] LinesContainingFailure(IReadOnlyList<string> lines, string failure) =>
        lines.Select((line, index) => (line, number: index + 1))
            .Where(item => item.line.TrimStart().StartsWith("fails with ", StringComparison.Ordinal)
                && item.line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(failure, StringComparer.Ordinal))
            .Select(item => item.number)
            .ToArray();

    private static string RecomputeStepId(JsonElement record, IReadOnlyList<string> sourceLines)
    {
        var kind = Text(record, "kind");
        var name = Text(record, "name").Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var line = record.GetProperty("line").GetInt32();
        var sourceLine = sourceLines[line - 1].Trim();
        Assert.True(SourceLineNamesRecord(sourceLine, kind, name), $"{kind} {name} is not declared by Phrase line {line}: {sourceLine}");
        var input = $"{kind}\n{line}\n{name}" + (kind == "failure" ? $"\n{Text(record, "endpoint")}" : "");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)))[..12].ToLowerInvariant();
    }

    private static bool SourceLineNamesRecord(string line, string kind, string name) => kind switch
    {
        "endpoint" => line.StartsWith($"endpoint {name} :", StringComparison.Ordinal),
        "step" => line == name,
        "phrase" => line.StartsWith($"avow to \"{name}\"", StringComparison.Ordinal),
        "failure" => line.StartsWith("fails with ", StringComparison.Ordinal)
            && line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(name, StringComparer.Ordinal),
        _ => false,
    };

    [Fact]
    [Trait("Slice", "170")]
    public async Task Failure_records_point_at_the_exact_declaration_line_of_the_phrase_the_endpoint_calls()
    {
        // Given Morgan's Tasks source resolved by the compiler and built by the CLI
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var sourceLines = await File.ReadAllLinesAsync(source);
        var resolved = Resolver.Resolve(string.Join('\n', sourceLines));
        Assert.Empty(resolved.Diagnostics);
        using var workspace = TempWorkspace.Create();
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace.Root, "Tasks.avowrymap.json")));
        var failureRecords = document.RootElement.GetProperty("records").EnumerateArray()
            .Where(record => Text(record, "kind") == "failure").ToArray();

        // When the expected line is taken from the phrase that the record's endpoint actually calls
        foreach (var record in failureRecords)
        {
            var endpoint = resolved.Module.Endpoints.Single(candidate => candidate.Name == Text(record, "endpoint"));
            var failure = Text(record, "name");
            var expectedLines = endpoint.Steps
                .Select(step => step switch
                {
                    BoundCallStep call => resolved.Module.Phrase(call.Call.PhraseKey),
                    BoundLetStep let => resolved.Module.Phrase(let.Call.PhraseKey),
                    _ => null,
                })
                .Where(phrase => phrase is not null)
                .SelectMany(phrase => phrase!.DeclaredFailures.Where(declared => declared.Name == failure).Select(declared => declared.Line))
                .Distinct()
                .ToArray();

            // Then the endpoint has exactly one declaration line for the failure and the record carries that exact line
            Assert.True(expectedLines.Length == 1, $"{endpoint.Name} {failure}: expected one declaring line, found {expectedLines.Length}");
            Assert.Equal(expectedLines[0], record.GetProperty("line").GetInt32());
            Assert.Contains(failure, sourceLines[expectedLines[0] - 1], StringComparison.Ordinal);
        }
    }

    private static string Text(JsonElement record, string property) => record.GetProperty(property).GetString()!;
}
