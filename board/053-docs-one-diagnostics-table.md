# Docs: one diagnostics table, not two

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** docs (thin slice from card 018; exempt from the hypothesis rule)
- **Source:** Grok independent review, 2026-10-25; split from 018 under the thin-slice rule (Decider D18).
- **Story:** As a Phrase author (an engineer or an LLM), I want exactly one place that holds the full diagnostics table, so that I never read two copies that disagree.
- **One observable behaviour:** only one of HANDOFF.md and docs/GRAMMAR.md holds the full diagnostics table, and the other points to it; GRAMMAR.md no longer wrongly says the table moved.
- **Done checks (at most 3):**
  1. Exactly one of the two files contains the table rows (mechanical count)
  2. The other file links to it, and GRAMMAR.md's "moved" claim is true or removed
  3. No source file changes; the repository gate passes with zero warnings

## Task force

- Docs author [author]: Copilot | why: edits the two docs
- Reviewer [reviewer]: Grok | why: checks the PH numbers and wording against the resolver

## Done when

```
python -c "import sys;h=open('HANDOFF.md',encoding='utf-8').read();g=open('docs/GRAMMAR.md',encoding='utf-8').read();n=sum(1 for t in (h,g) if t.count('| `PH33')>=3);sys.exit(0 if n==1 else 1)"
dotnet test Phrase.sln --filter "Status!=Pending"
```
