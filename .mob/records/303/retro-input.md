# Card 303 retro input (facilitator; facts only)

Card: the C# baseline endpoint tests pass on the Windows development machine (bug, owned for card 217's Done check 3). Signed off with conditions (.mob/records/303/signoff-ctx6.md); the follow-up bug is card 306 (uncapped diagnostic). Product-owner machine action: the ASP.NET Core 8 x64 runtime was installed after green 1; the two tests then passed.

## Flow metrics (UTC, from commits and records; commit times in git are local, UTC-8)
- Ready and task force: the ready seat's proposal (a Claude subagent; the proposal named Copilot, as on cards 297, 264 and 217), task-force window round 1 06:16 to 06:38 (five NO OBJECTION, one OBJECTION with a condition from the audit lens), round 2 06:47 to 07:04 (six NO OBJECTION; one D171 re-ask of the coach, who dropped markdown asterisks around the cite). Card text fixed (six edits).
- Slice: Muse (trial) returned no reply; Grok (re-probed OK after the sign-in) narrated and stopped (card 291's defect); Codex read-only delivered it; about 07:58 to 08:25. Red (Claude QA): one test, against a seam the green had to add, 08:25 to 08:31; red committed locally and pushed with the green (CI was not red on a red this time). Green (Codex ctx-4, full-access mode, `--json`): 08:32 to about 09:05, 15 commands; it ran the probe, wrote the three records, added the seam and the diagnostic, ran the focused test (pass) and the whole acceptance project (314 passed, 2 failed, the two owned tests), committed 30c9e84 and pushed it through the pre-push hook. CI on 30c9e84 (run 42192200361): success, about 9 minutes.
- Wait for the product owner's install: about 5 hours (09:19 to about 14:30 UTC), with no other card started.
- After the install: focused rerun 2 of 2 (31 s); whole project 316 of 316 in 11 m 46 s. Gate: two launch attempts failed on the facilitator's command quoting (cmd /c dropped the quotes), the third (copilot.cmd directly) gave SHIP; sign-off (Claude subagent, fresh) SIGNED OFF with two conditions; follow-up bug 306 filed.
- Caps: 1 cycle (check 1 was the code-visible slice; checks 2 and 3 closed by the install and CI). No green stop.
- Seat load: Codex used twice (architect read-only, green), Copilot once (gate) plus probes, Grok and Muse trial attempts at the slice (both no reply), agy out until Nov 8 (quota).

## Adjustments to review first (D155)
1. D190 (a gate says SHIP only on commands it ran or named; trial, next three cards with a gate), first card under it: the Copilot reviewer labelled every item rerun / record-only / blocked; its own `dotnet test` was blocked and it said so (Done check 2 record-only); the facilitator's rerun is in preship-gate.md. The sign-off seat used the same labels and reran four checks.
2. D189 (a slice's numbers carry their commands), second card under it: the Codex slice table labelled each number record-only or with its command; the facilitator's table noted line numbers differ by a few lines at the current HEAD.
3. D188 (in-flight refusal on a probe older than 50 minutes), third card under it: probes were retaken before the slice seats (07:51 to 07:58) and before the gate (15:07); no in-flight line refused.
4. The PO inbox (in use since the previous cycle; the proposal to adopt it is queued for a retro to weigh): read at every boundary on this card; four entries arrived and were acknowledged within one cycle (the Codex build seat decision, the card-format question, the Muse proposal, and the machine action arrived by chat).

## Observations
- The cause was shown by the probe, not guessed: the lead from the ready seat (no AspNetCore 8 runtime) was confirmed with the host's own error line, and the harness change (the diagnostic carries the output) is the part that stays useful.
- The red needed a seam in production code (PHRASE_BENCHCS_DOTNET) to be machine-independent; QA found that a fake `dotnet` on PATH fails on Windows (`.cmd` not resolved by Process.Start) and picked an env var; the slice's red ("fails on the current machine") had been machine-dependent and the facilitator's note caught it before the red.
- The sign-off found a defect the gate did not: the new diagnostic is uncapped and applies to non-endpoint tasks too (card 306).
- Codex as a build seat worked end to end: edit, explicit `git add`, commit, push through the pre-push hook under the single-writer lock. The delete rule was in the brief; the facilitator's read of the 15 commands and the sign-off's regex found no delete. The commit author is the machine's git identity, same as other seats. The scan for Codex (card 305) is still unbuilt.
- Launch friction: `cmd /c copilot -p "..."` strips the prompt quotes (two failed gate launches); `grok --prompt-file` ends after narration (card 291); `muse exec` plain mode printed no reply on a read-and-write-up brief. A seat run through a script that calls copilot.cmd directly works.
- Quota: Codex 45% weekly at a heavy burn per the product owner; this card used it for two turns only; Copilot 20% to Dec 1; agy out.

## Queued retro inputs (named in the voices' ask)
- The PO's card-format question (docs/retros/experiments.md), the Muse rotation proposal, the inbox-adoption proposal, the Codex-commit question (now answered by the PO), and the other logged experiments.
