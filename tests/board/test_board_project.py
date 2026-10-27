import io
import json
import unittest
from contextlib import redirect_stdout

from scripts.board import check_real_board, project


class FakeGH:
    def __init__(self):
        self.calls = []
        self.projects = []
        self.project_id = "PVT_123"
        self.project_fields = {}

    def _project_by_number(self, number):
        for project in self.projects:
            if str(project.get("number")) == str(number):
                return project
        return None

    def _field_list_for_project(self, project_id):
        fields = self.project_fields.get(project_id, [])
        if not fields:
            fields = [
                {"id": "status-id", "name": "Status", "type": "SINGLE_SELECT", "options": [{"id": "st1", "name": "Todo"}, {"id": "st2", "name": "In Progress"}, {"id": "st3", "name": "Done"}]},
                {"id": "rank-id", "name": "Rank", "type": "NUMBER", "options": []},
            ]
            self.project_fields[project_id] = fields
        return fields

    def __call__(self, args, *, check=True, input_text=None):
        cmd = list(args)
        self.calls.append(cmd)

        if cmd[:2] == ["project", "list"]:
            return json.dumps({"projects": self.projects, "totalCount": len(self.projects)})

        if cmd[:2] == ["project", "create"]:
            title = cmd[cmd.index("--title") + 1]
            item = {"id": self.project_id, "number": len(self.projects) + 1, "title": title, "name": title, "public": False, "visibility": "PRIVATE"}
            self.projects.append(item)
            self.project_fields.setdefault(item["id"], self._field_list_for_project(item["id"]))
            return json.dumps(item)

        if cmd[:2] == ["project", "edit"]:
            number = int(cmd[2])
            project = self._project_by_number(number)
            project["public"] = False
            project["visibility"] = "PRIVATE"
            return json.dumps(project)

        if cmd[:2] == ["project", "field-list"]:
            project_number = int(cmd[2])
            project = self._project_by_number(project_number)
            if project is None:
                raise RuntimeError(f"Project {project_number} not found")
            return json.dumps({"fields": self._field_list_for_project(project["id"]), "totalCount": len(self._field_list_for_project(project["id"]))})

        if cmd[:2] == ["project", "field-create"]:
            name = cmd[cmd.index("--name") + 1]
            data_type = cmd[cmd.index("--data-type") + 1]
            project_number = int(cmd[2])
            project = self._project_by_number(project_number)
            options = []
            if "--single-select-options" in cmd:
                opt_index = cmd.index("--single-select-options")
                options = [{"id": f"opt-{idx}", "name": val.strip()} for idx, val in enumerate(cmd[opt_index + 1].split(","), start=1) if val.strip()]
            field = {"id": f"{name.lower()}-id", "name": name, "type": data_type, "options": options}
            self.project_fields.setdefault(project["id"], [])
            self.project_fields[project["id"]].append(field)
            return json.dumps({"field": field})

        if cmd[:2] == ["api", "graphql"]:
            payload = input_text or ""
            if not payload and cmd:
                for i, part in enumerate(cmd):
                    if i > 0 and cmd[i - 1] == "-f" and part.startswith("query="):
                        payload = part.split("=", 1)[1]
                        break
            if "updateProjectV2Field" in payload:
                data = json.loads(payload)
                vars_ = data["variables"]
                field_id = vars_["fieldId"]
                options = vars_["singleSelectOptions"]
                for project in self.projects:
                    for field in self.project_fields.get(project["id"], []):
                        if field.get("id") == field_id:
                            field["options"] = [{"id": option.get("id") or f"opt-{idx}", "name": option["name"], "color": option.get("color", "GRAY"), "description": option.get("description", "")} for idx, option in enumerate(options, start=1)]
                            return json.dumps({"data": {"updateProjectV2Field": {"projectV2Field": {"name": "Status"}}}})
                return json.dumps({"data": {"updateProjectV2Field": {"projectV2Field": {"name": "Status"}}}})
            return json.dumps({"data": {}})

        raise AssertionError(f"Unhandled fake gh call: {cmd}")


class BoardProjectTests(unittest.TestCase):
    def test_default_is_dry_run_and_no_mutating_calls(self):
        fake = FakeGH()
        with redirect_stdout(io.StringIO()):
            project.main([], gh_runner=fake)
        self.assertFalse(any(call[:2] in (["project", "create"], ["project", "edit"], ["project", "field-create"]) for call in fake.calls))
        self.assertFalse(any("delete" in str(arg).lower() for call in fake.calls for arg in call))

    def test_dry_run_prints_same_plan_without_mutating(self):
        fake = FakeGH()
        first = io.StringIO()
        with redirect_stdout(first):
            project.main(["--dry-run"], gh_runner=fake)
        second = io.StringIO()
        with redirect_stdout(second):
            project.main(["--dry-run"], gh_runner=fake)

        self.assertEqual(first.getvalue(), second.getvalue())
        self.assertFalse(any("delete" in str(arg).lower() for call in fake.calls for arg in call))
        self.assertFalse(any(call[:2] in (["project", "create"], ["project", "edit"], ["project", "field-create"]) for call in fake.calls))

    def test_apply_creates_project_once_and_status_has_wanted_options(self):
        fake = FakeGH()

        first = io.StringIO()
        with redirect_stdout(first):
            project.main(["--apply"], gh_runner=fake)

        second = io.StringIO()
        with redirect_stdout(second):
            project.main(["--apply"], gh_runner=fake)

        project_calls = [call for call in fake.calls if call[:2] == ["project", "create"]]
        self.assertEqual(len(project_calls), 1)
        self.assertFalse(any("delete" in str(arg).lower() for call in fake.calls for arg in call))
        self.assertIn("Phrase Board", first.getvalue())
        self.assertIn("Status", first.getvalue())
        self.assertIn("Ready", first.getvalue())
        self.assertIn("Programming-Made-EZ", first.getvalue())
        self.assertIn("Phrase Board", second.getvalue())

        project_number = fake.projects[0]["number"]
        fields = fake.project_fields[fake.projects[0]["id"]]
        status_field = next(field for field in fields if field.get("name") == "Status")
        self.assertEqual([opt["name"] for opt in status_field["options"]], ["Ready", "In Progress", "Done", "Superseded/Withdrawn"])

    def test_rejects_non_phrase_owner_in_main_and_ensure_owner(self):
        fake = FakeGH()
        with self.assertRaises(ValueError):
            project.ensure_project(fake, owner="some-other-owner")
        with self.assertRaises(ValueError):
            project.main(["--dry-run", "--owner", "some-other-owner"], gh_runner=fake)

    def test_find_project_handles_real_gh_project_list_shape(self):
        fake = FakeGH()
        fake.projects = [{"id": "PVT_77", "number": 77, "title": "Phrase Board", "public": False, "closed": False}]
        found = project.find_project(fake)
        self.assertIsNotNone(found)
        self.assertEqual(found["id"], "PVT_77")

    def test_status_update_keeps_existing_ids_when_names_match(self):
        fake = FakeGH()
        project_id = "PVT_1"
        fake.projects = [{"id": project_id, "number": 1, "title": "Phrase Board", "public": False, "visibility": "PRIVATE"}]
        fake.project_fields[project_id] = [{
            "id": "status-id",
            "name": "Status",
            "type": "SINGLE_SELECT",
            "options": [{"id": "todo-id", "name": "Todo"}, {"id": "done-id", "name": "Done"}, {"id": "in-id", "name": "In Progress"}],
        }]
        project.ensure_project(fake)
        status = next(field for field in fake.project_fields[project_id] if field["name"] == "Status")
        self.assertEqual([option["name"] for option in status["options"]], ["Ready", "In Progress", "Done", "Superseded/Withdrawn"])
        self.assertEqual({option["id"] for option in status["options"] if option["name"] in {"In Progress", "Done"}}, {"in-id", "done-id"})

    def test_private_visibility_is_enforced(self):
        fake = FakeGH()
        fake.projects = [{"id": "PVT_2", "number": 2, "title": "Phrase Board", "public": True, "visibility": "PUBLIC"}]
        project.ensure_project(fake)
        self.assertFalse(fake.projects[0]["public"])
        self.assertTrue(any(call[:2] == ["project", "edit"] for call in fake.calls))

        with self.assertRaises(RuntimeError):
            check_real_board.verify_project(lambda args: json.dumps({"projects": [{"id": "PVT_2", "number": 2, "title": "Phrase Board", "public": True, "visibility": "PUBLIC"}], "totalCount": 1}))


if __name__ == "__main__":
    unittest.main()
