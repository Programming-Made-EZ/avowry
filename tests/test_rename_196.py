"""Card 196 (D157): the two test projects are renamed from the old project names to Avowry.Compiler.Tests and
Avowry.Acceptance.Tests, with nothing lost and nothing left pointing at the old paths.

One class per cycle, so later cycles add a class and never edit an earlier one:
  RenameCycle1     Criterion 1 (Z, E): the old names are gone from tracked live files; the new directories are tracked.
  ListsAndStrings  Criterion 2 (cycle 2, not yet written)
  PathBound        Criterion 3 (cycle 3, not yet written)

The old names are built from fragments below, so this file never contains the literal old names and the scan
never matches itself.

Run: python -m unittest tests.test_rename_196
No test deletes anything: copies live in tempfile.TemporaryDirectory.
"""

import re
import shutil
import subprocess
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent

_OLD_STEM = "Ph" + "rase"
OLD_NAMES = re.compile((_OLD_STEM + r"\.(Compiler|Acceptance)\.Tests").encode())
OLD_COMPILER = _OLD_STEM + ".Compiler.Tests"
OLD_ACCEPTANCE = _OLD_STEM + ".Acceptance.Tests"
NEW_NAMES = re.compile(rb"Avowry\.(Compiler|Acceptance)\.Tests")
NEW_COMPILER = "Avowry.Compiler.Tests"
NEW_ACCEPTANCE = "Avowry.Acceptance.Tests"

# Board, mob records and decision/retro history keep the old names as history.
SKIPPED_PREFIXES = ("board/", ".mob/", "docs/decisions/", "docs/retros/")


def git(*args):
    return subprocess.run(["git", *args], cwd=REPO, capture_output=True, check=True).stdout


def tracked_files():
    """Every tracked path (forward slashes), from the index."""
    return [p for p in git("ls-files", "-z").decode("utf-8").split("\0") if p]


def live(paths):
    return [p for p in paths if not p.startswith(SKIPPED_PREFIXES)]


def scan(root, paths):
    """Return 'path:line' for every line under root naming an old project name, over the given live paths."""
    hits = []
    for rel in live(paths):
        path = Path(root) / rel
        if not path.is_file():
            continue
        for number, line in enumerate(path.read_bytes().split(b"\n"), start=1):
            if OLD_NAMES.search(line):
                hits.append(f"{rel}:{number}")
    return hits


def copy_tracked(destination):
    """Copy the working-tree content of every tracked file into destination; return the tracked paths."""
    paths = tracked_files()
    for rel in paths:
        source = REPO / rel
        if source.is_file():
            target = Path(destination) / rel
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
    return paths


def put_back(path, line_number, matcher, replacement_for):
    """Rewrite one line of path in place, as bytes so CR survives, putting an old name back."""
    lines = path.read_bytes().split(b"\n")
    index = line_number - 1
    lines[index] = matcher.sub(lambda m: replacement_for(m.group(1).decode()).encode(), lines[index], count=1)
    path.write_bytes(b"\n".join(lines))


def first_line_with(path, pattern):
    for number, line in enumerate(path.read_bytes().split(b"\n"), start=1):
        if pattern.search(line):
            return number
    return None


def old_for(kind):
    return _OLD_STEM + "." + kind + ".Tests"


def summarise(hits, limit=15):
    files = sorted({h.rsplit(":", 1)[0] for h in hits})
    shown = "\n  ".join(hits[:limit])
    return f"{len(hits)} lines in {len(files)} files name an old project; first {min(limit, len(hits))}:\n  {shown}"


class RenameCycle1(unittest.TestCase):
    """Criterion 1. ZOMBIES: Z zero (no old name left), E exceptions (a single put-back is caught)."""

    def test_old_names_gone_from_tracked_live_files(self):
        hits = scan(REPO, tracked_files())
        self.assertEqual([], hits, summarise(hits))

    def test_new_dirs_tracked_old_gone(self):
        def count(directory):
            return len([p for p in git("ls-files", "-z", "--", directory).decode("utf-8").split("\0") if p])

        self.assertEqual(
            {NEW_COMPILER: 27, NEW_ACCEPTANCE: 100, OLD_COMPILER: 0, OLD_ACCEPTANCE: 0},
            {
                NEW_COMPILER: count("tests/" + NEW_COMPILER),
                NEW_ACCEPTANCE: count("tests/" + NEW_ACCEPTANCE),
                OLD_COMPILER: count("tests/" + OLD_COMPILER),
                OLD_ACCEPTANCE: count("tests/" + OLD_ACCEPTANCE),
            },
        )

    def _put_back_one(self, rel):
        """Copy the tracked files, assert the pristine copy is clean, put one old name back on the first line
        that carries the new name, and assert the scan reports exactly that file and line."""
        with tempfile.TemporaryDirectory() as temp:
            paths = copy_tracked(temp)
            pristine = scan(temp, paths)
            self.assertEqual([], pristine, "precondition: the pristine copy must scan clean. " + summarise(pristine))
            target = Path(temp) / rel
            self.assertTrue(target.is_file(), f"{rel} must be a tracked file")
            number = first_line_with(target, NEW_NAMES)
            self.assertIsNotNone(number, f"{rel} must name a new project")
            put_back(target, number, NEW_NAMES, old_for)
            self.assertIn(f"{rel}:{number}", scan(temp, paths))
            return rel, number

    def test_put_back_script(self):
        self._put_back_one("scripts/ci-log-outcomes-217.py")

    def test_put_back_namespace(self):
        self._put_back_one(f"tests/{NEW_ACCEPTANCE}/Support/BenchLauncher.cs")

    def test_put_back_gitattributes(self):
        rel, number = self._put_back_one(".gitattributes")
        self.assertEqual(1, number)


if __name__ == "__main__":
    unittest.main()
