# Card 276 sign-off (sign-off, ctx-6, fresh context)

Seat: Claude. Probe: 2026-11-03 18:17 UTC. Read: board/276, 277, 279 to 283; record-in-progress.json; preship-gate.md (HOLD); preship-regate.md (SHIP); turn-slice-architect.md and turn-scope to turn-scope4-architect.md; the windows 276-scope to 276-scope-4; D179 and D180; muse-trial-seat/evidence.json; retro-input.md. In mob-crosscli (HEAD 839380b, equal to origin/main): `git log` 5ef4b8f..839380b, core/backends.json, facilitator.py MOB_BACKENDS, decider.parse_vote. Ran in mob-crosscli: `python -m unittest tests.test_muse_cycle4b_real_logs tests.test_muse_cycle4c_condition tests.test_repo_path_lengths` (Ran 10, OK); a byte compare of the four fixtures against the live Muse logs; `scan_entry` on fixtures and on the live logs. Ran in the records repo: `python scripts/mob_process.py check` (OK, 104 closed cards, 13 records, 91 legacy). No full suite: I rely on suite_839380b (1643 OK, 1 skipped). No delete, no stash, no muse command, nothing under .mob/audits/, no repo file edited.

Intent (D165): decide whether card 276 can close inside D179, D180 and the adopted windows, and whether its record is honest enough to close.

Assumes (D166): full-suite lines in the record are as written; window votes are as recorded; file mtimes on the live Muse logs were not altered.

## Verdict: SIGNED OFF with conditions

Conditions, each a filing or a closing-record statement, no code change:
1. Closing record states the cap story as in section 3, including that four commit bodies carry wrong cycle lines.
2. Closing record corrects the stale and missing entries in section 4.
3. Facilitator tells the product owner (via Dispatch) that the clean scan arrived at cycle 6, past the cap 4 that D180 named, ratified after the fact by the team.
4. Re-rank closes or rewrites card 279 (section 5) and files cards for the uncarded defects in section 5.
5. Closing record states the trust-ladder placement: Muse is a trial voter only; no build seat (section 6).
6. Facilitator scans this seat's transcript and runs `check` again after the closing record.

## 1. Done checks

| Done check | Verdict | Evidence |
|---|---|---|
| 1. Install and sign-in | Met | Muse 1.4.14 installed natively by Dispatch at the product owner's request; sign-in reported by Dispatch, confirmed by `muse usage` (Tier, Window, Weekly); red 1b 970118c, green 2196c58. The record does not say whether the sign-in flow was device-code or URL; Dispatch's note is the only source. I accept it on the record. |
| 2. Probe | Met | Red dbe2e1c, green d3a9787, refactor c71c05f: distinct not-signed-in, disabled and unknown-pin states; live probe ok in evidence.json. |
| 3. Reader and first seat | Met (not by waiver) | Vote: session e338c0ee, NO OBJECTION with a D171 cite found verbatim (trial voter on scope-2, not counted). Scan: I ran `scan_entry` myself at 839380b. Fixture gives clean, 0 commands, 3 other tools, sha b066f64a (same as the re-gate). The live log gives clean, 0, 3, sha ae3591b2 (same as the facilitator's green 4b re-scan). Provenance: I checked it myself. The two top-level fixtures equal their live logs once `ringe` is replaced by `<user>` (11 and 25 hits). The two subagent fixtures are byte-identical. The live vote log was last written 07:01:52 UTC, inside the vote run. So the re-gate's "cannot prove" is settled. |

Check 3 has limits the closing record should keep. The scan the seat recorded at run time was "no command source"; the clean result is a later reader applied to the same bytes. The vote log has no shell command. On real Muse records, delete detection is shown only on the capture log, whose three commands never completed, and on a mutated copy of it.

## 2. D180

D180 held the card open and did not waive the clean scan. Its unblock condition was met: Dispatch ran the elevated setup, and `check` read ready at 16:07 UTC. The vote log now scans clean on the board's own wording, so D180 is met without a waiver. One gap: D180 described cycle 4 "as adopted in window 276-scope-3 (criterion 3 cap 4)". The clean scan came only at cycle 6. Cap mechanics belong to the team (D179), so this does not block. It is a change to what the product owner was told, so condition 3 applies.

## 3. Cap story

Criterion 3's cap went 1 (slice), then 2 (276-scope), then 3 (scope-2), then 4 (scope-3); each window was adopted before its red. Then cycle 4 closed at green 2e56d9f. Red 4b with green 7c04e2e, and red 4c with green 0000715, were cycles 5 and 6, run with no window. Audit 195 found this. The facilitator's cycle_count_ruling admits it plainly, including that the ruling came after green 4c and the refactor. Window 276-scope-4 ratified the cap at 6 (1+1+6 = 8), with five NO OBJECTION votes, all Claude Sonnet, and no further red. That is honest, and ratifying after the fact is acceptable for closing: the cycles stayed inside scope-3's adopted scope, the alternative was revert commits with no history rewrite, and the window says the D175 metric still counts the breach. The closing record must say:
- the D175 metric is hit: two reds ran past the cap with no window before them, and the ratification came after;
- the bodies of 25d50b8, 7c04e2e, 58b5c26 and 0000715 say "Cycle 4 of 4 ... red amendment 4b/4c" but are cycles 5 and 6 of 6. There is no amend, by rule. The re-gate's "each commit body carries ... cycle lines" passed over this;
- the retro proposal: a lettered red is allowed only before any green has passed the red it corrects.

## 4. Deviations

The record lists 22 deviations, candidly. None blocks closing. These are understated or stale:
- The deviation entry "cycle 4: red 5b3292e trimmed ... no cap change, as with red 1b and red 3b" is contradicted by the later cycle_count_ruling. Mark it superseded.
- `seats` stops at red 4b (18 entries). It is missing: red 5b3292e; green 2e56d9f; green 7c04e2e; red 4c 58b5c26 (with its delete-guard false positive); green 0000715; refactor fdfa821; fix 839380b; the re-gate ctx-5 (session cbd622dc); and the Muse trial voter as a seat. Each needs model, session and scan.
- evidence.json still says "no command source". Add the re-scan result (clean, other_tools 3, at 839380b) beside it; do not overwrite the original.
- Green commit rule: greens 7c04e2e and 0000715 name only their modules. Full suites ran on 2e56d9f and 839380b, not on those commits. The final state is covered (1643 OK). State it.
- Green 1's Co-authored-by trailer, and the fact that every commit from 2196c58 on is authored "Copilot": both recorded. They belong to card 275.

## 5. Follow-ups

Card 279: all three of its Done checks hold on HEAD (scope-4 says so, and my scan agrees for checks 1 and 3). Re-rank should close it as delivered by 276 and point to commits 25d50b8 to 0000715. Any remainder belongs to a new, narrower card (below), not to 279 kept open. Cards 277 and 280 to 283 stand as filed.

Uncarded defects, as card candidates:
- Muse's sandboxed shell does not complete a command. In capture a7505b9d, call 1 timed out at 10 s, call 2 hit an ACL publication lock timeout, and call 3 sat in approval_wait. This blocks any Muse build seat and any real executed-command log.
- decider.parse_vote turns a "NO OBJECTION" reply into vote "No" (decider-result-2.json line 35). A non-objection is stored as its opposite. The facilitator reads the first line instead, but the stored field misleads.
- The facilitator's window runner checks a literal first line. Muse prints two "muse:" lines after the answer in combined output, and before it with plain output (note muse_plain_output). Check whether card 281 covers this; if not, file it.
- locate_muse's injected root means either a home or a data root (smell noted at cycle 2). Small cleanup card.
- Retro items, not cards: lettered reds; resumed seats compacting near 128k, and auto picking a flash model; Grok plan-mode briefs.

## 6. Scope

The muse backend is enabled in core/backends.json, but it appears in no role fallback chain. facilitator.MOB_BACKENDS lists muse only so that a task-force card can name it. Nothing places it in a build seat, and no counted decision seat ran on Muse: scope-4's five seats were Claude. That fits D179 ("voting seats first, build seats later"). Because muse is in MOB_BACKENDS, a card could still name it for a build role, so the closing record should state the placement as trial voter only.

The shell that never completes does not matter for this card: a voting seat only reads, and the vote log has no shell call. It does matter before any build seat: see the sandbox card candidate.
