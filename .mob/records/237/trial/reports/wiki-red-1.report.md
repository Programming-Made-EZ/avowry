# Red plan: card 227, Done check 1 (run wiki-red-1, role red / QA seat)

## Rules and pages relied on
- `docs/process-wiki/INDEX.md`: the qa-red route row (core/index, role/qa-red, rules/one-criterion, rules/outside-in-tdd, howto/hostile-inputs). Those are the only wiki pages read.
- `docs/process-wiki/roles/qa-red.md`: write the intent line first (D165) for a criterion that implements an owner direction. The red body answers the hostile-input list and the assumption lines (D166, D167).
- `docs/process-wiki/rules/one-criterion.md`: each red commit names exactly one criterion (D157). This red names criterion 1 only.
- `docs/process-wiki/rules/outside-in-tdd.md`: the red commit adds the acceptance test for ONE criterion, marked pending, with a criterion trait (D137). The green commit removes the pending mark and leaves the test body byte-identical.
- `docs/process-wiki/howto/hostile-inputs.md`: this page is a one-line stub. It says to use the index row ids and statuses as the source of truth. It does not list the entries, so I answer the hostile forms the card names (see the hostile-input answers below) and flag the gap.
- Board card `board/227-...md`: Done check 1, the "Adopted condition (window 227-taskforce, D145)", and the Context line on D157, D165, D166 and D167.
- Repository as it is: `scripts/delete_guard.py` (`find_unsafe`, text only), `scripts/check-unsafe-deletes.py` (scans only added lines of committed files), and `tests/` (pytest style, `tests/test_mob_process.py` and others). I did not read `.mob/records/227*` or the git history of card 227.

## Intent line (first line of the red body, D165)
`Intent: product owner safety ruling 2026-10-31 ("one corrective card covering the scan fix and push-on-every-commit, because a mis-quoted delete ... could take out a repository including .git"): recursive deletes on variable paths are blocked, and the scan must see executed commands, not only reply text.`

## Assumption lines (D166)
- A1. Card 226's recorded slip is not available to me. I reconstruct it from the card text: a seat ran `rm -rf` on a variable scratch path (not known to be a literal scratch path) during card 226 criterion 1. I use the form `rm -rf "$SCRATCH/clone"`, and its Bash tool-call form. Green or the facilitator corrects the fixture if the record differs.
- A2. The command scan lives in `scripts/delete_guard.py` as a new public function, `scan_commands(source)`. It returns `Finding` objects, built on `find_unsafe`. `source` is a transcript JSONL path (one tool-call event per line, with `name` and `input.command`). A hook payload is the same event shape. The architect may rename it, and the test imports through one helper so the rename is one line.
- A3. "Fails" at the record check means a non-zero exit from `scripts/check-command-scan.py <transcript>...`, with the findings on stderr. A missing, empty or unscanned transcript for a seat also exits non-zero.
- A4. Safe literal paths stay allowed (the card's Invariants): `safe_delete` and a literal path inside a known scratch root produce no finding.

## Failing tests (all in one new file `tests/test_command_scan.py`, not written into the repo now)
Every test carries the criterion trait `Criterion=227-1` and `Status=Pending`, as one pending marker (D137). The ZOMBIES order is Zero, One, Many, Boundary, Interface, Exceptions, Simple.

Fixture helper `transcript(tmp_path, *commands, reply="")` writes a JSONL file with one Bash tool-call event per command and an assistant reply text line.

| # | Name | Case | Setup | Assertion |
|---|---|---|---|---|
| 1 | `test_zero_missing_source_fails` | Zero | the transcript path does not exist | `scan_commands` raises `CommandScanSourceError`. The CLI exits 1 and prints `command-scan source missing`. |
| 2 | `test_zero_empty_source_fails` | Zero | a 0-byte transcript | same error and exit 1, `command-scan source empty`. An empty source is not "clean". |
| 3 | `test_zero_no_tool_calls_is_clean` | Zero | a transcript with a reply and no tool calls | no findings, exit 0. This separates "nothing ran" from "could not read". |
| 4 | `test_one_executed_slip_reproduced` | One, executed-command case (D145) | one Bash call `rm -rf "$SCRATCH/clone"`, reply text only says "cleaning up" | exactly 1 finding, kind `recursive delete on a variable or unresolved path`, snippet contains `rm -rf`. The destructive command is absent from the reply text. |
| 5 | `test_one_reply_text_slip_still_found` | One | no tool call, reply text contains `rm -rf $TMP/x` | 1 finding, so the old reply scan is kept. |
| 6 | `test_many_forms_each_found` | Many (parametrised) | one command each: `rm -rf $d`; `rm -r "${WORK}/out"`; `Remove-Item -Recurse -Force $path`; `Remove-Item $p -Recurse`; `python -c "import shutil; shutil.rmtree(p)"`; `rd /s /q %TEMP%\x`; `rm -rf $(mktemp -d)` | each yields at least 1 finding. Parameter ids name the form. |
| 7 | `test_many_commands_one_transcript` | Many | 3 calls: safe `ls`, bad `rm -rf $a`, bad `Remove-Item -Recurse $b` | exactly 2 findings, in order. Each names the seat and the command (see 10). |
| 8 | `test_many_seats_each_scanned` | Many | 3 transcripts for seats `qa`, `dev`, `reviewer`, with the slip only in `dev` | the findings name `dev` only. The record check passes with all 3 present. |
| 9 | `test_boundary_chained_after_safe_command` | Boundary | `cd x && rm -rf "$D"` and `echo ok; rm -rf $D` | found in both. |
| 10 | `test_finding_names_command_and_seat` | Interface | the slip in the transcript of seat `qa` | the finding text contains the seat `qa` and the command string, matching "Fails with: a finding naming the command and the seat". |
| 11 | `test_exception_seat_without_transcript_fails_record_check` | Exception | the roster lists 3 seats, only 2 transcripts are supplied | the CLI exits 1, `unscanned seat: reviewer`. |
| 12 | `test_exception_malformed_line_fails_closed` | Exception | a transcript with one non-JSON line between two valid events | the CLI exits 1, `unreadable transcript line 2`. It does not silently skip it. |
| 13 | `test_exception_variable_path_in_record_check_fails` | Exception | the transcript with the slip, run through the record check | exit 1, with the findings listed. This is the "record check fails a variable-path recursive delete" in the card's Status line. |
| 14 | `test_simple_literal_scratch_path_allowed` | Simple (invariant) | `rm -rf scratch/run-1` and a `safe_delete(...)` call in a script | no finding, exit 0. |
| 15 | `test_simple_process_kill_rules_unchanged` | Simple (invariant) | `pkill node` | still 1 kill finding, kind `kill by process name or pattern`. |

Fifteen tests exceed no card limit I could verify (`rules/thin-slices.md` is not on my route and I did not read it). If the architect caps tests per criterion, cases 5, 9 and 15 are the first to drop.

## Hostile-input answers (D167, entries taken from the card since the how-to page is a stub)
- Disabled: a seat whose transcript source is disabled or absent fails (tests 1, 2, 11). It is never counted clean.
- Unknown: an unrecognised tool-call shape or malformed line fails closed (test 12).
- Repeated: the same slip twice in one transcript gives two findings, one per occurrence (extend test 7 with a duplicate line in green if wanted).
- Reordered: a bad command before or after a safe one is found in either order (tests 7, 9).
- Not covered in this criterion: hidden, encoded and non-shell forms and the contents of files a seat writes. They belong to card 228.

## Expected failing output
No `scan_commands` and no `scripts/check-command-scan.py` exist, so the tests fail on the missing code, not on an assertion:

```
$ python -m pytest tests/test_command_scan.py -p no:cacheprovider
FAILED tests/test_command_scan.py::test_zero_missing_source_fails - AttributeError: module 'delete_guard' has no attribute 'scan_commands'
FAILED tests/test_command_scan.py::test_zero_empty_source_fails - AttributeError: module 'delete_guard' has no attribute 'scan_commands'
FAILED tests/test_command_scan.py::test_zero_no_tool_calls_is_clean - AttributeError: ...
FAILED tests/test_command_scan.py::test_one_executed_slip_reproduced - AttributeError: ...
FAILED tests/test_command_scan.py::test_one_reply_text_slip_still_found - AttributeError: ...
FAILED tests/test_command_scan.py::test_many_forms_each_found[...] (7 params) - AttributeError: ...
FAILED tests/test_command_scan.py::test_many_commands_one_transcript - AttributeError: ...
FAILED tests/test_command_scan.py::test_many_seats_each_scanned - AttributeError: ...
FAILED tests/test_command_scan.py::test_boundary_chained_after_safe_command - AttributeError: ...
FAILED tests/test_command_scan.py::test_finding_names_command_and_seat - AttributeError: ...
FAILED tests/test_command_scan.py::test_exception_seat_without_transcript_fails_record_check - FileNotFoundError: scripts/check-command-scan.py
FAILED tests/test_command_scan.py::test_exception_malformed_line_fails_closed - FileNotFoundError: scripts/check-command-scan.py
FAILED tests/test_command_scan.py::test_exception_variable_path_in_record_check_fails - FileNotFoundError: scripts/check-command-scan.py
FAILED tests/test_command_scan.py::test_simple_literal_scratch_path_allowed - AttributeError: ...
FAILED tests/test_command_scan.py::test_simple_process_kill_rules_unchanged - AttributeError: ...
21 failed
```

Two of the tests would pass today by accident if they only called `find_unsafe` on a reply string. To avoid a false red, every test goes through `scan_commands` or the new CLI, so all fail for the right reason: the capability to read executed commands does not exist. Test 4 is the key one: today's `find_unsafe` is given only reply text, and the command is not in it, so the slip passes unseen.

Pending handling: the red commit marks all 15 tests pending (`Status=Pending`, `Criterion=227-1`), as the outside-in rule requires. The commit therefore leaves the suite green. The failure shown above is the run with the pending mark removed locally, which is what the red seat verifies before committing.

## Commit message (red)
```
Card 227: red for Done check 1, scan sees executed commands (pending)

Intent: product owner safety ruling 2026-10-31 ("one corrective card covering the scan fix and push-on-every-commit, because a mis-quoted delete like the 2026-10-08 home-folder wipe could take out a repository including .git"): recursive deletes on variable paths are blocked, in executed commands as well as reply text.

Criterion: 1

Adds tests/test_command_scan.py, 15 cases (21 with parameters), marked Status=Pending, Criterion=227-1:
Zero (missing and empty source fail, no tool calls clean), One (the executed slip absent from the reply text; reply-text slip), Many (7 command forms, many commands, many seats), Boundary (chained commands), Interface (finding names command and seat), Exceptions (unscanned seat, malformed line, record check fails), Simple (literal scratch path allowed, kill rules unchanged).

Assumptions: the slip is reconstructed as rm -rf "$SCRATCH/clone" because the card 226 record was not read; scan_commands and scripts/check-command-scan.py are the names the green will create; hidden and encoded forms are card 228.

Hostile inputs: disabled/absent source fails; unknown or malformed line fails closed; repeated slip gives one finding each; reordered commands found either way.

Without the pending mark the run fails with AttributeError (no scan_commands) or FileNotFoundError (no check-command-scan.py): 21 failed.
```

Size: 11548 bytes
