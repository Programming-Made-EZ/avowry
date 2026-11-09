"""Card 371 (D157 Criteria 1 to 3): the Owner items section shows each answer, nothing calls an answered
item open, and the section carries the trademark search brief."""

import re
import unittest

from tests.test_pre_publication_checks import PAGE, TARGETS, sect

ANSWERED_TAG = "answered 2026-11-08."
BRIEF_HEADING = "### Trademark search brief"
# key -> (line prefix, phrases the item line must carry after the answered tag)
ITEMS = {
    "name": ("- Public name:", (
        "Avowry confirmed (D125); registration status pending the owner's trademark search.",)),
    "holder": ("- Holder at", (
        "Programming Made EZ;", "2026 is an assumption the owner can correct (docs/LICENSING.md);",
        "same holder for the trademark line: yes (card 366).")),
    "search": ("- Professional trademark search:", (
        "commissioned by the owner; pending result", "Publishing the name waits on the result.")),
    "lawyer": ("- The four lawyer questions", ("skipped by owner decision.",)),
    "author": ("- Author identity going forward:", ("keep.",)),
    "vendor": ("- Unread vendor terms", ("read-first (card 367).",)),
    "reply-head": ("- The `reply_head` publication choice", (
        "trim for new rows only; pushed rows stay public (card 368).",)),
    "transitive": ("- Transitive package licences:", ("restore-and-review (card 369).",)),
    "home": ("- Home paths in tracked records:", ("scrub-tracked; history stays public (card 370).",)),
}
LAWYER_Q4 = "Who is the holder? (answered by the holder item above)"
# id -> stale phrase (S1 to S9; S6 is three clauses of one recommendation)
STALE = {
    "S1": "stay open", "S2": "needs a professional search", "S3": "commission a professional",
    "S4": "Every item is open", "S5": "year placeholder", "S6a": "holder needed", "S6b": "lawyer ask",
    "S6c": "stay until the holder is named", "S7": "decide the home-path", "S8": "the owner decides",
    "S9": "an open check",
}
BRIEF_LABELS = {
    "NuGet": "NuGet package id", "npm": "npm", "PyPI": "PyPI", "crates.io": "crates.io",
    "GitHub": "GitHub handle", "avowry.dev": "avowry.dev", "avowry.io": "avowry.io",
}
BRIEF_JURISDICTIONS = "Jurisdictions (proposed by the mob; the owner confirms with the provider):"
BRIEF_ANCHOR = "[Name collisions](#name-collisions-not-a-professional-search)"
TARGET_TAIL = "free on 2026-10-29 (D125); not checked since."
H40 = "b" * 40

ANSWERED = "\n".join([
    "# Pre-publication checks", f"HEAD `{H40}`",
    "## Package licences",
    "Compatibility: the lawyer questions were skipped by owner decision (Owner items).",
    "Recommendation: accept them, and treat the transitive licences as a review (card 369).",
    "## Secrets scan",
    "Recommendation: skim the hits once by eye and scrub the home paths from tracked records "
    "(card 370; history stays public).",
    "## Author metadata",
    "Recommendation: the owner kept the repository-local identity for the package and for later commits "
    "(answered 2026-11-08).",
    "## Name collisions (not a professional search)",
    "- Trademark databases and the earlier brand called Phrase: not reviewed: commissioned by the owner; "
    "pending result (Trademark search brief below).",
    "Recommendation: repeat the registry lookups on the publish day; the professional trademark search is "
    "commissioned by the owner, and publishing the name waits on its result.",
    "## Owner items",
    f"Command: read from NOTICE, TRADEMARKS.md, D121, D125 and the owner's answers of 2026-11-08 "
    f"(NOTICE and TRADEMARKS.md filled by card 366). Scan tip: {H40}. Every item is answered; the search "
    "waits on its result and four items are carried by cards 367 to 370.",
    "- Public name: answered 2026-11-08. Avowry confirmed (D125); registration status pending the owner's "
    "trademark search.",
    "- Holder at `NOTICE:2` and `TRADEMARKS.md:5`: answered 2026-11-08. Programming Made EZ; 2026 is an "
    "assumption the owner can correct (docs/LICENSING.md); same holder for the trademark line: yes (card 366).",
    "- Professional trademark search: answered 2026-11-08. commissioned by the owner; pending result. "
    "Publishing the name waits on the result.",
    "- The four lawyer questions (D121): answered 2026-11-08. skipped by owner decision.",
    "  1. Do the generated runtime helper files need a notice, given that generated code belongs to the user?",
    "  2. Is a trademark policy on this name enforceable?",
    "  3. Does the rename rule conflict with Apache-2.0 notice retention?",
    f"  4. {LAWYER_Q4}",
    "- Author identity going forward: answered 2026-11-08. keep.",
    "- Unread vendor terms (four vendors, plus the seat replies under `.mob/records`): answered 2026-11-08. "
    "read-first (card 367).",
    "- The `reply_head` publication choice (prefixes of up to 200 characters in the bench history files): "
    "answered 2026-11-08. trim for new rows only; pushed rows stay public (card 368).",
    "- Transitive package licences: answered 2026-11-08. restore-and-review (card 369).",
    "- Home paths in tracked records: answered 2026-11-08. scrub-tracked; history stays public (card 370).",
    "",
    BRIEF_HEADING,
    "",
    "- Applicant: Programming Made EZ.",
    "- Mark: Avowry.",
    "- Classes: 9 (downloadable software) and 42 (software development services).",
    f"- {BRIEF_JURISDICTIONS} United States, European Union, United Kingdom.",
    *[f"- {BRIEF_LABELS[t]}: {TARGET_TAIL}" for t in TARGETS],
    "- Trademark databases and the earlier brand called Phrase: not checked (the search covers them).",
    "- Registry state after 2026-10-29: not checked.",
    f"- Registry screen: {BRIEF_ANCHOR}.",
    "",
    "Recommendation: name Avowry confirmed, publishing waits on the search result.",
    "",
])


def read_page():
    return PAGE.read_text(encoding="utf-8").replace("\r\n", "\n")


def owner_body(page):
    return sect(page, "Owner items") or ""


def split_brief(body):
    at = body.find("\n" + BRIEF_HEADING)
    return (body, "") if at < 0 else (body[:at], body[at + 1:])


def top_items(page):
    head, _ = split_brief(owner_body(page))
    return [ln for ln in head.splitlines() if ln.startswith("- ")]


def owner_item_failures(page):
    items = top_items(page)
    out = []
    if len(items) != 9:
        out.append(f"owner-items: count: expected 9 items, found {len(items)}")
    for key, (prefix, phrases) in ITEMS.items():
        line = next((ln for ln in items if ln.startswith(prefix)), "")
        if ANSWERED_TAG not in line:
            out.append(f"owner-items: {key}: not answered 2026-11-08")
            continue
        out.extend(f'owner-items: {key}: missing "{p}"' for p in phrases if p not in line)
    if not any("Who is the holder?" in ln and "(answered by the holder item above)" in ln
               for ln in owner_body(page).splitlines()):
        out.append("owner-items: lawyer-q4: not marked answered by the holder item")
    return out


def stale_failures(page):
    out, heading = [], ""
    for n, line in enumerate(page.splitlines(), 1):
        if line.startswith("## "):
            heading = line[3:]
        if heading == "Owner items" and re.search(r"\bOPEN\b", line):
            out.append(f"owner-items: open: line {n}: OPEN")
        out.extend(f'owner-items: stale: line {n}: "{p}"' for p in STALE.values() if p in line)
    return out


def brief_failures(page):
    _, brief = split_brief(owner_body(page))
    lines = brief.splitlines()
    out = []

    def need(what, ok):
        if not ok:
            out.append(f'owner-items: brief: missing "{what}"')

    need(BRIEF_HEADING, bool(brief))
    need("Applicant: Programming Made EZ", "Applicant: Programming Made EZ" in brief)
    need("Mark: Avowry", "Mark: Avowry" in brief)
    classes = next((ln for ln in lines if ln.startswith("- Classes:")), "")
    need("9 (downloadable software)", "9 (downloadable software)" in classes)
    need("42 (software development services)", "42 (software development services)" in classes)
    need("classes 9 and 42 only", {int(x) for x in re.findall(r"\b\d+\b", classes)} == {9, 42})
    juris = next((ln for ln in lines if BRIEF_JURISDICTIONS in ln), "")
    need(BRIEF_JURISDICTIONS, bool(juris))
    for place in ("United States", "European Union", "United Kingdom"):
        need(place, place in juris)
    for target in TARGETS:
        line = next((ln for ln in lines if target in ln), "")
        need(f"{BRIEF_LABELS[target]}: {TARGET_TAIL}", f"{BRIEF_LABELS[target]}: {TARGET_TAIL}" in line)
    need("Trademark databases and the earlier brand called Phrase: not checked",
         "Trademark databases and the earlier brand called Phrase: not checked" in brief)
    need("Registry state after 2026-10-29: not checked.", "Registry state after 2026-10-29: not checked." in brief)
    need(BRIEF_ANCHOR, BRIEF_ANCHOR in brief)
    return out


def planted(old, new):
    assert old in ANSWERED, old
    return ANSWERED.replace(old, new)


def keyed(failures, key):
    return [f for f in failures if f.startswith(f"owner-items: {key}:")]


class PrePublicationOwnerItemsTests(unittest.TestCase):
    def test_each_owner_item_shows_its_answer(self):
        failures = owner_item_failures(read_page())
        for id_, key in (("A1", "name"), ("A2", "holder"), ("A3", "search"), ("A4", "lawyer"),
                         ("A4q", "lawyer-q4"), ("A5", "author"), ("A6", "vendor"), ("A7", "reply-head"),
                         ("A8", "transitive"), ("A9", "home")):
            with self.subTest(id=id_):
                self.assertEqual([], keyed(failures, key))
        with self.subTest(id="A11"):
            unanswered = [ln for ln in top_items(read_page()) if "answered 2026-11-08" not in ln]
            self.assertEqual([], unanswered)

    def test_the_page_has_nine_owner_items(self):
        self.assertEqual(9, len(top_items(read_page())))

    def test_the_owner_items_check_reports_a_missing_answer(self):
        search = "- Professional trademark search: answered 2026-11-08. commissioned by the owner; pending result. "
        holder = next(ln for ln in ANSWERED.splitlines() if ln.startswith("- Holder at"))
        cases = (
            ("E1", planted(search, "- Professional trademark search: OPEN, asked 2026-11-08. Answer: commission / skip. "),
             ["owner-items: search: not answered 2026-11-08"]),
            ("E2", planted(holder + "\n", ""),
             ["owner-items: count: expected 9 items, found 8", "owner-items: holder: not answered 2026-11-08"]),
            ("E3", planted("commissioned by the owner; pending result.", "commissioned by the owner."),
             ['owner-items: search: missing "commissioned by the owner; pending result"']),
            ("E4", planted("skipped by owner decision.", "ask."),
             ['owner-items: lawyer: missing "skipped by owner decision."']),
            ("E5", planted("\n" + BRIEF_HEADING, "\n- Extra item: answered 2026-11-08. none.\n\n" + BRIEF_HEADING),
             ["owner-items: count: expected 9 items, found 10"]),
        )
        for id_, page, expected in cases:
            with self.subTest(id=id_):
                self.assertEqual(expected, owner_item_failures(page))

    def test_no_page_line_still_calls_an_answered_item_open(self):
        failures = stale_failures(read_page())
        with self.subTest(id="Z1"):
            self.assertEqual([], keyed(failures, "open"))
        for id_, phrase in STALE.items():
            with self.subTest(id=id_):
                self.assertEqual([], [f for f in failures if f.endswith(f': "{phrase}"')])

    def test_the_stale_check_reports_a_planted_phrase(self):
        last = len(ANSWERED.splitlines()) + 1
        cases = (
            ("F1", "the lawyer questions stay open.", "stay open"),
            ("F2", "not reviewed: needs a professional search.", "needs a professional search"),
            ("F3", "Every item is open, none can be confirmed.", "Every item is open"),
            ("F4", "(which also holds a year placeholder)", "year placeholder"),
            ("F5", "treat the transitive licences as an open check.", "an open check"),
        )
        for id_, line, phrase in cases:
            with self.subTest(id=id_):
                self.assertEqual([f'owner-items: stale: line {last}: "{phrase}"'],
                                 stale_failures(ANSWERED + line + "\n"))
        with self.subTest(id="F6"):
            self.assertEqual([f"owner-items: open: line {last}: OPEN"],
                             stale_failures(ANSWERED + "- Extra item: OPEN, asked 2026-11-08.\n"))

    def test_the_stale_check_ignores_the_answered_page(self):
        with self.subTest(id="X1"):
            self.assertEqual([], stale_failures(ANSWERED))
        with self.subTest(id="X1-owner-items"):
            self.assertEqual([], owner_item_failures(ANSWERED))
        with self.subTest(id="X1-brief"):
            self.assertEqual([], brief_failures(ANSWERED))

    def test_the_page_carries_the_trademark_search_brief(self):
        failures = brief_failures(read_page())
        checks = (
            ("B1", ("Applicant: Programming Made EZ", "Mark: Avowry")),
            ("B2", ("9 (downloadable software)",)),
            ("B3", ("42 (software development services)",)),
            ("B4", ("classes 9 and 42 only",)),
            ("B5", (BRIEF_JURISDICTIONS, "United States", "European Union", "United Kingdom")),
            ("B6", tuple(f"{BRIEF_LABELS[t]}: {TARGET_TAIL}" for t in TARGETS)),
            ("B7", ("Trademark databases and the earlier brand called Phrase: not checked",
                    "Registry state after 2026-10-29: not checked.")),
            ("B8", (BRIEF_ANCHOR,)),
        )
        for id_, whats in checks:
            with self.subTest(id=id_):
                self.assertEqual([], [f for f in failures if any(f == f'owner-items: brief: missing "{w}"' for w in whats)])
        with self.subTest(id="B0"):
            self.assertNotIn(f'owner-items: brief: missing "{BRIEF_HEADING}"', failures)


if __name__ == "__main__":
    unittest.main()
