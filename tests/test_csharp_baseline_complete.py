import importlib.util
import json
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location(
    "check_csharp_baseline_complete",
    ROOT / "scripts" / "check-csharp-baseline-complete.py",
)
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


class CSharpBaselineCompleteTests(unittest.TestCase):
    def make_repo(self, phrase_tasks, csharp_tasks):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            bench = root / "bench"
            csharp = bench / "csharp-baseline"
            (csharp / "tasks").mkdir(parents=True, exist_ok=True)

            for task in phrase_tasks:
                if task["holdout"]:
                    continue
                meta_path = bench / "suite.json"
                meta_path.write_text(json.dumps({"version": 1, "tasks": phrase_tasks}, indent=2), encoding="utf-8")

            for item in csharp_tasks:
                task_id = item["id"]
                task_root = csharp / "tasks" / task_id
                task_root.mkdir(parents=True, exist_ok=True)
                (task_root / "prompt.md").write_text(f"prompt for {task_id}", encoding="utf-8")
                (task_root / "outcomes.json").write_text(json.dumps([{"name": "works", "given": {}, "when": "called", "then": {"returns": 1}}]), encoding="utf-8")
                if item["kind"] in {"read", "modify", "fix"}:
                    (task_root / "start.cs").write_text("namespace Demo; public static class Demo { }", encoding="utf-8")
            (csharp / "suite.json").write_text(json.dumps({"version": 1, "tasks": csharp_tasks}, indent=2), encoding="utf-8")
            yield root

    def build_task(self, task_id, kind, holdout=False):
        return {
            "id": task_id,
            "kind": kind,
            "prompt": f"tasks/{task_id}/prompt.md",
            "holdout": holdout,
        }

    def full_suite(self):
        phrase = [
            self.build_task("001-write-greeting", "write"),
            self.build_task("002-write-total", "write"),
            self.build_task("004-read-phrase", "read"),
            self.build_task("005-read-failure", "read"),
            self.build_task("007-modify-return", "modify"),
            self.build_task("008-modify-type", "modify"),
            self.build_task("010-fix-trigger", "fix"),
            self.build_task("011-fix-type", "fix"),
        ]
        csharp = [
            {"id": "001-write-greeting", "kind": "write", "prompt": "tasks/001-write-greeting/prompt.md", "outcomes": "tasks/001-write-greeting/outcomes.json", "holdout": False},
            {"id": "002-write-total", "kind": "write", "prompt": "tasks/002-write-total/prompt.md", "outcomes": "tasks/002-write-total/outcomes.json", "holdout": False},
            {"id": "004-read-phrase", "kind": "read", "prompt": "tasks/004-read-phrase/prompt.md", "outcomes": "tasks/004-read-phrase/outcomes.json", "holdout": False},
            {"id": "005-read-failure", "kind": "read", "prompt": "tasks/005-read-failure/prompt.md", "outcomes": "tasks/005-read-failure/outcomes.json", "holdout": False},
            {"id": "007-modify-return", "kind": "modify", "prompt": "tasks/007-modify-return/prompt.md", "outcomes": "tasks/007-modify-return/outcomes.json", "holdout": False},
            {"id": "008-modify-type", "kind": "modify", "prompt": "tasks/008-modify-type/prompt.md", "outcomes": "tasks/008-modify-type/outcomes.json", "holdout": False},
            {"id": "010-fix-trigger", "kind": "fix", "prompt": "tasks/010-fix-trigger/prompt.md", "outcomes": "tasks/010-fix-trigger/outcomes.json", "holdout": False},
            {"id": "011-fix-type", "kind": "fix", "prompt": "tasks/011-fix-type/prompt.md", "outcomes": "tasks/011-fix-type/outcomes.json", "holdout": False},
        ]
        return phrase, csharp

    def test_complete_suite_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            bench = root / "bench"
            csharp = bench / "csharp-baseline"
            csharp.mkdir(parents=True)
            (csharp / "tasks").mkdir(parents=True)
            phrase, csharp_tasks = self.full_suite()
            (bench / "suite.json").write_text(json.dumps({"version": 1, "tasks": phrase}, indent=2), encoding="utf-8")
            for item in csharp_tasks:
                task_root = csharp / "tasks" / item["id"]
                task_root.mkdir(parents=True, exist_ok=True)
                (task_root / "prompt.md").write_text(f"prompt for {item['id']}", encoding="utf-8")
                (task_root / "outcomes.json").write_text(json.dumps([{"name": "works", "given": {}, "when": "called", "then": {"returns": 1}}]), encoding="utf-8")
                if item["kind"] in {"read", "modify", "fix"}:
                    (task_root / "start.cs").write_text("namespace Demo; public static class Demo { }", encoding="utf-8")
            (csharp / "suite.json").write_text(json.dumps({"version": 1, "tasks": csharp_tasks}, indent=2), encoding="utf-8")
            issues = check.check_repo(root)
            self.assertEqual(set(), issues["missing"])
            self.assertEqual(set(), issues["extra"])
            self.assertEqual(set(), issues["holdout"])

    def test_missing_twin_fails_and_names_it(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            bench = root / "bench"
            csharp = bench / "csharp-baseline"
            csharp.mkdir(parents=True)
            (csharp / "tasks").mkdir(parents=True)
            phrase, csharp_tasks = self.full_suite()
            csharp_tasks = [item for item in csharp_tasks if item["id"] != "010-fix-trigger"]
            (bench / "suite.json").write_text(json.dumps({"version": 1, "tasks": phrase}, indent=2), encoding="utf-8")
            for item in csharp_tasks:
                task_root = csharp / "tasks" / item["id"]
                task_root.mkdir(parents=True, exist_ok=True)
                (task_root / "prompt.md").write_text(f"prompt for {item['id']}", encoding="utf-8")
                (task_root / "outcomes.json").write_text(json.dumps([{"name": "works", "given": {}, "when": "called", "then": {"returns": 1}}]), encoding="utf-8")
                if item["kind"] in {"read", "modify", "fix"}:
                    (task_root / "start.cs").write_text("namespace Demo; public static class Demo { }", encoding="utf-8")
            (csharp / "suite.json").write_text(json.dumps({"version": 1, "tasks": csharp_tasks}, indent=2), encoding="utf-8")
            issues = check.check_repo(root)
            self.assertIn("010-fix-trigger", issues["missing"])

    def test_extra_twin_fails_and_names_it(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            bench = root / "bench"
            csharp = bench / "csharp-baseline"
            csharp.mkdir(parents=True)
            (csharp / "tasks").mkdir(parents=True)
            phrase, csharp_tasks = self.full_suite()
            csharp_tasks = csharp_tasks + [{"id": "013-extra", "kind": "write", "prompt": "tasks/013-extra/prompt.md", "outcomes": "tasks/013-extra/outcomes.json", "holdout": False}]
            (bench / "suite.json").write_text(json.dumps({"version": 1, "tasks": phrase}, indent=2), encoding="utf-8")
            for item in csharp_tasks:
                task_root = csharp / "tasks" / item["id"]
                task_root.mkdir(parents=True, exist_ok=True)
                (task_root / "prompt.md").write_text(f"prompt for {item['id']}", encoding="utf-8")
                (task_root / "outcomes.json").write_text(json.dumps([{"name": "works", "given": {}, "when": "called", "then": {"returns": 1}}]), encoding="utf-8")
            (csharp / "suite.json").write_text(json.dumps({"version": 1, "tasks": csharp_tasks}, indent=2), encoding="utf-8")
            issues = check.check_repo(root)
            self.assertIn("013-extra", issues["extra"])

    def test_holdout_twin_present_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            bench = root / "bench"
            csharp = bench / "csharp-baseline"
            csharp.mkdir(parents=True)
            (csharp / "tasks").mkdir(parents=True)
            phrase = [self.build_task("003-write-endpoint", "write", holdout=True)] + [
                self.build_task("001-write-greeting", "write"),
                self.build_task("002-write-total", "write"),
                self.build_task("004-read-phrase", "read"),
                self.build_task("005-read-failure", "read"),
                self.build_task("007-modify-return", "modify"),
                self.build_task("008-modify-type", "modify"),
                self.build_task("010-fix-trigger", "fix"),
                self.build_task("011-fix-type", "fix"),
            ]
            csharp_tasks = [
                {"id": "003-write-endpoint", "kind": "write", "prompt": "tasks/003-write-endpoint/prompt.md", "outcomes": "tasks/003-write-endpoint/outcomes.json", "holdout": True},
                {"id": "001-write-greeting", "kind": "write", "prompt": "tasks/001-write-greeting/prompt.md", "outcomes": "tasks/001-write-greeting/outcomes.json", "holdout": False},
                {"id": "002-write-total", "kind": "write", "prompt": "tasks/002-write-total/prompt.md", "outcomes": "tasks/002-write-total/outcomes.json", "holdout": False},
                {"id": "004-read-phrase", "kind": "read", "prompt": "tasks/004-read-phrase/prompt.md", "outcomes": "tasks/004-read-phrase/outcomes.json", "holdout": False},
                {"id": "005-read-failure", "kind": "read", "prompt": "tasks/005-read-failure/prompt.md", "outcomes": "tasks/005-read-failure/outcomes.json", "holdout": False},
                {"id": "007-modify-return", "kind": "modify", "prompt": "tasks/007-modify-return/prompt.md", "outcomes": "tasks/007-modify-return/outcomes.json", "holdout": False},
                {"id": "008-modify-type", "kind": "modify", "prompt": "tasks/008-modify-type/prompt.md", "outcomes": "tasks/008-modify-type/outcomes.json", "holdout": False},
                {"id": "010-fix-trigger", "kind": "fix", "prompt": "tasks/010-fix-trigger/prompt.md", "outcomes": "tasks/010-fix-trigger/outcomes.json", "holdout": False},
                {"id": "011-fix-type", "kind": "fix", "prompt": "tasks/011-fix-type/prompt.md", "outcomes": "tasks/011-fix-type/outcomes.json", "holdout": False},
            ]
            (bench / "suite.json").write_text(json.dumps({"version": 1, "tasks": phrase}, indent=2), encoding="utf-8")
            for item in csharp_tasks:
                task_root = csharp / "tasks" / item["id"]
                task_root.mkdir(parents=True, exist_ok=True)
                (task_root / "prompt.md").write_text(f"prompt for {item['id']}", encoding="utf-8")
                (task_root / "outcomes.json").write_text(json.dumps([{"name": "works", "given": {}, "when": "called", "then": {"returns": 1}}]), encoding="utf-8")
                if item["kind"] in {"read", "modify", "fix"}:
                    (task_root / "start.cs").write_text("namespace Demo; public static class Demo { }", encoding="utf-8")
            (csharp / "suite.json").write_text(json.dumps({"version": 1, "tasks": csharp_tasks}, indent=2), encoding="utf-8")
            issues = check.check_repo(root)
            self.assertIn("003-write-endpoint", issues["holdout"])


if __name__ == "__main__":
    unittest.main()
