# Sign-off, card 227: Recursive deletes on variable paths are blocked and every commit is pushed

Run key: baseline-signoff-1. Seat: signoff (product manager, ctx-8 on the card's task force). Card treated as open at signoff.

## Sources relied on
- `board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md` (Done checks, Observes, Invariants, Context).
- `TEAM_WORKING_AGREEMENT.md`, read in full. Sections used: Board and cards (Definition of Ready, thin slices D136); Engineering discipline ("Push every commit (checked, card 227, F-071)", one criterion per red-green cycle D157); "Deletes: detect after the fact inside the working tree, never outside it" (D169); Safety (detectors, the command scan's limits); Driver trust ladder and its trust-ladder record; "Metrics are evidence, not targets (the Goodhart guard)"; Principles (Goodhart's Law; a rule without a detector isn't in force); "Claim equals commit, and a pre-ship evidence gate" (D154); Decider protocol (including step 6, independent re-run); "Process ideas are hypotheses, not doctrine"; Escalation; "Decisions stay reversible"; Staffing (roles over models).
- Repository as it stands: it holds `scripts/delete_guard.py`, `scripts/mob_process.py`, `scripts/check-unsafe-deletes.py`, `tests/test_mob_process.py`, `docs/PROCESS-CHECKS.md`. I did not run them, and I did not read `.mob/records/227*` or the card's history. So the evidence below is what I would require, not what I have verified.

## Done checks against the card text

| # | Done check (card text) | What the card text shows | Status at signoff |
|---|---|---|---|
| 1 | The scan blocks variable or computed-path recursive deletes in executed commands (tool calls via hook or transcript), not only reply text; failing test first, reproducing card 226's slip, then `Remove-Item -Recurse -Force`, `shutil.rmtree` and equivalents; ZOMBIES by QA at ready | The Status line says the command scan reads every seat's executed commands and the record check fails a variable-path recursive delete or an unscanned seat. Adopted condition (D145, window 227-taskforce): the red includes a failing test on a destructive command absent from reply text, and the Zero case (an empty or missing command source fails). | Claimed met, evidence needed. **Wording gap:** the check says "blocks", the delivered behaviour is "detects after the fact". The product owner's D169 ruling accepts detection for this card, so this is an accepted deviation, not a defect, and it must be recorded as such. |
| 2 | Trust-ladder entry recorded for the slip (seat, rule, step down or a stated reason none applies) | The Status line says trust-ladder entries for both slips. The agreement's Claude ledger row records the card 226 criterion 1 slip: Rung 1 stays, coached, line in `.mob/records/trust-ladder.jsonl`. | Claimed met. "Both slips" is more than the one slip the check names, so I must see which two, and that each line has a valid `step`. |
| 3 | Push on every commit: agreement and template say so; a detector FAILs any commit on avowry or mob-defaults unpushed for more than about 5 minutes; ZOMBIES by QA | The agreement carries the rule with the 300 pass / 301 fail boundary; a future-dated commit, a missing path, no `origin` and a non-zero git exit are FAILs; git only. The Status line gives mob-defaults 524171f..f8a45da. | Claimed met for the agreement and detector. The template half lives in mob-defaults, which is not in this repository, so I cannot see it here. |

Shape check: Contract, `Depends on: none`, `Enables: card 216`, three Done checks (the cap), one observable behaviour. Acceptable. Checks 1 and 3 each bundle two things; the thin-slice rule (one concern, at most three criteria) tolerates that only because the count is three. I note it and do not block on it.

## Verdict
**Sign off, conditionally: YES once the evidence listed below is cited by pushed commit hash and resolves on origin.** On the card text, the three Done checks are met in substance, and D169 explicitly accepts detection in place of blocking. I do not sign off on the word "blocked" as written: the title and check 1 should be read as "detected" (the Status line already says so). I would not close the issue on the Status line alone: under D154 (claim equals commit), no seat reports done or recorded unless it cites a pushed commit that holds it.

If required evidence is missing, the verdict is NOT signed off, and the fix goes back to the green seat. It is not a reason to change a rule.

## Evidence I require (judgment backed by data, not a threshold stamp)
Per Goodhart's Law and "Verdicts are judgments backed by data", these inform my judgment; none is a quota.

Metrics (from the card's Product value):
1. Unflagged recursive deletes on variable paths, expected 0. Evidence: the scan run over the card 226 recorded deviation (the exact slip) reports it; also `Remove-Item -Recurse -Force`, `shutil.rmtree`, `rm -r`, with real output pasted.
2. Commits unpushed for more than about 5 minutes, expected 0. Evidence: `python mob_process.py unpushed <repo>...` output on avowry and mob-defaults at signoff time, plus the boundary tests (300 s passes, 301 s fails, future date, missing path, no origin).

Counter-metrics (the card names them, so they are required):
3. False positives on a literal path inside a known scratch root through `safe_delete` (the card Invariant): a test that stays green, plus the count over a real stretch of records.
4. Replies reworded to dodge the scan: a review of recent seat replies and commands for forms the scan misses (hidden verbs, encoded forms, a delete written to a file and run later, non-Claude seats, Agent-tool sub-agents scanned by hand). The agreement lists these as known limits; I require them stated plainly in the record, not claimed as covered.
5. Cost side: extra commits and minutes from sign-off to close (D154 counter-metrics), and the push detector's false-FAIL rate.

Process evidence:
6. Red-first proof for check 1: a red commit whose body says `Criterion: 1`, a failing test on a command absent from reply text, and the Zero case. Green for criterion n before red for n+1 (D157), greens pushed at once; the pre-ship line lists Criterion order from git log with any breach explained.
7. Honest red: reverting only the fix turns the new tests red.
8. Independent review by a non-author in a fresh context (the card declares reviewers ctx-3 and ctx-6); no seat both [author] and [reviewer]; each Claude seat carries a recorded reason (the card cites probes at 08:39 UTC with Codex, Copilot, Grok and agy lacking usable quota; probes must be under 60 minutes old at each seat assignment).
9. Slice and diff caps: at most 15 countable files and 400 changed lines, or a verified `Slice-exception`; slice files at most 15360 bytes with a "Size:" line.
10. Experiment lines the card triggered: the "Intent:" line (D165), the "Assumes: ...; probe: ..." line (D166, since this card changes a rule and a checker), and the hostile-input list (D167: disabled, unknown, repeated, reordered command forms).
11. Pre-ship dry run of the closure record check with every cited commit and record path resolving on origin (D154), and CI on main green (`python scripts/check-ci-main.py --wait 1500`).
12. A qualitative cold read: a model reads the change cold and explains it, checked against the product principles.

## What the decision rules require me to record, and who decides
- **Declaring the card Done is a Decider.** My sign-off is the proposal; the declared seats vote simultaneously by role, through the sandboxed runner (`decider.py`), with an independent re-run by fresh seats on the final text before recording (step 6); the record shows both runs. The lighter objection window (D153) does not cover declaring Done.
- **Record the D169 acceptance as a deviation from the check's wording:** "blocks" delivered as "detects"; the product owner's ruling is on the card. Its two conditions are (1) every commit pushed, with the 5-minute detector actually run, and (2) no recursive delete outside the working tree, which card 228 guarantees. Until card 228 lands, condition 2 is not met by this card, and I say so rather than imply it.
- **Record the trust-ladder lines** in `.mob/records/trust-ladder.jsonl` (`python mob_process.py ladder <repo>` exits 0) and the Claude ledger row. Where the brief caused the miss, fix the brief rather than demote; a Rung 1 driver that misses stays at Rung 1, coached.
- **Record the scan's known limits** (Safety section) and name card 228 as owner of encoded forms, hidden verbs, non-Claude seats and Agent-tool seats.
- **Record the revert step** (Decisions stay reversible): how to undo the detector and the rule text, or say it is a one-way door. As written, neither is a one-way door; both are small and revertable.
- **Retro before close (D136, D154):** the card 227 retro is pushed before issue 227 is closed or any later commit that names a different card. It opens with flow metrics, covers the feedback-loop lens and the review of each previous adjustment (D155), and is followed by the PM-led re-rank with a committed re-rank note (D148).
- **Who decides a rule change:** the task force, by a full Decider with independent re-run (a standing-rule change is outside the lighter window). Process ideas enter as experiments (hypothesis, metric, counter-metric, trial scope), and only retro-kept experiments are written into the agreement. Explicit product owner standing rules apply directly. As PM I do not change a rule unilaterally. The "Push every commit" and deletes rules now rest on product owner rulings (D169): an amendment, or relaxing detect-after-the-fact, goes to the Decider, and to the product owner only if it touches the safety carve-out (destructive or irreversible changes) or the product's purpose, asked as a one-word question.
- **No escalation now:** the product owner has already ruled (D169), and nothing here is a purpose question or an unresolved model conflict.

## Residual risks, stated plainly
- Detection happens after the delete has run, so a mis-quoted delete can still take out `.git` before the scan sees it. The push rule limits the loss to about 5 minutes of commits, and only if the detector actually runs. A push to an `origin` that is a local path protects nothing; remote-tracking refs are local; an amend or rebase resets commit age.
- All eight card seats were Claude. The card gives the reason, so this is allowed, but the reason must be cited in the trail record.

Size: 10543 bytes
