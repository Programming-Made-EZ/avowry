# Card 211 sign-off (sign-off, ctx-6, fresh context)

Seat: Claude. Probe: 2026-11-02 22:40 UTC. Read: board/211, record-in-progress.json, preship-gate.md (SHIP), turn-slice-architect.md, turn-green-1.md and turn-green-3.md, the windows `.mob/records/decisions/211-taskforce.json` and `211-slice.json`, D177 (docs/decisions/process-and-staffing.md#d177), and in mob-defaults (HEAD `f6fb952`, equal to origin/main): DECIDERS.md, records/211/votes.json, records/211/rerun.json, records/211/final-review.json, and `git log` for the card's six commits. Ran `python scripts/mob_process.py check`: `mob-process: OK (103 closed cards, 12 records, 91 legacy)`. Ran `git rev-list` and a script over DECIDERS.md for the range. Opened the reviewer's events file. I did not re-run the 28-minute suites: I rely on the full-suite lines in the turn records (1556 on `ed97c75`, 1581 on `f6fb952`, and the re-run's three). Changed no code. No delete command, no git stash, nothing under .mob/audits/. Left .mob/records/228/body.tmp alone. Format model: .mob/records/210/signoff-ctx6.md.

Intent (D165): confirm that card 211 ended inside what the team decided and the product owner ruled (D177), and that its records are honest enough to close; sign the corrective for MD-010 (27 commits, one identity, no review record). I do not sign the 27 commits themselves.

Assumes (D166): the full-suite results in the turn records are as written, since I did not re-run them; a reviewer on a non-Claude model counts as "a different model from the authors" because the 27 commits came through Claude sessions (the review's own `author_models` field); the reviewer's findings are its words, not facts I verified, except where I say I checked.

## Verdict: SIGNED OFF (the corrective and its three records), with conditions the closing record must carry

Conditions, each a filing or a statement in the record, not a code change: (1) file the named defects as cards (section 4) before the card closes; (2) state the token gaps (section 5); (3) say in the closing record that the Decider rests on seats that mostly did not read the diffs, and that the final-state review read the commit before the last one (section 1); (4) the facilitator scans this seat's transcript and runs `check` again after the closing record is written.

## 1. Done checks against the gate's evidence

| Done check | Verdict | Evidence |
|---|---|---|
| 1. Decider with all role seats | MET, with the depth caveat below | Red `6fccd4a`, green `ed97c75` (11 record tests OK; full suite 1556 OK, 1 skipped, on the committed hash). `records/211/votes.json` has six votes, ctx-1 to ctx-6, one per declared seat, each with reason, D171 CITE and context. DECIDERS.md names the range and, by my script, all 27 commit hashes (none missing). |
| 2. Independent re-run | MET | Red `53ec0c3`, green `9f90dd4`. `rerun.json`: suites in fresh worktrees on `687410e` (163 tests OK, 1 skipped), `e3736d1` (270 OK, 2 skipped) and head `53ec0c3` (1570 run, 1 failure: the red test that needs this very file; no card 258 or 268 failure). Each run has command, cwd, shell, launch, commit and result: the D176 environment line. Failures are findings, not hidden: the audit subcommands do not exist at the range ends and are recorded as findings; the audit run that writes `.mob/audits/` was not run, because the seat may not write there (`not_run` says so). The head was not re-run after the record landed; the final green's full suite on `f6fb952` (1581 OK) covers that. |
| 3. Non-author final-state review in git | MET under D177's replacement evidence (the original identity wording is waived by ruling, not met) | See the D177 table below. Red `fe4c092`, green `f6fb952` (36 targeted tests OK; full suite 1581 OK, 1 skipped, on the committed hash). |

Check 1 caveat, from the record itself: five of six seats voted from the commit list and diffstat, not the diffs; the QA seat did not re-run the suite; the sign-off seat said "team-reviewed" overstates the evidence. All six are Claude: the Grok and Copilot decision seats returned empty replies and agy was out of quota. So the vote shows the seats were asked and answered in separate contexts; it is not evidence that the team read the code. The Decider record keeps both statements. I accept it as the card's Done check 1 (a vote with a reason from every seat) and not as a code review; the code review is the final-state review.

### Criterion 3 against D177

| D177 asks for | Found | Verdict |
|---|---|---|
| A fresh-context seat that authored none of the 27 commits | `seat_context` review-copilot, `authored_none_of_the_27: true` with an attestation; not ctx-1 to ctx-6. The attestation is self-reported; git cannot prove it, which is what D177 accepts. | Met |
| On a different model from the authors where available | `author_models: ["claude"]`; reviewer "copilot (auto) via the Copilot CLI". I opened the events file: `selectedModel` is "auto" and the 85 `model` fields and the `currentModel` say `mai-code-1.1-flash`, which is not Claude. The record says only "auto", so the true model is in the session file alone; the closing record should name it. | Met |
| Attribution: model, seat or context id, session path, range | model, `seat_context`, `session_path` (file exists, 1.1 MB), `session_window` 21:57 to 21:59 UTC, `range` 687410e..e3736d1, `head_reviewed` fe4c092 | Met |
| Cited from the Decider record | DECIDERS.md line 48 cites `records/211/final-review.json` | Met |
| Cited from the closing record | Not yet written. `record-in-progress.json` carries `final_review_model` and `po_ruling_check3`; the closing record must copy them | Carried to close |

Two weaknesses of the evidence, stated, neither a reason to refuse. (a) `head_reviewed` is `fe4c092`, the red-3 commit, one commit before the final state `f6fb952`; `f6fb952` adds only the review record and one DECIDERS line, so the code reviewed is the code of the final state. (b) Several findings are thin. The reviewer counts "26 commits from the same address" because the range `687410e..e3736d1` excludes `687410e` (I checked: `git rev-list --count` gives 26 for that range and 27 for `687410e^..e3736d1`; the card, the votes and the re-run include `687410e`, so 27 is right and the range notation hides one). Its ninth finding, a missing review artifact, was true at `fe4c092` and is closed by `f6fb952`. Its dispute of defect 2 rests on the re-run passing at the range end, which does not show that the gate tests fail when a gate is broken.

## 2. Windows, caps, green commit rule, refactor lines

| Item | Followed? | Evidence |
|---|---|---|
| Window 211-taskforce | Yes (adopted, no conditions) | Same six contexts in the record's `task_force` (ctx-1 Copilot ready, ctx-2 architect slice, ctx-3 QA, ctx-4 Copilot reviewer, ctx-5 facilitator, ctx-6 this seat). Five NO OBJECTION with valid cites, all Claude except the architect on agy, after Grok, Copilot and once agy returned no reply. It raised the identity blocker that D177 then ruled on. |
| Window 211-slice | Yes (adopted, no conditions) | Five NO OBJECTION, all Claude, after ten empty Grok and Copilot seats in one window (the window record says so). The slice's "criterion 3 waits on ruling" is superseded by D177 (`open_points` says so). |
| D175 caps, 1 + 1 + 1 | Held | The slice carries `Cap: criterion 1/2/3, at most 1 cycles`. Git shows exactly one red and one green per criterion: `6fccd4a`/`ed97c75`, `53ec0c3`/`9f90dd4`, `fe4c092`/`f6fb952`. `stand_downs` is empty. |
| Order and pushes | Yes | Green n precedes red n+1; each green was pushed about 9 seconds after its tests passed; mob-defaults equals origin/main. |
| Green commit rule | Yes | Each green names its own tests (11, 14, 36 OK) and the full suite on the committed hash (1556 on `ed97c75`, 1581 on `f6fb952`); green 2's deliverable is the re-run itself. |
| Refactor line per green | Present: "none needed: records only" on all three | Valid reason: no code in this card. |
| Subject token | Yes | Every commit says "MD-010" and "D157 Criterion n". |
| Invariant: the 27 commits not rewritten | Held | The card's six commits add tests, DECIDERS.md and records; the range is unchanged. |

Wording point carried: the gate says the `check` command is "recorded as part of the final green state" without a result. I ran it: OK (above).

## 3. Seat re-seating

The adopted architect seat (agy) failed at launch with "Individual quota reached" (resets in about 145 hours); the slice seat was re-seated on Copilot, the next model with a command reader in least-recently-used order. The window 211-slice `proposed_by` says "re-seated from agy, out of quota". It is recorded as a deviation and disclosed before the window opened; I count it as honest. Consequence to say at close: the cross-model diversity of the Decider was lost (all six voters Claude), which is why the final-state review matters more.

## 4. Open items, carried honestly

| Item | State | Verdict and what becomes a card |
|---|---|---|
| Hard-coded 60, 30 and 45-minute thresholds (mob_process.py:27-30: `STALE_PROBE_MINUTES`, `AUDIT_INTERVAL_MINUTES`, `AUDIT_OVERDUE_MINUTES`; loop_runner.py:28 `IDLE_FAIL_MINUTES` 30). The reviewer confirms them. | Named by the architect seat (defect 5) and confirmed in the final state. No card exists yet. | **Card**: make the thresholds parameters. The only defect both a seat and the reviewer confirmed. |
| Line-ending rewrite of test_card_mode.py and facilitator.py (defect 1) | Seat-named; the reviewer disputes it (no evidence in the final tree). Card 269 covers mob_process.py only. | **Card, small**: extend 269 or file a sibling for facilitator.py and test_card_mode.py. |
| Gates landed with their tests, no red shown (defect 2) | Seat-named; the reviewer disputes on the strength of the re-run, which proves passes, not that the tests catch a broken gate. | **Card**: a mutation or red-run check per gate. |
| Final-review record cannot express "unclear" or the reviewer's verdict wording (new, found by green 3) | The record maps the reviewer's 3 "unclear" findings to "adds" and keeps the original `kind` | **Card**: extend the record format and test, or fix the review prompt to the three stances. |
| Defects 4 (duplicate gate logic), 6 (about 3,900 lines may over-specify), 8 (reshape and reject barely available) | The reviewer calls them unproven. Defect 3 (one identity) is settled by D177. Defect 7 (seat independence unproven) is addressed by the votes and re-run records. | **One low-priority card** to decide whether to close 4, 6 and 8 as unproven. The product owner's request to consider a general seat-attribution fix if the same-identity limit recurs goes to the retro, not a card. |
| The range notation hides `687410e` (26 versus 27) | Found here; the card, votes and re-run use 27 | Say it in the closing record; no card. |

**The record tests narrowed the team's words.** Red 1's test accepts only NO OBJECTION, OBJECTION, Yes or No, so green 1 stored all six "Support-it" votes as "Yes" and kept the word in `decider_vote` and `reply`. Red 3's test accepts only confirm, add and dispute, so the three "unclear" findings are stored as "adds" and "ACCEPT WITH FINDINGS" as `accepted-with-findings`, the originals in `kind` and `verdict_verbatim`. Both mappings are disclosed in the files and the turn records, nothing the seats said was altered, and the originals are in git. The weakness is real: a reader of the main field sees "Yes" and "adds", which say less than what was said ("unclear" is not "adds"). I do not refuse sign-off; the closing record must say that the main fields are normalised and the verbatim fields are the team's words.

**Most Decider seats did not read the diffs.** Carried under check 1. This is the largest honest limit of the card: criterion 1 proves everyone answered, not that anyone read the 3,900 lines.

**A commit-body slip.** Green 3's body says "3 confirm"; the true split is 2 confirm, 3 dispute, 1 add, 3 unclear. The turn record notes it; no amend, by rule. Say so at close.

Scans: nine seats are in `command_scans` (eight clean; the re-run green's three "unreadable file" findings are runs of files absent at the range ends, `mob_process.py` not existing at 687410e: false positives, nothing deleted; card 254 covers this class, and I recommend adding these three lines to its reproduction list). Not yet scanned: this seat; the facilitator scans it. No 211 seat wrote `.mob/audits/` (the re-run's `not_run` says so).

## 5. Token-usage evidence

What exists: 10 rows in `token_usage`. Copilot rows with usage lines: ready (577.7k in, 491.9k cached, 6.4k out, 6 premium, 1m 49s), slice (521.0k in, 412.8k cached, 7.5k out, 6 premium, 1m 35s), final-state review (696.8k in, 646.1k cached, 11.2k out, 0.25 premium, 1m 43s), pre-ship gate (1.7m in, 1.6m cached, 16.2k out, 0.25 premium, 3m 27s). Six Claude subagent rows, one figure each: 84,703 (red 1), 82,357 (green 1), 83,938 (red 2), 89,672 (green 2), 85,465 (red 3), 78,860 (green 3), summing to 504,995.

What is missing, so the closing record must not call the usage complete:
- This seat (ctx-6) and the facilitator: no row.
- The decision-window voters in both windows (211-taskforce and 211-slice), including every Grok and Copilot seat that returned an empty reply (ten in the slice window alone); and, unless it is inside green 1's single figure, the six Decider voters.
- An input, output and cache split for the six Claude rows: each is a single `subagent_tokens` number, so it is not comparable with the Copilot counts. 504,995 is a floor for the build seats, not the card's cost.
- The Copilot "premium" figures (6, 6, 0.25, 0.25) are request units, not tokens, and the two kinds are not additive; the ready and slice seats cost 6 each against 0.25 for the later two, with no reason stated.
- The full-suite runs (about 28 minutes each) are time, not tokens, and are not counted.

## 6. For the retro

- Worth: a clean 1+1+1 under D175 with no stand-down; the first card to use a non-Claude reviewer through D177, with its model checkable in the events file; the re-run found and recorded that the audit subcommands did not exist at the range ends.
- Cost to look at: the Grok, Copilot and agy decision seats returned nothing in both windows and the whole Decider fell on Claude, so the "different model" promise was kept only in the final review. The product owner's question about a general seat-attribution fix belongs here.
- Add to the retro list: the vote-word and stance narrowing in the record tests; the range notation that hides a commit; `head_reviewed` being the red commit; "auto" naming no model in the record.

## Carried, not failed

- Close: file the cards in section 4; copy `final_review_model`, `po_ruling_check3` and the true model (`mai-code-1.1-flash`) into the closing record; state the token gaps; state the diff-reading limit and the normalised fields.
- Facilitator: scan this seat's transcript; run `python scripts/mob_process.py check` after the closing record is written.
