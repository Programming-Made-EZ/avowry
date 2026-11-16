## Green

Seat: dev, green; claude-sonnet-5-5; context ctx-58g (fresh); worktree `$HOME/projects/wt-md58`, branch card-58, base bb28fce. Nothing committed, pushed or stashed.

```
 core/backends.json                          |  1 +
 facilitator.py                              | 15 ++++++--
 mob_process.py                              | 53 ++++++++++++++++++++++++-----
 templates/TEAM_WORKING_AGREEMENT.default.md |  2 +-
 4 files changed, 60 insertions(+), 11 deletions(-)
```

What changed:
- `mob_process.py`: new `not_usable_now(cfg, gate_is_busy)` ("gate busy" for gate_bound with a busy gate; "no model pin" for `pinnable: false`). `claude_wait_problem` and `rotation_reason` take `backends=None, gate_is_busy=False` (existing callers unchanged). Skipped models do not count as "waited longer"; a refusal names the seat-instead model and each probe-ok model skipped as gate busy / no model pin (probe-down models stay out, R58-1). `rotation_reason`, when `backends` is given and no other model is usable now, appends `no other model can be seated now; skipped: <name (why)>` including `probe not ok, <state>` (state read with `.get`).
- `facilitator.py`: `CardRun.gate_busy` attribute, default `local_driver.gate_busy`; `gate_is_busy_now()` calls it at most once per check and only when some backend is gate_bound; `check_seat` and `commit_seat` pass backends and gate state.
- `core/backends.json`: `"gate_bound": true` on qwen. No `pinnable: false` anywhere.
- Template: rotation rule says it "counts only seats that can be seated now (probed available, not gate-blocked, pinnable)".

Runs (`--capture=sys`):
1. `tests/test_rotation_usable_now.py`: `12 passed in 0.42s`
2. The eleven-file regression set: `18 failed, 227 passed, 47 subtests passed in 200.92s (0:03:20)`. The same 18 failures (decider_gate Rotation x4 and SeatUseLock x1, decider_record, decider_record_units) occur on an unmodified `git archive` export of bb28fce (`18 failed, 227 passed, 47 subtests passed`); the lists are identical, so none are caused by this change. (A first attempt appended the skip list even when other models were usable and broke `test_decider_rotation.py::CommitVoters`; fixed by appending only when no other model is usable now.)

## Fix round 1 (R58-2)

Seat: dev, green; claude-sonnet-5-5; ctx-58g. Worktree `$HOME/projects/wt-md58`, HEAD 959f985 (rebased onto #61). Nothing committed, pushed or stashed.

New tests, class `Test58Fix1` in `tests/test_rotation_usable_now.py` (all 9 seen failing before the production change):
- `test_b1_the_real_cursor_is_not_pinnable_and_muse_and_qwen_are` failed: the real cursor had no `pinnable: false`.
- `test_b1_with_the_real_cursor_config_a_claude_seat_is_not_refused_for_cursor` failed: refused naming cursor.
- `test_b2_the_decider_record_reason_equals_the_stored_reason_in_the_e_setup` failed: the record lacked the skip note.
- `test_b2_the_reason_names_a_gate_busy_skip_when_another_model_is_usable` failed: no `qwen (gate busy)` in the reason.
- `test_m3_the_gate_is_asked_once_per_seat_and_decision_and_reason_agree` failed: two gate calls, reason disagreed.
- `test_m3_a_decider_asks_the_gate_once_per_claude_voter` failed: two gate calls.
- `test_m4_a_raising_gate_check_counts_as_busy_with_the_reason_gate_state_unknown` failed: the OSError escaped.
- `test_m5_a_backend_without_config_is_not_enabled_for_the_reason_either` failed: the no-other-model note was suppressed.
- `test_agy_m1_the_gate_seam_gets_the_runs_resolved_repo_path` failed: the seam got the unresolved `args.repo`.

```
 core/backends.json                |  3 +-
 decider.py                        |  7 +++-
 facilitator.py                    | 27 ++++++++++---
 mob_process.py                    | 17 +++++---
 tests/test_rotation_usable_now.py | 82 +++++++++++++++++++++++++++++++++++++++
 5 files changed, 123 insertions(+), 13 deletions(-)
```
(the stat covers the working tree against HEAD 959f985)

Changes:
- `core/backends.json`: `"pinnable": false` on cursor only, with a sentence in its notes saying why.
- `mob_process.py`: `GATE_UNKNOWN`; `not_usable_now` gives `gate state unknown` for it; `rotation_reason` treats a backend without a config entry as not enabled (as the decision does) and names gate-busy / gate-unknown / no-pin skips whenever any exist (probe-down models are listed only in the no-usable-model case, so the text is unchanged when there are no skips).
- `facilitator.py`: `check_seat` asks the gate once for a Claude seat and returns a `Checked` tuple (still unpackable as four items) carrying the answer; `commit_seat` reuses it and never calls the seam again; `gate_is_busy_now` passes `self.repo` (resolved) and turns an exception into `GATE_UNKNOWN` (counted as busy).
- `decider.py`: `start_voters` rebuilds a Claude voter's record reason with the decision's backends and the carried gate answer, so it equals `quota_reasons[context]`. It overrides only when the record already holds a `rotation_reason`, because the existing `StartVotersRecords` tests use a `voter_record` double with the old signature, which I may not edit.

Runs (`--capture=sys`):
1. `tests/test_rotation_usable_now.py`: `21 passed in 1.23s`
2. The twelve-file set (with `test_seat_use_model_outcome.py`): `374 passed, 49 subtests passed in 515.48s (0:08:35)`. No failures, so #61's fix cleared the earlier 18. A first attempt that passed new kwargs to `voter_record` failed 4 `StartVotersRecords` tests (the double's signature); fixed as above.
