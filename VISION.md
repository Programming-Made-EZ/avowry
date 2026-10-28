# Vision: the north star

Product decisions are judged against this page. It records Steve's purpose ruling (2026-10-25).

> "it's a language where the specific keywords are defined or common patterns that can easily be produced by an LLM for any implementation language. Things like loops and if/else are ordinary. we want to create a comprehensive set of keywords so that we minimize the amount of work an LLM has to do in order to build and understand the codebase of any language. this makes it both a spec as well as a library."
>
> Steve Bishop

## Purpose

**Phrase is a vocabulary of common patterns that an LLM can read and write for free, in any implementation language.**

- **Keywords are higher-level patterns, not primitives.** A keyword names something programmers do over and over
  (paginate a list, retry with backoff, validate a field, create-read-update-delete a record). Ordinary control flow,
  loops and if/else, is just ordinary: it is there, it is simple, and it is not where the language earns its keep.
- **Every keyword can be produced reliably, in any implementation language.** A keyword is defined once, by its
  sentence-name, its typed holes and its specs. Adapters turn it into each target language's idiomatic code (C#
  first); a keyword without a hand-written implementation is produced by an LLM, pinned by hash so the output is
  reproducible, and held to the same specs.
- **The goal is a comprehensive vocabulary that minimizes the work an LLM must do** to build a codebase (fewer
  tokens written, fewer native lines, fewer attempts to green) **and to understand one** (the names are the index,
  and the code is its own context).
- **Phrase is both a spec and a library.** As a spec, Given/When/Then is the truth, so a name cannot lie. As a
  library, the keyword catalogue carries spec-backed definitions and per-target implementations that every project
  reuses. One source compiles to many targets.

## Direction rulings (Steve, 2026-10-25)

**1. Synthesis (future).** Steve: "in the future it would be useful to synthesize existing code."
*Interpretation:* a reverse direction. Existing code in any language is read and Phrase is synthesized from it by
mapping the code's patterns to keywords. A **future** milestone (M5, after Usable v1 and Vocabulary v1; it displaces
nothing).

**2. Adapters.** Steve: "we should have adapters for each language."
*Interpretation:* one adapter per target language. The word "provider" in the code and docs becomes "adapter"; the C#
provider is the first adapter.

**3. Base library plus adapters.** Steve: "a base library for universal patterns and adapters that add keywords for
those languages."
*Interpretation:* the **base library** holds language-neutral universal pattern keywords, each with specs. Each
**adapter** (a) implements every base keyword for its language, verified by the shared specs, and (b) may add
language- or ecosystem-specific keywords of its own, also spec-backed. C# is built as the first adapter, not as a
one-off emitter.

**4. Human readability.** Steve: "I could actually see potential for a compiler here. something that converts between
the two."
*Confirmed by Steve (2026-10-25):* the converter translates between the LLM-optimized Phrase form and a
human-friendly rendering, in both directions. It pairs with synthesis: both are translations between
representations. A **future** milestone (M6, card 031).

**5. Consumer-observable outcomes (2026-10-26).** Steve, looking at a benchmark task whose spec reads `then succeeds`:
"I feel like we aren't adhering to consumer related output. 'succeeds' isn't really a result the customer gets value from."
*Interpretation (the team refines it into cards):* a spec's `then` should state an observable outcome that has value to the
consumer: what is returned, stored, emitted or responded. A bare "no error" proves almost nothing, and a phrase whose only
outcome is "does not throw" may be a weak abstraction (should a validation phrase return the validated or normalized value?).
A phrase that declares `fails with X` should have a spec that shows X, since the failure path is the most important
behaviour to specify. Status: direction; the grammar and linter rule, the audit of existing examples and benchmark tasks, and
the validation-return question are raw input on the board.

**6. Agile: iterate and increment, do not over-specify up front (2026-10-26).** Steve: "Just for general direction, I want this to aim toward agile philosophy. It's a common temptation to skew toward spec-heavy up-front planning. We should avoid this temptation despite building effectively a spec for planning. Iteration and incremental is the key to successful development."
*Interpretation (how to apply it is the mob's call):* a guiding principle for both the product and the process. **Product:** Phrase specs support emergent, just-in-time design that grows one slice at a time; they must not demand big up-front specification before something works. **Process:** cards evolve the language through small, working increments; a card is only as specified as the next slice needs, and a plan that is mostly written specification before any working increment is the temptation to avoid, even though the board is effectively a spec for planning. Used as a lens at every retro: where are we over-specifying up front? Status: direction (guiding principle).

**7. What Phrase is measured against (2026-10-28).** Steve: "I don't think we wanted to beat C#. we wanted to solve the problems identified earlier." The problems: AI-written code is unaccountable; teams cannot verify it as fast as it is written; intent vanishes with the prompt. **Phrase's success criteria are verifiability, preserved intent and reviewability, not out-performing C# on raw token cost.** The head-to-head benchmark against plain C# (pass rate, attempts, tokens on small tasks) stays as honest context and keeps being published, but it is not the success measure. The measures that are: the undetected-defect rate (how often planted defects survive each approach's checks), intent recovery (can a fresh reader state what each unit must do, including edge and failure cases) and review speed and accuracy (how fast and how correctly a reviewer approves or rejects AI-written changes). Each is pre-registered by a Decider before any row.

## What this commits us to

- The catalogue is a first-class artifact: every keyword has a name that is a sentence, typed holes, specs that run as
  tests on every target, and an implementation per adapter.
- The core stays small and boring; the vocabulary grows by evidence, not by taste. New keywords come from patterns
  found in real codebases and real LLM tasks.
- LLMs are the customers. The vocabulary is judged by what a model does with it.

## Goodhart guard: "comprehensive" is not a keyword count

Coverage is judged by **how much LLM work the vocabulary removes on real tasks**, measured on the benchmark, at equal
or better pass rate: tokens the model writes, native-code lines it needs, attempts to green. Every such target metric is
paired with counter-metrics:

- **vocabulary bloat and near-duplicate keywords** (the dedup linter flags two keywords that do the same thing), and
- **learnability** (a cold model, given only the docs, finds and uses the right keyword).

A bigger catalogue is not a win. A smaller catalogue that removes the same work is a better one.

This paragraph measures the vocabulary's economy (LLM work removed). It is context, not the success measure of Phrase: since ruling 7 (2026-10-28) the success criteria are verifiability, preserved intent and reviewability, and the economy metrics are reported honestly beside them without defining success.

## How this relates to the rest

- Design rules and the grammar: [docs/GRAMMAR.md](docs/GRAMMAR.md). Its rule 3 ("small fixed core, open phrase
  library") is reconciled with this vision by the keyword model ADR (card 022): keywords are the standard-library
  phrases that ship with Phrase.
- Who a story is about: [PERSONAS.md](PERSONAS.md). A story names one of these personas (not a role) and its "so that" is value to that persona (Steve, 2026-10-26).
- How the team works: [TEAM_WORKING_AGREEMENT.md](TEAM_WORKING_AGREEMENT.md). The board and milestones
  ([board/MILESTONE-usable-v1.md](board/MILESTONE-usable-v1.md), [board/MILESTONES.md](board/MILESTONES.md)) are
  assessed against this page.
