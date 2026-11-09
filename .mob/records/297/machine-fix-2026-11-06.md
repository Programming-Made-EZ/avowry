# Card 297: the machine fix is applied (closure of the limit the sign-off left open)

Date: 2026-11-06. Source: the product owner's decision of 2026-11-05 (inbox entry "Codex build seats use cmd"), which settles the machine-fix limit stated in .mob/records/297/signoff-ctx6.md (condition 2) and the card 297 closing record. Rode as the card 297 follow-up, not a new card: no product behaviour changed; the change is configuration, one machine-local launcher and one test pin. Code commit in mob-defaults: be90896 (core/backends.json and tests/test_launch_long_prompt.py), pushed.

## What the decision could and could not do

- "Configure Codex to use cmd" cannot be done on this Codex (codex-cli 0.149.1): `codex exec --strict-config -c <key>=...` rejects `shell`, `default_shell`, `windows.shell` and `terminal.shell` as unknown fields; the SHELL variable is ignored (a run with SHELL set to cmd.exe still used powershell.exe); with every PowerShell folder removed from PATH Codex still used C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe by absolute path. Codex looks up `pwsh` on PATH first (here the Microsoft Store alias, error 1920), then uses Windows PowerShell 5.1; it never uses cmd.exe as its first shell.
- What was done instead, which meets the decision's purpose (a Codex build seat that runs commands, with no Store alias): a machine-local launcher, C:\Users\<user>\bin\codex_nostore.py (source below), that starts Codex the way facilitator.py does for the npm shim (node plus the package script, stdin closed) with the WindowsApps folder removed from the PATH Codex sees, so Codex uses the real Windows PowerShell 5.1.
- A second blocker was found and removed by the same launcher: in workspace-write Codex grants its sandbox group a write ACE over the TEMP folder it sees. This PC's %LOCALAPPDATA%\Temp has 294,905 top-level entries, and every launch stalled for over a minute on that ACL work (the sandbox log, C:\Users\<user>\.codex\.sandbox\sandbox.2026-11-06.log, shows `granting write ACE to ...\Temp` and then nothing; the 143 exit seen at the end was the test's own timeout). The launcher gives Codex a small TEMP and TMP (C:\Users\<user>\.mob-codex-tmp).

## Evidence (all on 2026-11-06, Windows 11 Pro 10.0.26300, codex-cli 0.149.1, Python 3.15.1)

- Before: a read-only run's first command was `"C:\Users\<user>\AppData\Local\Microsoft\WindowsApps\pwsh.exe" -Command 'echo hello_from_shell'`, rejected (CreateProcess ... Rejected); the model then retried on its own. A workspace-write run with the Store alias hidden but the big Temp stalled: no command completed within 200 s.
- After: the real card 297 command probe, `facilitator.probe_backend("codex", <core/backends.json codex entry>, ...)`: ok true, command_probe ok true, `OK: shell started; shell_path=C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe sandbox_mode=workspace-write`, 27 s.
- A Codex seat in workspace-write through the launcher (a scratch git repository under C:\Users\<user>\.mob-codex-tmp, 16 to 51 s per run): `echo ... > out.txt` exit 0 (the file exists); `git log --oneline -2` exit 0.
- Tests after the change: 47 OK (test_driver_command_probe, test_command_seat_gate, test_command_driver_wiring, test_launch_long_prompt, test_seat_gate) and 223 OK (test_decider_launched, test_decider_pin_scrub, test_decider_runner, test_card_mode, test_command_scan_wiring).

## The limit that remains (new finding)

Codex keeps `.git` read-only inside workspace-write: `git add` and `git commit` fail with `Unable to create '.git/index.lock': Permission denied`, and `icacls .git` shows a DENY entry for the sandbox principal (`(DENY)(W,D,Rc,DC)`). `--add-dir <repo>/.git` and `-c sandbox_workspace_write.writable_roots=["<repo>/.git"]` (the key is accepted) do not lift it. So a Codex build seat can now write files and run commands (tests, builds), but cannot commit; card 297's Done check 2 wording ("commits on main in the one checkout") is not met for a real Codex green. The facilitator did not work around this (the only ways are the sandbox bypass flag that card 297 removed, or another writer committing the Codex seat's paths). Options for the product owner: (a) Codex takes build seats and the facilitator or a Claude seat commits its explicit paths after each turn; (b) Codex stays on read-only seats and voting; (c) re-allow the bypass flag for Codex build seats (reverses a card 297 invariant). Until decided, Codex is not assigned a build seat.

## The launcher (C:\Users\<user>\bin\codex_nostore.py, machine-local, reproduced here)

```python
NODE = r"C:\Users\<user>\nodejs20\node.exe"
ENTRY = r"C:\Users\<user>\nodejs20\node_modules\@openai\codex\bin\codex.js"
SEAT_TEMP = r"C:\Users\<user>\.mob-codex-tmp"

def without_store_alias(path):
    kept = [p for p in path.split(os.pathsep) if p and "\\microsoft\\windowsapps" not in p.lower().replace("/", "\\")]
    return os.pathsep.join(kept)

# main: makedirs(SEAT_TEMP); env = copy of os.environ with PATH = without_store_alias(PATH) and TEMP = TMP = SEAT_TEMP;
# sys.exit(subprocess.call([NODE, ENTRY, *sys.argv[1:]], env=env, stdin=subprocess.DEVNULL))
```

The full file with its explanation is the one on disk. core/backends.json now starts Codex with `python C:/Users/<user>/bin/codex_nostore.py exec {PROMPT} -m {MODEL} -s {SANDBOX_MODE} --skip-git-repo-check` (and the read-only form), and names the shell actually used in shell_path.

## Also on the product owner's list: the card-230 scratch worktrees

Nothing to remove. No card-230 worktree or branch exists in mob-crosscli (`git worktree list` shows the main checkout and two card-290 scratch worktrees; `git branch --list '*230*'` is empty; no stashes). The two that remain, wt-290-pre-green (HEAD 7e0f200, 1 modified file) and wt-290-probe (HEAD 2d4d24d, 5 modified files), have uncommitted changes, so under the rule "keep any that do and report them" they are kept; they are also card 290's, outside the approval for card 230's. Their HEAD commits are on origin/main. Card 290's records hold diffs of its stopped attempts (.mob/records/290/green3-attempt-1.diff and slice-probe-stopped.diff); whether those equal the uncommitted changes in these two working trees was not checked. No worktree was touched.
