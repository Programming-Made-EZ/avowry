# Card 422: the facilitator's rulings on the architect's slice (2026-11-15)

The slice is `slice-architect.md` and the corpus is `corpus.txt`: 176 rows in 6 families and 25 encodings. The seat was the architect (claude opus = claude-opus-5-5, ctx-1, Decider D268). Main was read at 7f4f805. The D268 proposal named e5bf308, the head before the kickoff commits; there is no code difference.

Ruling 1 (R-1): accepted. The reviewed licence-URL table lives in the C# test. A package that only gives a licence URL that is not in the table fails until a reviewer adds the row. Failing closed is the point: a new URL needs a human review before the page can claim its licence.

Ruling 2 (R-2): accepted as the reference has it. A tool package under a false Condition is still reported. The tool must not gain a package declaration in any form, whether or not this build evaluates it.

Ruling 3 (R-3): a named limit of this card, with no rows. A nearer `Directory.Build.props`, a `.targets` file or the project file itself can still override the two lock properties after the root props set them. It is filed as a follow-up card.

Ruling 4 (R-4): the Python test skips the commit-blob check with its reason on a shallow checkout. The C# wrapper writes the Python output to the test log, so a passing run shows the skip.

Ruling 5 (R-5): out of scope. `MOB.md` in the guard's allow-list with no History entry is filed with ruling 3's follow-up. This card removes only H6 and H7.

Ruling 6 (R-6): accepted. The page is checked as it is written. Rewording a figure sentence fails until the check's pattern changes in the same commit, and the failure names the sentence.

The 8 deliberate outcome changes in the slice (I-comment, I-cdata-text, O-warm-warning, O-warm-nopkg, O-partial, O-planted-offline, O-planted-nu10041, G-other-lock) are accepted with the reasons the slice gives. The red embeds every corpus row. The networked parts of [E] and [O] run only under the heavy-run lock at the gate and the sign-off; their offline parts are red and green on fixtures.

## Review round 1 rulings (facilitator, 2026-11-15)

Review round 1 is `review.md`, run on card-422 at 232be69 by the QA seat (claude opus = claude-opus-5-5, ctx-4). Verdict: FLAG, 2 BLOCKING, 10 MINOR. The facilitator replayed both BLOCKING findings in the source. `RestoredLicenceChecks.LicenceValueFailures` and `RestoreOutcome` are called only by the offline fixture Theories. F6 still checks only that a licence file or URL exists. F5's planted step and `RestoreLockedOrFail` still test the network-miss text first and match NU1004 as a substring.

Cause: the slice designed the two helpers for the networked tests to call, but the facilitator's green brief asked only to port them and replace the stubs. It never named F5, F6 and `RestoreLockedOrFail` as their callers. The red's rows tested the helpers in isolation, so the green passed every row with the networked path unchanged. This is a brief gap, not a seat error.

R1-1 (BLOCKING 1 and 2): fixed in this card's fix round.
- F6 builds evidence from each restored package (the packaged licence file's opening text, or its licence URL) and fails on `LicenceValueFailures` against the page rows.
- F5's planted step and `RestoreLockedOrFail` classify with `RestoreOutcome` and the restored-package count.

The fix slice moves the evidence building and the classification into pure functions that take the restore's output and the package folder's files as input, so that red rows can prove the wiring offline. A red row must fail today because the networked code path does not use the helper, not because a helper is wrong.

R1-2 (MINOR 3): fixed in the fix round, because ruling 4 is unmet on real runs. The card's `AssertPythonTestsPass` writes the Python output to the test log, so the shallow-checkout skip shows on a passing CI run.

R1-3 (MINOR 4 to 12): named limits of this card, filed on #444 with the review's minimal inputs. They are contrived, absent from this repository, older, or fail closed. MINOR 6 and 12 are new but contrived. MINOR 11's first two items (a wrapped URL, a notice item in a code span) fail closed on shapes the page does not use.

The fix round runs the same seats as D268:
- the architect's fix slice (claude opus);
- the red (claude sonnet);
- the green (cursor gpt-5.3-codex), whose brief names every caller;
- the gate on a quiet machine under the heavy lock;
- review round 2 (claude opus).

## Review round 2 rulings (facilitator, 2026-11-15)

Review round 2 is `review-2.md`, run on card-422 at c0787a7 by the QA seat (claude opus = claude-opus-5-5, ctx-4b). Verdict: FLAG, 1 BLOCKING, 1 MINOR. It confirmed that round 1's BLOCKING 1 and 2 are fixed: the networked F6 checks licence values, and F5 and `RestoreLockedOrFail` classify with `RestoreOutcome`. It replayed both with replicas over the real evidence.

The facilitator replayed the new BLOCKING. In a depth-1 clone of card-422, `python -m unittest tests.test_restored_package_licences` gives `FAILED (failures=1)` on `test_422_FL_py_L5_deep`. CI's `test` job checks out with `actions/checkout@v4` and no `fetch-depth`, then runs `Status!=Pending`, so the row would turn CI red on main.

R2-1 (BLOCKING 1): fixed in fix round 2, a test-design fix only.
- The row FL-py-L5-deep proves that nothing is printed on a deep checkout without depending on the checkout's history. It stubs the git lookup, or uses a page that cites no commit, as the reviewer proposes.
- Its D265 neighbour rows: the same assertion passes in a full clone and in a depth-1 clone.
- The seat proves it by running the Python module in a NEW depth-1 local clone of its working tree and in the worktree.
- From now on the gate script also runs the Python module in a depth-1 clone, the shape CI uses.

R2-2 (MINOR 2): fixed in the same round. The `F6Failures` and `ClassifyRestore` summaries describe what the functions do now. Theories named `*_pending` that no longer carry the Pending trait are renamed without the suffix, and their rows are unchanged.

Seat: the red seat owns the row (claude sonnet = claude-sonnet-5-5, ctx-2c, as D268 declares for test writing). The two summary comments are documentation and change no behaviour. One seat makes both changes, and the facilitator reruns every filter.
