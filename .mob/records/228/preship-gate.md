# Card 228 pre-ship gate (F-071 corrective), independent reviewer ship, ctx-4 (audit lens)

Checked 2026-11-02 `07:00Z` to `07:40Z`. Probe given: 2026-11-02 06:55 UTC. mob-crosscli `main` = `origin/main` = `743a4a1` (0 ahead). avowry `main` = `origin/main` at `d762ae0` (0 ahead) after `git fetch --all`.
Sources: board/228, `turn-slice-architect.md`, the five windows `.mob/records/decisions/228-*.json`, `record-in-progress.json`, every `turn-*.md`, `ruling-sshagent-architect.md`, mob-crosscli git history and tests, and the 25 seat transcripts under `C:\Users\ringe\.claude\projects\C--Users-ringe\73298da3-f8ff-4a30-af55-b3e95ffa16f4\subagents\` (ids in the record). Format model: `.mob/records/237/preship-gate.md`.
Disclosure: ctx-4 wrote none of the card's code or records. This seat's own transcript (`agent-a641b8a3fdce522b3.jsonl`) cannot be scanned until it ends; the facilitator scans it as for any seat.

**Verdict: HOLD.** One blocking finding (B1, the ssh-agent conflict). It needs a team decision, not a code edit by this seat. Everything else is met or honestly recorded; the non-blocking findings (N1 to N7) go into the closing record.

| # | Check | Verdict | Evidence |
|---|-------|---------|----------|
| 1 | Done checks met by committed evidence | **1 and 2 MET with recorded gaps, 3 NOT MET as worded (B1)** | Table below. |
| 2 | Criterion order, cycles, scope | **PASS on order, FAIL on the thin-slice limit (recorded, N3)** | Cycle table below. |
| 3a | `python -m unittest discover -s tests` on main | **PASS**: `Ran 1434 tests in 1807.823s`, `OK (skipped=1)` (matches the 1434 in cycle 3b). The log also prints a `SMOKE FAILED` block from a toy-card smoke test inside the suite; the run itself is OK and exit 0 | Run by this seat on `743a4a1`, started 07:02Z, ended 07:32Z. |
| 3b | `python scripts/mob_process.py check` (avowry) | **PASS** | `mob-process: OK (101 closed cards, 10 records, 91 legacy)` at 07:08Z. |
| 4 | ssh-agent conflict | **BLOCKS SHIP (B1)** | Ruling below. |
| 5 | Command scans | **PASS with one stale entry (N1) and one closable gap (N2)** | Scan section below. |
| 6a | Every commit pushed | **PASS** | mob-crosscli: `git rev-list --count origin/main..HEAD` = 0, 23 card commits (`05a838f` to `743a4a1`), 0 with a Co-Authored-By trailer. avowry: 0 ahead, 0 trailers in the 115 commits since 11-01 11:00 local. |
| 6b | No seat wrote under `.mob/audits/` | **PASS** | `git log --since=2026-11-01T11:00 --name-only -- .mob/audits` is empty in avowry; in mob-crosscli `.mob/` is untracked and holds only `audits/last.json`, written 07:02Z by the external auditor (its own `auditor` field, audit 145), not a seat. |
| 6c | Delete or stash command in a seat transcript | **PASS, none executed** | 25 transcripts, 627 Bash calls (also Monitor 32, Write 16, Edit 16, ToolSearch 3, TaskStop 1, Read 1). A clause-position scan for rm, rmdir, rd, del, Remove-Item, ri, shred, `git stash`, `git clean`, `git reset`, `git rm`, `git checkout -f/--`, `git worktree remove`, `git branch -D`, `find -delete`, unlink, truncate gave 47 hits; every one sits inside a heredoc, commit message, test source or Python string (read by hand). 0 `git stash`. |
| 7 | Deviations recorded honestly | **PARTIAL (N3, N4, N5)** | Section below. |

## Done checks on board/228

| Done check | Verdict | Evidence and gap |
|---|---|---|
| 1. Hidden, encoded and non-shell deletes reported from executed commands | **MET** | Seven cycles: `63e4f61` (quoted, split, `${V:-rm}`, assigned verb), `fdff54f` (`-EncodedCommand`, `base64 -d | sh`, depth 6), `9fe3913` (node, rimraf, perl, `[IO.Directory]::Delete`, `os.removedirs`, `os.system`, subprocess argv, `python -c` by ast), `4e35837` (`git reset --hard`, checkout force, iex, splatted or continued Remove-Item), `53666c3` (ruby, php, awk, deno, bun, perl unlink glob, rsync), `844f083` (hex, `<(...)`, `bash < f`, alias, `[char]`), `9fe71ca` (parity and ten-lines guards, born green). Each green made its red pass in the record. I re-ran the CLI on the 25 transcripts with HEAD: 20 stay clean, 5 give the findings judged under Command scans, so the new forms produce no new false alarms beyond those. Path forms (slice row "Path forms") moved to F4 by window 228-c1-scope, `decisions/228-c1-scope.json`; not in the Done check wording. |
| 2. Contents of files a seat writes then runs are scanned; other backends' transcripts read where they exist | **MET for files and Copilot, PARTIAL for "other backends"** | Files: `5ffc494` (`command_source.py:90` `calls_from_transcript`, `:275` `scan_file_runs`, `delete_guard.py:1065` `scan_file`), reads are not runs `f6a76f5`. Copilot: `bc0881e` (`command_source.py:363` `locate_copilot`; `core/backends.json:42` `"command_source": "copilot"`). Codex rollouts exist on this machine and are not read: the slice defers them to F2 (turn-slice-architect.md, T2 table), so "where they exist" is not met for Codex. Grok and agy have none (backends.json:31, 53, 64 stay `none`). Recorded in the slice, not as a deviation (N5). |
| 3. An opt-in hook blocks a flagged command before it runs, installed only by a human | **NOT MET as worded** | The hook exists (`hooks/delete_guard_hook.py`, `de65fe5`, `743a4a1`) and is not installed by anything: `git grep delete_guard_hook` outside tests and `hooks/` is empty; `--print-snippet` writes nothing (`tests/test_delete_guard_hook_3b.py:108-140`, `:159`, `:182`). But it does not block one flagged command: `eval "$(ssh-agent -s)"` is flagged by the scan and allowed by the hook (B1). |

## Criterion order from git log (mob-crosscli, oldest first, local time)

Every red is the commit directly before its green; no green appears before its red. Each red names `Criterion: n` (and so do all criterion 2 and 3 greens); the criterion 1 greens `63e4f61 fdff54f 9fe3913 4e35837 53666c3 844f083` carry no line (the rule is for reds, D157).

| Cyc | Red | Green | Size (red / green) |
|---|---|---|---|
| 1a | `05a838f` 12:51 | `63e4f61` 13:26 | 83 / 33+4 |
| 1b | `82c1e84` 13:31 | `fdff54f` 14:04 | 134 / 45+1 |
| 1c | `c743784` 14:30 | `9fe3913` 15:07 | 120 / 128+6 |
| 1d | `40555b4` 15:16 | `4e35837` 15:53 | 129 / 24+1 |
| 1e | `025b178` 15:59 | `53666c3` 16:36 | 124 / 31+1 |
| 1f | `ca59449` 16:47 | `844f083` 17:24 | 186 / 129+1 |
| 1g | `9fe71ca` 17:54 | none (born green, guards) | 183 |
| 2a | `dc58ebd` 18:06 | `5ffc494` 18:47 | 236 / 139+7 (2 files) |
| 2b | `fa2351f` 18:59 | `bc0881e` 19:12 | **427** / 182+43 (3 files) |
| 2c | `3d63e09` 20:16 | `f6a76f5` 21:04 | 289 / 68+6 |
| 3a | `8063a74` 21:29 | `de65fe5` 21:41 | 367 / 146+1 |
| 3b | `07132a0` 21:49 | `743a4a1` 22:25 | 192 / 13 |

Cycles per criterion against the rules: criterion 1 = 7 (slice: 1; thin-slice rule: at most 3 increments); criterion 2 = 3 (slice: 1; stand-down 1 cap: 2, third needed window 228-c2-scope); criterion 3 = 2 (slice: 1; stand-down 1 cap: 2). Twelve increments for a card limited to three. The scope decisions that cover them: `228-c1-scope` (capped criterion 1 at 1g, adopted without objection), `228-c2-scope` (allowed 2c), stand-down 1 in `record-in-progress.json` (cap of two on criteria 2 and 3, set at the boundary after green 2a). The cap was held: criterion 1 got no cycle after 1g, criterion 2 none after 2c, criterion 3 none after 3b. Cycle 1g being born green means the card has 11 failing-to-passing cycles and one guard-only red.
D136 size: all commits are at most 3 files; one exceeds 400 lines, `fa2351f` (427 inserted lines in `tests/test_command_scan_files_2b.py`), not recorded (N4).

## B1: the ssh-agent conflict (item 4 of the brief), ruling

Facts. `ruling-sshagent-architect.md` (15:23 local) rules: recorded finding, fail closed, "no clean twin; no green teaches an allow in card 228". Red 1e and green 1f honoured it (`turn-red-qa-1e.md:9`, `turn-green-dev-1f.md:10`). Red 3a did not see it: `tests/test_delete_guard_hook_3a.py:146-147` lists `'eval "$(ssh-agent -s)"'` among commands the hook must allow (`test_a_benign_command_exits_zero`). Green 3a `de65fe5` added `delete_guard.py:1129` `_SSH_AGENT_EVAL` and the filter at `:1147` (`not _SSH_AGENT_EVAL.fullmatch(f.snippet.strip())`) in `tree_verdict`. `turn-green-dev-3a.md:7` says "the one eval let through", neither seat flagged the conflict, and the facilitator raised it as an open point (`ab248f4`).

Reproduction (this seat, HEAD `743a4a1`, hook fed PreToolUse JSON, cwd = mob-crosscli):
- `eval "$(ssh-agent -s)"` exits 0 (allowed), while `scan_command` still reports it (finding kind recursive delete on an unresolved path).
- `ssh-agent(){ echo "rm -rf ~"; }; eval "$(ssh-agent -s)"` exits **0**. The function runs `rm -rf ~` inside the eval; the hook sees only an echo and the allowed eval.
- `PATH=/tmp/evil:$PATH; eval "$(ssh-agent -s)"` exits **0** (a shadow binary on the path).
- `eval "$(ssh-agent -s; rm -rf ~)"` exits 2 (the regex is anchored, so the wide form is caught).

Ruling. **It blocks ship.** Reasons: (1) the hook's job under D169 is to refuse what the scan flags; this allow turns a flagged form into a free pass, which is the fail-open the architect's ruling and D166 forbid, so Done check 3 as worded ("blocks a flagged command") is not met; (2) the allow is demonstrably bypassable by a function or PATH shadow in one clause, exactly the "only as safe as PATH and the binary" objection in the ruling; (3) the card's invariant says fail-closed rules are unchanged; (4) the tradeoff is cheap on the other side: the cost of refusing is one rare, easily reworded line (run `ssh-agent -s`, source the output from a file), which the ruling already priced in.

Fix needed, and who decides. The team must decide it; this seat changes no code. Criterion 3 is capped at two cycles (stand-down 1), so any code change needs a scope window first, as 228-c2-scope did. Options for the window:
- (a) Remove the allow. Delete `delete_guard.py:1129` and the filter at `:1146-1147`, flip the ssh-agent entry at `tests/test_delete_guard_hook_3a.py:147` out of the benign list and into a refused case with the rewording hint in the message. One red and one green (about 15 lines changed). Recommended.
- (b) Ratify the allow. The architect amends the ruling for the hook only, the card text and `turn-green-dev-3a.md` record that the hook deliberately passes this one form, the shadow bypass is written down as a known limit, and the Done check 3 wording is read as "except this form". No code change, but the decision must be a window with the bypass in the proposal, not an unrecorded default.
Either way the record must carry the decision before ship. The gate cannot clear while the only owner of the conflict, the open point in `record-in-progress.json` `open_points[2]`, is unresolved.

## Command scans (item 5)

Method: every card-228 Claude subagent transcript (25 files) re-scanned with HEAD `delete_guard.py scan-transcript`; seat id to record entry matched by description in each `.meta.json`. Results are in `record-in-progress.json` `command_scans` (29 entries: 1 Copilot, 25 seats, 3 re-scans).

| Seat | Record entry | My HEAD scan | Judgement |
|---|---|---|---|
| ctx-2 slice, QA red 1a-1g, dev green 1a, 1b, 1d-1f, architect ssh ruling, red 2b, green 2a, 2b, red 3a, 3b, green 3b | clean (counts 3 to 40) | clean, same | agree |
| dev green 1c (`a15527c90e35f4135`) | clean, 27 commands | **1 finding**: `unreadable file: error::DeprecationWarning` (call 11) | N1: stale clean; false positive, same F3 class as `-W error` |
| QA red 2a (`a94d64be2ed7d5a13`) | findings, false positives | `shutil.rmtree(p)` only | agree |
| QA red 2c, dev green 2c | `python -W error` read as a file named "error" | same, 1 and 2 findings | agree |
| dev green 3a (`ab501238be720886b`) | `rm -rf x` from a base64 string | same | agree |

The recorded false positives are truly false. I read the commands in the transcripts: red 2a's `shutil.rmtree(p)` is a `python -c` probe that hands the text to `scan_command` (`import delete_guard as d; d.scan_command('python -c ...')`), nothing runs; red 2c and green 2c's "error" is the option argument of `python -W error -c "import ast..."` and `python -W error -c "import command_source"`, a flag value not a file (run call 12 and 13); green 3a's `rm -rf x` is `d.scan_command('echo cm0gLXJmIHg= | base64 -d | sh', ...)` passed as a string, never executed. The red 2a "grep judged as a run" and "unreadable relative path" classes are gone at HEAD, as recorded. No scan hit corresponds to an executed delete.

Seats without a source. None of the 25 build seats is unscanned. The slice rule (an unscanned seat is a gap that blocks ship) bites on one entry:
- **N2, ctx-1 PM ready (Copilot CLI): entry `unscanned, "Copilot has no command source"`.** It ran 2026-11-01 20:00:32Z, about seven hours before the Copilot reader (`bc0881e`, 19:12 local = 03:12Z). The record is correct for the time it was written and stale now: the reader exists, so the slice's blocking-gap rule says the entry must be replaced. I re-scanned it with the reader. Session `959ac8e1-4a43-45bc-a07e-17fe5d07670d`, cwd `C:\Users\ringe\projects\phrase-lang`; `scan_entry('copilot', None, 'copilot', cwd=..., started='2026-11-01T20:00:32Z')` finds exactly one session and returns `clean`; the CLI says `clean: pm-ready-ctx1, 1 command(s) scanned`. The 12 tool calls are glob, view, grep, one PowerShell `New-Item -ItemType Directory -Force` into the scratchpad, and one `create` of the proposal; no delete. Meaning: the gap is closable and is closed by this evidence, but the record entry must be updated by the facilitator (replace with this result) before sign-off; until then it is a recorded gap that the slice says blocks ship. Not counted as a second blocker because the replacement is mechanical and the scan is clean.
- Decision-window seats: 20 Copilot attempts in the five windows are recorded `unscanned, 0 commands, empty reply`. I checked each: every Copilot session starting within 90 s of the attempt has no assistant record, so none ran a command. The reader says "no command source" for them because their `session.start` falls 13 to 24 s after the recorded `started_at` and `command_source.py:348` allows 10 s. The 20 Claude voter seats are recorded clean. The 9 agy voter seats are recorded `unscanned` (agy has no reader): by the slice sentence that is a gap that blocks ship, but window 228-agy-seats adopted it knowingly, with build seats kept off agy (N8).
- This seat: not scanned yet, see disclosure.

## Other findings (non-blocking, record in the closing record)

- **N1.** Green 1c's "clean, 27" is no longer clean at HEAD (`error::DeprecationWarning`, F3 class). Add it to `working_state.followup_F3_additions`, and say the early clean entries were taken with the code of their day.
- **N2.** Copilot ready seat: replace the entry with the re-scan above.
- **N3.** Deviations list only criterion 1's overrun (`deviations[2]`). Criterion 2 (3 cycles) and 3 (2 cycles) also exceeded the slice's one cycle each; they are covered by `228-c2-scope` and stand-down 1 but not named as deviations. Twelve increments against the rule of three should be stated once.
- **N4.** `fa2351f` is 427 lines against the slice's 400-line limit (D136); unrecorded. The slice said the Copilot reader would move to a follow-up if green 2b passed 400 lines; green was 225 lines but the red alone broke it.
- **N5.** Codex rollouts deferred to F2: "other backends' transcripts are read where they exist" is only partly true, and F1 to F6 (15 items) are still "one-line cards, not filed" (`board/` ends at 238). The deferrals have no owner until filed.
- **N6.** Copilot locator risk. 20 of 20 Copilot sessions started 13 to 24 s after the recorded `started_at` against a 10 s tolerance (`command_source.py:348`). `facilitator.py:351` takes `launch_time` just before the process starts, so the real lag may differ, but no live `run_backend` Copilot seat with a reply has been scanned to prove the 10 s holds. It fails safe (a blocking "no command source", never a false clean). Check in the first live Copilot seat.
- **N8.** The 9 agy decision seats are unscanned with no reader to close the gap (see above); sign-off should name it as accepted by 228-agy-seats rather than leave the slice sentence unanswered.
- **N7.** Counts in the record differ from my HEAD scan for green 1c, 1d, 1e, 2b (27/30, 31/40, 38/45): later reader changes count writes and PowerShell calls; findings are the same except N1.

## What the gate does not cover
The seat transcripts of the five decision windows other than Claude and agy ones were not re-read. The test suite was run on `743a4a1`, not on a clean clone; `.mob/` is untracked in mob-crosscli.

## Hand-off to sign-off
Clear B1 by a window (option a or b) and record it; then replace the ctx-1 entry (N2), add N1 and N3 to the closing record, and re-run `python scripts/mob_process.py check`. No re-gate is needed if the window picks (b); if (a) is picked, a red/green cycle 3c and its own gate line on the full suite follow.
