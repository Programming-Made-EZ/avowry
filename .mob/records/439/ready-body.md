**Status: from Decider D259 clause 3 (card 435, 2026-11-14).** Card 435 closed at its rework with known limits; this card replaces the bench path scrub's single regex with a linear scanner. Its red is every input of card 435's second and third reviews that the rework does not handle, plus every MINOR finding (`.mob/records/435/second-review*.md`, `third-review*.md`, the closure record `.mob/records/435.json`).

## Persona

Riley

## Story

As Riley, I want the bench path scrub to find path roots in one linear pass and decide each path's extent with small explicit rules instead of one large regex, so that every stored bench row can be shared without naming whose machine produced it, and a review round fixes findings without opening new ones (goal: evidence that Phrase reduces LLM work without losing correctness or learnability).

## Acceptance criteria

- Scenario: [Z][O] Given every BLOCKING input of card 435's third review (UNC hosts written `\\<host>`, `\\host(1)` and `\\host's`; an escaped quote then a glued flag on a drive path; a bare account folder glued to a drive letter; the tail before `-D:`), when scrubbed, then no account folder or stand-in survives and a second pass changes nothing.
- Scenario: [M][B] Given every row of the 418, 426 and 435 data sets and card 435's generator (100,000 inputs) extended with every review input as a shape, when scrubbed, then the outputs equal today's on every row the rework handles correctly, with 0 leaks and 0 second-pass changes.
- Scenario: [I] Given 20,000 and 1,000,000 characters of each blow-up shape named in the reviews (`\\;`, `\\,`, `\\a;`, repeated doubled UNC paths, `\\C:/a\n`), when scrubbed, then the time grows linearly (doubling the input at most about doubles the time) and stays under 50 ms at 20,000 characters, as measured evidence at the slice and the review (no wall-clock assertion in a test, D254).
- Scenario: [E] Given the slice, when it is written, then it lists the shared escape set and every constant once, and names a mutant for each rule with its killing row; the red runs the mutants and the old-against-new differential before any green.
- Scenario: [S] Given the review prompt, when a review runs, then it uses D259 clause 4's BLOCKING definition and reports new BLOCKING and new MINOR findings since the last round side by side.

## ZOMBIES coverage

- Z (Zero): covered
- O (One): covered
- M (Many): covered
- B (Boundaries): covered
- I (Interface): covered
- E (Exceptions): covered
- S (Simple): covered

## Dimensions of Difficulty

- Environment: no
- Scope: no
- Unknown: no
- Communication: no

## Links

- Parent epic: none
- Spikes: none
- Related: #435 (closed at its rework), Decider D259, Decider D256 (the differential generator)

