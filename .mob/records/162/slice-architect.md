# Card 162 slice (architect seat, read-only Claude context; every figure recomputed on main at 7972c74)

## 1. Recomputed facts and corrections to the draft

- Hashing: the runner hashes raw bytes (sha256_file, read_bytes), no line-ending handling. The files are LF in the index and worktree; no autocrlf, no eol rule for docs/. The red adds `docs/GRAMMAR.md text eol=lf` and `docs/CHEATSHEET.md text eol=lf` to .gitattributes.
- Current hashes (raw bytes) against the pins in llm-check-codex.json and llm-check-grok.json (lines 12-13):
  - docs/GRAMMAR.md (18944 bytes) current 8e4f91bd0bc81335..., pinned ad0f6e51fe1d6196...
  - docs/CHEATSHEET.md (2070 bytes) current 4b1b9ef51341b814..., pinned fe3d8a8b37f4b5bf...
  - bench/usable-v1-llm-check/tasks/U1-add-endpoint/prompt.md (252 bytes) 0a57714990b05621..., not pinned
  - .../U1-add-endpoint/outcomes.json 57d330cc5fad87974..., not pinned
  - bench/usable-v1-llm-check/suite.json 5ad91957db8fc1703..., not pinned
  - demo/tasks/Tasks.avowry eee88242defe..., not pinned
  - docs/PATTERNS.md 4bfa059251..., not pinned
- Orders source: tests/Avowry.Acceptance.Tests/Fixtures/orders.avowry, hash 4d7a24b9877a79b4..., no CRLF. The draft's "ratified unchanged" is wrong: D72 froze 194003df... as orders.phrase (confirmed at 3e41db9^); the file changed in the card 199 rename and again in 1dece86 (card 196). The generated-output pin 19fe6d71... (WebExtensionBoundary.cs:18) is unchanged.
- The check prompt does NOT contain the orders fixture. The runner's docs root holds only GRAMMAR and the cheat sheet (TryReadDocumentationBlock, Program.cs about 446-478). Block = GRAMMAR, blank line, sheet, blank line; docs_hash today 76e3d119482e..., about 5254 estimated tokens. Full prompt adds the cold preamble, prompt.md, "Task file bodies:", demo/tasks/Tasks.avowry copied at run time; about 6325 estimated tokens now (recorded tokens_in 5836 on 2026-10-28).
- Freeze window omission in the draft: it must also cover Tasks.avowry, outcomes.json, suite.json, tools/Avowry.Bench/Program.cs (prompt composition, cold flags) and scripts/run-llm-check.py.
- Cheat sheet size: ceil(chars/4) (PhraseCheatSheet.cs, cap 700 at line 81). Now 2070 chars = 518 tokens. GRAMMAR 4736 tokens.
- Bar6 today (UsableV1.cs:101-113, Pending): reads every llm-check-*.json; passes when failures.Count < files.Length (any one current file suffices); a file is current when passed, specs > 0, specs_passed == specs, build_warnings == 0 and both hashes match. It checks neither model, proof_level nor attempts. "At least 13 specs" is not enforced by Bar6 (the gate reviewer's addendum checks it).
- Report today: both pins stale so Bar6 fails: "5 of 6 criteria met", SUSPENDED line and warning, "USABLE v1 REACHED ... (with suspended bars)", exit 0.
- docs_hash rows: bench/results/history.jsonl has 296 rows labelled with only 6b9d6c04 and 233c3fc6, neither equals the current block hash. The check run writes a private results file, so appends no row.
- Draft errors: PatternDocs.cs should use ProcessRunner.RunAsync (as DocumentationPrinciples.cs does), needing no ProcessStartCallers entry (adding one fails the exact-set guard). AV329 (duplicate name in `fails with`, Resolver.cs:79) is missing from the GRAMMAR table. GRAMMAR line 26 omits the two-character symbols <= >= == != (lexer line 212). A failure code in an endpoint's ensure step is never AV326 (declaredFailures is null); it becomes an effective failure. ADR-extension-map.md cites GRAMMAR line numbers already off by about 3; green 1 shifts them further: leave and note the drift. AV011 is documented but no emitter was found in src.

## 2. Doc changes (green 1)

GRAMMAR.md
- Line 26: add "and the comparisons <=, >=, ==, !=".
- Line 33: replace the sentence with: "Inside avow templates and steps, words like `with` or `input` are ordinary words, except that `with {` after a name already in scope starts a record update, and a line that starts with `ensure` and contains `else` is an ensure step."
- EBNF lines 85-86:
  step = let_step | update_step | ensure_step | respond_step | fail_step | avow_call ;
  update_step = "let" , ident , "=" , path , "with" , record ;   (* root of path in scope *)
  ensure_step = "ensure" , condition , "else" , Ident ;
  condition = path , "is" , [ "not" ] , "blank" | path , "length" , "at" , "most" , number | path , compare_op , expr ;
  compare_op = "<" | "<=" | ">" | ">=" | "==" | "!=" ;
- Line 121: the step list becomes "`let`, `respond with`, `fail with`, an ensure step or a record update". Line 124: append "(an `ensure` line without `else` is an ordinary avow call)". Line 146 (ensure verb row): append "Built in as the ensure step; without `else` it is wording for your own avows." Line 140 unchanged (AV011 concerns template wording only).
- New `### Ensure step` before "Library lints" (anchor #ensure-step): form `ensure <condition> else <Failure>`; four bullets (`is blank`, `is not blank`, `length at most N` non-negative 32-bit, an Int comparison with a signed 32-bit literal or an Int path on the right); AV326 in an avow, an endpoint's effective failures, AV329, AV312, AV200; the sentence "An `ensure` line without `else` is an ordinary avow call".
- New `### Record update step` (anchor #record-update-step): copy semantics; source in scope (hole, `let` or `input`, optionally with a path); `with {` after any other word is ordinary words; AV200 for an empty list, a trailing comma or a nested update (bind the inner update to a `let`); AV311 unknown field or a source with no fields; AV303 repeated field; AV312 wrong type; AV313 existing name; the update is a `let` step only.
- Table: line 186 becomes "Composite fails with an undeclared code (`fail with` or `ensure ... else`)"; add row "AV329 | A failure code repeated in `fails with`" after AV328.
- AV sentence, first line under "Resolver diagnostics": "Every diagnostic code is `AV` followed by three digits (for example `AV001`), and a code keeps its number." Nothing in the new text may use bare PH.
- Line 269: "Composite avows with `steps` (`let`, ensure and record update steps) are emitted by the C# target; constructs it does not emit yet are `AV340`."

CHEATSHEET.md: append to the paragraph on line 3: "Steps: `ensure <Text path> is [not] blank|length at most N else Code`, or `ensure <Int path> <|<=|>|>=|==|!= <Int> else Code`; Code must be in `fails with` (AV326). No `else`: plain call. `let y = x with { f: v }` copies a record." Add two avows to the existing Shop fence before "count of":

    avow to "check {title: Text}"
      fails with Blank
      steps
        ensure title is not blank else Blank
      spec "rejects blank"
        given title = ""
        when called
        then fails with Blank

    avow to "close {item: Item}"
      fails with Empty
      steps
        let shut = item with { ok: false }
        ensure shut.name is not blank else Empty
      spec "rejects a blank name"
        given item = { name: "", qty: 1, price: 2.5, ok: true, tag: "t-1" }
        when called
        then fails with Empty

Size after: 2771 chars = 693 tokens (margin 7). If the cold reader needs more words, trim a spec first. Both avows follow the shapes of the passing slice-159 and slice-160 sources (EnsureStepCompiles.cs, RecordUpdateFormCompiles.cs). NOT compiled by the architect (read-only): green 1 must run `dotnet <Cli> build` and `dotnet test` on the generated specs, and PhraseCheatSheet must stay green.

PATTERNS.md: replace lines 17-26 with one sentence linking GRAMMAR.md#ensure-step; replace lines 36-40 with one sentence linking GRAMMAR.md#record-update-step. Keep both examples, the adapter paragraphs at lines 28 and 42, and the native-block table.

Prompt word: only bench/usable-v1-llm-check/tasks/U1-add-endpoint/prompt.md has it ("valid Phrase source" becomes "valid Avowry source"). Program.cs has no such text; the runner's docstring says "Phrase" but is a comment: leave it (freeze window).

## 3. Red (fresh Claude context)

tests/test_pattern_docs.py reads the working tree only (no git history), splits sections on `### `, collapses whitespace.

- G1 the ebnf `step` rule lists ensure_step and an ensure_step rule exists (fails: neither).
- G2 the Ensure section holds `is blank`, `is not blank`, `length at most`, and compare_op lists all six operators (no section).
- G3 the section holds `else`, `fails with`, `AV326`, and the AV326 table row says `ensure` (no section; row lacks it).
- G4 "An `ensure` line without `else` is an ordinary avow call" (absent).
- G5 update_step in the ebnf and in the step rule (absent).
- G6 the Record update section holds AV200, AV303, AV311, AV312, AV313, "trailing comma", "nested" (no section).
- G7 "Every diagnostic code is `AV` followed by three digits" (absent).
- G8 the old sentence "words like `with` or `input` are ordinary words." is gone and "starts a record update" present (old sentence still there).
- S1 a sheet fence line matching `^\s+ensure .+ else [A-Z]\w*$`; S2 a sheet fence line matching `^\s+let \w+ = [\w.]+ with \{ .+ \}$` (no match).

PatternDocs.cs: calls `python -m unittest tests.test_pattern_docs` through ProcessRunner.RunAsync, 120 s timeout; Traits Slice=162, Criterion=162-1, Status=Pending; no ProcessStartCallers change.

UsableV1.cs: extract `internal static string? Bar6Failure(string dir, string grammar, string sheet)`: requires exactly llm-check-codex.json and llm-check-grok.json, each passing; checks model equals the file name and proof_level is `specs-passed`; other llm-check-*.json ignored. The Bar6 test calls it, trait Criterion=162-2, stays Pending. Rows named `Evidence_rule_*` (a name starting `Bar6_` would clobber bar 6 in parse_trx): V0 both current (met); E1 grok stale; E2 codex missing; E3 passed false; E4 specs_passed < specs; E5 wrong model; E6 an extra stale third file ignored. They pass at red (the tightened rule lives in the test); the red record must show a mutation check: restore `failures.Count < files.Length` and watch E1-E5 fail.

UsableV1Closure.cs, all Pending, Criterion=162-3, no Milestone trait: Z1 usable-v1-ratchet.json has no `suspended` entry; Z2 the Bar6 method has no Status=Pending trait (reflection); R1 (section 4).

Characterisation, passing now: B1 (Sheet_is_at_most_700_estimated_tokens), the sheet compile test, EnsureStepCompiles, RecordUpdateFormCompiles, the card 361 live-tree guard, WebExtensionBoundary, tests.test_run_llm_check, usable-v1-report.py --self-test.

## 4. Re-freeze record (docs/decisions/language-and-grammar.md)

- When: after the cold reader and its page revisions, before Grok's run. Register the Decider in INDEX.md and areas.json.
- Text: seats and votes, the cold reader's file, the commit sha of the final document revision.
- Machine lines in a section marked `<a id="refreeze-162"></a>`, one per file: `- refreeze-162 <path> <old|-> <new>`. Files: GRAMMAR, the sheet, prompt.md, outcomes.json, suite.json, Tasks.avowry, orders.avowry; Program.cs and the runner (informational, launcher guard only).
- Old values: GRAMMAR ad0f6e51... (the pin; 88746d6b... at the card 361 green's parent, 8e4f91bd... at the tip). Sheet fe3d8a8b.... Orders 194003df..., changed by cards 199 and 196.
- Token report: GRAMMAR 4736 then the new figure; sheet 518 then at most 700; block 5254; prompt about 6325.
- Baseline answer: earlier docs_hash labels (6b9d6c04, 233c3fc6) are not compared with later rows; the scored arm's frozen set moves to the new GRAMMAR and sheet plus the current orders file.
- D126 `to` case: not forced by the fixed task; met by GRAMMAR line 35, the sheet's "never part of the name", and the parser tests.
- R1 is un-pended in the record commit so the gate enforces the freeze window: marker appears once; each new hash equals sha256 of the working file; old GRAMMAR and sheet pins equal the JSON pins; sheet tokens at most 700 and equal to the recorded figure.

## 5. Order and what turns green

1. Red: .gitattributes lines, the python test and wrapper, the UsableV1 helper and E rows, the closure tests. Green: E rows and characterisation. Pending: PatternDocs, Bar6, Z1, Z2, R1.
2. Green 1: GRAMMAR, sheet, PATTERNS, prompt.md. PatternDocs un-pended and green, plus the sheet cap. Bar6, Z and R1 stay Pending; the suspension stays.
3. Cold reader record (docs only). 4. Page revisions (GRAMMAR and sheet only; re-run PatternDocs and PhraseCheatSheet each time).
5. Re-freeze record: R1 turns green, its Pending trait comes off; the freeze window starts.
6. Gate review pass A (record only).
7. Grok run, evidence commit (llm-check-grok.json plus run-grok.md). Bar6 still fails (codex stale), still Pending and suspended. If Grok fails, stop and hold a Decider.
8. Codex run, evidence commit. After it Bar6 passes in the usable-v1 job but is still Pending in the gate.
9. Green 2: remove Pending from Bar6, Z1, Z2; delete `suspended.6`. Report: 6 of 6, no suspension warning.
10. Gate addendum, then sign-off in a clean clone with the full gate run alone.

## 6. Launcher llm_check_when_back.py (mob-crosscli)

CLI: `python llm_check_when_back.py --model {codex|grok} --card 162 [--repo DIR] [--refreeze-commit SHA] [--model-command CMD] [--timeout 600]`. --refreeze-commit defaults to `git log --grep='^Card 162 re-freeze record' -1`. Reuses facilitator.probe_backend, probe_all.row_sha256, probe_all.repo_head.

Order of work:
0. If .mob/records/162/run-<m>.done exists, exit 0 (the 30-minute trigger repeats 24 h).
1. Probe the model, append a row to probes.jsonl; on failure log the error verbatim, exit 75.
2. Create a clean detached worktree at the commit (fresh worktree has no bin, so the runner builds a fresh CLI; removes the stale-DLL risk).
3. Guard: parse the refreeze-162 lines, sha256 every listed file in the worktree; any difference exits 76 and the model stays held; also check Program.cs and the runner match the record.
4. If a dotnet test host or Avowry.sln process is running, exit 77.
5. Run `python scripts/run-llm-check.py --model <m> --out docs/usable-v1/llm-check-<m>.json --repo <worktree>`.
6. In a finally, write run-<m>.md (exit code, JSON fields, JSON sha256, CLI version, --model-command) and run-<m>.done ({outcome: passed|model-failed|harness-failed}); any .done stops retries. The facilitator commits the JSON byte-identical. A model-result failure: no second attempt at the same hashes. A harness failure: D236 repair, then one rerun.

Codex path: the seat plan launches Codex through the machine-local codex_nostore.py (Store-alias and TEMP stall fixes); the runner default for --model codex is plain `codex` as in the 2026-10-28 run; the launcher defaults to plain codex; on a harness error repair with --model-command "python <user>/bin/codex_nostore.py" and record the cause.

Tests (tests/test_llm_check_when_back.py, fakes only): planted-fail model command exiting 1 gives a model-failed .done; fake model command writing a canned reply; guard hash mismatch gives 76; probe failure 75; gate-busy 77; .done present gives 0 with no run. Smoke-test in a throwaway worktree before arming.

Scheduled task (Codex): New-ScheduledTaskAction with the full python path (bare `python` may not be on the task's PATH), argument `llm_check_when_back.py --model codex --card 162`, working directory the mob tooling; trigger Once at 2026-11-11 00:12, repetition every 30 minutes for 24 h; principal the interactive current user; settings WakeToRun, StartWhenAvailable, MultipleInstances IgnoreNew; task name avowry-162-codex. Unregister when .done appears.

## 7. Risks

- Both cheat sheet avows uncompiled: green 1 must run the CLI build and the generated specs, not only the PhraseCheatSheet compile test.
- Token margin 693 of 700: a cold reader fix may overflow it.
- Load-sensitive tests: BenchmarkHarnessAcceptance has a timed test sensitive to load; the launcher must not overlap a gate run; the sign-off gate runs alone.
- Freeze enforcement: CI protects the freeze only through R1 for content inputs; the launcher guard covers Program.cs and the runner.
- Eight Python tests fail in a clone (history-dependent): test_pattern_docs.py must not depend on git history.
- AV011 documented but no emitter found.
- No existing tests for the claims that AV200 covers a nested update, AV312 covers a field of the wrong type, and AV329 appears in the CLI: the red author must verify each by running the CLI once before writing the needle.
