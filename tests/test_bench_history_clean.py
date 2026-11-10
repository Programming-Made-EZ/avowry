import importlib.util
import os
import secrets
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location(
    "check_bench_history_clean",
    ROOT / "scripts" / "check-bench-history-clean.py",
)
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


class BenchHistoryCleanTests(unittest.TestCase):
    def scan(self, rows, accounts=None):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "history.jsonl"
            path.write_text("\n".join(rows) + "\n", encoding="utf-8")
            return check.scan_history(path, accounts=accounts)

    def test_reports_every_path_or_user_name_with_its_line_number(self):
        hits = self.scan(
            [
                r'{"reply_head":"C:\\Users\\someone\\reply.txt"}',
                r'{"diagnostics":["open ~/AppData/Local/Temp/x"]}',
                '{"error":"user ' + PLANTED.upper() + ' was named"}',
            ],
            accounts=(PLANTED,),
        )

        self.assertEqual([1, 2, 3], [hit[0] for hit in hits])
        self.assertEqual(
            ["drive path", "tilde-relative path", "user name"],
            [hit[1] for hit in hits],
        )

    def test_accepts_history_without_paths_or_the_user_name(self):
        self.assertEqual([], self.scan(['{"reply_head":"stored [path] is clean"}']))


PLANTED = "qa" + "name" + secrets.token_hex(3)
_BS = "\\"
_DRIVE = "C" + ":"
_USERS = "Us" + "ers"


def _win(account):
    return _DRIVE + _BS + _USERS + _BS + account


class BenchHistoryNameRows(unittest.TestCase):
    """376-1, rows B1 to B6: the bench checker refuses a planted account name and never prints it."""

    def scan(self, rows, accounts=None):
        root = Path(tempfile.mkdtemp(prefix="bench-history-376-"))
        path = root / "history.jsonl"
        path.write_text("\n".join(rows) + "\n", encoding="utf-8")
        return check.scan_history(path, accounts=accounts), path

    def pairs(self, hits):
        return [(hit[0], hit[1]) for hit in hits]

    def test_b1_a_planted_name_is_refused_at_its_line(self):
        hits, _path = self.scan(['{"ok":true}', '{"error":"user ' + PLANTED + ' was named"}'], accounts=(PLANTED,))
        self.assertEqual([(2, "user name")], self.pairs(hits), "B1: expected a user-name hit, got none")

    def test_b2_a_planted_name_in_upper_case_is_refused(self):
        hits, _path = self.scan(['{"error":"user ' + PLANTED.upper() + ' was named"}'], accounts=(PLANTED,))
        self.assertEqual([(1, "user name")], self.pairs(hits), "B2: expected a user-name hit, got none")

    def test_b3_many_planted_rows_each_give_a_hit(self):
        rows = []
        for index in range(50):
            rows.append('{"ok":true}')
            rows.append('{"error":"user ' + PLANTED + ' was named ' + str(index) + '"}')
        hits, _path = self.scan(rows, accounts=(PLANTED,))
        expected = [(line, "user name") for line in range(2, 101, 2)]
        self.assertEqual(expected, self.pairs(hits), "B3: expected a user-name hit, got none")

    def test_b4_the_neutral_form_is_accepted(self):
        hits, _path = self.scan(['{"error":"user <user> was named"}'], accounts=(PLANTED,))
        self.assertEqual([], self.pairs(hits), f"B4: expected no hit, got {self.pairs(hits)}")

    def test_b5_a_child_process_names_only_path_line_and_kind(self):
        root = Path(tempfile.mkdtemp(prefix="bench-history-376-b5-"))
        path = root / "history.jsonl"
        rows = [
            '{"reply_head":"' + _win(PLANTED).replace(_BS, _BS * 2) + _BS * 2 + 'reply.txt"}',
            '{"error":"user ' + PLANTED + ' was named"}',
        ]
        path.write_text("\n".join(rows) + "\n", encoding="utf-8")
        script = ROOT / "scripts" / "check-bench-history-clean.py"
        for variable in ("USERNAME", "USER", "LOGNAME"):
            with self.subTest(variable=variable):
                env = dict(os.environ)
                for name in ("LOGNAME", "USER", "LNAME", "USERNAME"):
                    env.pop(name, None)
                env[variable] = PLANTED
                result = subprocess.run(
                    [sys.executable, str(script), "history.jsonl"],
                    cwd=root, capture_output=True, text=True, env=env,
                )
                self.assertEqual(1, result.returncode, f"B5: expected exit 1, got {result.returncode}")
                expected = "history.jsonl:1:drive path\nhistory.jsonl:2:user name\nFound 2 potential leak(s).\n"
                self.assertEqual(expected, result.stdout, "B5: expected a user-name hit, got none")
                self.assertNotIn(PLANTED, result.stdout, "B5: the checker printed the planted name")
                self.assertNotIn(PLANTED, result.stderr, "B5: the checker printed the planted name")
                hits = check.scan_history(path, accounts=(PLANTED,))
                self.assertNotIn(PLANTED, repr(hits), "B5: a hit carries the planted name")

    def test_b6_a_name_inside_a_longer_word_is_ignored(self):
        rows = [
            '{"a":"x' + PLANTED + '"}',
            '{"b":"' + PLANTED + '7"}',
            '{"c":"9' + PLANTED + '"}',
        ]
        hits, _path = self.scan(rows, accounts=(PLANTED,))
        self.assertEqual([], self.pairs(hits), f"B6: expected no hit, got {self.pairs(hits)}")


if __name__ == "__main__":
    unittest.main()
