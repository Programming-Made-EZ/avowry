# Card 208 sign-off (product manager, ctx-7, fresh context)

Seat: Claude. The probes of 2026-10-30T23:03Z show Codex, Copilot, Grok and agy without usable quota (.mob/records/probes/2026-10-30.jsonl), so no other model was available and the least-recently-used rule lets this seat start. Read: the card text (board/208, as corrected by D164), D161, D163, D164, D157, D150, the record, review-ctx5.md with its re-check, preship-gate.md, and mob-defaults code at 9cda13d (C:\Users\ringe\mob-crosscli). Wrote nothing there; ran one demo script from the scratchpad against the code (in memory, temp repo).

## Verdict: SIGN OFF

## Against the product owner's intent

- D161 (lock the process into tooling): met for this card's slice. Three rules that were by hand are now refused by the runner with exit 2 before any backend call: a seat on a probe 60 minutes old or more (re-probe, or refuse if it cannot probe or keep the evidence); a Claude seat while another available model has waited longer; an enabled backend without a usable pinned probe model (refused at card start, and a failed probe with nothing launched when called directly).
- D163 (balanced rotation, not a rule against Claude): met. Claude is refused only while an available model (enabled, probe ok) was seated longer ago than Claude; a tie, a never-seated Claude, a failed or disabled model, and an all-Claude moment when nothing else has quota all let Claude start. Nothing stricter than D163 was added (D164 C3). The reason field is built from the probe and the history line, never typed by a seat.
- The three corrected Done checks and ZOMBIES: check 1 (fresh probe), check 2 (least-recently-used from the tracked append-only seat-use history, with the joiner's own vote and the seat-equals-probe model asserted) and check 3 (pinned probe model) each have Z, O, M, B, E, I covered; the reviewer confirmed this with mutations that fail the tests (tie refuses: 9 failures; seat launches "default": 3; joiner dropped from signoff: fails; unusable-pin branch off: 6 failures).
- D157: one "Criterion: n" per cycle, green for n before any red for n+1; the return to 2 after 3 is the D164 corrective cycle, not an overlap (gate item 4). No trailers, no rewrites.
- Re-check APPROVE (ctx-5): F1, F2 and F3 closed. The pre-ship gate (ctx-6): PASS on 35 hashes on origin, CI absent (nothing to be green), commit order, deviation, and the domain-expert answer.

## Deviations, stated plainly, and whether any blocks

1. The red d3125ea is 463 lines in two test files, over the 400-line D136 limit. Recorded in the commit body and the record, accepted by the reviewer (one criterion, one layer, tests only), not rewritten. Does not block: the rule's purpose (a reviewable change) held because only tests changed; the cost is a larger review, and the retro should take it as evidence for splitting reds by ZOMBIES group.
2. The first full run on the corrected criterion failed 13 tests from shared-fixture pollution (a quota test disabled codex in the BACKENDS dict that every test module shares), a defect targeted runs hid. It was fixed in fa88edb and the second full run was 444 tests OK, 1 skip, 1258 s at 2a01531. Does not block: the defect is fixed and the new tests were built to avoid the same class. It is a lesson for the retro: targeted module runs cannot stand in for a full run on shared module-level state.
3. The last commit, 9cda13d (review fix F1), is not in a full run. It is test-only, one file, 35 lines (git diff 2a01531..9cda13d), the production code is identical to the 444-test run; module runs (16 tests; 73 tests across the modules that touch BACKENDS) passed and the branch was shown by mutation. Does not block, with the limit named: a full discover at 9cda13d itself has not run, and the 21-minute command stays available (`python -m unittest discover -s tests` in C:\Users\ringe\mob-crosscli).
4. Smaller, none blocking: the green and units were developed in one working tree (10d9db5 and fa88edb say they ran on an uncommitted green); .mob/records/seat-use.jsonl does not exist yet and appears at the first seating of the next card-mode run, so the first card shipped under it must commit it (D164 C1); the gate does not bite on this hand-run card; mob-defaults 9cda13d is on origin.

Limit worth knowing (not a deviation): the gate refuses Claude jumping ahead; it does not choose among Codex, Copilot, Grok and agy, which still follow the card's declared assignee. That matches D163's failure mode (all-Claude while others have capacity) and the card text of check 2, but a facilitator should not expect the runner to pick the least-used non-Claude model.

## Persona demo (D150), run against the code at 9cda13d

Persona: the facilitator running a card (Riley, the mob). Real output from the demo script (scratchpad):

- Stale probe: a probe taken 18:10Z is reused at +55 minutes; at +61 minutes the facilitator re-probes first and the seat starts on a probe now cited as taken 19:11Z. If the probe tool is down, the seat is refused with: "the driver probe in force (taken 2026-10-30T19:11:06Z) is 60 minutes old or more, and a new probe could not be taken ... No seat starts on a stale probe. Seat refused: the observer seat on claude in no context."
- Claude seated while another model waited longer: "the reviewer seat in ctx-5 is on Claude while codex has waited longer since the last seating (.mob/records/seat-use.jsonl): a Claude seat starts on its own least-recently-used turn. Seat it on codex instead." With Claude's own turn oldest, the answer is empty and the seat starts.
- Backend with no probe pin: card start names each one ("backend grok needs probe_model, one of its models (None is not; models: default)", "backend agy needs probe_model ... ('nope' is not ...)"); the shipped backends.json passes (no problems); probe_backend on grok with no pin returns ok False, "no usable probe_model for grok in backends.json (None)", and the launch function was not called.
- The facilitator sees a named reason and an exit 2 in each case, instead of a seat that quietly starts on old evidence.

## The card's metric

"Standing rules the loop runner enforces versus those enforced only by hand" (D161), counted on the seven MD-002 rules as the record counts them: after this card the runner enforces 6 of 7 (3 after criterion 1's slice start, 5 after the first criterion 2, 6 now that the pinned probe model and the corrected rotation are in); 1 is still by hand, the record checker requiring quota_reason (card 210). Still by hand outside that seven: the in-flight line rules (own timestamp, probe age, cap source) and the gate biting on plain failover turns of a hand-run card. Companion metric (audit findings per card about runner-gated rules): no audit has run since F-008, so no value can be read yet. D164 C4's measures (seat share per model, all-Claude rosters while others had quota, target 0) have no reader yet; they are computable from seat-use.jsonl and belong to a later card.

## Not verified

A full discover run at 9cda13d; the first live seating that creates seat-use.jsonl; the product owner's open question (which Claude family seats use).
