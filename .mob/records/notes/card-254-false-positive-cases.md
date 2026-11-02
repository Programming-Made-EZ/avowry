# Card 254 reproduction cases (scanner false positives; each verified as no delete)

Each case is a real executed command the scan flagged though nothing was deleted. Card 254's red should pin each as clean, with its real-delete twin still reported.

## Case: bash -lc python heredoc read as a recursive delete (card 210 gate re-check, Copilot seat)

- Source: ~/.copilot/session-state/9d552b52-6df3-4616-9d20-e3f231f3e923/events.jsonl, tool.execution_start call 16, 2026-11-02T16:56:12.520Z.
- Scanner output: "recursive delete on a variable or unresolved path: bash.exe -lc \"python -".
- Verified by the auditor read-only (2026-11-02): the command only writes scratchpad/210-gate-recheck.md and contains no rm, del, remove, unlink, rmdir, rmtree or delete text; nothing was deleted (both repos intact, 0 tracked files deleted). The scanner trips on the heredoc quoting inside bash -lc "...".
- The command, verbatim (first 1500 characters):

```
bash.exe -lc "python - <<'PY'
from pathlib import Path
path = Path('/c/Users/ringe/AppData/Local/Temp/claude/C--Users-ringe/73298da3-f8ff-4a30-af55-b3e95ffa16f4/scratchpad/210-gate-recheck.md')
path.parent.mkdir(parents=True, exist_ok=True)
text = '''## Re-check of the HOLD

- Direct shell: `cd C:\\Users\\ringe\\mob-crosscli && python -m unittest tests.test_mob_summary`
  Result: FAILED (failures=2) in 193.629s.
  Failing tests:
  - `test_updates_are_docs_only_mob_commits_and_they_reach_the_remote`
  - `test_the_summary_can_be_published_to_another_repo_than_the_one_the_run_is_in`

- `cmd /c "cd /d C:\\Users\\ringe\\mob-crosscli && python -m unittest tests.test_mob_summary"`
  Result: FAILED (failures=2) in 188.546s.
  Same two failures as the direct-shell run.

- Git check in `mob-crosscli`:
  - `git --no-pager log --oneline --decorate -n 20 -- tests/test_mob_summary.py` shows the summary-test file was not touched on the card-210 commit.
  - `git --no-pager show --stat --oneline --decorate --summary c633d3a --` shows only `mob_process.py` changed for card 210. There was no modification to `tests/test_mob_summary.py` or the summary publishing code it exercises.

- New evidence since the HOLD gate:
  - Facilitator re-check on c633d3a from a plain bash shell: `1545 tests, OK (skipped=1)`.
  - `tests.test_mob_summary` alone on c633d3a: `16 OK`.
  - Green 3 full run on c633d3a: `1545 OK`.
  - The two failures are shell/environment dependent and are tracked as bug card 268, not a r
```

## Case: cmd /c and bash -lc wrappers read as runs of a file named by the quoted string (card 210 gate and re-check)

- Examples: cmd /c "cd /d C:/Users/ringe/mob-crosscli && python -m unittest discover -s tests"; cmd /c "... python scripts/mob_process.py check"; where; python --version; git -C ... log, each read as 'unreadable file'. No delete executed. Scan outputs are in .mob/records/210/record-in-progress.json command_scans.
