# Milestones (set by the mob)

Each milestone has a measurable target and an automated check. Progress is reviewed at every retro; the next
milestone is set when one is reached. Steve is told when a milestone is reached, and about Usable v1 itself.

| # | Milestone | Measurable target | Automated check | Status |
|---|---|---|---|---|
| M1 | **Usable v1** | The six bars in `MILESTONE-usable-v1.md` all pass | `tests/Phrase.Acceptance.Tests/UsableV1.cs`, CI job `usable-v1` | Not reached (cards 002-017) |
| M2 | **Measured** | LLM benchmark baseline stored for all four models on suite v1; noise floor reported | `bench/results/history.jsonl` has a baseline row per model; harness self-check | Not started (card 016) |
| M3 | **LLM-fluent** (provisional; targets fixed at the M2 baseline) | Cold pass rate on suite v1 of at least 80% for every available model, mean attempts to green at most 2 | Benchmark run in CI or by the mob after each feature card | Not started |

M2 comes before M3 because M3's targets are set from M2's baseline. Add milestones here; rejected or retired
ones stay listed with the reason.
