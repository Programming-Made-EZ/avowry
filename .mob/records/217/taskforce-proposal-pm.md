# Card 217 task-force declaration proposal (PM)

Card: the acceptance tests starve the thread pool (bug, Contract shape, Draft). It is corrective for open audit FAILs F-063 (pool starvation flakes) and F-065 (three failing tests labelled environmental).

All numbers below come from committed HEAD 082d2f5. The `git status` output listed untracked files only, and I changed nothing.

Facts I ran:
- **Blocking-wait scan.** `git grep -n -E "ReadToEnd\(|WaitForExit\(\)|\.WaitForExit\(|\.Result\b|\.Wait\(" -- tests/Phrase.Acceptance.Tests`, minus ReadToEndAsync, returned 15 lines in 7 files. Those are 7 blocking sites:
  - PrivateHoldoutResults.cs 3
  - LlmCustomerCheck.cs 3
  - LibraryBindingLayering.cs 3
  - ExtensionMap.cs 3
  - HoldoutTaskConversion.cs 1
  - HeadToHeadVerdict.cs 1
  - DocumentationPrinciples.cs 1

  Five sites use a bounded `WaitForExit(120000)` or `(180000)`, which still blocks a pool thread. There is no `.Result` or `.Wait(` hit. A separate scan for `GetAwaiter().GetResult` or `Thread.Sleep` gave 1 line.
- **Test count.** `git grep -h` of [Fact] and [Theory] gave 216 Facts and 14 Theories, which is 230 attributes. `dotnet test tests/Phrase.Acceptance.Tests --list-tests` (TEMP and TMP set to C:/Users/ringe/AppData/Local/Temp/claude/ready217) listed 278 cases in 11.8 s.
- **Full-run time.** Unrun: I may not run the full suite. The facilitator's last full local run was 278 tests in 10 m 50 s on 2026-11-05 at dc1c39c, with 274 passed.

## Task force
- **Product manager (ready):** Copilot, ctx-1. Domain: queue, scope, ready gate. Why: it keeps the card to the three checks and records the ready check below.
- **Architect (slice):** Codex (read-only, cmd.exe path), ctx-2. Domain: .NET thread pool, async process I/O, xunit parallelism, source scanning. Why: it chooses the probe design and the scan rule before any red, and it ranks the 7 sites.
- **QA engineer (red):** Claude, ctx-3. Domain: deterministic probe design. Why: it writes one red per done check.
- **Developer (green):** Claude, ctx-4. Domain: the acceptance harness and the 7 blocking sites. Why: Codex cannot run shell commands on this machine (card 297; the machine fix awaits the product owner), so Claude fills both coding seats.
- **Independent reviewer (ship):** Copilot, ctx-5. Domain: git-level review. Why: it did not author. It checks that no assertion or timeout changed and that the floor decision is recorded.
- **Signoff / Decider:** Claude, ctx-6, a context that did not write the green. Why: final decision and integration under the single-writer lock.

Placement facts: Grok stays out of counted windows until card 291. Whoever authors never reviews.

## Left out
- **UX, frontend, database:** no user interface or data.
- **Security:** no sandbox or product change.
- **Technical writer:** the floor decision goes in the card record.
- **DevOps:** the CI runner is unchanged. If CI shows a runner fault, that becomes a separate card.
- **Agile coach:** observer only, not counted.
- **Domain expert:** see below.

## Domain-expert question
Should an agent with different domain knowledge join? No for now. Card 264 ruled starvation out for the 2026-11-01 failure (a harness masking defect plus a build collision). So 217 is pool hygiene. The architect covers it. Revisit at a cycle boundary if the probe cannot be made deterministic.

## Ready check
FIX THE CARD TEXT BEFORE READY (small edits, no re-scope):

1. **Check 1 may be unreachable as worded.** "Delayed beyond its window on the tree without the floor" cannot be shown against the real suite. The floor is a module initializer, and card 264 recorded that no no-floor probe was run. Reword it: "a deterministic probe occupies the pool with N synthetic blocking waits, with the minimum thread count left at its default, and shows a readiness continuation missing its window (red). The same probe passes once the harness wait does not need a free pool thread." The outcome "cannot go red against the real suite" is then recorded with its evidence. It is not silently dropped.
2. **State the real-suite fact.** The Context should say that card 264 showed the 2026-11-01 failure was not starvation, so card 217 is hygiene and a mitigation review, not that root cause. The Source and Context text still reads as if starvation caused it. The Source's "ExtensionMap, LibraryBindingLayering... and others" is stale: the scan finds exactly the 7 sites listed above.
3. **F-065 ownership.** The three tests are no longer "owned by this card, root-caused and fixed". The two CSharpBaselineAcceptance endpoint tests are owned by the Windows-machine bug card (303). UsableV1.Bar6 is owned by the grammar re-freeze card (162). Rewrite the Context bullet as: "Counted, not fixed here. Done check 3 passes when the only failures are these three, each with its open owner card, the same three on both runs. F-065 closes when 303 and 162 close." Without this, the card is not done until two other cards ship, or someone silently re-owns them. Add 303 and 162 to Depends on only if the product owner wants 217 to wait. I recommend not waiting.
4. **A fourth failure is unexplained.** 278 tests with 274 passed leaves 4 failures, but F-065 names 3. The architect must identify the fourth from the trx before red. If it is a flake, it belongs to F-063 and to this card. It must not hide inside "same failures".
5. **Repository line.** It reads `phrase-lang (<phrase-lang>)`, which is a leftover placeholder. After the Avowry rename (D125) it should read avowry, tests/Phrase.Acceptance.Tests. Card 303 already uses avowry.
6. **Check 3 fits the 100-minute lesson.** Card 264's 100 minutes came from 20 loaded repeat runs. This card needs two full runs, about 11 minutes each locally (about 22 minutes as a machine job). CI took about 16 minutes per push on card 264. Two runs are cheap, so keep them. Add a method line: "same results" means the per-test outcome lists (trx) are identical, committed as evidence, with TEMP and TMP set. The CI pair is two consecutive runs on main at the same head.
7. **Floor decision.** The final green runs without the floor (floor removed), or the floor stays with a recorded reason. Remove the floor only with both run pairs green.
8. **Windows against Linux.** Locally the 303 tests fail and in CI they pass. So "same results" is checked per environment, not local against CI.

## ZOMBIES, one criterion per cycle
**Cycle 1, check 1 (starvation probe red, then green):**
- Z: zero blockers, so the continuation runs at once and the probe is green.
- O: one synthetic blocker per pool thread, with the minimum at its default, and the continuation misses its window (red).
- M: many blockers (N above the minimum thread count) plus several readiness waiters.
- B: blockers exactly equal to the minimum, and one more than the minimum.
- I: the probe runs in its own process or sets the minimum itself, so the module-initializer floor does not mask it.
- E: a probe that stays green is reported as "cannot go red", with its evidence.
- S: red on the unfixed wait, green on the fixed wait.

**Cycle 2, check 2 (no blocking waits):**
- Z: zero hits in the scan.
- O: one site converted, its scan hit gone, its test unchanged.
- M: all 7 sites converted. The scan rule covers ReadToEnd, WaitForExit, .Result, .Wait( and GetAwaiter().GetResult.
- B: a bounded `WaitForExit(timeout)` counts as a hit.
- I: the scan is a test or a script run in CI, listing class and line.
- E: a seeded blocking call is caught and named.
- S: the scan is clean and every assertion and timeout is unchanged.

**Cycle 3, check 3 (load-independent results):**
- Z: no failures other than the three F-065 tests.
- O: one full local run gives the per-test outcome list.
- M: two consecutive full local runs and two CI runs give identical outcome lists.
- B: a different failure set in run 2 is a finding, not a pass. The fourth unexplained failure is named.
- I: the 303 and 162 owner cards are linked in the record.
- E: any new failing test gets its own card or an owner before closure.
- S: the floor is removed or kept with a reason, and the pair-run evidence is committed.

## Value, metric, counter-metric
Value: a full run's result no longer depends on load, the F-063 FAIL closes, and the three F-065 tests each have an open owner.

Metric: 0 blocking waits in the scan, and 2 of 2 consecutive full runs locally and in CI with identical outcome lists.

Counter-metric: full-run time (about 11 minutes) must not grow, and no assertion or timeout may be weakened or skipped (must stay 0).

## What the window decides (facilitator's framing, not the seat's text)

- The task-force declaration above (roles, assignees, left out, domain-expert answer).
- The eight card-text fixes of the Ready check, taken as one card-text fix in the same window (D153 lists a declaration and a card-text fix as objection-window decisions): the card text is edited on the issue after the window, in the words the Ready check gives.
- Facilitator's fact for ready-check item 4: the fourth failure of the full local run on dc1c39c (278 tests, 4 failed) was GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers, a regression of card 264's green 2 that CI also showed, fixed in 2f331cd; the other three are the F-065 tests (UsableV1.Bar6 and the two CSharpBaselineAcceptance endpoint tests).
