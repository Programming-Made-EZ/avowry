# mob-defaults #62: red record (red seat: claude, claude-sonnet-5-5, ctx-62r)

File: `~/projects/wt-md62/tests/test_kickoff_probe_pool.py` (branch card-62 at 1ff374f, nothing committed). No production code, template or Avowry file touched.

## Counts
- Test62Pending: 53 tests, all fail on today's code.
- Test62Characterisation: 8 tests, all pass on today's code.

## Tests by scenario (Test62Pending unless noted)
- [Z] 3: no rows -> `kickoff_problems` names all eight; CLI exit 1 names all eight; `idle_services([])` lists all eight "no probe row".
- [O] 10: `probe_row` ok / quota-reached / every other state; probe_all appends one row per pool service; result mapping per probe result; launch failure is an error row; configured invocation per service (muse medium effort); append under the seat-use lock; existing history kept.
- [M] 8: ok unseated no reason; no_fit reason passes; blank reason (empty, spaces, None) fails; all seated passes; many unseated each named; two roles one service; quota/error rows need nothing; CLI exit 0 and 1.
- [B] 10: 60 min refused; older refused; 59 min fresh; 59m59s fresh and exactly 60m not; missing service refused; no_fit does not excuse missing row; seat does not excuse missing row; only `event: probe` rows count; latest row wins; CLI 60 vs 59.
- [I] 11 pending: live-PID heavy lock (temp locks dir); dead PID not busy; non-heavy lock ignored; empty dir not busy; busy test gate gives reason; qwen gate busy row and no launch (gate seam); qwen probed when seam returns None; default gate_reason calls `heavy_run_reason(repo, locks_dir=~/.mob-locks)` (patched, never touched); gate-busy error row needs nothing; no_fit citing gate busy accepted; CLI accepts gate-busy qwen.
- [E] 4: quota and error rows probed again with fresh rows; every service probed again whatever earlier rows say; recovered service is then ok and needs a seat; old quota row does not hide a fresh ok row.
- [S] 7: success within 24 h not idle; success outside 24 h idle; only success counts; outcome joins launch row's service; outcome with no launch uses own seat; reason is latest probe row's error text; no probe row says so; `seat_log.py idle-report` output.
- Characterisation (8): probe_all still probes all enabled and writes the probes jsonl with row_sha256; no skip from earlier rows; OUTCOME_VALUES and launch line shape; `gate_busy` true on fresh gate.lock, on a test process, false on stale lock and with none; seat_log outcome parser; `probe_is_stale` 60/59.

## Pending failure reasons (one line each, grouped)
- 21 tests: `AttributeError: module 'mob_process' has no attribute 'kickoff_problems'` (the M, B, E and I check tests, and Z1).
- 8 tests: `AttributeError: ... no attribute 'idle_services'` ([S] and Z3).
- 3 tests: `AttributeError: ... no attribute 'probe_row'` ([O] mapping).
- 4 tests: `ModuleNotFoundError: No module named 'kickoff_check'` (the CLI tests, Z/M/B/I).
- 5 tests: `AttributeError: module 'local_driver' has no attribute 'heavy_run_reason'` (the heavy-lock and gate-reason unit tests).
- 1 test: `SystemExit: 2` (`seat_log.py idle-report` is not a subcommand; argparse rejects it).
- 11 probe_all tests fail by assertion/KeyError because no probe row reaches seat-use.jsonl: `0 != 8` / `Lists differ: [] != [8 services]` (o all-rows, o launch-failure, o configured invocation (rows part), qwen-probed-when-None, row-result mapping KeyError 'agy'/'qwen', default gate reason KeyError 'qwen', qwen gate busy `0 != 8`); `1 != 9` (existing history kept, no rows appended); `8 != 16` and `[] != [('codex','ok'),('grok','ok')]` (E); `[] is not true : no seat-use lock was taken` (O lock); `0 != 1` (the gate seam test).

## Run lines
- `python -m pytest tests/test_kickoff_probe_pool.py -q --capture=sys`: `53 failed, 8 passed in 16.42s`
- `python -m pytest tests/test_probe_all.py tests/test_seat_use_model_outcome.py -q --capture=sys`: `135 passed in 206.90s (0:03:26)`

## Ambiguities (the tests pick one reading; green may need the card to confirm)
1. SETTLED by R62-1 (heavy lock format: real KEY=value shapes, WINPID else PID, bare integer accepted, unparseable is busy). The tests now use those shapes; liveness still goes through a patched `local_driver.pid_alive`.
2. `heavy_run_reason(repo, locks_dir=...)` is called with `locks_dir` as a keyword and the repo first; the default gate seam must name a directory ending `.mob-locks`.
3. The default `gate_reason` is tested only through a patched `local_driver.heavy_run_reason`; the real `~/.mob-locks` is never read.
4. `kickoff_problems(history, declaration, now)`: `now` is an aware datetime, history a list of dicts. `kickoff_check --now` takes `2026-11-16T15:00:00Z`.
5. "60 minutes or more" is stale (exactly 60m refused, 59m59s fresh), per `probe_is_stale`.
6. A blank, whitespace or null `no_fit` reason is not a reason.
7. A service seated in two roles counts once and is not a problem; quota and error rows (fresh) need nothing, not even a reason; only gate-busy was named in [I], so a plain error row needs nothing by [M].
8. Every problem string names the service (substring) and a missing or stale row's text contains `no probe row in the last 60 minutes`.
9. `kickoff_problems` and the CLI do not print on success (CLI exit 0 prints nothing).
10. `idle_services` returns pairs; reason for a service with a latest probe row is its `error` text (also for a quota row; the slice says "result: error text"); the boundary at exactly 24 h is untested.
11. `seat_log.py idle-report` has no `--now`, so its test builds rows relative to the real clock; it prints one line per idle service containing the service and its reason, nothing for non-idle services, and exits 0.
12. probe_all seams: `facilitator.probe_backend` patched on the facilitator module, `probe_all.gate_reason` as a module attribute called once (qwen is the only gate_bound backend), `probe_all.load_backends` real. The seat-use row's `taken_at` is the probe result's own `taken_at`. A probe that raises becomes an `error` row with the exception text.
13. The seat-use lock is checked by spying on `facilitator.hold_seat_use_lock` / `hold_fallback_seat_use_lock`; an implementation that locks some other way fails that one test.

## Red fix (R62-1)
Replaced the two bare-PID heavy_run_reason tests with seven: live one-line shape (file and OWNER named, WINPID asked, PID not asked); live one-per-line shape; dead WINPID is not busy (both shapes); WINPID checked not PID when both present; PID checked when no WINPID; bare integer still accepted; unparseable lock (`garbage`, no PID token, empty) is busy and names the file. The not-a-heavy-lock, empty-dir and busy-gate tests and all other tests are unchanged; no qwen gate-busy test writes a lock file.
- Run: `python -m pytest tests/test_kickoff_probe_pool.py -q --capture=sys`: `58 failed, 8 passed in 17.00s`
- Pending count: 58 (was 53); characterisation still 8. The seven new tests fail on `AttributeError: module 'local_driver' has no attribute 'heavy_run_reason'`.

## Fix round 1 red (R62-2)
Worktree at 1fdfa0b. Added class `Test62Fix1` (19 tests, `test_62_f1_<finding>_...`) to `tests/test_kickoff_probe_pool.py`; pure additions (236 lines), no existing test edited and none contradicts R62-2. All 19 fail on today's code.
- B1 (3): waits for a seat-use lock held by another holder (a thread holds the in-process lock 0.4 s; fails with SeatRefused "not released within 0 seconds"); the lock wait is not 0; the probes jsonl is written and exit is 1 with the error on stderr when the append fails (lock seam raises SeatRefused).
- M1 (2): disabled pool service is probed and gets an `error` row "disabled in backends.json"; a service absent from backends gets `error` "not configured" and is not launched.
- M2 (1): muse launched, cursor launched (same card, context, role), muse success: muse not idle, cursor idle.
- M3 (5): null, empty, garbage and non-string `taken_at` is not fresh and does not crash; +10 min future is not fresh (+4 min is fresh); CLI refuses naming the service with no traceback; garbage/null outcome `ts` ignored by `idle_services`; idle-report survives a garbage ts.
- M4 (3): `--card` mismatch is a problem (names both cards); matching passes and no `--card` skips the check; an empty/blank/null role or service does not count as seated.
- M5 (2): no `--now` works and defaults to the real UTC clock (fresh rows exit 0, 2-hour-old rows exit 1).
- M6 (3): reason names only OWNER up to the next KEY= token (both lock shapes); the qwen row reads "gate busy" once (default gate seam with a patched `Path.home` and `gate_busy`, never the real locks dir); an idle service whose latest probe was ok reads "probe ok, not seated in 24 h".
- Failure reasons: B1 SeatRefused / `[] != ...` / assertions on code and jsonl; M1 row-set assertions; M2 assertion; M3 `ValueError/TypeError` from `parse_time` and assertions; M4 argparse `SystemExit: 2` (no `--card`) and assertion; M5 `SystemExit: 2` (`--now` required); M6 assertions.
- Run: `python -m pytest tests/test_kickoff_probe_pool.py -q --capture=sys`: `19 failed, 66 passed in 19.06s` (the 19 are exactly Test62Fix1; the 66 are the earlier 58 now green plus the 8 characterisation tests).
- Assumptions: stderr message text is the SeatRefused text; `--card` compares as strings; the `not configured` and `probe ok, not seated in 24 h` texts are verbatim from R62-2 (M1 text for the disabled row is the facilitator's `disabled in backends.json` evidence).

## Red fix (R62-3)
The probe-row helper wrote `taken_at: at or ago(10)`, so None and "" became fresh rows and the M3 null/empty case never reached production. `probe_row` now defaults only an omitted argument (sentinel `_OMIT`), so None and "" are written as given. The same sentinel is the default of `all_fresh(at=...)`, which forwards its `at` to `probe_row` (without it every all_fresh row would be written with a null taken_at). No other edit; no production file touched.
- Run (fix round 1 green uncommitted in the worktree): `python -m pytest tests/test_kickoff_probe_pool.py -q --capture=sys`: `85 passed in 13.73s`
