# Empty Grok and Copilot decision votes: root cause (2026-11-03, about 01:45 UTC)

Diagnosed by the facilitator from Dispatch's hint (GitHub issue 270) and the product owner's coach-don't-bench principle (card 277). This covers cards 229 and 270, and the diagnosis half of card 277.

## Finding

Both Grok and Copilot ship on this machine as Windows batch shims: C:\Users\ringe\nodejs20\grok.CMD and copilot.CMD (facilitator.resolve_executable). Launching a .CMD goes through cmd.exe, whose command line is limited to 8191 characters.

facilitator._launch_backend collapses the prompt to one line and passes it as one argument. Every decision prompt in window 276-taskforce round 2 was 9304 to 9408 characters after collapsing, and the full command line was 9412. cmd.exe refuses before the CLI starts: rc 1, stdout 0 bytes, stderr "The command line is too long." (30 bytes).

decider.vote_seat then sets `reply = text if ok else ""` (decider.py:119), which discards that stderr text, and records "no response". So the failure looked like a silent model.

## Reproduction (diagnostic, not counted as a vote)

The exact decision-seat argv was built as facilitator does (readonly_invocation from backends-pinned-sonnet-scanned.json, the round-2 QA prompt w276tf2r2-qa.txt) and run with subprocess in a scratch folder:
- grok: rc 1, stdout 0, stderr "The command line is too long."
- copilot: rc 1, stdout 0, stderr "The command line is too long."

The probe prompt is one short line, so the probe parses and the gate keeps placing both models.

## Why other seats work

- Voices, ready, slice, review and gate seats on Copilot are launched by the facilitator from Git Bash (`copilot -p "$P"` resolves to the sh shim, not cmd.exe), so the length limit does not apply.
- Claude and agy are not .CMD shims, which is why only they ever voted.

## Not the cause

Grok's full-screen TUI in the card 276 slice seat (slice-276-tui.err, about 1 MB of control codes) was a separate facilitator launch error: the -p single-turn flag was missing. The decision path already uses `grok -p`.

## Fix directions (for the team at ready, not decided here)

1. Deliver long prompts without the command line: a prompt file or stdin where the CLI supports one, or launch the CLI's real entry point (node plus the JS file) instead of the .CMD shim.
2. Stop discarding failure text: keep the failure_excerpt in the vote record (reason "launch failed: ...", not "no response"), so the D178 diagnosis (card 277) sees it.
3. A test: a decision prompt over 8191 characters reaches a .CMD-shim backend whole (card 229's title).
