# Card 229 task-force declaration proposal (ctx-1, PM ready)

## Fold decision (229 + 270)

**Fold card 270 into card 229 at ready.**  
Reason: both cards share one reproduced root cause and one launch path (`facilitator._launch_backend` / `run_backend` to `.CMD` shims; cmd.exe >8191 chars fails before CLI starts), and both are amplified by the same decider behavior (`decider.py` around line 119 dropping failed-call text into "no response").

If folded: close **#270** as **Folded into #229 (one root cause, one fix path)** when #229 ships; #229 carries all #270 Done checks:
1. Cause named with failing reproduction.
2. Grok decision seat returns a parseable vote on a real window prompt.
3. Empty reply records exit status + stderr/failure excerpt (not bare "no response").

## Roles, models, and contexts

1. **PM (ready), model: Claude sonnet, context: ctx-1** — owns declaration, value metric, and D182 ordering compliance.
2. **Architect, model: Claude sonnet, context: ctx-2** — owns launch-path design choice (stdin/prompt-file/real entrypoint) and invariant checks.
3. **QA, model: Claude sonnet, context: ctx-3** — owns red-first guard: empty attempt fails (not counted), and counted votes keep valid D171 cites.
4. **Backend dev, model: Claude sonnet, context: ctx-4** — owns facilitator/decider implementation details and backend-specific wiring.
5. **Audit lens, model: Claude sonnet, context: ctx-5** — owns evidence trail quality (failure text, seat-use lines, diagnosis traceability).
6. **Decider, model: Claude sonnet, context: ctx-6** — owns vote-window validity and rule conformance on counted votes.
7. **Agile coach, model: Claude sonnet, context: ctx-7** — owns D175/D181 cap discipline and anti-theatre checks.

Left out of counted decision seats for this card: **Copilot and Grok** until this fix lands (known empty-vote launch failure, not quota/unavailability); **Muse/agy** not in this root-cause path.

## Domain-expert (T2) question

For each decision CLI backend (copilot, grok), what is the supported and stable long-prompt input path on Windows (**stdin**, **prompt file**, or **non-.CMD real entrypoint**) that preserves full prompt bytes and returns non-zero + stderr on launch failure?

## Value, metric, counter-metric

- **Value:** restore real multi-model decision voting and remove empty-seat tax.
- **Primary metric:** empty or voteless counted attempts per decision window (baseline observed: about two per seat; target: near 0).
- **Counter-metrics:** (a) every counted vote still has a valid D171 cite, (b) window minutes do not regress, (c) diagnosis records keep launch-failure evidence.

## Done checks with D175 caps (sum <= 3)

1. **Launch path correctness (Cap 1):** prompts above cmd.exe limit still reach Copilot/Grok decision seats whole (or are refused pre-seat with named reason) on real window-shaped prompts.
2. **Failure honesty + empty guard (Cap 1):** failed launches persist status + failure text in vote records; empty attempts fail red and are not counted.
3. **Neighbor consistency (Cap 1):** placeholder literals in prompts are not rewritten by launcher logic (card 282 coupling), while template placeholders still fill.

**Total cap: 3.**

## Applicable Deciders

- **D165:** apply (intent line before first red).
- **D166:** apply (assumption/probe lines: Windows shim path and prompt-delivery assumptions).
- **D167:** apply (hostile inputs list for backend/model maps and placeholder literals).
- **D168:** apply (slice size cap).
- **D171:** apply (counted-vote cite validity guard).
- **D175:** apply (written per-check caps; total <=3).
- **D176:** apply to gate evidence reporting for environment-sensitive failures.
- **D178:** apply as the diagnosis/exclusion boundary this fix must satisfy.
- **D181:** apply (no post-green lettered-red cap bypass).
- **D182:** apply (ordering: 229/270, then 282, then 277; QA red-first empty guard condition).

## Open points

1. Backend-by-backend decision on **stdin vs prompt-file vs direct non-.CMD entrypoint** for Copilot and Grok.
2. Whether each CLI reliably accepts that path in current installed versions.
3. Exact pre-seat refusal rule when full prompt cannot be passed (must fail before seat starts, with named reason).
4. Vote-record schema location for failure evidence (exit code + stderr/failure excerpt) so diagnosis in 277 can consume it directly.
