# Card 367 red record (red seat, Composer, ctx-367r2; worktree wt-367 @ 67cb044; no commit)

Failing tests only. Product code and docs untouched. `vendor_terms_problems(page_text, prepub_text, today)` lives in `tests/test_vendor_terms.py` (not product code). C# wrapper not built or run (shared laptop: no dotnet).

## Tests by criterion

### 367-1 [O][S] — cited row per pre-publication vendor

| Layer | Test name |
| --- | --- |
| Python (real page) | `test_every_vendor_has_a_cited_dated_row_naming_both_outputs` |
| Python (planted) | `test_the_vendor_terms_check_reports_a_planted_row_without_url_or_date` (subTests `367-1-good`, `367-1-no-url`, `367-1-no-date`) |
| C# | `VendorTermsRead.Every_vendor_has_a_cited_dated_row_naming_both_outputs` |

### 367-2 [Z][B] — Blockers vs `blocks publication`

| Layer | Test name |
| --- | --- |
| Python (real page) | `test_blockers_agree_with_the_blocks_publication_column` |
| Python (planted) | `test_the_vendor_terms_check_reports_a_planted_yes_row_missing_from_blockers` (subTests `367-2-listed`, `367-2-missing`) |
| C# | `VendorTermsRead.Blockers_agree_with_the_blocks_publication_column` |

### 367-3 [I] — pre-publication `terms status` matches `read on`

| Layer | Test name |
| --- | --- |
| Python (real page) | `test_pre_publication_status_matches_each_vendor_read_on_date` |
| Python (planted) | `test_the_vendor_terms_check_reports_a_vendor_left_not_reviewed` (subTests `367-3-not-reviewed`, `367-3-date-mismatch`) |
| C# | `VendorTermsRead.Pre_publication_status_matches_each_vendor_read_on_date` |

## Fail / pass (today)

### `python -m unittest tests.test_vendor_terms -v`

Summary: `Ran 6 tests in 0.005s` — `FAILED (failures=3)`.

**Fail (real-page; expected red):**

- `test_every_vendor_has_a_cited_dated_row_naming_both_outputs` — `docs/release/vendor-terms.md` is absent (empty read): four missing vendor rows, missing Blockers section, four `not reviewed` terms statuses.
- `test_blockers_agree_with_the_blocks_publication_column` — same empty page: `vendor-terms: blockers: section missing`.
- `test_pre_publication_status_matches_each_vendor_read_on_date` — pre-publication Vendor terms table still `not reviewed: needs the vendor terms page` for xAI, OpenAI, Google, GitHub.

**Pass (planted-text; checker behaviour):**

- `test_the_vendor_terms_check_reports_a_planted_row_without_url_or_date` — good fixture clean; planted missing URL (xAI) and invalid date (OpenAI) reported by vendor name.
- `test_the_vendor_terms_check_reports_a_planted_yes_row_missing_from_blockers` — listed blocker passes; planted `yes` without bullet fails as expected.
- `test_the_vendor_terms_check_reports_a_vendor_left_not_reviewed` — `not reviewed` and date mismatch reported for Google and xAI.

### `python -m unittest tests.test_pre_publication_owner_items tests.test_pre_publication_checks`

Summary: `Ran 13 tests in 42.071s` — `OK`.

## Ambiguities

- Vendor list is parsed from the live `## Vendor terms` table (strip `(inferred)` suffix); no hard-coded vendor set in tests.
- `sect` / table parsing is duplicated in `test_vendor_terms.py` rather than imported from `test_pre_publication_checks` (same logic as `sect` there); green may dedupe.
- Real-page failure for 367-1 asserts `real_problems()` is empty (aggregate), not only row problems; blockers and prepub issues appear in the same assertion — intentional until the page exists.
- C# acceptance tests not executed on this seat; gate runs them later like card 371.

## Red fix (R367-1)

Ruling: vendor-terms rows must carry `(vendor-terms.md)`; `not reviewed` fails with "still says not reviewed".

### Diff (`tests/test_pre_publication_checks.py`, `vendor-terms` branch only)

```diff
diff --git a/tests/test_pre_publication_checks.py b/tests/test_pre_publication_checks.py
index 9f6ef90..8414b23 100644
--- a/tests/test_pre_publication_checks.py
+++ b/tests/test_pre_publication_checks.py
@@ -181,9 +181,12 @@ def collect_failures(page, ctx):
                     else:
                         for model in models:
                             line = next((ln for ln in body.splitlines() if model in ln), "")
-                            if "reply_head" not in line or "not reviewed" not in line:
+                            if "reply_head" not in line or "(vendor-terms.md)" not in line:
                                 fail(cid, "row missing reply_head or terms status")
                                 break
+                            if "not reviewed" in line:
+                                fail(cid, "still says not reviewed")
+                                break
                             if "inferred" not in line and "vendor" not in line.lower():
                                 fail(cid, "row missing vendor")
                                 break
```

### Run

`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks -v`

Summary: `Ran 19 tests in 38.424s` — `FAILED (failures=1)`.

- `test_vendor_terms` (6): OK.
- `test_pre_publication_owner_items` (7): OK.
- `test_pre_publication_checks` (6): `test_compliant_fixture_page_gives_empty` fails — `vendor-terms: row missing reply_head or terms status` (COMPLIANT fixture still has `not reviewed` rows; green updates fixture/page next).
- Slice lists vendors as xAI, OpenAI, Google, GitHub from the table; seat replies under `.mob/records` and other vendors (Anthropic, Cursor, etc.) are explicitly out of scope per slice.

### R367-1 addendum (COMPLIANT fixture only)

```diff
diff --git a/tests/test_pre_publication_checks.py b/tests/test_pre_publication_checks.py
--- a/tests/test_pre_publication_checks.py
+++ b/tests/test_pre_publication_checks.py
@@ -43,10 +43,10 @@ COMPLIANT = "\n".join([
     "The Avowry tool restores no packages.", "Recommendation: proceed.",
     "## Vendor terms", f"Command: list. HEAD {H40}",
     "| model | vendor | published | terms |", "| --- | --- | --- | --- |",
-    "| grok-4.7 | xAI (inferred) | reply_head | not reviewed |",
-    "| copilot-default | GitHub (inferred) | reply_head | not reviewed |",
-    "| codex-gpt-5.6-sol | OpenAI (inferred) | reply_head | not reviewed |",
-    "| agy-gemini-4-argon-medium | Google (inferred) | reply_head | not reviewed |",
+    "| grok-4.7 | xAI (inferred) | reply_head | read 2026-11-16 (vendor-terms.md) |",
+    "| copilot-default | GitHub (inferred) | reply_head | read 2026-11-16 (vendor-terms.md) |",
+    "| codex-gpt-5.6-sol | OpenAI (inferred) | reply_head | read 2026-11-16 (vendor-terms.md) |",
+    "| agy-gemini-4-argon-medium | Google (inferred) | reply_head | read 2026-11-16 (vendor-terms.md) |",
     "Recommendation: hold.",
     "## Secrets scan", f"Command: python scripts/scan-history-for-secrets.py {H40}",
```

### Run (addendum)

`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks -v`

Summary: `Ran 19 tests in 36.442s` — OK.

## Fix round 1 red (R367-2)

Ruling M5 (review finding 5): checker reports missing not-legal-advice line and missing or incomplete `## How this was read`; real-page tests filter by criterion; C# 367-3 comment and 367-1 wrapper list updated.

### Diff stat

```
 tests/Avowry.Acceptance.Tests/VendorTermsRead.cs |  6 +-
 tests/test_vendor_terms.py                       | 86 +++++++++++++++++++++++-
 2 files changed, 87 insertions(+), 5 deletions(-)
```

### Run

`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks -v`

Summary: `Ran 21 tests in 50.374s` — OK.

## Fix round 2 red (R367-3)

R367-3 (review-2 B1, M5): per-criterion filters keep early-stop and `blocks publication` faults; not-legal-advice must be the first non-blank line after the H1; planted subtests pin empty page, `maybe`, and disclaimer-only-in-How-this-was-read.

### Diff stat

```
 tests/test_vendor_terms.py | 68 +++++++++++++++++++++++++++++++++++++++++++---
 1 file changed, 64 insertions(+), 4 deletions(-)
```

### Run

`python -m unittest tests.test_vendor_terms tests.test_pre_publication_owner_items tests.test_pre_publication_checks -v`

Summary: `Ran 22 tests in 48.590s` — OK.

review-3 MINOR: 367-2 `VendorTermsRead` wires `test_the_vendor_terms_check_pins_empty_page_and_invalid_blocks_publication_per_criterion` into `AssertPythonTestsPass`.