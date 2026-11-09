## Intent

Give every writing seat an isolated worktree, leave integration to the facilitator, and preserve git-verifiable proof that commits, pushes, staged files, and live Decider votes never overlap unsafely.

## Premises (D184)

- P1. Every seat currently runs in the card repository checkout, not a seat-specific worktree. Command: `more +1100 facilitator.py`. Output: `"run_turn(... cwd=self.repo ...)"` and `"You are running in the project repo root ({self.repo.name})."`

- P2. A ship seat, rather than `commit_seat`, is instructed to stage, commit, and push in that same checkout. Command: `rg -n commit facilitator.py`. Output: `"explicit paths only ... commit with a message describing the increment, push through"` and `if play_name == "ship": ... "also stage ..."`. `commit_seat` itself only records seat use: `"append the seat's line to the history"`.

- P3. After every seat, `turn` scans the same repository for unpushed commits but does not serialize writers. Command: `more +1100 facilitator.py` and `rg -n push facilitator.py`. Output: `"self.scan_pushes()"` after `run_turn`, and `scan_pushes` checks `"the repositories named in MOB_WATCHED_REPOS"` or `self.repo`.

- P4. Existing push enforcement is retrospective. Command: `rg -n "unpushed_commits|push_rule_problems" mob_process.py`. Output: `"698:def unpushed_commits(repo, run=None)"` and `"762:def push_rule_problems(repo)"`. These report unpushed or rule-breaking history; they do not refuse a second writer before a commit or push.

- P5. Nothing in the inspected write path currently owns a writer lock or rejects a second writer. Command: `rg -n "index[.]lock" facilitator.py decider.py mob_process.py tests`. Output: no match in the production modules. The only observed gate before a seat is quota/rotation: `gate_seat` calls `check_seat` then `commit_seat`.

- P6. Multiple worktrees exist, but current seat dispatch does not allocate or select one. Command: `git worktree list`. Output begins `"C:/Users/<user>/mob-crosscli 73eea8a [main]"` and lists detached `wt-*` worktrees; P1 still shows every card seat receiving `cwd=self.repo`.

- P7. `decider.py` already snapshots the real checkout immediately before and after concurrent voters, compares both `HEAD` and porcelain status, and exits 5 on any change. Command: `more +330 decider.py`. Output: `"before = snapshot(repo)"`, `"after = snapshot(repo)"`, `"changes = diff_snapshots(before, after)"`, and `"return 5"` when changes exist. This proves check 3’s violation at runtime: the real-tree `HEAD` and status were stable, or the vote is rejected.

- P8. The current Decider artifact does not store the start and end HEADs required by the adopted joined condition. Command: `more +330 decider.py`. Output: the record contains `"result"`, `"prompt_sha"`, `"probe"`, `"votes"`, `"counted_seats"`, `"violations"`, and `"integrity"`, but neither `head_start` nor `head_end`; `integrity.changes` only stores a diagnostic such as `"HEAD moved ..."`.

- P9. The current checkout was not clean during measurement, so the slice does not infer safety from it. Command: `git status --short`. Output: `"?? .mob/"`.

## Domain-expert (T2) answer

Proof is sufficient only when it is causal and reconstructible from git artifacts:

1. Each writer record names its resolved checkout root and git common directory. Concurrent seat writers must have distinct checkout roots and indices; only the facilitator’s integration step may write the real checkout. A same-checkout second writer is refused before its git write with a named owner and reason.
2. For every seat and facilitator commit, `git show --name-only <sha>` is compared with the paths staged by every other writer before integration. No commit may list another writer’s staged path. The other writer’s index and staged blob must remain unchanged.
3. Every vote record stores the real-tree full SHA as `head_start` and `head_end`. They must be equal. A barrier-controlled live-vote test attempts facilitator integration while voting is active and proves that integration is refused or deferred before `HEAD` moves—not merely detected afterward.
4. The negative control proves the guard is not a global write ban: separate seat worktrees can commit, the facilitator can integrate them in controlled order, and the resulting history can be pushed.

## Criterion 1 — two writers cannot collide

**Acceptance:** A red in `tests/test_writer_isolation.py` targets the checkout allocation/refusal and integration boundary used by `facilitator.CardRun.turn`, `gate_seat`, and the ship path.

The red has two deterministic halves:

- Index collision: a temporary repository maps facilitator and seat to the same checkout, explicitly holds that checkout’s `.git/index.lock`, and invokes the second writer. HEAD behavior reaches Git’s index-lock failure today; green must instead refuse the second writer before its git write with a named reason.
- Push race: a bare remote and two writer histories start at one known SHA. Two git processes are advanced in a controlled order with barriers, not sleeps: writer A pushes, then writer B attempts its stale push. Red proves today’s non-fast-forward race; green proves only facilitator integration pushes the ordered result and no seat independently races it.

The same cycle carries the negative control: two seats in distinct worktrees commit successfully, the facilitator integrates both, and one facilitator push succeeds. A guard that refuses all writes fails.

**ZOMBIES:**  
Z — no active writer: one seat commits in its worktree and facilitator integrates.  
O — one active writer: a second writer targeting that checkout is refused before writing.  
M — two distinct seat worktrees both commit; facilitator integrates both exactly once.  
B — same checkout expressed through different path spellings resolves to one checkout and is refused; distinct worktrees sharing a common object store remain allowed.  
I — refusal names checkout, current owner, attempted writer, and operation.  
E — index-lock fixture and stale-push fixture each fail independently if the corresponding protection is absent.  
S — existing single-seat commit-and-push behavior and `scan_pushes` reporting remain valid.

**Cap:** criterion 1, at most 1 cycle.

**Size:** M.

**Green may touch:** `facilitator.py`, and only if checkout identity or push-policy extraction is required, `mob_process.py`. The red may add `tests/test_writer_isolation.py` and adjust directly affected facilitator/push tests.

## Criterion 2 — no commit sweeps another writer’s staged file

**Acceptance:** A red in `tests/test_writer_isolation.py` targets the facilitator integration/ship function established in criterion 1. Writer A stages path A in its isolated worktree; writer B stages and commits path B in its own worktree; the facilitator integrates B. For every resulting SHA, `git show --name-only <sha>` must omit A. Writer A’s staged path and staged blob SHA must be unchanged after integration. The red fails if integration uses the real checkout’s ambient index or broad staging.

The proof is from each commit’s own file list, not only from final tree contents or `git status`.

**ZOMBIES:**  
Z — no staged paths: integration creates only the intended commit.  
O — one foreign staged path: it is absent from every integrated commit and remains staged for its owner.  
M — several writers stage disjoint paths; each commit lists only its owning delta.  
B — adjacent filenames, rename/delete entries, and an untracked file are not swept by broad pathspecs.  
I — failure reports commit SHA, unexpected path, staging owner, and checkout.  
E — a deliberate broad-stage implementation makes the red fail even when final file contents look correct.  
S — `git show --name-only` for ordinary single-writer integration remains unchanged.

**Cap:** criterion 2, at most 1 cycle.

**Size:** S.

**Green may touch:** `facilitator.py`; `mob_process.py` only if the commit-file-list validation is centralized there. No template or unrelated workflow file is included.

## Criterion 3 — a live Decider vote never observes facilitator HEAD movement

**Acceptance:** A red in `tests/test_decider_runner.py` targets `decider.run` and the facilitator’s integration gate.

A barrier-controlled voter starts and pauses after the real-tree start snapshot. While the vote is live, a facilitator integration attempt is made without sleeps. Green must refuse or defer that integration before `HEAD` changes. The vote completes with:

- exit status determined by the vote, not integrity exit 5;
- `integrity.ok == true`;
- full `head_start` and `head_end` fields in `decider-result.json`;
- `head_start == head_end == git rev-parse HEAD` for the protected vote interval.

A separate pin preserves today’s negative behavior: if an uncontrolled process really moves real-tree HEAD, `diff_snapshots` still reports it and `decider.run` exits 5.

**ZOMBIES:**  
Z — no facilitator integration attempt: equal start/end SHAs are still recorded.  
O — one integration attempt during one live vote is refused or deferred before HEAD moves.  
M — concurrent voters share one protected interval; integration remains excluded until all finish.  
B — integration attempted at the start barrier and immediately before the end snapshot cannot enter the interval.  
I — the artifact records full `head_start`, full `head_end`, and integrity diagnostics.  
E — a genuine external HEAD move still produces `"HEAD moved ..."` and exit 5.  
S — vote parsing, counted seats, scratch-clone containment, and result semantics remain unchanged.

**Cap:** criterion 3, at most 1 cycle.

**Size:** S.

**Green may touch:** `decider.py` and the integration guard in `facilitator.py`; `tests/test_decider_runner.py` is the red-owned test file.

## Out of scope

- Retrying index-lock or push failures after collision.
- A general-purpose lock library, worktree CLI, or reusable worktree helper.
- Changing Git history, push-on-every-commit policy, vote parsing, result counting, seat rotation, or quota rules.
- Cleaning existing worktrees or `.mob/`.
- Card 216’s broad default-template rollout.
- Product features, UX, schemas, language behavior, unrelated audits, and repository cleanup.
- Implementing the runner-owned lock unless criterion 1 proves seat worktrees cannot satisfy the adopted mechanism; that result requires a scope window first.

## Stop rule

If any premise above proves false in code during red or green—including seat cwd, commit ownership, existing push enforcement, Decider snapshot behavior, artifact shape, or absence of a second-writer refusal—the seat stops without widening the implementation or starting another red, records the contrary evidence, and the facilitator opens a D175/D181 scope window before work continues.

## Round 2: adopted condition (from round 1's QA objection)
Criterion 2's red places writer A's staged path in the facilitator's integration checkout itself (its ambient index), as in the card's source (3c82909: a facilitator helper commit swept a seat's staged file in the shared checkout). A deliberate broad-stage integration (for example git add -A or git commit -a) then really sweeps it, so the red fails on that implementation; the green must commit only the integrated writer's own paths, and A's staged path and blob stay staged and unchanged. Caps unchanged (1+1+1).
