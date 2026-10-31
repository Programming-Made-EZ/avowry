# Card 227 security review (ctx-3, adversarial lens)

Seat: security reviewer, ctx-3, a fresh context and a Claude seat (the 14:59 UTC probes in `.mob/records/probes/2026-10-30.jsonl` show Codex, Copilot, Grok and agy without usable quota). I wrote none of the code. An independent reviewer reads the same range in parallel, in its own file.

Range: mob-defaults (C:\Users\ringe\mob-crosscli) 56b39bc..00f9e93, and avowry 73cdf20 and 810e818 (text only: the agreement bullets for the trust-ladder record and for push-every-commit).

Method: about 500 strings passed to `delete_guard.scan_command` in a Python session (nothing was executed, no delete command was run anywhere), and 22 fixture repositories built in `C:\Users\ringe\AppData\Local\Temp\claude\C--Users-ringe\73298da3-f8ff-4a30-af55-b3e95ffa16f4\scratchpad\fix1793460129\` and read with `mob_process.unpushed_commits` / `unpushed_problems`. The fixtures are still there; none was deleted. Probe scripts: `...\scratchpad\p.py`, `p2.py`, `fx.py`. "Flagged" below is `scan_command(s, ["scratch"])` returning findings, run 2026-10-31.

## Verdict: CHANGES REQUESTED

The slice's three must-fix items are closed in the mechanisms they named (allow-list, fail-closed record check, fail-closed detector). The command-position scan was built, but the parser itself has holes that are not in card 228's list and that lie in ordinary, common idioms: PowerShell script blocks, command substitution, heredocs fed to an interpreter, and a one-line `if ...: shutil.rmtree(p)`. These are the shapes an agent writes, so fix findings 1 to 5 in card 227 before it is called Done. Findings 6 to 10 are medium and cheap. Everything else is low, accepted or card 228's.

## 1. The slice must-fix items

| Slice item | Status | Evidence |
|---|---|---|
| Must-fix 1: command-form gaps accepted into 227 (classes A to F, command position) | PARTLY CLOSED | Closed: flag position (`rm $d -rf`, `rm $d --recursive`), `-vrf`, `-R`, `--recursive --force`, `-- $d`, `ri`/`del`/`erase`/`rd`/`rmdir` with `-r`/`-Rec`/`-Recurse:$true`, `rd /s/q`, `del /s /q`, `shutil.rmtree` as a statement, `from shutil import rmtree`, `sh.rmtree`, `find -delete`, `xargs rm -rf`, `find -exec rm`, `git -C r clean -fdx`, `git clean -n` quiet, env prefix, `sudo`, `\rm`, `/bin/rm`. Open: findings 1 to 7, 15 and 16. |
| Must-fix 2: literal-path allow-list | CLOSED for the stated list | Flagged: `rm -rf .git`, `rm -rf /c/`, `rm -rf /c/Users/ringe/foo`, `rm -rf /tmp/x`, `rm -rf ~`, `scratch/../.git`, `scratch`, `scratch/`, `scratch-evil/x`, `../scratch/x`, `C:/scratch/x`, `foo`, `node_modules`, `rd /s /q C:\x`, `shutil.rmtree("C:/Users")`, `shutil.rmtree("..")`. Allowed: `scratch/x`, `./scratch/x`, `scratch\x`, `.\scratch\x`, `"scratch/x"`, `rmtree("scratch/x")`. `scan_command(c)` with no roots allows nothing. Residuals: finding 7 (relative root against cwd, links) and finding 6 (escaped quote). The scan-limits rule text the amendment promised is not in any agreement (finding 14). |
| Must-fix 3: fail-closed coverage | CLOSED in code, with residuals | Record check: `unscanned` fails (once per seat), a missing `command_scans` key fails from card 227 (int 227 and "227-x" both fail with the "needs one entry per task_force context" message). Detector: future-dated commit FAILs (fixtures 2, 3, 4: "future (dated 131531867 seconds after now)"); non-zero git exit FAILs (fixture 12, corrupt `.git/HEAD`: "not a git repository"); no origin FAILs (fixtures 9, 9b: "no origin", also for a remote named `github`); worktree HEADs counted (fixture 7b detached, fixture 8 linked worktree both FAIL when old). Residuals: findings 8 to 13. |
| Amendment: rule text states the scan's limits in the template and the avowry agreement | OPEN | `grep -n -i "scan"` finds only the old safety bullet (avowry TEAM_WORKING_AGREEMENT.md:443, template:510). 73cdf20 and 810e818 add the ladder and push bullets only. Finding 14. |
| Avowry 73cdf20 and 810e818 (text) | OK with two over-claims | Claims checked against code: 5-minute limit, 300 passes / 301 fails, HEAD + upstream branches + worktree HEADs, `MOB_WATCHED_REPOS`, audit runs on the audited repo and mob-defaults. Over-claims: "in every repository the mob works in" (only named repos are watched, finding 10) and "older of the committer time and the earliest reflog entry" (the reflog time is the committer date when one is forced, finding 12). |

## 2. Findings

Severity is the harm if the gap is used. Tags: **227** = belongs in this card, **228** = already owned by `board/228-*.md` (or should be added to it).

### Bypasses of the command-position scan

**1. HIGH, 227. PowerShell script blocks hide the delete.** Words inside `{ ... }` and after `if (...)` are never at command position.
Not flagged: `if (Test-Path $p) { Remove-Item -Recurse -Force $p }`; `if (Test-Path C:/Users) { Remove-Item -Recurse -Force C:/Users }`; `Get-ChildItem | ForEach-Object { Remove-Item -Recurse -Force $_ }`; `foreach ($d in $dirs) { Remove-Item $d -Recurse -Force }`; `try { Remove-Item -Recurse -Force $p } catch {}`; `if ($x) { rm -rf $p }`; `pwsh -Command "Get-ChildItem | ForEach-Object { Remove-Item -Recurse $_ }"`; `Start-Job { Remove-Item -Recurse $p }`; `Invoke-Command { Remove-Item -Recurse $p }`. (`Remove-Item -Recurse -Force $p }`, with the brace after, is flagged only because `}` becomes a target.) PowerShell is the primary shell on this machine and the 2026-10-08 wipe was a PowerShell/agy dispatch.
Fix: in the clause splitter make `{` and `}` clause boundaries, and make a `(...)` after `if`/`while`/`foreach`/`for`/`switch`/`elseif` a condition that ends before the block; add `%`, `foreach-object`, `where-object` as pass-through words. One red per row above.

**2. HIGH, 227. Command substitution contents are never scanned.** `$(...)`, backticks and `<(...)` are kept inside one word, and only the verb of the clause is judged.
Not flagged: `echo $(rm -rf $d)`; `x=$(rm -rf C:/Users)`; ``echo `rm -rf $d` ``; `echo "$(rm -rf $d)"`; `diff <(rm -rf $d) x`; `echo ${x:-$(rm -rf $d)}`. (Only a substitution used as the delete's own target, `rm -rf $(mktemp -d)`, is caught.)
Fix: for every word, find `$(`...`)`, backtick pairs and `<(`/`>(` and call `_scan_text` on the inside (depth-limited like `bash -c`), including inside double quotes.

**3. HIGH, 227. A heredoc fed to a shell or interpreter is stripped as data.** `_strip_heredocs` drops every heredoc body whatever reads it.
Not flagged: `bash <<'SH'\nrm -rf $d\nSH`; `sh -s <<'SH'\nrm -rf $d\nSH`; `python - <<'PY'\nimport shutil\nshutil.rmtree(p)\nPY`; `python3 <<'PY'\n...shutil.rmtree(sys.argv[1])\nPY`; `cat <<'PY' | python3\n...PY`; `powershell -Command - <<'PS'\nRemove-Item -Recurse $p\nPS`. `python - <<'PY'` is the commonest way an agent runs a longer Python snippet. The amendment's class H needs the heredoc that writes the rule to stay quiet (`cat <<EOF > x.md`), not the one a shell reads.
Fix: decide per heredoc by the command that owns it (or the next command in a pipeline): shell and `-s` shells scan the body with `_scan_text`; `python`/`py` with `-` or no argument scan it with the rmtree rule; `cat`, `tee`, `git commit -F -`, `echo`, redirects to a file stay data.

**4. MEDIUM-HIGH, 227. `shutil.rmtree` is only seen at the start of a statement.** `_RMTREE` is anchored on start of text, `;`, `|`, `&` or a line start.
Not flagged (inside `python -c`): `if os.path.isdir(p): shutil.rmtree(p)`; `for p in a: shutil.rmtree(p)`; `x = shutil.rmtree(p)`; `os.path.isdir(p) and shutil.rmtree(p)`; `[shutil.rmtree(p) for p in a]`; `list(map(shutil.rmtree, ps))`; `from shutil import rmtree as r; r(p)`; `r = shutil.rmtree; r(p)`. The one-line `if isdir: rmtree` is the standard cleanup idiom.
Fix: inside Python text (a `python -c` body, or a heredoc found by finding 3) flag every `rmtree` token that is not followed by a literal scratch child: `(?<![\w])(?:\w+\.)*rmtree\b`, whether it is called, assigned, passed or imported `as`. Do not run the rule on bare shell text (that is also the cure for finding 17).

**5. MEDIUM, 227. The wrapper list is a closed set, and the forms the slice review listed as flagged fall out.**
Not flagged: `timeout 30 rm -rf $d`; `ionice -c3 rm -rf $d`; `setsid rm -rf $d`; `watch rm -rf $d`; `parallel rm -rf {}`; `busybox rm -rf $d`; `busybox sh -c 'rm -rf $d'`; `wsl rm -rf $d`; `wsl -e rm -rf $d`; `ssh host rm -rf $d`; `ssh host "rm -rf $d"`; `fish -c 'rm -rf $d'`; `csh -c ...`; `env -S 'rm -rf' $d`; `find scratch/x -ok rm -rf {} \;`. The slice review (Forms table) marked `busybox`, `wsl`, `ssh host "..."` as flagged by the substring scan and "must flag"; the amendment's list names `sudo`, `command`, `time`, `nohup`, `env`, `bash -c`, `sh -c`, `xargs`, `eval`, `find -exec` only, and card 228 does not name the others, so they were dropped without being moved.
Fix: extend the wrapper set (`timeout`, `ionice`, `setsid`, `watch`, `parallel`, `chrt`, `taskset`, `flock`, `busybox`, `wsl`, `ssh`, `fish`, `csh`, `tcsh`, `-ok`, `-okdir`), and add a default for an unknown verb: scan `words[1:]` as a command when the verb is not a known data verb (`echo`, `printf`, `grep`, `rg`, `cat`, `git commit`, `sed`, `awk`, `ls`, `test`). One red per row.

**6. MEDIUM, 227. An escaped quote desynchronises the tokenizer.** `_clauses` opens a quote at `'` or `"` even when a backslash (bash) or backtick (PowerShell) escaped it, so the real separators that follow sit "inside" a quote.
Not flagged: `echo \'; rm -rf $d; echo \'`; ``Write-Host `'; Remove-Item -Recurse $p; Write-Host `'``; `f() { rm -rf $1; }; f /` and `f(){ rm -rf $1; }` (a function body is a word, not a clause).
Fix: outside quotes, a backslash (and a backtick for PowerShell) consumes the next character; split `name() {` and `function name {` so the body is scanned as a clause list.

**7. MEDIUM, 227. The scratch root is relative, but the command can change directory first; links are not seen.**
Not flagged: `cd /; rm -rf scratch/x`; `cd ..; rm -rf scratch/x`; `cd /c/Users && rm -rf scratch/x`; `pushd /; rm -rf scratch/x`; `ln -s / scratch/l; rm -rf scratch/l/x` (the second command alone is a literal child, the first creates the link; also `mklink /J scratch\l C:\ & rd /s /q scratch\l\x`, `New-Item -ItemType Junction ...`). A backslash is also read as a separator in bash: `rm -rf scratch\x` is allowed but bash deletes `./scratchx`.
Fix: after a `cd`/`pushd`/`Set-Location`/`sl`/`Push-Location` whose target is not itself a literal scratch child, treat every later relative path in the same command as unjudgeable (a finding). Say the symlink/junction residual in the rule text (`safe_delete` resolves links, the scan cannot).

**8. MEDIUM, 227. Only tools named `Bash` and `PowerShell` are read; other tools that take a `command` are counted and dropped.**
`commands_from_transcript` counts every other `tool_use` in `other`, and `scan_entry` throws the count away (`commands, _ = ...`). This session's own tool list has `Monitor`, `mcp__terminal__run_in_terminal`, `mcp__scheduled-tasks__create_scheduled_task`, each of which runs a command string. A seat can run `rm -rf $d` through one of them and the entry reads `clean`.
Fix: scan `input.command` of any `tool_use` whose input has a string `command` (same seam, one line); record `other_tools` in the entry and have `check_record` show it. Scanning Write/Edit contents stays card 228.

**9. MEDIUM, 227. The transcript the scan reads is the seat's to change or shadow.**
`locate_transcript` takes the first slug folder, in sorted order, that holds `<session>.jsonl`; the seat can create a clean decoy in an alphabetically earlier folder, or edit or truncate its own transcript before it exits (a truncated one is caught only if it has no assistant record; an edited one reads clean).
Fix: derive the slug from the seat's cwd (Claude's rule: non-alphanumerics become `-`) and treat more than one match as "no command source"; store `size` and `sha256` of the transcript in the scan entry and `command_scans` so a later edit shows.

**10. MEDIUM, 227. The record check trusts the record.**
Evidence from `_command_scans_problems` (strings through the function): `{"card": "F-071"}` and `{"card": "card-227"}` and `{"card": true}` return `[]` (no number, so the whole requirement is skipped); `{"context":"ctx-1","status":"not run"}` for every seat returns `[]` although the message says "with a reason" (no reason is checked); `clean` carries no session, transcript or command count, so `{"status":"clean","commands":0}` passes; `task_force: []` with `command_scans: []` passes; the status a real run produces, `no command source`, is rejected as "malformed" and reported as "has no entry" (fail-closed, but the message sends the reader the wrong way). Related: the ladder check keys on the word "safety" in a rule the author writes (a deviation worded "delete rule" is skipped) and a `step` of `none: x` is free text. Related for the detector: `Facilitator.scan_pushes` returns silently when `MOB_WATCHED_REPOS` is unset, and the audit watches only the audited repository and mob-defaults, while the agreement says "every repository the mob works in" (play-tracker, BaccaratSim and scratch clones are unwatched).
Fix: a card with no parseable number is a problem; `not run` needs a non-empty `reason`; `clean` and `findings` need `session` and an integer `commands`; map `no command source` to a failing status with its own message; unset `MOB_WATCHED_REPOS` posts a one-time "nothing watched" Push flag; reword "every repository" to "the repositories named".

**11. MEDIUM-LOW, 227 (the amendment accepted part of this as a residual). The detector misses commits that are on no upstream-tracking branch.**
Fixture 5: `git checkout -b backup`, commit (committer date 15 h old), `git checkout main` returns `[]`. Fixture 7: a commit made on a detached HEAD and left by `git checkout main` is invisible (only the older `second` commit is reported); it exists only in the reflog. Fixture 13: a stash returns `[]`. A new local branch that was never pushed is the commonest case of unpushed work, and the rule text says only "not a backup branch with no upstream".
Fix: take every `refs/heads/*` as a tip, not only those with an upstream (a branch already inside a remote ref adds nothing, so there are few false FAILs), add `refs/stash`, and add HEAD reflog entries from the last N days that no ref reaches. If this is judged too big, move it to card 228 and keep the rule text as is.

### New bypasses and limits of the detector (fixtures)

| # | Fixture | Result | Judgment |
|---|---|---|---|
| a | 2: commit with `GIT_COMMITTER_DATE=2031-01-01`, reflog on | `future (dated 131531867 seconds after now)` FAIL | closed |
| b | 3: same, `core.logAllRefUpdates=false` | `future` FAIL | closed |
| c | 4: same, evaluated 10 minutes later | `future` FAIL | closed |
| d | 1: committer date 15 h ago, unpushed | `55332 seconds` FAIL | closed |
| e | 7b, 8, 8b: detached HEAD, linked worktree branch, moved worktree folder | each FAILs when old | closed |
| f | 9, 9b: no origin; remote renamed `github` | FAIL `no origin` | closed (a false FAIL for a mob that names its remote otherwise: safe direction) |
| g | 12: corrupt `.git/HEAD`; 17: a subdirectory of a repository | FAIL `not a git repository` | closed |
| h | 6: `git update-ref refs/remotes/origin/main HEAD` | `[]` | accepted, named in the rule text |
| i | 14: `origin` is a local bare folder, pushed there | `[]` | accepted in effect (named: "an origin that points at a local clone"); LOW |
| j | 15: `git commit --amend --no-edit` on an old unpushed commit | new sha, age 0, `[]` | accepted ("a rewrite resets the committer time"); LOW |
| k | 16: `git rebase --force-rebase HEAD~1` | new sha, age 0, `[]` | same as j |
| l | 2 (reflog): `git reflog` shows `main@{1924992000 +0000}` for the future commit | the reflog time is the forced committer date | The "older of committer time and reflog" rule gives no independent clock against a forged date or a rewrite. It is harmless (the future date still FAILs) but the text over-claims. |

**12. LOW, 227 (wording) and 228 (design). Rewrites restart the clock and the reflog is not an independent clock** (rows j, k, l). Fix: say so in the rule text now. Later: remember each sha first seen in a small state file keyed by `git patch-id`, so a rewritten commit keeps its first-seen time. Optional: warn when the `origin` URL is a path or `file://` (row i).

**13. LOW, 227. Future-date tolerance and flag churn.** There is no skew tolerance: a commit one second after `now` is "future" (an audit run with `now` taken before a parallel seat commits will flag it). The key `scan_pushes` uses to dedupe contains "(dated N seconds after now)", and N changes each seat, so the same future commit is flagged again after every seat. Fix: dedupe on `repo + sha + "future"`; optionally allow 60 s of skew (a date a few minutes ahead still ages out and FAILs in minutes, so this opens no bypass).

### Further bypasses and gaps of the scan (smaller)

**14. LOW-MEDIUM, 227. The amendment's plain-words rule text is missing.** The template and the avowry agreement state the push and ladder rules but nothing about the scan: "the scan sees commands at command position in a Claude transcript, after the fact; encoded, hidden-verb and written-file forms are not covered; a non-Claude seat fails the record". Fix: add the bullet to both (template first), and cite card 228 as the owner of the rest.

**15. LOW, 227. Option and spelling gaps in class A to D.** Not flagged: `rm --rec $d`, `rm --recur $d` (GNU accepts unambiguous prefixes of `--recursive`); `rd/s/q scratch\x` (cmd accepts no space after the verb; the verb is read as `rd/s/q`); `git clean -f -- -n` (a path named `-n` counts as dry run); `g=git; $g clean -fdx` (hidden verb, 228). Fix: accept any `--r`..`--recursive` prefix of three or more letters for rm; split the cmd verb at `/`; stop reading options at `--` in the git clean dry-run test.

### False positives on ordinary commands

**16. LOW-MEDIUM, 227. PowerShell and shell idioms are flagged though the target is a literal scratch child** (fail-closed direction, but they invite rewording the command to dodge the scan, the counter-metric).
Flagged: `Remove-Item -Recurse -Force scratch/x -ErrorAction SilentlyContinue` and `-ErrorAction Ignore` (the parameter value is read as a second target); `Remove-Item -Recurse -Filter *.x scratch/x` and `-Exclude a scratch/x` (value read as a target); `{ Remove-Item -Recurse -Force scratch/x }` (`}` is a target); `rm -rf scratch/x # cleanup` (the comment word is a target); `find -H scratch/x -delete` (`-H` read as a start path). Also flagged by design and fine: `find . -name '*.pyc' -delete`, `git rm -r docs/old`, `find scratch -mindepth 1 -delete` (the root itself is refused; arguable).
Fix: a table of PowerShell common parameters that take a value (`-ErrorAction`, `-WarningAction`, `-Exclude`, `-Include`, `-Filter`, `-Stream`, `-Credential`, `-Encoding`, `-ErrorVariable`, `-OutVariable`), drop `}`/`)` tokens, drop a `#` word and the rest of the line outside quotes, accept `-H`/`-L`/`-P` before find's start path.

**17. LOW-MEDIUM, 228 (named there; fix is coupled to finding 4). The known false positive: `rmtree` named inside a read-only command's text.** `_RMTREE` runs on the raw text of every command and is anchored on `^`, `;`, `|`, `&` even inside quotes.
Flagged: `git commit -m 'a; shutil.rmtree(p)'`; `git commit -m "fix\n\nrmtree(p) in x"` (a message line starting with `rmtree(`); `grep -n 'x; rmtree(p)' f.py`; `rg "| rmtree(" .`; `python -m pytest -k 'a|rmtree(x)'`; `git log -S ';rmtree(p)' --oneline`; `echo 'x;rmtree(p)'`; `python x.py --opt 'a;rmtree(b)'`. Not flagged and right: `grep -rn 'shutil.rmtree(' .`, `grep "shutil.rmtree(p)" f.py`, `echo 'shutil.rmtree(p)'`, `git commit -m 'do not call shutil.rmtree(p)'`, `echo "rm -rf is banned"`, `git commit -m "never run rm -rf $d"`, `cat <<EOF > n.md ... EOF`, `safe_delete(p, roots)`, `rm -f file.txt`, `git clean -n`, `kill 1234`, `Remove-Item file.txt`, `rd`/`del` without `/s`.
Fix (in card 228 with finding 4): apply the rmtree rule only to Python text (a `python -c` body or a heredoc fed to python), never to the raw command string.

## 3. Split between the cards

**Card 227 (this card), by priority:** 1 (PowerShell blocks), 2 (substitution), 3 (heredoc to interpreter), 4 (rmtree anywhere in Python text), 5 (wrappers), 6 (escaped quote, functions), 7 (cd and links), 8 (any tool with a `command`), 9 (transcript locator and hash), 10 (record-check self-report, watched repos), 14 (scan rule text), 16 (false positives), 15 (spellings), 13 (future flag churn), 11 (optional, may move). Findings 1 to 5 are my must-fix for Done; 6 to 10 and 14 should follow in the same card because each is a few lines and a red case; the rest may be recorded as known gaps in the rule text.

**Card 228 (already owns; verified still open, no defect in 227):** hidden verbs (`"rm" -rf $d`, `r""m -rf $d`, `R=rm; $R -rf $d`, `$R -rf /`); `-EncodedCommand`; `base64 -d | sh`; `echo 'rm -rf $d' | bash`, `printf 'rm -rf $d' | sh`, `sh <<< 'rm -rf $d'` (the delete as a string piped or fed to a shell); `eval $x $d`; `Remove-Item` fed by the pipeline, splatted or line-continued; `[IO.Directory]::Delete`; `rsync -a --delete e/ $d/`; `os.removedirs`, `os.system("rm -rf " + p)`, `subprocess.run(["rm","-rf",p])`; `node -e "fs.rmSync(p,{recursive:true})"`; `rimraf`; Write and Edit contents and `bash x.sh`; other backends' readers; the `rmtree` false positive (17); the scan-transcript dedupe; the Decider rendering; automation of Agent-tool transcripts; `git reset --hard`, `git push --force`.
**Suggest adding to card 228's scope:** `robocopy empty C:\Users /MIR` and `/PURGE` (not flagged, named nowhere); `getattr(shutil,"rmtree")(p)`; a seat that runs a script it wrote in an earlier call; a repository the detector does not watch; the amend/rewrite clock (12).

## 4. Checks run on this file

`git diff --cached | tr -cd '\r' | wc -c` must print 0 before the commit; the commit message carries no trailer.

## Re-check (ctx-3)

Seat: security reviewer, ctx-3, a fresh context and a Claude seat (the 22:23 UTC probes in `.mob/records/probes/2026-10-30.jsonl` show Codex, Copilot, Grok and agy without usable quota). I wrote none of the code. Range: mob-defaults 00f9e93..f8a45da (greens 1c, 1d, 1e, 1f, 2b, 3c, 3d) and avowry c803a43 and 5deedcc. Method: a scratch COPY of `delete_guard.py`, `command_source.py` and `mob_process.py` in `...\scratchpad\rc3-copy-20261031a\` (no file deleted anywhere); about 400 strings passed to `scan_command(s, ["scratch"])` in a Python session (never executed); a fixture transcript tree (`projroot-a`) for the source; `_command_scans_problems` and `scan_limits_problems` called on hand-built records; and a scan of every transcript under `~/.claude/projects` for false positives. Probe scripts: `probe_old.py`, `probe_new.py`, `probe3.py`, `p4.py` to `p6.py`, `fpscan.py`, `fp2.py` in that folder. No delete command was run.

## Verdict (re-check): APPROVE

All five findings I called must-fix for Done (1 to 5) and the medium ones (6 to 10, 14) are closed with evidence, and the items the triage moved are present as lines 1 to 7 of card 228. The new bypasses I found are PowerShell or cmd spellings and eval-style string forms that are rare in agent output; none is an ordinary idiom, so none blocks Done. The false-positive cost is real but is a manual read, not a failing record (see 3). Everything new is tagged 228; N2 may be done in 227 if it is cheap.

### 1. Status of the 17 findings (strings re-run on f8a45da)

| # | Finding | Status | Evidence |
|---|---|---|---|
| 1 | PowerShell script blocks | CLOSED | All 9 rows flagged: `if (Test-Path $p) { Remove-Item -Recurse -Force $p }`, `ForEach-Object { ... }`, `foreach ... { }`, `try { } catch {}`, `if ($x) { rm -rf $p }`, `pwsh -Command "... { }"`, `Start-Job { }`, `Invoke-Command { }`. Also `& { ... }`, `$sb = { ri -r $p }; & $sb`, `do { } while`, `else { }`, `switch ... { 1 { } }`, `gci \| % { ri -r $_ }`, `function f { ri -r ... }` flagged. |
| 2 | Command substitution | CLOSED | All 6 rows flagged: `echo $(rm -rf $d)`, `x=$(rm -rf C:/Users)`, backticks, `"$(...)"`, `<(...)`, `${x:-$(...)}`. |
| 3 | Heredoc to a shell or interpreter | CLOSED | Flagged: `bash <<'SH'`, `sh -s <<'SH'`, `python - <<'PY'`, `python3 <<'PY'`, `cat <<'PY' \| python3`, `powershell -Command - <<'PS'`. Data stays quiet: `cat <<EOF > x.md`, `cat > f.py <<'EOF'`, `git commit -F - <<'EOF'`, `tee f <<'EOF'`. A delete after a closed heredoc (`cat <<EOF` ... `EOF` then `rm -rf $d`, `<<A <<B`, `<<-EOF`, `cat <<'EOF' >f; rm -rf $d`) is flagged. |
| 4 | `rmtree` only at statement start | CLOSED | All 8 rows flagged (`if isdir: shutil.rmtree(p)`, `for`, assignment, `and`, list comprehension, `map(shutil.rmtree, ps)`, `from shutil import rmtree as r`, `r = shutil.rmtree`), also `import shutil as s; s.rmtree(p)`, `__import__('shutil').rmtree(p)`, `py -3 -c`, `python3.12 -c`, a full interpreter path, `ipython -c`. |
| 5 | Closed wrapper list | CLOSED | All 15 rows flagged: `timeout`, `ionice`, `setsid`, `watch`, `parallel`, `busybox`, `busybox sh -c`, `wsl`, `wsl -e`, `ssh host`, `ssh host "..."`, `fish -c`, `csh -c`, `env -S`, `find -ok`. Also `time -p`, `sudo -u bob`, `env -i`, `env --`, `nice -n 5`, `bash -lc`, `-xc`, `-ec`, `--login -c`, `/usr/bin/env bash -c`. |
| 6 | Escaped quote, function bodies | CLOSED | A backslash-quote followed by `; rm -rf $d; echo` and a backslash-quote is flagged (six variants, backslash built with `chr(92)` in `p4.py`); PowerShell backtick-quote variant flagged; `f() { rm -rf $1; }; f /` and `f(){ rm -rf $1; }` flagged. |
| 7 | `cd` before a relative root; links | `cd` part CLOSED; links part MOVED to 228 (line 1, present) | `cd /; rm -rf scratch/x`, `cd ..; ...`, `cd /c/Users && ...`, `pushd /; ...`, `cd scratch/x && rm -rf ../../y` flagged. `ln -s / scratch/l; rm -rf scratch/l/x`, `mklink /J ... & rd /s /q scratch\l\x` and `rm -rf scratch\x` are still allowed; the scan-limits text says a link inside scratch is not seen (avowry line 451). Accepted. |
| 8 | Only Bash and PowerShell read | CLOSED | Fixture transcript with a `Monitor` tool_use: `rm -rf $d` gives `findings`, `commands 1`, `other_tools 1` (the Write call is counted, not read), with `size` and `sha256`. |
| 9 | Transcript locator | CLOSED for shadowing; residual N3 | A decoy `sess1.jsonl` in `aaa/` beside the real one in `zzz/` gives `no command source` (two folders). The hash is stored, but it is taken at scan time, so it shows no later edit. |
| 10 | Record check trusts the record | CLOSED, small residual | `{"card":"F-071"}`, `"card-227"`, `True` each give a "no parseable number" problem; `not run` without a reason, `clean` without a session or transcript, `clean` with `commands: True` each fail; `no command source` has its own message. Unset `MOB_WATCHED_REPOS` now watches the seat's repository and mob-defaults (`facilitator.py` lines 1312-1314). "Every repository the mob works in" is gone from the agreements (grep prints nothing). Residuals: `clean` with any session string and any count passes (self-report); `task_force: []` with `command_scans: []` passes. LOW, N3. |
| 11 | Commits on no upstream branch, detached, stash | MOVED to 228 (line 7, present) | Rule text still says "not a backup branch with no upstream". Accepted. |
| 12 | Rewrite resets the clock; reflog not independent | CLOSED (wording) | Avowry lines 103-109: "a forced committer date sets the reflog time too"; residuals name amend/rebase, a local-path origin, hand-written remote refs. The patch-id state is in card 228 line 7. |
| 13 | Future-date flag churn | CLOSED | `scan_pushes` keys on repository path plus the problem with the seconds figure normalised to `N seconds` (`facilitator.py` line 1319), so the same commit is flagged once. |
| 14 | Scan rule text missing | CLOSED | `scan_limits_problems` returns `[]` for mob-defaults and for avowry; avowry line 451 (c803a43) states after the fact, detector not a block, Agent-tool seats by hand, encoded, hidden-verb, written-file and non-Claude forms, links in scratch, and names card 228. `push_rule_problems` also returns `[]` on both. |
| 15 | Option and spelling gaps | MOVED to 228 (line 2, present) | Still unflagged: `rm --rec $d`, `rm --recur $d`, `rd/s/q scratch\x`, `git clean -f -- -n`. Accepted (LOW). |
| 16 | False positives in PowerShell and shell idioms | MOVED to 228 (line 3, present) | Still flagged: `Remove-Item -Recurse -Force scratch/x -ErrorAction SilentlyContinue`, `-Filter *.x`, `rm -rf scratch/x # cleanup`, `find -H scratch/x -delete`. `{ Remove-Item ... scratch/x }` is now clean. |
| 17 | `rmtree` named in read-only text | MOVED to 228 (the "Also owned here" line, present) | Still flagged, all 8 strings: `git commit -m 'a; shutil.rmtree(p)'`, `grep -n 'x; rmtree(p)' f.py`, `rg "\| rmtree(" .`, `git log -S ';rmtree(p)'`, `echo 'x;rmtree(p)'`, and the others. See 3. |

Strings that must stay clean are clean: `rm -rf scratch/x`, `git commit -m "never run rm -rf $d"`, `echo "rm -rf is banned"`, `grep 'shutil.rmtree(' .`, `git clean -n`, `rm -f file.txt`, `Remove-Item file.txt`. A further 66 ordinary commands (`git status`, `python -m pytest`, `cd x && ls`, `ssh host ls`, `timeout 30 python x.py`, `Get-ChildItem | ForEach-Object { $_.Name }`, a harmless `python - <<'PY'`, a `cat > f.sh` heredoc with `rm -rf $d` inside, `git rm -r --cached x`, `rm -r scratch/x`) gave 0 false positives.

### 2. Real red 1c delete (task 4)

`scan_command('M=$S/mut-227-1c; rm -rf "$M" 2>/dev/null; mkdir -p $M', ["scratch"])` returns one finding, kind `recursive delete on a variable or unresolved path`, snippet `rm -rf "$M"`; the bare `rm -rf "$M" 2>/dev/null` returns the same. Flagged. The scan also flags real variable deletes in other seat transcripts (see 3).

### 3. False-positive cost (task 3)

Measured with the current scanner on every `.jsonl` under `~/.claude/projects` (read only):
- last 3 days: 957 files, 928 clean, 29 with findings, 0 without a command source;
- last 14 hours: 1 file with a real `rm`-style hit and 48 `rmtree` hits in 9 files. All 48 are strings inside a Python heredoc or `-c` body that a seat was feeding to the scanner (test data, mutation names, scanner source being edited); none is a delete. Same cause as the record's six false-positive transcripts.
- the other findings in the 3-day window include genuine `rm -rf "$S/mut"` and `rm -rf $S` on variable scratch paths by seats (about 15 files); the scan is doing its job on those.

Does it block Done? No. A `findings` entry only needs a non-empty reason and does not fail the record (`_command_scans_problems`: a Safety flag, not missing coverage), so the false positives cost a manual read and a sentence, not a re-run. Do they invite rewording? A little, and the dodge is already visible: write the snippet to a file with a `cat > f.py` heredoc (quiet) and run `python f.py` (not scanned, card 228 line 6); I did this myself in this review without intending to. That makes the hole in 228 line 6 (a script written earlier and run later) the place where noise and evasion meet.

The larger cost is the one the record already shows: noise led the facilitator to skip reading a `findings` result, and a real delete (red 1c) went unreported for about 2 h 40 min. The record names the fix (classify every `findings` result before committing). A structural fix belongs in 228 (N1).

### 4. New findings

**N1. MEDIUM, 228. Parse Python text instead of matching it.** The `rmtree` rule runs on the text of a Python heredoc or `-c` body, so a string literal containing `rmtree` flags (48 hits in 14 h). Fix: `ast.parse` the body and flag a call whose function name is `rmtree` (or an alias from `from shutil import rmtree as r`), falling back to the text rule when it does not parse. Add to 228 line 3. Also have the scan entry keep the snippets of a `findings` result so the record shows what a "false positive" reason claims.

**N2. LOW-MEDIUM, 228 (227 if cheap). PowerShell and cmd spellings the parser does not group.** Not flagged: `@(Remove-Item -Recurse $p)`, `[void](Remove-Item -Recurse $p)`, `$r = @(ri -r $p)` (a bare `(Remove-Item ...)` and `$(...)` are flagged); `Invoke-Expression 'ri -r $p'` and `iex "Remove-Item -Recurse $p"` (the analogue of `eval 'rm -rf $d'`, which is flagged); `Start-Process pwsh -ArgumentList '-c ri -r $p'`; cmd `if exist x (rd /s /q %p%)` and `@rd /s /q %p%`; `pwsh -EncodedCommand` (already 228). Fix: treat `@(`, `[type](` and cmd `(`...`)` as groups, and add `invoke-expression` and `iex` to the eval-style runners.

**N3. LOW, 228. The record trusts a `clean` entry, and the sha is taken at scan time.** `clean` with any session string and any count passes `check_record`; the stored `sha256` cannot show an edit made before the scan; `task_force: []` with `command_scans: []` passes. Fix with the CardRun write path (228 line 4): the code writes the entry, and `check_record` compares `sha256` and `commands` with the transcript when it is present.

**N4. LOW, 228. Further forms not named in 228.** `uv run - <<'PY'` and other interpreter wrappers for a heredoc; an unquoted `python <<EOF` with `os.system('rm -rf ' + p)` (os.system is owned); `cmake -E remove_directory $p`; `Start-Process rm -ArgumentList`; `perl -e`, `ruby -e`, `php -r` calling a shell delete. Same family as lines 1 to 6; add to line 6.

### 5. Not re-run

The 22 detector fixture repositories were not rebuilt; I read `scan_pushes`, `push_rule_problems` and the agreement text and ran the two checks on the real repositories (both `[]`). The mutation runs in the record (C3, B3, B9, D4, M9, A2) are the QA seats' and were not repeated.
