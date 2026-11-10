"""Card 376, green 2: apply the F10 rule to the six records that still hold the local account name.

Run from the repository root: python .mob/records/376/green2_rewrite.py
Each file is read as bytes, passed through home_paths.scrub_account_bytes with the local accounts
read from the environment at run time, and written back; every other byte stays.
"""
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "scripts"))

import home_paths  # noqa: E402

PATHS = (
    ".mob/records/226/record-in-progress.json",
    ".mob/records/229/nohome-check/nohome229_check.py",
    ".mob/records/276/record-in-progress.json",
    ".mob/records/276/signoff-ctx6.md",
    ".mob/records/313/green1-codex-events.jsonl",
    ".mob/records/313/slice-architect.md",
)


def main():
    accounts = home_paths.local_accounts()
    if not accounts:
        print("no local account; nothing rewritten")
        return 0
    for relative in PATHS:
        target = ROOT / relative
        data = target.read_bytes()
        scrubbed = home_paths.scrub_account_bytes(data, accounts)
        if scrubbed != data:
            target.write_bytes(scrubbed)
        print(f"{relative}: {len(data)} -> {len(scrubbed)} bytes")
    return 0


if __name__ == "__main__":
    sys.exit(main())
