#!/usr/bin/env python3
"""Fail a commit if the staged MOB.md (published to a PUBLIC repo) looks like it holds a secret or a home path.

Used by .githooks/pre-commit. The published MOB.md is a generated summary and is already scrubbed by the facilitator
(mob-defaults/mob_summary.py); this is the second, independent check at the commit boundary.
"""

import re
import subprocess
import sys

PATTERNS = [
    (r"-----BEGIN [A-Z ]*PRIVATE KEY-----", "private key"),
    (r"\bgh[pousr]_[A-Za-z0-9]{20,}\b", "GitHub token"),
    (r"\bgithub_pat_[A-Za-z0-9_]{20,}\b", "GitHub token"),
    (r"\bsk-[A-Za-z0-9_\-]{16,}\b", "API key"),
    (r"\bAKIA[0-9A-Z]{16}\b", "AWS key"),
    (r"\bxox[abprs]-[A-Za-z0-9\-]{10,}\b", "Slack token"),
    (r"\bAIza[0-9A-Za-z_\-]{30,}\b", "Google key"),
    (r"(?i)\b(?:bearer|basic)\s+[A-Za-z0-9._~+/\-]{16,}=*", "auth header"),
    (r"(?i)\b(?:api[_\-]?key|secret|token|passwd|password|authorization|access[_\-]?key)\b\s*[:=]\s*\S+", "credential assignment"),
    (r"\b[A-Fa-f0-9]{40,}\b", "long hex string"),
    (r"[A-Za-z]:\\Users\\(?!<user>)[^\\\s]+", "home path"),
    (r"/(?:home|Users)/(?!<user>)[^/\s]+", "home path"),
]


def main():
    staged = subprocess.run(["git", "diff", "--cached", "--name-only"], capture_output=True, text=True).stdout.split()
    bad = []
    for name in staged:
        if name.split("/")[-1] != "MOB.md":
            continue
        text = subprocess.run(["git", "show", f":{name}"], capture_output=True, text=True, encoding="utf-8",
                              errors="replace").stdout
        for pattern, label in PATTERNS:
            for m in re.finditer(pattern, text):
                line = text.count("\n", 0, m.start()) + 1
                bad.append(f"{name}:{line}: looks like a {label}")
    if bad:
        print("pre-commit: MOB.md must not contain secrets or home paths (the repo is PUBLIC):", file=sys.stderr)
        for b in bad[:10]:
            print("  " + b, file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
