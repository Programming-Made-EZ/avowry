# Loop: resume after a crash or reboot (logon task)

- **Status:** Ready (rank: see `C:\Users\ringe\projects\phrase-lang\board\MILESTONE-usable-v1.md`, read-only)
- **Type:** tooling (thin slice; the code lives in mob-defaults; authored by the driver rotation and reviewed by a non-author under D15, with the smoke run)
- **Milestone:** process: the loop runner (retro seed P1: no turn ends with nothing running)
- **Source:** Steve, 2026-10-26 (mob-defaults is his product): "it should be a loop." The continuous loop must live in the tooling, not in the facilitator's chat turns; three idle gaps today all came from the loop depending on a chat turn (retro seed P1). Team design: Decider D23 (Codex Yes, Grok Yes, Copilot Support-it with changes; agy no response).
- **Depends on:** 068
- **Story:** As the mob (and Steve), I want the loop to live in `loop.py` in mob-defaults, so that the work keeps going without a chat turn and stalls cannot come from an idle facilitator.
- **One observable behaviour:** `loop.py --resume` continues cleanly after a crash or reboot: a live lock exits 0, a free lock continues the recorded phase, and a running card continues its log without `--fresh`.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then `--resume` with a live lock exits 0 without starting anything; with a free lock it continues the phase in state.json (tested by killing a loop child process by its stored PID, never by name)
  2. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then a worktree already gone (atomic mode) counts as a discarded attempt and the loop picks again from main
  3. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then the logon-task installer is written but is NOT run by the tool: registering it is Steve's one-word ask (a persistent system change); the real-CLI smoke run passes

## Context

- unattended restart. Registering a Windows logon task changes system configuration, so it needs Steve's explicit one-word yes.
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
python -m unittest tests.test_loop_resume
python -m unittest discover -s tests
python smoke.py
```
