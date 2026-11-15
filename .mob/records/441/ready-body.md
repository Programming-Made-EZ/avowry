**Status: the follow-up of card 439 under the stop rule (2026-11-15).** Card 439's review rounds 2, 3 and 4 classed these findings MINOR under Decider D259 clause 4 (older shapes the replaced regex shares, contrived compositions, a surviving mutant, a redundant condition). Each becomes a corpus row; any that a narrow, measured change can fix is fixed. Sources: `.mob/records/439/review2.md`, `review3.md`, `review4.md` and their raw files.

## Persona

Riley

## Story

As Riley, I want the bench path scrub's remaining MINOR shapes from card 439's reviews pinned as corpus rows and fixed where a narrow change can do it, so that every stored bench row can be shared without naming whose machine produced it (goal: evidence that Phrase reduces LLM work without losing correctness or learnability).

## Acceptance criteria

- Scenario: [Z][O] Given the older real shapes the reviews named (MSVC glued `/I` and `/Fo`, `-isystem` before a home path, `~name/`, JSON-escaped slashes, `\\?\UNC\...`, URLs with a home path, `MAIL=/var/mail/` + name, `ssh://host/home/` + name), when scrubbed, then each is a corpus row whose output is pinned, and each that a narrow measured change fixes leaks nothing and is stable on a second pass.
- Scenario: [M][B] Given the contrived one-letter chains (a one-letter name in a drive or UNC chain before `:/` plus a home path; `-v ~/x:/data`; `/tmp/x://server/...`), when scrubbed, then each is pinned as a corpus row, and a fix, if any, keeps every card 439 row and the `\\C:/a\n` blow-up row's second pass.
- Scenario: [I] Given the surviving mutant `ownletter-a-letters-only`, when its killing row (`-v /home/` + `u1a:/data`) is added, then the mutant fails it.
- Scenario: [E] Given the redundant condition `j > finalStart` in the one-letter rule, when it is removed, then every slice passes unchanged.
- Scenario: [S] Given the device-path prefixes `\\?\` and `\\.\` before a drive, when scrubbed, then their outputs stay pinned as card 439 pinned them, unless this card changes them deliberately with a reason.

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
- Related: #439 (the linear scanner), #435, Decider D259

