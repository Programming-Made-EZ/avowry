# Card 162 green 2 record (claude-sonnet-5-5, ctx-162g2)

Worktree ~/projects/wt-162, branch card-162-green2. Nothing committed. No dotnet run.

## Diff (3 files, 1 insertion, 9 deletions)

- tests/Avowry.Acceptance.Tests/UsableV1.cs: removed `[Trait("Status", "Pending")]` and the comment "Card 162 reruns the LLM check ...; green 2 removes this Pending trait." from Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone.
- tests/Avowry.Acceptance.Tests/UsableV1Closure.cs: removed the Pending trait and its "Pending until green 2 ..." comment from Z1_the_ratchet_has_no_suspended_bar and Z2_the_bar_6_test_is_not_pending.
- board/usable-v1-ratchet.json: `"suspended": { "6": "..." }` became `"suspended": {}`. Key kept, not deleted, because code reads it (below).

## Who reads `suspended`

- scripts/usable-v1-report.py `load_suspended()`: `.get("suspended", {})`, so `{}` is fine.
- scripts/mob_process.py `render_mob` (line ~418): `ratchet.get("suspended", {})`, prints "suspended: [none]" for `{}`.
- tests Z1 (UsableV1Closure.cs): accepts a missing key or an empty object; fails on any entry.
- Documentation only: board/162 card, .mob/records (text mentions).

## Report prints 6 of 6, no warning (by reading usable-v1-report.py)

With `suspended == {}` and bars 1 to 6 passing: `classify` gives met = 6 bars; the summary header is "Usable v1: 6 of 6 criteria met"; every bar line is MET; `regressed` and `missing` are empty; `open_suspensions` is empty so no `::warning::` line is printed; `len(met) + 0 == len(BARS)` so it prints the REACHED line and `::notice::USABLE v1 REACHED` without the "(with suspended bars)" suffix; exit 0. (The embedded self-test passes `{6: "reason"}` explicitly, so it is unaffected.)

## Left as is (outside "nothing else")

The UsableV1Closure.cs class header comment still says "All three rows are Pending until green 2 ... and the re-freeze record commit (R1)"; it is now stale. Flag for the gate or a follow-up.

## Not run

No dotnet build or test (heavy lock held elsewhere). The dotnet checks, including Bar6, Z1, Z2 and `dotnet test Phrase.sln --filter "Status!=Pending"`, are left to the gate.

## Follow-up (facilitator request)

Updated the UsableV1Closure.cs class header comment (comment only): it now says green 2 of card 162 removed the Pending traits from Z1 and Z2, so the earlier "stale comment" note above is resolved.

## M2 (gate review B, finding M2)

tests/Avowry.Acceptance.Tests/PatternDocs.cs: removed `[Trait("Status", "Pending")]` and the comment "Pending until green 1 folds the pattern steps ..." from the criterion 162-1 test (2 deletions, nothing else). Not committed; no dotnet.

Run line: `python -m unittest tests.test_pattern_docs` -> Ran 10 tests in 0.008s, OK.
