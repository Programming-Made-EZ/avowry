# Decider log moved

The append-only decision history is now split by area so readers can load one concern at a time.

Start with [the decision index](INDEX.md), which links every decision to its unchanged record. This path remains as a stable landing page for older references to `docs/decisions/DECIDERS.md`.

## How records are written

Durable copy of Decider runs. Per-card MOB.md files are gitignored, so results that outlive a card are kept here.
Format: `Decider D<n>: <proposal>`, votes, `Result: passed|failed`. All-Yes results may be a single line.
A new record is appended to its area file and gets one entry in `areas.json`.

To find a decision by number (for example D86 or D21), open INDEX.md: its table lists every number with its area file and link.
