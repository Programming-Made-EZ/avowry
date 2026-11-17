using System.Diagnostics;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: one shared place starts and stops nested dotnet processes in tests
//   As Morgan, I want the environment scrub, node-reuse settings, timeout and tree kill written once,
//   so that a copy can never drift and a timeout never leaves a child process behind.
// Card 172 criterion 2 (settings and source scans; the timeout and tree-kill proof is NestedDotnetTimeout172).
// The scans are offline text reads of this project's sources; comments are removed before matching, string literals are kept.
public class NestedDotnetHelper172
{
    private static readonly string[] Listed =
    [
        "Support/DotnetCli.cs", "Support/GeneratedApi.cs", "Support/PhraseCli.cs", "Support/ProcessRunner.cs",
        "Support/NoMsbuildNodeReuse.cs", "SourceMapDeployment.cs", "SourceMapTraceCommand.cs",
    ];

    private const string HelperFile = "Support/NestedDotnet.cs";

    private static readonly string[] ScrubList =
    [
        "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly",
        "OutputPath", "OutDir", "BaseOutputPath", "BaseIntermediateOutputPath", "IntermediateOutputPath",
        "Configuration", "TargetFramework", "UseAppHost",
    ];

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-2")]
    public void The_shared_helper_applies_node_reuse_roll_forward_and_scrubs_every_listed_variable()
    {
        // Given a start info that inherited every variable the outer test host may set
        var info = new ProcessStartInfo("dotnet");
        foreach (var name in ScrubList)
            info.Environment[name] = "inherited-from-the-outer-host";
        info.Environment["MSBUILDDISABLENODEREUSE"] = "0";
        info.Environment.Remove("DOTNET_ROLL_FORWARD");

        // When the shared helper prepares it
        NestedDotnetSeam172.Apply(info);

        // Then node reuse is off, roll-forward allows a newer runtime, and none of the scrubbed variables survives
        Assert.Equal("1", info.Environment["MSBUILDDISABLENODEREUSE"]);
        Assert.Equal("Major", info.Environment["DOTNET_ROLL_FORWARD"]);
        Assert.All(ScrubList, name => Assert.False(info.Environment.ContainsKey(name), $"{name} was not scrubbed"));
    }

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-2")]
    public void The_shared_helper_leaves_unrelated_variables_and_arguments_alone()
    {
        // Given a start info with its own argument and variable
        var info = new ProcessStartInfo("dotnet") { WorkingDirectory = Path.GetTempPath() };
        info.ArgumentList.Add("--version");
        info.Environment["AVOWRY_172_KEEP"] = "kept";

        // When the shared helper prepares it
        NestedDotnetSeam172.Apply(info);

        // Then those are untouched
        Assert.Equal("kept", info.Environment["AVOWRY_172_KEEP"]);
        Assert.Equal(new[] { "--version" }, info.ArgumentList.ToArray());
        Assert.Equal(Path.GetTempPath(), info.WorkingDirectory);
    }

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-2")]
    public void The_listed_helpers_call_the_shared_helper()
    {
        // Given the helpers that start nested dotnet processes
        var missing = new[]
        {
            "Support/DotnetCli.cs", "Support/GeneratedApi.cs", "Support/PhraseCli.cs", "SourceMapDeployment.cs", "SourceMapTraceCommand.cs",
        }.Where(file => !Code(file).Contains("NestedDotnet", StringComparison.Ordinal)).ToList();

        // Then every one of them uses NestedDotnet
        Assert.True(File.Exists(Source(HelperFile)), $"{HelperFile} does not exist.");
        Assert.True(missing.Count == 0, "These helpers do not call NestedDotnet: " + string.Join(", ", missing));
    }

    [Theory]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-2")]
    [InlineData("MSBUILD_EXE_PATH")]
    [InlineData("MSBuildSDKsPath")]
    [InlineData("IntermediateOutputPath")]
    [InlineData("UseAppHost")]
    [InlineData("MSBUILDDISABLENODEREUSE")]
    [InlineData("DOTNET_ROLL_FORWARD")]
    public void The_scrub_list_and_the_node_reuse_settings_are_defined_once_in_the_shared_helper(string literal)
    {
        // Given the helper and every listed copy
        var quoted = "\"" + literal + "\"";
        var counts = Listed.Append(HelperFile)
            .Where(file => File.Exists(Source(file)))
            .Select(file => (File: file, Count: Regex.Matches(Code(file), Regex.Escape(quoted)).Count))
            .Where(entry => entry.Count > 0)
            .ToList();

        // Then the literal is written exactly once, and that is in the shared helper
        Assert.True(
            counts is [(HelperFile, 1)],
            $"{quoted} must appear exactly once, in {HelperFile}; found: " + string.Join(", ", counts.Select(entry => $"{entry.File} x{entry.Count}")));
    }

    [Fact]
    [Trait("Slice", "172")]
    [Trait("Criterion", "172-2")]
    public void No_listed_helper_kills_a_process_itself_and_the_shared_helper_kills_the_whole_tree()
    {
        // Given the listed copies and the shared helper
        var killers = Listed
            .Where(file => File.Exists(Source(file)) && Regex.IsMatch(Code(file), @"\.\s*Kill\s*\("))
            .ToList();

        // Then none of the listed copies calls Kill( and the shared helper kills with the process tree
        Assert.True(killers.Count == 0, "These helpers still call Kill( themselves: " + string.Join(", ", killers));
        Assert.True(File.Exists(Source(HelperFile)), $"{HelperFile} does not exist.");
        Assert.Matches(@"\.\s*Kill\s*\(\s*entireProcessTree\s*:\s*true\s*\)", Code(HelperFile));
    }

    private static string Source(string relative) =>
        RepoPaths.Find("tests", "Avowry.Acceptance.Tests", relative.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>The file with line and block comments removed (string literals kept).</summary>
    private static string Code(string relative)
    {
        var path = Source(relative);
        Assert.True(File.Exists(path), $"{relative} does not exist.");
        var text = File.ReadAllText(path);
        text = Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        return Regex.Replace(text, @"(?<!:)//[^\n]*", string.Empty);
    }
}
