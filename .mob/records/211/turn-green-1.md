Intent (D165): Make red 1 of card 211 pass: write the Decider record and votes file that rule on 687410e..e3736d1 from the six seats' own replies, altering no vote.

What I wrote (mob-crosscli, commit ed97c75, pushed to origin main; nothing else):
- DECIDERS.md: a "Decider MD-010" entry in the decider.py --log format (Date, Seats, Votes, Result, Outliers), naming the range and all 27 commits, with notes stated as the seats said them: five of six did not read the diffs; the QA seat did not re-run the suite; the sign-off seat said "team-reviewed" overstates what the review shows.
- records/211/votes.json: range, the proposal text, and six votes (ctx-1 to ctx-6), each with reason, CITE (found in the proposal), context, plus the verbatim reply, run_context and start time.
- Two mappings, both disclosed in the files: the test accepts only NO OBJECTION / OBJECTION / Yes / No, so the vote field is "Yes" and the verbatim "Support-it" is kept in decider_vote and reply; and the role field uses the declared seat names (the architect and sign-off seats had voted under the labels "architect (slice)" and "sign-off"; those are kept in role_as_voted).
- Reasons are the replies minus the first and CITE lines. No reply text was changed.

Tests: python -m unittest tests.test_card_211_decider_record: 11 run, OK (red 1 was 1 failure of 11).
Time from tests passing (12:45:03) to push (12:45:12): about 9 seconds.
Full suite on the committed code (started on ed97c75 at 12:45): Ran 1556 tests in 1753 s, OK (skipped=1). No test_loop_state lock failure (card 258) and no test_mob_summary failure (card 268). The smoke script's "SMOKE FAILED" lines are printed output of tests that passed, not failures. Log: C:\Users\ringe\AppData\Local\Temp\claude\g211-1-632417253\full.log.
Unpushed commits of another seat: none. After my push, the QA seat pushed red 2 (53ec0c3) on top; I did not touch it.

Refactor: none needed, because this step added two records files and no code, so there is nothing to restructure.

Defects the seats named, for filing (each its own finding or card):
1. test_card_mode.py (~2031 lines) and facilitator.py (~692 lines) were rewritten wholesale, looking like line-ending churn; it hides which tests were red first (QA).
2. Gates landed with their tests in the same commits with no red run shown (QA).
3. All 27 commits share one identity, so non-author review (Done check 3) cannot be met (QA, reviewer, sign-off).
4. Large rewrites of facilitator.py and test_card_mode.py may duplicate gate logic (architect).
5. Hard-coded 45-minute, 30-minute and 60-minute thresholds should be parameters (architect).
6. About 3,900 added lines of audit-gate and idle-timeout machinery may over-specify up front, VISION ruling 6 (product manager, ready).
7. Loophole: a retroactive "team-reviewed" ruling lacks proof of seat independence; replies need committing under .mob/records/211 with context ids and the commits read (reviewer).
8. The motion leaves reshape and reject barely available, so the ruling mostly records that a review happened (sign-off).
Also: five seats did not read the diffs, and the decision wording "team-reviewed" overstates the evidence.
Size: 3176 bytes
