# Card 430 sign-off, card-430 at a659658ca67889c718bad20a7a66c3d459f71536

VERDICT: SIGN-OFF. All six scenarios are met on the evidence. The sign-off run is a valid [S] reading: Status!=Pending took 836 s against the 900 s ceiling on a verified quiet machine, all counts pass, and no tracked file changed. The guard code is identical to a5ca65f (a659658 only adds gate records and MOB.md after it). Seat: sign-off, claude sonnet = claude-sonnet-5-5, D266, ctx-5.

| Scenario | Evidence | Met |
|---|---|---|
| [Z][O] none named for a clean class; one `ElapsedMilliseconds` class without the collection is named | Criterion 430-1 rows (4) pass in Slice=430 (358 of 358); the 25 card-417 rows pass (25 of 25) | met |
| [M] each listed spelling is named (variable limit, InRange, reversed, no-space `<`, CTS FromMinutes / variable / `new(...)` / CancelAfter, WaitAsync, WhenAny+Delay, HttpClient Timeout) | 160 round-0 corpus rows plus 196 fix-round neighbour rows all pass; red record: 57 old-guard misses failed by assertion, then passed at green; review-2 reports 160 of 160 and 196 of 196 | met |
| [B] near misses unchanged (comments, strings, `--timeout-ms`, `WaitForExitAsync`, other or misspelled collection name) | process and collection family rows pass; review-2: no old row changed outcome, 23 of 23 committed 417 rows pass | met |
| [I] SkippableFact/Theory, record and record class, nested class judged with parent's collection | Criterion 430-4 testtype rows pass (e.g. nested-under-isolated, nested-record-under-isolated, record-decl-only). Ruling 3: xUnit not applying an outer collection to nested classes is a named limit, filed on #443 | met (named limit: nested-collection gap, #443) |
| [E] the repo's own hard-limit tests are isolated and the guard names none | In my clone's source, all nine classes carry `Collection("Isolated timing")` directly above the class: TasksOverHttp, GeneratedApiHazards, EndpointFailureMapsToHttpStatus, GetEndpointsCompile, PutAndDeleteEndpointsCompile, GeneratedApiBoundAddress, SourceMapDeployment, GeneratedApiBoundAddressEdges, StarvationProbe (7 per ruling 2 plus 2 per R1-1). Slice=417 live-tree facts L1 (no class outside the collection), L2, L3 pass, so the guard names no class in the live tree | met |
| [S] Status!=Pending under the heavy lock, below 15 min, outputs unchanged | 836 s (13.92 min Total time) against 900 s, on a quiet machine; 1369 of 1369 pass; `git status --short` in my clone after the run printed nothing | met |

Named limits, not gaps (no rows, on #443): R1-1 helper or child-process limits; R1-4 and R2-1 (the five round 2 MINOR findings, including whitespace between `WaitAsync(` and a token argument naming the class, which is a loud false name and absent from the repository); ruling 4 spellings (`[Fact(Timeout=...)]`, local elapsed alias, `Assert.False(... > limit)`, bare DateTime difference, undeclared client, very long initializers); ruling 3.

## [S] reading
- Quiet-machine check (lock `free` and 0 play-tracker testhost processes): first check at 2026-11-15T19:22:48Z was already quiet, so no waiting was needed. Machine quiet from 19:22:48Z.
- Gate run: Slice=430 from 19:23:59Z, Slice=417 from 19:24:03Z, Status!=Pending from 19:24:06Z, ended about 19:38:02Z.
- Status!=Pending wall 836 s (log Total time 13.9169 min) against the 900 s ceiling: under, with 64 s of margin. Green gate was 810 s (13.47 min); the invalid fix1 reading (1301 s) was under load.
- After the run: heavy lock check printed `free`, and 0 play-tracker testhost processes were running. I saw none before, during (at my checks) or after.

## Counts (from the filter logs)
- build: exit 0, 0 Warning(s), 0 Error(s)
- Slice=430: Total tests 358, Passed 358, Failed 0, Total time 2.4872 s
- Slice=417: Total tests 25, Passed 25, Failed 0, Total time 1.1379 s
- Status!=Pending: Total tests 1369, Passed 1369, Failed 0, Total time 13.9169 min
- dotnet 10.0.404 before and after; the SDK folders unchanged.
- Run records: `.mob/records/430/gate-signoff/` in wt-430 (written by the gate script; I committed nothing).
