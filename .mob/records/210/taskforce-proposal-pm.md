# Card 210 task-force declaration proposal

This card fixes MD-003 in mob-defaults by hardening the record checker in `mob_process.py`. The team rotates by least-recently-used as required: Copilot, agy and Claude are all in the rotation; build seats remain on command-reader models (Claude and Copilot), per window 228-agy-seats. The current 11:38 UTC probe shows Copilot and agy available; Codex and Grok have no quota.

## Team
- Product manager (ready): Copilot, ctx-1. Why: defines the card, names the corrective, and keeps the task force and record-check gates honest. Context: ctx-1.
- Architect (slice): Claude, ctx-2. Why: owns the schema and logic in `check_record`, including the three acceptance criteria and the evidence boundary; this card is a checker change, not product logic. Context: ctx-2.
- QA engineer (red): Claude, ctx-3. Why: writes the red tests for empty task force, empty probes, bad cycle derivation, and missing member metadata. Context: ctx-3.
- Backend developer (green): Claude, ctx-5. Why: fixes the checker so each red test passes without weakening the gate. Context: ctx-5.
- Independent reviewer (ship): Copilot, ctx-4. Why: owns the pre-ship review on a command-reader model; agy remains out because it has no command reader. Context: ctx-4.
- Product manager (sign-off): Claude, ctx-6. Why: fresh-context sign-off after the green and before ship. Context: ctx-6.

## Left out
- UX designer and frontend developer: no UI.
- Database developer: no persistence change.
- Security reviewer: no new trust boundary beyond record validation.
- DevOps: no workflow or hook change.
- Technical writer: no user docs.
- Language/grammar designers: no language change.
- Performance engineer: no hot path.
- agy: off the build team because it has no command reader and is only valid as a decision-only seat under 228-agy-seats.

## Domain-expert question (T2)
What is the real command source per backend, and does a no-source seat fail closed or merely get recorded as a gap? This is the T2 because the task force must not trust empty or self-reported fields when the checker is reading a record.

## Value, metric, counter-metric
- Value: close the MD-003 loophole that allowed empty `task_force` / `probes` and self-reported cycles to pass, before later cards inherit a bad record.
- Metric: the number of red tests for MD-003 that pass in one green run, including all four model probes, cycle derivation from evidence, and per-member record completeness.
- Counter-metric: re-run churn and false refusals; a good outcome stays inside the written cap and does not need a red past a cap without a scope window.

## Done checks (D157)
1. Non-empty task force and probes. Partial work fails because `task_force: []` or `probes: {}` still counts as truthy, or any non-Claude model is missing a probe timestamp. Cap: `Cap: criterion 1, at most 1 cycle`.
2. Cycles are evidence-derived. Partial work fails when the record's `cycles` field disagrees with the seat log or commits, or when no evidence exists. Cap: `Cap: criterion 2, at most 1 cycle`.
3. Per-member completeness and retro structure. Partial work fails when a member lacks domain/why/context, the roster is boilerplate, a Claude seat lacks `quota_reason`, or the retro uses truthy placeholders instead of real `flow_metrics` and `practice_metrics`. Cap: `Cap: criterion 3, at most 1 cycle`.

The total cap across all criteria is three, matching D175. A scope window is required before any red starts past a cap.

## D165 to D168, D171, D175
- D165: applies; the intent line goes before the first red.
- D166: applies only if the slice depends on an assumption line for the checker change.
- D167: applies; the slice must list hostile forms and label each as SEEN or INSURANCE.
- D168: applies; the slice must stay under the 15 KB cap and be rejected if it omits required size and scope lines.
- D171: applies for any vote or re-run window tied to the task-force decision.
- D175: applies directly; each criterion must carry a `Cap: criterion n, at most k cycles` line, the caps sum to at most three, hostile forms are labeled SEEN or INSURANCE, and a scope window precedes any red beyond a cap.

## Open points
- Confirm whether a no-source seat is a fail-closed block or a gap that is recorded until a source is found.
- Confirm the exact fresh-probe boundary and whether the record checker rejects stale probes before the first red.
- Confirm whether agy can sit as a pure decision-only member in this window without violating the build-seat rule.
