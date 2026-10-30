using System.Diagnostics;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Card 214 commit 1: the test seams of the generated API harness.
// These are characterisation (born-green) guards, each with a named mutation in the commit body.
public class GeneratedApiSeams
{
    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public async Task Guard_hook_is_null_by_default_and_fires_once()
    {
        // Given the options type, its seams default to today's behaviour
        var defaults = new GeneratedApiStartOptions();
        Assert.Null(defaults.BeforeChildStart);
        Assert.Null(defaults.StartupTimeout);
        Assert.Null(defaults.PrebuiltDll);

        // When a start is made with a hook that counts and throws, and a timeout of 1 second so a missing hook fails fast
        var calls = 0;
        ProcessStartInfo? seen = null;
        var options = new GeneratedApiStartOptions
        {
            PrebuiltDll = typeof(GeneratedApiSeams).Assembly.Location, // any existing file: the hook stops before a child starts
            StartupTimeout = TimeSpan.FromSeconds(1),
            BeforeChildStart = psi =>
            {
                calls++;
                seen = psi;
                throw new InvalidOperationException("hook stop");
            },
        };
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => GeneratedApi.StartAsync("unused.csproj", options));

        // Then the hook fired exactly once, with the start info prepared, and its own exception propagated
        Assert.Equal("hook stop", thrown.Message);
        Assert.Equal(1, calls);
        Assert.NotNull(seen);
        Assert.StartsWith("http://127.0.0.1:", seen!.Environment["ASPNETCORE_URLS"]);
    }

    [Fact]
    [Trait("Slice", "214")]
    [Trait("Kind", "BornGreen")]
    public void Guard_quietchild_dll_exists_next_to_tests()
    {
        var dll = QuietChildPath();
        Assert.True(File.Exists(dll), $"QuietChild.dll was not found at {dll}.");
        Assert.True(File.Exists(Path.ChangeExtension(dll, ".runtimeconfig.json")), "QuietChild.runtimeconfig.json is missing.");
    }

    internal static string QuietChildPath() => Path.Combine(AppContext.BaseDirectory, "QuietChild.dll");
}
