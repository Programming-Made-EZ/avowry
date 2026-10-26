# Loop: on Done run the retro step and re-rank, then the next card

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** tooling (thin slice; the code lives in mob-defaults; authored by the driver rotation and reviewed by a non-author under D15, with the smoke run)
- **Amended:** Steve, 2026-10-26 (simplification): "during every retro we have the product category where new features should be identified." So there is no separate discovery step: the loop is card, retro (all four categories; the product category produces the new cards), rank, next card.
- **Milestone:** process: the loop runner (retro seed P1: no turn ends with nothing running)
- **Source:** Steve, 2026-10-26 (mob-defaults is his product): "it should be a loop." The continuous loop must live in the tooling, not in the facilitator's chat turns; three idle gaps today all came from the loop depending on a chat turn (retro seed P1). Team design: Decider D23 (Codex Yes, Grok Yes, Copilot Support-it with changes; agy no response).
- **Depends on:** 066
- **Story:** As the mob (and Steve), I want the loop to live in `loop.py` in mob-defaults, so that the work keeps going without a chat turn and stalls cannot come from an idle facilitator.
- **One observable behaviour:** When a card is Done the loop runs the retro step and the re-rank hook exactly once and then picks the next card; an empty Ready list stops it cleanly.
- **Done checks (at most 3, each failing on partial work):**
  1. Exit 0 runs `flow_metrics.py` and writes a retro proposal, then the re-rank hook (a sandboxed Decider; the board changes only by a passed Decider's patch)
  2. A crash between phases never duplicates the retro or the Decider for the same run id
  3. The retro covers all four categories, and its product category (with LLM-customer feedback from every model that answers) is where new features are identified: its proposals become cards only through a passed sandboxed Decider (PM and domain expert refine, then rank) and must meet the thin-slice rules. An empty Ready list is a SIGNAL that the product category is under-producing: the loop records it as an input line for the next retro and keeps going (heartbeat, backoff); there is no special discovery mode and the loop never ends because Ready is empty

## Context

- the Done path; the retro stays the team's and the board stays the single source of truth.
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
python -m unittest tests.test_loop_done
python -m unittest discover -s tests
```
