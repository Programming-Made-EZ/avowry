# Card 374 independent gate review (fresh context)

Verdict: PASS (with the notes in section 7). Reviewed mob-defaults main at 396e6434b345c256903f1e260611be9b21bc9cc5. Work was done in throwaway clones under C:/Users/<user>/gate374-review-{pre,post,m1,m2,m3}; no tracked file in either checkout was edited. One slip to disclose: my first shell command contained a stray `rm -rf /dev/null` (a no-op, nothing deleted); no other recursive delete was run. The clones are left in place for the facilitator to remove.

## Step 0: order of commits (TDD red first, red and green on different seats)

Full hashes: red part 1 0e93c52849992e07942acededbc52f12eeaebe00, red part 2 a6f4385ae31643c8cb4688f5cc91e86b764a6337, Grok launcher 1b8e69455c4fcfd4520fd7a7478bae7b968289b3, green 396e6434b345c256903f1e260611be9b21bc9cc5.
`git merge-base --is-ancestor 0e93c52 396e643` and `... a6f4385 396e643` both true; `396e643` is an ancestor of main. Log order (newest first): 396e643, 1b8e694, a6f4385, 241b3fb, 0e93c52. Seats per red-record.md and seat-changes.md: Qwen red 1 (corrected by the facilitator, four corrections recorded), Cursor red 2, a fresh Claude context the green (recorded stopgap: Muse out of quota, Grok had no writing launcher at launch, launcher now exists). Red and green are on different seats. The facilitator committed all; no trailers seen in the commit subjects.

## 1. Red replay

Pre-change tree a6f4385 (clone, `sed -i '/@unittest.expectedFailure/d'` on both modules, grep -c = 0 and 0):
`python -m unittest tests.test_decider_prompt_files tests.test_decider_runner` -> Ran 51 tests, FAILED (failures=3), no errors:
- FAIL test_374_3b (AssertionError: Lists differ: the left-over probe file is in the repository root, expected [])
- FAIL test_374_3a (AssertionError: prompt file written to the repository root, expected under `.scratch-run`)
- FAIL test_374_1 (AssertionError: run a exits 5 with integrity ok False, change '+ ?? prompt-....txt'; expected exit 0, ok True)
Every message is an AssertionError; 374-2a, 374-2b, 374-3c pass on the old tree (characterisation), as the red record says.

Green 396e643 (clone, grep -c expectedFailure = 0 and 0): same two modules -> Ran 51 tests, OK (135 s).

## 2. Scope of the green

`git diff --stat a6f4385 396e643`: decider.py +3, tests/test_decider_prompt_files.py -2, tests/test_decider_runner.py -1, plus grok_write.py (+91) and tests/test_grok_write.py (+65) from the unrelated Grok launcher commit 1b8e694 in between. Excluding that commit the green changed decider.py (3 insertions) and removed exactly the three expectedFailure decorators. `git diff --numstat` and `git diff --ignore-cr-at-eol --numstat` agree line for line for all five files (3/0, 91/0, 0/2, 0/1, 65/0), so no line-ending churn.

## 3. Mutation checks (scratch copies of 396e643, tests selected with -k test_374)

- m1, restore old behaviour (the three `if probe_fn is None` lines removed, CardRun default): 374-3a, 374-3b, 374-1 FAIL (3 failures of 6).
- m2, snapshot ignores `prompt-*` lines in the root: 374-2b FAILS (1 failure), the others pass.
- m3, snapshot ignores a tracked file change (README.md lines): 374-2a FAILS (1 failure), the others pass.
Each mutation is caught by exactly the test(s) intended; the whitelist shortcut and a blind snapshot are both guarded.

## 4. Acceptance criteria

- Two runs at once both finish with counted votes: ProbePromptFileOverlap.test_374_1 (run A votes while run B's start probe holds a prompt file; both exit 0, integrity ok, no changes). Result: red on old tree, OK on green.
- A tracked file changed during a run is still refused: IntegrityStillRefuses.test_374_2a (exit 5, README.md named). OK on green; fails under m3.
- No prompt file remains in the repository root after a run: test_374_3a (file lands under the run directory), test_374_3b (a left-over file from a killed probe stays in the run directory), test_374_3c (control: a backend without the placeholder writes no file); plus 374-2b guarding that a planted root prompt file is still refused. All OK on green (5 tests, 10.5 s).
Flakiness: `ProbePromptFileOverlap` five consecutive runs, all Ran 1 test OK (2.4 to 4.3 s). No flakiness seen. The test uses a file-based rendezvous (in-*/go, 60 s cap) and not sleeps for ordering, which is the right shape.

## 5. Correctness of the decider.py diff

The lambda `lambda name, cfg, _repo, timeout, stall: facilitator.probe_backend(name, cfg, run_dir, timeout, stall)` is applied only when probe_fn is None. Late lookup of `facilitator.probe_backend` is correct (tests that monkeypatch the module attribute see it at call time). The arity matches the CardRun default call `(name, cfg, repo, timeout, stall)`. The run directory is passed in the repo slot, so both the availability prompt-file write and the command probe use it as cwd. Second review finding 1 is a misreading (the lambda already does this; 374-3a proves it). No forbidden names, no new imports; an explicitly supplied probe_fn is untouched. The run_dir is removed by `discard`, so probe files vanish with it. Snapshot ignores nothing new.

Second review finding 2 (scratch is not a git work tree): the facilitator checked live that agy, claude, cursor, grok and qwen all probe from a non-git directory (gate-baseline.md). Accepted; a future trust-folder CLI is a risk to watch, not a defect.
Second review finding 3 (probe_all and card-mode probes still use the real repository): acceptable limit for this card. The card's text covers the decision runner's start probe only, and the finding is named in the slice (finding 9 of 365). It is a real remaining leak for card runs and rotation probes, so it must stay filed as a follow-up and must not be described as fixed.
Qwen gate-lock side effect: the probe's cwd is now the scratch directory, so `.mob/gate.lock` is not seen from there; only the machine-wide dotnet check remains. This is acceptable: a decision window is read-only and does not touch the repository, so the per-repository gate lock was protecting nothing for it. It is a behaviour change and should be written in the card's close note, not left implicit.

## 6. Value rule

Yes. Before: a decision window's start probe wrote `prompt-*.txt` into the repository root, and any concurrent run's end snapshot flagged it (exit 5). After: the file is created under the run's own scratch directory, outside the checked tree, and 374-1 demonstrates two overlapping runs both finishing with counted votes while 374-2a/2b show real changes are still refused. The consumer (anyone running concurrent decision windows) gets the benefit directly.

## 7. Findings, ranked, and what is not delivered

1. (Medium, scope) probe_all and card-mode probes still write into the real repository: the same overlap failure is possible for them. Carry as a follow-up card; do not claim card 374 closes the class.
2. (Low, process) The live parallel decision window proof was not run; the evidence is in-process tests with fakes plus the facilitator's live non-git probe check. Acceptable for a unit-level card but the first real parallel window should be watched.
3. (Low, behaviour) The gate-lock check is no longer seen by the decision-run probe (see section 5); document it.
4. (Low, tests) 374-1 depends on a 60 s hold cap and real subprocesses; stable in five runs here, but a slow machine could lengthen it.
5. Not delivered, as declared: removal of the one-seat-at-a-time line from the D228 preflight, the live parallel window proof, bug 406, card-mode probes. None of these is required by the card's three scenarios.
6. Expectation notes: the pre-existing five failures in the full suite were not re-run by me (instructed not to); the two modules reviewed here are green.

Verdict: PASS. All three scenarios are covered by tests that fail for the right reason before the change, pass after, and fail under targeted mutations; the production change is three lines, correctly placed, with line endings unchanged.
