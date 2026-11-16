"""Card 367 (Criteria 367-1 to 367-3): the vendor terms page cites each vendor, flags blockers,
and the pre-publication table matches its read dates."""

import re
import unittest
from datetime import date
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
VENDOR_PAGE = REPO / "docs" / "release" / "vendor-terms.md"
PREPUB_PAGE = REPO / "docs" / "release" / "pre-publication-checks.md"
URL_RE = re.compile(r"https://[^\s|\)\]]+")
READ_RE = re.compile(r"^read (\d{4}-\d{2}-\d{2}) \(vendor-terms\.md\)$")
NONE_LINE = "None found in the terms read."
NOT_LEGAL_ADVICE = "This is a summary of published terms, not legal advice."
PLANTED_TODAY = date(2026, 2, 1)
PLANTED_DATE = "2026-01-15"

GOOD_PREPUB = "\n".join([
    "# Pre-publication checks",
    "## Vendor terms",
    "| model | vendor (inferred from the file name only) | reply_head published | terms status |",
    "| --- | --- | --- | --- |",
    f"| grok-4.7 | xAI (inferred) | reply_head present | read {PLANTED_DATE} (vendor-terms.md) |",
    f"| codex-gpt-5.6-sol | OpenAI (inferred) | reply_head present | read {PLANTED_DATE} (vendor-terms.md) |",
    f"| agy-gemini-4-argon-medium | Google (inferred) | reply_head present | read {PLANTED_DATE} (vendor-terms.md) |",
    f"| copilot-default | GitHub (inferred) | reply_head present | read {PLANTED_DATE} (vendor-terms.md) |",
    "",
])

GOOD_PAGE = "\n".join([
    "# Vendor terms for publishing model output",
    "This is a summary of published terms, not legal advice.",
    "## Vendors",
    "| vendor | model files | terms read | read on | publishing model output | blocks publication |",
    "| --- | --- | --- | --- | --- | --- |",
    f"| xAI | grok-4.7 | https://example.test/xai-terms | {PLANTED_DATE} | Allows publishing reply_head prefixes and seat replies under .mob/records. | no |",
    f"| OpenAI | codex-gpt-5.6-sol | https://example.test/openai-terms | {PLANTED_DATE} | Allows publishing reply_head prefixes and seat replies under .mob/records. | no |",
    f"| Google | agy-gemini-4-argon-medium | https://example.test/google-terms | {PLANTED_DATE} | Allows publishing reply_head prefixes and seat replies under .mob/records. | no |",
    f"| GitHub | copilot-default | https://example.test/github-terms | {PLANTED_DATE} | Allows publishing reply_head prefixes and seat replies under .mob/records. | no |",
    "## Blockers",
    NONE_LINE,
    "## How this was read",
    "Web fetch only, read on 2026-01-15; terms behind a login were not read.",
    "",
])

BLOCKED_PAGE = GOOD_PAGE.replace(
    "| GitHub | copilot-default | https://example.test/github-terms | " + PLANTED_DATE
    + " | Allows publishing reply_head prefixes and seat replies under .mob/records. | no |",
    "| GitHub | copilot-default | https://example.test/github-terms | " + PLANTED_DATE
    + " | Allows publishing reply_head prefixes and seat replies under .mob/records. | yes |",
).replace(
    NONE_LINE,
    "- GitHub: quoted https://example.test/github-terms ; recommendation: hold new GitHub output.",
)


def sect(text, title):
    needle = "## " + title + "\n"
    at = text.find(needle)
    if at < 0:
        return None
    nxt = text.find("\n## ", at + len(needle))
    return text[at + len(needle):] if nxt < 0 else text[at + len(needle):nxt]


def table_rows(body):
    rows = []
    for line in (body or "").splitlines():
        if not line.startswith("|"):
            continue
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if not cells or not cells[0] or set(cells[0]) <= set("-: "):
            continue
        rows.append(cells)
    return rows


def prepub_vendors(prepub_text):
    vendors = []
    for cells in table_rows(sect(prepub_text, "Vendor terms")):
        if len(cells) < 4 or cells[0].lower() == "model":
            continue
        vendor = re.sub(r"\s*\(.*$", "", cells[1]).strip()
        if vendor:
            vendors.append((vendor, cells[3]))
    return vendors


def page_rows(page_text):
    rows = {}
    for cells in table_rows(sect(page_text, "Vendors")):
        if cells[0].lower() != "vendor":
            rows[cells[0]] = cells
    return rows


def as_date(today):
    if isinstance(today, date):
        return today
    return date.fromisoformat(str(today)[:10])


def check_vendor_row(problems, vendor, cells, today):
    if len(cells) < 6:
        problems.append(f"vendor-terms: {vendor}: row has {len(cells)} columns, expected 6")
        return None
    if not URL_RE.search(cells[2]):
        problems.append(f"vendor-terms: {vendor}: missing https URL")
    read_on = None
    try:
        read_on = date.fromisoformat(cells[3])
    except ValueError:
        problems.append(f"vendor-terms: {vendor}: missing or invalid read on date")
    if read_on is not None and read_on > today:
        problems.append(f"vendor-terms: {vendor}: read on {read_on.isoformat()} is in the future")
    if "reply_head" not in cells[4]:
        problems.append(f"vendor-terms: {vendor}: summary does not mention reply_head")
    if ".mob/records" not in cells[4]:
        problems.append(f"vendor-terms: {vendor}: summary does not mention .mob/records")
    if cells[5].lower() not in ("yes", "no"):
        problems.append(f"vendor-terms: {vendor}: blocks publication must be yes or no")
    return read_on


def check_not_legal_advice(problems, page_text):
    if NOT_LEGAL_ADVICE not in (page_text or ""):
        problems.append("vendor-terms: page: missing not legal advice line")


def check_how_this_was_read(problems, page_text):
    body = sect(page_text or "", "How this was read")
    if body is None:
        problems.append("vendor-terms: page: missing How this was read section")
        return
    text = body.strip()
    if not text:
        problems.append("vendor-terms: page: How this was read section is empty")
        return
    if "not read" not in text.lower():
        problems.append("vendor-terms: page: How this was read does not say what was not read")
    if re.search(r"\bbrowser\b", text, re.I):
        for vendor in ("xAI", "OpenAI"):
            if vendor not in text:
                problems.append(
                    f"vendor-terms: page: How this was read does not name browser-read vendor {vendor}")


def check_blockers(problems, page_text, rows):
    body = sect(page_text, "Blockers")
    if body is None:
        problems.append("vendor-terms: blockers: section missing")
        return
    lines = [ln.strip() for ln in body.splitlines() if ln.strip()]
    yes = [v for v, c in rows.items() if len(c) >= 6 and c[5].lower() == "yes"]
    if not yes:
        if lines != [NONE_LINE]:
            problems.append(f"vendor-terms: blockers: expected '{NONE_LINE}'")
        return
    for vendor in yes:
        bullet = next((ln for ln in lines if ln.startswith("-") and vendor in ln), "")
        if not bullet:
            problems.append(f"vendor-terms: blockers: {vendor}: yes row missing from Blockers")
            continue
        if not URL_RE.search(bullet):
            problems.append(f"vendor-terms: blockers: {vendor}: missing quoted URL")
        if "recommend" not in bullet.lower():
            problems.append(f"vendor-terms: blockers: {vendor}: missing recommendation")


def check_prepub_status(problems, vendor, status, read_on):
    if "not reviewed" in status:
        problems.append(f"vendor-terms: {vendor}: pre-publication terms status not reviewed")
        return
    match = READ_RE.match(status)
    if not match:
        problems.append(
            f"vendor-terms: {vendor}: pre-publication terms status must read "
            "'read <date> (vendor-terms.md)'")
        return
    if read_on is not None and match.group(1) != read_on.isoformat():
        problems.append(
            f"vendor-terms: {vendor}: pre-publication date {match.group(1)} "
            f"does not match read on {read_on.isoformat()}")


def vendor_terms_problems(page_text, prepub_text, today):
    """Return problems with the vendor terms page against the pre-publication table."""
    problems = []
    today = as_date(today)
    vendors = prepub_vendors(prepub_text or "")
    if not vendors:
        return ["vendor-terms: pre-publication: no vendors found in the Vendor terms table"]
    check_not_legal_advice(problems, page_text or "")
    check_how_this_was_read(problems, page_text or "")
    rows = page_rows(page_text or "")
    read_dates = {}
    for vendor, _status in vendors:
        cells = rows.get(vendor)
        if cells is None:
            problems.append(f"vendor-terms: {vendor}: missing row")
        else:
            read_dates[vendor] = check_vendor_row(problems, vendor, cells, today)
    check_blockers(problems, page_text or "", rows)
    for vendor, status in vendors:
        check_prepub_status(problems, vendor, status, read_dates.get(vendor))
    return problems


def read_or_empty(path):
    return path.read_text(encoding="utf-8").replace("\r\n", "\n") if path.is_file() else ""


def real_problems():
    return vendor_terms_problems(read_or_empty(VENDOR_PAGE), read_or_empty(PREPUB_PAGE), date.today())


def keyed(problems, vendor):
    return [p for p in problems if f": {vendor}:" in p or f" {vendor}:" in p]


def problems_for_367_1(problems):
    return [p for p in problems if ": blockers:" not in p and "pre-publication" not in p]


def problems_for_367_2(problems):
    return [p for p in problems if ": blockers:" in p]


def problems_for_367_3(problems):
    return [p for p in problems if "pre-publication" in p]


class VendorTermsTests(unittest.TestCase):
    def test_every_vendor_has_a_cited_dated_row_naming_both_outputs(self):
        # Given the four vendors in the pre-publication Vendor terms table
        # When the vendor terms page is checked
        # Then each vendor has a row with an https URL, a non-future ISO date, and a summary naming both outputs
        self.assertEqual([], problems_for_367_1(real_problems()))

    def test_the_vendor_terms_check_reports_a_planted_row_without_url_or_date(self):
        # Given a planted page whose xAI row has no URL and whose OpenAI row has no date
        # When the pure check runs on the planted text
        # Then each fault is reported by vendor name, and the compliant planted page reports nothing
        with self.subTest(id="367-1-good"):
            self.assertEqual([], vendor_terms_problems(GOOD_PAGE, GOOD_PREPUB, PLANTED_TODAY))
        no_url = GOOD_PAGE.replace("https://example.test/xai-terms ", "")
        with self.subTest(id="367-1-no-url"):
            self.assertEqual(["vendor-terms: xAI: missing https URL"],
                             keyed(vendor_terms_problems(no_url, GOOD_PREPUB, PLANTED_TODAY), "xAI"))
        no_date = GOOD_PAGE.replace(
            f"| OpenAI | codex-gpt-5.6-sol | https://example.test/openai-terms | {PLANTED_DATE} |",
            "| OpenAI | codex-gpt-5.6-sol | https://example.test/openai-terms |  |")
        with self.subTest(id="367-1-no-date"):
            self.assertEqual(["vendor-terms: OpenAI: missing or invalid read on date"],
                             keyed(vendor_terms_problems(no_date, GOOD_PREPUB, PLANTED_TODAY), "OpenAI"))

    def test_the_vendor_terms_check_reports_a_page_missing_the_not_legal_advice_line(self):
        # Given a planted page whose opening disclaimer is removed
        # When the pure check runs on the planted text
        # Then the missing line is reported, and the compliant planted page reports nothing
        with self.subTest(id="367-1-legal-good"):
            self.assertEqual([], problems_for_367_1(
                vendor_terms_problems(GOOD_PAGE, GOOD_PREPUB, PLANTED_TODAY)))
        no_line = GOOD_PAGE.replace(NOT_LEGAL_ADVICE + "\n", "")
        with self.subTest(id="367-1-legal-missing"):
            self.assertEqual(
                ["vendor-terms: page: missing not legal advice line"],
                [p for p in vendor_terms_problems(no_line, GOOD_PREPUB, PLANTED_TODAY)
                 if p.startswith("vendor-terms: page:")])

    def test_the_vendor_terms_check_reports_a_page_missing_the_how_this_was_read_section(self):
        # Given a planted page with no How this was read section, or one that omits browser-read vendors
        # When the pure check runs on the planted text
        # Then each fault is reported, and the compliant planted page reports nothing
        with self.subTest(id="367-1-how-read-good"):
            self.assertEqual([], problems_for_367_1(
                vendor_terms_problems(GOOD_PAGE, GOOD_PREPUB, PLANTED_TODAY)))
        no_section = GOOD_PAGE.replace(
            "## How this was read\n"
            "Web fetch only, read on 2026-01-15; terms behind a login were not read.\n",
            "")
        with self.subTest(id="367-1-how-read-missing"):
            self.assertEqual(
                ["vendor-terms: page: missing How this was read section"],
                [p for p in vendor_terms_problems(no_section, GOOD_PREPUB, PLANTED_TODAY)
                 if p.startswith("vendor-terms: page:")])
        browser_unnamed = GOOD_PAGE.replace(
            "Web fetch only, read on 2026-01-15; terms behind a login were not read.",
            "Read through the built-in browser under D277; terms behind a login were not read.")
        with self.subTest(id="367-1-how-read-browser"):
            self.assertEqual(
                [
                    "vendor-terms: page: How this was read does not name browser-read vendor xAI",
                    "vendor-terms: page: How this was read does not name browser-read vendor OpenAI",
                ],
                [p for p in vendor_terms_problems(browser_unnamed, GOOD_PREPUB, PLANTED_TODAY)
                 if p.startswith("vendor-terms: page:")])

    def test_blockers_agree_with_the_blocks_publication_column(self):
        # Given the summaries on the real vendor terms page
        # When a row says it blocks publication
        # Then Blockers lists it with a URL and a recommendation, or says None when no row blocks
        self.assertEqual([], problems_for_367_2(real_problems()))

    def test_the_vendor_terms_check_reports_a_planted_yes_row_missing_from_blockers(self):
        # Given a planted page with a GitHub yes row absent from Blockers, and one correctly listed
        # When the pure check runs on the planted text
        # Then the missing vendor is reported, and the correctly listed page reports nothing
        with self.subTest(id="367-2-listed"):
            self.assertEqual([], vendor_terms_problems(BLOCKED_PAGE, GOOD_PREPUB, PLANTED_TODAY))
        missing = BLOCKED_PAGE.replace(
            "- GitHub: quoted https://example.test/github-terms ; recommendation: hold new GitHub output.",
            NONE_LINE)
        with self.subTest(id="367-2-missing"):
            self.assertIn("vendor-terms: blockers: GitHub: yes row missing from Blockers",
                          vendor_terms_problems(missing, GOOD_PREPUB, PLANTED_TODAY))

    def test_pre_publication_status_matches_each_vendor_read_on_date(self):
        # Given the real pre-publication Vendor terms table
        # When each terms status is compared with the vendor terms page
        # Then each reads read <date> (vendor-terms.md) matching that vendor's read on date
        self.assertEqual([], problems_for_367_3(real_problems()))

    def test_the_vendor_terms_check_reports_a_vendor_left_not_reviewed(self):
        # Given a planted pre-publication table leaving Google not reviewed, and one with a mismatched date
        # When the pure check runs on the planted text
        # Then the vendor is reported by name in each case
        not_reviewed = GOOD_PREPUB.replace(
            f"| agy-gemini-4-argon-medium | Google (inferred) | reply_head present | read {PLANTED_DATE} (vendor-terms.md) |",
            "| agy-gemini-4-argon-medium | Google (inferred) | reply_head present | not reviewed: needs the vendor terms page |")
        with self.subTest(id="367-3-not-reviewed"):
            self.assertEqual(["vendor-terms: Google: pre-publication terms status not reviewed"],
                             keyed(vendor_terms_problems(GOOD_PAGE, not_reviewed, PLANTED_TODAY), "Google"))
        mismatched = GOOD_PREPUB.replace(
            f"| grok-4.7 | xAI (inferred) | reply_head present | read {PLANTED_DATE} (vendor-terms.md) |",
            "| grok-4.7 | xAI (inferred) | reply_head present | read 2026-01-16 (vendor-terms.md) |")
        with self.subTest(id="367-3-date-mismatch"):
            self.assertEqual(
                ["vendor-terms: xAI: pre-publication date 2026-01-16 does not match read on 2026-01-15"],
                keyed(vendor_terms_problems(GOOD_PAGE, mismatched, PLANTED_TODAY), "xAI"))


if __name__ == "__main__":
    unittest.main()
