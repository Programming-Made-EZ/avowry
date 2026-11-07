# Card 266 red 1 (D157 Criterion 1), QA engineer seat

Criterion 1 (new-state): the nine scoped files hold the old name only on nine pinned exception lines. Run at HEAD 9675b3c (after green 2, before green 1); production files untouched. Class `GuardScan266` (13 tests, T4 dropped by the facilitator) in `tests/test_live_docs_266.py`; `UntouchedBytes266` and `ScopedLinks` are untouched (only the module docstring gained one line and three imports: difflib, tempfile).

## Pins (checked against the slice)
EXCEPTIONS is a literal list of 9 (file, exact full line, allowed token), pinned by content, never by line number. Eight lines are copied from `git show 0fa5b22:FILE`; length and sha256[:16] of each match the slice (HANDOFF 3: 720, 170679d11ddbc334; HANDOFF 83: 701, daaf19d102dde6bb; HANDOFF 99: 308, c92975158dee2d09; HANDOFF 115: 407, ac6c8236c84fc2e7; TWA 57: 458, fe9e1990b788788c; TWA 528: 551, 482f2b04abe3ef31; demo 124: 176, 297bd941c93ef211). Seven are unchanged by the sed rules.
bench/README.md line 19 is pinned as the green produces it: rule "protect the 5 tokens, every Phrase word to Avowry, restore" applied to the base line. Check quoted: base line is 367 bytes, sha256[:16] c39cf44b25b18dbd; the computed line is 367 bytes, sha256[:16] 8cbb6589e1dc65f8 = the slice's expected value (MATCH, asserted by the generator script before it wrote the literal). The ninth pin is the README note.

## Red-first for T13 and T14 (the guard tool itself)
Step 1: `scan_text` written as a stub returning `[]`, T13 and T14 in place. `python -m unittest` on the two tests: Ran 2, FAILED (failures=2), 0.3 s.
```
FAIL: test_t13_an_old_name_at_a_line_end_is_flagged_with_lf_and_with_crlf
AssertionError: Lists differ: [(2, 'the name is Phrase')] != [] : line ending '\n'
FAIL: test_t14_the_scan_is_not_vacuous
AssertionError: Lists differ: [1, 3, 4, 5] != []
Ran 2 tests ... FAILED (failures=2)
```
Step 2: the real scan (LF split, one trailing CR stripped, `\bPhrase\b|Phrase[A-Za-z]|phrase[-_][A-Za-z]`, case-sensitive). T13 and T14 then pass (see the run below). A name at a line end is a `\bPhrase\b` hit; T13 proves LF, CRLF and a last line without newline.

## The watched reds today
Command: `python -m unittest tests.test_live_docs_266.GuardScan266`. Result: Ran 13, FAILED (failures=11, errors=0), 4.4 s. Passing: T13 and T14 (the tool tests). Each failure is for the right reason:
- T1: `9 != 34 : 34 hit lines, 27 unexpected, 2 stale pins` (the 34 hit lines match the slice's prediction: 39 at base minus 5 demo lines changed by green 2; 7 pins matched, bench 19 and the note are the 2 stale pins).
- T2: the two missing pins only: `bench/README.md: pinned line found 0 times` and `README.md: pinned line found 0 times` (the other seven pass found-once, token present, clean after masking).
- T3: `layout paths that are not tracked: ['src/Phrase.Providers.CSharp']`.
- T5: `the note line appears 0 times in README.md`.
- T6: `'renamed in a later card' unexpectedly found` in README.
- T7: `'small fixed core, open avow library' != 'small fixed core, open phrase library'`.
- T8: six of the seven sed files differ from the rules applied to BASE: TEAM_WORKING_AGREEMENT.md (old 90716, expected 90716, committed 90716, first differing line 1), demo/tasks/README.md (3956 / 3957 / 3956, line 3: the sentence `is now a Phrase update` is one byte longer as `an Avowry update`), bench/README.md (2533 / 2533 / 2533, line 1), bench/holdout-template.md (3922 / 3922 / 3922, line 6), E1 (7136 / 7136 / 7136, line 23), E4 (2566 / 2566 / 2566, line 11). HANDOFF.md is not listed: the rules change nothing in it (only pinned token lines).
- T9: README differs from BASE-plus-hand-edits (note absent, Providers row still present, old sentence still on line 5) and VISION line 76 still says `open phrase`.
- T10, T11, T12: each fails at the precondition `the pristine copy does not scan clean (34 hit lines)`: the right-reason red, as the brief predicted. The put-back half is only exercised in the pretend green (below).

No test that should be red passed today. Nothing was born green except T13 and T14, which are the tool tests and were watched failing against the stub first.

## Pretend green (scratch clone, never committed here)
`pretend_green.py` (scratchpad): `git clone` of the main checkout in a TemporaryDirectory; BASE bytes of the seven sed files transformed by the REAL `sed -b -i -f r266.sed`; hand edits (README line 5, the note as new line 6, the three layout rows, VISION line 76); committed in the clone only as `Card 266 green 1 (pretend, scratch clone only)` so that T8 and T9 read a green commit's blobs.
Result: `GuardScan266`: Ran 13, OK (4.6 s). The whole module `tests.test_live_docs_266`: Ran 17, OK. The scan finds exactly 9 hit lines, all pinned (README 6, HANDOFF 3, 83, 99, 115, TWA 57, 528, demo 124, bench 19). This also proves the Python byte rules in T8 equal what the real sed script produces (CRLF lines of E1 and the +1 byte of the demo README included), and that the pins and byte rules are satisfiable and consistent with the green recipe.

## Mutation checks (extra evidence only)
Patches in `.mob/records/266/mutations/`, relative to the pretend green; each was applied in the pretend-green scratch clone, the class run, then reverted with `git apply -R` (class OK again after each revert). `git apply --check` and `git apply` exit 0 on a `git archive` copy of the pretend-green commit. They cannot apply to a `git archive HEAD` of the pre-green tree (M3 deletes a note that is not there yet; M2 patches the test class this commit adds).
- M1 `M1-put-phrase-back-in-readme-line-3.patch`: Ran 13, failures=4: T1 (a 10th hit, unexpected README.md:3), T10, T11, T12 (the pristine precondition fails).
- M2 `M2-add-a-stray-pin.patch` (a 10th pin that exists nowhere): failures=6: T1, T2 (stale pin), T10 to T12, T14 (fewer hits than pins).
- M3 `M3-delete-the-readme-note.patch`: failures=7: T1 (8 hits, 1 stale pin), T2, T5, T10 to T12, T14.

## Things the facilitator must know
1. T14's non-vacuity on the real tree asserts at least as many hit lines as pins (9), not "at least 9 pinned lines": the brief also wants T14 to pass once the helper exists, and today only 7 of 9 pins match, so the pinned-lines version would be red today. T1 and T2 carry the pin matching; the pretend green shows exactly 9.
2. T8 and T9 compare the newest `Card 266 green` commit's blobs (the working tree only if there is none). So a mutation of an uncommitted working-tree file is not seen by T8 and T9 in a clone where green 1 is committed (M1 and M3 are caught by T1, T5, T10 to T12 instead). The green seat must commit before T8 and T9 can pass.
3. T9 compares README as non-blank lines (a blank line next to the note is tolerated, since the slice does not say whether the note stands alone); all other README bytes, and VISION byte for byte, are pinned. The layout rows are pinned as the slice gives them (description from column 36).
4. T8's check that the rules change something guards against a vacuous run; the Python rules use ASCII `\b` (the real sed produced identical bytes here).
5. Cost: the class runs in about 4.5 s (T8, T9, T10 to T12 call git); no dotnet, nothing deleted by command (scratch copies live in TemporaryDirectory).
