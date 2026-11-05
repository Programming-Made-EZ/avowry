# Card 264 scope proposal turn (architect, Codex ctx-2, read-only; commands through cmd.exe; reply extracted)

Facilitator notes (facts): the seat wrote nothing (its reply is the text below, with the Codex CLI banner, prompt echo and a trailing shell-cwd line trimmed). Its Size line says 5537 bytes; the facilitator measured 5536 before that line. Trigger: green 1 attempt 1 (Claude ctx-4) stopped after one run: the class has 17 rows, a loaded run took 339 s, 20 runs are about 112 minutes. Nothing was committed by that attempt; its checker, helper and a partial record sit untracked in the working tree (the partial record must not be committed).

---

# D175 scope proposal — card 264

## D184 premise corrections

```text
> git rev-parse HEAD
ca0868ed3f5b3a8306dac9c47b33b593e9effd19
```

1. The repeat target is not four rows. It has two four-row Theories, seven Facts, and one two-row Theory: 17 rows.

```text
> findstr /n /c:Theory /c:Fact /c:InlineData tests\Phrase.Acceptance.Tests\PhraseExecutedOutcomesAcceptance.cs
18:    [Theory]
20:    [InlineData("004-read-phrase", "read")]
21:    [InlineData("005-read-failure", "read")]
22:    [InlineData("007-modify-return", "modify")]
23:    [InlineData("008-modify-type", "modify")]
45:    [Theory]
47:    [InlineData("004-read-phrase", "read")]
48:    [InlineData("005-read-failure", "read")]
49:    [InlineData("007-modify-return", "modify")]
50:    [InlineData("008-modify-type", "modify")]
72:    [Fact]
91:    [Fact]
107:    [Fact]
161:    [Fact]
196:    [Fact]
215:    [Fact]
462:    [Fact]
495:    [Theory]
497:    [InlineData("010-fix-trigger")]
498:    [InlineData("011-fix-type")]
```

2. A loaded class run is about 339 seconds. The partial record is not acceptance evidence—the old helper falsely marked this passing run failed—but its measurements are useful.

```text
> findstr /n /c:"wall_seconds" /c:"slowest_seconds" /c:"rows" /c:"passed" .mob\records\264\repeat-evidence.json
16:      "passed": false,
17:      "slowest_seconds": 36.55,
18:      "wall_seconds": 339.4,
19:      "rows": 17,
33:    "passed": 0,
```

The same record's captured process output says `Passed: 17, Failed: 0`; the helper expectation bug is fixed in the untracked helper. Twenty runs project to about 112 minutes, beyond the 60-minute seat cap.

3. Final criterion-1 evidence must describe the tree that ships. Criterion 2 changes the harness, and the board requires the corrective change on main before closure.

```text
> findstr /n /c:"corrective change is merged on main" board\264-*.md
24:  2. The root cause is named ... Card 264 and audit FAIL 123 stay open until the corrective change is merged on main, the same reproduction passes on it, and the evidence with its commands and results is committed; ...
```

Therefore a record taken before criterion 2 would need repeating after it.

## Amendment and cycle order

Amend criterion 1 into two cycles, ordered **1a, 2, 3, 1b**.

- **1a — green:** commit `scripts/check-repeat-evidence.py` and `scripts/repeat-run-264.py`. The 19 fixture tests in `tests/test_repeat_evidence.py` pass. Its two real-record tests intentionally stay red on main until 1b because no truthful fixed-tree record exists. This is acceptable here because CI runs no Python unit tests; only the process check is enforced, and this checker is not wired into it. It cannot look finished: the committed tests name the missing evidence, criterion 1 remains unsatisfied, the card/audit stay open, and the guardrail forbids closure before fixed-tree evidence is committed and checked.
- **2, then 3:** fix/diagnose the harness and commit findings; classify 11:27.
- **1b — record:** after 2 and 3 are green and merged, the facilitator starts the 20-run helper on that fixed commit as a machine job, not an LLM seat turn. It has a hard outer boundary per process and kills only the process tree belonging to its own PID. A seat verifies and commits the real `.mob/records/264/repeat-evidence.json`; both real-record tests then pass.

Raise criterion 1 from 1 cycle to 2. The sum becomes 4, above D175's limit of 3. The window is asked to approve exactly one extra cycle and the staged two-test red. Cost: one evidence review/commit cycle plus about 112 minutes of unattended machine execution, with no seat waiting.

## Batch load and scope

Keep `BenchmarkHarnessAcceptance` beside the target, using exactly two concurrent `dotnet test` processes: one filtered to `PhraseExecutedOutcomesAcceptance`, one to `BenchmarkHarnessAcceptance`. Build once, then use `--no-build`; record exact commands, concurrency, boundary, per-run TRXs, and timings.

Do not narrow to the Theory rows named on the card. Done check 1 says the class, now known to be 17 rows. Narrowing weakens the contract and removes some class-level process/parallel pressure. A Theory-only batch requires explicit window approval of that weakening; this proposal does not request it.

## Failure semantics

Record a failed run truthfully with run number, row, process exit, stdout/stderr, history existence, timing, and load result. The helper may continue to preserve the planned observation, but the batch no longer proves 20 consecutive passes: the card stays open and criterion 2's findings file must account for the failure.

The facilitator must not re-run to replace or hide a failure, select a later clean sequence, delete the failed entry, or trim/relabel the 20. A new batch is justified only by a committed correction or a documented helper/infrastructure defect; retain the failed batch.

## Reversal triggers and cost

Reverse to the three-cycle scope if the fixed-tree batch proves safe within a normal seat, CI starts enforcing the two real-record tests before 1b, or the window rejects the temporary red or fourth cycle. Re-slice if criteria 2 or 3 change the target, load, command, concurrency, or harness semantics. Stop and return to findings on any batch failure, timeout, missing/corrupt TRX, helper misclassification, load failure, or evidence that the job killed a PID it did not start.

Expected cost: about 112 minutes of machine time, no seat time waiting, then one bounded seat cycle to validate and commit the record.
Size: 5537 bytes

## Adopted condition (round 1 of the window; this is the text re-asked of one seat, D187 and D145: no new demands)

Round 1: NO OBJECTION from the product manager, QA engineer, backend developer, decider and agile coach; OBJECTION from the independent reviewer (audit lens) with one condition: closure must be blocked by committed CI checks that verify the committed repeat-evidence.json and both real-record tests on main, so passing cannot rely on untracked helpers or facilitator-only machine runs.

Adopted as: in cycle 1b, in the same commit that adds the real record, the seat wires scripts/check-repeat-evidence.py into scripts/mob_process.py check, so the CI job and the blocking pre-push check fail on main without a valid committed record, and the two real-record tests pass on a clean checkout with no untracked file. The wiring is not done earlier, because the record does not exist before 1b and the check would fail every push in between; that is why cycle 1a leaves it out. Closing issue 264 and audit FAIL 123 waits for that 1b commit and a green process check on main.
