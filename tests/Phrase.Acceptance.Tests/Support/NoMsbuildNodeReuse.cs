using System.Runtime.CompilerServices;

namespace Phrase.Acceptance.Tests.Support;

/// <summary>
/// Every test that starts a nested dotnet build inherits this setting. A reused MSBuild node outlives
/// its build and keeps the redirected output pipe open, which hung the whole gate (docs/retros/seeds.md).
/// </summary>
internal static class NoMsbuildNodeReuse
{
    [ModuleInitializer]
    internal static void Apply() => Environment.SetEnvironmentVariable("MSBUILDDISABLENODEREUSE", "1");
}
