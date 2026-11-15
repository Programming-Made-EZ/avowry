# Card 439 retro input (facilitator, from the card's records; committed before the voices)

Times local (UTC-8) unless marked Z; from the commit log and the seat start and done files.

## Flow metrics (first: constraint first, D258 clause 4)

- Lead time: kickoff Decider D261 committed 16:35 (2026-11-14) to the sign-off 23:03: about 6 h 30 min, one card, no pause.
- Rounds: one red and green, then THREE fix rounds, each a re-slice by the architect, a red, a port by the green seat, a light gate and a review. Review BLOCKING by round: 8, 2, 3, 0 (MINOR: 2, 4, 3, 4). D259's guard (new BLOCKING after a fix, target 2 or fewer): 2 and 3 after the first two fixes.
- Time by step: kickoff probe and D261 about 10 min; architect slice about 36 min (+ a resize); red 6.5 min; first green (a reimplementation) 44 min; first light gate 19 min; review round 1 48 min (agy argon); fix slices 9, 5 and 9 min; fix reds about 8, 5 and 5 min; ports 8, 5 and 3 min; light gates about 16 min each; review rounds 2 to 4 26, 26 and 9 min (claude opus); D262 5 min; sign-off 15 min.
- Waiting, not working: pushes (the kickoff and slice pushes 10 to 16 min each in the pre-push check); the reseat after agy's quota refusal (D262); the cross-mob heavy-run rule arrived mid-card (no run held back after it; one earlier acceptance run overlapped a play-tracker train, which failed).
- Constraint: the first green REIMPLEMENTED the design (939 lines) instead of porting the reference, and half of round 1's BLOCKING findings came from that departure; then the design itself had gaps the slice's measurements missed (glued flags, mixed slashes, Unix colon lists, Windows-host volume specs, an account folder before `:/`), found one family per round. Focusing steps applied: the green ports the reference (ruling 4); the reviewer measures candidate fixes as mutants. Next measured effect to check: review rounds per scrub card and BLOCKING after the first fix.
- Idle minutes: none over 10 with work available. Dispatch nudges since the kickoff: 3 (a watchdog at 00:51Z, a watchdog at 03:21Z that assumed agy had hung, the cross-mob heavy-run rule after play-tracker train 56 failed).

## What went well

- Pinned models at every seat from the kickoff (D261), declared before each launch, actual recorded; the one mid-card change went through a Decider (D262).
- The facilitator's four-way replay (pre-435, the replaced regex, the architect's reference, the green) separated design defects from port departures in round 1.
- Porting the reference: every later green took 3 to 8 minutes and matched the reference's sweep counts exactly.
- The reviewer measured candidate fixes as mutants (rounds 3 and 4), which the architect adopted or widened.
- The linear design held: every timed shape linear in all four rounds (the worst after the fix 2.3 ms at 20,000 characters, against 2 s for the replaced regex).

## Things that went wrong or nearly

1. The first green reimplemented instead of porting (939 lines; 5 of round 1's 8 BLOCKING findings were its own).
2. The architect's slice measured the review inputs it was given, but not the real-shape families the later reviews used (colon lists, Docker volume specs); each round found the next family.
3. The heredoc collapsed doubled backslashes again: the red seat's nine blow-up rows were wrong until fix round 1 (corrected and recorded); the facilitator hit the same in its own probes.
4. The repository's home-path hook refused path-shaped examples four times (slice size after scrubbing, the fix slice, a code comment, the reference copy); each was split by hand.
5. agy's strong tier ran out of quota at review round 2 (148 hours); D262 reseated QA on claude opus, the design's model family.
6. An acceptance run of this card overlapped play-tracker train 56, which failed Jest on 4 heavy suites; the interim heavy-run lock arrived from Dispatch mid-card (mob-defaults #59).
7. Main and the card branch diverged (a MOB.md commit on main after the red), so the card was rebased before main: the gated and signed-off commit ids changed (same content).

## Model allocation review (input for the voices)

See `.mob/records/439/model-allocation.md`. Seats: architect claude opus (ctx-1: slice and three fixes); red claude sonnet (ctx-2: red and three fix reds); green cursor gpt-5.3-codex pinned (ctx-3: one reimplementation, three ports; the CLI never reported a model); QA agy gemini-4-argon-high (ctx-4: round 1; quota out at round 2), then claude opus (ctx-6: rounds 2 to 4, D262); sign-off claude sonnet (ctx-5); Deciders D261 and D262 on agy gemini-3.8-flash-medium (one service). D262's question for this retro: did a review by the design's model family (claude opus reviewing a claude opus design) find less? Data: round 1 (agy argon) 8 BLOCKING with 2 not reproducible; rounds 2 to 4 (claude opus) 2, 3 and 0, every one replayed true, and 86 new real shapes tried in round 4.

## Retro voices, declared before launch

| Voices | Service | Model, pinned | Reason |
|---|---|---|---|
| product manager, audit lens, coach | agy (CLI, a fresh context each) | gemini-3.8-flash-medium (`--model`) | reflective judgement on committed records, mid tier |
| architect, qa, dev, decider | claude (subagent, a fresh context each) | sonnet = claude-sonnet-5-5 | the same, mid tier; two services so the voices are not one model |
