# Board: put each issue on the project with its Status, Rank and fields

- **Status:** Ready (rank: see `C:\Users\ringe\projects\phrase-lang\board\MILESTONE-usable-v1.md`, read-only)
- **Type:** tooling (tooling (phrase-lang `scripts/board/`)); thin slice; authored by the driver rotation, reviewed by a non-author (D15), run atomically once card 062 exists (until then in a scratch clone), smoke-tested on a fake `gh` before any real apply
- **Milestone:** process: the board moves to GitHub Projects (the loop's source of truth)
- **Source:** Steve, 2026-10-26: move Phrase's board to a GitHub Projects board, as play-tracker has, so he can view it as a real board on his phone; decided before more loop cards are built on the file-based picker. Team design: Decider D25 (Codex, Copilot, Grok Yes; agy no response).
- **Depends on:** 074
- **Story:** As Steve, I want the Phrase board as a real GitHub Projects board, so that I can see and re-rank it on my phone, and as the mob I want the loop to read that board, so that there is one source of truth.
- **One observable behaviour:** Each migrated issue is an item on the project with Status, Rank, Card number, Depends on, Type, Milestone and Author set from the ranked table and the card file.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the board tests with a fake `gh` and fixtures, When the behaviour above is exercised, Then Status comes from the card: Done where the rank table or card says Done (002, 016, 018, 032, 036), Superseded/Withdrawn from the status line, In Progress for the card whose committed MOB.md says it is running, Ready otherwise; a card missing from the rank table gets an empty Rank
  2. Given the board tests with a fake `gh` and fixtures, When the behaviour above is exercised, Then Rank equals the table order, unique among ranked cards; `Depends on` is copied exactly (a missing line stays missing, it is not `none`)
  3. Given the board tests with a fake `gh` and fixtures, When the behaviour above is exercised, Then a real-board check reads every item back through GraphQL and the fields equal the card files, with a second apply changing nothing

## Context

- Design (D25): Project v2 under Programming-Made-EZ named `Phrase Board`, PRIVATE (Steve views it logged in on his phone; the issue text is already public in the repo; making the project public is a one-word ask for Steve and the scripts have no public setting). Fields: Rank, Card number, Depends on, Type, Milestone, Author, plus Status (Ready, In Progress, Done, Superseded/Withdrawn); the built-in close-sets-Done workflow is off. Tooling cards whose code runs in mob-defaults are still issues in phrase-lang. A card is Done only when its Done-when passes and a non-author reviewed it; a bare issue close never implies Done.
- Safety: the migration and picker scripts use only the `gh` token's `project`, `repo` and `read:org` scopes, against `Programming-Made-EZ/phrase-lang` and its ONE project; they NEVER delete issues, comments, items or the project, never edit an issue without the `<!-- phrase-card:NNN -->` marker, never change visibility, permissions, workflows or secrets, and never touch play-tracker, mob-defaults' GitHub repo or any other mob; the default is `--dry-run`, apply refuses any other owner or repo, and a second apply creates nothing; each card is smoke-tested on a fake `gh` before any real apply.
- Phrase board and docs (read-only, absolute paths): cards in `C:\Users\ringe\projects\phrase-lang\board\`, `VISION.md` and `docs\` under `C:\Users\ringe\projects\phrase-lang\`.
- Tests first with the fake `gh`; Windows and PowerShell quoting traps apply (write scripts to files, no nested quoting).

## Task force

- Tooling author [author]: Codex | why: next in the rotation for tooling (D15)
- Reviewer [reviewer]: Copilot | why: non-author review of the diff (D15)
- Safety reviewer: Grok | why: checks the migration against the never-do list above
- Facilitator: Claude | why: runs the dry runs and the real apply after review; any takeover is logged

## Done when

Run from the phrase-lang repo root:

```
python -m unittest tests.test_board_fields
python scripts/board/check_real_board.py --fields
python -m unittest discover -s tests
```
