# Card 214 sign-off (ctx-5, product manager, signoff)

Run 2026-10-30 12:25 to 12:35 UTC, Claude seat, fresh context (probes 12:03 UTC, .mob/records/probes/2026-10-30.jsonl: Codex, Copilot, Grok, agy without usable quota). Basis: origin/main 917bfc6 (pre-ship gate), card code last at d18194c. No gh calls made; I rely on the gate's CI evidence. `mob_process.py check` not run, by instruction.

## Verdict: SIGN OFF

## Done checks
1. Root cause shown red, both hazards, before the fix: MET. Red commits 26681e1, 1fcc793, 391828f precede green 111960d (ec25197 seams first); hazards a and b green unchanged after the fix (review a477bc6, "The choose-then-release window").
2. Own the port: MET. ASPNETCORE_URLS is the literal http://127.0.0.1:0 (GeneratedApi.cs:85), the harness takes the address from the child's "Now listening on:" line, no retry on bind failure (guard counts exactly one start). Review verified kill discipline (only the recorded PID), timeouts and assertions unchanged.
3. No reuse window left: MET. `git grep FreeLoopbackPort -- '*.cs' '*.py' '*.yml'` returns 0 lines (I re-ran it). Exactly one removing commit, 111960d (`git log -S` also lists 6ebc758, which introduced it). Source-scan test passes. Closure evidence: CI run 41052829472, success, head 9046262, which contains d18194c with identical code (gate item 3 and 4); earlier green ubuntu run 41045702766 on afa4e40. Neither is a re-run of fb13e3d.

## ZOMBIES
The cases listed on checks 1 to 3 (Z, O, M, B, E, I, S) are covered by the 31 GeneratedApi, Hazard, Guard, SourceScan and Z tests, all passing in the gate's full run (253 of 256, zero card failures). I re-ran the source-scan tests and the Z timeout test now: 7 of 7 passed.

## Conditions
Window 214-rerank-4 (adopted, e6e2ab8): (1) task force polled before start: met via window 214-taskforce and D156. (2) 213 paused: not this seat's evidence. (3) check 1 red before the fix: met. (4) F-056 closed only on the removal commit plus a green full CI run on the last commit: met as above. 217-rerank-6b (f870506, amended): 214 ships on its own evidence plus its gate, and the floor stays a recorded mitigation until 217: met (d18194c comment, board/217 exists). D156: C1 roster seated as declared and domain-expert answers NO at refinement, slice, after green and at ship, with no Linux CI difference seen; C2 card text carries the second hazard; C3 D136 limits and no extra ceremony: only the D154 gate was added (I did not re-measure per-commit size; the gate does not report a breach); C4 red then green in separate commits: met; C5 all roles polled: met per the windows.

## Deviations the card carries, and whether they block
- F-060: all three checks' red (about 800 lines, ten guards) written before any green, with a single green in commit 5, against one-criterion-at-a-time. Does not block: the red is real, each red test went green for the stated reason, the mutation re-run proved the guards bite. It stays as a recorded process deviation for the next stand-down.
- The 500 ms ExitSettle is not in the slice (disclosed in the commit 5 body, review finding 1). Does not block: a bound on the print-then-exit race, costing about 0.5 s per start, no assertion changed. Gap left: the review asked for a comment line saying "bound, not proof"; I did not verify it was added.
- The ThreadPoolFloor (128) is a mitigation, not a fix (review finding 2, F-062 verdict EXPOSES). Does not block: the old harness already had the slow path (Bar5 failed at 4 m 56 s there), the floor is test-assembly only, labelled a mitigation in d18194c, and card 217 owns removal. Residual risk: one sample at 3x load could not separate "new harness more sensitive" from "extra tests add load"; if the 4-core ubuntu runner goes red, read the message before blaming a port.
- Evidence caveat: d18194c has no CI run of its own; the green is on descendant 9046262 with identical code. Accepted.

## Open items for the facilitator (not blocking sign-off)
- Done check 3's "I" case wants `mob_process.py check` to exit zero on the card's tree and the delivery note to show sha, run id and head sha side by side. The gate shows the three side by side; the check I was told not to run and did not.
- record-in-progress.json has no ship or CI entry for 9046262 / 41052829472 yet; add it.
- Gate residuals unchanged: 3 environmental failures (Bar6 stale evidence, two CSharpBaselineAcceptance endpoint_reply), none card 214.

## Persona demo (D150): a developer running the acceptance suite
- Before: the harness picked a port, released it, and a parallel class or an outgoing socket could take it, so a run died with "Failed to bind to address ... address already in use", and a pass on retry meant nothing.
- Now: every generated API starts on 127.0.0.1:0, the kernel assigns the port, and the harness connects only to the address that process printed; ready is never inferred from some other listener.
- A bad start now fails with the PID, the URL asked for (http://127.0.0.1:0) and the process log; a scan test fails the build if FreeLoopbackPort or a choose-then-release helper comes back.
- Honest limit: under heavy parallel load the suite can still starve the thread pool (card 217); the floor of 128 hides most of it, and a timeout now reads "did not report an address within N seconds" with the log, not a port error. Gate run: 253 passed, 3 known environmental failures, 10.7 min.
