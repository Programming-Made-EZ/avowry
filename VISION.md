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
*Interpretation (to confirm with Steve):* a converter between the LLM-optimized Phrase form and a human-friendly
rendering, in both directions. It pairs with synthesis: both are translations between representations. A **future**
milestone (M6). An alternative reading, one-way human-readable documentation generated from Phrase, is noted; the mob
will confirm which Steve means before building anything.

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

## How this relates to the rest

- Design rules and the grammar: [docs/GRAMMAR.md](docs/GRAMMAR.md). Its rule 3 ("small fixed core, open phrase
  library") is reconciled with this vision by the keyword model ADR (card 022): keywords are the standard-library
  phrases that ship with Phrase.
- How the team works: [TEAM_WORKING_AGREEMENT.md](TEAM_WORKING_AGREEMENT.md). The board and milestones
  ([board/MILESTONE-usable-v1.md](board/MILESTONE-usable-v1.md), [board/MILESTONES.md](board/MILESTONES.md)) are
  assessed against this page.
