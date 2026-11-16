# mob-defaults #61: the facilitator's rulings (2026-11-16)

## Review round 1 rulings

Review round 1 is `review.md`, run on card-61 at 145c5d1 by the QA seat (claude opus = claude-opus-5-5, ctx-4). Verdict: FLAG, 4 BLOCKING, 7 MINOR. All four BLOCKING findings come with probe replays in the reviewer's probe folder.

F1-1, fixed in fix round 1:
- BLOCKING 1 and 3, and MINOR 6: the Decider writes ONE outcome line per voter, paired with that voter's own launch line. This holds on every path: a counted vote, no vote, a failed clone, an exception, or a plain-tuple backend. Each voter's outcome comes from its own result, never from another voter's or the run's.
- BLOCKING 2: every new-format launch, `work_type` `turn` included, needs a later success outcome to count. `CardRun`'s seat turns write their outcome when the turn ends.
- BLOCKING 4: `last_seating` and its callers make a single pass over the history, so the time is linear.

F1-2, fixed in the same round because they are cheap and new:
- MINOR 5: a failed outcome write never loses the decision record. It is recorded the way `integrity.changes` records other history problems.
- MINOR 7: an outcome is paired to the latest unpaired launch of the same card, context and seat.
- MINOR 8: `seat_log.py outcome` requires and matches `--card`, and refuses an outcome with no matching launch.
- MINOR 9 and 10: `--duration-s` is a finite non-negative number, and the identifying arguments are non-empty.

F1-3, a named limit: MINOR 11. The Decider does not produce `refused` or `fallback` yet, and in roles mode `model_reported_by_cli` holds the wrapper's model. This goes to #60, the CLI-reported model.

The fix round: a red that embeds the reviewer's probes as tests (claude sonnet, ctx-2b), then a green (cursor gpt-5.3-codex, ctx-3b), then review round 2 (claude opus, ctx-4b).

## Fix round 1 red rulings (2026-11-16)

- **FX1-1:** MINOR 8 (refuse an outcome with no matching launch) conflicts with the red's earlier test `Test61PendingSeatLog.test_61_m_each_of_the_six_outcomes_is_accepted`, which writes outcomes with no launch. Ruling: that test's fixture writes a matching launch before each outcome; its assertions are unchanged. The green makes exactly this fixture change, named in its brief.
- **FX1-2:** the red seats used shell heredocs three times on this card, despite the brief's explicit ban: a test edit and a no-op in round 0, a class move and an empty run in fix round 1. None touched a file outside the seat's own test file; the CRLF rewrite was restored. This is recorded as a pattern for #61's retro and for D267/D270's sibling question of brief compliance.

## The Dispatch rotation check of 2026-11-16 01:50 UTC (the owner's pool ruling)

The check found only agy and muse in Avowry's seat log for the last 24 h, with claude unseated for 31 h, and five agy voters on D272.

Correction for the record: claude held many seats in that time, but none of them reached `seat-use.jsonl`, because Agent-tool seats are not logged. That is the gap #61's `seat_log.py` closes. Card 430's, card 422's and this card's `model-allocation.md` list them: architect, red, QA, sign-off and retro voices. The vote skew is real. Every Decider from D258 to D272 seated agy for all six roles, apart from D272's muse fallback.

Ruling R-ROT, binding from now on:
- **#61 review round 2:** two seats, each in a fresh context and a throwaway clone.
  - QA: claude opus = claude-opus-5-5.
  - A second, cross-family review: cursor-agent `--model auto`. It runs through `cursor_reply.py --mode ask`, which is read-only. Its model is declared as `auto`, an unpinned size, because gpt-5.3-codex is out until 2026-12-14. Per F-13, the flag stands as its actual model.
- **#61 sign-off:** claude sonnet = claude-sonnet-5-5.
- **Every Decider from now on:** no service holds more than two of the six roles. #61's retro Decider seats architect and qa on claude, audit lens and dev on cursor (auto), coach on muse, and decider on agy. A seat that gives no counted vote is re-seated on another healthy service.
- **Retro voices:** spread the same way. qwen joins once its local_driver finding is repaired.
- **Logging:** once #61 lands, every seat, Agent-tool and CLI seats included, writes its launch and outcome rows through `seat_log.py`.

## Fix round 1 green: Muse's passes and the facilitator's takeover (2026-11-16)

- **Pass 1** (muse, D272) fixed the 14 `test_decider_launched` selectors and the FX1-1 fixture; both were accepted. It also added a special case in `commit_seat`, under which task-force seat turns wrote the old one-line record and always counted. That reopened review BLOCKING 2 and was not accepted. The facilitator's ruling "do not edit `test_seat_rotation.py`" had pushed it there.
- **Pass 2** (muse) wrote every seat turn's outcome at turn end, which was accepted. It also added `fold_turn_outcome`, which rewrote the whole history file outside the seat-use lock, replacing a launch line and dropping the outcome line. That breaks the append-only rule and could erase another run's rows. Not accepted, nor was its matching folded-line rule in `last_seating`, under which a success-marked line could count as a seating.
- **Facilitator takeover** (no seat running in the worktree, D267). The facilitator found the cause:
  - With plain appends, rotation already decides correctly.
  - The `test_seat_rotation.py` failures came from its helpers counting every history line (`used()`, `len(self.history())` and `self.history()[0]`).
  - The facilitator removed the fold and the folded-line rule, and changed only those selectors to skip outcome lines, through a `launches()` helper. No assertion value changed.
- **Ruling FX1-3** corrects the earlier one. A test that counts or indexes seat-use lines is a selector the card's outcome lines legitimately change: count launch lines. A test's decision assertions never change.
- **Result:** b64725f, with card tests 112 of 112 and the rotation and decider set 151 (48 subtests). Muse produced two shortcut fixes in a row on a design-sensitive change. That is a finding for the #61 retro's model allocation review.

## Review round 2 rulings (2026-11-16)

Review round 2 had two seats, under ruling R-ROT:
- **claude opus** (`review-2.md`): SIGN-OFF, 0 BLOCKING, 5 MINOR. Every round 1 BLOCKING is fixed. On two real 2,633- and 2,671-row histories, rotation decisions and reason text are identical before and after. Nothing rewrites the history.
- **cursor auto** (`review-2-cursor.md`): FLAG, 2 BLOCKING, 2 MINOR.

The two reviewers disagree on the same two findings:
- **Pairing ignores the role.** `last_seating` pairs on (seat, card, context), while `append_seat_outcome` also filters on role. An outcome for one of two roles holding the same seat in the same context therefore pairs with the other role's launch. Cursor says BLOCKING; Opus says MINOR, because the shape needs two roles in one context at once, and fresh contexts per seat (R-ROT) rule that out.
- **Unavailable or dry-run turns write a launch with no outcome.** Both agree rotation stays correct, since no outcome means not served.

R2-1: both are MINOR by D259 clause 4. Neither changes a decision on a real shape: Opus replayed real histories, and the decider writes per-voter contexts. Three are fixed anyway in fix round 2, because the fixes are small:
- (a) pairing keys on (seat, card, context, role) in both reading and writing; old rows are untouched;
- (b) a seat refused or unavailable after its launch line writes outcome `refused`, and a dry run writes no launch line;
- (c) a lock refusal in the Decider's outcome phase is caught into `integrity.changes`, so `decider-result.json` is always written (ruling F1-2).

R2-2: named limits, filed on mob-defaults #61's follow-up list:
- Opus MINOR 2: a seat turn can crash after its backend has run, on a lock refusal; rotation fails safe.
- Opus MINOR 3: `call_backend` swallows every exception as a failed turn; rotation is correct, but this changes control flow and is reviewed again at #62.
- Cursor MINOR 3: a second outcome for one launch; the later one is orphaned.
- Cursor MINOR 4: blank `--declared-reason` and whitespace `--declared-model` are accepted.

Fix round 2 seats: red claude sonnet (ctx-2c), green muse (D272, ctx-3d), with the exact changes named in the brief and the facilitator checking the diff for history rewrites. Review round 3: claude opus plus cursor auto (R-ROT).

## The Dispatch check of about 03:20 UTC, and the Mob Board rank

- **Dispatch asked:** with qwen now seated, does the rotation rule still refuse Claude for D273's Decider role?
- **It does.** At 03:21:09 to 03:21:56 UTC the rule refused it because "muse, cursor, agy have waited longer". qwen is no longer named. muse is counted as waiting although its quota is exhausted, because the probe in force reads ok from 00:05 UTC.
- **So mob-defaults #58 (rotation counts only seats that can be seated now) is blocking flow.** As Dispatch directed, the Mob Board now runs mob-defaults #61 (In Progress, rank 0.95), then #58 (0.94), then #62 (0.93). All three are above the board's previous top of 0.88. #61 and #62 were added to the board; they had not been on it.
- **D273's Decider role is still re-seated on muse at 05:07 UTC** by a watcher, unless a better seat becomes possible first. #62's light work starts meanwhile.
