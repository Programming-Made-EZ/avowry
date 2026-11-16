CHANGES

# Card 367 review (QA seat, claude-opus-5-5, ctx-367q; clone of card-367 at 4c6f4f9; range 67cb044..HEAD; no file in the repo changed)

Sources checked: Google and GitHub rows against the cited pages by web fetch on 2026-11-16 (policies.google.com/terms, policies.google.com/terms/generative-ai, aistudio.google.com/docs/terms, the three docs.github.com pages). xAI and OpenAI rows against `.mob/records/367/captures/captures.md` (web fetch refused for those vendors).

## Findings

1. BLOCKING (check 2, xAI row): the unclear permission sentence is called ambiguous, but the row's closing sentence then resolves it. It says "conditions are attribution, a disclosure that the text is AI-generated, and human review" and drops the permission requirement. The capture quotes the sentence as "you are required to obtain our permission and attribute your generation of the Output to the Service" and notes it may cover any use of Output. On the wider reading, publishing the bench `reply_head` prefixes and the `.mob/records` replies needs xAI's permission, which is a possible blocker. The `no` call is defensible only on the "nothing plainly forbids" standard, and the page gives Dana no conditions list that names permission. Fix (one clause, page only): add "and, on the wider reading of the unclear sentence, xAI's permission (Brand Guidelines not read)" to the conditions, so the `no` stays visibly provisional.

2. MINOR (check 1, OpenAI row): "must not be extracted programmatically in bulk". The capture has no "in bulk" and reads "no automatically or programmatically extracting data or Output". The qualifier is invented. Drop "in bulk".

3. MINOR (checks 1 and 3, OpenAI row): "whether bulk committed records meet "reviewed before sharing"" puts a phrase in quotation marks that is not in the source. The capture has "manually review each generation before sharing" and "human review as appropriate, before using or sharing Output". It is also a second quote in that row, after "own the Output", and the limit is one per vendor. Remove the quotation marks or quote the source exactly instead of "own the Output".

4. MINOR (check 1, Google row): "can be published if labelled as AI-generated" makes a requirement stronger than the text read. The Google Terms (effective 2026-07-30) forbid "misleading others into thinking that generative AI content was created by a human". The Gemini API terms (effective 2026-03-23) contain no labelling requirement. Suggest "if not presented as human-made (a label is the simplest way)". Everything else in the Google row traces to the pages: the ownership quote is verbatim; the user is responsible for content shared with others; "Use discretion before relying on, publishing..."; grounded Search and Maps results go only to the end user who asked; no competing models; no ML training on AI content; generative-AI terms ended 2024-05-22 except for partners with a signed agreement.

5. MINOR (check 4, tests): the slice requires the "not legal advice" line and a "How this was read" section that lists what was not read. No test checks either one, so a page without them passes all three criteria. They are present on the real page. Also, the 367-1 real-page test asserts that the whole `real_problems()` is empty, so a 367-3 status fault fails 367-1 too. The red recorded this as intentional. The card's literal criterion 3 ("fails if a vendor has no URL and no read date") is exercised by the planted 367-1 test, not by the tests the 367-3 [Fact] calls, even though the comment on that [Fact] says it is. The coverage exists, but it is mapped to the wrong criterion.

6. MINOR (records): green-record.md still ends "Not edited. Needs a ruling" under "Test conflict" and was not updated after R367-1 and its addendum resolved the conflict (red-record.md has the fix and a passing run).

## Checks that pass

- GitHub row: traces fully to the pages. Section J.2 "GitHub does not claim ownership of your Input or Output." is verbatim. Output may be inaccurate or resemble third-party or open-source code, and the user reviews it and handles licences (J.2, J.4). Section J has no publishing restriction. The AUP mentions "automated excessive bulk activity and coordinated inauthentic activity". Copilot Business and Enterprise bought from GitHub fall under the Generative AI Services Terms (additional-products page), and the page lists those terms as not read.
- xAI and OpenAI rows otherwise match the captures: ownership, human review, AI-generated disclosure, the enterprise assignment and indemnity, dates 2026-10-27, 2026-01-01 and 2022-11-14, the Sharing & Publication conditions, no representing Output as human-generated, and no competing models. The xAI quote is verbatim from the capture.
- Blocker calls: all four `no` with the exact None line. The Google, GitHub and OpenAI calls are defensible from the text, with the OpenAI conditions stated. The xAI call is defensible only once finding 1 is fixed.
- Limits: "not legal advice" appears at the top and the bottom. Each row has at most one verbatim quote, except finding 3. The not-read list matches the captures and the green record. The D277 browser reads are disclosed.
- R367-1 did not weaken the card 186 check. Each row still needs `reply_head`, the vendor and a terms status. The status token changed from `not reviewed` to `(vendor-terms.md)`, and a row still saying `not reviewed` now fails. The date match is enforced by 367-3. The COMPLIANT fixture change is the four cells only, as ruled.
- C# wrapper: VendorTermsRead.cs follows PrePublicationOwnerItems.cs exactly: the same AssertPythonTestsPass helper, RepoPaths.Root, the 120000 ms timeout, and one [Fact] per criterion with Slice 367 and Criterion 367-N traits. It was not built (no dotnet).
- Pre-publication diff: only the four terms status cells changed, each `read 2026-11-16 (vendor-terms.md)`.

## Run

`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks`: Ran 19 tests in 75.684s, OK.
