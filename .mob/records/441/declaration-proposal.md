# Proposal for Decider D264: the task-force declaration and card text for card 441 (the bench path scrub's MINOR shapes from card 439's reviews become corpus rows, fixed where a narrow change can)

Facilitator, 2026-11-15 UTC, main at 275534b. VOTE on this declaration: adopt the card text of .mob/records/441/ready-body.md (the issue body, written from the card template; card_ready.py with PERSONAS.md prints ok) as card 441's Ready text, with the task force below, each role's service and exact model pinned before any seat runs. Card 441 is the follow-up of card 439 under the stop rule; it is ranked right after 439 (114.9). It changes no agreement text, so no confirmation step runs.

The seat plan cites the live probe .mob/records/441/probe-all-2026-11-15.jsonl (taken 2026-11-15 07:57 UTC): available agy (its default model; its strong tier gemini-4-argon-high refused card 439's review round 2 on its quota, reset around 2026-11-21), claude and cursor; qwen probes ok but stays unseated (a heavy local model under the cross-mob heavy-run rule); out: codex, copilot, grok and muse.

STANDING RULES: the card template and its Ready check; the kickoff checklist (checklists/ready.md, the model item); D263 (new: the slice names and commits a real-shape corpus of at least five families before the red; card 441 is its first trial card); D259 clause 4 (BLOCKING in a path-scrub review, new BLOCKING and new MINOR reported side by side); D256 (the differential generator, trial card 3 of 3); D260 (a push reported only after its result is read unpiped and the remote head equals the local commit); D251 (the toolchain stamp); D254 (no wall-clock assertion); the F-13 decision (the flag stands as the actual model until the CLI reports one); the interim cross-mob heavy-run lock before any heavy run; the account-name ruling in every brief.

## Task force (each role in its own context; models pinned at launch)

- Architect (corpus and narrow fixes): claude opus = claude-opus-5-5 | why: measures the real-shape corpus and any narrow fix against card 439's reference, strong tier (the guide's design row) | context: ctx-1
- Dev, red: claude sonnet = claude-sonnet-5-5 | why: the rows are given by the slice, mechanical test writing, mid tier, with the Pending convention and the backslash-safe way of writing rows in its brief | context: ctx-2
- Dev, green: cursor, `--model gpt-5.3-codex` | why: ports the architect's reference change, a code-writing tier, pinned (ports took 3 to 8 minutes on card 439) | context: ctx-3
- QA, adversarial review: claude opus = claude-opus-5-5 | why: tries to break the change, strong tier; agy's strong tier is out of quota (D262's reasoning carries over) | context: ctx-4
- Sign-off: claude sonnet = claude-sonnet-5-5 | why: runs the gate filters in a fresh clone and judges, mid tier; differs from the green seat | context: ctx-5

Domain-expert question at refinement: should an agent with different domain knowledge join? No: the shapes are listed in card 439's review records, which the architect and QA read; a cross-family reviewer is not available until agy's strong tier returns.

## Clauses

1. Adopt the Ready text of .mob/records/441/ready-body.md as card 441's text.
2. Adopt the task force above, with each role's service, exact model and reason, as declared before any seat runs.
3. The slice commits the D263 real-shape corpus (at least five families) and measures the reference against it before the red; a MINOR shape stays a pinned corpus row unless a narrow change fixes it with every card 439 row and the blow-up rows' second pass unchanged.
