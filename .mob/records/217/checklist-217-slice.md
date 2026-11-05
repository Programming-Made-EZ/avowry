# D158 pre-window checklist: window 217-slice (facilitator)

- Proposal: .mob/records/217/turn-slice-architect.md (architect, Codex ctx-2, read-only through cmd.exe, commands run on HEAD b3a383d; 13596 bytes before its Size line, under the 15360 limit).
- Card: 217 (text as fixed by window 217-taskforce). Declaration adopted there; the eight card-text fixes applied to the issue.
- Category: slice decision (D153 objection window). Seats polled: product manager (ready), QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the architect (proposer) does not sit. D171 cites verbatim, case-sensitive; an unread vote is re-asked once; D187 for one condition.
- D189 (in force, first slice under it): the slice's numeric claims, and where each command is:

| Claim in the slice | Command line carrying it | Facilitator re-run |
|---|---|---|
| 15 synchronous lines, 7 sites in 7 files, 5 bounded and 1 unbounded wait | Premises, "Blocking surface" (git grep ReadToEnd / WaitForExit) | Yes: 15 lines, 7 files, 1 unbounded WaitForExit() (PrivateHoldoutResults), matches |
| 0 GetAwaiter hits; 1 Thread.Sleep (TempWorkspace.cs:54) | same premise (git grep GetAwaiter, Thread.Sleep) | Not re-run |
| only LlmCustomerCheck isolated from parallel execution | Premises, "Parallelism" (git grep Collection) | Yes: only LlmCustomerCheck.cs:7 and :16, matches |
| the floor is 128 on worker and I/O minima via a module initializer | Premises, "Floor" (type ThreadPoolFloor.cs) | Yes: ThreadPoolFloor.cs:17 SetMinThreads(Math.Max(worker, 128), ...), matches |
| 13 test cases across the 7 files (3, 5, 1, 1, 1, 1, 1) | Premises, "Test cases" (git grep Fact / Theory / InlineData) | Not re-run; whole-project count marked "unrun: <reason>" with its fallback |
| 1 CI job and 1 step run the acceptance project (ci.yml:70) | Premises, "CI surface" (git grep dotnet.test, git show ci.yml) | Not re-run; CI duration marked unrun |
| 278 tests, 10 m 50 s, 274 passed, 4 failed on dc1c39c | stated as a historical datum from the facilitator (not run by the seat) | The datum is the facilitator's own run |

- Caps (D175): 1+1+1. Criterion 2's green is one cycle (partial migration cannot satisfy it). Criterion 1's red may first fail to compile against the async seam (as on card 264).
- Facilitator's notes for the seats, facts only: card 264 closed with the build-collision fix; this card is pool hygiene and a floor review; the F-065 tests are counted, not fixed (owners 303 and 162).
- Seats run with Grok disabled (backends-nogrok.json) until card 291.
- No commit to avowry while a voter runs.
- Probe: 2026-11-05 21:21 UTC (Copilot and Codex available), plus decider.py's own probe per run.
