SIGN-OFF

# Card 367 review round 3 (QA seat, claude-opus-5-5, ctx-367q; clone at f277ccd; range c6b3428..f277ccd, tests/test_vendor_terms.py only; no file in the repo changed)

Edge cases were probed in memory only, by importing tests.test_vendor_terms and calling the checker on modified strings. No file was written.

## Round 2 findings

- B1a fixed: `problems_for_367_2` keeps "blocks publication must be yes or no" again. The planted subtest `367-2-maybe-blocks` pins it: a `maybe` cell fails 367-2 and is not counted in 367-1. 367-1 no longer carries that problem. That is the per-criterion split R367-2 asked for, and 367-2 is back to its round-1 strength.
- B1b fixed: `is_early_stop_problem` keeps "no vendors found in the Vendor terms table" in all three filters. Subtest `367-empty-all-criteria` shows that an empty page and an empty pre-publication table fail 367-1, 367-2 and 367-3. That is the only early return in `vendor_terms_problems`. The other early returns (Blockers missing, How-this-was-read missing or empty) are problems that are still reported, not stops.
- M2 fixed: `check_not_legal_advice` now requires the sentence as the first non-empty line after the first `# ` heading. Subtest `367-1-legal-not-under-title` shows that the sentence only in How this was read is reported. The real page passes, with the title, a blank line, then the sentence.
- Nothing weakened: every round-2 assertion still holds. The real-page per-criterion tests changed only as listed above. The page, the pre-publication page, test_pre_publication_checks.py and the C# wrapper are unchanged in this range. On the real page, `vendor_terms_problems` returns `[]`.

## Named limit

Accepted. Probe: with the page text empty and the real pre-publication table, 367-3 gives `[]` because each status matches the regex and there is no `read on` to compare. 367-1 has 6 problems and 367-2 has 1, so the card fails as the facilitator states. A 367-3 that also required the row to exist would fold a 367-1 fault back into 367-3, against R367-2.

## Findings

1. MINOR (gate wiring): the new test `test_the_vendor_terms_check_pins_empty_page_and_invalid_blocks_publication_per_criterion` is not called by any [Fact] in tests/Avowry.Acceptance.Tests/VendorTermsRead.cs (unchanged in this range). The card's Done-when is `dotnet test`, so the gate will not run the planted proof of R367-3 (c). The python run below does run it. Suggest adding the name to the 367-2 [Fact] (it pins 367-2's filter and the shared early stop) before or at the gate. The `367-1-legal-not-under-title` subtest sits inside an already-wired test and is covered.

## Run

`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks`: Ran 22 tests in 50.128s, OK.
