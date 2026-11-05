# Card 230 sign-off (sign-off, ctx-6, fresh context)

Seat: Claude. Read: board/230, 297, 298; the card record; windows 230-taskforce, slice, scope, scope-2, scope-3; the scope-3 proposal and checklist; the gate (SHIP) with the facilitator's note; the working agreement's trunk-based and "Parallelism only for voting" lines. Code read on mob-crosscli 1732a82 = origin/main: facilitator.py Writers, seat_cwd, seat_claim, ship_push and both turn paths; decider.py hold_vote_lock, run, snapshot. I ran the three writer and decider modules (Ran 65, OK, 118s) with TEMP in the scratchpad. The tree stayed clean. I relied on the recorded full suite (full_suite_1732a82: 1840 tests OK). I ran no delete, stash, model or repository edit.

## Verdict: SIGNED OFF with conditions

The code meets the Done checks as restated by window 230-scope-3. It does not deliver the source case, where a hand-run facilitator and seats write one checkout. The card can close only if the closing record states that limit and a bug card is filed for it (conditions 1 and 2).

## 1. Done checks (as restated by 230-scope-3)

| Check | Verdict | Evidence |
|---|---|---|
| 1. One checkout on main. A seat runs with cwd = repo under the claim. There is no worktree or per-seat branch. The facilitator is refused while the seat holds the claim and writes once after release. A lone writer is never refused. | Met; the push-race half met by ruling | facilitator.py:1212-1215 (seat_cwd returns self.repo). 1202-1210 (seat_claim). 1365-1373 and 1711 (the claim is held for the whole writing turn and released in `finally`). 1134-1144 (_claim refuses before any git write). 1165-1168 (only the facilitator pushes). 1217-1246 (ship_push), called at 1377 after the release at 1373. Tests: test_writer_isolation.py:156 (an unprotected collision reproduces), 164, 195, 206, 686, 693, 709, 725 (a real repository's history), 736 (release on exception). |
| 2. With A's path staged, no commit by the facilitator or writer B lists A's path, and A's staged entry remains. | Met for facilitator commits; seat commits met by brief | facilitator.py:1154-1163 (`git commit -- <paths>`). test_writer_staging.py:101, 110, 122, 146, 158, 188. Seat commits go through plain git and are told to use explicit paths (facilitator.py:942, 1340). Test 188 simulates such a seat. |
| 3. The record holds full head_start and head_end, and the check fails when they differ. The claim excludes facilitator commit and push for the whole vote. | Met for one run | decider.py:313-344 (lock in the git dir; stale lock taken over). 378-386 (the lock spans both snapshots and is released in `finally`). 407 (head fields). 111 and 426 (a head move exits 5). facilitator.py:1109-1119 and 1136-1137 (a live lock refuses every Writers write). test_decider_runner.py:372-479. A second overlapping run is unprotected: card 298. |

Ruling on check 1: the card's original check 1 asked for a red that reproduces a push race. Window 230-scope-3 declared that case not live, because with one checkout and only the facilitator pushing there are no two stale tips, and red 1d removed both push-race tests. That ruling is sound for a CardRun. The closing record should say that check 1 is met by ruling for that half.

Limits I found in the code (for the closing record):
- The seat claim lives only in memory (facilitator.py:1131-1132). A seat's own git commands are never mediated. Protection comes from serial turns in one process.
- The vote lock binds only writes that go through Writers. A seat committing during a vote, or a plain `git commit` by a hand-run facilitator, is detected (exit 5) but not refused.
- If a vote is live when a seat starts, seat_claim raises WriterRefused out of turn() (facilitator.py:1136, 1365). The run stops with the reason named. That fits the card's "Fails with", but it is a stop, not a wait.
- stash_leftovers (facilitator.py:1815-1834, written before this card) runs `git stash push -u` in the one checkout outside Writers. That command can set aside another writer's staged file.

## 2. The source case (hand-run facilitator and seats)

Not delivered. All four source incidents happened on avowry while the mob was hand-run. So did the voided vote, which was a facilitator commit made with plain git. The card's Repository line names "the facilitator's helpers", and push_retry.sh, inflight.sh and the hand-run commit path were not changed. The defect recurred twice during this card:
- the ready seat's stray edit in the shared avowry checkout;
- the 19:46 UTC push race between two facilitator processes.

Neither would have been refused by the shipped code. Card 230's own red and green seats were safe in mob-crosscli only by convention ("the facilitator wrote nothing there"; checklist: "No commit to avowry while a voter runs").

The card can close with this as a stated limit. Window 230-scope-3 adopted the Done checks, and the remaining work can be separated from them. The follow-up is a **bug card**, not a retro experiment as the gate's (e) says. Under D183 this is a defect in the tooling: collisions are bugs under the flaky-is-a-bug ruling, and it is the exact behaviour this card names. Suggested shape:
- Title: "a second writer on one checkout is refused across processes".
- Mechanism: a writer lock file in the checkout's git dir, like the vote lock and with the same stale-lock rule.
- Who takes it: Writers, the facilitator's helpers, and hand-run seats.
- Enforcement: a pre-commit and pre-push hook, so that a plain `git commit` or `git push` is refused, with a named reason, while another writer or a live vote holds the lock. The hook also closes check 3 for hand-run writes.
- Done checks to reproduce: the 19:46 race, and a plain-git commit during a vote.
- Scope also covers stash_leftovers.

## 3. Deviations: honest? complete?

The eight deviations recorded are honest. The two product owner directions are recorded with their reasons. Window 230-scope-3's dropped OBJECTION is named in its decision text and carried into red 1d (the real-history test at test_writer_isolation.py:725). These points are understated:
- **Left-over seat worktrees and branches.** mob-crosscli still holds 10 `card230-*` branches and 15 card-230 worktrees under the scratchpad: 11 seat worktrees and 4 full-suite copies (`git branch -a`, `git worktree list`). These break the working agreement this card added ("No per-seat worktrees, branches or copies"). The record does not mention them, and the gate's safety section implies there are none.
- **Stale seat entries.** The red and green seat entries say "in its own worktree" with times of 17:34 to 18:10 and 18:42 to 19:05 UTC. Red 3b, red 1d and green 1c came hours later, and red 1d and green 1c were committed directly on main.
- **Gate evidence quality.** The facilitator's note corrected only the gate's run note. Two other faults stand:
  - The gate's line citations come from before green 1c and are wrong at 1732a82. It cites facilitator.py:1271-1321, 1297-1308 and 1338-1370, and decider.py:338-396. The actual code is seat_cwd at 1212, ship_push at 1217 and hold_vote_lock at 313-344.
  - The gate decided checks 2 and 3 from the record, not from its own run. Its safety section also claims the record "explicitly notes no delete/stash actions", and the record does not.
- **Source case.** The record calls the stray edit and the 19:46 race "an instance of this card's defect" but does not say that the shipped fix would not have prevented them (section 2).
- **Brief left as it was.** Green 1c says the role briefs never mention worktrees, which is true. But core/roles/devops-engineer.md:7 still tells the ship seat to `pull --rebase --autostash` and push, and only a prompt sentence overrides it (facilitator.py:1340-1341).

## 4. Conditions for the closing record

1. State that check 1 is met by ruling for its push-race half (section 1). State the limits from section 1 and the source-case limit from section 2. The shipped lock covers writers inside one CardRun process, plus a Writers write during a single live vote.
2. File the bug card from section 2 and name the source incidents and both recurrences in it. Remove "a process matter for the retro" from the gate entry, or mark it superseded by that card.
3. Add the understated deviations from section 3. List the left-over `card230-*` branches and worktrees as cleanup the product owner or facilitator owes. This seat deletes nothing.
4. Amend card 297's Done check 2 ("commits in its own worktree"). It now conflicts with the working agreement and should read "commits on main in the one checkout under the single-writer lock".
5. Correct the seat entries' worktree wording and times for red 3b, red 1d and green 1c.
