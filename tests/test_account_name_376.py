"""Card 376: the account name standing alone, the tree-scan exit code and a spaced account name.

Classes (D222 ids in the method names):
  WholeWordTreeScan            Criterion 376-1: W-A1 to W-A8, union of an independent whole-word
                               reading with home_paths.find(path, text, accounts)
  LocalAccountRows             Criterion 376-1: U1, local_accounts filtering
  TreeScanGatesByExitCode      Criterion 376-2: T1 to T4, G3, G4 in temporary repositories
  SpacedAccountRows            Criterion 376-3: S1 to S6, S1eol, S7
  SpaceRuleChangesNothingToday characterisation: C1
  RecordRewriteIsTheRule       characterisation: R1 (base by recorded hash in
                               .mob/records/376/scrub-commits.txt)

A failure message names a file, a line and a form id, never the matched text. Every planted
sample is built from parts at run time, so this file holds no account name. Temporary
repositories are made with tempfile.mkdtemp and left in place (no test deletes anything).
A skip for lack of a local account name is not a pass.

Run: python -m unittest tests.test_account_name_376
"""

import functools
import os
import re
import secrets
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "scripts"))

import home_paths  # noqa: E402

GUARD = ROOT / "scripts" / "check-no-secrets.py"
SCRUB_COMMITS = ROOT / ".mob" / "records" / "376" / "scrub-commits.txt"
NEUTRAL = home_paths.PLACEHOLDER
PLAIN = "notes/plan.md"

BS = "\\"
ACCT = "qa" + "acct" + "7"
SP = "qa" + "acct" + " " + "seven"
PLANTED = "qa" + "name" + secrets.token_hex(3)
DRIVE = "C" + ":"
USERS = "Us" + "ers"


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

RECORD_PATHS = (
    ".mob/records/226/record-in-progress.json",
    ".mob/records/229/nohome-check/nohome229_check.py",
    ".mob/records/276/record-in-progress.json",
    ".mob/records/276/signoff-ctx6.md",
    ".mob/records/313/green1-codex-events.jsonl",
    ".mob/records/313/slice-architect.md",
)

HAND_EDITED = (
    "scripts/check-bench-history-clean.py",
    "tests/test_bench_history_clean.py",
)


def whole_word_pattern(accounts):
    names = sorted({name for name in accounts if name}, key=len, reverse=True)
    if not names:
        return None
    return re.compile(r"(?<![^\W_])(?:" + "|".join(re.escape(name) for name in names) + r")(?![^\W_])", re.IGNORECASE)


def whole_word_lines(text, accounts):
    """Line numbers holding an account name as a whole word (an independent reading)."""
    pattern = whole_word_pattern(accounts)
    if pattern is None:
        return []
    return [number for number, line in enumerate(text.splitlines(), start=1) if pattern.search(line)]


def git(*args, cwd=ROOT, check=True, data=None):
    return subprocess.run(["git", *args], cwd=cwd, capture_output=True, check=check, input=data)


@functools.lru_cache(maxsize=None)
def tree_hits():
    """{area id: [(path, line, form)]}: find(accounts) unioned with the whole-word reading."""
    accounts = home_paths.local_accounts()
    hits = {area: [] for area in AREA_LABELS}
    for relative in home_paths.tracked_paths(ROOT):
        target = ROOT / relative
        if not target.is_file():
            continue
        data = target.read_bytes()
        if b"\0" in data:
            continue
        text = data.decode("utf-8", "replace")
        found = {}
        for hit in home_paths.find(relative, text, accounts):
            found[hit.line] = hit.form
        for number in whole_word_lines(text, accounts):
            found.setdefault(number, "F10")
        for number in sorted(found):
            hits[area_of(relative)].append((relative, number, found[number]))
    return hits


class WholeWordTreeScan(unittest.TestCase):
    """376-1, rows W-A1 to W-A8: no tracked file holds the local account name as a whole word."""

    def assert_area_clean(self, area):
        accounts = home_paths.local_accounts()
        if not accounts:
            self.skipTest("no local account name outside the allow-list")
        hits = tree_hits()[area]
        files = len({path for path, _, _ in hits})
        first = ", ".join(f"{path}:{line}:{form}" for path, line, form in hits[:3])
        self.assertEqual(
            0, len(hits),
            f"{area} {AREA_LABELS[area]}: {len(hits)} hits in {files} files (first: {first})",
        )

    def test_w_a1_mob_records(self):
        self.assert_area_clean("A1")

    def test_w_a2_board(self):
        self.assert_area_clean("A2")

    def test_w_a3_tests(self):
        self.assert_area_clean("A3")

    def test_w_a4_scripts(self):
        self.assert_area_clean("A4")

    def test_w_a5_docs(self):
        self.assert_area_clean("A5")

    def test_w_a6_handoff_and_working_agreement(self):
        self.assert_area_clean("A6")

    def test_w_a7_bench(self):
        self.assert_area_clean("A7")

    def test_w_a8_other(self):
        self.assert_area_clean("A8")


class LocalAccountRows(unittest.TestCase):
    """376-1, row U1: local_accounts drops allow-listed, generic, short and duplicate names."""

    def test_u1_local_accounts_filters_candidates(self):
        got = home_paths.local_accounts(["run" + "ner", "Run" + "ner", ACCT, ACCT.upper(), "ab", "ro" + "ot"])
        self.assertEqual((ACCT,), got, f"U1: expected ({ACCT!r},), got {got!r}")


def temp_repo(prefix):
    root = Path(tempfile.mkdtemp(prefix=prefix))
    git("init", "-q", cwd=root)
    return root


def planted_env():
    env = dict(os.environ)
    for name in ("LOGNAME", "USER", "LNAME", "USERNAME"):
        env.pop(name, None)
    env["USERNAME"] = PLANTED
    return env


def run_guard(root, *args):
    return subprocess.run(
        [sys.executable, str(GUARD), *args], cwd=root, capture_output=True, text=True, env=planted_env()
    )


def write_tracked(root, name, data):
    target = root / name
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(data)
    git("add", name, cwd=root)
    return target


class TreeScanGatesByExitCode(unittest.TestCase):
    """376-2: --tree and the staged guard gate by exit code (T1 to T4, G3, G4)."""

    def test_t1_a_tracked_home_path_exits_1(self):
        root = temp_repo("account-name-376-t1-")
        write_tracked(root, PLAIN, ("plan\nbuild in " + win(BS) + BS + "projects\n").encode("utf-8"))
        result = run_guard(root, "--tree")
        self.assertEqual(1, result.returncode, f"T1: expected exit 1, got {result.returncode}")
        self.assertIn("notes/plan.md:2:F1", result.stdout, "T1: the hit is not named as path:line:form")

    def test_t2_the_account_name_alone_exits_1_as_f10(self):
        root = temp_repo("account-name-376-t2-")
        write_tracked(root, PLAIN, ("plan\n" + PLANTED + "\n").encode("utf-8"))
        result = run_guard(root, "--tree")
        self.assertEqual(1, result.returncode, f"T2: expected exit 1, got {result.returncode}")
        self.assertIn("notes/plan.md:2:F10", result.stdout, "T2: the hit is not named as path:line:form")

    def test_t3_only_the_neutral_form_exits_0(self):
        root = temp_repo("account-name-376-t3-")
        text = "plan\n" + NEUTRAL + "\nbuild in " + win(BS, NEUTRAL) + BS + "projects\n"
        write_tracked(root, PLAIN, text.encode("utf-8"))
        result = run_guard(root, "--tree")
        self.assertEqual(0, result.returncode, f"T3: expected exit 0, got {result.returncode}")

    def test_t4_no_planted_text_and_the_hit_list_is_capped(self):
        home_root = temp_repo("account-name-376-t4a-")
        write_tracked(home_root, PLAIN, ("plan\nbuild in " + win(BS) + BS + "projects\n").encode("utf-8"))
        home_out = run_guard(home_root, "--tree").stdout
        self.assertNotIn(win(BS), home_out, "T4: the tree scan printed a planted home path")
        name_root = temp_repo("account-name-376-t4b-")
        write_tracked(name_root, PLAIN, ("plan\n" + PLANTED + "\n").encode("utf-8"))
        name_out = run_guard(name_root, "--tree").stdout
        self.assertNotIn(PLANTED, name_out, "T4: the tree scan printed the planted name")
        many_root = temp_repo("account-name-376-t4c-")
        write_tracked(many_root, PLAIN, "".join(PLANTED + "\n" for _ in range(60)).encode("utf-8"))
        many_out = run_guard(many_root, "--tree").stdout
        listed = [line for line in many_out.splitlines() if re.fullmatch(r"[^:]+:\d+:[A-Z][0-9]+", line)]
        self.assertLessEqual(len(listed), 50, f"T4: {len(listed)} hit lines listed, expected at most 50")
        self.assertIn("60 hits in 1 files", many_out.splitlines(), "T4: the total line does not read '60 hits in 1 files'")

    def test_g3_a_staged_account_name_is_refused(self):
        root = temp_repo("account-name-376-g3-")
        write_tracked(root, "notes/plan.txt", ("plan\n" + PLANTED + "\n").encode("utf-8"))
        result = run_guard(root)
        output = result.stdout + result.stderr
        self.assertEqual(1, result.returncode, f"G3: expected exit 1, got {result.returncode}")
        self.assertIn("notes/plan.txt:2", output, "G3: the staged file and line are not named")
        self.assertIn("F10", output, "G3: the form F10 is not named")
        self.assertNotIn(PLANTED, output, "G3: the guard printed the planted name")

    def test_g4_the_neutral_form_commits_cleanly(self):
        root = temp_repo("account-name-376-g4-")
        text = "plan\n" + NEUTRAL + "\nbuild in " + win(BS, NEUTRAL) + BS + "projects\n"
        write_tracked(root, "notes/plan.txt", text.encode("utf-8"))
        result = run_guard(root)
        self.assertEqual(0, result.returncode, f"G4: expected exit 0, got {result.returncode}")


class SpacedAccountRows(unittest.TestCase):
    """376-3: a two-word account segment becomes the placeholder; prose stays (S1 to S6, S1eol, S7)."""

    def check_row(self, row_id, text, expected, line, form):
        data = text.encode("utf-8")
        self.assertEqual(
            expected.encode("utf-8"), home_paths.scrub_bytes(data, PLAIN),
            f"{row_id}: the rewrite rule",
        )
        got = [(hit.line, hit.form) for hit in home_paths.find(PLAIN, text)]
        self.assertEqual([(line, form)], got, f"{row_id}: expected a hit at line {line} ({form}), got {got}")

    def test_s1_backslash(self):
        text = "see " + win(BS, SP) + BS + "projects\n"
        self.check_row("S1", text, "see " + win(BS, NEUTRAL) + BS + "projects\n", 1, "F1")

    def test_s1eol_at_end_of_line(self):
        text = "see " + win(BS, SP) + "\n"
        self.check_row("S1eol", text, "see " + win(BS, NEUTRAL) + "\n", 1, "F1")

    def test_s2_forward_slash(self):
        text = win("/", SP) + "/projects/x\n"
        self.check_row("S2", text, win("/", NEUTRAL) + "/projects/x\n", 1, "F2")

    def test_s3_doubled_backslash(self):
        text = 'p = "' + win(BS * 2, SP) + BS * 2 + 'x"\n'
        self.check_row("S3", text, 'p = "' + win(BS * 2, NEUTRAL) + BS * 2 + 'x"\n', 1, "F1")

    def test_s4_closing_quote_or_backtick(self):
        rows = (
            ('x = "' + win(BS, SP) + '"\n', 'x = "' + win(BS, NEUTRAL) + '"\n', "F1"),
            ("x = '" + win("/", SP) + "'\n", "x = '" + win("/", NEUTRAL) + "'\n", "F2"),
            ("in `" + win(BS, SP) + "` today\n", "in `" + win(BS, NEUTRAL) + "` today\n", "F1"),
        )
        for text, expected, form in rows:
            with self.subTest(form=form):
                self.check_row("S4", text, expected, 1, form)

    def test_s5_url_encoded_space(self):
        encoded = "qa" + "acct" + "%20" + "seven"
        rows = (
            (
                "q=" + "C" + "%3A%5C" + USERS + "%5C" + encoded + "%5Cdoc\n",
                "q=" + "C" + "%3A%5C" + USERS + "%5C" + NEUTRAL + "%5Cdoc\n",
            ),
            (
                "<file:///" + win("/", encoded) + "/x.md>\n",
                "<file:///" + win("/", NEUTRAL) + "/x.md>\n",
            ),
        )
        for text, expected in rows:
            with self.subTest(text=text[:12]):
                self.check_row("S5", text, expected, 1, "F8")

    def test_s6_prose_after_a_path_stays(self):
        rows = (
            win(BS) + " is the home\n",
            win(BS) + " and the files live in /srv/x\n",
            win(BS) + " one two three/x\n",
        )
        for text in rows:
            with self.subTest(text=text[:20]):
                self.check_row("S6", text, text.replace(ACCT, NEUTRAL), 1, "F1")

    def test_s7_account_root_re_captures_the_spaced_segment(self):
        pattern = home_paths.account_root_re(r"(?:\\|/)projects")
        match = pattern.search(win(BS, SP) + BS + "projects")
        self.assertIsNotNone(match, "S7: account_root_re did not match")
        self.assertEqual(SP, match.group("seg"), f"S7: expected the spaced segment, got {match.group('seg')!r}")


class SpaceRuleChangesNothingToday(unittest.TestCase):
    """Characterisation C1: the space rule changes no tracked file today."""

    def test_c1_scrub_tracked_changes_nothing(self):
        changed = home_paths.scrub_tracked(ROOT, exclude=())
        self.assertEqual([], changed, f"C1: scrub_tracked would change {changed[:5]}")


def recorded_commits(path=SCRUB_COMMITS):
    found = {}
    for line in Path(path).read_text(encoding="utf-8").splitlines():
        parts = line.split()
        if len(parts) == 2 and parts[0] in ("base", "after"):
            found[parts[0]] = parts[1]
    return found


def commit_exists(sha):
    return git("cat-file", "-e", sha + "^{commit}", check=False).returncode == 0


@functools.lru_cache(maxsize=None)
def tree_of(commit):
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


class RecordRewriteIsTheRule(unittest.TestCase):
    """Characterisation R1: each record at the target is its base blob or that blob with the rule applied."""

    def test_r1_records_equal_the_base_or_the_rule_applied(self):
        accounts = home_paths.local_accounts()
        if not accounts:
            self.skipTest("no local account name outside the allow-list")
        commits = recorded_commits()
        base = commits.get("base")
        self.assertIsNotNone(base, "R1: scrub-commits.txt has no base line")
        if not commit_exists(base):
            self.skipTest("the base commit is absent (a depth-1 clone)")
        target = commits.get("after") or git("rev-parse", "HEAD").stdout.decode("ascii").strip()
        base_blobs = blobs_at(base)
        hit_files = sorted(
            path for path, data in base_blobs.items()
            if b"\0" not in data and whole_word_lines(data.decode("utf-8", "replace"), accounts)
        )
        if not hit_files:
            self.skipTest("the base blobs show no F10 hit for the local accounts")
        self.assertEqual(
            sorted(RECORD_PATHS + HAND_EDITED), hit_files,
            f"R1: base files with an F10 hit are {hit_files}",
        )
        target_blobs = blobs_at(target, RECORD_PATHS)
        for path in RECORD_PATHS:
            before = base_blobs[path]
            after = target_blobs.get(path)
            unchanged = after == before
            ruled = (
                not unchanged
                and after == home_paths.scrub_account_bytes(before, accounts)
            )
            self.assertTrue(
                unchanged or ruled,
                f"R1: {path} at the target is neither its base blob nor the rule applied to it",
            )


if __name__ == "__main__":
    unittest.main()

