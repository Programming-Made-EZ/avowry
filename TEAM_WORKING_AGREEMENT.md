# Team Working Agreement: Phrase

Seeded from the mob-crosscli default template (`templates/TEAM_WORKING_AGREEMENT.default.md`). The
facilitator refuses to start a card in a repo without this file.

How we work lives here. Process improvements land in **Practices** at a retro; they are never board cards.
The mechanics (flags, scripts) live in the mob-crosscli docs. Seeded from the play-tracker mob's agreement,
generic parts only. Adopted 2026-10-25 by the mob facilitator on the product owner's standing instruction that every mob has an agreement.

## North star

[VISION.md](VISION.md) is the purpose of Phrase, from the product owner's ruling. **Product decisions are judged against it**,
at every Decider that touches the product, and the board and milestones are assessed against it. A question about
the product's purpose or direction is answered by VISION.md first; it escalates to product owner only if VISION.md does not
settle it.

## Task-Force Driven Development (TFDD)

**Principle:** use Task-Force Driven Development, assembling a task force for each card so the work has every role it needs and no ceremonial extras.

Before each card, a **task force** is assembled: every role the work actually needs, and only those. The standard
roles (PM, architect, QA, devs, UX, security, agile coach, enablement, DevOps, domain expert) are a menu to
consider; new card-specific roles are created whenever the work calls for them (compiler engineer, grammar
designer, technical writer, benchmark/eval designer, LLM-ergonomics reviewer, performance engineer). Each role has
an assignee (model or seat) and a one-line why. There is no minimum or maximum role count.

**Task-force driven means sized to the task (product owner clarification, 2026-10-29).** The number of members and who
they are is decided before the card starts and varies with the task. Members are added during the card as the need appears;
no member is removed mid-card: removal happens only at the retro, which decides whether each member is still needed. The
question of whether a member with different domain knowledge or context should join is always asked. The card-mode
seat list (ready, slice, red, green, ship, signoff) is a default, not a fixed roster. The detailed rules and their
accountability are under Practices, Task force before every card.

## 1. Values

- **Correctness over speed, but speed matters.** Never ship on a claim nobody independently reproduced.
- **Evidence over vibes.** Findings, retro items and "it works" are grounded in a log line, a timestamp or a
  re-run command.
- **Respect the product owner's attention.** The product owner is one person on a phone. Decide what the team can decide; ask only what
  needs their decision, and ask it so one word answers it.
- **The team owns outcomes, not tasks.** Done means a verified, user-observable increment on main.

## 2. Principles

- **Bishop's Law:** asynchronous work requires resynchronization, and resynchronization grows with work complexity. Independent streams have R = O(α log C + β), while conflicting streams have R = O(α C + β), where C is total async work complexity, α the scaling factor and β the fixed overhead. Design can reduce but not always eliminate that cost, so naive asynchronous approaches scale linearly or worse.
  *Applied (revised 2026-10-26, Decider D22):* work is divided into atomistic, independent units because dependencies prevent true parallelism. A task that is
  atomistic and dependency-free is the cheap **parallel** class, R = O(alpha log C + beta): its `Depends on:` line is `none` and it shares no files with another in-flight task,
  so it may run in its own worktree from current main, with merges serialized as fast-forwards. A task that names a dependency, edits files another in-flight task edits, or shares
  state is the **concurrent** class, R = O(alpha C + beta), and stays **WIP-of-one**: one card, one TDD cycle, one driver, one linear trunk in flight. Sharing the gate is the fixed
  overhead beta. Every card has a `Depends on:` line (a missing line does not mean none). A merge conflict reclassifies that pair as concurrent: the later task is discarded and
  retried from the new main.
- **Land or discard (experiment, Decider D22, under trial; not yet doctrine).** Work should be small, atomistic and all-or-nothing because clean units are easier to retry and distribute. Each task starts from current main in a fresh worktree and ends in exactly one state:
  LANDED (every check passes, a non-author has reviewed it, one commit on main) or DISCARDED (the linked worktree is removed with `git worktree remove` and, only for a strict child of a
  registered scratch root, `safe_delete`; nothing from the attempt remains on main or in a stash). A retry starts clean; the attempt's log lives outside the worktree and is never the
  seed of the next attempt. Metrics: retries per task, partial-state incidents (stashes, uncommitted carry-over), time to land. Counter-metric: merge conflicts and resync overhead from
  parallel work. Review after the first three thin cards that use it; the trial Decider keeps, changes or drops it.
- **Agile: iterate and increment (product owner, 2026-10-26; VISION ruling 6).** "I want this to aim toward agile philosophy. It's a common temptation to skew toward spec-heavy up-front planning. We should avoid this temptation despite building effectively a spec for planning. Iteration and incremental is the key to successful development." So: specify only as much as the next slice needs; prefer a working increment over a longer plan; let design emerge slice by slice; and ask at every retro, in the process category, "where are we over-specifying up front?" (long Done-when lists, cards written far ahead of their turn, ceremony per card, designs decided before evidence). A signal to watch, never a quota; the thin-slice rules (D18) are an application of it.
- **The Phrase Board on GitHub Projects (project 1 under Programming-Made-EZ, private) is the single source of truth.** `board/` is a generated read-only archive (`scripts/board/generate_archive.py`). The loop and the retro re-rank hook write Rank and Status to the project only, after a passed Decider, skipping any card whose Rank is newer than the Decider's snapshot. Card text is edited on the issue. Work is pulled from the top of the ranked Ready list.
  A feature idea from product owner is RAW INPUT: the mob reviews it, refines it into a card (a Connextra story
  plus ZOMBIES acceptance criteria a user can observe), and the PM ranks it. product owner gets back the card
  number, its position and any one-word question, never a design plan to approve. The board holds only
  product bugs and user stories; process changes live in this file.
- **Decide reversible things yourself.** If a flag or a later card can undo it, record the assumption and
  move on. Model disagreements go through the **Decider protocol** first. Escalate to product owner only what the
  **Escalation** rule below allows.
- **Independence of review.** A reviewer did not author the patch, starts from a fresh context, and shows
  evidence (commands and real output). Prefer a different model family.
- **Goodhart's Law.** Metrics are evidence for judgment; no number is the goal, and a verdict is a judgment backed by data, not a stamp against a threshold.
- **A rule without a detector isn't in force.** Prefer a mechanical check at a named transition to a
  promise to remember.
- **The product owner works from their phone.** No `AskUserQuestion`, no popups, no approval dialogs, no open-ended
  review requests.

## 3. Practices

### Engineering discipline

- **Trunk-based, direct commits to main (product owner ruling, 2026-10-29, D135).** There are no pull requests and no branch protection. Unfinished or risky work ships behind default-off feature flags (or an equivalent toggle) so that main stays deployable. Enforcement is detection: CI goes red and the stop-the-line rule applies, and the external audit runs every 30 minutes (`docs/PROCESS-CHECKS.md`).

- **Outside-in TDD with ZOMBIES (the product owner's definition, D137).** Outside-in TDD is done by first creating a failing test for one acceptance criterion. Then we identify the layer the test consumes. We then use unit-level TDD to build that layer, using test doubles for any dependencies. Once that layer is complete, we start building the real implementation of each dependency from the layer above using unit-level TDD, again with test doubles for each dependency. We continue moving through each dependency until we have enough of the implementation to make the original acceptance criterion pass. During this whole time the acceptance test is failing until the last unit is built. Then we move on to the next acceptance criterion. We use ZOMBIES (Zero, One, Many, Boundaries, Interface, Exceptions) to find all of the acceptance criteria and each of the unit tests at each layer.
  - In card mode the red seat writes ONE acceptance test for ONE acceptance criterion, never the whole card's tests; the inner loop is unit red, green, refactor from the outside in with test doubles for every dependency; the acceptance test stays red until the last unit lands; then the next criterion starts.
  - Trunk-based main stays green because the failing acceptance test is committed with `Status=Pending` (excluded from the gate) and a `Criterion=NNN-K` trait; the green commit removes `Pending` in the same commit that makes it pass; a CI job (`pending-red`) requires pending criterion tests to fail by assertion; at most one criterion test is pending at a time and none survives its card (D137).
  - The trail record shows the sequence per criterion (red commit, ordered unit commits, green commit) and, for the criteria and for each layer's unit tests, the ZOMBIES test per letter or "N/A because" a reason.

- **Code follows `docs/CODING_GUIDELINES.md`.** One short document for this repository's own code: high cohesion and low coupling, dependencies pointing one way, small functions, naming, no throwing on user input, deterministic cross-platform output, tests that can fail, no dead code. Reviewers check every change against its checklist. It is separate from the project-profile standards Phrase applies to generated code (`docs/adr/ADR-library-binding-layering.md`).
- **Outside-in TDD** is defined in the first bullet of this section (Outside-in TDD with ZOMBIES, D137); the other bullets here (honest red, red-green-refactor, trunk-based) apply to every cycle of it.
- **Honest red first.** Watch the test fail for the right reason. A red that survives when only the fix is
  reverted is not proof. Every test carries Given/When/Then comments.
- **Red → Green → Refactor → Commit → Push**, every cycle. Refactor covers test code too, and must keep
  each test as sensitive as before.
- **Trunk-based development.** Each green increment ships to main as soon as the gate is green; no
  long-lived branches. The gate (tests, zero warnings, any CI) passes before anything deploys.
- **No `Co-Authored-By` trailers on commits** (the product owner's standing coding rule). This covers drivers, the facilitator and
  the MOB.md publisher. A `commit-msg` hook (`.githooks/commit-msg`, enabled with `git config core.hooksPath .githooks`)
  rejects them. Pushed history is not rewritten.
- **Vertical slices.** Every slice has a user-observable acceptance criterion.
- **Independent review before shipping**, by a non-author with a fresh context (see Driver rotation).
- **Stage explicit paths only.** Never stage the mob's scratch directory (`.mob/`).

### Board and cards

- **Card shapes (product owner, 2026-10-26; Decider D28).** A card is a STORY or a CONTRACT. A STORY is the why: As <persona from PERSONAS.md>, I want <capability that persona can observe>, so that <value that persona gets>, with acceptance criteria the persona can observe. A CONTRACT is the how, for a low-level piece no persona can observe directly (the resolver, emitter internals, a keyword implementation, an adapter seam); its ACTOR is a SYSTEM ACTOR, the component outside the unit that interacts with it (the calling phrase, the next compiler stage, an adapter, the generated program at runtime), not a person. An actor, in the UML and use-case sense (the product owner's ruling, 2026-10-26), is anyone or anything outside the system that interacts with it: a persona is a HUMAN actor, a contract's actor is a SYSTEM actor: `Shape: Contract`, `Actor:` the system actor (the interacting component), `Given:` its inputs, `Observes:` the output the consumer can tell from a wrong result, `Fails with:` codes (or none), `Invariants:` checkable truths, `Enables:` the Story card(s) it serves. A card is a Contract ONLY when no persona can observe the outcome; persona-observable compiler work stays a Story. NO ORPHANS: every Contract enables at least one Story, and is cut from a failing persona-level acceptance test (outside-in TDD, consumer-driven contracts). Specs on either shape state an outcome observed by someone (a persona, or the consuming component), never a bare "succeeds", "compiles" or "does not throw". The Ready check accepts either shape and REJECTS a role-only story, a design-goal benefit, a person or role as a contract's consumer, a bare-success observation and an orphan contract. A Contract ranks behind the Story it enables unless that Story is Done.
- **Definition of Ready:** top of Ready; a Connextra story that names a PERSONA from `PERSONAS.md` (a specific user with context, goals and constraints, never a generic role such as "an engineer (or an LLM)") and a "so that" that is value THAT persona gets (not an internal design goal; product owner, 2026-10-26: "this is a role, not a persona"); the Ready check REJECTS a role-only story or a design-goal benefit; ZOMBIES acceptance criteria (Zero, One, Many,
  Boundaries, Interface, Exceptions) with a user-observable outcome; no unverified external dependency.
- **Retro after every Done card**, before the next card is pulled (D136: from card 195 on, the retro for card N is pushed before issue N is closed and before any later commit that names a different card; commits that name card N itself are exempt). It **opens with flow metrics** taken from
  timestamps: commit-to-main time, time waiting on gates or reviews, and minutes lost to failed or retried
  cycles. Then four areas, each with evidence: Individual, Team (mob), Organization, Product. One process
  improvement pick per retro; product findings become new cards; the PM re-checks the top of Ready.
- **Thin slices (D136, forward-only from card 195).** The architect decides SHIP AS IS or SPLIT for every card and outputs the split list; the PM also rejects oversized cards at ready time. A card is shippable as is only with one concern, at most three increments and at most three acceptance criteria. A card commit has at most 15 countable files and 400 changed lines (countable: not board/, docs/decisions/, MOB.md, .mob/records/, docs/retros/ or generated expected.json fixtures), with the exception kinds `Slice-exception: rename | rewrite s/OLD/NEW/ | generated` that the audit verifies. The trail record carries a slice field and started_at; commit-to-live (start to green CI) is reported at each retro as a median (target 45 minutes) and a 90th percentile (target 90 minutes), as data and never as a quota.
- **Process work** is chosen only at a retro, states its user benefit, and is timeboxed to one or two
  cycles. Stop-the-line exceptions only: data loss, a security hole, a broken trunk.

### Continuous loop (standing rule, product owner, 2026-10-25)

The mob runs continuously: card → ship each green increment → Done → retro → PM re-ranks → pull the next
top card → repeat. It does not stop between cards or wait for pings. It stops only when the board's Ready
column is empty AND the retro produced no new cards, or when product owner says stop. WIP stays at one card.

### LLMs are the customers (standing rule, product owner, 2026-10-25)

Phrase's target consumer is LLMs, so every model in the rotation (Codex, Copilot, Grok, agy/Gemini, and
Claude as a voice) is a source of product ideas.

- **Every retro covers four categories:** Individual, Team, Organization and **Product**, and opens with
  flow metrics.
- **In the product category, also ask who else would use or be affected by the product, and whether they are represented in `PERSONAS.md`.** The product's value is defined by the people and actors it serves: a missing persona means missing value and missing measurements. If a user type turns up that the file does not hold, add a persona with context, goals, pain points and constraints (examples of user types that can be missing: a reviewer approving AI-written changes, a team lead setting library and architecture standards, a newcomer reading an unfamiliar codebase, a cheaper or smaller model acting as an author). Then check whether existing or new cards should serve them, and say so in the retro entry.
- **Product feedback is gathered from each available model, in parallel and cheaply, as evidence and not
  opinion polling.** Each model reads the current `docs/GRAMMAR.md` plus a sample `.avowry` file, then
  writes or modifies a small phrase. It reports where it struggled (ambiguity, wasted tokens, misread
  intent, a missing construct) and proposes **at most 2 improvements, each with a concrete example**.
  Feedback-gathering turns are read-only and write no files in the repo.
- **These ideas are raw input.** The PM, with the domain expert (the language designer), dedups them and
  weighs value against effort and fit with Phrase's principles ("the code is the context", specs are
  truth). Winners are refined into Connextra + ZOMBIES cards and ranked on the board. **Every rejected
  idea is recorded with the reason it was rejected** (in the retro entry).
- **After each retro the facilitator reports to product owner** which cards came from model feedback and their
  board ranks.

### Escalation (standing rule, product owner, 2026-10-25)

**Principle:** escalate only questions about the language's overall purpose or unresolved model conflicts, because the mob owns implementation decisions and the Decider handles disagreements first.

- **The mob decides everything else itself:** features, syntax details, priorities, implementation and
  tooling. Do not ask product owner.
- **Model disagreement** (retro product feedback, design debates, reviews): run the Decider protocol first.
- **Whether anything escalates is itself decided by a Decider.**
- **Escalate to product owner ONLY if** (a) the question is about the language's overall purpose or direction
  (what Phrase is for, or who it serves), or (b) a model conflict the Decider could not resolve.
- **Format:** one short message with the options, each model's position in one line, and a recommendation;
  answerable in one word.
- **Safety carve-out (does not widen what gets escalated):** actions outside the mob's authority still need the product owner's
  explicit OK before execution, presented as a one-word ask: destructive or irreversible changes to production or user
  data, spending money, security or privacy exposure, and external commitments. Phrase specifics: publishing a package
  or release (an external commitment) and any spend beyond the prepaid driver allotments.

### Decider protocol (the default for every team decision)

**Principle:** use the Decider for team decisions as well as conflicts, because simultaneous votes expose disagreement before work proceeds.

**What goes through it:** the task force (at kickoff and every mid-card addition), card refinement and acceptance
criteria, PM ranking and re-ranking, design and architecture choices, keeping or reverting a feature on benchmark
data, declaring a card Done, milestones and their targets, retro picks and process experiments, trust-ladder
promotions and demotions, benchmark suite changes, and whether anything escalates to product owner.

**Not a Decider:** the driver's moment-to-moment implementation choices inside an increment. Those are covered by
independent review, and any reviewer objection becomes a Decider proposal.

1. **Proposal.** The PM, facilitator or a role owner states ONE concrete proposal.
2. **Simultaneous vote.** The task force votes at the same time, in parallel, each choosing **Yes**,
   **Support-it** (not my first choice, but I will back it) or **No**. Seats do not see each other's votes first.
3. **Outliers.** Each **No** voter says what it would take to get to yes, in a short reason; only outliers explain.
   The proposer may amend, and the amended proposal is voted once more.
4. **Result.** An "absolutely no" from any seat, or outliers still unresolved after the amendment, means the
   proposal **fails**. Otherwise it passes. One round, no long debate.
5. **Record.** Keep one concern per history: append the complete `Decider D<n>` record to its area file under
   `docs/decisions/`, then add one `docs/decisions/areas.json` entry with its id, area, status, title, and any
   `also` areas. Related areas link to the owning record instead of copying it. The decision-index check enforces
   ownership, completeness, valid supersession, summaries, and the generated index. A failed proposal remains
   history and may be raised again at the next retro.
6. **Independent re-run before recording (guardrail from the repeated Decider findings; audit 13, finding F-038).** Every Decider result, passed or failed, is re-run by fresh seats (each role in a new context, never one that voted before) on the final text before it is recorded; the record shows both runs and states any difference.
7. **Keep it lean.** Votes are short and collected in parallel so the Decider does not drain the driver allotments.

- **Run every Decider with the sandboxed runner** (`decider.py` in mob-defaults): each seat votes in its own throwaway clone of the
  repo, never in the real working tree, with the CLI's read-only mode where one exists; every seat gets the same full prompt; votes are
  parsed in one place; the real tree and HEAD are verified unchanged afterwards. (A voter once ran with write access in the real tree
  and edited source files; "read-only" in the prompt is a promise, the sandbox is the mechanism.)

**The task force is a Decider.** The initial task force is a Decider proposal from the facilitator or PM, so any seat
can object to a missing or padded role. A role proposed mid-card needs a Decider too: the proposer states the role, its
assignee and why it is needed; if it passes, the role is added to the card's `## Task force` with where it joined and
a pointer to the record (`| joined: cycle 2 | decider: D3`). Card mode refuses a mid-card role that carries no
`decider:` reference or whose Decider record is missing or did not pass.

### Milestones and hypothesis-driven work (standing rule, product owner, 2026-10-25)

**Principle:** the mob sets measurable milestones and tests feature hypotheses with data, because evidence makes progress and product value challengeable.

- **Self-set milestones.** The mob sets its own milestones on the board (`board/MILESTONES.md`). Each has a
  **measurable target** and an **automated check**. Progress against them is reviewed at every retro, and
  the next milestone is set when one is reached. product owner hears only when a milestone is reached, plus Usable v1 itself.
- **Hypothesis, or no card.** Every feature card (especially one that came from model feedback) carries a
  hypothesis in exactly this form: *"We believe <change> will <improve metric> for LLM authors/readers. We'll know when
  <metric> moves from <baseline> to <target> on <benchmark>."* Bugs and plumbing cards are exempt; a plumbing
  card names the milestone bar it advances instead.
- **Data, not opinion.** The LLM benchmark harness (card 016) is the instrument for everything else and is ranked high. Results are stored in the repo as a history so trends can be compared.
- **The loop.** Before building a feature, record its baseline on the benchmark. Afterwards, re-run it.
  Keep the change if the hypothesis held. Revise or revert if it did not, and record why in MOB.md (not during the bootstrap phase, below).
  Decider disputes are settled with data where possible. Retro ideas are triaged by expected metric impact.

### Process ideas are hypotheses, not doctrine

Any process change idea, from product owner, a model or a retro, enters as an **experiment**, not a rule. Each experiment has:

- a **hypothesis** ("We believe <change> will <improve metric> for <who>. We'll know when <metric> moves from
  <baseline> to <target>.")
- a **metric and a counter-metric** (Goodhart guard)
- a **trial scope**: which cards, or how long

The retro then decides **keep, revise or drop** through the Decider (on judgment, not metrics, during the bootstrap phase). **Only kept experiments are written into this
agreement as rules.** Explicit standing rules from product owner still apply directly, without a trial. Running experiments
are logged in `docs/experiments/` (one file each) and in MOB.md, not in this agreement.

### Bootstrap phase: no metric-based rejections until a baseline is established (standing rule, product owner, 2026-10-26)

**Principle:** do not reject ideas for failing to improve a metric until a baseline exists, because improvement cannot be measured against nonexistent evidence.

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

### Metrics are evidence, not targets (the Goodhart guard)

The aim is value for a persona, not a number. Teams go wrong when they pick a specific measurement and adjust behaviour to reach it: that puts the cart before the horse and creates perverse incentives (the cobra effect). A measure that becomes a target stops being a good measure.

- **Start from value.** Every improvement card states the value it is meant to create for a persona (for example "a reviewer can tell intent from a diff faster"), not a figure to reach (for example "tokens at 75 percent of the baseline"). The hypothesis says why the change should create that value; the data checks whether it did and what it cost.
- **Metrics are evidence and trade-off detectors, not targets.** Measure the effect honestly and report what improved, what got worse and by how much, then judge whether the value was delivered. A number moving is not success on its own, and a number not moving is not failure on its own.
- **Keep honest measurement hygiene.** Pre-register what will be measured and how, so results cannot be cherry-picked or re-scored after the fact. Keep holdouts and counter-metrics, and rotate or refresh tasks so nobody tunes the project to the suite. A pre-registered threshold is a guard against self-deception, not the goal and not the verdict.
- **Verdicts are judgments backed by data.** A pass or fail stamp against a frozen bar is not the verdict. Example: "Phrase costs about five times the tokens on tiny tasks, makes no accuracy difference for strong models, raised the weakest model to 100 percent and caught every planted bug where specs existed. So the bet on verifiability is supported, cost is a trade-off to work on, and beating the baseline was never the goal."
- **The suite is versioned.** Changing tasks, scoring or prompts needs a Decider run and a note in the status file, and is never bundled with the feature it measures.
- **No special-casing the benchmark.** No grammar or compiler hack that only helps benchmark tasks. Reviewers check this explicitly.
- **Qualitative check.** Every metric win also needs a model to read the change cold and explain it, and the change is sanity-checked against the product's principles.
- **Watch for perverse incentives in retros.** If a card optimises a metric without improving the value, or a metric improves while its counter-metrics or holdout degrade, call it out as metric gaming, not a win.
- **Same for flow metrics and trust-ladder evidence:** signals for retro discussion, never quotas.

### Task force before every card (TFDD, standing rule, product owner, 2026-10-25; revised same day)

**Principle:** declare and assign every applicable role before work starts, with no role-count quota, because task-force fit matters more than size.

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
- **The task force is living, not frozen at kickoff.** Add a newly needed role through the Decider because the work can reveal expertise that was not visible at assembly. Any member can flag mid-card that a role is needed (a security question surfaces, the work turns out
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
- **Sized to the task, changed by need, removed at the retro (product owner clarification, 2026-10-29).** For every card:
  1. **Declare the task force up front, sized to the task.** Each member has a role, a domain or context (what knowledge or perspective it brings), an assignee and a why. The card-mode seats (ready, slice, red, green, ship, signoff) are a default starting point, not a fixed roster: roles the card does not need are left out with a reason, and roles it does need are added.
  2. **Ask the domain-expert question and record the answer** at refinement and at every cycle boundary: should a member with different domain knowledge or context join now? The answer is yes (with the member proposed) or no (with a reason), never blank.
  3. **Every mid-card addition goes through a Decider** and is recorded with its reason, the boundary where it happened and the Decider id. A default seat the card does not need is left out when the task force is declared, before the card starts, with a reason.
  4. **No removals mid-card.** Once the card is underway, members are only added; removal happens only at the retro, which decides keep or remove for each member that is no longer needed and records the decision with its reason.
- **Accountability.** The trail record `.mob/records/NNN.json` carries the declared task force (role, domain or context, assignee, context id, why), the domain-expert answer at refinement and at each cycle boundary, each mid-card addition (boundary, reason, Decider id), each seat left out at declaration with its reason and the retro's keep or remove decision for each added member. The in-repo checker rule for these fields is decided by the findings-mirroring Decider. The external auditor FAILs independently: an undeclared task force, a missing domain-expert answer at any boundary, an unrecorded change, or any removal outside the retro.

### CI on main is part of the line (standing rule, product owner, 2026-10-28)

A green local gate is not a green main: the remote CI runs on a different operating system and a clean machine, and a trunk that nobody sees failing is a trunk that does not gate anything. The principle is that a constraint must report its own failure to the people who are responsible for it; a failure found by email is a process defect.

- After every push to main the facilitator runs `python scripts/check-ci-main.py --wait 1500` (bounded). Green or still pending is fine; red is **stop the line**: no other card starts, the next card is the fix, and the product owner is told in the next report.
- The check also runs at the start of every loop iteration, so a red main left by anything (a late run, a merge, a flaky environment) is caught within one card.
- Every test that launches a fake model, a shell or a path must work on both Windows and Linux (the CI is Linux); a test that can only pass on one is a defect found by the CI, not an excuse to ignore it.
- A red CI is logged as a bug card and covered in the next retro (why it went unseen).

### Driver availability (standing rule, product owner, 2026-10-28)

Never assume a driver is permanently out. A driver that is out of quota or otherwise unavailable is re-probed, not forgotten:

- **When:** at every card boundary, and on a timer while any driver is out (every 30 to 60 minutes, or at its known reset time, whichever is sooner). Also before any measurement run that depends on a model.
- **How:** a cheap availability probe per driver that reads status and spends as little quota as possible (for Copilot, the account's quota endpoint reads usage and spends none; for the others, a one-line prompt through the same launch path the harness uses). Every probe has a short timeout, writes its result and the probe time to the driver status table in DRIVERS.md, and a failed probe is logged with its error text (an error text that names a reset time is recorded as the reset time).
- **When a driver is back:** restore its author and voter seats in the rotation, rerun the rows that were marked unavailable for it (a benchmark run is not complete while a model it needs has unavailable rows and the model is back), and log the event in DRIVERS.md (and in MOB.md when the mob keeps one).
- **While a driver is out:** its seats rotate to the others, Deciders still seat every role (see Staffing: roles over models), a vote needs a voter who is not the author, and the verdict of a measurement states the unavailable driver and the reset time. A frozen rule that needs the driver (for example a minimum number of models) is never changed to work around the outage.
- **Local seats are supported.** A model running on the machine (a local inference server plus a headless coding agent, configured as an entry in the mob tooling's local model config) is a driver that never runs out of credits. It enters at the bottom of the trust ladder, and it loads only while the test gate is idle: the wrapper checks the gate lock and any running test process first, starts the server on demand, stops it by its own process id, never runs during a gate and never stays resident. Only one large model is loaded at a time. Adding another local model is a config entry, not new code.
- **Visible state:** DRIVERS.md in the repository root carries the driver status table (driver, status, last probe, known reset, note) and a short event log; MOB.md may link to it.

### Staffing: roles over models (standing rule, product owner, 2026-10-28)

Roles are the primary perspectives; models are the staff who fill them. Diversity of model matters, but diversity of roles matters more, because each role asks a different question of the work.

- **Roles first.** The task force names the roles for a card (product manager, architect, author, tester, reviewer, domain expert, security and so on). Every role is seated for every card, whatever models are available. Decider votes and reviews are seated BY ROLE: a vote does not shrink to "two seats" because a model is out; each role is filled by an available model.
- **Decider seats are by role, never by model.** When a model runs out of credits, the Decider still seats every role: each seat is filled by an available model in a fresh context, and one model may fill several role-seats (never reviewing its own authorship). A vote is never taken with fewer seats because a model is out. The decision record names which model filled which role-seat. Why: the diverse perspectives come from the roles, and shrinking the vote when quota runs out quietly weakens every decision.
- **Work never stalls for lack of a specific model.** When a model is out of quota, its roles are reassigned to available models. In the extreme one model fills several roles, each in a fresh context with its own role charter and without the other roles' reasoning, and Claude covers if every other driver is out.
- **Claude is a normal member of the rotation** (Sonnet by default, a stronger model only when the work needs it). It can fill roles, author, review and vote alongside Codex, Copilot, Grok and agy; in particular it takes seats while another model is out so that votes and reviews keep a full set of role perspectives. The only limit: do not let Claude carry everything while other models still have quota. It becomes the sole driver only when all the others are out.
- **How Claude is seated.** A seat is a fresh context given the role charter, the proposal and read access to the committed files, nothing from the facilitator's own reasoning. Where the headless CLI is signed in it is a driver like the others; otherwise the facilitator seats it as a fresh-context subagent. The log names the model and the role for every seat.
- **The whole team is every role, each in its own context (product owner clarification, 2026-10-29).** Whole team means all of the team roles, not necessarily every model. Good refinement comes from the different perspectives of separate agent contexts, so every role on a card, in every Decider and in every retro is seated in its own fresh context with its own role charter. Models rotate across the roles as much as availability allows; when the other models are out, a fresh Claude subagent per role is a legitimate seat. Retro feedback is gathered from every role's perspective, one context per role.
- **Quota first, then Claude (product owner rule, 2026-10-29).** The team must not slip back into seating only Claude while other models still have quota. At every card start Codex, Copilot, Grok and agy are re-probed (no probe older than 60 minutes at card start, with the probe evidence captured), and every model with quota is seated before any seat is filled by a Claude context. A Claude seat while another model had quota needs a stated reason in the trail record; without one the external auditor FAILs that seat. Reset pollers run for every driver that is out. When all quota is used by the other models, Claude only is acceptable, but all of the roles must still be filled: every role gets its own fresh Claude context. Role coverage and context independence never relax.
- **Repeat guardrail on staffing (audit 13, finding F-037, the second staffing failure after card 195).** Codex, or any non-Claude model with usable quota, is seated first in every role-seat it can take, each role in its own fresh context; one model filling every seat is acceptable when it is the only model with quota. Every Claude seat carries a recorded reason (for example: no non-Claude model has usable quota, or the role needs a model family different from the author's). Deciders count: a Decider seat is a seat.
- **No seat without fresh probes (audit 15, finding F-040, the third staffing failure).** Before every seat assignment, including every Decider seat, review seat and retro voice, the team checks that the Codex, Copilot, Grok and agy probes are under 60 minutes old and re-probes first if any is older; the probe evidence is committed under .mob/records/probes/ and DRIVERS.md cites it. This applies to the whole team, not only to card starts.
- **Rules that do not change.** One context never holds two roles, and above all never author and reviewer of the same change. The author of a change never votes on it; reviews are non-author by role and by context; one model may hold several roles on a card only in separate fresh contexts. A reviewer from a different model than the author is preferred; when no other model is available, a fresh context of the same model reviews, and the record says so.
- **Model diversity still matters, but never blocks.** Prefer spreading roles across different models when available. For Phrase's benchmarks the models are the customers and the test subjects, so which models were run is part of the evidence. Claude is a reasonable extra model for the benchmarks if the team wants another data point, decided by Decider and pre-registered as usual.

### Decisions stay reversible (standing rule)

**Principle:** with fewer models available to review, a wrong call is more likely, so the safety net is cheap reversal. Small, atomic units of work are therefore even more important.

- **Each card, commit and decision is small enough to revert cleanly on its own.** A card that cannot be reverted without side effects is split until each piece can be.
- **Prefer two-way doors.** A decision that cannot be undone cheaply is flagged as a one-way door and escalated as the Escalation section already says; everything else is decided by the Decider and recorded with how it would be reversed.
- **The record says how to undo it.** A Decider record for anything touching shared state (a published file, a frozen document, a board field, a benchmark baseline) names the revert step, or says it is a one-way door.

### Driver rotation

- **Rotation order: Codex → Copilot → Grok → agy, with Claude a normal member (see Staffing: roles over models).**
  Claude is the sole driver only when every other driver is out; the log says why a seat went to whom.
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

**Principle:** provide a way to regain trust and responsibility because an underperforming collaborator can still contribute with appropriate support. **Coach, never bench.** A quality problem
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
| Codex | 1 | 1/3 | 2026-10-25 cycles 2-3 NOT clean: Grok review CHANGES (throws, missed PH312, option coercion); count unchanged | Cycle 1 clean (Copilot sign-off). 2026-10-29 card 195 (card mode, two cycles, Codex in every seat): Claude Sonnet 5.5 signoff Yes with fixes (one out-of-scope line, one silent-failure gap); the same model also did the card-mode QA signoff of its own green, which an independent audit flagged; count unchanged (D138) |
| Copilot | 1 | 0/3 | 2026-10-25 authored a47ed6a (specs + fixture, gate 221+1 re-run by facilitator); awaiting independent review | Reviewed cycle 1: APPROVE |
| Grok | 1 | 0/3 | 2026-10-25 reviewed 59d8a9c+84b5737: CHANGES with red-check evidence (good review) | Drives hardening cycles next |
| agy | 1 | 0/3 | none | New to this mob; model gemini-4-argon-high (3.1 Pro not offered) |
| Claude (last resort) | 1 | 0/3 | 2026-10-29 card 195: non-author signoff review (Yes with fixes) and review fixes as a small TAKEOVER (D138) | CLI not signed in (re-checked 2026-10-29); seated as a fresh-context subagent |

Ledger note (catch-up retro, 2026-10-29): this ledger was not maintained from 2026-10-25 to 2026-10-29; no misreport or safety violation by any driver was found in that period, so the rungs above are unchanged, but the clean-cycle counts do not include the cards of that period. From card 195 on, the per-card trail records in `.mob/records/` carry the evidence for the ledger and each retro updates it.

### Safety

- **Never kill a process by name or pattern**; only by exact PID, and only your own.
- **Never `rm -rf` (or `Remove-Item -Recurse`, `rd /s`, `git clean`) on a variable or computed path.**
  A delete is allowed only on a literal path inside the repo's scratch directory, after asserting it is
  non-empty and absolute.
- **Detectors (product owner: constraints must self-report; a ban with no detector isn't in force).** Both bans above are enforced
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
- **Board:** the Phrase Board on GitHub Projects (project 1 under Programming-Made-EZ, private) is the single source of truth. `board/` is a generated read-only archive (`scripts/board/generate_archive.py`), one `NNN-slug.md` per card. Card text is edited on the issue. The loop and the retro re-rank hook write Rank and Status to the project only, after a passed Decider, skipping any card whose Rank is newer than the Decider's snapshot. Cards carry a Connextra story, ZOMBIES acceptance criteria and a "Done when" command list the facilitator runs.
- **Feature-flag policy (D136):** a flag is a named default-off feature registered in `docs/FLAGS.md` (name, owner card, what off means, the removal card or condition). The CLI reads `AVOWRY_FEATURES` (comma list) or `--enable NAME` through one `FeatureFlags` class. Incomplete user-visible behaviour (new syntax, a new command or an output change, a risky generator change) ships behind a flag or stays unreachable (not wired into an entry point); one test proves off leaves the output unchanged and one proves on. Atomic renames, refactors and documents need no flag. Flags of Done cards are removed.
- **Release target:** none yet (no published package). The mob decides release and grammar-compatibility
  questions itself under the Escalation rule below.
- **Code needing extra care:** `docs/GRAMMAR.md` (the language contract; a grammar change needs a recorded
  reason), `Semantics/Model.cs` (the IR the emitters consume; change only with a reason in the reply), and
  diagnostic messages (an LLM reads them to fix its own code: name the thing, say what was expected, list
  the valid options).
- **Domain rules:** the tests are the spec. If one looks wrong, say so in the reply instead of quietly
  editing it. The resolver must never throw, even on source with syntax errors.
