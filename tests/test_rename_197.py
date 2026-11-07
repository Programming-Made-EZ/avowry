"""Card 197 (D157): the solution file, the two Bench tool projects and the executed-complete script are renamed
from the old names to Avowry.sln, Avowry.Bench, Avowry.BenchCs and check-avowry-executed-complete.py, with every
live line that names them rewritten and every other byte untouched.

One class per cycle, so later cycles add a class and never edit an earlier one:
  RenameCycle1     Criterion 1 (Z, E, B): no old name in live lines, the new paths are tracked with exact case,
                   a single put-back is caught, and the rename commit is the old bytes under the rules (byte parity)

The old names are built from fragments below, so this file never contains the literal old names and the scan
never matches itself.

Run: python -m unittest tests.test_rename_197.RenameCycle1
No test deletes anything: copies live in tempfile.TemporaryDirectory.
"""

import re
import subprocess
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent

_OLD_STEM = "Ph" + "rase"
_LOWER_STEM = _OLD_STEM.lower()

# One old form of a name per entry: plain, escaped dot (one or two backslashes) and backslash paths all match
# `stem + \\{0,2}\.(sln|bench)`; the script name is the second alternative. Case-insensitive, over bytes.
OLD_NAMES = re.compile(
    (_OLD_STEM + r"\\{0,2}\.(sln|bench)|check-" + _LOWER_STEM + r"-executed-complete").encode(), re.IGNORECASE
)

# Board, mob records, decision/retro history and benchmark history data keep the old names as history.
SKIPPED_PREFIXES = ("board/", ".mob/", "docs/decisions/", "docs/retros/")
SKIPPED_SUFFIXES = ("history.jsonl",)

# Pinned by the slice: no live line may keep an old name. Hard-coded empty.
EXCEPTIONS = ()
# Pinned by the slice: every changed file is the old bytes under the rules. Hard-coded empty.
PARITY_EXCEPTIONS = ()

NEW_SOLUTION = "Avowry.sln"
NEW_PATHS = (
    "Avowry.sln",
    "tools/Avowry.Bench/Avowry.Bench.csproj",
    "tools/Avowry.Bench/Program.cs",
    "tools/Avowry.BenchCs/Avowry.BenchCs.csproj",
    "tools/Avowry.BenchCs/Program.cs",
    "scripts/check-avowry-executed-complete.py",
)
OLD_PATHS = (
    _OLD_STEM + ".sln",
    "tools/" + _OLD_STEM + ".Bench/" + _OLD_STEM + ".Bench.csproj",
    "tools/" + _OLD_STEM + ".Bench/Program.cs",
    "tools/" + _OLD_STEM + ".BenchCs/" + _OLD_STEM + ".BenchCs.csproj",
    "tools/" + _OLD_STEM + ".BenchCs/Program.cs",
    "scripts/check-" + _LOWER_STEM + "-executed-complete.py",
)

# The new name that a put-back turns into the old one, keeping the case of the kind (sln, Bench).
NEW_NAME_TOKEN = re.compile(rb"Avowry\.(sln|Bench)")

# Files of the rename commit that are outside the comparison: the mob records, the board, and this test file
# (it is created by an earlier commit than the rename commit).
PARITY_SKIPPED_PREFIXES = (".mob/", "board/")
PARITY_SKIPPED_PATHS = ("tests/test_rename_197.py",)


def git(*args, repo=None, data=None):
    return subprocess.run(["git", *args], cwd=repo or REPO, capture_output=True, check=True, input=data).stdout


def tracked_files(repo=None):
    """Every tracked path (forward slashes), from the index."""
    return [p for p in git("ls-files", "-z", repo=repo).decode("utf-8").split("\0") if p]


def is_live(path):
    return not path.startswith(SKIPPED_PREFIXES) and not path.endswith(SKIPPED_SUFFIXES)


def live(paths):
    return [p for p in paths if is_live(p)]


def scan(root, paths):
    """Return 'path:line' for every line under root naming an old name, over the given live paths."""
    hits = []
    for rel in live(paths):
        path = Path(root) / rel
        if not path.is_file():
            continue
        for number, line in enumerate(path.read_bytes().split(b"\n"), start=1):
            if OLD_NAMES.search(line):
                hits.append(f"{rel}:{number}")
    return hits


def summarise(hits, limit=15):
    files = sorted({h.rsplit(":", 1)[0] for h in hits})
    shown = "\n  ".join(hits[:limit])
    return f"{len(hits)} lines in {len(files)} files name an old name; first {min(limit, len(hits))}:\n  {shown}"


def old_for(kind):
    return _OLD_STEM + "." + kind


def put_back(path, line_number):
    """Rewrite one line of path in place, as bytes so CR survives, putting one old name back."""
    lines = path.read_bytes().split(b"\n")
    index = line_number - 1
    lines[index] = NEW_NAME_TOKEN.sub(lambda m: old_for(m.group(1).decode()).encode(), lines[index], count=1)
    path.write_bytes(b"\n".join(lines))


def line_with(path, pattern):
    for number, line in enumerate(path.read_bytes().split(b"\n"), start=1):
        if pattern.search(line):
            return number
    return None


# ---------------------------------------------------------------------------------------------------------------
# Byte parity (the Experimental entry "Mechanical rewrites carry a byte-parity check", D202)
# ---------------------------------------------------------------------------------------------------------------

BYTE_RULES = (
    (_OLD_STEM + ".sln", "Avowry.sln"),
    (_OLD_STEM + ".Bench", "Avowry.Bench"),
    (_OLD_STEM + r"\.Bench", r"Avowry\.Bench"),
    ("check-" + _LOWER_STEM + "-executed-complete.py", "check-avowry-executed-complete.py"),
)
_BENCH_DIRS = (("tools/" + _OLD_STEM + ".Bench/", "tools/Avowry.Bench/", _OLD_STEM + ".Bench.csproj", "Avowry.Bench.csproj"),
               ("tools/" + _OLD_STEM + ".BenchCs/", "tools/Avowry.BenchCs/", _OLD_STEM + ".BenchCs.csproj", "Avowry.BenchCs.csproj"))


def rename_commit(repo=None):
    """C: the earliest commit that adds Avowry.sln. A pathspec shows the rename target as the add."""
    added = git("log", "--diff-filter=A", "--reverse", "--format=%H", "--", NEW_SOLUTION, repo=repo).decode().split()
    return added[0] if added else None


def map_path(path):
    """The git mv map: the old path of a file -> its new path (unchanged when the file does not move)."""
    if path == _OLD_STEM + ".sln":
        return NEW_SOLUTION
    for old_dir, new_dir, old_csproj, new_csproj in _BENCH_DIRS:
        if path.startswith(old_dir):
            rest = path[len(old_dir):]
            return new_dir + (new_csproj if rest == old_csproj else rest)
    if path == "scripts/check-" + _LOWER_STEM + "-executed-complete.py":
        return "scripts/check-avowry-executed-complete.py"
    return path


def apply_rules(old_bytes):
    new_bytes = old_bytes
    for old, new in BYTE_RULES:
        new_bytes = new_bytes.replace(old.encode(), new.encode())
    return new_bytes


def parity_skipped(path):
    return path.startswith(PARITY_SKIPPED_PREFIXES) or path in PARITY_SKIPPED_PATHS


def tree_entries(commit, repo=None):
    """{path: (mode, blob id)} for every blob of commit's tree."""
    entries = {}
    for record in git("ls-tree", "-r", "-z", commit, repo=repo).split(b"\0"):
        if not record:
            continue
        meta, path = record.split(b"\t", 1)
        mode, kind, blob = meta.decode().split()
        entries[path.decode("utf-8")] = (mode, blob, kind)
    return entries


def blob_bytes(blob_ids, repo=None):
    """Raw bytes of each blob id (no eol or text conversion), through one git cat-file --batch."""
    unique = sorted(set(blob_ids))
    if not unique:
        return {}
    data = git("cat-file", "--batch", repo=repo, data=("\n".join(unique) + "\n").encode())
    position, out = 0, {}
    for blob in unique:
        newline = data.index(b"\n", position)
        header = data[position:newline].split()
        if header[-1] == b"missing":
            raise AssertionError(f"blob {blob} is missing")
        size = int(header[2])
        out[blob] = data[newline + 1:newline + 1 + size]
        position = newline + 1 + size + 1
    return out


def parity_differences(commit, repo=None):
    """Compare commit's tree with the old tree (commit^) under the rules; return a list of difference lines."""
    parent = commit + "^"
    old_tree = {p: v for p, v in tree_entries(parent, repo).items() if not parity_skipped(p)}
    new_tree = {p: v for p, v in tree_entries(commit, repo).items() if not parity_skipped(p)}
    expected_paths = {map_path(p): p for p in old_tree}
    differences = []
    for path in sorted(set(expected_paths) - set(new_tree)):
        differences.append(f"MISSING {path} (from {expected_paths[path]}): not in the committed tree")
    for path in sorted(set(new_tree) - set(expected_paths)):
        differences.append(f"EXTRA {path}: in the committed tree but no old path maps to it")
    common = sorted(set(expected_paths) & set(new_tree))
    data = blob_bytes([old_tree[expected_paths[p]][1] for p in common] + [new_tree[p][1] for p in common], repo)
    for path in common:
        if path in PARITY_EXCEPTIONS:
            continue
        old_path = expected_paths[path]
        old_mode, old_blob, _ = old_tree[old_path]
        new_mode, new_blob, _ = new_tree[path]
        old_bytes, committed = data[old_blob], data[new_blob]
        expected = apply_rules(old_bytes) if is_live(old_path) else old_bytes
        if expected != committed:
            differences.append(
                f"{path} (old {len(old_bytes)} bytes, expected {len(expected)}, committed {len(committed)})"
            )
        if old_mode != new_mode:
            differences.append(f"{path} mode (old {old_mode}, committed {new_mode})")
    return differences


class RenameCycle1(unittest.TestCase):
    """Criterion 1. ZOMBIES: Z zero (no old name left, the new paths tracked), E exceptions (a single put-back is
    caught, the scan is not vacuous), B boundary (every byte of the rename is the old byte under the rules)."""

    def test_old_names_gone_from_live_lines(self):
        hits = scan(REPO, tracked_files())
        self.assertEqual(list(EXCEPTIONS), hits, summarise(hits))

    def test_new_paths_tracked_exact_case(self):
        tracked = set(tracked_files())
        missing = [p for p in NEW_PATHS if p not in tracked]
        still_tracked = [p for p in OLD_PATHS if p in tracked]
        lowered = {p.lower() for p in NEW_PATHS}
        wrong_case = sorted(p for p in tracked if p.lower() in lowered and p not in NEW_PATHS)
        self.assertEqual(
            {"new paths not tracked": [], "old paths still tracked": [], "paths with the wrong case": []},
            {"new paths not tracked": missing, "old paths still tracked": still_tracked, "paths with the wrong case": wrong_case},
            f"expected exactly {len(NEW_PATHS)} new tracked paths and none of the {len(OLD_PATHS)} old ones",
        )

    def test_scan_flags_injected_old_name(self):
        stem = _OLD_STEM
        forms = [
            f"dotnet restore {stem}.sln",
            f"tools\\{stem}.Bench\\{stem}.Bench.csproj",
            f'RunArm("{stem}.BenchCs")',
            f"{stem}\\.Bench\\.csproj|Project: \"{stem}\\.Bench\"",
            f"{stem}\\\\.Bench",
            f"load check-{stem.lower()}-executed-complete.py",
            f"{stem.upper()}.SLN and {stem.lower()}.bench",
        ]
        clean = ["dotnet restore Avowry.sln", "Avowry\\.Bench", f"a {stem} noun", "board/ is skipped below"]
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / "live.txt").write_bytes(("\r\n".join(forms + clean) + "\r\n").encode())
            (root / ".mob").mkdir()
            (root / ".mob" / "skipped.txt").write_bytes(forms[0].encode())
            (root / "bench").mkdir()
            (root / "bench" / "history.jsonl").write_bytes(forms[0].encode())
            hits = scan(root, ["live.txt", ".mob/skipped.txt", "bench/history.jsonl"])
        self.assertEqual([f"live.txt:{n}" for n in range(1, len(forms) + 1)], hits)

    def _put_back_one(self, rel, number=None):
        """Copy the one tracked file, assert the pristine copy scans clean, put one old name back on the line, and
        assert the scan reports exactly that file and line."""
        source = REPO / rel
        self.assertTrue(source.is_file(), f"{rel} must be a tracked file")
        with tempfile.TemporaryDirectory() as temp:
            target = Path(temp) / rel
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(source.read_bytes())
            pristine = scan(temp, [rel])
            self.assertEqual([], pristine, "precondition: the pristine copy must scan clean. " + summarise(pristine))
            if number is None:
                number = line_with(target, re.compile(rb'File\.Exists\(.*Avowry\.sln"'))
                self.assertIsNotNone(number, f"{rel} must probe for Avowry.sln")
            self.assertRegex(target.read_bytes().split(b"\n")[number - 1].decode("utf-8"), NEW_NAME_TOKEN.pattern.decode(),
                             f"{rel}:{number} must name a new name")
            put_back(target, number)
            self.assertEqual([f"{rel}:{number}"], scan(temp, [rel]))

    def test_put_back_ci_line(self):
        self._put_back_one(".github/workflows/ci.yml", 62)

    def test_put_back_repopaths_probe(self):
        probes = [p for p in tracked_files() if p.startswith("tests/") and p.endswith("/RepoPaths.cs")]
        self.assertEqual(1, len(probes), f"one RepoPaths.cs expected under tests/: {probes}")
        self._put_back_one(probes[0])

    def test_put_back_run_llm_check(self):
        self._put_back_one("scripts/run-llm-check.py", 499)

    def test_byte_parity(self):
        commit = rename_commit()
        self.assertTrue(commit, "no commit adds " + NEW_SOLUTION)
        differences = parity_differences(commit)
        self.assertEqual(
            [], differences,
            f"{len(differences)} differences between {commit[:7]}^ under the rules and {commit[:7]}:\n  " + "\n  ".join(differences),
        )


if __name__ == "__main__":
    unittest.main()
