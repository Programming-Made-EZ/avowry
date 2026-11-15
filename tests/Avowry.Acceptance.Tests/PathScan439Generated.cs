using Xunit.Abstractions;

namespace Avowry.Acceptance.Tests;

// Card 439, D256: card 435's generator extended with the review inputs as shapes. Sweep A is card 435's own test
// (PathScrubGenerated, unchanged). Sweep B extends the lists with pieces taken from the review inputs (seed 439); sweep C uses
// every review input as a template (seed 4391). Per ruling 2 of the slice: 0 new leaks against PathScrubBefore435 and 0
// second-pass changes; the absolute leak counts are printed (all leaked before too: a root glued to a word, and the
// "bb:" limit), and no exclusion is committed. No wall-clock assertion (D254).
public sealed class PathScan439Generated
{
    private const int Inputs = 100_000;

    private readonly ITestOutputHelper output;

    public PathScan439Generated(ITestOutputHelper output) => this.output = output;

    private static readonly string[] Tokens =
    [
        "<user>",
        "aa" + "bb",
        "aa" + "\u00F6" + "bb",
        "aa" + @"\u00f6" + "bb",
        "aa(" + "bb)",
        "aa (" + "bb)",
        "aa " + "bb.cc",
        "aa " + "\u00F6" + "cc",
    ];

    private static readonly string[] Befores =
    [
        "", "see ", "-I", @"\u0027", @"\n", @"\t", @"\""", "(", "'", "in /tmp/x.cs at ", "moved ", "[",
    ];

    private static readonly (string Root, string Sep)[] Roots =
    [
        (Style("C:|Users|", @"\"), @"\"),
        (Style("C:|Users|", @"\\"), @"\\"),
        (Style("C:/Users|", "/"), "/"),
        (Style("file:///C:/Users|", "/"), "/"),
        (Style("c:|documents and settings|", @"\"), @"\"),
        (Style("C:|Documents and Settings|", @"\\"), @"\\"),
        (Style("||host|Users|", @"\"), @"\"),
        (Style("||host|c$|Users|", @"\"), @"\"),
        (Style("||host|c$|Users|", @"\\"), @"\\"),
        (Style("||wsl$|Ubuntu|home|", @"\"), @"\"),
        (Style("||wsl$|Ubuntu|home|", @"\\"), @"\\"),
        (Style("/home|", "/"), "/"),
        (Style("/Users|", "/"), "/"),
        (Style("/mnt/d/Users|", "/"), "/"),
        (Style("/d/Users|", "/"), "/"),
        (Style("/c/users|", "/"), "/"),
        (Style("/cygdrive/c/Users|", "/"), "/"),
        (Style("/opt/data/home|", "/"), "/"),
        (Style("file://localhost/home|", "/"), "/"),
    ];

    private static readonly string[] Folders = ["Temp", "My Projects", "src", "Sub Folder", "x.y"];

    private static readonly string[] Files = ["", "x.cs", "Candidate.cs(7,1)", "y.csproj"];

    private static readonly string[] Afters =
    [
        "", " end", ".", ", then", @"\u0027", @"\u0022", @"\nnext", @"\tnext", @"\""", " zz.", @" to D:\x.cs", "]",
        "' is denied.", " (line 3)", @"\n/" + "home/{token}/y.cs",
    ];

    // Extension for sweep B: pieces taken from the card 435 second and third review inputs.
    private static readonly string[] ExtraBefores =
    [
        "-I/tmp/a.", @"\t/tmp/a.", @"\u00f6/tmp/a\u00f6", @"\\", @"\/tmp/a/n", @"ld /tmp/a.c\n-I", @"ld /tmp/a.c\u0027-I",
        @"\u0027-I", "\"-I", "[-I", "-Wall,-I", @"C:\\a\\x.cs\b", @"C:\\a\\x.cs\f", @"C:\\a\\x.cs\r", @"\\host\share\x.cs;",
        @"[""\\host\share\x.cs"",""", @"C:\Users\{token}", @"C:\Users\{token}(1)", @"C:\Users\{token}(pp)qq", "see (",
        @"C:\a\x.cs\n", "ld /tmp/a.c -I", "see /tmp/x.cs (", @"\u0027-IC:\a\x.cs\u0027", @"C:\\a\\b.",
    ];

    private static readonly (string Root, string Sep)[] ExtraRoots =
    [
        (Style("||<host>|Users|", @"\"), @"\"),
        (Style("||host(1)|Users|", @"\"), @"\"),
        (Style("||host's|Users|", @"\"), @"\"),
        (Style(@"||h\u00f6st|Users|", @"\"), @"\"),
        (Style(@"||h\u00f6st|Users|", @"\\"), @"\\"),
        (Style("||host|c$|Documents and Settings|", @"\"), @"\"),
        (Style("||C:|Users|", @"\"), @"\"),
        (Style("||C:|Users|", @"\\"), @"\\"),
        (Style("/mnt/e/users|", "/"), "/"),
    ];

    private static readonly string[] ExtraAfters =
    [
        @"D:\Users\{token}\x.cs", @"_D:\Users\{token}\x.cs", ".file://localhost/home/{token}/x.cs", "(pp)",
        @"(pp)qqD:\Users\{token}\x.cs", @"(1)-D:\Users\{token}\x.cs", ")", "(pp))", @"\n~/y.cs", @"~\nx.cs",
        @"\f/home/{token}/y.cs", @"\b/home/{token}/y.cs", @"\r/home/{token}/y.cs", @"\u0027/home/{token}",
        @"\n-I/home/{token}/include", @"-D:\x.cs", " pp: denied", @"\\/home/{token}\nx.cs", @"\n/home/{token}",
        " -I/home/{token}/include", @"\u0027,\u0027/home/{token}/y.cs\u0027", @""",""C:\Users\{token}\y.cs""]",
        @";C:\Users\{token}\y.cs", @"\u00f6C:\\Users\\{token}", @".C:\Users\{token}", @"(\u00f6pp)", "(pp qq)",
        @" pp.\u00f6qq",
    ];

    [Fact]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    public void Id_439_2_sweep_B_extended_lists_add_no_leak_and_settle_in_one_pass()
    {
        // Given 100,000 strings from card 435's shapes extended with pieces of the review inputs, SplitMix64 seed 439
        // When PathScrub.Apply runs once and then again, and the base copy runs once
        // Then no input leaks where the base copy did not, a second pass changes nothing, and the leaks stay at or under the reference design's measured count; the counts are printed
        Sweep("B extended lists seed 439", 439UL, extended: true, reviewShapes: false, maxLeaks: 4559);
    }

    [Fact]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    public void Id_439_2_sweep_C_review_shapes_add_no_leak_and_settle_in_one_pass()
    {
        // Given 100,000 strings that wrap every review input as a template (the account folder becomes the token), seed 4391
        // When PathScrub.Apply runs once and then again, and the base copy runs once
        // Then no input leaks where the base copy did not, a second pass changes nothing, and the leaks stay at or under the reference design's measured count; the counts are printed
        Sweep("C review shapes seed 4391", 4391UL, extended: false, reviewShapes: true, maxLeaks: 2679);
    }

    private void Sweep(string label, ulong seed, bool extended, bool reviewShapes, int maxLeaks)
    {
        var befores = extended ? Befores.Concat(ExtraBefores).ToArray() : Befores;
        var roots = extended ? Roots.Concat(ExtraRoots).ToArray() : Roots;
        var afters = extended ? Afters.Concat(ExtraAfters).ToArray() : Afters;
        var templates = PathScan439.ReviewInputs
            .Select(t => t.Replace(" access denied", " denied", StringComparison.Ordinal).Replace("gcc ", "ld ", StringComparison.Ordinal))
            .ToArray();

        var state = seed;
        var leaksBase = 0;
        var leaksNew = 0;
        var regressions = 0;
        var secondPass = 0;
        for (var i = 0; i < Inputs; i++)
        {
            string input;
            string token;
            if (reviewShapes)
            {
                var before = Befores[Pick(ref state, Befores.Length)];
                var template = templates[Pick(ref state, templates.Length)];
                token = Tokens[Pick(ref state, Tokens.Length)];
                var after = Afters[Pick(ref state, Afters.Length)].Replace("{token}", token, StringComparison.Ordinal);
                input = before + template.Replace("<user>", token, StringComparison.Ordinal) + after;
            }
            else
            {
                var before = befores[Pick(ref state, befores.Length)];
                var (root, sep) = roots[Pick(ref state, roots.Length)];
                token = Tokens[Pick(ref state, Tokens.Length)];
                var folderCount = Pick(ref state, 4);
                var path = root + token;
                for (var n = 0; n < folderCount; n++)
                    path += sep + Folders[Pick(ref state, Folders.Length)];
                var file = Files[Pick(ref state, Files.Length)];
                if (file.Length > 0)
                    path += sep + file;
                var after = afters[Pick(ref state, afters.Length)].Replace("{token}", token, StringComparison.Ordinal);
                input = before.Replace("{token}", token, StringComparison.Ordinal) + path + after;
            }

            var baseOut = PathScrubBefore435.Apply(input);
            var newOut = PathScrub.Apply(input);
            var leakBase = HoldsTokenOrHalf(baseOut, token);
            var leakNew = HoldsTokenOrHalf(newOut, token);
            if (leakBase)
                leaksBase++;
            if (leakNew)
                leaksNew++;
            if (leakNew && !leakBase)
                regressions++;
            if (PathScrub.Apply(newOut) != newOut)
                secondPass++;
        }

        var counts = $"sweep={label} inputs={Inputs} leaks_base={leaksBase} leaks_new={leaksNew} regressions={regressions} second_pass={secondPass} exclusions=0";
        output.WriteLine(counts);
        Assert.True(regressions == 0 && secondPass == 0 && leaksNew <= maxLeaks, counts + $" max_leaks={maxLeaks}");
    }

    private static string Style(string template, string sep) => template.Replace("|", sep, StringComparison.Ordinal);

    private static int Pick(ref ulong state, int length) => (int)(Next(ref state) % (ulong)length);

    private static ulong Next(ref ulong state)
    {
        state += 0x9E3779B97F4A7C15UL;
        var z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    private static bool HoldsTokenOrHalf(string text, string token)
    {
        if (text.Contains(token, StringComparison.OrdinalIgnoreCase))
            return true;
        foreach (var half in new[] { "aa", "bb", "cc" })
        {
            if (token.Contains(half, StringComparison.OrdinalIgnoreCase)
                && text.Contains(half, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
