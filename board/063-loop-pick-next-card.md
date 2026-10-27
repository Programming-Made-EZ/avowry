# Loop: pick the next eligible card from the board

- **Status:** SUPERSEDED 2026-10-26 by card 076 (the picker reads the GitHub board). The logic slice passed its tests (mob-defaults 63511f3) but the non-author review (Decider D26: Codex, Copilot, Grok No) found it does not fit the real board or the real run state; those defects are carried into 076's criteria.
- **Type:** tooling (thin slice; the code lives in mob-defaults; authored by the driver rotation and reviewed by a non-author under D15, with the smoke run)
- **Milestone:** process: the loop runner (retro seed P1: no turn ends with nothing running)
- **Source:** Steve, 2026-10-26 (mob-defaults is his product): "it should be a loop." The continuous loop must live in the tooling, not in the facilitator's chat turns; three idle gaps today all came from the loop depending on a chat turn (retro seed P1). Team design: Decider D23 (Codex Yes, Grok Yes, Copilot Support-it with changes; agy no response).
- **Depends on:** none
- **Story:** As the mob (and Steve), I want the loop to live in `loop.py` in mob-defaults, so that the work keeps going without a chat turn and stalls cannot come from an idle facilitator.
- **One observable behaviour:** A function returns the highest-ranked Ready card whose dependencies are all Done, reading the ranked board table.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then a fixture board returns the first Ready card in table order; Done and SUPERSEDED rows are skipped
  2. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then a card whose `Depends on:` is not all Done is skipped; `none` means no dependency; a missing line is not `none`
  3. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then a card whose live run is in flight (its `.mob/<id>/state.json` pid is alive) is returned first (WIP-of-one)

## Context

- loop picker; one function shared by the loop and mob_summary.
- Design (D23): `loop.py` is a thin supervisor over card mode; state in the gitignored `.mob/loop/`; Claude supervises and reviews, it is not the engine. Safety rules apply in full (delete guard, no kill by name, no Co-Authored-By, never stage `.mob/`, sandboxed Deciders, spend ceilings per card and per day).
- Read first: the mob-defaults `README.md`, `TEAM_WORKING_AGREEMENT.md`, `facilitator.py` (card mode, exit codes), `mob_summary.py`, `status.py`, `delete_guard.py`, `decider.py`, and cards 062 and 036.
- Tests first with the fake CLIs; the loop is smoke-tested before it touches a real card.

- Phrase board and docs (read-only, absolute paths; they are NOT in the mob-defaults repo): cards in `C:\Users\ringe\projects\phrase-lang\board\`, `VISION.md` and `docs\` under `C:\Users\ringe\projects\phrase-lang\`; card numbers in this card (062, 036, 063 to 072) refer to those files.

## Task force

- Tooling author [author]: Codex | why: next in the rotation for tooling (D15)
- Reviewer [reviewer]: Copilot | why: non-author review of the diff (D15)
- Safety reviewer: Grok | why: checks deletes, kills, locks and spend against the Safety rules
- Facilitator: Claude | why: runs the smoke run and reviews; any takeover is logged

## Done when

Run from the mob-defaults repo root (the mob has no board of its own; this card lives on Phrase's board):

```
python -m unittest tests.test_loop_pick
python -m unittest discover -s tests
```
