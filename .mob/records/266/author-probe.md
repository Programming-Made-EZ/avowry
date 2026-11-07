# Card 266 build-seat probes (facilitator, 2026-11-07)

Rotation: Codex built cards 299 and 303, Copilot 196, Grok 197, so the next build seat is agy; the declared fallback order is Codex, then Copilot (the product manager's proposal and Decider D205).

- agy: `agy -p "Reply with exactly the word OK" --model gemini-4-argon-high --dangerously-skip-permissions`: `error: Individual quota reached. Please upgrade your subscription to increase your limits. Resets in 148h30m40s.` (RESOURCE_EXHAUSTED, code 429). Not available.
- Codex: `codex exec "Reply with exactly the word OK" -m gpt-5.6-sol -s read-only --skip-git-repo-check`, 120 second timeout, from the user's home directory: output `OK`, exit 0 (16,083 tokens). Available. Its write invocation for the green is the machine-local launcher with `--json`, in the card's own git worktree, with the delete rule and the keep-line-endings rule in the brief (the card 313 lesson).

Codex is the build seat for both greens of card 266. Rung 1 oversight: the facilitator re-runs every claim and recomputes the bytes. Codex stays out of Decider and retro voting seats (it echoes the placeholder cite).
