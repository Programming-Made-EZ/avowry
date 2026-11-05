# PRE-SHIP GATE: Card 297 (ctx-5 independent reviewer, Copilot auto via the Copilot CLI, read-only, 06:03 to about 06:15 UTC on 2026-11-05)

Facilitator's notes on the report below (facts, added after reading it against git):
- Its Environment line says HEAD=1732a82; the mob-crosscli checkout was at a092d9f when it ran, so that hash is wrong. The narrow run (27 tests, OK) names no commit; the full suite on a092d9f is the evidence for the final state (see the sign-off).
- Section 3 calls red a092d9f a "guard red ... no green yet". It is a guard: the test passes on HEAD (green 2caf7bd), no marker remains, and criterion 3 has no pending test. Cycle counts: criterion 1 one cycle; criterion 2 two cycles (6c10f91 with f951bde, ecd4fb8 with 1a90fa6); criterion 3 three cycles (7b08dee with 668ec09, e3d6a45 with 2caf7bd, a092d9f a test-only fix that passed at once). No lettered red; the slice carries no D175 Cap line. The cycle 3 red fixed a flaw in the cycle 2 red's third test (empty history, a tie).
- The lock and commit-on-main half of check 2 was left out of the red cycles by QA (needs a temp repo and a committing fake seat); the reviewer rules it covered by card 230's tests/test_writer_isolation.py. The sign-off re-checks that.
- The reviewer ran no full suite (by brief); the facilitator's full-suite run on a092d9f is recorded in the sign-off.

---

1) Done checks
- Check 1 Met — blocked shell fails the command probe with the resolved shell path, sandbox mode, and Windows error 1920. Evidence: `facilitator.py:803-815` and `tests/test_driver_command_probe.py:56-65`; the fake Codex shell emits the blocked-shell text and the assertion checks `STORE_ALIAS`, `"workspace-write"`, and `"1920"`.
- Check 2 Met for the probe-and-placement half; the lock/commit-on-main half is covered by existing Card 230 tests, not a gap. Evidence: `tests/test_driver_command_probe.py:77-84` (working shell passes and records path/mode), `tests/test_command_seat_gate.py:78-118` (fresh command probe under the invocation mode admits the driver), and `facilitator.py:1338-1368` (`command_probe_problem` refuses stale/mismatched/no-probe drivers). The explicit gap note is in `tests/test_command_seat_gate.py:1-18`: it says the main-commit single-writer lock is not in this file’s scope; the existing coverage is in `tests/test_writer_isolation.py:1-29` and `tests/test_writer_isolation.py:269-288` for the one-checkout commit-on-main lock path.
- Check 3 Met — driver refusal while read-only/voting seats still place. Evidence: `tests/test_command_driver_wiring.py:66-103` and `facilitator.py:1338-1368`; driver is refused before backend launch with the shell-path/mode/error text, while read-only and navigator seats are still placed.

2) Conditions A and B Met/Not met
- Condition A Met — the command-running invocation uses the same configured sandbox mode as the probe and contains no sandbox-bypass flag. Evidence: `core/backends.json:23-35` (`"invocation": "codex exec {PROMPT} -m {MODEL} -s {SANDBOX_MODE}..."`, no bypass flag), and `tests/test_driver_command_probe.py:87-116` verifies both the real seat launch and the command-capability probe use the same `sandbox_mode` and exclude `--dangerously-bypass-approvals-and-sandbox`.
- Condition B Met — the command-capability result is persisted as probe state with shell path, sandbox mode, and failure evidence, and stale/absent/mismatched records refuse a driver. Evidence: `facilitator.py:833-840` (persisted `command_probe` record) and `facilitator.py:1347-1368` (`command_probe_problem` checks stale, absent, and sandbox-mode mismatch), with `tests/test_command_seat_gate.py:85-118` asserting the guard.

3) Lettered reds
- Criterion 1: red `63180c2` -> green `ebf22e8` (1 cycle).
- Criterion 2: red `6c10f91` -> green `f951bde`; red `ecd4fb8` -> green `1a90fa6` (2 cycles).
- Criterion 3: red `7b08dee` -> green `668ec09`; red `e3d6a45` -> green `2caf7bd`; guard red `a092d9f` remains at HEAD (3rd red phase, no green yet). Repo rule: letter labels (Z/O/M/B/I/E/S) are the PM’s red-phase labels before green; the git log carries the actual red/green sequence, not the letters themselves.

4) Sandbox bypass flag left in `core/backends.json` for Codex entries
- No. `core/backends.json:23-35` shows `codex exec ... -s {SANDBOX_MODE}` and `readonly_invocation` as `-s read-only`; there is no `--dangerously-bypass-approvals-and-sandbox` in the Codex entries.

5) `shell_path` in `core/backends.json` is the placeholder `pwsh.exe`
- Acceptable for this card. The PM ready check explicitly says the real machine fix (non-Store PowerShell or `cmd`) is out of scope and left to product-owner approval; the card is about proving the command probe and placement behavior with explicit config and fake shell fixtures. The value is placeholder config, not a bypass.

6) Narrow test run
- Environment: OS=Microsoft Windows 10.0.26300; Python=3.15.1; command=python -m unittest tests.test_driver_command_probe tests.test_command_seat_gate tests.test_command_driver_wiring tests.test_seat_gate; HEAD=1732a8205b29b880b2d78482c59cbbbc587937a1
- Result: Ran 27 tests in 6.469s — OK.

7) Verdict line
SHIP: all card-297 conditions, done checks, and gate logic are satisfied in the current repo state; the command probe and placement logic are green, A/B conditions are met, no sandbox bypass remains, the single-writer commit-on-main path is covered by existing Card-230 writer-isolation tests, and the narrow validation suite passes.
