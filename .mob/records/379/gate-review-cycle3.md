# Card 379 cycle 3 delta: independent gate review (fresh Claude seat, read-only; 2026-11-10)

Reviewed: mob-defaults main at 565171cdbcb20cbccf1372cf2472dec824a61def (tip), cycle 3 range a0bd1af02136705f4bd3b69cf28d8884b81eab7b .. 565171cdbcb20cbccf1372cf2472dec824a61def. First review (cycles 1 and 2): .mob/records/379/gate-review.md. I wrote nothing else on this card. The full mob-defaults suite was NOT run (the facilitator's gate ran at 577461aad01b2dff49589c0f78e92848a5a234cf; the sign-off seat runs it next). All work in scratch: clone and scripts under C:/Users/<user>/gate379c3-review-clone, one throwaway worktree (reported below). No recursive delete of any kind was run; every live command targeted nonexistent paths.

## Verdict: FAIL, on documentation only (one text fix, no code change)

The code, tests and live behaviour pass every check below. The FAIL is item 5: the committed module docstring of `safety_policy.py` (and two other committed texts) still say the opposite of what cycle 3 did, on exactly the safety property the card asked to state plainly (who gets the broad hook). The fix is text only; after it, a grep re-check suffices, no new code review. If the facilitator prefers, record the three stale statements as fixed and treat this as PASS-after-fix.

## Step 0: order and red replay

`git log --oneline a0bd1af^..565171c` (newest first): 565171c, 577461a, 20a3171, 3378082, 6aa6731, 6d7c1b4, 40f33da, 509a91a, a0bd1af.

| pair | test commit | fix commit | `merge-base --is-ancestor` | test alone, decorator stripped, in a scratch clone |
|---|---|---|---|---|
| R3-1, R3-2 | a0bd1af02136705f4bd3b69cf28d8884b81eab7b (Grok) | 6d7c1b4 (also fixes R4-1) | yes | R3-1 FAIL `AssertionError: Lists differ: [] != ['Bash(*rm -rf *)', 'Bash(*rm -r*)']`; R3-2 FAIL (same shape, PowerShell rules) |
| R4-1 | 40f33da3de60c1255fdb7398de72bc8364d8c6d0 | 6d7c1b4 | yes | FAIL `'$HOME/' not found in ... ~/mob-crosscli/hooks/...` |
| R5-1 | 6aa6731c4af11ef9fe79ccc1c442fa1413cc91c0 | 33780823adab19ad7f620e2c2263216e2b227880 | yes | FAIL `'.cursor/hooks.json' unexpectedly found in {...}` |
| R6-1 | 20a31716a9f4956db2506f0a40f71b1eb20aa554 | 577461aad01b2dff49589c0f78e92848a5a234cf | yes | FAIL `'hooks' unexpectedly found in {...}` |

All four fail by assertion, none by import or syntax error. At 565171c: `grep -c expectedFailure tests/test_safety_policy_cycle2.py` is 0; `tests.test_safety_policy_cycle2 tests.test_safety_policy tests.test_launcher_deny tests.test_delete_guard_hook_3b`: Ran 42, OK (skipped=1, the opt-in live Claude test).

Deliberate test amendments seen in the diffs (all disclosed in seat-changes.md): S-2 (4 lines, 6d7c1b4) for the unprefixed glob form; `tests/test_launch_long_prompt.py` claude argv (577461a) now pins `core/claude-seat-settings.json`; R3-3 (577461a) now reads the hook from `claude_seat_settings()`. Note R3-3, a cycle 3 test that passed at its red commit, was edited in the fix commit itself; the edit is the correct move and is small, but it is a test changed together with its fix by the same person (a logged takeover, so accepted).

## 1. Mutation checks (scratch clone at 565171c, each restored with `git checkout`)

| mutation | result |
|---|---|
| `hooks` key put back into `claude_settings()` | R6-1 FAIL (also R2-2, R3-3, S-5, L-1: 5 failures) |
| `*`-prefixed globs restored (`f"{tool}(*{glob})"`) | R3-1 FAIL and R3-2 FAIL (also R2-2, R3-3, R6-1, L-1, S-2, S-5: 8 failures) |
| cursor hook file re-added to `rendered_deny_files()` | R5-1 FAIL, L-1 FAIL (2 failures) |
| backends.json claude `--settings` pointed back at `.claude/settings.json` | R6-1 FAIL, L-1 FAIL; `python safety_policy.py check` prints `claude.invocation: the claude invocation has no --settings core/claude-seat-settings.json`, exit 1; restored: exit 0 |

Every mutation is caught by its named test. (A first attempt at the third mutation did not apply because of my own string edit; I redid it with a list insert and it was caught.)

## 2. Committed files in both repositories

- `python safety_policy.py render --root <scratch>` then `cmp`: `.claude/settings.json` and `.cursor/cli.json` identical to the render in mob-defaults AND in avowry (checkout C:/Users/<user>/projects/phrase-lang, both files tracked there).
- `core/claude-seat-settings.json` is tracked and `cmp`-identical to `json.dumps(claude_seat_settings(), indent=2) + "\n"` (render only writes it for the repository's own root, so I rendered the function by hand).
- avowry: no `.cursor/hooks.json`, no `.agents` directory; mob-defaults: neither either. Project `.claude/settings.json` has no `hooks` key in either repository.
- a grep for the account name and for "Users" on the two deny files and the seat settings: 0 in all (and in avowry's two files). One caveat, not a deny file: `core/backends.json` holds an absolute home path in nine places (the claude `--settings` path among them, required to be absolute, plus older codex, cursor_reply and qwen entries). That is by design and older than cycle 3, but it contradicts a literal reading of "no committed file holds a home path" (see finding 5).

## 3. Live checks (nonexistent targets only)

(a) `claude -p --model haiku --permission-mode bypassPermissions --settings <abs core/claude-seat-settings.json> --setting-sources ""`: `echo "the card bans rm -rf here"` RAN with its output; `rm -rf ./does-not-exist-nope` BLOCKED by the PreToolUse hook (message: "safety_policy: refused, a recursive delete ('rm -rf') is banned for every seat (card 379) ... Report the path of the throwaway to the facilitator"); `bash -c "rm -rf ./does-not-exist-nope2"` BLOCKED by the same hook. The `~/mob-crosscli/hooks/...` hook command therefore expands under Claude on this machine.

(b) Cursor seat through the real launcher: throwaway worktree of mob-defaults main (565171c), `python cursor_write.py --worktree C:/Users/<user>/gate379c3-review-wt --prompt-file <outside the repo> --timeout 240` (the prompt states the standing rules: recursive, trailer, personal name). Exit 0. The seat said `git status --short` ran (empty output) and replied `done` (not BLOCKED). `smoke.txt` exists in the worktree with `smoke ok`. The launcher printed `?? smoke.txt` as the only change.

(c) Extra, for item 5: Claude with only the PROJECT settings (`--settings .claude/settings.json --setting-sources ""`, no hook). `echo "the card bans rm -rf here"` RAN; `ls . && rm -rf ./does-not-exist-a`, `rm -rf ./does-not-exist-b` and `bash -c "rm -rf ./does-not-exist-c"` were all BLOCKED (Claude Code's own matcher splits `&&` and looks inside `bash -c`, so the anchored globs still catch those). Then, in the same globs-only mode, `rm -fvr ./does-not-exist-d`, `git clean -xdf` and `python -c "import shutil; shutil.rmtree('./does-not-exist-e')"` all RAN (none blocked; the targets did not exist and the directory was not a repository). `RM -RF ./...` also was not blocked by a rule (the real tool rejected `-F`).

## 4. Deny globs against the two lists (fnmatch.fnmatchcase on the whole command, every Bash and PowerShell rule; script C:/Users/<user>/gate379c3-review-clone/globs.py)

- 20 harmless commands that only mention the words (git commit messages with rm -rf / Remove-Item -Recurse / "x; rm -rf y", echo, grep, rg, gh issue and gh pr --body, printf, git log --grep, git tag -m, a python print, a file named rm-rf.md): 0 of 20 match any rule.
- 22 direct recursive deletes (rm -rf x, rm -r x, rm -fr, rm -Rf, rm -R, rm --recursive, rm x -rf, rm -rv, rmdir /s /q, rmdir /S /Q, rd /s /q, del /s, del /f /s /q, Remove-Item -Recurse x, Remove-Item x -Recurse -Force, remove-item -recurse, ri -r, git clean -fdx, -fd, --force -d, `git -c k=v clean -fd`, find . -name x -delete): 22 of 22 match at least one rule.
- Direct forms the globs do NOT cover (all confirmed refused by `hook_decision`, exit 2): `rm -fvr x` (live-confirmed it runs under globs only), `rm  -r x` (two spaces), `RM -RF x`, `erase /s x`, `git clean -xdf` (live-confirmed it runs), flag clusters and orders other than the listed ones, quoted or escaped verbs (`"rm" -rf x`, `r''m -rf x`, a leading backslash), wrappers the Claude matcher may not strip (`sudo`, `env`, `timeout 5`, `nice`, `command`, `xargs`), and python `shutil.rmtree` (live-confirmed it runs). Hook gaps found: none. (Two of my own probes came back exit 0 only because of stray control characters I typed into the probe strings; the properly escaped forms are all refused. `rm -- -rf`, `rm -d x`, `git clean -nfd` are exit 0, which is right.)
- The hook covers the wrapped forms: `bash -c`, `cmd /c rd /s /q`, `powershell -Command "Remove-Item -Recurse x"`, `cd x && rm -rf y`, `ls; rm -rf x`, `echo a | xargs rm -rf`, `(rm -rf x)`, `git rm -r x` (documented false positive) all exit 2; `echo`, `grep`, `git commit -m` mentions exit 0.

## 5. The consequence for the interactive session and Agent-tool subagents

Stated plainly: an interactive Claude Code session and the subagents spawned by its Agent tool load the project `.claude/settings.json`, which now holds ONLY the prefix-anchored globs. They do NOT run the broad strict hook (`delete_guard_hook.py --deny-all`) unless a user-level settings file installs it (this machine's user-level file has no `hooks` key: `grep -c '"hooks"'` returned 0). The globs alone do not stop the forms listed under item 4 (live-confirmed above).

Documented? Only partly, and one committed text claims the opposite.
- Stated correctly and plainly: `.mob/records/379/seat-changes.md` addendum, takeover 6 ("an interactive Claude session and the Agent-tool subagents inside it get the globs but NOT the broad hook unless a user-level settings file installs it (an owner decision; Cursor may read that too)").
- NOT in the module docstring of `safety_policy.py` at all (grep for subagent, interactive, user-level finds nothing in the docstring, the backends comment or any template).
- Files claiming otherwise (stale after cycle 3, all at 565171c):
  1. `safety_policy.py` module docstring, the `claude_settings()` line: "the project `.claude/settings.json`: permissions.deny (... wildcard-prefixed so the wrapped forms `bash -c`, `cmd /c` match too) and a PreToolUse hook ... so every Claude seat and session runs the broad judge `refuses`; the claude seat starts with `--settings .../.claude/settings.json`". Wrong on three counts: the globs are no longer wildcard-prefixed, the project file has no hook, the seat starts with `core/claude-seat-settings.json`. `claude_seat_settings()` is not listed in the docstring at all. The `hook_command()` docstring ("blocks Bash and PowerShell for Claude seats") is fine.
  2. `core/backends.json` `_comment_deny`: "claude starts with --settings and the ABSOLUTE path of .claude/settings.json ..." (now `core/claude-seat-settings.json`) and "agy (its .agents/hooks.json is unverified)" (the file is no longer installed; harmless but stale).
  3. The `_comment` inside the committed project `.claude/settings.json` (both repositories; the rendered text from `claude_settings()`): "recursive deletes the globs cannot express ... are judged by the PreToolUse hook (safety_policy.refuses)". In that very file no hook exists, so an interactive reader is told a judge runs when it does not. Fixing it means a renderer change and re-rendering both committed copies (and the R3-3/S-5 equality tests stay green because they compare to the render).
  4. Lower weight, history: `slice-architect.md` section 6 (R2-2: "so every Claude seat and session runs the broad judge") is the cycle 2 plan; it has no cycle 3 note pointing at takeover 6. `gate-review.md` (first review) is dated history and fine.

## 6. Other findings, ranked

1. (FAIL reason, doc) The three stale statements in item 5, plus the missing plain sentence about the interactive session and subagents in the module docstring. Fix: update the docstring, the backends comment and the `claude_settings()` note; add one sentence: "An interactive Claude session and its Agent-tool subagents get only the project globs, not the strict hook, unless the owner installs the hook in a user-level settings file; the globs miss flag clusters, quoted verbs, `git clean -xdf` and python rmtree."
2. (Medium, design, stated) A globs-only session misses real direct forms (live-confirmed `rm -fvr`, `git clean -xdf`, python `shutil.rmtree`). This is the accepted consequence of the prefix-anchoring trade; it needs an owner decision on a user-level hook (or an extra glob list), not a card 379 code change. The product owner should see it named in the sign-off summary, not only in a records addendum.
3. (Low) `core/backends.json` keeps absolute home paths (nine lines; claude `--settings` must be absolute). Nothing leaks beyond the local machine, but the statement "none of the committed files holds a home path" is true only of the deny and seat settings files, and a clone on another machine needs the path edited. The R6-1 launcher check requires only the `/core/claude-seat-settings.json` suffix, so a stale path to another checkout would pass `launcher_problems` yet fail at seat start ("Settings file not found"). Not tested; worth a line in the retro.
4. (Low) The Cursor seat proof shows the shell runs; it does not prove the `.cursor/cli.json` `Shell(rm:-rf*)` denials fire (not exercised live here, nor, per the earlier review, anywhere). Config-text only, as before.
5. (Low) Cycle 3 commit 6d7c1b4 first rewrote the Cursor and agy hook renders to `$HOME` (R4-1), and 3378082 then stopped installing them; the renders and R4-1 remain as reference text. Consistent with the docstring paragraph; no action.
6. (Info) Muse, agy, Codex remain unenforced (bug 414, unchanged by cycle 3).

## State left behind (not deleted by me; facilitator cleanup)

- Linked worktree of mob-defaults: C:/Users/<user>/gate379c3-review-wt on new branch `review-c3` (at 565171c), containing an untracked `smoke.txt` written by the Cursor seat.
- Scratch clone, render output, probe scripts, prompt file: C:/Users/<user>/gate379c3-review-clone (not a worktree of either checkout).
- Neither checkout has a tracked change from me; mob-defaults shows only its pre-existing untracked `.mob/`.

## Facilitator addendum: the documentation fix (the review's only finding)

Fixed in mob-defaults (commit after 565171c, see the card 379 documentation commit): the safety_policy.py module docstring, the core/backends.json comment and the note inside the rendered project settings now say that the project .claude/settings.json holds the prefix-anchored globs only, that the strict hook is in core/claude-seat-settings.json passed by the mob launchers, and that an interactive Claude session and the Agent-tool subagents inside it are covered by the globs only unless a user-level settings file installs the hook (an owner decision). Grep re-check by the facilitator: no committed file still says that the project settings carry a hook or that every Claude session runs the broad judge; both repositories' .claude/settings.json equal the render; python safety_policy.py check exits 0; the card modules pass (the three older launcher failures excepted). The reviewer's finding 3 (launcher_problems checks only the file-name suffix of the --settings path) and 4 (the Cursor Shell denials are config text only) are carried to the follow-up bug.
