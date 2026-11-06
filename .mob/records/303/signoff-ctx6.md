# SIGN-OFF: Card 303 (ctx-6, Claude, a fresh subagent context that wrote neither the red nor the green; read-only)

Report as received (labels rerun / record-only per D190), condensed by the facilitator without changing a finding:

**1. Done checks: all hold.**
- Check 1 (cause named): holds. cause-probe.txt gives the exact `dotnet test` command without `--verbosity quiet`; the testhost exits with `Framework: 'Microsoft.AspNetCore.App', version '8.0.0' (x64)` not found, 10.0.14 only (record-only). runtimes.txt and ci-comparison.txt (CI 42146757413 attempts 1 and 2) committed (record-only). Its own `dotnet --list-runtimes` (rerun) now lists Microsoft.AspNetCore.App 8.0.33.
- Check 2 (pass locally after the install): holds. rerun of the focused `dotnet test ... endpoint_reply` (TEMP/TMP set to a short folder): Passed 2 of 2, 46 s. The failure before the install with a naming message is record-only (the probe's host error); the Codex seat's pre-install run (314 passed, 2 failed) is record-only and uncommitted. The 316-of-316 whole-project pass is record-only (facilitator run); the sign-off did not rerun it.
- Check 3 (CI on Linux): holds. rerun: `gh run view 42192200361` success (usable-v1, test, mob-process); `gh run list` four latest main runs success; Linux test job 127806682650 both endpoint tests Passed. rerun: it diffed the per-test lists of that job against the earlier Linux job 127659984503 (run 42146757413 attempt 1): no failures in either; the only difference is one added line, the new test (Passed). Attempt 2's job was not diffed.

**2. Invariants and scope (rerun, `git diff a408f12 HEAD -- tools tests`).** Two files: CSharpBaselineAcceptance.cs (one new test, one helper RunWithDotnet; no existing assertion touched) and Program.cs (7 lines). Nothing for UsableV1.Bar6; no skip or OS condition in the product path (the only OS branch is the red test's choice of a .cmd or .sh stand-in, which ran on both). The seam is read once at the one `dotnet test` start; unset or blank means dotnet. The non-endpoint branch `!endpoint && result.ModelTests == 0` still returns the bare label (the existing assertion expects it; passed in CI). The red is machine-independent (read; passed on Linux CI and Windows). Side effect: the harness-short branch (`HarnessTests < outcomes.Count`) now returns the captured output for non-endpoint tasks too, and the output is NOT capped (`output.Trim()` is unbounded, where CompactFailureSummary caps at 600 characters), so three attempts of verbose output could bloat the result JSON.

**3. Delete scan of the Codex seat (rerun).** 15 command_execution commands, matching the facilitator's report; zero matches for rm, rmdir, del, erase, Remove-Item, rd, unlink, git clean, Directory.Delete, File.Delete; no recursive delete on a variable path.

**4. Seat discipline.** rerun: Co-Authored-By count 0; CR count 0. record-only: the facilitator wrote nothing during the Codex turn.

**5. Conditions before close (none blocking).**
- A: record that the 316-of-316 pass is record-only (facilitator run) and that "Linux lists unchanged" rests on one earlier job's diff (attempt 1). Done in this file and in the closure record.
- B: a follow-up for the uncapped diagnostic (cap like CompactFailureSummary; note it applies to non-endpoint tasks). Filed by the facilitator as a bug card (see the closure record).

**Retro pointers:** the red b9e38ca was local-only and pushed with the green (check whether any CI run was red on a pushed red); the Muse and Grok seat failures and Grok's exclusion; Codex as build seat (15 commands, no deletes, commit author is the machine identity, delete scan was a hand read until card 305).

Verdict: SIGNED OFF with conditions (A: label the 316 pass and the one-attempt Linux diff as record-only; B: follow-up for the uncapped diagnostic).
