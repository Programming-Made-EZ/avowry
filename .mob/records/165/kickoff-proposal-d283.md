# D283: adopt card 165 with its task force

Proposer: the facilitator, 2026-11-17 about 05:15Z, after card 172 closed. The loop picker's next card is 165 (failure observer: the generated API raises a neutral failure event; tracing plugs in outside the runtime); it depends on cards 161 and 170 (done).

Probe at 05:11:56Z: agy, claude, cursor and grok ok; qwen gate busy (play-tracker's heavy run); codex, copilot and muse at their limits. kickoff_check.py passed on .mob/records/165/declaration.json, committed before this vote.

Task force:
- red: claude sonnet (claude-sonnet-5-5).
- green: cursor, pinned composer-2.5.
- QA and security review: claude opus (claude-opus-5-5). The card requires a security-role reviewer in the Decider record; this seat signs the security review: the event carries no request data, exception message, source file name, line or spec text.
- cross-family review: grok grok-4.6, normal mode in a throwaway clone, before the QA review is written.
- sign-off: agy gemini-3.8-flash-medium, read-only.
No service holds more than two seats. The facilitator writes the slice (interface and event shape, where the host registers the no-op, the isolation of a throwing or slow observer, reuse of the card 161 step ids) before the red. dotnet runs are the facilitator's, under Avowry's heavy lock; every proof after the green runs the full Status!=Pending suite (the card 172 retro).

Criterion 3 changes the generated runtime, so the recorded orders output hash (the Slice 142 pin, today 19fe6d71) is re-recorded honestly in this card with the runtime change named; the 422 bodies and the 11 demo specs stay byte-identical.

## Vote

Yes adopts the card and the task force. A No says what would change it.

After the vote and any sentence, end with one line: `CITE: ` followed by a phrase of six or more words copied exactly from this proposal.
