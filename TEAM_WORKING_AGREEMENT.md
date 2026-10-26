# Team Working Agreement: Phrase

Seeded from the mob-crosscli default template (`templates/TEAM_WORKING_AGREEMENT.default.md`). The
facilitator refuses to start a card in a repo without this file.

How we work lives here. Process improvements land in **Practices** at a retro; they are never board cards.
The mechanics (flags, scripts) live in the mob-crosscli docs. Seeded from the play-tracker mob's agreement,
generic parts only. Adopted 2026-10-25 by the mob facilitator on Steve's standing instruction that every mob has an agreement.

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
- **Escalate to Steve ONLY if** (a) the question is about the language's overall purpose or direction
  (what Phrase is for, or who it serves), or (b) a model conflict the Decider could not resolve.
- **Format:** one short message with the options, each model's position in one line, and a recommendation;
  answerable in one word.

### Decider protocol (for conflicting views)

When models disagree (in retro product feedback, design debates or reviews), run the Decider first.

1. **Proposal.** The proposer states ONE concrete proposal.
2. **Simultaneous vote.** All voting seats vote at the same time, each choosing **Yes**, **Support-it**
   (not my first choice, but I will back it) or **No**. Seats do not see each other's votes before voting.
3. **Outliers.** Each **No** voter (an outlier) says what it would take to get to yes; the proposer may
   amend the proposal, and the amended proposal is voted again.
4. **Result.** An "absolutely no" from any seat, or outliers still unresolved after the amendment, means the
   proposal **fails**. Otherwise it passes.
5. **Record.** Every Decider run is recorded in MOB.md: the proposal, each seat's vote, and the result.

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
