# Card 371 proposal (product manager seat, ready; reply-only; a Cursor seat on Probation rung 1, its fifth real seat)

The seat's reply (Cursor in ask mode, through cursor_reply.py, a fresh context; read-only, it ran nothing), committed verbatim below the facilitator's notes.

## Facilitator notes and rulings (2026-11-09 UTC, main at 8549a90)

- Rerun by the facilitator: `docs/release/pre-publication-checks.md` line 103 holds the holder item with "(which also holds a year placeholder)" and line 116 the stale recommendation ("holder needed (year and name)"); `tests/test_pre_publication_checks.py` lines 235 to 238 hold the 6-OPEN clause; `PrePublicationChecks.cs` runs the whole module through a direct process start (so a new test in that module would be gated); the new wrapper uses `ProcessRunner`, as `LicenseFiles.cs` does.
- Ruling 1, jurisdictions (the owner named none): the brief carries a line "proposed by the mob; the owner confirms with the provider" listing the United States, the European Union and the United Kingdom (the markets of the package registry and the code host the first preview uses); B5 checks the line and its label exist. The owner may change the list when engaging the provider.
- Ruling 2, lines `:62` and `:74` (and `:20`, `:93`, `:96`): in scope (S1 to S8); otherwise "the owner decides" stays in front of the sponsor persona after the owner decided.
- Ruling 3, D137 against D204: as on cards 361 and 366, one red commit may hold the criteria that turn green together; here the green is one page edit, so the three Pending wrappers `Criterion=371-1`, `371-2`, `371-3` are committed together after the revision commit and the green removes the three traits.
- Ruling 4: the story's actor is the sponsor persona (the page addresses that persona).
- Ruling 5: the new tests live in a new module `tests/test_pre_publication_owner_items.py` with its own wrapper `PrePublicationOwnerItems.cs` (via `ProcessRunner`, no literal process start); card 186's module is revised only by the 6-OPEN clause move described in section 3 (its own commit first).
- Ruling 6, seat worktrees: the product owner's direction of 2026-11-08 (a Claude QA seat in its own worktree) stands; the line at `TEAM_WORKING_AGREEMENT.md:79` is about the facilitator's checkout and is not touched by this card.
- Correction to the draft's section 5 (the declaration): Grok, Codex, Copilot and agy are out of quota. Declared: read-only architect slice (fresh Claude); red a fresh Claude QA context in its own worktree; green a different fresh Claude context in its own worktree; a cold reader (Qwen, the page, NOTICE and TRADEMARKS.md inlined) and a second read-only review (Cursor) both before the gate review and committed with any page revision (D219 as tuned by D226, blocking step 0); independent gate reviewer and sign-off two further fresh Claude contexts; Decider voters by rotation among Claude, Cursor, Qwen and Muse (short votes only for Muse).
- The review records are committed as `.mob/records/371/cold-reader.md`, `second-review.md`, `gate-review.md` and `signoff.md` before the card closes, with the closure evidence naming their commits (the D231 amendment).

---

# Card 371 Ready draft (product manager seat; read-only, nothing run)

**Summary:** the card can go Ready at size S once the facilitator rules on five points (listed at the end). Four facts from the code change its premise:

- **The brief's line numbers are partly off.** "Holds a year placeholder" and the holder OPEN item are both on `:103`, not `:100`. Line `:100` says "Every item is open". Line `:115` is blank, and the stale recommendation is `:116`. Lines `:20`, `:93`, `:96`, `:62` and `:74` are also stale.
- **The card 186 count cannot stay as it is, and cannot simply be tightened in the gate.** The check at `test_pre_publication_checks.py:235-238` needs at least 6 OPEN items, which the green makes false. But its wrapper (`PrePublicationChecks.cs:14`, not Pending) runs the whole module. So a tightened assertion that fails on the unrevised page would turn main red if it stayed in that module.
- **Criterion 3 as filed is not reader-observable.** "The test asserts…" breaks the Card format rule's requirement that each Then states what the actor observes.
- **The owner did not name the jurisdictions** the brief must list.

## 0. Standing-rules preflight

| Rule | How this card meets it |
|---|---|
| Card format (`TEAM_WORKING_AGREEMENT.md:120`) | Three Gherkin scenarios that together cover Z, O, M, B, I, E and S, each titled with its letters. The story actor should be **the sponsor persona**, because the page addresses that persona (`pre-publication-checks.md:3`), not the persona the card names. Criterion 3 is rewritten so its Then is something the actor sees. |
| TDD red-first (`:88`) | Every new-state id is watched failing on the pre-change page. The characterisation ids are declared in section 3. |
| D137 (`:81-83`) | Pending wrappers carry `Criterion=371-1`, `371-2` and `371-3`. Three Pending tests at once conflicts with "at most one", and the card 361 and 366 precedent covered an atomic green. Here the green is three separable page edits, so this needs a ruling (see the end). |
| D204 (`:522`) | One red commit per criterion. The first is the revision of card 186's test (section 3). |
| D206 guard rule (`:525`) | The checks read the live page from the working tree on purpose (they guard live state). The blob rule does not apply. |
| D206 cold reader (`:528`) | Applies (section 5). The promise check: "so that I see what is settled and what still waits" is covered by ids A1 to A9, which must say settled or name the card or result it waits on. |
| D219 and D226 (`:531`) | Apply. The gate review starts after the cold reader, the second review and any page revision are committed. Its step 0 is the blocking `git merge-base --is-ancestor` check. |
| D222 (`:534`) | Every multi-construct row below is split into ids. The red record lists them all. |
| `CODING_GUIDELINES.md` | Exact results (`:35`): whole-line asserts and exact failure strings. Every test can fail (`:31`): planted pages, written against a stub checker first. One definition (`:41`): the brief reuses `TARGETS` (`test_pre_publication_checks.py:27`) by import. Links, not copies (`:48`): the brief links the Name collisions section. Role, not person (`:49`). |
| Worktrees | Per-seat worktrees for the red and the green follow the card 366 practice, but `TEAM_WORKING_AGREEMENT.md:79` still says "no per-seat worktrees". The facilitator should confirm that its card 366 ruling stands. |

## 1. Premise facts (lines verified by reading)

**F1. Owner items section, `docs/release/pre-publication-checks.md`:**
- `:100` reads "Command: read from NOTICE, TRADEMARKS.md, D121 and D125 … Every item is open". This needs the inbox entry of 2026-11-08 as its source, plus the card 366 commit for NOTICE.
- `:102` Public name: OPEN.
- `:103` Holder: OPEN, "(which also holds a year placeholder)".
- `:104` Search: OPEN.
- `:105` Lawyer questions: OPEN. Its questions are at `:106-109`, and `:109` (question 4, "Who is the holder?") is answered by the holder item.
- `:110` to `:114` (author identity, vendor terms, reply_head, transitive licences, home paths): OPEN.
- `:116` Recommendation still says "holder needed (year and name)", "lawyer ask" (which contradicts the owner's skip), and "The holder placeholders … stay until the holder is named".

**F2. Stale lines in other sections of the same page:**
- `:20` "the lawyer questions stay open".
- `:93` "Trademark databases … not reviewed: needs a professional search". It should add "commissioned by the owner; pending result".
- `:96` "commission a professional trademark search". The search is now commissioned, and publishing the name waits on the result.
- `:62` "decide the home-path item". This is decided: scrub-tracked, card 370.
- `:74` "the owner decides whether that identity…". This is decided: keep.

The lines that still hold are `:22` (package metadata, the publish card), `:26`, `:43` (consistent with card 367) and `:113`.

**F3. Card 186's test:**
- `:235-238` fails with "need at least 6 OPEN items dated 2026-11-08".
- The `COMPLIANT` fixture at `:58-60` holds the six OPEN lines, and `:61` holds "Recommendation: ask.".
- `:239-240` requires exactly one `Recommendation:` line per `## ` section, so the brief must be a `###` subsection, not a new `##` section.
- `:213-215` fails if the page contains any email address or the commit identity. The holder name is not the commit identity name, but the brief must carry no provider email.
- The placeholder clause is gone (card 366).

**F4. The wrappers:** `PrePublicationChecks.cs:23-25` runs `-m unittest tests.test_pre_publication_checks`, the whole module. A failing test added to that module turns the gate red. `LicenseFiles.cs` is the per-test-id wrapper pattern to follow.

**F5. The files are already correct:**
- `NOTICE:2` reads "Copyright 2026 Programming Made EZ".
- `TRADEMARKS.md:5` reads "Trademark holder: Programming Made EZ".
- `docs/LICENSING.md:27` labels 2026 as an assumption.

None of these change.

**F6. What the owner-items assertion should be instead.** All of these hold within Owner items:
- Exactly nine top-level items, each carrying "answered 2026-11-08" and its answer form.
- The search item has the exact phrase "commissioned by the owner; pending result".
- The lawyer item says "skipped by owner decision", and question 4 says it is answered by the holder item.
- Items 6 to 9 name cards 367 to 370.
- The public-name item says the registration status is pending the owner's search (the cold reader's point).
- A `### Trademark search brief` exists.
- Zero "OPEN" tokens. No item stays OPEN: the only thing waiting on the owner is the search result, which is shown as "pending result".

## 2. Scenarios

**Story:** As the sponsor persona, I want the pre-publication page to show each owner item as answered, with the trademark search brief, so that I see what is settled and what still waits.

```gherkin
Scenario: [O][M][S] each of the nine owner items shows its answer
  Given the owner's answers of 2026-11-08
  When the sponsor persona opens the Owner items section
  Then exactly nine items each read "answered 2026-11-08" with their answer
  And the search item reads "commissioned by the owner; pending result", the lawyer item "skipped by owner decision"
  And items still waiting name what they wait on (the search result, cards 367 to 370)

Scenario: [Z][E] nothing on the page still calls an answered item open
  Given the answered page
  When the sponsor persona reads any section
  Then no line says OPEN, "year placeholder", "holder needed", "stay open", "lawyer ask" or "commission a" search
  And a page with one answer reverted, missing or half-written is reported by the item it breaks

Scenario: [B][I] the owner can hand the provider a brief
  Given the commissioned search
  When the owner hands the brief to a provider
  Then the page's "Trademark search brief" names the mark Avowry, classes 9 and 42 and no others, and the jurisdictions
  And it lists every registry from the Name collisions section with its 2026-10-29 result and what was not checked
```

**How each scenario fails on partial work:**
- Scenario 1 fails if any of the nine items is unanswered or carries the wrong wording.
- Scenario 2 fails if any one stale line is left, or if the checker is vacuous.
- Scenario 3 fails if any brief element is missing, or if the brief and the Name collisions section disagree.

**Ids (D222):**

| Criterion | Ids |
|---|---|
| 371-1 | A1 name confirmed, and registration status pending the search. A2 holder Programming Made EZ, and 2026 as an assumption. A3 the exact search phrase, and publishing waits on the result. A4 lawyer skipped. A4q question 4 answered by the holder item. A5 keep. A6 read-first, card 367. A7 trim for new rows, pushed rows stay, card 368. A8 restore-and-review, card 369. A9 scrub-tracked, history stays public, card 370. A10 exactly nine items. A11 every item answered 2026-11-08. Unit rows E1 to E5, below. |
| 371-2 | Z1 zero OPEN in Owner items. S1 `:20`. S2 `:93`. S3 `:96`. S4 `:100`. S5 `:103` year placeholder. S6a, S6b and S6c are the three stale clauses of `:116`. S7 `:62` and S8 `:74`, if ruled in scope. Planted rows F1 to F6 (one stale phrase each). Negative row X1: the answered fixture gives an empty list. |
| 371-3 | B1 the mark. B2 class 9. B3 class 42. B4 no other class number. B5 a jurisdictions line. B6 all seven `TARGETS` with 2026-10-29. B7 the not-checked items (trademark databases, and the earlier brand called Phrase). B8 a link to the Name collisions section. |

The unit rows for 371-1 are planted pages, each reported by an exact failure line in the form `owner-items: <item>: …`:
- E1 the search item reverted to "OPEN, asked 2026-11-08".
- E2 the holder item deleted, which leaves eight items.
- E3 "commissioned by the owner" without "; pending result".
- E4 the lawyer item reading "ask".
- E5 a tenth item.

## 3. Named tests and what each shows on the current tree

All the new tests go in a new module, `tests/test_pre_publication_owner_items.py`. It imports `PAGE`, `sect` and `TARGETS` from the card 186 module. Its wrapper is a new `tests/Avowry.Acceptance.Tests/PrePublicationOwnerItems.cs` with `Slice=371`, one Pending Fact per criterion, and per-test-id runs as in `LicenseFiles.cs`.

| Test | Kind | On the current tree |
|---|---|---|
| `test_each_owner_item_shows_its_answer` (A1 to A9, A11) | NEW-STATE, 371-1 | Fails by assertion: every item reads "OPEN, asked 2026-11-08". |
| subTest A10, nine items | CHARACTERISATION (a boundary kept from the shape of the old count) | Passes: `:102-105` and `:110-114` are nine top-level items. |
| `test_the_owner_items_check_reports_a_missing_answer` (E1 to E5) | NEW-STATE unit, 371-1 | Fails against a stub checker that returns nothing. The real checker then lands in the same red. |
| `test_no_page_line_still_calls_an_answered_item_open` (Z1, S1 to S8) | NEW-STATE, 371-2 | Fails, listing each stale line. |
| `test_the_stale_line_check_reports_a_planted_phrase` (F1 to F6) and X1 | NEW-STATE unit, plus a negative row | The F rows fail against a stub. X1 passes against the stub and against the real checker. |
| `test_the_page_carries_the_trademark_search_brief` (B1 to B8) | NEW-STATE, 371-3 | Fails: there is no `### Trademark search brief`. |
| `PrePublicationChecks.Pre_publication_findings_match_the_repo` and `test_compliant_fixture_page_gives_empty` | CHARACTERISATION after the first red commit (headings, recommendation count, names, author metadata) | Pass before and after the green. |

**How card 186's test is revised (the first red commit, its own commit, `Criterion: 371-1`).** This is a tightening, the opposite of card 366's loosening. It is done by moving the assertion, not by editing it in place:

1. Delete the 6-OPEN clause at `:235-238`, and the six fixture lines at `:58-60`. Keep the `## Owner items` heading and `:61`, so the fixture still has one recommendation per section.
2. In the same commit, add the answered-form assertion as 371-1 in the new module, behind a Pending wrapper.

The red record must show three things:
- The new assertion fails by assertion on the unrevised page.
- The gated card 186 wrapper passes both before and after this commit, so main is never red.
- The `pending-red` CI job sees 371-1 fail by assertion.

The gate reviewer replays 371-1 on the pre-change page.

**The red's commits:** the first is the card 186 revision with 371-1. Then come 371-2 and 371-3, either as one commit each or as one commit with a `Criterion:` line for each (D204).

**The green** changes only `docs/release/pre-publication-checks.md` and removes `Pending` from the three wrappers.

## 4. Size and what is not in the card

**Size: S.** Four files: the page, `test_pre_publication_checks.py`, the new test module and the new wrapper.

**Not in this card:**
- The trademark search itself (the owner engages and pays the provider), and recording its result later.
- Cards 367 to 370: the vendor terms reading, the reply_head trim, the transitive licence review and the home-path scrub. The page only names them.
- The publish card: package metadata, packing NOTICE, and repeating the registry lookups on the publish day.
- NOTICE, TRADEMARKS.md, `docs/LICENSING.md` and LICENSE are unchanged. A "registration pending" line in TRADEMARKS.md would be a new card if the cold reader asks for one.
- The decision record, `HANDOFF.md`, and the board files (which are generated).

**Done when:**
- `dotnet test Avowry.sln --filter "Status!=Pending"` passes.
- `python -m unittest tests.test_pre_publication_checks tests.test_pre_publication_owner_items` passes.
- `git diff --quiet <base> <green> -- NOTICE TRADEMARKS.md docs/LICENSING.md LICENSE` exits 0.
- CI on main is green.

## 5. Task-force declaration

| Seat | Who and what |
|---|---|
| Product manager (Ready) | Cursor, read-only (this draft). |
| Architect (slice) | A fresh read-only Claude context. It fixes the exact item and stale-line sentences, the exact failure strings, where the brief sits (a `###` under Owner items, with the nine-item count stopping at it), and the jurisdictions line per the ruling. |
| Red (QA) | A fresh Claude context in its own worktree. It commits the card 186 revision with 371-1 first, then 371-2 and 371-3, with stubs first for the E and F rows. The red record quotes every A, Z, S, E, F and B id failing, and A10, X1 and card 186's test passing. |
| Green (developer) | A different fresh Claude context in its own worktree. It edits only the page, removes `Pending`, and runs both Python modules and the gate. |
| Cold reader (D206) | **Yes.** Qwen, with the whole page, NOTICE and TRADEMARKS.md inlined, reading as the sponsor persona. It also answers one question: can a provider start from the brief alone? It edits nothing and runs nothing. |
| Second read-only review | **Cursor, yes.** The red reverses the direction of a gated assertion, and the brief leaves the mob for a paid provider. On card 186 the second review caught real defects. It runs after the cold reader and before the gate review, never beside it (D226). |
| Independent gate reviewer | A fresh Claude context. Step 0 is the blocking ancestor check against the last commit after both reviews and any revision. It replays the reds on the pre-change page, checks the ids against section 2 (D222), and confirms the four files above are unchanged. |
| Sign-off | A further fresh Claude context that runs its own full gate. |
| Decider voters | Claude, Cursor, Qwen and Muse by rotation (short votes only for Muse). Grok, Codex, Copilot and agy are out of quota. |

**For the facilitator to rule on before the slice:**
1. **Jurisdictions.** The owner named none. I recommend a line labelled "proposed by the mob; the owner confirms with the provider", or asking the owner. B5 checks only that the line exists and carries its label.
2. **Lines `:62` and `:74`.** I recommend they are in scope (S7 and S8), because otherwise "the owner decides" stays in front of the sponsor persona after the owner has decided.
3. **Three Pending wrappers at once against D137.** Either extend the card 361 and 366 precedent, or run D137 strictly as three red-then-green pairs.
4. **The story actor.** I recommend the sponsor persona.
5. **Where the new tests live.** I recommend the new module. The alternative is to narrow `PrePublicationChecks.cs` to name its classes.
