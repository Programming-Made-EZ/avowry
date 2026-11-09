# Card 210 green 1: backend developer (green, ctx-5)

Intent (D165): make the record check stop trusting a value just because it is present, for the task force and the probes (MD-003; D157 Criterion 1), with the smallest change to mob_process.py.

Green for red 572eb4b. Code commit 690975a (mob-crosscli, pushed to origin main). git status first: the repo was in sync with origin, no other seat's commit unpushed; only the untracked .mob/ was left alone.

Change:
- check_record, when the key is present, now calls two new helpers. _task_force_problems: not a list is refused ("task_force: must be a list"), an empty list is refused, a member that is not an object is refused. _probe_problems: probes must be an object with a parseable taken_at and a results object; codex, copilot, grok and agy each need an entry with a bool ok and non-blank evidence (a failed probe still covers its model); a probed_at, when present, must parse; claude and other names are ignored. A small _parses helper tests a time string.
- check_record builds a copy of the record with only object members, so the later checks (removals, command scans) cannot raise on a wrong type. The stale rule is unchanged and runs only when taken_at parses. Findings gate, commit rules and trust ladder untouched. Line endings kept (LF).
- Files: mob_process.py only (62 lines added, 6 removed).

Counts: red 1 module 25 tests OK. The 4 named modules (test_mob_process, test_command_scans_record, test_command_scans_record_strict, test_record_task_force_probes): 129 tests OK, down from 54 failures and 4 errors.

Time from tests passing to push: 13:27:52 to 13:27:54 UTC, about 2 seconds (commit and push).

Full suite on the committed code 690975a (python -m unittest discover -s tests, 13:28 to 13:56 UTC): Ran 1475 tests in 1716 s, OK (skipped=1). No failure, so no flaky card 258 hit. The log holds "SMOKE FAILED" lines printed by a test of the smoke script; they are expected test output, not failures. Log: C:\Users\<user>\AppData\Local\Temp\claude\dev210-1-a7k3\full.log.

MOB.md: not touched in this step.
Size: 2079 bytes
