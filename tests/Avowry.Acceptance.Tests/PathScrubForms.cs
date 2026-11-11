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
    [Trait("Status", "Pending")]
    // Pending until the green that changes tools/Avowry.Bench/PathScrub.cs
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
    [Trait("Status", "Pending")]
    // Pending until the green that changes tools/Avowry.Bench/PathScrub.cs
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
