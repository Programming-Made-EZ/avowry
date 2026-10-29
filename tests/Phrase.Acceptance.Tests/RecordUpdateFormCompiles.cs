using System.Diagnostics;
using System.Text;
using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;

namespace Phrase.Acceptance.Tests;

// Feature: A record update form compiles without native code
//   As Ari, I want to write a record with one field changed (for example a task
//   with done set to true) and keep the rest, so that completing an item is
//   expressed in Phrase and not in a native constructor.
public class RecordUpdateFormCompiles
{
    [Fact]
    [Trait("Slice", "160")]
    public async Task Phrase_and_endpoint_update_forms_compile_without_native_csharp()
    {
        // Given a native-free phrase and endpoint that copy a record with one field changed
        const string source = """
            module Tasks

            type Task
              id: Id
              title: Text
              done: Bool

            avow to "complete {task: Task}"
              fails with EmptyTitle
              steps
                let completed = task with { done: true }
                ensure completed.title is not blank else EmptyTitle
              spec "a titled task is accepted"
                given task = { id: "task-1", title: "Write the demo", done: false }
                when called
                then succeeds
              spec "a blank title is rejected"
                given task = { id: "task-1", title: "", done: false }
                when called
                then fails with EmptyTitle

            endpoint ShowCopy : POST /copy
              input
                id: Id
                title: Text
                done: Bool
              output
                originalDone: Bool
                done: Bool
                title: Text
                id: Id
              steps
                let completed = input with { done: true }
                respond with { originalDone: input.done, done: completed.done, title: completed.title, id: completed.id }
              spec "the copy is done, the original is unchanged, and other fields are kept"
                given input { id: "task-1", title: "Write the demo", done: false }
                when handled
                then responds { originalDone: false, done: true, title: "Write the demo", id: "task-1" }
            """;

        // When the C# adapter compiles the module and runs its generated specs
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var phrases = File.ReadAllText(workspace.FindSingle("Phrases.cs"));
        var endpoint = File.ReadAllText(workspace.FindSingle("ShowCopy.cs.cs"));
        var build = await BuildAsync(workspace.FindSingle("*.Api.csproj"));
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then the emitted C# uses a with-expression, the original is unchanged,
        // the copy has the new field, other fields are kept, and the specs pass
        Assert.DoesNotContain("native csharp", source, StringComparison.Ordinal);
        Assert.Contains("task with { Done = true }", phrases, StringComparison.Ordinal);
        Assert.Contains("input with { Done = true }", endpoint, StringComparison.Ordinal);
        Assert.True(build.ExitCode == 0, build.Log);
        Assert.Contains("0 Warning(s)", build.Log, StringComparison.Ordinal);
        Assert.True(result.Built, result.Log);
        Assert.Equal(3, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "160")]
    public void Tasks_demo_complete_uses_the_update_form()
    {
        // Given the Tasks demo source
        var source = File.ReadAllText(RepoPaths.Find("demo", "tasks", "Tasks.avowry"));

        // When CompleteTask is resolved and emitted
        var resolved = Resolver.Resolve(source);
        Assert.Empty(resolved.Diagnostics);
        var complete = Assert.Single(resolved.Module.Endpoints, endpoint => endpoint.Name == "CompleteTask");
        var output = new CSharpProvider().Emit(resolved.Module);
        var endpoint = Assert.Single(output.Files, file => file.Key.EndsWith("CompleteTask.cs.cs", StringComparison.Ordinal));

        // Then its path is find, update, save and respond, with no native complete-task phrase left,
        // and the generated endpoint uses an idiomatic C# with-expression
        Assert.Collection(complete.Steps,
            step => Assert.IsType<BoundLetStep>(step),
            step => Assert.IsType<BoundLetUpdateStep>(step),
            step => Assert.IsType<BoundCallStep>(step),
            step => Assert.IsType<BoundRespondStep>(step));
        // The old native "complete task" phrase is gone: restoring it (the revert of this card) fails here.
        Assert.DoesNotContain(resolved.Module.Phrases, phrase => phrase.Key == "complete task _");
        Assert.Contains("task with { Done = true }", endpoint.Value, StringComparison.Ordinal);
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
