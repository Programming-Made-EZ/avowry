# Card 298 cycle 2: tests pass on green 1; mutation evidence (QA seat ctx-3b, Claude, report committed by the facilitator)

Cycle 2 has no red. Green 1's release already unlinks only its own holder file and then tries rmdir, so the either-order behaviour was delivered in cycle 1. The tests are therefore characterization tests, accepted on mutation evidence instead of a first failure.

- On HEAD fd4c702, `python -m unittest tests.test_decider_runner.LiveVoteOverlap.<name>` (one run each, QA seat): `_a_first` OK, `_b_first` OK. Both print the known seat-use append-only message (bug card 307), which the cycle 1 test also tolerates.
- Mutation 1 (release deletes every *.json in the holders directory): `_a_first` and `_b_first` FAIL with "'entered' != 'refused': the facilitator's commit after run X ended was not refused" (HEAD moved).
- Mutation 2 (release unlinks its own file, skips rmdir): both FAIL with "the holders directory was left behind".
- Mutations ran only in a scratch copy (git archive of HEAD), not in the real repository. Commit: mob-crosscli "Card 298 cycle 2 tests".
- Choice by the seat, stated for review: the helper makes one commit attempt after the first run ends (no push), as the slice text asks for one attempt.
