# New proposal for card 313 after D194 failed (green seat in its own worktree):  a decision run that starts as another ends is never left without a vote lock

This is a new proposal made after Decider D194 failed (the product owner's ruling of 2026-11-07: record the proposal as failed, and make the worktree a new proposal voted as a fresh Decider with fresh seats, using only its round 1 conditions). It is the product manager seat's original note below with two changes: the card text's cycle 1 scenarios [O] and [B] now also assert, red first, that a facilitator commit and push during B's vote are each refused with zero git writes, and the green seat works in its own git worktree (the section at the end). Product manager seat (ready), fresh Claude context; read-only reply committed unchanged by the facilitator except that the persona name before a colon is written "the product owner" (the repository's personal-name check). The seat ran no test suite, wrote no file and did not call `gh`. Every line number below was produced by the command named beside it, run in `C:/Users/<user>/mob-crosscli`.

## Code facts (rerun)
- `git rev-parse --short HEAD` gives `1590679`.
- `git grep -n "def hold_vote_lock\|def release\|hold_vote_lock(" -- decider.py` gives `decider.py:313` (def), `:362` (`release`) and `:405` (the call in `run`).
- `git grep -n "directory.mkdir\|os.open(temporary\|os.replace(temporary\|directory.rmdir\|path.unlink()\|except OSError:$" -- decider.py`:
  - mkdir is at `:325`, the temporary open at `:355`, the replace at `:358`.
  - The outer `except OSError` that returns a no-op is at `:359`, the release unlink at `:364`, the release rmdir at `:368`.
- `git grep -n "decider_lock_live" -- facilitator.py` gives `:1169` (def), `:1198` (`Writers._claim`) and `:1230` (push).
- `sed -n 1143,1181p facilitator.py` shows `decider_holders_dir` at `:1143`, `decider_holders` at `:1149` and `decider_lock_live` at `:1169`. The last one only reads.
- `git grep -n "^class " -- tests/test_decider_runner.py` shows `LiveVote` at `:297` and `LiveVoteOverlap` at `:490`. The `def test_` lines in `LiveVoteOverlap` number 7, which agrees with the sign-off's "Ran 7".
- The test file sets `WAIT = 60` at `tests/test_decider_runner.py:289`, found by `git grep -n "WAIT =" tests/test_decider_runner.py`.
- `git grep -l "hold_vote_lock\|mob-decider\|decider_lock" -- tests` finds only `tests/test_decider_runner.py`, plus the data fixture `tests/fixtures/muse_real/e338c0ee.jsonl`, which is not a test.
- `git grep -n -i decider -- tests/test_writer_isolation.py` returns nothing.
- Unrun: the duration of any suite. The 19.9 s, 31-test and 70 s figures are record-only, from the 298 sign-off and slice records.

## Card text (replaces the issue body)

```
<!-- phrase-card:313 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** mob-defaults (mob-crosscli: decider.py; facilitator.py only read)
- **Source:** card 298's sign-off (.mob/records/298/signoff-ctx6.md), probe rerun against mob-crosscli HEAD 1590679: hold_vote_lock at decider.py:313, mkdir :325, os.open of the temporary file :355, os.replace :358, the OSError no-op :359, release unlink :364 and rmdir :368 (git grep -n, rerun).
- **Depends on:** card 298 (the holder format)
- **Consumer value:** the product owner: so that decisions cannot be disturbed by a commit mid-vote: the checkout is refused for the whole of every run's vote.
- **Actor (supporting):** the vote lock's registration step in hold_vote_lock, run by two overlapping decision runs
- **Given / Observes / Fails with (supporting):** run B calls hold_vote_lock just as run A releases and removes the empty holders directory; B always ends up with a holder file while it votes. Today B's temporary-file open fails after A's rmdir, the OSError branch returns a no-op, B holds nothing and decider_lock_live is false while B votes.
- **Invariants:** card 230 and card 298 tests pass; decider_lock_live stays read-only; no live run's holder is ever removed; a registration that gives up says so on stderr.
- **Not in this card:** pid reuse; a crashed run's .tmp file (card 314); a run that cannot create the directory starting unprotected without a warning (card 315); cross-process writer exclusion (card 299).

## Story
As Steve, I want the checkout refused for the whole of every decision run's vote, even when one run starts just as another ends, so that a decision's recorded start and end commits stay equal and I can trust the decision.

## Scenarios (the ZOMBIES letter in each title)

### Cycle 1: registration survives the other run's rmdir (Done check 1)
Scenario: [Z] a run with no other run registers once and leaves nothing
  Given no holders directory exists
  When a run registers and then releases
  Then one holder file named for its token existed while it voted
  And no holder file and no holders directory remain
Scenario: [O] the directory is removed between B's mkdir and B's open
  Given the holders directory is removed once, just before B opens its temporary file
  When B registers
  Then B has exactly one holder file with its own pid and token
  And decider_lock_live is true while B votes
  And a facilitator commit and a facilitator push during B's vote are each refused with owner decider and zero git writes
Scenario: [B] the removal lands on every attempt but the last allowed one
  Given the directory is removed before each of the first 4 attempts
  When B registers
  Then the 5th attempt leaves one holder file and the checkout is refused: a facilitator commit and a facilitator push are each refused with zero git writes
Scenario: [E] the removal lands on every attempt
  Given the directory is removed before each of the 5 attempts
  When B registers
  Then B stops after 5 attempts, no exception reaches the caller, and stderr names the vote lock and the failure
  And the second release call is harmless

### Cycle 2: A releases at every point of B's registration (Done check 2)
Scenario Outline: [M] A releases at <point> of B's registration
  Given A releases and removes the directory at <point>
  When B registers and votes
  Then a facilitator commit and a facilitator push during B's vote are each refused with owner decider and zero git writes
  Examples: after the mkdir | before the temporary open | before the replace
Scenario: [I] the readers see the holder without deleting anything
  Given B holds its registration
  When decider_lock_live and decider_holders are read twice
  Then both reads agree and the directory's entries are unchanged
Scenario: [B] the replace point cannot lose the directory
  Given a directory removal is attempted just before B's replace
  Then the removal fails because the temporary file is in the directory and B's holder file appears

### Cycle 3: nothing is stranded, nothing live is removed (Done check 3)
Scenario: [S] the card 298 and card 230 tests pass unchanged
  When the LiveVote and LiveVoteOverlap suites run
  Then every test passes
Scenario: [M] A ends first or B ends first
  Given A and B overlap
  When the first run ends
  Then a facilitator commit is refused until the last run ends
  And after both end no holder file and no holders directory remain
Scenario: [E] a dead holder and an empty directory left by a crash
  Given a holder naming a dead pid, or an empty directory, is on disk
  When a run registers
  Then the dead holder is swept, the run's own holder exists, and the checkout is refused

## Done checks (3, one per cycle)
1. Scenarios of cycle 1 pass; each fails on today's code except [Z].
2. Scenarios of cycle 2 pass: B holds a live holder whichever point A releases at.
3. Scenarios of cycle 3 pass; the existing suites stay green; nothing is left after both runs end.

## Context
- Filed by the facilitator on 2026-11-06 from card 298's sign-off; a defect, kept as a bug.
```

ZOMBIES coverage: Z, O, M, B, I, E and S are each present above. Per cycle:
- **Cycle 1:** Z alone; O one removal; M not exercised (cycle 2); B 4 removals then success; I the registration result; E 5 removals; S the plain path.
- **Cycle 2:** Z no removal at a point; O removal at one point; M three points; B the replace point; I the readers; E the removal fails because the directory is non-empty; S a plain overlap.
- **Cycle 3:** Z none left; O one run; M both end orders; B the last release; I the existing suites; E dead holder and empty directory; S the plain overlap.

## Slice guidance for the architect
- **Retry bound.** Run the whole registration (mkdir, sweep, open, write, replace) in a loop of at most 5 attempts (a module constant). It retries only on `FileNotFoundError` from the open or the replace, and reruns `mkdir`.
  - Each failed attempt needs another run to have ended inside the gap. Five is far above the realistic overlap of two or three runs.
  - On exhaustion, print one stderr warning and return the no-op. This is fail-open, but no longer silent. Card 315's general missing warning stays out of scope.
  - Any other `OSError` keeps today's behaviour.
- **Replace.** The seat's reading is that an rmdir cannot land between the open and the replace, because the `.tmp` file keeps the directory non-empty. The sweep skips `.tmp` names (`decider.py` `entry.name.endswith(".tmp")`). So "an equivalent for replace" is the same loop, not a second mechanism. The architect confirms this. It is the reason cycle 2's replace-point scenario may have no red.
- **Deterministic test, no sleeping, no threads.**
  - Patch `os.open` in the `decider` module with a wrapper. On a call whose path ends in `.tmp` and is under the holders directory, and whose attempt number is in a chosen set, the wrapper calls `holders_dir.rmdir()` and then the real open. This is the sign-off probe.
  - Do the same with `os.replace`, and for "after the mkdir" patch `facilitator.decider_lock_path` (called once, between the sweep and the open).
  - Call `decider.hold_vote_lock(repo)` directly on the `RunnerCase` repo. Then assert on `fac.decider_holders(repo)`, `fac.decider_lock_live(repo)` and a `Writers` commit attempt.
  - Put these in a new class `VoteLockRegistration(RunnerCase)`, so the quick command does not pay `LiveVoteOverlap`'s bare-origin setUp. The architect may overrule.
  - No production seam is needed, and none should be added.

## D193 trial items

**G1: table for the gate brief** (from `git grep` above; the facilitator fills the changed-file column from `git diff --name-only`):

| Changed file expected | Test file naming it | Narrow suite (from `C:/Users/<user>/mob-crosscli`) |
|---|---|---|
| `decider.py` (`hold_vote_lock`, `:313-370`) | `tests/test_decider_runner.py` | `python -m unittest tests.test_decider_runner.VoteLockRegistration` (new) |
| same | same | `python -m unittest tests.test_decider_runner.LiveVoteOverlap` (7 tests) |
| same | same | `python -m unittest tests.test_decider_runner.LiveVote` |
| same | same | `python -m unittest tests.test_decider_runner` (whole file) |
| `facilitator.py`, only if touched | `tests/test_writer_isolation.py` names `Writers` only | `python -m unittest tests.test_writer_isolation` (31 tests, about 70 s, record-only) |

The suite table is not frozen. If the diff names another file, the gate reruns `git grep -l` for it.

**G2: cycles that may have no red, with a committed mutation pair each** (two scratch-copy mutations, saved as patches under `.mob/records/313/`):
- **Cycle 1** has a red (today's code fails [O], [B] and [E]), so it needs no pair.
- **Cycle 2** may be green from the start for the replace point and the readers. Pair:
  - *M2a: retry reruns only the open, not the mkdir.* The before-open scenario fails. It breaks "registration recreates the directory".
  - *M2b: the temporary file is created outside the holders directory, then replaced into it.* The before-replace scenario fails. It breaks "the directory is non-empty from the first file onward".
- **Cycle 3** is expected to have no red, because it asserts behaviour 298 already has. Pair:
  - *M3a: release skips `rmdir`.* "No directory remains" fails. It breaks "the last run out removes the directory".
  - *M3b: release deletes every `*.json` in the directory.* The commit-refused-after-first-end scenario fails. It breaks "no live run's holder is removed".

**G3: steps and gaps of `hold_vote_lock`** (lines from the grep above):

| Step or gap | Failure, crash, or another run's rmdir |
|---|---|
| `:317` `decider_holders_dir`, None outside git | Returns a no-op. Intended; no vote lock outside git. |
| `:325` mkdir | `OSError` goes to `:359` and a no-op (card 315, unprotected without warning, out of scope). A crash leaves an empty directory; the next mkdir uses `exist_ok` and the release rmdir removes it. |
| Gap mkdir to iterdir | An rmdir makes `iterdir` fail; that is caught at `:328` and treated as no entries. Harmless. |
| Sweep (`:330-343`) and legacy sweep (`:345-353`) | Each unlink error is swallowed. Another run sweeping the same entry is idempotent. A crash here changes nothing. |
| Gap sweep to open | **The defect.** An rmdir here makes the open raise `FileNotFoundError` and returns a no-op. The fix is the retry. This is the gap cycles 1 and 2 test. |
| `:355` open of the temporary file | A failure other than a missing directory returns a no-op (unchanged). A crash leaves nothing. |
| Write | A failure leaves a `.tmp` and a no-op (card 314). A crash leaves a `.tmp` that is skipped forever (card 314). |
| Gap open to replace | An rmdir fails because the `.tmp` is inside. Cycle 2 asserts it. |
| `:358` replace | A failure returns a no-op and leaves a `.tmp` (card 314). A crash leaves a `.tmp`. |
| After replace | A crash leaves a holder with a dead pid. The next run's sweep removes it, and `decider_lock_live` ignores it meanwhile. |
| `:364` release unlink | A failure prints a warning (existing). A crash leaves a holder with a dead pid, handled as above. |
| Gap unlink to rmdir, `:368` | An rmdir on a non-empty directory fails and is swallowed. An rmdir on an empty directory removes B's fresh directory, which is the defect. The retry covers it. |

A line that reads "record-only" or "blocked" for any step makes the verdict HOLD under G3, until the facilitator runs the probe.

## Task-force declaration (same roles as card 298)
- **Product manager (ready):** Claude, fresh context.
- **Architect (slice):** Claude, fresh context. It confirms the replace-gap reading and picks the bound.
- **QA engineer (red):** Claude, fresh context. It writes the three red cycles and, for cycles 2 and 3, the mutation patches.
- **Backend developer (green):** Codex via the machine-local launcher in full-access mode.
  - The brief says: "never rm -rf or Remove-Item -Recurse on a variable or computed path; only explicit literal paths".
  - Run it with `--json` and store the stream as `.mob/records/313/green-codex-events.jsonl`. The reviewer reads it for any delete command.
  - The green seat works only in its own separate git worktree (section "Green workspace" below), not in the main checkout.
  - Fallback: a fresh Claude takes the seat if Codex weekly quota is under about 20% at green start. The facilitator reads the quota then.
- **Independent reviewer (ship):** Copilot. It did not author. It checks that no 230 or 298 assertion is weakened.
- **Sign-off:** a fresh Claude that did not write the green.
- **Sequencing:** no decision run on the mob-crosscli main checkout while a seat edits `decider.py` in the worktree and until its commits are merged, since this card changes the code the runs load.
- **Left out:**
  - UX, frontend, database: no UI or data.
  - Security: the lock is a safety guard, not a trust boundary.
  - Technical writer: the retry bound goes in the docstring.
  - DevOps: CI unchanged.
  - Domain expert: the architect covers Windows and POSIX rmdir semantics.
  - Agile coach: observer only, plus the D193 trial retro.

## Value, metric, counter-metric
- **Value:** the product owner can trust that no commit lands during any vote, so recorded head_start equals head_end even at an overlap boundary.
- **Metric:** all three Done checks pass, the 7 `LiveVoteOverlap` tests and the `LiveVote` tests stay green, and sign-off-only findings on this card are 0 (the D193 hypothesis, 1 of 3 trial cards).
- **Counter-metrics:**
  - A leftover holder file or directory after both runs end must stay 0.
  - A weakened 230 or 298 assertion must stay 0.
  - Gate duration must not exceed twice the previous card's.
  - A mutation that is padded (does not fail its named test) counts as a failure of the trial.

## What the window decides
1. This task-force declaration, including the Codex green seat with its delete rule and `--json` stream, and the Claude fallback.
2. The ready-check card-text fixes below, taken as one card-text edit in the same window.
3. The retry bound of 5 and the stderr warning on exhaustion (the one new observable behaviour beyond the card).

## Ready-check list (card-text fixes)
1. Replace the issue body with the card text above: Connextra story, Gherkin scenarios tagged with ZOMBIES letters, and ZOMBIES coverage for all seven letters.
2. Drop the "ZOMBIES to be written by QA at ready" tails.
3. Add the HEAD sha and line references to the Source line, as in the card text above.
4. Add the invariant "a registration that gives up says so on stderr", and the "Not in this card" entries for cards 314 and 315.
5. Reword Done check 2 from "every point" to the three named points: after the mkdir, before the open, before the replace.
6. State that `decider_lock_live` and `decider_holders` stay read-only. The facilitator checks `git diff` shows `facilitator.py` unchanged.
7. Confirm card 313 is the first of the three D193 trial cards, so its retro reports the G1, G2 and G3 results.

## Green workspace (the new part of this proposal)
- Before the green seat starts, the facilitator creates a separate worktree: `git -C C:/Users/<user>/mob-crosscli worktree add C:/Users/<user>/wt-313-green -b card-313-green HEAD`. The green seat's brief names C:/Users/<user>/wt-313-green as its working directory and the only repository it may write; the main checkout of mob-crosscli is not used by the seat, so it is untouched by construction.
- The facilitator records `git rev-parse HEAD` and `git status --porcelain` of the main checkout before and after the turn in the green record (two lines each); they must be equal, and the reviewer compares them.
- After the turn the facilitator fast-forwards main to the branch (`git merge --ff-only card-313-green`), pushes, and removes the worktree and branch with `git worktree remove C:/Users/<user>/wt-313-green` and `git branch -d card-313-green` (explicit names only; never a recursive delete).
- Nothing else about the declaration changes: roles, seats, the Codex green seat with the delete rule and the `--json` stream (stored under `.mob/records/313/`), the Claude fallback, the D193 trial items G1 to G3, the retry bound of 5 with a stderr warning on exhaustion.
