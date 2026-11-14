# Decider D259: card 435 after three reviews (the path scrub is not converging)

Proposer: the facilitator, 2026-11-14. Evidence: `second-review.md`, `third-review.md` and their replays (`second-review-verify.md`, `third-review-verify.md`), `slice-rework.md`. Vote on the whole; if you object to one clause, say which and the one condition that would bring you to Yes.

## The situation

Main carries the first green (547538f), which the second review broke with 11 BLOCKING findings, all replayed: new leaks, over-scrub of correct lines, second-pass changes and a quadratic blow-up of 7 seconds on 20,000 characters. The rework (e374ab7, branch card-435-rework) fixes every one of them and kills 27 mutants, but the third review found 7 new BLOCKING findings in the rework's own pieces, replayed: new leaks on UNC hosts written `\\<host>`, `\\host(1)` or `\\host's`, a leak after an escaped quote and a glued flag, a pass-2 change, and a new quadratic shape of 2 seconds on 20,000 characters of `\\C:/a\n`. Two rounds in a row, fixing one review's BLOCKING list produced a comparable number of new BLOCKING defects in the next. Measured against main, the rework is better on every comparison made: the extended sweep's leaks fall from 61,514 to 7,807 and the worst blow-up from 7 seconds to 2.

## Clauses

1. **Ship the rework now.** The rework (e374ab7) goes to main after the green gate (light while a play-tracker train is alive), because it is strictly better than what main runs today on every measured comparison.
2. **Close card 435 at the rework with its limits named.** The third review's BLOCKING findings and every MINOR finding of the second and third reviews become the red of ONE new card, and the card 435 header comment and closure record name the known limits; no fourth regex patch round in card 435.
3. **The new card replaces the single regex with a linear scanner.** Its architect designs a small scanner that finds root candidates in one pass and decides each path's extent with explicit rules, so time is linear by construction, and every input of the three reviews becomes a test row before the green.
4. **What counts as BLOCKING in a path-scrub review.** A leak or a regression on a shape in the generator corpus or on a named real shape (tool output, JSON logs, compiler arguments), a second-pass change on such a shape, or any super-linear time is BLOCKING; a contrived composition with no real source is MINOR and becomes a corpus row in the next card, so review rounds converge.

## How it lands

Clause 1 is the facilitator's next step after this Decider; clause 2 is card 435's sign-off, retro and closure; clause 3 is a new card from the card template with the model per role declared at kickoff; clause 4 enters the next path-scrub review prompt and the card. Cost: one gate run now and one new card. Reversal: a later Decider can reopen card 435.
