# Retro pick proposal (agile coach seat): the in-flight command refuses a seat start on a stale probe

Card 297 retro, 2026-11-05, revision 1. Proposer: agile coach seat. For a Decider. Sources: .mob/records/297/ (retro-voices.json, retro-input.md, signoff-ctx6.md, preship-gate.md); F-085; the queued item "a probe-age alert before an in-flight line" (docs/retros/experiments.md, two crossings on card 229).

Decider (number set at recording), the card 297 retro process pick, as a working-agreement experiment (D183: not a card)

PICK: the facilitator's in-flight command (inflight.sh) exits with an error and writes no in-flight line when the newest probe in .mob/records/probes/ is older than 50 minutes (F-085). No seat starts without an in-flight line, so no seat starts on a stale probe. Proposed by six of seven voices (all but the architect, whose reply was cut off by the Codex shell error); the product manager, QA, developer and coach asked for the check to be mechanical.

User benefit: the product owner's cards stop running red and green seats on a driver state nobody re-checked; a Codex seat that has gone unavailable shows up before it wastes a seat, not at the retro.

Why this one:
- Card 297 evidence: the in-flight lines for the seats after 05:17 UTC record last-probe ages of 66, 70, 72, 77, 80, 86, 112 and 145 minutes. Every voice rated F-085 "no" or "partly". The hand-run facilitator is the only path that skips the age check the seat launcher already applies at 60 minutes to command probes; the in-flight line only records the age.
- A reminder did not hold on card 229 (two crossings) or card 297 (eight). Making the line impossible to write without a fresh probe moves the rule from memory to the tool.

HYPOTHESIS: We believe a refusal at the in-flight command keeps every seat inside the 50-minute rule. We will know when the share of seats started on a probe older than 50 minutes falls from 8 of 14 (card 297: the in-flight lines of its 14 seat starts) to 0.

RULE UNDER TRIAL:
C1. inflight.sh reads the newest probe time as it does now; at an age above 50 minutes it prints "PROBE TOO OLD (N min): run probe_drivers.py first" and exits 1 before writing, committing or pushing anything.
C2. The product owner or the facilitator may pass an explicit override variable (INFLIGHT_STALE_OK=1) for a seat that needs no driver (a read-only reply from a subagent); the line then carries "stale probe override: <reason>". The override is counted.
C3. Not changed: the probe cadence itself, F-085's 50 minutes, D175 caps, the seat launcher's own 60-minute freshness for command probes.

METRIC: seats started on a probe older than 50 minutes, from the in-flight lines (card 297: 8 of 14). Target: 0 without override.

COUNTER-METRICS (evidence, not targets):
1. Probes per card (a probe costs model calls and time). A rise past one per 50 minutes of seat time means ceremony.
2. Overrides used. More than one per card means C2 became the path of least resistance.
3. Minutes added between seats (the probe wait). A probe took about 1 to 2 minutes here, longer when the machine is busy.

TRIAL SCOPE: the next 3 cards. Its retro decides keep, revise or drop.

COST: one edit to the facilitator's script and one line in the working agreement. No new seat, file or meeting.

REVERSIBILITY: two-way door. Remove the check.

Deferred (D183 retro log): the reboot-resilient watchdog and monitor handling (a Monitor on a shared output file reported a false completion); an integration-test voice for driver, probe, placement, command and commit together (card 297 sign-off condition 1); dropping one voting seat on a bug-fix card of this size (the coach's trial idea; trigger: a bug card with no condition raised).
