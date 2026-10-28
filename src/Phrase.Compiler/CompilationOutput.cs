namespace Phrase.Compiler;

/// <summary>The files an adapter emits, keyed by path relative to the output root.</summary>
public sealed record CompilationOutput(IReadOnlyDictionary<string, string> Files)
{
    public Semantics.ModuleModel? Module { get; init; }
    public void WriteTo(string outputRoot)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(outputRoot)) + Path.DirectorySeparatorChar;

        foreach (var (relativePath, contents) in Files)
        {
            var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
            if (!fullPath.StartsWith(root, StringComparison.Ordinal))
                throw new InvalidOperationException($"Adapter emitted a path outside the output root: {relativePath}");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, contents);
        }
    }
}
