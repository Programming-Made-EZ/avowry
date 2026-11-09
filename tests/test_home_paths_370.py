"""Card 370: tracked files carry no home path of a local account (the neutral form is the placeholder segment).

One class per concern (D222 ids in the method names):
  TreeScan                          Criterion 370-1 (new-state): rows A1-A8, every tracked file by area, read from
                                    the working tree; a line holds a home path when the rewrite rule would change it
                                    (or the scanner reports it) outside the allow-list
  GeneratorRow                      Criterion 370-1 (new-state): a generated board card keeps its placeholders and
                                    writes no home path
  ScannerRows                       Criterion 370-3 (new-state unit): home_paths.find over planted samples, forms F1-F9
  PreCommitRefusesAHomePath         Criterion 370-3 (new-state): G1 and G2, check-no-secrets.py in a temporary repo
  ScrubChangesOnlyAccountSegments   characterisation: N1-N4, the reach, this card's own files, the MIGRATION.txt pin

A failure message names a file, a line and a form id, never the matched text. Every sample is built from parts, so
this file holds no home path. Temporary repositories are made with tempfile.mkdtemp and left in place (no test
deletes anything).

Run: python -m unittest tests.test_home_paths_370
The characterisation rows read the base commit recorded in .mob/records/370/scrub-commits.txt; they skip when that
commit is absent (a depth-1 clone), and a skip is not a pass.
"""

import functools
import json
import os
import re
import subprocess
import sys
import tempfile
import unittest
from collections import namedtuple
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "scripts"))

import home_paths  # noqa: E402
from decision_files import AREAS as DECISION_AREAS, parse_records, proof  # noqa: E402
from scripts.board import generate_archive  # noqa: E402

GUARD = ROOT / "scripts" / "check-no-secrets.py"
HOOK = ROOT / ".githooks" / "pre-commit"
SCRUB_COMMITS = ROOT / ".mob" / "records" / "370" / "scrub-commits.txt"
NEUTRAL = home_paths.PLACEHOLDER

# Planted samples: a fictional account and the path parts, joined only at run time.
BS = "\\"
ACCT = "qa" + "acct" + "7"
DRIVE = "C" + ":"
USERS = "Us" + "ers"
HOME = "ho" + "me"


def win(sep, account=ACCT, drive=DRIVE, users=USERS):
    return drive + sep + users + sep + account


def area_of(path):
    if path.startswith(".mob/"):
        return "A1"
    if path.startswith("board/"):
        return "A2"
    if path.startswith("tests/"):
        return "A3"
    if path.startswith("scripts/"):
        return "A4"
    if path.startswith("docs/"):
        return "A5"
    if path in ("HANDOFF.md", "TEAM_WORKING_AGREEMENT.md"):
        return "A6"
    if path.startswith("bench/"):
        return "A7"
    return "A8"


AREA_LABELS = {
    "A1": ".mob/",
    "A2": "board/",
    "A3": "tests/",
    "A4": "scripts/",
    "A5": "docs/",
    "A6": "HANDOFF.md and TEAM_WORKING_AGREEMENT.md",
    "A7": "bench/",
    "A8": "other",
}

DRIVE_ROOT = re.compile(rb"(?:/mnt)?/[a-z](?:\x5c|/|%5[cC]|%2[fF])+(?i:users)")


def form_of(line, path):
    """The form id of the first non-allowed home path on a line (an independent reading of FORMS for messages)."""
    found = []
    for pattern in (home_paths.PATH, home_paths.SLUG):
        for match in pattern.finditer(line):
            if home_paths.scrub_bytes(match.group(0), path) != match.group(0):
                found.append((match.start(), pattern is home_paths.SLUG, match))
                break
    if not found:
        return "F?"
    _, is_slug, match = min(found, key=lambda item: item[0])
    if is_slug:
        return "F5"
    pre, sep = match.group("pre"), match.group("sep")
    if b"%" in pre or b"%" in sep or line[:match.start()].endswith(b"file:///"):
        return "F8"
    if pre[1:2] == b":":
        if pre[-5:] != b"Users":
            return "F3"
        return "F1" if pre[2:3] == b"\\" else "F2"
    if DRIVE_ROOT.fullmatch(pre):
        return "F4"
    return "F6" if pre.endswith(b"Users") else "F7"


def git(*args, cwd=ROOT, check=True, data=None):
    return subprocess.run(["git", *args], cwd=cwd, capture_output=True, check=check, input=data)


@functools.lru_cache(maxsize=None)
def tree_hits():
    """{area id: [(path, line, form)]} over git ls-files, read from the working tree."""
    hits = {area: [] for area in AREA_LABELS}
    for relative in home_paths.tracked_paths(ROOT):
        target = ROOT / relative
        if not target.is_file():
            continue
        data = target.read_bytes()
        if b"\0" in data:
            continue
        lines = data.split(b"\n")
        found = {}
        for number, line in enumerate(lines, start=1):
            if home_paths.scrub_bytes(line, relative) != line:
                found[number] = form_of(line, relative)
        for hit in home_paths.find(relative, data.decode("utf-8", "replace")):
            found.setdefault(hit.line, hit.form)
        for number in sorted(found):
            hits[area_of(relative)].append((relative, number, found[number]))
    return hits


class TreeScan(unittest.TestCase):
    """370-1, rows A1-A8: no tracked file carries a home path outside the allow-list."""

    def assert_area_clean(self, area):
        hits = tree_hits()[area]
        files = len({path for path, _, _ in hits})
        first = ", ".join(f"{path}:{line}:{form}" for path, line, form in hits[:3])
        self.assertEqual(0, len(hits), f"{area} {AREA_LABELS[area]}: {len(hits)} hits in {files} files (first: {first})")

    def test_a1_mob_records(self):
        self.assert_area_clean("A1")

    def test_a2_board(self):
        self.assert_area_clean("A2")

    def test_a3_tests(self):
        self.assert_area_clean("A3")

    def test_a4_scripts(self):
        self.assert_area_clean("A4")

    def test_a5_docs(self):
        self.assert_area_clean("A5")

    def test_a6_handoff_and_working_agreement(self):
        self.assert_area_clean("A6")

    def test_a7_bench(self):
        self.assert_area_clean("A7")

    def test_a8_other(self):
        self.assert_area_clean("A8")


class GeneratorRow(unittest.TestCase):
    """370-1, A2 generator rows: a regenerated board card keeps its placeholders and writes no home path."""

    def setUp(self):
        self._saved = os.environ.get("MOB_DEFAULTS")
        os.environ["MOB_DEFAULTS"] = win(BS) + BS + "mob-crosscli"

    def tearDown(self):
        if self._saved is None:
            os.environ.pop("MOB_DEFAULTS", None)
        else:
            os.environ["MOB_DEFAULTS"] = self._saved

    @staticmethod
    def generate(body):
        issue = {"number": 9901, "title": "(901) Planted card", "body": "<!-- phrase-card:901 -->\n\n" + body}
        return generate_archive.archive_text(issue, "901")

    def test_a2g_placeholders_are_kept(self):
        text = self.generate("See <phrase-lang> and <mob-defaults>.\n")
        kept = [name for name in ("<phrase-lang>", "<mob-defaults>") if name in text]
        changed = sum(1 for line in text.encode("utf-8").split(b"\n") if home_paths.scrub_bytes(line) != line)
        self.assertEqual(2, len(kept), f"A2g placeholders: {len(kept)} of 2 kept in the generated card")
        self.assertEqual(0, changed, f"A2g placeholders: {changed} generated lines hold a home path")

    def test_a2g_a_literal_account_is_neutralised(self):
        text = self.generate("Run it from " + win(BS) + BS + "projects.\nOr " + win("/") + "/projects.\n")
        changed = sum(1 for line in text.encode("utf-8").split(b"\n") if home_paths.scrub_bytes(line) != line)
        self.assertEqual(0, changed, f"A2g literal: {changed} generated lines hold a home path")
        self.assertEqual(2, text.count(NEUTRAL), "A2g literal: the account segments did not become the placeholder")


Row = namedtuple("Row", "id path text line form")

PLAIN = "notes/plan.md"

MUST_HIT = (
    Row("F1_single", PLAIN, "a\nsee " + win(BS) + BS + "projects\n", 2, "F1"),
    Row("F1_double", "tools/a.py", 'p = "' + win(BS * 2) + BS * 2 + 'x2"\n', 1, "F1"),
    Row("F1_quad", "tools/a.json", '{"p": "' + win(BS * 4) + BS * 4 + 'x"}\n', 1, "F1"),
    Row("F2_forward", PLAIN, "a\nb\n" + win("/") + "/projects/x\n", 3, "F2"),
    Row("F3_lowercase", PLAIN, "c:" + BS + "us" + "ers" + BS + ACCT + BS + "src\n", 1, "F3"),
    Row("F4_drive_root", PLAIN, "cd /c/" + USERS + "/" + ACCT + "/src\n", 1, "F4"),
    Row("F4_mnt", PLAIN, "cd /mnt/c/" + USERS + "/" + ACCT + "/src\n", 1, "F4"),
    Row("F5_slug", PLAIN, "dir C--" + USERS + "-" + ACCT + "-projects-x\n", 1, "F5"),
    Row("F6_macos", PLAIN, "open /" + USERS + "/" + ACCT + "/Library\n", 1, "F6"),
    Row("F7_unix_home", PLAIN, "cd /" + HOME + "/" + ACCT + "/src\n", 1, "F7"),
    Row("F8_file_url", PLAIN, "<file:///" + win("/") + "/x.md>\n", 1, "F8"),
    Row("F8_url_encoded", PLAIN, "q=C%3A%5C" + USERS + "%5C" + ACCT + "%5Cdoc\n", 1, "F8"),
    Row("F9_end_of_line", PLAIN, "cd " + win(BS) + "\nnext\n", 1, "F1"),
    Row("F9_double_quote", "tools/b.py", 'x = "' + win(BS * 2) + '"\n', 1, "F1"),
    Row("F9_single_quote", "tools/b.py", "x = '" + win("/") + "'\n", 1, "F2"),
    Row("F9_backtick", PLAIN, "in `" + win(BS) + "` today\n", 1, "F1"),
    Row("F9_space", PLAIN, win(BS) + " is the home\n", 1, "F1"),
    Row("F9_file_end_no_newline", PLAIN, "a\n" + win("/"), 2, "F2"),
    Row("F9_trailing_dot", PLAIN, "it lives in " + win(BS) + ".\n", 1, "F1"),
    Row("pinned_segment_in_another_file", "tests/other_fixture.py", "p = '" + win(BS, "someone") + "'\n", 1, "F1"),
)

MUST_NOT_HIT = (
    Row("neutral_F1_single", PLAIN, win(BS, NEUTRAL) + BS + "projects\n", 0, ""),
    Row("neutral_F1_double", PLAIN, win(BS * 2, NEUTRAL) + BS * 2 + "x\n", 0, ""),
    Row("neutral_F2", PLAIN, win("/", NEUTRAL) + "/x\n", 0, ""),
    Row("neutral_F3", PLAIN, "c:" + BS + "us" + "ers" + BS + NEUTRAL + BS + "x\n", 0, ""),
    Row("neutral_F4", PLAIN, "/mnt/c/" + USERS + "/" + NEUTRAL + "/x and /c/" + USERS + "/" + NEUTRAL + "/x\n", 0, ""),
    Row("neutral_F5", PLAIN, "C--" + USERS + "-" + NEUTRAL + "-projects\n", 0, ""),
    Row("neutral_F6_F7", PLAIN, "/" + USERS + "/" + NEUTRAL + "/x and /" + HOME + "/" + NEUTRAL + "/x\n", 0, ""),
    Row("neutral_F8", PLAIN, "file:///" + win("/", NEUTRAL) + "/x and C%3A%5C" + USERS + "%5C" + NEUTRAL + "\n", 0, ""),
    Row("userprofile_variable", PLAIN, "%USERPROFILE%" + BS + "projects and SpecialFolder.UserProfile\n", 0, ""),
    Row("tilde", PLAIN, "cd ~/projects/x\n", 0, ""),
    Row("ci_runner", PLAIN, "/" + HOME + "/" + "run" + "ner" + "/work/x\n", 0, ""),
    Row("api_users_url", PLAIN, "https://api.github.com/" + "users" + "/x and https://api.github.com/" + "users" + "/"
        + ACCT + "/repos\n", 0, ""),
    Row("url_home_path", PLAIN, "https://example.com/" + HOME + "/page\n", 0, ""),
    Row("placeholder_x", PLAIN, win(BS, "x") + BS + "projects\n", 0, ""),
    Row("pinned_bench_history_fixture", "tests/test_bench_history_clean.py",
        "p = '" + win(BS * 2, "someone") + BS * 2 + "x'\n", 0, ""),
    Row("pinned_csharp_baseline_fixture", "tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs",
        'var p = "' + win(BS * 2, "al" + "ice") + BS * 2 + 'x";\n', 0, ""),
)


class ScannerRows(unittest.TestCase):
    """370-3 unit rows: home_paths.find over (path, text); hits carry a line and a form id only."""

    def test_hits_carry_no_text(self):
        text = "a " + win(BS) + "\nb " + win("/") + "\n"
        hits = home_paths.find(PLAIN, text)
        self.assertEqual(2, len(hits), f"expected 2 hits, got {len(hits)}")
        for hit in hits:
            self.assertEqual(("line", "form"), hit._fields)
            self.assertIn(hit.form, home_paths.FORMS)
        self.assertNotIn(ACCT, repr(hits), "a hit carries the matched text")


def _must_hit(row):
    def test(self):
        data = row.text.encode("utf-8")
        expected = row.text.replace(ACCT, NEUTRAL).replace("someone", NEUTRAL).encode("utf-8")
        self.assertEqual(expected, home_paths.scrub_bytes(data, row.path), f"{row.id}: the rewrite rule")
        got = [(hit.line, hit.form) for hit in home_paths.find(row.path, row.text)]
        self.assertEqual([(row.line, row.form)], got, f"{row.id}: expected a hit at line {row.line} ({row.form}), got {got}")
    return test


def _must_not_hit(row):
    def test(self):
        data = row.text.encode("utf-8")
        self.assertEqual(data, home_paths.scrub_bytes(data, row.path), f"{row.id}: the rewrite rule changed it")
        got = [(hit.line, hit.form) for hit in home_paths.find(row.path, row.text)]
        self.assertEqual([], got, f"{row.id}: expected no hit, got {got}")
    return test


for _row in MUST_HIT:
    setattr(ScannerRows, f"test_hit_{_row.id}", _must_hit(_row))
for _row in MUST_NOT_HIT:
    setattr(ScannerRows, f"test_no_hit_{_row.id}", _must_not_hit(_row))


def temp_repo(prefix):
    root = Path(tempfile.mkdtemp(prefix=prefix))
    git("init", "-q", cwd=root)
    return root


def run_guard(root):
    return subprocess.run([sys.executable, str(GUARD)], cwd=root, capture_output=True, text=True)


def planted_plan(account):
    return (
        "plan\n"
        + "build in " + win(BS, account) + BS + "projects\n"
        + 'path = "' + win(BS * 2, account) + BS * 2 + 'x"\n'
        + "slug C--" + USERS + "-" + account + "-projects\n"
        + "end\n"
    ).encode("utf-8")


class PreCommitRefusesAHomePath(unittest.TestCase):
    """370-3: the commit guard refuses a staged file (not MOB.md) that adds a home path, naming file and line only."""

    def test_g1_a_staged_home_path_is_refused(self):
        root = temp_repo("home-paths-370-g1-")
        (root / "notes").mkdir()
        (root / "notes" / "plan.txt").write_bytes(planted_plan(ACCT))
        git("add", "notes/plan.txt", cwd=root)
        result = run_guard(root)
        output = result.stdout + result.stderr
        self.assertEqual(1, result.returncode, f"G1: expected exit 1, got {result.returncode}")
        self.assertNotIn(ACCT, output, "G1: the guard printed the account")
        missing = [line for line in (2, 3, 4) if f"notes/plan.txt:{line}" not in output]
        self.assertEqual([], missing, f"G1: lines not named: {missing}")

    def test_g2_the_neutral_form_commits_cleanly(self):
        root = temp_repo("home-paths-370-g2-")
        (root / "notes").mkdir()
        (root / "notes" / "plan.txt").write_bytes(planted_plan(NEUTRAL))
        git("add", "notes/plan.txt", cwd=root)
        result = run_guard(root)
        self.assertEqual(0, result.returncode, f"G2: expected exit 0, got {result.returncode}")

    def test_the_hook_still_calls_the_guard(self):
        self.assertIn("scripts/check-no-secrets.py", HOOK.read_text(encoding="utf-8"))


def recorded_commits():
    found = {}
    for line in SCRUB_COMMITS.read_text(encoding="utf-8").splitlines():
        parts = line.split()
        if len(parts) == 2 and parts[0] in ("base", "after"):
            found[parts[0]] = parts[1]
    return found


def commit_exists(sha):
    return git("cat-file", "-e", sha + "^{commit}", check=False).returncode == 0


@functools.lru_cache(maxsize=None)
def tree_of(commit):
    """{path: blob sha} for the regular files of a commit."""
    out = git("ls-tree", "-r", "-z", commit).stdout
    blobs = {}
    for entry in out.split(b"\0"):
        if not entry:
            continue
        meta, path = entry.split(b"\t", 1)
        mode, kind, sha = meta.split()
        if kind == b"blob" and mode != b"120000":
            blobs[path.decode("utf-8")] = sha.decode("ascii")
    return blobs


@functools.lru_cache(maxsize=None)
def blob_contents(shas):
    """{sha: bytes} through one git cat-file --batch."""
    out = git("cat-file", "--batch", data="".join(sha + "\n" for sha in shas).encode("ascii")).stdout
    contents, at = {}, 0
    while at < len(out):
        header_end = out.index(b"\n", at)
        sha, _, size = out[at:header_end].split()
        start = header_end + 1
        contents[sha.decode("ascii")] = out[start:start + int(size)]
        at = start + int(size) + 1
    return contents


def blobs_at(commit, paths=None):
    tree = tree_of(commit)
    wanted = {path: tree[path] for path in (paths if paths is not None else tree) if path in tree}
    contents = blob_contents(tuple(sorted(set(wanted.values()))))
    return {path: contents[sha] for path, sha in wanted.items()}


@functools.lru_cache(maxsize=None)
def base_reach(base):
    """{path: (base bytes, rule applied)} for the base files the rule changes (file pins applied; NUL files skipped)."""
    reach = {}
    for path, data in blobs_at(base).items():
        if b"\0" in data:
            continue
        scrubbed = home_paths.scrub_bytes(data, path)
        if scrubbed != data:
            reach[path] = (data, scrubbed)
    return reach


# Hand-edited by green 1 (value sites and their tests), so N1 does not compare them with the rule.
HAND_EDITED = (
    "scripts/board/generate_archive.py",
    "scripts/board/check_real_board.py",
    "scripts/board/migrate_cards.py",
    "scripts/check-no-secrets.py",
    "tests/board/test_board_archive.py",
    "tests/board/test_board_migrate.py",
    "tests/board/test_board_sync.py",
)

OWN_FILES = (
    "scripts/home_paths.py",
    "tests/test_home_paths_370.py",
    "tests/Avowry.Acceptance.Tests/NoHomePathsInTrackedFiles.cs",
    "tests/Avowry.Acceptance.Tests/PreCommitRefusesAHomePath.cs",
    ".mob/records/370/characterisation-record.md",
    ".mob/records/370/red-record.md",
    ".mob/records/370/scrub-commits.txt",
)

PINNED_COUNT = 106


class ScrubChangesOnlyAccountSegments(unittest.TestCase):
    """Characterisation (passes before and after the rewrite): N1-N4, the reach, own files, the MIGRATION.txt pin."""

    def setUp(self):
        commits = recorded_commits()
        self.base = commits.get("base")
        self.assertIsNotNone(self.base, "scrub-commits.txt has no base line")
        self.target = commits.get("after") or git("rev-parse", "HEAD").stdout.decode("ascii").strip()

    def need_base(self):
        if not commit_exists(self.base):
            self.skipTest("the base commit is absent (a depth-1 clone); run in a full clone")

    def test_n1_every_change_is_the_rule_applied_to_the_base_blob(self):
        self.need_base()
        reach = base_reach(self.base)
        self.assertGreater(len(reach), 0, "N1: the base reach is empty")
        compared = [path for path in reach if path not in HAND_EDITED and path not in home_paths.FILE_ALLOW]
        target = blobs_at(self.target, compared)
        wrong = []
        for path in compared:
            if path not in target:
                continue
            before, scrubbed = reach[path]
            after = target[path]
            if after == before:
                continue
            if path.startswith("board/"):
                if home_paths.scrub_bytes(after, path) != after:
                    wrong.append(f"{path}: a home path is left")
            elif after != scrubbed:
                wrong.append(f"{path}: differs from the rule applied to its base blob")
        self.assertEqual([], wrong[:10], f"N1: {len(wrong)} files")

    def test_n2_no_base_file_is_missing_at_the_target(self):
        self.need_base()
        missing = sorted(set(tree_of(self.base)) - set(tree_of(self.target)))
        self.assertEqual([], missing[:10], f"N2: {len(missing)} base files missing at the target")

    def test_n3_the_rewrite_leaves_untracked_files_and_line_ends_alone(self):
        root = temp_repo("home-paths-370-n3-")
        crlf = ("one\r\nat " + win(BS) + BS + "projects\r\nthree\r\n").encode("utf-8")
        no_newline = ("one\n" + win("/")).encode("utf-8")
        untracked = ("at " + win(BS) + "\n").encode("utf-8")
        board = ("see " + win(BS) + "\n").encode("utf-8")
        binary = b"\0" + ("at " + win(BS)).encode("utf-8")
        (root / "board").mkdir()
        files = {"crlf.txt": crlf, "tail.txt": no_newline, "board/card.md": board, "blob.bin": binary}
        for name, data in files.items():
            (root / name).write_bytes(data)
        git("add", *files, cwd=root)
        (root / "untracked.txt").write_bytes(untracked)

        changed = home_paths.scrub_tracked(root, apply=True)

        self.assertEqual(["crlf.txt", "tail.txt"], sorted(changed), "N3: changed files")
        self.assertEqual(untracked, (root / "untracked.txt").read_bytes(), "N3: the untracked file changed")
        self.assertEqual(board, (root / "board" / "card.md").read_bytes(), "N3: an excluded board file changed")
        self.assertEqual(binary, (root / "blob.bin").read_bytes(), "N3: a file with a NUL byte changed")
        self.assertEqual(crlf.replace(ACCT.encode("ascii"), NEUTRAL.encode("ascii")), (root / "crlf.txt").read_bytes(),
                         "N3: the CRLF file is not the rule applied")
        self.assertEqual(crlf.count(b"\r\n"), (root / "crlf.txt").read_bytes().count(b"\r\n"), "N3: CRLF count")
        self.assertEqual(("one\n" + win("/", NEUTRAL)).encode("utf-8"), (root / "tail.txt").read_bytes(),
                         "N3: the file with no final newline")

    def test_n4_history_keeps_the_base_blob_and_the_base_is_an_ancestor(self):
        self.need_base()
        handoff = blobs_at(self.base, ["HANDOFF.md"])["HANDOFF.md"]
        self.assertNotEqual(handoff, home_paths.scrub_bytes(handoff, "HANDOFF.md"), "N4: the base HANDOFF.md holds no home path")
        ancestry = git("merge-base", "--is-ancestor", self.base, self.target, check=False).returncode
        self.assertEqual(0, ancestry, f"N4: the base is not an ancestor of {self.target}")

    def test_the_reach_is_not_empty(self):
        self.need_base()
        reach = base_reach(self.base)
        self.assertGreater(len(reach), 0, "the rule changes no base file")
        outside_board = [path for path in reach if not path.startswith("board/")]
        self.assertGreater(len(outside_board), 0, "the rule changes only board files")

    def test_this_cards_own_files_hold_no_home_path(self):
        found = []
        for relative in OWN_FILES:
            data = (ROOT / relative).read_bytes()
            for number, line in enumerate(data.split(b"\n"), start=1):
                if home_paths.scrub_bytes(line, relative) != line:
                    found.append(f"{relative}:{number}:{form_of(line, relative)}")
        self.assertEqual([], found)

    def test_the_migration_pin_is_the_proof_over_the_rule_applied_records(self):
        self.need_base()
        names = ["docs/decisions/MIGRATION.txt", "docs/decisions/areas.json"]
        names += [f"docs/decisions/{area}.md" for area in DECISION_AREAS]
        base = blobs_at(self.base, names)
        old_text = base["docs/decisions/MIGRATION.txt"].decode("utf-8")
        old_hash = re.search(r"(?m)^sha256 ([0-9a-f]{64})\r?$", old_text).group(1)
        self.assertRegex(old_text, rf"(?m)^count {PINNED_COUNT}\r?$")
        metadata = json.loads(base["docs/decisions/areas.json"].decode("utf-8"))
        plain, scrubbed = {}, {}
        for area in DECISION_AREAS:
            name = f"docs/decisions/{area}.md"
            for identifier, record in parse_records(base[name].decode("utf-8")):
                plain.setdefault(identifier, record)
            rule_applied = home_paths.scrub_bytes(base[name], name).decode("utf-8")
            for identifier, record in parse_records(rule_applied):
                scrubbed.setdefault(identifier, record)
        ordered = sorted(metadata, key=lambda i: metadata[i].get("order", list(metadata).index(i)))[:PINNED_COUNT]
        self.assertEqual("D114", ordered[-1], "the pinned records end at D114")
        changed = sorted(i for i in ordered if plain[i] != scrubbed[i])
        self.assertEqual(["D51"], changed, "the pinned records the rule changes")
        self.assertEqual(old_hash, proof(plain, metadata, PINNED_COUNT)[1], "the base pin is the proof over the base records")
        new_hash = proof(scrubbed, metadata, PINNED_COUNT)[1]
        self.assertNotEqual(old_hash, new_hash, "the rule-applied proof equals the old pin")
        target = blobs_at(self.target, ["docs/decisions/MIGRATION.txt"])["docs/decisions/MIGRATION.txt"].decode("utf-8")
        if target != old_text:
            self.assertRegex(target, rf"(?m)^count {PINNED_COUNT}\r?$")
            self.assertRegex(target, rf"(?m)^sha256 {new_hash}\r?$",
                             "MIGRATION.txt is not the proof over the rule-applied base records")


if __name__ == "__main__":
    unittest.main()
