# Retro input: card 202 (a native block that fails with an undeclared code is a diagnostic)

Facilitator's facts from git, run logs and the records. Read the records named.

## What shipped
A resolver scan of each leaf avow's `native csharp` body for a literal `Fail("X")` (comments and string and char literals consumed so they cannot match; qualified calls, computed codes and other targets unchecked and named in a GRAMMAR.md clause): an undeclared X now fails the build with `PH337 (line:col)` naming the code, the leaf's template, the line and the fix, before emission. `NativeBlock` carries its body position; the corpus fixture `undeclared-native-failure` flips from builds to PH337 at line 5 (data only, the theory test untouched). One Pending-then-active test with an id per row (15 ids: 7 failing, 8 keep-building) in one aggregated assertion; the existing tests as the characterisations of criteria 2 and 3 (a declared Fail still builds and records; the corpus theory and the orders output hash unchanged).

## Flow metrics (local time, 2026-11-08, from git and run logs) and process against build time
- Ready draft launched 07:37 (a Grok PM seat, about 6 minutes), the facilitator's reruns (the fixture builds today; a script over the 41 tracked .avowry files: 18 Fail literals, only the target fixture undeclared) and record assembly 07:43, Decider D223 (window about 6 minutes, six Yes, no re-run: trigger (d)) adopted 08:02: about 25 minutes from the draft launch to the adopted record.
- Build: slice 08:07 (the architect ran 15 ids through the CLI), characterisation record 08:11, red 08:15, green 08:20 (about 18 minutes from the adopted record to the green), baseline gate 08:21 to 08:30, pushed 08:31, independent gate review 08:55, sign-off about 15 minutes later. Process against build time: about 1.4 to 1 before the gates (25 minutes of declaration against about 18 of building); the gates and reviews took about 50 minutes.
- D217 trial: the five counted Deciders D218 to D222 are done; D223 is the sixth declaration run under the rule (trigger (d)); the verdict is due at this retro.

## Seat mix (the product owner's clarification: report Claude's share and the reason for every Claude seat)
- Card seats (about 17): Ready PM Grok; D223 window Grok 4 (architect, QA, decider, coach after the Muse seat fell back), Muse 1 (developer), Claude 1 (audit lens); slice Claude; characterisation record and red Grok; green Claude; gate reviewer Claude; sign-off Claude. Claude share: audit lens, slice, green, gate reviewer, sign-off: 5 of about 17. Muse 1, Grok 11.
- Reasons for the Claude seats: Codex out of quota until about 2026-11-11, Copilot and agy out of quota, Grok wrote the PM draft and the red (a red and its green are never the same seat), the independence rule leaves no non-author non-trial driver in quota for the gate and the sign-off, the audit lens stays on a different model family.

## Red-first (permanent rule; report any born-green to the product owner)
Criteria 2 and 3 were characterisations that already existed and passed on the pre-change tree (47 tests, plus a mutation of the orders hash shown failing); the single Pending red was watched failing by assertion with exactly the 7 failing ids and none of the 8 keep-building ids; the green removed only the Pending trait; the test file is byte-identical. No test was born green undeclared.

## D222 (first exercise)
The red record listed the MULTI Ready row with its two ids first and then every hostile id with its source; the gate reviewer compared the list with the Ready draft and found no dropped construct (one tag was wrong: two ids came from Ready scenarios [M] and [E], not from the slice; corrected). No HOLD this card.

## Incidents and findings
- The architect's slice found two facts the PM draft missed: native blocks occur only on a leaf avow (an endpoint-step native block is a parse error, a composite cannot have one), and the `no-clause` fixture's emitted C# cannot compile (the `Fail` helper is emitted only when some avow declares failures): the card turns it into a diagnostic and does not change the helper emission.
- The gate reviewer found a plausible gap that still builds: `Fail ("X")` with a space before the parenthesis (bug card 357); the sign-off added an indirect method-group call. The column remains unpinned (bug card 352). Two mutations survive in the reviewer's run (the dedupe, harmless; the column).
- The Muse coach seat fell back to Grok again (one of two Muse seats; the other counted). Nothing else failed in the windows.
- The benchmark 20-second flake (issue 304) did not occur in any of the three full runs of this card.
- The facilitator's own probe of the repro had a syntax error (a bare `respond with r`); it was replaced by the corpus fixture and a script over all tracked sources, which settled the PM draft's claim with evidence.

## Carried to this retro (the mob's own questions)
The D217 trial verdict (adopt, tune or revert) with the five counted Deciders and the sixth run; the look-back at D208, D209 and D210 with the corrected confirmation step (to be posted to Discussions threads 328, 329, 330); posting each retro to the Discussions (done for cards 178, 179, 189); the two trials running (D222, 1 of 3 code cards used; D219, 0 of 3 docs cards).

## Questions for the voices
The adjustment review of the Experimental entries this card exercised (D217 trigger (d) again; D222 first exercise; D204 and the characterisations-as-existing-tests; D202; D174; the standing-rules preflight), the process-against-build ratio, the D217 verdict (give your position and the evidence), and one improvement candidate that is not a process loop (keep the retro lean; if you have none, say 'kept, no change').
