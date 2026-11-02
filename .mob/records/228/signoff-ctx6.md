# Card 228 sign-off (decider / sign-off, ctx-6, fresh context)

Seat: Claude. Probe: 2026-11-02 08:39 UTC (checked again at 09:05 UTC). Read: board/228, turn-slice-architect.md, the six decision windows `.mob/records/decisions/228-*.json` (taskforce, slice, agy-seats, c1-scope, c2-scope, gate-fix), record-in-progress.json, preship-gate.md (be1bc1a gate; 679144a re-check), the cycle table in the gate, mob-crosscli git log (`e4f7c29` is HEAD, 0 ahead of origin/main). Ran `python scripts/mob_process.py check`: `mob-process: OK (101 closed cards, 10 records, 91 legacy)`. Changed no code. No delete command, no git stash, nothing under .mob/audits/. Left .mob/records/228/body.tmp alone. Format model: .mob/records/237/signoff-ctx6.md.

Intent (D165): confirm that card 228 ended inside what the team decided and that its record is honest enough to close; sign the corrective for F-071, not the whole scan.

Assumes (D166): the reviewer's reproductions are right (the three ssh-agent bypasses exit 2 on `e4f7c29`; 24 scan and hook modules, 610 tests OK), since I did not re-run the hook or the suites. Probe: the red and green turn records, the cycle table and the commits agree on every number I could cross-check; I did not run the 30-minute full suite.

## Verdict: SIGNED OFF (the corrective and its record), with conditions the closing record must carry

Conditions, each a fix to the record and not to the code (details in sections 3 to 5): (1) the full suite on the final code was never green, so say so and file the flake; (2) say twelve agy decision seats, not nine; (3) refresh the open points that the fix has settled; (4) fill the token gaps or state them; (5) file F1 to F6 and name the Codex reader (F2) as the open part of Done check 2.

## 1. Done checks against the gate's evidence

| Done check | Verdict | Evidence |
|---|---|---|
| 1. Hidden, encoded and non-shell deletes reported from executed commands | MET | Gate: seven cycles (`63e4f61` to `844f083`, parity guards `9fe71ca`); the 25 real seat transcripts re-run at HEAD give no new false alarm beyond the recorded ones. Path forms were moved to F4 by window 228-c1-scope. |
| 2. File contents a seat writes then runs are scanned; other backends' transcripts read where they exist | MET for files and Copilot; PARTLY MET as the board words it | Files `5ffc494`, reads are not runs `f6a76f5`, Copilot reader `bc0881e`, live Copilot wiring `f6a76f5`. Codex rollouts exist on this machine and are not read: the adopted slice (window 228-slice) put them in F2. The board text still says "Codex and other backends'", so the open part is carried, not hidden. Grok and agy have no transcript to read. |
| 3. An opt-in hook blocks a flagged command before it runs, installed only by a human | MET after the fix | Gate had it NOT MET (B1: the hook allowed `eval "$(ssh-agent -s)"`, bypassable by a shell function or PATH shadow). Re-check on `e4f7c29`: all three bypasses exit 2, the control `ssh-agent -s` exits 0, `_SSH_AGENT_EVAL` is gone. Installed by nothing (`--print-snippet` writes nothing, `3b`). |

Re-check item 3 is the limit of this: the developer's full suite on `e4f7c29` is `Ran 1450 tests, FAILED (failures=1, skipped=1)`; the reviewer ran the 24 modules (610 OK) and the new gate-fix modules (73 OK) and the loop-state module three times (8 OK each). The last full pass was `743a4a1` (1434 OK). So I do not write "full suite OK on e4f7c29".

## 2. The team decisions on scope, and whether they were followed

| Window | Decision | Followed? | Evidence |
|---|---|---|---|
| 228-taskforce | Six-seat task force (ctx-1 Copilot, ctx-2 to ctx-6 Claude); split proposed into follow-ups | Yes | task_force in the record has the same six; ctx-6 is this seat. All five polled NO OBJECTION with a cite; the architect's first cite was a placeholder and was re-asked. |
| 228-slice | Slice 639c5e2: three criteria, Copilot has a source, a no-source seat is a gap that blocks ship, F1 to F6 split | Yes, with overruns covered below | Every red directly precedes its green. Slice said one cycle per criterion; actual counts are in the next block. |
| 228-agy-seats | agy takes decision seats only; build seats stay on Claude and Copilot until an agy reader exists | Yes on build seats | Every red and green is Claude; agy appears only as voters. One part I could not verify: item 3 (the facilitator reports the build-seat deferral to the product owner in plain text) is not shown in any record I read. |
| 228-c1-scope | Option (b): one capped cycle 1g (I and M ZOMBIES), no further criterion-1 cycle, path forms to F4 | Yes | 1g `9fe71ca` came back born green and stands as guards; no criterion-1 commit after it (gate). Note: F4 as worded in the slice names symlinks, junctions, trap, wrappers; when filing, copy in the slice's "Path forms" list (turn-slice-architect.md, line 65) so the routing is not lost. |
| 228-c2-scope | Option (a): one last criterion-2 cycle 2c (reads are not runs; Copilot cwd and start time) | Yes | `3d63e09` and `f6a76f5`; no criterion-2 commit after. |
| 228-gate-fix | Option (a): one fix cycle (ssh-agent allow out, Copilot tolerance to 60 s), then the gate re-checks only those | Yes | Red `52451b6` and green `e4f7c29`; the re-check (679144a) covers exactly those items. 5 files, 174 inserted, 8 deleted; under D136. |
| Stand-down 1 | Criteria 2 and 3 get at most two cycles; a third needs a scope window; 2b adds cwd resolution | Yes | Criterion 2 had three (third by 228-c2-scope); criterion 3 had two plus the fix (the third by 228-gate-fix). 2b carried the cwd resolution. |

Increments: criterion 1 seven (1a to 1g, 1g born green), criterion 2 three, criterion 3 two, plus the gate fix. That is twelve, thirteen with the fix, against a three-increment rule and a slice of three. The re-check says "fourteen"; by the cycle table it is thirteen (twelve plus the fix). Each overrun has a window or stand-down 1 behind it, but only criterion 1's cap was written before the overrun happened: the auditor's FLAG caught the first, and the later two were decided at the boundary they hit.

## 3. Open items, and whether they are carried honestly

| Item | State in the record | My verdict |
|---|---|---|
| Flaky `test_loop_state.LoopStateTests.test_lock_released_when_holder_exits` | In `open_points` (once in the full suite on `e4f7c29`; passes alone; 0.2 s sleep) and in the plan "file as a bug" | Honest. Not filed yet. File at close with the test name, the 0.2 s sleep and the 1450-test run. |
| Follow-ups F1 to F6 | `deviations`: "not filed yet, filed at card close" | Honest. 15 items across six cards. Not filed; they have no owner until filed. Include F2 (Codex reader, the open part of Done check 2) and the agy reader in F1 or F2. |
| False-positive classes for F3 | `working_state.followup_F3_additions` has three: `python -W error`/`-X`/`-m` read as a run file; delete text passed as a string to `scan_command` (rmtree, plain and base64 forms) | Incomplete by one: the gate's N1 (`python -W error::DeprecationWarning` read as a run, in green 1c, recorded clean earlier because it predated green 2a's file-run scan) is in `command_scans` but not in the F3 list. Add it. Also carry: the early "clean" entries (1a to 1f) were taken with the code of their day. |
| Gate N1 | In `command_scans` as "finding, false positive" | Carried; add to F3 list (above). |
| Gate N2 (ctx-1 Copilot entry stale) | The entry is now `clean` with the Copilot reader (be1bc1a) | Closed. |
| Gate N3 (twelve increments, unnamed overruns) | `deviations` has the criterion 1 overrun and a separate "12 increments" entry | Carried. Correct "fourteen" to thirteen. |
| Gate N4 (`fa2351f` 427 lines vs 400) | `deviations` has it | Carried. |
| Gate N5 (Codex deferred; F1 to F6 unfiled) | `deviations` has F1 to F6 | Carried; Codex named here. |
| Gate N6 (Copilot locator) | `open_points` still says the 10 s tolerance fails live | Stale. Fixed by `e4f7c29` (60 s). Replace with: fixed, proven only on 20 recorded offsets (13 to 24 s) and synthetic tests; check on the first live `run_backend` Copilot seat. |
| Gate N7 (counts differ for 1c, 1d, 1e, 2b) | Not in the record | Explanation only, no action; one line at close. |
| Gate N8 (agy seats unscanned) | `command_scans` last entry: "nine agy decision seats (228-agy-seats, 228-c1-scope, 228-c2-scope)" | Wrong count. Window 228-gate-fix also seated three agy voters (`w228gfcp1-pmready` and the QA and reviewer seats), so it is twelve, across four windows. The window accepted agy for decision seats as a class (voter only reads and answers), so the acceptance covers all twelve; the record must say twelve. Stand-down 1 also says "nine ... in 228-agy-seats and 228-c1-scope", which is six. Nothing else changes. |
| Gate's conflict `open_points[2]` (ssh-agent) | Still listed as open | Stale. Resolved by window 228-gate-fix and `e4f7c29`. Mark closed. |
| Other undecided gaps | 20 Copilot decision attempts "unscanned, 0 commands, empty reply" (gate checked: none ran a command); this seat and the gate's own transcript | Carried. The facilitator scans ctx-4's gate and re-check transcript and this seat's transcript when each ends. |

## 4. Token-usage evidence

What exists in record-in-progress.json `token_usage`: 29 rows. One is the Copilot ready seat (225.2k in, 188.2k cached, 7.1k out, 0.25 premium). The other 28 are Claude build and gate seats, each a single `subagent_tokens` figure from the Agent completion report; they sum to 2,716,966 (mean about 97k per seat; range 59,848 to 167,920). Rows run from the slice (122,505) through green gate-fix (73,078), including the ssh-agent ruling and the pre-ship gate.

What is missing, so the closing record must not call the card's usage complete:
- The gate re-check seat (ctx-4, second run), this sign-off seat (ctx-6) and the facilitator. None has a row.
- The decision-window voters: 22 Claude voter seats (five each in taskforce and slice, three in each of the other four windows), 12 agy seats and the Copilot attempts that returned nothing. No figure for any of them. agy and Copilot give no in, out or cache split in the record.
- A split into input, cache read, cache creation and output: the 28 figures are one number each, so they cannot be compared with card 237's counted-token method.
- The full-suite runs (about 30 minutes each) are time, not tokens, and are not counted anywhere.

So the figure 2.72 million is a floor for the 28 build and gate seats, not the cost of the card.

## 5. Retro question: was the card worth its cost?

It took twelve increments plus a fix, against a three-increment rule: four times over on increments, and each overrun was decided after the fact except the cap on criterion 1.

For the retro, the evidence on both sides:
- Worth: the gate found a real bypass (B1: a shell function or PATH shadow turned an allowed `eval` into a free delete), and a live-Copilot locator that would have failed on all 20 recorded sessions. Both are fixed and re-checked. The D169 reason (a mis-quoted delete wiped a home folder on 2026-10-08) is the cost the card is buying down.
- Cost: at least 2.72 million subagent tokens on 28 seats, plus unmeasured voter, gate and facilitator usage; about 12 hours from the task-force window (20:06 UTC) to the end of the gate-fix window (08:00 UTC); most of every green went to the 30-minute full suite. Criterion 1 took seven increments and its last (1g) was born green, so it added guards and no behaviour. The suite never ran green on the final commit.
- Not answered: what each of the extra increments bought. No record counts how many of the 1a to 1f forms have ever appeared in a real seat transcript; all 25 real transcripts gave no real delete, only false positives. If the answer is none, the forms were insurance, and the retro should price that insurance against the three-increment rule explicitly.
- What would have saved cost: a written cap on increments per criterion at the slice (the retro already notes it), and the green commit-then-suite rule used from green 3a (auditor FLAG on 2c), which should have been the default from 1a.

Retro pick I would put forward: write the increment cap into the slice template as a required line, and require the slice to say which hostile forms are insurance and which have been seen in a real transcript.

## Carried, not failed

- Close: file the flaky loop-state bug and F1 to F6 (with F2 and the path forms named); add N1 to the F3 list; correct the agy count to twelve and "fourteen" to thirteen; mark the ssh-agent and locator open points closed or fixed.
- Close: token gaps in section 4, filled from the Agent results and usage lines or stated as missing.
- Facilitator: scan the transcripts of ctx-4 (gate and re-check) and ctx-6 once they end; run `python scripts/mob_process.py check` again after the closing record is written.
- The first live Copilot seat is the proof of the 60 s tolerance.
