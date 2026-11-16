# D279: card 162 resumes for green 2, and D278 is rescinded

Proposer: the facilitator, 2026-11-16 about 20:45Z.

D278 (adopted 20:3xZ) moved cards 162 and 201 behind card 165 on the premise that card 162 still needs Codex and Grok runs. That premise was wrong, as the architect voter (grok) said in its No: both runs are committed on main (e2e5a4a Grok passed, f78588c Codex passed, 13 of 13 specs each), the re-freeze record D238 is on main (46179b4), and main's CI at 67cb044 reports "Usable v1: 6 of 6 criteria met". What card 162 still lacks is green 2 (slice-architect.md item 9: remove Pending from Bar6, Z1 and Z2 in tests/Avowry.Acceptance.Tests/UsableV1.cs; delete `suspended.6` in board/usable-v1-ratchet.json; the report reads 6 of 6 with no suspension warning), its gate review, its sign-off, its retro and its trail record. D278's ranks were never applied.

Change:
1. D278 is rescinded; cards 162 and 201 keep Ranks 116 and 117.
2. Card 162 resumes now under its declaration with the probe of 20:27:56Z: green 2 on cursor, pinned `--model composer-2.5` (ruling 4 put the green on Cursor; gpt-5.3-codex is out until 2026-12-14); the independent gate reviewer is a fresh claude opus context; the sign-off is a fresh claude sonnet context; nothing else changes. The gate runs under Avowry's heavy lock.

## Vote

Yes adopts the change. A No says what would change it.

After the vote and any sentence, end with one line: `CITE: ` followed by a phrase of six or more words copied exactly from this proposal.
