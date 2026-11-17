using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Card 165 red: avowry trace accepts a step id as its query (an exact match on a record's stepId), besides the existing names and frames.
// The map comes from the CLI's own build of the Tasks demo; no generated API is started here.
public class FailureObserverTrace165 : IClassFixture<PristineTasks165>
{
    private readonly PristineTasks165 tasks;

    public FailureObserverTrace165(PristineTasks165 tasks) => this.tasks = tasks;

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-2")]
    public async Task Trace_resolves_a_step_id_to_its_record_for_every_failure_and_for_the_other_kinds()
    {
        // Given the six failure records of the Tasks map and the first endpoint, step and avow record
        var picks = tasks.Records.Where(record => record.Kind == "failure")
            .Concat(new[] { "endpoint", "step", "avow" }.Select(kind => tasks.Records.First(record => record.Kind == kind)))
            .ToArray();
        Assert.Equal(9, picks.Length);

        foreach (var record in picks)
        {
            // When Morgan gives trace only the step id
            var trace = await PhraseCli.RunAsync("trace", record.StepId, "--map", tasks.Map);

            // Then it prints exactly the records carrying that id, with the avow file, line and endpoint
            Assert.True(trace.ExitCode == 0, $"trace {record.StepId}: {trace.Text}");
            Assert.Contains($"stepId {record.StepId}", trace.Stdout, StringComparison.Ordinal);
            Assert.Contains($"{record.SourceFile}:{record.Line} endpoint {record.Endpoint}", trace.Stdout, StringComparison.Ordinal);
            var expectedRecords = tasks.Records.Count(candidate => candidate.StepId == record.StepId);
            Assert.Equal(expectedRecords, FailureObserver165.Occurrences(trace.Stdout, "stepId "));

            // And for a failure that line is the `fails with` declaration read independently from the avow source
            if (record.Kind == "failure")
            {
                var calls = FailureObserver165.FailingCalls.Where(call => call.Code == record.Name && call.Endpoint == record.Endpoint).ToArray();
                Assert.True(calls.Length == 1, $"{record.Name} from {record.Endpoint} is not one of the six failing requests.");
                Assert.Equal(FailureObserver165.ExpectedLine(tasks.SourceLines, calls[0]), record.Line);
            }
        }
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-2")]
    [Trait("Kind", "BornGreen")]
    public async Task A_step_id_is_an_exact_match_only_so_a_prefix_or_an_extended_id_is_unknown()
    {
        // Given a real failure step id
        var id = tasks.Records.First(record => record.Kind == "failure").StepId;

        // When trace is given only a prefix of it, and the id with one more character
        var prefix = await PhraseCli.RunAsync("trace", id[..8], "--map", tasks.Map);
        var extended = await PhraseCli.RunAsync("trace", id + "0", "--map", tasks.Map);

        // Then neither resolves to a record
        Assert.Equal(1, prefix.ExitCode);
        Assert.Contains("Unknown trace name or frame", prefix.Stderr, StringComparison.Ordinal);
        Assert.Equal(1, extended.ExitCode);
        Assert.Contains("Unknown trace name or frame", extended.Stderr, StringComparison.Ordinal);
    }
}
