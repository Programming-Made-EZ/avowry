# D276: card 367's red moves from muse to cursor composer-2.5, and the sign-off moves from cursor to agy

Proposer: the facilitator, 2026-11-16 about 17:25Z, under the owner's 2026-11-14 ruling that a mid-card model change goes through a Decider.

Evidence: the red seat on muse (ctx-367r, adopted by D274) stopped at 17:23:39Z after about three minutes: its subscription window is used up until 2026-11-23T00:00:00Z (the muse seat log line and the probe at 17:24:09Z both record it). It left two untracked test files and no red record; copies are kept in .mob/records/367/muse-red-partial/ as evidence. The probe at 17:24:09Z: agy, claude and cursor ok; codex, copilot and muse at their limit; grok 402; qwen gate busy (play-tracker's heavy run).

Change:
1. The red becomes cursor, pinned `--model composer-2.5`, a fresh context ctx-367r2, in worktree wt-367. It may read muse's partial files as a draft but owns the result. Reason: the clean writer on mob-defaults #61, #58, #63 and #62.
2. The sign-off moves from cursor to agy gemini-3.8-flash-medium (a fresh context), so the sign-off is not the red's author.
3. Unchanged: the green (claude sonnet), the QA review (claude opus), the cross-family review (agy). Cursor and agy then hold two seats each and claude two; no service holds more than two.

## Vote

Yes adopts the change. A No says what would change it.

After the vote and any sentence, end with one line: `CITE: ` followed by a phrase of six or more words copied exactly from this proposal.
