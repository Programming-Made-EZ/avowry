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
