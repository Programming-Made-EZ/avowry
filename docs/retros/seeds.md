# Retro seeds

Items raised mid-card, to be taken at the next retro. Newest first.

## Steve's diagnosis and Dispatch proposal P6 (2026-10-26)

- **Diagnosis (Steve):** "it just seemed like we weren't getting very far because there were a lot of mistakes and new patches to fix those." So the retro frames the cost as
  REWORK: mistakes, patches, more mistakes. The retro adds a defect and rework table to the flow numbers (every stop, reopen, revert and incident since the resolver, classified by
  root cause and counted), runs a five-whys on the largest class, and fixes the cause. Table and first analysis: `docs/retros/process-retro-prep-2026-10-26.md`.
- **P6 (Dispatch). Hold the mob tooling to the product's own standard.** Most stops traced to tooling written fast and used on real cards at once. Standard for any change to
  `facilitator.py`, prompts or the harness: test-first; reviewed by a non-author model; smoke-tested on a toy card before it touches a real one. Metric: tooling-caused stops per
  card. Counter-metric: time to land tooling changes.

## Dispatch proposals for the process retro (raw input, 2026-10-26); they compete for the ONE pick

Evidence is from Dispatch watching from outside. Per Steve's clarification the process retro keeps its form: flow numbers (now with a defect and rework table), the four categories, at least one pick (more if small and clearly worth it), the rest as seeds.
Dispatch expects P1 and P2 to matter most for throughput.

- **P1. No turn ends without the next action already running.** Every report ends with a launched background job (with its expected completion) or the next step started.
  Hypothesis: nudges from Dispatch drop to 0. Metric: idle gaps longer than 30 minutes with no job running. (Evidence: the facilitator reported twice and went idle.)
- **P2. Fit the run shape to the task.** Classify each card's slices as code, data or long-running. Long or data steps become a scripted job run by the harness outside driver
  turns, with a driver writing and reviewing the script. Metric: runs that end "cap hit, not done". (Evidence: four runs on the benchmark fix hit the 3-cycle cap because one
  four-model run outlasts a driver turn.)
- **P3. Make facilitator takeovers visible.** Log work the facilitator does itself as TAKEOVER with a reason, so the rotation share is measurable and the cause is fixed, not
  normalized. Metric: facilitator share of commits. (Interim data: 84% of code lines.)
- **P4. Auto-fresh on card change.** Start fresh automatically when the card file's hash changed since its working log was seeded (or warn). It cost three starts on the dashboard card.
- **P5. Write reports for the reader.** One plain-English "what happened / what's next" line at the top of each MOB.md update, fewer internal labels (rows, Done-when, D11, cycle
  numbers). Partly covered by Steve's headline-number rule (`Usable v1: X of 6 criteria · next card: <title>`).

## Process retro approved by Steve (2026-10-26): scope, data and how it will be run

- **When:** the next retro, after "live mob dashboard (036)" is Done. **Focus:** the mob's own working process. Evidence-based.
- **(Revised twice 2026-10-26: first 'short, one pick', then Steve's clarification: BALANCE, not removal; the retro keeps its four categories; at least one pick, more if small and clearly worth it; the rest seeds.)** Open with flow metrics for every card done so far (resolver, hardening, benchmark v0, benchmark fix, dashboard), from git and the
  working logs, using `flow_metrics.py` in mob-defaults (tests first): runs and restarts, reopenings, cycles, elapsed and idle time, estimated
  tokens, turns by seat, cycle-cap stops and ready-check stops, who drove, and the facilitator's share of the work by area. This is the process baseline.
- **Interim numbers (036 still in progress, 2026-10-26):** 001 resolver: 5 runs, 1 reopening, about 1.0M est. tokens, 105 h idle between runs (waiting for the
  toolchain); 002 hardening: 1 run, 8 turns; 016 benchmark v0: 3 runs; 032 benchmark fix: 12 runs, 2 recorded reopenings, 201 turns, about 14M est. tokens,
  7.5 h; 036 dashboard: 1 resumed run, still going. Work split since Oct 20 (lines added): drivers 2,367 (code 1,231), facilitator 17,006 (code 6,468, tests 6,337,
  docs 4,190), automation 449. Facilitator share of code lines 84%: phrase-lang 74%, mob-defaults 100% (all tooling was written by the facilitator, no tooling card was
  on a board until 036). Tokens are estimates from prompt and reply sizes; MOB.md has no timestamps, so elapsed time comes from turn-file modification times.
- **The four pain points to examine with that data:** (a) Done checks that were too weak, and whether "one seat tries to break each check" works (first applied to 036:
  three rounds, about 11 loopholes closed); (b) stalls and idleness: the facilitator went quiet after reporting twice, the 3-cycle cap fits long or data-heavy tasks badly
  (cap stops are counted; each cycle's checks on 036 take about 13 minutes), and re-runs resumed a stale log (three wasted starts on 036); (c) the facilitator doing work
  itself instead of rotating drivers: the share above, and why; (d) throughput in cards per day.
- **Proposals:** collected from all four models (in sandboxes, read-only) plus the facilitator; ONE experiment chosen by Decider with a hypothesis, a metric and
  counter-metric and a trial scope; the rest logged as seeds. Bootstrap phase: judge, do not reject on metrics.

## Open items carried to named moments (2026-10-26)

- **Stashed emitter start (Copilot voter incident):** decided by a Decider, not by Steve, when the adapter/emitter card (003) is pulled: salvage it as a
  reviewed starting point, or drop it. The stash stays until then; it is not applied, shipped or deleted before that vote.
- **Trust-ladder ledger** (`TEAM_WORKING_AGREEMENT.md`, "Driver trust ladder"): update every driver's rung and clean-cycle count from the cards
  since Oct 25 at the next retro (it is stale). Under the bootstrap-phase rule this is judgment from review evidence, not a metric.
- **Tooling:** a re-run of a card should warn when the card file changed since its working log was seeded (three wasted starts on card 036).

## 2026-10-26: a Decider voter ran with full write access and edited the repo (area: Organization, safety)

- **What happened:** my PowerShell launcher flattened a one-item list, so the Copilot voter in one round received the bare prompt
  'd11' instead of the vote text. Copilot was started with `--allow-all-tools` in the repo directory, treated 'd11' as a task, and
  for about 15 minutes edited source files (an unreviewed start of the C# adapter, +580 lines in `CSharpProvider.cs`, plus a
  `DotnetCli.cs` roll-forward tweak). It made no commit and no push. I found it from file timestamps after a status check showed
  edits I did not make, traced the process chain (my launcher, the job runspace, node, copilot.exe), and stopped exactly those
  PIDs. The edits are in a labeled stash, not shipped.
- **Root causes:** (1) 'read-only' for voters was a promise in the prompt, not a mechanism (the same gap as the delete ban: no
  detector); (2) voters ran in the real working tree with permission-bypass flags; (3) my scratch launcher had no check that each
  seat got the intended prompt.
- **Fix to build next (test first, mob-defaults):** a Decider runner that clones the repo into a scratch directory and runs every
  voter there (so nothing a voter does can touch the real tree), uses each CLI's read-only or restricted mode where one exists,
  checks the prompt each seat received, parses votes in one place (narration, conditional 'No ... yes if', and echoed transcripts
  are the three parsing traps seen so far), and verifies the real tree and HEAD are unchanged afterwards, failing loudly if not.
- **Counter-metrics:** none to game; this is a safety fix. Any voter-caused change to the real tree is a bug in the runner.

## 2026-10-26: the safety bans had no detector (area: Organization, safety)

- **What happened:** while clearing a scratch folder the facilitator ran `rm -rf` on a variable path. The folder did not exist and
  nothing was deleted, but the standing ban (never `rm -rf` a variable or computed path) relied on memory.
- **Steve's rule applied:** "constraints must self-report": a ban with no detector isn't in force.
- **Shipped (mob-defaults `delete_guard.py`, 80 tests, and phrase-lang pre-commit):** a detector for recursive deletes of variable or
  unresolved paths and for kills by process name; `safe_delete(path, roots)` as the single safe way to delete; the safety rule
  appended to every prompt; a `Safety flag` raised in MOB.md (and on the published page) for any model reply that shows such a
  command; a pre-commit check on added script lines.
- **Hypothesis for the retro to check (a safety fix, not a trial):** every seeded violation in the tests is detected (25 unsafe
  patterns), and ordinary literal scratch deletes and exact-PID kills are not flagged (10 safe patterns). Counter-metrics: false
  flags per card (target 0; any false flag is a bug in the detector) and the tokens the extra prompt sentence adds (about 60 per turn).
- **Known gaps:** the guard cannot stop a command a driver runs in its own shell; it flags the evidence in its reply and blocks the
  commit of such a script. A symlink case is covered in code but its test is skipped on Windows (no symlink privilege).
- **Ask of the retro:** is a runtime wrapper around the drivers' shells worth the cost, or is detect-and-flag enough?


## Tooling authorship rule and TAKEOVER log (Dispatch, 2026-10-26; facilitator/PM call, challengeable)

Tooling changes are authored by the rotation (Codex, Copilot, Grok, agy); the facilitator reviews. Any exception is logged here as a TAKEOVER with a reason.

- **TAKEOVER 1: `smoke.py` (mob-defaults `4f22fc4`).** Reason: needed to unblock the smoke step while the dashboard run held the drivers. Non-author review (sandboxed Decider, 4 seats): Codex No, agy No, Copilot Support-it, Grok Support-it. Verdict: NOT sound to rely on yet.
  Fix list (to be authored by the rotation, Codex first, after the dashboard run ends):
  1. A missing or empty `MOB.md` must FAIL (today the failover/safety/flag checks pass on empty text).
  2. The decider check must not pass on an echoed prompt: the proposal must not contain a vote word; require a parsed vote from the reply with no echo, and reject narration-only.
  3. Plain card mode never runs the Grok/agy seats; run with the mode that exercises every seat, or check each seat's turn explicitly.
  4. Check the decider result's violations/README and write-access fields, not only `integrity.ok`.
  5. Quoting: exercise the Windows quoting path (a Done-when and a proposal with spaces and quotes).
  6. Add a test for the `delete_guard` read-only fix; make scratch cleanup failure a FAIL, not a warning with exit 0.

## Steve withdrew the course correction (2026-10-26)

Steve: "perhaps I'm being too reactionary. I should let the mob decide its working process... it may have been taking a long time but it clearly had value if all models signed off." Reverted: the product-first mandate, ~20% process budget, balance wording, lighter Decider, mandatory headline and the Ready re-rank (phrase-lang `cb7e3c5`; template mob-defaults `9f21b73`, `dd7e45f`). The Decider is the default again. The dashboard time-box is withdrawn (card 036 finished anyway: Done, D13). P1 to P6, the defect ledger and the five whys above are **optional input** for the process retro, not directives; the team decides what to keep, including the non-author review rule for tooling and the hardened `smoke.py`. Unchanged: shipped work, safety guards, the sandboxed Decider runner, TFDD, trust ladder, bootstrap phase, escalation and safety carve-out, Goodhart guard, LLMs-as-customers.

## Async and events: keyword seeds (Steve's input, Decider D21, 2026-10-26)

Not cards yet, by D21 and the bootstrap phase: wait for the ADR (059), the spike (061) and the demand evidence (060). Each would be a thin card on the base library after those: (1) "run {tasks} in parallel and collect the results" (input-ordered results, failure aggregation); (2) "process {items} concurrently, at most {n} at a time" (in-flight never exceeds n, proven by a controlled scheduler); (3) "publish {event}" with an observer keyword (the handler sentence must not start with the line-start word `when`); (4) a timeout / cancel sentence (declared failure code, virtual clock). Effects named by the language designer: Scheduler, Clock, Cancellation, Events. Usable v1 does not need any of this (Tasks demo: no; status API 038 first slice: no).

## P1 occurrence count and the 018 stall (2026-10-26)

P1 (no turn ends with the next action running) had a third occurrence today, and Steve's direction is that the loop lives in the tooling ("it should be a loop"; cards 063 to 070). The 018 stop itself was an ENVIRONMENT fault (the .NET 8 runtime not on the mob shell's path after I restarted the run from a new shell), counted in the defect ledger as class T (tooling, ledger row 1 class); card 065 pins the toolchain so it cannot be forgotten. Slicing trial data point: 018 (code only, four behaviours) took two runs and 6 cycles, but the cap hits were the environment fault, not size; all four behaviours passed after the first run.

## Run 063 stopped at the Ready check (2026-10-26, exit 3): same class as card 036's three wasted starts

The PM said NOT READY: (1) the thin cards' "Done checks" were not in Given/When/Then form, and (2) `board/MILESTONE-usable-v1.md` and card 062 are not in the mob-defaults repo, so the PM could not verify them. The card was written for the Phrase repo but run in mob-defaults. Fixed in the tooling cards (062, 063 to 070, 072): absolute read-only paths to the Phrase board, and Given/When/Then criteria. Lasting fix candidates for the retro: the card template for tooling cards (absolute paths and G/W/T by default); P4 auto-fresh; and the loop's picker (card 063) should run the Ready check itself and park a card that fails it, not stop. The watchdog caught this only after a nudge: P1 again (a run that stops at exit 3 needs the loop to react, which is cards 063 to 067).

## Agile lens for the next retro (Steve's ruling, 2026-10-26; VISION ruling 6)

Steve: "Just for general direction, I want this to aim toward agile philosophy. It's a common temptation to skew toward spec-heavy up-front planning. We should avoid this temptation despite building effectively a spec for planning. Iteration and incremental is the key to successful development." Apply it as a lens: where are we over-specifying up front? Candidates to examine with data: Done-when lists and acceptance criteria written long before a card's turn (the board holds ~80 cards, most written ahead of need); cards 077 to 083 and the loop cards 064 to 072 as a plan; the number of Deciders and records per small change; the keyword, actor and async ADRs placed before a working slice; and ceremony per card (task force, review, retro) versus the size of the increment. Questions for the retro: which cards should be deleted or merged until their turn, which ADRs should wait for a spike, and what is the smallest working increment that could replace each plan. Measure: share of effort on planning artefacts versus working increments, and cards written but not pulled within a week.

## Smoke run findings after the persona standard (2026-10-26, found while closing card 064)

The real-CLI smoke run now FAILS for two reasons that are tooling defects, not model results: (1) the toy card's story ("As the mob...") is role-only, so the product manager correctly says NOT READY under the persona standard (exit 3): the toy repo needs a PERSONAS.md and a persona story; (2) the Decider step demands exit 0, but the runner exits 1 when any seat votes No or does not answer, so one honest No fails the smoke run, and a seat that is out of quota fails it for ever. agy has been out of quota (`agy -p` answers `RESOURCE_EXHAUSTED ... Resets in 136h22m11s`, captured 2026-10-26 22:05), which also explains its silence in every Decider since D19. Fix (a small tooling card, rotation-authored): the smoke toy card names a persona; the Decider check requires every AVAILABLE seat to answer (any vote) and reports an unavailable seat with its captured error instead of failing; a `--probe` step lists seats and their quota status first. agy rejoins when a probe succeeds (about 2026-11-01 06:00).

## Grok rerun after the harness fix (card 085, 2026-10-27): harness-and-launch problem, not a language result

Rerun of the 8 development tasks (label rerun-085): 2 passed at parse-resolve (004, 011); 3 failed as no_phrase_found (001, 002, 007), each reply a one-line narration ("I'll look through the workspace for Phrase syntax..."); 3 unavailable with "Max turns reached" (005, 008, 010); the harness excluded Grok from the baseline (non-empty replies 5/8, minimum 75%). With the task files now IN the prompt the narration persists, so the cause is Grok's launch, not the missing files: `grok --max-turns 10 --prompt-file ...` is an agentic CLI that spends its turns exploring instead of answering. The harness now classifies this truthfully (completed no_phrase_found counts as a model failure of the model AS HARNESSED; max turns is unavailable), but the Grok rows are not a statement about the language until the launch is fixed. Fix: card 091 (cold runs: an empty working directory and, where the CLI offers it, restricted tools or an answer-only mode; record the flags in the row). Codex's review notes for a later pass: ReadPrompt takes only top-level non-JSON task files (no current task has others), and two of the new 085 tests are weaker regression tests than claimed.
