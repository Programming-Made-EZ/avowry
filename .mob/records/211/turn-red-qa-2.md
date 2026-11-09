211
Intent (D165): Make card 211 criterion 2 red: a committed record test that fails until mob-defaults holds a re-run record showing a non-author context ran the suite and the audit checks on 687410e, on e3736d1 and on the head.
Assumes (D166): "author context" means a seat context that took part in writing the 27 commits (D177: context attribution, not identity); the record declares its author_contexts and the test cannot check that list against git; probe: mob_process.py has the subcommands record, findings, pending, audit, audit-due, ladder, unpushed (no `check` in mob-defaults; `check` is the records repo's scripts/mob_process.py), and no records/211/rerun.json is tracked.

Seat: QA engineer, RED 2, criterion 2 only, cap 1 cycle (D175). Context ctx-3 (fresh, red).

Record path: records/211/rerun.json in mob-defaults, next to records/211/votes.json.
Fields: range; seat_context; author_contexts; runs[] of {kind suite|audit, name (audit only: one of the seven mob_process.py checks), label range_start|range_end|head, commit, command, cwd, shell, launch (D176 environment line), seat_context, result {status pass|fail, summary}, finding (required on fail)}.
Audit checks that exist in mob-defaults: python mob_process.py record|findings|pending|audit|audit-due|ladder|unpushed (ladder and audit take the repo path).

Test: tests/test_card_211_rerun_record.py, committed 53ec0c3, pushed to origin main (no other unpushed commit; git status showed only untracked .mob/).
Asserts: the record exists; range is 687410e..e3736d1; each of range_start, range_end and head has a suite run and an audit run; range_start resolves to 687410e and range_end to e3736d1; every run has command, cwd, shell, launch, result with summary, a resolvable commit and a seat_context equal to the re-run's and not an author context; a failed run needs a finding (failures are not hidden); author_contexts is non-empty; audit names are real checks.
ZOMBIES: Z no record or no run; O only the head checked; M all three commits pass; B each missing commit fails, endpoint label on the wrong commit fails, suite without audit fails; E empty command, missing environment key, missing result or commit, hidden failure, author context, unknown audit name.

Run on committed code (git status showed no modified file), from C:\Users\<user>\mob-crosscli, Git Bash, HEAD 53ec0c3: python -m unittest tests.test_card_211_rerun_record. Modules run: 1 (not the full suite). Ran 14 tests, FAILED (failures=1): the real-record test, "no re-run record records/211/rerun.json". The 13 fixture tests pass.

Open for green: a non-author context (not ctx-3 as author of this red, and not a context of the 27 commits) must run the suite and audit checks in fresh git worktrees at 687410e, e3736d1 and head, and write records/211/rerun.json. Scratch folder left in place: C:\Users\<user>\AppData\Local\Temp\claude\qa211-2-25250 (commit message only).
Size: 2916 bytes
