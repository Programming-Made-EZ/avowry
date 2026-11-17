using System.Net;
using System.Text.RegularExpressions;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Card 165 red, live half. The Tasks demo is built by the CLI, given a test observer (a file added to Tasks.Api and two calls added to its
// generated Program.cs, see InstrumentedTasks165), built once, and started once per observer mode. Each start sends the same six failing
// requests (four declared codes, NotFound from three endpoints), every one carrying the marker "req-secret" so a leak is visible.
//   none    no observer registered by the host: the generated no-op must be the one the host resolves
//   record  the test observer appends each event as JSON to a file
//   throw   the test observer records, then throws
//   slow    the test observer sleeps five seconds, then records
//   log     the test observer records and writes one plain log line to stdout
// Without the generated runtime types the instrumented project does not build, so every test here fails today with the build log.
[Collection("Isolated timing")]
public class FailureObserverLive165 : IClassFixture<InstrumentedTasks165>
{
    // A response that waits on a five-second observer is not a response that is "not delayed by more than a small bound".
    private static readonly TimeSpan SlowObserverBound = TimeSpan.FromSeconds(2);

    private static readonly Regex LogLine = new(
        @"avowry-failure v1 code=(\w+) endpoint=(\w+) stepId=([0-9a-f]{12}) buildId=(\w+)",
        RegexOptions.CultureInvariant);

    private readonly InstrumentedTasks165 tasks;

    public FailureObserverLive165(InstrumentedTasks165 tasks) => this.tasks = tasks;

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    public async Task With_no_observer_registered_the_host_uses_the_no_op_and_answers_the_declared_422s()
    {
        // Given the generated host and no observer registered by the host
        // When the six failing requests are sent
        var probe = await tasks.ProbeAsync("none");

        // Then the host resolved the generated no-op, every request is the declared 422, and nothing was recorded
        Assert.Contains("observer-type=NoOpAvowryFailureObserver", probe.Log, StringComparison.Ordinal);
        AssertDeclared422s(probe);
        Assert.Empty(probe.Events);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    public async Task An_observer_the_host_registered_first_is_kept_and_called_once_per_failure()
    {
        // Given a host that registered its own observer before the generated default
        var probe = await tasks.ProbeAsync("record");

        // Then the host's observer, not the no-op, is the one resolved and it received one event per failing request
        Assert.Contains("observer-type=TestFailureObserver", probe.Log, StringComparison.Ordinal);
        Assert.Equal(FailureObserver165.FailingCalls.Length, probe.Events.Length);
        AssertDeclared422s(probe);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    public async Task A_throwing_observer_leaves_every_status_and_body_unchanged_and_the_api_running()
    {
        // Given the same requests with no observer and with an observer that throws
        var none = await tasks.ProbeAsync("none");
        var thrown = await tasks.ProbeAsync("throw");

        // Then the observer really ran for every failure, and yet each status and body is the one the no-op host answered
        Assert.Equal(FailureObserver165.FailingCalls.Length, thrown.Events.Length);
        AssertSameResponses(none, thrown);
        foreach (var response in thrown.Responses)
            Assert.DoesNotContain("observer-secret-boom", response.Body, StringComparison.Ordinal);

        // And the process is still up and still answers
        Assert.True(thrown.Alive, "The API process is gone after an observer threw." + Environment.NewLine + thrown.Log);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, thrown.AfterStatus);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    public async Task A_five_second_observer_neither_delays_nor_changes_the_response()
    {
        // Given the same requests with no observer and with an observer that sleeps five seconds
        var none = await tasks.ProbeAsync("none");
        var slow = await tasks.ProbeAsync("slow");

        // Then every response came back inside the small bound, with the status and body the no-op host answered
        foreach (var response in slow.Responses)
            Assert.True(response.Elapsed < SlowObserverBound,
                $"{response.Call.Endpoint} {response.Call.Code} took {response.Elapsed.TotalSeconds:0.0} seconds with a five-second observer.");
        AssertSameResponses(none, slow);

        // And the slow observer was still called for every failure, after the responses, and the API still answers
        Assert.Equal(FailureObserver165.FailingCalls.Length, slow.Events.Length);
        Assert.True(slow.Alive, "The API process is gone after a slow observer." + Environment.NewLine + slow.Log);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, slow.AfterStatus);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-1")]
    public async Task The_event_has_exactly_the_five_neutral_fields_and_carries_no_request_data_or_source_name()
    {
        // Given the events a recording observer received for the six failing requests
        var probe = await tasks.ProbeAsync("record");
        Assert.Equal(FailureObserver165.FailingCalls.Length, probe.Events.Length);

        foreach (var observed in probe.Events)
        {
            // Then the event has schema version 1, the code, the endpoint, a 12-character opaque step id and the build id, and no other field
            Assert.Equal(FailureObserver165.EventFields.Order(StringComparer.Ordinal), observed.Names.Order(StringComparer.Ordinal));
            Assert.Equal(1, observed.SchemaVersion);
            Assert.Matches("^[0-9a-f]{12}$", observed.StepId);
            Assert.NotEmpty(observed.BuildId);

            // And nothing of the request, the exception or the Phrase source is in it
            Assert.DoesNotContain(FailureObserver165.RequestMarker, observed.Raw, StringComparison.Ordinal);
            Assert.DoesNotContain("avowry", observed.Raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("boom", observed.Raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotMatch(@"\.avow|spec ""|title|rejected", observed.Raw);
        }
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-2")]
    public async Task A_recording_observer_gets_one_event_per_declared_failure_that_resolves_through_the_map_to_the_exact_line_and_endpoint()
    {
        // Given a tracing observer that recorded the events of the four declared failures (six endpoint cases)
        var probe = await tasks.ProbeAsync("record");
        Assert.Equal(FailureObserver165.FailingCalls.Length, probe.Events.Length);
        Assert.Equal(
            new[] { "EmptyTitle", "NotFound", "StoreUnavailable", "TitleTooLong" },
            probe.Events.Select(observed => observed.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());

        foreach (var call in FailureObserver165.FailingCalls)
        {
            // When each event is resolved by its step id through the source map the CLI wrote
            var matches = probe.Events.Where(observed => observed.Code == call.Code && observed.Endpoint == call.Endpoint).ToArray();
            Assert.True(matches.Length == 1, $"Expected one {call.Code} event from {call.Endpoint}, got {matches.Length}.");
            var records = tasks.Records.Where(record => record.StepId == matches[0].StepId).ToArray();
            Assert.True(records.Length == 1, $"Step id {matches[0].StepId} resolves to {records.Length} map records.");

            // Then it is that endpoint's failure record, at the line of the `fails with` declaration read from the avow source
            var resolved = records[0];
            Assert.Equal("failure", resolved.Kind);
            Assert.Equal(call.Code, resolved.Name);
            Assert.Equal(call.Endpoint, resolved.Endpoint);
            Assert.Equal("Tasks.avowry", resolved.SourceFile);
            Assert.Equal(FailureObserver165.ExpectedLine(tasks.SourceLines, call), resolved.Line);
        }
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-2")]
    public async Task The_event_build_id_is_the_map_build_id_and_the_assembly_metadata()
    {
        // Given the events of a recording observer
        var probe = await tasks.ProbeAsync("record");
        Assert.NotEmpty(probe.Events);

        // Then every event carries the map's build id
        Assert.All(probe.Events, observed => Assert.Equal(tasks.BuildId, observed.BuildId));

        // And trace agrees the map belongs to the built assembly, whose AvowryBuildId metadata is that same value
        var trace = await PhraseCli.RunAsync("trace", "EmptyTitle", "--map", tasks.Map, "--assembly", tasks.Assembly);
        Assert.True(trace.ExitCode == 0, trace.Text);
        Assert.Contains("map matches assembly", trace.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-2")]
    public async Task The_plain_logging_observer_writes_a_production_line_that_trace_resolves_offline()
    {
        // Given a plain logging observer whose output is one line per failure, and the API already stopped
        var probe = await tasks.ProbeAsync("log");
        var lines = LogLine.Matches(probe.CallsLog).Select(match => match).ToArray();
        Assert.Equal(FailureObserver165.FailingCalls.Length, lines.Length);

        foreach (var line in lines)
        {
            var code = line.Groups[1].Value;
            var endpoint = line.Groups[2].Value;
            var stepId = line.Groups[3].Value;

            // Then the line carries no request data
            Assert.DoesNotContain(FailureObserver165.RequestMarker, line.Value, StringComparison.Ordinal);

            // And its step id alone, given to avowry trace with the map, names the failure, the avow file and line, and the endpoint
            var matching = FailureObserver165.FailingCalls.Where(call => call.Code == code && call.Endpoint == endpoint).ToArray();
            Assert.True(matching.Length == 1, $"The log line names {code} from {endpoint}, which is not one of the six failing requests.");
            var expectedLine = FailureObserver165.ExpectedLine(tasks.SourceLines, matching[0]);
            var trace = await PhraseCli.RunAsync("trace", stepId, "--map", tasks.Map);
            Assert.True(trace.ExitCode == 0, trace.Text);
            Assert.Contains($"{code}  failure  422  stepId {stepId}", trace.Stdout, StringComparison.Ordinal);
            Assert.Contains($"Tasks.avowry:{expectedLine} endpoint {endpoint}", trace.Stdout, StringComparison.Ordinal);
        }
    }

    [Fact]
    [Trait("Slice", "165")]
    [Trait("Criterion", "165-3")]
    public async Task The_422_bodies_are_byte_identical_whatever_observer_is_registered_and_hold_none_of_the_event()
    {
        // Given the same requests answered with no observer, a recording, a throwing, a slow and a logging observer
        var none = await tasks.ProbeAsync("none");
        var record = await tasks.ProbeAsync("record");
        var others = new[] { record, await tasks.ProbeAsync("throw"), await tasks.ProbeAsync("slow"), await tasks.ProbeAsync("log") };

        // Then every body equals the no-observer body byte for byte
        foreach (var other in others)
            AssertSameResponses(none, other);

        // And each body is the declared failure under generalErrors and nothing of the event (step id, build id, endpoint name as event data)
        foreach (var response in none.Responses)
            Assert.Equal(new[] { response.Call.Code }, FailureObserver165.GeneralErrors(response.Body));
        foreach (var observed in record.Events)
        {
            foreach (var response in record.Responses)
            {
                Assert.DoesNotContain(observed.StepId, response.Body, StringComparison.Ordinal);
                Assert.DoesNotContain(observed.BuildId, response.Body, StringComparison.Ordinal);
            }
        }
    }

    private static void AssertDeclared422s(ObserverProbe165 probe)
    {
        Assert.Equal(FailureObserver165.FailingCalls.Length, probe.Responses.Length);
        foreach (var response in probe.Responses)
        {
            Assert.True(
                response.Status == HttpStatusCode.UnprocessableEntity,
                $"{response.Call.Endpoint} {response.Call.Code} answered {(int)response.Status}: {response.Body}");
            Assert.Equal(new[] { response.Call.Code }, FailureObserver165.GeneralErrors(response.Body));
        }
    }

    private static void AssertSameResponses(ObserverProbe165 expected, ObserverProbe165 actual)
    {
        Assert.Equal(expected.Responses.Length, actual.Responses.Length);
        for (var index = 0; index < expected.Responses.Length; index++)
        {
            var label = $"{expected.Responses[index].Call.Endpoint} {expected.Responses[index].Call.Code} ({actual.Mode} observer)";
            Assert.True(expected.Responses[index].Status == actual.Responses[index].Status, $"{label}: status {actual.Responses[index].Status}.");
            Assert.True(
                string.Equals(expected.Responses[index].Body, actual.Responses[index].Body, StringComparison.Ordinal),
                $"{label}: body differs. Expected {expected.Responses[index].Body} but got {actual.Responses[index].Body}");
        }
    }
}
