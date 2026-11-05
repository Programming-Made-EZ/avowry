# Retro pick proposal (agile coach seat): every number a slice states carries the command that produced it

Card 264 retro, 2026-11-05, revision 1. Proposer: agile coach seat. For a Decider. Sources: .mob/records/264/ (retro-voices.json, retro-input.md, signoff-ctx6.md, preship-gate.md, the windows 264-scope and 264-scope-2); D184; D186 (the broader premise probe, not adopted); the retro experiments log.

Decider (number set at recording), the card 264 retro process pick, as a working-agreement experiment (D183: not a card)

PICK: a slice states a count, size or row number only with the command and output that produced it, or marks it "unrun: <reason>". The facilitator's D158 checklist for the slice window lists each numeric claim in the slice and the line that carries its command, before the window opens; a claim with no command and no "unrun" blocks the window until the architect adds one. Proposed by five of seven voices in some form (product manager, architect, backend developer, decider, coach), as running premises in code before the first green.

User benefit: the product owner stops paying for a stopped green and a scope window when the slice's own arithmetic was never checked.

Why this one, and why it is not D186 again:
- Card 264: the slice said the class under test has four rows and asked for "one complete four-row class run"; the class has 17 rows (two four-row Theories, seven Facts and a two-row Theory), a loaded run takes about 5 minutes and 20 runs about 100 minutes. Green 1 attempt 1 found it, stopped, and window 264-scope was needed to split the criterion. One command (counting InlineData, Theory and Fact lines) would have shown it.
- D184 already asks scope proposals to run their premises on HEAD; the slice window has no such check. D186 (a measure-only probe before the slice, 15 minutes, a typed premise table covering every premise) was not adopted after three votes. This pick drops the table, the probe and the time bound: it covers only numeric claims, which a command answers in seconds, and adds one line to a checklist the facilitator already writes.

HYPOTHESIS: We believe a numeric claim that carries its command cannot be wrong unseen. We will know when green stops and scope windows caused by a wrong count, size or row number fall from 1 stop and 1 window on card 264 to 0 on the next three bug cards.

RULE UNDER TRIAL:
C1. In the slice, each numeric claim (a count of rows, tests, files, call sites, a size, a duration) is followed by the command and its output lines run on committed HEAD, or by "unrun: <reason>" and what the green does if the number is false.
C2. The facilitator's D158 slice checklist lists the numeric claims it found and, for each, where its command is. A missing line blocks the window; the architect adds it and the checklist is committed again before the window.
C3. Not changed: D175 caps, D181, D184 for scope proposals, the objection window, the seats. No new seat, file, meeting or time bound.

METRIC: green stops and scope windows traced to a wrong numeric claim (card 264: 1 green stop and 1 scope window, the criterion split; the second scope window, for the unfixed build collision, came from a finding and is not counted). Target: 0 on the next 3 bug cards.

COUNTER-METRICS (evidence, not targets):
1. Minutes from slice proposal to window open (card 264: about 2). A rise past 10 means ceremony.
2. Numeric claims marked "unrun" per slice. More than two means the rule became a loophole.
3. Claims found wrong at the checklist before the window (a count above 0 shows the rule works).

TRIAL SCOPE: the next 3 bug cards. Its retro decides keep, revise or drop.

COST: one checklist line and a few commands per slice; no new file.

REVERSIBILITY: two-way door. Drop the checklist line and the slice lines.

Deferred (D183 retro log): a line-ending check in the pre-commit hook (existing files keep their line endings; four voices); a hard timeout and process kill in the probe script, and refusing a probe line older than the file's last line (three hung probes; the late stale commit); a "scope window" category and an aggregate cap check in the window records (the record used "mid-card addition" and the cap sum reached 5); an evidence-integrity gate before the pre-ship gate (HEAD match, line endings, duplicate lines; QA).
