# PRE-SHIP GATE: Card 303 (ctx-5 independent reviewer, Copilot auto via the Copilot CLI, read-only, brief .mob/records/303/gate-brief.md)

Facilitator's notes on the report below (facts, added after reading it against git and after running the checks myself):
- Two earlier launch attempts of this seat failed on the facilitator's command quoting (cmd /c dropped the prompt quotes: "Invalid command format"); neither reached the model. The third launch, directly through copilot.cmd from a script, produced the report.
- Done check 2 is `record-only` for the reviewer (its own `dotnet test` was blocked by the session's permission gate); the facilitator reran it: `dotnet test tests/Phrase.Acceptance.Tests --filter "FullyQualifiedName~CSharpBaselineAcceptance.endpoint_reply" --nologo` gave Passed 2 of 2 (31 s), and the whole project `--filter "Status!=Pending"` gave 316 of 316 in 11 m 46 s (rerun-after-install-2026-11-06.md, label `rerun` by the facilitator).
- The reviewer's "rerun: `dotnet --list-runtimes`" line is its own claim; the facilitator's rerun of it is in the same record (Microsoft.AspNetCore.App 8.0.33 and 10.0.14).
- Done check 3 for the last commit: the reviewer's `gh run list` read is a rerun; the CI run on the records commits after the green is read again at close.

---

Done 1 PASS [record-only]: cause-probe.txt records exact command `dotnet test ...Verification.csproj --nologo --logger "trx;LogFileName=probe.trx"` and the captured line `"Framework: 'Microsoft.AspNetCore.App', version '8.0.0' (x64)"`; runtimes.txt shows only `10.0.14` under `Microsoft.AspNetCore.App`, matching the cause.
Done 2 PASS [record-only]: rerun-after-install records `dotnet --list-runtimes` then `dotnet test tests/Phrase.Acceptance.Tests --filter "FullyQualifiedName~CSharpBaselineAcceptance.endpoint_reply" --nologo`, result `Passed! - Failed: 0, Passed: 2, Skipped: 0, Total: 2, Duration: 31 s`.
Done 3 PASS [record-only]: `gh run view 42192200361` and ci-comparison.txt record green Linux runs; per-test lines for both endpoint tests passed on Linux and match earlier attempts 1 and 2.
Extra (a) PASS [record-only]: `git diff a408f12 HEAD -- tools tests` shows no weakened/removed assertion, no skip, no OS condition, no timeout change, no EndpointVerificationProject/package version changes, and no UsableV1.Bar6 edits.
Extra (b) PASS [record-only]: `git diff a408f12 HEAD -- tools tests | grep -c $'\r'` is recorded as 0.
Extra (c) PASS [record-only]: green-codex-events.jsonl records explicit `git add -- tools/Phrase.BenchCs/Program.cs .mob/records/303/...` and no delete command (`rm`, `Remove-Item`, `del`, `rmdir`, `git clean`).
Extra (d) PASS [record-only]: commit `30c9e84` has no `Co-Authored-By:` trailer.
Extra (e) PASS [record-only]: commit order is red before green: `b9e38ca` then `30c9e84`.
Extra (f) PASS [record-only]: only change is PHRASE_BENCHCS_DOTNET seam plus zero-harness diagnostic; inside card, not a Windows-only branch.
Shell execution [blocked]: this session could not rerun `dotnet ...` or `gh ...`: `"Permission denied because no interactive user response was available"`.
Later GH check [blocked]: `gh run list --branch main --limit 3` was not rerun here; committed record-only evidence still names the green 1 run on the last commit.
Verdict: SHIP-with-record-only.



exit=0
