# mob-defaults #62: the facilitator's slice (light work, drafted while #61 waits; 2026-11-16)

The card: `gh issue view 62 -R Programming-Made-EZ/mob-defaults`, which has seven scenarios. It comes from the owner's 2026-11-15 rotation-pool ruling and its addendum. It depends on #61, whose seat rows carry service, model and outcome and whose `seat_log.py` logs every seat, so it starts after #61 is signed off. The Mob Board order is #61, then #58, then #62. This slice is written now as light work and is binding once the card's declaration Decider adopts it.

## Today

- `probe_all.py` probes every enabled backend in `core/backends.json`, which has all eight enabled: agy, claude, codex, copilot, cursor, grok, muse and qwen. It appends one row per driver to `.mob/records/probes/<date>.jsonl` (driver, state, ok, taken_at, evidence, head, row_sha256).
- Nothing reads those rows at kickoff, and nothing refuses a kickoff. The D236 seat-plan check (`check_seat_plan.py`) was never built.
- The probe rows do not reach the seat log, so the retro cannot list idle services from one file.

## Design

1. **Probe rows in the seat log [O][I].**
   - `probe_all.py` also appends one row per probed service to `.mob/records/seat-use.jsonl`, through the same lock and append-only rule as #61's writers. The row is `{"event": "probe", "seat": <service>, "service": <service>, "result": "ok" | "quota" | "error", "error": <evidence when not ok>, "taken_at": <probe time>, "card": <card or null>}`.
   - Mapping: state `available` is `ok`; `quota-reached` is `quota`; everything else is `error` with the evidence text.
   - qwen with the gate busy (the cross-mob heavy lock held, or a test process alive) is `error` with the reason `gate busy`.
   - Probe rows are never seatings in `last_seating`, just as #61's outcome lines are not.
2. **`kickoff_check.py <repo> <declaration file> [--card N] [--now ISO]` [Z][M][B].** It exits 0 when the kickoff may start and 1 with every reason otherwise.
   - The pool is every enabled service in `core/backends.json`.
   - **Refuse** when any pool service has no `event: probe` row in the seat log taken less than 60 minutes before `--now`. Name each such service ([Z], [B]). A service with no probe row at all is a refusal, never a warning.
   - **Refuse** when a service whose latest fresh probe row is `ok` is neither seated in the declaration nor named in a no-fit line. Name each such service ([M]).
   - Seated means the service appears in a task-force line of the declaration (`- <Role>: <service> ...`).
   - A no-fit line is `No-fit: <service>: <reason>`, and the reason must be non-empty.
   - qwen's `gate busy` error is accepted as a fresh row; it needs no no-fit line ([I]).
3. **Automatic re-probe [E].** The kickoff checklist runs `probe_all.py` before `kickoff_check.py`. A service that was out at the last kickoff is therefore probed again and, if ok, must be seated or given a reason. No memory of the earlier outage is needed: the check reads only fresh rows.
4. **The retro's idle-service list [S].** `seat_report.py <repo> --since-hours 24 [--now ISO]` lists every pool service with no `success` outcome line (#61) in the window, with the result and error of its latest probe row. The retro's Model allocation review pastes its output.

## Tests (the red writes them first; Python, `--capture=sys`)

There is one test file, `tests/test_kickoff_pool_check.py`, with at least one test per scenario.
- Seat-log fixtures are built in a temp repo. `probe_all.py` gets a stubbed `facilitator.probe_backend`, with no network and no real CLI.
- `--now` makes every freshness test deterministic; no wall-clock assertions (D254).
- The tests also cover: an old-format seat log (no probe rows: refusal names all eight), a 59- and a 60-minute-old row (the boundary), a declaration with an empty no-fit reason (refusal), and probe rows ignored by `last_seating` and `claude_wait_problem`.

## Seats (proposed for the declaration; ruling R-ROT applies)

Each seat is pinned, and no service holds more than two task-force seats:
- red: claude sonnet;
- green: cursor composer-2.5 or muse medium, whichever probes ok at the kickoff;
- QA: claude opus, plus agy as the cross-family reviewer;
- sign-off: a service other than the green's;
- voters across at least four services.

Before the declaration vote, the kickoff itself runs `probe_all.py`. Once this card ships, it also runs `kickoff_check.py`.
