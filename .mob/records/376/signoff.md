# Card 376 sign-off (fresh Claude context, sign-off seat)

Tested commit (full): b0d0568cb36372a338bc37abad0633c534521d01 (main), clean clone with
`git clone --no-hardlinks --single-branch -b main` into a new directory under signoff376-clone/repo; `git rev-parse HEAD` in the clone printed that hash.
No account name, personal name, email or real home path appears below; planted names are invented.

## Verdict: SIGN-OFF

## 1. Full gate (run alone, nothing else dotnet running at start)
- `dotnet build src/Avowry.Cli`: Build succeeded, 0 warnings, 0 errors.
- `dotnet test Avowry.sln --nologo --filter "Status!=Pending" --logger "console;verbosity=normal"` (output to a file outside the repo), exit 0.
  Assembly 1: Total 344, Passed 344. Assembly 2 (Avowry.Acceptance.Tests): Total 399, Passed 399, 9.6 minutes. Sum: 743 passed, 0 failed, 0 skipped.
- Failures by name: none. The load-sensitive BenchmarkHarnessAcceptance test passed in the full run (no re-run needed).

## 2. Scripts in the clone
- `python scripts/check-no-secrets.py --tree`: exit 0; last lines `A8 other: 0 hits in 0 files`, `F10: checked 1 local account name(s)`, `0 hits in 0 files`.
- `python scripts/home_paths.py --root .`: exit 0, final line `0 files`.
- `python scripts/check-bench-history-clean.py`: exit 0, `bench\results\history.jsonl: clean`.
- `python -m unittest -v tests.test_account_name_376 tests.test_bench_history_clean tests.test_home_paths_370`: Ran 90 tests, OK. Skips: 0 (no skip lines; a local account exists here so W-A1..W-A8 and R1 ran live).
- `python scripts/mob_process.py check`: first run failed only because the clone's origin was a local path (gh found no GitHub host); after pointing the clone's origin at the real origin it printed `mob-process: OK (130 closed cards, 39 records, 91 legacy)`, exit 0.
- `git grep -I -i -w -l -e "$USERNAME" | wc -l` in the clone: 0.

## 3. The three scenarios, my own inputs (temp git repo, invented names)
- 376-2 `--tree`: tracked file holding a sentence with the planted name `zorbwick9`, with USERNAME=zorbwick9: exit 1, output `a.txt:1:F10`, `1 hits in 1 files`, no planted text printed. With USERNAME=otherperson77: `0 hits in 0 files`, exit 0.
- 376-2 hook: staged add of a line with the planted name, USERNAME=zorbwick9, `check-no-secrets.py` in staged mode: exit 1, `b.txt:1: account name (F10)`, no match text.
- 376-3 rewrite with the two-word account `Quillon Vance` (invented), via `home_paths.scrub_text`:
  backslash path -> `C:\Users\<user>\projects\x`; quoted -> `"C:\Users\<user>"`; slash -> `C:/Users/<user>/work/a.md`; URL `%20` form -> `file:///C:/Users/<user>/doc/x.md`; doubled backslash (checked with repr) -> `C:\Users\<user>\x`.
  Prose after a path kept: `C:\Users\<user>\src, then Vance in prose` (path rewritten completely, the later prose word untouched). Whole segment replaced in every form.
- 376-1: tree and bench both at 0 hits for the real account (items above); the bench rows B1..B6 pass in the unittest run.

## 4. CI (origin, main)
`gh run list`: b0d0568cb36372a338bc37abad0633c534521d01 completed success; d6a12bc74849344be48c1a3b0437f341b3bc959d completed success; 381b37fa672eaf5f81a2fbbe8e026d215e97e800 (red parent) completed success. The red and green commits (e915aa9, 88ac67e, 586c2cd, 69d764e) have no separate run; they were pushed together and are covered by the d6a12bc run. Nothing in progress.

## Reasons and noted limits (not blockers; all stated by the gate and second reviews)
- History still holds the old name; CI cannot see a real name (runner is on the generic list), so the guarantee is local plus the hook. Both were declared out of scope.
- Second-review notes 2 and 3 (closers such as `)` `,` `.` after a spaced name; bench path printed as given) are filed as a bug after the card; they do not fail any of the three scenarios.
