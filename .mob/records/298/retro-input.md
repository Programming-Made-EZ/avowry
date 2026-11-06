# Card 298 retro input (facilitator; facts only)

Card: a second decision run on the same checkout never votes unprotected (bug, vote lock). Signed off with conditions (.mob/records/298/signoff-ctx6.md); follow-up bugs 313 (a run that starts as another ends can be left without a lock), 314 (a crashed run's temporary holder file is never cleaned), 315 (a run that cannot register starts unprotected, no warning), earlier 307 (two overlapping runs fail each other's seat-use history check). Code in mob-crosscli (mob-defaults), records here.

## Flow (UTC = local + 8 h for git times)
- Task-force window (Codex green seat with fallback), card text fixed; slice by a Claude architect (design: one holder file per run in mob-decider.lock.d), three cycles of one criterion each.
- Cycle 1: QA red (a commit and push after the first run ends are refused while the second votes), red 1b corrected an over-assertion (the first red also asserted integrity ok, which fails because of bug 307); Codex green 1 stopped without committing because the red over-asserted; facilitator committed green 1 (fd4c702, hold_vote_lock rewritten). Cycle 2: the two either-order tests PASSED on green 1 (no red); accepted on mutation evidence (two mutations each fail the matching test, in a scratch copy). Cycle 3: QA red had 2 failing tests and 2 characterization tests (mutation evidence); Codex green 3 (18 commands, no delete, one explicit-path git add, no push; commit 1590679), LiveVoteOverlap 7 plus LiveVote 8 OK.
- Whole mob-crosscli suite: 1855 tests, one failure (tests.test_loop_state.test_lock_released_when_holder_exits), identical on the base commit (a 0.2 s startup timing flake, no bug card filed). The suite took 2393 s. Watchdog: no push for about 67 minutes while waiting for it.
- Gate (Copilot): SHIP, every check PASS, it did not run LiveVote or writer-isolation. Sign-off (fresh Claude): SIGNED OFF with conditions, and its rerun probes found three real defects the gate did not (cards 313 to 315). Second card in a row where the sign-off found what the gate missed.
- Product-owner inbox this card: ruling that process changes are filed as story issues (Connextra persona, Gherkin) and D192's options a, b, c are all declined; stories 308 to 312 filed (decision channel, card format, Muse trial, coach conformance, tally checker); working agreement and D183 scope updated.
- Process notes: a stuck card-303 probe on plain `codex` (18 min, stopped by its own PIDs only; probes now go through the launcher); Muse trial still outstanding; the cycle 2 "no red" case had no rule.

## Adjustments in force (D155)
D190 (gate labels, second card), D189 (numbers carry commands), D188 (in-flight probe refusal, no firing), the product-owner inbox (now to be replaced by story 308), the strict Decider reading (followed exactly; no Decider window ran this card except none, so unknown).

## Queued inputs to weigh
Stories 308 to 312 are filed and are the live items for their topics; this retro's pick is a candidate story, not an experiment entry. A rule for a cycle whose tests already pass (characterization plus mutation evidence). Whether a gate should be required to run the whole-suite-adjacent narrow tests or the sign-off keeps that role.
