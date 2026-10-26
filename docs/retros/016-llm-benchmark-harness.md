# Retro: card 016, LLM benchmark harness v0 (2026-10-25)

## Flow metrics (from git timestamps)

- Card started about 19:28; Done on its written criteria at 20:48 (66f3df0): about 80 minutes of mob time, 10
  increments, about 8 minutes each, CI green on each, zero warnings.
- Minutes lost to rework: about 35. The first run stopped after one cycle on a skeleton (1 task, no runner) because the
  PM's Done-when was too weak; a concrete Done-when was written, and five further cycles went to runner features before
  the suite was authored.
- Review findings after the fact: 5 defects in the stored baseline (invalid invocations, parser scoring chatter, copilot
  rows missing, self-check rows polluting history, no README). Reverts: 0. Follow-up: bug card 032.

## Individual

- Codex (driver): built the runner and suite quickly; its own CLI flag (`--skip-git-repo-check`) is missing from the
  harness's Codex invocation, so half its rows are `unavailable`. Adjustment: run each CLI's invocation once by hand
  before storing results.
- Copilot (QA): signed off a baseline with no Copilot rows. Adjustment: QA checks the rows, not the claims.
- Grok: not a driver here; its 0 of 8 passes is a harness signal (PH104 on chatter), not a language one.
- Facilitator: Done-when check 4 ("at least 3 models") was satisfiable by the fake `self-check-v1` rows. I wrote a check
  that could pass without the thing it was meant to prove. Adjustment: every Done-when set needs a negative check that
  fixtures and fake data cannot satisfy.

## Team (mob)

- The weak Done-when cost a rerun for the second time (cards 001 and 016). Process pick below.
- Rotation: Codex drove all cycles; no per-cycle rotation exists in non-mobbing mode. E1 (true mobbing) rotates
  roles per cycle and starts with card 032.

## Organization

- Nine standing-rule and direction messages in one hour (Decider, TFDD, hypotheses, Goodhart guard, escalation, vision,
  adapters). The agreement and mob-defaults absorbed them, but the mob's board time went to process. The retro asks:
  keep the cadence of rule changes slower than the cadence of cards (area for next pick).

## Product: LLMs-as-customers feedback (round 3: propose base-library keywords)

All four models (Codex, Copilot, Grok, agy) were asked, from their own experience, to propose base-library keywords for
Steve's vision and to say where the docs are unclear.

| Finding | Models | Outcome |
|---|---|---|
| Reusable keywords need generic type parameters (`phrase<T>`, `List<T>`, `Option<T>`); PH010 also forces per-domain clones | all 4 | **Card 033**, rank 19 |
| Retry, transaction, idempotent-execute need an action as input and specs with test doubles ("given action fails twice then returns 7") | all 4 | **Card 034**, rank 20 |
| Failure codes should be parameterizable for reusable guards | agy, Copilot | **Card 035**, rank 24, Decider first |
| A base keyword should be a sentence plus specs in scope everywhere, body in the adapter file (not inline native) | Grok, agy | **Folded into card 022** |
| Each keyword needs a canonical failure contract and one adapter example | Copilot | **Folded into card 022** |
| Candidate keywords: retry (3 models), pagination (2), validation guards (2), idempotent execute (1), normalize email (1) | see table | **Recorded** in `docs/pattern-candidates-raw.md` as input to cards 024 and 025 |

Decider D5 declared 016 Done but not M2; Decider D6 ranked the new cards (Codex Yes; Copilot Yes; Grok Yes; agy Yes); see docs/decisions/DECIDERS.md.

## Experiments review

- Retro-002 pick (background results checked non-empty before relying on them): trial card 1 of 3. It held: all
  Decider and feedback runs were checked for non-empty results (this retro used six parallel runs, none silent).
- E1 (true mobbing): not started; card 032 is the first card under it.
- Mid-card task-force additions on this card: none.

## One process improvement (area: Team)

Hypothesis: We believe adding a negative check to every Done-when set (a check that fixtures and fake data cannot
satisfy) will cut Done-when reruns for the mob. We'll know when the number of cards reopened or rerun because the
Done-when passed too early goes from 2 of 3 recent cards to 0 over the next 3 cards. Counter-metric: facilitator
minutes spent writing Done-when sets. Trial scope: the next 3 cards. The retro decides keep, revise or drop via Decider.
