# mob-defaults #58 rulings

## R58-1 (facilitator slice ruling, 2026-11-16T09:05Z): the refusal's skip list covers the usable-now kinds only

- **Conflict.** The red's [I] test requires a Claude refusal to name a probe-down model (codex, `probe not ok`, `quota-reached`). The D164 tests `test_seat_rotation.py::test_exactly_one_older_available_model_refuses_claude_and_the_refusal_names_it_with_the_seat` and `test_seat_rotation_rules.py::test_a_never_seated_model_has_waited_longer_than_a_seated_claude` require probe-down models NOT to be named in a refusal. A green could meet both only by an old-format special case (naming a probe-down model only when its probe double carries `state`), which is forbidden.
- **Cause.** The facilitator's slice in the red brief asked for every not-counted model in the refusal. That over-reached the card: probe-down models were never counted, and their evidence is already cited in the Claude seat's reason (`rotation_reason`).
- **Ruling.** A refusal names the model to seat instead and, for each enabled non-Claude model skipped by the NEW usable-now checks, its name and why (`gate busy`, `no model pin`). Probe-down models stay out of the refusal, as D164 has them. The [E] reason (the seat's `quota_reasons` entry) names every skipped model with its reason, the probe-down kind included (`probe not ok`, its state).
- **Action.** The [I] test is amended by its author seat (Muse, ctx-58r) to assert codex is not named in the refusal; nothing else changes.

## R58-2 (facilitator, 2026-11-16, about 10:45Z): review round 1 findings to fix round 1

- Both reviews (Opus ctx-58q: CHANGES, 2 BLOCKING + 4 MINOR; agy ctx-58x: BLOCKING 2 + 1 MINOR) agree on the two BLOCKING findings.
- **B1 (pin).** The real cursor backend (`cursor_reply.py`, no `{MODEL}` in its invocations) cannot take a model, so it carries `"pinnable": false` in `core/backends.json`. muse and qwen run one fixed model each and are pinned by construction; they stay pinnable. A test reads the real `core/backends.json`.
- **B2 (one explanation).** `decider.voter_record` builds its reason with the same backends and the same gate answer as the decision, so a Decider record's reason equals the reason `commit_seat` stores for that seat. The reason names the gate-busy and no-pin skips whenever there are any; "no other model can be seated now" stays for the case with no usable model.
- **M3, M4, M5, agy-M1 are in scope:** the gate is asked once per seat and that answer is reused by the commit and the record; a raising gate check counts as busy with the reason `gate state unknown`; the reason and the decision treat a backend without config the same (not enabled); the gate check gets the run's resolved repo path.
- **M6 (another repo's gate lock) is out of scope:** filed as a follow-up and named as a limit of #58.
- Card-58 was rebased onto card-61 4cf462e (#61 fix round 3); the facilitator resolved the `mob_process.py` conflict by giving both cards' arguments (`also=` and `backends=`, `gate_is_busy=`). 104 rotation and record tests pass after it.
