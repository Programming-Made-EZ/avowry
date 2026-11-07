"""Card 196 (D157): the two test projects are renamed from the old project names to Avowry.Compiler.Tests and
Avowry.Acceptance.Tests, with nothing lost and nothing left pointing at the old paths.

One class per cycle, so later cycles add a class and never edit an earlier one:
  RenameCycle1     Criterion 1 (Z, E): the old names are gone from tracked live files; the new directories are tracked.
  ListsAndStrings  Criterion 2 (O, M, B): lists, strings and line endings survive the rename
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


# ---------------------------------------------------------------------------------------------------------------
# Cycle 2 (Criterion 2: O one list, M many strings, B boundary bytes). Born green on the finished rename:
# each test passes on the rename commit and has a named put-back (.mob/records/196/mutations/P2*.patch) that fails it.
# ---------------------------------------------------------------------------------------------------------------

BASELINE = REPO / ".mob" / "records" / "196" / "baseline"
CORPUS = "tests/" + NEW_ACCEPTANCE + "/Fixtures/corpus"
_SOURCE_ROW = re.compile(r'\(source: ".*$')


def normalised_test_list(project):
    """`dotnet test <project> --list-tests` reduced to its test rows (4-space lines), the 14 rows that embed the
    checkout path normalised to 'source: <path>)', sorted, duplicates kept."""
    result = subprocess.run(
        ["dotnet", "test", f"tests/{project}", "--list-tests"], cwd=REPO, capture_output=True, check=True
    )
    rows = []
    for line in result.stdout.decode("utf-8", "replace").splitlines():
        if line.startswith("    ") and line.strip():
            rows.append(_SOURCE_ROW.sub("(source: <path>)", line.rstrip()))
    return sorted(rows)


def baseline_test_list(file_name, old_project, new_project):
    rows = []
    for line in (BASELINE / file_name).read_text(encoding="utf-8").splitlines():
        if line.strip():
            rows.append(line.rstrip().replace(old_project + ".", new_project + ".", 1))
    return sorted(rows)


def multiset_difference(expected, actual, limit=8):
    from collections import Counter

    missing = list((Counter(expected) - Counter(actual)).elements())
    extra = list((Counter(actual) - Counter(expected)).elements())
    return (
        f"expected {len(expected)} rows, listed {len(actual)}; {len(missing)} missing, {len(extra)} extra.\n"
        f"missing (first {limit}): {missing[:limit]}\nextra (first {limit}): {extra[:limit]}"
    )


def corpus_fixtures():
    return [p for p in tracked_files() if p.startswith(CORPUS + "/") and p.endswith(".avowry")]


def eol_columns(text, old_project, new_project):
    """Map `git ls-files --eol` text to {path with the old project name: (index eol, worktree eol)}."""
    columns = {}
    for line in text.splitlines():
        left, path = line.split("\t", 1)
        index_eol, worktree_eol = left.split()[:2]
        columns[path.replace(new_project, old_project)] = (index_eol, worktree_eol)
    return columns


class ListsAndStrings(unittest.TestCase):
    """Criterion 2. O one (a single list per project, equal name for name), M many (every string and path that
    carries a name: the probe prefix, the corpus fixtures), B boundary (line-ending bytes survive the rename)."""

    def test_lists_equal(self):
        for old, new, file_name, count in (
            (OLD_COMPILER, NEW_COMPILER, "compiler-tests.list.txt", 284),
            (OLD_ACCEPTANCE, NEW_ACCEPTANCE, "acceptance-tests.list.txt", 317),
        ):
            with self.subTest(project=new):
                expected = baseline_test_list(file_name, old, new)
                actual = normalised_test_list(new)
                self.assertEqual(count, len(expected), "the committed baseline must hold the stated count")
                self.assertEqual(count, len(actual), multiset_difference(expected, actual))
                self.assertEqual(expected, actual, multiset_difference(expected, actual))

    def test_starvation_probe_prefix(self):
        rel = "tests/Fixtures/StarvationProbe/Program.cs"
        literals = []
        for number, line in enumerate((REPO / rel).read_text(encoding="utf-8").splitlines(), start=1):
            for found in re.finditer(r'StartsWith\("([^"]+)"', line):
                literals.append((number, found.group(1)))
        acceptance = [(n, s) for n, s in literals if "Acceptance" in s]
        self.assertEqual(1, len(acceptance), f"{rel} must probe the acceptance assembly once: {literals}")
        number, prefix = acceptance[0]
        csproj = sorted((REPO / "tests" / NEW_ACCEPTANCE).glob("*.csproj"))
        self.assertEqual(1, len(csproj), f"one csproj expected in tests/{NEW_ACCEPTANCE}: {csproj}")
        stem = csproj[0].stem
        self.assertEqual(
            stem,
            prefix + ".Tests",
            f"{rel}:{number} probes StartsWith(\"{prefix}\") but the project is {stem}",
        )

    def test_corpus_text_unset(self):
        fixtures = corpus_fixtures()
        self.assertEqual(14, len(fixtures), "14 tracked corpus fixtures expected")
        out = git("check-attr", "text", "--", *fixtures).decode("utf-8").splitlines()
        states = {}
        for line in out:
            path, _, state = line.rsplit(": ", 2)
            states[path] = state
        not_unset = {p: s for p, s in states.items() if s != "unset"}
        self.assertEqual({}, not_unset, f"{len(not_unset)} of {len(fixtures)} corpus fixtures are not 'text: unset', they say {sorted(set(not_unset.values()))}")
        self.assertEqual(sorted(fixtures), sorted(states))

    def test_corpus_renames_r100(self):
        added = git(
            "log", "--diff-filter=A", "--reverse", "--format=%H", "--", f"tests/{NEW_ACCEPTANCE}/Fixtures/orders.avowry"
        ).decode().split()
        self.assertTrue(added, "orders.avowry must have an adding commit")
        commit = added[0]
        # Pathless diff: a pathspec would hide the rename source and show an add instead.
        rows = git("diff", "-M", "--name-status", commit + "^", commit).decode("utf-8").splitlines()
        status = {}
        for row in rows:
            parts = row.split("\t")
            if parts[0].startswith("R"):
                status[parts[2]] = (parts[0], parts[1])
        fixtures = corpus_fixtures()
        self.assertEqual(14, len(fixtures))
        found = {p: status.get(p, ("not a rename",))[0] for p in fixtures}
        self.assertEqual({p: "R100" for p in fixtures}, found, f"commit {commit[:7]}")
        for p in fixtures:
            self.assertEqual(p.replace(NEW_ACCEPTANCE, OLD_ACCEPTANCE), status[p][1])

    def test_eol_unchanged(self):
        for old, new, file_name in (
            (OLD_COMPILER, NEW_COMPILER, "compiler-eol.txt"),
            (OLD_ACCEPTANCE, NEW_ACCEPTANCE, "acceptance-eol.txt"),
        ):
            with self.subTest(project=new):
                expected = eol_columns((BASELINE / file_name).read_text(encoding="utf-8"), old, new)
                actual = eol_columns(
                    git("-c", "core.quotepath=off", "ls-files", "--eol", "--", f"tests/{new}").decode("utf-8"), old, new
                )
                differing = sorted(p for p in set(expected) | set(actual) if expected.get(p) != actual.get(p))
                detail = "; ".join(f"{p}: baseline {expected.get(p)} now {actual.get(p)}" for p in differing[:10])
                self.assertEqual([], differing, f"{len(differing)} files differ in i/ w/ line endings: {detail}")


if __name__ == "__main__":
    unittest.main()
