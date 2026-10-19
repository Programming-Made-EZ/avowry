namespace Phrase.Acceptance.Tests.Support;

public static class RepoPaths
{
    public static string Root { get; } = FindRoot();

    public static string Find(params string[] parts) => Path.Combine([Root, .. parts]);

    private static string FindRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Phrase.sln")))
                return dir.FullName;
        }
        throw new DirectoryNotFoundException("Could not find Phrase.sln above the test output directory.");
    }
}
