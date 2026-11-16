CHANGES

# mob-defaults #62: QA review (claude, claude-opus-5-5, ctx-62q, fresh)

Range `git diff 1ff374f..1fdfa0b` (card-62), read in a read-only clone. No repo file edited, no real probe or kickoff check run, `~/.mob-locks` not read, and seat-use.jsonl was only read. Scratch reproductions ran in a temp git repo and a temp locks dir in the session scratchpad.

## Scenarios on production behaviour

- [Z] Met. `kickoff_problems` names all eight with "no probe row in the last 60 minutes" on empty history and on old-format history.
- [O] Met. `probe_all` writes one `event: probe` row per enabled POOL service with `service`, `result` (ok/quota/error), `error` and `taken_at`, through `facilitator.probe_backend` (each service's configured invocation). The exception is lock contention (B1).
- [M] Met. An ok row needs a seat or a non-blank `no_fit`. Quota and error rows need nothing. The declaration's `card` is not compared (M4).
- [B] Met. Exactly 60 min is refused; 59:59.999999 and 60 min minus 1 µs are fresh. A missing service is a refusal (problem string, exit 1), never a warning.
- [I] Met. The qwen row is `error`, "gate busy: <lock name> <OWNER>", and qwen is not launched. The check needs nothing for an error row.
- [E] Met. There is no skip from history; every enabled POOL service is probed on every run.
- [S] Met for the normal case. The edge cases are in M2 and M3.

## Real seat-use.jsonl (`~/projects/phrase-lang/.mob/records/seat-use.jsonl`, read-only, pure functions, now = 2026-11-16T15:03Z)

- The file has 2733 rows, none unparseable: 2697 old-format six-field rows, 19 `outcome: launched` rows and 17 `event: outcome, outcome: success` rows. It has no `event: probe` rows yet.
- `idle_services` answers `[('codex','no probe row'), ('copilot','no probe row'), ('grok','no probe row'), ('qwen','no probe row')]`. agy, claude, cursor and muse each have a joined success in the last 24 h. Old-format rows never count as a success, which is the intended reading.
- `kickoff_problems`, given the md62 declaration as JSON (seats claude, cursor, agy, muse; no_fit qwen), answers all eight with "no probe row in the last 60 minutes". This is expected because the 14:43:10Z kickoff probe went only to `probes/2026-11-16.jsonl`, which the check does not read. The first kickoff after adoption must run the new `probe_all.py` first.

## Facilitator's questions

(a) `wait=0` on the seat-use lock. It is loud, not silent, but it loses the whole run. See B1.

(b) A service disabled in backends.json. Yes: as built, every kickoff is refused with "<svc>: no probe row in the last 60 minutes" until it is re-enabled. The card ([B]: "a service skipped without a probe row is always a refusal") supports a refusal. It does not support making the refusal permanent, or a message that hides the cause. See M1.

## Findings

### B1 BLOCKING: probe_all drops every probe result, including the pre-existing probes jsonl, when the seat-use lock is held at the moment it appends

- Evidence: `probe_all.py:74` and `:76` pass `wait=0`. `probe_all.py:100` appends to the seat log before the probes jsonl is written (`:104`).
- With `wait=0`, `hold_seat_use_lock` raises `SeatRefused` on the first busy attempt. It does not wait the default 30 s that every other writer waits.
- Reproduced in a temp git repo: hold `.git/mob-seat-use.lock`, then run `probe_all.main` with a stubbed `probe_backend`. Result: `SeatRefused: probe_all cannot start: the seat-use lock ... is held by pid ...`, then `seat-use exists False`, `probes.jsonl exists False`.
- The traceback is not silent. But up to 120 s of eight paid probe calls are discarded, and the probes jsonl that `probe_all` always wrote before this change is lost too, which is a regression.
- Any seat of another card appending a launch or outcome row at that moment triggers it. The kickoff then fails closed (all eight stale) and must be re-probed.
- No test requires `wait=0`; `tests/test_kickoff_probe_pool.py` has no wait assertion.
- Fix: use the default wait (`wait=None`). Also write the probes jsonl before the seat-use append, or in a `finally`, so the evidence survives a refusal.

### M1 MINOR: a disabled (or missing) pool service gets no probe row, so every kickoff is refused with a misleading reason

- Evidence: `probe_all.py:94` filters on `enabled`.
- `facilitator.probe_backend` already returns `{"ok": False, "state": "disabled", "evidence": "disabled in backends.json"}` for a disabled backend (`facilitator.py:857-858`).
- Fix: drop the `enabled` filter, so a disabled service writes `result: error, error: "disabled in backends.json"`. The check then passes it (an error row needs nothing) while the cause stays visible, and the retro's idle report names it.
- If the owner wants a disabled service to block kickoffs, the refusal text should at least say why.

### M2 MINOR: an outcome is credited to the wrong service when one card/context/role was launched twice on different services

- Evidence: `mob_process.py:209` keys pending launches by `(card, context, role)` without `seat`, and `:221` takes the latest launch.
- Reproduced: launch muse, then launch cursor (same card, context and role), then a muse `success` outcome. The success is credited to cursor; muse stays idle.
- `last_seating` keys by `(seat, card, context, role)`; this join should do the same.

### M3 MINOR: an unparseable or null `taken_at` (or outcome `ts`) crashes the check and the idle report instead of being read as no fresh row

- Evidence: `mob_process.py:190` calls `probe_is_stale` with `parse_time(None)`, `''` or `'garbage'`, which raises `TypeError`/`ValueError`. `idle_services` raises the same way on a bad `ts`.
- `kickoff_check` exits 1 with a traceback, so it still refuses, but it names no service. `seat_log.py idle-report` dies.
- `probe_backend` always sets `taken_at`, so this needs a hand-edited or foreign row. Still, a hand-edited row must be read as a refusal, not crash the check.
- A future `taken_at` (`15:30Z` read at `now=15:00Z`) counts as fresh. Consider bounding it.

### M4 MINOR: the declaration's `card` is not compared

[M] says "seated in a declared role on this card". `kickoff_problems` ignores `declaration["card"]`, and an entry with an empty `role` counts as seated. Low risk, since the declaration file is per card.

### M5 MINOR: the `--now` flag is required

The slice says `[--now ISO]` (optional), but `kickoff_check.py:36` makes `--now` required. A real kickoff has to type the current time, or argparse exits 2. Default it to the current UTC time.

### M6 MINOR: cosmetic reason text

- `local_driver.py:140`: `OWNER=(.+)` swallows any tokens after OWNER when OWNER is not the last line. Reproduced: `'heavy-test.lock avowry WINPID=36084 UTC=...'`.
- `local_driver.py:144` returns "gate busy", so the row reads `gate busy: gate busy`.
- An unparseable lock's reason is the bare file name, with no "unparseable".
- `idle_services` gives an empty reason when the latest probe row is ok (`mob_process.py:237`). Something like "probed ok, not seated" would serve the retro better.

## heavy_run_reason on real lock shapes (R62-1)

Run in a temp locks dir with a fake `pid_alive` (36084 alive):

| Lock content | Answer |
|---|---|
| one-line play-tracker shape | `'heavy-test.lock play-tracker ship train'`, asked `[36084]` (WINPID, not PID) |
| one-per-line Avowry shape | `'heavy-test.lock avowry'` |
| bare integer, with or without whitespace or CRLF | busy, asks the integer |
| `garbage`, empty, or `WINPID=abc` | busy (the file name), asks nothing |
| PID only | asks PID |
| BOM-prefixed | parsed |
| dead WINPID | None |
| dead WINPID with the test gate busy | `gate busy` |

The mob's own lock: while Avowry's own heavy run holds `heavy-avowry.lock`, its own probe gives qwen `error, gate busy: heavy-avowry.lock avowry`. That is correct, since qwen shares the machine. It is not a false refusal, because the check needs nothing for an error row.

## Time handling

`taken_at` with Z, with +00:00, naive (read as UTC), with microseconds, with a space separator, and in compact basic format all parse on Python 3.15.

| Probe age at `now` | Result |
|---|---|
| 60 min exactly | refused |
| 60 min minus 1 µs | fresh |

| Success age at `now` | Counted? |
|---|---|
| 24 h exactly | counted (not idle) |
| 24 h plus 1 µs | not counted (idle) |

## Run

`python -m pytest tests/test_kickoff_probe_pool.py tests/test_probe_all.py -q --capture=sys`: `70 passed in 10.92s`
