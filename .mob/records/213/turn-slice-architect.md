# Card 213 slice (architect, ctx-6)

Decision: SHIP AS IS under D136. One concern (a window-file check in scripts/mob_process.py), 3 criteria, 3 increments (red tests and fixtures; green check_windows; wiring into run_check plus own-tree check), about 4 to 6 files (scripts/mob_process.py, tests/test_mob_process.py, board card, one record/note) and about 150 to 250 lines in the biggest commit, inside 15 files and 400 lines. No split cards.

## Criterion-by-layer table
| Done check | Layer that makes it pass | Status today |
|---|---|---|
| 1 category and end time | `check_windows(root)`: glob `.mob/records/decisions/[0-9][0-9][0-9]-*.json`, parse (own message on bad JSON), exact-match `category` against the closed list (task-force declaration, slice decision, loop-boundary re-rank, card-text or declaration fix, mid-card addition), non-empty `ended_at` | red-able (bad-category, bad-capitals, empty/missing ended_at, invalid JSON fixtures). Born-green, guard only: Z (no directory / no files passes), O (one complete window passes) |
| 2 seat fields | per-reply seat check inside `check_windows`: `role`, `model`, `context`, `probe_reference` non-empty strings; `reply` in {NO OBJECTION, OBJECTION}; `condition` null for NO OBJECTION, one non-empty string for OBJECTION (a list or two conditions fails); missing `reply` key has its own message; empty `replies` fails | red-able (missing-field fixtures) |
| 3 wiring and red first | `check_windows` added to the tuple in `run_check`; red commit precedes green commit | red-able (bad window fixture through `run_check` exits 1, today exits 0). Born-green, guard only: own tree exits zero, last commit green, D-number in subject (already enforced by D145 C2) |

Merge: none; the born-green items stay as named guard tests inside criteria 1 and 3.

## Outside-in plan for the first red-able criterion (1)
- Acceptance test consumes `mob.run_check(root, issues, git)` (existing `MobProcessTests` setUp tree, capture stdout): bad window gives return 1 and an `::error::mob-process:` line naming the file and the field; complete window gives 0.
- Layers: `run_check` (tuple entry) -> `check_windows(root)` (returns list of error strings, reads files only) -> `window_errors(path, data)` pure helper (category, ended_at; later seats).
- Doubles: none for the filesystem (tempdir, as the existing tests); `git` and `issues` use the existing stub/mock from setUp; `window_errors` unit tests take plain dicts, no I/O.

## Existing window files must pass
205-rerank-3.json and 213-taskforce.json already carry a listed category, `ended_at`, and per reply role, model, context, started_at, reply, condition (null), probe_reference. Nothing in them changes. Ignore extra keys (`raw`, `attempt`, `attempts`, `proposed_by`, `rule`, `note`); the proposer is not a declared role seat. Green seat must not require `condition` truthy for NO OBJECTION.

## Notes
Domain expert answer at slice: no. The audit-lens reviewer (the D153 domain expert) and backend developer already cover it; no register or language knowledge is needed.

Slice field: SHIP AS IS | about 5 files, under 250 lines | 3 increments | one concern: the objection-window field check in mob_process.py

- Seat: architect (slice), context ctx-6, Claude Sonnet subagent; reason for a Claude seat: probes at 07:24 UTC show Codex, Copilot, Grok and agy without usable quota.
