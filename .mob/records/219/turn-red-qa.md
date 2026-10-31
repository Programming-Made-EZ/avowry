# Card 219 turn record: QA engineer (red), ctx-3, Claude seat

Intent line commit: bca5510 (Card 219 intent line, D165; D159, D162). Criterion 2 reused it; no new intent commit.

## Red commits

1. 451f4ea, Criterion 1: record committed out of number order is accepted (D159 C1). Red by assertion on the old rule: `Lists differ: [] != ["Card 214 evidence <sha> predates Card 209's record"]` (74 tests, 1 failure).
   - Guard test_guard_records_closed_in_number_order_are_still_accepted, born-green, mutation MG1 (previous = min card number greater than this card): 1 failure of 3 run.
   - Guard test_guard_fixture_commit_dates_are_the_fixed_ones_in_close_order, born-green, mutation MG0 (env key renamed so commit dates are not fixed): 2 failures of 3 run.
2. 55b5794, Criterion 2 and Done check 3 (held-record sequence, born-green, mutation-proved).
   - Red: test_backdated_evidence_is_refused_naming_the_record_committed_immediately_before fails by assertion on b374ef7 (`["Card 209 evidence <sha> predates Card 214's record"] != []`; 77 tests, 1 failure).
   - Fixture-order deviation from the slice: records added in the order 216, 214, 209, not 205, 214, 209, because with 205, 214, 209 the staged rule (largest earlier number, 214) and the final rule (nearest earlier, 214) agree and the test could not be red. The error still names Card 214.
   - Born-green test_held_record_sequence_passes (order 205, 214, 209, 208, with add, hold back, stand-in 219 record, re-add in separate commits; one variant). Mutations on a scratch copy never committed: M1 fails it (7 failures in the run, "predates Card 208's record"); M5 fails it (1 failure, "predates Card 219's record"); M4 does not fail it (no same-commit pair in the single variant) and is killed by the previous_record table rows instead (1 failure, 1 error).
   - Existing tests the green will change: none (77 pass with the final rule in a scratch copy).

Criterion order: 1 then 2; Done check 3 rides in the Criterion 2 red.

This turn record is written at close from those commits.
