# Card Done and retro

When: after the card's last commit is pushed, before the next card starts.

- Flow numbers: commit-to-live median and p90, waits, minutes lost by cause. Gate: `mob_process.py pushed-retros` (the metrics are present). Evidence: the retro's flow metrics. Card: #41
- The four areas (individual, team, organization, product) each say something real about this card. Gate: `mob_process.py pushed-retros`
- The constraint is named from data, with the focusing step applied and its measured effect. Evidence: the retro's Constraint line. Card: #17
- Journey walks: the persona's journey walked end to end on the live product. Evidence: the committed walk notes.
- Trust rungs are reviewed with evidence. Evidence: the retro's review of each seat's rung against its lines in `.mob/records/trust-ladder.jsonl`.
- The Model allocation review: per seat, the declared against actual service and model or size, the outcome signals (first-pass review verdict, blocking findings caught or missed, rework cycles, timeouts or crashes, wall time, quota used where readable) and a keep, upsize or downsize call. Evidence: the retro's Model allocation review section and the seat log rows for the card. Card: #52
- The model-selection guide is updated only by Decider (the Decider item below applies): each row a hypothesis with a metric, a counter-metric, a sample size and an expiry. Evidence: the Decider record that adopted or revised the guide, or the retro's line that no row changed. Card: #52
- One Decider pick, with its tally pasted (or `none: <reason>`). Gate: `mob_process.py pushed-retros` (a tally is required only when the retro cites the Decider). Evidence: the tally in the retro. Card: #23
- The inbox is read, with inbox_open.py's own result line. Gate: `mob_process.py pushed-retros`
- Every open finding is addressed. Gate: `mob_process.py record`
- Keep or remove each member added mid-card; removal happens only here. Gate: `mob_process.py record`
- The value re-rank is posted, with a reason per card. Evidence: the posted ranked list. Card: #33
- A template card is filed for any change that isn't product-specific. Evidence: the Mob Board card linked from the retro.
- Any Decider run at the retro follows `checklists/decider.md`. Evidence: the Decider record.
