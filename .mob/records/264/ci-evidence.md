# Card 264 CI evidence (facilitator; facts only, fetched 2026-11-05 12:40 UTC with gh)

Why the facilitator fetched it: the slice seat (Codex, read-only) has no network access, and window 264-taskforce put the CI log reading first (check 3 and the findings file).

## The two failed runs named on the card (times 11:27 and 11:34 are Pacific, UTC minus 8 on 2026-11-01)

- **11:27, run 41341801479** (2026-11-01T19:27:14Z, ci workflow, sha f682823): the run failed, but its `test` job succeeded. The failing job is `mob-process` ("Check mob process"), a different job. The 11:27 failure is therefore not this test. A second workflow run on the same sha, `mob-process` (41341813972), also failed.
- **11:34, run 41342389875** (2026-11-01T19:34:11Z, ci workflow, sha dc7cfed, the `test` job failed; run time about 16 minutes):
  - Test: `Phrase.Acceptance.Tests.PhraseExecutedOutcomesAcceptance.Correct_read_and_modify_replies_pass_and_record_the_starting_code(task: "004-read-phrase", kind: "read")`, failed after 21 s (xUnit reported 28.27 s).
  - Error: `System.IO.DirectoryNotFoundException : Could not find a part of the path '/tmp/phrase-acceptance/67e882c73cf546a6883483c7ede6b5b4/bench/results/history.jsonl'.`
  - Stack: `File.ReadAllLines` called from `PhraseExecutedOutcomesAcceptance.ReadHistory(String suite)` at tests/Phrase.Acceptance.Tests/PhraseExecutedOutcomesAcceptance.cs:line 359, called from the test at line 36.
  - The sibling row `task: "007-modify-return", kind: "modify"` passed in the same run (26 s), and other acceptance classes passed around it, each taking 16 to 34 s.
- 11:20 and 11:30 (runs of 2026-11-01T19:20:54Z sha af5bf68, and 19:30:46Z sha 71f89ca): success.

## Other failed `ci` runs found in the run list (not inspected for this test unless stated)

- 2026-11-01T23:54:44Z sha 9ba855c; 2026-11-02T17:48:21Z sha 1229090: `ci` failed; the failing step was not fetched.
- 2026-11-03T23:52:50Z sha 3f55b5e: the failed test is `MobProcessAccountability.The_mob_process_checker_proves_its_audit_behaviors`, a different test.
- 2026-11-04T14:51Z to 15:18Z, shas 7a31627, ea1ea7b, 5bf08e2, 51e3612, 64615c7, 9e9e924 (six `ci` runs in a row): failure details not retrieved (no `[FAIL]` line matched in the failed log).

## What this shows (and does not)

- Shows: check 3's answer is "a different job (mob-process), not this test"; the 11:34 failure is a missing results file at the end of the test, not a timeout or an assertion on the outcome.
- Does not show: why the file was missing (the child process failing or not running, a concurrent build collision, a temp directory cleaned by another test) or any reproduction. That is the architect's and QA's work.
- The log has no record of the child process's own output; the test's `Run` helper's handling of a failed child is the first thing to read.
