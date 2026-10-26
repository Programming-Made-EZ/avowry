# Atomic runs: a card runs in a fresh worktree and ends landed or discarded

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** tooling (thin slice; the code lives in mob-defaults, `facilitator.py`; authored by the driver rotation under D15, reviewed by a non-author, smoke-tested on a toy card)
- **Milestone:** process experiment E3 (land or discard), under trial; not on the Usable v1 path
- **Source:** Steve, 2026-10-26: "it feels like work should be done atomistically. that way it gets an all or nothing result. the smaller and more atomistic the better. this makes retries easier and more distributable." Then, on Bishop's Law: "it still fits with Bishop's Law. For parallelism to work efficiently the work must be atomistic. Once dependencies are introduced true parallelism cannot be achieved." Decider D22 (Codex, Copilot, Grok Support-it, all with changes adopted here; agy no response).
- **Depends on:** none
- **Story:** As the mob, I want each card run in its own fresh worktree from current main that ends either LANDED (one commit on main) or DISCARDED (nothing left behind), so that a retry is always clean and independent tasks can later run in parallel.
- **One observable behaviour:** `facilitator.py --card ... --atomic` runs the card in a linked worktree and ends in exactly one of two states: one commit fast-forwarded onto main, or the worktree removed with no stash, no untracked file and no changed main.
- **Done checks (at most 3, each failing on partial work):**
  1. A card that reaches Done lands as exactly one commit on main (fast-forward, gate run after the merge); a card that does not is DISCARDED: the linked worktree is gone, `git status` and `git stash list` in the main checkout are unchanged (tested with the fake CLIs)
  2. A retry after DISCARD starts from the current main with a new log, never the previous attempt's log (the failure evidence is copied to a facilitator log outside the worktree first); a merge conflict ends the attempt DISCARDED
  3. Worktree removal goes through `git worktree remove` on a path that `git worktree list` shows as a linked worktree, and `safe_delete` only for a strict child of a registered scratch root; the main worktree, `.git`, home and drive roots are refused; the real-CLI smoke run passes

## Context

- Design notes from the D22 review (Codex, Copilot, Grok): stop that task's own PIDs before removal; never touch git stashes (the labeled Copilot voter stash stays until the Decider required before 039); each worktree keeps its own bin and obj while the global NuGet cache is shared; restore and `dotnet test` run as a harness job outside the driver turn so a retry does not spend the cycle cap on restore; merges are one fast-forward at a time then one gate run, rebased and re-run immediately before merge; LANDED means checks pass, a non-author reviewed, one commit on main.
- Trial starts at the next task boundary from current main (card 018 is in flight in place and finishes as it is). Parallel running of independent cards is a later step, after the first three thin cards show what sequential clean retries buy (metrics: retries per task, partial-state incidents, time to land; counter-metric: merge conflicts and resync overhead).
- The agreement text (TEAM_WORKING_AGREEMENT.md, Principles) already records the wording; this card builds the tool. Tests first, with the fake CLIs; the real-CLI smoke run (`smoke.py`) before it touches a real card.

## Task force

- Tooling author [author]: Codex | why: next in the rotation for tooling (D15); writes it test-first in mob-defaults
- Reviewer [reviewer]: Copilot | why: non-author review of the diff (D15)
- Safety reviewer: Grok | why: reviews the delete path against the Safety rules and the delete guard
- Facilitator: Claude | why: runs the smoke run and reviews; any takeover is logged

## Done when

Run from the mob-defaults repo root (the mob has no board of its own; this card lives on Phrase's board):

```
python -m unittest tests.test_atomic_run
python -m unittest discover -s tests
python smoke.py
```
