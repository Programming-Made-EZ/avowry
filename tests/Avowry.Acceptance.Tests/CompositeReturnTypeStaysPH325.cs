using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

public class CompositeReturnTypeStaysPH325
{
    [Fact]
    [Trait("Slice", "189")]
    public async Task A_composite_with_a_return_type_already_reports_PH325()
    {
        // Given a composite that claims to return a value
        using var workspace = TempWorkspace.Create();
        var source = Path.Combine(workspace.Root, "composite-return.avowry");
        await File.WriteAllTextAsync(source, """
            module Test

            avow to "count {n: Int}" -> Int
              steps
                ensure n is positive

            avow to "ensure {n: Int} is positive"
              fails with NotPositive
              native csharp
                return;
            """);
        var output = Path.Combine(workspace.Root, "out");

        // When avowry build compiles it
        var result = await PhraseCli.RunAsync("build", source, "-o", output);

        // Then it exits 1 with PH325 and no unhandled exception
        Assert.True(result.ExitCode == 1, result.Text);
        Assert.Contains("PH325 (", result.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Unhandled exception", result.Text, StringComparison.Ordinal);
    }
}
