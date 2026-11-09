# Card 298 slice (architect seat, ctx-2, Claude, read-only; reply committed by the facilitator unchanged below the line)

Facilitator's notes before red (facts, not the seat's text):
- The seat's open question (does any process outside mob-crosscli read `mob-decider.lock` directly?) was answered by the facilitator: `git grep -n "mob-decider"` in both repositories (excluding tests, records and docs) finds only `decider.py` (docstring at line 314) and `facilitator.py` (`DECIDER_LOCK` at line 1098 and the docstring at line 1132). No hook or tool reads the file.
- The seat ran `LiveVote` and `tests.test_writer_isolation` more than once to read the counts (it discloses two earlier runs whose tail cut off the summary). Its counts: 8 LiveVote tests in 19.397 s and 31 writer-isolation tests in 70.241 s are `rerun` by the seat; the whole-suite duration is unrun.
- Design chosen: option (a), one holder file per run in the directory `mob-decider.lock.d`, with `DECIDER_LOCK` and `decider_lock_path` unchanged so card 299 can still name the same constant.
- The green seat is Codex (full access, `--json` stream, delete rule in the brief); the red seat is Claude QA.

---

Slice for card 298 (ARCHITECT, ctx-2). Nothing was written and no git state changed.

**1. Design decision: (a), one holder file per run in a lock directory**
- **Layout.** The lock is the directory `mob-decider.lock.d`, a sibling of `mob-decider.lock` in the git dir. It holds one file `<token>.json` per run.
- **Data written.** Each file holds `{"pid", "started", "token"}`, the same keys as today, with `token = os.urandom(8).hex()`.
- **New helpers in `facilitator.py`.**
  - `decider_holders_dir(checkout)` returns `decider_lock_path(checkout).with_name(DECIDER_LOCK + ".d")`, or None when not a git checkout.
  - `decider_holders(checkout)` returns the list of `(path, dict)` for readable entries.
  - `DECIDER_LOCK` and `decider_lock_path` stay as they are, so card 299 can still name the same constant.
- **Registering.** `hold_vote_lock` runs `mkdir(exist_ok=True)`, then `os.open(dir/<token>.json, O_CREAT|O_EXCL|O_WRONLY)` and writes `mine`.
  - The token is unique per run, so no two runs ever contend on one file.
  - `O_EXCL` is atomic on both NTFS and POSIX.
  - No shared file is rewritten, so no mutex file is needed.
  - Write to `<token>.tmp` and `os.replace` to `<token>.json`, so a reader never sees a half-written entry.
- **Releasing.** `release` unlinks only its own `<token>.json`. It then tries `rmdir` on the directory, and that fails harmlessly if another entry exists.
  - `rmdir` only succeeds on an empty directory, so the last run out removes the directory with no counter and no race.
  - Release warns on `OSError`, as today, and never raises.
- **Reading, `decider_lock_live`.** It is true if any entry in `decider_holders` has `pid_alive(pid)`. That makes `Writers._claim` at line 1170 and `push` at line 1202 refuse while ANY entry is live, with no change to those two callers.
- **Dead entries.**
  - Cleaning happens in `hold_vote_lock` before it registers: for each entry whose pid is dead, `unlink` it and ignore `FileNotFoundError`.
  - `decider_lock_live` never deletes.
  - An unreadable or non-dict entry counts as stale and is removed by the same sweep.
  - Two runs sweeping at once are safe because `unlink` is idempotent.
- **Same pid in two runs.** Liveness is per pid, so both entries show live. Ownership is by filename token, so each run deletes only its own file.
- **Backward compatibility.**
  - If the old single file `mob-decider.lock` exists, `decider_lock_live` also reads it as a legacy holder, live while its pid is alive. This is a fall-through only for a file left by an old process; the new code never creates it.
  - `hold_vote_lock` unlinks a legacy file with a dead pid, ignoring errors. A live legacy file is left alone and the new run still registers, so the checkout stays refused on either shape.
- **Rejected.** Option (b) needs a mutex file with its own stale rule, and a crash while holding it blocks everyone. The card's suspect 2 is exactly the shared-file rewrite race.
- **Two things I changed from the declaration.**
  - The declaration's acceptance check "no lock file remains at the path `git rev-parse --git-path mob-decider.lock` gives" is met because the legacy file is never created. The red tests should also assert that the `.d` directory is gone.
  - The declaration's fix 6 says the format must be "documented for 299". The docstrings of `hold_vote_lock` and `decider_holders` carry that.

**2. Three cycles (one criterion each, caps 1+1+1)**
All three go in `tests/test_decider_runner.py` as a new class `LiveVoteOverlap(LiveVote)`, so `setUp`, `attempt`, `assert_refused`, `assert_clean_record` and `assert_released` are reused. Subclassing would rerun the 8 parent tests, so instead put shared helpers in a mixin or call the parent helpers through a small base. The green seat can restructure that; the class name stays. A helper `overlap(order)` follows this pattern.
- Patch `decider.vote_seat` once.
- Start two `decider.run` threads with `Event` gates, each wait bounded by `WAIT` (60 s).
- A's voter sets `a_in` and waits on `b_in`.
- B's voter sets `b_in` and waits on `a_gone`, or on whichever release event the order calls for.
- The "A ended" signal is set by wrapping `decider.hold_vote_lock` so that `release` sets an Event after it runs. Nothing sleeps.

*Cycle 1: A ends first, B still live.*
- Test: `test_a_commit_and_push_after_the_first_run_ends_are_refused_while_the_second_votes`.
- Behaviour: after A's release, B's voter calls `self.attempt(..., "commit")` and `self.attempt(..., "push")`. Both must be refused with owner decider and zero git writes, using `assert_refused`.
- Records: both records pass `assert_clean_record`. Each thread writes to its own `out` directory, because `decider-result.json` is a fixed name.
- Why it cannot pass on partial work: today A's release unlinks the file, so the write lands, and the test fails.
- A one-lock-per-run fix that does not count B also fails.

*Cycle 2: either order, lock gone at the end.*
- Tests: `test_the_checkout_stays_refused_until_the_last_run_ends_a_first` and `..._b_first`.
- In each, the first-ending run's release is followed by an attempt that must be refused. That attempt runs from the other run's voter, before its return.
- After both threads join, assert `not decider_lock_path(repo).exists()`, `not decider_holders_dir(repo).exists()`, and `assert_released()`.
- Why it cannot pass on partial work: B-first with a "release drops all" fix fails the refusal. A leftover directory fails the final assert.
- Mid-state note: before green, `decider_holders_dir` does not exist, so cycle 2's red has to reference it. Importing it as `fac.decider_holders_dir` makes the test fail with AttributeError. That is a legitimate red, but cycle 1 must not depend on it.

*Cycle 3: stale takeover.*
- Test 1: `test_a_dead_only_lock_is_replaced_and_the_new_run_refuses_writes`.
- Test 2: `test_one_dead_and_one_live_entry_refuse_writes_until_the_live_run_ends`.
- Dead pid: `p = subprocess.Popen([sys.executable, "-c", "pass"]); p.wait(); dead = p.pid`. A test should also assert `not fac.pid_alive(dead)` as a guard.
- Planted entry: the test writes `{"pid": dead, "started": ..., "token": "dead0001"}` to `<holders_dir>/dead0001.json`.
- Live entry: a real run or a hand-planted entry with `os.getpid()`.
- Test 1: start a run, and in its voter the attempt is refused. After the run, the dead entry is gone.
- Test 2: with the live entry in place, a facilitator attempt is refused. After the live entry's file is removed (plant it, then remove it inside the test), it is allowed.
- Legacy test: a plain old `mob-decider.lock` with a dead pid is replaced and removed. This test is small and covers the compat rule.
- Why it cannot pass on partial work: a bare counter or a single-pid reader fails test 2, and test 1 fails if the dead entry blocks.
- The Windows pid-reuse window is tiny after `p.wait()`, which is why the guard assert exists.

**3. Files**
- The green may touch `decider.py` (only `hold_vote_lock`, lines 313 to 344 at HEAD, plus its docstring) and `facilitator.py` (`decider_lock_live`, plus new `decider_holders_dir` and `decider_holders`).
- The green must not touch `tests/test_decider_runner.py` assertions that exist, `tests/test_writer_isolation.py`, core/backends.json, `Writers._claim`, `Writers.push`, `pid_alive`, `decider_lock_path` or `DECIDER_LOCK`.

**4. Verification commands, from `C:/Users/<user>/mob-crosscli`**
- `python -m unittest tests.test_decider_runner.LiveVote`: 8 tests today. After green the new class adds its own, so the count is 8 plus the new tests.
- `python -m unittest tests.test_writer_isolation`: 31 tests, OK, in 70 s.
- Whole suite before pushing: `python -m unittest discover tests`, per README line 109. There are 85 entries in `tests/`, and I did not run the suite, so its duration is unknown. The 19 s and 70 s figures above suggest several minutes. Run it in the background.

**5. Risks and the one open question**
- The heads-equal check (`head_start` equals `head_end`) is only as safe as the facilitator's own write path. A write through plain `git` is card 299's.
- Sweeping dead entries at registration means a crash between sweep and register is harmless, but another run could land in the gap. That is fine because it registers its own file.
- The `.d` directory could be mistaken for a lock file by an external scan. I found no code that globs `mob-decider*`.
- Open question I could not answer from the files: whether any process outside mob-crosscli (a hook script or the phrase-lang checkout's tooling) reads `mob-decider.lock` directly and would then see "no lock" during a vote. I searched only this repo (`grep` found only `decider.py` and `facilitator.py`).

**6. Numbers**

| Number | Source |
|---|---|
| HEAD e40962c | `git rev-parse --short HEAD`, run now |
| 8 LiveVote tests, 19.397 s | `python -m unittest tests.test_decider_runner.LiveVote`, run now |
| 31 writer-isolation tests, 70.241 s | `python -m unittest tests.test_writer_isolation`, run now |
| 20.1 s and 1 m 9.9 s wall | earlier `time` runs of the same two commands |
| 85 entries in tests/ | `ls tests \| wc -l` |
| Lines 313 to 344 (`hold_vote_lock`), 378 (taken), 1098, 1101, 1131, 1143, 1170, 1202 | `grep -n` and `sed -n` on `decider.py` and `facilitator.py`, run now |
| README line 109 (discover command) | `grep -n unittest README.md MOB.md` |
| Whole-suite duration | not produced (unrun) |
| 60 s `WAIT` | `grep` of `tests/test_decider_runner.py` line 292, as read; the declaration quotes it, record-only for the value of the line number |
