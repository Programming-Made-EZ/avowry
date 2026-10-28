# Milestones (set by the mob)

Each milestone has a measurable target and an automated check. Progress is reviewed at every retro; the next
milestone is set when one is reached. product owner is told when a milestone is reached, and about Usable v1 itself.

| # | Milestone | Measurable target | Automated check | Status |
|---|---|---|---|---|
| M1 | **Usable v1** | The six bars in `MILESTONE-usable-v1.md` all pass | `tests/Phrase.Acceptance.Tests/UsableV1.cs`, CI job `usable-v1` | Not reached (cards 002-017) |
| M2 | **Measured** | LLM benchmark baseline stored for all four models on suite v1; noise floor reported | `bench/results/history.jsonl` has a baseline row per model; harness self-check | **Pending**, aligned with the bootstrap-phase exit (Decider D11): reached when a Decider declares the baseline established. A zero-shot baseline (32 rows) is stored (cfb1952) but it has no docs in the prompt and the runs are not cold (card 037) |
| M3 | **LLM-fluent** (provisional; targets fixed at the M2 baseline) | Cold pass rate on suite v1 of at least 80% for every available model, mean attempts to green at most 2 | Benchmark run in CI or by the mob after each feature card | Not started |
| M4 | **Vocabulary v1** (from VISION.md) | On the catalogue benchmark, the seed vocabulary removes LLM work (tokens written, native lines, attempts to green) at equal pass rate versus no catalogue; counter-metrics: near-duplicate rate and learnability (a cold model finds and uses the right keyword). Targets are fixed from the measured baseline, never from a keyword count | Benchmark run (cards 027, 026) | Not started (cards 024-029) |
| M5 | **Synthesis** (FUTURE, VISION ruling 1) | Not set yet: set from the first measured pass of card 030; the counter-metric is the share of code left as native blocks | Round trip: synthesized Phrase compiles back and its specs pass | Not started; after M1 and M4 |
| M6 | **Human view** (FUTURE, VISION ruling 4) | Not set yet: lossless round trip on all fixtures; reviewer reading-time metric with LLM token count unchanged | Round-trip test (card 031) | Not started; reading confirmed by product owner (two-way) |

M2 comes before M3 because M3's targets are set from M2's baseline. Add milestones here; rejected or retired
ones stay listed with the reason.
