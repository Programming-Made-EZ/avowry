## Scope proposal — Card 230, criterion 1 cycle 2

### 1. Premises (D184)

All evidence is from `0e1e5f3` in the specified `wt-230-g1full` checkout. No files were changed.

- The toy repository has an initial commit, so it currently qualifies for seat worktrees. `git show 0e1e5f3:smoke.py` shows:

  > `git(repo, "init", "-q", "-b", "main")`  
  > `git(repo, "add", "-A")`  
  > `git(repo, "commit", "-q", "-m", "toy repo")`

- The toy driver writes `done.flag` relative to its process CWD. Command:

  `rg -n -C 4 "FAKE_DONE_AFTER" tests\fakes\fake_cli.py`

  Output:

  > `136-        counter = cwd / "green_count.txt"`  
  > `139:        if count >= int(os.environ.get("FAKE_DONE_AFTER", "1")):`  
  > `140-            (cwd / os.environ.get("FAKE_DONE_FILE", "done.flag")).write_text(`  
  > `141-                os.environ.get("FAKE_DONE_CONTENT", "done"))`

  On `0e1e5f3`, the driver CWD is therefore `<scratch repo>.mob-worktrees/001-smoke-greeting/<driver-role>`, not the scratch repo. That is where `done.flag` now appears.

- The fake toy seat does not commit `done.flag`. The quoted fake code only calls `write_text`; it contains no Git add or commit. `Writers.commit` is used for the facilitator’s audit trail, not automatically after a writing seat.

- Integrate does not run. Command:

  `more +1840 facilitator.py`

  Relevant output:

  > `done = self.checks(f"After cycle {c}")`  
  > `if self.args.ship:`  
  > `    self.write_record()`  
  > `    self.turn("ship")`

  And `turn("ship")` is the only path that calls:

  > `if play_name == "ship" and Path(cwd) != self.repo:`  
  > `    self.integrate_ship()`

  `smoke.py` invokes `facilitator.py` with `--true-mobbing` but without `--ship`, so this run never integrates the seat worktree.

- The scratch repository has no remote. `git show 0e1e5f3:smoke.py` shows initialization, local Git identity, add, and commit, but no `git remote add`. Thus even a requested ship would have no configured `origin` to receive the final push.

- The smoke assertion deliberately checks the scratch repository, not the seat worktree. `git show 0e1e5f3:smoke.py` shows:

  > `done_path = repo / args.done_file`  
  > `done_ok = done_path.exists() and done_path.read_text(...).strip() == args.expect`

  Consequently, the done check inside the facilitator can pass against `self.work_checkout`, and the facilitator can exit 0, while the externally observable product file remains only as an uncommitted file in a linked worktree. No merge can carry it because merges consume commits, and no integration is attempted anyway.

### 2. Cause

`0e1e5f3` enables linked seat worktrees solely from “own checkout root plus HEAD,” so the committed, remote-less, non-shipping smoke repo is moved into a workflow whose only publication path requires both a seat commit and a ship-time integration, neither of which occurs.

### 3. Proposed fix

Reopen criterion 1 for cycle 2 and raise its cap from 1 to 2; today’s caps become **2+1+1**.

The smoke test is correct; the green design is wrong on the merits. A toy seat that writes without committing models a real CLI outcome. Uncommitted output in a private worktree must not silently disappear from the caller’s repository. Worktree isolation should be enabled only when the run has a usable integration path. For a run without `--ship` or without a configured push remote, retain the established in-place behavior.

Red: retain `tests.test_smoke.Smoke.test_healthy_seats_pass_every_check` as the end-to-end red and add a focused case in `tests/test_writer_isolation.py`:

- **Given** an own-root Git repository with an initial commit but no remote, and a non-shipping run,
- **When** a writing seat requests its CWD and writes an uncommitted product file,
- **Then** its CWD is the repository itself, no linked seat worktree is selected, and the product file is visible at `repo / file`.

Short ZOMBIES:

- **Z:** zero remotes falls back in place.
- **O:** one usable integration remote plus `--ship` permits a seat worktree.
- **M:** additional remotes do not change the selected integration contract.
- **B:** blank/missing remote information falls back safely.
- **I:** own-root, HEAD, ship intent, and usable remote are all required.
- **E:** Git remote-query failure falls back in place without losing seat output.
- **S:** existing no-commit/not-own-root behavior remains unchanged.

Permitted green files:

- `facilitator.py`
- `tests/test_writer_isolation.py`

Do not weaken or rewrite `tests/test_smoke.py`; it remains the regression proof. No changes are needed in `smoke.py` or the fake seat.

### 4. Stop rule

Stop after the focused red is observed failing for the worktree-selection reason, the smallest eligibility change makes it pass, both failing smoke cases pass, and the criterion-1 writer-isolation suite remains green. If satisfying the red requires automatic commits, changing smoke expectations, adding a remote to the toy repo, or redesigning ship/integration semantics, stop and return to scope rather than expanding cycle 2.
