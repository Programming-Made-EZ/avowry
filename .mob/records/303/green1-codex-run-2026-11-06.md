# Card 303 green 1: the first real-card Codex build seat (2026-11-06)

Seat: Codex ctx-4 (gpt-5.6-sol), `codex exec --json` through the machine-local launcher, sandbox `danger-full-access`, started 08:32 UTC, ended about 09:05 UTC (the facilitator wrote nothing while it ran). Brief: .mob/records/303 slice file plus the rules (no recursive delete on a variable path, LF, explicit `git add` paths, no trailers). Event stream: .mob/records/303/green-codex-events.jsonl (15 command_execution events).

## Result of the end-to-end test the product owner asked for (commit and push from a Codex build seat under the single-writer lock)
- Edit: tools/Phrase.BenchCs/Program.cs (seam PHRASE_BENCHCS_DOTNET; zero-harness diagnostic carries the captured output), plus the three probe records. Diff is 7 lines in Program.cs, no CR bytes (`git show HEAD | grep -c $'\r'` gave 0).
- `git add` with explicit paths and `git commit` worked in the mob's own checkout: commit 30c9e84 (red 1 is b9e38ca, committed by the facilitator before the seat).
- `git push origin main` worked from the seat: `a408f12..30c9e84 main -> main`, through the pre-push hook (mob-process: OK).
- **Verdict: Codex can edit, commit and push in a build seat of a real card.** One thing to know: the commit author is the machine's git identity (Steve Bishop), same as the other seats' commits.

## Delete check by the facilitator (the product owner's rule; card 305 not yet built)
The `item.completed` / command_execution events were scanned by the facilitator for rm, rmdir, del, erase, Remove-Item, rd, unlink and `git clean`: none found in the 15 commands. (This is a facilitator read, not the delete scan; the scan does not cover Codex until card 305.)

## Facilitator's checks of the seat's claims (D190 labels)
- Program.cs diff read in full (rerun: `git show HEAD -- tools/Phrase.BenchCs/Program.cs`): as reported.
- The seat's test totals (focused: 1 passed; whole acceptance project with `--filter "Status!=Pending"`: 314 passed, 2 failed, 316 total) are record-only (the seat's own run; saved by it under its TEMP, not committed). The 2 failures are the two endpoint tests, per the seat's report.
- The probe's host error is quoted in cause-probe.txt: the test host exits because Microsoft.AspNetCore.App 8.0.0 (x64) is not installed; only 10.0.14 is present. That settles Done check 1's cause on this machine.
