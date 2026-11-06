# Re-rank 25 evidence pack (facilitator; generated 2026-11-06T18:23Z at HEAD b7d82ee)

Issue-text snapshots (gh issue view) of the cards the ordering rests on, committed so the proposal can be checked from git alone.

### Card 298
Title: (298) A second decision run on the same checkout never votes unprotected
State: OPEN

<!-- phrase-card:298 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** mob-defaults (mob-crosscli: decider.py's vote lock, hold_vote_lock)
- **Source:** card 230's green 3 (bf3e4ab) named it in its own body, and the pre-ship gate (.mob/records/230/preship-gate.md, 3c) judged it a follow-up bug, not a blocker for card 230's Done check 3. decider.py creates the vote lock (mob-decider.lock in the real checkout's git dir) for one run's vote interval. A second decider run that starts while a live run holds the lock does not take it; when the first run releases the lock, the second run's vote continues with no lock, so a facilitator write during the rest of it is no longer refused and that vote can see HEAD move.
- **Depends on:** card 230 (the vote lock)
- **Actor:** decider.py's vote lock
- **Given:** two decider runs on the same checkout whose vote intervals overlap
- **Observes:** the checkout stays locked until the last overlapping vote ends (for example a holder count or one lock entry per run), and each run's record still carries equal head_start and head_end
- **Fails with:** a facilitator write refused with WriterRefused(owner="decider") at any moment while any vote on that checkout is live
- **Invariants:** card 230's tests pass; a stale lock whose process is gone is still taken over; voters stay read-only on the one checkout (working agreement: parallelism only for voting)
- **Enables:** card 216 (the default templates carry backends that work for every enabled driver)
- **One observable behaviour:** while any vote on a checkout is live, no facilitator write lands there.
- **Done checks (at most 3, each failing on partial work):**
  1. With run A holding the lock and run B starting before A ends, a facilitator commit after A ends and before B ends is refused. ZOMBIES to be written by QA at ready.
  2. When both runs end, the lock is gone and a facilitator write succeeds. ZOMBIES to be written by QA at ready.
  3. A crashed run's lock entry is still taken over without blocking a live run's protection. ZOMBIES to be written by QA at ready.

## Context

- Filed by the facilitator on 2026-11-05 from card 230's pre-ship gate; a decision-vote safety defect, kept as a bug (D183).


### Card 299
Title: (299) A second writer on one checkout is refused across processes
State: OPEN

<!-- phrase-card:299 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug (flaky-is-a-bug; collisions are bugs)
- **Repository:** mob-defaults (mob-crosscli: facilitator.py Writers, the facilitator's helpers, a git hook) and avowry process tooling
- **Source:** card 230's sign-off (.mob/records/230/signoff-ctx6.md, section 2, condition 2). Card 230 shipped a single-writer lock that lives in one CardRun process's memory (facilitator.py Writers): it serialises writers inside an automated run, but a hand-run facilitator and seats writing one checkout outside a CardRun are not refused, which is the case in card 230's own source (three index-lock collisions and push races between 2026-10-30 and 2026-10-31, the swept staged file in 3c82909). It recurred twice during card 230 itself: the ready seat wrote over card 290's committed proposal in the shared avowry checkout, and at 19:46 UTC on 2026-11-04 two facilitator processes raced a push (cannot lock ref refs/heads/main). The vote lock binds only writes that go through Writers; a plain git commit during a vote is detected afterwards (exit 5), not refused.
- **Depends on:** card 230 (Writers and the vote lock)
- **Actor:** any writer to a checkout: Writers, the facilitator's helper scripts, and seats
- **Given:** a writer lock file in the checkout's git dir, taken and released like card 230's vote lock and with the same stale-lock rule
- **Observes:** a second writer, in any process, is refused with a named reason (checkout, owner, writer, operation) before it writes, and a plain `git commit` or `git push` is refused by a pre-commit and pre-push hook while another writer or a live vote holds the lock
- **Fails with:** a refusal that names the holder, instead of an index-lock collision, a ref-lock race or a swept staged file
- **Invariants:** card 230's tests pass; all writing stays serial on main in the one checkout and votes stay read-only (working agreement); a lock whose process is gone is taken over
- **Enables:** card 216 (the default templates carry the one-writer rule)
- **One observable behaviour:** two writers in two processes on one checkout never both write; the second is refused with the holder's name.
- **Done checks (at most 3, each failing on partial work):**
  1. Two processes on one checkout: the second writer's commit is refused before any git write while the first holds the lock, and succeeds after release. ZOMBIES to be written by QA at ready.
  2. A plain `git commit` and a plain `git push` are refused by the hook while a writer or a live vote holds the lock. ZOMBIES to be written by QA at ready.
  3. A stale lock (its process gone) is taken over, and a lone writer is never refused. ZOMBIES to be written by QA at ready.

## Context

- Filed by the facilitator on 2026-11-05 under card 230's sign-off condition 2; a collision defect, kept as a bug (D183). It supersedes the pre-ship gate's note that this was "a process matter for the retro".


### Card 304
Title: (304) The two card-214 readiness tests pass without the thread-pool floor on the development machine
State: OPEN

<!-- phrase-card:304 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** avowry, tests/Phrase.Acceptance.Tests
- **Source:** card 217's sign-off (.mob/records/217/signoff-ctx6.md) and its run-pair record (.mob/records/217/run-pairs.json). With tests/Phrase.Acceptance.Tests/Support/ThreadPoolFloor.cs removed (commit d6e8f26), the first of two consecutive full local runs failed two tests outside the three owned ones: GeneratedApiBoundAddress.I_readiness_comes_from_the_childs_own_output_before_any_connection_and_not_before_the_gate (20 s) and GeneratedApiHazards.Guard_two_parallel_starts_get_distinct_base_urls (32 s), the card-214 starvation symptom; the second run passed, and CI passed both no-floor attempts. Card 217 converted the seven synchronous process waits to an asynchronous runner and kept the floor with a recorded reason, so the remaining cause is not identified. The harness in Support/GeneratedApi.cs is already asynchronous. Untested candidates: synchronous Process.Start (23 call sites in the project), Thread.Sleep and synchronous file I/O (27 files), and CPU or disk saturation from parallel dotnet builds (not starvation at all).
- **Depends on:** card 217 (the shared async runner, the guard and the run-pair record)
- **Actor:** the acceptance suite run by dotnet test on the Windows development machine
- **Given:** the full acceptance project running without ThreadPoolFloor, repeated
- **Observes:** the cause of the first-run failures is named from evidence (a trx or thread dump of the two tests, a probe, or a measured pool state), and fixed; two consecutive full local runs give identical outcome lists without the floor
- **Fails with:** a failing test named with the pool or load state that explains it, instead of a bare timeout
- **Invariants:** every acceptance assertion is unchanged; no timeout is raised to hide the problem; card 217's guard and run-pair checker stay green
- **Enables:** card 217's observable (a test's result does not depend on the load beside it) and retiring the floor
- **One observable behaviour:** the floor can be removed and two consecutive full local runs still agree.
- **Done checks (at most 3, each failing on partial work):**
  1. The cause is named with the evidence (the command and its output), including whether it is thread-pool starvation at all. ZOMBIES to be written by QA at ready.
  2. The corrective change is committed and the guard from card 217 extended if the cause is a new blocking form. ZOMBIES to be written by QA at ready.
  3. With ThreadPoolFloor.cs removed, two consecutive full local runs and two consecutive CI runs at the same head give identical outcome lists (card 217's checker), and the floor stays removed. ZOMBIES to be written by QA at ready.
- **Not in this card:** raising timeouts; changing assertions; the three F-065 tests (cards 162 and 303).

## Context

- Filed by the facilitator on 2026-11-06 from card 217's sign-off condition 2; a flaky test is a bug (D183).


### Card 305
Title: (305) A Codex build seat's commands go through the delete scan
State: OPEN

<!-- phrase-card:305 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** tooling (follow-up of card 297 and the product owner's 2026-11-06 decision)
- **Repository:** mob-defaults (mob-crosscli: command_source.py, core/backends.json, delete_guard.py)
- **Source:** the product owner decided on 2026-11-06 that Codex build seats run in full-access mode so they can commit (.mob/records/297/bypass-reversal-2026-11-06.md), with "all existing safety guards still apply". The delete scan reads the commands a seat executed from a per-driver command source (core/backends.json `command_source`): claude, copilot and muse have one; codex is `none`, so a Codex seat's commands are not scanned and its record entry reads `no command source`. `codex exec --json` prints each executed command as an item.completed event of type command_execution with its command text and exit code (seen in card 297's follow-up runs), which is a candidate source.
- **Depends on:** card 297 (the command probe and placement), card 228 (the scan for hidden, encoded and file-content forms)
- **Actor:** the delete scan, run by the facilitator on a seat's executed commands after the seat's turn, and the record check that requires a scan entry for every seat
- **Given:** a Codex build seat that executed commands in a card's checkout
- **Observes:** the seat's executed commands are read from a command source for Codex (for example the command_execution events of `codex exec --json`, stored with the seat's output), the scan judges them with the same rules as for the other drivers, and the seat's record entry shows the scan result and the command count
- **Fails with:** a scan finding that names the command, the rule and the seat (for a delete on a variable path, a recursive delete, a hidden or encoded form), or `no command source` when no event stream was captured, which still fails the record check
- **Invariants:** the Codex output parsing the decider and gates do today is unchanged (the reply is still extracted as before; the JSON stream goes to a file); the read-only and voting invocations may stay as they are; no scan rule is weakened
- **Enables:** card 216 (the default templates carry backends that work for every enabled driver; here, Codex back in build seats with the delete scan covering it)
- **One observable behaviour:** a Codex build seat that ran a recursive delete on a variable path is flagged by the scan like a Claude seat.
- **Done checks (at most 3, each failing on partial work):**
  1. A Codex command source reads the command_execution events of a captured `codex exec --json` stream and yields the executed commands with their order. ZOMBIES to be written by QA at ready.
  2. The delete scan flags a seeded Codex stream that contains a recursive delete on a variable path, a hidden form and a delete written into a file and run later, and passes a clean one. ZOMBIES to be written by QA at ready.
  3. A Codex build seat's record entry carries the scan result and command count, and the record check fails a Codex build seat without them. ZOMBIES to be written by QA at ready.
- **Not in this card:** the grok and agy drivers; changing the sandbox mode; the single-writer lock.

## Context

- Filed by the facilitator on 2026-11-06 from the 297 follow-up records; until it lands, or until the product owner says to proceed without the scan, Codex is not assigned a build seat.


### Card 306
Title: (306) The zero-harness-tests diagnostic is capped like the other failure summaries
State: OPEN

<!-- phrase-card:306 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** avowry (tools/Phrase.BenchCs; product tests: CSharpBaselineAcceptance)
- **Source:** card 303's sign-off (.mob/records/303/signoff-ctx6.md, condition B). Card 303's green (commit 30c9e84) made a run with a trx but too few harness tests return `tests_did_not_run: ` followed by `output.Trim()`, the whole captured `dotnet test` output. `CompactFailureSummary` (the path for a missing trx) caps its text at 600 characters. The new text is uncapped, is returned on each of up to three attempts, and now also applies to non-endpoint tasks (their `ModelTests == 0` branch keeps the bare label).
- **Depends on:** card 303
- **Actor:** a developer or the benchmark harness reading a result's `diagnostics`
- **Given:** a verification run whose trx has fewer harness tests than outcomes and a long captured output
- **Observes:** the diagnostic is `tests_did_not_run: ` followed by a bounded excerpt of the captured output that keeps the host error lines (for example the "Framework: ... not found" line)
- **Fails with:** a result whose diagnostics exceed the bound, or an excerpt that drops the host's error line
- **Invariants:** the bare `tests_did_not_run` for a non-endpoint run with zero model tests is unchanged; card 303's test (marker after the prefix) still passes; no assertion is weakened
- **Enables:** card 216 (the default templates carry backends that work for every enabled driver; here, a result JSON that stays small when the host's output is long)
- **One observable behaviour:** a very long captured output makes a bounded diagnostic that still names the host error.
- **Done checks (at most 3, each failing on partial work):**
  1. A fake dotnet that prints more than the bound produces a diagnostic no longer than the bound plus its prefix. ZOMBIES to be written by QA at ready.
  2. The host's error line survives the cap when it is the last line of a long output (and when it is the first). ZOMBIES to be written by QA at ready.
  3. Card 303's marker test and the non-endpoint bare-label test still pass on Windows and Linux. ZOMBIES to be written by QA at ready.
- **Not in this card:** changing the 600-character cap of CompactFailureSummary; the sandbox or seat process.

## Context

- Filed by the facilitator on 2026-11-06 from card 303's sign-off; a defect introduced by a signed-off change, kept as a bug (D183).


## Head-size guard (re-rank 21: at most 4 bugs above the first product card)
Bugs at the head today: 298, 299, 304 (3); card 306 would be the 4th; 305 is tooling and 196 is the first product card (tests rename).
