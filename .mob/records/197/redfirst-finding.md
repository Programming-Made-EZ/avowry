# Card 197: the born-green tests of criteria 2 and 3 (red-first finding, product owner ruling 2026-11-07)

Facts, from git and a run on the facilitator's machine.

## What was run
The final tests/test_rename_197.py (classes ListsAndPaths = criterion 2, OldCommandRefused = criterion 3) was copied into a clone of main checked out at 21a2caa, the commit of red 1, which is the tree BEFORE any production change of this card (only Phrase.sln, tools/Phrase.Bench* and the old script exist there). Command: `python -m unittest -v tests.test_rename_197.ListsAndPaths tests.test_rename_197.OldCommandRefused` in that clone. Output: redfirst-run-output.txt (12 tests, 6 failures and 2 errors, 4 pass, 50 s).

## Result, test by test
Would have been watched red first for the right reason (they assert the new names or states; 8 of 12, new behaviour):
- ListsAndPaths.test_bench_self_check (the self-check against tools/Avowry.Bench: the project does not exist yet): FAIL
- ListsAndPaths.test_probe_matches_solution (the root probe and the fake layout name the single tracked Avowry.sln): FAIL
- ListsAndPaths.test_run_llm_check_path (the Bench project path in scripts/run-llm-check.py is a tracked path): FAIL
- ListsAndPaths.test_solution_projects (the ten Project lines of Avowry.sln): ERROR
- ListsAndPaths.test_triple_agree (the ADR, the script and its test name the same Avowry Bench paths): FAIL
- OldCommandRefused.test_guid_pairing_all_seven (GUID pairing in Avowry.sln): ERROR
- OldCommandRefused.test_no_compat_files (no tracked Phrase.* path at the root or under tools/): FAIL
- OldCommandRefused.test_old_command_fails (`dotnet test Phrase.sln` is refused): FAIL

Passed before the production change (they guard behaviour that already existed and the rename must preserve; 4 of 12, characterisation):
- ListsAndPaths.test_five_modules (the five path-bound python modules keep their counts): ok
- ListsAndPaths.test_launcher_scan_not_vacuous (the scan regex still matches files): ok
- ListsAndPaths.test_lists_equal_baseline (the two test projects, already renamed by card 196, list the same tests): ok
- OldCommandRefused.test_commands_name_tracked_solution (every `dotnet ... <sln>` command names a tracked solution file): ok, and true both before (Phrase.sln) and after (Avowry.sln), an invariant.

## Which seats wrote production changes before a red existed, and why the slice allowed it
- Criterion 1 followed the rule: red 1 (QA, 21a2caa) was watched failing for the right reason (6 of 7 tests red, quoted in red-record-1.md) before any production change; green 1 (Grok, 2d58c6c) then made the rename.
- For criteria 2 and 3 the production change was already made: the single atomic rename commit (Grok, 2d58c6c, the green of criterion 1) contains every production change of the card, so the tests of criteria 2 and 3 (QA, red 2 a102d64 and red 3 dc1193f) were written after it. Eight of those tests assert new behaviour and were never watched failing before the change; they are test-after. Their mutation patches (P2a to P2f, P3a to P3d, E1, E2) show sensitivity only; per the ruling they are an extra check and do not substitute for a watched red. The run above is a retroactive watched red of those eight tests against the pre-change tree: it is evidence that they would have failed for the right reason, not a substitute.
- Why the slice allowed it: the architect's slice (slice-architect.md, section 3) designed "Cycle 2 (O, M, I), born green" and "Cycle 3 (S, E), born green" after "the atomic rename", the same shape as card 196's slice, and the product manager's task-force proposal (taskforce-proposal-pm.md) said cycles 2 and 3 go green with criterion 1's commit, each shown red only by a named put-back. The six fresh seats of the task-force Decider D203 voted Yes or Support-it on that design (two of them, QA and audit lens, noted the reds were manual put-backs). The design rested on the D193 trial's G2 (a no-red cycle accepted with mutation evidence), which was adopted in the working agreement and is revised by the ruling. The facilitator (this seat) wrote the briefs and accepted the structure at the Decider, the slice, and the red briefs; the finding is the facilitator's too.
- Pattern: the same shape ran on cards 196 (cycles 2 and 3), 299 and 313 (D193 G2 mutation pairs); card 196's red 3 happened to be a real red (it found a missed regex), the others were born green.

## Were criteria 2 and 3 new behaviour or characterisation?
Mixed: 8 of the 12 tests are new behaviour (new names, an old command now refused) and 4 are characterisation of preserved behaviour. The card text did not say so; it should have, up front, per the ruling: criterion 2 (O, M, I) is mostly "the build graph still works under the new names" (4 characterisation, 4 new), criterion 3 (S, E) is "the old command is refused" (new) plus a tracked-command invariant (characterisation).

## What this finding asks of the retro
Record it in the individual and team areas, state the pick, and make the rule operate: a rename card's slice defines each criterion's red BEFORE the atomic green (the new-state assertions can all be written and watched red first, in red 1 or red 2 and red 3 before the green, because they assert the post-rename state), and a characterisation criterion is declared as such on the card.
