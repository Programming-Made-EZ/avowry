using System.Diagnostics;
using Xunit.Abstractions;

namespace Avowry.Acceptance.Tests;

// Card 439 red: the rows of the architect's slice (.mob/records/439/slice-architect.md) as tests.
// Criteria: 439-1 the BLOCKING inputs of card 435's third review; 439-2 every other review input (second and third review
// rows, MINOR findings, rows the rework already handles); 439-3 the blow-up shapes (measured at the slice, no wall-clock
// assertion, D254); 439-4 the mutant-killing rows; 439-5 is the review prompt (no test). Account names are the placeholder
// and a plain letter stand-in built at run time from the placeholder, as in PathScrubForms.
public sealed class PathScan439
{
    private readonly ITestOutputHelper output;

    public PathScan439(ITestOutputHelper output) => this.output = output;

    public static IEnumerable<object[]> ThirdReviewBlockingPending() => ThirdReviewBlockingPendingRows.Select(r => new object[] { r.Id, r.Input, r.Expected });

    public static IEnumerable<object[]> ThirdReviewBlockingCharacterisation() => ThirdReviewBlockingCharacterisationRows.Select(r => new object[] { r.Id, r.Input, r.Expected });

    public static IEnumerable<object[]> OtherReviewRowsPending() => OtherReviewRowsPendingRows.Select(r => new object[] { r.Id, r.Input, r.Expected });

    public static IEnumerable<object[]> OtherReviewRowsCharacterisation() => OtherReviewRowsCharacterisationRows.Select(r => new object[] { r.Id, r.Input, r.Expected });

    public static IEnumerable<object[]> MutantKillingRowsPending() => MutantKillingRowsPendingRows.Select(r => new object[] { r.Id, r.Input, r.Expected });

    public static IEnumerable<object[]> MutantKillingRowsCharacterisation() => MutantKillingRowsCharacterisationRows.Select(r => new object[] { r.Id, r.Input, r.Expected });

    [Theory]
    [MemberData(nameof(ThirdReviewBlockingPending))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-1")]
    public void Id_439_1_third_review_blocking_rows(string id, string input, string expected)
    {
        // Given a stored diagnostic of card 435's third review (a UNC host with angle brackets, parentheses or an apostrophe, an escaped quote and a glued flag, a glued root, a tilde after a hyphen, a root after an account folder)
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(ThirdReviewBlockingCharacterisation))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-1")]
    public void Id_439_1_third_review_blocking_rows_that_pass_today(string id, string input, string expected)
    {
        // Given a stored diagnostic of card 435's third review (a UNC host with angle brackets, parentheses or an apostrophe, an escaped quote and a glued flag, a glued root, a tilde after a hyphen, a root after an account folder) that the rework already handles
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing (characterisation: these rows pass on the current tree)
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(OtherReviewRowsPending))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    public void Id_439_2_other_review_rows(string id, string input, string expected)
    {
        // Given a stored diagnostic of card 435's second or third review (a MINOR finding, or a row the rework already fixed)
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(OtherReviewRowsCharacterisation))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    public void Id_439_2_other_review_rows_that_pass_today(string id, string input, string expected)
    {
        // Given a stored diagnostic of card 435's second or third review (a MINOR finding, or a row the rework already fixed)
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing (characterisation: these rows pass on the current tree)
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(MutantKillingRowsPending))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-4")]
    public void Id_439_4_mutant_killing_rows(string id, string input, string expected)
    {
        // Given the killing row of one mutant of the scanner (the row id names the mutant)
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(MutantKillingRowsCharacterisation))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-4")]
    public void Id_439_4_mutant_killing_rows_that_pass_today(string id, string input, string expected)
    {
        // Given the killing row of one mutant of the scanner (the row id names the mutant)
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, neither the placeholder nor the stand-in appears, and a second pass changes nothing (characterisation: these rows pass on the current tree; they kill a mutant of the green, not of today)
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    public static IEnumerable<object[]> BlowUpShapes()
    {
        yield return [@"\\;"];
        yield return [@"\\,"];
        yield return [@"\\a;"];
        yield return [@"\\\\host\\share\\x.cs\n"];
        yield return [@"\\C:/a\n"];
        yield return [@"\\C:\\a\n"];
        yield return [@"\\\\a$\n"];
        yield return ["/a"];
        yield return ["/-"];
    }

    [Theory]
    [MemberData(nameof(BlowUpShapes))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-3")]
    public void Id_439_3_blow_up_shapes_settle_in_one_pass(string unit)
    {
        // Given 20,000 characters of a blow-up shape named in the reviews
        var text = string.Concat(Enumerable.Repeat(unit, 20_000 / unit.Length));
        // When PathScrub.Apply runs, twice
        var clock = Stopwatch.StartNew();
        var once = PathScrub.Apply(text);
        clock.Stop();
        var twice = PathScrub.Apply(once);
        // Then a second pass changes nothing; the time is printed as evidence and never asserted (D254)
        output.WriteLine($"shape={unit} length={text.Length} first_pass_ms={clock.Elapsed.TotalMilliseconds:F1}");
        Assert.Equal(once, twice);
    }

    // Fix round 1 (review round 1, slice-fix.md): findings 4 to 8, the three new mutant rows, and the finding 1 blow-ups.
    // Each row runs with the placeholder, the plain stand-in and two stand-ins that start with an escape letter (n and r).
    private static readonly (string Id, string Input, string Expected)[] FixFindingRowsPending =
    [
        ("4a mixed-slash drive", @"C:/Users\<user>\x.cs", "x.cs"),
        ("4b mixed-slash file URL", @"file:///C:/Users\<user>\x.cs", "x.cs"),
        ("5a prefix folders before c/users", "/mnt/wsl/c/users/<user>/x.cs", "x.cs"),
        ("5b prefix folders before c/users", "/opt/cygdrive/c/users/<user>/x.cs", "x.cs"),
        ("5c prefix folder before c/users", "/a/c/users/<user>", "<path>"),
        ("7 JSON escaped regex before a path field", @"{""pattern"":""\\d+"",""path"":""C:\\Users\\<user>\\x.cs""}", @"{""pattern"":""\\d+"",""path"":""x.cs""}"),
        ("8a space before a bracket", @"C:\Users\<user> bb [Verification.csproj]", "<path> [Verification.csproj]"),
        ("8b space before a quote", @"C:\Users\<user> bb ""is denied""", @"<path> ""is denied"""),
        ("8c space before a dot", @"C:\\Users\\<user> .\nnext", @"<path> .\nnext"),
        ("6a bridged path in parentheses", "see C:/temp/x.cs (/home/<user>)", "see <path>)"),
        ("6b bridged path in parentheses", "see ~/a.cs (/home/<user>)", "see <path>)"),
    ];

    private static readonly (string Id, string Input, string Expected)[] FixMutantRowsPending =
    [
        ("drive-parent-guard", @"C:/Users\<user>\x.cs", "x.cs"),
        ("no-opener-bridge", "see C:/temp/x.cs (/home/<user>)", "see <path>)"),
    ];

    private static readonly (string Id, string Input, string Expected)[] FixMutantRowsCharacterisation =
    [
        ("glued-apos-first", @"C:\Users\<user>(bb)'\u00f6cc", "<path>"),
        ("unix-no-letter-users (new killing row)", "/tmp/a./c/users/<user>", "<path>"),
    ];

    public static IEnumerable<object[]> FixFindingsPending() => FixFindingRowsPending.Select(r => new object[] { r.Id, r.Input, r.Expected });

    public static IEnumerable<object[]> FixMutantsPending() => FixMutantRowsPending.Select(r => new object[] { r.Id, r.Input, r.Expected });

    public static IEnumerable<object[]> FixMutantsCharacterisation() => FixMutantRowsCharacterisation.Select(r => new object[] { r.Id, r.Input, r.Expected });

    [Theory]
    [MemberData(nameof(FixFindingsPending))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    public void Id_439_2_fix_round_findings_4_to_8(string id, string input, string expected)
    {
        // Given a stored diagnostic of review round 1 (mixed slashes after a drive, prefix folders before a drive-style users root, a JSON field, an MSBuild text after an account folder, a bridged path)
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed for every account form, none survives, and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertFixRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(FixMutantsPending))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-4")]
    public void Id_439_4_fix_round_mutant_killing_rows(string id, string input, string expected)
    {
        // Given the killing row of a mutant added by the fix slice (the row id names the mutant)
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed for every account form, none survives, and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertFixRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(FixMutantsCharacterisation))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-4")]
    public void Id_439_4_fix_round_mutant_killing_rows_that_pass_on_the_green(string id, string input, string expected)
    {
        // Given the killing row of a mutant added by the fix slice that the round 1 green already satisfies
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed for every account form, none survives, and a second pass changes nothing (characterisation)
        output.WriteLine($"row {id}");
        AssertFixRow(input, expected);
    }

    public static IEnumerable<object[]> FixBlowUpShapes()
    {
        yield return ["/=-I"];
        yield return ["/(-I"];
    }

    [Theory]
    [MemberData(nameof(FixBlowUpShapes))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-3")]
    public void Id_439_3_glued_flag_blow_up_shapes_stay_unchanged_and_settle(string unit)
    {
        // Given 20,000 characters of a glued flag after a failed Unix walk (review round 1, finding 1)
        var text = string.Concat(Enumerable.Repeat(unit, 20_000 / unit.Length));
        // When PathScrub.Apply runs, twice
        var clock = Stopwatch.StartNew();
        var once = PathScrub.Apply(text);
        clock.Stop();
        var twice = PathScrub.Apply(once);
        // Then the text holds no root and comes back unchanged (the reference's output), a second pass changes nothing; the time is printed as evidence and never asserted (D254)
        output.WriteLine($"shape={unit} length={text.Length} first_pass_ms={clock.Elapsed.TotalMilliseconds:F1}");
        Assert.Equal(text, once);
        Assert.Equal(once, twice);
    }

    private static void AssertFixRow(string input, string expected)
    {
        var standIns = new[] { "aa" + "bb", "nn" + "bb", "rr" + "bb" };
        foreach (var standIn in standIns)
        {
            var text = input.Replace("<user>", standIn, StringComparison.Ordinal);
            var once = PathScrub.Apply(text);
            Assert.Equal(expected, once);
            Assert.DoesNotContain("<user>", once, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(standIn, once, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(once, PathScrub.Apply(once));
        }

        var placeholderOnce = PathScrub.Apply(input);
        Assert.Equal(expected, placeholderOnce);
        Assert.Equal(placeholderOnce, PathScrub.Apply(placeholderOnce));
    }

    // Fix round 2 (review round 2, slice-fix2.md): Unix colon lists, the one-letter name before a colon-slash, the killing rows
    // of the seven round 2 survivors and the two new mutants, and the device paths. Expected outputs are the reference's
    // (the replaced regex's for the colon lists). Each row runs with the placeholder and the plain stand-in.
    private static readonly (string Id, string Input, string Expected)[] Fix2ColonRowsPending =
    [
        ("c01 docker volume spec", "docker run -v /home/<user>/work:/home/<user> -it ubuntu", "docker run -v work:<path>"),
        ("c02 PYTHONPATH", "PYTHONPATH=/home/<user>/lib:/home/<user>", "PYTHONPATH=lib:<path>"),
        ("c03 Git Bash PATH", "PATH=/c/Users/<user>/bin:/usr/bin:/c/Users/<user>", "PATH=bin:/usr/bin:<path>"),
        ("c04 GOPATH", "GOPATH=/home/<user>/go:/home/<user>", "GOPATH=go:<path>"),
        ("c05 NODE_PATH", "NODE_PATH=/home/<user>/node_modules:/home/<user>", "NODE_PATH=node_modules:<path>"),
        ("c06 CDPATH", "CDPATH=.:/home/<user>/src:/home/<user>", "CDPATH=.:src:<path>"),
        ("c07 CLASSPATH", "CLASSPATH=/home/<user>/lib/a.jar:/home/<user>/classes:/home/<user>", "CLASSPATH=a.jar:classes:<path>"),
        ("c08 java class path", "-Djava.class.path=/home/<user>/lib/a.jar:/home/<user>", "-Djava.class.path=a.jar:<path>"),
        ("c09 rsync change_dir", "rsync: change_dir \"/home/<user>/src:/home/<user>\" failed", "rsync: change_dir \"src:<path>\" failed"),
        ("c10 compose volumes", "volumes: [/home/<user>/data:/home/<user>]", "volumes: [data:<path>]"),
        ("c12 PATH with two homes", "PATH=/home/<user>/bin:/home/<user>/.local/bin:/usr/bin", "PATH=bin:bin:/usr/bin"),
        ("c13 LD_LIBRARY_PATH", "LD_LIBRARY_PATH=/home/<user>/lib:/home/<user>/.local/lib", "LD_LIBRARY_PATH=lib:lib"),
        ("c14 compose volume to root", "- /home/<user>/.config:/root/.config", "- .config:.config"),
        ("c15 tilde list", "~/bin:~/lib:/home/<user>", "bin:lib:<path>"),
        ("o1 one-letter name before colon-slash", "/tmp/x:/home/<user>", "x:<path>"),
        ("o2 one-letter name before colon-slash", "/home/<user>/a:/mnt/c/Users/<user>", "a:<path>"),
    ];

    private static readonly (string Id, string Input, string Expected)[] Fix2ColonRowsCharacterisation =
    [
        ("c16 Unix name then a drive", "/tmp/a:C:/Users/<user>/x.cs", "a:x.cs"),
    ];

    // c11 holds only the placeholder form: the plain stand-in ends in b, and the reference (like the regex) keeps one b there.
    private static readonly (string Id, string Input, string Expected)[] Fix2PlaceholderRowsCharacterisation =
    [
        ("c11 two homes", "/home/<user>:/home/<user>", "<path>:<path>"),
    ];

    private static readonly (string Id, string Input, string Expected)[] Fix2MutantRowsPending =
    [
        ("adjacency-any", "PATH=/home/<user>/bin:/home/<user>/.local/bin:/usr/bin", "PATH=bin:bin:/usr/bin"),
        ("final-start-root-stop", "/tmp/x:/home/<user>", "x:<path>"),
    ];

    private static readonly (string Id, string Input, string Expected)[] Fix2MutantRowsCharacterisation =
    [
        ("wordends-no-dash", @"C:\Users\<user> smith-jones", "<path>"),
        ("drive-parents-take-home", @"C:\home\" + "x.cs", "x.cs"),
        ("guard-letter-esc-gone", @"\\C:\u00e9x\y.cs", @"\\<path>"),
        ("unc-host-space-ok", @"\\ server\share\x.cs", @"\\ server\share\x.cs"),
        ("flagleft-no-semicolon", "CFLAGS=-O2;-I/home/<user>/inc", "CFLAGS=-O2;-Iinc"),
        ("root-ahead-tilde-gone", @"C:\a b -I~/x.cs", "a b -Ix.cs"),
        ("stopat-apos-gone", @"C:\Users\<user> x2'b", "<path> x2'b"),
    ];

    // Ruling 2: today's outputs for a device path before a drive are pinned (no leak, stable on a second pass).
    private static readonly (string Id, string Input, string Expected)[] Fix2DevicePathRows =
    [
        ("device path question mark", @"'\\?\C:\Users\<user>\x.cs'", @"'\\?\x.cs'"),
        ("device path dot", @"\\.\C:\Users\<user>\x.cs", "<path>x.cs"),
    ];

    private static IEnumerable<object[]> Rows(IEnumerable<(string Id, string Input, string Expected)> rows) =>
        rows.Select(r => new object[] { r.Id, r.Input, r.Expected });

    public static IEnumerable<object[]> Fix2ColonPending() => Rows(Fix2ColonRowsPending);

    public static IEnumerable<object[]> Fix2PlaceholderCharacterisation() => Rows(Fix2PlaceholderRowsCharacterisation);

    public static IEnumerable<object[]> Fix2MutantsPending() => Rows(Fix2MutantRowsPending);

    public static IEnumerable<object[]> Fix2ColonCharacterisation() => Rows(Fix2ColonRowsCharacterisation);

    public static IEnumerable<object[]> Fix2MutantsCharacterisation() => Rows(Fix2MutantRowsCharacterisation);

    public static IEnumerable<object[]> Fix2DevicePaths() => Rows(Fix2DevicePathRows);

    [Theory]
    [MemberData(nameof(Fix2ColonPending))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    [Trait("Status", "Pending")]
    // Pending until the card 439 fix-2 green that ports the reference scanner
    public void Id_439_2_fix2_colon_lists_and_one_letter_names(string id, string input, string expected)
    {
        // Given a Unix colon-separated list or a one-letter name before a colon-slash (review round 2, findings 1 to 3)
        // When PathScrub.Apply runs
        // Then the row reads exactly as the replaced regex gave it (the reference's output), no account name survives, and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(Fix2PlaceholderCharacterisation))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    public void Id_439_2_fix2_colon_list_with_two_homes_that_passes_on_the_port(string id, string input, string expected)
    {
        // Given two home folders joined by a colon (placeholder form only)
        // When PathScrub.Apply runs
        // Then both become the placeholder path and a second pass changes nothing
        output.WriteLine($"row {id}");
        var once = PathScrub.Apply(input);
        Assert.Equal(expected, once);
        Assert.Equal(once, PathScrub.Apply(once));
    }

    [Theory]
    [MemberData(nameof(Fix2MutantsPending))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-4")]
    [Trait("Status", "Pending")]
    // Pending until the card 439 fix-2 green that ports the reference scanner
    public void Id_439_4_fix2_mutant_killing_rows(string id, string input, string expected)
    {
        // Given the killing row of a round 2 survivor or of a mutant the second fix slice adds (the row id names the mutant)
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, no account name survives, and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(Fix2ColonCharacterisation))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    public void Id_439_2_fix2_colon_list_rows_that_pass_on_the_port(string id, string input, string expected)
    {
        // Given a colon list that the port of the first fixed reference already handles
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, no account name survives, and a second pass changes nothing (characterisation)
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(Fix2MutantsCharacterisation))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-4")]
    public void Id_439_4_fix2_mutant_killing_rows_that_pass_on_the_port(string id, string input, string expected)
    {
        // Given the killing row of a round 2 survivor, which the current port already satisfies
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed, no account name survives, and a second pass changes nothing (characterisation: it kills a mutant of the green, not of the port)
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(Fix2DevicePaths))]
    [Trait("Slice", "439")]
    [Trait("Criterion", "439-2")]
    public void Id_439_2_fix2_device_paths_before_a_drive_keep_their_outputs(string id, string input, string expected)
    {
        // Given a device path (the question mark or dot prefix) before a drive path
        // When PathScrub.Apply runs
        // Then the output is today's (no leak, stable on a second pass), pinned so a later change is deliberate (characterisation, ruling 2)
        output.WriteLine($"row {id}");
        AssertRow(input, expected);
    }

    private static string PlainLetterFolder() => "aa" + "bb";

    private static void AssertRow(string input, string expected)
    {
        var plain = PlainLetterFolder();
        foreach (var text in new[] { input, input.Replace("<user>", plain, StringComparison.Ordinal) }.Distinct())
        {
            var once = PathScrub.Apply(text);
            Assert.Equal(expected, once);
            Assert.DoesNotContain("<user>", once, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(plain, once, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(once, PathScrub.Apply(once));
        }
    }

    private static readonly (string Id, string Input, string Expected)[] ThirdReviewBlockingPendingRows =
    [
        ("3.1a", @"\\<host>\Users\<user>", @"<path>"),
        ("3.1b", @"\\<host>\Users\<user>\x.cs", @"x.cs"),
        ("3.1c", @"\\<host>\share\x.cs", @"x.cs"),
        ("3.2a", @"\\host(1)\Users\<user>", @"<path>"),
        ("3.2b", @"\\host's\Users\<user>", @"<path>"),
        ("3.2c", @"\\a<b>\Users\<user>", @"<path>"),
        ("3.3a", @"\u0027-IC:\a\x.cs\u0027/home/<user>", @"\u0027-Ix.cs\u0027<path>"),
        ("3.3b", @"\u0027-I\\host\share\x.cs\u0027/home/<user>/y.cs", @"\u0027-Ix.cs\u0027y.cs"),
        ("3.3c", @"gcc \u0027-IC:\Users\<user>\include\u0027", @"gcc \u0027-Iinclude\u0027"),
        ("3.3d", @"gcc \u0027-IC:\Users\<user>\u0027", @"gcc \u0027-I<path>\u0027"),
        ("3.3e", @"see a\u00f6-b/tmp/x.cs", @"see a\u00f6-b/tmp/x.cs"),
        ("3.4a", @"/tmp/a/foo-~\nx.cs", @"foo-~\nx.cs"),
        ("3.4b", @"/tmp/a/~~\nx.cs", @"~~\nx.cs"),
        ("3.4c", @"C:\\a\\foo-~\nx.cs", @"foo-~\nx.cs"),
        ("3.4e", @"C:\Users\<user>D:\Users\<user>\x.cs", @"<path>x.cs"),
        ("3.4f", @"C:\Users\<user>_D:\Users\<user>\x.cs", @"<path>x.cs"),
        ("3.4g", @"C:\Users\<user>.file://localhost/home/<user>/x.cs", @"<path>x.cs"),
        ("3.4h", @"\/tmp/a/nC:\Users\<user>", @"\n<path>"),
        ("3.4i", @"\t/tmp/a.file://localhost/home/<user>/x.cs", @"\ta.x.cs"),
        ("3.5a", @"see (C:\Users\<user>(bb))", @"see (<path>)"),
        ("3.5b", @"see (C:\Users\<user>(bb)) here", @"see (<path>) here"),
        ("3.5c", @"C:\Users\<user>(bb)ccfile:///C:/Users/<user>/x.cs", @"<path>x.cs"),
        ("3.5d", @"C:\Users\<user>(bb)ccD:\Users\<user>\x.cs", @"<path>x.cs"),
    ];

    private static readonly (string Id, string Input, string Expected)[] ThirdReviewBlockingCharacterisationRows =
    [
        ("3.5e", @"C:\Users\<user>(1)-D:\Users\<user>\x.cs", @"<path>-x.cs"),
        ("3.7a", @"gcc ""-I/home/<user>/include""", @"gcc ""-Iinclude"""),
        ("3.7b", @"gcc /tmp/a.c\t-I/home/<user>/include", @"gcc a.c\t-Iinclude"),
        ("3.7c", @"id u0041-I/tmp/a/x.cs", @"id u0041-I/tmp/a/x.cs"),
        ("3.7d", @"\t/tmp/a.d:\Users\<user>", @"\ta.<path>"),
        ("3.7e", @"\t/tmp/a.C:/Users/<user>", @"\ta.<path>"),
        ("3.7f", @"\\d:\\Users\\<user>\\x.cs", @"x.cs"),
        ("3.7g", @"\\\\host\\share\\x.cs\r/home/<user>/y.cs", @"x.cs\ry.cs"),
    ];

    private static readonly (string Id, string Input, string Expected)[] OtherReviewRowsPendingRows =
    [
        ("2.2i", @"[-I/home/<user>/include]", @"[-Iinclude]"),
        ("2.2j", @"(-I/home/<user>/include)", @"(-Iinclude)"),
        ("2.2k", @"-Wall,-I/home/<user>/include", @"-Wall,-Iinclude"),
        ("2.5a", @"C:\Users\<user>(\u00f6bb)", @"<path>"),
        ("2.5b", @"C:\\Users\\<user>(\u00f6bb)", @"<path>"),
        ("2.5c", @"\u0027C:\\Users\\<user>(\u00f6bb)\u0027", @"\u0027<path>\u0027"),
        ("2.5d", @"/home/<user>(\u00f6bb)", @"<path>"),
        ("2.5f", @"C:\Users\<user>(bb cc)", @"<path>"),
        ("2.5g", @"C:\Users\<user>(bb'cc)", @"<path>"),
        ("2.5h", @"C:\Users\<user> bb.\u00f6cc", @"<path>"),
        ("2.5i", @"C:\Users\<user> bb.(cc)", @"<path>"),
        ("2.5j", @"C:\Users\<user>'bb\x.cs", @"x.cs"),
        ("2.5k", @"C:\Users\<user>'bb", @"<path>"),
        ("2.5l", @"C:\Users\<user>\u00f6'bb", @"<path>"),
        ("2.5m", @"C:\Users\<user>" + "\u00F6" + @"'bb", @"<path>"),
        ("2.6a", @"gcc /tmp/a.c -I/home/<user>", @"gcc a.c -I<path>"),
        ("2.6b", @"gcc /tmp/a.c -I/Users/<user>", @"gcc a.c -I<path>"),
        ("2.6c", @"gcc /tmp/a.c -I/cygdrive/c/Users/<user>", @"gcc a.c -I<path>"),
        ("2.6d", @"gcc /tmp/a.c -I/home/<user>/include", @"gcc a.c -Iinclude"),
        ("2.6e", @"see /tmp/x.cs (/home/<user>)", @"see <path>)"),
        ("2.6f", @"C:\a\x.cs\n/home/<user>", @"<path>"),
        ("2.6g", @"C:\Users\<user>\n/home/<user>", @"<path>"),
        ("2.12a", @"C:\\a\\x.cs\n~/y.cs", @"x.cs\ny.cs"),
        ("2.12b", @"\u0027~/x.cs\u0027", @"\u0027x.cs\u0027"),
        ("2.12c", @"-I~/include", @"-Iinclude"),
        ("2.13", @"\\host\c$\Documents and Settings\<user>", @"<path>"),
        ("3.2d", @"\\\\h\u00f6st\\Users\\<user>", @"<path>"),
        ("3.2e", @"\\\\host\\sh\u00f6re\\Users\\<user>", @"<path>"),
        ("3.2f", @"\u0027\\h\u00f6st\Users\<user>\u0027", @"\u0027<path>\u0027"),
        ("3.4d", @"foo-~\nx.cs", @"foo-~\nx.cs"),
        ("3.8a", @"-I/tmp/a.\\host\c$\Users\<user>\nnext", @"-I<path>\nnext"),
        ("3.8b", @"\t/tmp/a.\\host\Users\<user>", @"\t<path>"),
        ("3.8c", @"\u00f6/tmp/a\u00f6\\wsl$\Ubuntu\home\<user>\tnext", @"\u00f6<path>\tnext"),
        ("3.8d", @"\u00f6/tmp/a\u00f6/home/<user>", @"\u00f6<path>"),
        ("3.8e", @"\\/opt/data/home/<user>/src/x.cs", @"\\<path>"),
        ("3.8f", @"\\/cygdrive/c/Users/<user>/x.cs\tnext", @"\\<path>\tnext"),
        ("hdr", @"match \\d+ here", @"match \\d+ here"),
    ];

    private static readonly (string Id, string Input, string Expected)[] OtherReviewRowsCharacterisationRows =
    [
        ("2.1a", @"C:\Users\<user>(1)D:\Users\<user>\x.cs", @"<path>x.cs"),
        ("2.1b", @"C:\Users\<user>(bb)D:\Users\<user>\x.cs", @"<path>x.cs"),
        ("2.1c", @"C:\Users\<user>(1)file://localhost/home/<user>/x.cs", @"<path>x.cs"),
        ("2.1d", @"C:\Users\<user>(bb)file:///C:/Users/<user>/x.cs", @"<path>x.cs"),
        ("2.2a", @"gcc /tmp/a.c\n-I/home/<user>/include", @"gcc a.c\n-Iinclude"),
        ("2.2b", @"gcc /tmp/a.c\t-I/home/<user>/include", @"gcc a.c\t-Iinclude"),
        ("2.2c", @"gcc /tmp/a.c\r-I/home/<user>/include", @"gcc a.c\r-Iinclude"),
        ("2.2d", @"gcc /tmp/a.c\u0027-I/home/<user>/include\u0027", @"gcc a.c\u0027-Iinclude\u0027"),
        ("2.2e", @"gcc /tmp/a.c\u0022-I/home/<user>/include\u0022", @"gcc a.c\u0022-Iinclude\u0022"),
        ("2.2f", @"gcc ""-I/home/<user>/include""", @"gcc ""-Iinclude"""),
        ("2.2g", @"gcc '-I/home/<user>/include'", @"gcc '-Iinclude'"),
        ("2.2h", @"gcc \u0027-I/home/<user>/include\u0027", @"gcc \u0027-Iinclude\u0027"),
        ("2.3a", @"C:\\a\\x.cs\b/home/<user>/y.cs", @"x.cs\by.cs"),
        ("2.3b", @"C:\\a\\x.cs\f/home/<user>/y.cs", @"x.cs\fy.cs"),
        ("2.3c", @"C:\\a\\x.cs\f\\host\Users\<user>\y.cs", @"x.cs\fy.cs"),
        ("2.4a", @"\u00f6/tmp/a\u00f6C:\\Users\\<user>", @"\u00f6a\u00f6<path>"),
        ("2.4b", @"\t/tmp/a.C:\\Users\\<user>", @"\ta.<path>"),
        ("2.4c", @"\n/tmp/a.C:\Users\<user>", @"\na.<path>"),
        ("2.4d", @"-I/tmp/a.C:\Users\<user>", @"-Ia.<path>"),
        ("2.4e", @"\t\\C:\Users\<user>", @"\t<path>"),
        ("2.4f", @"\n\\C:\Users\<user>", @"\n<path>"),
        ("2.5e", @"C:\Users\<user>(bb)\u00f6cc", @"<path>"),
        ("2.5n", @"C:\Users\<user> bb: access denied", @"<path> bb: access denied"),
        ("2.5o", @"C:\Users\<user>\u00f6: access denied", @"<path>: access denied"),
        ("2.7a", @"[""\\host\share\x.cs"",""C:\Users\<user>\y.cs""]", @"[""x.cs"",""y.cs""]"),
        ("2.7b", @"\u0027\\\\host\\share\\x.cs\u0027,\u0027/home/<user>/y.cs\u0027", @"\u0027x.cs\u0027,\u0027y.cs\u0027"),
        ("2.7c", @"\\host\share\x.cs;C:\Users\<user>\y.cs", @"x.cs;y.cs"),
        ("2.8", @"see C:\Users\<user>(bb).", @"see <path>."),
        ("2.9a", @"/tmp/a/~\nx.cs", @"<path>\nx.cs"),
        ("2.9b", @"C:\\a\\~\nx.cs", @"<path>\nx.cs"),
        ("2.9c", @"/home/<user>/~\nx.cs", @"<path>\nx.cs"),
        ("2.9d", @"/tmp/a/~\tx.cs", @"<path>\tx.cs"),
        ("2.9e", @"\\/home/<user>\nx.cs", @"\\<path>\nx.cs"),
        ("2.9f", @"\\/opt/data/home/<user>", @"\\<path>"),
        ("2.9g", @"\\/cygdrive/c/Users/<user>", @"\\<path>"),
        ("2.9h", @"/tmp/a/file://localhost/home/<user>/x.cs", @"<path>x.cs"),
        ("2.9i", @"C:\\a\\file://localhost/home/<user>/x.cs", @"<path>x.cs"),
        ("2.9j", @"C:\\a\\b.C:\Users\<user>", @"b.<path>"),
        ("2.11", @"/d/users/<user>", @"<path>"),
    ];

    private static readonly (string Id, string Input, string Expected)[] MutantKillingRowsPendingRows =
    [
        ("flag-left-quotes", @"[-I/home/<user>/include]", @"[-Iinclude]"),
        ("flag-any-u", @"see a\u00f6-b/tmp/x.cs", @"see a\u00f6-b/tmp/x.cs"),
        ("flag-no-esc", @"gcc \u0027-IC:\Users\<user>\include\u0027", @"gcc \u0027-Iinclude\u0027"),
        ("no-adjacent", @"C:\Users\<user>_D:\Users\<user>\x.cs", @"<path>x.cs"),
        ("unc-host-alone", @"match \\d+ here", @"match \\d+ here"),
        ("tilde-today", @"/tmp/a/foo-~\nx.cs", @"foo-~\nx.cs"),
        ("no-flag-split", @"gcc /tmp/a.c -I/home/<user>/include", @"gcc a.c -Iinclude"),
        ("apos-today", @"C:\Users\<user>'bb", @"<path>"),
        ("apos-no-mark", @"C:\Users\<user>'bb", @"<path>"),
        ("apos-no-esc", @"C:\Users\<user>\u00f6'bb", @"<path>"),
        ("parent-no-bridge", @"C:\a\x.cs\n/home/<user>", @"<path>"),
        ("unix-no-letter-users", @"see /tmp/x.cs (/c/users/<user>)", @"see <path>)"),
        ("group-no-esc", @"C:\Users\<user>(\u00f6bb)", @"<path>"),
        ("group-no-space", @"C:\Users\<user>(bb cc)", @"<path>"),
        ("dot-no-esc", @"C:\Users\<user> bb.\u00f6cc", @"<path>"),
        ("dot-no-paren", @"C:\Users\<user> bb.(cc)", @"<path>"),
        ("glued-parens", @"see (C:\Users\<user>(bb))", @"see (<path>)"),
        ("glued-no-root", @"C:\Users\<user>(bb)ccD:\Users\<user>\x.cs", @"<path>x.cs"),
        ("no-bare-unc", @"\\/cygdrive/c/Users/<user>/x.cs\tnext", @"\\<path>\tnext"),
    ];

    private static readonly (string Id, string Input, string Expected)[] MutantKillingRowsCharacterisationRows =
    [
        ("e1-unescaped", @"x\\n/tmp/a/y.cs", @"x\\n/tmp/a/y.cs"),
        ("esc-no-b", @"C:\\a\\x.cs\b/home/<user>/y.cs", @"x.cs\by.cs"),
        ("letter-any", @"C:\\Users\\<user>\u003e", @"<path>\u003e"),
        ("flag-letter-I", @"gcc -L/home/<user>/lib", @"gcc -Llib"),
        ("unc-pathmark", @"\\/home/<user>\nx.cs", @"\\<path>\nx.cs"),
        ("unc-always-sgl", @"\\\\host\\share\\x.cs\n/home/<user>/y.cs", @"x.cs\ny.cs"),
        ("unc-no-drive-host", @"\\C:\\Users\\<user>\\x.cs", @"x.cs"),
        ("tilde-no-paren", @"(~/x.cs)", @"(x.cs)"),
        ("walk-any-end", @"/x-/home/<user>", @"/x-<path>"),
        ("fixed-no-vartmp", @"/var/tmp/a/x.cs", @"x.cs"),
        ("unix-no-Users", @"/Users/<user>/x.cs", @"x.cs"),
        ("unix-drive-c", @"/d/users/<user>", @"<path>"),
        ("final-paren-ok", @"C:\a\(b)C:\Users\<user>", @"<path>(b)<path>"),
        ("names-semicolon-ok", @"PATH=/tmp/a;/home/<user>/bin", @"PATH=a;bin"),
        ("guard-n-only", @"C:\\a\\x.cs\r/home/<user>/y.cs", @"x.cs\ry.cs"),
        ("esc-off", @"\u0027\\host\share\x.cs\u0027/home/<user>/y.cs", @"\u0027x.cs\u0027y.cs"),
        ("parent-no-home", @"\\wsl$\Ubuntu\home\<user>", @"<path>"),
        ("parent-no-das", @"C:\Documents and Settings\<user>", @"<path>"),
        ("parent-cs", @"\\host\c$\Users\<user>", @"<path>"),
        ("no-parent-rule", @"C:\Users\<user>", @"<path>"),
        ("parent-drive-any-depth", @"D:\src\Users\UsersController.cs", @"UsersController.cs"),
        ("parent-unix-ci", @"/tmp/a/Home/x.cs", @"x.cs"),
        ("dir-space-ok", @"gcc /tmp/a.c -I /usr/include -I/home/<user>/include", @"gcc a.c -I /usr/include -Iinclude"),
        ("no-tail", @"C:\Users\<user>(bb)cc", @"<path>"),
        ("no-tilde-name", @"/tmp/a/~\nx.cs", @"<path>\nx.cs"),
        ("word-no-esc", @"C:\Users\<user> \u00f6cc", @"<path>"),
    ];

    // Every review input in the order of the architect's list (100 inputs); sweep C uses them as templates.
    public static readonly string[] ReviewInputs =
    [
        @"C:\Users\<user>(1)D:\Users\<user>\x.cs",
        @"C:\Users\<user>(bb)D:\Users\<user>\x.cs",
        @"C:\Users\<user>(1)file://localhost/home/<user>/x.cs",
        @"C:\Users\<user>(bb)file:///C:/Users/<user>/x.cs",
        @"gcc /tmp/a.c\n-I/home/<user>/include",
        @"gcc /tmp/a.c\t-I/home/<user>/include",
        @"gcc /tmp/a.c\r-I/home/<user>/include",
        @"gcc /tmp/a.c\u0027-I/home/<user>/include\u0027",
        @"gcc /tmp/a.c\u0022-I/home/<user>/include\u0022",
        @"gcc ""-I/home/<user>/include""",
        @"gcc '-I/home/<user>/include'",
        @"gcc \u0027-I/home/<user>/include\u0027",
        @"[-I/home/<user>/include]",
        @"(-I/home/<user>/include)",
        @"-Wall,-I/home/<user>/include",
        @"C:\\a\\x.cs\b/home/<user>/y.cs",
        @"C:\\a\\x.cs\f/home/<user>/y.cs",
        @"C:\\a\\x.cs\f\\host\Users\<user>\y.cs",
        @"\u00f6/tmp/a\u00f6C:\\Users\\<user>",
        @"\t/tmp/a.C:\\Users\\<user>",
        @"\n/tmp/a.C:\Users\<user>",
        @"-I/tmp/a.C:\Users\<user>",
        @"\t\\C:\Users\<user>",
        @"\n\\C:\Users\<user>",
        @"C:\Users\<user>(\u00f6bb)",
        @"C:\\Users\\<user>(\u00f6bb)",
        @"\u0027C:\\Users\\<user>(\u00f6bb)\u0027",
        @"/home/<user>(\u00f6bb)",
        @"C:\Users\<user>(bb)\u00f6cc",
        @"C:\Users\<user>(bb cc)",
        @"C:\Users\<user>(bb'cc)",
        @"C:\Users\<user> bb.\u00f6cc",
        @"C:\Users\<user> bb.(cc)",
        @"C:\Users\<user>'bb\x.cs",
        @"C:\Users\<user>'bb",
        @"C:\Users\<user>\u00f6'bb",
        @"C:\Users\<user>" + "\u00F6" + @"'bb",
        @"C:\Users\<user> bb: access denied",
        @"C:\Users\<user>\u00f6: access denied",
        @"gcc /tmp/a.c -I/home/<user>",
        @"gcc /tmp/a.c -I/Users/<user>",
        @"gcc /tmp/a.c -I/cygdrive/c/Users/<user>",
        @"gcc /tmp/a.c -I/home/<user>/include",
        @"see /tmp/x.cs (/home/<user>)",
        @"C:\a\x.cs\n/home/<user>",
        @"C:\Users\<user>\n/home/<user>",
        @"[""\\host\share\x.cs"",""C:\Users\<user>\y.cs""]",
        @"\u0027\\\\host\\share\\x.cs\u0027,\u0027/home/<user>/y.cs\u0027",
        @"\\host\share\x.cs;C:\Users\<user>\y.cs",
        @"see C:\Users\<user>(bb).",
        @"/tmp/a/~\nx.cs",
        @"C:\\a\\~\nx.cs",
        @"/home/<user>/~\nx.cs",
        @"/tmp/a/~\tx.cs",
        @"\\/home/<user>\nx.cs",
        @"\\/opt/data/home/<user>",
        @"\\/cygdrive/c/Users/<user>",
        @"/tmp/a/file://localhost/home/<user>/x.cs",
        @"C:\\a\\file://localhost/home/<user>/x.cs",
        @"C:\\a\\b.C:\Users\<user>",
        @"/d/users/<user>",
        @"C:\\a\\x.cs\n~/y.cs",
        @"\u0027~/x.cs\u0027",
        @"-I~/include",
        @"\\host\c$\Documents and Settings\<user>",
        @"\\<host>\Users\<user>",
        @"\\<host>\Users\<user>\x.cs",
        @"\\<host>\share\x.cs",
        @"\\host(1)\Users\<user>",
        @"\\host's\Users\<user>",
        @"\\a<b>\Users\<user>",
        @"\\\\h\u00f6st\\Users\\<user>",
        @"\\\\host\\sh\u00f6re\\Users\\<user>",
        @"\u0027\\h\u00f6st\Users\<user>\u0027",
        @"\u0027-IC:\a\x.cs\u0027/home/<user>",
        @"\u0027-I\\host\share\x.cs\u0027/home/<user>/y.cs",
        @"gcc \u0027-IC:\Users\<user>\include\u0027",
        @"gcc \u0027-IC:\Users\<user>\u0027",
        @"see a\u00f6-b/tmp/x.cs",
        @"/tmp/a/foo-~\nx.cs",
        @"/tmp/a/~~\nx.cs",
        @"C:\\a\\foo-~\nx.cs",
        @"foo-~\nx.cs",
        @"C:\Users\<user>D:\Users\<user>\x.cs",
        @"C:\Users\<user>_D:\Users\<user>\x.cs",
        @"C:\Users\<user>.file://localhost/home/<user>/x.cs",
        @"\/tmp/a/nC:\Users\<user>",
        @"\t/tmp/a.file://localhost/home/<user>/x.cs",
        @"see (C:\Users\<user>(bb))",
        @"see (C:\Users\<user>(bb)) here",
        @"C:\Users\<user>(bb)ccfile:///C:/Users/<user>/x.cs",
        @"C:\Users\<user>(bb)ccD:\Users\<user>\x.cs",
        @"C:\Users\<user>(1)-D:\Users\<user>\x.cs",
        @"gcc ""-I/home/<user>/include""",
        @"gcc /tmp/a.c\t-I/home/<user>/include",
        @"id u0041-I/tmp/a/x.cs",
        @"\t/tmp/a.d:\Users\<user>",
        @"\t/tmp/a.C:/Users/<user>",
        @"\\d:\\Users\\<user>\\x.cs",
        @"\\\\host\\share\\x.cs\r/home/<user>/y.cs",
        @"-I/tmp/a.\\host\c$\Users\<user>\nnext",
        @"\t/tmp/a.\\host\Users\<user>",
        @"\u00f6/tmp/a\u00f6\\wsl$\Ubuntu\home\<user>\tnext",
        @"\u00f6/tmp/a\u00f6/home/<user>",
        @"\\/opt/data/home/<user>/src/x.cs",
        @"\\/cygdrive/c/Users/<user>/x.cs\tnext",
        @"match \\d+ here",
    ];
}
