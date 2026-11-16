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

## Fix round 1 green (muse, after cursor's usage limit)

Starting point: the WIP commit at HEAD (the previous green seat's partial change, stopped at its usage limit), left uncommitted on top of it.

`git diff --stat`:

```
 facilitator.py                       | 6 +++++-
 tests/test_decider_launched.py       | 3 +++
 tests/test_seat_use_model_outcome.py | 4 +---
 3 files changed, 9 insertions(+), 4 deletions(-)
```

What changed:

- `facilitator.py`, `CardRun.commit_seat`: a task-force seat turn (a `Seat` with a context, `work_type == "turn"`) again writes the historical one-line seating record (no `outcome` and no `event` key), so it counts as a seating exactly as before under scenario [Z]. A turn by a seat named as a plain string keeps the new-format launch line and writes its success (or failure) outcome when the turn ends in `CardRun.seat_turn`, so that launch counts only on success. No test in `tests/test_seat_rotation.py` was edited.
- `tests/test_decider_launched.py`: only how lines are selected changed. `LaunchedCase.lines()` now returns launch lines only, meaning lines without `"event": "outcome"`, so `seats_in_history()` and the one direct `lines()` assertion count one line per voter as before. No expected count, index or other assertion was changed.
- `tests/test_seat_use_model_outcome.py` (ruling FX1-1, the only change in this file): `Test61PendingSeatLog.test_61_m_each_of_the_six_outcomes_is_accepted` now keeps the launch-then-outcome fixture (a matching launch written before each outcome) and no longer rewrites the history to hold only outcome lines; the final assertion selects the outcome lines and still expects the six outcomes in order.

Proof (both run with `--capture=sys`, after the final edit):

- `python -m pytest tests/test_seat_use_model_outcome.py -q --capture=sys` — `112 passed`.
- `python -m pytest tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py tests/test_seat_gate.py tests/test_record_cycles_evidence.py tests/test_decider_launched.py -q --capture=sys` — `151 passed, 48 subtests passed`.

The full suite was not run, as instructed. Changes are left uncommitted.

## Fix round 1 green, second pass

Cause found: in `CardRun.seat_turn` (facilitator.py), the outcome block at turn end was guarded by `if turn_context is None:`. `turn_context` is `seat.context` for a task-force `Seat` and `None` only for a plain-string seat, so every task-force seat turn skipped `append_seat_outcome` entirely; its new-format launch (once the `commit_seat` special case was removed) never got a success outcome and so never counted as a seating under `last_seating`. That is why the four whole-turn tests in `tests/test_seat_rotation.py` failed on rotation/reason once the special case was removed. A second gap on the same path: an exception from `run_backend` propagated out of `seat_turn` before any outcome could be written.

Change (the two kept test-file changes from the first pass are untouched, and `tests/test_seat_rotation.py` was not edited):

- `CardRun.commit_seat`: removed the task-force special case. Every seat turn now writes the new-format launch line (`service`, `declared_model`, `declared_reason`, `work_type`, `outcome: launched`); `grep -n "historical one-line" facilitator.py` has no match.
- `CardRun.seat_turn`: the outcome block now runs for every launched turn (`if probe is not None:` instead of `if turn_context is None:`), so a finished task-force turn writes `success` through the existing `append_seat_outcome`, and a failed backend call writes the matching failure (`quota_exhausted` / `timeout` / `no_vote`, mapped from the reply text as before). The backend call is wrapped in a small `call_backend` helper that converts a raised exception into that same failed-turn shape (`False, None, "Type: message"`), so a raising backend also gets its matching failure outcome at turn end and the turn returns `(False, reply)`.
- Why the pair double-counted, and the fix for it: `tests/test_seat_rotation.py`'s `used()`/`len(history())` count every history line, so a launch line plus a separate outcome line counted a task-force turn twice and broke the file's one-line-per-seat assertions. `CardRun.fold_turn_outcome` (called from `seat_turn` under the same seat-use lock, only for a `Seat` turn with a context) folds the outcome just appended by `append_seat_outcome` into that turn's launch line — the launch keeps its seat, role, context, card, time and probe fields and now carries the final outcome (plus duration and the two model fields) in place of `launched` — and `seat_use_seen` becomes the compacted file so this run's append-only check stays in step. Plain-string seat turns keep the separate launch and outcome lines exactly as before.
- `mob_process.last_seating`: a launch line carrying its final outcome in place (no `event`) counts as a seating only when that outcome is `success`, matching the pair rule; final non-success outcomes count for nobody.

`git diff --stat` (against the starting commit 07c318c, i.e. including the two kept test-file changes from the first pass):

```text
 facilitator.py                       | 48 +++++++++++++++++++++++++++++++++---
 mob_process.py                       |  7 ++++++
 tests/test_decider_launched.py       |  3 +++
 tests/test_seat_use_model_outcome.py |  4 +--
 4 files changed, 55 insertions(+), 7 deletions(-)
```

Proof (all with `--capture=sys`, after the final edit; the full suite was not run):

- `python -m pytest tests/test_seat_use_model_outcome.py -q --capture=sys` — `112 passed`.
- `python -m pytest tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py tests/test_seat_gate.py tests/test_record_cycles_evidence.py tests/test_decider_launched.py -q --capture=sys` — `151 passed, 48 subtests passed`.
- `grep -n "historical one-line" facilitator.py` — no match.
- A throwaway probe (kept outside the repo) drove `Seat` turns with a succeeding backend, a raising backend, and a failing backend: the history held one folded line per turn with outcomes `['success', 'timeout', 'quota_exhausted']`, and the raising turn returned `(False, "...timed out...")`.

Changes are left uncommitted.
