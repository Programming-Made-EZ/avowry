# Card 361 characterisation (D204), QA (red) seat

Declared CHARACTERISATION, not a red. These tests must pass on the pre-change tree: there is no production change in this worktree (nothing under `src/`, no docs change, no rename). Base: `57f94f200eb0e7e8256edcc4289e3b4245847128` (main, the architect's slice commit). Branch `wt-361-red`.

Reading note: this record lives under `.mob/`, which is on the history allow-list, so it may quote old-prefix codes as evidence. Every file under `tests/` written by this seat is clean under the scanner (character class `P[H]` in the pattern, samples built from parts).

## Declared up front

| Id | Test | Kind | Why it passes today and after the green |
|---|---|---|---|
| (i) | `CodesKeepTheirNumbers.Every_trigger_source_reports_the_same_number_position_and_message` (19 rows, one per trigger source of `sources.txt`) and `...The_snapshot_covers_every_trigger_source_and_holds_no_letters_before_a_number` (Compiler.Tests) | CHARACTERISATION | Compares the built diagnostics with the two-letter prefix removed (`ordinal`, 3 digits, line, column, message) to the prefix-free snapshot `tests/Avowry.Compiler.Tests/Diagnostics/numbers-361.txt`, generated from the pre-change tree. The snapshot has no prefix at all, so it holds before and after. It is the only evidence that the rewrite kept the numbers: the existing pins are rewritten by the same green. It also asserts each code is two letters plus three digits. |
| (ii) | `NoPhCodesInTheLiveTree.Every_history_allow_list_entry_still_holds_a_PH_code` (H1 `.mob/`, H2 `docs/decisions/`, H3 `docs/retros/`, H4 `bench/results/`, H5 `board/`, H6 `MOB.md`) | CHARACTERISATION | Each history area still holds at least one old-prefix code; a dead entry or a scrubbed history fails it, and the message prints the entry's reason. Reasons: records are dated history; decision records are append-only; retros are dated; pushed bench rows are schema; `board/` is generated from the issues; `MOB.md` is generated from the records. |
| (iii) | `NoPhCodesInTheLiveTree.Scanner_planted_rows` (22 rows) and `Scanner_reports_the_line_and_whether_the_hit_is_a_three_digit_code` | NEW-STATE unit, born with the real scanner (watched failing against a stub first) | The scanner is a pure function over (path, text) pairs; regex (case-sensitive, no left anchor): `P[H](?:[0-9]|n{3}|\b)`. Must hit (10 rows): a code, a fixture id with a hyphen before it, a class name ending in the prefix and a number, the two-digit string literal, the backslash-d regex text, the quoted bare prefix, the `nnn` form, the hash-run form, a bare word, the working-agreement ledger row. Must not hit (12 rows): `Phrase`, `PHRASE_BENCHCS_DOTNET`, `PHRASE_X`, `ORPHANS`, lowercase, the new prefix and `AVnnn`, and six rows planting a code under each allow-listed path. All samples are built from parts. |

## Commands and pass lines (pre-change tree)

`dotnet build src/Avowry.Cli`: `0 Error(s)`.

Stub scanner first (`HitsIn` returned an empty list), `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~NoPhCodesInTheLiveTree"`: 12 failures by assertion, 10 planted must-hit rows (`hit-code`, `hit-fixture-id`, `hit-no-left-boundary`, `hit-two-digit-prefix-literal`, `hit-backslash-d`, `hit-quoted-bare`, `hit-nnn`, `hit-hash-run`, `hit-bare-word`, `hit-working-agreement`: "expected a hit for ..., got 0"), the line/code-shape test, and the history test ("H1 .mob/: no old-prefix code left ..." through H6).

```
Failed!  - Failed:    12, Passed:    13, Skipped:     0, Total:    25, Duration: 10 s - Avowry.Acceptance.Tests.dll (net8.0)
```

(The 13 passes on the stub are the 12 must-not-hit rows and the Pending test, which finds nothing against a stub; the Pending test is the next commit's business.)

Real scanner, `dotnet test tests/Avowry.Compiler.Tests --nologo --filter "FullyQualifiedName~CodesKeepTheirNumbers"`:

```
Passed!  - Failed:     0, Passed:    20, Skipped:     0, Total:    20, Duration: 98 ms - Avowry.Compiler.Tests.dll (net8.0)
```

`dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~NoPhCodesInTheLiveTree&Status!=Pending"`:

```
Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24, Duration: 485 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

## Mutations (uncommitted, restored)

1. `numbers-361.txt` line 2 changed from `1|010|7|1|` to `1|011|7|1|`, then restored (verified by reading the line back):

```
Failed Avowry.Compiler.Tests.Diagnostics.CodesKeepTheirNumbers.Every_trigger_source_reports_the_same_number_position_and_message(id: "M01-PH010")
Expected: ["1|011|7|1|Avow 'ping {m: Int}' has the same wordin"···]
Actual:   ["1|010|7|1|Avow 'ping {m: Int}' has the same wordin"···]
Failed!  - Failed:     1, Passed:    19, Skipped:     0, Total:    20
```

2. Scanner pattern cut to `P[H]` (no digit/nnn/word-edge tail), then restored (byte-compared with a backup): the three must-not-hit rows that a loose pattern would catch fail.

```
miss-upper-phrase: expected no hit for tools/Program.cs, got 1
miss-underscore: expected no hit for tests/G.cs, got 1
miss-orphans: expected no hit for TEAM_WORKING_AGREEMENT.md, got 1
Failed!  - Failed:     3, Passed:    19, Skipped:     0, Total:    22
```

## Python neighbours on the pre-change tree

`python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map tests.test_getting_started tests.board.test_story_persona`: `Ran 78 tests`, `FAILED (failures=8)`: five in rename_196 (`test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`), two in rename_197 (`test_lists_equal_baseline` x2), one in live_docs_266 (T11). These are the pre-existing eight.
