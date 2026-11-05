"""Card 264 red 3 (D157 Criterion 3): findings.md classifies the 11:27 run.

.mob/records/264/findings.md must hold a section headed `## The 11:27 run` (does not exist yet) that:
  O  names run 41341801479 and sha f682823, says the `test` job succeeded and the failing job was `mob-process`;
  S  carries the verdict word `different` (a different job, not this test);
  Z  never claims the test failed (an uninspected failure is not this test);
  B  says the card does not widen;
  I  cites `.mob/records/264/ci-evidence.md`;
  M  contrasts the 11:34 run 41342389875, the 004-read-phrase row and the missing history file;
  E  says the committed evidence and sha are the fallback if the log expires;
  and agrees with ci-evidence.md: every run id, sha and backticked job name in the section, and the
  `test` job succeeded / `mob-process` failed conclusion, must appear in ci-evidence.md.

The checker is a function in this module (no production file). Red: the real section does not exist.
ZOMBIES: Z test_zero_*, O test_one_*, M test_many_*, B test_boundary_*, S test_simple_*.
Fixture trees use tempfile.mkdtemp and are left in place (no cleanup, by the safety rule).
"""

import re
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
REL = ".mob/records/264/findings.md"
EVIDENCE_REL = ".mob/records/264/ci-evidence.md"
HEADING = "## The 11:27 run"
RUN, SHA = "41341801479", "f682823"
EVIDENCE_TEXT = (
    "- **11:27, run 41341801479** (2026-11-01T19:27:14Z, ci workflow, sha f682823): the run failed, but its `test` job "
    "succeeded. The failing job is `mob-process` (Check mob process), a different job.\n"
    "- **11:34, run 41342389875** (sha dc7cfed, the `test` job failed): `004-read-phrase` ... history.jsonl missing.\n"
)


def section_of(text):
    m = re.search(r"^## The 11:27 run[ \t]*$(.*?)(?=^## |\Z)", text, re.M | re.S)
    return m.group(1) if m else None


def check(root):
    """Return a list of '<keyword> <detail>' findings for the 11:27 section of root/REL; empty means clean."""
    base = Path(root)
    path = base / REL
    if not path.is_file():
        return [f"missing {REL}"]
    sec = section_of(path.read_text(encoding="utf-8"))
    if sec is None:
        return [f"section: no '{HEADING}' heading in {REL}"]
    low = sec.lower()
    found = []
    if RUN not in sec:
        found.append(f"run-id: section must name run {RUN}")
    if SHA not in sec:
        found.append(f"sha: section must name sha {SHA}")
    if not re.search(r"`test`\s+job\s+(?:succeeded|passed|was successful)", sec):
        found.append("test-job: must state the `test` job succeeded")
    if not re.search(r"fail\w*[^.\n]{0,60}`mob-process`|`mob-process`[^.\n]{0,60}fail", sec):
        found.append("failing-job: must state the failing job was `mob-process`")
    if not re.search(r"\bdifferent\b", low):
        found.append("verdict: must carry the word 'different'")
    if re.search(r"`test`\s+job\s+(?:failed|did not succeed)|\b(?:this|the)\s+test\s+failed\b", low):
        found.append("claim: section claims the test failed")
    if not re.search(r"does not widen|not widen", low):
        found.append("scope: must say the card does not widen")
    if EVIDENCE_REL not in sec:
        found.append(f"cite: must cite {EVIDENCE_REL}")
    for need, label in (("41342389875", "run 41342389875"), ("004-read-phrase", "004-read-phrase row"),
                        ("history", "missing history file")):
        if need not in sec:
            found.append(f"contrast: 11:34 contrast must name the {label}")
    if not (re.search(r"fallback", low) and re.search(r"\bsha\b", low) and re.search(r"expire", low)):
        found.append("fallback: must say the committed evidence and sha are the fallback if the log expires")
    ev_path = base / EVIDENCE_REL
    if not ev_path.is_file():
        found.append(f"consistency: missing {EVIDENCE_REL}")
    else:
        ev = ev_path.read_text(encoding="utf-8")
        for rid in sorted(set(re.findall(r"\b\d{8,}\b", sec))):
            if rid not in ev:
                found.append(f"consistency: run id {rid} is not in ci-evidence.md")
        for sha in sorted(set(re.findall(r"\b(?=[0-9a-f]*\d)(?=[0-9a-f]*[a-f])[0-9a-f]{7}\b", sec))):
            if sha not in ev:
                found.append(f"consistency: sha {sha} is not in ci-evidence.md")
        if not re.search(rf"{RUN}.*?`test` job succeeded.*?failing job is `mob-process`", ev, re.S):
            found.append("consistency: ci-evidence.md does not say run 41341801479 had the test job succeed and mob-process fail")
    return found


GOOD = (
    "Run 41341801479 (sha f682823) failed, but its `test` job succeeded; the failing job was `mob-process`. "
    "Is it this test? No, a different job failed, so card 264 does not widen. "
    "Evidence: `.mob/records/264/ci-evidence.md`. Contrast: the 11:34 run 41342389875 failed the "
    "004-read-phrase row on the missing history file. If the log expires, the committed evidence and the sha "
    "f682823 are the fallback."
)


def make(section=GOOD, evidence=EVIDENCE_TEXT, heading=HEADING):
    root = Path(tempfile.mkdtemp(prefix="run1127-264-"))
    (root / REL).parent.mkdir(parents=True)
    body = "# Findings 264\n\nRoot cause: x.\n\n" + (f"{heading}\n\n{section}\n\n## After\n\nmore\n" if heading else "")
    (root / REL).write_text(body, encoding="utf-8")
    if evidence is not None:
        (root / EVIDENCE_REL).write_text(evidence, encoding="utf-8")
    return root


BAD = [
    ("no_heading", dict(heading=None), "section"),
    ("wrong_run_id", dict(section=GOOD.replace("41341801479", "41341801470")), "run-id"),
    ("wrong_sha", dict(section=GOOD.replace("f682823", "f682824")), "sha"),
    ("test_failed_claim", dict(section=GOOD.replace("`test` job succeeded", "`test` job failed")), "claim"),
    ("no_test_job", dict(section=GOOD.replace("its `test` job succeeded", "its job ended")), "test-job"),
    ("no_failing_job", dict(section=GOOD.replace("`mob-process`", "the lint job")), "failing-job"),
    ("no_different", dict(section=GOOD.replace("different", "another")), "verdict"),
    ("widens", dict(section=GOOD.replace("does not widen", "adds scope")), "scope"),
    ("no_cite", dict(section=GOOD.replace(".mob/records/264/ci-evidence.md", "the evidence")), "cite"),
    ("no_1134_contrast", dict(section=GOOD.replace("41342389875", "the later run")), "contrast"),
    ("no_004_row", dict(section=GOOD.replace("004-read-phrase", "a row")), "contrast"),
    ("no_fallback", dict(section=GOOD.replace("fallback", "plan")), "fallback"),
    ("evidence_lacks_run", dict(evidence=EVIDENCE_TEXT.replace("41341801479", "41341801470")), "consistency"),
    ("evidence_lacks_sha", dict(evidence=EVIDENCE_TEXT.replace("f682823", "a000000")), "consistency"),
    ("evidence_says_test_failed",
     dict(evidence=EVIDENCE_TEXT.replace("its `test` job succeeded", "its `test` job failed")), "consistency"),
    ("evidence_missing", dict(evidence=None), "consistency"),
]


class Run1127Findings264(unittest.TestCase):
    def test_zero_real_findings_has_the_1127_section_and_is_clean(self):
        self.assertEqual([], check(REPO), "card 264 criterion 3: findings.md has no '## The 11:27 run' section yet")

    def test_simple_good_fixture_is_clean(self):
        self.assertEqual([], check(make()))

    def test_one_section_is_scoped_to_its_heading(self):
        root = make()
        p = root / REL
        p.write_text(p.read_text(encoding="utf-8") + "\nthe test failed\n", encoding="utf-8")
        self.assertEqual([], check(root))

    def test_many_bad_fixtures_each_report_their_defect(self):
        for name, kwargs, expected in BAD:
            with self.subTest(name):
                found = check(make(**kwargs))
                self.assertTrue(any(f.startswith(expected) for f in found), (name, found))

    def test_boundary_missing_findings_file(self):
        self.assertEqual([f"missing {REL}"], check(tempfile.mkdtemp(prefix="run1127-264-")))


if __name__ == "__main__":
    unittest.main()
