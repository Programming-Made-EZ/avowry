# SCOPE PROPOSAL: window 276-scope-2

Intent: Finish D179's voting-seat metric on this card: one Muse decision seat, probed and scanned, returns a cited vote. Source: board 276 "One observable behaviour"; D179 "voting seats first"; Dispatch sign-in about 04:20 UTC. Cycle 2 enabled the backend; the live trial then refused `--model default`.

Assumes: dropping `--model`/`{MODEL}` is the working invocation; probe: Dispatch smoke `muse exec "Reply with exactly: pong"` (no `--model`) returned pong (record `sign_in`, about 04:20 UTC); QA red 244678b used no `--model`. Also assumes live `muse exec --model default` is refused; probe: metric_attempt_1 "model `default` does not exist or you lack access" (session 633ab116-3065-40f3-9307-09761bd6020d).

## Choice: (A) add C3 cycle 3

Cycle 2 of 2 is spent. The previous stop (`--model default` refusal; a new Muse CLI shape ends the cycle; a later window) fired as written. This is that later window.

Keep the metric on card 276. Closing now leaves muse enabled with a refusing invocation, so live Muse seats fail until a follow-up, and Done check 3 stays unproven (D175/D168: meeting the cap by sending the risk later). Green cefaf4e already shipped enablement, prompt-file, env roots, and the vote-evidence builder. Remaining work is one invocation string plus the live trial the adopted audit-lens condition already required.

Drop `--model`/`{MODEL}` from the muse invocation. Dispatch and QA already proved that shape. A catalog id needs a live `--model <id>` call that does not exist; `muse model-profile show` resolves any string and did not catch `default`.

Raise C3 from Cap 2 to Cap 3. Adopt cycle 3 of 3 before its red. Caps 1+1+3 = 5.

Why the total may exceed 3 (and the prior 4): the slice spent 1+1+1 before sign-in; window 276-scope added cycle 2 after sign-in. Cycle 2's stop probed `--model default`; omit-the-flag is now proven. One extra cycle finishes Done check 3 here. D136 stays at three criteria. Cut: catalog-id pinning; `--model` alias map; items (1) and (6) stay on follow-up cards.

## Cycles used

C1: 1 of 1 (5ef4b8f / 970118c, 2196c58, 41ecf78). C2: 1 of 1 (dbe2e1c, d3a9787, c71c05f). C3: 2 of 2 (ac5a5a2 / fd09aaf, 40d0d08, 5157e3f; 244678b / c0354cb, cefaf4e). Named fix e3608ae.

A stop now leaves muse enabled with `--model default` refused, Done check 3 unmet, and live Muse seats failing.

## Smallest scope

C3 cycle 3 (Done check 3 remainder).
Acceptance: muse invocation is `muse exec --session-id {SESSION} --prompt-file {PROMPT_FILE}` with no `--model`/`{MODEL}`; stdout shape parse_vote already reads (plain, no `--json`); one Muse voting seat probed available, scanned, first-line vote word plus D171 CITE, committed under `.mob/records/276/` with session id and log path (adopted audit-lens condition). Trust-ladder Rung 1. No seat runs `muse login`.
Cap: criterion 3, at most 3 cycles (this window adopts cycle 3 of 3).
Stop: a live `muse exec` refusal after dropping `--model` ends the cycle and the card; Done check 3 unmet, metric to a follow-up. Quota or unavailability still excludes (card 277). No catalog-id hunt.

ZOMBIES: Z `--model default` still on the muse line. O one Muse window seat available, scanned, cited, evidence committed. M no `--model`/`{MODEL}` on muse. B second live refusal after the drop (stop). I scan_entry source=muse still locates the session; parse_vote sees run_backend stdout. E unusable data_root => no-source (kept). S other backends' `{MODEL}` pins and role fallback_chain unchanged.

Inputs: disabled: n/a: cycle 2 enabled muse (c0354cb / cefaf4e). unknown pin: probe_model missing => unknown-pin, CLI not launched. SEEN: C2 test_s. repeated: two muse keys. INSURANCE (JSON keys unique). reordered: models map order leaves the pin unchanged. SEEN: C2 reordered test.

Ask: NO OBJECTION, or OBJECTION with one fixed condition.
Size: 3864 bytes
