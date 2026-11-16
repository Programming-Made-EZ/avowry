# mob-defaults #58 red record (ctx-58r)

Tests: `tests/test_rotation_usable_now.py` in worktree wt-md58 (branch card-58). No production code changed.

## Tests by scenario

- [Z][O] `test_58_z_only_usable_models_waited_less_than_claude_and_claude_starts` (Pending); `test_58_o_the_one_other_model_is_not_pinnable_and_claude_starts` (Pending); `test_58_z_with_no_other_model_at_all_a_claude_seat_starts` (Characterisation).
- [M][B] `test_58_m_gate_blocked_quota_and_unpinnable_models_do_not_count_and_claude_starts` (Pending); `test_58_b_gate_idle_makes_qwen_usable_again_and_claude_is_refused_naming_qwen` (Pending); `test_58_m_decider_claude_voter_with_the_m_setup_is_not_refused` (Pending, Decider); `test_58_b_a_model_without_gate_bound_is_counted_even_when_the_gate_is_busy` (Characterisation).
- [I] `test_58_i_a_refusal_names_the_usable_seat_and_why_each_other_model_was_skipped` (Pending); `test_58_i_a_usable_model_that_waited_longer_is_named_instead` (Characterisation).
- [E] `test_58_e_no_other_model_usable_claude_starts_and_its_reason_says_so` (Pending).
- [S] `test_58_s_the_agreement_template_counts_only_seats_that_can_be_seated_now` (Pending).
- Seam: `test_58_gate_busy_seam_defaults_to_local_driver_gate_busy` (Pending).

## Counts

- Pending: 9 tests, all FAIL on today's code.
- Characterisation: 3 tests, all pass on today's code.

## Pending failure reasons (one line each)

- `test_58_z_only_usable_models_waited_less_than_claude_and_claude_starts`: SeatRefused, qwen (gate-busy, never seated) counted as waited longer.
- `test_58_o_the_one_other_model_is_not_pinnable_and_claude_starts`: SeatRefused, cursor (pinnable: false, never seated) counted as waited longer.
- `test_58_m_gate_blocked_quota_and_unpinnable_models_do_not_count_and_claude_starts`: SeatRefused naming cursor, qwen; none of the three skipped kinds is excluded.
- `test_58_b_gate_idle_makes_qwen_usable_again_and_claude_is_refused_naming_qwen`: AssertionError, refusal says "Seat it on cursor instead.", not "Seat it on qwen instead.".
- `test_58_i_a_refusal_names_the_usable_seat_and_why_each_other_model_was_skipped`: AssertionError, "gate busy" not in refusal; no skipped-model reasons are given.
- `test_58_e_no_other_model_usable_claude_starts_and_its_reason_says_so`: SeatRefused naming cursor, qwen at gate_seat, so no quota_reason is recorded.
- `test_58_m_decider_claude_voter_with_the_m_setup_is_not_refused`: SeatRefused from decider.check_voters naming cursor, qwen.
- `test_58_s_the_agreement_template_counts_only_seats_that_can_be_seated_now`: AssertionError, phrase "counts only seats that can be seated now" not in the template.
- `test_58_gate_busy_seam_defaults_to_local_driver_gate_busy`: AssertionError, CardRun has no gate_busy attribute (missing seam).

## Runs

- `python -m pytest tests/test_rotation_usable_now.py -q --capture=sys`: `9 failed, 3 passed in 1.55s`
- `python -m pytest tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py tests/test_seat_gate.py -q --capture=sys`: `81 passed, 19 subtests passed in 38.43s`

## Ambiguities noted for the green

- [I] read literally (name every not-counted model, including probe-not-ok) sits against the existing `test_seat_rotation.py` refusal test asserting probe-down models (copilot/grok/agy) are NOT named; R58-1 settles the scope: a refusal names only the usable-now skips, and probe-down models stay out (see Red fix below).
- The probe "state" (e.g. quota-reached) is supplied by the probe double as result["state"], matching facilitator.probe_backend's real result shape; recorded doubles elsewhere omit it.

## Red fix (R58-1)

- In `test_58_i_a_refusal_names_the_usable_seat_and_why_each_other_model_was_skipped` only: kept the setup, the copilot assertion and the qwen/`gate busy` and cursor/`no model pin` assertions; replaced the codex/`probe not ok` and `quota-reached` assertions with `self.assertNotIn("codex", message)`; fixed the test's comment to match R58-1. No other test changed.
- Run: `python -m pytest tests/test_rotation_usable_now.py -q --capture=sys`: `9 failed, 3 passed in 1.32s` ([I] still fails on "gate busy").
