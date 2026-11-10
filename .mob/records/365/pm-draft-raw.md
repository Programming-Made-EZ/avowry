# Ready draft: card 365 (seat-use history race) and card 374 (stray prompt files)

The draft recommends two cards, built back to back. Card 365 adds a lock around the seat-use append and changes the end-of-run check to a prefix check. Card 374 runs the start probe in the run's scratch directory instead of the repository root. Both live in the mob-defaults repository. The avowry repository sees them through recorded runs, not a wrapper test.

## 0. Standing-rules preflight

- **Card format rule:** each card has a Connextra story, at most 3 Gherkin scenarios each titled with its ZOMBIES letters, and one observable behaviour. Every letter is covered across each card's scenarios.
- **TDD (D137, D204):**
  - Characterisation tests are declared first and must pass on the pre-change tree.
  - Each new criterion test fails by assertion on the pre-change tree. The gate reviewer replays it.
  - The green changes the red files only where the red is unpended.
  - mob-defaults has no `Status=Pending` trait. The architect names how the red is held: an unmerged red branch or `expectedFailure`, with the gate replay as the check.
- **D222:** each construct gets its own id (365-1a, 365-1b and so on).
- **D206:** guard tests read committed state and select by exact key.
- **D228 preflight still applies to these cards' own Decider windows,** run one seat at a time while either bug is open.
- **D230:** idle lines are recorded. **D232:** confirmation step for any No.
- **D234:** the evidence block is quoted in section 7. **D236:** a live probe under 60 minutes old is committed before the seat plan.
- **D240:** no seat brief carries the account name. `core/backends.json` invocation lines hold a home path, so they are never pasted into a prompt.
- **Decider protocol step 6:** these cards change no agreement text, so trigger (d) applies. The later removal of the one-seat-at-a-time line is an agreement change, so trigger (c) applies there.

## 1. Premise facts (mob-defaults unless marked)

**Card 365: the append-only check rejects any concurrent append.**

- **How a run is set up today.** The parallel launcher (`run_votes_par.py`, a scratchpad file) starts one `decider.py` process per seat. In the avowry history, each line reads `"card": "decider"` with `context: decider-<seat>` (`.mob/records/seat-use.jsonl`, last lines).
- **The check compares the whole file.** `facilitator.py:1483-1485` (`read_seat_use`) refuses unless the file lines equal `seat_use_seen`. The first read records the file (`facilitator.py:1486`), and an append adds only this run's own line (`facilitator.py:1504`). One extra line from another run fails the check, though nothing was lost.
- **Failure point 1: at start (exit 2, "No voter was started").** `start_voters` (`decider.py:246-257`) runs the check, then the append (`decider.py:240-243`), then reads the history again (`decider.py:254`). Another run that appends between this run's read and its append gets this run refused. This run's line has already been written, so the history keeps a line for a seat that never started.
- **Failure point 2: at the end (exit 5).** `verify_history` (`decider.py:260-262`) runs after the vote, and its refusal is added to the integrity changes (`decider.py:436-439`). The run then prints `INTEGRITY: the real working tree or HEAD changed while the vote ran` and exits 5 (`decider.py:461-467`). In a window of six, every run except the last to append fails here. So bug 365 alone also produces the INTEGRITY message.
- **The append is not atomic across processes.** `append_seat_use` (`facilitator.py:1495-1499`) checks whether the file ends with a newline and then appends to it, with no lock.
- **`line_index` drifts.** It is computed as `first = len(history at check)` (`decider.py:255`), so it is wrong if another run's line lands between the check and the append. The rotation reason is built over `written[:first+n]` (`decider.py:256`), so it shifts too.
- **The rotation rule** reads the full history plus the pending lines for a Claude voter (`facilitator.py:1523-1528`).

**Card 374: the start probe writes prompt files into the repository root.**

- **Where the files come from.** `open_run` calls `probe_drivers` (`decider.py:213`), which probes every enabled backend with `cwd=self.repo` (`facilitator.py:1680`, then `probe_backend` at `facilitator.py:830`, and the command probe at `facilitator.py:808`).
- **Why they land in the root.** `_write_prompt_file` writes `prompt-*.txt` into the given `cwd` (`facilitator.py:393-402`). Grok, Muse, Cursor and Qwen use `{PROMPT_FILE}` (`core/backends.json:52,75,85,96-97`).
- **Cleanup only covers a normal exit.** The file is deleted in `finally` (`facilitator.py:380-382`), so a killed process leaves it behind.
- **How it trips other runs.** `snapshot` uses `--untracked-files=all` and excludes only `.mob/` and `MOB.md` (`decider.py:101-106`). A file another run creates during its probe shows up as `?? prompt-*.txt` in this run's before-or-after diff, which means exit 5.
- **Voter prompt files are fine.** They go into each voter's own scratch clone (`decider.py:171`, `cwd=clone`).
- **Not covered by tests.** `RunnerCase` fakes use `{PROMPT}`, not `{PROMPT_FILE}` (`tests/test_decider_runner.py:83-84`), so no runner test writes a prompt file.

**The vote lock does not cause the race and does not prevent it.**

- `hold_vote_lock` (`decider.py:316-385`) is a registry with one holder file per run, not mutual exclusion. Its only consumer refuses commits and pushes by others (`facilitator.py:1182-1185, 1383`).
- It is taken after `start_voters` (`decider.py:419`) and released before `verify_history` (`decider.py:427, 436`). So it does not cover either point where the race fails.
- It must not be reused as the append lock, and it is out of scope (cards 313 and 315).

**Records (avowry):**

- `.mob/records/186/retro-input.md:26`: the Qwen coach seat was refused with "not append-only" when five roles launched at once.
- `.mob/records/203/retro-input.md:7,10`: the three windows failed on both causes. A 35-second stagger did not help because each run lasts minutes.

**Wording in the agreement.** The line the owner refers to sits inside the D174 "Parallel voter dispatch" entry as tuned by D228 (`TEAM_WORKING_AGREEMENT.md:495`): "While bug 365 … or bug 374 … is open, the window runs one seat at a time and the record says so."

## 2. Decision

**Card 365. Recommendation: a lock around the append, plus a prefix rule.**

A lock alone does not fix the bug, because the equality check still refuses another run's appended line. Both of these are needed:

1. **Prefix rule.** "Append-only" means every line this run has seen is still a prefix of the file. After each locked append, `seat_use_seen` becomes the whole file, including other runs' lines. A removed, changed or emptied line is still refused. An appended line from another run is accepted.
2. **Short lock.** The lock covers read, judge (including the Claude rotation rule), append and re-read. It lasts milliseconds and is never held across a probe or a vote. A stale re-probe in card mode happens before the lock is taken.
   - `line_index` becomes the actual position of the run's own line.
   - The rotation reason is computed over the lines before that position.
   - The orphan line from the current refuse-after-write path disappears.

Lock options:

| Option | Tamper-evidence | Windows | What a crashed seat leaves |
|---|---|---|---|
| Sidecar lock in the git directory (`git rev-parse --git-path mob-seat-use.lock`), created with `O_CREAT\|O_EXCL`, holding pid, start time and token, swept when the pid is dead, with a bounded wait (about 30 s) and then a named refusal | Prefix rule unchanged; the committed history stays the record | Works on both platforms; same idiom as `hold_vote_lock` and `pid_alive` | A lock file that is swept when its pid is dead (Windows pid reuse is a small residual risk) |
| `msvcrt.locking` on `seat-use.jsonl` itself | Same | Byte-range locks are mandatory: readers get `PermissionError`, which becomes "cannot be read". Linux CI needs a separate `fcntl` branch | Nothing, the OS releases it |
| Per-seat files merged at window end | Weaker: the merge rewrites the file, so it needs its own check | No lock needed | Part files that must be merged, or the start evidence is lost |
| Launcher writes all lines in one window commit (D174's literal wording) | Strong | No lock needed | Nothing, but the decider's own gate is bypassed or duplicated, and the launcher is not in the repository |

- The per-seat files option leaves the rotation rule blind to seats launched in the same window. Every reader would also need to read the union of files (`read_seat_use`, `mob_process.py:999`, the closure checks).
- **Recommendation: the sidecar lock in the git directory.**

**Card 374. Recommendation:** `open_run` runs the probe with `cwd=run_dir` (the scratch directory) through a `probe_fn` bound to `run_dir`. No `decider.py` run then writes into the real tree.

- **Rejected: ignore `prompt-*.txt` in `snapshot`.** That would let a voter that escaped its sandbox go unnoticed.
- **Rejected: always write prompt files to `SCRATCH`.** It breaks the guard that a prompt file sits in the seat's own working directory (`tests/test_cursor_qwen_seats.py:111`) and may break command-line tools that only read inside their workspace.
- A crashed probe now leaves its file in scratch, not in the root.

**Two cards, one after the other. Card 365 goes first.** Each card has one observable behaviour, per the Card format rule:

- Card 365: concurrent appends never fail the append-only check.
- Card 374: a run's own prompt file is never a change to the real tree.

Card 374's criterion 1 can only fail for the right reason once card 365 is green, because before that, exit 5 also comes from `verify_history`.

**Card text corrections for card 374:**

- `Repository` should name mob-defaults, not avowry.
- `Done when` should be `python -m unittest discover -s tests` in mob-defaults, not the avowry `dotnet test` command.

## 3. Scenarios

**Card 365.** As the sponsor persona, I want every seat of a decision window to start when they are launched at once, so that a vote window takes minutes, not half an hour.

1. **[Z][O][M][B] 365-1a, 365-1b: six seats launched together all start and all six lines are kept.**
   - Given an empty history (no file).
   - When run B appends between run A's read and run A's append (1a, forced by a barrier in the test), or when six real `decider.py` processes are released at one barrier (1b).
   - Then:
     - every run exits 0 with a counted vote;
     - the file holds exactly six terminated JSON lines, one per context;
     - no stderr contains "not append-only".
2. **[E] 365-2a to 365-2e: a line removed or changed by hand is still refused.**
   - Given a history this run has seen, with another run's line appended after it.
   - When a seen line is removed (2a), changed (2b) or the file is emptied (2c), or when a removal is combined with another run's append (2d).
   - Then the run refuses with "not append-only" and starts no voter, or (after the vote) exits non-zero.
   - Also 2e: a live lock held past the wait limit refuses, naming the lock; a dead holder's lock is swept.
3. **[I][S] 365-3a, 365-3b: the rotation rule gives the same answer.**
   - Given another run's line that lands just before a Claude voter's line.
   - Then:
     - the record's `line_index` is that voter's own line in the file;
     - `rotation_reason` equals `rotation_reason(probes, lines before it)` recomputed from the committed file (3a);
     - a lone run's record and its `RECORD_KEYS` are unchanged (3b).

**Card 374.** As the sponsor persona, I want concurrent decision runs to count every vote, so that a window is not lost to another run's temporary files.

1. **[M][B] 374-1: two runs at once both finish with counted votes.**
   - Given `{PROMPT_FILE}` backends using `tests/fakes/fake_reply.py`.
   - When run B's probe is held open while run A takes its snapshots.
   - Then both exit 0, `integrity.ok` is true and `changes` is empty.
2. **[E][I] 374-2a, 374-2b: a tracked file changed during a run is still refused.**
   - When `README.md` is changed during the vote (2a), or a voter writes `prompt-planted.txt` into the root (2b, which catches a whitelist shortcut).
   - Then the run exits 5, naming the file.
3. **[Z][O][S] 374-3: no prompt file remains in the repository root.**
   - Given a run with `{PROMPT_FILE}` backends, including a `probe_invocation` and a backend that can run commands.
   - Then:
     - every prompt file's parent is under the run's scratch directory;
     - the root never holds a `prompt-*.txt`, even after a probe that is killed;
     - a backend without `{PROMPT_FILE}` writes no file.

## 4. Named tests (all in mob-defaults `tests/`, Python unittest)

**New criterion tests and how each fails before the change:**

- **365-1a** (`test_decider_rotation.py`, in-process with a barrier): run A's `start_voters` raises "not append-only". The test asserts exit 0 and six lines, so it fails by assertion.
- **365-1b** (`test_decider_runner.py`, six subprocesses): at least five runs exit 2 or 5.
- **365-2d:** refused before the change too, so it is declared characterisation (a guard that a partial "accept a longer file" fix would break).
- **365-2e:** fails because no lock exists.
- **365-3a:** fails because the run is refused, and because `line_index` is off by one.
- **Tightened `LiveVoteOverlap`:** assert exit 0 and `integrity.ok` for both runs (the comment at `test_decider_runner.py:590` notes the bug). Fails on "append-only".
- **374-1:** with card 365 green, fails on a `?? prompt-*.txt` entry. The assertion names that entry, so the failure is for the right reason.
- **374-3:** fails because the prompt file's parent is the repository root.

**Characterisation tests (declared; must pass before the change):**

- `test_decider_rotation.VerifyHistory`: the three refusals and "as left passes".
- `test_decider_gate.test_a_history_a_voter_rewrites_while_it_runs_is_caught_afterwards`.
- `test_seat_rotation` at lines 275 and 283.
- `test_decider_record.test_rotation_is_recomputed_from_the_log_entry_and_the_history_alone`.
- `test_decider_runner.test_a_voter_that_escapes_to_the_real_tree_is_detected`.
- `FreshProbe`.
- `LiveVote` guards.
- `test_cursor_qwen_seats` prompt-file location.
- 374-2a and 374-2b.

**Known failure on the base commit:** `tests.test_loop_state.test_lock_released_when_holder_exits`. It is named, not fixed.

**How the avowry repository records the evidence (precedent: card 298):**

- The closure record `.mob/records/365.json` (and `374.json`) names:
  - the full mob-defaults hashes of the characterisation, red and green commits;
  - the suite command with its counts;
  - the base-commit failures.
- `.mob/records/<card>/` holds:
  - `red-record.md` with the ids, the red on the pre-change commit, and the multi-construct list per D222;
  - `gate-review.md` (replay in a fresh mob-defaults clone, plus the mutation table);
  - `second-review.md` and `signoff.md`.
- There is no wrapper test in avowry: it would need a path to a sibling checkout, which means a home path, and Linux CI has no such checkout.
- The live proof is the first parallel window after card 374 ships, in `.mob/records/deciders/D2xx-votes.json`, together with its seat-use lines.

## 5. Size, and what is not in the cards

**Size:**

- Card 365 is small to medium: about 60 to 90 lines of code (a lock helper, `read_seat_use`, `check_seat` and `commit_seat` under the lock, `start_voters` `line_index`) and about 8 tests. One red and one green.
- Card 374 is small: about 5 lines and 4 tests.

**Not in the cards:**

- `run_votes_par.py` (a scratchpad, not in the repository) and removing its stagger.
- The vote lock (cards 313 and 315).
- Facilitator card-mode probes in the repository root. If they are seen during a window, that is a new bug.
- The reply-format pre-check and the D240 launcher check.
- The agreement edit. That is a separate Decider after both cards ship.
- The wrapper timeout in the pre-publication test.

## 6. Measurement plan (before and after)

**Metric:** minutes per vote window, measured from the commit of the committed proposal to the commit of `.mob/records/deciders/D2xx-votes.json` (`git log --format=%cI`). This is the D174 counter-metric.

**Secondary:** the launch span of the window's lines in `.mob/records/seat-use.jsonl` (the `ts` field).

- Serial windows show seats about 2 to 3 minutes apart: for example 03:11, 03:14, 03:17, 03:19 and 03:20 UTC on 2026-11-10.
- Parallel windows should show seconds between seats.

**Before:**

- D239: about 25 minutes (retro for card 376, section on flow).
- D240: about 40 minutes, including the confirmation step.
- These two cards' own declaration windows, run one seat at a time with the same method.

**After:** the next 3 windows once both cards ship (the D174 trial count), plus retro length (now 60 to 90 minutes).

**Historic parallel reference:** D225, about 6 minutes (card 186 retro voices).

**One gap:** the launcher passes no `--ref`, so the lines say `card: decider`. The facilitator should pass `--ref D2xx` from now on, so windows can be told apart without matching timestamps.

## 7. Task force declaration

| Step | Card 365 | Card 374 |
|---|---|---|
| Ready | Cursor, reply-only (this draft) | Same draft |
| Architect slice | Fresh Claude context, read-only | Fresh Claude context |
| Characterisation and red | Cursor writing seat, worktree `wt-365-red` | Qwen writing seat (Probation, first small code card per the card 376 retro); fallback Cursor; worktree `wt-374-red` |
| Green | Muse writing seat, worktree `wt-365-green` | Muse or Cursor, never the red's seat |
| Second read-only review | Grok, before the gate review | Grok |
| Independent gate review | Fresh Claude context; step 0 ancestor line; replays the red on the pre-change mob-defaults commit | Same |
| Sign-off | A further fresh Claude context | Same |
| Declaration Decider voters | Least-recently-used order from the committed live probe (Qwen, agy, Grok, Muse, Cursor, Claude); one seat at a time while the D228 line stands | Same |

- Card 365 swaps the card 376 roles: Muse had the red there and Cursor both greens.
- Codex is out until 2026-11-11, and its card 162 check run comes first.
- Worktrees are in the mob-defaults repository. No seat brief carries a home path (D240).

**Standing evidence block (D234), quoted from `.mob/records/templates/declaration-evidence.md`:**

> 1. Review records. For every card the cold-reader report (a docs card), the second read-only review (when run), the independent gate review (opening with the step 0 line: the `git merge-base --is-ancestor` command, the tested commit and its exit status, and listing the files and criteria it checked) and the sign-off are committed under `.mob/records/<card>/`, and the closure record `.mob/records/<card>.json` names the full commit hashes of the ship and signoff seats (the closure check refuses a card without them).
> 2. Separate sessions. The closure record lists each seat with its own context label (Claude ctx-N, all distinct; the green never the red's), each seat's work is its own commit, and the red and the green work in their own worktree branches. The decider seats' launches are lines of `.mob/records/seat-use.jsonl` (seat, role, context, time). No claim of model isolation is made: separate sessions, worktrees and distinct commits are the evidence.
> 3. Red-first order. The characterisation and red commits precede the green in git; the gate review replays the red on the pre-change tree and the green changes the red files by the Pending traits only.
> 4. Idle minutes and Dispatch nudges are two committed lines in each retro (D230).
> 5. Per card, not only in general. Each card's retro document carries a section `Evidence (per card)` … the declaration of the next card quotes the previous card's section as the example of the evidence it will produce.

These cards also list the mob-defaults commit hashes next to the avowry record commits.

**Previous card's example, quoted from `docs/retros/card-376-2026-11-10.md`, section 9:**

> - Ready draft and rulings: `c2a3f56a837491d11ce72f21de669e6b05db0e1f` (a pointer to the draft, rulings and declaration proposal first committed in `d619ebbe6998751d74a40ada2a3ef88741a119d8`)
> - Architect slice: `cf5be43f28dfee04ca59e7c85772f17ef3b8ee58`
> - Characterisation and red (Muse writing seat): `e915aa99bf5553601943abbfddc183135c2624d3`
> - Green 1, the code (Cursor writing seat): `586c2cdd9f12e37b2d6892770376f8ebadae33b6`
> - Green 2, the six records by the rule (Cursor writing seat): `69d764ef002bcc32914346b32f8f741fb2885d9c`
> - Bug 397 fix (facilitator): `935305898914d5570e806ff5a4a4cfce2dfe7017`
> - Independent gate review (ship) and second read-only review: `4522f428f86ef16d5933dfafafe1817750e78a7b`
> - Sign-off: `2cc15a814e3cc67db573957c82920ae768cfe509`

**After both cards ship:**

- A Decider removes the one-seat-at-a-time sentence from the D174 entry as tuned by D228 (trigger (c)).
- The facilitator reports to the owner:
  - both card numbers;
  - the two causes: whole-file equality instead of a prefix check, and probe prompt files in the repository root;
  - minutes per window before and after, from section 6.
