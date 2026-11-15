SIGNED OFF

Card 441 sign-off (claude sonnet = claude-sonnet-5-5, pinned with the Agent model flag, ctx-5, a fresh context and a fresh clone, branch card-441). Written from the seat's report as relayed in the facilitator session.
HEAD tested: b2b9d3f (`git diff --stat 8be01b7 HEAD -- tools/ tests/` empty: the code and tests of the fix-round-2 green).

Commands and summary lines
- dotnet --version before: 10.0.404; after: 10.0.404 (equal)
- dotnet build Avowry.sln -c Debug: Build succeeded. 0 Warning(s), 0 Error(s)
- Slice=441: Passed! Failed: 0, Passed: 161, Total: 161
- Slice=439|Slice=435: Passed! Failed: 0, Passed: 341, Total: 341
- Slice=439 (detailed logger): Total tests: 229, Passed: 229
- Status!=Pending (the affected module, under Avowry's heavy lock): Passed! Failed: 0, Passed: 1011, Total: 1011 (15 m 19 s)
- git diff --stat 275534b HEAD -- tools/: tools/Avowry.Bench/PathScrub.cs only (151 insertions, 35 deletions)
- No Status=Pending trait left in PathScan441.cs

Sweep lines
- sweep=B extended lists seed 439 inputs=100000 leaks_base=45965 leaks_new=4559 regressions=0 second_pass=0 exclusions=0
- sweep=C review shapes seed 4391 inputs=100000 leaks_base=50216 leaks_new=2679 regressions=0 second_pass=0 exclusions=0

Lock lines
- acquire avowry play-tracker: acquired (first try)
- release avowry play-tracker: released (the script's exit trap)

Claims vs measured: the counts match gate-green3 and green-record.md; the sweeps match the records; the named limits and rulings are recorded as accepted or as follow-up work in slice-rulings.md; review-3.md's MINORs, round 1's MINORs 4 to 7, round 2's MINOR 5 and the named limit go to the follow-up card (Avowry #442). No mismatches. The full suite was not run (the cross-mob heavy-run rule's light form); the affected module ran instead.
