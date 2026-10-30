# Card 214 pre-ship gate (D154, ctx-7, delivery lead, ship)

Run 2026-10-30 12:06 to 12:30 UTC, Claude seat (probes 12:03 UTC, .mob/records/probes/2026-10-30.jsonl: Codex, Copilot, Grok, agy without usable quota). Basis: origin/main 7c3a054 (contains 9046262 and the card's last code commit d18194c), in a `git worktree add --detach` of origin/main (removed afterwards). Conditions applied: D156 (C3: D136 limits, no ceremony beyond D153 and D154), window 214-rerank-4 (adopted), window 217-rerank-6b (adopted, f870506).

## Verdicts

| # | Item | Verdict |
|---|---|---|
| 1 | Every cited hash and record path resolves on origin | PASS (two pre-rebase hashes are mapped; two path-name drifts noted) |
| 2 | Full acceptance suite on the pushed tree | PASS (253 passed, 3 failed, all 3 environmental, none card 214) |
| 3 | CI on the shipping commit | PASS (run 41052829472, success, head 9046262 containing d18194c) |
| 4 | F-056 closure evidence | PASS (111960d removes FreeLoopbackPort; green run 41052829472 on the head holding the last card commit) |
| 5 | Thread-pool floor recorded as a mitigation naming 217 | PASS (afa4e40 floor; window 217-rerank-6b f870506; comment in d18194c) |
| 6 | Domain-expert question at ship (T2) | NO, with reason below |

Overall: PASS on all six. The ship decision stays with the facilitator.

## 1. Hashes and paths resolve on origin (PASS)

Method: `git worktree add --detach .../wt-214-gate origin/main`; hashes taken by script from the board card, record-in-progress.json, review-ctx4.md, turn-slice-architect.md and decisions 214-rerank-4, 214-slice, 214-taskforce, 217-rerank-6, 217-rerank-6b; each checked with `git cat-file -e <h>^{commit}` and `git merge-base --is-ancestor <h> origin/main`.

All of these exist and are ancestors of origin/main: ec25197 (seams), 26681e1, 1fcc793, 391828f (red), 111960d (commit 5), afa4e40 (commit 5b), d18194c (commit 5c), a477bc6 and a7e9401 (review), afec23d, e963005, 5ef3563, 9046262, f870506, 693d9ea, 326c77f, 9b5e252, a5be583, 31c22dc, a71d197, e6fe23d, c613c86, e6e2ab8, 26f82d7, fb13e3d.

Pre-rebase hashes: a22b651 and 3aed9dc exist only in the author's local object store and are NOT ancestors of origin/main (the commit 5 and 5b bodies cite them). The record's hash_map maps them: a22b651 -> 111960d (commit 5), 3aed9dc -> afa4e40 (commit 5b). Both targets are on origin and their subjects match ("Card 214 green commit 5 ... the harness owns its port ...", "Card 214 green commit 5b ... raise the thread pool floor ..."). The mapping is the resolution; the two old hashes are the only cited hashes that do not resolve by themselves.

Paths (`git cat-file -e origin/main:<path>`): OK for .mob/records/214/record-in-progress.json, review-ctx4.md, turn-slice-architect.md, .mob/records/deciders/D156-votes.json, .mob/records/decisions/214-taskforce.json (and 214-rerank-4, 214-slice, 217-rerank-6, 217-rerank-6b), .mob/records/probes/2026-10-30.jsonl, docs/decisions/process-and-staffing.md, scripts/mob_process.py, tests/Phrase.Acceptance.Tests/Support/GeneratedApi.cs and ThreadPoolFloor.cs, board/214-*.md.

Two cited paths that do not resolve literally, neither blocking: (a) several window files and 214-taskforce.json cite `.mob/records/probes/2026-10-30.json`; the file on origin is `2026-10-30.jsonl` (name drift in the citation, content present). (b) 217-rerank-6.json cites `.mob/audits/last.json`, which is gitignored by design (`.gitignore:7 .mob/*`, tick files are not committed; the seats there said so themselves); it was not touched or read here.

## 2. Full acceptance suite on the pushed tree (PASS)

Command (in the worktree at 7c3a054, code identical to d18194c: `git diff --name-only d18194c 9046262` outside .mob/, board/ and MOB.md is empty): `dotnet test tests/Phrase.Acceptance.Tests -nodeReuse:false -p:UseSharedCompilation=false --logger trx --logger "console;verbosity=normal"`, 12:07 to 12:19 UTC, one run, no other suite running alongside.

Result: Total 256, Passed 253, Failed 3, Skipped 0, total time 10.67 min.

Failures, classified with the reviewer's list (review-ctx4.md "Verified"):
- UsableV1.Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone: "Bar 6 not met: no current passing LLM evidence ... grammar_sha256 is stale" (llm-check-codex.json, llm-check-grok.json). Environmental, known stale evidence; not card 214.
- CSharpBaselineAcceptance.endpoint_reply_fails_when_response_record_or_status_are_wrong (14 s): diagnostics "tests_did_not_run". Environmental, known (reviewer: occurs on the old harness too); not card 214.
- CSharpBaselineAcceptance.endpoint_reply_passes_when_response_record_and_status_match (13 s): "endpoint-correct-v1: failed 020-endpoint-greetings", tests_did_not_run x3. Same class, known; not card 214.

Nothing new flagged. Specifically: no failure mentions "address already in use" or "Failed to bind" (grep of the full log: none); the Z test with the 5c window passed (GeneratedApiBoundAddress.Z_a_child_that_never_reports_an_address_times_out_naming_pid_url_0_and_log, 5 s); UsableV1.Bar5 (the F-056 test) passed (1 m 2 s); all 31 GeneratedApi, Hazard, Guard, SourceScan and Z named tests passed; the holdout tests that need a git checkout (PrivateHoldoutResults x3, HoldoutTaskConversion) passed here because a worktree has git metadata; the load flakes the reviewer saw at 3x load (the CSharpBaseline `dotnet_test_timeout` cases, BenchmarkHarnessAcceptance.timed_out_model_tree...) did not recur in a single-load run (the latter passed, 16 s). Zero card 214 failures.

Hygiene: build-server reuse disabled for this run; the only VBCSCompiler on the machine (started 04:41) is not mine and was not touched; no PIDs killed.

## 3. CI on the shipping commit (PASS)

`gh run list --branch main --json databaseId,headSha,status,conclusion,...` at 12:07 (all three newest runs in progress), 12:19 and 12:25 (3 gh calls in total).
- d18194c had no run of its own (pushed together with 5ef3563 and 9046262; a push runs CI on the head only).
- Run 41052829472, head 9046262 (contains d18194c; ancestor of origin/main): completed, success, 12:06:04Z to 12:23:00Z.
- Run 41052908998, head 7c3a054: completed, success, 12:23:55Z.
- Earlier greens for the same card code: run 41045702766 on afa4e40 (completed success, the first ubuntu run of the harness), 41045969214 on afec23d (success), 41050269671 on 95ff1a8 (success); d98f1bf run 41052411074 success.
D154 hash rule: d18194c, 9046262 and 7c3a054 are all on origin.

## 4. F-056 closure evidence, Done check 3 (PASS)

- Removal commit: 111960d on origin (its diff of Support/GeneratedApi.cs removes `var port = FreeLoopbackPort();` and `private static int FreeLoopbackPort()`; PortIsOpenAsync is also gone). `git grep -n FreeLoopbackPort 7c3a054 -- '*.cs' '*.py' '*.yml'` returns 0 lines. The harness sets ASPNETCORE_URLS in one place, Support/GeneratedApi.cs:85, with the literal `"http://127.0.0.1:0"`.
- Green full CI on the card's last commit: the last card commit is d18194c (5c). The run that contains it, on a tree whose code is identical (no code file differs between d18194c and 9046262), is 41052829472, success. The same harness was green on ubuntu earlier at 41045702766 (afa4e40). Neither is a re-run of fb13e3d.
- Caveat for the facilitator: d18194c has no run id of its own; the evidence is the run on its descendant 9046262.

## 5. Thread-pool floor recorded as a mitigation naming card 217 (PASS)

- Floor: Support/ThreadPoolFloor.cs, `SetMinThreads(128,128)`, commit afa4e40 (on origin; its body cites local 3aed9dc).
- Review a477bc6 finding 2 asked for the word mitigation; d18194c adds the comment "A mitigation, not a fix: the floor stays only until card 217 removes the blocking waits that starve the pool." (file at 7c3a054).
- Window 217-rerank-6b, decision f870506, `.mob/records/decisions/217-rerank-6b.json` field `changed`: "214 ships on its own evidence plus its pre-ship gate ...; the thread-pool floor (afa4e40) stays as a recorded mitigation until 217 removes or justifies it"; adopted, six of six polled seats NO OBJECTION. Card 217 is on origin: board/217-acceptance-tests-do-not-starve-the-thread-pool.md. record-in-progress.json f062_cause names card 217 as the root-cause fix.

## 6. Domain-expert question, ship boundary (T2)

Should a member with different domain knowledge or context join now? NO. Reason: the change is one harness file plus a module initializer in the test assembly, using documented Kestrel behaviour (port 0, "Now listening on:"); the ubuntu CI runs on the harness (41045702766, 41052829472, 41052908998) are green, so the trigger D156 C1 set for adding a hosting or CI-runner expert (Linux CI differing) did not fire. The remaining thread-pool and CPU starvation question is owned by card 217 (test-infrastructure, no new domain), not a gap in this card's membership.
