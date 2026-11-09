# Proposal for Decider D231: the task-force declaration and card text for card 366 (the NOTICE and TRADEMARKS files name the holder and the year)

Facilitator, 2026-11-09 UTC, main at 5b797df. VOTE on this declaration: adopt the card text below (the product manager seat's Ready draft with the facilitator's rulings) as card 366's Ready text, and the task force named in the rulings block: red = a fresh Claude QA context in its own worktree (two commits: the new tests and Pending wrappers, then card 186's placeholder clause removed), green = a different fresh Claude context (never the red's seat), a read-only architect slice first, a cold reader (Qwen, inlined) before the independent gate review (D206, D219 and D226 ordering with the blocking step 0), independent gate reviewer and sign-off two further fresh Claude contexts, no second read-only review (dated skip line).

STANDING-RULES PREFLIGHT: TDD red-first hard rule, D137 against D204 (ruled as on card 361), D204 characterisation up front, D206 (cold reader; guards), D219 as tuned by D226, D222 (the id lists N, T, G, P, X, L, C), the Card format rule, the Value rule (the sponsor persona and Dana read the holder line), CODING_GUIDELINES (one definition of the placeholder text), D127. Each is answered in section 0 of the draft. Decider protocol step 6 (D224) applies; this proposal changes no agreement text. The 30 percent rule and the strict reading of a No are unchanged. The D228 preflight applies to this window.

The draft (verbatim, with the facilitator's notes first):

# Card 366 proposal (product manager seat, ready; reply-only; a Cursor seat on Probation rung 1, its fourth real seat)

The seat's reply (Cursor in ask mode, through cursor_reply.py, a fresh context; read-only, it ran nothing), committed verbatim below the facilitator's notes.

## Facilitator notes and rulings (2026-11-09 UTC, main at 5b797df)

- Rerun by the facilitator: NOTICE line 2 and TRADEMARKS.md line 5 hold the marked placeholder text as the draft quotes; `docs/LICENSING.md:27` says the holder "is a marked placeholder until the product owner confirms it"; a `git grep` for `test_license_files` outside history finds no wrapper or workflow that runs it (the gate does not run that module); `tests/test_pre_publication_checks.py` lines 109, 241 to 247 and 274 carry the placeholder context that fails once the files are filled.
- Ruling on `docs/LICENSING.md:27`: in scope (otherwise the card leaves a false sentence in front of the sponsor persona and Dana); the criterion 3 row L1 stays.
- Ruling on the cold reader: yes (a changed prose line; Qwen with NOTICE, TRADEMARKS.md and docs/LICENSING.md inlined, reading as the sponsor persona), run before the gate review; the D219 and D226 ordering applies (the gate review's brief begins with the blocking step 0).
- Ruling on D137 against D204: as on card 361, one red commit holds the criteria that turn green together; the Pending wrappers carry `Criterion=366-1`, `366-2` and `366-3`.
- Ruling on card 186's test: its placeholder clause is removed in the red as its own commit before the green (a loosening, not a red), so main is never red between the red and the green.
- Correction to the draft's section 5 (the declaration): Grok, Codex, Copilot and agy are out of quota. The seats are as the draft names them with the facilitator's rulings: slice by a fresh read-only Claude context; red a fresh Claude QA context in its own worktree; green a different fresh Claude context in its own worktree; independent gate reviewer and sign-off two further fresh Claude contexts; Decider voters by rotation among Claude, Cursor, Qwen and Muse (short votes only for Muse); no second read-only review (a dated skip line, an S card).
- The year 2026 is the owner's assumption (the project began in 2026; the owner can correct it): the LICENSING.md sentence says so, and the decision record the facilitator appends names it.

---

# Card 366 Ready draft (product manager seat; read-only, nothing run)

**Summary:** the card can go Ready at size S, but four facts from the code change its premise:

- **The tests named on the card are not run by the gate.** `tests/test_license_files.py` has no C# wrapper, and `.github/workflows/ci.yml` runs only `mob_process.py check`, `dotnet test` and `usable-v1-report.py`. The card's "Done when" command (`dotnet test`) therefore checks none of its criteria. The red must add a wrapper.
- **Filling the files turns card 186's gated test red.** `tests/test_pre_publication_checks.py:244-247` fails with `owner-items: NOTICE or TRADEMARKS.md placeholder missing`. It is run by `PrePublicationChecks.cs`, so main would go red.
- **A reader-facing page becomes false.** `docs/LICENSING.md:27` says the holder "is a marked placeholder". That is the natural place for the "year is an assumption" line, which card criterion 2 wrongly put on the pre-publication page (that page is card 371's).
- **The placeholder text is defined twice.** It appears at `test_license_files.py:7` and `test_pre_publication_checks.py:10-13`, which breaks the one-definition rule in `CODING_GUIDELINES.md:41`.

## 0. Standing-rules preflight

| Rule | How this card meets it |
|---|---|
| Card format (`TEAM_WORKING_AGREEMENT.md:120`) | A Connextra story for the sponsor persona and 3 Gherkin scenarios. Together they cover Z, O, M, B, I, E and S, and each title carries its letters. |
| TDD red-first (`:88`) | Every new-state id is watched failing on the current tree before the green. The characterisation ids are declared up front in section 3, with their reasons. |
| D137 (`:81-83`) | The Pending wrappers carry the traits `Criterion=366-1`, `366-2` and `366-3`. The green removes `Pending`. `UsableV1.Bar6_*` (`UsableV1.cs:101`) is already Pending, which is the same conflict as on card 361. The facilitator's card 361 ruling (D204 allows one red commit holding several criteria) is the precedent, and the facilitator confirms it. |
| D204 (`:522`) | One fill turns all three criteria green, so there is one red commit with each criterion on its own `Criterion:` line, and the green carries `Criterion: 1-3`. |
| D206 guard rule (`:525`) | The placeholder guard checks live state, so it reads the working tree on purpose. The blob-and-look-alike-commit rule does not apply to it. |
| D206 cold reader (`:528`) | Applies; see section 5. |
| D222 (`:534`) | Every row that names two constructs is split into ids in section 2. The red record lists them all. |
| D219 and D226 (`:531`) | Apply, because the card has a cold-reader seat (section 5). |
| `CODING_GUIDELINES.md` | Exact results (`:35`): whole-line asserts. Every test can fail (`:31`): the scanner has planted rows. Fix impacted tests (`:42`): the card 186 clause. No dead code (`:40`): card 186's placeholder constant and context keys are removed. One definition (`:41`). Role, not person (`:49`): "the product owner". |

## 1. Premise facts

- **F1.** `NOTICE:2` reads `Copyright [YEAR] [COPYRIGHT HOLDER: to be confirmed by the product owner at the pre-publication checks (card 186)]`. `NOTICE:5` already names the GitHub organisation in the URL `github.com/Programming-Made-EZ/avowry`.
- **F2.** `TRADEMARKS.md:5` reads `Trademark holder: [COPYRIGHT HOLDER: …]` with the same text. The file has no other square brackets.
- **F3.** `test_license_files.py:54-64` pins the placeholder in both files, asserts exactly one `Copyright ` line, and requires `^Copyright \[YEAR\] <placeholder>`. Lines `:44-52` hash-pin LICENSE. Lines `:18-42` and `:66-81` pin the Avowry name, the generated-code statement, the trademark terms and the licensing page. **No test or workflow runs this file** (no `tests.test_license_files` reference in any `.cs` file or workflow).
- **F4.** `LICENSE:190` is `Copyright [yyyy] [name of copyright owner]`. This is the Apache appendix boilerplate, covered by the hash, and must not change.
- **F5.** Card 186's test has a placeholder constant at `test_pre_publication_checks.py:10-13`. The context keys that carry it are at `:109` and `:274-275`, and the clause at `:241-247` requires the placeholder in both files. It is gated through `PrePublicationChecks.cs:14` (`Criterion=186-1`, not Pending).
  - Its other owner-items check (at least 6 `OPEN … 2026-11-08` lines on the page) keeps passing, because this card does not touch the page.
- **F6.** `docs/LICENSING.md:27` says the holder "is a marked placeholder until the product owner confirms it". No test pins that line.
- **F7.** Other places that become stale:
  - `docs/release/pre-publication-checks.md:102` and `:115` (the holder is OPEN; the placeholders stay). These are card 371's.
  - `HANDOFF.md:75` and `:119`. This is the facilitator's handoff. `test_live_docs_266.py` compares its base commit with the card 266 green, not with HEAD, so these edits do not break it.
- **F8.** No tracked file outside `.mob/` names a holder:
  - `Programming Made EZ` appears only in board cards 366 and 371.
  - `Programming-Made-EZ` appears only as the organisation in URLs and board text.
  - No `.csproj`, `.props` or `.targets` file has `Authors`, `Company` or `Copyright` (also stated at `pre-publication-checks.md:22`, and a matter for the publish card).
  - `README.md:31` links NOTICE and TRADEMARKS without naming a holder, so it is unchanged.

## 2. Scenarios

**Story:** As the sponsor persona, I want NOTICE and TRADEMARKS.md to name the holder and the year, so that I can see who owns the Avowry name before I adopt it.

```gherkin
Scenario: [O][S][I] the holder and the year are filled
  Given the owner's answer: holder "Programming Made EZ", year 2026, the same holder for the trademark line
  When the sponsor persona opens NOTICE and TRADEMARKS.md
  Then NOTICE line 2 is exactly "Copyright 2026 Programming Made EZ" and NOTICE has exactly one "Copyright " line
  And TRADEMARKS.md line 5 is exactly "Trademark holder: Programming Made EZ"

Scenario: [Z][E] no placeholder remains or returns
  Given NOTICE and TRADEMARKS.md
  When the placeholder scanner reads them
  Then it finds zero hits (the card 186 placeholder text, "[YEAR]", "to be confirmed", or an all-caps bracket token)
  And a planted placeholder makes the scanner report it, while a markdown link "[LICENSE](LICENSE)" does not

Scenario: [M][B] the other legal files keep their terms and the year is labelled an assumption
  Given LICENSE, README.md, docs/LICENSING.md and TRADEMARKS.md's policy text
  When the holder is filled
  Then LICENSE still hashes to the Apache-2.0 text (its "[yyyy]" boilerplate is not a placeholder to fill)
  And the existing standing-terms checks still pass
  And docs/LICENSING.md names "Programming Made EZ" and says 2026 is the year the project began, an assumption the product owner can correct
```

**How each scenario fails on partial work:**
- Scenario 1 fails if either file is left unfilled, or the year is filled without the holder (or the reverse).
- Scenario 2 fails if any bracket remains, or if the scanner is vacuous.
- Scenario 3 fails if LICENSE is touched, if a standing term is lost, or if the LICENSING status line is not rewritten.

**Ids (D222):**

| Kind | Ids |
|---|---|
| Criterion 1, NEW-STATE | N1 NOTICE holder; N2 NOTICE year; N3 one `Copyright ` line (this is a boundary check, so it passes on the current tree; it is kept from the old test, not a red); T1 TRADEMARKS holder |
| Criterion 2, NEW-STATE | G1 NOTICE has no hit; G2 TRADEMARKS has no hit. Planted rows, watched failing against a stub scanner: P1 `[YEAR]`, P2 the full card 186 text, P3 `[HOLDER NAME]`, P4 the bare phrase "to be confirmed by the product owner". Negative rows: X1 `[LICENSE](LICENSE)` and X2 the filled NOTICE line must produce no hit. |
| Criterion 3 | L1 LICENSING status line (NEW-STATE). C1 LICENSE hash (CHARACTERISATION). C2 the four standing-terms tests (CHARACTERISATION). |

## 3. Named tests and what each shows on the current tree

| Test | Kind | On the current tree |
|---|---|---|
| `LicenseFilesTests.test_notice_and_trademarks_name_the_holder_and_the_year` (subTests N1, N2, N3, T1) | NEW-STATE. **Replaces** `…carry_the_marked_placeholder_until_the_owner_confirms_the_holder` in the red commit: the old test is deleted there, and its one-Copyright-line assert moves in as N3. | N1, N2 and T1 fail by assertion: expected `Copyright 2026 Programming Made EZ`, got `Copyright [YEAR] [COPYRIGHT HOLDER: …]`. N3 passes. |
| `…test_notice_and_trademarks_hold_no_placeholder` (G1, G2) | NEW-STATE | Fails, listing the hits `[YEAR]` and the placeholder in each file. |
| `…test_the_placeholder_scanner_*` (P1 to P4, X1, X2) | NEW-STATE unit | P rows fail against a stub scanner that returns nothing (as on card 324). Then the real scanner lands in the red. |
| `…test_the_licensing_page_names_the_holder_and_the_year_assumption` (L1) | NEW-STATE | Fails: `LICENSING.md:27` says "marked placeholder". |
| `test_license_is_the_standard_apache_2_0_text_byte_for_byte` (C1) and the four standing-terms tests (C2) | CHARACTERISATION: LICENSE must not change, and the standing terms must survive the edit | Pass now and after. |
| New `tests/Avowry.Acceptance.Tests/LicenseFiles.cs` | Wrappers, following `GettingStartedDocs.cs` | Three Pending wrappers (`366-1`, `366-2`, `366-3`), each running its own Python test ids, fail by assertion. One non-Pending wrapper with `Slice=366` runs C1 and C2 and passes, so the gate now runs this Python file. |
| `PrePublicationChecks.Pre_publication_findings_match_the_repo` | Impacted, existing | Revised **in the red, as its own commit before the green** (see below). It passes before and after that commit. |

The red's revision of card 186's test deletes:
- the placeholder condition at `:241-247`, keeping the 6-OPEN check;
- the `PLACEHOLDER` constant at `:10-13`;
- the `placeholder`, `notice` and `trademarks` context keys at `:109` and `:274-275`.

That revision only loosens the test, so it is not a red. The red record shows the wrapper passing after it. Without it, the green turns that test red. The placeholder check moves to criterion 2, which is stricter, and the placeholder text is then defined once, in `test_license_files.py`. Card 371 later asserts the page's answered form.

**The green** changes only:
- NOTICE line 2;
- TRADEMARKS.md line 5;
- `docs/LICENSING.md:27`;
- removal of `Pending` from the three wrappers.

## 4. Size and what is not in the card

**Size: S.** About 6 files: NOTICE, TRADEMARKS.md, `docs/LICENSING.md`, `test_license_files.py`, `test_pre_publication_checks.py` and the new `LicenseFiles.cs`.

**Not in this card:**
- The pre-publication page text (`:102`, `:115`) and the 6-OPEN check: card 371.
- The professional trademark search: commissioned by the owner, result pending.
- The lawyer questions: skipped by the owner.
- LICENSE: hash-pinned, and line 190 stays.
- Package `Authors` and `Copyright` metadata and packing NOTICE into the tool package: the publish card.
- `HANDOFF.md:75` and `:119`: the facilitator's own handoff commit.
- The board files, which are generated.
- The decision record of the 2026 assumption: the facilitator appends it to `docs/decisions`.

**Done when:**
- `dotnet test Avowry.sln --filter "Status!=Pending"` passes.
- `python -m unittest tests.test_license_files tests.test_pre_publication_checks` passes.
- `git diff --quiet <base> <green> -- LICENSE` exits 0.
- CI on main is green.

## 5. Task-force declaration

| Seat | Who and what |
|---|---|
| Product manager (Ready) | Cursor, read-only (this draft). |
| Architect (slice) | The facilitator, or a fresh read-only Claude context. It fixes the scanner pattern, the wrapper split, the exact LICENSING.md sentence, and confirms the Pending precedent from card 361. |
| Red (QA) | A fresh Claude context in its own worktree. It makes two commits, both before the green. **The first commit** adds the new-state tests and the stub scanner, then the real scanner, plus the Pending wrappers and the characterisation wrapper. It also deletes the old placeholder test. **The second commit** is the card 186 clause removal. The red record quotes every N, T, G, P and L id failing, and C1, C2, X1, X2 and N3 passing. |
| Green (developer) | A different fresh Claude context in its own worktree, never the red's seat. It fills the three lines, removes `Pending`, and runs both Python modules and the gate. |
| Cold reader (D206) | **Yes.** The story promises what the sponsor persona sees, and `LICENSING.md:27` is a changed prose line. Qwen, or Muse, with only NOTICE, TRADEMARKS.md and `docs/LICENSING.md` inlined. Reading as the sponsor persona, it checks that the holder is clear and the year is labelled an assumption. No edits; it runs nothing. |
| Independent gate reviewer | A fresh Claude context. **D219/D226 ordering applies** because the card has a cold-reader seat: step 0 is the blocking `git merge-base --is-ancestor` check against the commit after the cold-reader report and any revision it caused. It replays the reds on the pre-change tree, checks the ids against section 2 (D222), and confirms LICENSE is unchanged. |
| Second read-only review | None. This is an S card, and the cold reader covers the prose. A declared seat that is later skipped gets a dated skip line. |
| Sign-off | A further fresh Claude context that runs its own full gate. |
| Decider voters | Claude, Cursor, Qwen and Muse, by rotation (short votes only for Muse). |

**For the facilitator to rule on before the red:** whether `docs/LICENSING.md:27` is in scope. I recommend yes, because otherwise the card leaves a false sentence in front of the sponsor persona. If the ruling is no, L1 drops from criterion 3. The assumption is then recorded only in the decision record, and the cold-reader seat and the D219/D226 ordering no longer apply.
