# Personas

Steve's story standard (2026-10-26, on card 003): *"this is a role, not a persona."* A Connextra story names a **persona**, a specific user with context, goals and constraints, not a generic role such as "an engineer (or an LLM)". The "so that" is **value that persona gets**, not an internal design goal. ("C# is the first adapter, not a one-off emitter" is architecture, not user value.) This pairs with VISION ruling 5: specs state consumer-observable outcomes.

The set below was drafted by the team (language designer, PM, UX lenses; Decider D27) from [VISION.md](VISION.md) and the LLM-customer feedback in [docs/retros/process-2026-10-26.md](docs/retros/process-2026-10-26.md). It is deliberately small. A story names one of these by name.

## Ari: cold-start coding agent

An LLM coding agent asked to build or extend a service, starting with only the repository, the Phrase documentation and the task. This is the benchmark situation (write, read, modify and fix tasks), and the primary customer: *LLMs are the customers.*

- **Goals:** find the right vocabulary without prompt-specific coaching; express the behaviour in few tokens and few native-code escapes; produce code and consumer-observable specs that compile and pass in few attempts; understand an unfamiliar Phrase program from its sentence-names and specs.
- **Pain points:** ambiguous syntax or duplicated vocabulary that burns context and retries; specs that say only that nothing went wrong; diagnostics with no location or fix; two documents that disagree.
- **Constraints:** a limited context window and bounded attempts; no private knowledge of the codebase; must act from the message and the docs; needs concise, deterministic feedback; works across target languages through documented adapters.

## Dana: developer directing an agent

A developer who owns a service outcome and directs an LLM agent (Ari) to create or change it with Phrase. Dana does not write the service by hand.

- **Goals:** state the behaviour once and get a working implementation in the chosen target language; review the Phrase source and its specs without reconstructing generated details; reuse standard, spec-backed patterns; change target language without changing the intended behaviour.
- **Pain points:** native glue code and adapter knowledge that distract from the outcome; weak specs that pass while the requested behaviour is absent; unclear failures that make supervising the agent slow; one-off emitters that undermine confidence Phrase is portable.
- **Constraints:** little time for supervising retries or reading generated code; must keep control of requirements and externally visible behaviour; needs repeatable builds and stable generated output; judges a slice by one command's exit and the spec result.

## Morgan: maintainer and reviewer

A person, or a second model with a fresh context, who reads or inherits Phrase source and generated code after the author has moved on.

- **Goals:** infer intent from phrase names, types and Given/When/Then specs; verify what is returned, stored, emitted and responded, including failure paths; trace generated behaviour back to one Phrase construct; extend the program without breaking it.
- **Pain points:** names that overpromise; specs that merely say it succeeds; two copies of the same table; target-specific hooks hidden in the core; a declared failure that arrives as a generic 500; large changes that bundle several independently reviewable behaviours.
- **Constraints:** no access to the author's reasoning; reads the diff, the docs and the test message, not the compiler; the same context limit when the reviewer is a model; needs small, independently testable increments.

## Riley: the mob, as the dogfood user

This mob, running Phrase on its own real work after Usable v1: the mob status API (card 038), the benchmark and the board tooling.

- **Goals:** a real service with validation, state, success responses and declared failures; missing vocabulary discovered through genuine use, not speculation; reproducible friction fed back into the catalogue, the grammar and the benchmark; evidence that Phrase reduces LLM work without losing correctness or learnability.
- **Pain points:** toy examples that hide gaps; a demo that cannot express what the mob actually posts; a program that stores something its specs never allowed.
- **Constraints:** first slices are in-memory records with no file or clock access inside Phrase; starts after Usable v1; no special dialect for the mob.

## Steve: sponsor, on a phone

The person who owns the direction and the mob tooling, and who is usually on a phone. Used for stories about the board, the loop and the tooling that serves him, and for direction questions.

- **Goals:** see the board's rank and status, and re-rank it, from a phone; answer a direction question in one word; trust that the mob is working without being asked.
- **Pain points:** a board that is only files; a mob that stops and needs a nudge; a design plan sent for approval instead of a card and its rank.
- **Constraints:** reads cards, ranks and one-word asks, not compiler diffs; escalations are only product direction, an unresolved Decider conflict, or the safety carve-out.

## How to write a story with these

`As <persona name>, I want <an observable capability>, so that <value that persona gets>.`

- **Good:** *As Ari, I want one phrase with one number input to compile to C# and its generated spec to pass, so that I can see my first behaviour work end to end in one attempt.*
- **Good:** *As Morgan, I want a declared failure to map to its HTTP status instead of a 500, so that I can tell from the test message which behaviour broke.*
- **Bad (a role, not a persona):** *As an engineer (or an LLM) writing Phrase, I want ...*
- **Bad (an internal design goal as the benefit):** *... so that C# is the first adapter, not a one-off emitter.* Say what a persona gets from that (for example Dana: *so that switching target language does not change my program's behaviour*).

The Ready check rejects a story that names no persona from this file or whose "so that" is an internal design goal. `scripts/check-story-persona.py` checks the first part mechanically.
