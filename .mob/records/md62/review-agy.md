CHANGES

# mob-defaults #62: cross-family review (agy, gemini-3.8-flash-medium, ctx-62x, fresh)

Range `git diff 1ff374f..1fdfa0b` (card-62), read in a read-only clone. No repo file edited, no real probe or kickoff check run, `~/.mob-locks` not read, and seat-use.jsonl was only read. Scratch reproductions ran in a temp git repo and a temp locks dir in the session scratchpad.

## Scenarios on production behaviour

- [Z] Met. `kickoff_problems` names all eight with "no probe row in the last 60 minutes" on empty history and on old-format history.
- [O] Met in general. `probe_all` writes one `event: probe` row per enabled POOL service with `service`, `result` (ok/quota/error), `error` and `taken_at`, through `facilitator.probe_backend`. However, lock contention drops all probe rows (B1).
- [M] Met. An ok row requires a seat or a non-blank `no_fit`. Quota and error rows need neither seat nor reason.
- [B] Met. Exactly 60 min is refused; 59m 59.999s is fresh. A missing service produces a refusal, never a warning.
- [I] Met. The qwen row is `result: error`, `error: "gate busy: <lock name> <OWNER>"`, and qwen is not launched. `kickoff_check` accepts this without requiring a seat or reason.
- [E] Met. Probes are rerun regardless of earlier history.
- [S] Met for standard rows; edge cases noted in M2 and M3.

## Real seat-use.jsonl (`~/projects/phrase-lang/.mob/records/seat-use.jsonl`, read-only, pure functions, now = 2026-11-16T15:03Z)

- File contains 2733 rows: 2697 old-format rows, 19 launch rows (`outcome: launched`), and 17 outcome rows (`event: outcome, outcome: success`). No `event: probe` rows exist yet.
- `idle_services` answers `[('codex', 'no probe row'), ('copilot', 'no probe row'), ('grok', 'no probe row'), ('qwen', 'no probe row')]`. Services `agy`, `claude`, `cursor`, and `muse` each have a joined `success` outcome within the last 24 h.
- `kickoff_problems` answers all eight services with "no probe row in the last 60 minutes", as expected prior to running the updated `probe_all.py`.

## Facilitator's questions

(a) `wait=0` on the seat-use lock: Loud, not silent, but discards the whole probe run. When another seat holds the lock, `probe_all` crashes with an uncaught `facilitator.SeatRefused` exception. All 8 probe results are lost, and neither `seat-use.jsonl` nor `probes/<date>.jsonl` is written. See B1.

(b) A service disabled in backends.json: Yes; `probe_all.py:94` filters out disabled backends so no probe row is logged. `kickoff_problems` iterates over the hardcoded 8-service `POOL`, finds no fresh row, and refuses every kickoff with `<service>: no probe row in the last 60 minutes`. Because missing probe rows cannot be excused by a `no_fit` reason, disabling a backend permanently blocks all kickoffs. See M1.

## Findings

### B1 BLOCKING: probe_all drops every probe result, including the pre-existing probes jsonl, when the seat-use lock is held at the moment it appends

- Evidence: `probe_all.py:74` and `:76` pass `wait=0`. `probe_all.py:100` appends to the seat log before `probes/<date>.jsonl` is written (`:104`).
- With `wait=0`, `hold_seat_use_lock` raises `SeatRefused` immediately on contention without waiting the default 30 s.
- In `_append_probe_rows`, there is no `try...except` handler.
- Reproduction in a temporary git repo: holding `.git/mob-seat-use.lock` and invoking `probe_all.main` raises `SeatRefused: probe_all cannot start: the seat-use lock ... is held by pid ...`. Both `seat-use.jsonl` and `probes.jsonl` are not created/appended.
- Consequence: Up to 120 s of 8 paid probe invocations are discarded, and the existing `probes/<date>.jsonl` log is never written.
- Fix: Use the standard wait timeout (`wait=None` or default 30 s). Write `probes/<date>.jsonl` before appending to `seat-use.jsonl` (or inside a `finally` block) so probe diagnostic records are preserved even if seat-use locking fails.

### M1 MINOR: a disabled (or missing) pool service gets no probe row, so every kickoff is refused with a misleading reason

- Evidence: `probe_all.py:94` filters backends with `configured[n].get("enabled")`.
- `facilitator.probe_backend` already returns `{"ok": False, "state": "disabled", "evidence": "disabled in backends.json"}` for disabled backends (`facilitator.py:857-858`).
- Consequence: A disabled service gets no row, so `kickoff_problems` reports `no probe row in the last 60 minutes`, which cannot be bypassed with a `no_fit` reason.
- Fix: Remove the `enabled` filter so `probe_backend` logs a disabled error row (`result: error, error: "disabled in backends.json"`), which `kickoff_check` accepts without requiring a seat.

### M2 MINOR: an outcome is credited to the wrong service when one card/context/role was launched twice on different services

- Evidence: `mob_process.py:209` keys pending launches by `(card, context, role)` without `seat`, and line 221 takes the latest launch.
- Reproduction: Launch service A, then launch service B under the same `(card, context, role)`. A subsequent `success` outcome for service A is credited to service B; service A remains idle.
- Fix: Include `seat` in the pending launch key: `(seat, card, context, role)`.

### M3 MINOR: an unparseable or null `taken_at` crashes kickoff_problems instead of being treated as stale/missing

- Evidence: `mob_process.py:190` passes `row.get("taken_at")` directly to `probe_is_stale`, where `parse_time(None)` or unparseable string raises `TypeError`/`ValueError`. Similarly, `idle_services` raises on bad `ts`.
- While `probe_backend` produces valid timestamps, malformed rows from manual edits crash the check with a Python traceback rather than a structured refusal.
- Fix: Wrap `parse_time` in `probe_is_stale` with try/except, or guard `if not row.get("taken_at")`.

### M4 MINOR: the declaration's `card` is not validated

- Evidence: `mob_process.py:181-198` ignores `declaration.get("card")`, and an entry with an empty `role` counts as seated.
- Low risk since declaration files are typically per-card, but the card ID is not checked against the current run context.

### M5 MINOR: the `--now` CLI option is required in kickoff_check.py

- Evidence: `kickoff_check.py:36` declares `parser.add_argument("--now", required=True)`.
- The slice specifies `[--now ISO]` (optional). In practical interactive use, requiring `--now` forces operators to type the current ISO timestamp manually.
- Fix: Make `--now` optional, defaulting to `datetime.now(timezone.utc)`.

### M6 MINOR: minor text formatting inconsistencies in reasons

- `local_driver.py:140`: `re.search(r"\bOWNER=(.+)", text.replace("\n", " "))` captures subsequent tokens across lines if `OWNER=` is not on the final line.
- `local_driver.py:144`: Returns `"gate busy"`, which `probe_all.py:61` formats as `"gate busy: gate busy"`.
- `idle_services`: Produces an empty reason string for a service whose latest probe succeeded (`result: ok`) but was never seated. A descriptive string like `"probed ok, unseated"` would be clearer.

## heavy_run_reason on real lock shapes (R62-1)

Tested against mock `pid_alive` (PID 36084 alive):
- One-line shape (`PID=392343 WINPID=36084 ... OWNER=play-tracker ship train`): inspects WINPID 36084 (not PID 392343); returns `'heavy-test.lock play-tracker ship train'`.
- One-per-line shape (`PID=...\nWINPID=36084\n...\nOWNER=avowry`): inspects WINPID 36084; returns `'heavy-test.lock avowry'`.
- Bare integer (`4242`): inspects 4242; returns `'heavy-test.lock'`.
- Unparseable / empty / garbage: treated as busy without inspecting PIDs; returns `'heavy-test.lock'`.
- Mob's own lock: Avowry's own heavy run holding `heavy-avowry.lock` is treated as busy, correctly marking qwen as gate busy and allowing kickoff to pass.

## Time handling

`taken_at` formats with `Z`, naive (interpreted as UTC), with microseconds, and space-separated date/time parse reliably via `parse_time`.
- 60-minute freshness boundary: exactly 60m is stale (refused); 59m 59.999s is fresh.
- 24-hour idle boundary: exactly 24h is within the cutoff (counted as active); > 24h is idle.

## Run

`python -m pytest tests/test_kickoff_probe_pool.py tests/test_probe_all.py -q --capture=sys`: `70 passed in 7.88s`
