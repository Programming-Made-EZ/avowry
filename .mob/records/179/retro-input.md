# Retro input: card 179 (the README and the getting-started guide for the C# adapter preview)

Facilitator's facts from git, run logs and the records. Read the records named.

## What shipped
README.md (33 lines: its own language, the C# adapter preview, who it is for, the three aims, the clone URL, links to the guide and prerequisites, the limitations page named as card 187), docs/GETTING_STARTED.md (new, about 230 lines: clone, pack, install from a local feed, build, 11 specs, start, create/read/failure calls, afterwards; every command with a PowerShell form), docs/PREREQUISITES.md newer-SDK paragraph; the status-table script and its test retired with their rename-test lines. One Pending-then-active drift test (tests/test_getting_started.py, 283 lines, wrapped by GettingStartedDocs.cs) with one independent check per bullet in one aggregated assertion (QA's adopted condition); a declared characterisation (demo README byte-identical).

## Flow metrics (local time, 2026-11-08, from git and run logs) and process against build time
- Ready draft (a Grok PM seat, about 17 minutes) and the declaration D218 (record assembled 01:32; primary window about 7 minutes; confirmation step about 5; recorded 01:49): about 40 minutes from the draft to the adopted record; D218 itself about 17 minutes from the committed proposal to the recorded result (D217's target: at most 20). Two real Nos (QA: a thin red; decider: the reworded sentence outside the cold reader's files), both adopted and confirmed.
- Build: slice 01:56, baseline probe 01:58, characterisation 02:07, red 02:13, green 02:20 (about 31 minutes from the adopted record to the green), cold reader 02:34 and page revision 02:37, gate review 02:58, sign-off about 15 minutes later. Process against build: about 40 minutes of declaration against about 48 of building (about 0.85 to 1); with the gates the build side is about 90.
- D217 trial (first Decider counted): the declaration used no separate re-run; trigger line (a). The primary window had 2 Nos; the confirmation step settled both.

## Seat mix (the product owner's clarification: report Claude's share and the reason for every Claude seat)
- Card seats (about 21): Ready PM Grok; D218 primary window Grok 3 (architect, QA, decider), Muse 2 (developer, coach), Claude 1 (audit lens); the confirmation step the same split; slice Claude; red and characterisation Grok; green and page revision Claude (two turns, never the red's seat); cold reader Muse (first cold-reader seat); gate reviewer Claude (two runs: the first, on the unrevised pages, was stopped and relaunched after the page revision); sign-off Claude. Claude share: 8 of about 21 (audit lens twice, slice, green twice, gate reviewer twice, sign-off). Muse 5, Grok 8.
- Reasons for the Claude seats: Codex out of quota until about 2026-11-11, Copilot and agy out of quota, Grok wrote the red (a red and its green are never the same seat), the independence rule leaves no non-author non-trial driver in quota for the gate and the sign-off, and the trial drivers take low-stakes read-only seats; the audit-lens seat stays on Claude so it is a different model family from the Grok and Muse seats.
- Muse: first counted Decider seats on card 178; on this card it held both counted seats twice and the cold-reader seat (about 1 minute, six useful findings and one misreading).

## Red-first (permanent rule; report any born-green to the product owner)
The characterisation (demo README) was declared and passed on the pre-change tree and on a scratch post-change tree before the red was filed (the card 178 lesson held: no late revision). The single Pending red was watched failing by assertion (python and the dotnet wrapper); the gate reviewer and the sign-off replayed it. The green removed only the Pending trait; the test file is byte-identical. No test was born green undeclared (the six self-tests are declared).

## Incidents and findings
- The first gate review ran on the unrevised pages (the cold reader had not finished); it was stopped and relaunched on the final tip (about 20 minutes of reviewer time lost; the order should be cold reader, page revision, gate review).
- The push hook refused twice: card 326 (closed as not planned) lacked the not-card label that makes the closure check skip it (fixed); MOB.md was six non-MOB commits stale at push (regenerated; the limit is three).
- The first push of the pair was a no-op because it ran before the regenerate; the hook output was read from the log only after the monitor reported.
- The reviewer's full gate had one acceptance test fail on a 20-second timeout (issue 304, third sighting), passing alone; the facilitator's and the sign-off's isolated runs were 328 of 328.
- The reviewer ran one `rm -f` on its own untracked file inside its own clone (disclosed); the green seat left one scratch script outside its worktree that it could not delete (the facilitator removed that one file).
- The red's output prints `dana-agent: dana-agent` and `known-limitations: known-limitations` for two checks (the id is repeated where the message should say what is missing): cosmetic; D137 forbids changing the test body at the green.
- The characterisation guards the demo README only at the guide-adding commit; nothing protects it after the green (named by the reviewer and the sign-off).
- Not verified by any run: the bash forms, Windows PowerShell 5.1, an 8.0.x SDK, the "about 15 minutes" claim: cards 180 and 181.

## Carried to this retro (the mob's own questions)
The look-at D208, D209 and D210 with the corrected confirmation step (redo each, post to Discussions threads 328, 329, 330); posting every retro to Discussions as a standing step (the product owner approved sending the proposal; this retro is posted as thread 340's successor); bugs 338 and 327; S1 to S6 Draft cards and their rank (the product owner's decisions of 2026-11-08).

## Questions for the voices
The adjustment review of the Experimental entries this card exercised (D217 confirmation step replaces the re-run, trial 1 of 5 counted from the next Decider; D204 red before the atomic green; D206 cold-reader seat and story-promise check; guard tests reading committed blobs; D202 isolated gates with a Load line; D174 parallel dispatch; the standing-rules preflight), the process-against-build ratio, and one improvement candidate that is not a process loop (keep the retro lean).
