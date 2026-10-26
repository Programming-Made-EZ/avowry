# Experiment E1: true mobbing (approved by Steve, 2026-10-25)

A process experiment, not a card. The retro decides via Decider whether to keep it. The TWA, the default template and
the mob-defaults README change only if it is kept.

## Practice

- **Driver:** writes the code, executing only what the navigator directs (strong-style).
- **Navigator:** a different model, ideally higher on the trust ladder. Steers each step in real time: keeps intent,
  acceptance criteria and the next failing test in view; directs red, green, refactor.
- **Observers:** the rest of the task force. They get the DELTA at each checkpoint (after red, after green, before
  commit) and reply `SIGN-OFF` or `FLAG: <one line of evidence>`. No per-keystroke watching.
- **Rotation:** each cycle driver, then navigator, then observer, following the trust ladder.
- **Ownership (Steve: "we still want the whole mob to own, so we still need a whole team sign-off"):** the whole task
  force owns the increment. Before each commit to main EVERY member (driver, navigator, every observer) gives a
  sign-off. Any FLAG is resolved first; a disputed FLAG goes to the Decider. This replaces the separate
  single-reviewer step for mobbed increments: "author never reviews own work" becomes "no increment ships without
  every member signing off."

## Hypothesis

We believe true mobbing will reduce review findings, reverts and rework per increment, and cut cycles-to-green, for
the Phrase mob. We'll know when, on the card it is run on, findings per increment, reverts per increment and
cycles-to-green move from the card-001 baseline to lower values, with these counter-metrics reported alongside
(Goodhart guard): tokens per shipped increment and wall-clock time per increment.

## Baseline (card 001, from git and MOB.md)

- 5 increments shipped in about 60 minutes of mob time (about 12 minutes each), CI green on each.
- Review findings after the fact: Grok's review of cycles 2-3 found 6 defects; the review of cards 002's specs
  found 4 more. Reverts: 0. Rework: the card was reopened once and split once (about 20 minutes).
- Cycles to green per class: 1 each, but 2 of 5 increments needed a follow-up fix card.
- Tokens per increment: not measured in card 001 (tooling added with this experiment: estimated from prompt and
  reply size, flagged `estimated`).

## Measurement plan

The facilitator records, per mobbed card, in MOB.md and `.mob/<card>/metrics.json`: turns, estimated tokens, wall-clock
seconds, FLAGs raised, fix rounds, cycles to green. Review findings and reverts after the fact are counted from
later reviews and `git log`. The retro compares these with the baseline and runs a Decider on keep, revise or drop.
A metric win with a counter-metric blowout is a red flag, not a win.

## Log

- 2026-10-25: approved by Steve; recorded here; experimental facilitator support (`--true-mobbing`) being added to
  mob-defaults, test first. Runs on the next card after card 016.
- 2026-10-25, run 1 on the baseline-fix card (032), cycle 1: 20 turns, about 330k estimated tokens, 998 s, 5 flags, 1 fix round. The before-commit whole-team checkpoint caught a real defect (the self-check rows still landed in `history.jsonl`) from three members at once. The unresolved flag at the re-check was a tooling false positive (Grok narrates before it answers and the verdict parser read the first word); fixed in mob-defaults (verdict = last SIGN-OFF/FLAG token, test first). The run then stopped (exit 4) with nothing shipped, so the run-1 figures are not yet a clean per-increment cost: the next run continues the same card.
- 2026-10-26, runs 2-8 on the baseline-fix card (032). Totals over the 8 runs: 127 turns, about 2.7M estimated tokens, about 2.4 hours wall-clock, 2 increments shipped (reply-source extraction, bc299b5; isolate self-check results, a4ab305), 0 reverts. Per increment: about 70 minutes against card 001's baseline of about 12, and roughly 1.3M estimated tokens (tokens per increment on card 001 were not measured).
  - **What worked (quality):** the whole-team gate caught real defects that a single reviewer might have missed: self-check rows leaking into the baseline history (flagged by three or four seats, twice) and a Copilot invocation that passed a temp file path instead of the prompt. Observers also caught driver scope creep in the red step.
  - **What did not (cost and flow):** five of the stops were bugs in the mobbing tooling, all fixed with tests: verdict parsing (Grok narrates before answering), stage-aware checkpoint prompts (red is red; later increments' Done-when checks stay red), a MOB.md digest (run 3 cost about 1.45M estimated tokens before it; later runs about 70-280k), and a clean-tree start (leftovers are stashed). Even after those, runs 7 and 8 still stopped because a seat re-flagged expected-red checks and a driver edited production code in the red step; and the remaining work on this card (an invocation fix, a README, a real four-model baseline run) is mostly not red-green-refactor work.
  - **Hypothesis status:** cycles-to-green moved the wrong way (about 4-5 cycles per increment against 1); review findings and rework per increment are not yet comparable (the two shipped increments have no after-the-fact review findings so far); counter-metrics (tokens and time per increment) are many times the baseline. Setup cost is a large share of both.
  - **Decision (Decider D8):** E1 is paused on this card, not dropped. 032 finishes in the standard driver-plus-review mode with a whole-team sign-off before the final ship. The E1 retro decides keep, revise or drop after 032 is Done, with these numbers and a split of setup cost from steady-state cost.
