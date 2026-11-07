# Retro input: card 324 (compiler diagnostics call the unit an avow)

Facilitator's facts, all from git, run logs and the card's records. Read the records named; do not take these summaries on trust where a record says more.

## What shipped
Card 324: the 19 message sites (18 distinct messages) in Resolver.cs (12) and Parser.cs (7) say "avow"; PH codes, positions, tails and CLI exit codes unchanged. Three cycles in the order 2, 1, 3. CI green on the code tip 5646c8a and on the series pushed in two pushes (cycle 1 pair, cycle 3 pair); the records commits pushed after. Decider D207 adopted the card (six Support-it, no No, re-run developer Yes).

## Timeline (local, 2026-11-07)
- D207 adopted 13:40; slice (architect) 13:55; seat plan 13:55; first push 13:58.
- Cycle 2 characterisation (Grok) 14:04 (seat record 14:07). Cycle 1: red (Codex) 14:08, green (Grok) 14:10, pushed 14:12-14:15, CI green on 142e02d. Cycle 3: red (Grok, the scanner and its tests, about 14:15 to 14:28), green (Codex) 14:30. Baseline gate run 1 22:31Z-22:40Z (63 Acceptance failures: stale CLI after the cherry-pick; the freshness guard did its job); gate run 2 after `dotnet build src/Avowry.Cli`: Compiler 324/324, Acceptance 323/323, 9 m 19 s.
- Pre-ship gate reviewer (SHIP) and ergonomics reviewer (nothing to change) about 14:35-14:50; fresh sign-off (SIGNED OFF) 14:55-15:08 including its own 9 m 33 s gate (3 known holdout failures in a git-less archive copy plus a 20 s timing flake that passed alone).
- Decision-to-build: D207 to slice about 15 minutes; build about 32 minutes of seat time over five seat turns; gates and sign-off about 38 minutes. Ratio about 0.5 to 1 against the card 266 figure of 1.8 to 1 (card 197: 1 to 1, card 196: 0.5 to 1, card 299: 1.7 to 1).

## Seat mix (the PO's seat-spread ruling: report it; more than half on one driver is a finding)
- Writing seats (QA reds, greens, characterisation): five turns: Grok 3 (cycle 2 characterisation, cycle 1 green, cycle 3 red), Codex 2 (cycle 1 red, cycle 3 green). Grok carried 3 of 5 = 60%: more than half of writing seats on one driver. Copilot (no quota) and agy (quota reset about 2026-11-14 by the probe; the PO note said 2026-11-08) excluded and recorded in seat-plan.md.
- Read-only seats: PM (Claude), architect (Claude), gate reviewer (Claude), ergonomics reviewer (Claude), sign-off (Claude), six D207 Decider seats (Claude). Claude is the majority of all seats by count; the reason in each case: the independence rule (authors of the commits are Grok and Codex) or no probe window yet. This retro's own voices are the first window under the ruling to probe every driver and seat least-recently-used.

## Red-first evidence (permanent rule: report any born-green to the product owner)
- Cycle 1: LeadingMessagesSayAvow failed 8 of 8 on the pre-change tree (red-record-1.md), green by a different driver.
- Cycle 3: OtherTen failed 11 of 11, the CLI test failed, the live scan failed listing exactly 11 sites (reproduced by the facilitator and by the sign-off in a clone); the two scanner tests (planted tree, pins) failed against a stub first (quoted in red-record-3.md; Grok's own run), then passed with the real scanner.
- Cycle 2: declared characterisation, passes before and after, a drift Fact fails on spacing, article and case drift.
- Born-green: none undeclared (the gate reviewer and the sign-off both judged so). Nothing for the PO to see on this card beyond the note that the scanner's stub-first evidence is record-only (the committed red ships the real scanner).

## Findings and incidents
- The first baseline gate failed 63 Acceptance tests because the Avowry CLI binary was stale after the facilitator's cherry-pick; the freshness guard said "run dotnet build src/Avowry.Cli". A gate step missing from the brief and the procedure: after any cherry-pick of src changes, build the CLI before the gate. Cost about 10 minutes.
- Cycle 3 red: the new C# totalled exactly 400 lines after Grok compacted it to meet the cap (the scanner file is 240 lines, dense). The gate reviewer called the cap inclusive and saw no cramming; the sign-off agreed. A cap hit exactly by compaction is a smell to weigh.
- Scanner limits (gate reviewer and sign-off): `TrackedSourceFiles` reads the working tree despite its name; a split literal "Phr" + "ase" evades the scan; registry key literals and ExtensionKeywords.cs:138 stay pinned as exceptions.
- The sign-off's archive-copy gate reported four failures (three holdout tests need .git; one timing flake) against the facilitator's 323/323: the brief's gate instructions should say "run in a clone, not an archive" (the holdout tests need history) or name the three as known in a git-less copy. The brief's "19 hunks" is true only at -U0.
- Worktree removal after the red cycle failed once ("Permission denied", transient: prune and delete the branch later); the push hook adds a "MOB.md regenerated (stale at push)" commit so ff-merge fails: cherry-pick again.
- Facilitator gate runs, brief writing and byte recomputation took the same shape as card 266: no watchdog fired during the build.

## Experimental section entries this card exercised (for the adjustment review)
- D202 byte-parity check for mechanical rewrites: the facilitator's recompute of the green's bytes from edits.py matched exactly on both greens (cycles 1 and 3); third card with the entry (197, 266, 324). 0 defects found after a clean report.
- D202 isolated gates and sign-off runs with a Load line: three cards; the one gate run that failed was a stale-binary issue, not load.
- D206 (guard tests read committed blobs; the cold-reader seat for docs cards; a red a CI test wraps is pushed with its green): the third entry exercised and held (the cycle 1 and cycle 3 pairs each pushed together; CI never red on the card); the first two have no new evidence (the card's guard tests that scan live state read the working tree on purpose, as the entry allows; no docs card).
- D204 red before the atomic green: not exercised. D193, D196 cite check, D157, D175, D158, D154, D190, D166-D168: each gained a card.

## Queued inputs to weigh
- The Dimensions of Difficulty matrix proposal in docs/retros/experiments.md (routed by the PO for this retro's vote; it enters mob-defaults only if adopted).
- The inbox entries already acknowledged (none open at this boundary).

## Open items carried
Card 326 (README status, unranked Draft) awaits a re-rank window; the Codex ledger is not caught up for cards 299, 303 and 313; 70 open issues still carry `dotnet test Phrase.sln` in Done-when (fix at each card's Ready step); the agy reset-time discrepancy was reported to the PO.
