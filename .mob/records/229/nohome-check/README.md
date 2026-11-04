# Card 229: no home path or secret in a failing seat's vote record

Window 229-scope's adopted condition: before the live step, the facilitator shows that a decider-result.json from a fake failing seat holds no home path and no secret.

- Run: nohome229_check.py (this folder) calls decider.main at mob-crosscli cda02f2 (green 2 cycle 3) with a fake copilot seat (tests/fakes/fake_launch_seat.py) that exits rc 1 with stderr "error: could not open C:<sep>Users<sep><user><sep>secret<sep>creds.txt; token ghp_<36 chars>; The command line is too long." on every attempt; decider.py's probe is replaced by a passing fake, as in the tests.
- Result (check.json; decider-result.json is the file as written, unedited):
  - home path absent: true. The stored excerpt and reason read "C:\Users\<user> token [redacted]; ...".
  - token absent: true.
  - attempt_log present: true (two attempts, rc 1 each); the vote is not counted.
  - contains_secret over the whole file: true, from one field only: prompt_sha, the SHA-256 of the prompt (64 hex characters match the long-token pattern). It is not a secret and predates card 229; no failure-text field matches.
- check.json has the account name replaced by <user> in its file paths (the facilitator's scratch path); decider-result.json needed no change.
