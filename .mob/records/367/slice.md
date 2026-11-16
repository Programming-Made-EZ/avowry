# Card 367 slice (facilitator; binding on red and green; adopted with the task force by D274)

## The page
New `docs/release/vendor-terms.md`:
- `# Vendor terms for publishing model output` and one line: "This is a summary of published terms, not legal advice."
- `## Vendors`: a Markdown table with exactly these columns: `| vendor | model files | terms read | read on | publishing model output | blocks publication |`. One row per vendor named in the "Vendor terms" table of `docs/release/pre-publication-checks.md` (xAI, OpenAI, Google, GitHub; the test derives the list from that table, not a constant). `terms read` holds one or more `https://` URLs of the vendor's own published terms of service and usage policy. `read on` is an ISO date (YYYY-MM-DD), not in the future. `publishing model output` summarises what the terms say, and must mention both the bench reply prefixes (`reply_head`) and the seat replies under `.mob/records`. `blocks publication` is `yes` or `no`.
- `## Blockers`: every vendor whose row says `yes` appears here as a bullet with a quoted URL and a recommendation; when no row says `yes`, the section's only body line reads `None found in the terms read.`
- `## How this was read`: web fetch only, the date, and what was not read (any terms behind a login).

## The pre-publication page
In `docs/release/pre-publication-checks.md` "Vendor terms" table, each `terms status` cell changes from `not reviewed: needs the vendor terms page` to `read YYYY-MM-DD (vendor-terms.md)`. The Owner items line for vendor terms keeps "read-first (card 367)." (card 371's test).

## Tests (the red writes them; the green makes them pass)
- `tests/test_vendor_terms.py` (unittest, like `tests/test_pre_publication_owner_items.py`), with a pure check function `vendor_terms_problems(page_text, prepub_text, today)` returning a list of problem strings, so tests can plant bad text.
  - 367-1 [O][S]: every vendor of the pre-publication table has a row with at least one https URL, a valid non-future ISO date, a summary naming `reply_head` and `.mob/records`; a planted row with no URL or no date is reported by vendor name.
  - 367-2 [Z][B]: the Blockers section agrees with the `blocks publication` column (a `yes` row is listed with a URL and a recommendation; no `yes` rows means the exact None line); a planted `yes` row missing from Blockers is reported.
  - 367-3 [I]: the pre-publication table's `terms status` for each vendor reads `read <date> (vendor-terms.md)` matching that vendor's `read on`; a vendor left `not reviewed` is reported.
- `tests/Avowry.Acceptance.Tests/VendorTermsRead.cs`: the C# acceptance wrapper, one [Fact] per criterion with `[Trait("Slice", "367")]` and `[Trait("Criterion", "367-N")]`, calling the python tests as `PrePublicationOwnerItems.cs` does.

## Not in this card
Legal advice; reading terms by anything but web fetch; changing pushed rows; vendors not in the pre-publication table (Anthropic, Cursor, Muse and Qwen seat replies under .mob/records are a named limit for a follow-up card).

## R367-1 (facilitator ruling, 2026-11-16 about 17:55Z): the card 186 vendor-terms check moves with the page

`tests/test_pre_publication_checks.py` (the `vendor-terms` branch, about line 184) fails a row unless it still says `not reviewed`. Card 367 exists to change that cell, so the old check contradicts the card (the green stopped and reported it without editing a test). As card 369 did for its licence rows, the check now requires each row to carry `(vendor-terms.md)` and fails a row that still says `not reviewed` (message "row missing reply_head or terms status" / "still says not reviewed"). The red seat makes this one test edit; nothing else in that file changes.
R367-1 addendum (about 17:48Z): the same file's `COMPLIANT` fixture (lines 46 to 49) still models the old page; its four vendor rows' last cell changes from `not reviewed` to `read 2026-11-16 (vendor-terms.md)`. No other edit.

## R367-2 (facilitator ruling, 2026-11-16 about 18:06Z): fix round 1 from review round 1

From review.md (Opus, CHANGES; agy's review-agy.md signed off with no findings, written before review.md existed):
- Page (green): B1, the xAI row states the wider reading as an open condition: "and, on the wider reading, xAI's permission (Brand Guidelines not read)", so its `no` is visibly provisional, and the Blockers section's None line stays only because no term plainly blocks. M2: drop "in bulk". M3: no quotation marks on "reviewed before sharing" (paraphrase), one quote per vendor. M4: the Google row says the terms forbid presenting AI content as human-made; no label requirement is claimed. M6: green-record.md notes R367-1 resolved the stop.
- Tests (red, first): M5, add checks that the page carries the not-legal-advice line and a "How this was read" section naming any browser-read vendor; the real-page test of each criterion asserts only its own criterion's problems (so a 367-3 fault does not fail 367-1); correct the 367-3 test's comment about which test covers "no URL and no date".

## R367-3 (facilitator ruling, 2026-11-16 about 18:14Z): fix round 2, tests only, from review round 2

review-2.md B1: R367-2's per-criterion split weakened two real-page tests. My ruling asked for the split without saying it must keep every round-1 assertion; that was the gap. The red: (a) `problems_for_367_2` keeps the "blocks publication must be yes or no" problem; (b) the "no vendors found in the Vendor terms table" problem, and any problem that stops the checker early, is kept in every criterion's filter, so an empty page fails all three; (c) planted subtests prove both (an empty page fails 367-1, 367-2 and 367-3; a `maybe` cell fails 367-2). M2: the not-legal-advice line must sit directly under the page title (the first non-empty line after the `# ` heading); planted subtest. M3 and M4 are accepted as they are.
Named limit (facilitator, after fix round 2): with the page deleted but the pre-publication table still citing `(vendor-terms.md)`, the 367-3 test alone passes (367-1 and 367-2 fail, so the card fails). Accepted; noted in the retro.
