# Card 366 red (D157 Criteria 366-1, 366-2, 366-3), QA (red) seat

Three Pending new-state wrappers committed together (one atomic fill turns them green together; D204 over D137's one-pending rule, as on card 361). No production file changed (NOTICE, TRADEMARKS.md, docs/LICENSING.md, LICENSE are untouched). Base: `4476596`. Commit 1 (loosening, not a red): `fc32c368f3cabd33af6b27074749babf76f6079b`. Characterisation record: `characterisation-record.md` (same commit as this record).

## D222 list: the Ready draft's rows first

| id | construct | kind |
|---|---|---|
| N1 | NOTICE line 2 holder | NEW-STATE (criterion 1) |
| N2 | NOTICE line 2 year | NEW-STATE |
| N3 | exactly one `Copyright ` line in NOTICE | CHARACTERISATION (boundary, kept from the old test) |
| T1 | TRADEMARKS.md line 5 holder | NEW-STATE |
| G1 | NOTICE has no placeholder hit | NEW-STATE (criterion 2) |
| G2 | TRADEMARKS.md has no placeholder hit | NEW-STATE |
| P1 | scanner reports `[YEAR]` | NEW-STATE unit (stub first) |
| P2 | scanner reports the full card 186 text | NEW-STATE unit (stub first) |
| P3 | scanner reports an all-caps bracket token | NEW-STATE unit (stub first) |
| P4 | scanner reports "to be confirmed" | NEW-STATE unit (stub first) |
| X1 | scanner ignores a markdown link | NEGATIVE row, passes always |
| X2 | scanner ignores the filled NOTICE line | NEGATIVE row, passes always |
| L1 | docs/LICENSING.md names the holder, 2026 and "assumption", no "marked placeholder" | NEW-STATE (criterion 3) |
| C1 | LICENSE hash | CHARACTERISATION |
| C2 | the four standing-terms tests | CHARACTERISATION |

## What the slice changed

| change | reason |
|---|---|
| commit 1: `PLACEHOLDER` constant, the `placeholder`, `notice`, `trademarks` context keys and the `owner-items` placeholder clause removed from `tests/test_pre_publication_checks.py`; the 6-OPEN check kept; local `ph`, `notice`, `marks` removed (no dead code) | filling the files would turn card 186's gated test red; the check moves to G1/G2, which is stricter; the placeholder text is now defined once |
| `test_notice_and_trademarks_carry_the_marked_placeholder_until_the_owner_confirms_the_holder` deleted, replaced by `test_notice_and_trademarks_name_the_holder_and_the_year` (subTests N1, N2, N3, T1) | the old test pins the placeholder; its one-Copyright-line assert moved in as N3 |
| `placeholder_hits(text)` added to `tests/test_license_files.py`; hit format `line <n>: <label>` with labels `card-186-text`, `[YEAR]`, `to-be-confirmed`, `bracket-token`, one per label per line, in that order | pure scanner, exact hit lists asserted; the bracket token pattern is case-sensitive all-caps and its lookahead ignores markdown links |
| tests `test_notice_and_trademarks_hold_no_placeholder` (G1, G2), `test_the_placeholder_scanner_reports_planted_placeholders` (P1 to P4), `test_the_placeholder_scanner_ignores_links_and_the_filled_holder` (X1, X2), `test_the_licensing_page_names_the_holder_and_the_year_assumption` (L1) | the ids above |
| new `tests/Avowry.Acceptance.Tests/LicenseFiles.cs`: three Pending wrappers (`Slice=366`, `Criterion=366-1/2/3`) and one non-Pending characterisation wrapper (`Slice=366`, no criterion) running C1 and C2 | wrapper design of the slice; `ProcessRunner` so the process-start guard is untouched |

Deviations: N1 and N2 split the NOTICE line into three parts (holder is the third, year the second; N2 also fixes the word `Copyright`); the scanner returns one hit per label per line, not one per match. No rename or count test needed adjusting. The Pending trait removal for the failing run was done on the new file only and restored (3 Pending traits verified by grep).

## Commands and pre-change results

Commit 1, on the unfilled tree:
- `python -m unittest tests.test_pre_publication_checks`: `Ran 6 tests ... OK`
- `dotnet build src/Avowry.Cli`: `0 Error(s)`
- `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~PrePublicationChecks"`: `Passed!  - Failed: 0, Passed: 1, Skipped: 0, Total: 1`

Stub scanner (`placeholder_hits` returned `[]`), the scanner and G tests: P1 to P4 failed by assertion, X1 and X2 passed (G1 and G2 also passed, vacuously, against the stub):

```
FAIL (id='P1')  AssertionError: Lists differ: ['line 1: [YEAR]', 'line 1: bracket-token'] != [] : Copyright [YEAR] Acme
FAIL (id='P2')  AssertionError: Lists differ: ['line 1: card-186-text', 'line 1: [YEAR]', ...4 hits] != []
FAIL (id='P3')  AssertionError: Lists differ: ['line 1: bracket-token'] != [] : (the planted holder-name line)
FAIL (id='P4')  AssertionError: Lists differ: ['line 2: to-be-confirmed'] != []
```

Real scanner, `python -m unittest -v tests.test_license_files` on the pre-change tree (`Ran 10 tests`, `FAILED (failures=6)`):

```
FAIL G1  AssertionError: Lists differ: [] != ['line 2: card-186-text', 'line 2: [YEAR]'[48 chars]ken']
FAIL G2  AssertionError: Lists differ: [] != ['line 5: card-186-text', 'line 5: to-be-confirmed', 'line 5: bracket-token']
FAIL N1  AssertionError: 'Programming Made EZ' != '[COPYRIGHT HOLDER: to be confirmed by the[52 chars]86)]'
FAIL N2  AssertionError: Lists differ: ['Copyright', '2026'] != ['Copyright', '[YEAR]']
FAIL T1  AssertionError: 'Trademark holder: Programming Made EZ' != 'Trademark holder: [COPYRIGHT HOLDER: to be confirmed by the[52 chars]86)]'
FAIL L1  AssertionError: 'Programming Made EZ' not found in '# Licensing ...'
```

Pass: N3, P1 to P4 (real scanner), X1, X2, C1 and the four C2 tests.

Wrappers with the Pending traits removed (uncommitted, restored): `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=366"`:

```
Failed Notice_and_trademarks_hold_no_placeholder_and_the_scanner_catches_one   (G1, G2)
Failed Notice_and_trademarks_name_the_holder_and_the_year                      (N1, N2, T1)
Failed The_licensing_page_names_the_holder_and_the_year_assumption             (L1)
Failed!  - Failed: 3, Passed: 1, Skipped: 0, Total: 4
```

The passing one is `The_license_and_standing_terms_are_unchanged` (C1, C2).

Mutation (uncommitted, restored with `git checkout -- LICENSE`): appending a line to LICENSE makes C1 fail: `AssertionError: 'cfc7749b...523d30' != '2b9baa3f...73322e'`; restored, `OK`.

Python suites: `python -m unittest tests.test_license_files tests.test_pre_publication_checks tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266`: `Ran 67 tests`, `FAILED (failures=14)` = the 6 new-state ids above plus the 8 pre-existing failures (five `test_rename_196`: eol x2, lists_equal x2, new_dirs_tracked_old_gone; two `test_rename_197` lists_equal_baseline; T11 in `test_live_docs_266`).

Gate: `dotnet test Avowry.sln --nologo --filter "Status!=Pending"` with the three Pending wrappers committed: `Passed! - Failed: 0, Passed: 344 (Avowry.Compiler.Tests)` and `Passed! - Failed: 0, Passed: 377 (Avowry.Acceptance.Tests)`; the process-start guard stays green.

## Test limits

- N3 and the C ids pass on the pre-change tree by design; they cannot fail on it (C1 was shown failing under a mutation).
- G1 and G2 pass vacuously against a stub scanner; P1 to P4 are what prove the scanner is not vacuous.
- The scanner is line-based: a placeholder split across lines is not seen. The bracket-token pattern only matches an all-caps start, so a mixed-case bracket token would pass; the other three patterns cover the known text.
- N1, N2 and T1 read line 2 of NOTICE and line 5 of TRADEMARKS.md by position; a line inserted above would fail them (a wanted failure, not a silent pass).
- L1 asserts substrings in order; after the first missing needle the rest are not reported in that run.
- Nothing checks that the year 2026 is correct: it is the owner's assumption.
