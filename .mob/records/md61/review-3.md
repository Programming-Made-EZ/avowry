# mob-defaults #61 review round 3, card-61 at 56d8f41 (fix round 2 green)

Seats, per Decider D273:
- QA: claude opus = claude-opus-5-5, ctx-4c, in a fresh clone of card-61 at 56d8f41.
- Cross-family: agy. The pin `gemini-4-argon-high` stopped at once on its quota (429, resets in about 122 h) and served nothing, so the seat was re-pinned to agy's default `gemini-3.8-flash-medium`. D273 names the service, not the model.

Diff reviewed: `33b9ecb..56d8f41` (decider.py, facilitator.py, mob_process.py), against ruling R2-1's three fixes.

## QA (claude opus): SIGN-OFF; new BLOCKING 0; new MINOR 1

| # | finding with a minimal input | replay | class with reason |
|---|---|---|---|
| N1 | **The dry-run early return in `CardRun.check_seat` (facilitator.py:1775) skips more than the launch line.** At 33b9ecb a dry run with a probe in force still ran three checks; at 56d8f41 it returns before all three. The checks are the command probe, the history integrity read and the Claude rotation rule. Inputs: claude command-capable with no command probe on a driver seat; a non-JSON history line; or a legacy claude row while grok is ok and has never been seated. Each is `SeatRefused` at 33b9ecb and `(True, 'SIGN-OFF')` at 56d8f41, with nothing launched and nothing written. | `r3probe/test_r3_dryrun.py::DryRun`; `test_production_dry_run_has_no_probe` | MINOR. It cannot happen in production: a dry run never probes (after `card_start_checks`, `probes` is None, so the earlier guard returns in both versions), and the Decider always runs with `dry_run=False`. Only fixtures that put a probe in force reach it. The narrower fix keeps the refusals and returns only `history=None`. |

Held:
- (a) `last_seating` keys on (seat, card, context, role) in one linear pass, and legacy rows are handled before the key.
- (b) A disabled or missing backend after its launch line gets exactly one `refused` outcome under the lock. `turn_context` and `started_at` are always bound there, and a launch line cannot exist with `probe is None`. A dry run writes no launch line.
- (c) A refused outcome-phase lock is added to `integrity.changes`, the run reports ok:false and exits with 5, and `decider-result.json` is written. Nothing else can raise inside the outer `except`.
- `append_seat_outcome` and `seat_log.py` already matched on role at b64725f; the seat_log red failed only through `last_seating`.
- The red's 19 Fix2 tests against b64725f's code: 13 fail and 6 pass. At 56d8f41 all pass.
- Forbidden patterns: none. No test edits, no `write_text` or `open(..., 'w')`, no folding, and no old-format special case.
- Tests: the seven-file set with `--capture=sys` gave 282 passed and 48 subtests.

Probe files are in the facilitator's session scratchpad (`r3probe/`).

## Cross-family (agy, gemini-3.8-flash-medium, plan mode): SIGN-OFF, no findings

It confirmed all of (a), (b) and (c), and that role matching was already in place. On the dry-run return it judged that no required check is skipped. QA's replay shows otherwise for a dry run with a probe in force (N1), so QA's finding stands. The agy process ended by its 25-minute timeout after it printed the verdict ("root agent idle; waiting ... for 1 background task"), and the reply was complete.

## Facilitator ruling R3-1

N1 is a MINOR and becomes a named limit for mob-defaults #60, beside R2-2: production cannot reach it, because a dry run takes no probe and the Decider never runs dry. It is not fixed in this card; #60 gets the narrow fix (keep the refusals on a dry run, return only `history=None`). Review round 3 has no BLOCKING, so the card goes to sign-off.
