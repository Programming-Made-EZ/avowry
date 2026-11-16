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
