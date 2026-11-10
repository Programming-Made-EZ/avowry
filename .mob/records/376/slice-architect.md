# Card 376 slice (architect seat, read-only Claude context; every figure recomputed by command on main at 367e3e2; the account name is never printed)

## 0. Figures recomputed, and what in the draft is wrong or loose

- Whole-word count confirmed: `git grep -I -i -w -l -e "$USERNAME"` lists 8 tracked files and 9 lines, exactly the draft's list: .mob/records/226/record-in-progress.json:208; 229/nohome-check/nohome229_check.py:20 and :48; 276/record-in-progress.json:38; 276/signoff-ctx6.md:25; 313/green1-codex-events.jsonl:5; 313/slice-architect.md:123; scripts/check-bench-history-clean.py:9; tests/test_bench_history_clean.py:28. By area: A1 7 lines in 6 files, A3 1/1, A4 1/1, all others 0. An independent whole-word regex (Unicode, `_` not a letter) gives the same 9 lines in 8 files.
- Machine facts: `getpass.getuser()` equals `Path.home().name`; the name is one word, no space; USERNAME is set, USER and LOGNAME are not (Windows).
- Line endings: all 8 files are LF with a final newline (use Python bytes or `git ls-files --eol`; Git Bash `grep -c` claims CRLF, an artefact). Byte-exact rewriting still applies.
- Draft line numbers all correct: home_paths.py SEGX :27, SEG :28, path_sub :84-87, find :138-156, _SEG_STR :104; check-no-secrets.py `return 0` :161, scan_tree :139-161, main :164-168, docstring :12-13, scan_staged :119-136; hook :4; bench checker print :26; F9_space test_home_paths_370.py:228.
- Wrong or understated in the draft:
  1. N1 (`ScrubChangesOnlyAccountSegments.test_n1_...`) already fails on main in a full clone (56 of 57 pass): TEAM_WORKING_AGREEMENT.md grew after card 370 and 370's scrub-commits.txt has no `after` line. In CI (depth 1) it skips. A one-line `after e19198dd2d2901b808f14bca60db1aa367595c22` in .mob/records/370/scrub-commits.txt fixes it; out of 376's scope: file it as a bug; the red record states N1 as a known baseline failure, not "existing rows pass".
  2. S1-S6 cannot go into class `ScannerRows` (the non-Pending 370-3 wrapper runs the whole class): they need their own class and their own Pending wrapper (376-3).
  3. Three C# wrappers (376-1, 376-2, 376-3) plus one non-Pending characterisation Fact.
  4. 434 untracked files hold the name (421 under .mob/records/234/scratch, 12 under .mob/records/237/trial, 228/body.tmp). Every scan keeps using `git ls-files`; the hook row G3 guards a careless `git add -A`.
  5. "--tree exits 0 on today's tree" is trivially true (it always returns 0). After green 1, on a machine with a name, `--tree` exits 1 until green 2.
  6. R1's "passes because target equals base" holds only if R1 allows "unchanged" (as 370's N1 does).
  7. A3 and A4 stay red only if the red leaves the old bench checker and old test row untouched; green 1 removes them.

## 1. The space rule (scripts/home_paths.py)

Current: SEGX excludes `\ / whitespace " ' backtick < > | * ? : , ; ( ) [ ] { } % & = $`; SEG is `[^SEGX]*[^SEGX.]` so it stops at the first space. PATH forms: F1/F2/F3/F8 drive letter plus `users`; F4 `/c/`, `/mnt/c/`; F6/F7 `/Users|home` case-sensitive; SLUG is F5 `[A-Za-z0-9_]+`. `scrub_bytes`: path_sub keeps the match if the segment is allowed (allow-list plus FILE_ALLOW), else returns `pre+sep+<user>`; slug_sub likewise. `find` uses the same two regexes and `_form_of`. `_SEG_STR` and `account_root_re(tail)` are the text copy (migrate_cards the only user). `scrub_tracked` skips `board/` by default.

Proposed bytes patterns (replace SEG; PATH otherwise unchanged):

    _WORD  = rb"[^"+SEGX+rb"]*[^"+SEGX+rb".]"
    _CLOSE = rb"(?=[\x5c/\x22\x27`]|%5[cC]|%2[fF]|\r?\n|\r?\Z)"
    SEG    = rb"(?P<seg>(?:"+_WORD+rb"(?: |%20)){1,2}"+_WORD+_CLOSE+rb"|"+_WORD+rb")"

The text pattern `_SEG_STR` is the same with `\s`-style classes: `(?P<seg>(?:W(?: |%20)){1,2}W(?=[\\/\"'`]|%5[cC]|%2[fF]|\r?\n|\r?\Z)|W)` where W is the old `_SEG_STR` body.

Rule in words: a segment may be up to three words joined by one literal space or `%20`; the multi-word form is taken only if a separator, a quote, a backtick, or the end of line or file directly follows the last word; otherwise it falls back to the single word (today's behaviour). Words cannot contain separators, quotes or spaces, so the parse is unambiguous: F9_space keeps the single word ("ACCT is the home" is four words, no closer after "is"). The end-of-line closer is an addition to the draft (without it a two-word name at a line end leaves its second word); measured on the 370 base tree (88ce447, 325 files): 0 lines change. Prose like `<profiles>\Jane here` at a line end is over-scrubbed to `<user>` (the safe direction). `_form_of`, `find`, `scrub_bytes` and `account_root_re` need no code change beyond the pattern.

Verified by prototype: 0 files differ between old and new over the HEAD tree (including board/) and the 88ce447 tree; the 45 tests of ScannerRows plus TreeScan pass under the new patterns including F9_space, F9_end_of_line, F9_file_end_no_newline; `scrub_tracked(ROOT, exclude=())` returns `[]` today.

Rows S1-S6 (`<profiles>` below stands for the drive letter, a colon and the Users folder in the separator form the row names; the red writes the real strings from parts; SP = "qa"+"acct"+" "+"seven" built from parts; ACCT = "qaacct7"; assert rewritten text with `scrub_bytes` and the hit with `find` on `notes/plan.md`):

| id | input | rewrite | find |
|---|---|---|---|
| S1 | `see <profiles>\SP\projects` | `see <profiles>\<user>\projects` | (1, F1) |
| S2 | `<profiles>/SP/projects/x` | `<profiles>/<user>/projects/x` | (1, F2) |
| S3 | `p = "<profiles>\\SP\\x"` (doubled) | `...\\<user>\\x"` | (1, F1) |
| S4 | `x = "<profiles>\\SP"`; `x = '<profiles>/SP'`; ``in `<profiles>\SP` today`` | `<user>` before the quote or backtick | F1, F2, F1 |
| S5 | `q=<profiles>%5Cqaacct%20seven%5Cdoc`; `<file:///<profiles>/qaacct%20seven/x.md>` | `<user>` | F8, F8 |
| S6 | (a) `<profiles>\ACCT is the home`; (b) `<profiles>\ACCT and the files live in /srv/x`; (c) `<profiles>\ACCT one two three/x` | only ACCT replaced; (c) pins the three-word cap | F1 each |

Extra recommended rows: S1eol (`<profiles>\SP\n` becomes `<user>`), S7 (`account_root_re(r"(?:\\|/)projects")` captures seg = SP on `<profiles>\SP\projects`). Known limit (docstring, not tested): `see <profiles>\ACCT then foo/bar` is over-scrubbed to `<user>/bar`. Not fixed: names of 4+ words, double spaces or tabs, a spaced name followed by prose at a non-closer.

## 2. local_accounts, F10, find, the hook, --tree

home_paths.py additions:

    FORMS["F10"] = "the local account name standing alone (whole word, any case)"
    MIN_ACCOUNT_LENGTH = 3
    GENERIC_ACCOUNTS = {...}   # root, administrator, admin, user, ubuntu, vsts, default, guest, public (reasons in a dict)
    def local_accounts(candidates=None) -> tuple
    def find_accounts(text, accounts) -> [Hit(line, "F10")]
    def find(path, text, accounts=())          # path forms as today, plus F10 when accounts is non-empty
    def scrub_account_bytes(data, accounts) -> bytes   # the F10 rewrite; never part of scrub_tracked

- local_accounts: default candidates are `getpass.getuser()` (try/except), `Path.home().name` (try/except), and the non-empty env values LOGNAME, USER, USERNAME; strip; drop names shorter than 3, names in ALLOWED_SEGMENTS or GENERIC_ACCOUNTS (case-folded) and duplicates (case-folded, first spelling kept); return a tuple, empty when nothing remains (in CI the account is `runner`, so empty). GENERIC_ACCOUNTS is a separate constant on purpose (putting them in ALLOWED_SEGMENTS would loosen the path rule).
- Whole-word regex on str lines (UTF-8 decode with replace), IGNORECASE, `(?<![^\W_])(?:alt)(?![^\W_])`, alt = re.escape'd names longest first. A letter or digit neighbour blocks a match; `_ ~ - \ .` do not (a superset of `git grep -w`).
- find: default accounts=() so existing 370 callers never see F10; one F10 hit per line, in line order with the path hits; F10 is reported for a line only if that line has no non-allowed path or slug hit (no duplicates; S/F rows keep exactly one hit).
- scrub_account_bytes: decode utf-8 surrogateescape, re.sub to `<user>`, re-encode; every other byte including line endings kept.
- Hook added-lines mode (check-no-secrets.py scan_staged): `accounts = home_paths.local_accounts()` computed once and passed to every `find` call including the MOB.md whole-file branch; an F10 hit prints `path:line: account name (F10)` on stderr; header becomes "... must not add secrets, home paths or the account name"; exit 1 on any hit else 0; `bad[:50]` as now. .githooks/pre-commit needs no edit.
- --tree (scan_tree): scans tracked files from the working tree with `accounts` passed in, exits 1 if any F1-F10 hit, else 0. Output in order: eight area lines `A1 .mob/: N hits in M files` (counts only); one line `F10: checked K local account name(s)` or `F10: skipped, no local account name outside the allow-list` (so an exit 0 is never read as proof); up to 50 lines `path:line:form` in area, path, line order, then `... K more hits`; a final line `N hits in M files`. No matched text, no account, no absolute path. Keep the PATTERNS list name (scan-history-for-secrets.py imports it). Update the docstring at :12-13.

## 3. Bench history checker and C# wrappers

check-bench-history-clean.py: drop the literal; `sys.path.insert` the scripts dir and `import home_paths`; `scan_history(path, accounts=None)` returns `Leak(line, kind)` pairs (`hit[0]`, `hit[1]` still work), no match text; accounts=None means `home_paths.local_accounts()`; kinds "drive path", "tilde-relative path" (the two regexes stay) and "user name" (from home_paths.find_accounts); main prints `{path}:{line}:{kind}` per hit, then `Found N potential leak(s).` and exit 1; clean prints `{path}: clean` and exit 0. Done-when addition: `python scripts/check-bench-history-clean.py` exits 0 on the real history.

Bench tests (tests/test_bench_history_clean.py, new class BenchHistoryNameRows; PLANTED = "qa"+"name"+secrets.token_hex(3); drive paths via the 370 `win()` helper; temp dirs by mkdtemp, nothing deleted):
- B1: accounts=(PLANTED,), a row with the name at line 2 gives [(2, "user name")]. B2: the same in upper case. B3: 50 name rows interleaved with clean rows give 50 hits at the right lines. B4: the same row with `<user>` gives []. B5: child process `python scripts/check-bench-history-clean.py history.jsonl` with cwd the temp dir; env a copy of os.environ with LOGNAME, USER, LNAME, USERNAME popped, then exactly one of USERNAME, USER or LOGNAME set to PLANTED (subTest per variable); rows: a drive path holding the planted account (line 1) and the name standing alone (line 2); assert exit 1 and stdout exactly `history.jsonl:1:drive path`, `history.jsonl:2:user name`, `Found 2 potential leak(s).`; PLANTED absent from stdout, stderr and repr(hits). B6: PLANTED glued to a letter or digit on either side gives [].
- The old class stays in the red. Green 1 replaces its literal row 3 and its upper-case literal expectation by a planted-name row, keeping the "someone" row and the clean-history test.

C# wrappers (LicenseFiles shape of the 370 wrappers; Slice 376):

| file | Fact | traits | runs |
|---|---|---|---|
| NoAccountNameInTrackedFiles.cs | The_account_name_is_gone_from_tracked_files_and_the_bench_checker_still_refuses_it | Criterion 376-1, Pending | WholeWordTreeScan, LocalAccountRows, tests.test_bench_history_clean (timeout 300000) |
| same file | The_space_rule_changes_no_tracked_file_and_the_records_are_the_rule_applied | Slice only, not Pending | SpaceRuleChangesNothingToday (C1), RecordRewriteIsTheRule (R1) |
| TreeScanGatesByExitCode.cs | The_tree_scan_and_the_hook_gate_by_exit_code | Criterion 376-2, Pending | TreeScanGatesByExitCode (T, G) |
| AccountNameWithASpace.cs | A_two_word_account_becomes_the_placeholder_and_prose_stays | Criterion 376-3, Pending | SpacedAccountRows (S) |

In CI the account is `runner`, so W-A skips with a stated reason and the planted rows (B, T, G, S) run. A skip is never counted as a pass in the records.

## 4. Test list (new file tests/test_account_name_376.py unless noted)

Characterisation (pass now): all existing 370 rows except N1 (ScannerRows 37, TreeScan 8, GeneratorRow 2, PreCommitRefusesAHomePath 3, the other ScrubChangesOnlyAccountSegments 6); C1 (SpaceRuleChangesNothingToday: `home_paths.scrub_tracked(ROOT, exclude=())` returns []); R1 (RecordRewriteIsTheRule: for each of the six record paths the committed blob at the target is the base blob or `scrub_account_bytes(base, local_accounts())`).

New state, Pending (376-1): W-A1..W-A8 (class WholeWordTreeScan, per area as in 370, using an independent test-local whole-word reading unioned with `find(path, text, accounts)`; message `A1 .mob/: N hits in M files (first: path:line:F10, ...)`; skips with a reason when local_accounts() is empty). Now: A1 fails (7 lines in 6 files), A3 fails (1/1), A4 fails (1/1); A2 and A5-A8 pass (0). B1..B6 (section 3). Now: B1, B2, B3 fail ("expected a user-name hit, got none"); B5 fails (stdout holds the planted drive path and misses the name line); B4 and B6 pass as controls. U1 (class LocalAccountRows): `local_accounts(["runner","Runner","qaacct7","QAACCT7","ab","root"])` returns ("qaacct7",); now fails against the red stub (which returns unfiltered names).

New state, Pending (376-2), class TreeScanGatesByExitCode, each in a mkdtemp repo (git init, git add), child process `python scripts/check-no-secrets.py [--tree]`, env as in B5 with PLANTED in USERNAME:
- T1: tracked notes/plan.md with a planted home path on line 2; `--tree` must exit 1, stdout contains `notes/plan.md:2:F1`. Now: fails ("expected exit 1, got 0").
- T2: the planted name standing alone; exit 1 and `notes/plan.md:2:F10`. Now: fails.
- T3: only `<user>` forms; exit 0 (control, passes now).
- T4: no planted text or real account in stdout of T1 and T2; plus a second file with 60 F10 lines: at most 50 `path:line:form` lines and the total line reads `60 hits in 1 files`. Now: the no-text half passes, the cap half fails.
- G3: staged notes/plan.txt adds the planted name on line 2; exit 1, `notes/plan.txt:2` and F10 named, planted absent. Now: fails (exit 0). G4: the same file with `<user>` forms exits 0 (control).

New state, Pending (376-3), class SpacedAccountRows: S1-S6 (+S1eol, S7). Now: S1-S5, S1eol and S7 fail on the rewrite assertion (second word left); S6 passes (control).

Red stubs (so each row fails by assertion, not by AttributeError or TypeError; the 370 precedent): home_paths.local_accounts returns the unfiltered machine names; find(path, text, accounts=()) accepts and ignores accounts; scan_history(path, accounts=None) accepts and ignores accounts; nothing else changes in the red.

## 5. Green 1, green 2, the record rule and R1

Green 1 (code, Cursor writing seat, left uncommitted, facilitator commits): home_paths.py (space rule, F10, local_accounts, GENERIC_ACCOUNTS, find_accounts, find(accounts), scrub_account_bytes; the real local_accounts replaces the stub); check-no-secrets.py (scan_staged and scan_tree as in section 2); check-bench-history-clean.py and tests/test_bench_history_clean.py (hand edits above); remove Status=Pending from the 376-2 and 376-3 wrappers. After green 1: T, G, S, B, U1, W-A3 and W-A4 pass; W-A1 still fails (the six records still hold the name); the 376-1 wrapper stays Pending.

Green 2 (the six records, by the rule only, Cursor, facilitator commits): six files (226/record-in-progress.json, 229/nohome-check/nohome229_check.py, 276/record-in-progress.json, 276/signoff-ctx6.md, 313/green1-codex-events.jsonl, 313/slice-architect.md (7 lines)); command: read bytes, `home_paths.scrub_account_bytes(data, home_paths.local_accounts())`, write bytes, with the command text committed in .mob/records/376/ with `$USERNAME`-free wording; commit trailer line `Slice-exception: rewrite s/ACCOUNT/<user>/`; plus one non-rule edit: remove Pending from the 376-1 wrapper. Check: `git diff --name-status` shows those six M and the one C# file; `git ls-files --eol` stays i/lf; each file's byte delta equals its occurrence count times the length difference of the replacement. The 229 script's check at line 48 changes meaning ("<user>" not in text), and the 226 and 313 JSON stay valid (verified parsing); they are records never re-run; the gate review records this.

R1 and scrub-commits.txt: file .mob/records/376/scrub-commits.txt with lines `base <full sha>` (the red's parent, written by the red; the six files still hold the name there) and, once green 2 lands, `after <full sha>`; without it the target is HEAD. Reader: `recorded_commits(path=SCRUB_COMMITS)` (one definition; add the optional path parameter to the 370 function). R1 reads committed blobs (`git cat-file`), per D206: skip with a reason if there is no local account, the base commit is absent (depth-1 clone), or the base blobs show no F10 hit for the local accounts; assert the set of base files with an F10 hit equals the six plus the two hand-edited files; assert each of the six at the target is the base blob or the rule applied to it. The gate reviewer runs R1 in a full clone and checks no skip.

## 6. Commit order, and what turns green when

0. Records commit: this slice, scrub-commits.txt with `base`.
1. Red (Muse): stubs, tests/test_account_name_376.py, B rows, the wrappers (three Pending, one characterisation), no change to the old bench row. `dotnet test --filter "Status!=Pending"` stays green; the python suite is as before, N1 aside.
2. Green 1: 376-2 and 376-3 un-pend. CI now runs T, G and S; `--tree` exits 1 locally until green 2.
3. Green 2: 376-1 un-pends and W-A1 turns green. `--tree` then exits 0 and `git grep -I -i -w -l -e "$USERNAME"` lists nothing.
4. Optional records commit adding `after <green 2 sha>`.

## 7. Risks

- Card 375: no shared file (it touches board/, the generator and the first-name checker; 376 touches neither). Landing order is free. A regenerated board/ is scanned by --tree area A2 (F10 only hits if a first name equals the account).
- Untracked look-alikes: 434 files (section 0); scans use `git ls-files`; G3 guards staging; nobody should `git add` .mob/records/234/scratch, 237/trial or 228/body.tmp.
- USERNAME unset or `runner` in CI: local_accounts() is empty, W-A and R1 skip, the planted rows (B, T, G, S) run. Runners with a real word as account (root, ubuntu) are covered by GENERIC_ACCOUNTS.
- Windows account forms: the profile folder can differ from the account (a suffix such as a domain, or a rename); both are included. Not covered: host or domain names, 8.3 short names of a spaced account, a display or full name (card 375 territory). Non-ASCII accounts fold case with Unicode rules on the str lines, but the separate `git grep` count uses its own idea of a word.
- Short or common-word accounts: the length floor and generic set trade a small gap for not blocking every commit.
- F5 slug: the segment is [A-Za-z0-9_]+, so a hyphenated or spaced account in a slug leaves the tail (`<user>-Doe`). The limit stays and is stated.
- Space rule limits: three words at most, single spaces only, some prose over-scrubbed (the `then foo/bar` case).
- Hook scope: only added lines are checked; history and untracked files are untouched; commit messages are not scanned.
- Pre-existing N1 failure (section 0, item 1): do not let the red record claim "all existing rows pass".
