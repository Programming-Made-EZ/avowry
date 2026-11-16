# Proposal for Decider D272: mob-defaults #61's green seat moves from cursor gpt-5.3-codex to muse (medium effort) for the rest of the card

Facilitator, 2026-11-16 UTC. VOTE on this mid-card model change. The owner's 2026-11-14 ruling sends a change of model mid-card through a Decider.

EVIDENCE: the fix-round-1 green (cursor `--model gpt-5.3-codex`, ctx-3b) stopped at 00:50:34 UTC with "ActionRequiredError: You've hit your usage limit ... Your usage limits will reset when your monthly cycle ends on 12/14/2026". It left a partial change in wt-md61:
- the 112 tests in the card's own file pass;
- 18 existing tests fail. Four are real regressions: old-format rows must rotate exactly as before (scenario [Z]). Fourteen are `test_decider_launched` tests that count one seat-use line per voter, which the card's outcome lines now intentionally exceed.

The partial change is kept uncommitted for the new seat to finish. No cursor process is running (D267).

THE CHANGE: the green seat for the rest of mob-defaults #61 is **muse**, launched through `muse_write.py` with `--effort medium` (60 steps, 20,000 tool-output bytes), in the existing linked worktree wt-md61, context ctx-3c.
- **Why muse:** it is a writing seat, it probed ok at 2026-11-16 00:05 UTC and answered as a retro voice, and the owner's rotation-pool ruling asks for it to take seats and relieve agy and claude.
- **The other options:** cursor with another model would change service and model at once and its model cannot be confirmed (F-13). Claude is already the red, QA and sign-off service.

The green brief carries the facilitator's rulings for the 18 tests:
- the [Z] regressions are fixed in code;
- the 14 line-count tests are changed only to count launch lines, the lines without `event: outcome`, with their assertions otherwise unchanged.

CLAUSES:
1. Seat muse (medium effort) as #61's green for the rest of the card, in ctx-3c.
2. Record cursor gpt-5.3-codex as out until 2026-12-14 in the seat plan and the next probes. Cursor may still be probed with other models, but each one is a new declaration.
