# Card 162 Ready draft (product manager seat, a Grok seat, read-only), kept for when the card is pulled

The seat's reply, committed verbatim on 2026-11-08 as the input of re-rank 31 (.mob/records/reranks/2026-11-08-31.md). Its conclusion (section 4 and section 6): card 162 cannot be pulled before Codex is back (about 2026-11-11 12:10) because criterion 2 needs the Codex and Grok pair and the runner accepts only those two models. When the card is pulled, the facilitator reruns the facts of section 1, re-probes the drivers, checks the Codex clock, and declares the task force through a Decider as usual.

# Card 162 Ready (product manager, read-only)

## 0. STANDING-RULES PREFLIGHT

- **D137.** One Pending test: existing `Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone` (already Pending from card 199). The red extends that body (or a python module it calls) with C1–C3 checks. The green removes only `[Trait("Status", "Pending")]`. Python body identical between red and green.
- **D154 / D157.** Red record stores pre-change commit, command, failing test, expected assertion. Gate replays that red on the commit before the first production change and lists Criterion order from `git log`.
- **D204.** C1–C3 are NEW-STATE. One Pending red, then one atomic green `Criterion: 1-3`. Red record has hash, command, test, expected failure.
- **D206.** Cold-reader at the gate reads only `docs/GRAMMAR.md` and `docs/CHEATSHEET.md` as Ari. Story-promise check is section 3. Live-state guard reads the working tree. Hash/history claims use `git show <sha>:path` and an exact key.
- **D217.** This Ready proposal does not change the working agreement. If the primary window has no No and no amendment: trigger (d), no separate re-run. An amendment uses the confirmation step. The later D72 re-freeze Decider is a separate window during the card.
- **D219.** Independent gate review starts on the final tip after the cold-reader report is committed (and after any page revision it caused). Record the `git merge-base --is-ancestor` line.
- **Process-start callers.** `UsableV1.cs` is already in `ProcessStartCallers` (`GeneratedApiSourceScan.cs:22–29`). A new C# wrapper must be named in the slice and added to that list in the **red**. No new wrapper if Bar6 stays the only C# test.
- **Card format rule.** Connextra with Ari; Gherkin titled with ZOMBIES letters; three criteria, each NEW-STATE, each with a named test and pre-change failure.
- **Value rule.** Ari (PERSONAS.md) reads the grammar and cheat sheet; Usable v1 bar 6 is met again for the v0.1 preview.
- **350-line python.** Any new `tests/test_pattern_docs.py` aims at ≤350 lines; fenced-block regex and substring needles (style of `tests/test_demo_readme.py`); no grammar parser.
- **Spike rule.** Does not apply (not a spike).
- **Red-before-green (permanent).** Watched red before any production edit of GRAMMAR.md, CHEATSHEET.md, evidence JSON, or the ratchet.

## 1. PREMISE FACTS

**Story still true.** `docs/PATTERNS.md:7–53` documents the ensure step (`ensure title is not blank else EmptyTitle`, condition vocabulary, `else` failure) and the record update (`let completed = task with { done: true }`). `docs/GRAMMAR.md:85–88` EBNF is `let_step | respond_step | fail_step | avow_call` with `let_step = "let" , ident , "=" , avow_call` — no `ensure_step`, no `with { }` update. Core keywords (`GRAMMAR.md:31`) do not include `ensure`. The table at `GRAMMAR.md:146` is the old controlled-vocabulary verb “Guard”, not the shipped step. Cheat sheet (`CHEATSHEET.md:42–53`) has an avow named `"ensure {ok: Bool} holds"` with a native block; no `ensure … else` and no `with {` update.

**Rename amendments are done (stale on the card).** Product/command/extension are Avowry / `avowry` / `.avowry` (`GRAMMAR.md:1,35,251`; `CHEATSHEET.md:1,7`; `orders.avowry:11` is `avow to`). Card 199 Done (D131); card 193 Done (D132); card 160 Done (D108). `Phrase.sln` is `Avowry.sln`. Depends-on 160, 199, 193 are closed.

**D72 freeze.** `docs/decisions/language-and-grammar.md:56–60`: freeze CHEATSHEET.md, GRAMMAR.md, and orders with recorded sha256; any change needs a new Decider and a fresh baseline. D101 re-froze GRAMMAR only (`0d78e599…` → `ad0f6e51…`); CHEATSHEET and orders byte-identical then. D127 (`process-and-staffing.md:140`): 162 is the single final re-freeze: fold the pattern steps, rerun bar 6, un-pend the hash-binding test, ratify hashes in one Decider. D131 (`compiler-and-adapters.md:238–241`): card 199 already edited the frozen docs; the hash-binding test was marked Pending; bar 6 was suspended instead of dropping the ratchet.

**Bar 6 and evidence.** `board/usable-v1-ratchet.json:3–13`: `must_pass` 1–6; `suspended.6` = card 162 reruns the LLM check, ratifies hashes, removes the suspension. Evidence `docs/usable-v1/llm-check-codex.json` and `llm-check-grok.json` (2026-10-28, D103): `passed: true`, 13/13 specs, 0 warnings, `grammar_sha256` `ad0f6e51fe1d61968fecb7ed3609bb00b204f3aa482defcf38f6fd6f6728dc9f`, `cheatsheet_sha256` `fe3d8a8b37f4b5bfe2c95e9a38bd9b59de5c57fa163c2c4526ef55d5c1300ae3`. Current file hashes (python sha256 of bytes): GRAMMAR.md `9a5dc03e003297cf8384dc52bdfe69c4aa7fad254349f43c86e72700293124a2`; CHEATSHEET.md `4b1b9ef51341b814616e8aadbe026836e72f254d74b3ce7dba1e6c70105c6d9e`; `orders.avowry` `4d7a24b9877a79b43b308d5052cb3a807af0dd88465399b9fc639958a4f637f8`. Both evidence hashes are stale.

**What Bar6 checks** (`UsableV1.cs:101–115`): every `docs/usable-v1/llm-check-*.json`; `passed` true; `specs > 0`; `specs_passed == specs`; `build_warnings == 0`; hashes match the working-tree documents. Pass rule is `failures.Count < files.Length` (at least one current passing file). Trait `Status=Pending` excludes it from `dotnet test … Status!=Pending`; the usable-v1 job (`ci.yml:87` → `usable-v1-report.py:147–148`) filters only `Milestone=UsableV1`, so Bar6 still runs, fails on stale hashes, and is reported SUSPENDED.

**Runner** (`scripts/run-llm-check.py:3–5,214–215,558–562`): `--model` required, `choices=("codex", "grok")` only; default commands `codex` / `grok`; optional `--model-command`, `--out` (default `docs/usable-v1/llm-check.json`), `--timeout` default 600. Cold copy of GRAMMAR.md + CHEATSHEET.md + current `Tasks.avowry`. Totals JSON fields (`423–453`): `model`, `date`, `attempts`, `tokens_in`, `tokens_out`, `passed`, `proof_level`, `specs`, `specs_passed`, `build_warnings`, `grammar_sha256`, `cheatsheet_sha256`; no reply/prompt/task text. Invoke:

```
python scripts/run-llm-check.py --model grok --out docs/usable-v1/llm-check-grok.json
python scripts/run-llm-check.py --model codex --out docs/usable-v1/llm-check-codex.json
```

**Driver-pair decision.** Bar 6 intent (D103, `UsableV1.cs:12–15`, `usable-v1-report.py:29`): a model that never saw the app adds an endpoint from those two docs; D103 used two models for independence, “not a general claim.” C2 names Codex and Grok. The runner cannot name Claude/Muse/Cursor. Recording another CLI as `--model grok` would falsify the JSON. TWA driver-availability (`TEAM_WORKING_AGREEMENT.md:353`): a frozen measurement pair is not changed to work around an outage. **Keep Codex and Grok. Do not substitute. Wait for Codex until about 2026-11-11 12:10.** Grok may be run whenever; both files must be current for C2.

**Token cap.** `PhraseCheatSheet.cs:191`: `ceil(length/4)`, limit 700. Current CHEATSHEET.md is 2070 bytes ≈ 518 estimated tokens (formula only; C# test not run).

**Unverified (no dotnet, no model run).** Whether Codex or Grok still pass on current or folded docs; exact token count from the C# test; Codex reset clock. Facilitator: the two `run-llm-check.py` commands above after the docs exist; `dotnet test tests/Avowry.Acceptance.Tests --filter FullyQualifiedName~PhraseCheatSheet.Sheet_is_at_most_700`; `python scripts/usable-v1-report.py`; `dotnet test Avowry.sln --filter "Status!=Pending"`.

## 2. THE CARD TEXT

```markdown
<!-- phrase-card:162 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** docs (M; D72 re-freeze; Usable v1 bar 6; v0.1 bar 3)
- **Repository:** avowry (`docs/GRAMMAR.md`, `docs/CHEATSHEET.md`, `docs/usable-v1/llm-check-codex.json`, `docs/usable-v1/llm-check-grok.json`, `board/usable-v1-ratchet.json`, `tests/Avowry.Acceptance.Tests/UsableV1.cs`; optional `tests/test_pattern_docs.py`)
- **Source:** cards 159/160 (D107/D108); D72; D103; D125–D127; D131
- **Depends on:** 160 (Done), 199 (Done), 193 (Done)
- **Story:** As Ari, I want the grammar and the cheat sheet to describe the ensure step and the record update form, so that a cold model can use them from the documentation alone.
- **Observes:** GRAMMAR EBNF and prose for the ensure step (condition vocabulary and `else` declared failure) and the record update (`let x = rec with { field: value }`); cheat sheet one short example of each, still ≤700 estimated tokens; both llm-check JSON files passed against the new hashes; ratchet has no bar 6 suspension; Bar6 not Pending.
- **Advances:** Usable v1 bar 6 (lift the D131 suspension); v0.1 bar 3 (patterns in the grammar and cheat sheet). Plumbing for those bars; hypothesis form exempt.

- **Criterion 1 (NEW-STATE):** `docs/GRAMMAR.md` describes the ensure step as in `docs/PATTERNS.md` (conditions: blank / not blank, length at most N, Int comparisons; `else` named failure on the containing avow’s `fails with`) and the record update form (`let name = record with { field: value }`, copy not mutate). EBNF `step` includes both. `docs/CHEATSHEET.md` has one short fenced example of each; estimated tokens (`ceil(chars/4)`) reported before and after and stay ≤700. A re-freeze Decider records old and new sha256 of GRAMMAR.md, CHEATSHEET.md, and `orders.avowry` (ratify current orders hash if that file is unchanged), the reason, and that the scored cheat-sheet arm is a new baseline labelled by document hash. Named test (shared with 2 and 3): `Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone`, with live-state needles in that test or `tests.test_pattern_docs`. Pre-change failure: EBNF has no ensure/`with` update; cheat sheet has only the native avow `"ensure {ok: Bool} holds"`.

- **Criterion 2 (NEW-STATE):** facilitator reruns `scripts/run-llm-check.py` with `--model grok` and `--model codex` (the only `choices`) against the folded docs; `docs/usable-v1/llm-check-grok.json` and `llm-check-codex.json` are totals-only, `passed: true`, `proof_level: specs-passed`, `specs_passed == specs > 0`, `build_warnings: 0`, hashes equal the new document sha256s. The prompt still asks for GET `/tasks/open`; it says Avowry/`avow to` (not Phrase) and the run exercises `avow to` and a call that wrongly starts with `to` (D126). Same named test. Pre-change failure: evidence hashes `ad0f6e51…` / `fe3d8a8b…` do not match current `9a5dc03e…` / `4b1b9ef5…`.

- **Criterion 3 (NEW-STATE):** `Bar6_…` has no `Status=Pending`; `board/usable-v1-ratchet.json` has no `suspended.6`; `python scripts/usable-v1-report.py` prints 6 of 6 met and `USABLE v1 REACHED` with no suspended-bar warning. Same named test. Pre-change failure: Pending trait present; `suspended.6` present; report lists bar 6 SUSPENDED.

Named test: extend the existing Pending `UsableV1.Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone` (`Slice=138`, `Milestone=UsableV1`) so one aggregated assertion covers C1 needles, C2 evidence, and C3 ratchet. Optional python helper, live-state, working tree. Red adds every check; green removes only Pending.

Scenario: [Z] docs before the fold
  Given the tree before this card’s green
  When the named test runs
  Then it fails by assertion: no ensure-step / record-update needles, evidence hashes stale, bar 6 still suspended

Scenario: [O,S] one ensure and one update
  Given Ari has only GRAMMAR.md and CHEATSHEET.md
  When Ari writes a guard and a one-field copy
  Then the grammar states `ensure … else Failure` and `let x = rec with { field: value }`, and the cheat sheet shows one short example of each

Scenario: [M] both evidence files
  Given the folded documents
  When Codex and Grok each run the card 137 runner once, cold
  Then both JSON files passed with the new hashes and 13 or more specs, 0 warnings

Scenario: [B] token cap and exact hashes
  Given the cheat sheet after the examples and the evidence files
  When tokens are estimated as ceil(chars/4) and Bar6 reads hashes
  Then tokens ≤ 700 and both sha256 fields match the files byte-for-byte

Scenario: [I] the check’s interface
  Given the runner copies only GRAMMAR.md and CHEATSHEET.md plus Tasks.avowry
  When a cold model adds GET /tasks/open
  Then it sees `avow to` (not the old phrase keyword) and can use the new steps; a name that starts with `to` is a name, not a filler

Scenario: [E] missing else, bad update, failed run
  Given an ensure without a declared failure, a `with` of an unknown field, or a model reply that does not build
  When Ari follows the grammar or the runner classifies the reply
  Then the grammar names those as errors; a failed check writes `passed: false` and a reason class, totals only

ZOMBIES: all letters apply.

- **Not in this card:** changing `run-llm-check.py` `--model` choices; substituting Claude/Muse/Cursor for Codex; card 166’s source-map wording (`GRAMMAR.md:261` `[Avow("...")]`); card 143 split sheet; store operations (145); rewriting `orders.avowry` examples onto the ensure step; requiring the hidden open-tasks task to use the new steps; PATTERNS.md rewrite; Windows CI (177); PERSONAS.md Phrase wording (204).

## Done when

```
python scripts/usable-v1-report.py
dotnet test Avowry.sln --filter "Status!=Pending"
```
```

## 3. THE STORY-PROMISE CHECK (D206)

Promise: Ari, from GRAMMAR.md and CHEATSHEET.md alone, uses the ensure step and the record update; bar 6 is honestly green for the v0.1 preview.

| Ari must find | Cover |
|---|---|
| Ensure step syntax, conditions, `else` failure | C1 [O][E] |
| Record update copy-with-fields | C1 [O] |
| One short example of each on the cheat sheet | C1 [O][B] |
| Cheat sheet still prompt-sized (≤700) | C1 [B] |
| Docs enough for a cold add-endpoint (bar 6) | C2 [I][M] |
| `avow to`; a call starting with `to` | C2 [I] |
| Two independent passing evidence files, current hashes | C2 [M][B] |
| CI 6 of 6, no suspension, Bar6 not Pending | C3 |
| Source-map vs `[Avow]` attribute | Not-in-this-card (166) |

## 4. SCOPE AND SLICE

SHIP AS IS: one concern (fold patterns, re-freeze, restore bar 6), three criteria, three increments. Do not pull until Codex is back (~2026-11-11 12:10) unless a Decider splits or changes the pair (WIP-of-one).

1. Characterisation on the pre-change tree: `Sheet_is_at_most_700_estimated_tokens` still passes; record current sha256s (section 1). Not a fourth criterion.
2. One Pending red: extend Bar6 with C1–C3 checks; watched failing for missing needles, stale hashes, suspension. Red record complete. Neighbouring tests before commit: `PhraseCheatSheet`, `tests.test_run_llm_check`, `tests.test_live_docs_266`.
3. One atomic green `Criterion: 1-3`: fold PATTERNS into GRAMMAR + CHEATSHEET; facilitator runs Grok then Codex; commit both JSON files; delete `suspended.6`; remove only Pending; re-freeze Decider records old/new hashes and the new baseline. LLM rerun is not code: facilitator, after the docs are in the working tree, before that green is pushed.

Caps: characterisation zero green cycles; C1–C3 one atomic green. Hostile inputs: restyle without the step syntax; one evidence file updated and one left stale; ratchet key left in place.

## 5. TASK-FORCE DECLARATION

- **Ready (PM):** Grok (this proposal). Domain: Ari’s docs promise and bar 6 restoration.
- **Slice (architect):** fresh Claude, read-only. Reason: PM is Grok; Codex out until about 2026-11-11 12:10; Copilot no quota; agy rate-limited ~134 hours. Domain: D72 re-freeze, one-Pending Bar6 extension, callers list.
- **Red (QA):** Grok, fresh. Never the green’s seat. Characterisation first, then the one Pending red; neighbouring tests before commit.
- **Green:** fresh Claude; never the red’s seat. Docs + evidence files the facilitator produced + ratchet + un-pend.
- **Cold-reader (D206, not a task-force role):** Muse (trial; low-stakes). Fallback Cursor. Reads only `docs/GRAMMAR.md` and `docs/CHEATSHEET.md` as Ari; writes `.mob/records/162/cold-reader.md`. Gate and sign-off are not trial-driver seats.
- **Independent gate reviewer:** fresh Claude. Replay the Pending red on the commit before the first production change; Criterion order from `git log`; starts on the final tip (D219).
- **Fresh sign-off:** a separate fresh Claude context that does not read the gate report before its own reruns.
- **LLM-check run:** facilitator, not a task-force seat. Why: D103 and card 137 already make the real model run a facilitator measurement on committed (or about-to-commit) docs; the runner supplies the cold isolation; a “check runner” seat would duplicate the facilitator and blur author/reviewer. Takeovers logged.

Left out: tech-writer (cold-reader + promise check); security (docs and JSON evidence); extra backend; delivery lead; language designer (syntax already shipped in 159/160). Domain-expert question: no.

## 6. RISKS AND WHAT YOU COULD NOT VERIFY

- **Wait for Codex** until about 2026-11-11 12:10. Do not split (D127 one Decider; a docs-only green that un-pends turns bar 6 red). Do not change the pair without a Decider plus a runner `choices` change. Grok+Claude would match bar 6’s cold-model intent; the repo does not support recording that pair today, and the outage rule forbids rewriting the frozen pair to dodge quota.
- Leaving a stale `llm-check-codex.json` beside a fresh Grok file would still let Bar6 pass (`failures.Count < files.Length`). C2 forbids that.
- Card 166 (Ready, depends on 162) expected to share this re-freeze for `GRAMMAR.md:261`. Folding it mixes concerns. After 162, 166 needs its own re-freeze or a Decider to amend “single final.”
- Cheat-sheet examples must stay under ~728 characters of headroom (518 → 700). `Sheet_names_every_keyword_and_core_type_and_its_examples_compile` will compile every new fence.
- Prompt.md still says “Return only valid Phrase source.” Slice updates it with C2.
- Did not run dotnet or a model. Facilitator commands in section 1. Brief’s quota facts were not re-probed here; re-probe before pull.

---

## Added from the card 361 sign-off (2026-11-09 UTC): what card 162's Ready step and fold must include now that the codes are AV
- Regenerate `grammar_sha256` in `docs/usable-v1/llm-check-codex.json` and `llm-check-grok.json` (pinned `ad0f6e51…`; GRAMMAR.md hashed `88746d6b…` before card 361 and `8e4f91bd…` after, so the pins were already stale); bar 6 (Pending) needs that.
- Add one sentence to `docs/GRAMMAR.md` stating that diagnostic codes use the `AV` prefix (the table header is just "Code").
- The fold's rows PH020, PH337 and PH340 are now AV020, AV337 and AV340 (and AV341 from card 203); the draft's older PH names are history.
- A one-line "formerly PH" pointer for readers of old retros and decision records (index note), or a doc card for it.
