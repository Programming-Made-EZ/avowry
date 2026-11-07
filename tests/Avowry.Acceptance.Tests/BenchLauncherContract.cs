using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Card 264 red 2b (D157 Criterion 2, cycle 2b): every Phrase.Bench child must be started through ONE shared launcher
// that never lets `dotnet run` build the child. Concurrent children that each built the same Debug obj output collided
// (one exit 1, no history). The outer test build already built Phrase.Bench through a ProjectReference, so the child
// runs `--no-build` in the configuration of the running test assembly.
//
// Proposed seam (the green creates it): tests/Avowry.Acceptance.Tests/Support/BenchLauncher.cs
//   internal static class BenchLauncher
//     ProcessStartInfo CreateStartInfo(string repoRoot, string testBaseDirectory, IEnumerable<string> benchArguments)
//       -> FileName "dotnet"; ArgumentList: run --no-build -c <Configuration> --project <repoRoot>/tools/Phrase.Bench/Phrase.Bench.csproj -- <benchArguments...>
//          (redirects stdout/stderr, UseShellExecute=false). Throws BenchLauncherException BEFORE any process exists
//          when output is missing or stale. Never runs a build.
//     ProcessStartInfo CreateStartInfo(IEnumerable<string> benchArguments)   // RepoPaths.Root + AppContext.BaseDirectory
//     string ConfigurationOf(string testBaseDirectory)   // "Debug" or "Release": the path segment named so, case-insensitive
//   internal sealed class BenchLauncherException : Exception
//       message names the configuration, the expected output directory <repoRoot>/tools/Phrase.Bench/bin/<Configuration>,
//       the word "stale" when stale, and a remediation containing "dotnet build Phrase.sln -c <Configuration>".
// Output location: tools/Phrase.Bench/bin/<Configuration>/<any TFM>/Phrase.Bench.dll.
// Freshness rule: Phrase.Bench.dll's last write time must be >= the newest last write time among
//   tools/Phrase.Bench/**/*.cs and Phrase.Bench.csproj (excluding bin/ and obj/); older than that is stale. Equal is fresh.
// Tests use a temp layout (no dotnet run, no build) so they do not depend on how the suite itself was built.
public class BenchLauncherContract
{
    private static readonly string[] ModelArguments =
        ["run", "--suite", "S", "--model", "m", "--model-command", "cmd /c echo hi", "--private-results", "P/history.jsonl"];

    [Theory]
    [InlineData("Debug")]
    [InlineData("Release")]
    [Trait("Slice", "264")]
    public void Start_info_is_no_build_in_the_configuration_of_the_test_assembly(string configuration)
    {
        var layout = Layout.Create(configuration, benchOutputFor: [configuration]);

        var info = BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, ModelArguments);
        var args = info.ArgumentList.ToList();

        Assert.Equal("dotnet", info.FileName);
        Assert.Equal("run", args[0]);
        Assert.Contains("--no-build", args);
        var c = args.IndexOf("-c");
        Assert.True(c >= 0 && c + 1 < args.Count, "no -c <configuration> argument: " + string.Join(" ", args));
        Assert.Equal(configuration, args[c + 1]);
        Assert.True(args.IndexOf("--no-build") < args.IndexOf("--"), "--no-build must be a dotnet run option, before --");
        Assert.True(info.RedirectStandardOutput && info.RedirectStandardError && !info.UseShellExecute);
    }

    [Fact]
    [Trait("Slice", "264")]
    public void Start_info_carries_the_bench_project_and_passes_model_command_and_private_results_through_after_the_separator()
    {
        var layout = Layout.Create("Release", benchOutputFor: ["Release"]);

        var args = BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, ModelArguments).ArgumentList.ToList();

        var project = Path.Combine(layout.Root, "tools", "Phrase.Bench", "Phrase.Bench.csproj");
        Assert.Equal(project, args[args.IndexOf("--project") + 1]);
        var separator = args.IndexOf("--");
        Assert.True(separator > args.IndexOf("--project"));
        Assert.Equal(ModelArguments, args.Skip(separator + 1).ToArray());
        Assert.Equal("cmd /c echo hi", args[args.IndexOf("--model-command") + 1]);
        Assert.Equal("P/history.jsonl", args[args.IndexOf("--private-results") + 1]);
    }

    [Fact]
    [Trait("Slice", "264")]
    public void Start_info_with_no_bench_arguments_still_ends_with_the_separator_and_never_disables_the_no_build_start()
    {
        var layout = Layout.Create("Debug", benchOutputFor: ["Debug"]);

        var args = BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, []).ArgumentList.ToList();

        Assert.Equal("--", args[^1]);
        Assert.Contains("--no-build", args);
    }

    [Theory]
    [InlineData(@"C:\x\tests\T\bin\Debug\net8.0\", "Debug")]
    [InlineData(@"C:\x\tests\T\bin\Release\net8.0", "Release")]
    [InlineData("/x/tests/T/bin/release/net8.0/", "Release")]
    [Trait("Slice", "264")]
    public void Configuration_is_read_from_the_test_assembly_directory(string baseDirectory, string expected) =>
        Assert.Equal(expected, BenchLauncher.ConfigurationOf(baseDirectory));

    [Fact]
    [Trait("Slice", "264")]
    public void Default_overload_uses_the_running_assembly_configuration_and_no_build()
    {
        // Builds an argument list against the real checkout (the suite's own build produced the output); starts nothing.
        var args = BenchLauncher.CreateStartInfo(["run"]).ArgumentList.ToList();

        Assert.Contains("--no-build", args);
        Assert.Equal(BenchLauncher.ConfigurationOf(AppContext.BaseDirectory), args[args.IndexOf("-c") + 1]);
    }

    [Theory]
    [InlineData("Debug")]
    [InlineData("Release")]
    [Trait("Slice", "264")]
    public void Missing_output_fails_before_any_process_names_configuration_path_and_remediation_and_builds_nothing(string configuration)
    {
        var layout = Layout.Create(configuration, benchOutputFor: []);
        var before = layout.Snapshot();

        var failure = Assert.Throws<BenchLauncherException>(() => BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, ModelArguments));

        Assert.Contains(configuration, failure.Message);
        Assert.Contains(Path.Combine(layout.Root, "tools", "Phrase.Bench", "bin", configuration), failure.Message);
        Assert.Contains($"dotnet build Phrase.sln -c {configuration}", failure.Message);
        Assert.Equal(before, layout.Snapshot()); // nothing was built or created
    }

    [Fact]
    [Trait("Slice", "264")]
    public void Output_only_in_the_other_configuration_counts_as_missing()
    {
        var layout = Layout.Create("Release", benchOutputFor: ["Debug"]);

        var failure = Assert.Throws<BenchLauncherException>(() => BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, ModelArguments));

        Assert.Contains("dotnet build Phrase.sln -c Release", failure.Message);
        Assert.DoesNotContain("-c Debug", failure.Message);
    }

    [Fact]
    [Trait("Slice", "264")]
    public void Output_older_than_the_newest_source_is_stale_and_fails_the_same_way()
    {
        var layout = Layout.Create("Debug", benchOutputFor: ["Debug"]);
        layout.SetDllTime("Debug", Layout.T0);
        layout.SetSourceTime("Program.cs", Layout.T0.AddSeconds(1));
        var before = layout.Snapshot();

        var failure = Assert.Throws<BenchLauncherException>(() => BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, ModelArguments));

        Assert.Contains("stale", failure.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Debug", failure.Message);
        Assert.Contains(Path.Combine(layout.Root, "tools", "Phrase.Bench", "bin", "Debug"), failure.Message);
        Assert.Contains("dotnet build Phrase.sln -c Debug", failure.Message);
        Assert.Equal(before, layout.Snapshot());
    }

    [Fact]
    [Trait("Slice", "264")]
    public void Output_newer_than_every_source_is_fresh_and_launches()
    {
        var layout = Layout.Create("Release", benchOutputFor: ["Release"]);
        layout.SetSourceTime("Program.cs", Layout.T0);
        layout.SetDllTime("Release", Layout.T0.AddSeconds(5));

        var info = BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, ModelArguments);

        Assert.Contains("--no-build", info.ArgumentList);
    }

    [Fact]
    [Trait("Slice", "264")]
    public void Output_with_exactly_the_newest_source_time_is_fresh()
    {
        var layout = Layout.Create("Release", benchOutputFor: ["Release"]);
        layout.SetSourceTime("Program.cs", Layout.T0);
        layout.SetDllTime("Release", Layout.T0);

        Assert.Contains("--no-build", BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, ModelArguments).ArgumentList);
    }

    [Fact]
    [Trait("Slice", "264")]
    public void Newer_files_under_bin_and_obj_do_not_make_fresh_output_stale()
    {
        var layout = Layout.Create("Release", benchOutputFor: ["Release"]);
        layout.SetSourceTime("Program.cs", Layout.T0);
        layout.SetDllTime("Release", Layout.T0.AddSeconds(5));
        layout.AddCs(Path.Combine("obj", "Release", "net8.0", "Gen.cs"), Layout.T0.AddSeconds(60));
        layout.AddCs(Path.Combine("bin", "Release", "net8.0", "Other.cs"), Layout.T0.AddSeconds(60));

        Assert.Contains("--no-build", BenchLauncher.CreateStartInfo(layout.Root, layout.TestBase, ModelArguments).ArgumentList);
    }

    // Temp layout under %TEMP%; left in place (tests here never delete). Tiny files only.
    private sealed class Layout
    {
        internal static readonly DateTime T0 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal string Root { get; }
        internal string TestBase { get; }

        private Layout(string root, string testBase) { Root = root; TestBase = testBase; }

        internal static Layout Create(string testConfiguration, string[] benchOutputFor)
        {
            var root = Path.Combine(Path.GetTempPath(), "bench-launcher-" + Guid.NewGuid().ToString("N"), "repo");
            var bench = Path.Combine(root, "tools", "Phrase.Bench");
            var testBase = Path.Combine(root, "tests", "Avowry.Acceptance.Tests", "bin", testConfiguration, "net8.0");
            Directory.CreateDirectory(bench);
            Directory.CreateDirectory(testBase);
            File.WriteAllText(Path.Combine(root, "Phrase.sln"), "");
            File.WriteAllText(Path.Combine(bench, "Phrase.Bench.csproj"), "<Project/>");
            File.WriteAllText(Path.Combine(bench, "Program.cs"), "class P {}");
            foreach (var f in Directory.GetFiles(bench)) File.SetLastWriteTimeUtc(f, T0);
            foreach (var cfg in benchOutputFor)
            {
                var dir = Path.Combine(bench, "bin", cfg, "net8.0");
                Directory.CreateDirectory(dir);
                var dll = Path.Combine(dir, "Phrase.Bench.dll");
                File.WriteAllText(dll, "");
                File.SetLastWriteTimeUtc(dll, T0.AddSeconds(10));
            }
            return new Layout(root, testBase);
        }

        internal void SetDllTime(string configuration, DateTime utc) =>
            File.SetLastWriteTimeUtc(Path.Combine(Root, "tools", "Phrase.Bench", "bin", configuration, "net8.0", "Phrase.Bench.dll"), utc);

        internal void SetSourceTime(string file, DateTime utc) =>
            File.SetLastWriteTimeUtc(Path.Combine(Root, "tools", "Phrase.Bench", file), utc);

        internal void AddCs(string relative, DateTime utc)
        {
            var path = Path.Combine(Root, "tools", "Phrase.Bench", relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "");
            File.SetLastWriteTimeUtc(path, utc);
        }

        internal string[] Snapshot() =>
            Directory.EnumerateFileSystemEntries(Root, "*", SearchOption.AllDirectories)
                .Select(p => p + "|" + (File.Exists(p) ? File.GetLastWriteTimeUtc(p).Ticks : 0)).Order().ToArray();
    }
}
