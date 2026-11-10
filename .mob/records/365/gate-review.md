# Card 365 gate review (independent gate reviewer, fresh Claude context, 2026-11-10)

Scope: concurrent seat-use appends never fail the append-only check. Code: mob-defaults, red commit 224738b9416b57f65bed1476e0056b5717213bbc, green commit 5d5da21e484fe0bb08b4d1878b649f62fa58b373 (main), base 77cebb7eb4640585151264a88d70117207429ad9. Records read: declaration-proposal.md, slice-architect.md, red-record.md, seat-changes.md, second-review.md, TEAM_WORKING_AGREEMENT.md rules (D219/D226 step 0, red-first, red and green on different seats, Card format rule). Work was done in throwaway clones outside both checkouts (`<user>`\gate365-review-red, -green, -mut); nothing was committed, pushed or edited in either checkout. The whole mob-defaults suite was not run (the facilitator's baseline gate ran it at 5d5da21: 1966 tests, failures=5 all pre-existing, skipped 1).

## VERDICT: PASS (with notes; no blocking finding)

Reasons: step 0 holds; the replay of the red fails for exactly the eleven red ids with zero errors, all AssertionError; the green turns all of them green and leaves 0 decorators; the green diff is confined to the declared files and its test diff is decorator removal plus the one comment fix; four mutation checks each fail the tests they should (and a fifth probe, below, found one untested branch); the six-process test passed 5 of 5; the design matches slice section 2. Findings are ranked in section 7 and are non-blocking.

## Step 0 (D219/D226) and seats

```
$ git merge-base --is-ancestor 224738b9416b57f65bed1476e0056b5717213bbc 5d5da21e484fe0bb08b4d1878b649f62fa58b373 && echo ANCESTOR_OK
ANCESTOR_OK
$ git merge-base --is-ancestor 5d5da21e484fe0bb08b4d1878b649f62fa58b373 main && echo GREEN_ON_MAIN
GREEN_ON_MAIN
$ git rev-parse 224738b^
77cebb7eb4640585151264a88d70117207429ad9
```
The red precedes the green on main, and the red's parent is the declared base. Both commits are authored by the facilitator identity and carry no trailer (`git log -2 --format=%B | grep -ci co-authored` gives 0).

Seats: red-record.md names Cursor as the red writing seat (QA, Probation rung 1). seat-changes.md records the green seat change Muse -> a fresh Claude context as a RECORDED STOPGAP: Muse out of weekly quota (429, reset 2026-11-16T00:00:00Z), quota not behaviour, so no repair owed; DRIVERS.md has the Muse row UNAVAILABLE (weekly quota), reset 2026-11-16T00:00Z, a one-time re-probe scheduled. Red and green are on different seats (Cursor / fresh Claude). Cursor could not take the green (it wrote the red). The amendment line is committed before the seat launched, as D236 asks. Note: the red commit also contains the characterisation (facilitator ruling in red-record.md: shared file, no hunk split); acceptable.

## (1) Replay of the red on the pre-change tree

Clone at 77cebb7eb4640585151264a88d70117207429ad9; `git checkout 224738b -- tests/test_decider_rotation.py tests/test_decider_gate.py tests/test_decider_record.py tests/test_decider_runner.py tests/fakes/fake_barrier_voter.py` (5 files, 545 insertions, no source file touched); decorators counted before the strip: rotation 4, gate 5, record 0, runner 2 = 11; `sed -i '/^\s*@unittest.expectedFailure/d'` on the four files; `grep -c expectedFailure` = 0 in each.

`python -m unittest tests.test_decider_rotation tests.test_decider_gate tests.test_decider_record tests.test_decider_runner`:
```
Ran 123 tests in 146.055s
FAILED (failures=12)          FAIL blocks: 12   ERROR blocks: 0   Tracebacks: 12   AssertionError lines: 12
                              AttributeError/TypeError/NameError/ImportError lines: 0
```
The 12 FAIL lines (11 ids; 4b counts once per subtest, as red-record.md says):
- 365-1a ConcurrentStarts.test_a_run_started_while_another_checks_its_voters_does_not_refuse_either
- 365-3a ConcurrentStarts.test_each_runs_record_points_at_its_own_lines_and_claudes_reason_is_recomputed_from_the_file
- 365-5a SingleWriterFallback.test_outside_git_two_runs_in_one_process_are_serialised_with_one_warning
- 365-1c VerifyHistory.test_a_line_another_run_appended_while_the_voters_ran_is_accepted
- 365-2f SeatUseLock.test_a_dead_holders_lock_is_swept_and_the_run_goes_ahead
- 365-2e SeatUseLock.test_a_live_holder_refuses_the_run_after_the_wait_and_nothing_is_written
- 365-4b SeatUseLock.test_a_refused_run_held_the_lock_for_its_checks_and_released_it (x2 subtests)
- 365-2g SeatUseLock.test_the_history_is_not_verified_past_a_live_holder
- 365-4a SeatUseLock.test_the_lock_is_held_for_each_check_and_append_and_never_for_a_probe_or_a_vote
- 365-1d LiveVoteOverlap.test_two_overlapping_runs_both_pass_with_a_clean_record_and_one_seat_use_line_each
- 365-1b SixSeatsAtOnce.test_six_runs_started_at_once_each_write_one_line_and_pass

This is exactly the red id list of red-record.md; ERROR count zero; every message is an AssertionError (for example "SeatRefused('... is not append-only ...') is not an instance of dict" for 1a/3a/5a, "5 != 0 : run a" for 1d, "0 != 2" for 2e, "SeatRefused not raised" for 2g). The characterisation ids (2a-2d, 3b) pass on the pre-change tree (they are in the 111 passes).

At 5d5da21e484fe0bb08b4d1878b649f62fa58b373 (fresh clone), the same four modules:
```
$ grep -c expectedFailure tests/test_decider_rotation.py tests/test_decider_gate.py tests/test_decider_record.py tests/test_decider_runner.py
0 0 0 0
Ran 123 tests in 162.237s
OK            (FAIL/ERROR lines: 0)
```

## (2) What the green changed

`git diff --stat 224738b..5d5da21`:
```
 decider.py                     |  34 ++++---
 facilitator.py                 | 203 +++++++++++++++++++++++++++++++++++++++--
 tests/test_decider_gate.py     |   5 -
 tests/test_decider_rotation.py |   4 -
 tests/test_decider_runner.py   |   4 +-
 5 files changed, 221 insertions(+), 29 deletions(-)
```
Only facilitator.py, decider.py and three of the four test files (tests/test_decider_record.py is unchanged, correct: it holds only the characterisation 3b with no decorator, so the brief's "four test files" is three in fact). I read the whole test diff: gate 5 lines deleted (the five `@unittest.expectedFailure  # 365-2e/2f/2g/4a/4b`), rotation 4 deleted (1c, 1a, 3a, 5a), runner 2 deleted (1d, 1b) plus one comment line changed (test_decider_runner.py:590, the stale "a separate bug" comment, now "the rc and integrity of overlapping runs are asserted in the 365-1d test below", which the slice and red-record asked for). No assertion, helper, fixture or test body changed; tests/fakes/fake_barrier_voter.py not touched.

## (3) Mutation checks (scratch clone at the green; reset with `git checkout -- .` between mutations)

| Mutation | Tests run | Result |
|---|---|---|
| `hold_seat_use_lock` made a no-op (`yield; return` at its top) | SeatUseLock, ConcurrentStarts, SixSeatsAtOnce, LiveVoteOverlap | 16 tests, failures=7: 4a, 4b (both subtests), 2e, 2f, 2g and 1b (SixSeatsAtOnce). 1b and 4a fail as required. (1a and 1d still pass without the lock: they are threads inside one process where the prefix rule alone suffices; the lock is what 1b, 4a, 2e-2g pin.) |
| prefix rule reverted to equality (`lines != self.seat_use_seen`) | SeatUseLock, ConcurrentStarts, VerifyHistory, LiveVoteOverlap, SingleWriterFallback | 21 tests, failures=11, errors=6. 1a FAIL, 3a FAIL, 5a FAIL, 1d FAIL; 1c ERROR (the append's own read-back now refuses, so the error is raised in the test's setup rather than as an assertion; it is still the red-green signal), plus the other overlap tests. 1a, 1c, 1d fail as required. |
| seen-line check disabled (`False`: a removed, changed or emptied seen line accepted) | PrefixRule, LoneRun | 9 tests, failures=12: 2a (end, start), 2b (end, start), 2c (end, start), 2d (three subtests at each layer). 2a-2d fail as required; 3b stays green (not a tamper test). |
| lock taken around a probe (a `probe_drivers` call inside the `start_voters` critical section) | SeatUseLock | failures=3: 4a ("the seat-use lock was held during a probe: {'probe': [F,F,F,F,F,T,T,T,T,T], ...}") and both 4b subtests. 4a fails as required. (A first attempt that put the probe in `gate_seat` survived because the decider path never goes through `gate_seat`, see finding F2.) |

## (4) Acceptance criteria (the card's three scenarios)

Covering tests (one run result each; all ran at 5d5da21 in one `-v` run: 24 tests in 23.978s, OK):
- Scenario 1, six seats launched together all start and six lines are kept: tests.test_decider_runner.SixSeatsAtOnce.test_six_runs_started_at_once_each_write_one_line_and_pass (1b: six real decider.py processes at a file barrier, all exit 0, six terminated JSON lines, `integrity.ok`, no "not append-only" on stderr) ... ok; also LiveVoteOverlap.test_two_overlapping_runs_both_pass_with_a_clean_record_and_one_seat_use_line_each (1d) ... ok; ConcurrentStarts.test_a_run_started_while_another_checks_its_voters_does_not_refuse_either (1a) ... ok; VerifyHistory.test_a_line_another_run_appended_while_the_voters_ran_is_accepted (1c) ... ok.
- Scenario 2, a line removed, changed or emptied is still refused: PrefixRule 2a (removed), 2b (changed plus a foreign line), 2c (emptied), 2d (removed plus foreign lines, equal and longer length, reorder), each at the start layer and the end layer, 8 methods ... all ok (their mutation proof is in section 3); VerifyHistory removed/changed/emptied existing tests ... ok.
- Scenario 3, same rotation answer / `line_index` is the verified position: ConcurrentStarts.test_each_runs_record_points_at_its_own_lines_and_claudes_reason_is_recomputed_from_the_file (3a) ... ok (Claude's rotation_reason recomputed from the committed file); LoneRun.test_a_lone_runs_vote_holds_the_record_keys_and_its_line_follows_the_seeded_lines (3b) ... ok; the existing rotation/CommitVoters/Auditor tests passed unchanged in the 123-test module run.
- Supporting: SeatUseLock 2e, 2f, 2g, 4a, 4b ... ok; SingleWriterFallback 5a ... ok.

Flakiness of the six-process test, `python -m unittest tests.test_decider_runner.SixSeatsAtOnce` five times in a row at 5d5da21:
```
Ran 1 test in 6.720s OK
Ran 1 test in 6.006s OK
Ran 1 test in 7.412s OK
Ran 1 test in 6.871s OK
Ran 1 test in 6.017s OK
```
5 of 5 passed; no flakiness observed (the same test also passed inside the 123-test run and the 24-test run, 7 of 7 in all). Before the change the same scenario gave non-zero exits every time (red replay: exits like {2, 5} on 5 of 6 seats).

## (5) Correctness of the facilitator.py and decider.py diff against slice section 2

Read in full (git diff 77cebb7..5d5da21). Matches the slice:
- Lock never held across a probe: `start_voters` calls `run.reprobe_if_stale(...)` BEFORE `with run.seat_use_lock(...)`; `reprobe_if_stale` raises SeatRefused ("the probe went stale while waiting for the seat-use lock ... never repeated while the lock is held") if the lock is held and the probe is stale; `gate_seat` likewise refreshes before the lock. Mutation 4 and test 4a prove the probe slots.
- Never across a vote: `start_voters` returns (lock released by its `with`) before `hold_vote_lock(repo)` and `vote_seat` in `run()`; `verify_history` runs after `release()` of the vote lock, and takes the seat-use lock only for its single read. So no nesting with the vote lock; nesting of the seat-use lock itself raises RuntimeError (`_seat_use_held`).
- Release by token: `_release_seat_use_lock` reads the file and unlinks only when the token matches (returns if another token), retries about 2 s on OSError, falls back to rewriting `{"pid": 0, "released": true}`; `_seat_use_lock_stale` treats `released` or a dead pid as stale, an empty/unreadable file older than 2 s as stale, and a `started` older than 120 s as stale; the sweeper re-reads and unlinks only if the raw content is unchanged. The wait is 30 s (read at call time, patchable) and the refusal names the lock, pid, start time and the file name.
- Prefix rule: `lines[:len(seen)] != seen` refuses; `append_seat_use` returns the verified position (`base`), re-reads through `read_seat_use` and requires `lines[base] == text`; `seat_use_index[context]` feeds `voter_record(..., index, written[:index])`, i.e. `line_index` is the verified position and the Claude reason is recomputed from the lines before it. The lock file lives in the git directory (`git rev-parse --git-path`), per worktree like the history file; `decider.py` carries no forbidden constant (the source-guard tests at test_decider_gate.py:134 and test_decider_record_units.py:182 passed in the module runs; `judged_seats` lives in decider.py as a small helper, no banned name).
- Fallback warning: `hold_fallback_seat_use_lock` keys the lock by resolved history path and warns once per repository per process (set `_FALLBACK_WARNED`, guarded by `_FALLBACK_GUARD`); 5a passes, and other tests on non-git temp repos print one line each (stderr noise only).
- Record-keeping: `check_voters` and `commit_voters` keep their signatures (the `watch(run, seat=None, role=None, pending=())` test double still works).

## (6) Value rule

Yes. The sponsor persona (the product owner, who launches a vote window) gets voters that start at once: six real processes started together all exit 0 and each keeps its own line, where before at least five of six were refused or failed with INTEGRITY and lines were lost or torn. The history stays tamper-evident: another run's appended line is accepted, but every line a run has seen must still be a prefix of the file, so a removed, changed, reordered or emptied line is refused at both the start and the end (mutation check confirms the tests bite). Honest limit: the history is tamper-evident against edits to what a run saw, not authenticated against a cooperating writer that skips the lock (second-review.md finding 5, outside this card).

## (7) Findings, ranked (none blocking)

- F1 (low, carried from second-review.md findings 1-2, 4): residual lock hazards the facilitator already ruled on and filed as a follow-up bug: a live holder swept by the 120 s age rule alone; `_release_seat_use_lock` also unlinks when the lock file cannot be read (held is None but the path exists), so it can remove a successor's lock; the create-then-write gap longer than 2 s on Linux; a zombie or reused pid is a bounded multi-minute outage. Backstopped by the prefix rule and the position check. I agree with the ruling and add one precision: the unreadable-token unlink in `_release_seat_use_lock` (lines `if held is not None and ...` only guards a readable foreign token) is the cheapest fix and belongs in that bug.
- F2 (low, a coverage gap my mutation found): the card-mode branch of `gate_seat` (take the lock around `commit_seat(check_seat(...))` when a seat and probes exist) has no test. Removing that `with self.seat_use_lock(...)` and running test_seat_gate, test_quota_first, SeatUseLock and ConcurrentStarts gave `Ran 44 tests ... OK` (the mutant survived). The decider path, which is this card's scope and the one the parallel window uses, is covered by 1a-1d, 2e-2g, 4a, 4b. The slice's test list never asked for a card-mode test, so this is not a red/green miss; recommend a small follow-up test (a card-mode gate_seat that observes the lock file present at check and append, as 4a does for the decider).
- F3 (low): `gate_seat` passes `None` as the context when the seat is not a `Seat`, so the refusal text would read "in None"; cosmetic.
- F4 (info): the green changed three test files, not four; tests/test_decider_record.py is untouched (correct). The fallback warning prints once per repository, so non-git temp repos in other tests add one stderr line each (noise only, no failure; the full-suite gate showed no new failure).
- F5 (info): the mob-defaults main checkout shows an untracked `.mob/` directory (`git status --short` gives `?? .mob/`); it is not part of either commit.

## (8) Not delivered by card 365 (later steps, as declared)

- Card 374: the start probe still runs with `cwd=repo`, so a `{PROMPT_FILE}` backend's `prompt-*.txt` can still land in the repository root during a probe and trip another run's snapshot (INTEGRITY) when runs overlap; 374's red/green is a later step. The six-process test avoids it because its fake backends do not use `{PROMPT_FILE}`.
- The removal of the one-seat-at-a-time line from the D228 preflight entry (its own Decider, trigger (c), after both cards ship, with before and after vote-window minutes).
- The live parallel window proof (a real window with six seats launched at once on real drivers) and the retirement of the launcher stagger.
- The independent sign-off seat's own full-suite run.

Clean-up: the three review clones (`<user>`\gate365-review-red, -green, -mut) and four small scratch files (`<user>`\gate365-*.out, gate365-mut.py) remain outside both checkouts; they are clones, not worktrees, so there was no `git worktree remove` to run and no recursive delete was used.
