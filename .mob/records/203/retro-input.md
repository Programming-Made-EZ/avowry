# Retro input (card 203, written at the declaration): root cause of four failed vote attempts, 2026-11-08 UTC

Dispatch watchdog: no product commit between 14:32 and about 16:30. Process time lost to the declaration of card 203: about 75 minutes against no build time yet. Root causes, in order of cost:

1. The Ready brief was sent through decider.py (the vote wrapper): it reads any prompt as a proposal to vote on, so Cursor answered "Yes" and Muse hit the runner cap on a long reading brief (1,500 s, no reply). A Ready draft goes through cursor_reply.py (or a reply-only seat), never through the vote wrapper. About 30 minutes.
2. Concurrent decider runs in one checkout: each run's integrity check saw another run's prompt-*.txt in the repository root, and the seat-use history check failed ("not append-only", bug 365); a 35-second stagger did not help because each run lasts minutes. Muse was also out of quota (429) by then and Grok was still in the fallback chains, so every fallback added more concurrent runs. About 25 minutes.
3. The shared vote prompt template for the run_votes_par.py runner (the reply format and the CITE line, D171) lives in the CTX variable I set by hand in the D226 window; for D227 I launched the runner without it, so six seats answered a bare "Yes" and nothing counted. The cause is an ad-hoc launch after the seat swap, not the runner. About 20 minutes.
4. The Ready draft's section 6 still named Grok as the red seat; one Qwen attempt answered No on that. The declaration proposal now marks the section superseded.

Fixes: the shared vote prompt (reply format and CITE) is stored in a committed file and the launcher reads it (no hand-set variable); run_votes_par.py drops dead drivers from its fallback chains; windows are staggered but the integrity-check interference needs a repair (bug 365 covers the seat-use race; the stray prompt files in the repository root are a second cause to file); a Ready draft never goes through the vote wrapper.

---

# Retro input: card 203 (names reserved by generated code are rejected), facts from git and run logs (2026-11-08 local times)

Names, addresses, home paths and secret values are not printed here. Read the records named.

## What shipped
A guard in the C# adapter that rejects, before any C# is written, a type, endpoint, module, avow or field whose name the generated code reserves, with one new diagnostic (PH341, renamed with the other codes by card 361): a `ReservedNames` class shared by emitter and guard, `Line`/`Column` on three model records, nine reject rows in one acceptance test, five reject corpus fixtures, three near-miss characterisation fixtures, one GRAMMAR row; generated C# of valid sources unchanged (golden test). Green 5a82e7d; gate review SHIP; sign-off SIGNED OFF WITH CONDITIONS. Bugs filed: 372 (field line), 373 (test limits), 374 (stray prompt files), plus 365 (seat-use race, earlier).

## Flow metrics and process against build time
- Declaration: Ready draft (Muse seat capped at 1,500 s, the wrapper misuse, then Cursor through cursor_reply.py: about 25 minutes to a usable draft), the facilitator's reruns, three failed vote windows (root causes above), the counted sequential window and the record: about 90 to 100 minutes from the first launch to D227 recorded at 16:32.
- Build: slice 16:42, characterisation 16:47, red 17:11 and its revision 17:26, green 17:43: about 61 minutes from the record to the green.
- Process against build before the gates: about 1.5 to 1. After the green: baseline gate (one load flake, rerun green) and push 17:43, independent gate review 18:39 (about 56 minutes, mutation table), sign-off 18:51 (about 12 minutes).
- Red-first held: characterisation declared and proven before the red; the red failed by assertion on nine rows (facilitator reran it on main); the green changed the red file by one line (the Pending trait). No born-green finding.

## Seat mix (the product owner's direction: report Claude's share and the reason for each Claude seat)
Card seats: Ready draft Cursor (after a failed Muse attempt); declaration Decider seats: Qwen 2 (architect, audit lens), Cursor 3 (QA, coach, decider), Claude 1 (developer); slice Claude; red Claude QA context in its own worktree; green a different Claude context; gate reviewer Claude; sign-off Claude; Cursor and Muse reading roles not yet seated (second read-only review skipped, see below). Claude held 5 of about 15 seats beyond the votes... exact: slice, red, green, gate review, sign-off, one vote. Reasons: Grok, Codex, Copilot and agy out of quota, Muse out of quota (429) from 15:58; red and green on different contexts (the independence rule); the gate review and sign-off must run the gate, which Cursor (ask mode) and Qwen (reply-only) cannot.

## Incidents and findings
- The slice was wrong about two things the red seat found by building: R5 (a real C# error, not a silent overwrite) and R11 to R15 (build today, not rejects); the Ready draft's R5 was right. The facilitator dropped R11 to R15 as rejects and kept them as a near-miss fixture. The red cost 45 minutes with its revision.
- The independent gate reviewer found F1 (the field collision names the type's line) and F2 (dead model fields), plus four surviving mutants (test limits); bugs 372 and 373.
- The baseline gate failed once on a load-sensitive benchmark timeout test (passes alone; passed on the rerun and in the review and the sign-off).
- The second read-only review (Cursor) named in the declaration was not run (Cursor ask mode was used for the Ready draft; the gate review covered the ground).
- Leftover clones: gate203-clone, gate203-review, signoff203-clone, worktrees wt-203-red and wt-203-green (no recursive delete is allowed; removal is by `git worktree remove` for the worktrees only).
- The card file board/203 still says Phrase.sln and lists AvowryRuntime/AvowryBuild as reserved; the Ready draft's corrections govern.

## Carried to this retro
The look-back at D208, D209, D210 (posting to Discussions threads 328, 329, 330); the trials running: D222 (a Ready row naming two constructs becomes two ids: 3 of 3 code cards used now: 189, 186, 203) and D219 as tuned by D226 (no docs card yet); the follow-up cards named by the proposal (derived names, the `.cs.cs` extension, case-insensitive file systems, the `Specs` namespace) to be filed.

## Questions for the voices
The adjustment review of the Experimental entries this card exercised (D222 third exercise; D204; D202; D174; the Decider protocol step 6 as permanent practice), the process-against-build ratio (about 1.5 to 1 before the gates, most of it harness failure), the drivers left (Claude, Cursor, Qwen), whether a Claude-only red and green are enough independence, and one process improvement candidate.

## Misdiagnosed driver: Muse (added after the product owner's correction)
The facilitator treated Muse as out of quota from about 15:58 local and never re-probed it. The actual error was `API error 429 ... Subscription quota exhausted. Your usage window resets at 2026-11-09T02:11:22Z (rate_limit_error)`: real at the time, but the reset (18:11 local) passed about two hours before the D228 window, and the window then used only Cursor, Qwen and Claude. `muse usage` at 20:28 local read 21 percent of the window and 9 percent weekly; a one-line prompt answered OK in 9 seconds. Effects: Muse idle for about two hours; the rotation rule refused the Claude seats in the D228 window (Muse, unseated, had waited longest), so two seats were re-asked. Not a probe fault in the tooling: the standing Driver availability rule (re-probe at every boundary and after a known reset time) was not applied; DRIVERS.md did not list Muse at all. Fixes: DRIVERS.md now lists Muse with the evidence; Muse is seated next by the rotation; the preflight (D228) names the fallback chains and the out-of-quota check with a probe age.
