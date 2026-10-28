import importlib.util
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location(
    "check_phrase_executed_complete",
    ROOT / "scripts" / "check-phrase-executed-complete.py",
)
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


class PhraseExecutedCompleteTests(unittest.TestCase):
    def build_repo(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            phrase_root = root / "bench" / "tasks"
            csharp_root = root / "bench" / "csharp-baseline" / "tasks"
            phrase_root.mkdir(parents=True, exist_ok=True)
            csharp_root.mkdir(parents=True, exist_ok=True)
            for task_id in check.TASK_IDS:
                (phrase_root / task_id).mkdir(parents=True, exist_ok=True)
                (csharp_root / task_id).mkdir(parents=True, exist_ok=True)
                payload = json_text(task_id)
                (phrase_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")
                (csharp_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")
            yield root

    def test_complete_suite_passes(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            phrase_root = root / "bench" / "tasks"
            csharp_root = root / "bench" / "csharp-baseline" / "tasks"
            phrase_root.mkdir(parents=True, exist_ok=True)
            csharp_root.mkdir(parents=True, exist_ok=True)
            for task_id in check.TASK_IDS:
                (phrase_root / task_id).mkdir(parents=True, exist_ok=True)
                (csharp_root / task_id).mkdir(parents=True, exist_ok=True)
                payload = json_text(task_id)
                (phrase_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")
                (csharp_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")
            issues = check.check_repo(root)
            self.assertEqual([], issues["missing"])
            self.assertEqual([], issues["drifted"])

    def test_missing_copy_fails_and_names_task(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            phrase_root = root / "bench" / "tasks"
            csharp_root = root / "bench" / "csharp-baseline" / "tasks"
            phrase_root.mkdir(parents=True, exist_ok=True)
            csharp_root.mkdir(parents=True, exist_ok=True)
            for task_id in check.TASK_IDS:
                if task_id == "010-fix-trigger":
                    continue
                (phrase_root / task_id).mkdir(parents=True, exist_ok=True)
                (csharp_root / task_id).mkdir(parents=True, exist_ok=True)
                payload = json_text(task_id)
                (phrase_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")
                (csharp_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")
            issues = check.check_repo(root)
            self.assertIn("010-fix-trigger", issues["missing"])

    def test_drifted_copy_fails_and_names_task(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            phrase_root = root / "bench" / "tasks"
            csharp_root = root / "bench" / "csharp-baseline" / "tasks"
            phrase_root.mkdir(parents=True, exist_ok=True)
            csharp_root.mkdir(parents=True, exist_ok=True)
            for task_id in check.TASK_IDS:
                (phrase_root / task_id).mkdir(parents=True, exist_ok=True)
                (csharp_root / task_id).mkdir(parents=True, exist_ok=True)
                payload = json_text(task_id)
                (phrase_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")
                (csharp_root / task_id / "outcomes.json").write_text(payload.replace('"returns": 1', '"returns": 2'), encoding="utf-8")
            issues = check.check_repo(root)
            self.assertIn("001-write-greeting", issues["drifted"])

    def test_endpoint_responds_with_status_copy_passes_and_drift_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            phrase_root = root / "bench" / "tasks"
            csharp_root = root / "bench" / "csharp-baseline" / "tasks"
            for task_id in check.TASK_IDS:
                (phrase_root / task_id).mkdir(parents=True, exist_ok=True)
                (csharp_root / task_id).mkdir(parents=True, exist_ok=True)
                payload = endpoint_json() if task_id == "020-endpoint-greetings" else json_text(task_id)
                (phrase_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")
                (csharp_root / task_id / "outcomes.json").write_text(payload, encoding="utf-8")

            self.assertEqual([], check.check_repo(root)["drifted"])
            (phrase_root / "020-endpoint-greetings" / "outcomes.json").write_text(
                endpoint_json().replace('"status": 200', '"status": 201'), encoding="utf-8"
            )
            self.assertIn("020-endpoint-greetings", check.check_repo(root)["drifted"])


def json_text(task_id):
    if task_id in {"010-fix-trigger", "011-fix-type"}:
        value = 1 if task_id.endswith("trigger") else 42
        return (
            '[{"name": "returns ' + ("hello" if task_id.endswith("trigger") else "forty two") + '", "given": {}, '
            '"when": "called", "then": {"returns": ' + str(value) + '}}]'
        )
    return '[{"name": "works", "given": {}, "when": "called", "then": {"returns": 1}}]'


def endpoint_json():
    return '[{"name": "greets Ada", "given": {"name": "Ada"}, "when": "called", "then": {"responds": {"message": "hello", "name": "Ada"}, "status": 200}}, {"name": "rejects blank name", "given": {"name": ""}, "when": "called", "then": {"fails_with": "MissingName"}}]'


if __name__ == "__main__":
    unittest.main()
