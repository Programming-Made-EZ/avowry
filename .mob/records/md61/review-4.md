# #61 review round 4 (fix round 3, card-61 4cf462e)

Seat: QA, review round 4; agy gemini-3.8-flash-medium (argon-high out, 429); context ctx-r4; read-only clone. Claude already held two #61 seats (R-ROT); qwen was gate-blocked by the play-tracker train.

## agy's reply (verbatim from its output, the VERDICT line and findings)

VERDICT: SIGN-OFF

### Test Edits (G-1, FC-3, FC-3c)
All test edits are of permitted kinds and preserve full assertion strength without hiding regressions:
- **Selectors (Class A):** `tests/test_card_mode.py`, `tests/test_decider_gate.py`, and `tests/test_decider_runner.py` filter out lifecycle rows (`event == "outcome"`) from launch history. `Decider61.lines()` in `tests/test_seat_use_model_outcome.py` cleanly overrides `RotationCase.lines()` to keep outcome lines for outcome-verification tests.
- **Fakes (Class B):** `tests/test_decider_record_units.py` equips `open_run_stub` and `vote_seat` doubles with required card, lock, outcome, and duration stubs; `StartVotersRecords` stub accepts `also=()`.
- **Expected values (Class D):** `LoneRun.VOTE_FIELDS` adds `duration_s` and `structured_launch` introduced by card 61. `start_voters` asserts voter specification dictionaries. The `4*n + 2` read limit in `test_seat_use_model_outcome.py` accounts for the two timestamp lookups in `rotation_reason`, authorized under FC-3 Class D.

### The `also=` Rule & Consistency (FC-3b, FC-3c)
- In `mob_process.py`, `last_seating(history, backend, also=())` correctly treats old-format rows, served launches (`outcome == "success"`), and any launch index present in `also` as seatings.
- Consistency: `claude_wait_problem` and `rotation_reason` propagate `also` to each `last_seating` invocation. In `decider.py`, `start_voters` derives `also` from `run.seat_use_index` for prior lines (`i < index`) and passes it via `voter_record` to `rotation_reason`.
- Recomputes: `Auditor` (`tests/test_decider_record.py`) and `ConcurrentStarts` (`tests/test_decider_rotation.py`) consistently pass `also` containing the run's logged voter line indices.

### Boundary Integrity of `also`
- **Cannot include foreign lines:** `run.seat_use_index` is an instance dictionary on `CardRun` populated exclusively under lock during `commit_seat` for this run's turns. Recomputes draw strictly from the run's own result records.
- **Cannot miss companion lines:** `judged_voters` strictly orders non-Claude voters ahead of Claude. Hence, all companion voters in the run are committed before Claude and satisfy `i < index`.

### Diff Inspection
- No special cases keyed on row shape.
- `decider.py` safely avoids leaking internal keys (`model`, `seat`) into vote results.
- Commit `4cf462e` is clean, conforms to rulings G-1, FC-3, FC-3b, and FC-3c, and introduces no regressions.
VERDICT: SIGN-OFF

### Test Suite Execution
Execution of all 412 tests across the modified and dependent test suites (`tests/test_card_mode.py`, `tests/test_decider_gate.py`, `tests/test_decider_record.py`, `tests/test_decider_record_units.py`, `tests/test_decider_rotation.py`, `tests/test_decider_runner.py`, and `tests/test_seat_use_model_outcome.py`) passed cleanly (`OK`, 412 passed in 1286.78s).

### Test Edits (G-1, FC-3, FC-3c)
All test edits are of permitted kinds and preserve full assertion strength without hiding regressions:
- **Selectors (Class A):** `tests/test_card_mode.py`, `tests/test_decider_gate.py`, and `tests/test_decider_runner.py` filter out lifecycle rows (`event == "outcome"`) from launch history. `Decider61.lines()` in `tests/test_seat_use_model_outcome.py` overrides `RotationCase.lines()` to keep outcome lines for outcome-verification tests.
- **Fakes (Class B):** `tests/test_decider_record_units.py` equips `open_run_stub` and `vote_seat` doubles with required card, lock, outcome, and duration stubs; `StartVotersRecords` stub accepts `also=()`.
- **Expected values (Class D):** `LoneRun.VOTE_FIELDS` adds `duration_s` and `structured_launch` introduced by card 61. `start_voters` asserts voter specification dictionaries. The `4*n + 2` read limit in `test_seat_use_model_outcome.py` accounts for the two timestamp lookups in `rotation_reason`, authorized under FC-3 Class D.

### The `also=` Rule & Consistency (FC-3b, FC-3c)
- In `mob_process.py`, `last_seating(history, backend, also=())` correctly treats old-format rows, served launches (`outcome == "success"`), and any launch index present in `also` as seatings.
- Consistency: `claude_wait_problem` and `rotation_reason` propagate `also` to each `last_seating` invocation. In `decider.py`, `start_voters` derives `also` from `run.seat_use_index` for prior lines (`i < index`) and passes it via `voter_record` to `rotation_reason`.
- Recomputes: `Auditor` (`tests/test_decider_record.py`) and `ConcurrentStarts` (`tests/test_decider_rotation.py`) consistently pass `also` containing the run's logged voter line indices.

### Boundary Integrity of `also`
- **Cannot include foreign lines:** `run.seat_use_index` is an instance dictionary on `CardRun` populated exclusively under lock during `commit_seat` for this run's turns. Recomputes draw strictly from the run's own result records.
- **Cannot miss companion lines:** `judged_voters` strictly orders non-Claude voters ahead of Claude. Hence, all companion voters in the run are committed before Claude and satisfy `i < index`.

### Diff Inspection
- No special cases keyed on row shape.
- `decider.py` safely avoids leaking internal keys (`model`, `seat`) into vote results.
- Commit `4cf462e` is clean, conforms to rulings G-1, FC-3, FC-3b, and FC-3c, and introduces no regressions.
