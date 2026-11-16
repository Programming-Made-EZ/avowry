SIGN-OFF

# mob-defaults #62: QA review round 2 (claude, claude-opus-5-5, ctx-62q)

Range `git diff 1fdfa0b..98ccece`: fix round 1 red (2ef5f79), the R62-3 helper fix (bd9327f) and fix round 1 green (98ccece). I read it in the read-only clone against rulings R62-2 and R62-3.

I edited no repo file and ran no real probe_all or kickoff_check. `~/.mob-locks` was not read. The reproductions used temp git repos and a temp locks dir in the session scratchpad, with a stubbed `probe_backend` (the real one only for its disabled branch, which makes no call).

## Round 1 findings

### B1: FIXED

The probes jsonl is now written first (`probe_all.py` main). The seat-use append uses the default wait. A `SeatRefused` is printed on stderr and the run exits 1.

Reproduced with a held lock in two temp repos:

| Case | Exit | Probes jsonl | Seat-use rows | Notes |
|---|---|---|---|---|
| Another writer held the lock 3 s | 0 | 8 | 8 | waited 3.1 s |
| The lock was held past the wait (`SEAT_USE_LOCK_WAIT` set to 1 s for the repro) | 1 | 8 (survived) | no file | stderr: `probe_all cannot start: the seat-use lock ... is held by pid ... and was not released within 1 seconds; no line was written ...` |

### M1: FIXED

Every POOL service present in backends.json is probed, whether enabled or not.

- With grok disabled and copilot removed, the rows are `('grok','error','disabled in backends.json')` and `('copilot','error','not configured')`.
- `kickoff_problems` then passes with every other service seated.

### M2: FIXED for the reported case

- The retry case (launch muse, launch cursor, muse success) now credits muse.
- New fallback: see N1.

### M3: FIXED

- A `taken_at` of None, `''`, `'garbage'` or an int counts as no fresh row and does not crash.
- More than 5 minutes in the future is not fresh; the test covers 4 minutes (fresh) and 10 minutes (not fresh).
- A bad outcome `ts` is ignored by `idle_services` and by `idle-report`.

### M4: FIXED

- `--card` is compared with the declaration's `card`.
- A seat with an empty or non-string role or service does not count as seated.
- A type mismatch remains: see N2.

### M5: FIXED

`--now` is optional and defaults to the real UTC clock.

### M6: FIXED

| Input | Reason |
|---|---|
| play-tracker one-line lock | `heavy-t.lock play-tracker ship train` |
| Avowry lock with OWNER mid-file | `heavy-t.lock avowry` |
| test gate busy | `test or gate lock active`, so the qwen row reads `gate busy: test or gate lock active` (once) |
| idle service whose latest probe was ok | `probe ok, not seated in 24 h` |

## No existing test weakened

- The only removed lines in `tests/` are the two helper signatures that R62-3 rules on.
- `probe_row(..., at=_OMIT)` gives `ago(10)` only when the argument is omitted. `all_fresh(at=_OMIT)` passes the sentinel through, so `all_fresh()` still writes `ago(10)`.
- No existing caller passes `at=None`, `""` or a falsy value to either helper. I grepped every call. The only None, `""` and garbage values are in the new M3 tests, which is R62-3's purpose.
- Every round 1 test is otherwise unchanged and still passes.

## New findings from the fix

### N1 MINOR: the M2 fallback can still credit a different seat's launch

- Evidence: `mob_process.py` `_success_services_within`, the `by_ctx` fallback. When no launch matches `(seat, card, context, role)` and exactly one launch shares `(card, context, role)`, the outcome is credited to that launch's service, whatever its seat.
- Reproduced: a cursor launch in ctx `c`, plus a hand-launched muse success outcome in the same ctx with no launch row. The success is credited to cursor, and muse stays idle.
- The fallback exists to keep `test_62_s_an_outcome_joins_to_its_launch_rows_service` passing. In that test the outcome's seat is `"seat-wrapper"`, which is not a pool name.
- Suggested narrowing: use the fallback only when the outcome's own `seat` is not a POOL service.
- The `len(candidates) > 1` branch can never find a launch the exact-key lookup missed, so it is dead code.
- Real `seat_log.py` writes the same `seat` on launch and outcome rows, so the effect is limited to hand-written rows. That is why this is MINOR.

### N2 MINOR: `--card` compares types strictly

A declaration with `"card": 62` (an int) and `--card 62` gives the problem `declaration card 62 does not match --card '62'`. Comparing `str(declaration.get("card"))` would avoid a false refusal. The md62 declarations use strings, so this is MINOR.

### N3 MINOR (cosmetic)

- `local_driver.py` still imports `re`, which is now unused.
- A future-dated or garbage `taken_at` is reported as "no probe row in the last 60 minutes". A row exists, so the text could name the bad stamp.
- The B1 stderr message says "probe_all cannot start", but the probes already ran. It comes from the shared `hold_seat_use_lock` text, so it is acceptable.

None of N1 to N3 breaks a scenario or the R62-2 ruling, so this round signs off.

## Run

`python -m pytest tests/test_kickoff_probe_pool.py tests/test_probe_all.py -q --capture=sys`: `89 passed in 19.37s`
