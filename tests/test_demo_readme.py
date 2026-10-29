"""The Tasks demo README cannot drift from demo/tasks/Tasks.avowry."""

from __future__ import annotations

import importlib.util
import json
import re
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
README = ROOT / "demo" / "tasks" / "README.md"
PHRASE = ROOT / "demo" / "tasks" / "Tasks.avowry"

FENCE = re.compile(r"```(?:[^\n`]*)\n(.*?)```", re.DOTALL)
MODULE = re.compile(r"^module\s+(\S+)", re.M)
ENDPOINT = re.compile(
    r"^endpoint\s+\S+\s*:\s*(GET|POST|PUT|PATCH|DELETE)\s+(\S+)", re.M
)
FAILS = re.compile(r"(?:fails with|then fails with)\s+([A-Za-z0-9_, ]+)")
PATH_TOKEN = re.compile(
    r"\b(?:src|demo|docs|tests|scripts|board)/[A-Za-z0-9_./-]+\b"
)
PROJECT = re.compile(r"\b([A-Za-z][A-Za-z0-9_]*)\.(Api|Specs)\b")
CURL_VERB = re.compile(r"-X\s+(GET|POST|PUT|PATCH|DELETE)\b", re.I)
CURL_URL = re.compile(r"""https?://[^/\s'"]+(/[^?\s'"]*)(\?[^ \t'"]*)?""")
CURL_BODY = re.compile(r"""-d\s+'([^']*)'|-d\s+"([^"]*)\"""")
STATUS = re.compile(r"\bHTTP\s+(200|422)\b|\b(200|422)\s+(?:with|\{)")
WRONG_STATUS = re.compile(r"\bHTTP\s+(?!200\b|422\b)([1-5]\d{2})\b")
FAILURE_MENTION = re.compile(
    r"422\s+([A-Z][A-Za-z0-9]+)|fails with\s+([A-Z][A-Za-z0-9]+)"
)
COMPILE = re.compile(
    r"dotnet\s+run\s+--project\s+src/Phrase\.Cli\s+--\s+build\s+"
    r"demo/tasks/Tasks\.avowry\s+-o\s+(\S+)"
)
BUILD_API = re.compile(r"dotnet\s+build\s+\S*Tasks\.Api")
TEST_SPECS = re.compile(r"dotnet\s+test\s+\S*Tasks\.Specs")
RUN_API = re.compile(
    r"dotnet\s+run\s+--project\s+\S*Tasks\.Api\b[^\n]*--urls\s+"
    r"http://localhost:5099"
)


def unfold(block: str) -> str:
    return re.sub(r"\\\n[ \t]*", " ", block.replace("\r\n", "\n"))


def fenced_blocks(text: str) -> list[str]:
    return [unfold(block) for block in FENCE.findall(text.replace("\r\n", "\n"))]


def phrase_surface(source: str) -> tuple[str, list[dict], set[str]]:
    match = MODULE.search(source)
    if match is None:
        raise ValueError("Tasks.avowry has no module line")
    module = match.group(1)
    endpoints = []
    for found in ENDPOINT.finditer(source):
        rest = source[found.end() :]
        fields = []
        input_block = re.search(r"\n  input\n((?:    .*\n)+)", rest)
        if input_block:
            for line in input_block.group(1).splitlines():
                field = re.match(r"    ([A-Za-z][A-Za-z0-9_]*)\s*:", line)
                if field:
                    fields.append(field.group(1))
        endpoints.append(
            {"verb": found.group(1), "route": found.group(2), "fields": fields}
        )
    failures: set[str] = set()
    for found in FAILS.finditer(source):
        for name in re.split(r"[,\s]+", found.group(1).strip()):
            if name:
                failures.add(name)
    return module, endpoints, failures


def route_matches(template: str, path: str) -> bool:
    wanted = template.strip("/").split("/")
    got = path.split("?", 1)[0].strip("/").split("/")
    if len(wanted) != len(got):
        return False
    for part, actual in zip(wanted, got):
        if part.startswith("{") and part.endswith("}"):
            continue
        if part != actual:
            return False
    return True


def parse_curls(blocks: list[str]) -> list[dict]:
    calls = []
    for block in blocks:
        if not re.search(r"\bcurl\b", block):
            continue
        for line in block.splitlines():
            stripped = line.strip()
            if not stripped.startswith("curl"):
                continue
            verb_match = CURL_VERB.search(stripped)
            verb = verb_match.group(1).upper() if verb_match else "GET"
            url_match = CURL_URL.search(stripped)
            if url_match is None:
                continue
            path = url_match.group(1)
            query = (url_match.group(2) or "").lstrip("?")
            fields = []
            if query:
                for item in query.split("&"):
                    name = item.split("=", 1)[0]
                    if name:
                        fields.append(name)
            body_match = CURL_BODY.search(stripped)
            if body_match:
                raw = body_match.group(1) or body_match.group(2) or ""
                try:
                    payload = json.loads(raw)
                except json.JSONDecodeError:
                    payload = None
                if isinstance(payload, dict):
                    fields.extend(str(key) for key in payload)
            calls.append({"verb": verb, "path": path, "fields": fields, "raw": stripped})
    return calls


def outside_or_ignored(output: str) -> bool:
    posix = output.strip("\"'").replace("\\", "/")
    if posix.startswith("/tmp") or posix.startswith("~/") or posix.startswith("$"):
        return True
    if posix.startswith("../"):
        return True
    return False


def check_readme(readme: str, source: str, root: Path) -> list[str]:
    problems: list[str] = []
    module, endpoints, failures = phrase_surface(source)
    blocks = fenced_blocks(readme)
    joined = "\n".join(blocks)
    whole = readme.replace("\r\n", "\n")

    if not blocks:
        problems.append("README has no fenced commands")
        return problems

    compile_match = COMPILE.search(joined)
    if compile_match is None:
        problems.append("missing compile command: dotnet run --project src/Phrase.Cli -- build demo/tasks/Tasks.avowry -o <out>")
    else:
        output = compile_match.group(1)
        if not outside_or_ignored(output):
            problems.append(f"output folder {output} must be outside the repo or git-ignored")
    if BUILD_API.search(joined) is None:
        problems.append("missing dotnet build of Tasks.Api")
    if TEST_SPECS.search(joined) is None:
        problems.append("missing dotnet test of Tasks.Specs")
    if RUN_API.search(joined) is None:
        problems.append("missing dotnet run of Tasks.Api on http://localhost:5099")

    for token in PATH_TOKEN.findall(whole):
        path = root / token
        if not path.exists():
            problems.append(f"path {token} does not exist")

    for name, kind in PROJECT.findall(joined):
        if name != module:
            problems.append(f"project {name}.{kind} does not match module {module}")

    if f"{module}.Api" not in joined or f"{module}.Specs" not in joined:
        problems.append(f"fenced commands must name {module}.Api and {module}.Specs")

    calls = parse_curls(blocks)
    if not calls:
        problems.append("fenced commands include no curl calls")

    matched: set[tuple[str, str]] = set()
    mentioned_fields: set[str] = set()
    allowed_fields = {field for endpoint in endpoints for field in endpoint["fields"]}
    for call in calls:
        hits = [
            endpoint
            for endpoint in endpoints
            if endpoint["verb"] == call["verb"] and route_matches(endpoint["route"], call["path"])
        ]
        if not hits:
            problems.append(f"no endpoint for {call['verb']} {call['path']}")
            continue
        matched.add((hits[0]["verb"], hits[0]["route"]))
        for field in call["fields"]:
            mentioned_fields.add(field)
            if field not in allowed_fields:
                problems.append(f"request field {field} is not an endpoint input in Tasks.avowry")

    missing_endpoints = {(endpoint["verb"], endpoint["route"]) for endpoint in endpoints} - matched
    for verb, route in sorted(missing_endpoints):
        problems.append(f"README never calls {verb} {route}")

    required_fields = {"id", "title", "available"}
    for field in sorted(required_fields - mentioned_fields):
        if field in allowed_fields:
            problems.append(f"README never sends request field {field}")

    mentioned_verbs = {call["verb"] for call in calls}
    allowed_verbs = {endpoint["verb"] for endpoint in endpoints}
    for verb in sorted(mentioned_verbs - allowed_verbs):
        problems.append(f"HTTP verb {verb} is not used by Tasks.avowry")
    for verb in sorted(allowed_verbs - mentioned_verbs):
        problems.append(f"README never uses HTTP verb {verb}")

    statuses = set()
    for found in STATUS.finditer(whole):
        statuses.add(found.group(1) or found.group(2))
    wrong = WRONG_STATUS.findall(whole)
    for code in wrong:
        problems.append(f"unexpected HTTP status {code}")
    if "200" not in statuses:
        problems.append("README never states expected status 200")
    if "422" not in statuses:
        problems.append("README never states expected status 422")

    mentioned_failures = {a or b for a, b in FAILURE_MENTION.findall(whole)}
    for name in sorted(mentioned_failures - failures):
        problems.append(f"failure {name} is not declared in Tasks.avowry")
    for name in sorted(failures):
        if name not in whole:
            problems.append(f"README never names declared failure {name}")

    spec_count = len(re.findall(r'(?m)^\s*spec\s+"', source))
    if f"Passed: {spec_count}" not in whole:
        problems.append(f"README must state Passed: {spec_count} (the number of specs in Tasks.avowry)")

    served = re.findall(r"--urls\s+http://[^\s:]+:(\d+)", whole)
    called = {m for call in calls for m in re.findall(r"://[^\s/:]+:(\d+)", call["raw"])}
    if not served:
        problems.append("README never says which port the API is served on (--urls)")
    elif called - set(served):
        problems.append(f"curl calls use port(s) {sorted(called - set(served))}, the API is served on {served[0]}")

    return problems


def load_report():
    spec = importlib.util.spec_from_file_location(
        "usable_v1_report", ROOT / "scripts" / "usable-v1-report.py"
    )
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class DemoReadmeTests(unittest.TestCase):
    def test_fenced_commands_match_tasks_phrase(self):
        # Given the Tasks demo README and the Phrase source it describes
        self.assertTrue(README.is_file(), "demo/tasks/README.md is missing")
        self.assertTrue(PHRASE.is_file(), "demo/tasks/Tasks.avowry is missing")
        readme = README.read_text(encoding="utf-8")
        source = PHRASE.read_text(encoding="utf-8")

        # When the README's fenced commands are parsed
        problems = check_readme(readme, source, ROOT)

        # Then every path, project name, route, verb, field and status exists in Tasks.avowry
        self.assertEqual(problems, [])

    def test_invented_route_is_rejected(self):
        source = PHRASE.read_text(encoding="utf-8")
        fake = """# Demo
```bash
dotnet run --project src/Phrase.Cli -- build demo/tasks/Tasks.avowry -o /tmp/phrase-tasks
dotnet build /tmp/phrase-tasks/Tasks.Api
dotnet test /tmp/phrase-tasks/Tasks.Specs
dotnet run --project /tmp/phrase-tasks/Tasks.Api --urls http://localhost:5099
curl -X POST http://localhost:5099/invented
```
Expected HTTP 200 with {} and HTTP 422 EmptyTitle.
"""
        problems = check_readme(fake, source, ROOT)
        self.assertTrue(any("invented" in problem for problem in problems))

    def test_invented_field_is_rejected(self):
        source = PHRASE.read_text(encoding="utf-8")
        fake = """# Demo
```bash
dotnet run --project src/Phrase.Cli -- build demo/tasks/Tasks.avowry -o /tmp/phrase-tasks
dotnet build /tmp/phrase-tasks/Tasks.Api
dotnet test /tmp/phrase-tasks/Tasks.Specs
dotnet run --project /tmp/phrase-tasks/Tasks.Api --urls http://localhost:5099
curl -X POST http://localhost:5099/tasks -d '{"id":"task-1","title":"Write the demo","priority":"high"}'
curl http://localhost:5099/tasks?available=true
curl -X PUT http://localhost:5099/tasks/task-1/complete
curl -X GET http://localhost:5099/tasks/nope
curl -X DELETE http://localhost:5099/tasks/task-1
```
Expected HTTP 200 with {} and HTTP 422 EmptyTitle, 422 NotFound, 422 StoreUnavailable.
"""
        problems = check_readme(fake, source, ROOT)
        self.assertTrue(any("priority" in problem for problem in problems))

    def test_spec_count_failure_names_and_port_must_match_the_source(self):
        source = PHRASE.read_text(encoding="utf-8")
        readme = README.read_text(encoding="utf-8")
        self.assertEqual([], check_readme(readme, source, ROOT))
        extra_spec = source + '  spec "one more"\n'
        self.assertTrue(any("Passed:" in problem for problem in check_readme(readme, extra_spec, ROOT)))
        renamed = source.replace("TitleTooLong", "TitleTooLongX")
        self.assertTrue(any("TitleTooLongX" in problem for problem in check_readme(readme, renamed, ROOT)))
        wrong_port = readme.replace("localhost:5099/tasks/nope", "localhost:5100/tasks/nope")
        self.assertTrue(any("5100" in problem for problem in check_readme(wrong_port, source, ROOT)))

    def test_output_folder_inside_the_repo_is_rejected(self):
        source = PHRASE.read_text(encoding="utf-8")
        fake = """# Demo
```bash
dotnet run --project src/Phrase.Cli -- build demo/tasks/Tasks.avowry -o demo/tasks/out
```
"""
        problems = check_readme(fake, source, ROOT)
        self.assertTrue(any("outside" in problem for problem in problems))

    def test_usable_v1_report_points_at_readme_only_when_it_exists(self):
        # Given the Usable v1 report's milestone line
        report = load_report()
        self.assertEqual(report.self_test(), 0)

        # When the Tasks README is present
        self.assertTrue(README.is_file(), "demo/tasks/README.md is missing")
        self.assertIn("demo/tasks/README.md", report.reached_line())

        # Then a missing README is omitted from the line
        with tempfile.TemporaryDirectory() as tmp:
            empty = Path(tmp)
            self.assertEqual(
                report.reached_line(empty),
                "USABLE v1 REACHED: demo app at demo/tasks",
            )
            (empty / "demo" / "tasks").mkdir(parents=True)
            (empty / "demo" / "tasks" / "README.md").write_text("# demo\n", encoding="utf-8")
            self.assertIn("demo/tasks/README.md", report.reached_line(empty))


if __name__ == "__main__":
    unittest.main()
