SIGN-OFF
# mob-defaults #58 acceptance sign-off

Seat: acceptance, sign-off; service muse, model default, effort medium; context ctx-58s (fresh). Worktree wt-md58, branch card-58 at 51d6a13. No code or test changed by this seat.

## Scenario-to-test table

All tests are in `tests/test_rotation_usable_now.py` and drive production behaviour (`CardRun.check_seat` / `gate_seat` / `commit_seat`, `decider.check_voters` / `decider.start_voters`, `mob_process.claude_wait_problem` / `rotation_reason` / `not_usable_now`), not a reimplementation.

| Scenario | Proving test(s) | Production path |
|---|---|---|
| [Z][O] | `test_58_z_only_usable_models_waited_less_than_claude_and_claude_starts`; `test_58_o_the_one_other_model_is_not_pinnable_and_claude_starts`; char. `test_58_z_with_no_other_model_at_all_a_claude_seat_starts` | `check_seat` -> `claude_wait_problem(backends, gate_is_busy)` |
| [M][B] | `test_58_m_gate_blocked_quota_and_unpinnable_models_do_not_count_and_claude_starts`; `test_58_b_gate_idle_makes_qwen_usable_again_and_claude_is_refused_naming_qwen`; char. `test_58_b_a_model_without_gate_bound_is_counted_even_when_the_gate_is_busy` | `check_seat`; gate seam consulted, idle restores qwen |
| [I] | `test_58_i_a_refusal_names_the_usable_seat_and_why_each_other_model_was_skipped`; char. `test_58_i_a_usable_model_that_waited_longer_is_named_instead` | `SeatRefused` from `check_seat`; per R58-1 probe-down codex is asserted NOT named in the refusal (`assertNotIn codex`), gate-busy / no-pin skips are named |
| [E] | `test_58_e_no_other_model_usable_claude_starts_and_its_reason_says_so`; fix1 `test_b2_the_decider_record_reason_equals_the_stored_reason_in_the_e_setup` | `gate_seat`/`commit_seat` -> `quota_reasons`; Decider record equals stored reason |
| [S] | `test_58_s_the_agreement_template_counts_only_seats_that_can_be_seated_now` | reads real `templates/TEAM_WORKING_AGREEMENT.default.md` |

Supporting fix-round-1 tests (R58-2): `test_b1_the_real_cursor_is_not_pinnable_and_muse_and_qwen_are`, `test_b1_with_the_real_cursor_config_a_claude_seat_is_not_refused_for_cursor`, `test_b2_the_reason_names_a_gate_busy_skip_when_another_model_is_usable`, `test_m3_the_gate_is_asked_once_per_seat_and_decision_and_reason_agree`, `test_m3_a_decider_asks_the_gate_once_per_claude_voter`, `test_m4_a_raising_gate_check_counts_as_busy_with_the_reason_gate_state_unknown`, `test_m5_a_backend_without_config_is_not_enabled_for_the_reason_either`, `test_agy_m1_the_gate_seam_gets_the_runs_resolved_repo_path`, `test_58_gate_busy_seam_defaults_to_local_driver_gate_busy`, `test_58_m_decider_claude_voter_with_the_m_setup_is_not_refused`.

## Real config and Decider path coverage

- Real config (`core/backends.json`): covered for the pin rule. The two `test_b1_*` tests load the real file: cursor carries `pinnable: false` (and no other backend does; muse and qwen return usable from `not_usable_now`), and a Claude seat driven with the real cursor config is not refused for cursor. Corroborated by reading the file directly at this HEAD: cursor `pinnable: false`, qwen `gate_bound: true`, all enabled. Note: no test in this file asserts qwen's `gate_bound: true` from the real file; the [M][B] gate tests use a synthetic `gate_bound` config, so the real qwen flag rests on the direct file read, not on a real-file test assertion.
- Decider path: covered. `test_58_m_decider_claude_voter_with_the_m_setup_is_not_refused` drives `decider.check_voters`; `test_b2_*` and `test_m3_a_decider_asks_the_gate_once_per_claude_voter` drive `decider.start_voters` and assert the voter's `rotation_reason` equals `quota_reasons` and the gate is asked once per Claude voter.

## Run

`python -m pytest tests/test_rotation_usable_now.py tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py -q --capture=sys` -> `89 passed, 19 subtests passed in 36.07s`

## Named limits

- M6 (out of scope, R58-2): another repo's gate lock is not read; the gate check reads this run's resolved repo only, so a gate lock held in another repo is not seen by #58.
- Round 2 MINOR (accepted limit): `voter_record` itself takes no backends or gate answer; `decider.start_voters` rebuilds the Claude voter's reason afterwards with the decision's backends and gate answer, so production records via `start_voters` agree with `quota_reasons`, but a direct unit call to `voter_record` produces a reason without skip notes.
