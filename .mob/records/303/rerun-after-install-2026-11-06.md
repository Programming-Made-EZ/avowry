# Card 303: reruns after the product owner's runtime install (2026-11-06, facilitator, HEAD 3b0 see git log)

Product-owner machine action (reported in chat): Microsoft.AspNetCore.App 8.0.33 (x64) installed under C:\Program Files\dotnet.

Commands the facilitator ran (D189/D190: all "rerun" unless stated), TEMP and TMP set to C:/Users/ringe/AppData/Local/Temp/claude/ready303rerun:
1. `dotnet --list-runtimes | grep -i aspnet` gave `Microsoft.AspNetCore.App 8.0.33` and `Microsoft.AspNetCore.App 10.0.14`.
2. `dotnet test tests/Phrase.Acceptance.Tests --filter "FullyQualifiedName~CSharpBaselineAcceptance.endpoint_reply" --nologo` gave `Passed!  - Failed: 0, Passed: 2, Skipped: 0, Total: 2, Duration: 31 s`. Done check 2 (the two tests pass locally after the install; the install branch the card names) is met.
3. `dotnet test tests/Phrase.Acceptance.Tests --nologo --filter "Status!=Pending"` gave `Passed!  - Failed: 0, Passed: 316, Skipped: 0, Total: 316, Duration: 11 m 46 s`, exit 0. The whole project is green on the development machine for the first time in this record (the same filter CI uses; UsableV1.Bar6 is Pending and excluded, card 162).

Done check 3 (CI) evidence: CI run 42192200361 on green 1 (30c9e84): conclusion success, jobs usable-v1, test, mob-process all success (record-only for the gate: read from `gh run view 42192200361`). Per-test outcome lists for Linux compared with the earlier run 42146757413 attempts 1 and 2 (ci-comparison.txt): both endpoint tests passed on Linux there; the per-test list for 42192200361 is checked by the gate.
