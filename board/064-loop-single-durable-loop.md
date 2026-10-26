# Loop: one durable loop at a time (lock, state, heartbeat)

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** tooling (thin slice; the code lives in mob-defaults; authored by the driver rotation and reviewed by a non-author under D15, with the smoke run)
- **Milestone:** process: the loop runner (retro seed P1: no turn ends with nothing running)
- **Source:** Steve, 2026-10-26 (mob-defaults is his product): "it should be a loop." The continuous loop must live in the tooling, not in the facilitator's chat turns; three idle gaps today all came from the loop depending on a chat turn (retro seed P1). Team design: Decider D23 (Codex Yes, Grok Yes, Copilot Support-it with changes; agy no response).
- **Depends on:** 063
- **Story:** As the mob (and Steve), I want the loop to live in `loop.py` in mob-defaults, so that the work keeps going without a chat turn and stalls cannot come from an idle facilitator.
- **One observable behaviour:** Starting a second loop is refused, and an interrupted loop resumes its recorded phase exactly once.
- **Done checks (at most 3, each failing on partial work):**
  1. A second `loop.py` exits 2 without writing anything while the OS file lock is held; the lock dies with the process, and a stale PID file is never cleared by killing anything
  2. `.mob/loop/state.json` (phase, card id, run id, attempt, last exit) is replaced atomically and written before each attempt starts; a crash mid-phase resumes that phase once
  3. `.mob/loop/heartbeat.json` (pid, phase, timestamp) is refreshed on a timer while a child runs

## Context

- loop state under the main checkout's gitignored `.mob/loop/`, outside any card worktree.
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
python -m unittest tests.test_loop_state
python -m unittest discover -s tests
```
