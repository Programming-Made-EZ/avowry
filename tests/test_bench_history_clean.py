import importlib.util
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
    def scan(self, rows):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "history.jsonl"
            path.write_text("\n".join(rows) + "\n", encoding="utf-8")
            return check.scan_history(path)

    def test_reports_every_path_or_user_name_with_its_line_number(self):
        hits = self.scan(
            [
                r'{"reply_head":"C:\\Users\\someone\\reply.txt"}',
                r'{"diagnostics":["open ~/AppData/Local/Temp/x"]}',
                '{"error":"user RINGE was named"}',
            ]
        )

        self.assertEqual([1, 2, 3], [hit[0] for hit in hits])
        self.assertEqual(
            ["drive path", "tilde-relative path", "user name"],
            [hit[1] for hit in hits],
        )

    def test_accepts_history_without_paths_or_the_user_name(self):
        self.assertEqual([], self.scan(['{"reply_head":"stored [path] is clean"}']))


if __name__ == "__main__":
    unittest.main()
