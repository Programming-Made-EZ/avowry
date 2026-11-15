using System.Globalization;
using System.Text;

// Shared by Avowry.Bench and, as a linked file, Avowry.BenchCs. Platform independent on purpose:
// Path.GetFileName would not split a Windows path on Linux.
// Card 441 design: a linear scanner with the card 441 fixes from the reference slice.
// Known pinned limits (named in slice-rulings.md): 2.5n, 3.4d, 3.8a-d, 9b, and the pinned MINOR rows.
public static class PathScrub
{
    public static string Apply(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;
        return new Scan(text).Run();
    }

    private sealed class Scan
    {
        // ---- constants (each listed once) ----
        private const string PathMark = "<path>";                         // output for a path with no short name; never a UNC host
        private static readonly string[] AccountParents = { "users", "home", "documents and settings" }; // any case: a folder that holds account folders
        private static readonly string[] DriveParents = { "users", "documents and settings" };          // any case: the first folder after a drive
        private static readonly string[] UnixFixedRoots = { "/root/", "/tmp/", "/var/tmp/", "/var/folders/", "/private/tmp/", "/private/var/folders/" };
        private const string EscLetters = "nrtbf";                         // \n \r \t \b \f: end a path, open a new one
        private const string NameStops = "\\/\"'`|*?:;,<>[]";             // never in a folder name (plus \r \n)
        private const string FinalStops = "\\/\"'`|*?:;,()<>[]";          // never in a final name or a glued group (plus whitespace)
        private const string FlagLeft = "\"'([,=;";                        // a glued flag `-X` may follow these (plus start, whitespace, an escape)
        private const string TildeLeft = "\\/\"'`|*?:;,()<[]";            // a `~` root may follow these (plus start, whitespace, an escape)

        private readonly string t;
        private readonly int n;
        private int prevEnd = -1;
        private int deadFrom = -1, deadTo = -2;
        private char lastForm;
        private int finalStart = -1;
        private bool slashChain;
        // A letter before `:/` stays in the final name when it follows an ASCII letter, digit or `_` (any chain), or when it
        // starts the final name of a Unix or `~` chain: `.../alice:/data`, `C:\...\proj:/app` and `/tmp/x:/home/...` are colon lists.
        // (b) also holds in a drive or UNC chain after a folder when an account marker follows (`C:\tmp\x:/home/...`).
        private bool finalAfterFolder;
        private bool OwnLetter(int j) => At(j + 2) == '/' &&
            (AsciiWord(At(j - 1))
             || (j == finalStart && (slashChain || (finalAfterFolder && UnixRootAt(j + 2)))));

        public Scan(string text) { t = text; n = text.Length; }

        public string Run()
        {
            StringBuilder? sb = null;
            int last = 0, p = 0;
            while (p < n)
            {
                if (!TryAt(p, out var end, out var repl)) { p++; continue; }
                // A path right after a bare `\\` keeps no short name: `\\name\...` would be a UNC root on a second pass.
                if (p - 2 >= last && t[p - 1] == '\\' && t[p - 2] == '\\') repl = PathMark;
                sb ??= new StringBuilder(n);
                sb.Append(t, last, p - last).Append(repl);
                last = prevEnd = p = end;
            }
            if (sb == null) return t;
            return sb.Append(t, last, n - last).ToString();
        }

        // ---------- character classes ----------
        private char At(int i) => i >= 0 && i < n ? t[i] : '\0';
        private static bool Hex(char c) => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
        private static bool Ascii(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
        private static bool AsciiWord(char c) => Ascii(c) || c is >= '0' and <= '9' || c == '_';
        private static bool Lnu(char c) => char.IsLetter(c) || char.IsNumber(c) || c == '_';
        private static bool Space(char c) =>
            c is '\t' or '\n' or '\v' or '\f' or '\r' or '\x85' || char.GetUnicodeCategory(c) is UnicodeCategory.SpaceSeparator or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator;
        private static bool RegexWord(char c) =>
            char.IsLetter(c) || char.GetUnicodeCategory(c) is UnicodeCategory.NonSpacingMark or UnicodeCategory.DecimalDigitNumber or UnicodeCategory.ConnectorPunctuation;
        private static bool Sep(char c) => c is '\\' or '/';
        private bool Has(int i, string s) => i >= 0 && i + s.Length <= n && string.CompareOrdinal(t, i, s, 0, s.Length) == 0;
        private bool HasCi(int i, string s) => i >= 0 && i + s.Length <= n && string.Compare(t, i, s, 0, s.Length, StringComparison.OrdinalIgnoreCase) == 0;

        // ---------- the shared escape set ----------
        private bool UAt(int i) => At(i) == '\\' && At(i + 1) == 'u' && i + 5 < n && Hex(t[i + 2]) && Hex(t[i + 3]) && Hex(t[i + 4]) && Hex(t[i + 5]);
        private bool AsciiCode(int i) => t[i + 2] == '0' && t[i + 3] == '0' && t[i + 4] is >= '0' and <= '7';
        private bool LetterEscAt(int i) => UAt(i) && !AsciiCode(i) && At(i - 1) != '\\';
        private bool UBefore(int p) => p >= 6 && UAt(p - 6);
        private bool AsciiUBefore(int p) => UBefore(p) && AsciiCode(p - 6);
        private string Letters => EscLetters;
        private bool E1Before(int p) => p >= 2 && t[p - 2] == '\\' && Letters.IndexOf(t[p - 1]) >= 0 && At(p - 3) != '\\';
        private bool EndEscAt(int i) => At(i) == '\\' && ((Letters + "\"").IndexOf(At(i + 1)) >= 0 || UAt(i));

        private bool RootStartAt(int i) => (Ascii(At(i)) && At(i + 1) == ':' && Sep(At(i + 2))) || (Has(i, "file:") && Sep(At(i + 5)));

        // A glued flag `-X` just before p: 0 none, 1 plain, 2 after an ASCII \uXXXX (the path is escape-quoted).
        // A glued flag: `-` plus one or more ASCII letters (`-I`, `-isystem`); before a drive or UNC root also `/` plus letters
        // (MSVC `/I`, `/Fo`). The letters are scanned back once per root candidate, so the scan stays linear.
        private int FlagBefore(int p, bool slash = false, bool multi = true)
        {
            var k = p - 1;
            while (k >= 0 && Ascii(t[k])) k--;
            var letters = p - 1 - k;
            if (letters == 0 || k < 0) return 0;
            var dash = t[k];
            // A `/` flag (MSVC) has one or two letters and only glues to a backslash drive or a UNC root.
            if (!(dash == '-' || (slash && dash == '/' && letters <= 2))) return 0;
            if (!multi && letters != 1) return 0;
            var d = k;
            var left = FlagLeft;
            if (d == 0 || Space(t[d - 1]) || left.IndexOf(t[d - 1]) >= 0 || E1Before(d)) return 1;
            if (dash == '-' && AsciiUBefore(d)) return 2;
            return 0;
        }

        // ---------- roots ----------
        private bool TryAt(int p, out int end, out string repl)
        {
            end = 0;
            repl = "";
            var c = t[p];
            return (Ascii(c) && TryDrive(p, out end, out repl))
                || (c == '\\' && (TryUnc(p, out end, out repl) || TryJsonHome(p, out end, out repl)))
                || (c == '~' && TryTilde(p, out end, out repl))
                || ((c == '/' || c == 'f') && TryUnix(p, out end, out repl));
        }

        private bool StartOk(int p, bool plainOk, bool rootGlue, out bool esc, bool slashFlag = false)
        {
            esc = false;
            if (plainOk) return true;
            if (UBefore(p)) { esc = true; return true; }
            if (E1Before(p)) return true;
            var f = FlagBefore(p, slashFlag);
            if (f == 2) { esc = true; return true; }
            if (f == 1) return true;
            return rootGlue && p == prevEnd;
        }

        private bool RootSep(ref int q, out bool sgl)
        {
            sgl = false;
            if (At(q) == '\\' && !Sep(At(q + 1))) { sgl = true; q++; return true; }
            if (!Sep(At(q))) return false;
            while (Sep(At(q))) q++;
            return true;
        }

        private bool TryDrive(int p, out int end, out string repl)
        {
            end = 0;
            repl = "";
            var q = Has(p, "file:///") ? p + 8 : p;
            if (!(Ascii(At(q)) && At(q + 1) == ':')) return false;
            // Adjacency (after any path) only for `file:///` or a backslash drive (`X:\`):
            // `…/lib:/home/…` and `C:\…\proj:/app` are colon lists, not a drive `b:` or `j:`.
            var glue = q > p || At(q + 2) == '\\';
            // A bare `X:/` right after `/` is no drive: `a/b:/home/...` is a colon list (`file:///C:/` is read at its `f`).
            if (q == p && At(p - 1) == '/' && At(q + 2) == '/') return false;
            if (!StartOk(p, p == 0 || !AsciiWord(t[p - 1]), glue, out var esc, At(q + 2) == '\\')) return false;
            q += 2;
            if (!RootSep(ref q, out var sgl)) return false;
            return Chain(q, 'D', sgl, esc, false, out end, out repl);
        }

        private bool TryUnc(int p, out int end, out string repl)
        {
            end = 0;
            repl = "";
            if (At(p + 1) != '\\') return false;
            var prev = At(p - 1);
            if (!StartOk(p, p == 0 || !(RegexWord(prev) || prev == '\\' || prev == ':'), false, out var esc, true)) return false;
            // `\\?\UNC\server\share\...` is a UNC path written in the long form.
            var longUnc = Has(p, "\\\\?\\UNC\\");
            var four = !longUnc && At(p + 2) == '\\' && At(p + 3) == '\\';
            var q = longUnc ? p + 8 : four ? p + 4 : p + 2;
            if (q >= n || Sep(t[q]) || Space(t[q])) return false;
            if (Has(q, PathMark)) return false;
            return Chain(q, 'U', !four, esc, false, out end, out repl);
        }

        private bool TryTilde(int p, out int end, out string repl)
        {
            end = 0;
            repl = "";
            var ok =
                p == 0 || Space(t[p - 1]) || TildeLeft.IndexOf(t[p - 1]) >= 0 || E1Before(p) || AsciiUBefore(p) || FlagBefore(p, false, false) != 0;
            if (!ok) return false;
            var q = p + 1;
            // `~name/`: a named user's home; the name is the account folder and is never output.
            if (!Sep(At(q)))
            {
                var k = q;
                if (At(k) == '<') k += Token(k, true);
                else if (Ascii(At(k))) while (AsciiWord(At(k)) || At(k) is '.' or '-') k++;
                if (k == q || !Sep(At(k))) return false;
                q = k;
            }
            if (!RootSep(ref q, out var sgl)) return false;
            return Chain(q, 'T', sgl, UBefore(p), false, out end, out repl);
        }

        private bool TryUnix(int p, out int end, out string repl)
        {
            end = 0;
            repl = "";
            var q = p;
            var isFile = false;
            if (Has(q, "file://"))
            {
                q += 7;
                isFile = true;
                if (Has(q, "localhost")) q += 9;
            }
            if (At(q) != '/') return false;
            var prev = At(p - 1);
            var plainOk = p == 0 || (!(AsciiWord(prev) || prev == '.') && !(prev == '/' && At(p - 2) == ':'));
            var ssh = !plainOk && !isFile && SshHostBefore(p);
            if (ssh) plainOk = true;
            if (!StartOk(p, plainOk, isFile, out _)) return false;

            // The walk: prefix folders (no whitespace, ending in an ASCII letter, digit or _) until an account marker.
            // The walk is a function of its position, so a walk that starts on a `/` an earlier failed walk visited
            // (deadFrom..deadTo) fails the same way: skip it. Each character is walked at most once.
            var w = q;
            var dead = q >= deadFrom && q <= deadTo;
            while (!dead)
            {
                var len = AccountMarkerAt(w);
                if (len > 0) return SshSlash(ssh, Chain(w + len, 'X', false, false, true, out end, out repl), ref repl);
                var j = w + 1;
                while (j < n && !Space(t[j]) && NameStops.IndexOf(t[j]) < 0) j++;
                if (j > w + 1 && At(j) == '/' && AsciiWord(t[j - 1])) { w = j; continue; }
                deadFrom = q;
                deadTo = w;
                break;
            }
            foreach (var r in UnixFixedRoots)
                if (Has(q, r)) return SshSlash(ssh, Chain(q + r.Length, 'X', false, false, false, out end, out repl), ref repl);
            return false;
        }

        // Would a Unix root start at w (after `:`)? The walk to an account marker, else a fixed root. The walk stops at the
        // next `:` (a NameStop), so in a colon list each character is checked at most once more.
        private bool UnixRootAt(int w)
        {
            if (At(w) != '/') return false;
            var v = w;
            while (true)
            {
                if (AccountMarkerAt(v) > 0) return true;
                var j = v + 1;
                while (j < n && !Space(t[j]) && NameStops.IndexOf(t[j]) < 0) j++;
                if (j > v + 1 && At(j) == '/' && AsciiWord(t[j - 1])) { v = j; continue; }
                break;
            }
            foreach (var r in UnixFixedRoots) if (Has(w, r)) return true;
            return false;
        }

        // After `ssh://host` the path keeps its leading `/`: `ssh://host/repo.git`, not `ssh://hostrepo.git`.
        private bool SshSlash(bool ssh, bool ok, ref string repl)
        {
            if (ok && ssh) repl = "/" + repl;
            return ok;
        }

        // `ssh://[user@]host[:port]` or `sftp://...` right before p: the path is a file-system path, not a web path.
        private bool SshHostBefore(int p)
        {
            var k = p - 1;
            while (k >= 0 && (AsciiWord(t[k]) || t[k] is '.' or '-' or '@' or ':')) k--;
            return k < p - 1 && At(k) == '/' && At(k - 1) == '/' && (Has(k - 5, "ssh:") || Has(k - 6, "sftp:"));
        }

        // A JSON string with escaped slashes: `\/home\/` + `name` or `\/Users\/` + `name`.
        private bool TryJsonHome(int p, out int end, out string repl)
        {
            end = 0; repl = "";
            var len = Has(p, "\\/home\\/") ? 8 : Has(p, "\\/Users\\/") ? 9 : 0;
            if (len == 0) return false;
            var prev = At(p - 1);
            if (!StartOk(p, p == 0 || !(AsciiWord(prev) || prev == '.' || prev == '\\'), false, out _)) return false;
            return Chain(p + len, 'X', false, false, true, out end, out repl);
        }

        // `/home/`, `/Users/` (exact case), or `/x/users/` (x a lowercase drive letter, `users` any case).
        // or a mail spool `/var/mail/`, `/var/spool/mail/` (the file is named after the account).
        // `/mnt/x/users/` needs no rule of its own: the walk passes `/mnt` and finds `/x/users/` with the same end.
        private int AccountMarkerAt(int w)
        {
            if (Has(w, "/home/")) return 6;
            if (Has(w, "/Users/")) return 7;
            if (Has(w, "/var/mail/")) return 10;
            if (Has(w, "/var/spool/mail/")) return 16;
            if (At(w + 1) is >= 'a' and <= 'z' && At(w + 2) == '/' && HasCi(w + 3, "users") && At(w + 8) == '/')
                return 9;
            return 0;
        }

        // ---------- names ----------
        private int Token(int j, bool final)
        {
            if (j >= n) return 0;
            var c = t[j];
            if (c == '<')
            {
                var k = j + 1;
                while (Ascii(At(k))) k++;
                return k > j + 1 && At(k) == '>' ? k + 1 - j : 0;
            }
            if (c == '\\') return LetterEscAt(j) ? 6 : 0;
            if (c == '\'') return Apostrophe(j) ? 1 : 0;
            if (final)
            {
                var stops = FinalStops;
                return Space(c) || stops.IndexOf(c) >= 0 || (RootStartAt(j) && !OwnLetter(j)) ? 0 : 1;
            }
            var nstops = NameStops;
            if (c is '\r' or '\n' || nstops.IndexOf(c) >= 0) return 0;
            // A folder name never holds whitespace followed by an opener (a glued flag `-X`, or `(`) and a root.
            if (Space(c) && At(j + 1) == '-' && Ascii(At(j + 2)))
            {
                var k = j + 2;
                while (Ascii(At(k))) k++;
                if (RootAhead(k)) return 0;
            }
            return 1;
        }

        private bool RootAhead(int k) => At(k) == '/' || (At(k) == '\\' && At(k + 1) == '\\') || At(k) == '~' || (Ascii(At(k)) && At(k + 1) == ':');

        // An apostrophe inside a name: a letter, a placeholder's `>` or an escaped letter before; a letter or an escaped letter after.
        private bool Apostrophe(int j)
        {
            var before = char.IsLetter(At(j - 1)) || At(j - 1) == '>' || (j >= 6 && LetterEscAt(j - 6));
            return before && (char.IsLetter(At(j + 1)) || LetterEscAt(j + 1));
        }

        private bool Guard(int r, bool sgl, bool esc)
        {
            if (UAt(r) && !AsciiCode(r)) return false;
            if (!sgl)
                return !EndEscAt(r);
            return !(esc && UAt(r));
        }

        private bool NamedCi(int s, int e, string[] names)
        {
            foreach (var h in names)
                if (e - s == h.Length && HasCi(s, h)) return true;
            return false;
        }

        // The account rule. The last folder (s..e, index idx; the folder before it ps..pe) is an account parent when:
        // drive: it is the first folder and `users` or `documents and settings` (any case);
        // UNC: it follows the host and is `users`, `home` or `documents and settings` (any case);
        // Unix: it is `home` or `Users` (exact case), or `users` (any case) after a one-letter folder;
        // any form: the chain is bridged (a separator run between folders differs from the chain's own run,
        // so a second root is glued on) and it is `users`, `home` or `documents and settings` (any case).
        private bool IsAccountParent(char form, int idx, int ps, int pe, int s, int e, bool bridged)
        {
            if (form == 'D' && idx == 0 && NamedCi(s, e, DriveParents)) return true;
            if (form == 'U' && idx > 0 && NamedCi(s, e, AccountParents)) return true;
            if (form == 'X')
            {
                if (e - s == 4 && Has(s, "home") || e - s == 5 && Has(s, "Users")) return true;
                if (e - s == 5 && HasCi(s, "users") && pe - ps == 1 && t[ps] is >= 'a' and <= 'z') return true;
            }
            return bridged && NamedCi(s, e, AccountParents);
        }

        // The chain: folders, the final name, then (for an account folder) its tail. Every character read is consumed.
        private bool Chain(int q, char form, bool sgl, bool esc, bool rootIsParent, out int end, out string repl)
        {
            end = 0;
            repl = "";
            var unc = form == 'U';
            // The chain's own separator run: one `\` (single-backslash), `\\` (doubled), or `/`.
            var own = sgl ? "\\" : t[q - 1] == '/' ? "/" : "\\\\";
            int dirs = 0, ls = -1, le = -1, ps = -1, pe = -1;
            var bridged = false;
            if (unc && Ascii(At(q)) && At(q + 1) == ':' && Sep(At(q + 2)) && Guard(q + 2, sgl, esc))
            {
                dirs = 1; ls = q; le = q + 2;
                q += 2;
                while (Sep(At(q))) q++;
            }

            while (true)
            {
                var r = q;
                for (var k = Token(r, false); k > 0; k = Token(r, false)) r += k;
                // The drive's account parent (first folder in DriveParents) is followed by any separator, unguarded:
                // `C:/Users\` + `nate\x.cs` is a mixed-slash path, not a path ended by `\n`.
                var parentSep = form == 'D' && dirs == 0 && r > q && Sep(At(r)) && NamedCi(q, r, DriveParents);
                if (r > q && !Space(t[r - 1]) && Sep(At(r)) && (parentSep || Guard(r, sgl, esc)))
                {
                    if (dirs > 0 && !(q - le == own.Length && Has(le, own))) bridged = true;
                    // A folder that ends in whitespace and `(` opens a second path: `x.cs (/home/...`.
                    if (r - q >= 2 && t[r - 1] == '(' && Space(t[r - 2])) bridged = true;
                    dirs++; ps = ls; pe = le; ls = q; le = r;
                    q = r;
                    while (Sep(At(q))) q++;
                    continue;
                }
                break;
            }
            if (unc && dirs == 0) return false; // `\\host` alone is no UNC path
            finalStart = q; slashChain = form is 'X' or 'T';
            finalAfterFolder = dirs > (unc ? 1 : 0);
            var f1 = q;
            for (var k = Token(f1, true); k > 0; k = Token(f1, true)) f1 += k;
            // The final name is an account folder after the root's own marker or after an account parent.
            var account = (rootIsParent && dirs == 0) || (dirs > 0 && IsAccountParent(form, dirs - 1, ps, pe, ls, le, bridged));
            end = account ? Tail(f1) : f1;
            var file = t.Substring(q, f1 - q);
            repl = file.Length == 0
                || file == "~"
                || account
                || (file.Length == 1 && Ascii(file[0]) && At(f1) == ':' && At(f1 + 1) == '/' && !UnixRootAt(f1 + 1))
                ? PathMark
                : file;
            lastForm = form;
            return true;
        }

        // ---------- the account tail ----------
        private int Tail(int q)
        {
            while (At(q) == '(')
            {
                var k = q + 1;
                while (k < n)
                {
                    var c = t[k];
                    if (LetterEscAt(k)) { k += 6; continue; }
                    if (c == ' ') { k++; continue; }
                    if (c == '\'' && Apostrophe(k)) { k++; continue; }
                    if (Space(c) || FinalStops.IndexOf(c) >= 0) break;
                    k++;
                }
                if (At(k) != ')') break;
                q = GluedWord(k + 1);
            }

            while (true)
            {
                var s = q;
                while (At(s) == ' ') s++;
                if (s == q) break;
                var e = SpacedWord(s);
                if (e == s) break;
                q = e;
            }
            return q;
        }

        private List<int> WordEnds(int s, bool parens, bool stopAtRoot)
        {
            var ends = new List<int>();
            var j = s;
            while (j < n)
            {
                if (stopAtRoot && RootStartAt(j)) break;
                var c = t[j];
                int k;
                if (Lnu(c) || c == '-' || (parens && c is '(' or ')')) k = 1;
                else if (c == '\'' && (Apostrophe(j) || (stopAtRoot && j == s && (char.IsLetter(At(j + 1)) || LetterEscAt(j + 1))))) k = 1;
                else if (LetterEscAt(j)) k = 6;
                else if (c == '.' && (Lnu(At(j + 1)) || LetterEscAt(j + 1) || (parens && At(j + 1) == '('))) k = 1;
                else
                    break;
                j += k;
                ends.Add(j);
            }
            return ends;
        }

        // A word glued after `)`: no parentheses, never across a root; it may end right before a root glued to it.
        private int GluedWord(int s)
        {
            var ends = WordEnds(s, false, true);
            for (var i = ends.Count - 1; i >= 0; i--)
            {
                var e = ends[i];
                var c = At(e);
                if (!(Lnu(c) || (c == ':' && Sep(At(e + 1))))) return e;
                if (RootStartAt(e) && Lnu(t[e - 1])) return e;
            }
            return s;
        }

        private int SpacedWord(int s)
        {
            var ends = WordEnds(s, true, false);
            for (var i = ends.Count - 1; i >= 0; i--)
                if (!StopAt(ends[i])) return ends[i];
            return s;
        }

        // Prose follows a spaced word if a letter, digit, `_ : / -`, a non-escape `\`, `'x` or `.x` comes next.
        private bool StopAt(int e)
        {
            if (e >= n) return false;
            var c = t[e];
            if (Lnu(c) || c is ':' or '/' or '-') return true;
            if (c == '\\')
                return !((EscLetters + "\"").IndexOf(At(e + 1)) >= 0 || UAt(e));
            if (c == '\'' && Ascii(At(e + 1))) return true;
            return c == '.' && Lnu(At(e + 1));
        }
    }
}
