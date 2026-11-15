# Proposal for Decider D261: the task-force declaration and card text for card 439 (the bench path scrub finds path roots in one linear pass, replacing the single regex)

Facilitator, 2026-11-15 UTC, main at 90b49be. VOTE on this declaration: adopt the card text of .mob/records/439/ready-body.md (the issue body, written from the card template; card_ready.py with PERSONAS.md prints ok) as card 439's Ready text, with the task force below, each role's service and exact model pinned before any seat runs. Card 439 comes from Decider D259 clause 3 and carries card 435's known limits as its red; it is ranked right after 435 (114.85). It changes no agreement text, so no confirmation step runs.

The seat plan cites the live probe .mob/records/439/probe-all-2026-11-15.jsonl (taken 2026-11-15 00:25 UTC): available agy, claude and cursor; qwen probes ok but is not seated while a play-tracker train is alive; out: codex (usage limit until 2026-11-18), copilot (no quota), grok (balance exhausted), muse (until 2026-11-16).

STANDING RULES: the card template and its Ready check; the kickoff checklist (checklists/ready.md, including the model item of mob-defaults #56); red before green with every review input of card 435 as a test row and a generator shape; D256 (the differential generator, trial card 2 of 3); D259 clause 4 (what counts as BLOCKING in a path-scrub review, with new BLOCKING and new MINOR reported side by side each round); D260 (a push is reported only after its result is read unpiped and the remote head equals the local commit); D251 (the toolchain stamp); D254 (no wall-clock assertion in a test: linear time is measured evidence at the slice and the review); the account-name ruling in every brief.

## Task force (each role in its own context; models pinned at launch)

- Architect (scanner design and the escape set): claude opus = claude-opus-5-5 | why: a linear scanner design with measured evidence, strong tier (the card 435 guide row for design) | context: ctx-1
- Dev, red: claude sonnet = claude-sonnet-5-5 | why: the rows are given by the reviews and the slice, mechanical test writing, mid tier, with the Pending-trait convention in its brief | context: ctx-2
- Dev, green: cursor, `--model gpt-5.3-codex` | why: implements the slice's scanner, a code-writing tier, pinned (card 435's green ran unpinned) | context: ctx-3
- QA, adversarial review: agy, `--model gemini-4-argon-high` | why: tries to break the scanner, strong tier (it found card 435's blow-ups and surviving mutants) | context: ctx-4
- Sign-off: claude sonnet = claude-sonnet-5-5 | why: runs the gate filters in a fresh clone and judges, mid tier; differs from the green seat | context: ctx-5

Domain-expert question at refinement: should an agent with different domain knowledge join? No: the path formats are the architect's and QA's domain on card 435, and their records carry them; a performance engineer is not added, because linear time is a design rule of the architect and is measured by QA.

## Clauses

1. Adopt the Ready text of .mob/records/439/ready-body.md as card 439's text.
2. Adopt the task force above, with each role's service, exact model and reason, as declared before any seat runs.
3. The slice lists the shared escape set and every constant once, names a mutant and its killing row for each rule, and includes a 20,000-character linear-time row as measured evidence; the red runs the mutants and the old-against-new differential before any green.
