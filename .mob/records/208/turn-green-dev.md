# Card 208, green developer (ctx-4): turn record

This turn record is written at close, from the mob-defaults commits below; it is not a live transcript. Repository: mob-defaults (C:\Users\ringe\mob-crosscli).

## Criterion 1 (a seat starts only on a probe under 60 minutes old)
- 38075b7 unit layer 1 (L2). Doubles: recorded probe double RECORDED (probe lines of 18:10 UTC), scripted Clock, run_backend and run_turn mocks; stub gate_seat.
- 26aaa0c unit layer 2 (L3). Doubles: none (plain datetimes).
- e81ed21 green. Full run: 356 tests in 1162 s, OK (skipped=1).

## Criterion 2 as first built (per-seat quota-first; superseded by D163/D164)
- 56b3686 unit layer 1 (L2, L5). Doubles: recorded probe double of SeatGateCase, scripted Clock, run_backend mock; LoopDeps fake for G7.
- 832ca6a unit layer 2 (L3). Doubles: none (plain dicts); stubs quota_first_problem and quota_reason.
- 36f09a0 green. Full run: 384 tests in 1194 s, OK (skipped=1); its first full run (19:06 UTC) failed only in TrueMobbing (a Claude member beside models with quota).

## Criterion 3 (the probe model is pinned)
- 6effcdb unit layer 1 (fixture, L1). Doubles: none new (the shared card-mode fixture pins probe_model).
- f24885e unit layer 2 (L3). Doubles: none (plain dicts shaped like backends.json); stub probe_model_problem.
- be9ec1b green. Full run: 402 tests in 1211 s, OK (skipped=1).

## D164 correction of criterion 2 (least-recently-used rotation; red d3125ea by the QA seat)
- 249eaa2 unit layer 1: the four existing tests rewritten to the new rule. Doubles: the SeatGateCase probe double and Clock; fake_cli.py env knob FAKE_FAIL_BACKEND.
- fcc396d unit layer 2 (L3): seat_use_line, last_seating, claude_wait_problem, rotation_reason with stubs. Doubles: plain dicts (probe results shaped like the recorded lines, history lists).
- 10d9db5 unit layer 3: corrected the card-mode refusal test (refusal falls on ctx-1 on a later turn, not ctx-2).
- fa88edb unit layer 4: test_quota_first disabled the shared tests.test_seat_gate.BACKENDS codex entry for the rest of the process; fixed by replacing the dict with a copy.
- 2a01531 green. Mutation runs of nine guards are in its body.

## Full-suite runs of the correction
- First run, started 21:53:10 UTC: 444 tests in 1259 s, FAILED (failures=12, errors=1, skipped=1), all in tests/test_seat_rotation.py. Cause: the shared BACKENDS dict pollution (codex left disabled by a test_quota_first test that runs earlier under discover), fixed in fa88edb.
- Second run, started 22:24:25 UTC: 444 tests in 1258 s, OK (skipped=1). The green 2a01531 was committed on this run.
