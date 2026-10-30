# Input for the next retro (card 208's): the nested-loops lens (product owner and Dispatch proposal, 2026-10-30)

A lens, not a mandate; the team decides (D161 reporting rule: describe, not cite).

## The lens
The development process as nested loops, tightest first: (1) pair programming, (2) unit-level TDD, (3) outside-in acceptance TDD, (4) slice, (5) feature, (6) epic, (7) the overall development lifecycle. Proposal: every loop has an entry condition, a feedback signal, a review at its boundary, and one metric.

## What the retro does with it
- Each role, in its own context, maps the team's current process onto the seven loops from its perspective (entry, feedback, boundary review, metric per loop).
- The team identifies which loops lack any of the four, and where drift has come from.
- The mapping is recorded in the retro file.
- At most ONE experiment comes out of the mapping, with hypothesis, metric, counter-metric and trial scope (it may be the retro's process pick).
- If the team adopts the lens, it goes into the mob-defaults template through a card (product owner direction D161: lock the process into the template).

## Dispatch's candidate (input only)
The pairing loop is the thinnest: roles hand off whole steps with no live steering. Candidate: a navigator seat steers live during the green step. Metric: defects found at review, and time to green. Counter-metric: seat cost per card.
