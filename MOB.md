**Usable v1: 5 of 6 criteria met (bar 6 suspended until the docs re-freeze, card 162) · held card: (195)**

<!-- mob-state: {"as_of": "1ea38a7aa930", "generated_at": "2026-10-29T20:33:48Z", "generator": "interim (hand-run; replaced by scripts/mob_process.py)", "held_card": 195, "process": "restoring", "schema": 1, "usable_v1": {"met": 5, "of": 6, "suspended_bars": [6]}} -->

# Mob status

**Project:** Avowry &nbsp;|&nbsp; **State:** process restoration in progress
**Held:** card 195 (Codex finished its run; the result is not imported until the process is restored)

## Process lapse, stated plainly

From 2026-10-26 16:36 (card 063) to 2026-10-29 the mob did not run cards in card mode with role rotation, did not refresh this page and did not hold a retro after each Done card. About 70 cards were closed that way, 18 of them after the Usable v1 retro. What was done instead: a rotation author in a throwaway clone, an import by the facilitator, one fresh-context review and a Decider record per card, and a CI check. The full account is in the product owner escalation reply of 2026-10-29.

Restoration order: this page, then checks that run in CI outside the facilitator (page freshness, a retro for every Done card, a role-rotation trail in `.mob/records/` for every Done card), then one catch-up retro, then card mode resumes.

## Recently Done (rotation author, fresh-context review, Decider record; not card mode)

- Card 199: the avow keyword with optional filler words and the .avowry extension, one mechanical commit (D131)
- Card 198: the repository is renamed to avowry (D127)
- Card 194: the compiler project and namespace are Avowry.Compiler (D133)
- Card 193: generated code uses Avowry names (D132)
- Card 191: the command is avowry; the map file is Module.avowrymap.json (D130)
- Card 190: the product is called Avowry in user-facing documents (D129)
- Card 175: four more edge-case fixtures (D128)
- Card 174: the edge-case fixture corpus (D123)
- Card 183: Apache-2.0 licence files and trademark policy (D124)

## What is next

Rename cards 195 to 197 are held; then card 162 (the single docs re-freeze with the LLM check rerun) and the v0.1 release lane. See DRIVERS.md for which models are available.
