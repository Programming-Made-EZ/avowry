# Task-force declaration proposal: card 290

Context: D185 re-rank 21 ranked card 290 first. D183 says user stories and bugs stay on the board; card 290 is a bug in decider result counting. Copilot may sit counted decision seats again after card 229 sign-off. Grok stays non-counted until card 291. In card 229 windows, 8 of 10 Grok attempts ended without a vote.

## Task force
- Ready: Copilot CLI, ctx-1. Why: product owner and queue owner; keeps the card moving and records the proposal.
- Architect: Claude, ctx-2. Why: checks invariants, scope premise, and the counted-vote contract.
- QA: Claude, ctx-3. Why: writes the red tests and ZOMBIES for uncited-Yes and counted-vote behavior.
- Backend developer: Claude, ctx-4. Why: owns decider.py and tests/fakes/fake_voter.py, the result fixer and helper path.
- Audit lens: Copilot, ctx-5. Why: re-checks the result path and the vote-reader logic before sign-off.
- Decider: Claude, ctx-6. Why: final yes/support-it/no vote on the bug and any scope window.
- Agile coach: Claude, ctx-7. Why: holds the retro and the process guardrails.

Left out: Grok in counted windows until card 291 lands. Reason: 8 of 10 attempts in card 229 stopped after narration or returned no vote; counted roles should not spend minutes on a known non-vote. Dispatch proposal to weigh: skip Grok in counted roles until 291, via a per-window backend-disabled list or a backends override file for the affected windows. Cost: lower model diversity while the defect is open; acceptable tradeoff to avoid empty-seat churn.

## Domain-expert T2 question
What evidence makes a vote valid enough to change a decision result: a valid proposal cite, the vote line, or both?

## Value
Decision results should reflect only cited, read votes; uncited Yes values must not pass a decision.

Metric: zero uncited votes change the decider result.
Counter-metric: extra window minutes and loss of model diversity from skipping Grok.

## Done checks and caps
Cap total: 1 + 1 + 1 = 3.
1. Uncited Yes is not counted and does not pass the result.
2. Cited Yes passes and No fails, unchanged from today.
3. The 27 doubles use valid cites, or the fake_voter default is replaced by a helper that injects a valid CITE line before the result is evaluated.

## Applicable rules
- D171: cite-a-point votes apply; every vote needs CITE.
- D175: cycle cap per criterion applies; keep total cap to 3.
- D183: this is a bug on the board, not a retro-experiment card.
- D184: run each scope premise in code on HEAD before the window.
- D185: re-rank 21 ranked card 290 first.
- D181: if a lettered red is needed, it must stop at the first passed green.
- D165-D168: no direct process-pick trial on this card.
- D176: no direct gate-report issue here.

## Open points
- Decide whether the 27 doubles get valid cites individually or whether they vote through a helper that adds a valid CITE line.
- Decide whether this card also fixes card 286 (NO OBJECTION stored as No) or leaves it separate; keep 291 separate unless the same parser fix is required.
- Keep the fix narrow: the bug is in result_of and counted-vote semantics, not in the general parser policy for objection windows.
