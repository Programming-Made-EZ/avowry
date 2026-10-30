# Card 209 sign-off (ctx-5, product manager signoff)

Claude seat, fresh context; probes at 15:33 UTC (.mob/records/probes/2026-10-30.jsonl) show Codex, Copilot, Grok and agy without usable quota.
Code: mob-defaults (C:\Users\ringe\mob-crosscli) at e239b12, clean except untracked .mob/. Nothing pushed by me; no gh calls.

## Verdict: SIGN OFF

## Reasons

- **MD-001 and the ruling (no removals mid-card; removals only at the retro):** met. One rule, `mob_process.removal_problem`, is used by both facilitator.py (`check_task_force`) and the record checker (`check_record`). It accepts only "retro" (case and spaces ignored) and refuses cycle, blank and unknown-role removals, naming the member and "removals happen only at the retro". The card removes nothing else; no seat, feature or test outside the card's scope was removed (the deleted mid-card test at old line 1069 was named by the card). Card 218 stays out of scope.
- **Done checks:** (1) facilitator refusal: red first, ZOMBIES named tests per the review; (2) mob_process refusal with the same message, `TheTwoEntryPointsAgree`; (3) template cleaned, plus TASK_FORCE.md via F2. Grep `removed: cycle` in templates and *.py: no hits; in tests/*.py exactly 5 hits, all refusal cases, as the card allows. I re-ran at e239b12: test_template_removal, test_mob_process and the card-mode removal classes, 94 tests OK. Full suite: 338 OK (skipped=1) per the record at e239b12, not re-run by me; nothing changed since.
- **D157:** git order has criterion 1 greens before criterion 2 reds, and 2 before 3; every red body carries "Criterion: n"; no criteria mixed (confirmed by review-ctx4.md and preship-gate.md section 4).
- **Review:** ctx-4 re-check is APPROVE; F1 and F2 verified fixed by mutation. **Gate:** ctx-7 PASS on hashes on origin, suite evidence, no CI workflow, Criterion order, grep.

## Deviations, stated plainly

1. **44112b4 / 37b1273 (D136 line-ending churn):** a sed slip turned CRLF to LF in tests/test_mob_process.py (1181 lines changed), then 37b1273 restored it (1304 lines). Both break the 400-line limit. Net diff is +121 lines in one file. Not rewritten, disclosed. Does not block: the limit exists for readability, the net is clean, and the process error is recorded.
2. **Developer amended an unpushed commit (b21d94e, amended away 29 s later):** never on origin, so no published history changed. Does not block, but it is a recorded departure from "no rewrites"; the retro should consider it.
3. **Review-fix commits without Criterion lines (73026aa, 7fdb168, e239b12; also 37b1273):** they follow all three criteria being green and fix named findings F1 and F2, so there is no slice criterion to name. A red-shaped commit (7fdb168) without a line is a real gap against C1 as written. Does not block, because order is intact and the findings are named; the retro should decide whether review fixes get a line such as "Review fix: F2".

None blocks. I record that I weigh deviation 3 as the most substantive.

## Persona demo (D150): a facilitator using mob-defaults

1. The facilitator writes `removed: cycle 3 | decider: D5` for the backend developer, even with a passed Decider: the card-mode run exits non-zero and says `role 'Backend developer' removal 'cycle 3' refused: removals happen only at the retro (write 'removed: retro')`.
2. A blank `removed:` is refused the same way, not ignored; the roster stays unchanged.
3. `mob_process` on the record gives the identical message, so the two entry points cannot disagree.
4. `removed: retro` (any capitals) is accepted. Both templates now only teach the retro form.
(I ran the rule directly: removal_problem for "cycle 3" and blank return the refusal above, "retro" returns None. I did not run a full card-mode session.)
