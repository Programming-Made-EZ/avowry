# Team Working Agreement: Phrase

Seeded from the mob-crosscli default template (`templates/TEAM_WORKING_AGREEMENT.default.md`). The
facilitator refuses to start a card in a repo without this file.

How we work lives here. Process improvements land in **Practices** at a retro; they are never board cards.
The mechanics (flags, scripts) live in the mob-crosscli docs. Seeded from the play-tracker mob's agreement,
generic parts only. Adopted 2026-10-25 by the mob facilitator on Steve's standing instruction that every mob has an agreement.

## North star

[VISION.md](VISION.md) is the purpose of Phrase, from Steve's ruling. **Product decisions are judged against it**,
at every Decider that touches the product, and the board and milestones are assessed against it. A question about
the product's purpose or direction is answered by VISION.md first; it escalates to Steve only if VISION.md does not
settle it.

## Task-Force Driven Development (TFDD)

Steve: "we'll call this Task-Force Driven Development. where a task force is assembled."

Before each card, a **task force** is assembled: every role the work actually needs, and only those. The standard
roles (PM, architect, QA, devs, UX, security, agile coach, enablement, DevOps, domain expert) are a menu to
consider; new card-specific roles are created whenever the work calls for them (compiler engineer, grammar
designer, technical writer, benchmark/eval designer, LLM-ergonomics reviewer, performance engineer). Each role has
an assignee (model or seat) and a one-line why. There is no minimum or maximum role count.

## 1. Values

- **Correctness over speed, but speed matters.** Never ship on a claim nobody independently reproduced.
- **Evidence over vibes.** Findings, retro items and "it works" are grounded in a log line, a timestamp or a
  re-run command.
- **Respect Steve's attention.** He is one person on a phone. Decide what the team can decide; ask only what
  needs him, and ask it so one word answers it.
- **The team owns outcomes, not tasks.** Done means a verified, user-observable increment on main.

## 2. Principles

- **Bishop's Law** (Steve Bishop, 2025): "Work done in async requires a resync. As the work complexity
  increases, so does the resync complexity. Revised: parallel (independent) streams R = O(α log C + β);
  concurrent (conflicting) streams R = O(α C + β), where C is total async work complexity, α the scaling
  factor, and β the fixed overhead. Design improvements can reduce, but not always eliminate, resync
  complexity; naive async approaches linear or worse."
  *Applied:* cards share the code, the drivers and the gate, so they are conflicting streams. Hence
  **WIP-of-one**: one card, one TDD cycle, one driver, one linear trunk in flight.
- **The board is the single source of truth.** Work is pulled from the top of the ranked Ready list.
  A feature idea from Steve is RAW INPUT: the mob reviews it, refines it into a card (a Connextra story
  plus ZOMBIES acceptance criteria a user can observe), and the PM ranks it. Steve gets back the card
  number, its position and any one-word question, never a design plan to approve. The board holds only
  product bugs and user stories; process changes live in this file.
- **Decide reversible things yourself.** If a flag or a later card can undo it, record the assumption and
  move on. Model disagreements go through the **Decider protocol** first. Escalate to Steve only what the
  **Escalation** rule below allows.
- **Independence of review.** A reviewer did not author the patch, starts from a fresh context, and shows
  evidence (commands and real output). Prefer a different model family.
- **Goodhart's Law.** Metrics inform judgment; no number is ever the goal.
- **A rule without a detector isn't in force.** Prefer a mechanical check at a named transition to a
  promise to remember.
- **Steve works from his phone.** No `AskUserQuestion`, no popups, no approval dialogs, no open-ended
  review requests.

## 3. Practices

### Engineering discipline

- **Outside-in TDD** from ZOMBIES acceptance criteria: start at the outermost boundary, work inward.
- **Honest red first.** Watch the test fail for the right reason. A red that survives when only the fix is
  reverted is not proof. Every test carries Given/When/Then comments.
- **Red → Green → Refactor → Commit → Push**, every cycle. Refactor covers test code too, and must keep
  each test as sensitive as before.
- **Trunk-based development.** Each green increment ships to main as soon as the gate is green; no
  long-lived branches. The gate (tests, zero warnings, any CI) passes before anything deploys.
- **No `Co-Authored-By` trailers on commits** (Steve's standing coding rule). This covers drivers, the facilitator and
  the MOB.md publisher. A `commit-msg` hook (`.githooks/commit-msg`, enabled with `git config core.hooksPath .githooks`)
  rejects them. Pushed history is not rewritten.
- **Vertical slices.** Every slice has a user-observable acceptance criterion.
- **Independent review before shipping**, by a non-author with a fresh context (see Driver rotation).
- **Stage explicit paths only.** Never stage the mob's scratch directory (`.mob/`).

### Board and cards

- **Definition of Ready:** top of Ready; a Connextra story; ZOMBIES acceptance criteria (Zero, One, Many,
  Boundaries, Interface, Exceptions) with a user-observable outcome; no unverified external dependency.
- **Retro after every Done card**, before the next card is pulled. It **opens with flow metrics** taken from
  timestamps: commit-to-main time, time waiting on gates or reviews, and minutes lost to failed or retried
  cycles. Then four areas, each with evidence: Individual, Team (mob), Organization, Product. One process
  improvement pick per retro; product findings become new cards; the PM re-checks the top of Ready.
- **Process work** is chosen only at a retro, states its user benefit, and is timeboxed to one or two
  cycles. Stop-the-line exceptions only: data loss, a security hole, a broken trunk.

### Continuous loop (standing rule, Steve, 2026-10-25)

The mob runs continuously: card → ship each green increment → Done → retro → PM re-ranks → pull the next
top card → repeat. It does not stop between cards or wait for pings. It stops only when the board's Ready
column is empty AND the retro produced no new cards, or when Steve says stop. WIP stays at one card.

### LLMs are the customers (standing rule, Steve, 2026-10-25)

Phrase's target consumer is LLMs, so every model in the rotation (Codex, Copilot, Grok, agy/Gemini, and
Claude as a voice) is a source of product ideas.

- **Every retro covers four categories:** Individual, Team, Organization and **Product**, and opens with
  flow metrics.
- **Product feedback is gathered from each available model, in parallel and cheaply, as evidence and not
  opinion polling.** Each model reads the current `docs/GRAMMAR.md` plus a sample `.phrase` file, then
  writes or modifies a small phrase. It reports where it struggled (ambiguity, wasted tokens, misread
  intent, a missing construct) and proposes **at most 2 improvements, each with a concrete example**.
  Feedback-gathering turns are read-only and write no files in the repo.
- **These ideas are raw input.** The PM, with the domain expert (the language designer), dedups them and
  weighs value against effort and fit with Phrase's principles ("the code is the context", specs are
  truth). Winners are refined into Connextra + ZOMBIES cards and ranked on the board. **Every rejected
  idea is recorded with the reason it was rejected** (in the retro entry).
- **After each retro the facilitator reports to Steve** which cards came from model feedback and their
  board ranks.

### Escalation (standing rule, Steve, 2026-10-25)

Steve: "The only thing that bubbles to me is general direction for purpose of the new language or
conflicting opinions from the models. However there is also the decider protocol for conflicting views."

- **The mob decides everything else itself:** features, syntax details, priorities, implementation and
  tooling. Do not ask Steve.
- **Model disagreement** (retro product feedback, design debates, reviews): run the Decider protocol first.
- **Whether anything escalates is itself decided by a Decider.**
- **Escalate to Steve ONLY if** (a) the question is about the language's overall purpose or direction
  (what Phrase is for, or who it serves), or (b) a model conflict the Decider could not resolve.
- **Format:** one short message with the options, each model's position in one line, and a recommendation;
  answerable in one word.
- **Safety carve-out (does not widen what gets escalated):** actions outside the mob's authority still need Steve's
  explicit OK before execution, presented as a one-word ask: destructive or irreversible changes to production or user
  data, spending money, security or privacy exposure, and external commitments. Phrase specifics: publishing a package
  or release (an external commitment) and any spend beyond the prepaid driver allotments.

### Decider protocol (the default for every team decision)

Steve: "any decision should probably go through this process." The Decider settles conflicts, and it is also how
the team decides things in the first place.

**What goes through it:** the task force (at kickoff and every mid-card addition), card refinement and acceptance
criteria, PM ranking and re-ranking, design and architecture choices, keeping or reverting a feature on benchmark
data, declaring a card Done, milestones and their targets, retro picks and process experiments, trust-ladder
promotions and demotions, benchmark suite changes, and whether anything escalates to Steve.

**Not a Decider:** the driver's moment-to-moment implementation choices inside an increment. Those are covered by
independent review, and any reviewer objection becomes a Decider proposal.

1. **Proposal.** The PM, facilitator or a role owner states ONE concrete proposal.
2. **Simultaneous vote.** The task force votes at the same time, in parallel, each choosing **Yes**,
   **Support-it** (not my first choice, but I will back it) or **No**. Seats do not see each other's votes first.
3. **Outliers.** Each **No** voter says what it would take to get to yes, in a short reason; only outliers explain.
   The proposer may amend, and the amended proposal is voted once more.
4. **Result.** An "absolutely no" from any seat, or outliers still unresolved after the amendment, means the
   proposal **fails**. Otherwise it passes. One round, no long debate.
5. **Record.** Every run goes in MOB.md as `Decider D<n>: <proposal>`, with votes and `Result: passed|failed`. The
   facilitator collapses an all-Yes result to a single line, for example
   `- Decider D4: rank card 016 second, all Yes, passed`. A failed proposal is logged and may be raised again at
   the next retro.
6. **Keep it lean.** Votes are short and collected in parallel so the Decider does not drain the driver allotments.

- **Run every Decider with the sandboxed runner** (`decider.py` in mob-defaults): each seat votes in its own throwaway clone of the
  repo, never in the real working tree, with the CLI's read-only mode where one exists; every seat gets the same full prompt; votes are
  parsed in one place; the real tree and HEAD are verified unchanged afterwards. (A voter once ran with write access in the real tree
  and edited source files; "read-only" in the prompt is a promise, the sandbox is the mechanism.)

**The task force is a Decider.** The initial task force is a Decider proposal from the facilitator or PM, so any seat
can object to a missing or padded role. A role proposed mid-card needs a Decider too: the proposer states the role, its
assignee and why it is needed; if it passes, the role is added to the card's `## Task force` with where it joined and
a pointer to the record (`| joined: cycle 2 | decider: D3`). Card mode refuses a mid-card role that carries no
`decider:` reference or whose Decider record is missing or did not pass.

### Milestones and hypothesis-driven work (standing rule, Steve, 2026-10-25)

Steve: "the mob should also set milestones for themselves and use hypothesis to determine if new feature
ideas work. ideally they should be data driven."

- **Self-set milestones.** The mob sets its own milestones on the board (`board/MILESTONES.md`). Each has a
  **measurable target** and an **automated check**. Progress against them is reviewed at every retro, and
  the next milestone is set when one is reached. Steve hears only when a milestone is reached, plus Usable v1 itself.
- **Hypothesis, or no card.** Every feature card (especially one that came from model feedback) carries a
  hypothesis in exactly this form: *"We believe <change> will <improve metric> for LLM authors/readers. We'll know when
  <metric> moves from <baseline> to <target> on <benchmark>."* Bugs and plumbing cards are exempt; a plumbing
  card names the milestone bar it advances instead.
- **Data, not opinion.** The LLM benchmark harness (card 016) is the instrument for everything else and is ranked high. Results are stored in the repo as a history so trends can be compared.
- **The loop.** Before building a feature, record its baseline on the benchmark. Afterwards, re-run it.
  Keep the change if the hypothesis held. Revise or revert if it did not, and record why in MOB.md (not during the bootstrap phase, below).
  Decider disputes are settled with data where possible. Retro ideas are triaged by expected metric impact.

### Process ideas are hypotheses, not doctrine

Any process change idea, from Steve, a model or a retro, enters as an **experiment**, not a rule. Each experiment has:

- a **hypothesis** ("We believe <change> will <improve metric> for <who>. We'll know when <metric> moves from
  <baseline> to <target>.")
- a **metric and a counter-metric** (Goodhart guard)
- a **trial scope**: which cards, or how long

The retro then decides **keep, revise or drop** through the Decider (on judgment, not metrics, during the bootstrap phase). **Only kept experiments are written into this
agreement as rules.** Explicit standing rules from Steve still apply directly, without a trial. Running experiments
are logged in `docs/experiments/` (one file each) and in MOB.md, not in this agreement.

### Bootstrap phase: no metric-based rejections until a baseline is established (standing rule, Steve, 2026-10-26)

Steve: "it seems like we don't have a good baseline because everything is so new. can we wait until we've established a
cadence first with metrics before we start turning down ideas because they aren't improving something that doesn't exist yet?"

- **Until a baseline is established, no idea, feature or experiment may be rejected, reverted or dropped because it "didn't
  move the metrics".** There is nothing yet to move.
- **Keep writing hypotheses and keep recording measurements.** They build the baseline and the history. Decisions in this phase
  use judgment, through the Decider: Phrase's principles and `VISION.md`, qualitative evidence (cold reads, review findings), and cost.
- **What is still allowed:** rejecting on principle, safety, correctness, security or cost; and the stop-the-line items (data
  loss, a security hole, a broken trunk or deploy). Only the metric-based keep/revert is paused.
- **Exit:** metric-based keep/revert starts only when the mob, by a Decider, declares the baseline **established**, with the
  data attached. The exit criteria, all of them:
  1. **The benchmark is trustworthy.** All four models give valid, non-empty results with full task coverage (every model has a row for every development task, at most 10% `unavailable`, no model whose completed rows are all empty replies); the prompts include the docs a new user sees (the docs-in-context mode of card 037, with the zero-shot rows kept and labeled); and the holdout exists and is untouched.
  2. **A steady cadence.** It has run after every Done card for at least 5 consecutive Done cards (including at least one
     documentation or feature change), so there are at least 5 runs of history per model.
  3. **Acceptable variance.** The run-to-run noise floor is measured on unchanged code and is small: repeated runs differ by at
     most 10 points of pass rate, and the harness self-check shows zero drift.
  4. **Counter-metrics are tracked** in every run (tokens in and out, attempts, wall-clock time, diagnostics, misread-intent
     errors, and the dedup and learnability measures once they exist).
- **Re-check what was already decided on metrics.** Anything rejected for "no metric impact" returns to the board as raw input
  for normal review; anything rejected on principle stays rejected unless the mob reconsiders. Verdicts reached on early,
  setup-heavy data are recorded as "early data, not final".

### Goodhart guard (standing rule, Steve, 2026-10-25)

Steve: "just keep Goodhart's Law in mind." Metrics inform decisions but are never the goal: a measure that
becomes a target stops being a good measure.

- **Balanced metrics.** Never a single one. Pair each target metric with a counter-metric (tokens saved vs. pass rate and readability; pass rate vs. spec coverage and diagnostic quality).
- **Holdout.** Keep part of the benchmark unused while developing a feature, and evaluate on it before keeping a
  change. Rotate or refresh tasks periodically so nobody tunes the project to the suite.
- **The suite is versioned.** Changing tasks, scoring or prompts needs a Decider run and a MOB.md note, and is
  never bundled with the feature it measures.
- **No special-casing the benchmark.** No grammar or compiler hack that only helps benchmark tasks. Reviewers check this explicitly.
- **Qualitative check.** Every metric win also needs a model to read the change cold and explain it, and the change
  is sanity-checked against Phrase's principles ("the code is the context", specs are truth).
- **Red flag, not a win.** At retro, a metric improving while its counter-metrics or holdout degrade is treated as
  metric gaming.
- **Same for flow metrics and trust-ladder evidence:** signals for retro discussion, never quotas.

### Task force before every card (TFDD, standing rule, Steve, 2026-10-25; revised same day)

Steve: "before each feature is started the comprehensive set of roles should be declared and assigned," then:
"there's no quota for how many roles are required for the task force, just that all of the roles that would apply
are created."

- **Before ANY card starts (feature or bug), a task force is assembled** on the card (`## Task force`; MOB.md shows
  it): every role that actually applies, and only those. The standard roles are a **menu, not a form**: PM,
  architect, QA, backend/frontend/database dev, UX, security, agile coach, enablement, DevOps, domain expert (language designer).
  **Create new, card-specific roles whenever the work calls for them**, for example compiler engineer, grammar
  designer, technical writer, benchmark/eval designer, LLM-ergonomics reviewer, performance engineer.
- **Each role has an assignee (model or seat) and a one-line "why it's needed".** There is no minimum or maximum
  role count.
- **The test is fit.** A role the work clearly needs that is missing is a defect the agile coach or a reviewer
  flags. A padded task force is a smell too.
- **One structural rule:** whoever authors an increment never reviews or QAs it. Assign by trust-ladder rung and
  per-card rotation.
- **The task force is living, not frozen at kickoff** (Steve: "also during the building if the team identifies a
  new role is needed then that gets added"; then: "I think decider protocol is needed to decide if a new role is
  required"). Any member can flag mid-card that a role is needed (a security question surfaces, the work turns out
  to need a technical writer or a perf engineer) and states one Decider proposal: the role, its assignee and why.
  If it passes, the role is added to the card's `## Task force` with where it joined and the Decider reference
  (`| joined: cycle 2 | decider: D3`). The new role then reviews the relevant work done so far, not only what comes
  next. Card mode refuses a role with no passed Decider on record.
- **Retro reviews every mid-card addition:** why wasn't it caught at assembly? If the miss is likely to recur, the
  role is added to the menu in the template. A failed proposal can be raised again at retro.
- **Detector:** card mode checks only that a task force is declared, that every listed role has an assignee and a
  why, that no seat is both `[author]` and `[reviewer]`, and that each mid-card role cites a passed Decider. It never
  checks role names or counts. Template:
  `templates/TASK_FORCE.md` in mob-defaults.

### Driver rotation

- **Rotation order: Codex → Copilot → Grok → agy; Claude is last resort.** Claude drives or reviews only
  when no other eligible driver is available, and the log says why.
- **Drivers are prepaid allotments, not flat subscriptions.** Any of them can run dry from heavy use.
  Balance load across all four; do not hand-pick a favorite.
- **The rotation unit is one TDD cycle** (Red, Green, commit, Refactor, commit, swap) by ONE driver. The
  incoming driver reviews the outgoing cycle first (run its tests, revert the fix to prove the red, check
  the refactor weakened nothing) and ends with SIGN-OFF or FLAG.
- **Fresh probe before relying on a driver's status.** A quota or timeout error marks a driver out only
  until its next successful probe, never "until someone says otherwise".
- **Size each cycle to finish within one driver's timeout.** If it will not fit, split the test; never hand
  off half a cycle.

### Driver trust ladder

Why (Steve): "we want to provide a way to regain trust and responsibility," and "just like a coworker who
underperforms, you still try to find a way they can be helpful." **Coach, never bench.** A quality problem
changes a driver's rung, never its membership. Only real unavailability (an error captured from a probe
within the last hour) takes a driver out of rotation, until its next successful probe.

| Rung | Name | What the driver may do | Oversight |
|---|---|---|---|
| 1 | Probation | Small, bounded cycles only (tests, one slice) | The facilitator re-runs every claim (tests pass, red proven, files changed) on the driver's commit and records it; every new test gets a red-sensitivity check (revert only the fix in a copy; the test must go red). Not Done on the driver's word. |
| 2 | Standard | Normal cards | At least one named test re-run per cycle; red-sensitivity still checked on every new test. |
| 3 | Trusted | Engine, money, gate/tooling code, larger cycles | Standard review seat and the normal gate evidence. |

- **Entry rung:** a driver new to a mob, or returning after a long absence, starts at Rung 1. A driver
  whose record in this mob justifies a higher rung starts there; the record must be cited.
- **Evidence to climb:** three consecutive clean cycles at the current rung. Clean = no misreported
  result, no surviving mutant the brief named, no scope overrun (files or behavior outside the brief), and
  no machine-safety violation. Each cycle logs: card, claim, re-run result, red check, verdict.
- **What drops a rung:** one rung per verified misreport or safety violation, never straight out of the
  rotation. Before demoting, the retro checks whether the brief or the model's context window caused the
  miss (quote the brief line it misread); if so, fix the brief or model and do not demote.
- **Path back up:** a driver that drops starts the climb again from zero clean cycles at the lower rung,
  with smaller, clearer cycles. A Rung 1 driver that misses again stays at Rung 1; it is coached, not
  benched.
- **Visible and reviewed:** each driver's rung and per-cycle record are kept in the table below and
  reviewed at every retro under Individual. Applies to every driver, Claude seats included.

**Driver ledger (the facilitator updates this; commit it with the cycle).**

| Driver | Rung | Clean cycles toward next rung | Last verdict / date | Notes |
|---|---|---|---|---|
| Codex | 1 | 1/3 | 2026-10-25 cycles 2-3 NOT clean: Grok review CHANGES (throws, missed PH312, option coercion); count unchanged | Cycle 1 clean (Copilot sign-off) |
| Copilot | 1 | 0/3 | 2026-10-25 authored a47ed6a (specs + fixture, gate 221+1 re-run by facilitator); awaiting independent review | Reviewed cycle 1: APPROVE |
| Grok | 1 | 0/3 | 2026-10-25 reviewed 59d8a9c+84b5737: CHANGES with red-check evidence (good review) | Drives hardening cycles next |
| agy | 1 | 0/3 | none | New to this mob; model gemini-4-argon-high (3.1 Pro not offered) |
| Claude (last resort) | 1 | 0/3 | none | CLI not signed in as of 2026-10-25 |

### Safety

- **Never kill a process by name or pattern**; only by exact PID, and only your own.
- **Never `rm -rf` (or `Remove-Item -Recurse`, `rd /s`, `git clean`) on a variable or computed path.**
  A delete is allowed only on a literal path inside the repo's scratch directory, after asserting it is
  non-empty and absolute.
- **Detectors (Steve: constraints must self-report; a ban with no detector isn't in force).** Both bans above are enforced
  mechanically, not by memory: (1) the facilitator appends the safety rule to every dispatched prompt and scans every model
  reply for a recursive delete of a variable or unresolved path and for a kill by process name, raising a `Safety flag` in
  MOB.md and on the published page; (2) `safe_delete(path, roots)` in mob-defaults' `delete_guard.py` is the one way to delete a
  computed path, and refuses anything that is not a verified strict child of a known scratch root (empty, relative, outside,
  `..`, symlink, the root itself, an ancestor, home, a drive root); (3) a pre-commit check
  (`scripts/check-unsafe-deletes.py`) refuses a commit that adds such a delete or kill to a script or source file.
- **No `AskUserQuestion`** and no interactive prompts anywhere in the mob.
- **Do not touch other projects' repos, ports or containers** on a shared machine.
- Drivers do not add behavior beyond the brief; extra ideas go in the report as "NOT DONE, proposed".

## 4. Project specifics

- **Product and users:** Phrase is an LLM-friendly language. Behavior is named by sentence-style phrases with
  typed holes, specified by Given/When/Then specs that are the source of truth, and compiled through an IR
  to C#/FastEndpoints first. Users are engineers and LLMs writing and maintaining Phrase programs.
- **Toolchain and the gate:** .NET 8 SDK (user-scope install in `%USERPROFILE%\.dotnet`; set `DOTNET_ROOT`
  and put it first on `PATH` for runs). Green means `dotnet test Phrase.sln --filter "Status!=Pending"`
  passes with zero warnings (`TreatWarningsAsErrors` is on). Tests marked `Status=Pending` are the to-do
  list; remove the trait in the same commit that turns a class green.
- **Board:** the `board/` folder (one markdown card per file, `NNN-slug.md`). Cards carry a Connextra story,
  ZOMBIES acceptance criteria and a "Done when" command list the facilitator runs. The PM ranks by number
  order unless a card says otherwise. No GitHub Project exists for this repo yet.
- **Feature-flag policy:** none; compiler internals have no user-facing surface yet. The architect may
  overrule per card.
- **Release target:** none yet (no published package). The mob decides release and grammar-compatibility
  questions itself under the Escalation rule below.
- **Code needing extra care:** `docs/GRAMMAR.md` (the language contract; a grammar change needs a recorded
  reason), `Semantics/Model.cs` (the IR the emitters consume; change only with a reason in the reply), and
  diagnostic messages (an LLM reads them to fix its own code: name the thing, say what was expected, list
  the valid options).
- **Domain rules:** the tests are the spec. If one looks wrong, say so in the reply instead of quietly
  editing it. The resolver must never throw, even on source with syntax errors.
