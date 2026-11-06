# Re-rank 24 evidence pack (facilitator; generated 2026-11-06T05:17Z at HEAD cb79689)

Added for the audit lens's round 1 condition: the issue-text snapshots and the failing-run artifacts the ordering rests on, committed so the proposal can be checked from git alone.

## Committed failing-run artifacts (cards 217 and 303/304)
- Card 304's origin: .mob/records/217/runs/local-nofloor-1.outcomes.json (local run 1 with the floor removed, head d6e8f262f5f2a226d1569422e42d29f64c490efa) lists two failures outside the owned three; .mob/records/217/runs/local-nofloor-2.outcomes.json passed them; .mob/records/217/run-pairs.json holds the floor decision 'kept' and its reason.
- The differing tests between the two no-floor local runs (read from those files by the facilitator):
  - Phrase.Acceptance.Tests.GeneratedApiBoundAddress.I_readiness_comes_from_the_childs_own_output_before_any_connection_and_not_before_the_gate: run 1 Failed, run 2 Passed
  - Phrase.Acceptance.Tests.GeneratedApiHazards.Guard_two_parallel_starts_get_distinct_base_urls: run 1 Failed, run 2 Passed
- Card 303's origin: the with-floor local lists (.mob/records/217/runs/local-floorC-1/2.outcomes.json, head 2bb2f306c686692097fe33a41a867f244ea4a24f) fail exactly:
  - Phrase.Acceptance.Tests.CSharpBaselineAcceptance.endpoint_reply_fails_when_response_record_or_status_are_wrong: Failed
  - Phrase.Acceptance.Tests.CSharpBaselineAcceptance.endpoint_reply_passes_when_response_record_and_status_match: Failed
  - Phrase.Acceptance.Tests.UsableV1.Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone: Failed

## Issue-text snapshots (gh issue view, state at generation time)

### Card 303
Title: (303) The C# baseline endpoint tests pass on the Windows development machine
State: OPEN

<!-- phrase-card:303 -->

- **Status:** Draft
- **Shape:** Contract
- **Type:** bug
- **Repository:** avowry (product tests: CSharpBaselineAcceptance)
- **Source:** card 264's regression run of the whole acceptance project on 2026-11-05 (Windows 11, dotnet 10.0.403). Two tests fail locally: CSharpBaselineAcceptance.endpoint_reply_passes_when_response_record_and_status_match and CSharpBaselineAcceptance.endpoint_reply_fails_when_response_record_or_status_are_wrong, with diagnostics `tests_did_not_run` on task 020-endpoint-greetings (three attempts). They fail identically on the tree before card 264's launcher change (commit a7ad8c5, run in a scratch export), and CI on Linux passes them (ci run 42058818877, head 2f331cd). The cause is not known; the diagnostics carry no reason for why the generated baseline project's tests did not run.
- **Depends on:** none
- **Actor:** a developer running the acceptance suite on the Windows development machine
- **Given:** the machine that the mob and the product owner use for local runs
- **Observes:** the two tests pass, or fail with a reason (the generated project's build or restore error) instead of the bare `tests_did_not_run`
- **Fails with:** a diagnostic naming why the baseline project's tests did not run (for example a restore error, a missing tool or a path problem)
- **Invariants:** the CI result on Linux is unchanged; no assertion is weakened
- **Enables:** card 099 and every local full-suite run (a suite that is red on the development machine hides real failures)
- **One observable behaviour:** the full acceptance project is green on the development machine, as it is in CI.
- **Done checks (at most 3, each failing on partial work):**
  1. The cause is named with the command that shows it. ZOMBIES to be written by QA at ready.
  2. The two tests pass locally, or the failure message names the cause. ZOMBIES to be written by QA at ready.
  3. CI stays green on the same change. ZOMBIES to be written by QA at ready.

## Context

- Filed by the facilitator on 2026-11-05 from card 264's regression run; a failing test is a bug (D183). Note: UsableV1.Bar6 also fails locally on stale LLM-check evidence (grammar_sha256 stale, tied to card 162's re-freeze); it is not part of this card.


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

