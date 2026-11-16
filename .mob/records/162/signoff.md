SIGN-OFF

# Card 162 acceptance sign-off (sign-off seat, agy gemini-3.8-flash-medium, context ctx-162s; branch card-162-green2 at 1a9791d; seated by D279)

All three acceptance criteria pass on the repository files and are supported by commits, files, and tests that fail on partial or incorrect work. No repository file was modified by this seat.

## Criterion-to-evidence table

| Criterion | Proving evidence (commit, file, test) | Left for gate and CI |
|---|---|---|
| **162-1** [Docs & Re-freeze]<br>Given `docs/PATTERNS.md`, When merged, Then `docs/GRAMMAR.md` describes the ensure step and record update form; `docs/CHEATSHEET.md` has one short example of each (token cost reported before and after); re-freeze Decider records old/new sha256 of both docs with reason and baseline effect. | **Commits:** `1bcae82` (green 1 doc additions), `85e5b2c` (cold reader clarifications), `46179b4` (Decider D238 re-freeze record), `0a8ca1e` (gate review B M2: un-pends PatternDocs).<br>**Files:** `docs/GRAMMAR.md` (lines 143-160, EBNF, AV tables), `docs/CHEATSHEET.md` (lines 3, 55-73, 2771 chars = 693 tokens, cap 700), `docs/PATTERNS.md` (links to GRAMMAR sections), `docs/decisions/language-and-grammar.md` (D238 record).<br>**Tests:** Python `tests/test_pattern_docs.py` (all 10 needles G1-G8, S1, S2 pass); C# `PatternDocs.The_grammar_and_the_cheat_sheet_teach_the_ensure_step_and_the_record_update` (un-pended in `0a8ca1e`); characterisation tests `PhraseCheatSheet.Sheet_is_at_most_700_estimated_tokens`, `Sheet_names_every_keyword_and_core_type_and_its_examples_compile`, `EnsureStepCompiles`, `RecordUpdateFormCompiles`. | **Gate:** `dotnet test Avowry.sln -c Release --filter "Status!=Pending"` to execute `PatternDocs` and `PhraseCheatSheet` tests.<br>**CI:** Test job on PR and main post-merge. |
| **162-2** [LLM Customer Check Rerun]<br>Given changed docs, When LLM-customer check is rerun with Codex and Grok, Then `docs/usable-v1/llm-check-codex.json` and `llm-check-grok.json` carry new hashes and show passed, bar 6 stays met, check may use new steps. | **Commits:** `e2e5a4a` (Grok evidence), `f78588c` (Codex evidence), `70ab77e` (green 2: un-pends Bar6).<br>**Files:** `docs/usable-v1/llm-check-codex.json` (codex, passed=true, 13/13 specs, pins match new doc hashes), `docs/usable-v1/llm-check-grok.json` (grok, passed=true, 13/13 specs, pins match new doc hashes), `tests/Avowry.Acceptance.Tests/UsableV1.cs`.<br>**Tests:** C# `UsableV1.Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone` (un-pended); `UsableV1.Evidence_rule_V0`..`E6` (prove Bar6 validation against passing and mutated files); `python scripts/usable-v1-report.py --self-test` (passes). | **Gate:** Run `scripts/usable-v1-report.py` (verifies bar 6 passed) and `dotnet test Avowry.sln -c Release --filter "Status!=Pending"` (executes Bar6).<br>**CI:** `usable-v1` and `test` jobs. |
| **162-3** [CI Usable v1 6 of 6 on Main]<br>Given CI, When it runs after merge, Then the `usable-v1` job reports 6 of 6 criteria met on ubuntu (no stale evidence). | **Commits:** `70ab77e` (green 2: empties ratchet `suspended` to `{}`, un-pends `Z1` and `Z2`), `46179b4` (un-pends `R1`), `1a9791d` (gate review B M1: regenerates `MOB.md` to reflect `suspended: [none]`).<br>**Files:** `board/usable-v1-ratchet.json` (`must_pass: [1..6]`, `suspended: {}`), `tests/Avowry.Acceptance.Tests/UsableV1Closure.cs` (`Z1`, `Z2`, `R1`), `MOB.md`.<br>**Tests:** C# `UsableV1Closure.Z1_the_ratchet_has_no_suspended_bar`, `Z2_the_bar_6_test_is_not_pending`, `R1_the_refreeze_record_matches_the_files_the_check_runs_against` (all un-pended); report self-test handles `{}` suspension cleanly. | **Gate:** Full gate run with `dotnet test Avowry.sln -c Release --filter "Status!=Pending"`, `python scripts/usable-v1-report.py` (confirming "Usable v1: 6 of 6 criteria met", no warning).<br>**CI:** CI on `main` post-merge runs `ci.yml` (`mob-process`, `test`, `usable-v1`), with `usable-v1` reporting 6 of 6 criteria met on ubuntu. |

## Run lines

1. `python -m unittest tests.test_pattern_docs`
```
..........
----------------------------------------------------------------------
Ran 10 tests in 0.004s

OK
```

2. `python scripts/usable-v1-report.py --self-test`
```
## Usable v1: 6 of 6 criteria met

- MET: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry)
- MET: bar 2, One CLI command compiles it to C#/FastEndpoints
- MET: bar 3, dotnet build of the generated projects has zero warnings
- MET: bar 4, The generated xUnit specs all pass, one test per spec
- MET: bar 5, The app runs and live HTTP matches the specs
- MET: bar 6, A model that never saw the app adds an endpoint from the docs alone
USABLE v1 REACHED: demo app at demo/tasks (see demo/tasks/README.md for how to run it)
::notice::USABLE v1 REACHED
## Usable v1: 0 of 6 criteria met

- not met: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry) (x)
- not met: bar 2, One CLI command compiles it to C#/FastEndpoints (x)
- not met: bar 3, dotnet build of the generated projects has zero warnings (x)
- not met: bar 4, The generated xUnit specs all pass, one test per spec (x)
- not met: bar 5, The app runs and live HTTP matches the specs (x)
- not met: bar 6, A model that never saw the app adds an endpoint from the docs alone (x)
::error::Usable v1 REGRESSION: bar(s) [1, 2, 3, 4, 5, 6] previously passed (board/usable-v1-ratchet.json) and now fail
## Usable v1: 5 of 6 criteria met

- MET: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry)
- MET: bar 2, One CLI command compiles it to C#/FastEndpoints
- MET: bar 3, dotnet build of the generated projects has zero warnings
- MET: bar 4, The generated xUnit specs all pass, one test per spec
- MET: bar 5, The app runs and live HTTP matches the specs
- not met: bar 6, A model that never saw the app adds an endpoint from the docs alone (stale)
::error::Usable v1 REGRESSION: bar(s) [6] previously passed (board/usable-v1-ratchet.json) and now fail
## Usable v1: 5 of 6 criteria met

- MET: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry)
- MET: bar 2, One CLI command compiles it to C#/FastEndpoints
- MET: bar 3, dotnet build of the generated projects has zero warnings
- MET: bar 4, The generated xUnit specs all pass, one test per spec
- MET: bar 5, The app runs and live HTTP matches the specs
- SUSPENDED: bar 6, A model that never saw the app adds an endpoint from the docs alone (stale)
::warning::Usable v1 bar 6 is SUSPENDED, its evidence is stale: reason
USABLE v1 REACHED: demo app at demo/tasks (see demo/tasks/README.md for how to run it)
::notice::USABLE v1 REACHED (with suspended bars)
## Usable v1: 4 of 6 criteria met

- MET: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry)
- MET: bar 2, One CLI command compiles it to C#/FastEndpoints
- MET: bar 3, dotnet build of the generated projects has zero warnings
- MET: bar 4, The generated xUnit specs all pass, one test per spec
- not met: bar 5, The app runs and live HTTP matches the specs (x)
- SUSPENDED: bar 6, A model that never saw the app adds an endpoint from the docs alone (stale)
::error::Usable v1 REGRESSION: bar(s) [5] previously passed (board/usable-v1-ratchet.json) and now fail
self-test ok
```

## Named limits

1. **Freeze-window edit to informational bench file (Gate review B M3)**: `tools/Avowry.Bench/Program.cs` (an informational D238 freeze-window file for launcher guard) was touched during the freeze window by card 368 (`8d4ce9b`) and card 418 (`5617134`) before the Codex run. Evidence is unaffected because both Grok and Codex runs executed in a clean detached worktree at commit `46179b4` (where launcher guards passed), and the modifications only scrubbed diagnostics paths and blanked reply_head. Current sha256 (`2caac177...`) differs from D238 informational line (`0e9e91c2...`). To be recorded in the card retro.
2. **Light sign-off seat (no dotnet)**: In adherence to shared-laptop rules, no C# compilation or test suite was run in this seat (`no dotnet`). The C# tests (`PatternDocs`, `Bar6`, `Z1`, `Z2`, `R1`, `Evidence_rule_*`) are verified by code inspection and gate review B, and are executed at the integration gate under Avowry's heavy lock.
3. **M1 & M2 resolutions confirmed present at tip**: Gate review B minor findings M1 (`MOB.md` regenerated for `suspended: [none]`) and M2 (`PatternDocs.cs` un-pended) are committed at `1a9791d` and `0a8ca1e` respectively, forming the HEAD of `card-162-green2`.
4. **Gate review A non-blocking observations**: Findings L1 (seat provenance), L2 (`ensure` cheat sheet syntax nuance), and L3 (grammar failure code capitalization) remain documented in `gate-review-a.md` for retro consideration.
5. **Codex runner harness attempts**: As noted in `gate-review-b.md` (N1) and `codex-run-notes.md`, Codex required 3 harness repairs before its passing run; all earlier attempts aborted with zero output tokens and did not constitute model failures.
