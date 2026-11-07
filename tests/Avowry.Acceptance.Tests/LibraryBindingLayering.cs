using System.Diagnostics;
using System.IO;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: Library bindings and a project profile sit under each adapter
//   As Dana, I want a written layering of extension, adapter, library binding
//   and project profile, so that the same spec can come out as FastEndpoints
//   today and as Minimal APIs or controllers later without changing a spec.
public class LibraryBindingLayering
{
    [Fact]
    [Trait("Slice", "152")]
    public async Task The_library_binding_layering_check_passes()
    {
        // Given the library-binding ADR and the checker that reads it
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        start.ArgumentList.Add("tests.test_library_binding_layering");

        // When the library-binding tests run
        var result = await ProcessRunner.RunAsync(start, TimeSpan.FromMilliseconds(120000));

        // Then the ADR defines the layers, the FastEndpoints audit and the second-binding rule
        Assert.True(result.ExitCode == 0, result.Stdout + result.Stderr);
    }
}
