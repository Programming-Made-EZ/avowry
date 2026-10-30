# Card 209 slice (architect, ctx-6)

Decision: SHIP AS IS as one card (no split) under D136 and D157. One concern: a single refusal rule, written once and read by both entry points. Three criteria, three increments, 10 commits, the largest about 120 changed lines and 3 files. Files estimate: 5 files in mob-defaults (facilitator.py, mob_process.py, templates/TEAM_WORKING_AGREEMENT.default.md, tests/test_card_mode.py, tests/test_mob_process.py) plus one new test file for the template scan.

Seat: architect (slice), context ctx-6, Claude; reason for a Claude seat: probes at 12:03 UTC show Codex, Copilot, Grok and agy without usable quota.

## 0. What the code does today (read at mob-crosscli 46864d0)

- Removals are parsed in facilitator.py `parse_task_force` (line 594): the `removed` field is the text after `removed:` up to the next `|`. A **blank** value (`removed:` and nothing after it) is stored as "" and so reads as "no removal": an ignored malformed line today.
- The only refusal is `check_task_force` (facilitator.py:712): a role that has `joined` **and** `removed` other than "retro". A removal of an original member at any cycle is accepted. `sync_task_force` (line 938) then needs only a passed Decider, records `{"change": "remove", "cycle": r.removed}` in `task_force_changes`, and `mob_seats` (line 624) drops the role from the mob from the next cycle.
- A role that is new **and** removed in the same edit lands in `added` (line 950: `left` requires `r.role in self.known_roles`), so it is never refused. This is the "E" case of check 1.
- mob_process.py has **no card parser**. Its entry point is `main(["record", repo, card_id])` and it reads the machine record `.mob/records/NNN.json`. `check_record` (line 143) checks `task_force_changes` only for a reason and a decider; it never looks at `change` or `cycle`. So "the same refusal in mob_process.py" means: a `task_force_changes` entry with `change == "remove"` whose `cycle` is not "retro". facilitator.py already imports from mob_process (line 43), so the shared rule lives in mob_process.py (the reverse import would be circular).
- The template (templates/TEAM_WORKING_AGREEMENT.default.md): line 288 "Adding or removing a member mid-card is a Decider proposal", line 290 `| removed: cycle 3 | decider: D5`, lines 292-294 "Added members stay until the retro" (the contradiction), line 300 "A mid-card addition or removal that carries no passed Decider is refused", line 385 retro keep-or-remove.

### Card text that does not match the code (for the PM seat to correct before red)

1. **tests/test_card_mode.py:914 is not a removal test.** Line 914 sits in a record-gap test just before `class TrueMobbing`. The tests that assert the old behaviour are:
   - `test_a_removal_is_recorded_too` (line 879; `removed: cycle 1 | decider: D1`),
   - `test_a_member_removed_mid_card_by_decider_leaves_the_mob_from_the_next_cycle` (line 1069),
   - `test_a_removal_without_a_passed_decider_is_refused` (line 1086; `removed: cycle 1`, no Decider),
   - `test_a_removal_cannot_leave_the_mob_without_a_driver_navigator_and_observer` (line 1096; `removed: cycle 1`),
   - `Units.test_a_member_added_mid_card_can_only_be_removed_at_the_retro` (line 1361; a joined role, `removed: cycle 2`; stays valid).
2. **The suite cannot go green on criterion 1 with those tests in place:** 879, 1069 and 1096 write `removed: cycle 1` and expect success or an observer refusal. Their rewrite or deletion therefore lands in the **criterion 1 green commit** (a green suite cannot hold a test of the removed behaviour). Plan: 879 and 1096 become `removed: retro` (they keep guarding the retro removal and the driver/navigator/observer floor); 1069 is **deleted** (item 3); 1086 stays unchanged (still refused, now by the rule). Criterion 3's "test deleted" is thereby true from green 1; criterion 3 red is the template scan only, and "no cycle-removal test remains" is a delivery-lead command (`grep -n "removed: cycle" tests/*.py`: only refusal cases from criteria 1 and 2 may match), not a test.
3. **Open point, kept out of the card:** an accepted `removed: retro` on the card today still drops the seat from `mob_seats` from the next cycle (facilitator.py:624), so written mid-card it acts like a mid-card removal. The card invariant says the retro removal is unchanged, so this slice does not touch it; test 1069 is deleted rather than converted so the suite stops asserting "removal drops the seat mid-card". PM decides whether to file a follow-up (suggest yes, one board line).
4. **B in check 1** ("removed: retro with trailing spaces or capitals is the boundary of the accepted form") is ambiguous. The code accepts "Retro" and " retro " today (parse strips, the comparison lower-cases). Slice decision: keep that. Accepted: `retro`, case-insensitive, surrounding whitespace ignored. Refused: `retro cycle 3`, `retrospective`, `retro2`. QA pins this in the red.
5. **One message, defined once:** `role '<Role>' removal '<value>' refused: removals happen only at the retro (write 'removed: retro')`. Both entry points emit it from the same function. A blank value is refused with the same sentence (`removal ''`). A removal naming a role that is not on the task force gets its own message: `role '<Role>' is removed but is not on the task force: removals happen only at the retro, and only for a declared member`.

## 1. Seams (smallest set)

| Seam | Verdict | Why |
|---|---|---|
| S1 `mob_process.REMOVAL_RULE` (constant) and `mob_process.removal_problem(role, removed, on_task_force=True)` returning a message or None | NEEDED | The single place where "only removed: retro" lives. facilitator `check_task_force` and mob_process `check_record` both call it, so the two cannot disagree (check 2). Pure, no I/O. |
| S2 `parse_task_force` marks a present-but-blank `removed:` as a refused removal | NEEDED | Today blank is ignored (E in check 2: malformed is refused, not ignored). Smallest change: when the field is found with an empty value, store the sentinel `"(blank)"` in `TaskForceRole.removed` (still a str, no new field). |
| S3 `sync_task_force` passes `on_task_force=False` to S1 for a role in `added` that carries `removed` | NEEDED | The only way the "not on the task force" case is reachable from a card edit. |
| Fake drivers for the CLI | none new | The acceptance layer reuses `FAKE_REPLACE_IN_CARD` and `FAKE_DECIDER_RECORD` (tests/fakes/fake_cli.py). A **passed** Decider is supplied on purpose, so the refusal is shown to come from the rule and not from a missing Decider. |

No feature flag (D136 part 3): this is a refusal added to tooling, not incomplete user-visible syntax.

## 2. Criterion-by-layer table (outermost first; D157 numbering)

Layers: **L1** CLI entry (`facilitator.py --card ...` through `CardModeTestCase.run_facilitator`; `mob_process.main(["record", ...])`); **L2** `sync_task_force`; **L3** `check_task_force` (card text, pure) and `check_record` (record dict, pure); **L4** `parse_task_force` (pure); **L5** `removal_problem` (pure).

### Criterion 1: refusal in facilitator.py (Done check 1; ZOMBIES Z O M B E I)

| Case | Layers, outermost first | Red-able today? | Doubles consumed |
|---|---|---|---|
| O `removed: cycle 3 \| decider: D5` refused naming the member and "removals happen only at the retro"; exit 2 | **L1 acceptance** -> L2 -> L3 -> L5 | red-able: accepted today, exit 0 | fake CLI, FAKE_REPLACE_IN_CARD, FAKE_DECIDER_RECORD (D5 passed) |
| M several removals, one `removed: retro` accepted, the others refused (message lists only the refused) | L3 -> L4 -> L5 | red-able | none (card text strings) |
| B cycle 1 and the last cycle refused; `retro`, `Retro`, ` retro ` accepted; `retro cycle 3`, `retrospective` refused | L3 -> L5 | refused half red-able; accepted half **born-green** (G2) | none |
| E removal naming a role not on the task force (new and removed in one edit): own message, exit 2 | L1 (second acceptance case) -> L2 -> L5 | red-able (goes to `added` today, accepted) | FAKE_REPLACE_IN_CARD |
| E blank `removed:` refused, not ignored | L3 -> L4 | red-able (parsed as no removal) | none |
| Z no removal line: roster unchanged, no problem | L3 | **born-green** (G1) | none |
| I accepted `removed: retro` leaves the member in every earlier cycle's record (`task_force` still lists the role; the change entry has cycle "retro") | L1 (CLI, retro card) | **born-green** (G3; recording already does this) | FAKE_REPLACE_IN_CARD, FAKE_DECIDER_RECORD |
| `removal_problem` itself: O, B, E; message names role, value and rule | L5 | red-able (stub returns None) | none |

### Criterion 2: same refusal in mob_process.py (Done check 2)

| Case | Layers | Red-able today? | Doubles consumed |
|---|---|---|---|
| O record with one `{"change": "remove", "cycle": "cycle 3"}`: `main(["record", repo, id])` exits 1 and prints the message | **L1 acceptance** -> L3 `check_record` -> L5 | red-able (no check) | tmp repo with a record file, stdout capture (as test_mob_process.py:447 does) |
| M one retro and one cycle removal in the same record: only the cycle one flagged | L3 -> L5 | red-able | record dict |
| B removal in cycle 1 flagged; `retro` (any case) not | L3 | cycle half red-able; retro half **born-green** (G4) | record dict |
| E a remove entry with no `cycle` (malformed) flagged; a remove for a role absent from `task_force` flagged with its own message | L3 | red-able | record dict |
| I the problem text names the member and contains "removals happen only at the retro" | L3 | red-able | record dict |
| Agreement: for each of {cycle 3, cycle 1, blank, retro} the message from `check_task_force` and from `check_record` is the same string | L3 (both) -> L5 | red-able (the record side returns nothing) | card text and record dict |
| Z no remove entry: no removal problem | L3 | **born-green** (G4) | record dict |

### Criterion 3: template and test removed (Done check 3)

| Case | Layers | Red-able today? | Doubles consumed |
|---|---|---|---|
| Z the template has no `removed: cycle` text | **L1 acceptance** = scan of `templates/TEAM_WORKING_AGREEMENT.default.md` | red-able (line 290) | none; the real file via `Path(__file__)` |
| M every template line that names removing a member says retro (lines 288, 290, 300 agree with 292) | same scan, one test | red-able | none |
| B the old line is gone, not commented: no `<!--` block containing the old syntax | same scan | red-able | none |
| O one or more `removed: retro` remains | same scan | **born-green** (G5) | none |
| E a card using the old syntax fails the check | covered by Criterion 1 (O, L3); not re-tested here | n/a | n/a |
| I the full suite stays green | delivery-lead command, not a test | n/a | n/a |
| The cycle-removal tests are gone | done in green 1 (section 0 item 2); grep evidence at ship | n/a | n/a |

Scan self-reference: the scan builds its needle by concatenation (`"removed: " + "cycle"`) and reads only the template, never test files, so it cannot match itself.

Overlap: the red L5 tests of Criterion 1 are the proof layer for Criterion 2 as well (no second copy); check 2 adds only the `check_record` call site and the agreement test.

## 3. Born-green cases: named guard tests and mutations (card 214 slice section 5 applied)

Five tests pass before their green, so each is named `test_guard_<what>` and carries a one-line mutation applied in a **scratch copy** of mob-crosscli (never committed) that turns that test red and no other. The failing output (test name, assertion text, the mutation as a one-or-two-line diff) is pasted in the body of the commit that adds the test, before it lands. For a guard the green rewrites, the same mutation is re-run on the green tree and pasted in the green commit body. The reviewer (ctx-4) may re-run any of them. A guard whose mutation does not turn it red is removed or rewritten in the same commit.

| # | Test | Criterion | Guard | Mutation that must turn it red | Lands in commit |
|---|---|---|---|---|---|
| G1 | test_guard_no_removal_line_leaves_the_roster_unchanged | 1 Z | `check_task_force` reports nothing for a card without removals | make `removal_problem` return the message for `removed == ""` | 2 |
| G2 | test_guard_retro_removal_is_accepted_in_every_spelling | 1 B | `retro`, `Retro`, ` retro ` accepted | compare case-sensitively (drop `.lower()`) or drop the strip | 2 |
| G3 | test_guard_retro_removal_keeps_the_member_in_the_record | 1 I | removed role stays in `task_force`; change has cycle "retro" | filter removed roles out of `record["task_force"]` (facilitator.py:1039) | 1 |
| G4 | test_guard_record_without_removals_and_with_retro_has_no_removal_problem | 2 Z, B | `check_record` is silent on no removal and on retro | make `check_record` flag every `change == "remove"` regardless of cycle | 6 |
| G5 | test_guard_template_still_documents_the_retro_removal | 3 O | `removed: retro` remains in the template | delete the `removed: retro` sentence in the template | 8 |

Ordering: G1 to G3 are committed before green 1, G4 before green 2, G5 before green 3. At each green the reviewer sees all reds green, all guards still green, and the pasted mutation output.

## 4. Commit plan (D157: one criterion per red-green cycle; "Criterion: n" line in each red body; each green pushed before the next criterion's red)

Subjects start `Card 209 ... (MD-001, D145, D157)`. Countable files exclude board/, docs/decisions/, .mob/records/, docs/retros/. D136 limits per commit: at most 15 files and 400 changed lines.

| # | Commit | Files | Lines (est.) | Criterion |
|---|---|---|---|---|
| 1 | red: acceptance (O CLI refusal with passed D5; new-and-removed CLI case; G3) | tests/test_card_mode.py | 70 | 1 |
| 2 | red: unit layers 3 and 4 (check_task_force Z/O/M/B/E; blank-removed parse; G1, G2) | tests/test_card_mode.py | 110 | 1 |
| 3 | red: unit layer 5 (`removal_problem` cases) plus the S1 stub (`REMOVAL_RULE`, `removal_problem` returning None) | mob_process.py, tests/test_mob_process.py | 70 | 1 |
| 4 | **green 1, pushed:** S1 real, S2 blank sentinel, S3 on_task_force, `check_task_force` calls S1; tests 879 and 1096 edited to `removed: retro`, 1069 deleted | facilitator.py, mob_process.py, tests/test_card_mode.py | 120 | 1 |
| 5 | red: acceptance (`main(["record", ...])` exits 1 with the message) | tests/test_mob_process.py | 50 | 2 |
| 6 | red: unit layer `check_record` (M, B, E, I, agreement; G4) | tests/test_mob_process.py | 90 | 2 |
| 7 | **green 2, pushed:** `check_record` calls S1 per remove change | mob_process.py | 25 | 2 |
| 8 | red: template scan (Z, M, B; G5) | tests/test_template_removal.py (new) | 45 | 3 |
| 9 | **green 3, pushed:** template lines 288, 290, 300 rewritten retro-only; facilitator.py comment at line 573 loses "cycle 3" | template, facilitator.py | 25 | 3 |
| 10 | closure evidence (D154 gate output with the Criterion log order; CI on main; board note) | .mob/records only | 0 countable | all |

Largest commit: green 1, about 120 lines and 3 files, far under 400 lines and 15 files. The git log order of Criterion lines must read 1,1,1, green 1, 2,2, green 2, 3, green 3 (D157 C4 gate line). No red is committed in advance.

## 5. Slice field and the domain-expert question (T2)

Slice field: SHIP AS IS | 5 files over 10 commits, none above 120 lines or 3 files | 3 increments (one per criterion) | one concern: the removal rule is written once and read by both entry points.

Domain-expert question at the slice boundary (T2): should an agent with different domain knowledge or context join now? **No.** The work is a pure rule in two Python modules plus a template paragraph; every seam and risk is inside facilitator.py, mob_process.py and the template, which the backend developer, the QA engineer and the independent reviewer (audit lens) already cover. Nothing touches a trust boundary, a workflow or hook, a user interface or a language design. The two process points found (the stale line reference, and the retro removal still dropping a seat mid-run) are for the PM seat to correct in the card, not a missing perspective.
