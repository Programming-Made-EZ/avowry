using System.Text.RegularExpressions;

// Shared by Avowry.Bench and, as a linked file, Avowry.BenchCs. Platform independent on purpose:
// Path.GetFileName would not split a Windows path on Linux.
// Known limits (card 418 second review, card 426): prose followed by a relative path on one line collapses into that path
// (`copy /tmp/x.cs to src/y.cs` becomes `copy y.cs`: it over-scrubs and never leaks); an account folder alone swallows the
// plain words that follow it (`C:\Users\<user> is denied` becomes `<path>`); `\\?\C:\...` becomes `<path>?\short-name`;
// a path that starts after a JSON `\uXXXX` escape ends before a folder named `u` plus four hex digits; a Linux TMPDIR
// outside the listed roots is left as is.
public static class PathScrub
{
    private const string Separator = @"[\\/]+";
    private const string Segment = @"(?:<[A-Za-z]+>|(?<=[A-Za-z])'(?=[A-Za-z])|[^\\/\r\n""'`|*?:;,<>\[\]])+";
    private const string FinalSegment = @"(?:<[A-Za-z]+>|(?<=[A-Za-z])'(?=[A-Za-z])|[^\s\\/""'`|*?:;,()<>\[\]])*";

    private static readonly Regex RootedPath = new(
        @"(?:" +
        @"(?:(?<![A-Za-z0-9_])|(?<=(?<esc>\\u[0-9A-Fa-f]{4})))(?:file:///)?[A-Za-z]:" + Separator +
        @"(?:(?<home>(?i:users|documents and settings))" + Separator + @")?" +
        @"|(?<![\w\\:])(?:\\\\)?\\\\(?=[^\\/\s])(?:[^\\/\s]+" + Separator + @"(?<home>(?i:users))" + Separator + @")?" +
        @"|(?<![\w.])~" + Separator +
        @"|(?<![A-Za-z0-9_.])(?:file://(?:localhost)?)?" +
        @"(?:(?<home>/home/|/Users/|(?:/mnt)?/[a-z]/(?i:users)/)|/root/|/tmp/|/var/tmp/|/var/folders/|/private/tmp/|/private/var/folders/)" +
        @")" +
        @"(?:(?<dir>" + Segment + @")(?<!\s)(?(esc)(?!\\u[0-9A-Fa-f]{4}))" + Separator + @")*" +
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
