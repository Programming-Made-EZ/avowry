# Docs: GRAMMAR and README name the adapter contract and the HTTP status mapping

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** docs (thin slice; exempt from the hypothesis rule)
- **Milestone:** Usable v1 (plumbing for bar 2 to 4) (`board/MILESTONE-usable-v1.md`)
- **Source:** Process retro 2026-10-26 (Decider D18): thin slices, trialled on the C# adapter chain; this card replaces part of old cards 003 to 007. Steve: "I do have a concern the slices are too big."
- **Story:** As an engineer (or an LLM) writing Phrase, I want a reader of the docs sees the target extension point named ITargetAdapter and the table of failure-to-HTTP-status mapping, so that the Phrase compiler grows one observable behaviour at a time.
- **One observable behaviour:** A reader of the docs sees the target extension point named ITargetAdapter and the table of failure-to-HTTP-status mapping.
- **Done checks (at most 3, each failing on partial work):**
  1. docs/GRAMMAR.md and README.md name ITargetAdapter and Phrase.Adapters.CSharp, matching the code
  2. GRAMMAR.md has the failure-to-HTTP-status table card 049 implements
  3. No source file changes; the repository gate passes with zero warnings
- **Size rule (the hypothesis under trial):** fits one run of about 3 cycles. If it will not, split it before starting. Runs and cap hits are recorded for the retro.

## Context

- Docs-only card (code, data and docs on separate cards). Rank after 040 and 049 so the docs describe what exists.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`. No tests are required (docs-only).

## Task force

- Backend author [author]: Codex | why: writes the slice test-first and the code
- Reviewer [reviewer]: Copilot | why: independent review and sign-off; not the author
- Language designer: Grok | why: checks the slice against GRAMMAR.md and the adapter contract

## Done when

All must exit 0, run from the repo root:

```
python -c "import sys;g=open('docs/GRAMMAR.md',encoding='utf-8').read();r=open('README.md',encoding='utf-8').read();sys.exit(0 if 'ITargetAdapter' in g and 'ITargetAdapter' in r and 'Phrase.Adapters.CSharp' in r else 1)"
dotnet test Phrase.sln --filter "Status!=Pending"
```

The first command fails unless both docs name the adapter contract, so a docs card cannot pass on the gate alone.
