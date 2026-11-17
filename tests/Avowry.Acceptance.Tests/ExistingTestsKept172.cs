using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: card 172 removes and renames no test and leaves the generated C# alone
//   As Morgan, I want the existing tests and the generated output to stay as they were,
//   so that tidying the helpers cannot hide a lost check.
// Card 172 criterion 3. ExistingTestNames172.txt lists every test method (Fact or Theory) of both test projects
// as it stood before the card, one "relative/path.cs::Method" per line. These are guards: they pass today and must keep passing.
public class ExistingTestsKept172
{
    private static readonly Regex TestAttribute = new(@"^\s*\[(?:Fact|Theory)\b", RegexOptions.CultureInvariant);
    private static readonly Regex MethodDeclaration = new(@"\b(?:Task|void)\s+(\w+)\s*\(", RegexOptions.CultureInvariant);

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-3")]
    public void Every_test_that_existed_before_the_card_is_still_there_under_the_same_name_and_file()
    {
        // Given the list of test names recorded before the card
        var before = File.ReadAllLines(RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "ExistingTestNames172.txt"))
            .Where(line => line.Length > 0)
            .ToList();
        Assert.True(before.Count >= 600, $"The recorded list looks truncated ({before.Count} names).");

        // When the sources are scanned the same way now
        var now = CurrentNames();

        // Then none was removed, renamed or moved
        var lost = before.Where(name => !now.Contains(name)).ToList();
        Assert.True(lost.Count == 0, "These tests are gone or renamed:" + Environment.NewLine + string.Join(Environment.NewLine, lost));
    }

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-3")]
    public void The_generated_csharp_pins_are_unchanged()
    {
        // Given the Slice 142 orders pin and the Slice 176 demo golden
        var boundary = File.ReadAllText(RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "WebExtensionBoundary.cs"));
        var golden = RepoPaths.Find("tests", "Avowry.Acceptance.Tests", "TasksDemoGeneratedCsharp.golden");

        // Then the orders hash still starts ec3a552 and the golden file is still there (card 165 observer re-record)
        Assert.Contains("OrdersOutputHash = \"ec3a552", boundary, StringComparison.Ordinal);
        Assert.True(File.Exists(golden));
    }

    private static HashSet<string> CurrentNames()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var project in new[] { "Avowry.Acceptance.Tests", "Avowry.Compiler.Tests" })
        {
            var folder = RepoPaths.Find("tests", project);
            foreach (var file in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(RepoPaths.Root, file).Replace(Path.DirectorySeparatorChar, '/');
                if (relative.Split('/').Any(part => part is "bin" or "obj"))
                    continue;
                var pending = false;
                foreach (var line in File.ReadAllText(file).Split('\n'))
                {
                    if (TestAttribute.IsMatch(line))
                    {
                        pending = true;
                        continue;
                    }
                    if (pending && MethodDeclaration.Match(line) is { Success: true } match)
                    {
                        names.Add(relative + "::" + match.Groups[1].Value);
                        pending = false;
                    }
                }
            }
        }
        return names;
    }
}
