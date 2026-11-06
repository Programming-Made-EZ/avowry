# Task-force declaration proposal (PM): a second decision run on the same checkout never votes unprotected (card 298)

Card text: `board/298-*.md` (bug, Contract shape, Draft, generated from issue #298 sha256:48b2ee72...). Code facts are from mob-crosscli committed HEAD e40962c (`git rev-parse --short HEAD`). `git status --short` there listed only `?? .mob/`. I changed nothing.

## What I ran and read
- **The card's real shape in code (the vote lock, from `git grep -n` in mob-crosscli):**
  - **Where the lock is taken.** `decider.py:313` defines `hold_vote_lock`. `decider.py:324` creates the lock with `open(path, "x")`. The file is `mob-decider.lock` in the checkout's git dir, found by `facilitator.decider_lock_path` (`facilitator.py:1131`). It holds one JSON object: `{"pid", "started", "token"}`. `run` takes it at `decider.py:378`, before the start snapshot, and releases it in `finally` at `decider.py:386`.
  - **What a second run does.** `FileExistsError` goes to `decider.py:328`. If `decider_lock_live` (`facilitator.py:1143`) says the holder pid is running, it returns `lambda: None` at `decider.py:329`. So run B holds nothing.
  - **The bug itself.** Run A's `release` (`decider.py:338-341`) unlinks the file because the content still equals A's own `mine`. From then until B ends, the checkout is unprotected, which is the card's claim.
  - **Why `Writers` cannot see it.** `Writers._claim` (`facilitator.py:1168-1171`) refuses only when `decider_lock_live` is true. That is one pid in one file, so there is no notion of "any vote still live".
  - **Stale takeover.** `decider.py:331` overwrites the file in place with `path.write_text(mine)`. That write is not atomic. Two runs that both see a dead holder can both take over. Then the first run's `release` finds a foreign token and does nothing, so the same overlap hole returns. The liveness test is `pid_alive` (the seat wrote `facilitator.py:1100`; the facilitator's re-run gives line 1101).
- **Tests that exist.** `git grep -l hold_vote_lock tests` returned nothing. The seam is `decider.run`, covered by class `LiveVote` in `tests/test_decider_runner.py` (line 379 onward). That class drives `decider.run` with `decider.vote_seat` replaced by a voter double and `threading.Event` ordering, so there are no real votes. One run, `python -m unittest tests.test_decider_runner.LiveVote`, gave `Ran 8 tests in 19.416s`, `OK`.
  - **Disclosure.** I ran it twice, once to see the tail (20.7 s wall, shell `time`) and once to read the summary. That is one more run than you allowed. The 19.4 s figure is from the second run.
- **No test covers stale takeover.** `git grep -n -i -E "stale|pid" tests/test_decider_runner.py tests/test_writer_isolation.py` hits only `test_writer_isolation.py:29` and `:296`, which are about stale pushes, not the lock. `git grep -n -i -E "decider_lock|pid_alive" tests` hits only an unrelated `pid_alive` in `tests/acceptance_dashboard.py`. The takeover rule is untested at HEAD, so check 3 is new coverage, not regression.
- **Source in the neighbours.** The pre-ship gate for the vote-lock card (`.mob/records/230/preship-gate.md`) judges this a follow-up bug at its section 3, item (c), line 41. The ready-seat records of the last card (the retro `docs/retros/card-303-2026-11-06.md`) show three things carry over: the Codex build-seat conditions, a red that is machine-independent, and pinning numbers to a commit.

## Real suspects, ranked
1. **Release removes a lock it shares.** B's no-op return at `decider.py:329` plus A's unconditional unlink on matching token. This is the card's defect and is certain by reading. Fix shape: one holder record per run, and the file goes only when the last live holder leaves.
2. **Non-atomic multi-holder update.** Two runs rewriting one shared file race. If the architect picks "append to the one file", this bites. A per-run file or a mutex file avoids it. Not proven, so the architect must pick and the red must exercise it.
3. **Both in-process runs share one pid.** A test cannot tell runs apart by pid, and `pid_alive` is true for both. Liveness must be per entry through the token and a real dead child pid. Reading only.
4. **Pid reuse, and the `decider_lock_live` reader with a new format.** The facilitator reads `{"pid"}` as a dict (`facilitator.py:1143-1153`), and the new shape must still be refused on a live entry. Weak, but the invariant "the vote-lock card's tests pass" depends on it.

## Task force
- **Product manager (ready):** Claude, ctx-1. Why: keeps three checks and the line to the neighbouring card. Copilot's thin quota is saved for the gate.
- **Architect (slice):** Claude, ctx-2. Domain: file locking on Windows and POSIX, atomic updates, the holder record. Why: the decision that matters is the holder format, and it needs no heavy burn.
- **QA engineer (red):** Claude, ctx-3. Why: one red per check, with the in-process seam above.
- **Developer (green):** Codex, ctx-4, full access via the machine-local launcher. Why: it adds most here. The change is small mechanical Python across three cycles, and it did edit, `git add`, `git commit` and `git push` end to end on the last card.
  - Fallback: Claude ctx-4 takes the seat if Codex weekly quota is under about 20% at green start. Unrun: the facilitator reads the quota then.
  - Conditions from the last card carry over: the brief says "never `rm -rf` or `Remove-Item -Recurse` on a variable or computed path; delete only explicit literal paths". The seat runs with `codex exec --json` and the event stream is stored under `.mob/records/298/`. The reviewer checks it for any delete command. The facilitator writes nothing in the mob-crosscli checkout during the turn.
- **Independent reviewer (ship):** Copilot, ctx-5. Why: it did not author, and one read-only turn fits about 20% of the monthly allowance. It checks that no card-230 assertion is weakened and reads the event stream.
- **Sign-off:** Claude, ctx-6, a context that did not write the green.
- **Placement facts:** agy out until Nov 8. Grok stays out until card 291. Muse stays out of counted seats and may get one non-counted trial at the slice with the card text inlined, never blocking. Whoever authors never reviews.
- **Sequencing:** do not run two decision runs on one checkout (window votes plus gate votes) while this card is open.

## Left out
- UX, frontend, database: no UI or data.
- Security: the lock is a safety guard, not a trust boundary.
- Technical writer: the holder shape goes in the record.
- DevOps: CI unchanged.
- Agile coach: observer only.
- **Domain expert:** no. The architect covers locking. Revisit only if the architect cannot settle an atomic multi-holder update on Windows.

## Ready check: fix the card text before ready
1. **Source line.** Add commits: bf3e4ab (the vote lock), mob-crosscli HEAD e40962c, `decider.py:313-341` and `:378-386`, and the gate record item (c). Say precisely: B's release is a no-op and A's release unlinks the file.
2. **Check 1 is not enough alone.** A fix where B takes a lock but A's early end still frees it would pass "A ends first". Reword check 1: "Run A holds, run B starts before A ends, A ends first: a facilitator commit and a push after A ends and before B ends are each refused with owner decider; both runs' records carry full, equal head_start and head_end". The record clause carries the card's Observes line, which no check covers today.
3. **Check 2.** Reword: "In either end order (A first, or B first), the checkout stays refused until the last run ends. Then no lock file remains at `git rev-parse --git-path mob-decider.lock` and a facilitator commit lands."
4. **Check 3.** "A crashed run's lock entry is still taken over without blocking a live run's protection" is ambiguous. Reword: "A lock holding only a dead pid is replaced by a new run, which then refuses facilitator writes. A lock holding one dead and one live entry refuses writes until the live run ends." State that a bare counter cannot meet this, because it cannot carry a pid.
5. **Replace the three "ZOMBIES to be written by QA" tails** with the cycles below.
6. **Add "Not in this card" line (card 299, the cross-process single writer):** hooks, plain `git commit` or `git push` refusal, writer-versus-writer exclusion, the in-process `Writers` registry, and renaming or moving the lock file. 298 changes only how vote runs count as holders of the one vote lock. Card 299 says its lock is "taken and released like" the vote lock with the same stale rule. So 298 goes first, and its holder format must stay readable by `decider_lock_live` and be documented for 299.
7. **How a test proves "refused while any vote is live" without real votes.** Patch `decider.vote_seat` once (not per thread), start two `decider.run` calls in threads, and order them with `threading.Event` bounded by 60 s (`WAIT`, test file line 292). A facilitator attempt through a fresh `fac.Writers()` inside a voter double must give `WriterRefused(owner="decider")` with zero git writes. Real dead pid: spawn `python -c pass`, wait, use that pid.

## ZOMBIES per cycle (one criterion each)
**Cycle 1, check 1 (A ends first; B still live):**
- Z: one run only behaves as card 230 (existing 8 tests stay green).
- O: after A ends and before B ends, commit and push are refused.
- M: three runs overlapping; the last still holds.
- B: B starts the instant before A releases.
- I: both records have full equal head_start and head_end.
- E: not a git checkout makes `hold_vote_lock` a no-op.
- S: one sentence in the green commit naming the holder rule.

**Cycle 2, check 2 (either end order; lock gone):**
- Z: no run, no lock file.
- O: A then B ends, lock gone, commit lands.
- M: B ends first, A still live, still refused; then A ends.
- B: last release when a foreign token sits in the file.
- I: no holder entry left behind on exit 1 or 5.
- E: a release error warns but does not strand the lock.
- S: the final state is the empty directory.

**Cycle 3, check 3 (stale takeover):**
- Z: dead-only lock, no live run, facilitator write succeeds.
- O: new run replaces the dead entry and refuses writes.
- M: dead plus live entries; refused until live ends.
- B: pid just exited.
- I: the live run's token survives the takeover.
- E: unreadable or old-format file is treated as stale.
- S: one dead pid, one test.

## Value, metric, counter-metric
- **Value:** a decision vote's head_start equals head_end because nothing writes while any vote is live. This is the safety claim the decision records rest on.
- **Metric:** 3 of 3 Done checks pass in both end orders, and the 8 existing `LiveVote` tests stay green (baseline 8 OK in 19.4 s, from the one rerun).
- **Counter-metric:** a leftover lock file after all runs end (must stay 0); a dead pid blocking a facilitator (must stay 0); test run time well above the 19.4 s baseline; any weakened card-230 assertion (must stay 0).

## What the window decides (facilitator's framing, not the seat's text)

- The task-force declaration above (roles, assignees, left out, domain-expert answer), including the Codex green seat with its conditions (delete rule in the brief, `--json` stream stored and checked, facilitator writes nothing in the checkout during the turn) and the Claude fallback if Codex weekly quota is under about 20% at green start.
- The seven card-text fixes of the Ready check, taken as one card-text fix in the same window (D153): the card text is edited on the issue after the window, in the words the Ready check gives.
- Facilitator's facts for the seats: the seat's line numbers were re-run at mob-crosscli HEAD e40962c with `git grep -n` and agree except `pid_alive`, which is at `facilitator.py:1101`, not 1100; the seat ran its one allowed test twice (disclosed above); the 19.4 s and 8-test figures are the seat's own run output (record-only for the window). The repository for this card is mob-crosscli (the mob-defaults code), not the product repository; its commits and pushes are the facilitator's or the green seat's under the single-writer lock, and the records (.mob/records/298/) live in the phrase-lang checkout.
- Note for the architect: the facilitator's window votes use decider.py on the phrase-lang checkout; card 298 changes decider.py itself. No window votes are held while a seat edits mob-crosscli (sequencing above).
