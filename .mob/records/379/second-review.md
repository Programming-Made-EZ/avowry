# Card 379 second review (read-only, fresh context): every seat launcher refuses recursive deletes; one reviewed cleanup helper

Reviewed: mob-defaults main be8fff6 (diff 63fb99f..be8fff6) against the architect slice. Nothing in either checkout was edited. The
three card test files pass from a scratch cwd (18 tests, 1 skipped = the live check). All probes ran on strings or on throwaway
directories I created under C:/Users/<user>/safety-scratch/r2 (the helper only against my own temp parent). Paths below say <user>/<home>.

## Verdict: ACCEPT WITH NOTES
The five banned forms and the wrapped forms are refused, the helper held against every path attack I tried, and every launcher with a deny
file fails closed; but the deny globs that actually bind Claude and Cursor seats are much narrower than `refuses()`, the broad hook is
installed nowhere, and Muse, agy and Codex have no enforcement while the card headline says "every seat launcher".

## Findings, ranked

### F1 (high) The enforced layer for Claude/Cursor seats is only the glob list, and it misses ordinary recursive deletes
`refuses()` and the strict hook are broader, but the hook is not installed anywhere (its `--print-snippet` even prints the NON-strict
hook, without `--deny-all`). The seat runs on `permissions.deny` globs. Emulating the Claude globs with case-sensitive fnmatch (the
live Claude matcher was not run on these; the facilitator's live proof covered other inputs), these are NOT denied but ARE recursive deletes:
- `del /f /s /q x`, `del /q /s x`, `del /s/q x` (the glob needs `del /s ` first)
- `rm -Rf x`, `rm -rv x`, `rm -vR x`, `rm -f -R x`, `rm x -rf`, `RM -rf x`, `rm -rF x`
- `Remove-Item -Path x -r`, `ri -r x`, `ri x -recurse`, `Remove-Item x -R -Force`, mixed case `Remove-Item x -recurse`
- `rmdir x /s`, `rd/s/q x`, `rmdir/s/q x`, `CMD /C RD /S /Q x`
- `git clean --force`, `git clean -x -d -f`, `git -c k=v clean -fd`
Fix: widen the globs (`*rm -*[rR]*`, `*/s*` for del/rd/rmdir, `Remove-Item * -r*`, `ri *`) or install the strict hook in the committed
settings. The Cursor strings (`Shell(rm:-rf*)`) have the same shape of gap.

### F2 (high-medium) `refuses()` misses (hook layer), input -> result None
Shell keywords: `for d in a b; do rm -rf $d; done`, `while ...; do rm -rf x; done`, `if true; then rm -rf x; fi`, `! rm -rf x`.
Wrappers: `timeout 5 rm -rf x`, `nice -n 5 rm -rf x`, `stdbuf -o0 rm -rf x`, `builtin rm -rf x`, `eval 'rm -rf x'`,
`%ComSpec% /c rd /s /q x`, `& 'C:\Windows\System32\cmd.exe' /c rd /s /q x`, `cmd /c start /b rd /s /q x`, `find x -exec cmd /c rd /s /q {} \;`.
Quoting: `"rm" -rf x`, `'rm' -rf x`, `r''m -rf x`, `rm -r"f" x`. No-space cmd forms: `rd/s/q x`, `rmdir/s/q x`, `del/s x`.
Flag cluster over 4 letters: `rm -rfdvi x`. rmtree aliases: `from shutil import rmtree as r; r('x')`, `getattr(shutil,'rm'+'tree')`.
PowerShell pipeline: `Get-ChildItem x -Recurse | Remove-Item` (deletes recursively). Documented bypass classes (acceptable): `robocopy empty x /mir`,
`[IO.Directory]::Delete`, `node fs.rmSync`, `iex`, `$a='rm'; & $a -rf x`.
Also not refused (by design or out of scope): `git rm -r x`, `git rm -rf x`, `ssh h rm -rf x`, `docker ... rm -rf`, `rsync --delete`.

### F3 (medium) The reviewed helper is reachable by a seat, and no pin/exemption exists
`refuses("python remove_throwaway.py --allow-parent X X/wt-foo")` is None, so a seat can run it with `--allow-parent` of its choosing and
remove any clean `gate*-clone`/`wt-*` clone or linked worktree it can name. The architect's risk text promised a sha256 pin and
exemption in the hook: grep finds neither (no `sha256`, `pinned`, `exempt`), and the brief templates were not changed ("seats only
REPORT") although the slice listed that. "Facilitator only" is a convention, not enforced.
Related: the helper runs `git status` inside the seat-controlled repo; a local `core.fsmonitor` in that clone's config executed (a marker
file was written during `remove_throwaway.py`), so repo config runs code as the facilitator. Low in practice, easy to avoid with
`git -c core.fsmonitor=false -c core.hooksPath=NUL`.

### F4 (medium) Helper "main checkout" rule and ignored content
Plain clone with a remote and no linked worktrees whose leaf matches is removed: `wt-plain` (my plain clone) -> exit 0. Nothing can tell a
main checkout from a throwaway plain clone; the discriminators are only: leaf pattern, allow-listed parent, a remote exists, no linked
worktrees, clean. Refused correctly: main checkout with linked worktrees; repo with no remote.
`git status --porcelain` ignores ignored files, so a "clean" clone can hold a nested full repository (ignored via info/exclude): it was
deleted silently (exit 0). A junction inside the clone pointing at a protected directory was NOT followed (sentinel survived).
A clone that contains the home or a main checkout is not checked for.

### F5 (medium) Wiring gaps and overclaim
- `launcher_problems` is called by nothing at launch (only `safety_policy.py check` and tests). Removing `--deny`/`--deny-tool` from
  backends.json launches facilitator and decider seats unchanged; only the L-1 test notices. The five launchers call `deny_file_problems`.
- `cursor_reply.py` without `--worktree` (what the `cursor` entry in backends.json does) started in a directory with no deny files:
  `AGENT-STARTED`, exit 0. Ask mode is read-only, which is the mitigation.
- Muse, agy and Codex (`codex_nostore.py exec`, powershell -Command) have no deny at all; `muse_write`'s file check verifies files Muse is not
  known to read (the muse_write comment says so, but it still reads as a guard). `codex_rules()` is rendered and installed nowhere. The only
  gap statements are a docstring line and a muse_write comment; backends.json `_comment_deny` names claude, copilot, grok only. The card
  headline "every seat launcher refuses recursive-delete commands" cannot be backed for these three; no file lists them as uncovered.
- Copilot covers rm/rmdir/rd/del/erase/Remove-Item/ri/git clean but not python rmtree or find -delete.

### F6 (low) False positives
- `Bash(*rmtree*)` (Claude, Grok) denies `grep -n rmtree remove_throwaway.py`, so a reviewer seat cannot grep this very code.
- `refuses("echo 'shutil.rmtree is banned'")` -> rmtree; `refuses('git commit -m "x; rm -rf y"')` -> rm -rf (quotes are split).
- Copilot denies `rm file.txt`, `del`, `Remove-Item file.txt`; Claude globs deny `git rm -r --cached` (accepted in the slice).

### F7 (low) Unverified syntax and a hardcoded path
- Grok README: `*` is a "single-level wildcard", `**` recursive. Whether `Bash(rm -rf *)` matches `rm -rf C:/a/b` (slashes) is not proved;
  if `*` stops at `/`, absolute-path deletes escape the Grok deny. Needs one live check with `--deny` on a nonexistent target.
- Cursor: bundle confirms the project `.cursor/cli.json` is read from the git root down to the cwd and the `permissions.deny` `Shell(...)`
  shape; the `cmd:args` matching of `Shell(rm:-rf*)` is not proved.
- Copilot: `copilot help permissions` confirms `shell(cmd)` and first-level `shell(git clean)`, and that deny beats `--allow-all-tools`;
  the flags parse (`--version` exit 0 through the .CMD shim, parentheses survive). `--allow-all-tools -s` stays last and intact.
- backends.json commits an absolute `<home>/mob-crosscli/.claude/settings.json` path (precedent: the cursor and qwen entries); the
  `launcher_problems` check accepts any path ending `/.claude/settings.json`, not the repository's own file. A relative path is flagged.

## Table: refuses() (strings only; full lists in sections above)
Refused (correct): rm -fr; rm -Rf; rm --recursive --force; rm -r -f; `\rm -rf`; command/env/sudo/`/bin/rm`/xargs/time/wsl rm -rf; rmdir /S /Q;
RD /S; del /f /s /q; Remove-Item -Force -Recurse; remove-item -r; ri -r; `cmd /c rd /s /q`; `powershell -Command "Remove-Item -Recurse x"`;
`bash -c 'rm -rf x'`; python shutil.rmtree; git clean -fdx; find . -delete; `echo a; rm -rf x`; `$(rm -rf x)`; find -exec rm -rf.
Not refused (misses): see F2; robocopy /mir; flags split by quotes (`r''m`, `"rm" -rf`).
Not refused (correct): git rm -r --cached; echo "rm -rf"; grep rm -rf file; git worktree remove p; rm file.txt; Remove-Item file.txt; git clean -n;
git clean -nfd; rm -f x; ls -R; Remove-Item x -Force.
Refused (false positive): echo 'shutil.rmtree is banned'; git commit -m "x; rm -rf y".

## Hook strict mode (`--deny-all`), exit codes
Claude Bash/PowerShell payloads 2; Cursor `{"command"}` 2; agy `CommandLine` 2; not JSON 2; JSON list 2; Write tool (no command) 0; benign 0;
each refusal logs one JSON line. Without `--deny-all`, a Cursor-shaped payload exits 0 (the old hook only reads `tool_input`).
Fail-open: any shape with no recognised command key (`{}`, `{"shellToolCall":{"args":{"command":...}}}`) exits 0 in strict mode.
`--log` as the last argument silently falls back to the default log.

## Helper attempts (exit code; only my temp dirs)
1 refused: `..` in path (twice, including out of the parent); relative path; no --allow-parent; leaf not matching (`gate1-clone-old`, the parent
dir, `realclone`); nonexistent `gate1-clone.bak`; junction named gatej-clone; path via junctioned parent; drive root as target and as parent;
home as target (leaf) and as parent; wildcard; `~`; `%TEMP%`; target equal to the parent; cwd equal to target; cwd inside target; dirty clone;
dirty linked worktree; main checkout with linked worktrees; repo without remote; non-git dir; subdirectory of a clone.
0 removed (intended or hole): clean clone with trailing slash; same clone with different case (`GATE4-CLONE`); clean linked worktree through
`git worktree remove`; plain clone named wt-plain (F4); clone with an ignored nested repo and an ignored junction (F4; junction target survived);
clone with a fsmonitor config (F3); control clone with cwd outside.

## NOT A PROBLEM
- Path attacks: `..`, symlink/junction leaf and mid-path, lookalike leaves, drive root, home, cwd, parent of cwd, trailing slash, case on Windows, dirty.
- A junction inside a clone is not followed by the removal.
- Missing deny file: cursor_write, grok_write, muse_write, cursor_reply --worktree, local_driver refuse with exit 2 and name the file;
  a tampered or partial file is refused by the equality check. Claude with a missing absolute --settings file fails (Settings file not found).
- Committed files in phrase-lang equal the mob-defaults copies and the render; `safety_policy.py check` exits 0 for both roots.
- Grok `--deny` and Copilot `--deny-tool` are real flags with documented deny-wins semantics; git worktree remove, git worktree remove --force,
  rm file.txt, Remove-Item file.txt are not blocked by `refuses()`.
- Older avowry worktrees (wt-162-* ...) lack the files, so the five launchers refuse to seat in them (fail closed).
