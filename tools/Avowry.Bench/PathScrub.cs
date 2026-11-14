using System.Text.RegularExpressions;

// Shared by Avowry.Bench and, as a linked file, Avowry.BenchCs. Platform independent on purpose:
// Path.GetFileName would not split a Windows path on Linux.
// Known limits (card 418 second review, cards 426 and 435): prose followed by a relative path on one line collapses into that path
// (`copy /tmp/x.cs to src/y.cs` becomes `copy y.cs`: it over-scrubs and never leaks); an account folder alone swallows the
// words that follow it, with parentheses, dots and non-ASCII letters (`C:\Users\<user> is denied` becomes `<path>`);
// `\\?\C:\...` becomes `<path>?\short-name`; `match \\d+ here` becomes `match d+ here`; a single-backslash folder named `u`
// plus four hex digits after a name joins that name as an escaped letter; only `-` plus one letter may be glued to a root
// (`-isystem/home/<user>` is left as is); a root outside the listed ones that holds no `home` or `Users` folder is left as is.
// In a path written with doubled backslashes or slashes, a lone `\` before `n r t b f "` or `\uXXXX` ends the path.
public static class PathScrub
{
    private const string Separator = @"[\\/]+";
    private const string EscLetter = @"(?<!\\)\\u(?!00[0-7])[0-9A-Fa-f]{4}";
    private const string EscBefore = @"(?<=(?<!\\)\\[nrt])";
    private const string Glued = @"(?<=(?<!\S)-[A-Za-z])";
    private const string Plain = @"[^\s\\/""'`|*?:;,()<>\[\]]";
    private const string Word = @"(?:[\p{L}\p{N}_()-]|(?<=[A-Za-z])'(?=[A-Za-z])|" + EscLetter + @"|\.(?=[\p{L}\p{N}_]))+";
    private const string Stop = @"[\p{L}\p{N}_:/-]|\\(?![nrtbf""]|u[0-9A-Fa-f]{4})|'[A-Za-z]|\.[\p{L}\p{N}_]";
    private const string Segment = @"(?:<[A-Za-z]+>|" + EscLetter + @"|(?<=[A-Za-z])'(?=[A-Za-z])|[^\\/\r\n""'`|*?:;,<>\[\]])+";
    private const string FinalSegment = @"(?:<[A-Za-z]+>|" + EscLetter + @"|(?<=[A-Za-z])'(?=[A-Za-z])|[^\s\\/""'`|*?:;,()<>\[\]])*";

    private static readonly Regex RootedPath = new(
        @"(?:" +
        @"(?:(?<![A-Za-z0-9_])|(?<=(?<esc>\\u[0-9A-Fa-f]{4}))|" + EscBefore + "|" + Glued + @")(?:file:///)?[A-Za-z]:(?:(?<sgl>\\)(?![\\/])|" + Separator + ")" +
        @"(?:(?<home>(?i:users|documents and settings))" + Separator + @")?" +
        @"|(?:(?<![\w\\:])|(?<=(?<esc>\\u[0-9A-Fa-f]{4}))|" + EscBefore + "|" + Glued + @")(?:\\\\\\\\|(?<sgl>)\\\\)(?=[^\\/\s])(?:[^\\/\s]+" + Separator + @"(?:[^\\/\s]+" + Separator + @")*?(?<home>(?i:users|home))" + Separator + @")?" +
        @"|(?<![\w.])~(?:(?<sgl>\\)(?![\\/])|" + Separator + ")" +
        @"|(?:(?<![A-Za-z0-9_.])(?<!:/)|(?<=(?<esc>\\u[0-9A-Fa-f]{4}))|" + EscBefore + "|" + Glued + @")(?:file://(?:localhost)?)?" +
        @"(?:(?:/[^\s\\/""'`|*?:;,<>\[\]]*[A-Za-z0-9_])*?(?<home>/home/|/Users/|(?:/mnt)?/[a-z]/(?i:users)/)|/root/|/tmp/|/var/tmp/|/var/folders/|/private/tmp/|/private/var/folders/)" +
        @")" +
        @"(?:(?<dir>" + Segment + @")(?<!\s)(?!\\u(?!00[0-7])[0-9A-Fa-f]{4})(?(sgl)(?(esc)(?!\\u[0-9A-Fa-f]{4}))|(?!\\(?:[nrtbf""]|u[0-9A-Fa-f]{4})))" + Separator + @")*" +
        @"(?<file>" + FinalSegment + @")" +
        @"(?(dir)(?:)|(?(home)(?<tail>(?:\(" + Plain + @"*\)" + Plain + @"*)*(?:[ ]+" + Word + "(?!" + Stop + @"))*)))",
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
