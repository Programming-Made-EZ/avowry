# Decider D262: card 439, the QA role's model changes mid-card (agy's strong tier is out of quota)

Proposer: the facilitator, 2026-11-15. The owner's model ruling of 2026-11-14 says a mid-card change of model or size is recorded with its reason, like any task-force change; task-force changes here go through a Decider. Vote on the whole.

## The situation

D261 declared the QA role (adversarial review) as agy with gemini-4-argon-high pinned, context ctx-4. Its round 1 review ran (FLAG, fixed test-first). Its round 2 review could not start: agy refused at launch with "Individual quota reached", resetting in about 148 hours (around 2026-11-21). The other services: codex, copilot, grok and muse are out of quota; cursor wrote the green, so it cannot review it; agy's mid-tier models (gemini-3.8-flash-medium, gpt-oss-120b-medium) answer.

## Clauses

1. **The QA role is reseated on claude opus = claude-opus-5-5, in a new context ctx-6**, for review round 2 and any later round of card 439, because adversarial review of a scanner needs the strong tier (the model-selection guide's first row) and claude is the only strong tier with quota. The architect's context ctx-1 is never reused for review; the reviewer gets the same brief, the same D259 clause 4 rule and no access to the architect's conversation.
2. **The change is recorded** in .mob/records/439/model-allocation.md (declared against actual, with this Decider) and reviewed at the retro's Model allocation review, including whether a review by the same model family as the design found less.

## How it lands

The facilitator launches round 2 on the new seat after this Decider. Reversal: if agy's strong tier returns before the card closes, a later round may go back to it.
