# Card 430: the red record

Red seat: dev, red, claude sonnet = claude-sonnet-5-5 (Decider D266), context ctx-2. The seat wrote the tests. Its build and test runs were blocked behind play-tracker's ship-train 62 (heavy lock held since 15:43:11Z, PID alive). The 16:51Z watchdog ruled the red's build and Slice-filtered runs light, so the facilitator ran them at about 16:55Z in the worktree on branch card-430 (from main dc5d326). The acceptance project's `Status!=Pending` run and the full gate stay queued behind the lock.

## What changed

- New file `tests/Avowry.Acceptance.Tests/WallClockSpellings430.cs` (1054 lines). It is a second `partial` part of `WallClockTestsRunIsolated`, so its rows call the guard's private `Find`, `Src` and `Describe`.
- The only edit to `WallClockTestsRunIsolated.cs` is the `partial` keyword on its class declaration (1 line).
- The wall-clock spellings in rows are built from pieces, so the live-tree fact does not name the new class.

## Rows

All 160 corpus.txt rows are embedded, with ids `<family>-NN`:

| Family | Rows |
|---|---|
| elapsed | 34 |
| cts | 23 |
| waitasync | 24 |
| whenany | 17 |
| httpclient | 24 |
| testtype | 21 |
| collection | 11 |
| process | 6 |

- The rows are split into Pending and characterisation theories. Theories and facts carry the Criterion traits 430-1 to 430-5.
- A completeness fact reads `.mob/records/430/corpus.txt` from the repository at test time, as card 441's PathScan441.cs does, and checks that every row is embedded.
- The [E] fact reads the real source text and checks the seven classes of ruling 2 for `Collection("Isolated timing")`: TasksOverHttp, GeneratedApiHazards, EndpointFailureMapsToHttpStatus, GetEndpointsCompile, PutAndDeleteEndpointsCompile, GeneratedApiBoundAddress and SourceMapDeployment.
- Ruling 4's named limits have no rows.

## Runs

- `dotnet build Avowry.sln -c Debug`: Build succeeded, 0 Warning(s), 0 Error(s).
- `--filter "Slice=430&Status=Pending"`: Total tests 58, Failed 58.
  - 57 are the corpus rows the old guard misses (the slice's 57).
  - The 58th is the [E] fact.
  - Every failure is an assertion failure, either "expected ..." or the guard's "a test class that asserts a wall-clock limit ..." message. There are 0 exception failures.
- `--filter "Slice=430&Status!=Pending"`: Total tests 104, Passed 104. These are the characterisation rows, the near misses and spellings today's guard already catches, plus the completeness fact.
- `--filter "Slice=417"`: Total tests 25, Passed 25, including the live-tree fact.

Pending: 58. Characterisation: 104.

## Disagreements with the reference

None found. The failing rows match the slice's section 3 family by family, as reference minus V0 passes: elapsed 15, cts 8, waitasync 7, whenany 5, httpclient 9, testtype 13, process 0, collection 0. That makes 57, plus the [E] fact.
