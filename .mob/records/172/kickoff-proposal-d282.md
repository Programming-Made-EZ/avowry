# D282: adopt card 172 with its task force, and keep today's avow-record endpoint rule

Proposer: the facilitator, 2026-11-17 about 03:55Z, after card 171 closed. The loop picker's next card is 172 (trace hardening follow-ups and one shared test process helper); it depends on card 168 (done).

Probe at 03:51:43Z: agy, claude, cursor, grok and qwen ok (qwen gate idle); codex, copilot and muse at their limits. kickoff_check.py passed on .mob/records/172/declaration.json, committed before this vote.

Task force:
- red: claude sonnet (claude-sonnet-5-5). Criterion 1 by true reds (a missing generated file; an absolute or `..` csFile refused with exit 2); criterion 2 by tests of the shared helper (scrub list, node reuse, timeout, process-tree kill) and a scan that the duplicated copies are gone.
- green: cursor, pinned composer-2.5.
- QA review: claude opus (claude-opus-5-5).
- cross-family review: qwen, local model, reply-only with the diff inlined (rotation: no Avowry seat in about 25 hours), run under Avowry's heavy lock so no heavy run shares the machine.
- sign-off: agy gemini-3.8-flash-medium, read-only.
- grok holds no task-force seat on this card (recorded no-fit reason) and votes here.

Criterion 3's orders hash: the card's `409bf3c6` is stale; the current pin is `19fe6d71` (tests/Avowry.Acceptance.Tests/WebExtensionBoundary.cs:18), and that pin must stay unchanged.

The card's open question (from the card 174 review): should an avow record in the source map list every endpoint that reaches the avow, instead of the first endpoint that calls it directly? Proposal: keep today's rule, documented in docs/TRACEABILITY.md, and do not change it in this card; a later card can change it if a reader needs it.

## Vote

Yes adopts the card, the task force and the rule decision. A No names which part and what would change it.

After the vote and any sentence, end with one line: `CITE: ` followed by a phrase of six or more words copied exactly from this proposal.
