# Card 237 step 0d: miss-test key (QA, ctx-3, non-runner)

Intent (D165): seed one must-read rule per planned stage-A run (6 roles x 2 arms) so a miss is judged against a key written after the freeze by a seat that neither wrote the freeze (ctx-2) nor runs (ctx-5), per window 237-slice condition 4.

Assumes (D166): each of the six rules is carried by the TEAM_WORKING_AGREEMENT.md and by the wiki route of the role; probe: I read the six agreement lines (145, 126, 142, 147, 135, 202) and the wiki pages on each brief's route (INDEX.md route table) and matched them by quote.

Hostile inputs (D167):
- Unknown run key: keys are exactly {baseline,wiki}-{ready,slice,red,green,ship,signoff}-1; the checker fails a ledger row not in the key.
- Repeated key: each run_key appears once (12 entries).
- Disabled arm or role: none; no entry is marked disabled.
- Reordered: order is by arm then role_order; the checker matches by run_key, not position.
- Rule that is a task echo: no rule is named in any task; each is a rule the task alone does not state in full.
- Stage B: keys use -1; a stage B run would need -2 keys added by a new key commit before it, which the order check allows only before the first run.

Key: .mob/records/237/trial/miss-key.json, 12 keys, author_seat ctx-3. Rules: ready, every seat incl. slice and ship present or removed with a reason (D158); slice, thin-slice numeric limits (D136); red, Intent line first (D165); green, green pushed at once before next red (D157/D169); ship, dry-run of closure record check with hashes resolving on origin (D154); signoff, standing-rule change needs a full Decider (D153). Each entry carries the agreement line, the wiki page and line, and the grader test.

Wiki route finding: all six routes carry their rule, so no likely wiki miss recorded in advance. Weaknesses to note: howto/hostile-inputs.md holds no rule (one sentence), and the D158 seat rule lives in the facilitator's checklist in the agreement but on the product manager page in the wiki.

Secrecy finding (OPEN POINT): the frozen tasks forbid only .mob/records/227*, .mob/records/decisions/227* and card 227 history. Nothing forbids reading .mob/records/237/trial/miss-key.json, or the rest of .mob/records/237/ (including this file and the architect's slice, which names the miss test), or git history. A run could read the key. The tasks cannot be edited (freeze, sha256 in freeze.json). Options for the Decider or runner: run each seat from a checkout or worktree that lacks .mob/records/237/ (the key must still be committed first, so use a checkout with that folder removed from the working tree is not possible without deleting; better a sparse checkout excluding .mob/records/237/), or put a forbidden-path line in the runner's wrapper outside the frozen text and apply it to both arms identically (this changes the prompt, so the Decider should rule it a non-task addition). Until one is chosen, a read of the key by a run is undetectable except from its tool log; the reviewer should check run logs for that path.

Checker (`python scripts/check-wiki-trial.py`, exit 1, expected; only run-derived files missing):
```
.mob/records/237/trial/ledger.json: missing file
.mob/records/237/trial/recompute.json: missing file
```
`python -m unittest tests.test_wiki_trial`: Ran 35 tests in 113 s, FAILED (failures=1): the one failure is test_real_trial_folder_is_ready, which asserts the real trial folder is ready and stays red until ledger and recompute exist. No checker fix needed.

Commits: b0014b1 (miss-key.json, pushed, pre-push OK). This record is committed after it.

Size: 3604 bytes
