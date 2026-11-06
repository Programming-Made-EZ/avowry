# Re-rank 26 evidence pack (facilitator; generated 2026-11-06T23:22Z at HEAD 6624394)

Issue-text snapshots (gh issue view) of the cards the ordering rests on, committed so the proposal can be checked from git alone. Current order (rerank dry run): 299 (96), 306 (97), 304 (98), 305 (99), 196 (100), 197, 266, 302, 300, 301 ... ; 307, 308-312, 313-315 and 316 are unranked.

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


### Card 307
Title: (307) Two overlapping decision runs on one checkout do not fail each other's seat-use history check
State: OPEN

<!-- phrase-card:307 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** mob-defaults (mob-crosscli: decider.py's integrity check of .mob/records/seat-use.jsonl)
- **Source:** card 298's red and green (.mob/records/298/overlap-seat-use-failure.txt, mob-crosscli commits 0cd4830, 4b05fa6, fd4c702). With the vote lock now held until the last overlapping run ends, two decider runs on one checkout can overlap safely, but each ends with exit status 5 and a record whose integrity is `{"ok": false}` because "the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is missing or changed)": each run appends its own seat-use lines, and the other run's lines make this run's append-only check fail. The QA seat first saw the stderr line before any change of card 298's green.
- **Depends on:** card 298 (the overlap holder format)
- **Actor:** decider.py's seat-use history check (the run's integrity record)
- **Given:** two decider runs on the same checkout whose vote intervals overlap, both writing seat-use history
- **Observes:** each run exits 0 when its own lines were appended and nothing of the history was removed or changed, and each record carries `integrity` ok with equal full head_start and head_end
- **Fails with:** a finding that names the line, the run and the check when a seat-use line is removed or changed, and still exit 5 then
- **Invariants:** a run that really loses or changes a history line still fails (exit 5, integrity not ok); card 230's and card 298's tests pass; no history line is rewritten to make the check pass
- **Enables:** card 216 (the default templates carry backends that work for every enabled driver; here, decision runs that can overlap on one checkout without false integrity failures)
- **One observable behaviour:** two overlapping decision runs both report integrity ok, and a run that truly loses a seat-use line still fails.
- **Done checks (at most 3, each failing on partial work):**
  1. Two overlapping runs that each append their own seat-use lines both exit 0 with integrity ok. ZOMBIES to be written by QA at ready.
  2. A run during which a seat-use line is removed or edited still exits 5 and names the line. ZOMBIES to be written by QA at ready.
  3. The seat-use history after both runs holds every line of both runs, in append order, with no line rewritten. ZOMBIES to be written by QA at ready.
- **Not in this card:** the vote lock (card 298), cross-process writer exclusion (card 299), changing what the history records.

## Context

- Filed by the facilitator on 2026-11-06 from card 298's red 1; a defect found while testing the overlap, kept as a bug (D183).


### Card 313
Title: (313) A decision run that starts as another run ends can be left without a vote lock
State: OPEN

<!-- phrase-card:313 -->
<!-- phrase-card:313 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** mob-defaults (mob-crosscli: decider.py, facilitator.py)
- **Source:** card 298's sign-off (.mob/records/298/signoff-ctx6.md, rerun probes against mob-crosscli HEAD 1590679).
- **Depends on:** card 298 (the holder format)
- **Actor:** the vote lock's registration step in decider.py hold_vote_lock, run by two overlapping decision runs
- **Given:** run B calls hold_vote_lock just as run A releases and removes the now-empty holders directory
- **Observes:** run B always ends up with a holder file while it votes, so a facilitator commit or push is refused until B ends
- **Fails with:** B's open of its temporary holder file fails after A's rmdir, the OSError branch returns a no-op, B holds nothing and decider_lock_live is false while B votes (the sign-off simulated it with an rmdir inside a patched os.open)
- **Invariants:** card 230's and card 298's tests pass; decider_lock_live stays read-only; no live run's holder is ever removed
- **Enables:** card 299 (the cross-process single writer reads the same holders)
- **One observable behaviour:** run B always ends up with a holder file while it votes, so a facilitator commit or push is refused until B ends.
- **Done checks (at most 3, each failing on partial work):**
  1. Run B's registration retries or otherwise survives a release's rmdir and always leaves a live holder file. ZOMBIES to be written by QA at ready.
  2. With A releasing at every point between B's mkdir and B's replace, a commit during B's vote is refused each time. ZOMBIES to be written by QA at ready.
  3. The card 298 tests and card 230 tests pass; no holder is left after both runs end. ZOMBIES to be written by QA at ready.
- **Not in this card:** pid reuse (a reused pid keeps a dead holder alive: fail-safe, as card 230 was; recorded in the card 298 sign-off); cross-process writer exclusion (card 299).

## Context

- Filed by the facilitator on 2026-11-06 from card 298's sign-off; a defect, kept as a bug.


### Card 314
Title: (314) A holder's temporary file left by a crashed run is never cleaned up
State: OPEN

<!-- phrase-card:314 -->
<!-- phrase-card:314 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** mob-defaults (mob-crosscli: decider.py, facilitator.py)
- **Source:** card 298's sign-off (.mob/records/298/signoff-ctx6.md, rerun probes against mob-crosscli HEAD 1590679).
- **Depends on:** card 298 (the holder format)
- **Actor:** the vote lock's holders directory sweep and release in decider.py hold_vote_lock
- **Given:** a run crashed between creating its <token>.tmp and renaming it, leaving the .tmp file in mob-decider.lock.d
- **Observes:** a later run removes the stale .tmp (older than a stated number of seconds, never a live run's) and when all runs end the directory is gone
- **Fails with:** the sweep skips every name ending .tmp, so the file survives every later run, the directory is never removed, and a test of the form 'no holders directory remains' fails forever
- **Invariants:** card 230's and card 298's tests pass; decider_lock_live stays read-only; no live run's holder is ever removed
- **Enables:** card 299 (the cross-process single writer reads the same holders)
- **One observable behaviour:** a later run removes the stale .tmp (older than a stated number of seconds, never a live run's) and when all runs end the directory is gone.
- **Done checks (at most 3, each failing on partial work):**
  1. A .tmp file older than the stated age is removed by the next run's sweep. ZOMBIES to be written by QA at ready.
  2. A .tmp file younger than that age, belonging to a run still registering, is never removed. ZOMBIES to be written by QA at ready.
  3. After the last run ends the directory is gone even when a stale .tmp had been left. ZOMBIES to be written by QA at ready.
- **Not in this card:** pid reuse (a reused pid keeps a dead holder alive: fail-safe, as card 230 was; recorded in the card 298 sign-off); cross-process writer exclusion (card 299).

## Context

- Filed by the facilitator on 2026-11-06 from card 298's sign-off; a defect, kept as a bug.


### Card 315
Title: (315) A decision run that cannot register its vote lock starts without any warning
State: OPEN

<!-- phrase-card:315 -->
<!-- phrase-card:315 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** mob-defaults (mob-crosscli: decider.py, facilitator.py)
- **Source:** card 298's sign-off (.mob/records/298/signoff-ctx6.md, rerun probes against mob-crosscli HEAD 1590679).
- **Depends on:** card 298 (the holder format)
- **Actor:** the vote lock's registration step in decider.py hold_vote_lock when the holders directory cannot be created or written
- **Given:** the git directory path for mob-decider.lock.d is a regular file or is not writable
- **Observes:** the run refuses to start, or starts with a loud stderr warning and a note in the decision record that the checkout was unprotected
- **Fails with:** hold_vote_lock returns a no-op, decider_lock_live is false for the whole vote, and nothing is printed (the sign-off made the .d path a regular file to show it)
- **Invariants:** card 230's and card 298's tests pass; decider_lock_live stays read-only; no live run's holder is ever removed
- **Enables:** card 299 (the cross-process single writer reads the same holders)
- **One observable behaviour:** the run refuses to start, or starts with a loud stderr warning and a note in the decision record that the checkout was unprotected.
- **Done checks (at most 3, each failing on partial work):**
  1. When the lock cannot be registered the run exits non-zero before any seat votes, or prints the warning and records it. ZOMBIES to be written by QA at ready.
  2. A normal run on a writable checkout is unchanged and prints nothing extra. ZOMBIES to be written by QA at ready.
  3. A non-git directory still holds no lock and starts normally. ZOMBIES to be written by QA at ready.
- **Not in this card:** pid reuse (a reused pid keeps a dead holder alive: fail-safe, as card 230 was; recorded in the card 298 sign-off); cross-process writer exclusion (card 299).

## Context

- Filed by the facilitator on 2026-11-06 from card 298's sign-off; a defect, kept as a bug.


### Card 308
Title: (308) The product owner's rulings and the team's replies are recorded where a check can verify them
State: OPEN

<!-- phrase-card:308 -->
<!-- phrase-card:308 -->

- **Status:** Draft
- **Shape:** Story
- **Type:** tooling (process change, filed as a story under the product owner's 2026-11-06 ruling)
- **Source:** decisions D191 and D192 (adopting the product-owner inbox) failed the strict count twice; D192's remaining No (the audit lens) was an evidence concern: the inbox file sits outside the repository, so nothing in git proves an entry was read, acknowledged or routed. The product owner ruled on 2026-11-06 that D192's options (a), (b) and (c) are all declined and this story is filed instead, with the audit lens's evidence concern in its acceptance criteria. Shapes weighed so far, not decided: a committed log plus an in-repository snapshot and a checker script (D192 C2b, C6, C6b).
- **Depends on:** none
- **Story:** As Steve, I want each of my rulings, and the team's reply to it, recorded in a repository file that a mechanical check verifies, so that I can see in git what was decided, when it reached the team, and where it went, without trusting a file outside the repository.
- **One user-observable outcome:** Steve opens one committed log and sees, per ruling, when the team first read it, when it was acknowledged and where it went, with the check passing.
- **Acceptance criteria (Gherkin, at most 3, each testable and failing on partial work):**
  1. Scenario: a ruling is acknowledged. Given a ruling filed in the inbox, When the facilitator acknowledges it at a boundary, Then one line is committed to a repository log with the ruling's identity, read time, acknowledgement time, boundary and destination, and the destination exists in the repository.
  2. Scenario: the check rejects a gap. Given the log and an in-repository snapshot of the inbox headings at each boundary, When a heading is shown as acknowledged but has no log line, or a log line is duplicated, later than the stated limit, or has no destination, Then the check fails and names the entry.
  3. Scenario: the evidence cannot be rewritten. Given committed log and snapshot lines, When an earlier line is edited or removed, Then the check fails against the committed history.
- **Not in this card:** moving the inbox itself into the repository (the product owner declined that option); changing who may write rulings.

## Context

- Filed by the facilitator on 2026-11-06 under the product owner's ruling of the same day: process changes are filed as story issues with a persona who gains the value, then refined, ranked and pulled like any card (this supersedes the process part of D183). The team refines the shape at ready; nothing here is decided.


### Card 309
Title: (309) A reviewer reads a card cold in the format that gives the fewest misreads
State: OPEN

<!-- phrase-card:309 -->
<!-- phrase-card:309 -->

- **Status:** Draft
- **Shape:** Story
- **Type:** tooling (process change, filed as a story under the product owner's 2026-11-06 ruling)
- **Source:** the product owner's 2026-11-06 question for retro: which card format serves this mob best (the Connextra story with Given/When/Then criteria, the Contract template, Gherkin throughout, or a hybrid), judged on persona and consumer-value clarity, how often seats misread cards or rework them, review and audit effort, and fit for tooling cards. It was routed to docs/retros/experiments.md and has waited for a Decider with fresh seats.
- **Depends on:** none
- **Story:** As Morgan, I want every card written in the format that a fresh-context reader misreads least, so that I can review a card or its change without reconstructing what the author meant.
- **One user-observable outcome:** Morgan reads a card in a fresh context and restates its actor, behaviour and checks correctly.
- **Acceptance criteria (Gherkin, at most 3, each testable and failing on partial work):**
  1. Scenario: a measured comparison. Given the last ten closed cards (user-facing and tooling), When each is rewritten in each candidate format and restated by a fresh-context seat, Then the report counts misreads and rework per format and names the format with the fewest for each card kind.
  2. Scenario: the decision is recorded. Given the report, When a Decider window scores the options, Then the vote, the reasoning in 3 to 5 lines and any change framed as a retro experiment with a review point are written to the closure record and reported to the product owner.
  3. Scenario: the change is trialled. Given a chosen format, When the next three cards are written in it, Then the retro compares misreads and rework against the report's baseline and keeps, revises or drops the change.
- **Not in this card:** rewriting existing cards; changing the Ready check before the decision.

## Context

- Filed by the facilitator on 2026-11-06 under the product owner's ruling of the same day: process changes are filed as story issues with a persona who gains the value, then refined, ranked and pulled like any card (this supersedes the process part of D183). The team refines the shape at ready; nothing here is decided.


### Card 310
Title: (310) The board keeps moving when one driver's quota runs out, with Muse trialled as a fourth driver
State: OPEN

<!-- phrase-card:310 -->
<!-- phrase-card:310 -->

- **Status:** Draft
- **Shape:** Story
- **Type:** tooling (process change, filed as a story under the product owner's 2026-11-06 ruling)
- **Source:** the product owner's 2026-11-06 proposal to bring Muse into the driver rotation. Muse 1.4.14 passes the one-word probe but returned no reply on card 303's read-and-write-up slice brief; the card 303 retro kept Muse out of counted seats until one more trial with the card text inlined.
- **Depends on:** none
- **Story:** As Steve, I want a fourth driver (Muse) trialled in the rotation, so that when one driver's quota is exhausted the board keeps moving without a nudge from me.
- **One user-observable outcome:** Steve sees a seat delivered by Muse, or a recorded reason it cannot be, instead of a stalled board.
- **Acceptance criteria (Gherkin, at most 3, each testable and failing on partial work):**
  1. Scenario: a trial that can fail. Given a non-counted seat on a card and its text inlined in the brief, When Muse is asked for the seat's reply, Then a non-empty reply is stored under the card's records, or the seat is recorded as a seat failure with the launch command and its output.
  2. Scenario: placement follows the result. Given two successful trial seats, When a window scores Muse's rotation placement, Then the rotation file names the seats Muse may take and the limits (no build seat until a build trial passes), and a failed trial keeps Muse out with the reason written.
  3. Scenario: quota exhaustion is survivable. Given one driver's quota exhausted, When the next counted seat is assigned, Then it goes to a driver that passed its trial, and the record names why.
- **Not in this card:** Muse in build seats; buying more quota.

## Context

- Filed by the facilitator on 2026-11-06 under the product owner's ruling of the same day: process changes are filed as story issues with a persona who gains the value, then refined, ranked and pulled like any card (this supersedes the process part of D183). The team refines the shape at ready; nothing here is decided.


### Card 311
Title: (311) A Decider vote is scored as written and the agile coach flags any deviation at the vote
State: OPEN

<!-- phrase-card:311 -->
<!-- phrase-card:311 -->

- **Status:** Draft
- **Shape:** Story
- **Type:** tooling (process change, filed as a story under the product owner's 2026-11-06 ruling)
- **Source:** the product owner's 2026-11-06 feedback: the coach should have caught the D191 deviation (an unresolved No scored as a pass on an interpretive reading); record it as an owned finding against the coach role and let the team decide how the coach catches it (a conformance item in the coach's brief, a mechanical tally check, or both). The same-day ruling: the protocol is followed exactly, a No is never reinterpreted, and a facilitator who finds it ambiguous raises a retro proposal.
- **Depends on:** none
- **Story:** As Steve, I want the agile coach to flag at the vote any departure from the Decider protocol as written, so that no proposal is scored by interpretation and I do not find the departure afterward.
- **One user-observable outcome:** Steve reads a Decider record whose coach conformance line says the count followed the protocol, or names the deviation before the result stands.
- **Acceptance criteria (Gherkin, at most 3, each testable and failing on partial work):**
  1. Scenario: the coach reads the tally. Given a finished window, When the agile coach seat is given the raw votes and the protocol text, Then its reply carries a conformance line that says conforms, or names the rule, the seat and the vote that departs from it, before the facilitator records the result.
  2. Scenario: a deviation blocks the record. Given a conformance line that names a departure, When the facilitator would record the result, Then the record is withheld until the departure is fixed or the proposal is recorded as failed.
  3. Scenario: the catch rate is visible. Given the next three Decider windows, When the retro reviews them, Then it reports how many deviations the coach caught against how many the product owner or a later check found.
- **Not in this card:** changing the Decider protocol; the mechanical tally check (its own story).

## Context

- Filed by the facilitator on 2026-11-06 under the product owner's ruling of the same day: process changes are filed as story issues with a persona who gains the value, then refined, ranked and pulled like any card (this supersedes the process part of D183). The team refines the shape at ready; nothing here is decided.


### Card 312
Title: (312) A Decider record is rejected when its result does not follow from its votes
State: OPEN

<!-- phrase-card:312 -->
<!-- phrase-card:312 -->

- **Status:** Draft
- **Shape:** Story
- **Type:** tooling (process change, filed as a story under the product owner's 2026-11-06 ruling)
- **Source:** the product owner's 2026-11-06 ruling that the protocol is followed exactly and an outlier unresolved after the amendment fails the proposal. The D191 record passed on an interpretive reading and was corrected only after the product owner caught it. The tally checker was an open process item in the card 303 retro.
- **Depends on:** none
- **Story:** As Steve, I want a check that fails a Decider record whose result does not follow from its votes under the protocol as written, so that a wrongly passed proposal cannot reach the working agreement.
- **One user-observable outcome:** Steve sees the repository check refuse a record that says passed while an outlier No is unresolved.
- **Acceptance criteria (Gherkin, at most 3, each testable and failing on partial work):**
  1. Scenario: a wrong pass is refused. Given a Decider vote file with an unresolved No after the amendment round, When the record says passed or adopted, Then the check fails and names the vote and the rule.
  2. Scenario: a right result passes. Given vote files for passed, failed and conditional outcomes that follow the protocol, When the check runs, Then it passes each, and the existing records still pass.
  3. Scenario: the check runs where the work does. Given a commit that adds or edits a Decider record, When the push is attempted, Then the same check runs and refuses the push on a bad tally.
- **Not in this card:** judging the quality of a seat's reasoning; the coach's conformance line (its own story).

## Context

- Filed by the facilitator on 2026-11-06 under the product owner's ruling of the same day: process changes are filed as story issues with a persona who gains the value, then refined, ranked and pulled like any card (this supersedes the process part of D183). The team refines the shape at ready; nothing here is decided.


### Card 316
Title: (316) The gate runs the changed area's narrow suites and checks each step's failure path
State: OPEN

<!-- phrase-card:316 -->
<!-- phrase-card:316 -->

- **Status:** Draft
- **Shape:** Story
- **Type:** tooling (process change adopted by Decider D193, filed as a story under the product owner's 2026-11-06 ruling)
- **Source:** Decider D193 (the card 298 retro pick, adopted as a three-card trial); the baseline is cards 303 and 298, where the pre-ship gate said SHIP and the sign-off then found defects the gate did not report (card 298: bugs 313 to 315).
- **Depends on:** none
- **Story:** As Morgan, I want the pre-ship gate to run the changed area's narrow test suites and to check each step's failure path, so that the defects a fresh reader would find at sign-off are found at the gate, with raw results I can re-read in git.
- **One user-observable outcome:** Morgan opens a gate record and sees every suite run for the changed files with its command, count and raw result line, and a labelled failure check for each step and each crash gap.
- **Acceptance criteria (Gherkin, at most 3, each testable and failing on partial work):**
  1. Scenario: the gate runs the narrow suites. Given a card that changed code files, When the gate brief is written, Then it carries a committed table from `git diff --name-only` of each changed file, the test files that name it and their narrow suites, and the gate record lists each suite with command, count and raw result line, or marks it blocked with the facilitator-run evidence line.
  2. Scenario: a no-red cycle is proven. Given a cycle whose new tests pass before any change, When the cycle is accepted, Then the record holds two committed mutation patches, each failing the matching test with its assertion text and each naming the Done check or stated behaviour it breaks.
  3. Scenario: the failure path is checked. Given a card that changes lock, registration or cleanup code, When the gate runs, Then the brief lists one line per step and per gap between steps, each line is labelled rerun, record-only or blocked, and a record-only or blocked line makes the verdict HOLD until the facilitator's probe output is attached.
- **Trial:** the next three cards that change mob-crosscli code; sign-off-only findings must fall to 0 on 2 of 3 cards; the retro tags each later defect to the rule that should have caught it (G1, G2, G3); counter-metrics are gate duration (reshape if over twice), padded mutations and watchdog idle time. The third card's retro decides keep, revise or drop. Owner: agile coach.
- **Not in this card:** a second independent reviewer on all lock cards; changing the sign-off seat or the Decider protocol.

## Context

- Filed by the facilitator on 2026-11-06 from Decider D193 (.mob/records/deciders/D193-votes.json, .mob/records/298/retro-pick-proposal-coach-r2.md).


### Card 196
Title: (196) Rename the two test projects to Avowry.*.Tests
State: OPEN

<!-- phrase-card:196 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** tests (S; Avowry rename 7 of 9)
- **Milestone:** Purpose: verifiability, preserved intent and reviewability
- **Source:** The product owner's naming decision (the public name is Avowry) and Decider D125 (2026-10-29).
- **Depends on:** 195
- **Story:** As Morgan, I want the test projects named after the product, so that a search for the old name finds nothing under tests/.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given tests/Phrase.Compiler.Tests and tests/Phrase.Acceptance.Tests, When they are renamed to Avowry.Compiler.Tests and Avowry.Acceptance.Tests, Then every test still runs (same total counts) and none is removed
  2. Given the scripts, CI workflow and docs that name those paths, When the card is merged, Then they use the new paths and CI is green on Windows and Linux
  3. Given tests/test_demo_readme.py and other path-bound tests, When they run, Then they pass

## Context

- Counts before and after are compared; any difference is a defect of the rename. Follow docs/CODING_GUIDELINES.md; stay green at every step; keep the change small enough to revert alone; nothing is published.

## Task force

- Author [author]: next in the rotation | why: D15 rotation authorship
- Reviewer [reviewer]: a non-author seat | why: non-author review of the diff (D15)
- Facilitator: Claude | why: verifies the diff before the gate; takeovers are logged

## Done when

```
dotnet test Phrase.sln --filter "Status!=Pending"
```


### Card 197
Title: (197) Rename the solution, CI jobs, scripts and paths to Avowry
State: OPEN

<!-- phrase-card:197 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** tooling (S; Avowry rename 8 of 9)
- **Milestone:** Purpose: verifiability, preserved intent and reviewability
- **Source:** The product owner's naming decision (the public name is Avowry) and Decider D125 (2026-10-29).
- **Depends on:** 196
- **Story:** As Morgan, I want the solution file, CI workflow names, scripts and build property names to say Avowry, so that nothing the contributor touches uses the old name.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given Phrase.sln, the CI workflow, scripts/ and Directory.Build.props, When they are renamed, Then every reference is updated in the same commit and the gate and CI are green
  2. Given a grep for Phrase over the repository, When the card is merged, Then each remaining hit is listed with its reason (the language word phrase, history, the frozen docs until card 162, or the repository name until card 198)
  3. Given the release bar, When it is read, Then its gap table marks the rename cards as done

## Context

- No history rewrite. Follow docs/CODING_GUIDELINES.md; stay green at every step; keep the change small enough to revert alone; nothing is published.

## Task force

- Author [author]: next in the rotation | why: D15 rotation authorship
- Reviewer [reviewer]: a non-author seat | why: non-author review of the diff (D15)
- Facilitator: Claude | why: verifies the diff before the gate; takeovers are logged

## Done when

```
dotnet test Phrase.sln --filter "Status!=Pending"
```


### Card 266
Title: (266) Docs and code consistently use the name Avowry
State: OPEN

<!-- phrase-card:266 -->

- **Status:** Draft
- **Shape:** Story
- **Type:** story
- **Repository:** avowry (docs and code) and mob-defaults references
- **Source:** GitHub issue #259, filed by Dispatch for the product owner: the product was renamed Phrase to Avowry on 2026-10-29 and 281 markdown files still say Phrase
- **Depends on:** none
- **Story:** As Ari, a cold-start coding agent reading the docs, I want one consistent name, Avowry, in the live docs and code, so that I use and search for the language I actually install. Observes: one consistent public name, Avowry, in live and user-facing docs and code, with history handled as the team decides (verbatim with a 'formerly Phrase' note, or renamed)
- **Enables:** none
- **One observable behaviour:** one consistent public name.
- **Acceptance criteria (at most 3, each failing on partial work):**
  1. Live and user-facing docs use Avowry; the team's decision on historical records is applied and recorded. ZOMBIES to be written by QA at ready.
  2. Code identifiers, CLI names and test names follow the team's decision with no failing test or benchmark harness. ZOMBIES to be written by QA at ready.
  3. A check (for example git grep -w Phrase over the agreed scope) finds no unexplained occurrence. ZOMBIES to be written by QA at ready.

## Context

- Carded by the facilitator on 2026-11-02 from Dispatch's issue (see Source), so the board can rank it; the original issue stays open as the source.


### Card 302
Title: (302) The C# baseline benchmark children never build concurrently
State: OPEN

<!-- phrase-card:302 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** avowry (product tests: the acceptance harness)
- **Source:** card 264's green 2b and its findings (.mob/records/264/findings.md). Card 264 found that acceptance tests starting `dotnet run` children of Phrase.Bench let each child build the shared obj output, and two concurrent builds collided (one child exited 1 with no history). It fixed the Phrase.Bench starts with one no-build launcher (tests/Phrase.Acceptance.Tests/Support/BenchLauncher.cs). The Phrase.BenchCs starts keep the old build-capable `dotnet run` and carry the same risk: CSharpBaselineAcceptance.cs (about line 870), the BenchCs arm of PrivateHoldoutResults.cs (RunArm) and the BenchCs arm of BenchmarkHarnessAcceptance.cs (RunBenchmarkArm), through BenchLauncher.CreateStartInfoFor.
- **Depends on:** card 264 (the launcher)
- **Actor:** the acceptance suite run by dotnet test, locally and in CI
- **Given:** two acceptance classes that each start a Phrase.BenchCs child while its build output is stale
- **Observes:** every Phrase.BenchCs child is started without building, in the test assembly's configuration, and missing or stale output fails clearly before launch
- **Fails with:** a launcher error naming the configuration, the expected path and the remedy, instead of a child that exits 1 on a file lock
- **Invariants:** card 264's tests pass; outcome assertions, timeouts and rows are unchanged
- **Enables:** card 099 (a flaky acceptance test hides real failures)
- **One observable behaviour:** no acceptance test child builds a benchmark project while another one runs.
- **Done checks (at most 3, each failing on partial work):**
  1. A source scan finds no BenchCs `dotnet run` start outside the shared launcher. ZOMBIES to be written by QA at ready.
  2. The launcher fails clearly on missing or stale BenchCs output, without building. ZOMBIES to be written by QA at ready.
  3. The forced concurrent-build diagnostic, run against the BenchCs launch shape, shows no anomalies. ZOMBIES to be written by QA at ready.

## Context

- Filed by the facilitator on 2026-11-05 from card 264's green 2b report; a latent flake of the same class as audit FAIL 123, kept as a bug (D183).

