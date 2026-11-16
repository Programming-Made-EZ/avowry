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
