# mob-defaults #62: green record (green seat: cursor, composer-2.5, ctx-62g)

Worktree wt-md62, branch card-62. No commit.

## git diff --stat

```
 kickoff_check.py | 48 ++++++++++++++++++++++++++++++++++++++++++++++
 local_driver.py  | 65 +++++++++++++++++++++++++++++++++++++++++++++++++++++
 mob_process.py   | 92 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 probe_all.py     | 42 +++++++++++++++++++++++++++++++++++++---
 seat_log.py      | 27 ++++++++++++++++++++++++-
 5 files changed, 269 insertions(+), 5 deletions(-)
```

## What changed

- `mob_process.py`: `POOL`; pure `probe_row`, `kickoff_problems`, `idle_services` (24 h success join, 60 min freshness via `probe_is_stale`).
- `kickoff_check.py`: CLI reads seat-use history and declaration JSON, calls `kickoff_problems`, prints problems and exits 1 on refusal.
- `local_driver.py`: `heavy_run_reason` over `heavy-*.lock` (KEY=value, WINPID else PID, bare integer, unparseable busy) then `gate_busy`.
- `probe_all.py`: probes enabled POOL backends only; `gate_reason` seam (default `heavy_run_reason` + `~/.mob-locks`); appends one probe row per service under the seat-use lock.
- `seat_log.py`: `idle-report --repo` prints idle pool services with reasons (real clock, no `--now`).

## Run lines

- `python -m pytest tests/test_kickoff_probe_pool.py -q --capture=sys`: `66 passed in 10.91s`
- `python -m pytest tests/test_probe_all.py tests/test_seat_use_model_outcome.py tests/test_record_task_force_probes.py -q --capture=sys`: `160 passed, 55 subtests passed in 189.18s (0:03:09)`

## Fix round 1 green (R62-2)

### git diff --stat

```
 kickoff_check.py | 11 ++++++++---
 local_driver.py  | 23 ++++++++++++++++++++---
 mob_process.py   | 55 +++++++++++++++++++++++++++++++++++++++++++++++++------
 probe_all.py     | 24 ++++++++++++++++--------
 4 files changed, 93 insertions(+), 20 deletions(-)
```

### What changed

- `probe_all.py` (B1, M1): probes every POOL name (disabled backends still probed; missing backends get `not configured` without a launch). Writes probes jsonl before seat-use append; default lock wait (no `wait=0`); on `SeatRefused`, probes jsonl kept, stderr + exit 1.
- `mob_process.py` (M2–M4, M6): `probe_row_is_fresh` (unparseable / >5 min future not fresh); outcome join on `(seat, card, context, role)` with single-launch fallback for non-pool outcome seats; seated services need non-empty role and service; idle ok probes → `probe ok, not seated in 24 h`.
- `kickoff_check.py` (M4, M5): optional `--card` (mismatch is a problem); optional `--now` (defaults to UTC now).
- `local_driver.py` (M6): OWNER text stops at the next KEY= token; gate-only busy reason is `test or gate lock active` (avoids `gate busy: gate busy` in qwen rows).

### Run lines

- `python -m pytest tests/test_kickoff_probe_pool.py -q --capture=sys`: `84 passed, 1 failed in 16.78s` — `Test62Fix1.test_62_f1_m3_a_null_or_garbage_taken_at_is_not_a_fresh_row_and_does_not_crash` fails on `bad=None` only: the red `probe_row` helper uses `taken_at: at or ago(10)`, so `at=None` and `at=""` become a fresh stamp before `kickoff_problems` runs; production treats `garbage` and `12345` correctly. Red should use `taken_at: ago(10) if at is None else at` (or literal dicts) — cannot fix in production without editing the test.
- `python -m pytest tests/test_probe_all.py tests/test_seat_use_model_outcome.py tests/test_record_task_force_probes.py -q --capture=sys`: `160 passed, 55 subtests passed in 242.06s (0:04:02)`