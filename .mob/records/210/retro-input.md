# Card 210 retro input (facilitator; facts only)

Card: the card record check refuses an empty task force, empty probes, self-reported cycles, members without a role, why or context, and empty retro metrics (corrective for MD-003, in mob-defaults). Signed off (c9e236a) with closing-record conditions.

## Flow metrics (UTC, from commits)
- First card commit 16:48 on 2026-11-02; sign-off about 17:06: about 5 hours.
- First card under D175: caps 1+1+1, held; one red and one green per criterion (572eb4b/690975a, edc0837/1baece8, 242677e/c633d3a); no stand-down needed.
- Greens pushed 2 s, 9 s and about 1 min after their own tests passed; full suite on each committed green (1475, 1517, 1545 OK).
- Windows: 210-taskforce and 210-slice, each about 15 minutes serial; agy took five decision seats, Copilot's six placed seats returned no reply.
- Pre-ship gate on Copilot (its CLI): HOLD on two test_mob_summary failures that reproduce only inside the Copilot CLI environment; the facilitator's plain-shell re-run was green (1545 OK); re-check SHIP; filed as card 268.

## Adjustments to review first (D155)
1. D175 written cycle caps (card 228 retro pick): first card under it.
2. Window 228-agy-seats: agy on decision seats.
3. Auditor FLAG green commit rule.
4. Refactor step after each green (Dispatch reminder mid-card): greens 1 and 2 recorded "none needed" late, green 3 in its own turn.
5. D168 Size line measure: the facilitator's briefs asked for whole-file wc -c against D168 C2; corrected mid-card.

## Observations
- The ready seat's T2 question was copied from card 228; the slice wrote this card's own T2 and it shaped Done check 3.
- Green 3 rewrote mob_process.py from LF to CRLF (card 269); the gate missed it, sign-off found it.
- The gate's first HOLD came from the seat's environment, not the code (card 268).
- New scanner false positives from Copilot transcripts: cmd /c and bash -lc wrappers read as file runs (card 254).
- Carding: Dispatch issues 243, 247, 259, 263 became cards 264 to 267 at a mid-card boundary; numbering raced with new issues once.
- Facilitator slips: a probe commit mislabelled (14:22 for 14:11); a stale probe at a seat launch after the restart (card 228).

## Queued inputs to weigh
- Open proposal-labelled issues (gh issue list --label proposal), and .mob/records/notes/2026-11-01-queued-retro-and-template-inputs.md.

## Standing section: the feedback-loop lens (D165)
Each voice names, for the 7 loops, who or what gives the feedback, its latency, what the team does with it, and MISSING where none exists.
