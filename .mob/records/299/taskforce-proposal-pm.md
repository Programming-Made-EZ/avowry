# Card 299 proposal (product manager seat, ready; read-only)

The seat's reply, committed unchanged except that the persona name before a colon, and one possessive, is written "the product owner" (the repository's personal-name check).

Disclosure: the seat ran no test suite, wrote no file and called no `gh`. Every code fact below was re-run in `C:/Users/<user>/mob-crosscli` with the command quoted beside it. Wherever it says "unprobed", no command has been run for it.

## Code facts (rerun)
- `git rev-parse --short HEAD` gives `3b3e486`. `git status --porcelain` shows only `?? .mob/`.
- `git config --get core.hooksPath` gives `.githooks`. `git ls-files | grep -i "hook\|\.sh$"` shows the only tracked hook is `.githooks/commit-msg`, next to `hooks/delete_guard_hook.py`.
  - The hook is enabled per clone (`.githooks/commit-msg:3` comment, `README.md:174`, both from `git grep -n "githooks\|hooksPath"`).
- `git grep -n "class WriterRefused\|^def _git\|^def pid_alive\|^def decider_lock_path\|^def decider_holders\|^def decider_lock_live\|^def checkout_root\|^class Writers\|    def _claim\|    def commit\|    def push\|    def release" -- facilitator.py` locates the code this card touches:

| Item | Line in `facilitator.py` |
|---|---|
| `WriterRefused` | `:1075` |
| `_git` | `:1089` (no `env` parameter) |
| `pid_alive` | `:1101` |
| `decider_lock_path` | `:1131` (uses `git rev-parse --git-path`) |
| `decider_holders_dir`, `decider_holders`, `decider_lock_live` | `:1143`, `:1149`, `:1169` |
| `checkout_root` | `:1184` |
| `Writers` | `:1189` |
| `Writers._claim` | `:1196` |
| `Writers.release`, `Writers.commit`, `Writers.push` | `:1211`, `:1216`, `:1227` |

- `Writers.__init__` holds `self.holders = {}`, an in-memory dict (`sed -n 1189,1195p facilitator.py`). That is why a second process is not refused.
- `_claim` refuses only on `decider_lock_live(root)` (`:1198`) or a different writer name in the same dict. A second process using the same name, `facilitator`, would also pass the name test.
- `git grep -n "hold_vote_lock\|VOTE_LOCK_ATTEMPTS" -- decider.py` gives the constant at `:313` and the def at `:316`. Card 313's fix is already in HEAD.
- `decider_lock_live` counts every `*.json` in `mob-decider.lock.d` as a vote (`:1169`, `sed -n 1169,1182p facilitator.py`).
- `git ls-files --eol facilitator.py decider.py tests/test_writer_isolation.py` gives `i/mixed w/mixed` for `facilitator.py` and `decider.py`, and `i/lf w/lf` for the test file.
- `git grep -n "^class \|    def test_" tests/test_writer_isolation.py` shows `Fixture` at `:97`: a local bare `origin.git`, a checkout and one pushed commit. Also `GitWrites` at `:75` and `WRITE_VERBS` at `:49`, which count git write verbs.
- `git worktree list` shows two old scratch worktrees (`wt-290-pre-green`, `wt-290-probe`). They are not ours and are untouched.
- Unprobed:
  - The `--git-path` result in a linked worktree.
  - Whether `os.link` works on this Windows volume.
  - Whether `pre-push` runs when the remote does not exist. Git documents the hook as running after the remote status check, so the test must use a local bare origin.

## Judgement: card 299 must SPLIT
Three Done checks are not the problem. The problem is that Done check 2 bundles unrelated work.
- It needs two new hook files, a per-clone install step, and a way for `Writers` to tell the hook that this git call is the owner's. That last part means `_git` gains an `env` parameter and every call site changes.
- It also needs a hook test with a real `git commit` and `git push` run as subprocesses.
- The cross-process lock stands alone and already serves the product owner. Any two processes that go through `Writers` (two facilitator runs, a facilitator and a seat) are refused with the holder's name. That covers the 2026-11-04 push race between two facilitator processes, if both went through `Writers`; the card does not say.
- Honest limit, stated on the card: a plain `git commit` by hand still is not refused by this card. That is the second card.

**First card (keeps number 299, 3 cycles):** the cross-process writer lock in `Writers`, in `facilitator.py` only. Done check 1 is the cross-process refusal. Done check 2 is the lone writer, stale takeover and own-lock release. Done check 3 is the life-cycle: crash, failing git operation and takeover race. The Risk line stays.

**Second card (the facilitator files it as a new bug card):** "A plain `git commit` and `git push` are refused while another writer or a live vote holds the checkout". It depends on 299.
- Risk line: the same ship-risk text (a push lost on 2026-11-04, the card 290 proposal overwritten).
- Actor the product owner. Value: a hand-run facilitator or seat cannot collide with a running writer or vote.
- Three criteria:
  1. `pre-commit` refuses.
  2. `pre-push` refuses.
  3. The owner's own `Writers` git calls pass, via a token in the git subprocess environment, and a clone without `core.hooksPath` is told so by one named check.
- Hook installation point: tracked `.githooks/pre-commit` and `.githooks/pre-push` beside `.githooks/commit-msg`, calling one script under `hooks/`. They are enabled by the existing `core.hooksPath .githooks`.
- Test proof: reuse `Fixture` (`:97`) and its local bare origin.
  - Copy the hooks into the temp repo and set `core.hooksPath` to them.
  - A live subprocess holds the lock.
  - Run plain `git commit` and `git push` as subprocesses.
  - Assert a non-zero exit, stderr naming the holder, and HEAD and `origin` main unchanged. A negative control with no holder commits and pushes.
- A bare local origin is used because the hook runs only after the remote is reachable (unprobed).

## Design: second lock file, not a holder in the vote directory
- The writer lock is a second file, `mob-writer.lock`, at `git rev-parse --git-path mob-writer.lock`. It is a single exclusive file, not a directory.
- Why not the vote directory: `decider_lock_live` (`:1169`) would read the writer's own holder as a live vote, and `_claim` would refuse its own writer as `decider`. The vote lock also sweeps that directory.
- Why a single file: votes share a checkout (many holders), but writers exclude each other (one holder). Exclusivity needs an atomic create.
- Reused from cards 298 and 313: the JSON format `{"pid","started","token"}`, `pid_alive` (`:1101`) as the stale rule, and the `--git-path` lookup pattern from `decider_lock_path` (`:1131`). Two keys are added, `writer` and `operation`, so the refusal can name them. A shared private reader serves both; `decider_holders`, `decider_lock_live` and `decider.py` do not change, and no vote holder is ever touched.
- Consequence for `Writers._claim`: check the vote first (unchanged), then the in-process dict, then the file. The refusal rule is "holder token is not mine", not "writer name differs". Two processes both named `facilitator` must still be refused, and so must a second `Writers()` in one process.
- Consequence for the vote lock: a vote starting while a writer holds is not refused. That is beyond this card (a write-then-vote race the working agreement already serialises).
- `WriterRefused` keeps its fields and gains `holder` (pid, token, lock path) in the message.

## Card text (replaces the issue body)
```
<!-- phrase-card:299 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug (collisions are bugs)
- **Risk to shipping:** committed work can be overwritten or a push lost, seen twice in card 230 (the card 290 proposal overwritten; a failed push on 2026-11-04), which stops delivery.
- **Repository:** mob-defaults (mob-crosscli: facilitator.py Writers and its lock helpers)
- **Source:** card 230's sign-off condition 2 (.mob/records/230/signoff-ctx6.md), rerun against mob-crosscli HEAD 3b3e486: Writers at facilitator.py:1189 keeps `self.holders = {}` in memory, so a second process is not refused; _claim :1196.
- **Depends on:** card 230 (Writers), card 298 (the holder format)
- **Consumer value:** the product owner: committed work is never overwritten by a second writer and pushes do not collide, so the board keeps moving without a nudge.
- **Actor (supporting):** any writer going through Writers, in any process
- **Given / Observes / Fails with (supporting):** a writer lock file in the checkout's git dir (mob-writer.lock, holder {pid, started, token, writer, operation}); a second writer in another process is refused, naming checkout, holder, pid, writer and operation, before any git write; today it runs and collides on the index lock or the ref.
- **Invariants:** card 230, 298 and 313 tests pass; decider.py and the vote holders are unchanged; a lone writer is never refused; a lock whose process is gone is taken over; votes stay read-only.
- **Not in this card:** a plain `git commit` / `git push` refused by git hooks (filed as its own bug card); pid reuse; writes by a linked worktree to the shared refs.

## Story
As Steve, I want a second writer on one checkout, in any process, refused with the holder's name before it writes, so that committed work is never overwritten and pushes do not collide.

## Scenarios (the ZOMBIES letter in each title)

### Cycle 1: a second process is refused (Done check 1)
Scenario: [Z] no lock file exists before or after a lone commit
  Given a checkout with no writer lock
  When one writer commits
  Then the commit lands and no mob-writer.lock remains
Scenario: [O] a second process is refused while the first holds the checkout
  Given process A holds the writer lock on the checkout
  When process B claims a commit
  Then B is refused before any git write
  And the refusal names the checkout, writer, pid and operation of A
Scenario: [M] three claimants take turns
  Given A holds, B and C each try
  Then B and C are refused, and once A releases exactly one of B and C may claim
Scenario: [B] the same writer name in another process is still refused
  Given A holds as facilitator
  When another process claims as facilitator
  Then it is refused
Scenario: [I] a second Writers in one process is refused like a second process
  Given one Writers holds the checkout
  When a second Writers() claims
  Then it is refused and a live vote is still reported as owner decider
Scenario: [E] the first writer's git operation fails
  Given A's commit fails
  Then A's lock is released and B then claims successfully
Scenario: [S] after release the second writer commits
  Given A released
  Then B's commit lands, and both commits are on main in order

### Cycle 2: a lone writer, a stale lock, release (Done check 2)
Scenario: [Z] a lone writer is never refused and leaves nothing
Scenario: [O] a lock whose pid is gone is taken over
  Given mob-writer.lock names a dead pid
  When a writer claims
  Then the claim succeeds and the file now holds the new writer's token
Scenario: [M] many sequential writers each claim and release
  Then no refusal occurs and no lock remains after the last
Scenario: [B] a release by a late holder removes only its own lock
  Given A's lock was taken over by B
  When A releases late
  Then B's lock is still on disk
Scenario: [I] reading the lock for the refusal deletes nothing
Scenario: [E] an unreadable or empty lock file
  Then it is treated as being written: the second writer is refused, not given a takeover
Scenario: [S] card 230, 298 and 313 suites pass unchanged

### Cycle 3: nothing is stranded (Done check 3)
Scenario: [Z] a killed holder (child process killed after it claims) leaves a lock the next writer takes over
Scenario: [O] two writers race to take over one stale lock
  Then exactly one holds and the other is refused
Scenario: [M] holder killed, taken over, killed again, taken over again
Scenario: [B] the lock is created whole: a reader never sees a half-written file
Scenario: [I] the refusal text names the lock file path, so the holder of a reused pid can be found by hand
Scenario: [E] the lock cannot be created (read-only git dir)
  Then one stderr warning names the lock and the failure, the git operation runs as today, no exception hides the git error
Scenario: [S] one claim, one commit, one push through Writers is unchanged

## Done checks (3, one per cycle)
1. Cycle 1 scenarios pass; [O], [M], [B] and [I] fail on today's code.
2. Cycle 2 scenarios pass; a lone writer is never refused and the stale lock is taken over.
3. Cycle 3 scenarios pass; a crash or a failed git operation never leaves the checkout refused to the next writer.

## Context
- Filed by the facilitator on 2026-11-05 under card 230's sign-off condition 2; a collision defect, kept as a bug (D183). The hook half is split into its own bug card.
```
ZOMBIES: all seven letters appear in each cycle. [B] in cycle 1 is the same-name boundary, in cycle 2 the late-release boundary, and in cycle 3 the half-written-file boundary. Per cycle: Z is the lone commit, O is one holder, M is a sequence, B is the boundary above, I is the reader/interface, E is the failure path, S is the plain path.

## G1: table for the gate brief
All narrow suites run from `C:/Users/<user>/mob-crosscli` (rerun commands, durations record-only).

| Changed code file expected | Test files that name it | Narrow suite and exact command |
|---|---|---|
| `facilitator.py` (`Writers`, new `writer_lock_path` and helpers, `WriterRefused`) | `tests/test_writer_isolation.py` | `python -m unittest tests.test_writer_isolation.CrossProcessWriters` (new class) |
| same | same | `python -m unittest tests.test_writer_isolation` (31 tests, about 70 s, record-only) |
| same | `tests/test_decider_runner.py` (calls `fac.Writers()` at `:329`, `:407`) | `python -m unittest tests.test_decider_runner` |
| `decider.py` expected unchanged | none | `git diff --name-only` must not list it |
| `tests/test_writer_staging.py` (cited by the card 230 gate) | exists? unprobed | `python -m unittest tests.test_writer_staging` |

The facilitator fills the first column from `git diff --name-only`. If the diff lists another file, the gate reruns `git grep -l "<name>" -- tests` for it.

## G2: cycles that may be born green, two mutations each
- **Cycle 1** has a real red: no cross-process state exists today. No pair is needed.
- **Cycle 2** may be green once cycle 1's lock is in. The pair is committed as patches under `.mob/records/299/`:
  - M2a: the stale rule ignores `pid_alive`, so any existing file refuses. It fails the "dead pid is taken over" scenario and breaks "a lock whose process is gone is taken over".
  - M2b: release unlinks the file without checking the token. It fails the "late release removes only its own lock" scenario and breaks "a release removes only its own lock".
- **Cycle 3** may be green too. The pair:
  - M3a: remove the `finally` release when the git operation raises. It fails the failed-operation scenario and breaks "a failed git operation never strands the checkout".
  - M3b: the takeover unlinks whatever is at the path without comparing the stale token. It fails the takeover-race scenario and breaks "exactly one taker wins".
- Each patch must `git apply` clean on a scratch copy (no comment line before the diff) and fail the named test. A padded mutation fails the trial.

## G3: steps and gaps of the lock life-cycle (changed code: `facilitator.py` Writers)
Every line is "unprobed" until the QA or green seat runs it. The facilitator probes the first four before ready, so the gate is not HOLD on record-only lines.

| Step or gap | What happens when it fails |
|---|---|
| Lock path from `git rev-parse --git-path mob-writer.lock` | Outside git, `checkout_root` raises today (`:1184`), unchanged. |
| Acquire: write a temporary file, `os.link` it to `mob-writer.lock` (atomic; `os.link` on this volume is unprobed, fallback `O_CREAT\|O_EXCL`) | `FileExistsError` means read the holder, then refuse or take over. Any other `OSError` prints one stderr warning and the git operation runs unguarded. |
| Gap create to complete content | The file appears whole or not at all (link, not write in place). An empty or unparsable file is "being written": refuse, never take over. |
| Write the holder `{pid, started, token, writer, operation}` | Failure leaves a stray temp file with no lock; the claim fails open with a warning. |
| Run the git operation | An exception goes through the existing `try/finally` (`:1216`, `:1227`) and releases. A refusal happens before any git write (`GitWrites` verbs stay empty). |
| Release | Reads the file, unlinks only if the token is mine. A missing file is harmless; an unlink error prints a warning. |
| Gap read to unlink in release | A takeover in between is possible only if my own pid is dead, so it cannot happen to a live holder. |
| Crash between acquire and release | The lock stays with a dead pid; the next writer takes it over by `pid_alive` (`:1101`). |
| Stale takeover | Read the holder, rename the stale file to a unique name, re-read the renamed file to confirm it still carries the stale token, then link my own. The architect confirms the exact steps. Two takers: exactly one wins. |
| Pid reuse | A dead holder's pid is reused by another process, so the lock looks live and the claim is refused. This is the card 298 and 313 gap (the stale rule is `pid_alive` only). The refusal text names the lock file path so the product owner can remove it by hand. No automatic delete. |
| Vote then writer, writer then vote | The vote check (`:1198`) comes first and is unchanged. A vote that starts during a writer's hold is not blocked (not in this card). |
| Linked worktree | `--git-path` of an unknown name may resolve per worktree (unprobed). A worktree's lock does not guard a ref shared with the main checkout. Not in this card. |

A "record-only" or "blocked" mark on any line makes the verdict HOLD under G3 until the facilitator's probe output is attached.

## Green workspace (as D195)
- The facilitator creates the worktree with `git -C C:/Users/<user>/mob-crosscli worktree add C:/Users/<user>/wt-299-green -b card-299-green HEAD`. It is the green seat's only working directory and the only repository it may write. The main checkout is untouched by construction.
- The facilitator records `git rev-parse HEAD` and `git status --porcelain` of the main checkout before and after the turn (today: `3b3e486` and `?? .mob/`). They must be equal, and the reviewer compares them.
- Brief lines for the green seat:
  - "Never `rm -rf` or `Remove-Item -Recurse` on a variable or computed path; only explicit literal paths."
  - "Keep each file's existing line endings and rewrite no untouched line." `facilitator.py` is mixed (`git ls-files --eol`), so the diff must be small.
  - Run with `--json` and store the stream as `.mob/records/299/green-codex-events.jsonl`; the reviewer reads it for delete commands.
- Fallback: a fresh Claude takes the green seat if Codex weekly quota is under about 20% at green start. The facilitator reads the quota then.
- After the turn: `git merge --ff-only card-299-green`, push, then `git worktree remove C:/Users/<user>/wt-299-green` and `git branch -d card-299-green` (explicit names only). Nothing is deleted without those named commands.
- The worktree's `core.hooksPath` is shared config (unprobed). This card needs no hook, so it does not matter here.

## Task-force declaration
- **Product manager (ready):** Claude, fresh context.
- **Architect (slice):** Claude, fresh context. It decides link versus `O_EXCL` and the takeover steps, and confirms `--git-path` in a worktree.
- **QA engineer (red):** Claude, fresh context. It writes the three cycles, the real two-process test (a `subprocess` child holding the lock) and the mutation patches.
- **Backend developer (green):** Codex, full access, `--json`, in `wt-299-green`.
- **Independent reviewer (ship):** Copilot. It checks that no 230 or 298 assertion is weakened and that `decider.py` is unchanged.
- **Sign-off:** a fresh Claude that did not write the green.
- **Left out:**
  - UX, frontend, database: no UI or data.
  - Security: a safety guard, not a trust boundary.
  - Technical writer: the docstring is enough.
  - DevOps: CI unchanged.
  - Domain expert: the architect covers Windows and POSIX link semantics.
  - Agile coach: observer only, plus the D193 trial retro (this is the second of three trial cards).
- **Sequencing:** no `Writers` use by the facilitator on the main checkout of mob-crosscli while the green seat's branch is unmerged.

## Value, metric, counter-metric
- **Value:** the product owner can trust that two writers never both write one checkout, so committed work survives and pushes land.
- **Metric:**
  - All three Done checks pass.
  - The existing writer-isolation and decider-runner suites stay green.
  - Sign-off-only findings on this card are 0 (the D193 hypothesis, card 2 of 3).
  - The incident count (an overwritten proposal, a lost push) over the next three cards is 0, measured by `git log` and push errors.
- **Counter-metrics:**
  - A stranded lock after a run ends must stay 0.
  - A false refusal of a lone writer must stay 0.
  - A weakened card 230 or 298 assertion must stay 0.
  - A padded mutation counts as a failure of the trial.
  - Gate duration must not exceed twice the previous card's.

## What the vote decides
1. The split: card 299 narrows to the cross-process lock, and the hooks become a second bug card with its own risk line.
2. The second lock file `mob-writer.lock` in the git dir, not a holder in the vote directory, with `writer` and `operation` added to the holder JSON.
3. Fail-open with one stderr warning when the lock cannot be created (other than "exists"), the same stance as card 313.
4. Pid reuse stays out of this card, with the refusal naming the lock path.
5. The task force and the Codex green seat in its own worktree, as above.

## Ready-check list (card-text fixes)
1. Replace the body with the card text above: Connextra story, ZOMBIES-tagged Gherkin, all seven letters per cycle.
2. Drop the "ZOMBIES to be written by QA at ready" tails.
3. Add the HEAD sha and line references to Source.
4. Remove the hook sentences from Observes and Done check 2, and file them as the second bug card with its Risk line.
5. Set the "Not in this card" entries: hooks, pid reuse, worktree-shared refs.
6. State that `decider.py` and the vote holders do not change, and that the check is `git diff --name-only`.
7. Replace the card's phrase "taken and released like card 230's vote lock" with the real mechanism: one exclusive file and an atomic create, not a holder directory.
8. State "refused" means before any git write, proven by the `GitWrites` verb list at `tests/test_writer_isolation.py:75`.
9. Confirm card 299 is the second D193 trial card, so its retro reports G1, G2 and G3.

## Facilitator's note (not the seat's text)
- In the G2 section the facilitator added "(no comment line before the diff)" to the git-apply-clean sentence, from the card 313 retro (the `# breaks:` header stopped `git apply`); the breaks note goes in the record, not in the patch file.
