# Card 229 sign-off (sign-off, ctx-6, fresh context; card 270 folded in)

Seat: Claude. Probe: the live step's decider.py probes (02:10 and 02:12 UTC on 2026-11-04: claude, copilot and grok ok). Read: board/229, 270, 290; record-in-progress.json, the task-force proposal, slice and both scope proposals, the checklists, preship-gate.md, nohome-check/, live/, green2-cycle2-attempt.diff; the four 229 windows (every Grok attempt); D181 to D183. In mob-crosscli (HEAD 2d4d24d = origin/main): `git log ab8f4a5^..2d4d24d` bodies and decider.py. Ran: `python -m unittest tests.test_launch_long_prompt tests.test_launch_failure_honesty tests.test_prompt_placeholders` (Ran 70, OK). I rebuilt the live prompt with decider.build_prompt from proposal-file.txt. Records repo: `git log` on the card's files; `python scripts/mob_process.py check` (OK, 105 closed, 14 records, 91 legacy). No full suite: I rely on suite_2d4d24d (1713 OK). No delete, stash, decider/copilot/grok/muse run, or repo edit.

Intent (D165): decide whether card 229 (with 270) can close inside the adopted windows and D181 to D183, and whether its record is honest enough to close.

Assumes (D166): window replies and session paths are as recorded (I did not open the session folders, outside git); full-suite lines as written.

## Verdict: SIGNED OFF with conditions

Each is a filing or a closing-record statement; no code change.
1. Closing record adds the deviations in section 3.
2. Closing record corrects the windows' Grok wording (section 3) and states the D181 trial reading (section 2).
3. File one bug card (section 4): Grok decision seats end on narration, and the first-line runner drops a vote that follows narration.
4. Card 270 closes as folded into 229. Card 290 stays open as a bug.
5. Closing record states the placement in section 5: Copilot rejoins counted seats; Grok stays non-counted until the section 4 bug is Done.

## 1. Done checks (as adopted by window 229-taskforce)

| Check | Verdict | Evidence |
|---|---|---|
| 1. Delivery: (a) a committed over-limit test and (b) a live Copilot and Grok parsed, cited vote with committed evidence | Met | See (a) and (b) below the table. |
| 2. Failure honesty and the empty guard | Met | Red 858649b came before green 4674d19 (D182 QA). Then a2bb1d4, d13148e and cda02f2. Vote records store attempt_log with rc, a scrubbed failure_excerpt and reason. An empty reply reads "empty reply (rc 0)" and is never counted. nohome-check shows the home path and token masked. The cited-only *result* rule was a 229-scope addition, not adopted check text. Window 229-scope-2 moved it to card 290, so the check is met and that part is out by ruling. |
| 3. Placeholder literals arrive unchanged | Met | Red cef48a7, then green 2d4d24d (14 tests). A template with neither placeholder is refused by name. |

Check 1 (a): red ab8f4a5, then green 4674d19. The fixture is 9923 bytes, over 8191. It passes at HEAD in my run.

Check 1 (b), verified:
- Both records hold vote Support-it, one attempt with rc 0, cite_ok true and counted true.
- Both cites appear verbatim in proposal-file.txt, at lines 19 and 22.
- build_prompt gives 9774 characters on 61 lines, sha256 1efdfc02...4bcc. Its whitespace-normalised sha is f5fcf8b5...5acc, which equals prompt_sha in both records.
- seat-use.jsonl lines 928 (copilot) and 929 (grok) match each record's ts and probe_taken_at. line_index is 927 and 928 (0-based). Both lines were committed in 6510168.

Cards 229 and 270's own checks map here: fake-shim truncation and cause (red 1), named refusal before Popen (criterion 1), live votes (1 (b)), rc and stderr (check 2).

Limits to keep in the closing record: Grok's vote followed narration (the prompt asks for it first; parse_vote accepts it); Grok's command_scan is "unscanned"; the prompt was the decided re-rank 20 prompt (recorded).

## 2. Cap story (D175, D181)

Criterion 2's cap went from 1 to 2, then to 3. Git and the window JSON agree:

| Step | Window ended | Window committed | Red | Red label |
|---|---|---|---|---|
| 1 to 2 | 229-scope, 00:36:24Z | a92eb0b, 00:36:34Z | 7c26629, 00:44:15Z | "Cycle 2 of 2" |
| 2 to 3 | 229-scope-2, 01:18:15Z | 2d38b94, 01:19:05Z (board/290 in the same commit) | d13148e, 01:24:35Z | "Cycle 3 of 3" |

No red that follows a passed green is labelled as the same cycle. Red 7c26629 follows green a2bb1d4, which passed, and it is labelled cycle 2. Red cef48a7 follows cda02f2, but it belongs to criterion 3. Green-cycle 2 was never committed; its diff is kept.

Red d13148e is stricter than D181 C2 needs (no green had passed 7c26629), but it moved scope out, so a window was needed anyway.

The final caps are 1+3+1 = 5. Cycles used: 1, 3 (one of them with no green), 1.

D181 trial reading for the closing record:
- Reds past a cap with no window before them: 0. Card 229 is the first capped card under D181.
- Scope windows: 2, both adopted.
- Minutes from failing evidence to the next red: about 58 (review 00e49f2 to 7c26629) and about 31 (de8a40a to d13148e).

## 3. Deviations

None blocks closing. The record omits or understates these:
- **Pre-ship gate on Copilot (unrecorded).** The declaration placed every role on Claude, and the slice said Copilot and Grok rejoin counted seats after card 229 is Done. The SHIP gate (ctx-5) ran on Copilot, the card's subject, before Done. This Claude sign-off re-checked every Done check, so it does not block. Record it.
- **Copilot counted seats (understated).** The record names only 229-scope round 2 (2 seats). Round 1 counted 3 (product manager, developer, coach), and 229-scope-2 counted 3 (product manager, developer, decider). No outcome depends on them.
- **Grok wording (inaccurate).** The decisions in 229-scope and 229-scope-2 say Grok "returned only narration" in every attempt. Two of the ten Grok attempts did end with NO OBJECTION and a valid CITE after the narration (w229scr2cp1-coach and w229sc2cp1-coach). The first-line runner dropped them. The other 8 stopped after narration with no vote.
- **Missing prompt file (unrecorded).** The slice named live/prompt.txt, but only proposal-file.txt was committed. The built prompt can be reproduced (section 1), so state that.
- **Seat entries.** Add the green continuations (33, 36, 45 commands), the live-step voters and this sign-off.
- **Commit author.** Every subject commit is authored "Copilot" although the seats were Claude. That belongs to card 275; state it.

## 4. Follow-ups (D183 sort)

- **Card 290: stays open, as a bug.** It is a decision-vote defect: result_of passes an uncited Yes, which is reproduced by the two moved cases.
- **Card 270: close** as folded into 229. All three of its checks are carried (section 1).
- **New bug card (decision votes, reproduced in the window JSON).** A Grok decision seat ends on narration with no vote in 8 of 10 window attempts. When it does vote after narration, the facilitator's first-line runner drops the vote (2 of 10). Under decider.py a no-vote is "none", which fails result_of, so Grok in a counted seat would fail Deciders.
- **Check for an existing card.** From card 276: parse_vote stores "NO OBJECTION" as "No". If nothing covers it, file it as a bug.
- **Retro experiments, not cards:** a per-card seat exclusion in the gate; a gate seat independent of the card's subject backend.

## 5. Copilot and Grok placement after this card

- **Copilot may rejoin counted decision seats** from the first window after this card closes. Its evidence: parsed, cited votes in 229-scope, in 229-scope-2 and in the live step. Its window command_scan reads "unscanned" (no command source). Keep that flagged, as the card's "Not in this card" says.
- **Grok stays a non-counted trial seat** until the section 4 bug is Done. That means a Grok seat returns its vote, and the window runner reads a vote that follows narration (or Grok's brief makes the vote the first line). Grok's command_scan is also "unscanned". One live vote in one run is not enough to count it.
