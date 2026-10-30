# Card 205, red seat: QA engineer (ctx-3, Claude Sonnet subagent)
- Done check 1: acceptance test test_205_1 written red (AssertionError 5 != 0), skipped as PENDING(205-1); it reached mob-defaults main inside b33793a, another session's commit (recorded as a defect).
- Done check 2: test_205_2 already green (the Done check 1 gate covers it), committed as a regression guard 2290341. Done check 3: test_205_3 already green, mutation-proved (AssertionError 0 != 5 with the fail-closed branch broken), guard 46864d0.
- Stand-down 1 (after Done check 1): explicit pathspecs, a staged line, a first-failing-line check.
- Reason for a Claude seat: probes at 04:36 and 06:35 UTC show no other model with usable quota.
