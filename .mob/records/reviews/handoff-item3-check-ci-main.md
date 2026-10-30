# HANDOFF required item 3: scripts/check-ci-main.py judges the ci run of the exact head commit

- Defect: at c20bc15 the script printed green while the ci push run was in progress, because it read the newest run of any workflow (a scheduled mob-process success).
- Seats (each a fresh Claude Sonnet context; reason: Codex at its usage limit, Copilot, Grok and agy out; the later seats were assigned on the probes of 04:03 UTC, .mob/records/probes/2026-10-30.jsonl; the red seat and the first green seat relied on probes from 02:16, which is audit finding F-040):
  - QA red seat: 14 tests, 9 failing by assertion (the c20bc15 case, ZOMBIES cases), committed skipped as PENDING(ci-head-1) in f2eac94; line endings normalised to LF in 0f7225f (whitespace only).
  - Backend developer green seat: head-commit verdict; the skips removed, the test bodies unchanged.
  - Reviewer 1: CHANGES REQUESTED (a non-commit head answer fell back to the old logic; unknown exited 0).
  - Backend developer seat 2: resolve_head raises, unknown exits 2, two tests red then green.
  - Reviewer 2: CHANGES REQUESTED (an empty head from an injected resolver still reached the headless path; an older failed ci attempt beat a newer successful rerun).
  - Backend developer seat 3: empty or non-string head exits 2 with no gh call; the newest ci attempt per SHA governs (createdAt, then run id); three tests (two red then green, one guard).
  - Reviewer 3: APPROVE, no findings against the requirement; minor note: --limit 50 could truncate a SHA with more than 50 runs.
- Tests: tests.test_check_ci_main 24 OK; full Python suite 278 OK, 7 skipped.
