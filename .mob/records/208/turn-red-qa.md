# Card 208, red seat turn record (QA, ctx-3)

This turn record is written at close from the commits below, not during the turns.

## Commits (mob-defaults)

| Commit | Criterion | Subject |
|---|---|---|
| 83714f1 | Criterion: 1 | red: a seat starts only on a probe under 60 minutes old |
| 6af1cca | Criterion: 2 | red: a Claude seat starts only while no other model has quota, and the record says why |
| 40cc57c | Criterion: 3 | red: the probe model is pinned, each enabled backend names probe_model and the probe launches it |
| d3125ea | Criterion: 2 (D164 correction cycle) | red: seats rotate least-recently-used from a tracked seat-use history |
| 9cda13d | Criterion: 3 (review fix F1, ctx-5 finding) | probe_backend with a missing, non-string or unknown pin fails the probe and launches nothing |

## Criterion order

1, 2, 3, then the D164 correction cycle of Criterion 2 (d3125ea, replacing the quota-first rule of 6af1cca with least-recently-used rotation), then the Criterion 3 review fix F1 (9cda13d).

## Guards and their mutations (scratch copies, never committed)

- 83714f1: G2 (limit 60 changed to 59, red); G1 (staleness always true, red; the existing DriverProbes count test also fails). G3 was not shown red: no mutation discriminates it on that base; the red E test is the real detector (reported to the reviewer).
- 6af1cca: G5 (claude_only_problem forced to empty, two DriverProbes tests red); G6 (every Claude seat raises SeatRefused, guard errors).
- 40cc57c: G8 at L1 (disabled backend no longer skipped in probe_model_problem, red); G8 at L4 (pin read before the enabled check, KeyError); G9 (grok pin deleted from core/backends.json, red).
- d3125ea: four born-green guards, each red under its mutation: unread probe result skipped (backend missing from results; result without ok), enabled flag ignored, failed probe counted as available.
- 9cda13d: the unusable-pin branch of probe_backend disabled so the pin falls back to the first model: 6 failures in 16 tests (missing pin, non-string pins, unknown pins).

## Deviation

D136: red d3125ea is 463 lines in two test files (tests/test_card_mode.py +124, tests/test_seat_rotation.py +339), larger than one small commit. One criterion, one layer, tests only; the acceptance tests need the unit-level doubles beside them. The independent reviewer (review-ctx5.md) recorded and accepted it; no rewrite.
