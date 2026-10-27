# Board: show a card's Shape and what it Enables

- **Status:** Done 2026-10-26 (Shape and Enables fields live on the private board; 83 items match; Contract checks in check_real_board.py --fields)
- **Shape:** Story
- **Type:** tooling (phrase-lang `scripts/board/`); thin slice; authored by the driver rotation, reviewed by a non-author (D15); smoke-tested before it touches a real card
- **Milestone:** process: the board
- **Source:** Steve, 2026-10-26 (proposal A): two card shapes, Story and Contract, joined by outside-in TDD; Decider D28. The board needs Shape and Enables fields.
- **Depends on:** 075, 079
- **Story:** As Steve, I want to see on the board whether a card is a Story or a Contract and which Story a Contract enables, so that I can tell the why from the how on my phone and see that no contract is an orphan.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then the project has a single-select field Shape (Story, Contract) and a text field Enables, created by `project.py --apply` (idempotent, private project unchanged, GitHub-reserved names avoided by a test)
  2. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then `set_fields.py` writes Shape from the card's `Shape:` line (a card without one is a Story) and Enables from a Contract's `Enables:` line, and a second apply writes nothing
  3. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then a real-board check reads every item back and a Contract item has a non-empty Enables that names a Story item

## Context

- Safety rules apply in full (never delete, default dry run, only the Phrase repo, no other mob). Bootstrap phase: no metric-based rejection until a Decider declares the baseline.
- Phrase board and docs (read-only, absolute paths): `C:\Users\ringe\projects\phrase-lang\board\`, `C:\Users\ringe\projects\phrase-lang\PERSONAS.md`, `C:\Users\ringe\projects\phrase-lang\VISION.md`, `C:\Users\ringe\projects\phrase-lang\docs\decisions\DECIDERS.md`.

## Task force

- Author [author]: Codex | why: next in the rotation (D15)
- Reviewer [reviewer]: Copilot | why: non-author review of the diff (D15)
- Language designer: Grok | why: checks the work against GRAMMAR.md, 022 and the outcome rule
- Facilitator: Claude | why: runs the checks and any real apply after review; takeovers are logged

## Done when

All must exit 0, run from the phrase-lang repo root:

```
python -m unittest discover -s tests -p "test_board*.py"
python -m scripts.board.check_real_board --fields
python scripts/check-story-persona.py
```
