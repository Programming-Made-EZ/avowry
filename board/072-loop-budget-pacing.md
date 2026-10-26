# Loop: pace the work against the finite driver allotments

- **Status:** Ready (rank: see `C:\Users\ringe\projects\phrase-lang\board\MILESTONE-usable-v1.md`, read-only)
- **Type:** tooling (thin slice; the code lives in mob-defaults; authored by the driver rotation and reviewed by a non-author under D15, with the smoke run)
- **Milestone:** process: the loop runner (continuous, never ends)
- **Source:** Steve, 2026-10-26: "don't forget the team can find their own features to add. so theoretically the loop is continuous and never ends." The loop stops only for a STOP file or Steve saying stop, a one-way door that needs Steve's OK, or a driver allotment running out. Part of the loop design, Decider D23 and its amendment D24.
- **Depends on:** 066
- **Story:** As the mob (and Steve), I want the loop to keep finding and doing work within the finite driver allotments, so that it is continuous and never ends for lack of cards.
- **One observable behaviour:** The loop tracks each driver's usage against its allotment, rotates away from a nearly empty driver, and pauses without ending when all are out.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then per-driver usage (estimated tokens and turns from the run logs) is recorded per turn and compared with a configured allotment per driver
  2. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then a driver near its allotment is skipped in the rotation for new turns; when every driver is out the loop pauses with a heartbeat and starts no paid turn, and resumes when the configuration says the allotment refreshed
  3. Given the loop tests with the fake CLIs and a fixture board, When the behaviour above is exercised, Then spend beyond the prepaid allotments is never started: it is a one-way door written to the Needs-Steve file; the counter-metric (cards landed per allotment) is reported in the published MOB.md

## Context

- pacing and budget-awareness as part of the design (Steve): the prepaid allotments are finite, so pacing is a counter-metric and a safety guard together.
- The loop stops ONLY for a STOP file or Steve saying stop, a one-way door that needs Steve, or a driver allotment running out (which pauses it). Safety rules apply in full.
- Read first: the mob-defaults `README.md`, `TEAM_WORKING_AGREEMENT.md`, `facilitator.py`, `flow_metrics.py`, `decider.py`, `delete_guard.py`; Phrase `VISION.md`, `docs/retros/seeds.md`, `docs/pattern-candidates-raw.md`, `bench/`; cards 063 to 070.
- Tests first with the fake CLIs; smoke-tested before it touches a real card.

- Phrase board and docs (read-only, absolute paths; they are NOT in the mob-defaults repo): cards in `C:\Users\ringe\projects\phrase-lang\board\`, `VISION.md` and `docs\` under `C:\Users\ringe\projects\phrase-lang\`; card numbers in this card (062, 036, 063 to 072) refer to those files.

## Task force

- Tooling author [author]: Codex | why: next in the rotation for tooling (D15)
- Reviewer [reviewer]: Copilot | why: non-author review of the diff (D15)
- Domain expert: Grok | why: owns the discovery sources and the refinement lenses
- Facilitator: Claude | why: runs the smoke run and reviews; any takeover is logged

## Done when

Run from the mob-defaults repo root (the mob has no board of its own; this card lives on Phrase's board):

```
python -m unittest tests.test_loop_budget_pacing
python -m unittest discover -s tests
python smoke.py
```
