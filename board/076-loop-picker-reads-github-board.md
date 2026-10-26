# Loop: the picker reads the GitHub board (card 063's logic, new source)

- **Status:** Ready (rank: see `C:\Users\ringe\projects\phrase-lang\board\MILESTONE-usable-v1.md`, read-only)
- **Type:** tooling (tooling (mob-defaults)); thin slice; authored by the driver rotation, reviewed by a non-author (D15), run atomically once card 062 exists (until then in a scratch clone), smoke-tested on a fake `gh` before any real apply
- **Milestone:** process: the board moves to GitHub Projects (the loop's source of truth)
- **Source:** Steve, 2026-10-26: move Phrase's board to a GitHub Projects board, as play-tracker has, so he can view it as a real board on his phone; decided before more loop cards are built on the file-based picker. Team design: Decider D25 (Codex, Copilot, Grok Yes; agy no response).
- **Depends on:** 063, 075
- **Story:** As Steve, I want the Phrase board as a real GitHub Projects board, so that I can see and re-rank it on my phone, and as the mob I want the loop to read that board, so that there is one source of truth.
- **One observable behaviour:** The picker from card 063 selects the next eligible card from the GitHub project through `gh api graphql` instead of the board files, with the same rules.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the board tests with a fake `gh` and fixtures, When the behaviour above is exercised, Then with a fake `gh`, the same fixtures as card 063 give the same answers: first Ready by Rank, Done and Superseded/Withdrawn skipped, unmet `Depends on` skipped, `none` means none and a missing line is not `none`, a live pid wins; duplicate or missing ranks fail validation instead of inventing an order
  2. Given the board tests with a fake `gh` and fixtures, When the behaviour above is exercised, Then a phone edit of the Rank number or a Status drag is honoured on the next pick; an open issue closed without Status Done is skipped and noted for the retro
  3. Given the board tests with a fake `gh` and fixtures, When the behaviour above is exercised, Then a real-board check runs the picker read-only against the live Phrase project and returns the top eligible card, which equals the first Ready item by Rank

## Context

- Design (D25): Project v2 under Programming-Made-EZ named `Phrase Board`, PRIVATE (Steve views it logged in on his phone; the issue text is already public in the repo; making the project public is a one-word ask for Steve and the scripts have no public setting). Fields: Rank, Card number, Depends on, Type, Milestone, Author, plus Status (Ready, In Progress, Done, Superseded/Withdrawn); the built-in close-sets-Done workflow is off. Tooling cards whose code runs in mob-defaults are still issues in phrase-lang. A card is Done only when its Done-when passes and a non-author reviewed it; a bare issue close never implies Done.
- Safety: the migration and picker scripts use only the `gh` token's `project`, `repo` and `read:org` scopes, against `Programming-Made-EZ/phrase-lang` and its ONE project; they NEVER delete issues, comments, items or the project, never edit an issue without the `<!-- phrase-card:NNN -->` marker, never change visibility, permissions, workflows or secrets, and never touch play-tracker, mob-defaults' GitHub repo or any other mob; the default is `--dry-run`, apply refuses any other owner or repo, and a second apply creates nothing; each card is smoke-tested on a fake `gh` before any real apply.
- Phrase board and docs (read-only, absolute paths): cards in `C:\Users\ringe\projects\phrase-lang\board\`, `VISION.md` and `docs\` under `C:\Users\ringe\projects\phrase-lang\`.
- Tests first with the fake `gh`; Windows and PowerShell quoting traps apply (write scripts to files, no nested quoting).

## Task force

- Tooling author [author]: Copilot | why: next in the rotation for tooling (D15)
- Reviewer [reviewer]: Grok | why: non-author review of the diff (D15)
- Safety reviewer: Grok | why: checks the migration against the never-do list above
- Facilitator: Claude | why: runs the dry runs and the real apply after review; any takeover is logged

## Done when

Run from the phrase-lang repo root (the picker lives in mob-defaults: run the tests there):

```
python -m unittest tests.test_loop_pick_board
python scripts/board/check_real_board.py --pick
python -m unittest discover -s tests
```
