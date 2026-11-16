# mob-defaults #61: green record

Seat: green (dev, green), gpt-5.3-codex, declared by D269, context ctx-3. Worktree `wt-md61`, branch `card-61`. Changes are uncommitted.

## git diff --stat

```text
decider.py     | 40 +++++++++++++++++++++++++--
facilitator.py | 86 ++++++++++++++++++++++++++++++++++++++++++++++++++--------
mob_process.py | 59 +++++++++++++++++++++++++++++++++++-----
3 files changed, 165 insertions(+), 20 deletions(-)
```

Untracked:
- `seat_log.py`

## What changed

- `mob_process.py`
  - Extended `seat_use_line(...)` with `service`, `declared_model`, `declared_reason`, `work_type`; when any is provided, it writes the launch shape and `outcome: launched`.
  - Added `seat_outcome_line(...)` with the six allowed outcomes only (`success`, `quota_exhausted`, `timeout`, `refused`, `no_vote`, `fallback`), raising `ValueError` otherwise.
  - Updated `last_seating(...)` to:
    - keep old rows counting exactly as before;
    - never count outcome rows;
    - count launch rows only when required by the new outcome rule (later matching `success`) for the new launch/outcome workflow.

- `facilitator.py`
  - Imported and used `seat_outcome_line`.
  - Extended `CardRun.append_seat_use(...)` to pass the new launch fields through to `seat_use_line(...)`, while preserving append-only/read-back behavior and pre-validating history readability before write.
  - Added `CardRun.append_seat_outcome(...)` with the same append-only/read-back refusal behavior as seat-use append.
  - Extended `CardRun.commit_seat(...)` and `CardRun.gate_seat(...)` to pass the launch metadata through.
  - Updated seat-turn gating path to write launch rows with explicit turn metadata.

- `decider.py`
  - `commit_voters(...)` now passes launch metadata (`service`, declared model pin or `undeclared`, `declared_reason="decider voter"`, `work_type="vote"`).
  - Added per-voter duration capture in `vote_seat(...)`.
  - Added voter outcome classification and outcome-row append flow (`success`, `no_vote`, `timeout`, `quota_exhausted`) with `duration_s`, `actual_model_by_wrapper_config`, and `model_reported_by_cli`.

- `seat_log.py` (new, repo root)
  - Added `launch` and `outcome` commands.
  - Enforced required arguments and bad-input exit code `2` (no writes on bad input).
  - Reused the same seat-use lock, append-only, and read-back refusal behavior through `CardRun` append methods.

## Command summaries

- `python -m pytest tests/test_seat_use_model_outcome.py -q --capture=sys`
  - `71 passed in 91.43s`.

- `python -m pytest tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py tests/test_seat_gate.py tests/test_record_cycles_evidence.py tests/test_decider_launched.py -q --capture=sys`
  - `151 passed, 48 subtests passed in 118.69s`.

## CLI-reported model

- No CLI-reported model string was printed by the required command outputs.
