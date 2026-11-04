# Card 230 — criterion 1, cycle 3 scope (caps 3+1+2)

## Premises on `1cc249d01b54b716bc2a2bf60f4d57ed973282fa`

Command:

`git rev-parse HEAD`

Quoted output:

`1cc249d01b54b716bc2a2bf60f4d57ed973282fa`

Command:

`findstr /n ".*" facilitator.py`

Quoted production seams:

- `1136:class Writers:`
- `1137:    """One registry per run: at most one writer holds a checkout at a time.`
- `1186:    def catch_up(self, checkout, worktrees):`
- `1217:    def integrate(self, checkout, worktrees, remote="origin", branch="main"):`
- `1252:        self.seat_checkouts = {}`
- `1255:    def worktrees_available(self):`
- `1271:    def seat_claim(self, cwd, role):`
- `1283:    def seat_checkout(self, role):`
- `1293:                _git(self.repo, "worktree", "add", ...`
- `1297:    def seat_cwd(self, role):`
- `1303:        cwd = self.seat_checkout(role)`
- `1307:                self.writers.catch_up(cwd, others)`
- `1327:    def integrate_ship(self):`
- `1340:            head = self.writers.integrate(self.repo, list(self.seat_checkouts.values()),`

Thus `Writers.catch_up`, `Writers.integrate`, `CardRun.worktrees_available`, `CardRun.seat_checkout`, `seat_checkouts`, the worktree branch of `seat_cwd`, and the integration half of `integrate_ship` are the removable worktree path. `Writers._claim/claim/release/commit/push`, `CardRun.seat_claim`, and the in-place return from `seat_cwd` are already the lock-on-main mechanism.

Command:

`rg -n def tests\test_writer_isolation.py tests\test_writer_staging.py tests\test_decider_runner.py`

Worktree-dependent tests:

- `test_writer_isolation.py`: `test_with_no_active_writer_a_seat_commits_in_its_worktree_and_the_facilitator_integrates`; `test_worktrees_sharing_one_object_store_are_distinct_checkouts`; both `PushRace` tests; `test_two_worktrees_commit_and_are_integrated_exactly_once_with_one_push`; both `ScanPushesStaysValid` tests; and `CardRunWiring` tests through `test_ship_integrates_through_writers_once_and_only_the_facilitator_pushes`.
- Its worktree-selection guards in `InPlaceFallback`: `test_guard_own_root_head_ship_and_origin_select_a_seat_worktree` and `test_guard_extra_remotes_do_not_change_the_selection`.
- `test_writer_staging.py`: both `FacilitatorIntegration` tests and `ShipAuditTrail.test_the_ship_audit_trail_commit_and_integration_do_not_sweep_a_seats_staged_file`.
- `test_decider_runner.py`: `LiveVote.setUp` creates `seat-a`; `attempt(..., "integrate")` calls `Writers.integrate`; consequently `test_a_facilitator_integration_during_the_vote_is_refused_before_head_moves` depends on the removed path.

Already lock-on-main:

- `IndexCollision`, `PathSpellings`, `OneWriter.test_a_second_writer_claiming_a_held_checkout_is_refused_and_may_claim_after_release`, and `test_the_facilitator_alone_in_its_checkout_commits_and_pushes`.
- `InPlaceFallback.test_in_place_a_play_turn_seat_holds_the_claim_for_its_whole_turn` and `test_in_place_a_driver_seat_turn_holds_the_claim_for_its_whole_turn`.
- `test_writer_staging.py`’s `FacilitatorCommit` and `BroadStageIsCaught` file-list tests.
- `LiveVote`’s commit, barrier, shared-interval, external-HEAD-move, record, and release tests. Voter scratch clones in `Isolation` are decision isolation, not seat worktrees, and remain.

## Restated Done checks

1. All seat and facilitator writes use the one checkout on `main`. A writing seat runs with `cwd == repo` while holding the checkout claim. No worktree or per-seat branch is created. Any other writer, including the facilitator, is refused before Git writes while that claim is held; the facilitator performs its pending commit/push after release. Negative control: one writer on `main` is never refused, and the facilitator does write after the seat releases.

   The old stale-push race is not a separate live case: with one checkout, one lock holder, and only the facilitator pushing, there cannot be two stale tips racing. Keep at most an unprotected historical guard if useful; the Done proof is one serialized push after release, not simulated competing worktree pushes.

2. With writer A’s path staged in the one checkout, no commit by the facilitator or writer B lists A’s path, and A’s exact staged entry remains. Inspect every new commit’s own file list (`git show --name-only` or stricter name-status); A’s path appears only in A’s eventual explicit-path commit.

3. Unchanged: each decision record contains full `head_start` and `head_end` commit IDs for the real tree, and the check fails when they differ. The checkout claim excludes facilitator commit/push for the whole vote interval.

Each check fails on partial work: retaining any `git worktree add`/`mob/<card>/<role>` path fails check 1; serialization without staged-entry/file-list preservation fails check 2; locking without equal recorded heads fails check 3.

## Cycle 3 window

This is criterion 1 cycle 3, bringing caps to **3+1+2**.

Red may touch only:

- `tests/test_writer_isolation.py`
- `tests/test_writer_staging.py`
- `tests/test_decider_runner.py`

Rewrite/remove the worktree-dependent tests named above. Add pins that:

- patch/spy Git and prove no `worktree`, branch-creation checkout/switch, merge, or catch-up command occurs;
- prove every writing seat launches with `cwd == repo`;
- prove the seat owns the claim for its whole turn;
- attempt facilitator commit/push during that turn and observe refusal before Git writes;
- then release the seat and prove the facilitator writes exactly once;
- prove a lone seat and lone facilitator each acquire/write/release without false refusal;
- retain check 2’s staged-entry and per-commit file-list proof in the single checkout;
- replace the Decider integration attempt with facilitator commit/push attempts under the vote lock.

Green may touch only:

- `facilitator.py`
- `core/process.md` and role briefs only where they explicitly instruct seats to use worktrees, branches, or facilitator integration.

Delete the worktree selection/creation/catch-up/integration behavior. `seat_cwd` always selects `self.repo`; writing turns always use `seat_claim`; ship commits the audit trail with explicit paths and pushes only after the seat claim is released.

ZOMBIES: Z lone writer; O seat then facilitator; M sequential seats plus facilitator; B refusal at mid-turn boundaries; I staged rename/delete/untracked and adjacent names; E exceptions still release the claim; S one checkout, one branch, one push, stable Decider heads.

Criterion 2’s `Writers.integrate` tests do **not** still apply: there is nothing to integrate. Preserve their underlying invariant by the explicit-path `Writers.commit` and ship-audit tests. A passing merge/integration test would preserve the architecture being removed.

## Stop rule

Stop at the first full-suite pass where no production or test path creates/reuses a seat worktree or per-seat branch, every writing seat uses `cwd == repo` under the claim, the facilitator writes only after release, check 2’s staged state and commit file lists remain exact, and check 3 remains green.

Return to scope if success requires concurrent writers, polling/retry/sleeps, a second checkout, merge/catch-up logic, weakening Decider head equality, changing commit history semantics beyond removal of integration commits, touching files outside the window, or exceeding caps **3+1+2**.
