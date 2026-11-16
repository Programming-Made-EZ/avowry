PASS
# Card 162 gate review, pass B (independent gate reviewer: claude opus, claude-opus-5-5, context ctx-162q, fresh; seated by D279)

Reviewed the clone ~/.../scratchpad/r162o, branch card-162-green2 at 70ab77e (parent = main 6254108). Read-only: no repo file edited, no commit, no dotnet run. This file is the only file written.

Verdict: green 2 (70ab77e) may land. It is exactly slice item 9 and the evidence it relies on is current. Two conditions on the push and the sign-off are below (M1 must ride in the same push; M2 must be done before sign-off).

## Step 0

```
$ git merge-base --is-ancestor e2e5a4a HEAD && git merge-base --is-ancestor f78588c HEAD && git merge-base --is-ancestor 46179b4 HEAD; echo "exit=$?"
exit=0
```
All three (Grok evidence e2e5a4a, Codex evidence f78588c, D238 re-freeze record 46179b4) are ancestors of HEAD.

## Check 1: evidence carries the current hashes and shows passed: PASS

Recomputed by `sha256sum` at 70ab77e (raw bytes; 0 CR bytes in either file; .gitattributes has `eol=lf` for both):
- docs/GRAMMAR.md 92029d22b0aaf52d266ef2710238a75cb5908afeacf1146e44343a808dc9d222
- docs/CHEATSHEET.md 3611d24bac1de57a2b0de0225a6125916c9a610a899697cb37660b65107cc189

Both equal the D238 new hashes.
- llm-check-codex.json: model codex, date 2026-11-11, passed true, proof_level specs-passed, specs 13, specs_passed 13, build_warnings 0, attempts 1, grammar_sha256 and cheatsheet_sha256 = the two values above. File sha256 4c245e18...bda5c5 = run-codex.md; `cmp` with .mob/records/162/llm-check-codex.json: identical.
- llm-check-grok.json: model grok, date 2026-11-10, same fields, same pins. File sha256 8cdc563f...46491e = run-grok.md; `cmp` with the record copy: identical.
- Both runs record re-freeze commit 46179b4 (detached worktree, launcher hash guard passed).
- Bound content files unchanged since 46179b4 (`git diff 46179b4 HEAD` over GRAMMAR, CHEATSHEET, bench/, demo/, orders.avowry, run-llm-check.py: empty). See M3 for Program.cs.

## Check 2: green 2 is exactly slice item 9, and nothing else holds bar 6 or the three rows: PASS

`git show 70ab77e` touches 3 files only:
- UsableV1.cs: removes `[Trait("Status", "Pending")]` and its comment from Bar6_... (Criterion 162-2).
- UsableV1Closure.cs: removes the Pending trait and comment from Z1 and Z2; rewrites the class header comment (comment only; it now states the true state).
- board/usable-v1-ratchet.json: `"suspended": {"6": ...}` becomes `"suspended": {}`; must_pass still [1..6].

Nothing else keeps them held:
- `grep Pending` over UsableV1.cs and UsableV1Closure.cs: only Z2's own string literal and comments; no remaining trait.
- Readers of `suspended`: usable-v1-report.py `load_suspended()` (`.get("suspended", {})`, so `{}` gives no suspension), mob_process.py render_mob (prints `[none]`), Z1 (accepts missing key or empty object). No other code or test reads it.
- Z2 selects `Bar6_` by prefix; `Bar6Failure` (internal) does not match, so `.Single` holds.
- `python scripts/usable-v1-report.py --self-test`: `self-test ok`. With an all-pass result and `{}`, report() prints "Usable v1: 6 of 6 criteria met", no `::warning::` line, `::notice::USABLE v1 REACHED` with no "(with suspended bars)", exit 0.

## Check 3: criterion 3, CI usable-v1 6 of 6 after merge: expected PASS (dotnet not run here)

ci.yml has three jobs: mob-process, test, usable-v1.
- usable-v1 job: `dotnet build Avowry.sln -c Release`, then `python3 scripts/usable-v1-report.py`, which runs `dotnet test tests/Avowry.Acceptance.Tests -c Release --filter "Milestone=UsableV1"` (no Status filter) and parses Bar1_..Bar6_ from the TRX. Bar6 runs here (it carries Milestone=UsableV1; it ran here before green 2 too, Pending or not). Z1 and Z2 have no Milestone trait, so they do NOT run in this job. With both evidence files current, Bar6 passes, every ratcheted bar is met, suspended is empty: 6 of 6, no warning, exit 0.
- test job: `dotnet test Avowry.sln --filter "Status!=Pending"`. Bar6 now also runs here (newly), and Z1, Z2 run here (newly). R1 already ran here.
- Network and model calls: none. Bar6 reads two JSON files and sha256s two docs (Bar6Failure, EvidenceFailure, ModelFailure: File/JsonDocument/SHA256 only); the UsableV1Fixture is a Lazy that Bar6 never touches, so Bar6 does not even build the demo app. Z1 parses the ratchet JSON; Z2 is reflection over trait attributes. No process start, no HTTP, no model.

## Check 4: risk to bars 1 to 5: none found

Green 2 changes no source, no demo, no fixture and no test of bars 1 to 5; must_pass is unchanged. The only behavioural change for the usable-v1 job is that bar 6 can no longer hide behind a suspension: if Bar6 ever fails, the job goes red as a regression (intended, D131). Bars 1 to 5 share the class fixture, but Bar6 does not call it, so nothing about their ordering or build changes.

## Findings

- M1 (MINOR, but must ride in the same push): MOB.md goes stale. MOB.md line 1 at 70ab77e is `Usable v1: 6 bars ratcheted, suspended: [6]`; render_mob (mob_process.py lines 418-428) reads the working-tree ratchet and will now render `suspended: [none]`. check_mob (line 468) compares bytes, so the ci `mob-process` job on 70ab77e alone fails with "MOB.md is not byte-identical to write-mob-md output". The usual "MOB.md regenerated" commit must be in the same push (push CI runs on the tip). Not verified by running the checker (it needs `gh issue list`); evidence is the code above.
- M2 (MINOR for green 2; must be done before sign-off): criterion 162-1's C# test is still Pending. tests/Avowry.Acceptance.Tests/PatternDocs.cs:14 `[Trait("Status", "Pending")]`, :15 comment "Pending until green 1 folds the pattern steps ..." (stale; gate review A N1). Slice section 5 item 2 says green 1 un-pends it; gate review A expected green 2 to; neither did, and item 9 does not list it, so green 2 is right not to. Effect: ci.yml has no python test step, so the ten needles (G1-G8, S1, S2) are enforced nowhere in CI once the freeze window ends at green 2. The needles hold today: `python -m unittest tests.test_pattern_docs` -> `Ran 10 tests ... OK`; `python -m pytest tests/test_pattern_docs.py -q` -> `10 passed`. Fix: remove the trait and the comment (a one-line test-only change, its own commit), then the gate's `Status!=Pending` run covers it. The card must not be signed off with its criterion 1 test Pending.
- M3 (MINOR, process): tools/Avowry.Bench/Program.cs, a D238 freeze-window file (informational, launcher guard only), was edited inside the window by card 368 (8d4ce9b) and card 418 (5617134), both before the Codex run. The evidence is unaffected: both runs used a detached worktree at 46179b4 (run-codex.md, run-grok.md, codex-run-notes.md; launcher guard passed), and the edits only scrub paths in diagnostics and blank reply_head, not prompt composition. Current Program.cs sha256 2caac177... no longer equals the D238 informational line 0e9e91c2.... Record it in the retro; R1 does not bind this file, so no test fails.
- N1 (note): Codex needed three harness repairs before its passing attempt (codex-run-notes.md); each failed attempt had 0 tokens out and reason error, so none was a model result. Consistent with the card's harness-failure rule.

## What the gate must run (clean clone at the tip that will be pushed, under Avowry's heavy lock, alone)

1. Apply M2 (un-pend PatternDocs) and regenerate MOB.md (M1) first, or accept that the card cannot be signed off and that mob-process goes red.
2. `python scripts/usable-v1-report.py` : expect "Usable v1: 6 of 6 criteria met", no `::warning::` SUSPENDED line, `::notice::USABLE v1 REACHED` without "(with suspended bars)", exit 0.
3. `dotnet test Avowry.sln -c Release --filter "Status!=Pending"` : expect all green, and confirm by name in the log that Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone, Z1_the_ratchet_has_no_suspended_bar, Z2_the_bar_6_test_is_not_pending, R1_the_refreeze_record_matches_the_files_the_check_runs_against, the Evidence_rule_* rows and (after M2) The_grammar_and_the_cheat_sheet_teach_the_ensure_step_and_the_record_update ran and passed.
4. `python scripts/usable-v1-report.py --self-test` and `python -m unittest tests.test_pattern_docs`.
5. `python3 scripts/mob_process.py check` (needs gh read access) after the MOB.md regeneration.
6. After merge: confirm ci on main shows the usable-v1 job "6 of 6 criteria met" on ubuntu (criterion 3) and the test and mob-process jobs green.
