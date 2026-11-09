# Ready record, card 227 (run baseline-ready-1)

Seat: ready, product manager (ctx-1). Card 227, Contract, treated as open at ready. Source: `board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md`. I used the card's story-side text (shape, actor, observes, invariants, Done checks, Not in this card). I did not use its Done status or the lines written after ready (adopted window condition, post-sign-off ruling).

## Rules and pages relied on

All in `TEAM_WORKING_AGREEMENT.md`:
- Task-Force Driven Development, and Practices, "Task force before every card" (declare up front, sized to the task; left-out roles with a reason; domain-expert question never blank; no removals mid-card; each role has assignee, domain or context, why).
- Practices, "Board and cards": Card shapes (Contract) and Definition of Ready; Thin slices (D136).
- Ceremonies: Three Amigos (D150), lighter path for routine per-card decisions (D153, Decider protocol section), Intent line (D165), Assumption line (D166), Hostile-input list (D167), one criterion per cycle (D157), facilitator pre-window checklist (D158), slice size cap (D168).
- "Staffing: roles over models" (quota first, fresh probes, every Claude seat needs a recorded reason).
- "Metrics are evidence, not targets", "Process ideas are hypotheses", Bootstrap phase (no metric-based rejection).
- Safety (literal-path delete rule, detectors and the scan's limits); Practices "Push every commit (F-071)".
- Escalation (nothing here escalates).

## Ready check (Definition of Ready, Contract shape)

- Shape: Contract. Actor is a system actor (`delete_guard.find_unsafe` and the unpushed-commit detector), not a person. PASS.
- Observes is an outcome the consumer can tell from a wrong result (a finding naming command and seat; a FAIL naming repository, commit, age). Not a bare success. PASS.
- No orphan: Enables card 216 and the closing of the audit 62 failure. A Contract ranks behind the Story it enables unless that is Done; it is a stop-the-line corrective, so the re-rank places it.
- `Depends on: none` is present. No unverified external dependency (git only, no gh calls).
- Hypothesis: a corrective tooling card is exempt as a bug/plumbing card. It names the bar it advances: the deletion-ban rule in force by detector, not habit ("a rule without a detector isn't in force").
- Thin-slice check (the PM rejects oversized cards at ready): three Done checks = at most three criteria, OK. Concern count is the risk: scan fix and push detector are arguably two concerns. I do not reject the card: the product owner ruled one corrective card for both. The architect's SHIP AS IS or SPLIT decision at slice is mandatory and must address this explicitly; I lean SPLIT only if the slice would exceed 15 KB, 15 files or 400 lines (D168, D136).
- Done checks 1 and 3 say "ZOMBIES to be written by QA at ready". The agreement puts that in the Three Amigos step before red (PM, QA, developer, each in own context). The acceptance criteria are therefore not written in this record; the step is owed.

## Task force declared (objection window under D153, not a full Decider: this is neither a one-way door nor a standing-rule change)

Assignees are Claude contexts, each a separate fresh context. Reason for Claude on every seat: Codex, Copilot, Grok and agy must be re-probed (probe under 60 minutes old, evidence under `.mob/records/probes/`) before assignment, and any non-Claude model with usable quota takes a seat first. If probes show none has usable quota, the recorded reason is "no non-Claude model has usable quota". A Claude seat without a recorded reason is an audit FAIL.

| Role | Domain or context | Assignee | Why |
|---|---|---|---|
| Product manager (ready, signoff) | Product value, DoR, metrics | Claude ctx-1 (ready); a different fresh context for signoff | Ready check, value and metrics, final user-observable acceptance |
| Architect (slice) | Detector design, scan architecture, ship-or-split | Claude ctx-2 | Numbers the criteria, writes the Assumes, Inputs and Size lines, decides SHIP AS IS or SPLIT |
| Security reviewer, adversarial lens [reviewer] | Destructive-command evasion (quoting, encoding, wrappers, PowerShell and cmd forms) | Claude ctx-3 | Hard safety item; the authors of the patterns cannot be the only ones trying to defeat them |
| QA engineer (red) | ZOMBIES derivation, reconstructing the slip from evidence | Claude ctx-4 | Owns the criteria and the failing tests |
| Backend developer [author] (green, refactor) | Python tooling in mob-defaults (`delete_guard.py`, `mob_process.py`) | Claude ctx-5 | Implements; never reviews |
| Independent reviewer, audit lens [reviewer] | Audit findings F-071 and audit 62 | Claude ctx-6 | Non-author review of each cycle against the audit failure |
| Delivery lead (ship) | Push state, CI, pre-ship evidence gate (D154) | Claude ctx-7 | Pushed hashes, record check, this card's own push rule |

No seat is both [author] and [reviewer].

## Left out, with a reason each

- UX designer, frontend developer: no user interface.
- Database developer: no database.
- Technical writer: the rule text is two short edits, covered by the developer and checked by the independent reviewer.
- Language designer, grammar designer: no language or grammar change.
- Performance engineer: no hot path; the scan runs after the fact.
- DevOps or runner expert: git only, no hook or workflow change. Reopen through a Decider if the slice picks a hook for command capture.
- Agile coach: this card proposes no process-change experiment.
- Enablement: no new consumer-facing doc; card 216 carries the template rollout.
- Compiler engineer, benchmark designer, LLM-ergonomics reviewer: the card touches no language or benchmark.

## Domain-expert question and answer

Question: should a member with different domain knowledge or context join now?
Answer: Yes. The security reviewer with an adversarial lens (in the table above) joins at declaration, because this is a hard safety item after the 2026-10-08 home-folder wipe and the pattern authors need someone trying to defeat the scan. No further member is proposed now. The question is asked again at every cycle boundary; a later expert joins only through a Decider with its id recorded.

## Open points to resolve at the slice

1. The exact slip is not recorded verbatim; QA reconstructs it from card 226's recorded deviation and the trust-ladder line, and the red test uses that reconstruction (state the evidence used).
2. Command source: the scan reads replies and may never see an executed command. The slice decides where commands come from (hook or transcript), what happens for a seat with no command source, and the Zero case (empty or missing source). Done check 1 requires executed commands, not only reply text.
3. Where the trust-ladder record lives (the template's trust-ladder record; confirm file and line format).
4. Which repositories the push detector watches (avowry and mob-defaults per the card; the avowry path is C:/Users/<user>/projects/phrase-lang).
5. Whether the card ships as is or splits into scan and push-detector slices (see the thin-slice check).
6. Which command forms count as "equivalents" for the hostile-input list (`rm -rf`, `rm -r`, `Remove-Item -Recurse` with and without `-Force`, `shutil.rmtree`, others), and which are named but out of reach.
7. Assumption line (D166; this card changes a rule and a checker): name the invariant the checker relies on and probe it against a real record.
8. Intent line before the first red (D165), quoting the product owner's 2026-10-31 safety ruling; the facilitator confirms it.

## Product value and metrics

Value: no seat can delete a repository by a variable-path recursive delete unseen, and unpushed work is never much older than five minutes, so a mis-quoted delete loses at most about five minutes of work.

Metrics (evidence for judgment, not targets; Goodhart guard; no metric-based rejection during the bootstrap phase):
- Unflagged recursive deletes on variable or computed paths, in replies and in executed commands, across card runs (expect 0, read as evidence).
- Commits on watched repositories unpushed for more than about 5 minutes (expect 0, read as evidence).
- Share of recorded safety deviations that have a trust-ladder line (should be all).

Counter-metric: false positives on literal scratch paths through `safe_delete`, and replies reworded to dodge the scan (gaming); also seat minutes and extra commits per card.

Judgment check: a metric at zero is not success if the scan cannot see commands. The security reviewer's evasion attempts and a cold read by a seat that did not write the patterns are the qualitative check.

Revert step (decisions stay reversible): each criterion is its own commit; reverting restores the previous scan and removes the detector call. Neither is a one-way door.

Escalation: none. This is an implementation matter the mob decides; the product owner's ruling is already on record.
Size: 9007 bytes
