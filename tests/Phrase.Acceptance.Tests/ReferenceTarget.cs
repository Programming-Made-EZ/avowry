using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Reference C# target
//   reference/csharp is the hand-written output the C# provider should emit for orders.phrase.
//   Keeping it green proves two things before any compiler exists:
//   the target design is valid, and the acceptance harness can tell pass from fail.
public class ReferenceTarget
{
    [Fact]
    public async Task Reference_output_builds_and_every_spec_passes()
    {
        // Given the hand-written reference output for orders.phrase, copied to a fresh workspace
        var expectedSpecCount = Fixture.CountSpecs(Fixture.Read("orders.phrase"));
        using var workspace = TempWorkspace.Create();
        workspace.CopyFrom(RepoPaths.Find("reference", "csharp"));

        // When its specs project is tested
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then it builds
        Assert.True(result.Built, result.Log);

        // And one test runs per spec in orders.phrase
        Assert.Equal(expectedSpecCount, result.Total);

        // And every test passes
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    public async Task Reference_runner_reports_a_mutated_expectation_as_failed()
    {
        // Given a fresh copy of the reference with one expected value made wrong
        using var workspace = TempWorkspace.Create();
        workspace.CopyFrom(RepoPaths.Find("reference", "csharp"));
        var specs = workspace.FindSingle("PhraseSpecs.cs");
        var source = await File.ReadAllTextAsync(specs);
        Assert.Contains("Assert.Equal(5, result);", source);
        await File.WriteAllTextAsync(specs, source.Replace("Assert.Equal(5, result);", "Assert.Equal(6, result);"));

        // When the same runner tests the mutated specs project
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then it still builds, but proves that the runner reports a failing spec
        Assert.True(result.Built, result.Log);
        Assert.True(result.Failed > 0, result.Log);
    }
}
