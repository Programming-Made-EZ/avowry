# Loop: run every card atomically (land or discard)

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** tooling (thin slice; the code lives in mob-defaults; authored by the driver rotation and reviewed by a non-author under D15, with the smoke run)
- **Milestone:** process: the loop runner (retro seed P1: no turn ends with nothing running)
- **Source:** Steve, 2026-10-26 (mob-defaults is his product): "it should be a loop." The continuous loop must live in the tooling, not in the facilitator's chat turns; three idle gaps today all came from the loop depending on a chat turn (retro seed P1). Team design: Decider D23 (Codex Yes, Grok Yes, Copilot Support-it with changes; agy no response).
- **Depends on:** 062, 067
- **Story:** As the mob (and Steve), I want the loop to live in `loop.py` in mob-defaults, so that the work keeps going without a chat turn and stalls cannot come from an idle facilitator.
- **One observable behaviour:** The loop runs each card with `--atomic` so every attempt ends landed or discarded and a retry starts from clean main.
- **Done checks (at most 3, each failing on partial work):**
  1. The loop passes `--atomic` and treats a discarded attempt as a normal retry input (never reusing the old log)
  2. A landed card advances the picker; a discarded one follows the failure policy
  3. The whole-loop smoke run on a toy board with two toy cards passes with real CLIs

## Context

- pairs the loop with experiment E3 (card 062).
- Design (D23): `loop.py` is a thin supervisor over card mode; state in the gitignored `.mob/loop/`; Claude supervises and reviews, it is not the engine. Safety rules apply in full (delete guard, no kill by name, no Co-Authored-By, never stage `.mob/`, sandboxed Deciders, spend ceilings per card and per day).
- Read first: the mob-defaults `README.md`, `TEAM_WORKING_AGREEMENT.md`, `facilitator.py` (card mode, exit codes), `mob_summary.py`, `status.py`, `delete_guard.py`, `decider.py`, and cards 062 and 036.
- Tests first with the fake CLIs; the loop is smoke-tested before it touches a real card.

## Task force

- Tooling author [author]: Copilot | why: next in the rotation for tooling (D15)
- Reviewer [reviewer]: Grok | why: non-author review of the diff (D15)
- Safety reviewer: Grok | why: checks deletes, kills, locks and spend against the Safety rules
- Facilitator: Claude | why: runs the smoke run and reviews; any takeover is logged

## Done when

Run from the mob-defaults repo root (the mob has no board of its own; this card lives on Phrase's board):

```
python -m unittest tests.test_loop_atomic
python -m unittest discover -s tests
python smoke.py
```
