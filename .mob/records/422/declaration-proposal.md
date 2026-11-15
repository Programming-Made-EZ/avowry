# Proposal for Decider D268: the task-force declaration and card text for card 422 (the restored-package licence checks fail when the page is wrong)

Facilitator, 2026-11-15 UTC, main at e5bf308. VOTE on this declaration: adopt the card text of .mob/records/422/ready-body.md as card 422's Ready text. It is the issue body, written from the card template, and card_ready.py with PERSONAS.md prints ok. Adopt it with the task force below, each role's service and exact model pinned before any seat runs. Card 422 is the top Ready card on the Phrase Board (rank 295, no dependency). It changes no agreement text, so no confirmation step runs.

The seat plan cites the live probe .mob/records/422/probe-all-2026-11-15.jsonl, taken 2026-11-15 20:08 UTC:
- available: agy (gemini-3.8-flash-medium; its strong tier is out of quota until about 2026-11-21), claude and cursor;
- probes ok but stays unseated: qwen (a heavy local model under the cross-mob heavy-run rule);
- out: codex and copilot (quota), grok (balance exhausted) and muse (until 2026-11-16 00:00 UTC).

STANDING RULES:
- the card template and its Ready check;
- the kickoff checklist (checklists/ready.md, the model item);
- D263 with D265: the checks recognise text (MSBuild project text, page prose and rows, lockfile lines), so the slice names and commits a real-shape corpus of at least five families, with one row per input encoding the checks meet, before the red; each fix slice commits its changed rules' neighbour variants before its red. Card 422 is D265's third trial card, and its encoding list includes leading, inner and line-break whitespace forms;
- D267: a waiting seat is stopped before its runs are taken over;
- D259 clause 4 (BLOCKING, new BLOCKING and new MINOR side by side);
- D260 (a push is reported only after its result is read unpiped and the remote head equals the local commit);
- D251 (the toolchain stamp);
- the F-13 decision (the flag stands as the actual model until the CLI reports one);
- the interim cross-mob heavy-run lock before any heavy run (the networked checks and the acceptance project are heavy runs);
- light work (refine, slice, red, per-slice offline runs) goes ahead of any heavy run;
- the account-name ruling in every brief.

## Task force (each role in its own context; models pinned at launch)

- Architect (slice, corpus and reference): claude opus = claude-opus-5-5 | why: the checks are text recognisers over MSBuild, page prose and lockfiles, so the slice needs a real-shape corpus with encodings and a reference measured against it, strong tier (the guide's design row; cards 439, 441 and 430) | context: ctx-1
- Dev, red: claude sonnet = claude-sonnet-5-5 | why: rows given by the slice, mechanical test writing, mid tier (0 expectation errors across 356 rows on card 430) | context: ctx-2
- Dev, green: cursor, `--model gpt-5.3-codex` | why: ports the architect's reference checks, a code-writing tier, pinned (9 of 9 ports equal to the reference on cards 439, 441 and 430) | context: ctx-3
- QA, adversarial review: claude opus = claude-opus-5-5 | why: tries to break the checks with page, MSBuild and lockfile shapes outside the corpus, strong tier; agy's strong tier is out of quota | context: ctx-4
- Sign-off: claude sonnet = claude-sonnet-5-5 | why: runs the gate filters and the networked checks in a fresh clone under the heavy-run lock on a quiet machine and judges, mid tier; differs from the green seat | context: ctx-5

Domain-expert question at refinement: should an agent with different domain knowledge join? No. The shapes come from the card 369 second review and the repository's own files, which the architect and QA read. A cross-family reviewer is not available until agy's strong tier returns.

## Clauses

1. Adopt the Ready text of .mob/records/422/ready-body.md as card 422's text.
2. Adopt the task force above, with each role's service, exact model and reason, as declared before any seat runs.
3. Before the red, the slice commits the D263 and D265 real-shape corpus: at least five families of page, MSBuild and lockfile shapes, with one row per input encoding the checks meet (plain text, comments, CDATA and entities where MSBuild allows them, attribute order and child-element forms, Condition expressions, leading, inner and line-break whitespace, and the page's table and prose forms). It measures the reference against the corpus and against the real repository files. The networked checks and any acceptance run happen only under the heavy-run lock.
