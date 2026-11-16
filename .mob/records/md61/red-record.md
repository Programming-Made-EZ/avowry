# mob-defaults #61: red record

Seat: red (dev, red), claude-sonnet-5-5, declared by D269, context ctx-2. Worktree wt-md61, branch card-61 at 4c1addd. Nothing committed.
File written: `tests/test_seat_use_model_outcome.py` (new; no other file changed).

## Counts

- Pending (fail on today's code): 44 tests in `Test61Pending` and `Test61PendingDecider`.
- Characterisation (pass today): 9 tests in `Test61Characterisation`.
- Total 53. Failures are AttributeError (seat_outcome_line, append_seat_outcome), TypeError (new seat_use_line arguments), KeyError (new fields) or AssertionError. None is a setup error. Loops use a no-op `case()` context, not `subTest`, so one pytest result is one test.

## Runs

1. `python -m pytest tests/test_seat_use_model_outcome.py -q --capture=sys`: `44 failed, 9 passed in 30.72s`.
2. Existing suite. The full `python -m pytest tests -q -x --ignore=...` did not finish: 2,282 tests, about 5% after 25 minutes on this laptop (decider and subprocess tests), so I stopped it. No leftover process. The existing files that touch the code under change ran with `--capture=sys`: `tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py tests/test_seat_gate.py tests/test_record_cycles_evidence.py`: `123 passed, 43 subtests passed in 63.88s`. Also `tests/test_decider_launched.py`: `28 passed, 5 subtests passed in 69.81s`. The first 5% of the full run had no failures (u = subtests passed). Only a new file was added, so no existing test can change.

## Tests by scenario

Characterisation (pass today):
- [Z] old history read unchanged and not backfilled; `seat_use_line` with none of the new arguments is exactly the six fields; old rows count as seatings; rotation refusal and `rotation_reason` on an old history unchanged.
- [I] `read_seat_use`, `rotation_reason`, `card_seat_log` and `mob_process.py record` (the checker) accept a mix of old, launch and outcome rows.
- [S] the existing writer keeps the history append-only.

Pending:
- [Z] an old row counts beside new rows; outcome-only history is -1; empty history; unknown model reads `undeclared`.
- [O] `seat_use_line` new shape (all five fields, `outcome: launched`; any one argument switches it on); `commit_seat` writes one launch line with service, declared model, reason, work type; `seat_outcome_line` exact fields and None defaults; a launch is no turn until a success outcome; decider launch lines carry `service`, `decider voter`, `vote`, `launched`.
- [M] all six outcomes accepted, others raise ValueError; `append_seat_outcome` line shape, per-context targeting, invalid outcome writes nothing, append-only and read-back refusal, unwritable file refuses; a decider run of four voters writes success, no_vote, quota_exhausted and timeout outcomes with `duration_s`, after all launches, none for unlaunched voters; wrapper model recorded.
- [B] launch plus later success is a seating (index of the launch); any other outcome, none, success before the launch, or a mismatch of card, context or seat is not; last served launch wins; outcome rows are never seatings; `claude_wait_problem`, `rotation_reason` and `check_seat` treat a failed or unfinished codex launch as not served; after a decider run only the success voter counts.
- [I] rotation on mixed rows; `read_seat_use` returns both shapes; `record` check on rows the new writer wrote.
- [E] unpinned seat gets `undeclared` and its line is still written (gate and decider); backend with no `probe_model` or models.
- [S] old rows stay byte-identical, only new rows carry new fields; exact launch key set; decider run leaves an old history untouched.

## Notes for the green

- The declared model is read as the backend config's `probe_model` pin (the key `decider.vote_seat` launches with); the tests give the pin and its models value the same text (`pin-model`), so key or value both pass. A backend with no `probe_model` is `undeclared`.
- `commit_seat(role, checked)` is called with today's two arguments; its new fields must come from the run's backend config.
- `fallback` and `refused` are tested only at the `seat_outcome_line` and `append_seat_outcome` level; the slice names no decider trigger for `refused`, and a fallback cannot be driven through `decider.main` today.
- The mock launches use `facilitator.LaunchResult` (a plain 4-tuple fails the 3-way unpack in `vote_seat`).
- Running pytest here needs `--capture=sys`: without it every test that spawns a subprocess (including the existing decider tests) fails with `OSError: [WinError 6] The handle is invalid` before reaching its assertions.
- The existing checker is `mob_process.py record <repo> <card>`; there is no `check` subcommand.

## Addendum: every seat

The card's second [M] scenario (the owner's addendum of 2026-11-15): a seat launched outside `CardRun` logs its rows through `seat_log.py` at the repository root. Added class `Test61PendingSeatLog` (18 tests, same file) that runs `seat_log.py` with `sys.executable` in a subprocess against a temporary repo. The earlier 53 tests are unchanged.

- Rerun of the whole file, `python -m pytest tests/test_seat_use_model_outcome.py -q --capture=sys`: `62 failed, 9 passed`.
- Pending: 62 (44 earlier + 18 new). Characterisation: 9 (unchanged). `-k SeatLog`: `18 failed, 53 deselected`.
- Tests: launch line equals `seat_use_line(...)` with the keywords, `outcome` launched, ts from the clock, `probe_taken_at` null or from `--probe-taken-at`; outcome line equals `seat_outcome_line(...)` with and without `--actual-model` and `--cli-model`; all six outcomes accepted; an outcome outside the six exits 2 and writes nothing; any missing required argument, no command and an unknown command exit 2 and write nothing; old rows stay byte for byte, an unterminated last line is kept, an unreadable history refuses and is left as it is; in a git checkout the seat-use lock is released afterwards and a live lock held by another run refuses the write; rotation counts a launch only with a later success outcome and treats seat_log rows like CardRun rows; `mob_process.py record` accepts the rows.
- Guard: a missing script also exits 2, so every refusal test first requires a good launch in a sibling repo to exit 0 (`module_works`). One test that passed today by seeing an empty history was fixed to assert the two rows first.
- The live-lock test makes the green's subprocess wait for the lock (up to the seat-use lock wait, 30 s) before it refuses.
- No file in this addendum was written with a heredoc. I did run one stray heredoc that only printed "no-op" and wrote nothing.
