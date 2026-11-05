"""Card 264 red 2B (D157 Criterion 2): the committed root-cause findings file.

.mob/records/264/findings.md (does not exist yet) must hold a markdown table with one row for each of the five
suspects, each row citing evidence (a command with its output, or a CI log line / run id) and a result of exactly
`ruled in` or `ruled out`; one sentence naming ONE root cause; and the reproduction (or `not reproduced locally`
with the logged failure as the observation).

Findings shape:
  | Suspect | Evidence | Result |
  | shared build | `dotnet build ...` output: ... | ruled out |
  ...
  Root cause: <one sentence>.
  Reproduction: <what was run and what failed before the fix> | not reproduced locally: <logged failure>

The checker is a small function in this module (no production file). Red: the real file does not exist.
ZOMBIES: Z test_zero_*, O test_one_*, M test_many_*, B test_boundary_*, S test_simple_*.
Fixture trees use tempfile.mkdtemp and are left in place (no cleanup, by the safety rule).
"""

import re
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
REL = ".mob/records/264/findings.md"
SUSPECTS = {
    "shared build": ("shared build",),
    "thread-pool starvation": ("thread-pool starvation", "thread pool starvation", "starvation"),
    "external dependency": ("external dependency",),
    "timing and ordering": ("timing and ordering", "timing/ordering", "timing"),
    "missing results file after an early child exit": ("missing results file", "early child exit", "early-exit"),
}
EVIDENCE = re.compile(r"`[^`]+`|\brun \d{6,}\b|\bCI log\b", re.I)


def check(root):
    """Return a list of '<keyword> <detail>' findings for root/REL; empty means clean."""
    path = Path(root) / REL
    if not path.is_file():
        return [f"missing {REL}"]
    text = path.read_text(encoding="utf-8")
    findings = []
    rows = []
    for line in text.splitlines():
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if line.strip().startswith("|") and len(cells) >= 3 and not set(cells[0]) <= set("-: "):
            rows.append(cells)
    for name, aliases in SUSPECTS.items():
        row = next((r for r in rows if any(a in r[0].lower() for a in aliases)), None)
        if row is None:
            findings.append(f"suspect {name}: no row")
            continue
        if row[-1].strip("` ") not in ("ruled in", "ruled out"):
            findings.append(f"result {name}: must be exactly 'ruled in' or 'ruled out', got {row[-1]!r}")
        if not EVIDENCE.search(" ".join(row[1:-1])):
            findings.append(f"evidence {name}: no command output or CI log line / run id")
    causes = re.findall(r"^\s*(?:\*\*)?root cause(?:\*\*)?\s*:\s*(.+)$", text, re.I | re.M)
    if len(causes) != 1 or len(causes[0].strip()) < 20 or len(re.findall(r"[.!?](?:\s|$)", causes[0].strip())) > 1:
        findings.append("root-cause: need exactly one 'Root cause:' line holding one sentence")
    repro = re.findall(r"^\s*(?:\*\*)?reproduction(?:\*\*)?\s*:\s*(.+)$", text, re.I | re.M)
    if not repro:
        findings.append("reproduction: no 'Reproduction:' line")
    elif repro[0].lower().startswith("not reproduced locally") and not re.search(
            r"DirectoryNotFoundException|exit code|run \d{6,}", repro[0], re.I):
        findings.append("reproduction: 'not reproduced locally' must cite the logged failure")
    return findings


def good_rows(overrides=None, drop=None):
    rows = {
        "shared build": ("`dotnet build tools/Phrase.Bench` twice concurrently: both succeeded", "ruled out"),
        "thread-pool starvation": ("`ThreadPoolFloor` probe output: min threads 32", "ruled out"),
        "external dependency": ("CI log run 41341801479: restore ok", "ruled out"),
        "timing and ordering": ("`dotnet test --filter X` x3 output: order unchanged", "ruled out"),
        "missing results file after an early child exit": ("CI log: DirectoryNotFoundException at ReadHistory", "ruled in"),
    }
    rows.update(overrides or {})
    if drop:
        rows.pop(drop)
    out = ["# Findings 264", "", "| Suspect | Evidence | Result |", "|---|---|---|"]
    out += [f"| {k} | {e} | {r} |" for k, (e, r) in rows.items()]
    return out


TAIL = ["", "Root cause: the harness read history before checking the child's exit, masking an early child failure.",
        "Reproduction: `BenchRunHarness` exit-42 test failed with DirectoryNotFoundException before the fix and passes after."]


def make(lines):
    root = Path(tempfile.mkdtemp(prefix="findings264-"))
    target = root / REL
    target.parent.mkdir(parents=True)
    target.write_text("\n".join(lines) + "\n", encoding="utf-8")
    return root


BAD = [
    ("missing_suspect", good_rows(drop="shared build") + TAIL, "suspect shared build"),
    ("missing_result", good_rows({"shared build": ("`cmd` output", "")}) + TAIL, "result shared build"),
    ("other_result", good_rows({"shared build": ("`cmd` output", "probably")}) + TAIL, "result shared build"),
    ("no_evidence", good_rows({"external dependency": ("seems fine", "ruled out")}) + TAIL, "evidence external dependency"),
    ("no_root_cause", good_rows() + ["", TAIL[2]], "root-cause"),
    ("two_sentence_cause", good_rows() + ["", "Root cause: it was the harness. It was also the build.", TAIL[2]], "root-cause"),
    ("no_reproduction", good_rows() + TAIL[:2], "reproduction"),
    ("not_reproduced_without_log", good_rows() + TAIL[:2] + ["Reproduction: not reproduced locally"], "reproduction"),
]


class FindingsFile264(unittest.TestCase):
    def test_zero_real_findings_file_exists_and_is_clean(self):
        self.assertEqual([], check(REPO), "card 264 criterion 2: .mob/records/264/findings.md does not exist yet")

    def test_simple_good_fixture_is_clean(self):
        self.assertEqual([], check(make(good_rows() + TAIL)))

    def test_one_not_reproduced_locally_with_logged_failure_is_clean(self):
        tail = TAIL[:2] + ["Reproduction: not reproduced locally; observation is CI log run 41341801479 DirectoryNotFoundException."]
        self.assertEqual([], check(make(good_rows() + tail)))

    def test_many_bad_fixtures_each_report_their_defect(self):
        for name, lines, expected in BAD:
            with self.subTest(name):
                found = check(make(lines))
                self.assertTrue(any(f.startswith(expected) for f in found), (name, found))

    def test_boundary_missing_file_in_fixture_root(self):
        self.assertEqual([f"missing {REL}"], check(tempfile.mkdtemp(prefix="findings264-")))


if __name__ == "__main__":
    unittest.main()
