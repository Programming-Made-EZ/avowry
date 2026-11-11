using System.Text.RegularExpressions;

// Shared by Avowry.Bench and, as a linked file, Avowry.BenchCs. Platform independent on purpose:
// Path.GetFileName would not split a Windows path on Linux.
// Known limits (card 418 second review): prose between two rooted paths on one line, with no quote, colon, comma, semicolon
// or bracket between them, collapses to the last short name (so `copy /tmp/a/x.cs to /tmp/b/y.cs` becomes `copy y.cs`: it over-scrubs
// and never leaks); an account folder alone swallows the plain words that follow it (`C:\Users\<user>` becomes `<path>`);
// `\\?\C:\...` becomes `<path>?\short-name`; /mnt/c/Users, Git Bash /c/Users, /var/tmp, /private/tmp and `Documents and
// Settings` roots are not matched; a Linux TMPDIR outside the listed roots is left as is.
public static class PathScrub
{
    private const string Separator = @"[\\/]+";
    private const string Segment = @"(?:<[A-Za-z]+>|(?<=[A-Za-z])'(?=[A-Za-z])|[^\\/\r\n""'`|*?:;,<>\[\]])+";
    private const string FinalSegment = @"(?:<[A-Za-z]+>|(?<=[A-Za-z])'(?=[A-Za-z])|[^\s\\/""'`|*?:;,()<>\[\]])*";

    private static readonly Regex RootedPath = new(
        @"(?:" +
        @"(?<![A-Za-z0-9_])(?:file:///)?[A-Za-z]:" + Separator + @"(?:(?<home>(?i:users))" + Separator + @")?" +
        @"|(?<![\w\\:])\\\\(?=[^\\/\s])" +
        @"|(?<![\w.])~" + Separator +
        @"|(?<![A-Za-z0-9_.])(?:file://)?(?:(?<home>/home/|/Users/)|/root/|/tmp/|/var/folders/|/private/var/folders/)" +
        @")" +
        @"(?:(?<dir>" + Segment + @")" + Separator + @")*" +
        @"(?<file>" + FinalSegment + @")" +
        @"(?(dir)(?:)|(?(home)(?<tail>(?:[ ]+(?:[A-Za-z0-9_-]|(?<=[A-Za-z])'(?=[A-Za-z]))+)*)))",
        RegexOptions.CultureInvariant);

    public static string Apply(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;
        return RootedPath.Replace(text, match =>
        {
            var file = match.Groups["file"].Value;
            // An account folder alone (C:\Users\<user>, /home/<user>) must not survive as its own short name.
            if (file.Length == 0 || (match.Groups["home"].Success && match.Groups["dir"].Captures.Count == 0))
                return "<path>";
            return file;
        });
    }
}
