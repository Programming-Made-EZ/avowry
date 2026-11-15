using System.Text;

// Shared by Avowry.Bench and, as a linked file, Avowry.BenchCs. Platform independent on purpose:
// Path.GetFileName would not split a Windows path on Linux.
// Known limits (card 418 second review, cards 426 and 435): prose followed by a relative path on one line collapses into that path
// (`copy /tmp/x.cs to src/y.cs` becomes `copy y.cs`: it over-scrubs and never leaks); an account folder alone swallows the
// words that follow it, with parentheses, dots and non-ASCII letters (`C:\Users\<user> is denied` becomes `<path>`);
// `\\?\C:\...` becomes `<path>?\short-name`; `match \\d+ here` becomes `match d+ here`; a single-backslash folder named `u`
// plus four hex digits after a name joins that name as an escaped letter; `-` plus one letter may be glued to a root after a
// space, a quote or an escape (`-isystem/home/<user>` is left as is); a root outside the listed ones that holds no `home` or
// `Users` folder is left as is.
// In a doubled-backslash or slash path a lone `\` before `n r t b f "` or `\uXXXX` ends the path, and a new path, or a glued
// flag, may start right after it.
// A UNC folder before `Users` or `home` never holds a quote, comma, semicolon, bracket, colon or JSON escape.
public static class PathScrub
{
    private const string PathMark = "<path>";

    private static readonly string[] AccountParents = ["users", "home", "documents and settings"];
    private static readonly string[] DriveParents = ["users", "documents and settings"];
    private static readonly string[] UnixFixedRoots =
    [
        "/root/",
        "/tmp/",
        "/var/tmp/",
        "/var/folders/",
        "/private/tmp/",
        "/private/var/folders/",
    ];

    public static string Apply(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;
        var once = Scan(text);
        return Scan(once);
    }

    private static string Scan(string text)
    {
        var outp = new StringBuilder(text.Length);
        var p = 0;
        var lastPathEnd = -1;
        while (p < text.Length)
        {
            if (TryDrive(text, p, lastPathEnd, out var drive, out var consumeDrive))
            {
                outp.Append(drive);
                p = consumeDrive;
                lastPathEnd = consumeDrive;
                continue;
            }

            if (TryUnc(text, p, lastPathEnd, out var unc, out var consumeUnc))
            {
                outp.Append(unc);
                p = consumeUnc;
                lastPathEnd = consumeUnc;
                continue;
            }

            if (TryTilde(text, p, lastPathEnd, out var tilde, out var consumeTilde))
            {
                outp.Append(tilde);
                p = consumeTilde;
                lastPathEnd = consumeTilde;
                continue;
            }

            if (TryUnix(text, p, lastPathEnd, out var unix, out var consumeUnix))
            {
                outp.Append(unix);
                p = consumeUnix;
                lastPathEnd = consumeUnix;
                continue;
            }

            outp.Append(text[p]);
            p++;
        }

        return outp.ToString();
    }

    private enum RootKind
    {
        Drive,
        Unc,
        Tilde,
        Unix,
    }

    private readonly struct RootStart
    {
        public RootStart(int rootIndex, int chainIndex, bool esc, bool sgl, string chainSep, bool sawMarker, bool markerAtRoot, bool bareUnc)
        {
            RootIndex = rootIndex;
            ChainIndex = chainIndex;
            Esc = esc;
            Sgl = sgl;
            ChainSep = chainSep;
            SawMarker = sawMarker;
            MarkerAtRoot = markerAtRoot;
            BareUnc = bareUnc;
        }

        public int RootIndex { get; }
        public int ChainIndex { get; }
        public bool Esc { get; }
        public bool Sgl { get; }
        public string ChainSep { get; }
        public bool SawMarker { get; }
        public bool MarkerAtRoot { get; }
        public bool BareUnc { get; }
    }

    private static bool TryDrive(string text, int p, int lastPathEnd, out string replacement, out int end)
    {
        replacement = string.Empty;
        end = p;
        if (!AllowsStart(text, p, RootKind.Drive, lastPathEnd, out var esc))
            return false;

        var i = p;
        if (StartsWith(text, i, "file:///"))
            i += 8;
        if (i + 2 >= text.Length || !IsAsciiLetter(text[i]) || text[i + 1] != ':')
            return false;
        i += 2;
        if (!ReadDriveOrTildeSeparator(text, i, out var next, out var sgl, out var chainSep))
            return false;

        var root = new RootStart(p, next, esc, sgl, chainSep, sawMarker: false, markerAtRoot: false, bareUnc: IsAfterBareUnc(text, p, lastPathEnd));
        if (!ReadPathTail(text, root, RootKind.Drive, out replacement, out end))
            return false;
        return true;
    }

    private static bool TryUnc(string text, int p, int lastPathEnd, out string replacement, out int end)
    {
        replacement = string.Empty;
        end = p;
        if (!AllowsStart(text, p, RootKind.Unc, lastPathEnd, out var esc))
            return false;
        if (text[p] != '\\' || p + 1 >= text.Length || text[p + 1] != '\\')
            return false;

        var i = p;
        var sgl = true;
        var chainSep = "\\";
        if (p + 3 < text.Length && text[p + 2] == '\\' && text[p + 3] == '\\')
        {
            i += 4;
            sgl = false;
            chainSep = "\\\\";
        }
        else
        {
            i += 2;
        }

        if (i >= text.Length || IsSeparator(text[i]) || char.IsWhiteSpace(text[i]))
            return false;
        var hostStart = i;
        while (i < text.Length && !IsSeparator(text[i]) && !char.IsWhiteSpace(text[i]))
            i++;
        if (i == hostStart)
            return false;
        var host = text.Substring(hostStart, i - hostStart);
        if (host == PathMark)
            return false;

        if (!ReadSeparatorRun(text, i, out var sepStart, out var sepEnd, out _))
            return false;
        if (sepStart == sepEnd)
            return false;

        i = sepEnd;
        var root = new RootStart(p, i, esc, sgl, chainSep, sawMarker: false, markerAtRoot: false, bareUnc: IsAfterBareUnc(text, p, lastPathEnd));
        if (!ReadPathTail(text, root, RootKind.Unc, out replacement, out end))
            return false;
        return end > i;
    }

    private static bool TryTilde(string text, int p, int lastPathEnd, out string replacement, out int end)
    {
        replacement = string.Empty;
        end = p;
        if (text[p] != '~')
            return false;
        if (!AllowsTildeStart(text, p))
            return false;
        if (!ReadDriveOrTildeSeparator(text, p + 1, out var next, out var sgl, out var chainSep))
            return false;

        var root = new RootStart(p, next, esc: false, sgl, chainSep, sawMarker: false, markerAtRoot: false, bareUnc: IsAfterBareUnc(text, p, lastPathEnd));
        if (!ReadPathTail(text, root, RootKind.Tilde, out replacement, out end))
            return false;
        return true;
    }

    private static bool TryUnix(string text, int p, int lastPathEnd, out string replacement, out int end)
    {
        replacement = string.Empty;
        end = p;
        if (!AllowsStart(text, p, RootKind.Unix, lastPathEnd, out var esc))
            return false;

        var i = p;
        if (StartsWith(text, i, "file://"))
        {
            i += 7;
            if (StartsWith(text, i, "localhost"))
                i += 9;
        }

        if (i >= text.Length || text[i] != '/')
            return false;

        var rootAt = i;
        var sawMarker = false;
        var markerAtRoot = false;
        foreach (var fixedRoot in UnixFixedRoots)
        {
            if (StartsWith(text, rootAt, fixedRoot))
            {
                i = rootAt + fixedRoot.Length;
                markerAtRoot = false;
                sawMarker = false;
                goto RootReady;
            }
        }

        var w = rootAt;
        while (w < text.Length && text[w] == '/')
        {
            if (StartsWith(text, w, "/home/"))
            {
                i = w + 6;
                sawMarker = true;
                markerAtRoot = true;
                goto RootReady;
            }

            if (StartsWith(text, w, "/Users/"))
            {
                i = w + 7;
                sawMarker = true;
                markerAtRoot = true;
                goto RootReady;
            }

            if (w + 8 < text.Length
                && text[w] == '/'
                && IsLowerAscii(text[w + 1])
                && text[w + 2] == '/'
                && StartsWithIgnoreCase(text, w + 3, "users/")
                && (w == rootAt || IsPrefixFolder(text, rootAt + 1, w - 1)))
            {
                i = w + 9;
                sawMarker = true;
                markerAtRoot = true;
                goto RootReady;
            }

            if (w + 12 < text.Length
                && StartsWith(text, w, "/mnt/")
                && IsLowerAscii(text[w + 5])
                && text[w + 6] == '/'
                && StartsWithIgnoreCase(text, w + 7, "users/")
                && (w == rootAt || IsPrefixFolder(text, rootAt + 1, w - 1)))
            {
                i = w + 13;
                sawMarker = true;
                markerAtRoot = true;
                goto RootReady;
            }

            var segStart = w + 1;
            var slash = text.IndexOf('/', segStart);
            if (slash < 0)
                break;
            if (!IsPrefixFolder(text, segStart, slash))
                break;
            w = slash;
        }
        return false;

    RootReady:
        var root = new RootStart(p, i, esc, sgl: false, chainSep: "/", sawMarker, markerAtRoot, bareUnc: IsAfterBareUnc(text, p, lastPathEnd));
        if (!ReadPathTail(text, root, RootKind.Unix, out replacement, out end))
            return false;
        return true;
    }

    private static bool ReadPathTail(string text, RootStart root, RootKind kind, out string replacement, out int end)
    {
        replacement = string.Empty;
        end = root.ChainIndex;
        var i = root.ChainIndex;
        var dirs = new List<string>();
        var seps = new List<string>();

        while (true)
        {
            if (!ReadFolderCandidate(text, i, out var segEnd))
                break;
            if (!ReadSeparatorRun(text, segEnd, out _, out var sepEnd, out var sepRun))
                break;
            if (!FolderPassesGuard(text, segEnd, root))
                break;
            dirs.Add(text.Substring(i, segEnd - i));
            seps.Add(sepRun);
            i = sepEnd;
        }

        var fileStart = i;
        var fileEnd = ReadFinal(text, i);
        var file = text.Substring(fileStart, fileEnd - fileStart);
        i = fileEnd;

        var account = IsAccountFolder(root, kind, dirs, seps, file);
        if (account)
            i = ReadTail(text, i);

        var afterBareUnc = root.BareUnc;
        replacement = file.Length == 0 || file == "~" || account || afterBareUnc ? PathMark : file;
        end = i;
        return end > root.RootIndex;
    }

    private static bool IsAccountFolder(RootStart root, RootKind kind, List<string> dirs, List<string> seps, string file)
    {
        if (file.Length == 0)
            return false;
        if (root.SawMarker && root.MarkerAtRoot && dirs.Count == 0)
            return true;
        if (dirs.Count == 0)
            return false;
        var last = dirs[^1];
        if (kind == RootKind.Drive)
        {
            if (InSet(dirs[0], DriveParents))
            {
                for (var n = 0; n < seps.Count; n++)
                {
                    if (!SameSepShape(seps[n], root.ChainSep))
                        return true;
                }
            }
            if (InSet(dirs[0], DriveParents) && InSet(last, DriveParents))
                return true;
        }
        if (kind == RootKind.Unc)
        {
            if (InSet(last, AccountParents))
                return true;
            var anyParent = false;
            for (var n = 0; n < dirs.Count; n++)
            {
                if (InSet(dirs[n], AccountParents))
                {
                    anyParent = true;
                    break;
                }
            }
            if (anyParent && !IsPlainShortName(file))
                return true;
            return false;
        }
        if (kind == RootKind.Unix)
        {
            if (last == "home" || last == "Users")
                return true;
            if (dirs.Count >= 2 && dirs[^2].Length == 1 && InSet(last, ["users"]))
                return true;
        }

        for (var n = 0; n < dirs.Count && n < seps.Count; n++)
        {
            if (seps[n] != root.ChainSep && InSet(dirs[n], AccountParents))
                return true;
            if (seps[n] != root.ChainSep && n + 1 < dirs.Count && InSet(dirs[n + 1], AccountParents))
                return true;
            if (seps[n] != root.ChainSep)
            {
                for (var m = n + 1; m < dirs.Count; m++)
                {
                    if (InSet(dirs[m], AccountParents))
                        return true;
                }
            }
        }
        return false;
    }

    private static int ReadTail(string text, int i)
    {
        while (i < text.Length && text[i] == '(')
        {
            var close = FindGroupClose(text, i + 1);
            if (close < 0)
                break;
            i = close + 1;
            i = ReadGluedWord(text, i);
        }

        while (i < text.Length && text[i] == ' ')
        {
            var w = ReadSpacedWord(text, i + 1);
            if (w == i + 1)
                break;
            i = w;
        }

        // Keep card-435 behavior for account tails that end right before an escape.
        while (true)
        {
            var next = ReadEscapeBoundToken(text, i);
            if (next == i)
                break;
            i = next;
        }
        return i;
    }

    private static int ReadEscapeBoundToken(string text, int i)
    {
        if (i >= text.Length || text[i] != ' ')
            return i;
        var p = i + 1;
        var seen = false;
        while (p < text.Length)
        {
            if (TryLetterEscape(text, p, out var e))
            {
                p = e;
                seen = true;
                continue;
            }

            var c = text[p];
            if (c == '\'' && IsApostrophe(text, p))
            {
                p++;
                seen = true;
                continue;
            }

            if (c == '\\' || char.IsWhiteSpace(c) || IsFinalStop(c))
                break;
            p++;
            seen = true;
        }

        if (!seen || p >= text.Length || text[p] != '\\')
            return i;
        if (IsE1(text, p) || IsUnicodeEscape(text, p) || IsEscapedQuote(text, p))
            return p;
        return i;
    }

    private static int FindGroupClose(string text, int i)
    {
        var p = i;
        while (p < text.Length && text[p] != ')')
        {
            if (TryLetterEscape(text, p, out var e))
            {
                p = e;
                continue;
            }
            if (text[p] == '\'' && IsApostrophe(text, p))
            {
                p++;
                continue;
            }
            if (IsFinalStop(text[p]))
                return -1;
            p++;
        }
        return p < text.Length && text[p] == ')' ? p : -1;
    }

    private static int ReadGluedWord(string text, int i)
    {
        var p = i;
        var end = i;
        while (p < text.Length)
        {
            if (IsRootStartForTail(text, p))
                break;
            if (TryLetterEscape(text, p, out var e))
            {
                p = e;
                if (IsValidGluedEnd(text, i, p))
                    end = p;
                continue;
            }
            var c = text[p];
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
            {
                p++;
                if (IsValidGluedEnd(text, i, p))
                    end = p;
                continue;
            }
            if (c == '\'' && p + 1 < text.Length && (char.IsLetter(text[p + 1]) || IsEscLetterAt(text, p + 1)))
            {
                p++;
                if (IsValidGluedEnd(text, i, p))
                    end = p;
                continue;
            }
            if (c == '.' && p + 1 < text.Length && (char.IsLetterOrDigit(text[p + 1]) || text[p + 1] == '_' || IsEscLetterAt(text, p + 1)))
            {
                p++;
                if (IsValidGluedEnd(text, i, p))
                    end = p;
                continue;
            }
            break;
        }
        return end;
    }

    private static int ReadSpacedWord(string text, int i)
    {
        var p = i;
        var end = i;
        while (p < text.Length)
        {
            if (TryLetterEscape(text, p, out var e))
            {
                p = e;
                if (!SpacedWordStopFollow(text, p))
                    end = p;
                continue;
            }
            var c = text[p];
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '(' || c == ')' || c == ' ')
            {
                p++;
                if (!SpacedWordStopFollow(text, p))
                    end = p;
                continue;
            }
            if (c == '.' && p + 1 < text.Length && text[p + 1] == '(')
            {
                p++;
                if (!SpacedWordStopFollow(text, p))
                    end = p;
                continue;
            }
            if (c == '.' && p + 1 < text.Length && (char.IsLetterOrDigit(text[p + 1]) || text[p + 1] == '_' || IsEscLetterAt(text, p + 1)))
            {
                p++;
                if (!SpacedWordStopFollow(text, p))
                    end = p;
                continue;
            }
            if (c == '\'' && p + 1 < text.Length && char.IsLetter(text[p + 1]))
            {
                p++;
                if (!SpacedWordStopFollow(text, p))
                    end = p;
                continue;
            }
            break;
        }
        return end;
    }

    private static bool ReadFolderCandidate(string text, int i, out int end)
    {
        end = i;
        var p = i;
        while (p < text.Length)
        {
            if (TryLetterEscape(text, p, out var escEnd))
            {
                p = escEnd;
                continue;
            }
            if (TryPlaceholder(text, p, out var phEnd))
            {
                p = phEnd;
                continue;
            }
            if (text[p] == '\'' && IsApostrophe(text, p))
            {
                p++;
                continue;
            }
            if (IsNameStop(text[p]) || text[p] == '\r' || text[p] == '\n')
                break;
            if (char.IsWhiteSpace(text[p]) && IsFlagBoundaryAhead(text, p))
                break;
            p++;
        }
        if (p == i)
            return false;
        if (char.IsWhiteSpace(text[p - 1]))
            return false;
        end = p;
        return true;
    }

    private static int ReadFinal(string text, int i)
    {
        var p = i;
        while (p < text.Length)
        {
            if (TryLetterEscape(text, p, out var escEnd))
            {
                p = escEnd;
                continue;
            }
            if (TryPlaceholder(text, p, out var phEnd))
            {
                p = phEnd;
                continue;
            }
            if (text[p] == '\'' && IsApostrophe(text, p))
            {
                p++;
                continue;
            }
            if (char.IsWhiteSpace(text[p]) || IsFinalStop(text[p]))
                break;
            if (LooksLikeRootPrefix(text, p))
                break;
            p++;
        }
        return p;
    }

    private static bool FolderPassesGuard(string text, int segEnd, RootStart root)
    {
        if (IsEscLetterAt(text, segEnd))
            return false;
        if (!root.Sgl)
        {
            if (IsE1(text, segEnd) || IsEscapedQuote(text, segEnd) || IsUnicodeEscape(text, segEnd))
                return false;
        }
        else if (root.Esc && IsUnicodeEscape(text, segEnd))
        {
            return false;
        }
        return true;
    }

    private static bool ReadDriveOrTildeSeparator(string text, int i, out int next, out bool sgl, out string chainSep)
    {
        next = i;
        sgl = false;
        chainSep = string.Empty;
        if (i >= text.Length || !IsSeparator(text[i]))
            return false;
        if (text[i] == '\\' && (i + 1 >= text.Length || !IsSeparator(text[i + 1])))
        {
            sgl = true;
            chainSep = "\\";
            next = i + 1;
            return true;
        }

        if (!ReadSeparatorRun(text, i, out _, out var sepEnd, out var sepRun))
            return false;
        next = sepEnd;
        chainSep = AllSlash(sepRun) ? "/" : "\\\\";
        return true;
    }

    private static bool ReadSeparatorRun(string text, int i, out int start, out int end, out string run)
    {
        start = i;
        var p = i;
        while (p < text.Length && IsSeparator(text[p]))
            p++;
        end = p;
        run = p > i ? text.Substring(i, p - i) : string.Empty;
        return p > i;
    }

    private static bool AllowsStart(string text, int p, RootKind kind, int lastPathEnd, out bool esc)
    {
        esc = false;
        if ((kind == RootKind.Drive || kind == RootKind.Unix) && p == lastPathEnd)
            return true;
        if (PlainLeft(text, p, kind))
            return true;
        if (p >= 6 && IsUnicodeEscape(text, p - 6) && p - 6 + 6 == p)
        {
            esc = true;
            return true;
        }
        if (p >= 2 && IsE1(text, p - 2) && p - 2 + 2 == p)
            return true;
        if (IsGluedFlagStart(text, p, out var flagEsc))
        {
            esc = flagEsc;
            return true;
        }
        return false;
    }

    private static bool AllowsTildeStart(string text, int p)
    {
        if (p == 0)
            return true;
        var c = text[p - 1];
        if (char.IsWhiteSpace(c) || IsIn(c, "\\/\"'`|*?:;,()<>[]"))
            return true;
        if (p >= 2 && IsE1(text, p - 2) && p - 2 + 2 == p)
            return true;
        if (p >= 6 && IsUnicodeEscape(text, p - 6) && p - 6 + 6 == p && IsAsciiUnicodeEscape(text, p - 6))
            return true;
        return IsGluedFlagStart(text, p, out _);
    }

    private static bool IsGluedFlagStart(string text, int p, out bool esc)
    {
        esc = false;
        if (p < 2 || text[p - 2] != '-' || !IsAsciiLetter(text[p - 1]))
            return false;
        if (p >= 8 && IsUnicodeEscape(text, p - 8) && p - 8 + 6 == p - 2 && IsAsciiUnicodeEscape(text, p - 8))
        {
            esc = true;
            return true;
        }
        if (p >= 8 && IsUnicodeEscape(text, p - 8) && p - 8 + 6 == p - 2 && !IsAsciiUnicodeEscape(text, p - 8))
            return false;
        if (p >= 4 && IsE1(text, p - 4) && p - 4 + 2 == p - 2)
            return true;
        if (p == 2)
            return true;

        var left = text[p - 3];
        return char.IsWhiteSpace(left) || IsIn(left, "\"'([,=;");
    }

    private static bool PlainLeft(string text, int p, RootKind kind)
    {
        if (p == 0)
            return true;
        var left = text[p - 1];
        if (kind == RootKind.Drive)
            return !IsWord(left);
        if (kind == RootKind.Unc)
            return !IsWord(left) && left != '\\' && left != ':';
        if (kind == RootKind.Unix)
        {
            if (IsWord(left) || left == '.')
                return false;
            if (p >= 2 && text[p - 2] == ':' && text[p - 1] == '/')
                return false;
            return true;
        }
        return false;
    }

    private static bool IsPrefixFolder(string text, int start, int end)
    {
        if (start >= end || end > text.Length)
            return false;
        if (char.IsWhiteSpace(text[end - 1]) || !IsWord(text[end - 1]))
            return false;
        for (var i = start; i < end; i++)
        {
            if (char.IsWhiteSpace(text[i]) || IsNameStop(text[i]))
                return false;
        }
        return true;
    }

    private static bool IsFlagBoundaryAhead(string text, int ws)
    {
        var i = ws;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;
        if (i + 1 >= text.Length || text[i] != '-' || !IsAsciiLetter(text[i + 1]))
            return false;
        var j = i + 2;
        if (j >= text.Length)
            return false;
        if (text[j] == '~' || text[j] == '/' || text[j] == '\\')
            return true;
        return j + 1 < text.Length && IsAsciiLetter(text[j]) && text[j + 1] == ':';
    }

    private static bool LooksLikeRootPrefix(string text, int i)
    {
        if (i + 1 < text.Length && IsAsciiLetter(text[i]) && text[i + 1] == ':')
            return true;
        if (StartsWithIgnoreCase(text, i, "file:/"))
            return true;
        return false;
    }

    private static bool TryPlaceholder(string text, int i, out int end)
    {
        end = i;
        if (i >= text.Length || text[i] != '<')
            return false;
        var p = i + 1;
        if (p >= text.Length || !IsAsciiLetter(text[p]))
            return false;
        while (p < text.Length && IsAsciiLetter(text[p]))
            p++;
        if (p >= text.Length || text[p] != '>')
            return false;
        end = p + 1;
        return true;
    }

    private static bool TryLetterEscape(string text, int i, out int end)
    {
        end = i;
        if (!IsUnicodeEscape(text, i))
            return false;
        if (i > 0 && text[i - 1] == '\\')
            return false;
        if (IsAsciiUnicodeEscape(text, i))
            return false;
        end = i + 6;
        return true;
    }

    private static bool IsEscLetterAt(string text, int i) => TryLetterEscape(text, i, out _);

    private static bool IsApostrophe(string text, int i)
    {
        if (i <= 0 || i + 1 >= text.Length || text[i] != '\'')
            return false;
        return IsApostropheLeft(text, i - 1) && IsApostropheRight(text, i + 1);
    }

    private static bool IsApostropheLeft(string text, int i)
    {
        if (i < 0)
            return false;
        if (char.IsLetter(text[i]))
            return true;
        if (text[i] == '>')
            return true;
        return i >= 5 && TryLetterEscape(text, i - 5, out var end) && end == i + 1;
    }

    private static bool IsApostropheRight(string text, int i)
    {
        if (i >= text.Length)
            return false;
        if (char.IsLetter(text[i]))
            return true;
        return TryLetterEscape(text, i, out _);
    }

    private static bool IsUnicodeEscape(string text, int i)
    {
        if (i < 0 || i + 5 >= text.Length || text[i] != '\\' || text[i + 1] != 'u')
            return false;
        return IsHex(text[i + 2]) && IsHex(text[i + 3]) && IsHex(text[i + 4]) && IsHex(text[i + 5]);
    }

    private static bool IsAsciiUnicodeEscape(string text, int i)
    {
        if (!IsUnicodeEscape(text, i))
            return false;
        var value = (HexValue(text[i + 2]) << 12) | (HexValue(text[i + 3]) << 8) | (HexValue(text[i + 4]) << 4) | HexValue(text[i + 5]);
        return value <= 0x7F;
    }
    private static bool IsE1(string text, int i) => i >= 0 && i + 1 < text.Length && text[i] == '\\' && IsIn(text[i + 1], "nrtbf") && (i == 0 || text[i - 1] != '\\');
    private static bool IsEscapedQuote(string text, int i) => i >= 0 && i + 1 < text.Length && text[i] == '\\' && text[i + 1] == '"' && (i == 0 || text[i - 1] != '\\');
    private static bool IsAfterBareUnc(string text, int p, int lastPathEnd) => p >= 2 && text[p - 1] == '\\' && text[p - 2] == '\\' && lastPathEnd < p - 1;
    private static bool IsRootStartForTail(string text, int p) => LooksLikeRootPrefix(text, p) || (p + 1 < text.Length && text[p] == '\\' && text[p + 1] == '\\') || text[p] == '/' || text[p] == '~';
    private static bool SpacedWordStopFollow(string text, int p)
    {
        if (p >= text.Length)
            return false;
        var c = text[p];
        if (IsAsciiLetter(c) || char.IsDigit(c) || c == '_' || c == ':' || c == '/' || c == '-')
            return true;
        if (c == '\\')
            return !IsE1(text, p) && !IsUnicodeEscape(text, p);
        if (c == '\'' && p + 1 < text.Length && IsAsciiLetter(text[p + 1]))
            return true;
        return c == '.' && p + 1 < text.Length && (IsAsciiLetter(text[p + 1]) || char.IsDigit(text[p + 1]) || text[p + 1] == '_' || IsEscLetterAt(text, p + 1));
    }
    private static bool IsValidGluedEnd(string text, int start, int p)
    {
        if (p <= start)
            return false;
        if (p >= text.Length)
            return true;
        var c = text[p];
        if (IsAsciiLetter(c) || char.IsDigit(c) || c == '_')
        {
            var prev = text[p - 1];
            return (IsAsciiLetter(prev) || char.IsDigit(prev) || prev == '_') && IsRootStartForTail(text, p);
        }
        return !(c == ':' && p + 1 < text.Length && IsSeparator(text[p + 1]));
    }

    private static bool IsSeparator(char c) => c == '\\' || c == '/';
    private static bool IsWord(char c) => IsAsciiLetter(c) || char.IsDigit(c) || c == '_';
    private static bool IsPlainShortName(string s) => s.Length > 0 && s.All(c => IsAsciiLetter(c) || char.IsDigit(c) || c == '_' || c == '-' || c == '.');
    private static bool SameSepShape(string a, string b) => SepShape(a) == SepShape(b);
    private static int SepShape(string s)
    {
        var slash = false;
        var back = false;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == '/')
                slash = true;
            else if (s[i] == '\\')
                back = true;
        }
        if (slash && !back)
            return 0;
        if (!slash && back)
            return 1;
        return 2;
    }
    private static bool IsAsciiLetter(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    private static bool IsLowerAscii(char c) => c >= 'a' && c <= 'z';
    private static bool IsHex(char c) => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
    private static int HexValue(char c) => c <= '9' ? c - '0' : (c <= 'F' ? c - 'A' + 10 : c - 'a' + 10);
    private static bool IsNameStop(char c) => IsIn(c, "\\/\"'`|*?:;,<>[]");
    private static bool IsFinalStop(char c) => IsNameStop(c) || c == '(' || c == ')';
    private static bool IsIn(char c, string chars) => chars.IndexOf(c, StringComparison.Ordinal) >= 0;
    private static bool AllSlash(string s) => s.All(c => c == '/');
    private static bool StartsWith(string text, int i, string value) => i >= 0 && i + value.Length <= text.Length && string.CompareOrdinal(text, i, value, 0, value.Length) == 0;
    private static bool StartsWithIgnoreCase(string text, int i, string value) => i >= 0 && i + value.Length <= text.Length && string.Compare(text, i, value, 0, value.Length, StringComparison.OrdinalIgnoreCase) == 0;
    private static bool InSet(string value, string[] set) => set.Any(s => string.Equals(value, s, StringComparison.OrdinalIgnoreCase));
}
