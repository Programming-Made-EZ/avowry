# Card 197 build-seat probe (facilitator, 2026-11-07)

Rotation: Codex built cards 299 and 303, Copilot built card 196, so the next build seat is Grok (trust Rung 1, 0/3; DRIVERS.md is stale since 2026-10-30, hence a fresh probe).

Probe: `grok -p "Reply with exactly the word OK" -m grok-4.6 --always-approve --permission-mode plan` (the read-only invocation of the backends file), run from the user's home directory with a 90 second timeout: output `OK`, exit 0. Grok is available now. Its write invocation for the green is `grok -p <brief> -m grok-4.6 --always-approve` (the backends file's `invocation`), run in the card's own git worktree.

Grok stays out of Decider and retro voting seats (a standing exclusion of this mob's decision runs, not a build exclusion). Oversight at Rung 1: the facilitator re-runs every claim and recomputes the bytes.
