# Retro input: card 189 (an unsupported construct is a diagnostic, not an unhandled exception)

Facilitator's facts from git, run logs and the records. Read the records named.

## What shipped
Eight reachable crashes of the C# emitter (a plain-call step in a composite, `fail with` in a composite, a leaf with only a python or typescript native, two effects, a non-variable call argument, a spec `given` of none, an effectful avow in a composite `let`, a store seed of an Int field) plus the `then returns none` outcome found at the gate, now exit 1 with `PH340 (line:col) The C# target does not support ... yet.` or `PH020`; the CLI path unchanged (CompilationFailedException); `Column` carried by the bound model for the diagnostic position; nine corpus fixtures; two GRAMMAR.md table rows. One Pending-then-active test with an id per row in one aggregated assertion (UnsupportedCsharpConstructs.cs), a declared characterisation (a composite return type stays PH325 through the CLI; the orders hash and the corpus theory pass).

## Flow metrics (local time, 2026-11-08, from git and run logs) and process against build time
- Ready draft (a Grok PM seat, about 7 minutes) plus the facilitator's rerun of the repro (the crash reproduced on HEAD, exit not 1), record assembly and Decider D221 (window about 7 minutes, six Yes, no re-run: trigger (d)): about 22 minutes from the draft launch (12:46) to the adopted record (13:08). Re-rank 31 (Decider D220) before it: about 8 minutes from the proposal to the adopted record.
- Build: slice 13:08 to 13:20 (the architect ran every row through the CLI: about 20 minutes of seat time), red 13:36, green 13:45 (164b928), baseline gate 13:28 to 13:38 (the facilitator's clone), independent gate review 14:00 (HOLD), red revision 14:05, green revision 14:13, gate 14:22, re-review about 4 minutes, sign-off about 15 minutes. Process against build time: about 0.5 to 1 before the gates (22 minutes of declaration against about 45 of building); the HOLD added about 45 minutes of revisions and a second gate (about 10).
- D217 trial: D220 and D221 both used no separate re-run (trigger (d)); the trial counts D218, D219, D220, D221 (4 of 5).

## Seat mix (the product owner's clarification: report Claude's share and the reason for every Claude seat)
- Card seats (about 23): Ready PM Grok; D220 and D221 windows Grok 3, Muse 2, Claude 1 each; slice Claude; red, characterisation and red revision Grok (two turns); green and green revision Claude (one context, two turns, never the red's seat); gate reviewer Claude (a first review and a delta re-review on the same context); sign-off Claude. Claude share: audit lens x2, slice, green x2, gate reviewer x2, sign-off: 8 of about 23. Muse 4, Grok 11.
- Reasons for the Claude seats: Codex out of quota until about 2026-11-11, Copilot and agy out of quota, Grok wrote the red (a red and its green are never the same seat), the independence rule leaves no non-author non-trial driver in quota for the gate and the sign-off, the audit lens stays on a different model family; the architect (slice) is on Claude because the Grok seat wrote the PM draft and the red.

## Red-first (permanent rule; report any born-green to the product owner)
The characterisation was declared and passed on the pre-change tree before the red was filed (it passed 1 of 1 and failed under a code mutation). The single Pending red was watched failing by assertion with 12 id lines (the eight rows and four hostile inputs); the red revision (the missing row) was watched failing with exactly that one row before the green revision; the green removed only the Pending trait and the revised test file is byte-identical. No test was born green undeclared.

## Incidents and findings
- The independent gate review returned HOLD: the card's table row named two halves (`given` and `then returns` of none) and the slice's row list and the red carried only `given`; the second half still crashed. Cost: one red revision, one green revision, a second isolated gate and a delta re-review (about 45 minutes, plus about 10 for the gate). The reviewer's mutations and its own probes found it; the PM draft's table had listed it, the slice's table had not, and the QA seat copied the slice.
- A mutation that sets a diagnostic's column to 0 survives: nothing pins the column (the corpus convention is code and line). Filed as bug card 352; the message polish as bug card 353.
- The green seat made an extra edit to tests/test_rename_196.py (the R100 rename check restricted to fixtures present at the rename commit) that the slice had missed; the reviewer judged it non-weakening with a rename mutation.
- The commit hook prints a long listing of board issue lines before "decision index ... passed" (both greens noted it): an old output, not an error.
- Earlier on the board: the six platform Draft cards were filed as plain issues first (the board tool new_card.py would have run the persona lint and added them to the project); the project add and fields were applied with the tool's own functions afterwards; the push hook demands the `not-card` label on a card closed as not planned and MOB.md within three commits (both fixed before the pushes).
- The benchmark 20-second flake (issue 304) hit the reviewer's full run (fourth sighting); it passed alone and in two other isolated runs.
- Card 162 was the top card and could not be pulled (Codex out of quota; its LLM check is frozen to the Codex and Grok pair); card 201 is owner-run: the facilitator moved both below card 203 through a Decider (D220) instead of idling; the Ready draft for 162 is kept in the records.

## Carried to this retro (the mob's own questions)
The look-back at D208, D209 and D210 with the corrected confirmation step (to be posted to Discussions threads 328, 329, 330); D217 trial (4 of 5 counted, no re-run on D220 and D221); whether a ready-draft row that names two halves must become two ids in the slice's table (a slice-brief line, not an agreement change); posting each retro to the Discussions (done for cards 178 and 179).

## Questions for the voices
The adjustment review of the Experimental entries this card exercised (D217 confirmation step instead of the re-run, trial; D204 red before the atomic green and the red revision; D202 isolated gates with a Load line; D174 parallel dispatch; the standing-rules preflight), the process-against-build ratio, and one improvement candidate that is not a process loop (keep the retro lean; the HOLD's cause is the obvious candidate).
