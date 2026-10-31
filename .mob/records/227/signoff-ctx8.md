# Card 227 sign-off (product manager, ctx-8, fresh context)

Seat: Claude. The 23:1x UTC probes in .mob/records/probes/2026-10-30.jsonl show Codex, Copilot, Grok and agy without usable quota. In-flight line committed first (1aa0fef, 23:11:15 UTC). Read: the card text (board/227), the Intent line and the product owner's ruling in record-in-progress.json, D161, the record (deviations, command_scans, cycles, the open direction question), .mob/records/trust-ladder.jsonl, review-ctx6.md and review-security-ctx3.md with their re-checks (both APPROVE), preship-gate.md (7c45132), the three turn records, and card 228 (board/228). Code: mob-defaults (C:\Users\ringe\mob-crosscli) at f8a45da. Changed no code. Ran one demo script from the scratchpad: C:\Users\ringe\AppData\Local\Temp\claude\C--Users-ringe\73298da3-f8ff-4a30-af55-b3e95ffa16f4\scratchpad\demo227_ctx8.py. It passes strings to the scanner and writes two fixture transcripts into a new uniquely named scratch folder (demo227-ctx8-<time>). No delete command was run and no test suite was run by this seat.

## Verdict: SIGN OFF, with F-071 closed in detection only

## Against the product owner's ruling

The ruling has four parts. Where each stands:

1. "The reply/command scan must block rm -rf (and equivalents like Remove-Item -Recurse -Force) on any variable or computed path." MET AS DETECTION, NOT AS PREVENTION. The scan now reads the commands a seat executed (every tool call with a command, from the session transcript), not only reply text, and flags a recursive delete on any variable, computed or unreadable path. It allows only a literal strict child of a scratch root. It runs after the fact and fails the record; it does not stop a command from running. Prevention before execution is card 228's opt-in hook, and the product owner has not answered whether "block" requires it (the open direction question, carried by the pre-ship gate item 10). Done check 1 names "through a hook or transcript", so the transcript route the card chose satisfies the check as written. That is a reading of the card, not an answer to the owner's word "block".
2. "Write the failing test first, reproducing tonight's exact slip." MET. Red 524171f reproduces the card 226 slip (SP=<scratchpad>; rm -rf $SP/mut 2>/dev/null; mkdir -p $SP/mut, recorded in the cycles with its transcript line). 46 of 132 tests failed by assertion; 86 born-green guards were shown red under six mutations. The green is 2fc4f98.
3. "Record the trust-ladder entry for it, or a stated reason none applies." MET, and twice. Card 226's slip has an entry (13:07:00Z). The red 1c slip during this card has its own (18:27:41Z). Both state "step none": Claude is the last-resort driver, already at Rung 1, with no rung below; the coaching is the scan, the ladder check and a seat brief that says how to refresh a scratch copy without a delete. `mob_process.py ladder` prints ok. The honest limit: "no step down" means the ladder cannot demote this driver, so the entry records and coaches; it does not prevent a repeat. The second miss came with a brief that said "NEVER execute any delete command", so the brief is not the control.
4. "Push on every commit ... Add a detector (unpushed commits older than ~5 minutes = FAIL)." MET. `mob_process.py unpushed` is built and called (run_audit, the facilitator's per-seat scan, which after green 3d watches the seat's repository and mob-defaults by default, and the audit's push-rule text check). The rule text is in the avowry agreement and the template. A commit unpushed over the limit is reported as FAIL naming the repository, commit and age.

## Against D161

D161's goal is a locked-down process, enforced by tooling and saved into the mob-defaults template. This slice does that: the scan, the ladder check, the unpushed check and the scan-limits and push-rule text checks are code, called from run_audit, and the template carries the rule texts. D161's second direction (describe, do not cite IDs) is followed here; IDs appear only in parentheses.

## The three Done checks

- Check 1 (the scan blocks recursive deletes on variable or computed paths in executed commands; failing test first; then Remove-Item -Recurse -Force, shutil.rmtree and equivalents): MET as detection. See the demo. Both reviews went from CHANGES REQUESTED to APPROVE after their findings (PowerShell script blocks, quote concatenation, command substitution, heredocs and more) were fixed red then green in cycles 1c to 1f. The adopted condition holds: a command absent from the reply text is caught, and scan-transcript exits 2 for no command source, never clean.
- Check 2 (trust-ladder entry for the slip): MET (above).
- Check 3 (push on every commit: rule text in the agreement and template, and a detector that reports FAIL for a commit unpushed about 5 minutes): MET. The reviewers' earlier finding that the rule-text check was built and never called is closed (green 3c, run_audit calls it).

## Demo (D150), against f8a45da

Persona: the facilitator scanning a returned seat. Real output of the script above:

- scan_command(..., ["scratch"]) flags the card 226 slip (`SP=/tmp/pad; rm -rf $SP/mut 2>/dev/null; mkdir -p $SP/mut`), the red 1c slip (`M=$S/mut-227-1c; rm -rf "$M" 2>/dev/null; mkdir -p $M`), `Remove-Item -Recurse -Force $p` and `rm -rf "$(pwd)/x"`, one finding each. It stays quiet on `rm -rf scratch/mut-1` (a literal scratch child) and on a bare `mkdir -p scratch/mut-2`.
- scan-transcript on a fixture transcript whose tool call is `M=$S/mut; rm -rf "$M"; mkdir -p $M` while the reply says only "Done. I prepared the scratch copy." exits 1: "demo: recursive delete on a variable or unresolved path: rm -rf "$M"". A fixture whose only command is `ls; mkdir -p scratch/new-1` exits 0: "clean: demo, 1 command(s) scanned".
- `mob_process.py ladder` on avowry and on mob-defaults prints ok (exit 0 each). `mob_process.py unpushed` on both prints ok (exit 0).

## The two things I was asked to weigh

1. "Block" sliced as detect-and-fail-the-record. This is the honest weak point. The product owner's reason was that a mis-quoted delete could take out a repository including .git. Detection after the fact does not stop that delete; it reports it afterwards. What limits the damage for now is the second half of the ruling: push on every commit, so a wiped repository loses at most about 5 minutes of work, and that half is real and detected. Together the two halves bound the harm; neither prevents the first delete. I sign off because the card as written asked for the scan to see executed commands "through a hook or transcript" and chose the transcript, the slice, the task force and both reviewers read it that way, and the preventive piece has its own card with its own Done check (card 228, check 3). My sign-off is not an answer to the owner's direction question; it stays open and I carry it to the owner unchanged.
2. A seat ran a real recursive delete on a variable path during this card, and the facilitator noticed only 2 h 40 min later. Two things follow. The scan works: it flagged the exact slip the card exists for, on the card's own seat. And detection is only as good as the reading: the facilitator recorded "findings" without reading it. The recorded fix is a habit (read and classify every findings result before the record commit); it is not enforced. 6 of the 7 findings entries in the record are false positives (a delete command named inside a Python string or test data), which is the noise that makes unread findings likely; card 228 owns that fix (item 8). Until it lands, real alerts will keep arriving mixed with noise and the delay can recur. I do not count this as a blocker for 227, because the failure was in the reading, not the scan, and the record states it.

## Can F-071 be called closed?

Only closed in detection. The accurate sentence for the ledger: the scan now sees executed commands and flags a recursive delete on a variable or computed path, and unpushed commits older than about 5 minutes fail the check; a delete is still not stopped before it runs, and a flagged result still depends on someone reading it. It should not be recorded as "variable-path recursive deletes are blocked", which is the card title and the owner's word. If the owner answers that prevention is required, F-071 stays open until card 228's hook (check 3) is shipped and a human installs it. I recommend the facilitator say "detected" when reporting this card to the owner.

## Deviations, stated plainly, and whether any blocks

- The red 1c seat's real delete and the 2 h 40 min unread finding: in the record's deviations and the ladder; does not block, with the limit above.
- The card exceeds the per-card guide by one increment and one concern (the owner's ruling put the scan and push together); every commit stays within D136 (23 of 23 within 15 files and 400 lines per the gate).
- Born-green guards without their own red commit, and several record-text slips (a stated "+20 lines" that is +19; a red body citing the wrong line; a commit title that hides a swept ladder entry; the intent line committed before its confirmation): disclosed, none rewritten. Do not block.
- Mutation survivors named in the record (for example a non-integer card field in the ladder is untested): none is on a Done check.

## Carried, not failed

- The direction question for the product owner (whether "block" needs prevention before a command runs).
- The command scans of ctx-7 (delivery lead) and of this seat (ctx-8) are run by the facilitator on return, per the stand-down 2 step.
- avowry CI: success on e5105b1, 5c93c4b and 130a2c2 (read by the facilitator at about 23:10 UTC); five later runs were in progress. Not read by this seat (no gh call).
- Owned by card 228, so not covered today: hidden, encoded and non-shell delete forms; scripts a seat writes and then runs; other backends' transcripts; the opt-in hook; the string-literal false-positive class; automating the subagent scan; detector residuals (a branch with no upstream, a local-path origin, rewritten commits). Today the scan is a detector for ordinary command forms at command position, as both reviewers confirmed; it is not a wall around an unusual command.

## The card's metric

Unflagged recursive deletes on variable paths: target 0. The one real slip in the card's own run was flagged (0 unflagged) but read late. Commits unpushed for more than about 5 minutes: target 0; unpushed prints ok on both repositories. Counter-metric, false positives on literal scratch paths through safe_delete: none seen; false positives on deletes only named, not run: 6 of 7 findings entries, owned by card 228.

## Not verified

A full discover run at f8a45da by this seat (the record holds one: 1253 OK, 1 skip, at f8a45da); a live hook (none exists yet); avowry CI for the last five runs.
