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
        // Given every form in 418-U1
        // When Apply runs twice
        // Then the second pass equals the first
        foreach (var row in Forms())
        {
            var input = (string)row[0];
            var once = Apply(input);
            Assert.Equal(once, Apply(once));
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

    private static object[] R(string input, string expected) => [input, expected];

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
