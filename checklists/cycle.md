# Cycle (red, green, refactor)

When: at every cycle boundary, before the next cycle starts. A seat in a cycle reads this file, and `checklists/decider.md` when a Decider runs.

- Red proven on the parent commit: the new test fails there for the right reason. Evidence: the committed raw red output. Card: #14
- New and changed tests carry Given/When/Then comments. Evidence: the diff of the test files. Card: #43
- Each claim the seat made is re-run as its trust rung requires. Evidence: the re-run commands and their raw output, beside the seat's rung in `.mob/records/trust-ladder.jsonl`.
- The cycle's commits are pushed. Gate: `mob_process.py unpushed`
- A commit held back names its hold reason in MOB.md (the unpushed gate keeps refusing it until it is pushed). Evidence: the hold line in MOB.md.
- The domain-expert question is asked again and answered ("should a role with different domain knowledge join?"). Gate: `mob_process.py record`
- Any mid-card task-force addition or removal is recorded with its Decider number; a removal reads `removed: retro`. Gate: `facilitator.py` (card mode). Gate: `mob_process.py record`
- Any Decider run at this boundary follows `checklists/decider.md`. Evidence: the Decider record.
