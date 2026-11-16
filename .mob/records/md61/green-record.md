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

## Fix round 2 green

Seat: green (dev, green), composer-2.5, declared by D273, context ctx-3e. Worktree wt-md61, branch card-61 at b64725f plus fix round 2 red tests. Nothing committed.

`git diff --stat`:

```text
 decider.py     | 29 ++++++++++++++++-------------
 facilitator.py |  7 +++++++
 mob_process.py |  3 ++-
 3 files changed, 25 insertions(+), 14 deletions(-)
```

What changed:

- `mob_process.py`, `last_seating`: pending launch/outcome pairing now keys on `(seat, card, context, role)` in the same single pass; legacy rows still count exactly as before.
- `facilitator.py`, `append_seat_outcome`: already matched the latest unpaired launch on those four fields (no edit required); `seat_log.py outcome` still routes through this path.
- `facilitator.py`, `check_seat`: a dry run returns `history is None`, so `gate_seat` writes no launch line.
- `facilitator.py`, `seat_turn`: when the backend is disabled or missing after `gate_seat` wrote its launch line, appends exactly one `refused` outcome for that launch through `append_seat_outcome` before returning; `run_backend` is never called.
- `decider.py`: a `SeatRefused` from taking the seat-use lock for the outcome phase is caught into `integrity.changes`, like per-outcome errors, so `decider-result.json` is always written.

Proof (all with `--capture=sys`; full suite not run):

- `python -m pytest tests/test_seat_use_model_outcome.py -q --capture=sys` — `131 passed`.
- `python -m pytest tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py tests/test_seat_gate.py tests/test_record_cycles_evidence.py tests/test_decider_launched.py -q --capture=sys` — `151 passed, 48 subtests passed`.
- `git diff | grep -nE "write_text|open\\(.*['\\\"]w"` — no match.

Changes are left uncommitted.

## Fix round 3 green (gate)

Seat: green (dev, green), composer-2.5, declared by D273, context ctx-3f. Worktree wt-md61, branch card-61. Nothing committed.

`git diff --stat`:

```text
 decider.py                           |  4 +--
 mob_process.py                       | 55 ++++++++++++++++++++++++++++++++----
 tests/test_card_mode.py              |  2 ++
 tests/test_decider_gate.py           |  9 +++++-
 tests/test_decider_record.py         |  9 ++++--
 tests/test_decider_record_units.py   | 18 ++++++++++--
 tests/test_decider_runner.py         |  9 ++++--
 tests/test_seat_use_model_outcome.py |  7 ++++-
 8 files changed, 95 insertions(+), 18 deletions(-)
```

| test | class | change | D justification |
|------|-------|--------|-----------------|
| SeatRotation::test_a_mixed_roster... | A | `history_seats()` skips `event=outcome` | |
| SeatRotation::test_a_second_claude_seat... | A | same | |
| SeatRotation::test_claude_takes_every_turn... | A | same | |
| Rotation::test_claude_voter_runs_on_its_own_turn... | A | `RotationCase.lines()` skips outcome rows | |
| Rotation::test_claude_voter_runs_when_every_model_that_has_waited_longer... | A | same | |
| Rotation::test_claude_voter_runs_when_the_other_available_models... | A | same | |
| Rotation::test_the_order_of_seats_does_not_change_who_is_judged_first | A | same | |
| SeatUseLock::test_a_dead_holders_lock_is_swept... | A | same | |
| Auditor::test_rotation_is_recomputed... | C | `last_seating_for_rotation`, `claude_wait_problem(..., count_open_launches=True)` on auditor recompute | |
| LoneRun::test_a_lone_runs_vote... (codex/claude) | C/D | `start_voters` does not merge `seat` into votes; `LoneRun.VOTE_FIELDS` adds `duration_s` and `structured_launch` | card-61 `vote_seat` records duration and structured-launch flag on every vote |
| ManyVoters::test_each_voter_has_its_own_model... | A | `lines()` filter | |
| NoVoter::test_the_first_line_of_an_empty_history... | A | same | |
| SameRunReason (4 tests) | C | `last_seating_for_rotation` in `rotation_reason` | |
| RunComposes (3 tests) | B | `open_run` stub: `card`, `backends`, `seat_use_lock`, `append_seat_outcome`; `vote_seat` returns duration fields | |
| StartVotersRecords::test_one_record_per_voter... | B/C | no extra `seat` key on `start_voters` records (`decider.py`) | |
| RunComposes::test_the_record_of_each_voter... | D | expected `start_voters` call list is voter dicts with `D169-<seat>` context | `decider.run` passes voter dicts, not bare seat names |
| LiveVoteOverlap::test_two_overlapping_runs... | A | history assertion uses launch rows only | |
| SixSeatsAtOnce::test_six_runs_started_at_once... | A | same | |
| Test61Fix1::test_61_f1_b4_the_item_reads... | D | read budget `4*n+2` | citing a found line reads its timestamp once per model in `rotation_reason` |
| Decider61 stack | A | `Decider61.lines()` reads full jsonl (outcomes included) | |

Proof (`--capture=sys`):

- `python -m pytest <the 22 tests> -q --capture=sys` — 22 passed, 2 subtests passed.
- `python -m pytest tests/test_seat_use_model_outcome.py … test_decider_runner.py -q --capture=sys` — 131 + 396 passed, no in-scope failure on gate/decider stack.

Changes are left uncommitted.

## Fix round 3b (FC-3)

Seat: green (dev, green), composer-2.5, declared by D273, context ctx-3f. Worktree wt-md61, branch card-61. Nothing committed.

Per ruling FC-3: removed `last_seating_for_rotation`. `last_seating(..., count_open=True)` treats every unpaired launch as a seating at its launch index (`last = max(last, indices[-1])`, no `last < 0` gate). `rotation_reason`’s `seated(name)` calls `last_seating(history, name, count_open=True)`. Auditor recompute still uses `count_open_launches=True` / `count_open=True` (unchanged).

Proof (`--capture=sys`):

- `python -m pytest` (22 gate tests from fix round 3 record) — `22 passed, 2 subtests passed in 69.59s`.
- `python -m pytest tests/test_seat_use_model_outcome.py tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py tests/test_decider_record.py tests/test_decider_record_units.py tests/test_decider_gate.py tests/test_card_mode.py tests/test_decider_runner.py -q --capture=sys` — `1 failed, 449 passed, 78 subtests passed in 1343.02s` (`Test61Fix2::test_61_f2_a_rotation_reason_cites_the_qa_launch_line_as_the_served_line`; see report).

Changes are left uncommitted.

## Fix round 3c (FC-3b)

Seat: green (dev, green), composer-2.5, declared by D273, context ctx-3f. Worktree wt-md61, branch card-61. Nothing committed.

Per ruling FC-3b: removed `count_open` / `count_open_launches`. `last_seating(history, backend, also=())` counts served launches and legacy rows, plus launch lines whose index is in `also`. `rotation_reason` and `claude_wait_problem` pass `also` through. `start_voters` builds `also` from `run.seat_use_index` values below each voter's line; `voter_record` passes it to `rotation_reason`. Auditor recompute uses `also=` the set of logged `line_index` values. `StartVotersRecords` mock accepts `also=()` (signature only).

Proof (`--capture=sys`):

- `python -m pytest` (22 gate tests from fix round 3 record) — `22 passed, 2 subtests passed in 84.64s`.
- `python -m pytest tests/test_seat_use_model_outcome.py tests/test_decider_record.py tests/test_decider_record_units.py tests/test_decider_gate.py tests/test_card_mode.py tests/test_decider_runner.py tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py -q --capture=sys` — `1 failed, 449 passed, 78 subtests passed in 1627.92s` (`ConcurrentStarts::test_each_runs_record_points_at_its_own_lines_and_claudes_reason_is_recomputed_from_the_file`; stored reason uses same-run `also` for D169 codex; assertion calls `rotation_reason(probes, before)` with `also=()`).

Changes are left uncommitted.
