# Card 210 sign-off (decider / sign-off, ctx-6, fresh context)

Seat: Claude. Probe: 2026-11-02 16:45 UTC. Read: board/210, turn-slice-architect.md, turn-green-dev-2.md and turn-green-dev-3.md, the two decision windows `.mob/records/decisions/210-taskforce.json` and `210-slice.json`, record-in-progress.json, preship-gate.md (HOLD, then the SHIP re-check, 3d03129), board/268 and board/254, and the mob-crosscli git log (`c633d3a` is HEAD, 0 ahead and 0 behind origin/main). Ran `python scripts/mob_process.py check`: `mob-process: OK (102 closed cards, 11 records, 91 legacy)`. Ran four record modules at `c633d3a` (test_mob_process, test_record_task_force_probes, test_record_cycles_evidence, test_record_members_retro): 170 tests OK. I did not run the 25-minute full suite. Changed no code. No delete command, no git stash, nothing under .mob/audits/. Left .mob/records/228/body.tmp alone. Format model: .mob/records/228/signoff-ctx6.md.

Intent (D165): confirm that card 210 ended inside what the team decided and that its record is honest enough to close; sign the corrective for MD-003 (the record check trusting a present value), not the mob_summary failures.

Assumes (D166): the facilitator's plain-shell run (1545 OK on `c633d3a`) and the gate's two-shell reproduction of the 2 failures are as written, since I did not re-run the suite or `tests.test_mob_summary`. Probe: the cycle table, the commits and the turn records agree on every hash and every count I cross-checked; the mob_summary files are untouched by any card 210 commit.

## Verdict: SIGNED OFF (the corrective and its record), with conditions the closing record must carry

Conditions, each a fix to the record or a filed item, not to the code: (1) name the line-ending churn in green 3 (section 3) and file it; (2) file card 268's dependency honestly as the one open red in the full suite; (3) state the token gaps (section 5); (4) the facilitator scans ctx-4's and this seat's transcripts and runs `check` again after the closing record is written.

## 1. Done checks against the gate's evidence

| Done check | Verdict | Evidence |
|---|---|---|
| 1. Non-empty task force and probes: `"task_force": []` and `"probes": {}` refused; Codex, Copilot, Grok and agy each need a probe with a time; a Claude seat needs none | MET | Red `572eb4b` (54 failures, 4 errors on committed code), green `690975a` (4 modules, 129 OK; full suite 1475 OK, 1 skipped). Commit subjects state the wrong-type refusals (list or string), the claude-optional rule and that other names are ignored. |
| 2. Cycles derived from MOB.md `#### Cycle N` headings and the seat log, never from the self-reported field | MET | Red `edc0837` (41 of 42 failed), green `1baece8` (5 modules, 171 OK; full suite 1517 OK, 1 skipped). Body names the three refusals: a mismatch, an unverifiable claim, a wrong-typed field. The facilitator now writes the heading (`facilitator.py`, one line). |
| 3. Per-member checks and a real retro: role, why and context per member; the template roster refused; a Claude seat needs `quota_reason`; `flow_metrics` and `practice_metrics` non-empty objects | MET | Red `242677e` (56 failures, 7 errors, all in the new module), green `c633d3a` (6 modules, 199 OK; full suite 1545 OK, 1 skipped). One wording point: the board says each member needs a domain; the commit says "a domain key too when present". That follows the slice's answer to T2 (the facilitator's `write_record` writes no domain key, so a hard domain rule would reject an honest record). The slice was adopted by window 210-slice, so I treat the board wording as settled by that window; the record should say so in one line. |

The gate's HOLD (2 failures in `tests/test_mob_summary.py`) was a branch-level finding, not a Done check. The re-check shows no card 210 commit touches that test file or the publish code, the 2 failures reproduce in the Copilot CLI environment directly and through `cmd /c`, and the facilitator's plain-shell full run on `c633d3a` is 1545 OK. That is a re-check by the facilitator's run plus the gate's own two reproductions; the gate did not itself see a green full suite. I accept it, with the weakness stated in section 4.

## 2. The decisions, and whether they were followed

| Decision | Followed? | Evidence |
|---|---|---|
| Window 210-taskforce (task force adopted, no conditions; a reviewer seat named "Independent reviewer (audit lens)", not auditor) | Yes | The record's `task_force` has the same six contexts (ctx-1 Copilot ready, ctx-2 slice, ctx-3 red, ctx-5 green, ctx-4 Copilot gate, ctx-6 this seat). The build seats are Claude subagents with distinct contexts; the ready and gate seats are Copilot, as the window said. |
| Window 210-slice (adopted, no conditions): three criteria, T2 answered from `write_record` | Yes | Every red directly precedes its green (below); T2's answer shaped Done check 3. |
| D175 caps, 1 + 1 + 1 | Held | The slice carries `Cap: criterion 1/2/3, at most 1 cycles` (lines 31, 54, 78). Git shows exactly one red and one green per criterion: `572eb4b`/`690975a`, `edc0837`/`1baece8`, `242677e`/`c633d3a`. No fourth cycle, no stand-down (`stand_downs` is empty). Contrast card 228: thirteen increments against three. |
| D157 order: green n before red n+1; "Criterion: n" in each red body; greens pushed at once | Yes | Order in the log: 572eb4b, 690975a, edc0837, 1baece8, 242677e, c633d3a. Bodies carry `Criterion: n`. Pushes 2 s, 9 s and about 1 min after the tests passed. Origin/main equals HEAD. |
| Green commit rule: a green commits on its own tests plus the named modules, then the full suite on committed code | Yes | Each green names its modules (129, 171, 199 OK) and records the full suite on the committed hash (1475, 1517, 1545, each 1 skipped). |
| Refactor line for each green | Present | Green 1 and green 2: "none needed" with reasons (turn-green-dev-2.md line 16). Green 3: "none needed" with a reason (turn-green-dev-3.md line 13). The reasons are written, so the rule is met; the content of the third is in section 4. |
| MD-003 subject token (F-043) | Yes | Every commit subject says "MD-003". |

## 3. Invariant: a record that passed before still passes

Held in the way the card states it. Evidence: `check` passes on the live records; the four record modules pass (170 OK); the full suite was 1475, 1517 and 1545 OK after greens 1, 2 and 3.

Did the fixture changes in greens 2 and 3 weaken anything? No, with the following facts (checked in the diffs, not taken from the gate):
- Green 2 (`1baece8`) changed two test helpers, `run_record` in `tests/test_mob_process.py` and in `tests/test_record_task_force_probes.py`: each now writes a MOB.md containing `#### Cycle 1 roles`, because those records claim cycle 1 and `main` now always supplies evidence. Three lines each. The fixture makes the claimed number agree with the evidence; it removes no assertion and relaxes no check. (The red commit `edc0837` also touched `tests/test_mob_process.py`, 3 lines.) Before green 2, a record claiming cycles with nothing to show them passed; after, it is refused as unverifiable. That is the intended tightening, and the fixtures were adjusted to stay honest, not to be accepted by a weaker rule.
- Green 3 (`c633d3a`) changed no test file at all. The gate's statement that green 3 "reuses the existing blank helper" and keeps checks strict is true, but "fixture changes in green 3" is not: the stat is `mob_process.py` only. The fixtures that matter for criterion 3 live in the red module (`test_record_members_retro.py`, 314 lines added in the red).
- One check was honestly narrowed in the commit, not weakened silently: the member check needs a domain key only when present (section 1, Done check 3).

**New finding the gate did not report: line-ending churn in green 3.** `mob_process.py` was LF-only at `1baece8` (1010 lines, 0 CRLF) and is CRLF-only at `c633d3a` (1063 lines, all CRLF). `git show --stat c633d3a` reads 1063 insertions and 1010 deletions; with whitespace ignored it is 57 insertions and 4 deletions. The gate says "only `mob_process.py` changed" and does not mention this. Effects: (a) the behaviour is correct (the four modules (170) and the full suite (1545) OK on CRLF); (b) the diff a reviewer sees is 2073 lines instead of 61, which hides the real change from `git blame` and from the 400-line rule (D136) in the way card 228's `fa2351f` (427 lines) did; (c) the file now differs from the other `mob_process.py` history. The green 3 record does not say it chose CRLF. I do not treat it as a reason to refuse sign-off, because it changes no behaviour and the real change is small and checked. It must be recorded and a decision made (restore LF, or add `.gitattributes`). This is a retro point and a filing, not a hidden fix.

## 4. Open items, and whether they are carried honestly

| Item | State in the record | My verdict |
|---|---|---|
| Card 268: the 2 `test_mob_summary` publish tests fail in the Copilot CLI environment | In `preship_gate_recheck`, `open_points` and board/268 (bug, Draft; "passes in every shell") | Honest, and filed. Two cautions. The gate's re-check evidence for "plain shell passes" is the facilitator's run, not the gate's. And until 268 is fixed the full suite is not green when started from a Copilot seat; 216 depends on it. The `open_points` entry for 268 already carries the correction (both directly and through `cmd /c`, not only through `cmd /c`); board/268 still words its Given as `cmd /c` or bash, so refine it at its ready step. |
| `BLANK_WORDS` repeats the facilitator's placeholder set (`TASK_FORCE_PLACEHOLDERS`) | In `refactor` ("left for the retro because merging means editing facilitator.py") and turn-green-dev-3.md | Honest. Two lists drift. For the retro: name who owns the single list, and file it if the retro does not take it. |
| The Size-line measure deviation | `deviations` and `open_points`: briefs asked whole-file `wc -c`; D168 C2 measures the bytes before the Size line | Honest, and the handling is stated (briefs use D168 C2 now). Add it to the retro question about the brief template, since this card's slice used the whole-file measure too. |
| The gate's `cmd /c` false positives (3 "unreadable file" findings: `cmd /c "cd /d ... && python ..."` and `git -C ... --format=/` read as a run of a file named by the quoted string) | In `command_scans` as "findings, false positives", with the raw lines; routed to card 254 | Honest. Board/254 exists (the delete scan cuts its known false positives). Confirm the three raw lines are in 254's reproduction list. No delete was executed. |
| ctx-1 command scan | Gate says "pending"; the record shows `clean` (1 command, read with the Copilot reader) | The gate line is stale. The record is right. |
| The probe commit before green 2 labelled 14:22 UTC when the probe ran at 14:11 | In `deviations` | Carried. |
| Scans still to do | ctx-4 (gate and re-check) is scanned (3 false positives, no delete); the re-check run of ctx-4 and this seat are not | The facilitator scans them when each ends. |
| The record `status` is "in progress" | Expected until the closing record is written | Carried. |

## 5. Token-usage evidence (`token_usage` in record-in-progress.json)

What exists: 10 rows. Seven Claude subagent rows, each a single `subagent_tokens` figure from the Agent completion report: 149,486 (slice), 99,354 (red 1), 78,855 (green 1), 111,103 (red 2), 96,199 (green 2), 111,177 (red 3), 87,626 (green 3); they sum to 733,800. Three Copilot rows with usage lines: ready (612.4k in, 560.6k cached, 11.3k out, 0.25 premium, 1m 37s), gate (1.5m in, 1.4m cached, 14.2k out, 0.25 premium, 28m 24s) and gate re-check (339.1k in, 319.7k cached, 10.3k out, 0.25 premium, 8m 26s).

What is missing, so the closing record must not call the usage complete:
- This sign-off seat (ctx-6) and the facilitator. No row.
- The decision-window voters in both windows (210-taskforce and 210-slice). No row at all, and none for any attempt that returned nothing.
- An input, output and cache split for the seven Claude rows: each is one number, so it cannot be compared with a counted-token method. The 733,800 is a floor for the build seats, not the card's cost.
- The full-suite runs (25 minutes for the gate's run) are time, not tokens, and are not counted.
- The reason the Copilot rows differ in "premium" units (0.25 each) from the Claude token counts is not stated; the record should say the two are not additive.

## 6. For the retro

- Worth: a clean 1+1+1 under D175, the first slice to carry the cap lines, and a corrective that closes the loopholes in the way the card names. Compare card 228, which ran thirteen increments.
- Cost to look at: the gate ran a full suite from the wrong environment and issued a HOLD that a plain-shell run overturned; the two re-checks (28 and 8 minutes of Copilot time) were spent on an environment problem. A pre-gate rule that the gate states which shell it runs the suite in would have saved one of them (this is the question card 268 and the `observations_for_next_stand_down` line already raise).
- Add to the retro list: the line-ending rewrite in green 3, `BLANK_WORDS`, the Size-line measure, the cmd /c false positives (254).

## Carried, not failed

- Close: file the line-ending churn (restore LF or add `.gitattributes`); record the domain-wording point in one line; fill or state the token gaps in section 5; mark the gate's "ctx-1 pending" line as superseded by the record.
- Facilitator: scan the transcript of ctx-4's re-check and this seat once they end; run `python scripts/mob_process.py check` after the closing record is written.
- Card 268 stays the open red in the full suite; card 254 carries the scan false positives.
