# Pre-ship gate: card 313 (Copilot ctx-5, read-only; the report is committed unchanged below the line 'Reviewer report'; the facilitator-run evidence section above it is the facilitator's own)

Verdict by the reviewer: HOLD (D193 trial G3: record-only lines make the verdict HOLD until the facilitator runs the probe; and one LiveVote test failed in the reviewer's shell).

## Facilitator-run evidence (rerun by the facilitator after the HOLD; these are the facilitator's runs, not the reviewer's)
- The failing test the reviewer named, `test_a_facilitator_push_during_the_vote_is_refused_and_lands_once_after`: 6 of 6 runs OK (`python -m unittest tests.test_decider_runner.LiveVote.test_a_facilitator_push_during_the_vote_is_refused_and_lands_once_after`, six separate runs); `python -m unittest tests.test_decider_runner.LiveVote` twice: Ran 8, OK both; the whole file `python -m unittest tests.test_decider_runner`: Ran 43, OK in 73.9 s. The reviewer's failure text is not in its report, so the cause is not known; it was not reproduced in eight runs of that class and one of the file (the reviewer ran several suites in one session, possibly in parallel on a shared temp area). This is carried to the retro as an open flake observation, not as a pass for the flake.
- G2: the reviewer's two reproductions were `blocked` (its scratch apply did not change decider.py: the patch files start with a `# breaks:` comment line, which `git apply` rejects). The facilitator removed that header line and reproduced two mutations in a scratch git copy of HEAD: M2b fails test_the_replace_point_cannot_lose_the_directory ("False is not true : the holders directory was removed at the replace point"); M3a fails the either-order test ("True is not false : the holders directory was left behind", both subtests). The patch files keep the header for the record; apply them with the first line removed.
- G3 lines the reviewer marked record-only, probed by the facilitator (`python scripts-less probe g3probe.py` in the scratchpad, results as printed): outside git: no exception, no holder, no stderr (a no-op); `Path.mkdir` PermissionError: no-op, silent (card 315); `os.open` PermissionError: no-op, silent; write failure (`os.fdopen` OSError): no-op, silent, one `.tmp` left in the holders directory (card 314, closed as not planned); `os.replace` PermissionError: no-op, silent, one `.tmp` left; release unlink PermissionError: one stderr warning "could not release the vote lock ...", no exception. Every case returns a release that is safe to call twice. None of these paths is changed by card 313 (the retry catches FileNotFoundError only).
- (b) `git diff --stat 1590679 HEAD -- decider.py`: 44 insertions, 30 deletions (the follow-up commit restored the original line endings); (d) Co-Authored trailers in 1590679..HEAD: 0; (e) `git worktree list` shows the card worktree and main checkout HEAD/status equal in main-checkout-before.txt and main-checkout-after.txt; (c) the Codex stream green1-codex-events.jsonl: 11 commands, none matching rm, del, rmdir, Remove-Item, git clean, git push or the main checkout path (facilitator's regex scan).

## Reviewer report (Copilot, unchanged)
Done 1: PASS rerun — VoteLockRegistration ran 9 tests in 8.833s and OK.
Done 2: PASS rerun — the A-release-at-each-point and replace-point checks passed in the same run.
Done 3: PASS rerun — the dead-holder and empty-directory checks passed in the same run.
G1: `python -m unittest tests.test_decider_runner.VoteLockRegistration -v` => `Ran 9 tests in 8.833s`, `OK` rerun
G1: `python -m unittest tests.test_decider_runner.LiveVoteOverlap -v` => `Ran 7 tests in 11.921s`, `OK` rerun
G1: `python -m unittest tests.test_decider_runner.LiveVote -v` => `Ran 8 tests in 11.838s`, `FAILED (failures=1)` rerun
G1: `python -m unittest tests.test_decider_runner -v` => `Ran 43 tests in 54.907s`, `FAILED (failures=1)` rerun
G1: `git grep -l "hold_vote_lock\|decider_lock_live\|mob-decider" -- tests` => no other test file names the lock code; `git diff --name-only 1590679 HEAD` => `decider.py` and `tests/test_decider_runner.py` only; no `facilitator.py` change rerun
G2: M2b blocked — scratch archive apply did not change `decider.py` in this shell; the matching test stayed OK.
G2: M3a blocked — scratch archive apply did not change `decider.py` in this shell; the matching test stayed OK.
G3: outside git none => record-only
G3: `mkdir` OSError => record-only
G3: gap mkdir to iterdir => rerun
G3: sweep and legacy sweep => rerun
G3: gap sweep to open => rerun
G3: `os.open` other OSError => record-only
G3: write OSError => record-only
G3: gap open to replace => rerun
G3: `os.replace` FileNotFoundError => rerun
G3: after replace => rerun
G3: release unlink OSError => record-only
G3: gap unlink to release rmdir => rerun
G3: after exhaustion warning => rerun
Extra (a): `git diff 1590679 HEAD -- decider.py` only changed `hold_vote_lock`, its docstring, and `VOTE_LOCK_ATTEMPTS`; `release` untouched rerun
Extra (b): line endings / 44-line diff / `git show --stat` follow-up record-only
Extra (c): Codex event stream command_execution list, delete commands, main-checkout path, push usage: record-only
Extra (d): `git log --format=%B 1590679..HEAD | grep -c Co-Authored` => zero trailer count record-only
Extra (e): main checkout before/after and `git worktree list` equal and no stray worktree effect: record-only
Extra (f): red before green for cycle 1 and mutation evidence adequacy: record-only
Extra (g): race review: no spin / no `.tmp` retention / read-only: record-only
Verdict: HOLD — `python -m unittest tests.test_decider_runner.LiveVote -v` still fails 1 test (`test_a_facilitator_push_during_the_vote_is_refused_and_lands_once_after`), so the file is not green and this card cannot SHIP under D190. rerun=20 record-only=11



exit=0
