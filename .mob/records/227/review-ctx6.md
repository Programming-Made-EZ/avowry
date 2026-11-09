# Card 227 review (ctx-6, independent reviewer, audit lens, runner lens)

Seat: Claude (the 14:59 UTC probes in .mob/records/probes/2026-10-30.jsonl show Codex, Copilot, Grok and agy without usable quota). Reviewed mob-defaults 56b39bc..00f9e93 (8 "Card 227" commits, pushed; origin/main equals main) and avowry 73cdf20, 810e818 and the ladder entry inside 3c82909. I wrote none of it. Scratch copies only (git archive of 00f9e93 under the session scratchpad); the repository was not touched; strings only, no delete was run.

## Verdict: CHANGES REQUESTED

The three Done checks hold on the forms the tests list, and the invariants hold. Four things block approval: a common PowerShell form of the very slip is not detected (F1), a quoting form defeats the nested shell payload (F2), `push_rule_problems` is built and never called (F3), and the scan rule text the amendment promised is in no template (F4). The rest are small.

## Findings

F1 (high, Done check 1 holds only on listed forms). delete_guard.py:205-260 (_clauses) and 361-380 (_scan_words). A brace block on one line is not split into clauses, so the delete inside it is never at command position. Evidence (scan_command(cmd, ["scratch"]), strings only): `if (Test-Path $p) { Remove-Item $p -Recurse -Force }` no finding; `Get-ChildItem x | ForEach-Object { Remove-Item -Recurse $_ }` none; `ls x | % { rm -r $_ }` none; `try { Remove-Item $p -Recurse } catch {}` none; `foreach ($d in $ds) { Remove-Item -Recurse $d }` none. The same text with a newline after `{` IS flagged, so it is a parser gap, not a design choice. PowerShell is one of the two scanned tools (SHELL_TOOLS) and this is the natural way to write the card 226 slip in it. Card 228 names pipeline-fed, splatted and continued Remove-Item, not braces. Fix (test first): treat a standalone `{` and `}` as clause boundaries in _clauses (not inside `${...}`, the `{}` of find -exec, or brace expansion), add the five strings above to the form table; or move it to 228 with a line in the rule text.

F2 (medium to high, quote concatenation hides the payload). delete_guard.py:49 (_unquote) used at 386 for `bash -c`. _unquote strips only when the first and last characters are the same quote, so a word built from pieces is read as text with quote characters in it. `bash -c 'rm -rf '"$d"` no finding; `sh -c 'rm -rf '$d` none; shlex.quote nesting `bash -c 'bash -c '"'"'rm -rf $d'"'"''` (depth 2) none, while depth 1 is flagged; I nested 2 to 9 deep, all unflagged. The slice (Inputs: "a target the parser cannot read is a finding") says an unreadable form fails closed; here it passes silently. Fix: unquote by shell rules (concatenate adjacent quoted and bare pieces) before rescanning a -c payload, and make an unparseable -c payload a finding.

F3 (medium, "built means called", stand-down 2). mob_process.py:586 push_rule_problems has no caller outside the tests (grep of *.py outside tests: the def only). The slice says "a repo whose agreement lacks it fails `unpushed`"; `unpushed` and run_audit never read the agreement. The 00f9e93 body lists callers of unpushed_problems, ladder_check and scan_pushes but omits this one, and avowry 810e818 says "checked with push_rule_problems", which was a manual call. So the first half of Done check 3 ("the working agreement and the template say so") is not enforced by any run: an agreement that loses the rule passes the audit. The known surviving M9 (only "unpushed" required) sits on this dead code, so wiring the call is what makes M9 matter. Fix: call push_rule_problems from run_audit for the audited repository (and mob-defaults), add a test that an audit of an agreement without the rule fails, and close M9 with one case per missing part.

F4 (medium, promised rule text missing). The amendment (2e2e0f3: "Until that card is done, the rule text ... says it plainly") and its commit plan item 15 promise a sentence in the template and the avowry agreement: the scan sees commands at command position in a Claude transcript, after the fact; encoded, hidden-verb and written-file forms are not covered. templates/TEAM_WORKING_AGREEMENT.default.md "Detectors" bullet (about line 509) still describes the reply scan only; a grep for "command position", "scan-transcript" and "executed command" over the template, mob-defaults' agreement, avowry's TEAM_WORKING_AGREEMENT.md and README.md finds nothing. The README snippet moved to 228; this sentence did not. Card 216 is enabled by this card to carry "the scan". Fix: one bullet in the template, the mob-defaults copy and avowry (separate avowry commit), naming F1 and F2 too if they are not fixed.

F5 (medium, MOB_WATCHED_REPOS). facilitator.py:1310. Set nowhere (the green body says so). The rule text (template and both agreements) says the facilitator runs the check after every seat on the repositories in `MOB_WATCHED_REPOS`; with the variable unset scan_pushes returns at once, so no Push flag appears in a normal run and the only live coverage is the audit pass and the manual command. The mutation that removes scan_pushes from seat_turn is killed only because the tests set the variable. Fix: default to the seat's repository plus the mob-defaults folder when unset, or state in the rule text that the per-seat scan is opt-in. A mob that reads the rule thinks it is watched.

F6 (medium to low, guard gaps; mutations that survived in scratch copies of 00f9e93, run against the six command-scan modules or test_push_detector).
- M9 (known): push_rule_problems needing only "unpushed" survives (see F3).
- C3: removing `$` from _PATH_BAD (delete_guard.py:168) survives. With it, `rm -rf scratch/$x` is ALLOWED (x can be `../..`). The tests only use `$SP/...`. Add `rm -rf scratch/$x`, a backtick form and `scratch/%X%` as unsafe cases.
- A2: run_audit's `watched` default (mob_process.py:793, mob-defaults) set to [] survives: the rule text says the audit pass checks mob-defaults, nothing tests the default.
- B3: dropping `and launched` (facilitator.py:353) survives: a backend that never started gets a scan entry (a "no command source" flag for a seat that did not run).
- B9: decider.command_scan taking the first attempt's status instead of the worst survives.
- D4: a boolean `card` in a ladder line counts as card 1 (the record's stated gap; low).
Each is a one-line test.

F7 (low, duplicate rule). delete_guard.py:444 transcript_commands and command_source.py:24 commands_from_transcript are two copies of the transcript reading rule (assistant record, Bash and PowerShell, input.command). They already differ (an OSError raise versus NoCommandSource). A fix to one will miss the other. Fix: one reader, imported by the other.

F8 (low, record check validates shape, not evidence). mob_process.py:377-407. `{"status": "not run"}` with no reason exempts a seat; `{"status": "clean"}` with no transcript or command count passes; a card id with no leading number skips the check (_card_number returns None). Verified by calling _command_scans_problems with those entries. Also nothing writes record["command_scans"] from the scan entries: seat_turn drops `scans` after scan_commands, so a clean scan leaves no trace and the entry is typed by hand (the red-3 and green-3 entries carry no `commands` count, the standdown-1 entry has null). Fix: require `reason` for "not run" and "findings"; have CardRun append the entry to a file the record check reads (228 can own this if stated).

F9 (low, unscanned paths and list gaps). facilitator.py:562 (run_turn in the non-card loop) and 692 (probe) call run_backend without scan_out; the legacy loop's seats are unscanned. scan_entry uses only the repo-relative `scratch` root (command_source.py:17), so a literal delete under an absolute scratch folder is a finding unless the facilitator hand-scans with --root. Not flagged: wrapper verbs outside the list (`timeout 5 rm -rf $d`, `setsid`, `ionice`, `flock`, `busybox`, `wsl`, `ssh host rm ...`, `parallel`, `watch`, `trap "rm -rf $d" EXIT`); a heredoc fed to an interpreter (`python - <<'X'` with a shutil.rmtree call inside, `bash <<X`), because _strip_heredocs treats every heredoc body as data; `python -c "if os.path.exists(p): shutil.rmtree(p)"` and a list comprehension, because _RMTREE (delete_guard.py:176) is anchored to a line start or `; & |`. None of these is in card 228's text (hidden verbs, written files). Fix: add them to 228's list now so they are owned.

F10 (info). The unpushed detector runs only `git` (no gh, no network verb; the tests pin NETWORK = fetch, pull, push, ls-remote, clone). test_mob_process's audit test now runs git on the live mob-defaults checkout; it asserts exit 1 either way, so it does not depend on the state. Avowry's own scripts/mob_process.py (D134, pre-push gate) has neither `ladder` nor `unpushed`; enforcement is only through mob-defaults' audit. The rule text's two documented gaps (local remote-tracking refs, rewrites) are honest. test_loop_state failed once (test_lock_released_when_holder_exits) when run in parallel with another suite and passes alone (8 OK); not caused by this card.

## Done checks

1. Command scan: holds for the listed forms (card 226 slip, flag order, aliases, rmtree styles, find -delete, git clean, literal scratch-child allow-list, fail-closed on no source). Fails on partial work (mutation, failures): NoCommandSource read as clean 7; commands not counted 5; missing-SESSION rule 1; root itself allowed 10; `..` allowed 2; git clean 4; find -delete 2; exit 2 to 1 3; heredoc not stripped 1; depth 0 4; rmtree off 10; sudo wrapper off 1; card boundary 10; unscanned not failing 1; "not run" removed 1; scan_commands status 1; decider attempts 1. Not holding: F1, F2, F9 forms.
2. Trust ladder: holds. Boundary 15, none-without-reason 5, cover-by-card-only 3, records-folder 2, case 2, run_audit call 2 failures; the bool-card mutation survives (F6). Real run: `python mob_process.py ladder C:/Users/<user>/projects/phrase-lang` prints ok, exit 0.
3. Push on every commit: detector holds (`>` to `>=` 2; reflog min to max 1; worktree tips dropped 4 failures and 2 errors; no-origin passes 1 error; future ignored 1 error; repository spellings 1; run_audit docstring-only 1). The watched-default mutation survives (F6). Rule text exists in the template and both agreements, but nothing checks it (F3) and the per-seat scan does not run by default (F5). Real runs: `python mob_process.py unpushed C:/Users/<user>/projects/phrase-lang C:/Users/<user>/mob-crosscli` prints ok, exit 0; a missing path prints "nonexistent not a git repository", exit 1.

## Invariants

- safe_delete and the kill rules unchanged: `git diff --numstat 56b39bc 00f9e93 -- delete_guard.py` is 344 added and 0 deleted; tests/test_delete_guard.py has no deleted line in the range. No existing test was loosened (deleted test lines: none).
- Git only in the detector: mob_process.py _git_run runs `git`; no gh and no network verb anywhere in the range.
- Line endings: decider.py and facilitator.py are CRLF in base and now, every line (341 of 341 and 1719 of 1719); every other changed file and all new tests and fixtures are LF (checked with `git show 00f9e93:<file> | tr -cd '\r' | wc -c`).

## D157, D136, bodies

- D157 order: red 524171f, green 2fc4f98 (1); red 3beff31, green 8440697 (1b); red 5cc58d2, green 79c572e (2); red 2dbcb29, green 00f9e93 (3). No red for n+1 before the green for n. Every commit has "Criterion: n"; the Intent line 254e1d9 is cited in six of eight; no trailers (0 of 8); no CR in bodies.
- D136 (git show --numstat, files/lines): 10/233, 2/363, 4/397, 6/284, 1/387, 2/108, 1/399, 4/161; all under 15 files and 400 lines (two reds sit 1 and 3 lines under). The card exceeds the per-card increment and concern guide by one each, recorded at the slice.
- Body claims re-checked with command and count: test counts 34, 40, 14, 6, 5, 11 (unittest loader count); 00f9e93 numstat 4 files +157 -4 (112+23+11+11, 3+1); 2fc4f98 +344 for delete_guard. Not re-run: the 822, 748, 718 and 788 full runs and the author's 17 and 12 mutation runs. The 3c82909 title ("MOB.md regenerated") hides the swept ladder entry; recorded as a deviation, not rewritten; accepted. 73cdf20 and 810e818 are text only, LF, and match their claims.
- Stand-down 2 "built means called": unpushed (run_audit, subcommand, scan_pushes), ladder (run_audit, subcommand), scan_entry (run_backend) and _command_scans_problems (check_record) have callers; push_rule_problems does not (F3); decider.command_scan is written to the result and read by nothing (owned by 228, disclosed).
- Record: ctx-6 and ctx-8 have no command_scans entry yet; check_record on the 227 record reports exactly those two until the facilitator scans this seat.

## What I ran

- New modules (scratch copy of 00f9e93): `python -m unittest tests.test_command_scan tests.test_command_scan_entry tests.test_command_scan_wiring tests.test_command_scans_record tests.test_command_source tests.test_delete_guard tests.test_trust_ladder tests.test_push_detector tests.test_loop_state`: Ran 261, one failure (test_loop_state under parallel load, F10; 8 OK alone).
- Decider and seat modules: `python -m unittest tests.test_decider_gate tests.test_decider_launched tests.test_decider_pin_scrub tests.test_decider_probe tests.test_decider_record tests.test_decider_record_units tests.test_decider_rotation tests.test_decider_runner tests.test_seat_gate tests.test_seat_rotation tests.test_seat_rotation_rules`: Ran 205, OK.
- Mutations: about 40, one change each, separate scratch copies; those that finished when I wrote are counted above. A9 to A15 (other than the known M9) had not reported when I wrote; I make no claim about them. About 190 command strings through scan_command; ten hostile long strings timed (all under 0.6 s, no recursion error).
- Not run: the full 822-test suite.

## To approve

Close F1, F2, F3 and F4 (test first), and F5 either way. F6 to F9 can be one small commit or named in 228. I will re-read the diff.

## Re-check (ctx-6)

Seat: Claude (the 22:23 UTC probes show Codex, Copilot, Grok and agy without usable quota), fresh context, audit lens and runner lens. Reviewed mob-defaults 00f9e93..f8a45da (15 commits, pushed; origin/main equals f8a45da) and avowry c803a43 and 5deedcc. I wrote none of it. Scratch copy of f8a45da under the session scratchpad (git archive, a new folder for each mutation); the repositories were not touched; strings only, no delete was run.

### Verdict: APPROVE

All four blockers (F1 to F4) and F5 are closed with evidence, the F6 survivors die, and no invariant broke. One item is not blocking but must be written down before Done: the new rmtree rule makes the command scan flag test-data and mutation strings in Python text (see "Counter-metric").

### Per finding

- F1 closed. `scan_command(cmd, ["scratch"])` flags all five PowerShell block strings (the `if (Test-Path $p) { Remove-Item ... }`, ForEach-Object, `% { rm -r $_ }`, try/catch and foreach forms), `f(){ rm -rf $1; }; f $d`, `cd /; rm -rf scratch/x` and `pushd /tmp; rm -rf x`. Harmless twins stay clean (`ls {a,b}`, `${HOME}`, `find -exec echo {}`, `Get-ChildItem | % { $_.Name }`, `if (...) { Get-Item $p }`).
- F2 closed. `bash -c 'rm -rf '"$d"`, `sh -c 'rm -rf '$d` and shlex.quote nesting from depth 1 to 12 are all flagged (past the depth limit is a finding); an unclosed `bash -c 'rm -rf ` is a finding.
- F3 closed. `run_audit` calls `push_rule_problems` for the audited repository and every watched one (mob_process.py:849, the only caller outside the tests). Mutation M9 (parts reduced to "unpushed") now fails 13 tests.
- F4 closed. The scan-limits bullet ("command position ... after the fact ... a detector, not a block", Agent-tool seats by hand until 228) is in templates/TEAM_WORKING_AGREEMENT.default.md:518, mob-defaults TEAM_WORKING_AGREEMENT.md:331 and avowry TEAM_WORKING_AGREEMENT.md:451 (c803a43, text only, LF); `scan_limits_problems` is called by `run_audit`. The 3d wording names the watched repositories and the rewrite and local-origin residuals ("every repository the mob works in" is gone).
- F5 closed. `scan_pushes` with MOB_WATCHED_REPOS unset watches the seat's repository and mob-defaults; flags are keyed on realpath plus finding (age normalised, so a future-dated commit is one flag per repository). The 17 tests in test_push_watch_default pass.
- F6 closed. All six survivors re-run as single-change mutations in fresh copies now die: C3 (remove `$` from `_PATH_BAD`) 6 failures; A2 (audit default watched = []) 1; M9 13; B3 (drop `and launched`) 1; D4 (bool card counts as 1) 1; B9 (first attempt's status) 8. `rm -rf scratch/$x`, a backtick form and `scratch/%X%` are flagged.
- F7 closed. `delete_guard.transcript_commands` is gone; `command_source.commands_from_transcript` is the one reader and delete_guard imports it inside `main` (avoids the circular import).
- F8 closed for the check; the writer is moved to 228 with its line present. `_command_scans_problems` now rejects `not run` and `findings` without a reason, `clean` without a transcript or an integer count (the string "5" and True are refused), `no command source` as a failure, and a card id without a number. Nothing writes the entry yet: 228 "Also owned here" item 4 says so.
- F9 moved to 228, lines present. Wrapper verbs (`timeout`, `setsid`, `ionice`, `flock`, `busybox`, `wsl`, `ssh`, `parallel`, `watch`), a heredoc owned by python or bash, and `python -c` with rmtree after a colon or inside a list comprehension are now flagged. Not flagged: `trap "rm -rf $d" EXIT` (228 item 6). The legacy loop, the probe seat and the absolute scratch root are 228 item 5.
- F10 info, unchanged. The detector still runs only git; avowry's own scripts lacking `ladder` and `unpushed` is 228 item 7.

All seven lines of the triage's section D are present in board/228-*.md (lines 24 to 30).

### Counter-metric: false positives on real seat transcripts

I ran `delete_guard.py scan-transcript --root .mob/scratch` at f8a45da on six real transcripts: my own first review, the security review, stand-down 1, green 1d, red 1d and red 1c. Five are flagged only by "rmtree" text inside Python string literals or test data (`shutil.rmtree(p)` in a list of test commands, test names such as `rmtree literal child never allowed`, a `'shutil.rmtree('*20000` stress string). Only red 1c is real (`rm -rf "$M"`). A minimal probe confirms the class: a `python - <<'PY'` heredoc with `names=['test_rmtree_anywhere', 'shutil.rmtree(p)']` is flagged, and so is `python -c "print('shutil.rmtree is flagged')"`; the same heredoc with `rm -rf $d` inside a string is clean, as are a grep for the rmtree text and a git log --grep.

Judgement: acceptable for card 227, not blocking. The direction is fail-closed (nothing unsafe is allowed), the class is confined to seats that write Python text naming rmtree (in practice the seats that test this scanner), the record entries carry a reason and were read by hand, and the same read caught the real delete. It is a real cost: every finding needs a manual read, and the facilitator already once recorded a real finding unread (a deviation in the record). Two conditions: (1) card 228's false-positive line (item 3) names `-ErrorAction`, `-Filter`, `-Exclude`, a `#` word, `find -H` and "rmtree in a read-only command's text" but not strings inside Python string literals or test data in heredocs (5 of 6 transcripts at f8a45da); add that, with the cheap idea of not flagging a match inside a string literal of a Python heredoc, or reporting it as "possible string, read it"; (2) the rule "read and classify every findings result before the record commit" stays. The record already carries the observation with its count.

### The test_card_mode isolation (f8a45da)

Acceptable. `run_facilitator` now sets MOB_WATCHED_REPOS to the fixture repository (`{**os.environ, "MOB_WATCHED_REPOS": str(self.repo), **(env or {})}`), so a value passed by a test still wins and the variable is never empty (an empty value would fall back to the real mob-defaults). One inserted line and one modified; no assertion changed. The default path through the facilitator is covered by test_push_watch_default. The other edited test lines (tests/test_command_scans_record.py: the helper builds complete entries, and the `no-number` card is dropped from a loop that expects no problem; tests/test_push_detector.py: the sample agreement uses the newly required wording) follow from the stricter checks; neither lowers an expectation, and the strict suite tests the no-number card as a failure.

### Invariants, D157, D136, bodies

- safe_delete and the kill rules: the text from `_make_writable_and_retry` to the end of delete_guard.py has the same md5 at 00f9e93 and f8a45da. No deleted line in tests/test_delete_guard.py in the range. delete_guard.py's other deletions are the replaced parser pieces.
- Git only in the detector: unchanged; no gh and no network verb.
- Line endings: decider.py 341 of 341 lines CRLF and facilitator.py 1723 of 1723 CRLF at f8a45da (the 3d body says 1727; the file has 1723 lines and 1723 CRs, so the body count is off by 4, not a break; noted, not amended). delete_guard.py, command_source.py, mob_process.py and both agreements have 0 CR; avowry c803a43 and 5deedcc have 0 CR. No CR in any commit body.
- Trailers: 0 of 15 commits.
- D157 order: criterion 1 red then green for 1c, 1d (plus guard d5ccc7f), 1e, 1f; then criterion 2 (2b); then criterion 3 (3c, 3d). No red for n+1 before the green for n; every commit has "Criterion: n".
- D136 (files/lines per commit from `git show --numstat`): 1/277, 2/134, 1/345, 1/162, 1/81, 1/176, 2/34, 1/205, 5/73, 1/153, 1/9, 1/172, 2/10, 1/205, 4/39. All under 15 files and 400 lines.
- Bodies: 3d's 17 tests in test_push_watch_default re-ran OK; its real-run claims (unpushed ok on both repositories) match my reading. The full-suite runs (1253) and the author's mutation counts are not re-run (not my brief). The 3d body explains why the existing test was edited, with numbers.
- Stand-down 2 "built means called": `push_rule_problems`, `scan_limits_problems`, `_command_scans_problems`, `scan_entry`, `scan_pushes` and decider `command_scan` all have callers outside the tests.

### What I ran

- Command-scan group on f8a45da (scratch copy): test_command_scan, _blocks, _entry, _forms2, _guards_1d, _limits, _wiring, test_command_scans_record, _record_strict, test_command_source, _source_tools, test_delete_guard, test_trust_ladder, test_push_detector, test_push_rule_audit, test_push_watch_default: Ran 684, OK (skipped=1).
- Decider and seat group: test_decider_gate, _launched, _pin_scrub, _probe, _record, _record_units, _rotation, _runner, test_seat_gate, _seat_rotation, _seat_rotation_rules: Ran 205, OK.
- Mutations: C3, A2, M9, B3, D4, B9, each in its own scratch copy (all killed). About 70 command strings through `scan_command` (bad forms, harmless twins, hostile long strings: 3000 braces, 500 substitutions, 100000 clauses, all under 0.8 s, no recursion error).
- Not run: the full 1253-test suite.

### Not blocking, to record

- Add the string-literal false-positive class to card 228's item 3 (condition 1 above).
- `trap "rm -rf $d" EXIT` is still unflagged; it is 228 item 6.
