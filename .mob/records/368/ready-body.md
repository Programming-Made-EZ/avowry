<!-- phrase-card:368 -->

- **Status:** Ready
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
     Given bench/results/history.jsonl with its 336 pushed rows and bench/csharp-baseline/results/history.jsonl with its 120, whose SHA-256 values are 6fcd386156464c405c34cc6e7ae68847e45213c63eac3e8647c3a8dfcc6f7202 and 525b4574e4f8c87fac364cd200b305de947ebb4b51efbca1449699116aafbac3 (recorded at Ready, 2026-11-10, commit dae063b)
     When each bench appends a new row after this change
     Then the pushed lines still hash to the recorded values, and the stored-result reader reads every old row and the new row with the model, task, passed and status that the line holds
- **ZOMBIES:** Z empty reply (2); O one run (1); M two benches times four endings (2); B a reply past the old 200-character cut (1); I row shape and the stored-result reader (1, 3); E model error after an earlier reply (2); S a plain passing run (1).
- **Rulings (facilitator, 2026-11-10):** the private holdout path is in scope (the writers share one code path; nothing may keep a reply); the tracked per-model files are in scope; reply_head may be absent or empty and the tests accept either; the bootstrap-phase evidence that rests on a non-empty reply_head is replaced for new rows by the existing workdir_empty_at_launch field and is noted in the retro, not changed here; persona Dana stays.
- **Not in this card:** Rewriting, trimming or deleting any pushed row or file (the two history files, the self-check fixture history, the invalid folder, the pushed model files); seat replies under .mob/records; a new source of reply evidence; changes to scripts/check-bench-history-clean.py.
- **Size:** S

## Context

- Filed by the facilitator on 2026-11-08 UTC from the product owner's inbox entry on the nine owner items.
- Today both writers keep a masked prefix of up to 200 characters (CreateReplyHead in Avowry.Bench, ReplyHead in Avowry.BenchCs) and write it to results/<model>.json and to history.jsonl. Avowry.BenchCs also keeps an earlier attempt's reply on its exception path.
- Seven acceptance tests assert the current prefix and change with this card: four masking tests and the cold-run check in BenchmarkHarnessAcceptance.cs, and two masking tests in CSharpBaselineAcceptance.cs. The cold run already records workdir_empty_at_launch, which can carry that evidence instead.

## Done when

    dotnet test Avowry.sln --filter "Status!=Pending"
