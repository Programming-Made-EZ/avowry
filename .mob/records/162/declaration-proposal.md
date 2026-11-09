# Proposal for Decider D237: the task-force declaration and card text for card 162 (fold the pattern steps into the grammar and cheat sheet; re-freeze; rerun the LLM check with Codex and Grok)

Facilitator, 2026-11-09 UTC, main at d45c40c. VOTE on this declaration: adopt the card text below (the product manager seat's Ready draft with the facilitator's rulings) as card 162's Ready text, with the one-card WIP exception of ruling 2 and the task force of the rulings block and the draft's section 7: read-only architect slice; red by a fresh Claude QA context in its own worktree; green 1 (the fold) by a Cursor WRITING seat; green 2 (the Codex evidence, the un-pending and the suspension removal) by a fresh Claude context; a Muse cold reader; the re-freeze Decider record; Grok's check run as the first thing Grok does after the re-freeze record, Codex's at its reset by the scheduled launcher; independent gate reviewer and sign-off two further fresh Claude contexts.

STANDING-RULES PREFLIGHT: TDD red-first hard rule, D137 against D204 (ruled as on cards 361 to 371), D204 characterisation up front, D206, D219 as tuned by D226, D222 (the id lists G, S, V, E, Z, R), the Card format rule, the Value rule (Ari reads only the grammar and the cheat sheet), D72 and D127 (one re-freeze), D131 (the bar 6 suspension), CODING_GUIDELINES, D234 (the standing evidence block below), D236 (this seat plan cites a live probe taken within 60 minutes: .mob/records/162/probe-all-2026-11-09.jsonl; Grok is not seated here because the product owner holds it for its check run: a recorded cause), the product owner's decision of 2026-11-09 (wait for Codex and Grok; no swap-ins; each model's check run is the first thing it does; wired automatically). Decider protocol step 6 (D224) applies with the D232 paragraph; this proposal changes no agreement text. The 30 percent rule and the strict reading of a No are unchanged. The D228 preflight applies to this window.

STANDING EVIDENCE OF THE PRACTICE (D234; the committed block, quoted):

STANDING EVIDENCE OF THE PRACTICE (committed template; every line can be checked with git; included in every task-force declaration proposal):
1. Review records. For every card the cold-reader report (a docs card), the second read-only review (when run), the independent gate review (opening with the step 0 line: the `git merge-base --is-ancestor` command, the tested commit and its exit status, and listing the files and criteria it checked) and the sign-off are committed under `.mob/records/<card>/`, and the closure record `.mob/records/<card>.json` names the full commit hashes of the ship and signoff seats (the closure check refuses a card without them).
2. Separate sessions. The closure record lists each seat with its own context label (Claude ctx-N, all distinct; the green never the red's), each seat's work is its own commit, and the red and the green work in their own worktree branches. The decider seats' launches are lines of `.mob/records/seat-use.jsonl` (seat, role, context, time). No claim of model isolation is made: separate sessions, worktrees and distinct commits are the evidence.
3. Red-first order. The characterisation and red commits precede the green in git; the gate review replays the red on the pre-change tree and the green changes the red files by the Pending traits only.
4. Idle minutes and Dispatch nudges are two committed lines in each retro (D230).
5. Per card, not only in general. Each card's retro document carries a section `Evidence (per card)` that lists the full commit hashes of that card's review records (the cold-reader report, the second review when run, the gate review, the sign-off) and of its red and green commits, copied from the closure record `.mob/records/<card>.json`, so that the block's lines 1 to 4 are instantiated for the card in git and checked by `git show` on those hashes; the declaration of the next card quotes the previous card's section as the example of the evidence it will produce.

The previous card's per-card evidence (the example the next declaration quotes): section 9 of docs/retros/card-370-2026-11-09.md, which lists the full hashes of card 370's Ready draft, slice, red (Cursor writing seat), greens (Muse writing seat), gate review and sign-off.

The draft (verbatim, with the facilitator's notes first):

# Card 162 proposal (product manager seat, ready; reply-only; a Cursor seat on Probation rung 1, its seventh real seat)

The seat's reply (Cursor in ask mode, through cursor_reply.py, a fresh context; read-only, it ran nothing), committed verbatim below the facilitator's notes. This file also carries the earlier kept Ready draft and the card 361 sign-off notes in .mob/records/162/taskforce-proposal-pm.md (the earlier text is replaced by this one; git history keeps it).

## Facilitator notes and rulings (2026-11-09 UTC, main at d45c40c)

- The hashes, the token count and the line citations of the draft are carried from earlier reads (the draft says so): the architect, the red and the gate reviewer recompute every one by command. The facilitator reran the claim that matters most: `docs/usable-v1/llm-check-codex.json` and `llm-check-grok.json` pin grammar_sha256 `ad0f6e51…` and cheatsheet_sha256 `fe3d8a8b…`, and the current GRAMMAR.md and CHEATSHEET.md hash differently (the AV rename of card 361 changed GRAMMAR.md), so bar 6 is already stale and SUSPENDED.
- Ruling 1, one card (D127: a single re-freeze in one Decider); green 2 waits for both models; the suspension and the Pending traits stay until green 2.
- Ruling 2, WIP of one (TEAM_WORKING_AGREEMENT.md:153) and D136 part 4: a one-card exception, recorded in the declaration: once the re-freeze record is committed and Grok's run is recorded, card 162 is parked as "waiting for Codex (reset 2026-11-11 12:10 AM local)" with no TDD cycle open (a measurement only); the next card may be pulled; card 162's evidence commit and green 2 may follow commits of that card; its retro is pushed before the issue closes. This follows the product owner's decision of 2026-11-09 (continue with the other v0.1 cards meanwhile).
- Ruling 3, the freeze window: from the re-freeze record to green 2, nothing edits GRAMMAR.md, CHEATSHEET.md, the check prompt or the runner; Bar6's hash binding and the launcher guard enforce it; a card that needs a GRAMMAR row waits (card 166 among them).
- Ruling 4, green 1 on a writing seat: Cursor through `cursor_write.py` (D236: a smoke test first; the facilitator re-runs every claim; the brief says leave changes uncommitted so the facilitator commits); if it fails the cause is isolated and fixed (D236), and a fresh Claude context takes the seat only as a recorded stopgap. The red by a fresh Claude context (it must run dotnet and python; a Cursor or Muse red on a docs card is possible later).
- Ruling 5, the launcher `llm_check_when_back.py` and the scheduled tasks: written in mob-crosscli with tests and smoke-tested with a fake model command before it is armed; the schedule for Codex is registered with the machine's task scheduler at the product owner's explicit instruction ("wire it so it happens automatically; schedule a re-probe at the stated reset time"); Grok's run is launched by hand right after the re-freeze record is pushed (and the same launcher guards the order).
- Ruling 6, seats this window: Grok is HELD for its check run (the product owner's decision) and is not seated; Codex and Copilot are out (the live probe of this window); Decider voters by rotation among Claude, Cursor, Muse, Qwen and agy. The live probe is committed as `.mob/records/162/probe-all-2026-11-09.jsonl`.
- Ruling 7, the earlier kept draft's rows PH020, PH337, PH340 are now AV020, AV337, AV340 (AV341 added by card 203); the draft's section 6 (the "formerly PH" pointer is not Ari's concern) stands; card 166's wording is flagged to the next re-rank.
- Correction to the draft's section 7 (the declaration): the seats are as above; the cold reader is Muse with GRAMMAR and the cheat sheet inlined (fallback agy on gemini-3.8-flash, then Qwen); no second read-only review (a dated skip line); the independent gate reviewer and sign-off are two further fresh Claude contexts; the Grok and Codex check runs are the model under test, not seats and not voters.

---

# Card 162 Ready draft (product manager seat, Cursor; read-only, nothing run)

**Summary.** Card 162 can go Ready as one card. Everything except the two model runs can land now:

- the fold of the pattern steps into the grammar and cheat sheet,
- the sentence on the AV code convention,
- the re-freeze record,
- the reds.

Main stays green throughout, because bar 6 is already suspended and its test is already Pending.

Four corrections to the brief and the card:

1. **Un-pending the hash-binding test depends on the models.** If it is un-pended before both evidence files exist, the `Status!=Pending` gate (`ci.yml:70`) turns red. It lands in the final green.
2. **The card's "one change" line is stale.** The Context line ("re-freeze, rerun and push in one change") was written before D131 added the suspension. Now the suspension simply stays until both halves are in.
3. **Bar 6 must be tightened in the red.** Today it passes with one current file (`UsableV1.cs:113`), so a fresh Grok file next to a stale Codex file would count as met.
4. **The run must wait for the cold reader.** Grok's run is only valid against the final, post-cold-reader document hashes. So it starts as soon as the re-freeze record lands, not before.

## 0. Standing-rules preflight

| Rule | How the card meets it |
|---|---|
| Card format (`TEAM_WORKING_AGREEMENT.md:120`), Definition of Ready and persona (`:116`) | A Connextra story for Ari and 3 Gherkin scenarios. Together they cover Z, O, M, B, I, E and S, and each title carries its letters. |
| TDD red-first (D137, `:81-88`); D204 (`:528`) | One red commit holds three Pending tests (`Criterion=162-1`, `162-2`, `162-3`), as in the card 361 and 366 rulings. Green 1 carries `Criterion: 1`. Green 2 carries `Criterion: 2-3` and changes the red files by their Pending traits only. |
| D206 cold reader (`:534`); D219 and D226 (`:537`) | This is a docs card, so the cold reader is in. The gate review's step 0 runs `git merge-base --is-ancestor` against the last page revision. |
| D222 | Every two-construct row becomes ids (section 3). The red record lists them, and the gate reviewer holds the card if one is missing. |
| D234 | The block `.mob/records/templates/declaration-evidence.md` is included in the declaration, which quotes `docs/retros/card-371-2026-11-09.md` §9 as the example. |
| D236 (`:504`) | The seat plan cites a `probe_all.py` output under 60 minutes old, committed under `.mob/records/162/`. Leaving Grok out of seats has a recorded cause: the product owner's decision of 2026-11-09 holds it for its run. |
| D72, D127, D131 | One re-freeze Decider records the old and new hashes. The pair stays Codex and Grok, and is never changed to work around an outage (`TEAM_WORKING_AGREEMENT.md:353`). |
| WIP of one (`:153`); D136 part 4 (retro before another card's commit) | Needs a ruling (section 2). |
| `CODING_GUIDELINES.md` | Exact asserts. Every test can fail (`:60`). Deterministic on Windows and Linux (`:59`, which covers line endings in the hashes). One definition and links rather than copies (`:41`, `:48`), so `PATTERNS.md` keeps the adapter idiom and links to GRAMMAR for the syntax. |

## 1. Premise facts

**What `PATTERNS.md` has that GRAMMAR and the cheat sheet lack:**

- **Ensure step (`PATTERNS.md:7-28`).**
  - The form is `ensure <cond> else <Failure>`.
  - The failure must be in the containing unit's `fails with`, and endpoints may use it (`:17`).
  - Failure names in a list must be unique (`:18`).
  - There are four condition forms: blank, not blank, `length at most N`, and an Int comparison (`:22-24`).
  - Paths may be field paths, and numbers are bounded to 32-bit (`:26`).
- **Record update (`:32-42`).**
  - The form is `let x = rec with { f: v }`, a copy and not a mutation.
  - `with {` is reserved only after a name already in scope. An empty or malformed list is AV200 (`:38`).
  - Unknown and duplicate fields are errors, a trailing comma is not allowed, and a nested update is rejected (`:40`).
- **The compiler matches this:**
  - An ensure step is recognised only when the line contains `else` (`Parser.cs:621`); without `else`, `ensure ...` stays an avow call.
  - An update needs an in-scope root (`:660-668`).
  - An undeclared ensure failure is AV326 (`Resolver.cs:232`).
  - A repeated update field is AV303 (`:270`), an unknown field AV311 (`:280`), and an existing variable AV313 (`:290`).

**GRAMMAR lines the fold must change:**

| Line | Today | Change |
|---|---|---|
| 85-86 | EBNF `step` lacks an ensure step and an update step | Add both. |
| 31 | Keywords | No `ensure` or `else`; no change needed. |
| 33 | Says `with` is an ordinary word in steps | False since card 160. |
| 124 | Uses `ensure items is not empty` as a plain avow call | True only without `else`; say so. |
| 140, 146 | `ensure` listed as a controlled-vocabulary verb | Must be reconciled with the step. |
| 186 | AV326 reads only "Composite fails with an undeclared code" | Also covers an ensure failure. |
| 269 (§7) | "Emitting composite avows with steps ... the first provider will not" | Contradicts `PATTERNS.md:28` and AV340's list; the slice checks it. |

- **Cheat sheet:** its only "ensure" is a native avow (`CHEATSHEET.md:42-53`), and no fence contains `with {`.

**The AV rename (card 361).** GRAMMAR has 67 lines that carry a code, including 56 table rows. Line 162 lists `AV300` to `AV335` plus AV337, AV340 and AV341. No sentence states the convention; the table header is just "Code" (`.mob/records/361/signoff.md:28`). The fold adds one sentence: codes are `AV` plus three digits, and the numbers are stable. The new text must not contain `PH` followed by a digit, or card 361's live-tree guard fails. The "formerly PH" pointer for history readers is not Ari's concern (section 6).

**Hashes and where each is bound** (carried from earlier reads; recompute before use):

| Item | Pinned value | Current | Bound in |
|---|---|---|---|
| GRAMMAR.md | `ad0f6e51…` | `8e4f91bd…` (card 361 tip) | `llm-check-codex.json:12`, `llm-check-grok.json:12`; checked by Bar6 (`UsableV1.cs:108,131`) |
| CHEATSHEET.md | `fe3d8a8b…` | `4b1b9ef5…` | Same files, `:13`; Bar6 `:109,133` |
| Orders source | D72 froze `194003df…` (as `orders.phrase`) | `4d7a24b9…` (unverified) | Not pinned now. The re-freeze ratifies it, unchanged. |
| Orders generated output | `19fe6d71…` | (unchanged) | `WebExtensionBoundary.cs:18` |

- Bar 6 is excluded from the main gate by `Status=Pending` (`UsableV1.cs:101`).
- The usable-v1 job filters `Milestone=UsableV1` (`usable-v1-report.py:147-148`), so Bar6 still runs there. It fails, and the report shows it as SUSPENDED (`board/usable-v1-ratchet.json:11-13`; `usable-v1-report.py:89,102-107`). The job stays green with the warning.

**Token cost of the cheat sheet now.** It is about 2,070 characters, which is 518 estimated tokens by `ceil(len/4)` (`PhraseCheatSheet.cs:191`). The cap is 700 (`:81`), and it was 477 at the D71 freeze. That leaves 182 tokens, about 728 characters, for both new examples. The check prompt is GRAMMAR plus the cheat sheet plus the task plus `Tasks.avowry` (`tools/Avowry.Bench/Program.cs:465-468`). Its `tokens_in` was 5,836 on 2026-10-28, and it will rise. The card reports both numbers, before and after.

**The runner** (`scripts/run-llm-check.py`):

- **Command:** `python scripts/run-llm-check.py --model {codex|grok} --out docs/usable-v1/llm-check-<model>.json`. `--model` accepts only `codex` and `grok` (`:558`). The default timeout is 600 s (`:562`), and there is one attempt (`:524-525`).
- **Inputs:** a cold copy of GRAMMAR and the cheat sheet (`:230-236`), the U1 prompt, the hidden outcomes, and `Tasks.avowry` (`:218-227`). The prompt still says "Return only valid Phrase source" (`bench/usable-v1-llm-check/tasks/U1-add-endpoint/prompt.md:1`).
- **Output:** totals-only JSON (`:440-459`).
- **"Passed" means `proof_level: specs-passed`.** The reply compiles with the Avowry CLI, the generated API builds with 0 warnings, every generated spec passes, the hidden outcomes pass, and the bench row is not marked unavailable (`:270-397`, `:423-439`). Bar6 additionally requires `specs > 0`, `specs_passed == specs`, and both hashes current.
- **Stale CLI risk:** the runner uses the newest existing `Avowry.Cli.dll` (`:246-249`), so rebuild it first.

**What a Codex or Grok run needs:**

- The CLI logged in and on PATH. Codex runs `exec --skip-git-repo-check --sandbox read-only` (`Program.cs:614,689`). Grok runs with a scratch working directory, its tools removed, and web search off (`:615`).
- dotnet and python, and a worktree at the re-freeze commit.
- An idle test gate, because the acceptance tests are sensitive to load.
- The CLI version and model id recorded in the run record. The 2026-10-28 JSON does not record them, so comparability with that run is "same pair, task and runner", not the same model build.

## 2. Decision

**One card; green 2 waits for both models.** Reasons:

- D127 asks for a single re-freeze in one Decider.
- The product owner keeps "162" ranked, with "the reruns the only step left".
- A split would close a card that has changed the documents while bar 6 is still suspended, which gives Ari no value yet.

**Model-independent steps, now, in order:**

1. The red.
2. Green 1: the fold, the AV sentence, the PATTERNS link trim, and the prompt word "Phrase" changed to "Avowry".
3. The cold reader, then any page revisions.
4. The re-freeze Decider record, in `docs/decisions/language-and-grammar.md`. It holds:
   - old and new sha256 of GRAMMAR, the cheat sheet, the orders source (ratified unchanged) and the prompt;
   - the token report;
   - the baseline answer: the scored arms change, and bench rows are labelled by `docs_hash` (`PhraseCheatSheet.cs:153`), so earlier and later rows are not compared;
   - D126's `to` case, met by `GRAMMAR.md:35` and the parser tests (section 6).
5. Gate review pass A.

**Model-dependent steps:**

- **Grok:** runs as soon as the record lands. Its JSON is then committed as an evidence commit.
- **Codex:** runs at its reset. Then green 2: the Codex JSON, removing `Pending` from Bar6 and the ratchet test, and deleting `suspended.6`. Then the gate addendum and the sign-off.

**What keeps main green:**

- The suspension and Pending traits stay until green 2.
- The tightened Bar6 keeps failing (and SUSPENDED) while either file is stale.
- **Freeze window:** from the re-freeze record to green 2, nothing edits GRAMMAR, the cheat sheet, the prompt or the runner. Bar6's hash binding enforces it, and so does the launcher guard (section 5). Card 166 and any GRAMMAR-row card wait.

**Ruling needed (the facilitator, or the product owner).** The product owner's "ahead of any other seat" implies other cards run during the roughly 33-hour wait for Codex. That conflicts with WIP of one (`TEAM_WORKING_AGREEMENT.md:153`) and with D136 part 4. My recommendation: the declaration records a one-card exception.

- Card 162 is parked as "waiting for Codex" once the record lands. No TDD cycle is open, only a measurement.
- Its evidence commit and green 2 may follow commits of the next card.
- Its retro is pushed before the issue closes.

If the exception is refused, the mob pulls no card until about 2026-11-11 08:10 UTC.

## 3. Scenarios

**Story:** As Ari, I want the grammar and the cheat sheet to describe the ensure step and the record update, so that a cold model can use them from those two documents alone.

```gherkin
Scenario: [O][S][B] the grammar and the cheat sheet teach the ensure step and the record update
  Given docs/PATTERNS.md and the parser rules (ensure only with else; with { only after a name in scope)
  When Ari reads only docs/GRAMMAR.md and docs/CHEATSHEET.md
  Then the EBNF step has an ensure step and an update step, the prose states the four conditions, the else failure in fails with (AV326), that ensure without else is an avow call, and the update rules (copy, AV200, AV303, AV311, no nesting), and one sentence states the AV code convention
  And the cheat sheet has one compiling example of each and stays at most 700 estimated tokens

Scenario: [M][I][E] both models pass from the re-frozen documents alone
  Given GRAMMAR.md and CHEATSHEET.md at the re-freeze record's new hashes
  When the card 137 runner runs once, cold, with Grok and once with Codex
  Then llm-check-grok.json and llm-check-codex.json each show passed true, specs-passed, specs_passed equal to specs and at least 13, 0 warnings, 1 attempt, and both hashes equal to the record's
  And bar 6 fails, naming the file, when either file is missing, stale, failed, or partial

Scenario: [Z] no suspended bar and a recorded re-freeze
  Given main after green 2
  When the gate and the usable-v1 job run
  Then the ratchet has no suspended entry, Bar6 has no Pending trait, the report prints 6 of 6 met and USABLE v1 REACHED with no suspended-bar warning
  And the re-freeze record lists the old and new sha256 of GRAMMAR, CHEATSHEET, the orders source and the prompt (the new ones equal HEAD), the token report, and the baseline answer
```

**How each scenario fails on partial work:**

- Scenario 1 fails if any id is missing.
- Scenario 2 fails with only one model's file.
- Scenario 3 fails if the suspension is left in place or the record is missing.

**Ids (D222):**

| Criterion | Ids |
|---|---|
| 1 | G1 ensure step in the EBNF; G2 the four conditions; G3 `else` failure and AV326; G4 ensure without `else` is an avow call; G5 update step in the EBNF; G6 the update rules; G7 the AV sentence; G8 line 33 qualified; S1 ensure example; S2 update example; B1 token cap (characterisation) |
| 2 | V-grok; V-codex; E1 one current file plus one stale file; E2 the Codex file missing; E3 `passed` false; E4 `specs_passed` less than `specs` |
| 3 | Z1 no suspension; Z2 Bar6 not Pending; R1 the record's hashes, token report and baseline answer |

## 4. Named tests and what each shows on the current tree

| Test | Kind | On the current tree |
|---|---|---|
| `tests/test_pattern_docs.py` (G1 to G8, S1, S2), with wrapper `PatternDocs.cs` (`Pending`, `Criterion=162-1`; added to `ProcessStartCallers`, `GeneratedApiSourceScan.cs:22-30`) | NEW-STATE | G1: the EBNF `step` (`GRAMMAR.md:85`) has no ensure step. S1: no `ensure … else` in any avowry fence. The other ids fail the same way. |
| `UsableV1.Bar6_…`: body tightened (exactly the two model files, each passing), plus `Criterion=162-2`; planted rows E1 to E4 | NEW-STATE | V-ids: both files are stale (`ad0f6e51…` against `8e4f91bd…`). E1 and E2 pass under the old rule (`:113`), so they are watched failing against it. E3 and E4 are CHARACTERISATION of `EvidenceFailure` (`:117-141`). |
| `UsableV1Closure` (Z1, Z2, R1), `Pending`, `Criterion=162-3`, no Milestone trait | NEW-STATE | Fails: `suspended.6` is present; Bar6 is Pending; there is no record with the new hashes. |
| `PhraseCheatSheet` (cap and compile), `EnsureStepCompiles`, `RecordUpdateFormCompiles`, `WebExtensionBoundary`, `LlmCustomerCheck`, `tests.test_run_llm_check`, `usable-v1-report.py --self-test`, Bar1 to Bar5, card 361's live-tree guard | CHARACTERISATION | Pass now and after. |

## 5. Wiring the automatic runs

**Launcher** (mob tooling, beside `probe_all.py`): `llm_check_when_back.py --model <m> --card 162`. Before it is armed, it is smoke-tested per D236 in a throwaway worktree with a fake model command (the pattern at `LlmCustomerCheck.cs:194-199`). Its steps and exit codes:

1. Probe the model and append the row (with `head` and `row_sha256`) to `.mob/records/162/probes.jsonl`. If the probe fails, log the error text verbatim and exit 75 (retry).
2. Guard: a worktree exists at the re-freeze commit, and its document sha256 values equal the record's new hashes. Otherwise exit 76: keep the model held and retry. This makes the order of fold and return irrelevant.
3. If a test gate is running, exit 77 (retry).
4. Rebuild `src/Avowry.Cli`, then run the runner.
5. Write `.mob/records/162/run-<m>.md` (exit code, `passed`, reason, tokens, CLI version and model, the JSON's sha256) and a `.done` flag in a trap.
   - **Passed:** the facilitator commits the JSON byte-identical and lifts the hold.
   - **Model-result failure** (reason compile, build, spec or outcome): stop. No second attempt at the same hashes; a Decider chooses between revising the documents (both models rerun) and recording bar 6 as not met.
   - **Harness failure** (timeout or error): a D236 repair, then one rerun.

**Hold.** While a model is held, the DRIVERS.md note for it reads "HELD for card 162 check run (product owner decision 2026-11-09)". Seat plans cite that note as the recorded cause, and the hold lifts when the `.done` flag shows the run finished.

**Codex.** Register the task now. It runs as the logged-on user, so the CLI logins apply:

```powershell
$a = New-ScheduledTaskAction -Execute python -Argument '<mob-tooling>\llm_check_when_back.py --model codex --card 162'
$t = New-ScheduledTaskTrigger -Once -At '2026-11-11T00:12' -RepetitionInterval (New-TimeSpan -Minutes 30) -RepetitionDuration (New-TimeSpan -Hours 24)
$s = New-ScheduledTaskSettingsSet -WakeToRun -StartWhenAvailable -MultipleInstances IgnoreNew
Register-ScheduledTask -TaskName avowry-162-codex -Action $a -Trigger $t -Settings $s
```

- The first try is at 00:12 local, two minutes after the stated reset (08:12 UTC).
- The facilitator arms a monitor (D230) on success, failure, and expiry at 01:00 local.

**If the Codex probe still fails then:**

- The row and any new reset time go to the probe log and DRIVERS.md.
- The task keeps retrying every 30 minutes, and Codex stays held.
- No model is substituted (the product owner's decision; `TEAM_WORKING_AGREEMENT.md:353`).
- After 24 hours of failed probes, the facilitator sends the product owner an inbox note to check the account, and re-registers the task.

**Grok.** Register the same task now (`-At` now). It exits 76 until the re-freeze record lands, then runs within 30 minutes; the facilitator can also run it by hand right after the push. Grok takes no other seat until then.

## 6. Size and what is not in the card

**Size: M.**

- About 10 countable files: GRAMMAR, the cheat sheet, PATTERNS (the link trim), `prompt.md`, `UsableV1.cs`, the new Python test module and its wrapper, `GeneratedApiSourceScan.cs`, the ratchet, and the two JSON files. The decision record is not countable.
- Every commit stays at 15 files or fewer.
- Increments: the red, green 1, and green 2 (delivered as the Grok evidence commit plus the Codex commit).

**Not in this card:**

- Changing the runner's `choices` or the model pair.
- Changing the U1 task or its outcomes. That includes forcing D126's "a call that wrongly starts with `to`" case: a fixed task cannot force it without breaking the baseline, so it is met by `GRAMMAR.md:35` and the parser tests and recorded in the Decider. A separate measurement card can follow if wanted.
- Card 166's wording at `GRAMMAR.md:265`. After card 162, that card needs its own re-freeze, which amends D127's "single final"; flagged for the facilitator.
- The "formerly PH" pointer: its own S docs card, or a line in the index.
- Rewriting `orders.avowry` or the Tasks demo onto the new steps.
- Card 143's split sheet, card 145's store steps, and PATTERNS' native-block table.

## 7. Task-force declaration

| Seat | Who and why |
|---|---|
| Ready (PM) | Cursor (this draft). |
| Slice (architect) | A fresh read-only Claude context. It fixes the EBNF names, the needles, the form of the Bar6 tightening, the PATTERNS trim, the prompt-word ruling, and the GRAMMAR lines 33, 124, 140, 146, 186 and 269. |
| Red (QA) | A fresh Claude context in its own worktree. It must run dotnet and python to watch the reds fail. Cursor's command running in its writing mode is unproven, and Muse cannot finish a repository-reading brief (D229 and D230 findings). |
| Green 1 | **Cursor writing seat** (`cursor_write.py`) in its own worktree, after a D236 smoke test. The fold is reading-heavy prose and EBNF work. Cursor has six repository-reading seats, and its work is caught by the pre-written red, the compile test, the cold reader and the gate. Fallback: a fresh Claude context (a recorded stopgap). Not Muse. |
| Green 2 | A fresh Claude context, not the red's. A mechanical change, followed by the full gate and the usable-v1 report. |
| Cold reader (D206) | **Muse**, with GRAMMAR and the cheat sheet inlined (no repository reading), reading as Ari. Fallback agy (gemini-3.8-flash), then Qwen. Report: `.mob/records/162/cold-reader.md`, before the re-freeze record. |
| Second read-only review | None; a dated skip line. The gate reviewer checks the EBNF against `Parser.cs`, and another review would delay Grok's run. |
| Independent gate reviewer | A fresh Claude context. Pass A runs on the post-revision tip before Grok's run (step 0 per D226; replays the reds; checks the ids). An addendum on green 2 compares the JSON hashes with `git show HEAD:` sha256 values, and checks the ratchet and the Bar6 trait. |
| Sign-off | A further fresh Claude context in its own clone: the full gate, and 6 of 6 with no warning. |
| Decider voters (declaration and re-freeze) | Claude, Cursor, Muse (short votes), Qwen (staggered) and agy, by rotation. Grok is held (the product owner's cause), Codex is out (probe of 22:20 UTC), Copilot is out. |
| Grok and Codex check runs | The model under test: not seats, not voters. Launched by the launcher and recorded in `run-<m>.md`. |

**Not verified (nothing was run):**

- the current hashes and token count (carried from earlier reads);
- what `cursor_write.py` can do;
- whether the machine wakes for the scheduled task;
- the line-ending configuration. Bar 6 was green in CI before card 199, so the local hashes have matched LF so far. The launcher's `git show` hash check guards it anyway.
