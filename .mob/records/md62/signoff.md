SIGN-OFF
# mob-defaults #62: acceptance sign-off (muse, default, effort medium, ctx-62s, fresh)

Branch card-62 at 98ccece. No code or test changed by this seat.

## Scenario-to-test table

| Scenario | Proving tests | Real CLI covered | Verdict |
|---|---|---|---|
| Z | 3 tests, `test_62_z_*` | kickoff_check.main | Met |
| O | 10 tests, `test_62_o_*` | probe_all.main | Met |
| M | 8 tests, `test_62_m_*` | kickoff_check.main | Met |
| B | 10 tests, `test_62_b_*` | kickoff_check.main | Met |
| I | 16 tests, `test_62_i_*` | probe_all + kickoff_check | Met |
| E | 5 tests, `test_62_e_*` | probe_all.main | Met |
| S | 8 tests, `test_62_s_*` | seat_log idle-report | Met |

Fix round 1 adds 19 `test_62_f1_*` tests (B1, M1-M6 below); all pass.

## Tests by scenario (all in tests/test_kickoff_probe_pool.py)

- [Z] `test_62_z_no_probe_rows_is_a_refusal_naming_all_eight`;
  `test_62_z_no_probe_rows_exits_1_through_the_cli_and_prints_every_service`;
  `test_62_z_idle_services_with_no_history_lists_all_eight_with_no_probe_row`.
- [O] `test_62_o_probe_row_maps_an_ok_result_to_ok_with_empty_error`;
  `test_62_o_probe_row_maps_quota_reached_to_quota_with_the_evidence`;
  `test_62_o_probe_row_maps_every_other_state_to_error_with_the_evidence`;
  `test_62_o_probe_all_appends_one_probe_row_per_pool_service_to_the_seat_use_log`;
  `test_62_o_probe_all_row_results_follow_each_probe_result`;
  `test_62_o_a_launch_failure_is_an_error_row_not_a_crash`;
  `test_62_o_each_service_is_probed_with_its_configured_invocation` (muse medium effort);
  `test_62_o_the_append_is_made_under_the_seat_use_lock`;
  `test_62_o_existing_history_is_kept_and_rows_are_appended_after_it`;
  characterisation `test_62_o_probe_all_still_probes_every_enabled_driver_and_appends_the_probes_jsonl`;
  fix1 `test_62_f1_m1_a_disabled_pool_service_is_probed_and_gets_an_error_row`;
  `test_62_f1_m1_a_pool_service_absent_from_backends_gets_a_not_configured_error_row`;
  `test_62_f1_b1_probe_all_waits_for_a_seat_use_lock_held_by_another_writer_then_writes_both_files`;
  `test_62_f1_b1_the_append_does_not_use_a_zero_wait`;
  `test_62_f1_b1_the_probes_jsonl_is_written_even_when_the_seat_use_append_fails_and_the_exit_is_1`.
- [M] `test_62_m_an_ok_service_with_no_seat_and_no_reason_is_a_problem_naming_it`;
  `test_62_m_an_ok_service_with_a_no_fit_reason_passes`;
  `test_62_m_a_blank_no_fit_reason_is_not_a_reason`;
  `test_62_m_every_ok_service_seated_passes`;
  `test_62_m_many_unseated_ok_services_are_each_named`;
  `test_62_m_one_service_seated_in_two_roles_passes`;
  `test_62_m_services_that_probed_quota_or_error_need_no_seat_and_no_reason`;
  `test_62_m_the_cli_exits_0_on_a_compliant_declaration_and_1_on_an_unseated_ok_service`;
  fix1 `test_62_f1_m4_a_seat_with_an_empty_role_or_service_does_not_count_as_seated`;
  `test_62_f1_m4_card_flag_mismatch_with_the_declaration_is_a_problem`;
  `test_62_f1_m4_card_flag_matching_the_declaration_passes_and_no_card_flag_skips_the_check`.
- [B] `test_62_b_a_probe_row_60_minutes_old_is_a_refusal`;
  `test_62_b_a_probe_row_older_than_60_minutes_is_a_refusal`;
  `test_62_b_a_probe_row_59_minutes_old_is_fresh`;
  `test_62_b_59_minutes_59_seconds_is_fresh_and_60_minutes_exactly_is_not`;
  `test_62_b_a_service_missing_from_the_probe_rows_is_a_refusal_not_a_warning`;
  `test_62_b_a_no_fit_reason_does_not_excuse_a_missing_probe_row`;
  `test_62_b_a_seat_does_not_excuse_a_missing_probe_row`;
  `test_62_b_only_probe_events_count_as_probe_rows`;
  `test_62_b_the_latest_probe_row_of_a_service_is_the_one_that_counts`;
  `test_62_b_the_cli_refuses_with_exit_1_on_a_60_minute_row_and_passes_on_59`;
  characterisation `test_62_b_the_existing_probe_staleness_helper_is_unchanged`;
  fix1 `test_62_f1_m3_a_null_or_garbage_taken_at_is_not_a_fresh_row_and_does_not_crash`;
  `test_62_f1_m3_a_taken_at_more_than_5_minutes_in_the_future_is_not_fresh`;
  `test_62_f1_m3_the_cli_refuses_naming_the_service_on_a_garbage_taken_at_without_a_traceback`;
  `test_62_f1_m5_now_is_optional_and_defaults_to_the_real_utc_clock`;
  `test_62_f1_m5_without_now_a_two_hour_old_row_is_stale_by_the_real_clock`.
- [I] `test_62_i_a_live_one_line_heavy_lock_gives_a_reason_naming_the_file_and_owner_and_asks_winpid`;
  `test_62_i_a_live_one_per_line_heavy_lock_gives_a_reason_naming_the_file_and_owner_and_asks_winpid`;
  `test_62_i_a_dead_winpid_makes_either_real_shape_not_busy`;
  `test_62_i_liveness_is_checked_on_winpid_not_pid_when_both_are_present`;
  `test_62_i_without_winpid_liveness_is_checked_on_pid`;
  `test_62_i_a_bare_integer_lock_is_still_accepted`;
  `test_62_i_an_unparseable_heavy_lock_is_busy`;
  `test_62_i_a_lock_that_is_not_a_heavy_lock_is_ignored`;
  `test_62_i_an_empty_locks_dir_is_not_busy`;
  `test_62_i_a_busy_test_gate_gives_a_reason_even_with_no_heavy_lock`;
  `test_62_i_qwen_gate_busy_is_an_error_row_and_qwen_is_not_launched`;
  `test_62_i_qwen_is_probed_when_the_gate_seam_returns_none`;
  `test_62_i_the_default_gate_reason_asks_heavy_run_reason_for_the_repo_and_the_home_locks_dir`;
  `test_62_i_a_gate_busy_error_row_needs_nothing_more`;
  `test_62_i_a_no_fit_reason_citing_gate_busy_is_accepted_for_an_ok_row`;
  `test_62_i_the_cli_accepts_a_gate_busy_qwen_row_with_no_seat_and_no_reason`;
  characterisation `test_62_i_a_fresh_gate_lock_in_the_repo_makes_the_gate_busy`;
  `test_62_i_a_test_process_makes_the_gate_busy_and_no_process_or_lock_does_not`;
  `test_62_i_a_stale_gate_lock_is_not_busy`;
  fix1 `test_62_f1_m6_the_reason_names_only_the_owner_value_up_to_the_next_key_token`;
  `test_62_f1_m6_the_gate_busy_text_appears_once_in_the_qwen_row`.
- [E] `test_62_e_services_that_probed_quota_or_error_at_the_last_kickoff_are_probed_again_with_fresh_rows`;
  `test_62_e_every_pool_service_is_probed_again_whatever_its_earlier_rows_say`;
  `test_62_e_a_recovered_service_is_ok_in_the_next_check_and_needs_a_seat`;
  `test_62_e_an_old_quota_row_does_not_hide_a_fresh_ok_row`;
  characterisation `test_62_e_probe_all_does_not_skip_a_service_because_of_earlier_seat_use_rows`.
- [S] `test_62_s_a_service_with_a_success_outcome_in_the_last_24_hours_is_not_idle`;
  `test_62_s_a_success_outside_24_hours_leaves_the_service_idle`;
  `test_62_s_only_success_outcomes_count`;
  `test_62_s_an_outcome_joins_to_its_launch_rows_service`;
  `test_62_s_an_outcome_with_no_launch_row_uses_its_own_seat`;
  `test_62_s_the_reason_is_the_error_text_of_the_latest_probe_row`;
  `test_62_s_a_service_with_no_probe_row_says_so`;
  `test_62_s_idle_report_prints_each_idle_service_and_its_reason`;
  fix1 `test_62_f1_m2_an_outcome_joins_on_seat_card_context_and_role_not_the_latest_launch`;
  `test_62_f1_m3_an_outcome_with_a_null_or_garbage_ts_is_ignored_by_idle_services`;
  `test_62_f1_m3_the_idle_report_survives_a_garbage_outcome_ts`;
  `test_62_f1_m6_an_idle_service_whose_latest_probe_was_ok_reads_probe_ok_not_seated_in_24_h`.

## Real CLI coverage

Covered in production behaviour, in-process (not subprocess):

- `probe_all.main(["--repo", R, "--timeout", "1", "--jsonl", F])` via `run_probe_all`
  and direct calls: writes probes jsonl first, then one seat-use probe row per
  POOL service, under the seat-use lock.
- `kickoff_check.main(["--repo", R, "--declaration", F, "--now", ISO])` via
  `kickoff_main`, plus fix1 calls with optional `--now` and `--card`:
  exit 0 silent, exit 1 naming each problem service.
- `seat_log.main(["idle-report", "--repo", R])` via `idle_report_main`:
  prints each idle service and reason, exit 0.

Limits of that coverage: `facilitator.probe_backend`, `probe_all.gate_reason`,
`local_driver.pid_alive`/`heavy_run_reason`/`gate_busy` are faked or patched;
repos and locks dirs are temp dirs; no real service, network or real locks dir
is exercised. That matches the card's test-first slice.

## Run

`python -m pytest tests/test_kickoff_probe_pool.py tests/test_probe_all.py tests/test_seat_use_model_outcome.py -q --capture=sys`: `220 passed in 203.43s (0:03:23)`

## Named limits (review-2.md, accepted, none blocks a scenario)

- N1: the outcome-join fallback to a lone launch whatever its seat. When no
  launch matches `(seat, card, context, role)` and exactly one launch shares
  `(card, context, role)`, a success is credited to that launch's service even
  if the outcome's seat differs (hand-written rows only; real seat_log writes
  the same seat on launch and outcome). Suggested narrowing, not done: use the
  fallback only when the outcome's own seat is not a POOL service.
- N2: `--card` compared strictly on type. A declaration `"card": 62` (int)
  against `--card 62` (string) is a false refusal
  (`declaration card 62 does not match --card '62'`); declarations in use are strings.
- N3: cosmetic texts. Unused `re` import in local_driver.py; a future-dated or
  garbage `taken_at` is reported as "no probe row in the last 60 minutes" though
  a row exists; the B1 stderr text says "probe_all cannot start" although the
  probes already ran (shared lock-wait text).

review-agy.md was not treated as independent evidence (review.md was in its folder).
