# Proposal for Decider D246: the task-force declaration and card text for card 368 (new bench rows publish no reply prefix)

Facilitator, 2026-11-10 UTC, main at a5bfc8e. VOTE on this declaration: adopt the Ready card text of .mob/records/368/ready-body.md (the product manager seat's draft with the facilitator's rulings; the issue body is the same text) as card 368's Ready text, with the task force below. The card is size S. It changes no agreement text; Decider protocol step 6 (D224) applies with the D232 paragraph; the D228 preflight and the D234 evidence block apply; the seat plan cites .mob/records/368/probe-all-2026-11-10.jsonl (taken 18:50 UTC; available: agy, claude, cursor, grok, qwen; out: codex, copilot, muse until 2026-11-16).

STANDING-RULES PREFLIGHT: Card format rule (the three scenarios cover Z, O, M, B, I, E, S; each is titled with its letters); TDD red-first (every new-state id is watched failing on the current tree; the seven existing acceptance tests that pin the prefix are rewritten red-first as the changed behaviour: four masking tests and the cold-run check in BenchmarkHarnessAcceptance.cs, two masking tests in CSharpBaselineAcceptance.cs, plus the empty-reply test); D204: one red commit with the criteria as Pending traits, then the greens; D206 (record tests read committed blobs); D219/D226 step 0 in the gate review; D222 id lists in the red record; D234 evidence block; D236 live probe cited; D243 merge verification before every gate; D245 does not apply (nothing here changes how a seat starts). Consumer value: Dana reads bench rows that carry scores and no model reply text (the owner's ruling of 2026-11-08: trim for new rows only, pushed rows stay public).

## Facilitator rulings (main at a5bfc8e)

- Facts rerun by the facilitator by command: bench/results/history.jsonl has 336 rows, all with reply_head, sha256 6fcd386156464c405c34cc6e7ae68847e45213c63eac3e8647c3a8dfcc6f7202; bench/csharp-baseline/results/history.jsonl has 120 rows, all with reply_head, sha256 525b4574e4f8c87fac364cd200b305de947ebb4b51efbca1449699116aafbac3. The slice, the red and the gate reviewer recompute every figure.
- Ruling 1, scope: both writers (Avowry.Bench and Avowry.BenchCs), the private holdout path and the tracked per-model files are in scope; reply_head may be absent or empty and the tests accept either; no pushed row or file is rewritten.
- Ruling 2, the seats: read-only architect slice by a fresh Claude context (the only plan slot Claude takes); the RED by a Grok WRITING seat through grok_write.py in its own linked worktree (Grok passed its live probe; a Claude stopgap is not needed); the GREEN by a Cursor WRITING seat through cursor_write.py in its own linked worktree with the changes left uncommitted and the facilitator committing them (the hook refuses a seat trailer); red and green on different seats; the facilitator runs every gate command himself (isolated clone baseline gate with D243 merge verification), a second read-only review (agy), an independent gate reviewer (a fresh Claude context) and a fresh sign-off. A takeover by the facilitator is logged and, when it changes production code, comes with its failing test committed first.
- Ruling 3, every seat brief repeats: keep each file's existing line endings; never type, echo or write the local account name or a home path; no recursive delete; no commit trailer. The seat events that get committed must pass the scan.
- Ruling 4, the retro (seven voices at once) is due after the sign-off; the idle-minutes and Dispatch-nudge lines, the Evidence (per card) section and the list of drivers sidelined for behaviour are required (D230, D234, D236).
- Ruling 5, the bootstrap-phase evidence that rests on a non-empty reply_head is replaced for new rows by workdir_empty_at_launch; the retro records it; nothing else changes.

## The card (verbatim)

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
