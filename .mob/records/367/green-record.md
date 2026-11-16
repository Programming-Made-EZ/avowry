# Card 367 green record (green seat, ctx-367g; worktree wt-367; no commit)

Updated after D277. Page and table written; one old test now contradicts the slice (below). Stopped per brief: "If a test seems wrong, stop and report."

## Read dates (all 2026-11-16 UTC)
- Google, web fetch (about 17:35-17:40Z): https://policies.google.com/terms, https://policies.google.com/terms/generative-ai, https://ai.google.dev/gemini-api/terms (301 to https://aistudio.google.com/docs/terms). Blocker: no.
- GitHub, web fetch: https://docs.github.com/en/site-policy/github-terms/github-terms-of-service, .../github-terms-for-additional-products-and-features, .../acceptable-use-policies/github-acceptable-use-policies. Blocker: no.
- OpenAI, browser capture under D277 (17:45-17:46Z): https://openai.com/policies/terms-of-use/, https://openai.com/policies/sharing-publication-policy/. Blocker: no (conditions: human review, attribution, clear AI-generated label, no representing as human-made).
- xAI, browser capture under D277 (17:47-17:49Z): https://x.ai/legal/terms-of-service, https://x.ai/legal/terms-of-service-enterprise. Blocker: no (ambiguous permission/attribution sentence; AI-generated disclosure; human review).

## Blocker calls
All four `no`; Blockers section reads "None found in the terms read." Each `no` rests on the text not plainly restricting publication; ambiguities are written in the rows.

## Not read
xAI Brand Guidelines and Acceptable Use Policy; OpenAI Business Terms, Service Terms, Usage Policies; GitHub Generative AI Services Terms; Google agy-client terms; which terms govern the CLI subscriptions.

## Files changed
- docs/release/vendor-terms.md (new)
- docs/release/pre-publication-checks.md (four terms status cells)

## Test conflict
`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks -v`: Ran 19 tests, FAILED (failures=1).
The failing test is in tests/test_pre_publication_checks.py, lines 175-185 (the vendor-terms check): it requires each Vendor terms row to contain "not reviewed" ("row missing reply_head or terms status"). The slice and tests/test_vendor_terms.py (367-3) require that same cell to read `read <date> (vendor-terms.md)`. The two cannot both pass. All tests in test_vendor_terms pass. Not edited. Needs a ruling: relax the old test to accept "read <date> (vendor-terms.md)" (a test edit, not mine to make).

R367-1 resolved the stop above: the red seat relaxed the card 186 vendor-terms check (and the COMPLIANT fixture) to require `(vendor-terms.md)`, so the test conflict no longer stands.

## Fix round 1 green (R367-2)
Edited docs/release/vendor-terms.md only: B1 xAI row now lists "and, on the wider reading, xAI's permission (Brand Guidelines not read)" as an open condition and calls the `no` provisional; M2 dropped "in bulk" (OpenAI); M3 removed the quotation marks around the review-before-sharing phrase (paraphrased; one quote per vendor); M4 Google row says the terms forbid presenting AI content as human-made and that no label requirement is claimed.
git diff --stat: docs/release/vendor-terms.md | 6 +++---  (1 file changed, 3 insertions(+), 3 deletions(-))
Run: python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks -> Ran 21 tests, OK.
