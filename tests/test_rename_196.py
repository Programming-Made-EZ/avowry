"""Card 196 (D157): the two test projects are renamed from the old project names to Avowry.Compiler.Tests and
Avowry.Acceptance.Tests, with nothing lost and nothing left pointing at the old paths.

One class per cycle, so later cycles add a class and never edit an earlier one:
  RenameCycle1     Criterion 1 (Z, E): the old names are gone from tracked live files; the new directories are tracked.
  ListsAndStrings  Criterion 2 (O, M, B): lists, strings and line endings survive the rename
  PathBound        Criterion 3 (I, S): path-bound scripts, the solution and a byte-exact anchored rewrite hold

The old names are built from fragments below, so this file never contains the literal old names and the scan
never matches itself.

Run: python -m unittest tests.test_rename_196
No test deletes anything: copies live in tempfile.TemporaryDirectory.
"""

import re
import shutil
import subprocess
import sys
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
        self.assertEqual(25, len(fixtures), "25 tracked corpus fixtures expected")
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
        # Only the fixtures the rename commit carried; later fixtures (card 189) are new files, not renames.
        at_commit = git("ls-tree", "-r", "--name-only", commit).decode("utf-8").splitlines()
        fixtures = [p for p in corpus_fixtures() if p in at_commit]
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


# ---------------------------------------------------------------------------------------------------------------
# Cycle 3 (Criterion 3: I integration with the five python modules, S the solution and the byte-exact rewrite).
# Born green on the finished rename. Named put-backs: .mob/records/196/mutations/P3*.patch
# (see .mob/records/196/red-record-3.md).
# ---------------------------------------------------------------------------------------------------------------

PYTHON_MODULES = (
    ("tests.test_bench_launcher_scan_264", 6),
    ("tests.test_outcome_extractors_217", 17),
    ("tests.test_repeat_evidence", 21),
    ("tests.test_run_pairs_217", 36),
)
RENAME_MARKER = "tests/" + NEW_COMPILER + "/" + NEW_COMPILER + ".csproj"
STARVATION_PROBE = "tests/Fixtures/StarvationProbe/Program.cs"
# Paths of the rename commit that are not part of the rewrite: this test file and the mob records.
REWRITE_EXEMPT = ("tests/test_rename_196.py", ".mob/")


def rename_commit():
    """R: the commit that first added the Compiler csproj. Without --follow the log reports the rename target as
    the add, which is what we want here."""
    added = git("log", "--diff-filter=A", "--reverse", "--format=%H", "--", RENAME_MARKER).decode().split()
    if not added:
        raise AssertionError(f"no commit adds {RENAME_MARKER}")
    return added[0]


def changed_entries(commit):
    """(status, old path, new path) for every file changed between commit^ and commit, renames detected."""
    fields = [f for f in git("diff", "-M", "--name-status", "-z", commit + "^", commit).decode("utf-8").split("\0") if f]
    entries, i = [], 0
    while i < len(fields):
        status = fields[i]
        if status[0] in "RC":
            entries.append((status, fields[i + 1], fields[i + 2]))
            i += 3
        else:
            entries.append((status, fields[i + 1], fields[i + 1]))
            i += 2
    return entries


def blobs(specs):
    """Raw bytes of each 'rev:path' (no eol or text conversion), through one git cat-file --batch."""
    process = subprocess.run(
        ["git", "cat-file", "--batch"], cwd=REPO, input=("\n".join(specs) + "\n").encode("utf-8"),
        capture_output=True, check=True,
    )
    data, position, out = process.stdout, 0, {}
    for spec in specs:
        newline = data.index(b"\n", position)
        header = data[position:newline].split()
        if header[-1] == b"missing":
            raise AssertionError(f"{spec} is missing")
        size = int(header[2])
        out[spec] = data[newline + 1:newline + 1 + size]
        position = newline + 1 + size + 1
    return out


def anchored_rewrite(old_bytes, path):
    """The one anchored rewrite, at the byte level, plus the one explicit StarvationProbe edit."""
    new_bytes = OLD_NAMES.sub(lambda m: b"Avowry." + m.group(1) + b".Tests", old_bytes)
    if path == STARVATION_PROBE:
        new_bytes = new_bytes.replace(
            ('StartsWith("' + _OLD_STEM + '.Acceptance"').encode(), b'StartsWith("Avowry.Acceptance"'
        )
    return new_bytes


def run_module(module):
    """Run python -m unittest <module>; return (tests run, ok, seconds, stderr tail)."""
    import time

    started = time.time()
    result = subprocess.run([sys.executable, "-m", "unittest", module], cwd=REPO, capture_output=True)
    seconds = time.time() - started
    text = result.stderr.decode("utf-8", "replace")
    ran = re.search(r"Ran (\d+) tests?", text)
    ok = result.returncode == 0 and re.search(r"^OK\b", text, re.M) is not None
    return (int(ran.group(1)) if ran else None), ok, seconds, "\n".join(text.strip().splitlines()[-12:])


def load_script(name):
    import importlib.util

    spec = importlib.util.spec_from_file_location(name.replace("-", "_") + "_196", REPO / "scripts" / f"{name}.py")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def project_guids(text):
    """{project name: (csproj path, guid)} from the Project(...) lines of a solution file."""
    found = {}
    pattern = r'^Project\("\{[0-9A-Fa-f-]+\}"\) = "([^"]+)", "([^"]+)", "\{([0-9A-Fa-f-]+)\}"'
    for m in re.finditer(pattern, text, re.M):
        found[m.group(1)] = (m.group(2), "{" + m.group(3).upper() + "}")
    return found


class PathBound(unittest.TestCase):
    """Criterion 3. I integration (scripts and tests that resolve a project by name or path do real work, not
    vacuous work), S solution (Avowry.sln pairs each project with its own GUID, the fixture link resolves, and every
    changed file is the old file's bytes under the one anchored rewrite)."""

    def test_five_modules(self):
        problems, timings = [], []
        for module, expected in PYTHON_MODULES:
            ran, ok, seconds, tail = run_module(module)
            timings.append(f"{module} {ran} {'OK' if ok else 'FAILED'} {seconds:.1f}s")
            if ran != expected or not ok:
                problems.append(f"{module}: expected {expected} tests OK, got ran={ran} ok={ok}\n{tail}")
        self.assertEqual([], problems, "\n".join(problems) + "\n" + "; ".join(timings))

    def test_launcher_scan_not_vacuous(self):
        import importlib

        module = importlib.import_module("tests.test_bench_launcher_scan_264")
        directory = Path(module.ACCEPTANCE)
        self.assertTrue(directory.is_dir(), f"ACCEPTANCE {directory} is not a directory")
        sources = [p for p in directory.rglob("*.cs") if not {"bin", "obj"} & set(p.relative_to(directory).parts)]
        # 69 tracked .cs files at the rename (the brief's 90 was a miscount: 100 is every tracked file in the project).
        self.assertGreaterEqual(
            len(sources), 60, f"ACCEPTANCE {directory} holds {len(sources)} .cs files; a missed path scans nothing"
        )

    def test_extractor_not_empty(self):
        extractor = load_script("ci-log-outcomes-217")
        acceptance = NEW_ACCEPTANCE
        lines = [
            f"   Passed {acceptance}.Fake.One [3 s]",
            f"   Passed {acceptance}.Fake.Two [3 s]",
            f"   Failed {acceptance}.Fake.Three [3 s]",
            "", "Test Run Failed.", "Total tests: 3", "     Passed: 2", "     Failed: 1", " Total time: 1 Minutes",
        ]
        log = "\n".join(f"test\tUNKNOWN STEP\t2026-11-05T17:59:45.0000000Z{line}" for line in lines) + "\n"
        try:
            outcomes = extractor.extract_text(log.encode("utf-8"), "synthetic.txt", job="test")["outcomes"]
        except extractor.Rejected as rejected:
            self.fail(f"the 217 extractor rejects a log of {acceptance} result lines: {rejected}")
        self.assertEqual(
            {f"{acceptance}.Fake.One": "Passed", f"{acceptance}.Fake.Two": "Passed", f"{acceptance}.Fake.Three": "Failed"},
            outcomes,
        )

    def test_solution_guids(self):
        wanted = {}
        for line in (BASELINE / "project-guids.txt").read_text(encoding="utf-8").splitlines():
            if line.strip():
                name, guid = line.split()
                wanted[name.replace(_OLD_STEM + ".", "Avowry.", 1)] = guid.upper()
        self.assertEqual({NEW_ACCEPTANCE, NEW_COMPILER}, set(wanted), "the baseline must hold both project GUIDs")
        solution = project_guids((REPO / "Avowry.sln").read_text(encoding="utf-8"))
        wrong = []
        for name, guid in sorted(wanted.items()):
            expected_path = f"tests\\{name}\\{name}.csproj"
            found = solution.get(name)
            if found is None:
                wrong.append(f"{name}: no Project line in Avowry.sln")
                continue
            if found != (expected_path, guid):
                wrong.append(f"{name}: Avowry.sln says {found}, expected {(expected_path, guid)}")
            if not (REPO / expected_path.replace("\\", "/")).is_file():
                wrong.append(f"{name}: {expected_path} does not exist")
        self.assertEqual([], wrong, "\n".join(wrong))
        csproj = (REPO / "tests" / NEW_COMPILER / (NEW_COMPILER + ".csproj")).read_text(encoding="utf-8")
        links = re.findall(r'<None Include="([^"]+)/\*\*/\*" LinkBase="Fixtures"', csproj)
        self.assertEqual(["../" + NEW_ACCEPTANCE + "/Fixtures"], links, "the Compiler csproj must link the acceptance fixtures")
        target = (REPO / "tests" / NEW_COMPILER / links[0]).resolve()
        self.assertTrue(target.is_dir(), f"the Compiler csproj link target {links[0]} ({target}) does not exist")

    def test_rewrite_is_byte_exact_and_anchored(self):
        commit = rename_commit()
        entries = [e for e in changed_entries(commit) if not e[2].startswith(REWRITE_EXEMPT)]
        self.assertGreater(len(entries), 100, f"rename commit {commit[:7]} changes {len(entries)} files")
        unexpected = [f"{s} {o} -> {n}" for s, o, n in entries if s[0] not in "RM"]
        self.assertEqual([], unexpected, f"only R and M entries expected in {commit[:7]}")
        specs = [f"{commit}^:{old}" for _, old, _ in entries] + [f"{commit}:{new}" for _, _, new in entries]
        data = blobs(specs)
        differing, untouched_not_r100 = [], []
        for status, old, new in entries:
            old_bytes, new_bytes = data[f"{commit}^:{old}"], data[f"{commit}:{new}"]
            expected = anchored_rewrite(old_bytes, new)
            if expected != new_bytes:
                differing.append(f"{new} (old {len(old_bytes)} bytes, rewrite {len(expected)}, committed {len(new_bytes)})")
            if expected == old_bytes and status != "R100":
                untouched_not_r100.append(f"{new} is {status} but needs no rewrite")
        self.assertEqual(
            [], differing,
            f"{len(differing)} of {len(entries)} changed files differ from the old bytes under the one rewrite: " + "; ".join(differing),
        )
        self.assertEqual([], untouched_not_r100, "a file the rewrite leaves alone must be a pure rename (R100)")


if __name__ == "__main__":
    unittest.main()
