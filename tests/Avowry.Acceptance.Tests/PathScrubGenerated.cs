using Xunit.Abstractions;

namespace Avowry.Acceptance.Tests;

public sealed class PathScrubGenerated
{
    private const int Inputs = 100_000;
    private const int PinnedExclusionCount = 0;

    private static readonly (string Shape, Func<string, bool> Matches, string Reason)[] Exclusions = [];

    private readonly ITestOutputHelper output;

    public PathScrubGenerated(ITestOutputHelper output) => this.output = output;

    [Fact]
    [Trait("Slice", "435")]
    [Trait("Criterion", "435-3")]
    public void Id_435_3_base_copy_still_reproduces_three_known_leaks()
    {
        // Given the verbatim base copy of PathScrub at the card's base commit, renamed PathScrubBefore435
        var user = "<user>";
        // When PathScrubBefore435.Apply runs on three known leaking rows
        // Then it still reproduces those leaks, so a patched copy cannot quietly empty the comparison
        Assert.Equal(
            "gcc a.c -I /usr/include -I/" + "home/" + user + "/include",
            PathScrubBefore435.Apply("gcc /tmp/a.c -I /usr/include -I/" + "home/" + user + "/include"));
        Assert.Equal(
            "in x.cs at /opt/data/" + "home/" + user + "/x.cs",
            PathScrubBefore435.Apply("in /tmp/x.cs at /opt/data/" + "home/" + user + "/x.cs"));
        Assert.Equal(
            user,
            PathScrubBefore435.Apply(@"\\host\c$\" + "Users\\" + user));
    }

    [Fact]
    [Trait("Slice", "435")]
    [Trait("Criterion", "435-3")]
    [Trait("Status", "Pending")]
    // Pending until the green that changes tools/Avowry.Bench/PathScrub.cs
    public void Id_435_3_generated_inputs_leak_nothing_and_settle_in_one_pass()
    {
        // Given 100,000 strings built from the shapes of criteria 1 to 3 with SplitMix64 seed 435
        Assert.True(
            Exclusions.Length == PinnedExclusionCount,
            $"exclusions={Exclusions.Length} pin={PinnedExclusionCount}");

        var tokens = new[]
        {
            "<user>",
            "aa" + "bb",
            "aa" + "\u00F6" + "bb",
            "aa" + @"\u00f6" + "bb",
            "aa(" + "bb)",
            "aa (" + "bb)",
            "aa " + "bb.cc",
            "aa " + "\u00F6" + "cc",
        };
        var befores = new[]
        {
            "",
            "see ",
            "-I",
            @"\u0027",
            @"\n",
            @"\t",
            @"\""",
            "(",
            "'",
            "in /tmp/x.cs at ",
            "moved ",
            "[",
        };
        var roots = new (string Root, string Sep)[]
        {
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
        };
        var folders = new[] { "Temp", "My Projects", "src", "Sub Folder", "x.y" };
        var files = new[] { "", "x.cs", "Candidate.cs(7,1)", "y.csproj" };
        var afters = new[]
        {
            "",
            " end",
            ".",
            ", then",
            @"\u0027",
            @"\u0022",
            @"\nnext",
            @"\tnext",
            @"\""",
            " zz.",
            @" to D:\x.cs",
            "]",
            "' is denied.",
            " (line 3)",
            @"\n/" + "home/{token}/y.cs",
        };

        var state = 435UL;
        var exclusionMatches = new int[Exclusions.Length];
        var leaksBase = 0;
        var leaksNew = 0;
        var regressions = 0;
        var secondPass = 0;

        // When PathScrub.Apply runs once and then again, and the base copy runs once
        for (var i = 0; i < Inputs; i++)
        {
            var before = befores[Pick(ref state, befores.Length)];
            var (root, sep) = roots[Pick(ref state, roots.Length)];
            var token = tokens[Pick(ref state, tokens.Length)];
            var folderCount = Pick(ref state, 4);
            var path = root + token;
            for (var n = 0; n < folderCount; n++)
                path += sep + folders[Pick(ref state, folders.Length)];
            var file = files[Pick(ref state, files.Length)];
            if (file.Length > 0)
                path += sep + file;
            var after = afters[Pick(ref state, afters.Length)].Replace("{token}", token, StringComparison.Ordinal);
            var input = before + path + after;

            var excluded = false;
            for (var e = 0; e < Exclusions.Length; e++)
            {
                if (!Exclusions[e].Matches(input))
                    continue;
                exclusionMatches[e]++;
                excluded = true;
            }

            var baseOut = PathScrubBefore435.Apply(input);
            var newOut = PathScrub.Apply(input);
            var leakBase = HoldsTokenOrHalf(baseOut, token);
            var leakNew = HoldsTokenOrHalf(newOut, token);
            var twice = PathScrub.Apply(newOut);
            var changed = twice != newOut;
            if (excluded)
                continue;
            if (leakBase)
                leaksBase++;
            if (leakNew)
                leaksNew++;
            if (leakNew && !leakBase)
                regressions++;
            if (changed)
                secondPass++;
        }

        var counts =
            $"inputs={Inputs} leaks_base={leaksBase} leaks_new={leaksNew} regressions={regressions} second_pass={secondPass} exclusions={Exclusions.Length}";
        output.WriteLine(counts);
        for (var e = 0; e < Exclusions.Length; e++)
        {
            var (shape, _, reason) = Exclusions[e];
            output.WriteLine($"exclusion shape={shape} matched={exclusionMatches[e]} reason={reason}");
            Assert.True(exclusionMatches[e] > 0, $"exclusion {shape} matched no input ({reason})");
        }

        // Then the new output holds no planted token or half-token, no input leaks where the old scrub did not,
        // and a second pass changes nothing. Each exception is a committed exclusion naming card 435.
        Assert.True(leaksNew == 0 && regressions == 0 && secondPass == 0, counts);
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
