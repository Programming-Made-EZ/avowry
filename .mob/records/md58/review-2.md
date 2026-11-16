# #58 review round 2 (fix round 1, card-58 51d6a13)

Seat: QA, review round 2; agy gemini-3.8-flash-medium; context ctx-58x2; read-only clone. Claude held its two #58 seats (R-ROT); qwen was gate-blocked by the play-tracker train.

VERDICT: SIGN-OFF

All in-scope findings from R58-2 are resolved in production behaviour:
- **B1**: `cursor` has `"pinnable": false` in `core/backends.json`, preventing Claude seat refusal when cursor has waited longer.
- **B2**: `rotation_reason` records skipped models (`held_back`) both when other models are usable and when none are usable.
- **M3**: The gate check is evaluated once in `check_seat` for Claude, attached to the `Checked` tuple, and reused by `commit_seat` and `start_voters`.
- **M4**: Exceptions in `gate_busy` are caught and mapped to `GATE_UNKNOWN` ("gate state unknown"), preventing unhandled crashes.
- **M5**: `rotation_reason` defaults backend `enabled` to `False`, matching `claude_wait_problem`.
- **Resolved repo path**: `self.repo` is passed to `gate_busy` instead of unresolved `self.args.repo`.

The 9 new tests in `Test58Fix1` fail on plausible wrong implementations (real config pinnability, single gate invocation, exception handling, unconfigured backends, and path resolution). No existing test was modified or loosened.

Regarding `decider.start_voters` rebuilding `rotation_reason`:
- **Consistency**: The record written to the log and returned by `start_voters` is consistent with `quota_reasons` and the check decision.
- **Gate answer**: The cached `gate_is_busy` attribute on `Checked` cannot be missing (`getattr` fallback to `False`) and cannot be stale (sampled during `check_seat` under `seat_use_lock` and reused across commit and record).

MINOR: decider.py:382: `decider.voter_record(probes, "claude", 0, [])`: `voter_record` does not take `backends` or `gate_is_busy`; `start_voters` rebuilds the reason after return. Direct unit calls to `voter_record` produce reasons without skip notes, though production runs via `start_voters` emit consistent records.

## Facilitator note

The MINOR (voter_record itself takes no backends or gate answer; start_voters rebuilds the Claude reason) is accepted as a named limit of #58, recorded on the card as a follow-up.
