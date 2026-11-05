# Card 297 sign-off (sign-off, ctx-6, fresh context; re-run after the machine restart killed the first attempt at 06:36 UTC)

Seat: Claude (a fresh subagent, read-only; reply recorded by the facilitator). Environment: Windows 11 Pro 10.0.26300, Python 3.15.1, mob-crosscli HEAD a092d9f (working tree clean except the old untracked .mob/). Ran: tests.test_driver_command_probe, test_command_seat_gate, test_command_driver_wiring, test_writer_isolation, test_seat_gate: 58 tests OK (about 60 s). It edited nothing.

## Verdict: SIGNED OFF with conditions

| Check | Result | Evidence |
|---|---|---|
| 1. Blocked shell fails the command probe, naming shell path, sandbox mode and 1920 | Met | facilitator.py:799-815 (command_capability_probe fails closed unless the nonce returns); tests/fakes/fake_codex_blocked_shell.py:15-24; tests/test_driver_command_probe.py:56-65 |
| 2. Working shell passes with path and mode recorded; a Codex driver is placed only on a fresh command probe under the invocation's mode | Met for probe and placement; the lock and commit-on-main half is inherited from card 230's tests | facilitator.py:813, 829-840, 1338, 1348-1368, 1744, 1770; tests/test_driver_command_probe.py:77-84; tests/test_command_seat_gate.py; lock half: tests/test_writer_isolation.py:195-216, 268-285 (header of test_command_seat_gate.py:12-14 says it is out of scope) |
| 3. Voting and read-only seats are still placed when only the command probe fails | Met | tests/test_command_driver_wiring.py:64-85 and 100-110 (rotation guard); facilitator.py:826-840, 1353; decider.py:165, 235; core/backends.json:35 |

Condition A (same sandbox mode for probe and seat, no bypass flag): met (facilitator.py:456; core/backends.json:25; tests/test_driver_command_probe.py:87-116; git grep of a092d9f finds no bypass flag outside old scratch copies). Condition B (probe state persisted with path, mode, evidence; stale, absent, failed and mismatched records refuse a driver): met (facilitator.py:833-840, 1351-1368; tests/test_command_seat_gate.py). Cycles: criterion 1 one, criterion 2 two, criterion 3 three (a092d9f is a test-only guard; no pending red).

## Conditions for the closing record

1. Check 2 is proven by composition: no test runs a Codex-shaped driver through probe, placement, command and commit together; all tests use fakes.
2. The machine fix is not applied. Codex build seats stay refused on this machine (with path, mode and 1920) until the product owner approves a non-Store PowerShell or Codex set to use cmd.
3. shell_path "pwsh.exe" in core/backends.json:32 is a placeholder; the probe reports it verbatim and does not resolve it.
4. Behaviour change: Codex build seats used to run with the bypass flag and now run in workspace-write, so card-mode Codex drivers may be refused. Intended by the card.
5. The legacy turn() path (facilitator.py around 1424) calls gate_seat() with no command_required, so a legacy driver turn on Codex is not refused on a blocked machine. Record as a limit or file a follow-up bug.
6. The probe nonce also appears in the prompt text (facilitator.py:803-806), so a model that quotes the command in an error could pass a blocked shell. Limit; a derived value would be stronger.
7. The command probe adds one model call per probe for command-capable backends, with the same 60-minute freshness.
8. tests/test_launch_long_prompt.py changed one expectation (the Codex argv carries -s workspace-write); intended.
9. The card's "setup notes name the one-time fix" is not implemented in 1732a82..a092d9f; state it as deferred or file it. Card text: any older "own worktree" wording in Done check 2 is amended to "commits on main in the one checkout under the single-writer lock" (the live card text already says so).
10. The pre-ship gate's errors are the two the facilitator already noted; the sign-off found no more.
