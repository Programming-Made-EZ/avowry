# Card 371 red (D157 Criteria 371-1, 371-2, 371-3), QA (red) seat

Three Pending new-state wrappers committed together (one page edit turns them green together; D204 over D137's one-pending rule, as on cards 361 and 366). No production file changed: `docs/release/pre-publication-checks.md`, NOTICE, TRADEMARKS.md, docs/LICENSING.md and LICENSE are untouched. Base: `1a82c2f`. Commit 1 (the card 186 revision, a loosening, not a red): `935573555c547e056e8a48b1acb38248f65a60f1`. Characterisation record: `characterisation-record.md` (same commit as this record).

## D222 list: the Ready draft's rows first

| id | construct | kind |
|---|---|---|
| A1 | name item: Avowry confirmed (D125), registration status pending the owner's trademark search | NEW-STATE (371-1) |
| A2 | holder item: Programming Made EZ, 2026 an assumption, same holder for the trademark line (card 366) | NEW-STATE |
| A3 | search item: "commissioned by the owner; pending result", publishing the name waits on the result | NEW-STATE |
| A4 | lawyer item: skipped by owner decision | NEW-STATE |
| A4q | lawyer question 4 marked answered by the holder item | NEW-STATE |
| A5 | author identity: keep | NEW-STATE |
| A6 | vendor terms: read-first (card 367) | NEW-STATE |
| A7 | reply_head: trim for new rows only, pushed rows stay public (card 368) | NEW-STATE |
| A8 | transitive licences: restore-and-review (card 369) | NEW-STATE |
| A9 | home paths: scrub-tracked, history stays public (card 370) | NEW-STATE |
| A10 | exactly nine owner items | CHARACTERISATION (boundary) |
| A11 | every top-level item reads "answered 2026-11-08" | NEW-STATE |
| E1 | planted page: search item reverted to OPEN | NEW-STATE unit (stub first) |
| E2 | planted page: holder item deleted (eight items) | NEW-STATE unit (stub first) |
| E3 | planted page: "; pending result" cut | NEW-STATE unit (stub first) |
| E4 | planted page: lawyer item reads "ask" | NEW-STATE unit (stub first) |
| E5 | planted page: a tenth item | NEW-STATE unit (stub first) |
| Z1 | zero OPEN in Owner items | NEW-STATE (371-2) |
| S1 | `:20` "stay open" | NEW-STATE |
| S2 | `:93` "needs a professional search" | NEW-STATE |
| S3 | `:96` "commission a professional" | NEW-STATE |
| S4 | `:100` "Every item is open" | NEW-STATE |
| S5 | `:103` "year placeholder" | NEW-STATE |
| S6a, S6b, S6c | `:116` "holder needed", "lawyer ask", "stay until the holder is named" | NEW-STATE |
| S7 | `:62` "decide the home-path" | NEW-STATE |
| S8 | `:74` "the owner decides" | NEW-STATE |
| S9 | `:26` "an open check" (not in the Ready draft; added by the slice) | NEW-STATE |
| F1 to F5 | planted stale phrase (one each: "stay open", "needs a professional search", "Every item is open", "year placeholder", "an open check") | NEW-STATE unit (stub first) |
| F6 | planted OPEN token inside Owner items | NEW-STATE unit (stub first) |
| X1 | the stale check reports nothing on the answered fixture page | NEGATIVE row, passes always |
| B1 | brief names the applicant and the mark Avowry | NEW-STATE (371-3) |
| B2 | class 9 with its gloss | NEW-STATE |
| B3 | class 42 with its gloss | NEW-STATE |
| B4 | the class numbers on the Classes line are exactly 9 and 42 | NEW-STATE |
| B5 | jurisdictions line with its "proposed by the mob; the owner confirms with the provider" label and the three markets | NEW-STATE |
| B6 | all seven `TARGETS` with "free on 2026-10-29 (D125); not checked since." | NEW-STATE |
| B7 | the two "not checked" lines (trademark databases and the earlier brand called Phrase; registry state after 2026-10-29) | NEW-STATE |
| B8 | link to the Name collisions section | NEW-STATE |
| 186 | card 186's own module (`tests.test_pre_publication_checks`) | CHARACTERISATION, passes before and after commit 1 |

## What the slice changed

| change | reason |
|---|---|
| commit 1: the six `- ...: OPEN, asked 2026-11-08` fixture rows and the whole `elif cid == "owner-items":` block (the at-least-6-OPEN check) removed from `tests/test_pre_publication_checks.py`; the `## Owner items` heading, `HEADINGS` entry and `Recommendation: ask.` kept | the green makes "at least 6 OPEN items" false; the card 186 wrapper is gated, so the assertion moves to the new module (A, Z, S ids). A removed check cannot fail, so the gated wrapper passes before and after |
| S9: stale phrase `an open check` for `:26` ("treat the transitive licences as an open check") | the slice found `:26` stale and missing from the draft; the lowercase "open" escapes an uppercase OPEN scan, so it is a phrase |
| new `tests/test_pre_publication_owner_items.py`: parser, `owner_item_failures`, `stale_failures`, `brief_failures` and the `ANSWERED` fixture page; imports only `PAGE`, `sect`, `TARGETS`; reads the page with `\r\n` normalised | the module design of the slice |
| failure strings: `owner-items: count: expected 9 items, found N`; `owner-items: <key>: not answered 2026-11-08`; `owner-items: <key>: missing "<phrase>"`; `owner-items: lawyer-q4: not marked answered by the holder item`; `owner-items: open: line N: OPEN`; `owner-items: stale: line N: "<phrase>"`; `owner-items: brief: missing "<what>"` | as the slice, so the green and the reviewer can match them |
| new `tests/Avowry.Acceptance.Tests/PrePublicationOwnerItems.cs`: three Pending facts (`Slice=371`, `Criterion=371-1/2/3`) and one non-Pending characterisation fact (`Slice=371`, no `Criterion`) running A10 and X1; `ProcessRunner`, no literal process start | wrapper design of the slice |

Deviations:
- A4q is its own subTest (the Ready table lists it; the user's id list names A1 to A9 and A11 only).
- Extra subTest ids not in the Ready table: `B0` (the `### Trademark search brief` heading exists), `X1-owner-items` and `X1-brief` (the answered fixture satisfies the other two checkers, so the planted-page rows stay meaningful). `B0` fails now like the B ids; the two X1 siblings pass.
- The 371-1 Pending wrapper runs `test_each_owner_item_shows_its_answer` and `test_the_owner_items_check_reports_a_missing_answer` (A and E); the 371-2 wrapper runs the Z and S test and the F test; 371-3 runs the B test. The slice's test names are used; the slice called the F test `test_the_stale_check_reports_a_planted_phrase`.
- No rename or count test needed adjusting: the same seven rename failures appear with and without my new files (checked by stashing them).

## Commands and pre-change results

Commit 1, on the unrevised page:
- `dotnet build src/Avowry.Cli`: `0 Error(s)`
- `python -m unittest tests.test_pre_publication_checks`: `Ran 6 tests ... OK`
- `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~PrePublicationChecks"`: `Passed!  - Failed: 0, Passed: 1, Skipped: 0, Total: 1`

Stub first (`owner_item_failures` and `stale_failures` returned `[]`; the parser and `brief_failures` real), `python -m unittest -v tests.test_pre_publication_owner_items`. The E and F rows failed by assertion against the stub (the A and Z and S rows passed vacuously, as expected, and the stale-check X1 passed):

```
FAIL (id='E1')  AssertionError: Lists differ: ['owner-items: search: not answered 2026-11-08'] != []
FAIL (id='E2')  AssertionError: Lists differ: ['owner-items: count: expected 9 items, fo[50 chars]-08'] != []
FAIL (id='E3')  AssertionError: Lists differ: ['owner-items: search: missing "commissioned by the owner; pending result"'] != []
FAIL (id='E4')  AssertionError: Lists differ: ['owner-items: lawyer: missing "skipped by owner decision."'] != []
FAIL (id='E5')  AssertionError: Lists differ: ['owner-items: count: expected 9 items, found 10'] != []
FAIL (id='F1')  AssertionError: Lists differ: ['owner-items: stale: line 47: "stay open"'] != []
FAIL (id='F2')  AssertionError: Lists differ: ['owner-items: stale: line 47: "needs a professional search"'] != []
FAIL (id='F3')  AssertionError: Lists differ: ['owner-items: stale: line 47: "Every item is open"'] != []
FAIL (id='F4')  AssertionError: Lists differ: ['owner-items: stale: line 47: "year placeholder"'] != []
FAIL (id='F5')  AssertionError: Lists differ: ['owner-items: stale: line 47: "an open check"'] != []
FAIL (id='F6')  AssertionError: Lists differ: ['owner-items: open: line 47: OPEN'] != []
```

The stub was not committed; the real checkers are in the committed module. With the real checkers, E1 to E5, F1 to F6 and X1 pass (`test_the_owner_items_check_reports_a_missing_answer ... ok`, `test_the_stale_check_reports_a_planted_phrase ... ok`, `test_the_stale_check_ignores_the_answered_page ... ok`, `test_the_page_has_nine_owner_items ... ok`).

Real checkers, `python -m unittest -v tests.test_pre_publication_owner_items` on the unrevised page (`Ran 7 tests`, `FAILED (failures=32)`):

```
FAIL A1   Lists differ: [] != ['owner-items: name: not answered 2026-11-08']
FAIL A2   Lists differ: [] != ['owner-items: holder: not answered 2026-11-08']
FAIL A3   Lists differ: [] != ['owner-items: search: not answered 2026-11-08']
FAIL A4   Lists differ: [] != ['owner-items: lawyer: not answered 2026-11-08']
FAIL A4q  Lists differ: [] != ['owner-items: lawyer-q4: not marked answered by the holder item']
FAIL A5   Lists differ: [] != ['owner-items: author: not answered 2026-11-08']
FAIL A6   Lists differ: [] != ['owner-items: vendor: not answered 2026-11-08']
FAIL A7   Lists differ: [] != ['owner-items: reply-head: not answered 2026-11-08']
FAIL A8   Lists differ: [] != ['owner-items: transitive: not answered 2026-11-08']
FAIL A9   Lists differ: [] != ['owner-items: home: not answered 2026-11-08']
FAIL A11  Lists differ: [] != ['- Public name: OPEN, asked 2026-11-08. C[1126 chars]c).']
FAIL Z1   Lists differ: [] != ['owner-items: open: line 102: OPEN', 'own[286 chars]PEN']
FAIL S1   Lists differ: [] != ['owner-items: stale: line 20: "stay open"']
FAIL S2   Lists differ: [] != ['owner-items: stale: line 93: "needs a professional search"']
FAIL S3   Lists differ: [] != ['owner-items: stale: line 96: "commission a professional"']
FAIL S4   Lists differ: [] != ['owner-items: stale: line 100: "Every item is open"']
FAIL S5   Lists differ: [] != ['owner-items: stale: line 103: "year placeholder"']
FAIL S6a  Lists differ: [] != ['owner-items: stale: line 116: "holder needed"']
FAIL S6b  Lists differ: [] != ['owner-items: stale: line 116: "lawyer ask"']
FAIL S6c  Lists differ: [] != ['owner-items: stale: line 116: "stay until the holder is named"']
FAIL S7   Lists differ: [] != ['owner-items: stale: line 62: "decide the home-path"']
FAIL S8   Lists differ: [] != ['owner-items: stale: line 74: "the owner decides"']
FAIL S9   Lists differ: [] != ['owner-items: stale: line 26: "an open check"']
FAIL B1   Lists differ: [] != ['owner-items: brief: missing "Applicant: [63 chars]ry"']
FAIL B2   Lists differ: [] != ['owner-items: brief: missing "9 (downloadable software)"']
FAIL B3   Lists differ: [] != ['owner-items: brief: missing "42 (software development services)"']
FAIL B4   Lists differ: [] != ['owner-items: brief: missing "classes 9 and 42 only"']
FAIL B5   Lists differ: [] != ['owner-items: brief: missing "Jurisdictio[204 chars]om"']
FAIL B6   Lists differ: [] != ['owner-items: brief: missing "NuGet packa[584 chars]e."']
FAIL B7   Lists differ: [] != ['owner-items: brief: missing "Trademark d[134 chars]d."']
FAIL B8   Lists differ: [] != ['owner-items: brief: missing "[Name colli[48 chars]h)"']
FAIL B0   'owner-items: brief: missing "### Trademark search brief"' unexpectedly found in [...]
```

Pass: A10, X1 (with X1-owner-items and X1-brief), E1 to E5, F1 to F6. The page line numbers above (20, 26, 62, 74, 93, 96, 100, 103, 116) are the stale lines the green must rewrite.

Wrappers with the three Pending traits removed (uncommitted, restored; 3 Pending traits verified by grep): `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=371"`:

```
Failed Each_of_the_nine_owner_items_shows_its_answer     (A1 to A9, A4q, A11 by assertion)
Failed No_page_line_still_calls_an_answered_item_open    (Z1, S1 to S9 by assertion)
Failed The_page_carries_the_trademark_search_brief       (B0 to B8 by assertion)
Failed!  - Failed: 3, Passed: 1, Skipped: 0, Total: 4
```

The passing one is `The_owner_items_stay_nine_and_the_stale_check_ignores_the_answered_page` (A10, X1).

Mutation (uncommitted, restored with `git checkout -- docs/release/pre-publication-checks.md`): adding a tenth top-level item line to Owner items makes the characterisation fact fail: `dotnet test ... --filter "Slice=371&Status!=Pending"` gives `AssertionError: 9 != 10`, `Failed: 1`; restored, `Passed: 1`.

Gate and Python suites with the Pending facts committed:
- `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`: `Passed! - Failed: 0, Passed: 344` (Avowry.Compiler.Tests) and `Passed! - Failed: 0, Passed: 381` (Avowry.Acceptance.Tests, 9 m 58 s); no failures.
- `python -m unittest tests.test_pre_publication_checks tests.test_pre_publication_owner_items tests.test_license_files tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266`: `Ran 74 tests`, `FAILED (failures=40)` = the 32 new-state ids above (A1 to A9, A4q, A11 = 11; Z1 and S1 to S9 = 12; B0 to B8 = 9) plus the 8 pre-existing failures (five `test_rename_196`: eol x2, lists_equal x2, new_dirs_tracked_old_gone; two `test_rename_197` lists_equal_baseline; T11 in `test_live_docs_266`). `test_pre_publication_checks` and `test_license_files` pass.

## Test limits

- The checkers match fixed phrases per item; they do not understand meaning, so a green that keeps the phrase but contradicts it elsewhere on the line passes. The stale list is closed (S1 to S9): a new stale clause not in the list is not caught.
- The OPEN scan is case-sensitive, whole word, and only inside Owner items; other sections are covered by the stale phrases.
- A4q checks the question 4 text anywhere in Owner items, not only inside the lawyer item.
- B6 finds each target by the first brief line containing its name; B4 reads the numbers on the `- Classes:` line only.
- The `ANSWERED` fixture is the red seat's reading of the slice's text; the green's page need not match it byte for byte, only satisfy the checkers.
- Whether the `pending-red` CI job accepts a Pending fact that runs several test ids is unverified here (the card 366 wrapper does the same).
