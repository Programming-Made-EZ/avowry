using Xunit.Abstractions;

namespace Avowry.Acceptance.Tests;

// Card 441 red: the D263 real-shape corpus (.mob/records/441/corpus.txt, embedded below row for row; a completeness test
// reads the file and fails if the two drift) and the killing rows of the slice's new mutants.
// Criteria: 441-1 the older real shapes (the corpus families and the older MINOR shapes of card 439); 441-2 the contrived
// one-letter chains and the named limits; 441-3 the killing rows of the mutants (rows that are corpus rows stay in the
// corpus sets; the rows added here are the ones the corpus does not hold); 441-4 (the redundant condition) has no test of
// its own, every card 439 and 435 row must still pass; 441-5 the device paths before a drive.
// Each row runs as written and with the placeholder replaced by a plain letter stand-in built at run time (expected the
// same way). A row whose expected output still holds the account name pins an older limit (a `# pinned:` row of the corpus).
// Rows holding a literal name are split strings (ruling 4). No wall-clock assertion (D254).
public sealed class PathScan441
{
    private readonly ITestOutputHelper output;

    public PathScan441(ITestOutputHelper output) => this.output = output;

    private static IEnumerable<object[]> Rows(IEnumerable<(string Id, string Input, string Expected)> rows) =>
        rows.Select(r => new object[] { r.Id, r.Input, r.Expected });

    private static readonly (string Id, string Input, string Expected)[] OlderShapesPendingRows =
    [
        ("compiler-02", @"gcc -isystem/home/<user>/include main.c", @"gcc -isysteminclude main.c"),
        ("compiler-08", @"cl.exe /c /IC:\Users\<user>\include /FoC:\Users\<user>\obj\main.obj main.c", @"cl.exe /c /Iinclude /Fomain.obj main.c"),
        ("compiler-16", @"clang -idirafter/home/<user>/inc -iquote/home/<user>/q a.c", @"clang -idirafterinc -iquoteq a.c"),
        ("env-08", @"MAIL=/var/mail/<user>", @"MAIL=<path>"),
        ("env-13", @"MAIL=/var/spool/mail/<user>", @"MAIL=<path>"),
        ("json-03", @"{""file"":""\/home\/<user>\/x.cs""}", @"{""file"":""x.cs""}"),
        ("json-11", @"{""home"":""\/Users\/<user>""}", @"{""home"":""<path>""}"),
        ("remote-01", @"git remote add origin ssh://host/home/<user>/repo.git", @"git remote add origin ssh://host/repo.git"),
        ("remote-06", @"sftp://host/home/<user>/x.txt", @"sftp://host/x.txt"),
        ("remote-11", @"git clone ssh://git@host:2222/home/<user>/r.git", @"git clone ssh://git@host:2222/r.git"),
        ("minor-01", @"~<user>/x.cs", @"x.cs"),
        ("minor-02", @"ls ~<user>/src", @"ls src"),
        ("minor-03", @"\\?\UNC\server\share\Users\<user>\x.cs", @"x.cs"),
        ("minor-07", @"a/b:/home/<user>", @"a/b:<path>"),
    ];

    public static IEnumerable<object[]> OlderShapesPending() => Rows(OlderShapesPendingRows);

    private static readonly (string Id, string Input, string Expected)[] OlderShapesCharacterisationRows =
    [
        ("compiler-01", @"gcc -I/home/<user>/include -L/home/<user>/lib -o out main.c", @"gcc -Iinclude -Llib -o out main.c"),
        ("compiler-03", @"gcc -isystem /home/<user>/include main.c", @"gcc -isystem include main.c"),
        ("compiler-04", @"clang -I/Users/<user>/src/include -c a.c", @"clang -Iinclude -c a.c"),
        ("compiler-05", @"/home/<user>/src/main.c:12:5: error: expected ';' before '}' token", @"main.c:12:5: error: expected ';' before '}' token"),
        ("compiler-06", @"In file included from /home/<user>/src/a.h:3,", @"In file included from a.h:3,"),
        ("compiler-07", @"ld: cannot find -lfoo in /home/<user>/lib", @"ld: cannot find -lfoo in lib"),
        ("compiler-09", @"cl.exe /c /I C:\Users\<user>\include main.c", @"cl.exe /c /I include main.c"),
        ("compiler-10", @"C:\Users\<user>\src\main.cpp(12): error C2065: 'x': undeclared identifier", @"main.cpp(12): error C2065: 'x': undeclared identifier"),
        ("compiler-11", @"LINK : fatal error LNK1104: cannot open file 'C:\Users\<user>\lib\foo.lib'", @"LINK : fatal error LNK1104: cannot open file 'foo.lib'"),
        ("compiler-12", @"gcc -Wl,-rpath,/home/<user>/lib main.c", @"gcc -Wl,-rpath,lib main.c"),
        ("compiler-13", @"gcc ""-I/home/<user>/include"" main.c", @"gcc ""-Iinclude"" main.c"),
        ("compiler-14", @"CFLAGS=-O2 -I/home/<user>/inc", @"CFLAGS=-O2 -Iinc"),
        ("compiler-15", @"csc /out:C:\Users\<user>\bin\a.exe /reference:C:\Users\<user>\lib\b.dll a.cs", @"csc /out:a.exe /reference:b.dll a.cs"),
        ("env-01", @"PATH=/home/<user>/bin:/home/<user>/.local/bin:/usr/bin", @"PATH=bin:bin:/usr/bin"),
        ("env-02", @"PYTHONPATH=/home/<user>/lib:/home/<user>", @"PYTHONPATH=lib:<path>"),
        ("env-03", @"CLASSPATH=/home/<user>/lib/a.jar:/home/<user>/classes", @"CLASSPATH=a.jar:classes"),
        ("env-04", @"Path=C:\Users\<user>\AppData\Local\Microsoft\WindowsApps;C:\Windows\system32", @"Path=WindowsApps;system32"),
        ("env-05", @"PSModulePath=C:\Users\<user>\Documents\WindowsPowerShell\Modules;C:\Program Files\WindowsPowerShell\Modules", @"PSModulePath=Modules;Modules"),
        ("env-06", @"HOME=/home/<user>", @"HOME=<path>"),
        ("env-07", @"USERPROFILE=C:\Users\<user>", @"USERPROFILE=<path>"),
        ("env-09", @"OLDPWD=/home/<user>/src", @"OLDPWD=src"),
        ("env-10", @"XDG_DATA_DIRS=/home/<user>/.local/share/flatpak/exports/share:/usr/share", @"XDG_DATA_DIRS=share:/usr/share"),
        ("env-11", @"GOPATH=/home/<user>/go", @"GOPATH=go"),
        ("env-12", @"export JAVA_HOME=/home/<user>/jdk", @"export JAVA_HOME=jdk"),
        ("volume-01", @"docker run -v /home/<user>/work:/work -it ubuntu", @"docker run -v work:/work -it ubuntu"),
        ("volume-02", @"docker run -v /home/<user>:/data ubuntu", @"docker run -v <path>:/data ubuntu"),
        ("volume-03", @"docker run -v C:\Users\<user>\proj:/app node", @"docker run -v proj:/app node"),
        ("volume-04", @"docker run -v C:/Users/<user>/proj:/home/<user> ubuntu", @"docker run -v proj:<path>"),
        ("volume-05", @"podman run -v C:\Users\<user>\data:/home/<user> x", @"podman run -v data:<path>"),
        ("volume-06", @"docker run --mount type=bind,src=/home/<user>/data,dst=/data x", @"docker run --mount type=bind,src=data,dst=/data x"),
        ("volume-07", @"docker run -v /mnt/c/Users/<user>/proj:/app x", @"docker run -v proj:/app x"),
        ("volume-08", @"docker run -v \\wsl$\Ubuntu\home\<user>\proj:/app x", @"docker run -v proj:/app x"),
        ("volume-09", @"  - /home/<user>/.config:/root/.config", @"  - .config:.config"),
        ("volume-10", @"volumes: [/home/<user>/data:/home/<user>]", @"volumes: [data:<path>]"),
        ("volume-11", @"docker run -v ""C:\Users\<user>\src:/src"" x", @"docker run -v ""src:/src"" x"),
        ("volume-12", @"docker run -v //c/Users/<user>/proj:/app x", @"docker run -v /proj:/app x"),
        ("passwd-01", @"/home/<user>:/bin/bash", @"<path>:/bin/bash"),
        ("passwd-02", @"svc:x:1000:1000:Svc,,,:/home/<user>:/bin/bash", @"svc:x:1000:1000:Svc,,,:<path>:/bin/bash"),
        ("passwd-03", @"svc:*:501:20:Svc:/Users/<user>:/bin/zsh", @"svc:*:501:20:Svc:<path>:/bin/zsh"),
        ("passwd-04", @"www-data:x:33:33:www-data:/var/www:/usr/sbin/nologin", @"www-data:x:33:33:www-data:/var/www:/usr/sbin/nologin"),
        ("passwd-05", @"svc:x:1000:1000::/home/<user>:/usr/bin/fish", @"svc:x:1000:1000::<path>:/usr/bin/fish"),
        ("passwd-06", @"root:x:0:0:root:/root:/bin/bash", @"root:x:0:0:root:/root:/bin/bash"),
        ("passwd-07", @"svc:x:1002:1002::/home/<user>/:/bin/sh", @"svc:x:1002:1002::<path>:/bin/sh"),
        ("passwd-08", @"nobody:x:65534:65534:nobody:/nonexistent:/usr/sbin/nologin", @"nobody:x:65534:65534:nobody:/nonexistent:/usr/sbin/nologin"),
        ("passwd-09", @"svc:x:1000:1000::/home/<user>:", @"svc:x:1000:1000::<path>:"),
        ("json-01", @"{""file"":""/home/<user>/src/a.py"",""line"":3}", @"{""file"":""a.py"",""line"":3}"),
        ("json-02", @"{""path"":""C:\\Users\\<user>\\x.cs""}", @"{""path"":""x.cs""}"),
        ("json-04", @"{""cwd"":""/Users/<user>/proj"",""argv"":[""node"",""/Users/<user>/proj/a.js""]}", @"{""cwd"":""proj"",""argv"":[""node"",""a.js""]}"),
        ("json-05", @"{""msg"":""open C:\\Users\\<user>\\a.txt: denied""}", @"{""msg"":""open a.txt: denied""}"),
        ("json-06", @"{""pattern"":""\\d+"",""path"":""C:\\Users\\<user>\\x.cs""}", @"{""pattern"":""\\d+"",""path"":""x.cs""}"),
        ("json-07", @"{""home"":""/home/<user>""}", @"{""home"":""<path>""}"),
        ("json-08", @"{""stack"":""at f (/home/<user>/a.js:3:5)\nat g (/home/<user>/b.js:4:1)""}", @"{""stack"":""at f (a.js:3:5)\nat g (b.js:4:1)""}"),
        ("json-09", @"{""uri"":""file:///C:/Users/<user>/x.cs""}", @"{""uri"":""x.cs""}"),
        ("json-10", @"{""file"":""C:\/Users\/<user>\/x.cs""}", @"{""file"":""x.cs""}"),
        ("remote-02", @"git clone git@host:/home/<user>/repo.git", @"git clone git@host:repo.git"),
        ("remote-03", @"scp a.txt host:/home/<user>/x.txt", @"scp a.txt host:x.txt"),
        ("remote-04", @"rsync -av src/ host:/home/<user>/backup/", @"rsync -av src/ host:<path>"),
        ("remote-05", @"ssh host cat /home/<user>/.bashrc", @"ssh host cat .bashrc"),
        ("remote-07", @"git clone file:///home/<user>/repo.git", @"git clone repo.git"),
        ("remote-08", @"git@github.com:org/repo.git", @"git@github.com:org/repo.git"),
        ("remote-09", @"rsync: change_dir ""/home/<user>/src"" failed: No such file or directory (2)", @"rsync: change_dir ""src"" failed: No such file or directory (2)"),
        ("remote-10", @"fatal: repository '/home/<user>/repo' does not exist", @"fatal: repository 'repo' does not exist"),
        ("trace-01", @"   at Foo.Bar() in C:\Users\<user>\src\Foo.cs:line 42", @"   at Foo.Bar() in Foo.cs:line 42"),
        ("trace-02", @"  File ""/home/<user>/proj/app.py"", line 10, in <module>", @"  File ""app.py"", line 10, in <module>"),
        ("trace-03", @"    at com.x.Main.main(Main.java:5)", @"    at com.x.Main.main(Main.java:5)"),
        ("trace-04", @"Exception in thread ""main"" java.io.FileNotFoundException: /home/<user>/a.txt (No such file or directory)", @"Exception in thread ""main"" java.io.FileNotFoundException: a.txt (No such file or directory)"),
        ("trace-05", @"    at Object.<anonymous> (/Users/<user>/proj/index.js:1:7)", @"    at Object.<anonymous> (index.js:1:7)"),
        ("trace-06", @"   at Program.Main() in /home/<user>/src/Program.cs:line 3", @"   at Program.Main() in Program.cs:line 3"),
        ("trace-07", @"thread 'main' panicked at src/main.rs:2:5", @"thread 'main' panicked at src/main.rs:2:5"),
        ("trace-08", @"  File ""C:\Users\<user>\proj\app.py"", line 10, in f", @"  File ""app.py"", line 10, in f"),
        ("trace-09", @"  File ""/home/<user>/.local/lib/python3.11/site-packages/x.py"", line 1", @"  File ""x.py"", line 1"),
        ("trace-10", @"    at f (file:///home/<user>/a.mjs:1:1)", @"    at f (a.mjs:1:1)"),
        ("minor-06", @"https://example.com/home/<user>/x", @"https://example.com/home/<user>/x"),
        ("volume-13", @"docker run -v /c/Users/<user>/proj:/app x", @"docker run -v proj:/app x"),
        ("compiler-17", @"cl.exe /c /Fe:C:\Users\<user>\bin\a.exe a.c", @"cl.exe /c /Fe:a.exe a.c"),
    ];

    public static IEnumerable<object[]> OlderShapesCharacterisation() => Rows(OlderShapesCharacterisationRows);

    private static readonly (string Id, string Input, string Expected)[] ContrivedChainsPendingRows =
    [
        ("minor-08", @"\\server\share\a:/home/<user>", @"a:<path>"),
        ("minor-09", @"C:\tmp\x:/home/<user>", @"x:<path>"),
        ("minor-10", @"C:\Users\<user>\a:/home/<user>", @"a:<path>"),
        ("minor-11", @"-v ~/x:/data", @"-v <path>:/data"),
        ("minor-12", @"/tmp/x:/x", @"<path>:/x"),
        ("minor-13", @"-v /home/<user>/v:/v", @"-v <path>:/v"),
        ("minor-14", @"/tmp/x://server/home/<user>", @"<path>://server/home/<user>"),
    ];

    public static IEnumerable<object[]> ContrivedChainsPending() => Rows(ContrivedChainsPendingRows);

    private static readonly (string Id, string Input, string Expected)[] ContrivedChainsCharacterisationRows =
    [
        ("minor-15", @"/tmp/x'y:/home/<user>", @"x'<user>"),
        ("minor-16", @"C:\Users\<user> bb: access denied", @"<path> bb: access denied"),
        ("minor-17", @"C:\Users\<user> 1's~/Users\<user> 1's~/Users\<user>", @"<path> 1's~/Users\<user> 1's~/Users\<user>"),
        ("minor-18", @"/home/<user>:/home/<user>", @"<path>:<path>"),
        ("minor-19", @"see /proj:\x", @"see /proj:\x"),
        ("minor-20", @"docker run -v /pj:/app x", @"docker run -v /pj:/app x"),
    ];

    public static IEnumerable<object[]> ContrivedChainsCharacterisation() => Rows(ContrivedChainsCharacterisationRows);

    private static readonly (string Id, string Input, string Expected)[] MutantRowsPendingRows =
    [
        ("mut-01", @"cl.exe /c /IC:\Users\<user>\include main.c", @"cl.exe /c /Iinclude main.c"),
    ];

    public static IEnumerable<object[]> MutantRowsPending() => Rows(MutantRowsPendingRows);

    private static readonly (string Id, string Input, string Expected)[] MutantRowsCharacterisationRows =
    [
        ("mut-02", @"-I/home/<user>/src~\nx.cs", @"-Isrc~\nx.cs"),
        ("mut-03", @"C:\a\D:/b", @"<path>b"),
        ("mut-04", @"-v /home/<user>:/data", @"-v <path>:/data"),
        ("ownletter-a-letters-only", "-v /home/" + "u1a:/data", "-v <path>:/data"),
        ("https-url-with-a-users-folder", "see https://a/" + "Users/x ok", "see https://a/" + "Users/x ok"),
    ];

    public static IEnumerable<object[]> MutantRowsCharacterisation() => Rows(MutantRowsCharacterisationRows);

    private static readonly (string Id, string Input, string Expected)[] DevicePathsCharacterisationRows =
    [
        ("minor-04", @"'\\?\C:\Users\<user>\x.cs'", @"'\\?\x.cs'"),
        ("minor-05", @"\\.\C:\Users\<user>\x.cs", @"<path>x.cs"),
    ];

    public static IEnumerable<object[]> DevicePathsCharacterisation() => Rows(DevicePathsCharacterisationRows);

    private static IEnumerable<(string Id, string Input, string Expected)> AllRows() =>
        Enumerable.Empty<(string Id, string Input, string Expected)>()
        .Concat(OlderShapesPendingRows)
        .Concat(OlderShapesCharacterisationRows)
        .Concat(ContrivedChainsPendingRows)
        .Concat(ContrivedChainsCharacterisationRows)
        .Concat(MutantRowsPendingRows)
        .Concat(MutantRowsCharacterisationRows)
        .Concat(DevicePathsCharacterisationRows)
        ;

    [Theory]
    [MemberData(nameof(OlderShapesPending))]
    [Trait("Slice", "441")]
    [Trait("Criterion", "441-1")]
    [Trait("Status", "Pending")]
    // Pending until the card 441 green that ports the reference scanner
    public void Id_441_1_older_real_shapes(string id, string input, string expected)
    {
        // Given a stored diagnostic of a real shape (compiler, environment, container, passwd, JSON, remote or stack trace) or an older MINOR shape of card 439
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed in both account forms, no account name survives (unless the row pins an older limit), and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertCorpusRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(OlderShapesCharacterisation))]
    [Trait("Slice", "441")]
    [Trait("Criterion", "441-1")]
    public void Id_441_1_older_real_shapes_that_pass_today(string id, string input, string expected)
    {
        // Given a stored diagnostic of a real shape (compiler, environment, container, passwd, JSON, remote or stack trace) or an older MINOR shape of card 439
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed in both account forms, no account name survives (unless the row pins an older limit), and a second pass changes nothing (characterisation: the row passes on the current tree)
        output.WriteLine($"row {id}");
        AssertCorpusRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(ContrivedChainsPending))]
    [Trait("Slice", "441")]
    [Trait("Criterion", "441-2")]
    [Trait("Status", "Pending")]
    // Pending until the card 441 green that ports the reference scanner
    public void Id_441_2_contrived_chains(string id, string input, string expected)
    {
        // Given a contrived one-letter chain or a named limit of card 439
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed in both account forms, no account name survives (unless the row pins an older limit), and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertCorpusRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(ContrivedChainsCharacterisation))]
    [Trait("Slice", "441")]
    [Trait("Criterion", "441-2")]
    public void Id_441_2_contrived_chains_that_pass_today(string id, string input, string expected)
    {
        // Given a contrived one-letter chain or a named limit of card 439
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed in both account forms, no account name survives (unless the row pins an older limit), and a second pass changes nothing (characterisation: the row passes on the current tree)
        output.WriteLine($"row {id}");
        AssertCorpusRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(MutantRowsPending))]
    [Trait("Slice", "441")]
    [Trait("Criterion", "441-3")]
    [Trait("Status", "Pending")]
    // Pending until the card 441 green that ports the reference scanner
    public void Id_441_3_mutant_killing_rows(string id, string input, string expected)
    {
        // Given the killing row of a mutant the slice names
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed in both account forms, no account name survives (unless the row pins an older limit), and a second pass changes nothing
        output.WriteLine($"row {id}");
        AssertCorpusRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(MutantRowsCharacterisation))]
    [Trait("Slice", "441")]
    [Trait("Criterion", "441-3")]
    public void Id_441_3_mutant_killing_rows_that_pass_today(string id, string input, string expected)
    {
        // Given the killing row of a mutant the slice names
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed in both account forms, no account name survives (unless the row pins an older limit), and a second pass changes nothing (characterisation: the row passes on the current tree)
        output.WriteLine($"row {id}");
        AssertCorpusRow(input, expected);
    }

    [Theory]
    [MemberData(nameof(DevicePathsCharacterisation))]
    [Trait("Slice", "441")]
    [Trait("Criterion", "441-5")]
    public void Id_441_5_device_paths_that_pass_today(string id, string input, string expected)
    {
        // Given a device path before a drive
        // When PathScrub.Apply runs
        // Then the row reads exactly as listed in both account forms, no account name survives (unless the row pins an older limit), and a second pass changes nothing (characterisation: the row passes on the current tree)
        output.WriteLine($"row {id}");
        AssertCorpusRow(input, expected);
    }

    [Fact]
    [Trait("Slice", "441")]
    [Trait("Criterion", "441-1")]
    public void Id_441_1_the_embedded_rows_are_the_corpus_file()
    {
        // Given the corpus file of the slice and the rows embedded in this class
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, ".mob", "records", "441", "corpus.txt")))
            dir = dir.Parent;
        Assert.True(dir is not null, "corpus.txt was not found above the test binaries.");
        var file = File.ReadAllLines(Path.Combine(dir!.FullName, ".mob", "records", "441", "corpus.txt"))
            .Where(l => l.Length > 0 && !l.StartsWith('#'))
            .Select(l => l.Split('\t'))
            .Select(p => (Input: p[1], Expected: p[2]))
            .ToList();
        // When every file row is looked up among the embedded rows
        var embedded = AllRows().Select(r => (r.Input, r.Expected)).ToHashSet();
        // Then none is missing
        var missing = file.Where(r => !embedded.Contains(r)).ToList();
        output.WriteLine($"file_rows={file.Count} embedded_rows={embedded.Count} missing={missing.Count}");
        Assert.Empty(missing);
    }

    private static void AssertCorpusRow(string input, string expected)
    {
        var plain = "aa" + "bb";
        // A row whose expected output holds the placeholder pins an older limit: the account name is meant to survive there.
        var pinsAnOlderLimit = expected.Contains("<user>", StringComparison.Ordinal);
        var forms = new[]
        {
            (Input: input, Expected: expected),
            (Input: input.Replace("<user>", plain, StringComparison.Ordinal), Expected: expected.Replace("<user>", plain, StringComparison.Ordinal)),
        }.Distinct();
        foreach (var (text, want) in forms)
        {
            var once = PathScrub.Apply(text);
            Assert.Equal(want, once);
            if (!pinsAnOlderLimit)
            {
                Assert.DoesNotContain("<user>", once, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(plain, once, StringComparison.OrdinalIgnoreCase);
            }

            Assert.Equal(once, PathScrub.Apply(once));
        }
    }
}
