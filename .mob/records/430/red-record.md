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

## Fix round 1 red

Red seat: dev, red, claude sonnet = claude-sonnet-5-5 (Decider D266), context ctx-2b, a fresh context. Branch card-430, worktree wt-430, from c7f6c30. No commit made. The guard's detection code and every `[Collection]` attribute are untouched.

### What changed

- New file `tests/Avowry.Acceptance.Tests/WallClockFix430.cs`: a third `partial` part of `WallClockTestsRunIsolated`. It embeds all 196 rows of `corpus-fix.txt` (generated), ids `<family>-NN/<corpus id>`, each theory carrying `Slice=430` and a `Criterion` trait. The 1,000-space runs are built by `Spaces430(1000)` and the 12-line comment plus block-comment run by `Comments430()`; the rows still equal the corpus text. Rows are strings, so the live-tree fact does not name the class (Slice=417 passes).
- `WallClockSpellings430.cs`: the completeness fact now reads `corpus.txt` and `corpus-fix.txt` and checks every row of both is embedded (356 rows, no duplicates). The [E] fact is renamed `E_the_nine_real_classes_...`, lists nine classes (the seven plus GeneratedApiBoundAddressEdges and StarvationProbe) and carries `Status=Pending`.

### Rows by family and criterion (196)

| Family | Criterion | Rows | Pending | Characterised |
|---|---|---|---|---|
| fix-time | 430-2 (names a class) | 32 | 6 | 26 |
| fix-time | 430-3 (no name) | 14 | 0 | 14 |
| fix-waitasync | 430-2 (names a class) | 24 | 0 | 24 |
| fix-waitasync | 430-3 (no name) | 126 | 33 | 93 |

Pending: 40 (39 rows plus the [E] fact). Characterisation: 318 tests under `Slice=430&Status!=Pending`: the 161 of round 0 (160 rows plus the completeness fact, all passing since the round 0 green) plus 157 new rows.

Every expectation was checked against the fix reference (`probe430fix`, fix.out): all 196 rows agree, and the 39 Pending rows are exactly those where the round 0 guard differs from the reference. No disagreement with the slice.

### Runs

- `dotnet build Avowry.sln -c Debug`: Build succeeded, 0 Warning(s), 0 Error(s).
- `--filter "Slice=430&Status=Pending"`: Failed!  - Failed: 40, Passed: 0, Skipped: 0, Total: 40.
- `--filter "Slice=430&Status!=Pending"`: Passed!  - Failed: 0, Passed: 318, Skipped: 0, Total: 318.
- `--filter "Slice=417"`: Passed!  - Failed: 0, Passed: 25, Skipped: 0, Total: 25.

### Failing Pending ids (all by assertion, 0 exceptions)

[E] fact: `no Collection("Isolated timing") above: GeneratedApiBoundAddressEdges, StarvationProbe`

fix-time (first assertion line is `<id>: expected tests/Avowry.Acceptance.Tests/<corpus id>.cs: <class>, got:` and nothing named):
- fix-time-03/FT-init-q-after-run-sp (Probe)
- fix-time-23/FT-init-q-after-run-cm (Probe)
- fix-time-41/FT-init-q-tight (Probe)
- fix-time-42/FT-decl-q-tight (Probe)
- fix-time-43/FT-decl-q-tight-target (Probe)
- fix-time-44/FT-cts-q-tight (Capped)

fix-waitasync (first assertion line is `<id>: expected no name, got:` the guard's "a test class that asserts a wall-clock limit must carry Collection..." message naming Gate): the 33 FW-token rows, forms named, underscore-ct, linked-ct, underscore-linked-ct, member-ct, cancel, underscore-cancel, member-cancel, infinite-span, infinite-span-qualified and infinite, each in code, interp-hole and raw-hole (ids `fix-waitasync-NN/FW-token-<form>-<enc>`).
