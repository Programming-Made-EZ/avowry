using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

public class BenchmarkHarnessAcceptance
{
    [Fact]
    public void v1_benchmark_harness_files_exist_for_the_cli_contract()
    {
        var suite = RepoPaths.Find("bench", "suite.json");
        var runner = RepoPaths.Find("tools", "Phrase.Bench", "Phrase.Bench.csproj");
        var resultsDir = RepoPaths.Find("bench", "results");
        var history = RepoPaths.Find("bench", "results", "history.jsonl");

        Assert.True(File.Exists(suite), $"Missing benchmark suite: {suite}");
        Assert.True(File.Exists(runner), $"Missing benchmark runner: {runner}");
        Assert.True(Directory.Exists(resultsDir), $"Missing benchmark results directory: {resultsDir}");
        Assert.True(File.Exists(history), $"Missing benchmark history file: {history}");
    }
}
