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

## Review round 3 rulings (2026-11-16)

- **R3-1.** QA finding N1 (the dry-run return in `CardRun.check_seat` skips the command probe, the history read and the rotation refusals when a probe is in force) is a MINOR and a named limit for mob-defaults #60. No production path reaches it. Review round 3: QA claude opus SIGN-OFF, cross-family agy SIGN-OFF, no BLOCKING. The card goes to sign-off on muse (medium effort).

## Gate ruling G-1 (2026-11-16, 07:32Z)

The full suite on card-61 at 56d8f41 (depth-1 clone, under the heavy lock, 06:49 to 07:32Z) had 31 failed and 2331 passed. A filtered run of the same tests on main (4c1addd) showed 7 of them failing there too (card_211 RealRecord x3, launch_long_prompt x2, muse_cycle3_scope, muse_install), so they are out of scope. **22 are new: BLOCKING.** The targeted test sets in every round left out `test_card_mode`, `test_decider_gate`, `test_decider_record`, `test_decider_record_units` and `test_decider_runner`, which read seat-use rows. Fix round 3 (gate) goes to cursor composer-2.5 (D273, ctx-3f). Each failure is classed as a test selector (A), a test fake (B), a production reader (C) or a legitimate new row (D), and no assertion may be weakened. Retro note: a red or green brief for a change to a shared file format names every test file that reads that format.

## FC-3 (facilitator check of fix round 3, 2026-11-16T09:45Z): one seating rule, no row-shape case

- **Found.** Fix round 3 green (cursor composer-2.5, ctx-3f) passes the 22 gate tests and the wider set, and its class A, B and D changes are sound. Its class C fix adds `mob_process.last_seating_for_rotation`, a second copy of `last_seating` with a `legacy` flag: an open launch counts only when the backend's last row was an old-format row. That is a case keyed on row shape, which the fix round 3 brief forbids. `last_seating(count_open=True)` also counts an open launch only when no other seating exists, so a newer open launch is ignored after an older success.
- **Ruling.** One function. `last_seating(history, backend, count_open=False)`; with `count_open=True`, a launch with no outcome yet counts as a seating at its own index, like any other seating (the latest seating wins). `rotation_reason` cites `last_seating(..., count_open=True)`, and `last_seating_for_rotation` goes. The Auditor recompute keeps `count_open_launches=True`: it judges the record as `check_voters` judged it, with the voters in flight counted as seated. The `4*n+2` read budget stands as class D.
- **Action.** Fix round 3b, the same seat and context (cursor composer-2.5, ctx-3f).

## FC-3b (facilitator check of fix round 3b, 2026-11-16, about 10:20Z): this run's own launches count, not every open launch

- **Found.** Under FC-3's rule (every open launch is a seating), `Test61Fix2::test_61_f2_a_rotation_reason_cites_the_qa_launch_line_as_the_served_line` fails: a newer open UX launch outranks the served QA launch. That test is fix round 2's contract (an open launch from another seat is not a seating). `SameRunReason` and the `Auditor` need the opposite for the launches the same Decider run wrote just before its Claude voter. FC-3 was too broad.
- **Ruling.** The difference is not row shape. It is whose launch it is: `check_voters` counts the voters of the same run as just seated ("a model voting in this run has not waited longer"). So `last_seating(history, backend, also=())` counts, besides served launches and old-format rows, the launch lines whose indices are in `also` (this run's own lines). `count_open` and `count_open_launches` go. `rotation_reason(probes, history, also=())` and `claude_wait_problem(..., also=())` pass it through. `decider.voter_record` passes the indices of this run's lines before the voter's own (from `run.seat_use_index`). The Auditor passes the logged records' `line_index` values. With `also` empty, f2_a's served line is cited.
- **Action.** Fix round 3c, the same seat and context (cursor composer-2.5, ctx-3f).

## FC-3c (facilitator, 2026-11-16, about 11:05Z): the race test's recompute passes its run's own lines

- Fix round 3c met FC-3b; the 22 gate tests pass and the wider set had one failure, `test_decider_rotation.py::ConcurrentStarts::test_each_runs_record_points_at_its_own_lines_and_claudes_reason_is_recomputed_from_the_file`, which the seat stopped on as told. It recomputes run A's Claude reason without run A's own lines, the same recompute FC-3b already fixed in the Auditor. The facilitator applied the same one-line change there (`also` = run A's records' `line_index` values); no assertion was loosened. `test_decider_rotation.py` and `test_seat_use_model_outcome.py`: 161 passed, 14 subtests.
