# SCOPE PROPOSAL: window 276-scope-3

Intent: Finish D179's voting-seat metric so a Muse decision seat, already probed and cited, also scans clean. Source: board 276 Done check 3 "returns a cited vote and scans clean"; D179 "voting seats first"; gate HOLD 07:16 UTC (preship-gate.md).

Assumes: Muse tool calls are envelope rows with payload_type "tool_batch.effect.started" and payload.record.tool_name, including nested retained_frame children[].record_json and subagent/<id>/session.jsonl; a log with those tools and no shell command is scanned (clean, 0 commands), never no-source. Probe: metric_attempt_2 session e338c0ee-ba55-4c61-9106-2b2546c9f686 (evidence.json:3-4,12-13,18,22); `_read_muse_envelope` raises when payload.command is absent (command_source.py:458-466). Shell-command field path is unprobed until the QA capture below.

## Choice: (a) C3 cycle 4

Cycle 3 of 3 is spent. The gate named (a) fix the reader, or (b) a product-owner waiver that accepts no-source and redefines Done check 3. The team owns (a). (b) is refused here: Done check 3 requires scans clean; waiving would ship a reader blind to the live log, so every later Muse seat stays no-source, which is D175/D168 meeting the cap by sending the risk later.

Keep the metric on card 276. Green 2f34856 already dropped `--model`; the live seat voted with a cite. Remaining work is the envelope reader plus one QA capture of a real shell-command row.

Raise C3 from Cap 3 to Cap 4. Adopt cycle 4 of 4 before its red. Caps 1+1+4 = 6.

Why the total may exceed 3 (and the prior 5): slice 1+1+1 before sign-in; window 276-scope added cycle 2 after sign-in; window 276-scope-2 added cycle 3 after `--model default` refusal. Cycle 3's stop did not fire (the seat answered). Red 3b assumed payload.command; the live log uses tool_batch.effect.started. One extra cycle finishes Done check 3 here. D136 stays at three criteria. Cut: (b); catalog-id pinning; build seats; items (1) and (6) stay on follow-up cards.

## Cycles and hours used

C1: 1 of 1 (5ef4b8f / 970118c, 2196c58, 41ecf78). C2: 1 of 1 (dbe2e1c, d3a9787, c71c05f). C3: 3 of 3 (ac5a5a2 / fd09aaf, 40d0d08, 5157e3f; 244678b / c0354cb, cefaf4e; 228d2df, 2f34856). Named fix e3608ae.

Hours: first card commit 5ef4b8f about 02:18 UTC to gate HOLD 07:16 UTC, about 5 h of committed work; task-force from 00:32 UTC, about 7 h of card clock.

A stop now leaves: muse enabled with a cited vote; scan no-source on the real shape; Done check 3 unmet; later Muse seats fail-safe blind.

## Facts the red needs

QA seat, one read-only capture, before or as the red: `muse exec --session-id <new uuid> --prompt-file <prompt>` in the records repo (a git working tree). Prompt: run `git status` and print its first line; create, edit or delete nothing. Commit the session.jsonl tool_batch.effect.started row that holds the shell tool_name and the command-string field path under `.mob/records/276/` (or a tests fixture copied from it). That path is the red's pin. Vote-log rows (e338c0ee sequences 64, 71, 78: tool_name read_file) pin other_tools. Subagent logs live at `<session>/subagent/<id>/session.jsonl`. No `muse login`. No second live vote seat this cycle; re-scan the committed e338c0ee evidence after the reader lands.

## Smallest scope

C3 cycle 4 (Done check 3 remainder: reader real shape).
Acceptance: `_read_muse_envelope` walks tool_batch.effect.started (top-level, nested record_json, and subagent/<id>/session.jsonl); read-only tools (tool_name read_file and any tool with no string command) count as other_tools; shell commands (string command on the QA-captured tool_name) are extracted in file order and judged by scan_command; a located parsable log with tool records and no shell command is status clean, commands 0, other_tools >= 1, never no-source; missing, unreadable, empty, unparsable, unresolved-root stay no-source. Re-scan of the committed Muse vote evidence is clean. Trust-ladder Rung 1. No seat runs `muse login`.
Cap: criterion 3, at most 4 cycles (this window adopts cycle 4 of 4).
Stop: the QA `muse exec` capture produces no tool_batch.effect.started shell-command record, or a new CLI/log shape this cycle cannot name, ends the cycle; Done check 3 unmet, reader-shape to a follow-up. Quota or unavailability still excludes (card 277). No catalog-id hunt; no build-seat routing; no waiver.

ZOMBIES: Z payload.command-only reader still no-source on e338c0ee. O vote log scans clean, commands 0, other_tools >= 1. M several tool_batch.effect.started; a repeat counted twice; subagent logs included. B missing/empty/unparsable still no-source; a captured shell command that is a delete is findings. I scan_entry source=muse walks the session folder and its subagent/*/session.jsonl. E nested retained_frame record_json; unknown tool_name => other_tools. S claude/copilot readers and role fallback_chain unchanged.

Inputs: disabled: n/a: muse enabled (cefaf4e). unknown pin: probe_model missing => unknown-pin. SEEN: C2 test_s. repeated: two muse keys. INSURANCE (JSON keys unique). reordered: models map order leaves the pin unchanged. SEEN: C2 reordered test. unknown envelope tool_name: other_tools. INSURANCE until QA capture for the shell field; SEEN for read_file (e338c0ee). repeated tool_batch.effect.started: counted twice. SEEN: C3 test_D167_repeated_command_is_counted_twice (shape updates). reordered envelopes: file order. SEEN: C3 reordered test.

Ask: NO OBJECTION, or OBJECTION with one fixed condition.
Size: 5480 bytes
