# Card 297 retro input (facilitator; facts only)

Card: a Codex seat on Windows runs shell commands in its sandbox, and the probe proves it (bug); code in mob-defaults 63180c2..a092d9f (11 commits); signed off with conditions (.mob/records/297/signoff-ctx6.md); gate SHIP (.mob/records/297/preship-gate.md) with two facilitator corrections.

## Flow metrics (UTC, from commits)
- Ready 04:14, task-force window 04:18 to 04:28 (six NO OBJECTION; the coach re-asked once), slice 04:34 to 04:38 (Codex, read-only), slice window 04:38 to 04:58 (two conditions adopted, full re-poll, six NO OBJECTION), then red and green in three criteria: criterion 1 one cycle (05:02 to 05:06), criterion 2 two cycles (05:09 to 05:19), criterion 3 three cycles (05:23 to 05:39; cycle 3 was a test-only fix of the cycle 2 red's rotation test, which passed at once).
- Caps: none raised; no lettered red; no green stop. Seat windows were 3 to 6 minutes each for about 500 lines of tests and code in 8 files.
- Pre-ship gate (Copilot, read-only) 06:02 to 06:07: SHIP. Sign-off first attempt started 06:36 and was killed by a laptop reboot; the machine was back about 07:39; a fresh probe, a fresh in-flight line and a fresh sign-off seat ran 07:44 to 07:58: SIGNED OFF with conditions. Card start to sign-off about 3 hours 44 minutes, of which about 1 hour was the reboot.
- Full suite on a092d9f: 1854 tests OK (skipped 1) in the first sign-off attempt; a re-run in the resumed session was started and had not finished when this input was written.

## Adjustments to review first (D155)
1. D187 (targeted re-ask, trial; card 230 the baseline): the task-force window had one seat's condition and re-asked only the coach; the slice window had two conditions and ran a full re-poll as the rule says. Both adopted.
2. Trunk-based writing (all writes serial on main in the one checkout, no worktrees or branches): every red and green of this card committed directly on main in mob-crosscli; no worktree, branch or copy was made.
3. F-085 (re-probe at 50 minutes before every seat): the in-flight lines for the seats after 05:17 UTC record last-probe ages of 66, 70, 72, 77, 80, 86, 112 and 145 minutes (probe of 04:10 UTC). The red and green seats ran on a probe past the 50-minute rule for the whole second half of the card, and the gate and sign-off seats ran on one past 60 minutes. A fresh probe was taken only after the reboot (07:41).
4. Experiments queue (docs/retros/experiments.md): none newly adopted on this card.

## Observations
- The pre-ship gate reported a wrong HEAD (1732a82; the checkout was at a092d9f) and called the final guard red "no green yet"; the facilitator corrected both in the report and the sign-off confirmed no further errors.
- The sign-off found the following not covered by the card's tests: no test runs a Codex driver through probe, placement, command and commit together; the legacy non-card turn path is not gated (a legacy Codex turn on a blocked machine now starts in workspace-write and cannot run commands, where before it ran with the bypass flag); the probe's nonce also appears in the prompt text, so a model that quotes the command in an error could pass a blocked shell; the "setup notes name the one-time fix" part of the card is not implemented; shell_path in core/backends.json is the placeholder pwsh.exe.
- The machine fix (a non-Store PowerShell, or Codex set to use cmd) is a machine change that needs the product owner's approval. Until then Codex build seats stay refused on this machine, with the shell path, mode and error 1920 named.
- The reboot killed the sign-off seat mid-turn; no work was lost because the seat is read-only and the tree was clean. A background Monitor armed on a shared output file read an earlier command's output and reported a false completion; a later watchdog caught it.
- Open follow-ups to file as bug cards at closure: the legacy turn path is not command-gated; the command probe's nonce is copyable from the prompt.

## Standing section: the feedback-loop lens (D165)
Each voice names, for the 7 loops, who or what gives the feedback, its latency, what the team does with it, and MISSING where none exists.
