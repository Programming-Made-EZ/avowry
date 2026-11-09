# Retro input: card 366 (the NOTICE and TRADEMARKS files name the holder and the year), facts from git and run logs (2026-11-09 local times)

Names, addresses, home paths and secret values are not printed here. Read the records named.

## What shipped
NOTICE line 2 now reads "Copyright 2026 Programming Made EZ", TRADEMARKS.md line 5 names the same holder, and docs/LICENSING.md says the holder is named and 2026 is the year the project began, an assumption the product owner can correct (the product owner's answers of 2026-11-08). LICENSE untouched. A placeholder scanner with planted rows, a test that the three files name the holder, and a Python module that the gate had never run (`tests/test_license_files.py`) is now run by the gate through four wrappers (three un-pended, one characterisation). Card 186's placeholder clause was removed in its own loosening commit. Green b5f381a; gate review SHIP; sign-off SIGNED OFF WITH CONDITIONS (card 371 must also fix the stale lines of the pre-publication page).

## Flow metrics and process against build time (local times)
- Ready draft: Cursor through cursor_reply.py with the facts supplied, about 8 minutes (committed 01:02). Decider D231: primary window 01:03 to 01:15 (five Yes, one No from the audit lens), amendment, confirmation window 01:18 to 01:34 (six Yes), recorded 01:35: about 33 minutes from the commit of the proposal to the record, of which about 20 were the audit lens's No and its confirmation step.
- Build: slice 01:40, loosening 01:42, red 01:54, green 02:13: about 38 minutes from the record to the green. Cold reader 02:17, baseline gate and push 02:22 to 02:33, independent gate review 02:50, sign-off 03:16.
- Process against build before the gates: about 0.9 to 1 (33 minutes against 38; the declaration alone, with the Ready draft, about 45).
- Red-first held: characterisation declared first, the scanner's planted rows watched failing against a stub; six ids failed by assertion on the pre-change tree (the facilitator reran them on main; the gate reviewer replayed them); the green changed the test file by the Pending traits only. No born-green finding.

## Seat mix
Card seats beyond votes: Ready draft Cursor; cold reader Qwen (reply-only, inlined); slice, red (own worktree), green (a different context), gate reviewer and sign-off all Claude: five of seven non-vote card seats. D231 votes: Muse 2, Cursor 2, Qwen 1 (the No and its confirmation), Claude 1. Reasons: Grok, Codex, Copilot and agy out of quota; red and green separate contexts and worktrees; the gate review and sign-off must run the gate.

## Experimental entries this card exercised
D228 preflight (third exercise): no harness-lost window (target 0). D230 no idle between steps (first exercise): every wait had a failure-aware monitor; Dispatch nudges 0. D219 as tuned by D226 (the gate review's blocking step 0 and the cold-reader ordering): first docs card since the tuning: the cold reader finished and was committed before the gate review started, the review opened with the ancestor line, gate reviews started on a pre-revision tip 0 (target 0). D222: used (the id lists N, T, G, P, X, L, C); no dropped half. Metric for the D231 audit lens: Qwen voted No on D230 and D231 and on D226 (3 of the last 6 Deciders); each No cost a confirmation window (about 15 to 25 minutes) and its adopted condition restated a practice already in force (committed review records) or added reporting (idle lines).

## Findings
- The Ready draft's facts held on the facilitator's rerun and the architect's by-command check; the architect added the process-start allow-list trap (use ProcessRunner, not a literal process start) that the draft missed; the red then needed no revision.
- The first cold reader run on a docs card since D226: Qwen's UNCLEAR was about external proof of a trademark registration, which the card does not promise; no page revision; a wording note went to card 371.
- The gate had never run `tests/test_license_files.py`; the characterisation wrapper is a real gain (the module guards the Apache text hash and the standing terms).
- A load-sensitive benchmark timeout test failed again in the green seat's full run and the sign-off's first run (two of the five full-gate runs on this card; passes alone and in a quiet rerun); issue 304 is the third sighting class.
- Idle minutes on this card: about 3 (the gap between a green seat's interim notice and the facilitator's next action, one monitor armed late); Dispatch nudges 0.

## Carried
The look-back at D208, D209, D210; D222's adoption; the card 203 follow-ups; the card 361 test-limit cards; card 371 (stale lines).

## Questions for the voices
The adjustment review of D228 (third exercise), D230 (first exercise), D219 as tuned by D226 (first docs card), D222, D204, D202; process against build; the audit lens's three Nos in six Deciders (how to keep a standing objection from costing a confirmation window each time); idle minutes; drivers left; one process improvement candidate.
