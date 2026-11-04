# Card 290 sign-off (sign-off, ctx-6, fresh context)

Seat: Claude. Read: board/290, the card record, proposal, slice, scope turns, gate (SHIP), the five 290 windows, D171/D175/D181/D183-D185. mob-crosscli HEAD 73eea8a = origin/main: commit bodies, result_of, cite_ok, call site. Ran: the three permitted test modules (Ran 98, OK); `mob_process.py check` (OK, 106 closed); two pure-function probes of cite_ok and parse_vote. Full suite relied on (suite_73eea8a, 1788 OK). No delete, stash, model run or repo edit.

## Verdict: SIGNED OFF with conditions

Closing-record statements and one filing; no code change.
1. The closing record states that Done check 1 is met by ruling for its second half (section 1).
2. The closing record adds the understated deviations in section 4.
3. File one bug card: cite_ok reads a Codex prompt echo (section 5).
4. Log the section 5 retro experiments.

## 1. Done checks

| Check | Verdict | Evidence |
|---|---|---|
| 1. An uncited Yes is uncounted; an uncited Yes beside cited votes "fails" | Met, the second half by ruling | Red 1 38b8eb5, red 1b 7e0f200, green 1 93c4f62, red 1c 8f88c93 and green 1b 640c41e. Module OK in my run. |
| 2. A cited Yes passes and a No fails, as today | Met | Red 2 78ab63b pins old_rule. It passes before and after green 1. |
| 3. The doubles carry cites and the decider modules pass | Met | Red 3 3d503c8 and green 3 73eea8a. I ran test_fake_voter_cites and test_decider_record: OK. The rest rests on the record (12 modules, 270 tests; suite 1788). |

Ruling on check 1: the card's words and card 229's moved case (`test_an_uncited_yes_beside_a_cited_yes_still_fails`, 7c26629) expect "failed". The adopted condition of window 290-taskforce leaves an uncited vote out, so the result is the cited-only result: "passed" beside cited Yes votes. The slice restated the check as "cannot tip the result", and red 1's M and I cases test that. The record never says the card text was inverted. Condition 1.

I read the adopted condition in the code myself:
- Call site, decider.py:351-354: `counted = vote != "none" and cite_ok`. result_of receives the counted votes plus any seat whose vote is "none" with an empty reply.
- result_of({}) returns "failed: no counted votes". Any "none" fails the result.
- An uncited reply, a CITE line with no vote line, and an uncited No are all left out. A seat that never answered (failed launch, timeout or empty reply) still fails.
- cite_ok reads the first CITE line only and does not cross a newline. The phrase must be 6 or more words found verbatim, outer quotes stripped, or "line N: w1 w2 w3 w4". This matches D171 C1 except for the title-line and VOTING RULE exclusions, which window 290-scope cut.

## 2. Cap story (D175, D181)

| Step | Window ended / committed | Red | Label |
|---|---|---|---|
| Criterion 1 cycle 1 widened | 290-scope 10:56:07Z / dcacc8c 10:56:08Z | red 1b 7e0f200 11:04Z | "Cycle 1 of 1, red 1b (no green has run)" |
| Criterion 1 cap 1 to 2 | 290-scope-2 12:27:50Z / 93ceeaf 12:27:51Z | red 1c 8f88c93 12:36Z | "Cycle 2 of 2" |

- Red 1b came before any green (green 1 landed at 11:41Z).
- Green 1 passed red 1. The criterion-1 red after it (1c) is labelled cycle 2, and its window was committed first.
- Reds 2 and 3 follow green 1 but belong to other criteria.
- No red after a passed green is labelled as the same cycle.
- Final caps 2+1+1 = 4; cycles used 2, 1, 1.
- D181 metrics: reds past a cap with no window before them: 0. Minutes from failing evidence to the next red: about 33 (green 3 stop 2671f0d at 12:03Z to red 1c).
- Nits, not blockers: red 1b's body lacks D181 C2's literal line "Corrects red <hash>; no green since". The names "red 1c" and "green 1b" carry letters although they are a new cycle; their Cycle lines are correct.

## 3. The facilitator's readings

- **Declaration fix "as none":** acceptable. The 290-declfix text contradicts the earlier adopted condition and red 1's tests ("as none" lets an uncited No fail). Green 1 followed the condition and said so in its body; the record names the drafting error. A correcting window would have been cleaner but would not change the code. Retro signal: six NO OBJECTION votes passed the contradiction.
- **Green 1b followed the Intent:** acceptable, no window needed. The literal "counted or none" breaks red 1's adopted E cases; the Intent line covers the narrower reading. Red 1c flagged it, the brief and commit body record it, and the gate judged it.
- **Green 3 and the zero-voter test:** acceptable. The slice's C3 ZOMBIE E explicitly allows a test that asserts an uncited or zero-vote pass to be rewritten to the new rule and named in the green. The commit body names it. The new expectation, exit 1, is criterion 1's Z case.

## 4. Deviations

No closing blocker. Recorded honestly:
- The slice attempt that ran 46 minutes (cap 30) was stopped. It built the fix instead of the slice, and its diff is kept as evidence.
- Both green stops worked as the stop rules intended, with no commit. The green 3 stop found green 1's dropped "did not answer" votes; the record lists this as a deviation.
- F-087: an in-flight commit was pushed 19 minutes late. The cause and the fix are recorded.
- Scanner false findings: the slice seat's "unreadable file" and the gate's "rmtree" (cards 254 and 283).

Understated, for condition 2:
- In window 290-scope-2, Copilot's audit-lens reply was an OBJECTION with a CONDITION (replace A1 and A2 with git evidence, or mark them unrun). It was dropped as unread because its first line was not a window line, and Claude took the seat. The decision text says only "six NO OBJECTION". On substance A1 holds (decider.py:76-81 fails on "none"), but the record should say so.
- The slice seat edited its slice file after its turn. This sits in p6_result, not in deviations.
- Green 3 attempt 1 left an uncommitted change in the shared tree. The facilitator saved it as a diff and restored the file.
- Red 1's Environment line reads "commit before 2d4d24d", but 2d4d24d was its parent.

## 5. Follow-ups (D183)

**Bug card, decision votes (reproduced):** cite_ok reads the whole stored reply, including Codex's prompt echo, while parse_vote strips that echo. Suppose a proposal has a line that starts with "CITE:" and a Codex seat echoes the prompt and then answers a bare Yes. My probe gave parse_vote "Yes" and cite_ok True: an uncited Yes is counted. No committed proposal has such a line today, so the defect is latent. Fix: cite_ok reads the same de-echoed text that parse_vote reads.

Retro experiments, not cards:
- There is no counted quorum. One cited Yes passes when every other seat's reply is unread (narration only). The result is right by the adopted rule; counted_seats shows the gap.
- build_prompt says "Nothing else" and never asks for a CITE. Counting now depends on the proposal's own voting rule. This fails closed.
- A time cap for seat turns, given the 46-minute slice attempt.
- Check the F-087 push fix on the next push failure.
