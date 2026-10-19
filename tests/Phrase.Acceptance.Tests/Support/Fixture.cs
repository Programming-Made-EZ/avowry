namespace Phrase.Acceptance.Tests.Support;

public static class Fixture
{
    public static string Read(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    /// <summary>
    /// Independent oracle for the number of specs in a source file. Deliberately
    /// does not use the compiler, so a compiler that drops specs cannot pass.
    /// </summary>
    public static int CountSpecs(string source) =>
        source.Split('\n').Count(line => line.TrimStart().StartsWith("spec \"", StringComparison.Ordinal));
}
