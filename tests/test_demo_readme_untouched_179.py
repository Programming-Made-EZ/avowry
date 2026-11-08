"""Card 179 (D157): the Tasks demo README stays byte-identical to BASE.

Declared characterisation: it passes on the pre-change tree. Every comparison
uses `git show REV:demo/tasks/README.md` (committed blobs, exact key). The
comparison commit is the one that added docs/GETTING_STARTED.md; HEAD if none.
No C# wrapper: CI's shallow clone cannot see BASE; the gate runs this in a clone.
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
BASE = "b30803420f30f4ff749f77d39be7640461d0506f"
PATH = "demo/tasks/README.md"
PAGE = "docs/GETTING_STARTED.md"


def git(*args):
    return subprocess.run(
        ["git", *args], cwd=REPO, capture_output=True, check=True
    ).stdout


def comparison_commit():
    """The commit that added the guide (`git log --diff-filter=A`); HEAD if none.

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


def differing_demo_readme(left, right):
    return [PATH] if blob(left, PATH) != blob(right, PATH) else []


def _synthetic_repo(scratch):
    """A git-init repo whose history cannot see the real page-adding commit."""
    repo = Path(scratch) / "repo"
    repo.mkdir()
    ident = [
        "-c",
        "user.name=scratch",
        "-c",
        "user.email=scratch@example.invalid",
        "-c",
        "commit.gpgsign=false",
    ]

    def in_repo(*args):
        return subprocess.run(
            ["git", *ident, *args], cwd=repo, check=True, capture_output=True
        )

    in_repo("init", "-q", "-b", "main")
    (repo / "README.md").write_bytes(b"base README\n")
    demo = repo / "demo" / "tasks"
    demo.mkdir(parents=True)
    (demo / "README.md").write_bytes(b"base demo README\n")
    in_repo("add", "README.md", PATH)
    in_repo("commit", "-q", "--no-verify", "-m", "base")
    base = in_repo("rev-parse", "HEAD").stdout.decode("ascii").strip()
    return repo, in_repo, base


class DemoReadmeUntouched179(unittest.TestCase):
    def test_base_resolves_and_is_an_ancestor_of_head(self):
        # Given the BASE constant recorded at the characterisation commit
        full = git("rev-parse", BASE).decode("ascii").strip()

        # When it is resolved, Then it equals the constant and is an ancestor of HEAD
        self.assertEqual(BASE, full, f"BASE resolves to {full}")
        probe = subprocess.run(
            ["git", "merge-base", "--is-ancestor", BASE, "HEAD"], cwd=REPO
        )
        self.assertEqual(0, probe.returncode, "BASE is not an ancestor of HEAD")

    def test_the_demo_readme_is_byte_identical_to_the_base(self):
        # Given BASE and the comparison commit (the guide-adding commit, or HEAD)
        other = comparison_commit()

        # When the committed blob of demo/tasks/README.md is compared
        differing = differing_demo_readme(BASE, other)

        # Then it is byte-identical (i/lf w/lf) and the blob is not empty
        self.assertEqual(
            [],
            differing,
            f"demo README differs between {BASE[:7]} and {other[:7]}: {differing}",
        )

    def test_look_alike_commit_and_uncommitted_edit_still_report_equal(self):
        # Given a synthetic repo: guide added with the demo README unchanged, then a
        # later commit with look-alike subject `Card 179 green 1 (scratch)` edited
        # README.md, and demo/tasks/README.md has an uncommitted append
        with tempfile.TemporaryDirectory() as scratch:
            repo, in_repo, base = _synthetic_repo(scratch)
            page = repo / PAGE
            page.parent.mkdir(parents=True, exist_ok=True)
            page.write_bytes(b"# Getting started\n")
            in_repo("add", PAGE)
            in_repo("commit", "-q", "--no-verify", "-m", "Card 179 green 1")
            readme = repo / "README.md"
            readme.write_bytes(readme.read_bytes() + b"\nA later look-alike line.\n")
            in_repo("add", "README.md")
            in_repo("commit", "-q", "--no-verify", "-m", "Card 179 green 1 (scratch)")
            demo = repo / PATH
            demo.write_bytes(demo.read_bytes() + b"\nan uncommitted later line\n")

            # When the helpers read committed blobs in the synthetic repo
            module = sys.modules[__name__]
            with mock.patch.object(module, "REPO", repo), mock.patch.object(
                module, "BASE", base
            ):
                other = comparison_commit()
                got = blob(other, PATH)
                want = git("show", f"{BASE}:{PATH}")

        # Then they still report the demo README equal (page-adding commit, not HEAD)
        self.assertEqual(
            got,
            want,
            "look-alike commit or uncommitted edit changed the helper",
        )

    def test_adding_the_guide_and_editing_the_demo_readme_reports_a_difference(self):
        # Given a synthetic repo whose second commit adds the guide and edits the demo README
        with tempfile.TemporaryDirectory() as scratch:
            repo, in_repo, base = _synthetic_repo(scratch)
            page = repo / PAGE
            page.parent.mkdir(parents=True, exist_ok=True)
            page.write_bytes(b"# Getting started\n")
            demo = repo / PATH
            demo.write_bytes(demo.read_bytes() + b"\nmutated demo README\n")
            in_repo("add", PAGE, PATH)
            in_repo("commit", "-q", "--no-verify", "-m", "Card 179 green 1 (scratch)")

            # When the helpers compare BASE to the comparison commit
            module = sys.modules[__name__]
            with mock.patch.object(module, "REPO", repo), mock.patch.object(
                module, "BASE", base
            ):
                other = comparison_commit()
                differing = differing_demo_readme(BASE, other)

        # Then they report a difference
        self.assertEqual(
            [PATH],
            differing,
            f"expected {PATH} to differ at {other[:7] if other else 'none'}",
        )


if __name__ == "__main__":
    unittest.main()
