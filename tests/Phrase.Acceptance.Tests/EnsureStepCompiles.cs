using System.Diagnostics;
using System.Text;
using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: Declared validation rules compile without native code
//   As Ari, I want title rules and their named failures to be visible in Phrase,
//   so that I can understand and change validation without reading C#.
public class EnsureStepCompiles
{
    [Fact]
    [Trait("Slice", "159")]
    public async Task Whitespace_record_fields_and_called_phrase_failures_reach_the_generated_API()
    {
        // Given a native-free record-field guard called by an endpoint with a whitespace-only title spec
        const string source = """
            module Tasks
            uses web

            type Task
              title: Text

            phrase "validate {task: Task}"
              fails with EmptyTitle
              steps
                ensure task.title is not blank else EmptyTitle

            endpoint CreateTask : POST /tasks
              input
                title: Text
              steps
                validate { title: input.title }
              spec "whitespace title is rejected"
                given input { title: "   " }
                when handled
                then fails with EmptyTitle
            """;

        // When the C# adapter compiles the module and runs its generated API spec
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var endpoint = File.ReadAllText(workspace.FindSingle("CreateTask.cs.cs"));
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then the inherited failure is exercised as HTTP 422 and the whitespace title is rejected
        Assert.Contains("catch (PhraseFailure failure)", endpoint, StringComparison.Ordinal);
        Assert.Contains("await Send.ErrorsAsync(422, ct);", endpoint, StringComparison.Ordinal);
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "159")]
    public async Task Int_comparisons_with_literals_and_inputs_compile_and_execute()
    {
        // Given a native-free phrase comparing an Int with both a literal and another input
        const string source = """
            module Counts

            phrase "validate count {count: Int} minimum {minimum: Int}"
              fails with Zero, TooSmall
              steps
                ensure count != 0 else Zero
                ensure count >= minimum else TooSmall
              spec "a count above the minimum is accepted"
                given count = 2
                and minimum = 1
                when called
                then succeeds
              spec "zero is rejected"
                given count = 0
                and minimum = 0
                when called
                then fails with Zero
              spec "a count below the minimum is rejected"
                given count = 1
                and minimum = 2
                when called
                then fails with TooSmall
            """;

        // When the C# adapter compiles the phrase and runs its generated specs
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then literal and input comparisons enforce their declared failures
        Assert.True(result.Built, result.Log);
        Assert.Equal(3, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "159")]
    public async Task Text_rules_compile_to_guard_clauses_and_pass_their_boundary_specs()
    {
        // Given a native-free phrase with declared blank and maximum-length rules
        var boundary = new string('x', 200);
        var overBoundary = boundary + "x";
        var source = $$"""
            module Titles

            phrase "validate title {title: Text}"
              fails with EmptyTitle, TitleTooLong
              steps
                ensure title is not blank else EmptyTitle
                ensure title length at most 200 else TitleTooLong
              spec "a title is accepted"
                given title = "Write the demo"
                when called
                then succeeds
              spec "a blank title is rejected"
                given title = ""
                when called
                then fails with EmptyTitle
              spec "length 200 is accepted"
                given title = "{{boundary}}"
                when called
                then succeeds
              spec "length 201 is rejected"
                given title = "{{overBoundary}}"
                when called
                then fails with TitleTooLong
            """;

        // When the C# adapter compiles the phrase and its generated specs
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var generated = File.ReadAllText(workspace.FindSingle("Phrases.cs"));
        var build = await BuildAsync(workspace.FindSingle("*.Api.csproj"));
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then the native-free phrase builds warning-free and every boundary spec passes
        Assert.True(result.Built, result.Log);
        Assert.True(build.ExitCode == 0, build.Log);
        Assert.Contains("0 Warning(s)", build.Log, StringComparison.Ordinal);
        Assert.Contains("if (string.IsNullOrWhiteSpace(title)) PhraseFailure.Raise(\"EmptyTitle\");", generated, StringComparison.Ordinal);
        Assert.Contains("if (title.Length > 200) PhraseFailure.Raise(\"TitleTooLong\");", generated, StringComparison.Ordinal);
        Assert.Equal(4, result.Total);
        Assert.Equal(0, result.Failed);
    }

    private static async Task<(int ExitCode, string Log)> BuildAsync(string project)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(project)!,
        };
        foreach (var argument in new[] { "build", project, "--nologo", "--disable-build-servers" })
            start.ArgumentList.Add(argument);
        start.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            start.Environment.Remove(name);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet build.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, new StringBuilder().Append(await stdout).Append(await stderr).ToString());
    }
}
