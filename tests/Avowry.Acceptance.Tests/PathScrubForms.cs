using System.Reflection;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

public sealed class PathScrubForms
{
    public static IEnumerable<object[]> Forms()
    {
        const string shortForm = "Candidate.cs(7,1): error CS1529: planted [Verification.csproj]";
        yield return R(
            @"C:\Users\<user>\AppData\Local\Temp\phrase-bench-cs\a\verification-b\Candidate.cs(7,1): error CS1529: planted [C:\Users\<user>\AppData\Local\Temp\phrase-bench-cs\a\verification-b\Verification.csproj]",
            shortForm);
        yield return R(
            "/home/<user>/tmp/phrase-bench-cs/a/verification-b/Candidate.cs(7,1): error CS1529: planted [/home/<user>/tmp/phrase-bench-cs/a/verification-b/Verification.csproj]",
            shortForm);
        yield return R(@"\\host\share\folder\Candidate.cs", "Candidate.cs");
        yield return R(@"~\AppData\Local\Temp\Candidate.cs", "Candidate.cs");
        yield return R("~/tmp/Candidate.cs", "Candidate.cs");
        yield return R("/tmp/phrase-bench/Candidate.cs", "Candidate.cs");
        yield return R("/var/folders/xx/yy/T/Candidate.cs", "Candidate.cs");
        yield return R("/root/tmp/Candidate.cs", "Candidate.cs");
        yield return R(
            @"An error occurred trying to start process 'D:\My Projects\Sub Folder\missing.exe' with working directory 'D:\My Projects\Sub Folder'. The system cannot find the file specified.",
            "An error occurred trying to start process 'missing.exe' with working directory 'Sub Folder'. The system cannot find the file specified.");
        yield return R(@"D:\My Projects\Sub Folder\x.cs", "x.cs");
        yield return R(@"C:\\Users\\<user>\\Temp\\Candidate.cs", "Candidate.cs");
        yield return R(@"C:\Users\<user>\Temp\Candidate.cs(7,1)", "Candidate.cs(7,1)");
        yield return R(@"[C:\Users\<user>\Temp\Verification.csproj]", "[Verification.csproj]");
        yield return R(@"C:\", "<path>");
        yield return R(@"C:\Users\<user>", "<path>");
        yield return R("/home/<user>", "<path>");
        yield return R("file:///C:/Users/<user>/Temp/Candidate.cs", "Candidate.cs");
    }

    // Found by the card 418 second review: an account folder with a space or an apostrophe, at the end of a path or alone.
    // The names are built from parts at run time, never typed as one literal (card 418 account-name rule).
    public static IEnumerable<object[]> OddAccountFolders()
    {
        var spaced = "Two" + " " + "Words";
        var apostrophe = "O" + "'" + "Brien";
        yield return R($@"Access to the path 'C:\Users\{spaced}' is denied.", "Access to the path '<path>' is denied.");
        yield return R($"see /home/{spaced.ToLowerInvariant()}", "see <path>");
        yield return R($@"path C:\Users\{apostrophe}\x.cs end", "path x.cs end");
        yield return R($@"C:\Users\{apostrophe}", "<path>");
        yield return R($@"C:\Users\{spaced}\AppData\x.cs", "x.cs");
    }

    public static IEnumerable<object[]> Untouched()
    {
        yield return R("</Project>", "</Project>");
        yield return R("https://host/path", "https://host/path");
        yield return R("https://example.com/home/x", "https://example.com/home/x");
        yield return R(@"obj\Debug\x.cs", @"obj\Debug\x.cs");
        yield return R("src/a/b.cs", "src/a/b.cs");
        yield return R("/a/b", "/a/b");
        yield return R("Build FAILED.", "Build FAILED.");
        yield return R("tests_did_not_run", "tests_did_not_run");
        yield return R("error CS0117: 'Greeter' does not contain a definition for 'Handled'", "error CS0117: 'Greeter' does not contain a definition for 'Handled'");
        yield return R("error CS0029: Cannot implicitly convert type 'int' to 'string'", "error CS0029: Cannot implicitly convert type 'int' to 'string'");
    }

    public static IEnumerable<object[]> WordsAroundAPath()
    {
        yield return R("Could not find file /tmp/x.cs in /usr/lib/dotnet", "Could not find file x.cs in /usr/lib/dotnet");
        yield return R("copy /tmp/a/x.cs to /tmp/b/y.cs", "copy x.cs to y.cs");
        yield return R(@"see C:\a\b.cs (line 3) and /docs/x", "see b.cs (line 3) and /docs/x");
        yield return R(@"match \d+ here", @"match \d+ here");
        yield return R(@"see C:\a\x.cs.", "see x.cs.");
        yield return R(@"C:\a\x.cs, then", "x.cs, then");
        yield return R("/x then /tmp/a/y.cs", "/x then y.cs");
    }

    public static IEnumerable<object[]> WordsAroundAPathPending() => WordsAroundAPath().Take(3);

    public static IEnumerable<object[]> WordsAroundAPathCharacterisation() => WordsAroundAPath().Skip(3);

    public static IEnumerable<object[]> MissedRoots()
    {
        yield return R("/mnt/c/Users/<user>/x.cs", "x.cs");
        yield return R("/mnt/c/Users/<user>", "<path>");
        yield return R("/c/Users/<user>/x.cs", "x.cs");
        yield return R("/c/Users/<user>", "<path>");
        yield return R("/var/tmp/a/x.cs", "x.cs");
        yield return R("/private/tmp/a/x.cs", "x.cs");
        yield return R("file://localhost/home/<user>/x.cs", "x.cs");
        yield return R(@"C:\Documents and Settings\<user>", "<path>");
        yield return R(@"\\host\Users\<user>", "<path>");
        yield return R(@"\u0027C:\\Users\\<user>\\x.cs\u0027", @"\u0027x.cs\u0027");
        yield return R(@"\\\\host\\Users\\<user>\\x.cs", "x.cs");
        yield return R("C:/Users/<user>/x.cs", "x.cs");
    }

    public static IEnumerable<object[]> MissedRootsPending() => MissedRoots().Take(11);

    public static IEnumerable<object[]> MissedRootsCharacterisation() => MissedRoots().Skip(11);

    // Rows from the card 426 second review (findings 1 to 3 and 7): a path after a JSON escape must not leak, an account folder must not swallow a drive letter,
    // and a one-letter URL host is not a Git Bash drive.
    public static IEnumerable<object[]> SecondReviewRows()
    {
        yield return R(@"copy /tmp/a.cs to \u0027/home/<user>/b.cs\u0027", @"copy a.cs to \u0027b.cs\u0027");
        yield return R(@"\u0027/home/<user>/x.cs\u0027", @"\u0027x.cs\u0027");
        yield return R(@"\u0027/home/<user>\u0027", @"\u0027<path>\u0027");
        yield return R(@"\u0027/mnt/c/Users/<user>/x.cs\u0027", @"\u0027x.cs\u0027");
        yield return R(@"\u0027\\host\Users\<user>\x.cs\u0027", @"\u0027x.cs\u0027");
        yield return R(@"moved C:\Users\<user> to D:\Users\<user>\x.cs", "moved <path> x.cs");
        yield return R("/c/users/<user>/x.cs", "x.cs");
        yield return R("see https://a/" + "Users/x ok", "see https://a/" + "Users/x ok");
    }

    // Card 435 criterion 1: glued, unlisted and UNC roots. The seven failing rows first, then the twelve
    // characterisation rows (nine that pass today plus the three escape-lookbehind mutant rows).
    public static IEnumerable<object[]> GluedAndUncRoots()
    {
        var user = "<user>";
        yield return R("gcc /tmp/a.c -I /usr/include -I/" + "home/" + user + "/include", "gcc a.c -I /usr/include -Iinclude");
        yield return R("in /tmp/x.cs at /opt/data/" + "home/" + user + "/x.cs", "in x.cs at x.cs");
        yield return R("Restoring /tmp/x.csproj for /cygdrive/c/" + "Users/" + user + "/x.csproj", "Restoring x.csproj for x.csproj");
        yield return R("/cygdrive/d/" + "Users/" + user, "<path>");
        yield return R(@"\\host\c$\" + "Users\\" + user, "<path>");
        yield return R(@"\\wsl$\Ubuntu\" + "home\\" + user, "<path>");
        yield return R(@"\\\\host\\c$\\" + "Users\\\\" + user, "<path>");
        yield return R("/" + "Users/" + user + "/x.cs", "x.cs");
        yield return R("/" + "Users/" + user, "<path>");
        yield return R("/mnt/d/" + "Users/" + user + "/x.cs", "x.cs");
        yield return R("/d/" + "Users/" + user, "<path>");
        yield return R("c:" + "\\documents and settings\\" + user, "<path>");
        yield return R(@"\\host\share\x.cs", "x.cs");
        yield return R("https://example.com/" + "Users/x", "https://example.com/" + "Users/x");
        yield return R("https://example.com/" + "home/x", "https://example.com/" + "home/x");
        yield return R("/opt/data/x.cs", "/opt/data/x.cs");
        yield return R(@"id u0041C:\a\x.cs", @"id u0041C:\a\x.cs");
        yield return R(@"id u0041\\host\share\x.cs", @"id u0041\\host\share\x.cs");
        yield return R("id u0041/tmp/a/x.cs", "id u0041/tmp/a/x.cs");
    }

    public static IEnumerable<object[]> GluedAndUncRootsPending() => GluedAndUncRoots().Take(7);

    public static IEnumerable<object[]> GluedAndUncRootsCharacterisation() => GluedAndUncRoots().Skip(7);

    // Card 435 criterion 2: account folders that are not plain ASCII. Each row carries a declared
    // fails-today flag (21 fail, 28 pass). Folders are built from parts at run time.
    public static IEnumerable<object[]> NonAsciiAccountFolders()
    {
        var realLetter = "aa" + "\u00F6" + "bb";
        var escaped = "aa" + @"\u00f6" + "bb";
        var gluedParens = "aa(" + "bb)";
        var spacedParens = "aa (" + "bb)";
        var dotted = "aa " + "bb.cc";
        var spacedAccent = "aa " + "\u00F6" + "cc";
        var folders = new[] { realLetter, escaped, gluedParens, spacedParens, dotted, spacedAccent };
        var prefixes = new (string Prefix, string Suffix)[]
        {
            ("C:" + "\\Users\\", ""),
            ("C:" + "\\\\Users\\\\", ""),
            ("\\u0027C:" + "\\\\Users\\\\", "\\u0027"),
            ("/" + "home/", ""),
        };

        foreach (var folder in folders)
        {
            var isRealLetter = folder == realLetter;
            var isEscaped = folder == escaped;
            foreach (var (prefix, suffix) in prefixes)
            {
                var quoted = suffix.Length > 0;
                yield return F(prefix + folder + suffix, quoted ? "\\u0027<path>\\u0027" : "<path>", !isRealLetter);
                var fileFails = !isRealLetter && quoted && isEscaped;
                yield return F(prefix + folder + @"\x.cs" + suffix, quoted ? "\\u0027x.cs\\u0027" : "x.cs", fileFails);
            }
        }

        yield return F("see C:" + "\\Users\\" + "<user> bb.", "see <path>.", false);
    }

    public static IEnumerable<object[]> NonAsciiAccountFoldersPending() =>
        NonAsciiAccountFolders().Where(row => (bool)row[2]).Select(row => R((string)row[0], (string)row[1]));

    public static IEnumerable<object[]> NonAsciiAccountFoldersCharacterisation() =>
        NonAsciiAccountFolders().Where(row => !(bool)row[2]).Select(row => R((string)row[0], (string)row[1]));

    // Card 435 criterion 3: escapes after a path. The five failing rows first, then the two-quote
    // text and the empty string (characterisation). No doubled-backslash regex row (ruling 6).
    public static IEnumerable<object[]> EscapesAfterAPath()
    {
        var user = "<user>";
        yield return R(@"C:\\a\\x.cs\nnext", @"x.cs\nnext");
        yield return R(@"/tmp/a/x.cs\tnext", @"x.cs\tnext");
        yield return R(@"see C:\\a\\x.cs\u0022", @"see x.cs\u0022");
        yield return R(@"\""C:\\a\\x.cs\""", @"\""x.cs\""");
        yield return R(@"C:\\a\\x.cs\n/" + "home/" + user + "/y.cs", @"x.cs\ny.cs");
        yield return R("\"\"", "\"\"");
        yield return R(string.Empty, string.Empty);
    }

    public static IEnumerable<object[]> EscapesAfterAPathPending() => EscapesAfterAPath().Take(5);

    public static IEnumerable<object[]> EscapesAfterAPathCharacterisation() => EscapesAfterAPath().Skip(5);

    [Theory]
    [MemberData(nameof(OddAccountFolders))]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-U")]
    public void Id_418_U5_an_account_folder_with_a_space_or_an_apostrophe_leaves_no_fragment(string input, string expected)
    {
        // Given an account folder with a space or an apostrophe, at the end of a path or alone
        // When PathScrub.Apply runs
        // Then no part of the folder name survives (the short name, or <path> for the folder itself)
        Assert.Equal(expected, Apply(input));
    }

    [Theory]
    [MemberData(nameof(Forms))]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-U")]
    public void Id_418_U1_each_rooted_form_becomes_the_short_name(string input, string expected)
    {
        // Given a string that holds one of the rooted path forms the scrub must match
        // When PathScrub.Apply runs
        // Then the absolute path is the file's short name, or <path> when there is no file segment
        Assert.Equal(expected, Apply(input));
    }

    [Theory]
    [MemberData(nameof(Untouched))]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-U")]
    public void Id_418_U2_must_not_touch_rows_stay_byte_identical(string input, string expected)
    {
        // Given a compiler message, a relative name, a URL, a tag, or a label with no rooted path
        // When PathScrub.Apply runs
        // Then the text is unchanged
        Assert.Equal(expected, Apply(input));
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-U")]
    public void Id_418_U2_apply_is_idempotent_on_every_form()
    {
        // Given every form in 418-U1, the words-around-a-path rows, and both forms of the missed-root rows
        // When Apply runs twice
        // Then the second pass equals the first
        foreach (var row in Forms().Concat(WordsAroundAPath()))
        {
            var input = (string)row[0];
            var once = Apply(input);
            Assert.Equal(once, Apply(once));
        }

        foreach (var row in MissedRoots())
        {
            foreach (var text in BothAccountFolderForms((string)row[0]))
            {
                var once = Apply(text);
                Assert.Equal(once, Apply(once));
            }
        }
    }

    [Fact]
    [Trait("Slice", "418")]
    [Trait("Criterion", "418-U")]
    public void Id_418_U4_csharp_tool_links_the_shared_file_and_defines_no_scrub_class()
    {
        // Given the C# baseline project file and its Program.cs
        var csproj = File.ReadAllText(RepoPaths.Find("tools", "Avowry.BenchCs", "Avowry.BenchCs.csproj"));
        var program = File.ReadAllText(RepoPaths.Find("tools", "Avowry.BenchCs", "Program.cs"));

        // When the link and the program text are read
        // Then the project compiles the shared PathScrub.cs as a link, and Program.cs defines no scrub class of its own
        Assert.Contains(@"Include=""..\Avowry.Bench\PathScrub.cs""", csproj, StringComparison.Ordinal);
        Assert.Contains(@"Link=""PathScrub.cs""", csproj, StringComparison.Ordinal);
        Assert.DoesNotContain("class PathScrub", program, StringComparison.Ordinal);
        Assert.DoesNotContain("static class PathScrub", program, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(WordsAroundAPathPending))]
    [Trait("Slice", "426")]
    [Trait("Criterion", "426-1")]
    public void Id_426_1_the_words_around_a_path_stay(string input, string expected)
    {
        // Given a stored diagnostic holding a text with words around a rooted path
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed (the words around the path stay)
        Assert.Equal(expected, Apply(input));
    }

    [Theory]
    [MemberData(nameof(WordsAroundAPathCharacterisation))]
    [Trait("Slice", "426")]
    [Trait("Criterion", "426-1")]
    public void Id_426_1_rows_that_pass_today(string input, string expected)
    {
        // Given a stored diagnostic holding a text that already keeps the words around a path
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed (characterisation: these rows pass on the current tree)
        Assert.Equal(expected, Apply(input));
    }

    [Theory]
    [MemberData(nameof(MissedRootsPending))]
    [Trait("Slice", "426")]
    [Trait("Criterion", "426-2")]
    public void Id_426_2_no_account_folder_survives_in_the_missed_forms(string input, string expected)
    {
        // Given each missed-root text, written once with the <user> placeholder and once with a plain-letter folder built from parts
        // When PathScrub.Apply runs
        // Then each row reads exactly as listed, neither the placeholder nor the plain folder appears, and a second pass changes nothing
        AssertMissedRoot(input, expected);
    }

    [Theory]
    [MemberData(nameof(MissedRootsCharacterisation))]
    [Trait("Slice", "426")]
    [Trait("Criterion", "426-2")]
    public void Id_426_2_drive_slash_users_row_passes_today(string input, string expected)
    {
        // Given the C:/Users missed-root text, written once with the <user> placeholder and once with a plain-letter folder built from parts
        // When PathScrub.Apply runs
        // Then each row reads exactly as listed, neither the placeholder nor the plain folder appears, and a second pass changes nothing (characterisation: this row passes on the current tree)
        AssertMissedRoot(input, expected);
    }

    [Theory]
    [MemberData(nameof(SecondReviewRows))]
    [Trait("Slice", "426")]
    [Trait("Criterion", "426-4")]
    public void Id_426_4_second_review_rows(string input, string expected)
    {
        // Given each text of the second review, written once with the <user> placeholder and once with a plain-letter folder built from parts
        // When PathScrub.Apply runs
        // Then each row reads exactly as listed, neither the placeholder nor the plain folder appears, and a second pass changes nothing
        AssertMissedRoot(input, expected);
    }

    [Theory]
    [MemberData(nameof(GluedAndUncRootsPending))]
    [Trait("Slice", "435")]
    [Trait("Criterion", "435-1")]
    public void Id_435_1_glued_unlisted_and_unc_roots(string input, string expected)
    {
        // Given a stored diagnostic holding a glued flag, an unlisted Unix root, a Cygwin root or a UNC account folder
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing
        AssertMissedRoot(input, expected);
    }

    [Theory]
    [MemberData(nameof(GluedAndUncRootsCharacterisation))]
    [Trait("Slice", "435")]
    [Trait("Criterion", "435-1")]
    public void Id_435_1_rows_that_pass_today(string input, string expected)
    {
        // Given a stored diagnostic holding a rooted form that already scrubs, a URL, or an escape-lookbehind mutant row
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing (characterisation: these rows pass on the current tree)
        AssertMissedRoot(input, expected);
    }

    [Theory]
    [MemberData(nameof(NonAsciiAccountFoldersPending))]
    [Trait("Slice", "435")]
    [Trait("Criterion", "435-2")]
    public void Id_435_2_non_ascii_account_folders(string input, string expected)
    {
        // Given an account folder with a non-ASCII letter, a JSON escape, parentheses or a dotted tail, after each listed prefix
        // When PathScrub.Apply runs
        // Then the folder alone is <path> (quoted as \u0027<path>\u0027), a file is x.cs, no half of the stand-in survives, and a second pass changes nothing
        AssertNonAsciiFolder(input, expected);
    }

    [Theory]
    [MemberData(nameof(NonAsciiAccountFoldersCharacterisation))]
    [Trait("Slice", "435")]
    [Trait("Criterion", "435-2")]
    public void Id_435_2_rows_that_pass_today(string input, string expected)
    {
        // Given a plain accented account folder, a folder followed by a file that already scrubs, or the sentence-ending dot row
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, none of aa, bb or cc appears, and a second pass changes nothing (characterisation: these rows pass on the current tree)
        AssertNonAsciiFolder(input, expected);
    }

    [Theory]
    [MemberData(nameof(EscapesAfterAPathPending))]
    [Trait("Slice", "435")]
    [Trait("Criterion", "435-3")]
    public void Id_435_3_escapes_after_a_path(string input, string expected)
    {
        // Given a stored diagnostic holding a path followed by a JSON or C escape
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing
        AssertMissedRoot(input, expected);
    }

    [Theory]
    [MemberData(nameof(EscapesAfterAPathCharacterisation))]
    [Trait("Slice", "435")]
    [Trait("Criterion", "435-3")]
    public void Id_435_3_rows_that_pass_today(string input, string expected)
    {
        // Given the two-quote text or the empty string
        // When PathScrub.Apply runs
        // Then the text is unchanged (characterisation: these rows pass on the current tree)
        AssertMissedRoot(input, expected);
    }

    private static object[] R(string input, string expected) => [input, expected];

    private static string PlainLetterFolder() => "aa" + "bb";

    private static IEnumerable<string> BothAccountFolderForms(string text)
    {
        var plain = PlainLetterFolder();
        yield return text;
        yield return text.Replace("<user>", plain);
    }

    private static void AssertMissedRoot(string input, string expected)
    {
        var plain = PlainLetterFolder();
        foreach (var text in BothAccountFolderForms(input))
        {
            var once = Apply(text);
            Assert.Equal(expected, once);
            Assert.DoesNotContain("<user>", once, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(plain, once, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(once, Apply(once));
        }
    }

    private static object[] F(string input, string expected, bool failsToday) => [input, expected, failsToday];

    private static void AssertNonAsciiFolder(string input, string expected)
    {
        foreach (var text in BothAccountFolderForms(input))
        {
            var once = Apply(text);
            Assert.Equal(expected, once);
            Assert.DoesNotContain("aa", once, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("bb", once, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("cc", once, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<user>", once, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(once, Apply(once));
        }
    }

    private static string Apply(string text)
    {
        var type = typeof(BenchmarkResults).Assembly.GetType("PathScrub");
        Assert.True(type is not null, "PathScrub is missing from the Phrase tool assembly.");
        var method = type!.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static);
        Assert.True(method is not null, "PathScrub.Apply is missing.");
        var result = method!.Invoke(null, [text]);
        Assert.True(result is string, "PathScrub.Apply did not return a string.");
        return (string)result!;
    }
}
