# Card 299 slice (architect seat, fresh context, read-only; reply committed unchanged by the facilitator)

**Disclosures.**
- The seat wrote no repo file, changed no git state, called no `gh`, and ran no recursive delete.
- Probe scripts are in the scratchpad `.../scratchpad/probe299{,b,c}.py`. They worked in `.../scratchpad/p299/`, and every file in it was removed with literal paths. `ls` of that directory afterwards printed nothing.
- The seat ran the three narrow suites once each from `C:/Users/ringe/mob-crosscli` with `PYTHONDONTWRITEBYTECODE=1`. Its `| tail -3` cut the summary lines, so pass or fail is **unconfirmed (record-only)**; the run is not a baseline. Wall times from `date +%s` (rerun): `test_writer_staging` 20 s, `test_writer_isolation` 60 s, `test_decider_runner` 76 s.
- `git rev-parse --short HEAD` gives `3b3e486` in the main checkout (rerun). `git status --porcelain` gives `?? .mob/` in main and nothing in `wt-299-green` (rerun). Python is 3.15.1 (`python --version`, rerun).
- Line numbers come from the seat's `Read` of `wt-299-green/facilitator.py`.

## 1. Design decisions

**D1. Acquire with `os.link`, confirmed over `O_CREAT|O_EXCL`.**
- Probe results (rerun, `python -I probe299.py`):
  - `os.link(tmp, lock)` works on this volume. `st_nlink` was 2.
  - A second `os.link` to an existing name raises `FileExistsError` (winerror 183, errno 17).
  - `os.open(O_CREAT|O_EXCL)` on an existing file also raises `FileExistsError` (errno 17).
  - Six processes released at one instant by a busy-wait on `time.time()` and calling `os.link` to the same name gave `['L','L','L','L','L','W']`. That is exactly one winner.
- Why link over `O_EXCL`: link makes the file appear whole or not at all. `O_EXCL` creates an empty file first, so a crash before the write leaves an empty file that strands the next writer.
- Fallback: none. Any `OSError` from the link except `FileExistsError` fails open (D6). A silent `O_EXCL` fallback was rejected because it brings the empty-file strand back. Risk 1 covers filesystems without hard links.
- Temporary file: `mob-writer.lock.<token>.tmp` in the same directory. Create it with `os.open(O_CREAT|O_EXCL|O_WRONLY)`, write with `os.fdopen`, and unlink it in a `finally` after the link attempt.

**D2. Holder JSON.**
- Exact text: `json.dumps({"pid": os.getpid(), "started": datetime.now(timezone.utc).isoformat(), "token": os.urandom(8).hex(), "writer": writer, "operation": operation})`.
- `started` and `token` match `hold_vote_lock` (`decider.py` ~313-345).

**D3. Empty, half-written or unparsable file means refuse, never take over.**
- Unreadable counts as: empty, invalid JSON, not a dict, or `pid` not an `int`.
- The refusal is `WriterRefused` with `holder=None`. The file is not touched.
- Through the link path our own writes are never half-written. This case therefore means a foreign or old file, and the message tells the product owner to remove it by hand.
- A `FileNotFoundError` on read means the file vanished. That is not a refusal; retry the loop.

**D4. Stale takeover.** Loop at most 3 times: `WRITER_LOCK_ATTEMPTS = 3`.
1. Try `os.link(tmp, lock)`. Success ends the loop. `FileExistsError` goes to step 2. Any other `OSError` fails open.
2. Read the file. Vanished means `continue`. Unreadable means refuse (D3). A live `pid_alive(pid)` means refuse.
3. Dead pid, so stale. Call `os.replace(lock, lock.with_name("mob-writer.lock.stale-" + mytoken))`.
   - `FileNotFoundError` or `PermissionError` means someone else moved it, or a reader holds it: `continue`.
   - `PermissionError` (errno 13) was probed for `os.replace` and `os.unlink` while a Python reader had the file open (rerun).
4. Read the renamed file. If the read fails, the seat lost: `continue`.
5. If its token is not the stale token read in step 2, a live lock was moved. Restore with `os.link(stale, lock)` (ignore `FileExistsError`), unlink the stale name, and `continue`. The next pass refuses.
6. Token matches: `os.unlink(stale)`. Only the unlink that succeeds wins. `FileNotFoundError` or `OSError` means lost: `continue`. A win goes back to step 1 and links the seat's own file.
7. After 3 attempts, re-read once and refuse with that holder.

**Probe-driven change to the proposal.** Rename alone is not exclusive on this Windows volume.
- Six processes renaming one stale file at the same instant all returned success, 6 of 6, with `os.replace` and with `os.rename` (rerun, `probe299b.py`). Only one file survived, because the rename follows the file object through its open handle.
- So "rename and verify the token" is not the arbiter. The arbiter is the single successful `os.unlink` of the seat's own stale name, plus a failed read of its own name for the losers.
- With steps 4 and 6, 25 rounds of 6 racers gave exactly 1 `WIN` per round and 125 `lost-read` (rerun, `probe299c.py`, output `{'WIN': 25, 'lost-read': 125} rounds with WIN!=1: 0`).

**Late release by a replaced holder.** `release_writer_lock` reads the file and unlinks only if `token == mine`. A foreign token, a missing file or an unreadable file leaves everything alone and prints nothing. Only an `OSError` on the unlink prints a warning (D6).

**D5. `Writers` changes.**
- `__init__` gains `self.tokens = {}` next to `holders`. The `holders` tuple shape is unchanged.
- `_claim` order:
  1. `root = checkout_root(...)`.
  2. Vote check, unchanged (`:1198`). A vote refuses before any lock file is touched.
  3. In-process dict, unchanged (`:1201-1203`). A different name raises `WriterRefused`. The same name returns `(root, False)` without touching the file.
  4. Only when `held is None`: `token = acquire_writer_lock(root, writer, operation)`. This raises `WriterRefused` when the holder token is not mine, or returns `None` on fail-open.
  5. Then `self.holders[key] = (writer, operation)` and `self.tokens[key] = token`.
- `release`: delete the dict entry only when the writer name matches, exactly as at `:1213`. Then `token = self.tokens.pop(key, None)`. If it is not `None`, call `release_writer_lock(root, token)`. That call never raises.
- Refusal rule: "the file's token is not mine". A second `Writers()` in one process is refused, and so is a second process with the same name.
- New helpers, placed after `checkout_root` (`:1186`):
  - `WRITER_LOCK = "mob-writer.lock"`
  - `writer_lock_path(checkout)`, copied from `decider_lock_path`'s 8 lines. `decider_*` and `pid_alive` stay untouched.
  - `_read_writer_lock(path)`
  - `acquire_writer_lock(root, writer, operation)`
  - `release_writer_lock(root, token)`

**D6. `WriterRefused`.**
- The constructor becomes `__init__(self, checkout, owner, writer, operation, holder=None, lock_path=None)`. Existing four-argument callers keep working, and `.owner` stays a string.
- Cross-process refusals set `owner = holder["writer"]`, or `"unknown"` when unreadable. They also set `.holder` and `.lock_path`.
- Message tail, appended to the existing text: `; lock file {lock_path}: pid {pid}, writer {w}, operation {op}, since {started}. If that process is not running, remove the file by hand.`
- Unreadable variant: `; lock file {lock_path} is empty or unreadable. If no writer is running, remove it by hand.`
- Fail-open warning, one stderr line:
  `warning: could not take the writer lock {path}: {ExcName}: {exc}; {writer} {operation} in {root} runs without cross-process protection`
- Triggers for that warning:
  - Any `OSError` from creating or writing the temp file.
  - Any `OSError` from `os.link` except `FileExistsError`. This includes `FileNotFoundError` when the git directory is missing.
  - Release prints `warning: could not remove the writer lock {path}: {ExcName}: {exc}` on any `OSError` other than `FileNotFoundError` from its unlink.
- Not triggers: `FileExistsError` (a refusal), reader or rename `FileNotFoundError` or `PermissionError` (a retry), `WriterRefused`.
- Chmod does not make a Windows directory read-only for file creation (probe: creating a file in a `S_IREAD` directory succeeded, rerun). So the read-only case must be injected by patching `fac.os.link`.

**D7. Lock lifetime is a dict entry (claim to release), not the `Writers` lifetime.**
- `commit` and `push` do a fresh `_claim` with `fresh = held is None`. They release in `finally` only when fresh (`:1224`, `:1235`). A seat's `claim` held across its turn (`seat_claim`, `:1269`) makes the inner `commit` non-fresh, so it never touches the file.
- Commit then push by one `Writers` is two claims: take, release, take, release. This matches today's in-process semantics. Holding it for the `Writers` lifetime would strand the lock on any caller that never releases (the tests at `:166`, `:181`). It would also block the facilitator's own later push through a second `Writers`.
- Only `tests/` create a `Writers()` besides `CardRun` (`:1262`) (`Grep 'Writers\('`, rerun). No second production `Writers` exists on one repo.

## 2. Three cycles
**File and class:** a new class `CrossProcessWriters(Fixture)` appended in `tests/test_writer_isolation.py` before `if __name__` (line ~761, `grep -n "^class "`, rerun). It reuses `Fixture.setUp` (temp bare origin, temp checkout, `ISOLATED` env, `base_sha`), `self.writers()`, `self.write`, `git`, `GitWrites`, `WRITE_VERBS`. The file is `i/lf w/lf` (`git ls-files --eol`, rerun). No `Fixture` change.

**Helpers (module level, in the class's new section):**
- `lock_file(repo)` returns `Path(git(repo, "rev-parse", "--git-path", "mob-writer.lock"))` joined to `repo` when relative.
- `class Child` runs `[sys.executable, "-c", CHILD, repo, writer, op, mode]` with piped stdin and stdout.
  - `CHILD` inserts the repo root into `sys.path`, imports `facilitator`, calls `w.claim(...)`, and prints one JSON line `{"pid":..., "token": w.tokens[key]}`. It then blocks on `sys.stdin.readline()`; the line `release` makes it call `w.release(...)` and exit. `mode` is `hold` or `race`.
  - `ready()` starts a thread that reads one stdout line and sets a `threading.Event`, then `assertTrue(event.wait(30))`.
  - `release()` writes `release\n` to stdin and calls `wait(30)`.
  - `crash()` calls `terminate()` (Windows `TerminateProcess`, no `finally` runs) and `wait(30)`.
  - `addCleanup` terminates survivors and closes the pipes.
- All waits use `Event.wait(timeout)` or `Popen.wait(timeout)`. There is no `sleep`.
- Assertions use the literal strings `"mob-writer.lock"`, `"token"` and so on, never `fac.WRITER_LOCK`, so no test is red on today's code for an `AttributeError`.
- `GitWrites` patches `subprocess.Popen` and counts only git `WRITE_VERBS`. The child is not a git command, so it is not counted.

### Cycle 1 (Done check 1): a second process is refused before any git write
| Test | Technique and assertions | Today |
|---|---|---|
| `test_a_lone_commit_holds_the_file_during_the_git_write_and_leaves_none` [Z] | Wrap `fac._git`: at the `commit` call, read the lock JSON into a list (or `None` if absent). After: `assertIsNotNone(during[0])`, `writer == "facilitator"`, `operation == "commit"`, `pid == os.getpid()`. The commit lands, and `assertFalse(lock.exists())`. | RED at `assertIsNotNone(during[0])`: no file exists today |
| `test_a_second_process_is_refused_before_any_git_write_naming_the_holder` [O] | A child holds as `seat-a`/`seat turn`. The parent's fresh `Writers().commit(repo, SEAT, ...)` runs inside `GitWrites()`. Assert `WriterRefused`, `spy.writes == []`, HEAD equals `base_sha`. Text contains `str(lock)`, the child's pid, `seat-a`, `seat turn` and the checkout. `.owner == "seat-a"`, `.holder["pid"] == child.pid`. | RED at `assertRaises`: "WriterRefused not raised", the commit lands |
| `test_the_same_writer_name_in_another_process_is_still_refused` [B] | The child holds as `facilitator`. The parent's `claim(repo, "facilitator", "push")` is refused. | RED, same assertion |
| `test_three_claimants_take_turns` [M] | The child holds. `w1` and `w2` (two parent `Writers`) are refused. `child.release()`. `w1.claim` succeeds and `w2.claim` is refused with `holder["pid"] == os.getpid()`. | RED at the first `assertRaises` |
| `test_a_second_writers_in_one_process_is_refused` [I] | `w1.claim(FACILITATOR, "commit")`. `Writers().claim(SEAT, "commit")` raises with `holder["token"] == w1.tokens[key]`. | RED, not raised |
| `test_a_live_vote_is_still_reported_as_decider_and_makes_no_lock_file` [I] | Write `mob-decider.lock.d/<tok>.json` with the own pid. Claim raises with `owner == "decider"`, and `assertFalse(lock.exists())`. | green today (guard) |
| `test_a_failing_git_operation_releases_for_the_next_process` [E] | Patched `fac._git` records `lock.exists()` at `add`, then raises `GitWriteFailed`. Assert `GitWriteFailed` propagates, the recorded value is `True`, and the lock is gone afterwards. A child then claims (`ready()` succeeds). | RED at `assertTrue(during)` |
| `test_after_release_the_second_writer_commits_in_order` [S] | A child commits `a.txt` via `w.commit` and exits. The parent commits `b.txt`. `git log --format=%s` lists both in order. | green today (guard) |

**Right reason and no accidental pass:**
- The parent's `Writers` is fresh (empty dict) and no vote exists. The only thing that can refuse is the child's file.
- `ready()` is set only after the child's `claim` returned. The refusal assertions read the child's pid from the lock, so a child that never held cannot produce a pass.
- On today's code the child holds only in memory, so the parent commits and the first failing assertion is `assertRaises`.

### Cycle 2 (Done check 2): lone writer, stale lock, release
Expected state after the cycle 1 green: the "O" and "E" tests may be red or green depending on how much the green wrote. The QA seat records each as red or green against the cycle 1 green.

| Test | Technique and assertions |
|---|---|
| `test_a_lone_writer_is_never_refused_and_leaves_nothing` [Z] | 5 fresh `Writers`, each claim then release. No refusal. `os.listdir(lock.parent)` has no name starting with `mob-writer.lock`. |
| `test_a_lock_whose_pid_is_gone_is_taken_over` [O] | Dead pid from a reaped `python -c pass` child (`fac.pid_alive` is False, rerun). Write the holder with a known token. `claim` succeeds. The file now holds `pid == os.getpid()` and a new token. No `.stale-`/`.tmp` leftovers. If cycle 1 refuses any existing file, this is a real red: `WriterRefused raised`. |
| `test_many_sequential_writers_each_claim_and_release` [M] | 3 `Writers` x 3 claims, interleaved release. No refusal, no lock left. |
| `test_a_late_release_removes_only_its_own_lock` [B] | `w1.claim`. Replace the file with a dead-pid holder via a temp file and `os.replace`. `w2.claim` takes over. `w1.release` leaves the file with `w2.tokens[key]`. `w2.release` removes it. |
| `test_reading_the_lock_for_a_refusal_changes_nothing` [I] | A child holds. Two refused claims. Lock bytes are equal before and after, and the directory listing is unchanged. |
| `test_an_empty_or_unreadable_lock_refuses_and_is_left_alone` [E] | `subTest` over `b""`, `b"{not json"`, `b"[]"`, `b'{"pid":"x"}'`. The claim raises with `holder is None` and the text contains the path and "unreadable". Bytes unchanged. |
| `test_cards_230_298_313_suites_pass_unchanged` [S] | No new test. The three suites in section 4 are the proof. |

### Cycle 3 (Done check 3): nothing is stranded
| Test | Technique and assertions |
|---|---|
| `test_a_killed_holder_leaves_a_lock_the_next_writer_takes_over` [Z] | A child holds. `child.crash()`. Assert the lock exists with the child's pid and token, and `pid_alive` is False. The parent's claim succeeds with a new token. |
| `test_two_processes_racing_for_one_stale_lock_exactly_one_holds` [O] | Stale file written. Two `race` children spin on a start file (`os.path.exists` loop, deadline 30 s, no sleep) and then `claim`. Each prints `HELD` or `REFUSED`. A winner blocks until it reads `release`. Assert exactly one `HELD`, the file token is the winner's, and the loser's text names the winner's pid. |
| `test_a_takeover_that_moved_a_live_lock_puts_it_back_and_refuses` [O] | Stale lock. Patch `fac.os.replace` with a wrapper that, on the first call, runs `w2.claim` fully (guarded against recursion) and then calls the real replace. `w1.claim` raises `WriterRefused`, `holder["token"] == w2.tokens[key]`, the file is still `w2`'s, and no `.stale-` leftover remains. |
| `test_holder_killed_taken_over_killed_again_taken_over` [M] | Child1 holds then crash; child2 claims (takes over) then crash; the parent claims. Tokens pairwise differ. |
| `test_the_lock_is_created_whole_by_a_link` [B] | Wrap `fac.os.link`: at call time the source parses to a dict with the 5 keys and the destination does not exist; the call count is 1. Pins D1. |
| `test_the_refusal_text_names_the_lock_path_for_a_reused_pid` [I] | Holder with the own (live) pid and a foreign token. The text contains `str(lock)`, the pid and "remove the file by hand". `redirect_stderr` is empty (a refusal is not a warning). |
| `test_a_lock_that_cannot_be_created_warns_once_and_git_runs` [E] | `subTest`: (a) `os.link` raises `PermissionError(13,"denied")`; (b) `os.open` of the `.tmp` raises `OSError(28,"no space")`. Under `redirect_stderr`: the commit lands (HEAD moved), stderr is one line with "warning", "mob-writer.lock" and the exception name, no lock file, no `.tmp` left. A bad pathspec still raises `GitWriteFailed`, not an OSError. |
| `test_one_claim_one_commit_one_push_is_unchanged` [S] | commit then push; `origin_main == sha`; no lock. |

### G2: which cycles may be born green, and the mutation pairs
Cycle 1 has real reds, so it needs no pair. Cycles 2 and 3 may be born green. Each patch is a plain `diff -u` with no comment line before `---`; the QA seat verifies it with `git apply --check` on the green. The edits below are against the shapes in D4 to D5.

| ID | Edit | Test that must fail |
|---|---|---|
| M2a | In `acquire_writer_lock`, `if pid_alive(holder.get("pid")):` becomes `if True:` | `test_a_lock_whose_pid_is_gone_is_taken_over` (`WriterRefused raised`) |
| M2b | In `release_writer_lock`, `if holder.get("token") == token:` becomes `if True:` | `test_a_late_release_removes_only_its_own_lock` (`assertTrue(lock.exists())` after `w1.release`) |
| M3a | In `commit`, `if fresh:` becomes `if fresh and sys.exc_info()[0] is None:` | `test_a_failing_git_operation_releases_for_the_next_process` (lock still there) |
| M3b | In the takeover, the token comparison `if renamed_token == stale_token:` becomes `if True:` | `test_a_takeover_that_moved_a_live_lock_puts_it_back_and_refuses` (no `WriterRefused`) |

## 3. Files
- **The green may touch:** `facilitator.py` only. Edit `WriterRefused` (`:1075`), add `WRITER_LOCK` and the helper functions after `checkout_root` (`:1186`), and edit `Writers.__init__`, `_claim` and `release` (`:1193-1214`). Keep the file's mixed line endings and rewrite no untouched line.
- **The green must not touch:**
  - `decider.py`
  - `decider_lock_path`, `decider_holders_dir`, `decider_holders`, `decider_lock_live`, `pid_alive` (`:1101-1181`)
  - `core/backends.json`
  - any existing assertion in `tests/`
  - `Writers.commit` and `Writers.push` bodies (the `try/finally` and `fresh` logic stay as they are)

## 4. Verification commands (G1)
All run from `C:/Users/ringe/mob-crosscli`, with `PYTHONDONTWRITEBYTECODE=1`.

| Command | Expected | Label |
|---|---|---|
| `python -m unittest tests.test_writer_isolation.CrossProcessWriters` | cycle 1: [Z], [O], [B], [M], [I] and [E] tests red, [S] and vote-first green | does not exist yet |
| `python -m unittest tests.test_writer_isolation` | 31 tests before the card (`grep -c "    def test_"`, rerun); run once, 60 s, result **unconfirmed** | record-only |
| `python -m unittest tests.test_decider_runner` | 43 tests (grep count, rerun); once, 76 s, result **unconfirmed** | record-only |
| `python -m unittest tests.test_writer_staging` | 7 tests (grep count, rerun); once, 20 s, tail showed `OK` | record-only |
| `git diff --name-only` | lists `facilitator.py` and the test file only, never `decider.py` | not yet run |
| `git ls-files --eol facilitator.py tests/test_writer_isolation.py` | `i/mixed w/mixed`, `i/lf w/lf` before; unchanged after | rerun |

## 5. G3: life-cycle steps and gaps
`rerun` means the seat ran a small `python -c`/script probe. `blocked` means the code does not exist yet, so the green or QA runs the named test.

| Step or gap | Fails / crash falls here / another process acts here | Label and command |
|---|---|---|
| Path: `git rev-parse --git-path mob-writer.lock` | Outside git, `checkout_root` raises (`:1186`), unchanged. Main prints `.git/mob-writer.lock` (relative to the root); the worktree prints an absolute per-worktree path (section 6). | rerun, both commands in section 6 |
| Write temp file | `OSError` fails open with a warning. Crash here leaves `.tmp`, no lock. | blocked, `test_a_lock_that_cannot_be_created_warns_once_and_git_runs` (b) |
| Link into place | `FileExistsError` means contend. Other `OSError` fails open. Another process acts: one link wins. | rerun, `python -I probe299.py` (race `['L'x5,'W']`) |
| Gap link to first read by others | Never half-written (link). Reader sees whole JSON or `FileNotFoundError` (retry). | blocked, `test_the_lock_is_created_whole_by_a_link` |
| Run the git operation | Exception goes through `finally` to release. Crash here leaves a stale lock; the next claim takes over by `pid_alive`. | blocked, `test_a_failing_git_operation_releases_for_the_next_process`, `test_a_killed_holder_leaves_...` |
| Read holder | Unreadable means refuse. Vanished means retry. A reader holding the file open makes another's replace or unlink raise `PermissionError`. | rerun, `probe299.py` lines `unlink while open: PermissionError 13`, `replace while open: PermissionError 13` |
| Stale rename | Other taker acts: on Windows all racers' renames can succeed (handle-based). | rerun, `python -I probe299b.py`: `['W' x6]`, one file left |
| Verify token of renamed file | A mismatch means a live lock was moved; restore and refuse. Another process acts here. | blocked, `test_a_takeover_that_moved_a_live_lock_puts_it_back_and_refuses` |
| Unlink my stale name | Only one unlink succeeds, so one winner. | rerun, `python -I probe299c.py`: `{'WIN': 25, 'lost-read': 125}` |
| Gap stale rename to restore link | A third writer can claim fresh while the live holder's file is gone. The lock is lost for one holder (risk 2). | record-only |
| Release | The token is checked; a foreign or missing file is left alone. An unlink `OSError` warns. A crash before this falls back to takeover. | blocked, `test_a_late_release_removes_only_its_own_lock` |
| Gap read to unlink in release | A takeover here needs my pid dead, so it cannot hit a live holder. | record-only, argument |
| Pid reuse | A dead holder's reused pid reads live, so the claim is refused with the path in the text. Out of card. | blocked, `test_the_refusal_text_names_the_lock_path_for_a_reused_pid` |

## 6. Worktree
- `git rev-parse --git-path mob-writer.lock` in `C:/Users/ringe/wt-299-green` prints `C:/Users/ringe/mob-crosscli/.git/worktrees/wt-299-green/mob-writer.lock` (rerun).
- The same command in `C:/Users/ringe/mob-crosscli` prints `.git/mob-writer.lock` (rerun).
- `git rev-parse --git-dir --git-common-dir` in the worktree prints `.../.git/worktrees/wt-299-green` and `.../.git` (rerun).
- The lock is per worktree, which matches `checkout_root` keys: linked worktrees are distinct keys, per the test file docstring (`:12-14`).
- `writer_lock_path` must join a relative result to the root. Do it exactly as `decider_lock_path` does at `:1140`.
- The lock in a worktree does not guard refs shared with the main checkout. That is "not in this card".
- `Writers` and the tests use their own temp repositories: `Fixture.setUp` makes `tempfile.TemporaryDirectory(prefix="wi-")` with `git init` (`:101-121`). Nothing depends on the checkout path or on `--git-path` of the real repository.
- `wt-299-green` is removed with `git worktree remove` after the merge (proposal, last bullets). The lock directory under `.git/worktrees/` goes with it.

## 7. Risks and the open question
1. **Hard links are required.** A git directory on a filesystem without them (some network or FAT volumes) raises `OSError` on every link and the claim fails open with a warning: the card 313 stance. D1 deliberately has no fallback, because `O_EXCL` brings back the empty-file strand.
2. **Restore gap in D4 step 5.** Between moving a live lock and linking it back, a third writer can claim fresh, leaving two live holders for that gap. It needs three processes and one stale lock to line up. Say so on the card instead of promising "exactly one".
3. **`PermissionError` on rename or unlink while a reader has the file open.** A takeover or release can lose its race and refuse, or print a release warning that leaves the file in place while its pid is still alive (stranded until the process ends). Mitigation: `acquire` retries (3) and the refusal text names the file. A sleeping retry in `release` was rejected; this is risk, not fixed.
4. **False refusal for a caller that never releases.** `Writers.claim` without `release` now blocks any later `Writers` in the same process, because the lock stays live until exit. No production caller does this (`seat_claim` pairs with its release callback). Several tests do (`:166`, `:181`, `:209`); each uses its own temp repo, so they pass.
5. **Existing tests that hold a lock without release leave a live file in their temp `.git`.** `TemporaryDirectory` (`ignore_cleanup_errors=True`) removes it.
6. **The all-wins finding.** The rename-and-verify design in the proposal's G3 row ("Stale takeover") is unsafe alone on this volume (probe, section 1 D4). The green must implement steps 4 and 6 as written.

**Open question for the Decider:** should an empty or unreadable lock file ever be taken over automatically (for example after a bounded time), or stay a manual removal? D3 picks manual removal because the card says so. That means one corrupted file blocks every writer until someone deletes it, while a timed takeover would reintroduce a clock into a card that has none.

## Facilitator's note (not the seat's text)
- Disposition of the open question: manual removal stays (D3). The refusal text names the file and says to remove it by hand; a timed takeover is not added.
- The scratchpad probe scripts the seat names (probe299.py, probe299b.py, probe299c.py) are not committed; their results are quoted above.
