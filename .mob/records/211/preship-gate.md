# PRE-SHIP GATE — Card 211

Verdict: SHIP

No blocking findings. The record is honest, the D177 replacement evidence is in force, and the required evidence is committed and consistent.

1. Done checks met by committed evidence
- Criterion 3 is satisfied under the product-owner replacement rule, not by the original identity-by-commit requirement. D177 says: "option 3, WAIVE the different-committing-identity requirement for card 211" and replaces it with a fresh-context, non-author final-state review recorded with model, seat or context id, session or events path, review range, and MD-010 citation (`docs/decisions/process-and-staffing.md:794-806` in the current file; the ruling is the D177 section).
- The board card states the exact Done checks and the original blocking condition; the record-in-progress file records that D177 supersedes the earlier identity requirement and that the replacement evidence is the accepted route (`board/211-a-whole-team-review-of-the-mob-defaults-rulings-commits.md:24-35`, `.mob/records/211/record-in-progress.json:1-60`).
- The final-state review evidence is committed and recorded as a fresh-context Copilot CLI review on a different model from the authors where available: `green 3` states review-copilot, seat/context `review-copilot`, model `copilot (auto) via the Copilot CLI`, session path `C:\Users\ringe\.copilot\session-state\b4bfd6c9-0a9a-46cc-a82a-08cacda5bced\events.jsonl`, and range `687410e..e3736d1` (`.mob/records/211/turn-green-3.md:1-27`).
- The Decider record and votes file are committed; the green 1 turn record states the exact commit `ed97c75` and the range, and explains the vote-word normalization (`.mob/records/211/turn-green-1.md:1-32`).
- The rerun record is committed; the green 2 turn record states `9f90dd4`, the endpoints `687410e`, `e3736d1`, and `53ec0c3`, and the actual suite and audit findings (`.mob/records/211/turn-green-2.md:1-39`).

2. Criterion order in mob-crosscli git log; D175 caps hold
- The record-in-progress cycle list names the exact red-to-green sequence: criterion 1 `6fccd4a -> ed97c75`, criterion 2 `53ec0c3 -> 9f90dd4`, criterion 3 `fe4c092 -> f6fb952` (`.mob/records/211/record-in-progress.json:80-115`).
- The slice cap lines are explicit and sum to 1+1+1: `Cap: criterion 1, at most 1 cycles`, `Cap: criterion 2, at most 1 cycles`, `Cap: criterion 3, at most 1 cycles` (`.mob/records/211/turn-slice-architect.md:1-16`), matching the D175 rule (`docs/decisions/process-and-staffing.md:751-786`).
- The red-green order matches the card’s record and issue; there is no over-cap cycling. The 211 taskforce and slice proposals both note the same cap and the product-owner blocker (`.mob/records/decisions/211-taskforce.json:1-120`, `.mob/records/decisions/211-slice.json:1-120`).

3. Record tests and mob_process check were run; environment lines and reference shell are recorded
- Green 1 records the `python -m unittest tests.test_card_211_decider_record` run as `11 run, OK` and the full suite as `1556 tests in 1753s, OK (skipped=1)` on `ed97c75` (`.mob/records/211/turn-green-1.md:18-23`).
- Green 2 records the `python -m unittest tests.test_card_211_rerun_record` run as `14 tests OK` and the suite comparison run on the range endpoints plus the head (`.mob/records/211/turn-green-2.md:12-28`). It also states the D176 environment line: `Git Bash (POSIX sh) on Windows 11 Pro... launched by the Claude Code Bash tool ... cd <worktree> && python -m unittest discover -s tests`.
- Green 3 records the combined gate run `python -m unittest tests.test_card_211_final_review tests.test_card_211_decider_record tests.test_card_211_rerun_record` as `Ran 36 tests, OK`, and the full suite on `f6fb952` as `Ran 1581 tests in 1701.7s, OK (skipped=1)` with the reference shell run (`.mob/records/211/turn-green-3.md:20-33`).
- The `python scripts/mob_process.py check` command is recorded in the card’s gate evidence as part of the final green state; the record notes that the suite is green on the committed final state and that no card 268 or 258 failure was present in that run (`.mob/records/211/turn-green-3.md:29-33`).

4. Vote-word narrowing and review wording did not materially weaken the evidence
- The record tests intentionally narrow the team’s wording: `Support-it` is stored as `Yes`, and the reviewer's `unclear` findings are stored as `adds` with the original kind kept verbatim in the file (`.mob/records/211/record-in-progress.json:93-104`, `.mob/records/211/turn-green-3.md:10-18`).
- This does not materially weaken the evidence because the originals are preserved in the verbatim fields (`decider_vote`, `reply`, `verdict_verbatim`, `kind`) and the record-in-progress notes the normalization explicitly; it is a schema-normalization issue, not a hidden change in meaning.
- The record is honest: it says the decider seats did not read the diffs and the sign-off seat said “team-reviewed,” which overstates the evidence; the record keeps both rather than smoothing them out (`.mob/records/211/record-in-progress.json:20-35`, `.mob/records/211/turn-green-1.md:7-14`).

5. Review depth: criterion 1 records the actual depth honestly
- The in-progress record states: “Most seats voted from the commit list and diffstat without reading the diffs; the sign-off seat said 'team-reviewed' overstates the evidence; the record keeps both.” (`.mob/records/211/record-in-progress.json:20-35`).
- That is an honest statement of the evidence. It does not falsely describe the record as a full diff review; it records the actual depth and the gap.
- By contrast, the final-state reviewer did read the final state and filed the review record; that is the separate, stronger evidence used under D177 (`.mob/records/211/turn-green-3.md:1-11`).

6. Every seat has a scan entry, every commit is pushed, .mob/audits is empty, and no delete/stash command appears
- Every seat has a scan entry in the command-scan record, and the scans are “clean” or declare a specific, expected failure case rather than an unsafe delete (`.mob/records/211/record-in-progress.json:30-82`).
- The record says “no unpushed commits of another seat,” and green 3 explicitly states that the final review push was to `origin main` and that there were no unpushed commits in either repo (`.mob/records/211/turn-green-3.md:22-27`).
- The record also says `.mob/audits/` was intentionally not written because the audit subcommands are not run at the range endpoints; it does not show writes under that path (`.mob/records/211/turn-green-2.md:25-36`).
- No delete or stash command appears in the record trail or in the scan evidence; the record explicitly states the absence of delete/stash evidence and the expected missing-file findings rather than any unsafe command (`.mob/records/211/record-in-progress.json:30-82`, `.mob/records/211/turn-green-2.md:25-36`).

7. Deviations are honest and disclosed
- The in-progress record lists the actual deviations: the seat placement deviation (agy re-seated on Copilot when quota was reached), and the fact that the “Support-it” and “unclear” wording was normalized by the tests (`.mob/records/211/record-in-progress.json:32-73`, `.mob/records/211/record-in-progress.json:93-104`).
- Green 3 also notes the commit-body mismatch (“3 confirm” vs the actual 2 confirm / 3 dispute / 1 add / 3 unclear split), and says it was left as-is because the file is correct and the no-amend rule applies (`.mob/records/211/turn-green-3.md:31-35`).
- These are honest deviations and do not invalidate the gate.

Conclusion: SHIP. There are no blocking findings. The record’s only material caveat is that the evidence is not a full diff review from all seats; the record says so honestly, and under D177 the final-state review by a fresh-context Copilot CLI seat and the committed rerun/Decider evidence are sufficient to close criterion 3. The red-to-green order, D175 caps, environment lines, and gate results all hold.
