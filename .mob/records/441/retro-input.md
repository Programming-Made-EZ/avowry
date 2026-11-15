# Card 441 retro input (facilitator, from the card's records; committed before the voices)

Times local (UTC-8) unless marked Z; from the commit log and the seat start and done files.

## Flow metrics (first: constraint first, D258 clause 4)

- Lead time: kickoff Decider D264 committed 00:02 (2026-11-15) to the sign-off 06:43: about 6 h 41 min, one card, no pause by the mob.
- Waiting on the other mob's heavy lock: about 3 h 50 min of the 6 h 41 min. Play-tracker's serial ship-train held the lock from 09:43Z to about 11:51Z (the fix-round-1 gate held, about 2 h 20 min) and from 12:28Z to 14:01Z (the fix-round-2 red's runs held, about 1 h 33 min). Work time without those waits: about 2 h 50 min.
- Rounds: one red and green, then TWO fix rounds (architect re-slice, red, port, light gate, review). Review BLOCKING by round: 3, 2, 0 (MINOR: 7, 3, 3). Card 439 for comparison: 8, 2, 3, 0 over four rounds. D259's guard (new BLOCKING after a fix, target 2 or fewer): 2 after the first fix (met), 0 after the second.
- D263 first trial (the slice commits a real-shape corpus of at least five families before the red): met. The slice committed 104 rows in 8 families before the red; the corpus grew to 130 rows and 11 families at fix round 1 and 151 rows at fix round 2. Round 1's three BLOCKING findings were all OUTSIDE the corpus families at the time (Java and URI forms, escaped JSON colon lists, forward-slash Windows volume specs); round 2's two were neighbours of round 1's fixes (separator runs after a drive after `/`; a lone letter before an escaped `:\/`).
- Time by step: kickoff probe and D264 about 15 min; architect slice about 20 min (+ re-timing after the lock cleared); red 4 min (reproduced in a clean clone 3 min later); ports 2 min 35 s, 1 min 55 s and 1 min 52 s (Cursor gpt-5.3-codex, the three green seat runs); light gates about 13 to 17 min each; review rounds 1 to 3 about 15, 41 and 18 min (claude opus); fix slices about 18 and 14 min; fix reds about 2 min and about 13 min of work (plus the lock wait); sign-off about 18 min (15 min 19 s of it the affected module).
- Pushes: the card branch was first pushed mid-card (the owner's "push every commit"); two pushes were refused by the pre-push MOB.md staleness check (7 and then 4 non-MOB commits; the check counts from the checkout's HEAD, not the pushed commit); each push 10 to 16 min.
- Constraint: the cross-mob heavy-run lock (waiting on the other mob's ship-trains), then the design's real-shape coverage (each review found a neighbouring family). Focusing steps applied: light work while the lock was held (slice, review reading, follow-up card), the red seat stopped at `busy` instead of running; D263's corpus at the slice.
- Idle minutes over 10 with work available: one (about 40 min after the red reached main, before the 09:21Z watchdog). Dispatch nudges since the kickoff: 3 (the 08:21Z watchdog to keep light under play-tracker's lock, the 09:21Z watchdog about the idle 40 minutes, the 11:51Z watchdog that the lock was clear).

## What went well

- Every seat declared its service, exact model and reason before launch (D264); actual recorded per seat in `model-allocation.md`; no mid-card model change.
- The green seat PORTED the reference each time (2 to 3 minutes, sweeps equal to the reference); one facilitator takeover in fix round 1 (two Pending traits left), none in fix round 2.
- The reviewer checked a candidate fix for each BLOCKING finding against the three slices; the architect took one as given and improved the other.
- Every timed shape linear in every round; sweeps B and C unchanged (4,559 and 2,679 new leaks against the base, 0 regressions, 0 second-pass changes).
- The heavy lock: Avowry's own gate and sign-off took the lock; the red seat refused to run while the other mob held it.

## What went wrong

- Two lock breaches recorded: the architect's slice sweeps ran during play-tracker's train 56 (the facilitator's helper misread the other mob's lock format; fixed at 08:25Z); one chained probe pass in fix round 2 ran right after a `busy` check (no build).
- The heredoc rule failed again in the facilitator's own session (a Python patch through a shell heredoc lost its backslashes; caught by its own assertion, nothing written).
- The commit hook refused the raw gate logs twice (home paths) and one fix-slice example (an escaped path shape); scrubbed and split.
- Two card-branch pushes refused for a stale MOB.md.
- The gate summary's grep missed the pass counts (the detailed logger's format); the counts came from the filter logs.

## Model allocation review (input for the voices)

See `.mob/records/441/model-allocation.md`. Seats: architect claude opus (ctx-1: slice and two fix slices, fresh contexts); red claude sonnet (ctx-2: red and two fix reds); green cursor gpt-5.3-codex pinned (ctx-3: three ports; the CLI never reports a model, F-13); QA claude opus (ctx-4: rounds 1 to 3, fresh contexts); sign-off claude sonnet (ctx-5); Decider D264 on agy gemini-3.8-flash-medium. agy's strong tier gemini-4-argon-high was out of quota for the whole card. Card 439's question carried over: does a review by the design's model family (claude opus reviewing a claude opus design) find less? Data here: rounds 1 to 3 found 3, 2 and 0 BLOCKING, every one replayed true; round 2's finding 1 had been missed by round 1 (it leaked at the first green too).

## Retro voices, declared before launch

| Voices | Service | Model, pinned | Reason |
|---|---|---|---|
| product manager, audit lens, coach | agy (CLI, a fresh context each) | gemini-3.8-flash-medium (`--model`) | reflective judgement on committed records, mid tier |
| architect, qa, dev, decider | claude (subagent, a fresh context each) | sonnet = claude-sonnet-5-5 (the Agent model flag) | the same, mid tier; two services so the voices are not one model |
