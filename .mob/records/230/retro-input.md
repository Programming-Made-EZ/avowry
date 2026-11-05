# Card 230 retro input (facilitator, facts only)

Card 230: the facilitator and seats never write the same checkout at once. Done 2026-11-05 after sign-off with conditions (.mob/records/230/signoff-ctx6.md). Code in mob-crosscli 9b9e162..1732a82; full suite on 1732a82: 1840 OK (skipped=1).

## Flow (UTC, from commits)
- Ready 16:14 (2026-11-04) to sign-off 02:35 (2026-11-05): about 10 hours 20 minutes.
- Windows: 230-taskforce (2 rounds), 230-slice (2 rounds + 1 re-ask), 230-scope (2 rounds), 230-scope-2 (2 rounds + 1 re-ask), 230-scope-3 (1 round + 1 re-ask). Five windows, nine polling rounds of six seats, three single-seat re-asks. Each full round about 10 to 12 minutes with Grok disabled.
- Caps 1+1+1 at the slice, raised to 2+1+1 (230-scope, smoke regression), 2+1+2 (230-scope-2, two older pins), 3+1+2 (230-scope-3, the product owner's trunk-based direction).
- Reds 1, 1b, 2, 1c, 3, 3b, 1d; greens 1, 1b, 2, 3, (3b none), 1c. Two full-suite stops (green 1: tests.test_smoke; green 3: the result-shape pin and the privacy scan).
- Design: per-seat worktrees with the facilitator integrating (task force, slice), narrowed to worktrees only with an integration path (scope), then removed on the product owner's trunk-based direction (scope-3): all writing serial on main under the single-writer lock; parallelism only for voting (working agreement line added).
- Seats: ready Copilot; slice and scopes Codex read-only (its PowerShell refused under the Windows sandbox, error 1920: card 297; commands ran through cmd); reds and greens Claude (green 1 attempt 1 on Codex could not launch a shell); gate Copilot (attempt 1 timed out at 30 minutes without a verdict; attempt 2 SHIP); sign-off Claude.
- Recurrences of the card's own defect during the card: the ready seat wrote over card 290's committed proposal in the shared avowry checkout; two facilitator processes raced a push at 19:46 UTC; audit 219 found four needless MOB.md regeneration commits from the facilitator's push retry (fixed: push_retry.sh).
- Probe age past 50 minutes at a seat start: green 1 (65 to 69 minutes; audit 220, F-085); re-probed before every later seat.
- Filed: cards 295 and 296 (legacy characterization, product refinement), 297 (Codex shell in the Windows sandbox), 298 (a second decider run votes unprotected), 299 (a second writer refused across processes).

## Adjustments to review (4)
1. D184 (each scope premise is run in code on HEAD): second card under it. Three scope proposals, each with quoted premises from Codex read-only via cmd.
2. D181 (a lettered red ends at the first passing green): red 1b in-cycle; red 1c, red 3b and red 1d each after a passed green as a new cycle with its window first.
3. Grok skipped in counted windows (since card 290): window rounds about 10 to 12 minutes.
4. Seat worktrees used by the facilitator for this card's own reds and greens (until the trunk-based direction), then seats committing directly on main while the facilitator wrote nothing there.

## Queued retro inputs to weigh
- Retro experiment (owner agile coach; trigger: the next card's decision windows, so this card is its baseline): when round 1 has no No and only one seat's condition, fold it in and re-ask only that seat plus any seat whose cited point it touches; full re-poll only for a real No or conflicting conditions. Measure window wall-clock per card; guard: reversed decisions and findings traced to a skipped re-poll must not rise. Data here: 230-taskforce, 230-slice, 230-scope-2 each ran a full six-seat round 2 for one seat's condition (230-scope had two compatible conditions).
- The two full-suite stops each cost a scope window; the slice's S case named the smoke behaviour but no red pinned it before green 1.
