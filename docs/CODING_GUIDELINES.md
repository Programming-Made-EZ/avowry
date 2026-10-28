# Coding guidelines for this repository

These rules cover the code of this repository: the compiler, the adapters, the command line tool, the scripts and the tests. They are different from the project-profile coding standards that Phrase applies to the code it generates for users (see `docs/adr/ADR-library-binding-layering.md`); that profile describes the output, this file describes the tool that makes it. Reviewers check every change against this file.

Principle for every rule below: a reader, human or a fresh-context seat, should be able to understand and change one thing by reading only that thing. The rules exist to keep each unit cohesive, its dependencies few and pointing one way, and its behaviour pinned by a test that can fail.

## Design

- **High cohesion, low coupling.** A unit (function, type, file, document) has one reason to change. If describing it needs the word "and", split it. Why: small cohesive units are cheap to read, test and revert.
- **Dependencies point one way.** Lower layers never know about higher ones. The order is lexer, parser, resolver and model, target adapter, command line, tooling. A lower layer exposes data or an interface it owns; a higher layer plugs in. The generated code is the lowest layer of all and must not know about the compiler that produced it or the tools that trace it (Decider D111). Why: an inverted dependency makes the low-level artifact change whenever the tooling changes.
- **Small functions.** Aim for about 20 lines; past 30 is a sign of several responsibilities. Prefer at most two parameters; when more values travel together, name them as a record. Why: short functions are testable one behaviour at a time.
- **No flag parameters.** A boolean that switches behaviour is two functions. A function either computes and returns, or performs an action, not both. Why: callers read the call site, not the body.
- **Immutable data.** Syntax trees, models and diagnostics are records that are never mutated after construction; build a new value instead. Why: stages stay independent and results are reproducible.
- **Separate policy from mechanism.** Choices that vary by target, library or environment live in an adapter or an option, not in the core.

## Naming

- Functions and methods are verbs for one action; types are nouns; booleans read as predicates (`IsDeclared`, `HasFailure`). Avoid `Manager`, `Helper`, `Util` and `Data` as names: they hide the responsibility.
- No abbreviations except universally known ones (`id`, `url`). Diagnostics keep the existing `PHnnn` code style, one code per distinct problem.
- A name states what a thing is or does, not how it is used today.

## Errors and determinism

- **The compiler never throws on user input.** A bad or hostile source file produces a diagnostic with a code and a location, not an exception. Exceptions are for programmer errors only. Why: a tool that crashes teaches nothing and hides the real problem.
- **Output is deterministic and portable.** The same source gives the same bytes on Windows and Linux and from any working directory: no timestamps, no absolute paths, no culture-dependent formatting, no dependence on line endings or file ordering. CI runs on Linux, so a test that only passes on one platform is a defect.
- **Fail loudly at boundaries, trust inside.** Validate at the edge (files, arguments, model replies); inside, rely on the types.

## Tests

- **Test first.** Watch the test fail for the right reason before writing the code; a red that survives when only the fix is reverted proves the test measures the change.
- **A test must be able to fail.** Before accepting a test, name the change that would break it, and check it when the check is cheap. Assertions that cannot fail (a check on something that cannot contain the value, a count that is never zero, a regex that matches anything) are deleted or replaced. Why: a test that cannot fail is false assurance.
- **Name the behaviour.** A test name is a sentence about observable behaviour (`Unknown_field_in_an_update_reports_a_diagnostic`), not the method under test. One behaviour per test.
- **Given, When, Then comments** mark the three parts; the setup, the single action and the observable result. Cover zero, one, many, boundaries, interface and exception paths for each behaviour.
- **Assert the exact result.** Prefer the full expected value over a pattern; when a pattern is needed, anchor it.
- **Keep tests independent and fast.** No ordering between tests, no sleeps, no shared mutable state, no network. Use fakes for time, processes and models. Nested tool runs stay disabled for node reuse and are stopped by exact process id.
- **Traits tie a test to its card** (`Slice`), so the card's check can count them.

## Code hygiene

- **No dead code.** Delete unused code, commented-out code and stale options; history keeps them. Why: unread code misleads the next reader.
- **No hidden coupling through strings.** Shared names (keywords, diagnostic codes, failure names) have one definition that others reference.
- **Leave touched files cleaner, in scope.** Rename a poor name, extract a helper, remove dead code in the files a change touches. Code that a change impacts is part of the change: fix a test the change turns red, do not hand it to someone else.
- **Small, atomic, revertable commits.** One concern per commit and per card, so any decision can be undone cleanly. Stage explicit paths only.
- **Comments say why**, never what; the code says what. A comment that repeats the code is deleted.

## Documents

- One concern per document; a reader loads only what they need. Cross-references are links, not copies.
- State a direction as a principle with its rationale, never as who asked for it; use the role "the product owner" where an owner must be named.
- Generated files carry no knowledge of the layers above them.

## Review checklist

A reviewer confirms, for the change in front of them:

1. Each new unit has one responsibility and a name that says it; no function over about 30 lines without a reason.
2. Dependencies point downward; nothing low-level learned about something above it.
3. Bad input produces diagnostics, never an exception.
4. Output is deterministic on Windows and Linux.
5. Every new test could fail: the reviewer can name the breaking change; no assertion is vacuous.
6. Test names describe behaviour; Given, When, Then comments are present; edge and failure paths are covered.
7. No dead code, no commented-out code, no duplicated shared names.
8. The change is small enough to revert alone; unrelated cleanup is not mixed in.
9. Documents touched hold one concern, link instead of copying, and give rationale.
10. The full gate was run alone and CI on Linux is green.
