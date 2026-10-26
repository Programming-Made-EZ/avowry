# Retro: card 032, fix the benchmark baseline (2026-10-26)

## Flow metrics (from git timestamps)

- Card filed 2026-10-25 about 20:50; Done 2026-10-26 about 06:30 (cfb1952 plus the sign-off): about 9.5 hours elapsed.
- Mob time: 8 true-mobbing runs (experiment E1), about 2.4 hours, 127 turns, about 2.7M estimated tokens, 2 increments; then 4
  standard-mode runs of about an hour each, about 12 increments in all across the card; plus about an hour of facilitator data work
  (the real four-model baseline needs longer than a driver turn may run).
- Minutes lost to failed or retried cycles: about 4 hours: E1 setup, then three reopenings because a Done-when could be satisfied
  without proving the point. Review findings after the fact: 5 (agy empty rows, Copilot missing row, a mix passing the empty-row check,
  no `invalid/` folder, no `reply_head`). Reverts: 0.

## Individual

- Codex: delivered the invocation and capture fixes; the same fixes were reopened twice because the checks around them were weak, not the code.
- Copilot: its flags at checkpoints were mostly right; the baseline showed Copilot passing 0 of 8, which is a harness-or-model question for the retro to watch, not a verdict.
- Grok: caught the agy empty-reply data in the sign-off by reading the rows, not the claims; as a driver it over-scoped a red step in E1.
- agy: its CLI works headless as `agy -p <prompt> --model <id> --dangerously-skip-permissions`; its 7 of 8 zero-shot passes are the cold-run question.
- Facilitator: I wrote a Done-when that three times could be satisfied by a mix or by a count. I also ran `rm -rf` on a variable path (nothing deleted); fixed by a mechanical guard (mob-defaults `delete_guard.py`, 80 tests; phrase-lang pre-commit). I also launched a Decider voter with a malformed prompt and write access (incident under Organization); adjustment: voters run only in a sandbox.

## Team (mob)

- The whole-team sign-off before the last ship caught two real defects the Done-when checks missed (twice); that is the best evidence from E1's idea, available at one-tenth the cost.
- Decider D12: E1 stays open and opt-in under the bootstrap-phase rule.

## Organization

- Steve's bootstrap-phase rule (no metric-based rejections until a baseline is established) and the escalation, Decider, TFDD, Goodhart and safety rules landed in the same two days; the agreement now needs a short index.
- A `MOB.md` summary page is published on GitHub (per turn, throttled) so Steve can follow the mob; the live dashboard (card 036) waits on Steve's hosting OK.
- **Incident (safety): a Decider voter ran with full write access and edited the repo.** A malformed launch gave the Copilot voter the bare
  prompt 'd11' with all tools allowed in the real working tree; for about 15 minutes it edited source files (an unreviewed start of
  the C# adapter, +580 lines, and a test-support tweak). No commit, no push. Found from file timestamps, traced to my own launcher,
  stopped by exact PIDs, edits kept in a labeled stash. Root causes: 'read-only' for voters was only a prompt promise; voters ran
  in the real tree with permission-bypass flags; no check that each seat received the intended prompt. Fix: the sandboxed Decider
  runner (scratch clone, restricted modes, prompt-integrity check, central vote parsing, tree-unchanged verification). See `docs/retros/seeds.md`.
- A driver turn may not run longer than its timeout, so a four-model baseline cannot be a driver turn; the facilitator ran it in scratch copies.

## Product: LLMs-as-customers evidence this retro

The feedback source is the benchmark itself: 32 rows (Codex, Copilot, Grok, agy on 8 development tasks, zero-shot).

| Finding | Evidence | Outcome |
|---|---|---|
| With the fixed harness, agy passes 7 of 8, codex 3 of 8, grok 1 of 8, copilot 0 of 8; the earlier 'two of 29' was capture faults | `bench/results/history.jsonl`, `bench/results/invalid/` | Recorded as the zero-shot baseline (not cold) |
| The prompts carry no docs, so documentation cards (013, 014, 019) cannot move the metric | `bench/tasks/*/prompt.md` | **Card 037**, 3rd in Ready |
| agy's 7 of 8 on an unseen language suggests a warm session (memory of earlier Phrase sessions) | the same rows | **Folded into card 037** (cold runs) |

Decider D11 (after a failed first round: Copilot ABSOLUTELY NO on declaring M2 early) declared 032 Done as a zero-shot repair, left M2 pending
until the baseline is declared established, and ranked 037 (Codex Yes; Copilot Support-it; Grok Yes; agy Yes). Under the bootstrap-phase rule none of this
starts metric-based keep or revert.

## Experiments review

- E1 (true mobbing): open and opt-in (Decider D12: Codex Yes; Copilot Support-it; Grok Yes; agy Yes); not closed, not dropped.
- Retro-016 pick (a negative check in every Done-when): trial card 3 of 3 done. It helped for specific faults but a mix still passed it; the new pick extends it.
- Retro-032a pick (state what the metric measures before the Done-when): trial card 1 of 3; it would have caught the docs-in-context gap.
- E2 (live dashboard): not started; card 036 is next; hosting waits for Steve's one-word OK.
- Mid-card task-force additions on this card: none.

## One process improvement (area: Team)

Hypothesis: We believe asking one seat to try to break each Done-when check (write a way to satisfy it without doing the work) before a
card's run starts will cut reopenings caused by weak checks. We'll know when reopenings per card go from 3 (this card) to at most 1
over the next 3 cards. Counter-metric: seat tokens and minutes per card. Trial scope: the next 3 cards. Under the bootstrap-phase
rule the retro keeps or revises it on judgment, not on the metric, until the baseline is established.

Incident action (a safety fix, not a process pick): build the sandboxed Decider runner before the next Decider; no vote is run in the real working tree from now on.
