# Proposal for Decider D266: the task-force declaration and card text for card 430 (the wall-clock guard recognises the missed time-limit spellings, and the hard-limit tests run isolated)

Facilitator, 2026-11-15 UTC, main at 2bb5d09. VOTE on this declaration: adopt the card text of .mob/records/430/ready-body.md (the issue body, written from the card template; card_ready.py with PERSONAS.md prints ok) as card 430's Ready text, with the task force below, each role's service and exact model pinned before any seat runs. Card 430 is the top Ready card on the Phrase Board (rank 297, no dependency). It changes no agreement text, so no confirmation step runs.

The seat plan cites the live probe .mob/records/430/probe-all-2026-11-15.jsonl (taken 2026-11-15 15:24 UTC): available agy (gemini-3.8-flash-medium; its strong tier gemini-4-argon-high out of quota until about 2026-11-21), claude and cursor; qwen probes ok but stays unseated (a heavy local model under the cross-mob heavy-run rule); out: codex, copilot, grok (balance exhausted) and muse (until 2026-11-16 00:00 UTC).

STANDING RULES: the card template and its Ready check; the kickoff checklist (checklists/ready.md, the model item); D263 with D265 (the guard recognises text: the slice names and commits a real-shape corpus of at least five families, one row per input encoding the guard meets, before the red, and each fix slice commits its changed rules' neighbour variants before its red; card 430 is the second trial card); D259 clause 4 (BLOCKING, new BLOCKING and new MINOR side by side); D260 (a push reported only after its result is read unpiped and the remote head equals the local commit); D251 (the toolchain stamp); D254 (no wall-clock assertion); the F-13 decision (the flag stands as the actual model until the CLI reports one); the interim cross-mob heavy-run lock before any heavy run (the [S] ceiling check is one); light work (refine, slice, red, per-slice runs) ahead of any heavy run; the account-name ruling in every brief.

## Task force (each role in its own context; models pinned at launch)

- Architect (slice, corpus and reference): claude opus = claude-opus-5-5 | why: the guard is a text scanner, so the slice needs the real-shape corpus from the repository's own tests and a reference change measured against it, strong tier (the guide's design row; card 439 and 441 data) | context: ctx-1
- Dev, red: claude sonnet = claude-sonnet-5-5 | why: rows given by the slice, mechanical test writing, mid tier, with the Pending convention and the backslash-safe way of writing rows in its brief | context: ctx-2
- Dev, green: cursor, `--model gpt-5.3-codex` | why: ports the architect's reference guard and adds the collection attribute to the named tests, a code-writing tier, pinned (7 of 7 ports equal to the reference on cards 439 and 441) | context: ctx-3
- QA, adversarial review: claude opus = claude-opus-5-5 | why: tries to break the guard with spellings outside the corpus, strong tier; agy's strong tier is out of quota | context: ctx-4
- Sign-off: claude sonnet = claude-sonnet-5-5 | why: runs the gate filters and the [S] ceiling check in a fresh clone under the heavy-run lock and judges, mid tier; differs from the green seat | context: ctx-5

Domain-expert question at refinement: should an agent with different domain knowledge join? No: the spellings come from the card 417 second review and the repository's own tests, which the architect and QA read; a cross-family reviewer is not available until agy's strong tier returns.

## Clauses

1. Adopt the Ready text of .mob/records/430/ready-body.md as card 430's text.
2. Adopt the task force above, with each role's service, exact model and reason, as declared before any seat runs.
3. The slice commits the D263 and D265 real-shape corpus (at least five families of time-limit spellings and near misses taken from real test code, with one row per input encoding the guard meets: plain code, comments, string and verbatim and raw string literals, attribute lists, nested and partial types) and measures the reference against it and against the real test project before the red; the [S] ceiling check runs only under the heavy-run lock.
