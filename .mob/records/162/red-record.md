# Card 162 red record (QA RED seat, fresh Claude context, worktree on branch card-162-red from main 9474a73)

Red commit (a): `70511e654244c0567177af84f589047192d4782e` (tests and .gitattributes only; no docs, no production code).
This record is commit (b); its own hash is the branch tip and is reported to the facilitator, not written here.
Builds: `dotnet build src/Avowry.Cli` first, then `dotnet build tests/Avowry.Acceptance.Tests` (0 warnings, 0 errors). Only the tests listed here were run; the full gate was not.

## Claims recomputed by command (slice sections 1 and 3)

- `sha256sum` of docs/GRAMMAR.md 8e4f91bd0bc81335e9abba267bd552bd6b508b7525609ca19d6a05a0accd75ff (18944 bytes), docs/CHEATSHEET.md 4b1b9ef51341b814616e8aadbe026836e72f254d74b3ce7dba1e6c70105c6d9e (2070 bytes), docs/PATTERNS.md 4bfa05925169fd0b498a1dce0f89b5f905c90f2d88aef4f60046a1ae07baf082, orders.avowry 4d7a24b9877a79b43b308d5052cb3a807af0dd88465399b9fc639958a4f637f8. All match the slice. No CR bytes in either frozen doc; core.autocrlf is unset (false).
- The slice demanded each of these be verified by running the CLI (`dotnet src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build <file> -o <dir>`). Actual output:
  - nested update `let b = a with { qty: 2 } with { ok: false }` -> `AV200 (12:31) Unexpected 'with'; expected end of line.` (and `with { name: item with { qty: 2 } }` -> `AV200 (11:36) Expected '}', found 'with'.`) CONFIRMED.
  - wrong field type `item with { qty: "x" }` -> `AV312 (11:30) Expected Int, but got Text.` CONFIRMED.
  - duplicate name in `fails with Empty, Empty` -> `AV329 (8:1) Failure 'Empty' is repeated in 'fails with'.` CONFIRMED.
  - also confirmed: empty list `AV200 ... Expected at least one field in a record update`; trailing comma `AV200 ... Expected a field after ',' in a record update`; unknown field `AV311`; repeated field `AV303`; existing name `AV313`; source with no fields `AV311 Type Int has no fields; cannot update it.`; undeclared failure in an ensure step of an avow `AV326 Ensure failure 'Nope' is not declared in 'fails with'.`; `length at most`, `<=` and `!=` conditions compile.
- Satisfiability: the ten needles all pass against scratch copies of GRAMMAR and the sheet edited per slice section 2 (scratch only, not committed). So the needles are achievable and not over-fitted.

## Tests added, status at red

| Test | Trait | Status at red |
|---|---|---|
| tests/test_pattern_docs.py G1 ebnf step lists ensure_step and defines it | n/a | FAILS |
| G2 Ensure section: four conditions, compare_op has all six | n/a | FAILS |
| G3 Ensure section: else, fails with, AV326; AV326 row says ensure | n/a | FAILS |
| G4 "An `ensure` line without `else` is an ordinary avow call" | n/a | FAILS |
| G5 update_step in the ebnf and in the step rule | n/a | FAILS |
| G6 Record update section: AV200 AV303 AV311 AV312 AV313, trailing comma, nested | n/a | FAILS |
| G7 "Every diagnostic code is `AV` followed by three digits" | n/a | FAILS |
| G8 old "ordinary words" sentence gone, "starts a record update" present | n/a | FAILS |
| S1 sheet fence line `ensure ... else Failure` | n/a | FAILS |
| S2 sheet fence line `let x = rec with { ... }` | n/a | FAILS |
| PatternDocs.The_grammar_and_the_cheat_sheet_teach_the_ensure_step_and_the_record_update | Slice=162, Criterion=162-1, Status=Pending | FAILS (Pending) |
| UsableV1.Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone (calls Bar6Failure) | Slice=138, Milestone=UsableV1, Criterion=162-2, Status=Pending | FAILS (Pending) |
| UsableV1.Evidence_rule_V0 .. E6 (7 rows) | Slice=162 | PASS |
| UsableV1Closure.Z1_the_ratchet_has_no_suspended_bar | Slice=162, Criterion=162-3, Status=Pending | FAILS (Pending) |
| UsableV1Closure.Z2_the_bar_6_test_is_not_pending | same | FAILS (Pending) |
| UsableV1Closure.R1_the_refreeze_record_matches_the_files_the_check_runs_against | same | FAILS (Pending) |

Ids per D222: criterion 1 = G1-G8, S1, S2 (+ B1 characterisation); criterion 2 = V0, E1-E6 + the Bar6 test; criterion 3 = Z1, Z2, R1. V-grok and V-codex of the card text are the two per-model halves of V0 and the Bar6 test.

## Pre-change failure messages (actual output)

`python -m unittest tests.test_pattern_docs` -> `Ran 10 tests`, `FAILED (failures=10)`; the assertion line of each:

```
S1: AssertionError: False is not true : no fence line in docs/CHEATSHEET.md matches an ensure ... else Failure step
S2: AssertionError: False is not true : no fence line in docs/CHEATSHEET.md matches a let x = rec with { f: v } step
G1: AssertionError: 'ensure_step' not found in 'let_step | respond_step | fail_step | avow_call' : the ebnf step rule does not list ensure_step: let_step | respond_step | fail_step | avow_call
G2: AssertionError: unexpectedly None : docs/GRAMMAR.md has no '### Ensure step' section
G3: AssertionError: unexpectedly None : docs/GRAMMAR.md has no '### Ensure step' section
G4: AssertionError: False is not true : docs/GRAMMAR.md never says: An `ensure` line without `else` is an ordinary avow call
G5: AssertionError: 'update_step' not found in 'let_step | respond_step | fail_step | avow_call' : the ebnf step rule does not list update_step: let_step | respond_step | fail_step | avow_call
G6: AssertionError: unexpectedly None : docs/GRAMMAR.md has no '### Record update step' section
G7: AssertionError: False is not true : docs/GRAMMAR.md never says: Every diagnostic code is `AV` followed by three digits
G8: AssertionError: True is not false : docs/GRAMMAR.md still says: words like `with` or `input` are ordinary words.
```

dotnet, `--filter "FullyQualifiedName~PatternDocs|~UsableV1Closure|~Evidence_rule|~Bar6_"` (no Status filter), Error Messages:

```
UsableV1Closure.R1_...: docs/decisions/language-and-grammar.md must hold the <a id="refreeze-162"></a> marker exactly once, found 0.
UsableV1Closure.Z2_...: Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone is still marked Status=Pending.
UsableV1Closure.Z1_...: board/usable-v1-ratchet.json still suspends bar(s): 6
UsableV1.Bar6_...: Bar 6 not met.
  llm-check-codex.json: grammar_sha256 is stale; re-run against docs/GRAMMAR.md.
  llm-check-grok.json: grammar_sha256 is stale; re-run against docs/GRAMMAR.md.
PatternDocs.The_grammar_...: (the ten python failures above, ExitCode 1)
```

Note the Bar6 message reports only the grammar hash as stale because EvidenceFailure returns the first failing check per file; the sheet hash is also stale.

## Mutation check (tightened rule is what makes E1-E5 pass)

The body of `Bar6Failure` was temporarily replaced by the old rule (`Directory.GetFiles(dir, "llm-check-*.json")`, pass when `failures.Count < files.Length`), rebuilt, and the Evidence_rule rows run:

```
Failed Evidence_rule_E5_a_file_whose_model_is_not_its_file_name_fails_naming_it   Assert.NotNull() Failure: Value is null
Failed Evidence_rule_E2_a_missing_codex_file_fails_naming_codex                   Assert.NotNull() Failure: Value is null
Failed Evidence_rule_E3_a_file_with_passed_false_fails_naming_it                  Assert.NotNull() Failure: Value is null
Failed Evidence_rule_E1_a_stale_grok_file_next_to_a_current_codex_file_fails_naming_grok   Assert.NotNull() Failure: Value is null
Failed Evidence_rule_E4_a_partial_run_with_specs_passed_below_specs_fails_naming_it        Assert.NotNull() Failure: Value is null
Failed!  - Failed:     5, Passed:     2, Skipped:     0, Total:     7
```

V0 and E6 stay green under the old rule, as expected. The tightened file was then restored from a saved copy (grep for `failures.Count < files.Length` in UsableV1.cs: 0 hits) and rebuilt; all 7 rows passed again, and the committed file is the restored one.

## Characterisation (declared up front, all PASS before any doc change)

```
dotnet test --filter "~Evidence_rule|~PhraseCheatSheet|~EnsureStepCompiles|~RecordUpdateFormCompiles|~WebExtensionBoundary"
  Passed!  - Failed: 0, Passed: 16, Skipped: 0, Total: 16, Duration: 29 s
  (7 Evidence_rule rows, PhraseCheatSheet: Sheet_is_at_most_700_estimated_tokens + Sheet_names_every_keyword_and_core_type_and_its_examples_compile, EnsureStepCompiles, RecordUpdateFormCompiles, WebExtensionBoundary)
python -m unittest tests.test_run_llm_check   -> Ran 15 tests ... OK
python scripts/usable-v1-report.py --self-test -> self-test ok
dotnet test --filter "~NoPhCodesInTheLiveTree|~PhraseCliFreshness" (run after commit (a), so the new files are tracked and scanned)
  Passed!  - Failed: 0, Passed: 28, Skipped: 0, Total: 28   (card 361 live-tree guard clean over the new files)
```

## Slice findings and decisions the next seats need

1. R1 "old GRAMMAR and sheet pins equal the JSON pins" cannot hold forever: after the Grok evidence commit (and later the Codex one) the JSON pins become the NEW hashes. R1 therefore requires each model file's pin to equal the recorded old OR the recorded new hash. Same intent, survives green 2.
2. The slice did not fix the machine-line format for the token figure. R1 expects, after the marker `<a id="refreeze-162"></a>` and before the next `<a id=`: `- refreeze-162 <repo-relative path> <old sha256 or -> <new sha256>` for these seven paths: docs/GRAMMAR.md, docs/CHEATSHEET.md, bench/usable-v1-llm-check/tasks/U1-add-endpoint/prompt.md, bench/usable-v1-llm-check/tasks/U1-add-endpoint/outcomes.json, bench/usable-v1-llm-check/suite.json, demo/tasks/Tasks.avowry, tests/Avowry.Acceptance.Tests/Fixtures/orders.avowry; plus `- refreeze-162-tokens sheet <n>` where n is ceil(UTF-16 chars / 4) (min 1) of the sheet. Hashes are lowercase 64-hex of raw bytes. Program.cs and the runner are not checked by R1 (launcher guard only).
3. The slice's two new cheat sheet avows are NOT both buildable as specs: compiling the sheet with them and running `dotnet test` on the generated specs project fails with `CS1503: Argument 3: cannot convert from 'double' to 'decimal'` for the `close` spec's `price: 2.5` record literal. The same error already occurs for the EXISTING sheet (the Place endpoint spec and the label spec carry `price: 2.5`), so the current sheet compiles to C# (which is all PhraseCheatSheet checks) but its generated specs project does not build today. This is a pre-existing C# adapter defect (a Decimal field in a record literal emits a double), not caused by the slice; green 1 should not claim "compiled and tested" for the sheet module as a whole, and may want to give the `close` avow an item type without a Decimal field. The `check` avow (ensure on a Text) generated fine. Worth a separate bug issue; not fixed here.
4. Slice section 1 figures all reproduced; the section 2 line numbers in GRAMMAR are by content not by number (the lines move once green 1 edits), the needles use headings and rules, not line numbers.
5. Bar6 message: EvidenceFailure reports one failure per file, so a file with both hashes stale names only grammar_sha256. Acceptable; not changed.
6. Evidence_rule rows have only Slice=162 (no Criterion, no Milestone): they are characterisation of the helper, and a Milestone trait would put them in the usable-v1 job whose parse_trx maps names by `Bar<n>_`.
