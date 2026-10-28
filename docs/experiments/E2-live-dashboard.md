# Experiment E2: live mob dashboard (raw input from product owner, 2026-10-25)

A process/tooling experiment. The retro decides via Decider whether to keep it; only then does 'every mob publishes to
the dashboard' enter the default TWA template and the README.

## Problem (product owner)

"we don't have a lot of insight into what's happening after we hand off a task to another model. it just runs in a prompt
and we wait to hear back." Cost seen: a Grok review sat empty for about 40 minutes and nobody noticed.

## Practice

A per-seat JSONL event stream written by `facilitator.py` as each driver runs, with stall detection; a `status.json`
snapshot per project; one dashboard for all mobs, readable on a phone, refreshed about every 30 seconds. Built in
mob-defaults so every mob started from the defaults publishes automatically. See card 036 (Live mob dashboard) for the
acceptance criteria and design.

## Hypothesis

We believe a live dashboard fed by a per-seat event stream with stall detection will cut the time to notice a stalled or silent handoff, and cut the product owner's 'progress?' check-ins, for the mob. We'll know when the median time from a seat going silent to a stall flag moves from about 40 minutes (the empty Grok review) to at most 5 minutes over the next 5 cards, and the product owner's progress check-ins per card fall from the current count to at most half of it. Counter-metrics (Goodhart guard): tooling overhead (tokens and seconds per cycle, at most +3%), data exposure incidents (zero; the allow-list test never fails), and false stall stops (zero stops of a healthy seat).

## Baseline

- Stall detection time: about 40 minutes (the empty Grok review of 8ba3699, 2026-10-25).
- the product owner's progress check-ins per card: to be counted from the conversation for the card before and the first card
  after the dashboard (the facilitator logs each request that asks for status).
- Overhead: tokens and seconds per cycle from the E1 metrics (`.mob/<card>/metrics.json`).

## Trial scope

The next 5 cards after the dashboard first publishes. Review at the retro after the fifth: keep, revise or drop via
Decider.

## Log

- 2026-10-25: raw input received; refined into card 036; ranked by Decider D7 (Codex Yes; Copilot Support-it; Grok Yes; agy Yes); build starts after the
  baseline-fix card finishes its mobbing trial. No publish to any public URL until the product owner's one-word OK.
