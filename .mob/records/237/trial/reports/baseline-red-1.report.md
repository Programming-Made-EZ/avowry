# Red plan: card 227, Done check 1 (criterion 1) - run baseline-red-1

Seat: red (QA engineer). Nothing is written into the repository except this file; the test code below is a plan, not a commit.

## Rules and pages relied on (all in `TEAM_WORKING_AGREEMENT.md`)

- Section 3 Practices, Outside-in TDD with ZOMBIES (D137): the red seat writes ONE acceptance test for ONE criterion; unit tests per layer follow; ZOMBIES letters or "N/A because" are shown.
- Honest red first: the test fails by assertion, for the right reason; Given/When/Then comments in every test.
- One acceptance criterion per red-green cycle (D157): the red commit body has one line `Criterion: 1`.
- Intent line before the first red (D165); hostile-input list answered in the red body (D167, the scan takes a list of command forms); the assumption line (D166) is the architect's slice line, I only cite it.
- No `Co-Authored-By` trailers (Practices, Engineering discipline): the commit message has none.
- Push every commit (card 227, F-071): the red is pushed straight after it is made.
- Stage explicit paths only; never stage `.mob/`.
- Thin slices (D136): limits 15 files / 400 lines; this red is 2 files.
- Safety section and "The command scan's limits": the scan is a detector after the fact, not a block; encoded forms, hidden-verb forms, files run later and non-Claude seats are not covered. The red claims no more.
- Board card `board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md`: Done check 1 and the Adopted condition (window 227-taskforce, D145).
- Independence: I do not author the green; the backend developer does.

## What exists today (`scripts/delete_guard.py`, 206 lines)

- `find_unsafe(text)` already flags `rm -rf "$VAR/..."`, `Remove-Item -Recurse -Force`, `shutil.rmtree(var)`, `rd /s`, `git clean -f` in TEXT.
- There is no `main`, no `scan-transcript` subcommand, no command-source reader and no per-seat attribution. The agreement's "command scan's limits" paragraph describes that scan, but the code is not in this tree. The missing behaviour: read the commands a seat EXECUTED (tool calls) and report them.
- Consequence: the red cannot import a function that does not exist (ImportError is the wrong reason). The acceptance test drives `python scripts/delete_guard.py scan-transcript <file>` with `subprocess`; today the script has no `__main__`, exits 0 and prints nothing, so the exit-code and output assertions fail by assertion.

## Intent (D165)

Intent: the product owner wants a variable-path recursive delete caught even when it was executed and never appeared in the reply, because "a mis-quoted delete like the 2026-10-08 home-folder wipe could take out a repository including .git" (ruling 2026-10-31, audit 62, finding F-071); condition of window 227-taskforce (D145): the scan must see executed commands, and an empty or missing source fails. Traced to the card text; nothing unconfirmed.

## The acceptance test (ONE, criterion 227-1)

File (plan): `tests/test_delete_guard_transcript.py`, class `ExecutedCommandScanTests`, unittest style like `tests/test_mob_process.py`.

`test_scan_reports_variable_path_recursive_delete_executed_by_a_seat_and_absent_from_its_reply`

```
# Given a seat transcript (JSONL, one tool call per line) whose reply text is clean
#   ("Cycle done, nothing was deleted.") and whose Bash tool call executed
#   rm -rf "$SCRATCH_DIR/clone-$n"   (reconstructed form of the card 226 slip, see Open point 1)
# When  python scripts/delete_guard.py scan-transcript <file> --seat backend-developer-ctx-4 runs
# Then  the exit code is 1
# And   stdout names the seat "backend-developer-ctx-4" and quotes the command "rm -rf"
# And   find_unsafe(reply text) == []   (control: the finding came from the executed command, not the reply)
```

Expected failing output today (assertion, not import error):

```
F
FAIL: test_scan_reports_variable_path_recursive_delete_executed_by_a_seat_and_absent_from_its_reply
AssertionError: 0 != 1 : scan-transcript exit code (no finding for executed 'rm -rf "$SCRATCH_DIR/clone-$n"')
Ran 1 test in 0.1s
FAILED (failures=1)
```

The reply-text control passes today; the failure is on the exit code first.

## Inner loop: unit tests the green builds, outside-in

Layer C: CLI wrapper (exit code, message). Layer A: `scan_transcript(path, seat)` (new; reads the source, yields commands; test doubles for the file reader). Layer B: `find_unsafe(command)` (exists, reused).

### ZOMBIES for the criterion

| Letter | Case | Layer | Today -> after green |
|---|---|---|---|
| Zero | transcript file is empty (0 bytes) | A/C | exit 0, no output -> exit 1, "no command source" naming the seat (adopted condition) |
| Zero | transcript path does not exist | A/C | exit 0 or traceback -> exit 1, "no command source: <path>" |
| Zero | commands present, none destructive | A | exit 0 -> exit 0 (guard, passes today; no false alarm) |
| One | single Bash call `rm -rf "$SCRATCH_DIR/clone-$n"` (the acceptance test) | C | exit 0 -> exit 1, KIND_DELETE finding with seat and command |
| One | single `Remove-Item -Recurse -Force $dir` executed | A | no finding -> 1 finding |
| One | single `shutil.rmtree(path)` inside an executed `python -c` | A | no finding -> 1 finding |
| Many | five calls: two safe, `rm -rf $a`, `Remove-Item -Recurse -Force "$b"`, `rd /s /q %TEMP%\x` | A | 0 findings -> exactly 3, in transcript order, each with its own command text |
| Many | the same destructive command executed twice | A | 0 -> 2 findings (repeats not collapsed) |
| Boundaries | `safe_delete(...)` with a literal scratch path; `rm -rf ./scratch/literal-dir` | A | exit 0 before and after (invariant: literal scratch path allowed; passes today, must stay passing) |
| Boundaries | `rm -rf` target is a bare variable, `~`, `/`, or empty | A | no finding -> a finding for each (reuses `_unsafe_target`) |
| Boundaries | destructive command on the last line with no trailing newline | A | no finding -> 1 finding |
| Interface | `scan-transcript <file> --seat NAME`: exit 0 clean, 1 findings, 2 usage error (no `--seat`); stdout one line per finding `seat: kind: command` | C | subcommand absent -> as stated |
| Interface | executed `pkill python` reported as KIND_KILL (kill rules unchanged) | A | no finding -> 1 finding |
| Exceptions | a line that is not JSON | A/C | traceback or exit 0 -> exit 1, "unreadable line N" (never throws, never skips silently) |
| Exceptions | a tool call with no command field or an unknown tool name | A | -> exit 0, treated as "not a command", no crash |
| Exceptions | a non-Claude seat, no transcript supplied | C | -> exit 1, "no command source" (agreement: such a seat's record entry is `no command source` and fails) |
| Exceptions | file unreadable (permission error) | A/C | traceback -> exit 1 with a message, no throw |

### Executed-command case (adopted condition, D145)

`test_destructive_command_absent_from_reply_text_is_found_in_the_executed_commands`: the assistant text is `"Done."`, the Bash tool call is `Remove-Item -Recurse -Force "$env:TEMP\$name"`. Today: `AssertionError: 0 != 1 : scan-transcript exit code`. After green: exit 1, command quoted.

### Zero case (adopted condition)

`test_empty_or_missing_command_source_fails`: Given a 0-byte file and a missing path; When scanned; Then exit 1 each time and stdout contains `no command source`. Today: `AssertionError: 0 != 1` for the empty file; the missing path also fails by assertion because the text check is part of the test (a usage error or traceback is not accepted as a pass).

Order: acceptance test first (committed pending); then unit reds one at a time (Zero source, reader, then each command form); the acceptance test stays red until the last unit lands.

## Hostile-input list answers (D167)

Inputs: disabled: n/a: no feature toggles a command form off, the scan has no disable list; unknown: a delete behind a wrapper verb (`Invoke-Expression "rm -rf $x"`) is a residual the agreement names ("hidden-verb forms"); `test_wrapper_verb_is_a_known_residual` documents current behaviour and is not red; repeated: the same destructive command twice gives two findings; reordered: destructive command first, last and between safe ones gives the same findings.

## Pending mechanism (the delivery lead confirms)

`Status=Pending` / `Criterion=NNN-K` is a .NET convention (`dotnet test --filter "Status!=Pending"`). This repo's Python tests are plain `unittest`, and I found no Python equivalent. Plan: commit the acceptance test with a skip decorator whose reason is `Status=Pending Criterion=227-1`, and run it with the skip lifted locally to capture the failing output above. Risk: a skip is not "fails by assertion" for a `pending-red` CI job; the delivery lead decides whether that job needs a Python variant. Main must stay green meanwhile.

## Open points

1. The exact slip text is not recorded verbatim. The card 226 record says only: "ran rm -rf once on a variable path (a scratchpad folder that did not exist)". `rm -rf "$SCRATCH_DIR/clone-$n"` is my reconstruction; swap in the real text if the card 226 transcript holds it.
2. Transcript format (Claude Code JSONL: assistant message, `tool_use`, `name: Bash`, `input.command`) is assumed; the architect's slice should confirm it. Sub-agent seats are scanned by hand until card 228 (agreement), so the test covers main-seat transcripts only.
3. Done checks 2 and 3 are out of scope for this red.

## Commit message

```
Card 227: red - scan-transcript must report a variable-path recursive delete a seat executed

Pending acceptance test for Done check 1: a seat's executed Bash command
rm -rf "$SCRATCH_DIR/clone-$n" is absent from its reply text; the scan must
exit 1 naming the seat and the command. Also pending: an empty or missing
command source fails (adopted condition, window 227-taskforce, D145).

Criterion: 1
Intent: the owner wants a variable-path recursive delete caught from the commands a seat executed, not only its reply (ruling 2026-10-31, audit 62, F-071; D145).
Inputs: disabled: n/a: no disable list; unknown: wrapper verb is a documented residual (test_wrapper_verb_is_a_known_residual); repeated: test_same_destructive_command_twice_gives_two_findings; reordered: test_destructive_command_first_last_and_between.
Failing today by assertion: AssertionError: 0 != 1 (scan-transcript exit code); no subcommand exists.
Status=Pending Criterion=227-1
```

Staged by explicit path: `tests/test_delete_guard_transcript.py` and `tests/Fixtures/transcript_rm_variable.jsonl`. Pushed straight after the commit; no trailer.

Size: 10609 bytes
