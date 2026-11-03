# SCOPE PROPOSAL: window 276-d175

Intent: Finish D179's voting-seat metric on this card now that Muse is signed in: one Muse decision seat, probed and scanned, returns a cited vote. Source: board 276 "One observable behaviour"; D179 "voting seats first"; Dispatch sign-in about 04:20 UTC.

Assumes: muse exec without --json prints the model reply as stdout, so decider.parse_vote can read a first-line vote word; probe: Dispatch headless muse exec "Reply with exactly: pong" returned pong (record sign_in, about 04:20 UTC). Also assumes a Muse session log for {SESSION} lands under %USERPROFILE%\.local\share\muse\sessions\YYYY\MM\DD\<id>\session.jsonl on this laptop; probe: slice finding (c) and turn-slice-architect.md (local-tracing present). --model default is unprobed against muse exec; a refusal is a stop, a later window.

## Cycles and hours used

C1: 1 of 1 (red 1 5ef4b8f / 1b 970118c, green 2196c58, refactor 41ecf78).
C2: 1 of 1 (red 2 dbe2e1c, green d3a9787, refactor c71c05f).
C3: 1 of 1 (red 3 ac5a5a2 / 3b fd09aaf, green 40d0d08, refactor 5157e3f).
Named mid-card fix (already spent): e3608ae, wiring pin muse -> muse and --session-id {SESSION}.
Hours: first card commit 5ef4b8f 02:18 UTC to e3608ae 04:27 UTC, about 2.2 h of committed work; task-force from 00:32 UTC, about 4 h of card clock. Sign-in probe 04:22 UTC.

A stop now leaves: muse enabled false; --json stdout vs parse_vote (voteless); locate_muse on Path.home() only; C1 helper sign_in unknown on Tier/Window/Weekly and on No usage data (server_sent_no_usage); untrusted-workspace exec; Done check 3 and the board metric unmet.

## Past-cap items

(1) Follow-up: Muse usage text classifies signed-in and unknown-quota. _detect_sign_in is the install helper; the live C1 test already accepts unknown. Seating uses probe_state_for on muse exec (ok => available). Quota-unknown (server_sent_no_usage) is usage-text work.

(2) Keep, C3 cycle 2. backends.json invocation is muse exec --session-id {SESSION} --model {MODEL} --prompt-file {PROMPT_FILE} --json. run_backend returns raw stdout; parse_vote wants a sentence-start Yes / Support-it / No. Gate_gaps (1): a Muse vote would be voteless. Form: JSONL / --json reply with no vote word. SEEN: gate_gaps (1); invocation at HEAD.

(3) Keep, C3 cycle 2. This is the board metric. enabled false still. Roles fallback_chain unchanged (build seats later, D179). Evidence: one facilitator-run Muse voting seat in a decision window. Form: backend disabled at probe. SEEN: core/backends.json muse enabled false; C2 test_e_d167_disabled_state_is_named_explicitly.

(4) Keep, C3 cycle 2. Slice C3: data_root = XDG_DATA_HOME, else HOME/.local/share, else USERPROFILE/.local/share. locate_muse uses Path.home() then .local/share/muse/sessions. Tests inject root. Needed for scanned on the metric. Form: missing or unusable env root. SEEN: slice C3 path rule; test_E_unresolved_windows_root_is_no_source_never_clean. INSURANCE for a live XDG_DATA_HOME on this Windows box.

(5) Drop. C2 still pins disabled and unknown-pin (test_muse_probe_state.py test_e, test_s). Cycle 2 acceptance is enabled true. C3 unknown-envelope cases remain in test_muse_command_source.py.

(6) Follow-up: Muse exec in an untrusted non-repo workspace. Dispatch saw "Agent delegation: auto unavailable: workspace is untrusted" outside a repo. Probes pass the card repo; decider.vote_seat clones a repo. Off the vote path.

## Smallest scope

Keep the metric on card 276. A follow-up would leave Done check 3 unproven (D175/D168 counter: meeting the cap by sending the risk later).

Raise C3 from Cap 1 to Cap 2. Adopt cycle 2 of 2 before its red. Caps 1+1+2 = 4.

Why the total may exceed 3: the slice spent 1+1+1 on install facts, the unsigned-in probe, and the fixture reader, with the cited vote waiting on sign-in. Sign-in landed after those caps were spent. One extra cycle finishes Done check 3 here. D136 stays at three criteria. Cut: items (1), (5), (6); no muse on role fallback chains; no readonly-flag hunt; no --model alias work unless live exec refuses default.

The wiring fix e3608ae is already spent and is named here (D175 C1). This window does not adopt a further fix cycle.

C3 cycle 2 (Done check 3 remainder).
Acceptance: muse enabled true; invocation keeps muse exec --session-id {SESSION} --prompt-file {PROMPT_FILE} and a stdout shape parse_vote reads (drop --json; pong smoke); locate_muse uses XDG_DATA_HOME, else HOME, else USERPROFILE; facilitator evidence: one Muse voting seat probed (state available), command scan other than unscanned, first-line vote word plus D171 CITE. Trust-ladder Rung 1. No seat runs muse login.
Cap: criterion 3, at most 2 cycles (this window adopts cycle 2 of 2).
Stop: a new Muse CLI shape (model flag, json-only stdout, unexpected log root) ends the cycle; a later window.

ZOMBIES: Z --json / JSONL reply => parse_vote none. O one Muse window seat: available, scanned, cited vote. M locate_muse finds the session under XDG_DATA_HOME, else HOME, else USERPROFILE. B enabled false; --model default refusal (stop). I scan_entry(..., source="muse") and locate_muse share the env-root rule; parse_vote sees the same text run_backend returned. E unusable data_root => no-source. S claude/copilot readers and role fallback_chain unchanged.

Inputs: disabled: muse enabled false => probe disabled (withdrawn this cycle). SEEN: backends.json. unknown pin: probe_model missing => unknown-pin, CLI not launched. SEEN: C2 test_s. repeated: two muse keys. INSURANCE (JSON keys unique). reordered: models map order leaves the pin unchanged. SEEN: C2 reordered test.

Ask: NO OBJECTION, or OBJECTION with one fixed condition.
Size: 5664 bytes
