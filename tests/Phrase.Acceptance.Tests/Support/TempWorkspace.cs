namespace Phrase.Acceptance.Tests.Support;

public sealed class TempWorkspace : IDisposable
{
    public string Root { get; }

    private TempWorkspace(string root) => Root = root;

    public static TempWorkspace Create()
    {
        var root = Path.Combine(Path.GetTempPath(), "phrase-acceptance", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return new TempWorkspace(root);
    }

    public void CopyFrom(string sourceDir)
    {
        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, file);
            if (IsBuildArtifact(relative)) continue;

            var target = Path.Combine(Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }

    public string FindSingle(string pattern)
    {
        var matches = Directory.GetFiles(Root, pattern, SearchOption.AllDirectories);
        Assert.True(matches.Length == 1,
            $"Expected exactly one {pattern} in output, found {matches.Length}: {string.Join(", ", matches)}");
        return matches[0];
    }

    public void Dispose()
    {
        try { Directory.Delete(Root, recursive: true); }
        catch (IOException) { /* best effort; temp dir */ }
    }

    private static bool IsBuildArtifact(string relativePath) =>
        relativePath.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj");
}
