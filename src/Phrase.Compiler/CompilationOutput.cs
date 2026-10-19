namespace Phrase.Compiler;

/// <summary>The files a provider emits, keyed by path relative to the output root.</summary>
public sealed record CompilationOutput(IReadOnlyDictionary<string, string> Files)
{
    public void WriteTo(string outputRoot)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(outputRoot)) + Path.DirectorySeparatorChar;

        foreach (var (relativePath, contents) in Files)
        {
            var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
            if (!fullPath.StartsWith(root, StringComparison.Ordinal))
                throw new InvalidOperationException($"Provider emitted a path outside the output root: {relativePath}");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, contents);
        }
    }
}
