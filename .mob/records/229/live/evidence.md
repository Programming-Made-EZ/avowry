# Card 229 live step: criterion 1 (b), a real Copilot seat and a real Grok seat cast parsed, cited votes

- **Run:** by the facilitator on 2026-11-04 (Copilot seat from 02:10:03 UTC, Grok seat from 02:11:58 UTC), after greens 1 (4674d19), 2 (a2bb1d4, cda02f2) and 3 (2d4d24d), before the pre-ship gate. decider.py at mob-crosscli 2d4d24d, `--seats copilot`, then `--seats grok`, `--timeout 300`, readonly invocations.
- **Seating:** each seat NON-COUNTED. Window 229-taskforce left Copilot and Grok out of counted seats until this card ships, and the slice says the fix's subject does not count on its own proof. The re-rank 20 vote these replies answer was decided by D182 and is not reopened.
- **Prompt:** proposal-file.txt is the re-rank 20 Decider prompt the facilitator's runner sent to the architect seat (role text, the Decider header and the re-rank 20 proposal). decider.build_prompt wraps it to 9774 characters on 61 lines (sha256 1efdfc022a35857313294bf09f6f74870b104f290b45e625738a763453c34bcc), over cmd.exe's 8191.
- **Deviation from the slice:** it named the adopted 229-taskforce round-2 proposal, but that builds to 7161 characters, under the limit (red 1's finding), so it could not show the over-limit path.
- **CLI versions:** GitHub Copilot CLI 1.0.104 by `copilot --version` (the voter's session file reports copilotVersion 1.0.105); grok 1.0.46 (2765805b9442).
- **Seat use:** .mob/records/seat-use.jsonl line 928 (copilot, 2026-11-04T02:10:36.081317Z) and line 929 (grok, 2026-11-04T02:12:26.864515Z), both role "decider voter".
- **Copilot session:** %USERPROFILE%/.copilot/session-state/6d00324b-1a36-4669-98e1-d5be520d6628/events.jsonl (start 02:10:54Z; cwd the decider's throwaway clone under %TEMP%/mob-deciders/run-f514tngi/copilot).
- **Grok session:** %USERPROFILE%/.grok/sessions/<encoded %TEMP%/mob-deciders/run-hdzno714/grok>/01a1a454-809d-7fb2-ab57-19cc943f9322/.

## Votes (copilot/decider-result.json and grok/decider-result.json, as written; neither holds the account name)

| Seat | Vote | Model | CITE | cite_ok | counted | attempt_log |
|---|---|---|---|---|---|---|
| copilot | Support-it (its reply starts with the vote line) | auto | "The root cause is known and reproduced" | true | true | one attempt, rc 0, no failure excerpt |
| grok | Support-it | grok-4.6 | "rank only process cards that block safe, honest product work" | true | true | one attempt, rc 0, no failure excerpt |

- Grok's reply opens with narration (what it read) and runs straight into "Support-it" on the same line; decider.py's parser finds the vote word there. The facilitator's window runner, which requires the vote on the first line, would not read it. That is a finding for the retro and for card 270's live use, not a failed check: the vote is parsed and cited.
- The facilitator checked both CITE phrases verbatim against proposal-file.txt (cite_ok true in each record).
