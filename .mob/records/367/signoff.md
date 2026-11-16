SIGN-OFF

# Card 367 acceptance sign-off (sign-off seat, agy gemini-3.8-flash-medium, ctx-367s; card-367 at 5522ac6 / green c6b3428; no repo file changed)

All three acceptance criteria pass on the real repository files and are backed by tests that fail on partial or incorrect work.

## Criterion-to-test table

| Criterion | Tests proving criterion | Real page passes |
| --- | --- | --- |
| 367-1 [O][S] one cited summary per vendor | Python: `test_every_vendor_has_a_cited_dated_row_naming_both_outputs`, `test_the_vendor_terms_check_reports_a_planted_row_without_url_or_date` (`367-1-good`, `367-1-no-url`, `367-1-no-date`), `test_the_vendor_terms_check_reports_a_page_missing_the_not_legal_advice_line` (`367-1-legal-good`, `367-1-legal-missing`, `367-1-legal-not-under-title`), `test_the_vendor_terms_check_reports_a_page_missing_the_how_this_was_read_section` (`367-1-how-read-good`, `367-1-how-read-missing`, `367-1-how-read-browser`), `test_the_vendor_terms_check_pins_empty_page_and_invalid_blocks_publication_per_criterion` (`367-empty-all-criteria`). C#: `VendorTermsRead.Every_vendor_has_a_cited_dated_row_naming_both_outputs`. | Yes |
| 367-2 [Z][B] blockers are flagged | Python: `test_blockers_agree_with_the_blocks_publication_column`, `test_the_vendor_terms_check_reports_a_planted_yes_row_missing_from_blockers` (`367-2-listed`, `367-2-missing`), `test_the_vendor_terms_check_pins_empty_page_and_invalid_blocks_publication_per_criterion` (`367-empty-all-criteria`, `367-2-maybe-blocks`). C#: `VendorTermsRead.Blockers_agree_with_the_blocks_publication_column`. | Yes |
| 367-3 [I] no unread vendor remains | Python: `test_pre_publication_status_matches_each_vendor_read_on_date`, `test_the_vendor_terms_check_reports_a_vendor_left_not_reviewed` (`367-3-not-reviewed`, `367-3-date-mismatch`), `test_the_vendor_terms_check_pins_empty_page_and_invalid_blocks_publication_per_criterion` (`367-empty-all-criteria`), `test_pre_publication_checks.py` vendor-terms branch. C#: `VendorTermsRead.Pre_publication_status_matches_each_vendor_read_on_date`. | Yes |

## Run line

`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks`
Ran 22 tests in 49.715s — OK.

## Named limits

1. **367-3 limit (slice.md R367-3)**: With `docs/release/vendor-terms.md` deleted but `docs/release/pre-publication-checks.md` still citing `(vendor-terms.md)`, the 367-3 test alone passes (367-1 and 367-2 fail, so the card fails). Accepted; noted in the retro.
2. **Terms not read (from docs/release/vendor-terms.md)**:
   - xAI Brand Guidelines and Acceptable Use Policy;
   - OpenAI Business Terms, Service Terms, and Usage Policies;
   - GitHub Generative AI Services Terms (for Copilot Business and Enterprise);
   - Any Google terms specific to the agy client;
   - Which terms govern the CLI subscriptions used (codex through a ChatGPT plan, grok through Grok Build, copilot-default);
   - No terms behind a login were read;
   - Out of scope for this card: seat replies from Anthropic, Cursor, Muse, and Qwen under `.mob/records` (named limit for a follow-up card per slice.md).
3. **C# wrapper built only at the gate**: `tests/Avowry.Acceptance.Tests/VendorTermsRead.cs` follows the `PrePublicationOwnerItems.cs` pattern and was inspected; under shared laptop rules (`no dotnet`), the C# test suite is not built or run in this seat and is executed only at the integration gate.
