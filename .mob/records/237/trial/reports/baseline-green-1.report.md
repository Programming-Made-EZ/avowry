# Green plan: card 227, Done check 1 (run baseline-green-1)

Seat: green (backend developer [author], ctx-5). Plan only; nothing is changed.

## Rules relied on
- `TEAM_WORKING_AGREEMENT.md` (the only process context), sections:
  - Engineering discipline: outside-in TDD with ZOMBIES (D137), honest red, Red -> Green -> Refactor -> Commit -> Push, push every commit (F-071), no `Co-Authored-By` trailers, stage explicit paths only.
  - Ceremonies: one acceptance criterion per red-green cycle (D157), claim equals commit (D154), thin slices (D136).
  - Safety: no recursive delete on a variable or computed path; a delete only on a literal scratch path; the command scan's limits.
  - Driver rotation and task force: the green author never reviews; the red seat's test is not edited by green.
- Board card `board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md`: Done check 1 and the adopted condition from window 227-taskforce (D145).
- No process-wiki page was read.

## State of the repository (read, not assumed)
- `scripts/delete_guard.py` has `find_unsafe(text)` and `safe_delete`. `find_unsafe` already flags `rm -rf $VAR`, `Remove-Item -Recurse -Force $var`, `shutil.rmtree(var)`, `rd /s` and `git clean -f`. It has no entry point that reads the commands a seat executed (no transcript or tool-call scan) and no Zero-case rule.
- There is no `test_delete_guard*` file, no `scan_transcript`, no `unpushed` verb in `scripts/mob_process.py`, and no `.githooks` directory in this checkout. The red commit for criterion 1 is not here.
- **Precondition:** green starts only from a pushed red commit whose body carries `Criterion: 1`, the Intent line, the Assumes line, the hostile-input answers and the Size line. I do not write or edit the red test. If it is absent, this step is blocked and goes back to the red seat.

## Smallest change that turns the red green
Add one function and one CLI verb to `scripts/delete_guard.py`, nothing else:
1. `scan_transcript(path)`: read a Claude seat transcript (JSONL), take the `command` field of each executed shell tool call (Bash and PowerShell tool_use blocks, sub-agent entries included when present in the file), pass each through the existing `find_unsafe`, and return findings carrying the seat and the command. The check reads the command, so a destructive command that never appears in the reply text is caught (D145 condition).
2. Zero case: a missing file, an empty file, or a file with no executed-command entries returns a failing result (exit 1), never "clean".
3. CLI: `python scripts/delete_guard.py scan-transcript <file>` exits 1 on any finding or an empty source, 0 otherwise.
4. Reuse `find_unsafe` unchanged. Its patterns already cover Remove-Item -Recurse -Force, rmtree and the other equivalents. Extend a pattern only if the red names a form that fails by assertion, and record which red test forced it.
5. Invariants kept: a literal path through `safe_delete` is not flagged; kill rules untouched; no `gh` calls.
6. Not done: trust-ladder entry (check 2), push detector and rule text (check 3), encoded or hidden-verb forms (card 228).

Process: run the red first and watch it fail for the stated reason (honest red). Write only enough to pass it. Refactor keeping every test as sensitive as before.

## Files touched (countable files and lines well inside D136: 15 files, 400 lines)
- `scripts/delete_guard.py` (the change above).
- The red test file (name set by the red commit): only the pending marker for criterion 1 is removed, in the same commit that makes it pass. No assertion is edited; if the test looks wrong, say so in the reply.
- Not touched: `board/`, `MOB.md`, `.mob/` (never staged), existing records, `TEAM_WORKING_AGREEMENT.md` (check 3 owns the push-rule text).

## Proof block for the green commit body
```
Card 227 green (Criterion: 1)
Proof:
- Red commit: <sha> (pushed; Criterion: 1). Red run before the change: <command> -> <N> failed by assertion, names: <tests>
- Green run after: <command> -> <N> passed, 0 failed
- Red-sensitivity: reverted only scan_transcript in a copy -> <named tests> red again (output pasted)
- Full gate: python -m unittest discover -s tests -> OK; dotnet test Phrase.sln --filter "Status!=Pending" unchanged, green, zero warnings
- Zero case: scan-transcript on an empty file -> exit 1; on a missing file -> exit 1
- Hostile-input answers (D167): each red-body entry mapped to a passing test name
- Files changed: scripts/delete_guard.py, <test file>; countable files <n>, lines <n> (D136)
- Criterion order from git log: red 1, green 1 (D157)
- Pushed: <sha> on origin/main at <time>; git log origin/main..HEAD empty
Not done: checks 2 and 3, card 228 forms.
```
The agreement defines no fixed "Proof block" layout; this one is built from honest red, red-sensitivity, D157, D136 and D154. Every line is a re-runnable command or pasted real output. No `Co-Authored-By` trailer is added: the agreement bans it (a `commit-msg` hook rejects it) and outranks the session's attribution reminder.

## Push step
1. Stage explicit paths only (`git add scripts/delete_guard.py <test file>`), never `.mob/`.
2. Commit with the Proof body and no trailers.
3. Push at once: `git push origin HEAD:main` (trunk-based, no PR, no branch). A commit unpushed for more than 5 minutes is a FAIL.
4. Confirm: `git log origin/main..HEAD` is empty and `git rev-parse HEAD origin/main` match. `python scripts/mob_process.py unpushed .` does not exist yet in this checkout (check 3 builds it), so run it only once it lands.
5. The facilitator then runs `python scripts/check-ci-main.py --wait 1500`; red is stop the line. The red for criterion 2 does not start before this green is pushed.

Size: 5724 bytes
