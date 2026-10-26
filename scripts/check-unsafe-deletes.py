#!/usr/bin/env python3
"""Fail a commit that ADDS a recursive delete of a variable or unresolved path, or a kill by process name, in a script or
source file. Steve's safety rules ("a ban with no detector isn't in force"). Used by .githooks/pre-commit.

The detector is scripts/delete_guard.py, a copy of mob-defaults/delete_guard.py (keep them in sync). Use safe_delete from it
instead of deleting a computed path yourself. Docs (.md) are not scanned: they describe the rule.
"""

import re
import subprocess
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import delete_guard  # noqa: E402

SCANNED = {".py", ".sh", ".ps1", ".psm1", ".cmd", ".bat", ".cs", ".yml", ".yaml", ".js", ".mjs", ".ts"}
SKIP = {"scripts/delete_guard.py", "scripts/check-unsafe-deletes.py"}


def added_lines():
    diff = subprocess.run(["git", "diff", "--cached", "-U0", "--no-color"], capture_output=True, text=True,
                          encoding="utf-8", errors="replace").stdout
    current, line_no, found = None, 0, {}
    for raw in diff.splitlines():
        if raw.startswith("+++ b/"):
            current = raw[6:]
            continue
        hunk = re.match(r"@@ -\d+(?:,\d+)? \+(\d+)", raw)
        if hunk:
            line_no = int(hunk.group(1)) - 1
            continue
        if raw.startswith("+") and not raw.startswith("+++"):
            line_no += 1
            if current and current not in SKIP and Path(current).suffix.lower() in SCANNED:
                found.setdefault(current, []).append((line_no, raw[1:]))
    return found


def main():
    problems = []
    for name, lines in added_lines().items():
        for number, text in lines:
            for finding in delete_guard.find_unsafe(text):
                problems.append(f"{name}:{number}: {finding.kind}: {finding.snippet}")
    if problems:
        print("pre-commit: unsafe delete or kill added (Steve's safety rules):", file=sys.stderr)
        for p in problems[:10]:
            print("  " + p, file=sys.stderr)
        print("  Use delete_guard.safe_delete(path, roots) for deletes; kill only by exact PID.", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
