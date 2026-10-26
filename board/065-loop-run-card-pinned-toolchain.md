# Loop: run one card with a pinned toolchain

- **Status:** Ready (rank: see `C:\Users\ringe\projects\phrase-lang\board\MILESTONE-usable-v1.md`, read-only)
- **Type:** tooling (thin slice; the code lives in mob-defaults; authored by the driver rotation and reviewed by a non-author under D15, with the smoke run)
- **Milestone:** process: the loop runner (retro seed P1: no turn ends with nothing running)
- **Source:** Steve, 2026-10-26 (mob-defaults is his product): "it should be a loop." The continuous loop must live in the tooling, not in the facilitator's chat turns; three idle gaps today all came from the loop depending on a chat turn (retro seed P1). Team design: Decider D23 (Codex Yes, Grok Yes, Copilot Support-it with changes; agy no response).
- **Depends on:** 064
- **Story:** As the mob (and Steve), I want the loop to live in `loop.py` in mob-defaults, so that the work keeps going without a chat turn and stalls cannot come from an idle facilitator.
- **One observable behaviour:** The loop launches card mode for the picked card with the .NET 8 environment set from a checked-in toolchain file, and records the exit code.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then a missing or wrong .NET runtime fails BEFORE card mode is dispatched, with a clear message (the 018 stall class)
  2. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then with the toolchain file set (`DOTNET_ROOT`, `PATH`), the child sees them; the card's `--max-cycles` and `--ship --publish-summary` flags are passed
  3. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then the exit code and a classified outcome (done, cap, usage, not-ready, flag, tree-changed) are persisted to state

## Context

- toolchain config lives in the board repo (`board/toolchain.json`), read by the loop; never a per-shell habit.
- Design (D23): `loop.py` is a thin supervisor over card mode; state in the gitignored `.mob/loop/`; Claude supervises and reviews, it is not the engine. Safety rules apply in full (delete guard, no kill by name, no Co-Authored-By, never stage `.mob/`, sandboxed Deciders, spend ceilings per card and per day).
- Read first: the mob-defaults `README.md`, `TEAM_WORKING_AGREEMENT.md`, `facilitator.py` (card mode, exit codes), `mob_summary.py`, `status.py`, `delete_guard.py`, `decider.py`, and cards 062 and 036.
- Tests first with the fake CLIs; the loop is smoke-tested before it touches a real card.

- Phrase board and docs (read-only, absolute paths; they are NOT in the mob-defaults repo): cards in `C:\Users\ringe\projects\phrase-lang\board\`, `VISION.md` and `docs\` under `C:\Users\ringe\projects\phrase-lang\`; card numbers in this card (062, 036, 063 to 072) refer to those files.

## Task force

- Tooling author [author]: Grok | why: next in the rotation for tooling (D15)
- Reviewer [reviewer]: Codex | why: non-author review of the diff (D15)
- Safety reviewer: Grok | why: checks deletes, kills, locks and spend against the Safety rules
- Facilitator: Claude | why: runs the smoke run and reviews; any takeover is logged

## Done when

Run from the mob-defaults repo root (the mob has no board of its own; this card lives on Phrase's board):

```
python -m unittest tests.test_loop_run
python -m unittest discover -s tests
```
