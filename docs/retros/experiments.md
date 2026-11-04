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

## Deferred by D184 (no card number)

- Built-prompt evidence (its SHA) before a live vote. Owner: architect. Trigger key: `live-vote`. Status: open.
- One seat policy, a per-card backend exclusion in the decision-seat gate, and a gate seat that is not the card's subject. Owner: decider. Trigger key: `live-vote`. Status: open.
- A probe-age alert before an in-flight line (two crossings on card 229). Owner: facilitator. Trigger key: `every-story`. Status: open.
