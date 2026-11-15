**Status: refined from the card 417 second review (its findings 3, 4 and 5) into the card template, 2026-11-15.** The wall-clock guard (`tests/Avowry.Acceptance.Tests/WallClockTestsRunIsolated.cs`, card 417) names a test class that asserts a time limit unless it runs in `Collection("Isolated timing")`. It recognises only `Elapsed` or `TotalSeconds`/`TotalMilliseconds` compared with `<`/`<=`, and `new CancellationTokenSource(TimeSpan.FromSeconds/FromMilliseconds(...))`. The review found spellings it misses and hard-limit tests that are not isolated. The guard recognises text, so D263 and D265 apply: the slice commits a real-shape corpus (at least five families, each input encoding the guard meets) before the red. Today the full gate is green; only one timing test has ever failed under load. The ceiling check in [S] is a heavy run: it waits for the cross-mob heavy-run lock and runs under Avowry's own lock.

## Persona

Dana

## Story

As Dana, I want every test that asserts a time limit to be recognised by the wall-clock guard and to run in the isolated timing collection, so that a slow or loaded machine never fails the release gate by chance (goal: state the behaviour once and get a working implementation in the chosen target language).

## Acceptance criteria

- Scenario: [Z][O] Given a test class with no time-limit assertion, when the guard runs, then it names nothing; given one class that asserts a limit through `ElapsedMilliseconds` compared with a number, when the guard runs without the isolated collection, then it names that file and class.
- Scenario: [M] Given test classes that assert a limit through each of these spellings: `ElapsedMilliseconds` or `Elapsed.TotalSeconds` against a variable limit, `Assert.InRange` on an elapsed value, a reversed comparison (`limit > sw.Elapsed`), a comparison with no space after `<`, a `CancellationTokenSource` built from `TimeSpan.FromMinutes`, from a variable, with `new(...)` target typing or through `CancelAfter`, `.WaitAsync(TimeSpan...)`, `Task.WhenAny` with `Task.Delay`, and `HttpClient { Timeout = ... }`, when the guard runs, then each class without the isolated collection is named.
- Scenario: [B] Given the near misses the guard already ignores (comments, strings, a process `--timeout-ms` argument, `WaitForExitAsync` with a TimeSpan, a different or misspelled collection name), when the guard runs after this card, then their outcomes are unchanged.
- Scenario: [I] Given a `[SkippableFact]` or `[SkippableTheory]` method, a `record` or `record class` test type, and a nested test class under an isolated parent, when the guard runs, then each is seen as a test class, and a nested class asserting a limit is judged with its parent's collection as card 417 judges nested types.
- Scenario: [E] Given the repository's own tests that assert short hard limits (TasksOverHttp: `WaitAsync` 15 s and `HttpClient` timeouts of 15 and 30 s; GeneratedApiHazards: `WaitAsync` 10 s; the 30 s `HttpClient` timeouts in EndpointFailureMapsToHttpStatus, GetEndpointsCompile and PutAndDeleteEndpointsCompile), when the guard runs on the real test project, then every one carries the isolated collection and the guard names none.
- Scenario: [S] Given the acceptance project's `Status!=Pending` run, when it runs under the heavy-run lock after the isolated collection is applied, then it stays under the 15 minute ceiling and its committed outputs are unchanged.

## ZOMBIES coverage

- Z (Zero): covered
- O (One): covered
- M (Many): covered
- B (Boundaries): covered
- I (Interface): covered
- E (Exceptions): covered
- S (Simple): covered

## Dimensions of Difficulty

- Environment: no
- Scope: no
- Unknown: no
- Communication: no

## Links

- Parent epic: none
- Spikes: none
- Related: #417 (the wall-clock guard), Decider D263 and D265 (the real-shape corpus with encodings)
