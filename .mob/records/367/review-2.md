CHANGES

# Card 367 review round 2 (QA seat, claude-opus-5-5, ctx-367q; clone at c6b3428; range 4c6f4f9..HEAD, red 5522ac6 and green c6b3428; no file in the repo changed)

Edge cases were probed in memory only, by importing tests.test_vendor_terms and calling vendor_terms_problems on modified strings. No file was written.

## Findings

1. BLOCKING (test strength, M5 split): the new per-criterion filters lose two assertions that the real-page tests made in round 1.
   - a. `problems_for_367_2` keeps only `": blockers:"`. The round-1 filter also kept `"blocks publication"`, so the 367-2 real-page test no longer fails on an invalid `blocks publication` cell. Probe: the real page with xAI's cell changed to `maybe` gives `['vendor-terms: xAI: blocks publication must be yes or no']`, and `problems_for_367_2` of that is `[]`. 367-1 catches it now, which makes the fault a 367-1 failure, but the 367-2 assertion is weaker than before.
   - b. `problems_for_367_1` drops every problem containing `"pre-publication"`. That includes the early return `vendor-terms: pre-publication: no vendors found in the Vendor terms table`, and on that return no row, Blockers, legal-advice or How-this-was-read check runs. Probe: `vendor_terms_problems('', '', today)` gives that one problem; `problems_for_367_1` and `problems_for_367_2` are both `[]`, so 367-1 and 367-2 pass vacuously on an empty page. In round 1, 367-1 failed here because it asserted all problems.
   - Fix (red, tests only): add `or "blocks publication" in p` to `problems_for_367_2`. Count the "no vendors found" problem for every criterion, or exclude only the status-line problems from 367-1. A planted subtest for each would pin both.

2. MINOR (M5, legal-advice check): `check_not_legal_advice` accepts the sentence anywhere on the page. The real page has it twice, at the top and as the last sentence of How this was read. Probe: removing the opening line leaves the sentence once and gives no problem. The slice puts the line directly under the title. The substance survives because the closing sentence remains. A check of the first non-blank line after the H1 would match the slice.

3. MINOR (M5, browser-read check): the vendors that must be named when "browser" appears are hard-coded `("xAI", "OpenAI")`. That is true to D277, but it is a constant where the rest of the file derives vendors from data. It also fires only if the word "browser" is present. Acceptable for this card; note it for any re-read.

4. MINOR (M4, Google row, no new claim about the terms): "a label is the simplest way" is an editorial suggestion (my round-1 wording), not something the text says. "The text read claims no label requirement" is accurate: neither the Google Terms nor the Gemini API terms require a label, and the Gemini API terms mention only attribution that law may require. Consider dropping "a label is the simplest way;" so the row only reports the terms. Not blocking.

## Fixed as ruled

- B1: the xAI row now lists "and, on the wider reading, xAI's permission (Brand Guidelines not read)" among the conditions and says "the `no` is provisional until that sentence is settled". The ambiguity is no longer resolved silently. Blockers still reads "None found in the terms read.", which is consistent because no term plainly blocks.
- M2: "in bulk" is gone. The row now reads "must not be extracted programmatically", which matches the capture.
- M3: "review-before-sharing condition" is now a paraphrase with no quotation marks. The OpenAI row has one quote ("own the Output"). The xAI, Google and GitHub rows have one quote each.
- M4: the Google row says the terms forbid presenting AI content as human-made and that no label requirement is claimed (see finding 4 for the remaining editorial clause).
- M5: legal-advice and How-this-was-read checks were added with planted subtests. The real-page tests now filter per criterion (but see finding 1). The 367-3 [Fact] comment is corrected to describe the status check. The C# 367-1 [Fact] calls the two new python tests and otherwise still follows the PrePublicationOwnerItems.cs pattern. Not built (no dotnet).
- M6: green-record.md now notes that R367-1 resolved the stop.
- Nothing else changed: no other rows, sections or files changed. The pre-publication page and test_pre_publication_checks.py are unchanged in this range.

## Run

`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks`: Ran 21 tests in 51.179s, OK.
