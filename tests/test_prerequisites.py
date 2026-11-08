"""Card 178 (D157 Criteria 1 and 2): the prerequisites page matches what CI tests.

Named test: test_prerequisites_page_matches_what_CI_uses.
Live-state guard: reads the working tree on purpose. Standard library only.
"""

from __future__ import annotations

import re
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
PAGE_PATH = REPO / "docs" / "PREREQUISITES.md"
HEADER = (
    "docs/PREREQUISITES.md must match the CI workflows; if a workflow gains "
    "runs-on, a matrix or a pin (for example card 177 adding a Windows job), "
    "update docs/PREREQUISITES.md in the same change.\n"
)
JOB_HEADER = re.compile(r"^  (\S+):\s*$")
MATRIX_EXPR = re.compile(r"^\$\{\{\s*matrix\.(os|dotnet-version|runs-on)\s*\}\}$")
ARCH = re.compile(r"(?i)(?:^|[-_/])(arm64|aarch64|arm32|amd64|x64|x86)(?:$|[-_/])")
SDK_TOKEN = re.compile(r"\b\d+\.\d+\.(?:x|\d{3,})\b")
OS_TOKEN = re.compile(r"\b(?:ubuntu|windows|macos)-[\w.]+\b")
FENCE = re.compile(r"^```([^\n`]*)\n(.*?)```", re.DOTALL | re.MULTILINE)
HEADING = re.compile(r"(?m)^#{1,6} .+$")
BASH = {"bash", "sh", "shell"}
PWSH = {"powershell", "pwsh"}
MATRIX_KEYS = ("os", "dotnet-version", "runs-on")
LOCAL_FEED = (
    "dotnet tool install Avowry",
    "--add-source",
    "local feed",
    "not published",
    "public registry",
)
COMMAND_NEEDLES = (
    "dotnet --list-sdks",
    "dotnet pack",
    "dotnet tool install Avowry",
)
LABELS_ONLY = "CI runs on `ubuntu-latest` with .NET SDK `8.0.x`.\n"
MINIMAL_PAGE = """# Prerequisites

CI runs on `ubuntu-latest` with .NET SDK `8.0.x`.

CPU architecture is unpinned; the workflows name none.

Any other operating system or CPU architecture is untested. If you try anyway, you need the .NET 8 SDK.

v0.1 is .NET-first: the emitter writes C# for net8.0, so building and running the output needs the .NET 8 SDK. A newer ASP.NET Core runtime alone is not enough.

The CLI is installed from a local feed. It is not published to a public registry.

## Check the SDK

Run from: the repository root
Needs: the .NET 8 SDK

```bash
dotnet --list-sdks
```

Same in PowerShell.

## Pack

Run from: the repository root
Needs: the .NET 8 SDK

```bash
dotnet pack src/Avowry.Cli/Avowry.Cli.csproj -c Release -o feed
```

Same in PowerShell.

## Install

Run from: the repository root
Needs: a packed local feed

```bash
dotnet tool install Avowry --tool-path tools --add-source feed
```

Same in PowerShell.
"""
SYNTHETIC = """
jobs:
  build:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest]
        dotnet-version:
          - 8.0.x
          - 9.0.x
        include:
          - os: macos-latest
            dotnet-version: 8.0.412
    steps:
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ matrix.dotnet-version }}
  listed:
    runs-on: [ubuntu-22.04, windows-2022]
    steps:
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            8.0.100
            10.0.x
"""


def normalize(text):
    return text.replace("\r\n", "\n").replace("\r", "\n")


def leading(line):
    return len(line) - len(line.lstrip(" "))


def strip_value(value):
    value = value.split("#", 1)[0].strip()
    if len(value) >= 2 and value[0] == value[-1] and value[0] in "\"'":
        value = value[1:-1].strip()
    return value


def parse_scalar_or_list(value):
    value = strip_value(value)
    if value.startswith("[") and value.endswith("]"):
        return [item for part in value[1:-1].split(",") if (item := strip_value(part))]
    return [value] if value else []


def unique(items):
    seen = []
    for item in items:
        if item not in seen:
            seen.append(item)
    return seen


def has_token(text, value):
    return re.search(r"(?<![\w.])" + re.escape(value) + r"(?![\w.])", text) is not None


def paragraphs(text):
    out = []
    for chunk in re.split(r"\n\s*\n", normalize(text)):
        collapsed = re.sub(r"[ \t]+", " ", chunk.strip()).replace("\n", " ")
        if collapsed:
            out.append(collapsed)
    return out


def sentences(text):
    result = []
    for para in paragraphs(text):
        result.extend(p.strip() for p in re.split(r"(?<=[.!?])\s+", para) if p.strip())
    return result


def split_jobs(text):
    lines = normalize(text).split("\n")
    start = next((i + 1 for i, line in enumerate(lines) if line.startswith("jobs:")), None)
    if start is None:
        return {}
    jobs, name, body = {}, None, []
    for line in lines[start:]:
        header = JOB_HEADER.match(line)
        if header:
            if name is not None:
                jobs[name] = body
            name, body = header.group(1), []
            continue
        if name is not None and line.strip() and leading(line) < 2:
            jobs[name] = body
            name, body = None, []
            continue
        if name is not None:
            body.append(line)
    if name is not None:
        jobs[name] = body
    return jobs


def take_block(lines, index, indent):
    values = []
    index += 1
    while index < len(lines) and (not lines[index].strip() or leading(lines[index]) > indent):
        if lines[index].strip():
            values.append(strip_value(lines[index].strip()))
        index += 1
    return values, index


def take_dash_list(lines, index, indent):
    values = []
    while index < len(lines):
        line = lines[index]
        if not line.strip():
            index += 1
            continue
        match = re.match(r"^\s+-\s+(.+)$", line)
        if match and leading(line) > indent:
            values.extend(parse_scalar_or_list(match.group(1)))
            index += 1
            continue
        break
    return values, index


def parse_matrix(lines):
    values = {key: [] for key in MATRIX_KEYS}
    arch_key = False
    index = 0
    while index < len(lines):
        line = lines[index]
        if re.match(r"^\s+matrix:\s*$", line):
            base, index = leading(line), index + 1
            while index < len(lines):
                line = lines[index]
                if not line.strip():
                    index += 1
                    continue
                indent = leading(line)
                if indent <= base:
                    break
                stripped = line.strip()
                if stripped.startswith("include:"):
                    inc, index = indent, index + 1
                    while index < len(lines):
                        line = lines[index]
                        if not line.strip():
                            index += 1
                            continue
                        if leading(line) <= inc:
                            break
                        match = re.match(
                            r"^\s*-?\s*(os|dotnet-version|runs-on):\s*(.+)$", line
                        )
                        if match:
                            values[match.group(1)].extend(parse_scalar_or_list(match.group(2)))
                        if re.match(r"^\s*-?\s*(arch|architecture):", line):
                            arch_key = True
                        index += 1
                    continue
                match = re.match(r"^\s*(os|dotnet-version|runs-on):\s*(.*)$", line)
                if match:
                    key, rest, key_indent = match.group(1), match.group(2).strip(), indent
                    if rest == "|":
                        block, index = take_block(lines, index, key_indent)
                        values[key].extend(block)
                        continue
                    if rest:
                        values[key].extend(parse_scalar_or_list(rest))
                        index += 1
                        continue
                    block, index = take_dash_list(lines, index + 1, key_indent)
                    values[key].extend(block)
                    continue
                if re.match(r"^\s*(arch|architecture):", line):
                    arch_key = True
                index += 1
            continue
        index += 1
    return values, arch_key


def collect_key(lines, key):
    values = []
    index, pattern = 0, re.compile(r"^(\s+)" + re.escape(key) + r":\s*(.*)$")
    while index < len(lines):
        match = pattern.match(lines[index])
        if match:
            rest, indent = match.group(2).strip(), len(match.group(1))
            if rest == "|":
                block, index = take_block(lines, index, indent)
                values.extend(block)
                continue
            if rest:
                values.extend(parse_scalar_or_list(rest))
            else:
                block, index = take_dash_list(lines, index + 1, indent)
                values.extend(block)
                continue
        index += 1
    return values


def expand(values, matrix):
    expanded, leftover = [], []
    for value in values:
        expr = MATRIX_EXPR.match(value)
        if expr:
            expanded.extend(matrix.get(expr.group(1), []))
        elif "${{" in value:
            leftover.append(value)
        else:
            expanded.append(value)
    return unique(expanded), leftover


def parse_job(name, lines):
    matrix, arch_key = parse_matrix(lines)
    runs, leftover_runs = expand(collect_key(lines, "runs-on"), matrix)
    if not runs and matrix["os"]:
        runs = unique(matrix["os"])
    pins, leftover_pins = expand(collect_key(lines, "dotnet-version"), matrix)
    if not pins and matrix["dotnet-version"]:
        pins = unique(matrix["dotnet-version"])
    leftover = leftover_runs + leftover_pins
    is_dotnet = any("uses: actions/setup-dotnet" in line for line in lines)
    return {
        "name": name,
        "runs_on": runs,
        "pins": pins,
        "is_dotnet": is_dotnet,
        "leftover": leftover,
        "arch_key": arch_key,
    }


def parse_workflow_text(text):
    return [parse_job(name, body) for name, body in split_jobs(text).items()]


def parse_all_workflows():
    folder = REPO / ".github" / "workflows"
    paths = sorted(folder.glob("*.yml")) + sorted(folder.glob("*.yaml"))
    jobs = []
    for path in paths:
        jobs.extend(parse_workflow_text(path.read_text(encoding="utf-8")))
    return jobs


def read_page():
    if not PAGE_PATH.is_file():
        return None
    return PAGE_PATH.read_text(encoding="utf-8")


def fence_lang(info):
    return info.strip().split()[0].lower() if info.strip() else ""


def command_failures(text):
    failures = []
    fences = list(FENCE.finditer(text))
    bash = [(i, match) for i, match in enumerate(fences) if fence_lang(match.group(1)) in BASH]
    if not bash:
        failures.append("commands: no bash, sh or shell fenced block")
    for index, (fence_i, match) in enumerate(bash):
        start = match.start()
        prev_end = fences[fence_i - 1].end() if fence_i else 0
        before = text[prev_end:start]
        headings = list(HEADING.finditer(before))
        if headings:
            before = before[headings[-1].end() :]
        lines = before.splitlines()
        if not any(line.startswith("Run from:") for line in lines):
            failures.append(f"commands-{index}: bash block {index} has no Run from: before it")
        if not any(line.startswith("Needs:") for line in lines):
            failures.append(f"commands-{index}: bash block {index} has no Needs: before it")
        after_end = bash[index + 1][1].start() if index + 1 < len(bash) else len(text)
        after = text[match.end() : after_end]
        pwsh = False
        for later in fences[fence_i + 1 :]:
            if later.start() >= after_end:
                break
            if fence_lang(later.group(1)) in PWSH:
                pwsh = True
        same = any(line.strip() == "Same in PowerShell." for line in after.splitlines())
        block = match.group(2)
        needs_pwsh = bool(
            re.search(r"(?m)\\[ \t]*$", block)
            or "/tmp/" in block
            or "~/" in block
            or re.search(r"\$[A-Za-z_{]", block)
            or re.search(r"\bcurl\b", block)
        )
        if needs_pwsh and not pwsh:
            failures.append(f"commands-{index}: bash block {index} needs a real powershell block")
        elif not needs_pwsh and not pwsh and not same:
            failures.append(f"commands-{index}: bash block {index} has no powershell form")
    missing = [needle for needle in COMMAND_NEEDLES if needle not in text]
    if missing:
        failures.append("commands: page does not contain " + ", ".join(missing))
    return failures


def collect_failures(page, jobs):
    failures = []

    def fail(cid, message):
        failures.append(f"{cid}: {message}")

    leftover, unpinned, d_runs, d_pins, all_runs, arch_hits = [], [], [], [], [], []
    for job in jobs:
        all_runs.extend(job["runs_on"])
        leftover.extend(f"{job['name']}: {value}" for value in job["leftover"])
        if job["arch_key"]:
            arch_hits.append(job["name"] + " matrix names architecture")
        for value in job["runs_on"]:
            if ARCH.search(value):
                arch_hits.append(f"{job['name']} runs-on {value}")
        if job["is_dotnet"]:
            d_runs.extend(job["runs_on"])
            d_pins.extend(job["pins"])
            if not job["pins"]:
                unpinned.append(job["name"])
    if leftover or unpinned:
        bits = [f"leftover expression {value}" for value in leftover]
        bits += [f"dotnet job {name} has no pin" for name in unpinned]
        fail("parser", "extend the parser: " + "; ".join(bits))
    if not unique(d_pins) or not unique(d_runs):
        fail("pins", "expected at least one pin and one runs-on from dotnet jobs")
    if arch_hits:
        fail(
            "architecture-workflows",
            "update the page: the workflows now name a CPU architecture ("
            + ", ".join(arch_hits)
            + ")",
        )
    page_ids = (
        [f"runs-on {value}" for value in unique(d_runs)]
        + [f"dotnet-version {value}" for value in unique(d_pins)]
        + [
            "reverse-sdk",
            "reverse-os",
            "architecture",
            "untested",
            "dotnet-first",
            "roll-forward",
            "local-feed",
            "commands",
        ]
    )
    if page is None:
        fail("page", "docs/PREREQUISITES.md is missing")
        for cid in page_ids:
            fail(cid, "blocked: page missing")
        return failures
    text = normalize(page)
    for value in unique(d_runs):
        if not has_token(text, value):
            fail(f"runs-on {value}", f"{value} is not on the page as a whole token")
    for value in unique(d_pins):
        if not has_token(text, value):
            fail(f"dotnet-version {value}", f"{value} is not on the page as a whole token")
    extra_sdk = [m.group(0) for m in SDK_TOKEN.finditer(text) if m.group(0) not in set(d_pins)]
    extra_os = [m.group(0) for m in OS_TOKEN.finditer(text) if m.group(0) not in set(all_runs)]
    if extra_sdk:
        fail("reverse-sdk", "SDK tokens on the page not in the workflow pins: " + ", ".join(unique(extra_sdk)))
    if extra_os:
        fail("reverse-os", "OS tokens on the page not in workflow runs-on: " + ", ".join(unique(extra_os)))
    if not any("architecture" in s and "unpinned" in s for s in sentences(text)):
        fail("architecture", "no sentence contains architecture and unpinned")
    paras = paragraphs(text)
    if not any(
        "untested" in p and "try" in p and ".NET 8 SDK" in p and "other operating system" in p
        for p in paras
    ):
        fail("untested", "no paragraph contains untested, try, .NET 8 SDK and other operating system")
    if not any(
        "net8.0" in p and ".NET 8 SDK" in p and "ASP.NET Core runtime" in p and "not enough" in p
        for p in paras
    ):
        fail("dotnet-first", "no paragraph contains net8.0, .NET 8 SDK, ASP.NET Core runtime and not enough")
    if "DOTNET_ROLL_FORWARD" in text:
        fail("roll-forward", "page contains DOTNET_ROLL_FORWARD")
    missing = [needle for needle in LOCAL_FEED if needle not in text]
    if missing:
        fail("local-feed", "page does not contain " + ", ".join(missing))
    failures.extend(command_failures(text))
    return failures


class PrerequisitesSelfTests(unittest.TestCase):
    def test_parser_reads_a_synthetic_matrix_workflow(self):
        # Given a workflow with matrix lists, dash items, include rows and a | pin block
        jobs = {job["name"]: job for job in parse_workflow_text(SYNTHETIC)}

        # When it is parsed, Then both dotnet jobs expose every OS and pin form
        build, listed = jobs["build"], jobs["listed"]
        self.assertTrue(build["is_dotnet"] and listed["is_dotnet"])
        self.assertEqual(["ubuntu-latest", "windows-latest", "macos-latest"], build["runs_on"])
        self.assertEqual(["8.0.x", "9.0.x", "8.0.412"], build["pins"])
        self.assertEqual(["ubuntu-22.04", "windows-2022"], listed["runs_on"])
        self.assertEqual(["8.0.100", "10.0.x"], listed["pins"])
        self.assertEqual([], build["leftover"] + listed["leftover"])

    def test_labels_only_page_fails_every_prose_check(self):
        # Given a page that only names the workflow labels
        jobs = parse_workflow_text(
            "jobs:\n  test:\n    runs-on: ubuntu-latest\n    steps:\n"
            "      - uses: actions/setup-dotnet@v4\n        with:\n"
            "          dotnet-version: 8.0.x\n"
        )
        failures = collect_failures(LABELS_ONLY, jobs)
        ids = [item.split(":", 1)[0] for item in failures]

        # Then the forward match passes and every prose check fails
        self.assertNotIn("runs-on ubuntu-latest", ids)
        self.assertNotIn("dotnet-version 8.0.x", ids)
        for cid in ("architecture", "untested", "dotnet-first", "local-feed", "commands"):
            self.assertIn(cid, ids, failures)

    def test_minimal_compliant_page_passes(self):
        # Given a page that satisfies every check against today's CI pins
        jobs = parse_all_workflows()
        failures = collect_failures(MINIMAL_PAGE, jobs)

        # Then the failure list is empty
        self.assertEqual([], failures)


class PrerequisitesPageTests(unittest.TestCase):
    def test_prerequisites_page_matches_what_CI_uses(self):
        # Given the CI workflows and docs/PREREQUISITES.md on the working tree
        failures = collect_failures(read_page(), parse_all_workflows())

        # Then every independent check is reported in one aggregated assertion
        self.assertEqual([], failures, HEADER + "\n".join(failures))


if __name__ == "__main__":
    unittest.main()
