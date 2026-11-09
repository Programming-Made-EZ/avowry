# Card 313 slice (architect seat, fresh context, read-only; nothing written, no git state changed, no `gh`; reply committed unchanged by the facilitator)

Disclosures (the seat's):
- It ran `PYTHONDONTWRITEBYTECODE=1 python -m unittest tests.test_decider_runner.LiveVoteOverlap` and `...LiveVote` once each, from `C:/Users/<user>/mob-crosscli`. Its `| tail -4` cut off the summary lines, so pass or fail is unconfirmed (record-only). Wall times 16.2 s and 16.1 s (`time`, rerun).
- Test counts from counting `def test_` lines with awk: 8 in `LiveVote` (lines 297-489) and 7 in `LiveVoteOverlap` (line 490 on) (rerun count, not a unittest summary).
- One `python -I` probe in a temp directory, deleted afterwards. `git status --porcelain` shows only `?? .mob/`. HEAD `1590679` (rerun).

## 1. Design decision: confirm the bounded retry, with one narrowing

**The rmdir claim holds.** The probe (python 3.15.1, Windows, rerun): `Path.rmdir()` on a directory holding the temporary file raises `OSError` (`WinError 145`, `errno 41`), both while the descriptor is open and after it is closed, and after the replace too; it succeeds only on an empty directory. `os.open` of a new file in a missing directory raises `FileNotFoundError` (`errno 2`). POSIX not run: rmdir on a non-empty directory is `ENOTEMPTY`, an `OSError` there as well (record-only).

**Window map in the current code** (line numbers from the seat's `Read` of `decider.py`):
- The directory can be lost only between `directory.mkdir` (:325) and `os.open` (:355). That is the defect.
- From the open onward the `.tmp` keeps it non-empty. Release's rmdir (:368) swallows the error and succeeds only on an empty directory. The sweep skips `.tmp` names (:331); only the owning run deletes its own `.tmp`.
- So the replace never needs its own mechanism. It stays in the retry as a harmless catch, and cycle 2 proves it.

**Narrowing.** Catch `FileNotFoundError` only around the open, write and replace, not around `mkdir` or the sweep: a missing git directory makes `mkdir` raise `FileNotFoundError` too, and looping and warning on that would change card 315's silent no-op. That case keeps going to the outer `except OSError` and returns a no-op.

**Structure.** The constant `VOTE_LOCK_ATTEMPTS = 5` goes just above `hold_vote_lock`.
```
token, mine, path, temporary                  # outside the loop; legacy = decider_lock_path(repo) stays too
try:
    for _ in range(VOTE_LOCK_ATTEMPTS):
        directory.mkdir(exist_ok=True)        # inside: each retry recreates the directory
        <iterdir sweep + legacy sweep, unchanged>   # inside: idempotent
        try:
            descriptor = os.open(temporary, O_CREAT|O_EXCL|O_WRONLY)
            with os.fdopen(...) as handle: handle.write(mine)
            os.replace(temporary, path)
        except FileNotFoundError:
            continue
        break
    else:
        print(f"warning: could not register the vote lock {path} after {VOTE_LOCK_ATTEMPTS} attempts: the holders directory {directory} was removed each time ({exc})", file=sys.stderr)
        return lambda: None
except OSError:
    return lambda: None                       # any other OSError: today's silent no-op, unchanged
```
Bind the exception (`except FileNotFoundError as exc`) and keep it in a variable so the warning can name it. The warning names the vote lock and the failure, on one line. `release` is untouched; after exhaustion the caller holds the no-op lambda (calling it twice is harmless). The docstring gains one sentence: "at most 5 attempts when another run removes the empty directory; gives up with one stderr warning".

**Known limit.** If the replace raised `FileNotFoundError` while the `.tmp` still existed, the retry's `O_EXCL` open would raise `FileExistsError`, giving the silent no-op. Only an injected test can produce this; no unlink is added for it.

## 2. Three cycles (class `VoteLockRegistration(RunnerCase)`, kept)

**Class setUp.** `super().setUp()`, `self.attempts = []`, `self.n = 0`, and borrow `head`, `attempt`, `assert_refused`, `assert_released` from `LiveVote`, as `LiveVoteOverlap` does. No origin (a push is refused before any git call). No thread, no sleep, no `vote_seat`. **Assertions use the literal `5`, not `decider.VOTE_LOCK_ATTEMPTS`**, otherwise every test is red today as an AttributeError for the wrong reason.

**Helpers:** `holders_dir = fac.decider_holders_dir(self.repo)`; `count_opens(strikes, strike)` patches `decider.os.open` (wrapper passes everything through, counts calls whose path ends in `.tmp`, calls `strike()` first when the count is in `strikes`, then the real open, returns the count); `refused_both(where)` runs `attempt(where, "commit")`, `assert_refused([where], "commit")`, resets `self.attempts`, then `attempt(where, "push")` and `assert_refused([where], "push")` (owner decider, zero git writes, through a fresh `fac.Writers()`). Patches wrap only the `hold_vote_lock` call; attempts run after the `with` block, while B's registration is held.

### Cycle 1 (Done check 1)

| Test | Technique | Asserts | Today |
|---|---|---|---|
| `test_a_lone_run_registers_once_and_leaves_nothing` [Z] | open counter only | one `<token>.json` exists; open count is 1; after `release()` no file and no directory | green |
| `test_a_removal_before_the_open_still_registers` [O] | `strikes={1}`, `strike=holders_dir.rmdir` | open count 2; `fac.decider_holders` has 1 entry with `pid == os.getpid()` and `token == path.stem`; `decider_lock_live` true; `refused_both` | RED |
| `test_a_removal_before_each_of_the_first_four_attempts_still_registers` [B] | `strikes={1,2,3,4}` | open count 5; same single-holder assertions; `refused_both`; after `release()` the directory is gone and `assert_released()` passes | RED |
| `test_a_removal_before_every_attempt_stops_after_five_with_one_warning` [E] | `strikes={1,2,3,4,5}`, `redirect_stderr(io.StringIO())` | open count equals 5; no exception; `fac.decider_holders` is `[]` and `decider_lock_live` false; stderr is one line containing "vote lock" and "5 attempts"; a second `release()` raises nothing and adds no stderr | RED |

Red on today's code (HEAD `1590679`): [O] and [B] first fail at `assertEqual(len(holders), 1)` (the first `FileNotFoundError` returns a no-op: 0 holders). [E] first fails at `assertEqual(opens, 5)` (1 against 5); its stderr assertion would fail next.

### Cycle 2 (Done check 2): one outline test, one replace-point test, one readers test

`test_a_release_by_the_other_run_at_each_point_leaves_this_run_registered`: three subTests `after the mkdir`, `before the temporary open`, `before the replace`. Each starts with `a = decider.hold_vote_lock(self.repo)` (a real run A); the strike is `a()` once only (a second call would print a warning). Hooks: after the mkdir, patch `type(holders_dir).mkdir` with a wrapper that calls the real mkdir and then strikes once when `self == holders_dir`; before the open, the open wrapper; before the replace, the same shape on `os.replace`, filtered on a `.tmp` source. Asserts: B has exactly one holder after `a()` removed its own; `refused_both`; then `b()` and the directory is gone. Today: the first two subTests RED (the same `len(holders)==1` failure); the third born green.

`test_the_replace_point_cannot_lose_the_directory`: strike before the replace with `a()`; right after, assert `holders_dir.is_dir()`, the `.tmp` is in it, and the open count is 1 (no retry); afterwards B's `<token>.json` exists. Born green.

`test_the_readers_see_the_holder_and_delete_nothing` [I]: B registers; plant a dead-pid file in the directory (a `dead_pid()` copied from `LiveVoteOverlap`); read `fac.decider_holders` and `fac.decider_lock_live` twice, both reads agree; `sorted(os.listdir)` is identical before and after, so the dead file is still there. Born green.

Mutations for the born-green tests (D193 G2), applied to scratch copies:
- **M2b.** In `decider.py`, change `temporary = directory / f"{token}.tmp"` to `directory.parent / f"{token}.tmp"`. Must fail: `test_the_replace_point_cannot_lose_the_directory` at `assertTrue(holders_dir.is_dir())` (the before-replace subTest fails too).
- **M2d.** In a scratch copy of `facilitator.py` only, add `path.unlink()` for a dead holder inside `decider_holders`. Must fail: `test_the_readers_see_the_holder_and_delete_nothing` at the listdir comparison.
- **M2a** (red-coverage guard, optional): move `directory.mkdir(exist_ok=True)` above the loop. Fails the first two subTests and [O].

### Cycle 3 (Done check 3): born green

`test_the_first_run_to_end_leaves_the_checkout_refused_in_either_order`: two direct `hold_vote_lock` calls, A then B. Subtest `a_first`: end A, `refused_both("after A ended")`, end B, the directory is gone, `assert_released()`. Subtest `b_first`: the roles swapped.

`test_a_dead_holder_and_an_empty_directory_are_swept_and_the_run_is_registered` [E]: subtest 1 plants a dead-pid holder, registers; the dead file is gone, one own holder exists, `refused_both` holds. Subtest 2: an empty directory; register; one own holder exists. [S] is the existing suites staying unchanged (section 4).

Mutations (scratch copies of `decider.py`):
- **M3a.** Replace the `try: directory.rmdir() except OSError: pass` block in `release` with `pass`. Must fail: the order test, at the directory-gone assertion.
- **M3b.** Replace `path.unlink()` in `release` with `for f in directory.glob("*.json"): f.unlink()`. Must fail: the order test, at the `refused_both` call after the first end.
- **M3c.** Change the sweep's `entry.unlink()` in the dead-holder branch (:336) to `pass`. Must fail: the dead-holder test, at the dead-file-gone assertion.

## 3. Files

**The green may touch:** `decider.py`: the `hold_vote_lock` body, its docstring, and the new constant `VOTE_LOCK_ATTEMPTS`.
**The green must not touch:** `facilitator.py` (`decider_holders_dir`, `decider_holders`, `decider_lock_live`, `Writers`): `git diff --name-only` must not name it; existing assertions in `tests/test_decider_runner.py`; `tests/test_writer_isolation.py`; `core/backends.json`; `release` (its body stays as it is; only `hold_vote_lock` changes).
The red seat adds `VoteLockRegistration` after `LiveVoteOverlap` in `tests/test_decider_runner.py`.

## 4. Verification commands (from `C:/Users/<user>/mob-crosscli`, or the worktree)

| Command | Expected | Label |
|---|---|---|
| `python -m unittest tests.test_decider_runner.VoteLockRegistration` | new; 3 reds in cycle 1 and 2 reds in cycle 2 before green; all green after | not runnable yet |
| `python -m unittest tests.test_decider_runner.LiveVoteOverlap` | 7 tests (awk count); about 16 s | rerun by the seat, summary unseen |
| `python -m unittest tests.test_decider_runner.LiveVote` | 8 tests (awk count); about 16 s | rerun by the seat, summary unseen |
| `python -m unittest tests.test_decider_runner` | whole file; the classes above plus the new one | record-only |
| `python -m unittest tests.test_writer_isolation` | 31 tests, about 70 s, from the card 298 record | record-only; gate only if `facilitator.py` is touched |
| `git diff --name-only 1590679` | `decider.py` and `tests/test_decider_runner.py` only | record-only |

## 5. G3: steps and gaps of `hold_vote_lock` after the fix

| Step or gap | Fails | Crash falls here | Another run's rmdir | Label |
|---|---|---|---|---|
| Outside git: `decider_holders_dir` is None | no-op, intended | nothing written | n/a | record-only: no new test; `git grep -n "is None" decider.py` shows it |
| `mkdir` | `OSError` gives the no-op (card 315, not retried) | an empty directory remains; the next `mkdir` accepts it and release removes it | none can precede it | record-only |
| Gap mkdir to iterdir | `FileNotFoundError` (an `OSError`) gives `entries=[]` | n/a | harmless | rerun: cycle 2 after-the-mkdir subTest |
| Sweep and legacy sweep | each error swallowed | nothing changes | idempotent | rerun: cycle 3 dead-holder test |
| Gap sweep to open | `FileNotFoundError` gives a retry (the defect) | nothing on disk | **the defect**, fixed by the retry | rerun: cycle 1 [O], [B], [E]; cycle 2 before-the-open subTest |
| `os.open` | other `OSError` gives the no-op | leaves nothing | n/a | record-only |
| Write | `OSError` gives the no-op and a `.tmp` (card 314, closed as not planned) | a `.tmp` is skipped forever | rmdir refused | record-only |
| Gap open to replace | n/a | a `.tmp`; the directory stays | rmdir refused: non-empty | rerun: cycle 2 replace-point test |
| `os.replace` | `FileNotFoundError` gives a retry; other `OSError` gives the no-op | a `.tmp` remains | rmdir refused | rerun: cycle 2 before-the-replace subTest (rmdir refused only) |
| After replace | n/a | a holder with a dead pid; the next sweep removes it and the readers ignore it | rmdir refused | rerun: cycle 3 dead-holder test |
| Release unlink | warns on `OSError`, as today | a dead-pid holder, as above | n/a | record-only |
| Gap unlink to release rmdir | harmless; `OSError` swallowed | an empty directory remains | removes the new run's fresh directory: covered by the retry | rerun: cycle 1 [O] (bare rmdir stands for it); the real-A variant is cycle 2 |
| After exhaustion | one warning; a no-op release | nothing | n/a | rerun: cycle 1 [E] |

"Rerun" is a label for after the red seat writes the tests. Nothing is blocked.

## 6. The green workspace

**Checkout path:** nothing depends on it. `git grep -n "mob-crosscli\|ringe\|show-toplevel" -- tests/test_decider_runner.py decider.py` returns nothing (rerun). Imports resolve through `HERE = Path(__file__).resolve().parent` (`tests/test_decider_runner.py` line 13, `decider.py` line 28), so code and tests load from the worktree. `run_runner` starts `HERE.parent/decider.py`, again from the worktree. `RunnerCase.setUp` makes its own repository with `git init` in a `TemporaryDirectory`, so the lock path is in that temporary repository's `.git`, not the worktree's. `core/backends.json` is not used when `--backends` is passed.

**In a linked worktree:** `git rev-parse --git-path mob-decider.lock.d` resolves under the per-worktree git dir (confirmed on an existing linked worktree `wt-290-probe`, rerun: `C:/Users/<user>/mob-crosscli/.git/worktrees/wt-290-probe/mob-decider.lock.d`; in the main checkout it is `.git/mob-decider.lock`, rerun). `decider_holders_dir` derives from `decider_lock_path(...).with_name(DECIDER_LOCK + ".d")` and handles absolute and relative results, so it is correct in a worktree. A facilitator `Writers` in the main checkout does not see a lock held in the worktree; that does not matter for these tests.

**Branch:** `card-313-green` is created from HEAD `1590679`. `.mob/` is untracked in the main checkout, so it is absent in the worktree; the tests do not read it from the checkout.

## 7. Risks and one open question

- A unit-test patch of `os.open` is global to the process: the wrappers must pass every other path through and be scoped to the `hold_vote_lock` call; start no threads inside the patch.
- Patching `type(holders_dir).mkdir` needs a plain function and `self == holders_dir` filtering, or other `mkdir` calls in the test break.
- Python is 3.15.1 here (rerun). The mkdir hook depends on `pathlib` keeping `mkdir` as a class method, which `mock.patch.object` on `type(path)` handles.
- Overlapping runs still trip the existing `seat-use` append-only check (seen in the stderr of the seat's `LiveVoteOverlap` run). A separate bug (card 307), out of scope.
- The orphan-`.tmp` limit (section 1) is real only for an injected replace failure; not worth a line of production code.
- **Open question:** should a `FileNotFoundError` from the `mkdir` step (missing git directory) also warn on stderr? The seat recommends no: that is card 315, and the narrowing keeps this card to one observable change. If the product owner prefers it, the loop moves one `try` outward and cycle 1 [E] gets a sibling test.
