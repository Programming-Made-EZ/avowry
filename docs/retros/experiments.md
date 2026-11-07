# Retro experiments (D183 retro log)

Process improvements are not board cards (D183, working agreement line 36). They serve the story being worked. Each entry below was a board card until re-rank 21 (Decider D185) moved it here under the D183 sort (.mob/records/reranks/rerank-21-sort.md).

When a story's ready step matches an entry's trigger key, the story may take the experiment into its own diff and proves it by the story's outcome. The entry's Status then names the card. A retro may drop an entry with a reason. No new card is filed for an experiment.

Fields: Owner (a role), Proof (the kind of story and the measure), Trigger key (machine-matchable: a group key, and a file path or card tag where one applies), Source (the sort's line), Status (open, taken by card N, or dropped by retro).

## F-008 (FLAG): corrective owner

- Owner: the audit lens (independent reviewer seat). The external auditor is never a mob seat; the audit lens points the auditor at this section in the next audit brief.
- Corrective path: RX-206, RX-207, RX-220, RX-221, RX-222, RX-223, RX-224, RX-225 (card 205 is Done).
- Trigger: the next unattended loop run, or the next audit that finds a file and register mismatch. .mob/findings.json does not exist yet (card 206 was to create it), so this section is F-008's owner of record until it does.
- Guard (D182, D185): an audit that reports F-008 as unowned counts as a rise in findings.

## Trigger: Next unattended loop run (`loop-run`)

<a id="rx-220"></a>
### RX-220: The loop runner starts a corrective card while a finding blocks

- Owner, proof and trigger (from the sort): Architect; proof: an unattended run of a corrective card proceeds; measure loop stops per card; trigger: next unattended loop run
- Trigger key: `loop-run`
- Source: card 220, re-rank 21 sort: F-008 (a FLAG: missing detector) runner gap; no wrong run recorded.
- Status: open

<a id="rx-221"></a>
### RX-221: The loop runs a card in the repository its line names

- Owner, proof and trigger (from the sort): Architect; measure commits in the wrong repository = 0; trigger: next unattended loop run
- Trigger key: `loop-run`
- Source: card 221, re-rank 21 sort: Runner gap found by reading code; cards are run by hand, never observed.
- Status: open

<a id="rx-222"></a>
### RX-222: The loop wires retro and re-rank steps and stops when one fails

- Owner, proof and trigger (from the sort): Architect; measure a failed ceremony stops the loop; trigger: next unattended loop run
- Trigger key: `loop-run`
- Source: card 222, re-rank 21 sort: Exit codes ignored, found at slice; no incident.
- Status: open

<a id="rx-223"></a>
### RX-223: The loop takes the per-card retro from every role

- Owner, proof and trigger (from the sort): Agile coach; measure facilitator minutes per retro; trigger: next unattended loop run
- Trigger key: `loop-run`
- Source: card 223, re-rank 21 sort: Hand-run today; an improvement.
- Status: open

<a id="rx-224"></a>
### RX-224: The loop re-ranks Ready through the PM and a Decider

- Owner, proof and trigger (from the sort): Product manager; measure hours from Done to next card; trigger: next unattended loop run
- Trigger key: `loop-run`
- Source: card 224, re-rank 21 sort: Hand-run today; an improvement.
- Status: open

<a id="rx-062"></a>
### RX-062: Atomic runs: fresh worktree, landed or discarded

- Owner, proof and trigger (from the sort): Architect; measure partial landings = 0; trigger: first story run unattended by the loop
- Trigger key: `loop-run`
- Source: card 062, re-rank 21 sort: Loop design (D22); improvement.
- Status: open

<a id="rx-065"></a>
### RX-065: Loop: run one card with a pinned toolchain

- Owner, proof and trigger (from the sort): Architect; measure toolchain-drift failures; trigger: first story run unattended by the loop
- Trigger key: `loop-run`
- Source: card 065, re-rank 21 sort: 2026-10-26 loop design (D23); card 208's runner now runs cards; improvement.
- Status: open

<a id="rx-068"></a>
### RX-068: Loop: liveness, publish, STOP and Needs-PO file

- Owner, proof and trigger (from the sort): Facilitator; proof: an unattended run writes a Needs-product-owner file when needed; trigger: as 062
- Trigger key: `loop-run`
- Source: card 068, re-rank 21 sort: STOP file and heartbeat exist (064, 208); the rest is improvement.
- Status: open

<a id="rx-070"></a>
### RX-070: Loop: run every card atomically

- Owner, proof and trigger (from the sort): Architect; proof and trigger as 062
- Trigger key: `loop-run`
- Source: card 070, re-rank 21 sort: Depends on 062; improvement.
- Status: open

## Trigger: First loop stop 'blocked' (`loop-blocked`)

<a id="rx-066"></a>
### RX-066: Loop: bounded retry, park and split

- Owner, proof and trigger (from the sort): Architect; proof: a failing story parks and splits, not stops; trigger: first loop stop "blocked"
- Trigger key: `loop-blocked`
- Source: card 066, re-rank 21 sort: Retry exists (run_loop max_attempts=3); park and split are improvements.
- Status: open

## Trigger: Next story touching the file named (`file`)

<a id="rx-269"></a>
### RX-269: mob_process.py keeps LF line endings

- Owner, proof and trigger (from the sort): Backend developer; proof: the next story touching mob_process.py adds .gitattributes and its diff matches the change; trigger: that story
- Trigger key: `file file:mob_process.py`
- Source: card 269, re-rank 21 sort: Real churn (mob_process.py is still i/crlf) but not a safety, vote or test defect.
- Status: open

<a id="rx-272"></a>
### RX-272: facilitator.py and test_card_mode.py keep line endings

- Owner, proof and trigger (from the sort): Backend developer; fold with 269 (facilitator.py still i/crlf); trigger: next story editing facilitator.py
- Trigger key: `file file:facilitator.py`
- Source: card 272, re-rank 21 sort: Sibling of 269; disputed for the final tree; no failing check.
- Status: open

<a id="rx-287"></a>
### RX-287: locate_muse takes one kind of root

- Owner, proof and trigger (from the sort): Backend developer; done inside the next story touching command_source.py
- Trigger key: `file file:command_source.py`
- Source: card 287, re-rank 21 sort: API cleanup.
- Status: open

<a id="rx-255"></a>
### RX-255: The delete scan covers the residual forms and path forms

- Owner, proof and trigger (from the sort): QA; proof: a story whose seat uses a listed form gets a red for it first; measure missed forms in scans = 0; trigger: first seat command in a listed form, or next story touching delete_guard.py
- Trigger key: `file file:delete_guard.py`
- Source: card 255, re-rank 21 sort: Uncovered forms enumerated at slice, none reproduced from a seat command.
- Status: open

<a id="rx-271"></a>
### RX-271: The loop and audit thresholds are parameters

- Owner, proof and trigger (from the sort): Architect; proof: a story that needs another threshold sets it without a code edit; trigger: that story
- Trigger key: `file`
- Source: card 271, re-rank 21 sort: Hard-coded values are a cleanup, not a wrong outcome.
- Status: open

<a id="rx-273"></a>
### RX-273: Each gate has a red run that proves its test

- Owner, proof and trigger (from the sort): QA; measure gates whose test passes with the gate removed = 0; trigger: next story adding a gate
- Trigger key: `file`
- Source: card 273, re-rank 21 sort: No gate shown to pass its test while broken; reviewer disputed.
- Status: open

## Trigger: Next Muse seat (`muse-seat`)

<a id="rx-280"></a>
### RX-280: Muse usage text reads as signed in

- Owner, proof and trigger (from the sort): Backend developer; measure Muse seats wrongly unavailable = 0; trigger: next story with a Muse seat
- Trigger key: `muse-seat`
- Source: card 280, re-rank 21 sort: Availability defect of a trial driver, not safety, votes or tests.
- Status: open

<a id="rx-285"></a>
### RX-285: Muse's sandboxed shell completes a command

- Owner, proof and trigger (from the sort): Backend developer; measure escalation requests = 0; trigger: next story with a Muse driver seat
- Trigger key: `muse-seat`
- Source: card 285, re-rank 21 sort: Reproduced (session a7505b9d) but fails closed: availability, not a safety outcome.
- Status: open

## Trigger: Next wiki-routed story (review at re-rank 23) (`wiki-story`)

<a id="rx-236"></a>
### RX-236: The process wiki closes the writer's seven content gaps

- Owner, proof and trigger (from the sort): Agile coach; proof: the next wiki-routed story closes the gaps it touches; measure wiki misses per card; trigger: next wiki-routed seat
- Trigger key: `wiki-story`
- Source: card 236, re-rank 21 sort: Seven minor content gaps; no failing check.
- Status: open

<a id="rx-238"></a>
### RX-238: The reshaped process wiki carries the slice limits and upkeep

- Owner, proof and trigger (from the sort): Agile coach; proof: a story whose seats read the wiki; measure token delta and upkeep minutes per card; review: re-rank 23
- Trigger key: `wiki-story`
- Source: card 238, re-rank 21 sort: D173 wiki follow-up; no defect.
- Status: open

## Trigger: Next template or new-mob story (`template-story`)

<a id="rx-216"></a>
### RX-216: The default templates carry the pending process rulings

- Owner, proof and trigger (from the sort): Agile coach; proof: the next new mob starts from the template with no hand-added ruling; trigger: next mob spin-up or template story
- Trigger key: `template-story`
- Source: card 216, re-rank 21 sort: Template content; D183 itself changes what it must say.
- Status: open

<a id="rx-267"></a>
### RX-267: mob-defaults docs contain no dates

- Owner, proof and trigger (from the sort): Agile coach; proof: the next story editing a template removes dates in what it touches; measure dated lines; trigger: that story
- Trigger key: `template-story`
- Source: card 267, re-rank 21 sort: Template wording principle; no defect.
- Status: open

<a id="rx-212"></a>
### RX-212: Every mob re-ranks at every loop boundary

- Owner, proof and trigger (from the sort): Product manager; measure missing re-rank notes over the next three stories; review: re-rank 23
- Trigger key: `template-story`
- Source: card 212, re-rank 21 sort: Done by hand already; template part belongs to 216.
- Status: open

<a id="rx-215"></a>
### RX-215: Every mob reviews each previous adjustment

- Owner, proof and trigger (from the sort): Agile coach; measure reviewed / prior adjustments at the next story retro; trigger: next story retro
- Trigger key: `template-story`
- Source: card 215, re-rank 21 sort: Ceremony improvement; template part belongs to 216.
- Status: open

## Trigger: Next live decision vote (`live-vote`)

<a id="rx-277"></a>
### RX-277: An empty decision vote opens a diagnosis

- Owner, proof and trigger (from the sort): Decider seat; proof: on a story whose window gets an empty or voteless vote, a diagnosis is filed and the model returns; measure empty attempts per window near 0 (D182 guard); trigger: first empty or voteless decision vote after 291 is Done
- Trigger key: `live-vote`
- Source: card 277, re-rank 21 sort: The empty-vote defects are fixed (229, check 2) or carded (291); what remains is a diagnose-and-rotate rule, not a wrong vote.
- Status: open

<a id="rx-265"></a>
### RX-265: Decision windows launch voters in parallel safely

- Owner, proof and trigger (from the sort): Backend developer; proof: a story with a multi-voter window; measure window minutes with seat-use.jsonl still valid; trigger: a window over its cap from serial launches
  - Model to assume (working agreement, 2026-11-04): voters run in parallel on the one checkout with no per-voter copies, worktrees or branches; writing is serial under the single-writer lock, and card 230's vote lock keeps HEAD fixed for the vote. decider.py today gives each voter a scratch clone (its Isolation tests); a parallel launch under this model drops those copies, and that is a change for the story that takes this experiment, not for card 230.
- Trigger key: `live-vote`
- Source: card 265, re-rank 21 sort: Speed improvement; no lost or corrupted vote named.
- Status: open

<a id="rx-213"></a>
### RX-213: The checker rejects an incomplete objection window

- Owner, proof and trigger (from the sort): Audit lens; measure incomplete windows found by audit = 0; trigger: first incomplete window an audit finds
- Trigger key: `live-vote`
- Source: card 213, re-rank 21 sort: D153 L3b checker rule; no incomplete window that passed is named.
- Status: open

<a id="rx-218"></a>
### RX-218: A mid-card retro removal does not drop the seat early

- Owner, proof and trigger (from the sort): Facilitator; proof: a mid-card "removed: retro" keeps the seat to the retro; trigger: the first such line written mid-card
- Trigger key: `live-vote`
- Source: card 218, re-rank 21 sort: Staffing loophole found at slice; no seat was dropped.
- Status: open

<a id="rx-225"></a>
### RX-225: Card mode reads only role lines as the task force

- Owner, proof and trigger (from the sort): Facilitator; proof: the next story's task force parses to its declared roles; trigger: a card refused for a non-role line
- Trigger key: `live-vote`
- Source: card 225, re-rank 21 sort: Misparse verified at slice, but it fails closed and is not a vote, safety or test defect.
- Status: open

## Trigger: Next audit finding (`audit-finding`)

<a id="rx-206"></a>
### RX-206: The findings file mirrors the audit register

- Owner, proof and trigger (from the sort): Audit lens; measure file/register mismatches per audit; trigger: next audit that finds a mismatch
- Trigger key: `audit-finding`
- Source: card 206, re-rank 21 sort: F-008 corrective (FLAG); no wrong audit outcome named.
- Status: open

<a id="rx-207"></a>
### RX-207: The findings file history rules are checked

- Owner, proof and trigger (from the sort): Audit lens; measure history-rule violations = 0; trigger: as 206
- Trigger key: `audit-finding`
- Source: card 207, re-rank 21 sort: F-008 corrective (FLAG); no named history rewrite.
- Status: open

<a id="rx-257"></a>
### RX-257: Git-state detectors catch unpushed and hidden work

- Owner, proof and trigger (from the sort): Audit lens; proof: a story in a side branch is flagged when unpushed; measure hidden work found by audit, not detector = 0; trigger: first audit finding hidden or unpushed work
- Trigger key: `audit-finding`
- Source: card 257, re-rank 21 sort: No named loss or incident for these states.
- Status: open

## Trigger: Every story (`every-story`)

<a id="rx-235"></a>
### RX-235: Green commit bodies carry a checked Proof block

- Owner, proof and trigger (from the sort): Audit lens; proof: every green of the next story carries a checked Proof block; measure greens without one = 0; trigger: next story's first green
- Trigger key: `every-story`
- Source: card 235, re-rank 21 sort: Missing Proof blocks (c941885, 9daab9f) are record hygiene, not a failing test.
- Status: open

<a id="rx-256"></a>
### RX-256: Command-scan reports are readable and trusted

- Owner, proof and trigger (from the sort): QA; proof: a story's command-scan entry renders and a later edit is flagged; review: re-rank 23
- Trigger key: `every-story`
- Source: card 256, re-rank 21 sort: Report plumbing; no wrong scan outcome named.
- Status: open

<a id="rx-078"></a>
### RX-078: Ready check: reject a role-only story

- Owner, proof and trigger (from the sort): Product manager; measure stories sent back for role-only wording; trigger: next such story
- Trigger key: `every-story`
- Source: card 078, re-rank 21 sort: Ready-check lint; improvement.
- Status: open

<a id="rx-072"></a>
### RX-072: Loop: pace the work against driver allotments

- Owner, proof and trigger (from the sort): Product manager; measure quota stops per card; trigger: next quota exhaustion
- Trigger key: `every-story`
- Source: card 072, re-rank 21 sort: Improvement.
- Status: open

<a id="rx-069"></a>
### RX-069: Loop: resume after a crash or reboot

- Owner, proof and trigger (from the sort): Backend developer; measure minutes lost; trigger: first reboot during an unattended run
- Trigger key: `every-story`
- Source: card 069, re-rank 21 sort: Improvement; no incident named.
- Status: open

<a id="rx-139"></a>
### RX-139: Loop step: re-probe unavailable drivers

- Owner, proof and trigger (from the sort): Facilitator; measure hours a recovered driver stays out; trigger: next driver quota-out
- Trigger key: `every-story`
- Source: card 139, re-rank 21 sort: Mob loop tooling; an improvement ("coach, don't bench").
- Status: open

<a id="rx-233"></a>
### RX-233: Evaluate side-by-side pairing tooling

- Owner, proof and trigger (from the sort): Agile coach; proof: a driver/navigator slice; measure rework cycles and tokens per slice against a solo slice; trigger: the first story planned for pairing
- Trigger key: `every-story`
- Source: card 233, re-rank 21 sort: A spike; no defect.
- Status: open

## Trigger: Next story retro (`story-retro`)

<a id="rx-275"></a>
### RX-275: Decide card 211's unproven defects; seat attribution

- Owner, proof and trigger (from the sort): Facilitator; proof: the retro decides close or card; trigger already met: card 229's sign-off says every subject commit is authored "Copilot" (D177 recurrence), so take it to the next story's retro
- Trigger key: `story-retro`
- Source: card 275, re-rank 21 sort: A pending decision, not a defect card.
- Status: open

## Added after re-rank 21 (no card number)

- Board scripts fetch the board once per run. scripts/board/edit_card.py (and set_fields.read_board) re-fetch the whole issue list with bodies, the project, its fields and all items on every edit, about 90 GraphQL points each; 27 edits spent the hourly budget during re-rank 21, while one paged fetch costs 3 points and a direct updateProjectV2ItemFieldValue mutation about 1 (the lean script rr21_apply_lean.py, recorded in .mob/records/reranks/2026-11-04-21.md). Owner: backend developer. Proof: the next story that edits many cards (a re-rank or a sort) applies them within the hourly budget; measure GraphQL points per edited card. Trigger key: `file file:scripts/board/edit_card.py`. Status: open.
- The decider stores a Codex seat's CLI banner and prompt echo after its vote (windows 290-taskforce and 290-slice; the facilitator trims them from window records). Owner: backend developer. Proof: the next story with Codex in a window stores only the reply; measure stored characters after the CITE line. Trigger key: `live-vote`. Status: open.

- A counted quorum for Decider results: one cited Yes passes when every other seat's reply is unread (card 290 sign-off). Owner: decider. Proof: the next story with a window where most replies are unread; measure counted seats per window against seats placed. Trigger key: `live-vote`. Status: open.

- decider.build_prompt asks for a CITE: today it says 'Nothing else' and counting depends on each proposal's own voting rule (card 290 sign-off). Owner: architect. Proof: the next story with a Decider; measure the share of replies with a CITE line. Trigger key: `live-vote`. Status: open.

- A time cap for seat turns that the runner enforces (card 290's first slice attempt ran 46 minutes against a 30-minute cap). Owner: facilitator. Proof: the next story; measure seat turns past their cap. Trigger key: `every-story`. Status: open.

- A measure-only premise probe before the slice (card 290 retro; D186, not adopted after three votes). The slice's premise "two lines in result_of" was false, D184 covers scope proposals only, and slice attempt 1 built for 46 minutes. Objections to meet before it goes to a vote again: bounded terms, one premise per row, probes rerun from a clean committed HEAD, normalised assertion-based expected results with a findings schema, and a check that every premise line has a row. Owner: agile coach. Proof: the next story whose slice rests on a premise about existing code; measure green stops caused by a false slice premise. Trigger key: `every-story`. Status: open.

- Re-ask only the seats a single condition touches. When a window's round 1 has no No and only one seat's condition, the facilitator folds the condition in and re-asks only that seat, plus any seat whose cited point the joined text changes; a full re-poll stays for a real No or for conditions from several seats that conflict. A non-verbatim cite is still re-asked alone (D171). Why: windows 290-taskforce, 230-taskforce and 230-slice each ran a full six-seat round 2 for one seat's condition, about 12 to 15 minutes each with Grok disabled; a single condition narrows the text the other seats already accepted. Owner: agile coach. Proof: the next card's decision windows; measure window wall-clock per card. Guard: decisions later reversed, and audit or sign-off findings traced to a skipped re-poll, must not rise. Trigger key: `live-vote` (the next card's decision windows). Status: in trial under D187 (2026-11-05) for the next three cards; their windows record who was re-asked.

- Each slice S-case gets its own red before the first green (card 230 retro, architect). Card 230's two full-suite stops (the smoke toy card after green 1; two older pins after green 3) came from behaviour the slice named in its S case, or that existing pins covered, which no red pinned before the green. Owner: architect. Proof: the next infrastructure card; measure full-suite stops caused by named slice behaviour. Counter-metric: red-to-first-green minutes. Trigger key: `every-story`. Status: open.

## Deferred by D184 (no card number)

- Built-prompt evidence (its SHA) before a live vote. Owner: architect. Trigger key: `live-vote`. Status: open.
- One seat policy, a per-card backend exclusion in the decision-seat gate, and a gate seat that is not the card's subject. Owner: decider. Trigger key: `live-vote`. Status: open.
- A probe-age alert before an in-flight line (two crossings on card 229). Owner: facilitator. Trigger key: `every-story`. Status: adopted as D188 (card 297 retro, trial for the next three cards).

## Added after card 297 (no card number)

- Seat-start commits match in-flight lines one to one (card 297 pick vote, audit lens): a seat started with no in-flight line is never counted by D188's metric. Owner: audit lens. Proof: the next story; compare red/green/ship commits with in-flight lines. Trigger key: `every-story`. Status: open.
- An integration-test voice for driver, probe, placement, command and commit together (card 297 sign-off condition 1; retro developer voice). Owner: QA. Proof: the next card that changes seat placement. Trigger key: `every-story`. Status: open.
- Monitors armed on a shared output file can report a false completion (card 297: a Monitor read an earlier command's output after a reboot). Owner: facilitator. Proof: the next story; count false completion events. Trigger key: `every-story`. Status: open.
- Trial dropping one voting seat in the windows of a bug card of this size (card 297 coach voice; no condition was raised in any window). Owner: agile coach. Trigger: a bug card whose task-force window raises no condition. Trigger key: `story-retro`. Status: open.

## Added after card 264 (no card number)

- A line-ending check at commit: a commit may not change an existing file's line endings (card 264: a seat commit rewrote scripts/mob_process.py and tests/test_mob_process.py as CRLF, a 3,089-line diff for 21 changed lines; four voices asked for it). Owner: backend developer. Proof: the next story; count whole-file diffs. Trigger key: `every-story`. Status: open.
- A hard timeout and process kill in the probe script, and refusing a probe line older than the last line of the file (card 264: the Codex half hung three times; a stuck background command committed a stale probe line five hours late). Owner: facilitator. Proof: the next story; count hung probes and out-of-order probe lines. Trigger key: `every-story`. Status: open.
- A "scope window" category in the closed list of window categories and an aggregate cap check in the window records (card 264: the category used was "mid-card addition" and the cap sum reached 5 against the D175 limit of 3). Owner: audit lens. Proof: the next story with a cap raise. Trigger key: `every-story`. Status: open.
- An evidence-integrity gate before the pre-ship gate: HEAD matches the card's last commit, a line-ending scan, duplicate or out-of-order record lines (card 264: the gate missed eight CRLF files and named a later HEAD; QA). Owner: QA. Proof: the next story; count integrity defects found by the gate against defects found by sign-off. Trigger key: `every-story`. Status: open.
- Run the full acceptance project before pushing a harness change (card 264: CI was red for about an hour on a guard test after only one class was run). Owner: facilitator. Proof: the next story with a harness change; count red CI runs. Trigger key: `every-story`. Status: open.

## Added after card 217 (no card number)

- A checker rejects a pre-ship gate record with no command output lines, and a check that "rerun" output is real (card 217: a "rerun" label cannot be verified from git; architect, audit lens, QA). Owner: audit lens. Proof: the next story with a gate; count gate records without output lines. Trigger key: `every-story`. Status: open.
- An evidence inventory before any scope window: list each claim a scope proposal rests on and the file or command that shows it (card 217 retro, agile coach; the scope windows of cards 264 and 217 each re-polled on a missing artifact or protocol). Owner: agile coach. Proof: the next story with a scope window; count re-asks and re-polls. Trigger key: `story-retro`. Status: open.
- A seat time cap the runner enforces, with the 90-minute overrun of card 217's cycle 3a (waiting for two 11-minute runs against a 60-minute cap) as new evidence (extends the card 290 entry). Owner: facilitator. Proof: the next story; count seat turns past their cap and caps raised outside a window. Trigger key: `every-story`. Status: open.
- A check that seat briefs for file-writing seats carry the LF instruction, and that the committed new files have no CRLF (card 217: the brief instruction worked; no new CRLF file). Owner: facilitator. Proof: the next story; count CRLF files. Trigger key: `every-story`. Status: open.
- A machine check of a D189 numeric-claims table: read the command column mechanically (card 217 architect voice). Owner: audit lens. Proof: the next story with a slice; count claims whose command does not run. Trigger key: `every-story`. Status: open.

## Added from the product owner's inbox, 2026-11-06 (no card number)

- Adopt the product owner's inbox (a file outside the repo, C:/Users/ringe/.mob-inbox/avowry.md) as the channel for product-owner decisions and process proposals: the facilitator reads it at every card and cycle boundary and at every retro, acknowledges each entry in the file (moved under ACKNOWLEDGED with a timestamp and where it went), routes process proposals here and product-owner decisions to the card or closure record they affect; GitHub issues stay bugs and stories only. Why: messages from Dispatch land only when a facilitator turn ends, so decisions sat undelivered for more than an hour. In use since 2026-11-06 (the first four entries were read and acknowledged on that day). Owner: facilitator. Proof: the next story; count decisions delivered within one cycle and entries left unacknowledged at a retro. Trigger key: `story-retro`. Status: open (the retro decides by Decider).
- How a Codex build seat's work is committed now that its shell works (card 297 machine fix, .mob/records/297/machine-fix-2026-11-06.md): Codex keeps .git read-only in workspace-write, so it can write files and run commands but cannot git add or commit. Options put to the product owner: another writer commits the Codex seat's explicit paths after each turn; Codex stays on read-only seats and voting; or the bypass flag that card 297 removed is allowed again. Owner: facilitator (decision: product owner). Proof: the first card with a Codex build seat. Trigger key: `every-story`. Status: open.


## Question from the product owner's inbox, 2026-11-06: which card format serves this mob best?
Not a ruling. The product owner asks for the team's own evidence-based answer, weighed at the next retro through Decider with fresh seats: keep the contract template (Shape, Actor, Given, Observes, Fails with), switch to Connextra stories with a named persona plus Gherkin scenarios (as play-tracker uses), or a hybrid (for example Connextra plus Gherkin for user-facing stories, the contract template for tooling). Suggested evidence: how well each drives outside-in TDD from acceptance tests, ZOMBIES coverage, persona and consumer-value clarity, how often seats misread or rework cards, review and audit effort, fit for tooling and contract cards. Report the decision, the vote and 3 to 5 lines of reasoning back through the inbox acknowledgement; a change is framed as a retro experiment with a review point.

## Proposal from the product owner's inbox, 2026-11-06: bring Muse into the driver rotation
Proposal, decided by Decider at the next boundary or retro. Evidence so far (.mob/records/303/driver-trials-2026-11-06.md): Muse 1.4.14 passes the one-word probe (07:57 UTC) and has a delete-scan command source, but on the card 303 slice brief it returned no reply text (run 1) and was cut by a timeout while still reading files (run 2, `--json`). Next trial: the same kind of brief with the card text inlined, at a seat where an empty reply costs nothing; if it delivers, weigh a fallback-order position (suggested: after Copilot, before Grok, which is out of counted seats until card 291) and a trial verdict under the coach-don't-bench rule. Quota context from the inbox (agy out until Nov 8, Codex 45% weekly at about 27% a day, Copilot 20% to Dec 1) argues for adding a driver; Grok's probe passed on 2026-11-06 but its seat failed on card 291's defect.

## Added from the card 303 retro (2026-11-06)
- **Checker for the inbox log (follow-up to D191; QA, audit lens, architect):** a committed script with failing fixtures that reconciles .mob/records/inbox-log.jsonl against the acknowledged entries of the external inbox file (missing, duplicate, destination-less lines). Trigger: any skipped or duplicated log line in the trial, or the third card's retro.
- **A slice states whether its red can pass or fail on any machine (developer; QA):** card 303's slice said "the run fails on the current machine", which would have made the red fail in Linux CI; the facilitator's note caught it, and the red then needed a production seam (PHRASE_BENCHCS_DOTNET). Hypothesis: asking the slice "does this red pass or fail the same way on Linux CI and on the development machine, and does it need a seam in production code" removes reds that need a later seam. Metric: reds that need a seam the slice did not name. Trial: the next three cards with a red.
- **Gate with a blocked shell says "SHIP on facilitator-run evidence" in the verdict line (QA, audit lens; this is D190's own rule):** on card 303 the Copilot gate wrote SHIP with record-only labels and the facilitator's reruns sat in the notes, without the separate "facilitator-run evidence" verdict line D190 asks for; not a new rule, a miss to correct on the next gate.
- **A gate or voting seat's launch is scripted, not typed (developer, coach):** `cmd /c copilot -p "..."` lost the prompt quotes twice; a script calling copilot.cmd directly worked. Also: the retro-voices runner accepted a Codex echo of the prompt as a reply because the prompt contains the heading it checks for, and rejected a Copilot vote that followed narration; fix the runner to take the vote after narration and to reject an echo. Facilitator tooling, not a card.
- **One task-force round for a bug card whose cause the probe confirms (coach):** window minutes per card with no new objections after round 1; counter-metric: defects found at the gate or sign-off that a second round would have caught. Not picked; round 2 on card 303 was needed for a polled amendment (the product-owner decision), so the evidence is mixed.
- **An evidence box per card (decider) and an audit bundle per Done check (audit lens):** near-duplicates of D190's labels; wait for D190's three-card review before adding more.
- **Seats that return no reply (Muse, Grok) are recorded as a seat failure, not a driver ban:** Muse needs one more trial with the card text inlined; Grok stays out of counted and slice seats until card 291.

## Finding for the next retro, from the product owner's inbox 2026-11-06: protocol conformance of a Decider result (owned finding against the agile coach role)
Evidence: Decider D191's record (docs/decisions/process-and-staffing.md#d191, .mob/records/deciders/D191-votes.json, window records of the card 303 retro). The architect's round-2 No was scored by the facilitator as an unresolved outlier read as "a new demand" and the pick was recorded as passed; the Decider protocol as written fails a proposal with an outlier unresolved after the amendment. The agile coach seat, the guardian of the team's process, was the proposer of the pick, was not shown the tally before it was recorded, and did not object; the product owner counts this as a coaching miss and an owned finding, not only a facilitator slip. Facts for the retro (not a defence): the facilitator wrote the reading into the record before any process seat saw it; no step of the card or retro loop asks the coach to check a result against the protocol before it is recorded; D191 and D192 were re-scored and re-voted strictly the same day (D192 also failed and was escalated).
Question for Decider at the retro, with fresh seats: how does the coach catch this every time? Option 1: put protocol conformance in the coach's brief and the loop: before any Decider result is recorded, the coach (a fresh context, not the proposer) checks the tally against the protocol as written and objects when a vote is reread. Option 2: a mechanical tally check (a script that applies the protocol's pass rule to the votes file and refuses a "passed" record that the rule fails), which the coach must confirm. Option 3: both. Suggested metric: results recorded as passed that the strict rule fails (target 0); counter-metric: minutes added per Decider record (more than 5 is ceremony).

## Process evidence from card 298's ready step (2026-11-06): the probe script's Codex half hung for 18 minutes
The facilitator's probe script (scratchpad, not in the repo) launched plain `codex exec`, which stalls on this machine (the Store alias and the huge Temp folder, card 297); it hung from 11:26 to 11:44 PST until the facilitator stopped its own three processes by PID (probe python, node wrapper, codex.exe; nothing else was touched). Fix applied: the probe now goes through the machine-local launcher (codex_nostore.py), and the Codex probe then passed in seconds (19:44 UTC). Lesson for the retro: every facilitator tool that starts Codex goes through the launcher and carries a hard timeout; a bounded wait would have caught this in minutes.

## Converted to story issues, 2026-11-06 (product-owner ruling)

The open process items below are now board stories with a persona and Gherkin criteria, to be refined and ranked: the card-format question (card 309), the Muse rotation trial (card 310), the coach protocol-conformance finding (card 311), the tally checker (card 312), and the decision-channel question left by D191/D192 (card 308). Their earlier experiment entries stay as history; the story is the live item.

## Process improvements moved off the board, 2026-11-07 (product-owner ruling: process improvements are not issues)

The product owner ruled on 2026-11-07 that the board holds only product stories and product bugs. A process improvement is a proposed change to TEAM_WORKING_AGREEMENT.md or the relevant process document, identified in a retro, written as a concrete diff with its user benefit, and voted for adoption through Decider after the retro; once adopted it is applied as retro follow-through, including any tooling it needs. The twelve process and tooling issues below were closed as not planned with the label not-card and moved here. Each keeps its evidence and its acceptance checks (the scenarios of the issue, in short). Rulings the product owner already gave are applied directly (marked APPLIED) and need no vote; adopted Deciders are applied as written (marked ADOPTED); the rest are PROPOSED for the next retro's vote.

User benefit common to all: the product owner's product work is not slowed or put at risk by process work that a consumer never sees.

### 316 (the gate runs the changed area's narrow suites and checks each failure path): ADOPTED (Decider D193, a three-card trial; card 313 was its first card)
- Document change: TEAM_WORKING_AGREEMENT.md, a trial paragraph (now applied): G1 the pre-ship gate brief carries a table of the changed code files, the test files that name them and their narrow suites, and the gate runs and quotes each; G2 a cycle whose tests pass before any change carries two committed mutation patches, each failing the matching test and naming the behaviour it breaks; G3 for lock, registration and cleanup code the brief lists every step and gap, each labelled rerun, record-only or blocked, and a record-only or blocked line makes the verdict HOLD until the facilitator's probe is attached. Trial: three cards that change mob-crosscli code; the third retro decides keep, revise or drop.
- Evidence and acceptance checks: card 298's gate said SHIP and the sign-off found three defects; card 313's gate HOLD on record-only lines was closed by the facilitator's probes. Scenarios: the gate record lists each suite with command, count and raw result; a no-red cycle has two mutations that each fail; each step and gap has a label.
- Follow-through: patch files must be git-apply clean (card 313 retro).

### 322 (decisions stop spending rounds on capital letters and full stops): ADOPTED (Decider D196, a three-card trial)
- Document change: TEAM_WORKING_AGREEMENT.md, a trial paragraph (now applied): K1 the cite check matches a cited point of at least six words after normalising case, markdown emphasis and trailing punctuation, the words still copied in order; a failing cite is re-asked once, then the seat is replaced; K2 a reply whose cite is the prompt's placeholder or that echoes the prompt is recorded as an echo, re-asked once on a different driver, and the seat-use record names the echoing driver; K3 nothing else changes. Metric: format-caused re-asks per Decider at most 2 (card 313 baseline 10); counter-metrics: votes counted without a real cited point (0), wrong-seat attribution (0).
- Follow-through (tooling): the cite check in decider.py and the seat-use record, test first; not yet built.
- Evidence and acceptance checks: card 313's decision took about 111 minutes of seat time, ten of the re-asks being format. Scenarios: no cite is re-asked; a case difference counts; case, emphasis and a trailing full stop together count while five words do not; words not in the proposal fail; an echo is re-asked on a different driver; the trial is measured at the retro.

### 320 (team decisions conclude without stalling or being escalated): APPLIED (product-owner ruling, 2026-11-07)
- Document change: TEAM_WORKING_AGREEMENT.md, the Decider protocol, applied now: the outlier's promise (an outlier states precisely what it requires and, once given it, drops all resistance; a later demand from a seat whose condition was given is not adopted); unresolved outliers kill the proposal and someone makes a new proposal (an impasse is not escalated; only a real purpose or direction question goes to the product owner); an absolute No kills the proposal and that seat makes the next one; if No plus Support-it votes are more than about 30 percent the proposer drops it; after a minor amendment the other voters get a quick check and any may demand a formal re-vote. The mob-defaults copy is follow-through.
- Evidence: card 313's task-force decision (Deciders D194 and D195).

### 319 (every seat reviews an amendment before a decision is recorded; replaces D187): PROPOSED (product owner's filing; for the next retro's vote)
- Document change: the Decider protocol: after any amendment every voting seat receives only the change (a diff and a one-line why) in a fresh context and replies NO OBJECTION or OBJECTION with one condition; a full re-poll stays for a real No and for conflicting conditions; D187's targeted re-ask is replaced. Interaction with the aligned protocol (320): it costs rounds, so it should be voted together with the quick-check rule of 320, which it overlaps.
- Evidence and acceptance checks: scenarios [S][O] one condition folded in and every seat reviews the diff; [E] a seat objects to the amendment; [Z] no amendment needed; [M] several amendments in sequence; [B] a seat does not reply in time; [I] the record shows the final text, every diff and each seat's reply.

### 321 (a wait never leaves the mob idle): PROPOSED (product owner's filing; for the next retro's vote)
- Document change: the facilitator duties in TEAM_WORKING_AGREEMENT.md: every background wait has a monitor that wakes the facilitator when it ends, dies or expires; a seat, test run or CI wait has a hard timeout after which the facilitator stops only that run's own process ids and re-runs on another available driver; idle minutes per card are a flow metric in each retro.
- Evidence: about five idle stalls of 40 to 70+ minutes on 2026-11-06. Acceptance checks: a monitor that ends without a result wakes the facilitator; a hung seat is stopped and re-run; the retro reports idle minutes.
- Follow-through (tooling): the monitor and timeout wrappers.

### 317 (a card that names no consumer value cannot reach Ready): PROPOSED (follow-through of rulings already applied as text)
- Document change: the Ready check (scripts/check-story-persona.py and the Contract branch of scripts/board/new_card.py) refuses a card with no consumer actor, no Gherkin scenarios or a missing ZOMBIES letter without a one-line reason. The value, format and product-first rules are already in TEAM_WORKING_AGREEMENT.md; this is their mechanical check.
- Acceptance checks: a story with a mob role as actor is refused; a contract with no Consumer value line is refused; a card in the full format passes; letter coverage is checked letter by letter; the board is checked as a whole.

### 318 (a new mob starts from templates that carry the team's core rulings): PROPOSED (follow-through: the mob-defaults copy)
- Document change: mob-defaults (the working-agreement template, Ready check, card template, story and persona guidance and the Decider text) carry, in timeless wording with no dates or personal names: a No is a No, the aligned Decider, product first, the value rule, the card format with ZOMBIES letters, and that process improvements are amendments voted after the retro and not issues.
- Acceptance checks: a card with no consumer value is refused in a new mob; a full-format card passes; letter coverage fails by letter; the templates carry the rulings; the template text is timeless.

### 312 (a Decider record is rejected when its result does not follow from its votes): PROPOSED
- Document change: scripts/mob_process.py check (and the Decider protocol text) fails a record whose result is passed while an unresolved No remains after the amendment round. Tooling follow-through, test first.
- Evidence: Decider D191 passed on an interpretive reading and was corrected only after the product owner caught it. Acceptance checks: a wrong pass is refused; right results pass; the check runs on push.

### 311 (a Decider vote is scored as written and the agile coach flags any deviation at the vote): PROPOSED
- Document change: the agile coach seat brief: before any Decider result is recorded, check the tally against the protocol as written and object when a vote is reread; the result is withheld until the departure is fixed or the proposal is recorded as failed.
- Evidence: the D191 deviation the coach did not catch. Acceptance checks: the coach's reply carries a conformance line; a named departure blocks the record; the catch rate is reported at the next three Deciders.

### 310 (the board keeps moving when one driver's quota runs out, with Muse trialled as a fourth driver): PROPOSED
- Document change: the driver rotation file and the seat briefs: Muse takes non-counted seats after a trial with the card text inlined; a failed trial keeps it out with the reason written; a quota-exhausted driver's seat goes to a driver that passed its trial.
- Evidence: Muse passed the one-word probe but returned no reply on card 303's slice brief. Acceptance checks: a trial seat's reply is stored or a seat failure is recorded with the launch command and output; placement follows the result.

### 308 (the product owner's rulings and the team's replies are recorded where a check can verify them): PROPOSED
- Document change: the inbox section of TEAM_WORKING_AGREEMENT.md: each ruling and the team's reply is logged in a committed file with its time and destination, and a check refuses a gap or a rewritten line. Decisions D191 and D192 (the inbox adoption) failed the strict count twice; this is the replacement for that question.
- Acceptance checks: an acknowledged ruling has one log line with identity, read time, acknowledgement time, boundary and destination; a heading shown acknowledged with no line fails; an edited earlier line fails.

### 305 (a Codex build seat's commands go through the delete scan): PROPOSED
- Document change: the Codex build-seat brief and the command-source rule: the seat runs with `--json` and the command_execution events are read by the delete scan like any other driver's; a seat whose stream is missing fails the record check. Today the facilitator reads the stream by hand.
- Evidence: Codex build seats now run in full-access mode (cards 303, 298, 313: 15, 18 and 11 commands, none a delete). Acceptance checks: a seeded stream with a recursive delete on a variable path is flagged; a clean one passes; the record carries the scan result and command count.

## Proposed change (product owner direction, 2026-11-07): an "Experimental values, principles and practices" section, and a visible review of the permanent agreement at each retro

- Status: PROPOSED for the next retro's vote (the ruling part, the four areas and the permanent-agreement review, is applied directly in TEAM_WORKING_AGREEMENT.md, "Retro improvement and review").
- Document change: TEAM_WORKING_AGREEMENT.md gains a section "Experimental values, principles and practices", separate from the permanent ones. Each experiment states its hypothesis, the signal or metric, a trial length or data threshold, and the date it started. The scattered "(experiment, next N cards, Dxxx)" items move there (about twenty today: D22, D150 and the ceremonies of lines 137-158, D168, D171, D174 to D178, D181, D184, D187 to D190, D193, D196, among them D153 and D187 which expired by count without a verdict). When an experiment has enough data the team decides through Decider to adopt it into the permanent section, revise and extend it, or drop it, with the evidence recorded; an experiment past its trial length is decided at the next retro and does not linger. The same structure goes into the mob-defaults working-agreement template, in timeless wording.
- User benefit: the product owner can see which rules are settled and which are still on trial, and no rule keeps costing rounds after its trial ended (card 313's decision cost about 111 minutes of seat time partly because a lighter path had expired unrecorded).
- Evidence and acceptance checks: card 313's status check found D153 expired without a verdict; every experiment line in the agreement shows its start date, metric and trial length; the first retro under the section records adopt, revise or drop for each experiment past its length and "kept, no change" for the permanent items it reviews.
