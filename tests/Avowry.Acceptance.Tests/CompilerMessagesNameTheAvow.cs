using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: Compiler source literals name the avow (card 324 criterion 3)
public class CompilerMessagesNameTheAvow
{
    private const string KeywordsPath = "src/Avowry.Compiler/ExtensionKeywords.cs";

    [Fact]
    [Trait("Criterion", "3")]
    public void A_planted_old_name_is_found_with_its_file_and_line()
    {
        // Given a planted tree with every C# string kind, plus ignored hole, comment, char, obj and bin copies
        using var workspace = TempWorkspace.Create();
        Plant(workspace.Root, "src/Foo.cs", string.Join('\n',
        [
            "class Foo",
            "{",
            """    string Regular = "Phrase one";""",
            """    string Interpolated = $"Phrase two {x}";""",
            """    string Hole = $"{phrase.Template}";""",
            """    string Verbatim = @"phrase three";""",
            """"    string Raw = """Phrase four""";"""",
            "    // Phrase in a comment",
            "    /* Phrase in a block comment */",
            "    char C = 'P';",
            "}",
        ]));
        const string stale = """string X = "Phrase";""";
        foreach (var relative in new[] { "obj/Stale.cs", "bin/Stale.cs", "src/obj/Stale.cs", "src/bin/Stale.cs" })
            Plant(workspace.Root, relative, stale);

        // When the tree is enumerated and scanned
        var result = CSharpLiteralScanner.Scan(CSharpLiteralScanner.TrackedSourceFiles(workspace.Root));

        // Then only the four planted literals are reported, at their file and line
        (string File, int Line)[] expected = [("src/Foo.cs", 3), ("src/Foo.cs", 4), ("src/Foo.cs", 6), ("src/Foo.cs", 7)];
        Assert.Equal(expected, result.Violations.Select(hit => (hit.File, hit.Line)));
    }

    [Fact]
    [Trait("Criterion", "3")]
    public void The_registry_keys_in_ExtensionKeywords_are_the_only_named_exceptions()
    {
        // Given ExtensionKeywords.cs with the six pinned literals
        var six = string.Join('\n',
        [
            """    var a = "phrase-steps";""",
            """    var b = "phrase";""",
            """    var c = "phrase";""",
            """    var d = "phrase";""",
            """    var e = "phrase";""",
            """    var f = "phrase";""",
        ]);

        // When those six are scanned
        var pinned = CSharpLiteralScanner.Scan([(KeywordsPath, six)]);

        // Then they match the pins and produce no violations
        Assert.Empty(pinned.Violations);
        Assert.Equal(6, pinned.ExceptionsMatched);

        // Given a seventh phrase in that file and the same literal in Resolver.cs
        var extra = CSharpLiteralScanner.Scan(
        [
            (KeywordsPath, six + "\n" + """    var g = "phrase";"""),
            ("src/Avowry.Compiler/Semantics/Resolver.cs", """    var x = "phrase";"""),
        ]);

        // Then each extra occurrence is a violation
        Assert.Equal(6, extra.ExceptionsMatched);
        Assert.Equal(2, extra.Violations.Count);
        Assert.Contains(extra.Violations, hit => hit.File == KeywordsPath && hit.Text == "phrase");
        Assert.Contains(extra.Violations, hit => hit.File == "src/Avowry.Compiler/Semantics/Resolver.cs" && hit.Text == "phrase");
    }

    [Fact]
    [Trait("Criterion", "3")]
    public void No_message_in_src_names_the_old_unit()
    {
        // Given the working-tree src
        var result = CSharpLiteralScanner.Scan(CSharpLiteralScanner.TrackedSourceFiles(CSharpLiteralScanner.RepoRoot()));

        // Then only the six pinned registry keys remain
        Assert.True(result.FilesScanned >= 15, $"FilesScanned={result.FilesScanned}");
        Assert.Equal(6, result.ExceptionsMatched);
        Assert.True(
            result.Violations.Count == 0,
            string.Join("; ", result.Violations.Select(hit => $"{Path.GetFileName(hit.File)}:{hit.Line} {hit.Text}")));
    }

    [Fact]
    [Trait("Criterion", "3")]
    public async Task Build_prints_the_avow_wording_for_an_unmatched_step()
    {
        // Given the unmatched-step source in a fresh output folder
        using var workspace = TempWorkspace.Create();
        var sourcePath = Path.Combine(workspace.Root, "probe.avowry");
        await File.WriteAllTextAsync(sourcePath, """
            module Test

            avow to "do nothing"
              native csharp
                return;

            endpoint Probe : POST /probe
              input
                n: Int
              steps
                frobnicate widgets
            """);

        // When avowry build runs
        var result = await PhraseCli.RunAsync("build", sourcePath, "-o", Path.Combine(workspace.Root, "out"));

        // Then stderr carries the avow wording and the process exits 1
        Assert.Equal(1, result.ExitCode);
        var lines = result.Stderr.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n').Split('\n');
        Assert.Equal("""PH001 (11:5) No avow matches this step. Closest: "do nothing".""", lines[1]);
    }

    private static void Plant(string root, string relative, string text)
    {
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }
}
