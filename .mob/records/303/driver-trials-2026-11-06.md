# Driver trials during card 303's slice (2026-11-06)

Probes first (committed in .mob/records/probes/2026-10-30.jsonl, 07:51-07:58 UTC): copilot, codex, grok (signed in again, grok-4.6) and muse (1.4.14) pass the one-word probe; agy fails with quota exhausted ("Resets in 61h27m").

Same slice brief, three seats in turn (the first two read-only by brief, nothing written; tracked tree unchanged after each):
- **Muse (trial driver):** run 1, `muse exec --prompt-file`, ended with only the two preamble lines and no reply text. Run 2 with `--json`, cut by a 110 s timeout, was still reading files (253 KB of events, 5 tool results, no final message). Verdict so far: starts and signs in, but delivered no reply to a read-and-write-up brief; one more trial wanted with the card text inlined in the prompt (so it need not read files first) at a seat where an empty reply costs nothing. Not added to the fallback order yet.
- **Grok (grok-4.6, plan mode via --prompt-file):** narration only ("I'll read ...") and stopped with exit 0: the defect of card 291. Probe passes, so Grok stays enabled in core/backends.json (it already is, and is third in the fallback order), but it stays out of counted decision seats and out of build/slice seats that need a reply until 291; a seat with a probe pass and no reply is recorded as a seat failure, not a driver ban.
- **Codex (read-only, declared architect):** delivered the slice (slice-architect.md), 19,658 tokens.
