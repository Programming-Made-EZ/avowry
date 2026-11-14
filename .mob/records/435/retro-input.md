# Card 435 retro input (facilitator, from the card's records; committed before the voices)

Times are local (UTC-8) unless marked Z; commit times from `git log`.

## Flow metrics (first, per D258 clause 4: constraint first)

- Lead time: Ready 2026-11-11 08:59 to the rework on main 2026-11-14 14:27; paused by the owner 2026-11-11 11:10 to the resume 2026-11-14 about 10:00. Active time about 2 h 10 min before the pause and about 4 h 30 min after.
- Rounds: one red, three greens' worth of work (the first green, the rework), three reviews (two FLAG after one quota death), one Decider (D259) that stopped the patch loop.
- Where the time went after the resume, by step: red repro 2.5 min; first green 23 min (seat); first light gate 17 min; second review 15.6 min lost (Grok died on 402) + 37 min (agy); architect re-slice 34 min; rework red 2.5 min + a takeover; rework green 3 min; third review 31 min; D259 10 min across three windows; rework light gate 16 min.
- Waiting, not working: pushes. The pre-push process check took 10 to 16 minutes per push (it calls git once per commit); pushes were refused twice for a stale MOB.md (about 25 minutes lost before the cause was seen, because an early push printed exit 0 through a pipe and hid the refusal); one commit was refused by the home-path hook (a stand-in name inside a path) and a later push then printed "Everything up-to-date". Commit-to-live for the resume work: committed about 10:30, live 11:52 (about 82 minutes); for the green: committed 11:15, live 11:52.
- Constraint (the biggest flow loss, from the data): rework caused by review rounds that did not converge on a single-regex design (two FLAG rounds, each fixing one list and opening a new one of similar size), then the push path (the slow pre-push check plus stale-MOB.md refusals). Focusing step applied: D259 (stop the patch loop; the scanner card; a BLOCKING definition that converges). Push path: a candidate for the retro pick.
- Idle minutes: none over 10 min with work available, that I know of; every wait had a completion notice. Dispatch nudges on this card since the resume: 3 (a watchdog at 20:51Z that assumed agy had hung; the push stuck on the stale MOB.md; the model pin ruling).

## Shape

Ready (Cursor PM) -> D257 declaration -> architect slice (Claude) -> RED by a Grok writing seat (34 failing by assertion) -> owner pause -> resume: red reproduced in a clean clone (light gate; a play-tracker train alive) -> GREEN by a Cursor writing seat (77 of 77) -> light gate -> second review (Grok died on its balance; agy argon FLAG, 11 BLOCKING, 9 confirmed by the facilitator's replay, 1 refuted) -> rework re-slice (Claude opus, 27 mutants named) -> rework RED (Claude sonnet, 21 failing; a takeover added the Pending traits) -> rework GREEN (Cursor, 112 of 112) -> third review (agy argon FLAG, 7 new BLOCKING from the rework's own pieces, replayed) -> D259 (six of six Yes, all on agy) -> rework light gate (112 of 112, 621 of 621, generator 0 leaks) -> main.

## Things that went wrong or nearly

1. The first green seat ran the full suite during a play-tracker train (against the resume rule) and printed a home path into its own output.
2. The facilitator printed home paths into its own tool output twice (a failed `which` printed PATH; a process listing printed the Git Bash form). Nothing was written to a file.
3. Model allocation: the first green ran before the model-size ruling; the rework architect and red were declared by alias, and the rework green ran on Cursor's unpinned default (corrected after Dispatch's note; `model-allocation.md`).
4. The rework red seat left out the Pending traits (it read the post-green file as the convention); the facilitator added them.
5. A push hid its refusal behind a pipe's exit 0; a later push reported "Everything up-to-date" because the commit had been refused by the home-path hook, and the facilitator reported it as committed.
6. A launch through a shell `&` inside a foreground call left no output file to read; the facilitator could not tell whether the push was alive without a process listing.
7. D259's six counted ballots all came from one service: claude was refused by the rotation rule (qwen and codex counted as waiting though unusable), codex hit its usage limit at launch, and cursor's vote launcher takes no model flag.
8. The second review's BLOCKING definition (any regression on any input) made rounds diverge on contrived compositions; D259 clause 4 narrows it.

## What went well

- The facilitator's replay of every review claim before acting: it refuted claims in both rounds (4c; the `foo-~` row) and caught one input set mangled by the facilitator's own heredoc.
- The strong-tier reviewer (agy argon) found the quadratic blow-ups and surviving mutants the writing seat and the tests missed; the strong-tier architect fixed every confirmed finding in one design with measured evidence.
- The rework is better than what main ran on every measured comparison (extended-sweep leaks 61,514 to 7,807; worst blow-up 7 s to 2 s; generator 0 leaks, 0 second-pass changes).

## Model allocation review (first one; owner ruling 2026-11-14)

See `model-allocation.md` for declared against actual per seat. Outcome signals per seat are in the table there and above. Draft calls for the voices to judge: keep the strong tier for adversarial review and regex design (it found and fixed what the others missed); the mid tier for given-row test placement is adequate but missed a convention (keep, with the convention in the brief); a paste-ready green fits a mid or default tier (keep, but pin it); a Decider vote on a mid tier gave counted, cited votes (keep).
