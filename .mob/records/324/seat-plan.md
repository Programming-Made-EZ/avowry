# Card 324 seat plan under the product owner's seat-spread ruling (facilitator, 2026-11-07)

Probes (fresh, 21:55Z, one-line prompt "Reply with exactly the word OK"):
- Grok (`grok -p ... -m grok-4.6 --always-approve --permission-mode plan`): OK. Available.
- Codex (`codex exec ... -m gpt-5.6-sol -s read-only --skip-git-repo-check`): OK. Available.
- Copilot (`copilot.cmd -p ... --model auto -s --deny-tool=write`): "You have no quota". **Excluded: out of quota.** Re-probe at every seat assignment.
- agy (`agy -p ... --model gemini-4-argon-high --dangerously-skip-permissions`): RESOURCE_EXHAUSTED, "Individual quota reached ... Resets in 145h51m31s". **Excluded: out of quota.** The probe says about 146 hours (about 2026-11-13); the owner's note said it resets on Nov 8: the discrepancy is reported to the owner; re-probe at every seat assignment.
- Claude: available (fresh subagent contexts); used only as the fallback or for independence from an earlier Claude seat.

Least-recently-used among the drivers that pass: the most recent build seat was Codex (card 266 greens), the one before Grok (card 197), so Grok is least recently used. Plan, alternating, a red and its green never written by the same driver:
- cycle 2 red (characterisation, the QA role): Grok; no green.
- cycle 1 red (QA role): Codex; cycle 1 green: Grok.
- cycle 3 red (QA role): Grok; cycle 3 green: Codex.
- Pre-ship gate reviewer, the ergonomics reviewer and the sign-off: Claude fresh contexts, by the seat rule's independence (the authors of the card's commits are Grok and Codex; Copilot and agy are out of quota); each recorded as such.
- The retro voices and the Deciders: the next window probes every driver again and seats least-recently-used across those that pass, Claude as fallback; the exclusions then (Grok's card 291 narration defect for Decider seats; Codex's placeholder-cite echo, now handled by the D196 echo rule) are re-probed, not assumed.

Writing seats (Grok and Codex, Rung 1) work in their own git worktree and the facilitator rebases and fast-forwards their commits into main after re-running every claim; a seat that fails its quality check (a test that cannot fail, a damaged byte, a claim the facilitator's rerun contradicts) is replaced by Claude with the reason recorded.
