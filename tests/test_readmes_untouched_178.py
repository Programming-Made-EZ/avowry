"""Card 178 (D157 Criterion 3): the two READMEs stay byte-identical to BASE.

Declared characterisation: it passes on the pre-change tree. Every comparison
uses `git show REV:path` (committed blobs). The comparison commit is the one
that added docs/PREREQUISITES.md; HEAD if none. No C# wrapper: CI's shallow
clone cannot see BASE; the gate runs this in a clone, not a git-archive copy.
"""

from __future__ import annotations

import subprocess
import sys
import tempfile
import unittest
from pathlib import Path
from unittest import mock

REPO = Path(__file__).resolve().parent.parent

# git rev-parse HEAD at the characterisation commit (the pre-change tree).
BASE = "b9e642348fdac7f80aac4fc33bdc2f74dcddaff2"
PATHS = ("README.md", "demo/tasks/README.md")
PAGE = "docs/PREREQUISITES.md"


def git(*args):
    return subprocess.run(
        ["git", *args], cwd=REPO, capture_output=True, check=True
    ).stdout


def comparison_commit():
    """The commit that added the page (`git log --diff-filter=A`); HEAD if none.

    Selected by the added path, never by a subject prefix (TWA D206).
    """
    log = git(
        "log", "--diff-filter=A", "--format=%H", "-1", "--", PAGE
    ).decode("ascii").strip()
    if log:
        return log
    return git("rev-parse", "HEAD").decode("ascii").strip()


def blob(rev, path):
    data = git("show", f"{rev}:{path}")
    if not data:
        raise AssertionError(f"{rev}:{path} is empty")
    return data


def differing_readmes(left, right):
    return [path for path in PATHS if blob(left, path) != blob(right, path)]


def _scratch_clone(scratch):
    clone = Path(scratch) / "clone"
    subprocess.run(
        ["git", "clone", "-q", "--shared", "--no-checkout", str(REPO), str(clone)],
        check=True,
        capture_output=True,
    )
    ident = [
        "-c",
        "user.name=scratch",
        "-c",
        "user.email=scratch@example.invalid",
        "-c",
        "commit.gpgsign=false",
    ]

    def in_clone(*args):
        subprocess.run(
            ["git", *ident, *args], cwd=clone, check=True, capture_output=True
        )

    in_clone("read-tree", "HEAD")
    in_clone("checkout", "-q", "HEAD", "--", *PATHS)
    return clone, in_clone


class ReadmesUntouched178(unittest.TestCase):
    def test_base_resolves_and_is_an_ancestor_of_head(self):
        # Given the BASE constant recorded at the characterisation commit
        full = git("rev-parse", BASE).decode("ascii").strip()

        # When it is resolved, Then it equals the constant and is an ancestor of HEAD
        self.assertEqual(BASE, full, f"BASE resolves to {full}")
        probe = subprocess.run(
            ["git", "merge-base", "--is-ancestor", BASE, "HEAD"], cwd=REPO
        )
        self.assertEqual(0, probe.returncode, "BASE is not an ancestor of HEAD")

    def test_the_two_readmes_are_byte_identical_to_the_base(self):
        # Given BASE and the comparison commit (the page-adding commit, or HEAD)
        other = comparison_commit()

        # When the committed blobs of README.md and demo/tasks/README.md are compared
        differing = differing_readmes(BASE, other)

        # Then they are byte-identical (i/lf w/lf) and neither blob is empty
        self.assertEqual(
            [],
            differing,
            f"READMEs differ between {BASE[:7]} and {other[:7]}: {differing}",
        )

    def test_look_alike_commit_and_uncommitted_edit_still_report_equal(self):
        # Given the BASE blobs, and a scratch clone where the page has been added,
        # a later commit with look-alike subject `Card 178 green 1 (scratch)` edited
        # README.md, and demo/tasks/README.md has an uncommitted append
        want = {path: git("show", f"{BASE}:{path}") for path in PATHS}
        with tempfile.TemporaryDirectory() as scratch:
            clone, in_clone = _scratch_clone(scratch)
            page = clone / PAGE
            page.parent.mkdir(parents=True, exist_ok=True)
            page.write_bytes(b"CI runs on ubuntu-latest.\n")
            in_clone("add", PAGE)
            in_clone("commit", "-q", "--no-verify", "-m", "Card 178 green 1")
            readme = clone / "README.md"
            readme.write_bytes(readme.read_bytes() + b"\nA later look-alike line.\n")
            in_clone("add", "README.md")
            in_clone("commit", "-q", "--no-verify", "-m", "Card 178 green 1 (scratch)")
            demo = clone / "demo" / "tasks" / "README.md"
            demo.write_bytes(demo.read_bytes() + b"\nan uncommitted later line\n")

            # When the helpers read committed blobs in the clone
            module = sys.modules[__name__]
            with mock.patch.object(module, "REPO", clone):
                other = comparison_commit()
                got = {path: blob(other, path) for path in PATHS}

        # Then they still report the two READMEs equal (page-adding commit, not HEAD)
        wrong = [path for path in PATHS if got[path] != want[path]]
        self.assertEqual(
            [],
            wrong,
            "look-alike commit or uncommitted edit changed the helper: " + str(wrong),
        )

    def test_adding_the_page_and_editing_a_readme_reports_a_difference(self):
        # Given a scratch clone whose one commit adds the page and edits a README
        with tempfile.TemporaryDirectory() as scratch:
            clone, in_clone = _scratch_clone(scratch)
            page = clone / PAGE
            page.parent.mkdir(parents=True, exist_ok=True)
            page.write_bytes(b"CI runs on ubuntu-latest.\n")
            readme = clone / "README.md"
            readme.write_bytes(readme.read_bytes() + b"\nmutated README\n")
            in_clone("add", PAGE, "README.md")
            in_clone("commit", "-q", "--no-verify", "-m", "Card 178 green 1 (scratch)")

            # When the helpers compare BASE to the comparison commit
            module = sys.modules[__name__]
            with mock.patch.object(module, "REPO", clone):
                other = comparison_commit()
                differing = differing_readmes(BASE, other)

        # Then they report a difference
        self.assertEqual(
            ["README.md"],
            differing,
            f"expected README.md to differ at {other[:7] if other else 'none'}",
        )


if __name__ == "__main__":
    unittest.main()
