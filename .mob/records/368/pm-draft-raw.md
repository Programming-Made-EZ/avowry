The card's premise holds, but the draft misses one writer and one output. There are two bench history writers, not one. Each saves the reply prefix in two places: a line appended to the history file and a per-model file that is rewritten on every run. Every row already pushed carries `reply_head`.

## 1. Key facts I verified

**The writers today**
- `tools/Avowry.Bench/Program.cs` is the Phrase bench. `CreateReplyHead` (lines 803–808) masks secret-looking tokens and paths, then cuts the reply to 200 characters. `PersistResult` (lines 813–829) writes `results/<model>.json` and appends the same JSON line to `results/history.jsonl`, or to the private path for holdout runs. It sets the field on three paths: a completed run (line 343), an empty reply (line 321), and an exception, which passes an empty string (line 356).
- `tools/Avowry.BenchCs/Program.cs` is the C# baseline bench, with its own copy, `ReplyHead` (lines 836–841). `Persist` (lines 848–862) does the same two writes. `CreateResult` passes the reply on every ending: passed, failed, empty reply and exception (lines 153, 170, 181, 189–190). On the exception path the reply can be an earlier attempt's text, so in this writer text leaks even when the run ends in an error.
- Both writers serialize with snake_case, which is where the `reply_head` key comes from. The stored-result reader (`BenchmarkResults.ReadStoredResult`, line 1481) defaults `ReplyHead` to an empty string, so a row without the key would still read. No other reader needs the field: `scripts/run-llm-check.py` (lines 398–408) uses the status of the last row, and `scripts/check-bench-history-clean.py` only scans the text.

**Rows already pushed**

Git status shows `bench/` clean, with `main` level with `origin/main`.

| File | Rows | Rows with `reply_head` | Empty values |
| --- | --- | --- | --- |
| `bench/results/history.jsonl` | 336 | 336 | 5 |
| `bench/csharp-baseline/results/history.jsonl` | 120 | 120 | 0 |
| `bench/results/self-check/history.jsonl` (fixture model) | 120 | 120 | 0 |
| `bench/results/invalid/superseded-rows-2026-10-26.jsonl` | 53 | 53 | 53 |

- The newest row in each of the two main files carries model code in `reply_head`: line 336, label `head2head-dev-r5`, and line 120, label `baseline-094-r5`.
- Of the per-model files, only `bench/results/agy-gemini-4-argon-medium.json` and the self-check fixture file have `reply_head`. The grok, codex and copilot files do not, but any future run rewrites them with it.
- `docs/release/pre-publication-checks.md` line 114 records the owner's ruling: "trim for new rows only; pushed rows stay public (card 368)".

**Tests that cover the writers**
- `tests/Avowry.Acceptance.Tests/BenchmarkHarnessAcceptance.cs` asserts the current behaviour, which this card reverses:
  - four tests that check masking and the 200-character cut (lines 147, 160, 170, 180), read through `RunAndReadReplyHead` from the per-model file (line 1474);
  - the empty-reply test, which expects `reply_head` to be empty (line 716);
  - the cold-run test, which uses `reply_head` to prove the working folder started empty (lines 1001–1004).
- `tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs` has two masking tests (lines 437 and 451).
- Related guards that are not writer tests:
  - `PrivateHoldoutResults.cs` line 169: the published holdout summary has no `reply_head`.
  - `LlmCustomerCheck.cs` line 178 and `tests/test_run_llm_check.py` line 127: the totals JSON has no `reply_head`, which is already enforced in `scripts/run-llm-check.py` line 26.
  - `tests/test_bench_history_clean.py`: tests the path and user-name scanner, not the writer.

## 2. Revised card

```markdown
# New bench rows no longer publish a reply prefix

- **Status:** Draft
- **Shape:** Story
- **Type:** story
- **Repository:** avowry (tools/Avowry.Bench, tools/Avowry.BenchCs; bench/results and bench/csharp-baseline/results)
- **Source:** product-owner answers to the nine owner items on docs/release/pre-publication-checks.md, inbox entry of 2026-11-08 (card 186): "trim for new rows only; pushed rows stay public". Ranked before the v0.1 publish card.
- **Depends on:** none
- **Story:** As Dana, I want each new bench result Avowry publishes to carry its scores and no part of the model's reply, so that I can read and cite the published benchmark evidence without it republishing vendor model output whose terms nobody has reviewed.
- **Consumer value:** From this card on, every bench row and model file Dana reads shows passed, attempts, tokens, status and proof level, and none of the model's reply text.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Scenario: [O][S][B] a completed run saves its scores and no reply text
     Given the Phrase bench runs one task and the model replies with a passing program longer than 200 characters that starts with a known marker
     When the bench saves the result
     Then the new line in results/history.jsonl and the file results/<model>.json both have reply_head empty or absent, the marker appears nowhere in either, and model, task, passed, attempts, tokens_in, tokens_out, status, proof_level and label hold the values the run produced
  2. Scenario: [M][Z][E] both benches, every way a run can end
     Given the Phrase bench and the C# baseline bench, each run four times: a passing reply, a reply that fails on every attempt, an empty reply, and a model that exits with an error after an earlier attempt replied with a known marker
     When each bench saves its rows
     Then none of the eight history lines or model files contains any part of any reply, reply_head is empty or absent in every one, and status and error still read completed, completed, unavailable/"empty reply" and unavailable/<the error> as before
  3. Scenario: [I] pushed rows stay and still read
     Given bench/results/history.jsonl with its 336 pushed rows and bench/csharp-baseline/results/history.jsonl with its 120, whose SHA-256 is recorded on this card at Ready
     When each bench appends a new row after this change
     Then the pushed lines still hash to the recorded values, and the stored-result reader reads every old row and the new row with the model, task, passed and status that the line holds
- **ZOMBIES:** Z empty reply (2); O one run (1); M two benches times four endings (2); B a reply past the old 200-character cut (1); I row shape and the stored-result reader (1, 3); E model error after an earlier reply (2); S a plain passing run (1).
- **Not in this card:** Rewriting, trimming or deleting any pushed row or file (the two history files, the self-check fixture history, the invalid folder, the pushed model files); seat replies under .mob/records; a new source of reply evidence; changes to scripts/check-bench-history-clean.py.
- **Size:** S

## Context

- Filed by the facilitator on 2026-11-08 UTC from the product owner's inbox entry on the nine owner items.
- Today both writers keep a masked prefix of up to 200 characters (CreateReplyHead in Avowry.Bench, ReplyHead in Avowry.BenchCs) and write it to results/<model>.json and to history.jsonl. Avowry.BenchCs also keeps an earlier attempt's reply on its exception path.
- Seven acceptance tests assert the current prefix and change with this card: four masking tests and the cold-run check in BenchmarkHarnessAcceptance.cs, and two masking tests in CSharpBaselineAcceptance.cs. The cold run already records workdir_empty_at_launch, which can carry that evidence instead.

## Done when

    dotnet test Avowry.sln --filter "Status!=Pending"
```

## 3. Open points for the facilitator

- **Holdout rows:** the draft also removes the prefix from rows written to the private holdout path; confirm, or exempt that path, since it is never published.
- **Per-model files:** the draft brings `results/<model>.json` into scope because those files are tracked and rewritten on every run; confirm.
- **Absent or empty:** the draft accepts either; the architect should pick one for both writers. The reader tolerates both.
- **Hashes for criterion 3:** someone outside read-only mode must record the two SHA-256 values at Ready; I could not run commands.
- **Bootstrap evidence:** `docs/decisions/bootstrap-phase.md` uses "every completed row has a non-empty `reply_head`" as evidence the benchmark is valid. That evidence lapses for new rows, so the mob needs a replacement signal, perhaps on a follow-up card.
- **Persona:** I chose Dana. If the value is really the owner's publication risk, the sponsor persona in `PERSONAS.md` is the alternative actor.
- **Where to edit:** the board file is generated, so the revised text goes into the issue: [issue #368](https://github.com/Programming-Made-EZ/avowry/issues/368).
e. The Slice may want a narrower `FullyQualifiedName~BenchmarkHarness|FullyQualifiedName~CSharpBaseline` command for the red/green loop.
- **Unchecked:** I could not run git here, so the date of the last commit to each history file is not verified.
