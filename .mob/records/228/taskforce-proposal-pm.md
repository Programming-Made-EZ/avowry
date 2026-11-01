# Card 228 task-force declaration proposal

Purpose: stop-the-line corrective for F-071 after card 227 and the card 237 gate. Scope stays on the missing coverage: hidden/encoded/non-shell recursive deletes, the contents of files a seat writes and then runs, and the human opt-in refusal before execution. The allow-list and fail-closed rule remain as in card 227; no change to this machine's Claude Code settings.

## Team
- Ready: Copilot CLI, ctx-1. Model rotates by LRU; this seat is the product-manager owner, writes the declaration, and keeps the safety ruling visible. Reason: the card is a corrective for a live safety gap and needs product ownership at the start.
- Slice: Claude Sonnet, ctx-2. Reason: architect the detector boundary, the allow-list, and the split so the card stays under D168. It owns the rule wording, not the execution of the fix.
- Red: Claude Sonnet, ctx-3. Reason: QA must fail on a partial scan and on any backend with no command source; it writes the hostile-input and source-coverage tests.
- Green: Claude Sonnet, ctx-5. Reason: implementation owner for the detector, file-content scan, and opt-in block; it carries the code change and the proof commit.
- Ship: Claude Sonnet, ctx-4. Reason: pre-ship gate and command-scan review, to check the run is clean and the evidence meets D169.
- Signoff: Claude Sonnet, ctx-6. Reason: final safety sign-off on the working-tree and no-out-of-tree rule before close.

Who is left out: Codex, Grok and agy, because they are unavailable or have no quota under the standing availability rule and the card is deliberately a minimal valid role-set. We do not add extra seats to avoid D136/D168 bloat. Copilot is not duplicated across every role because the fleet is not meant to be a full all-model panel; the role-vote rule is filled by available models, not by absent ones.

## Domain-expert question (T2)
What is the real command source for each execution backend, and when a backend provides no transcript or no command source, does the system fail closed or record the gap as an explicit finding? This is the T2 question because it directly decides whether a scan can prove a delete was executed, or whether it is silently unscanned.

## Value, metric, counter-metric
Value: stop recursive deletes before they escape the repo or hit a temp file logged as a normal run; the scan also catches encoded and non-shell variants that shell grep misses.
Metric: number of unsafe delete forms caught in executed commands and in files a seat writes then runs, with zero misses on the seeded hidden/encoded/non-shell set. Counter-metric: false positives plus unscanned-seat count; both must be near zero, because a gate that chases every benign text string is not safe enough to keep.

## Done checks (D157)
1. Hidden, encoded and non-shell delete forms are reported from executed commands and from files a seat writes and then runs. Partial work fails if it only catches `rm -rf` or only scans transcripts but not the file contents a seat later executes.
2. Every command source is accounted for; no-source seats are recorded as gaps, not as clean. Partial work fails if some backends are silently skipped or if the card treats a missing transcript as if the command had been scanned.
3. An opt-in hook blocks a flagged command before it runs, installed only by a human. Partial work fails if it only reports after the fact, or if it blocks by default without a human opt-in.

## Also owned here: keep vs split
Keep in this card:
- the detector for hidden, encoded and non-shell delete forms in executed commands
- the file-content scan for a seat's own writes and later runs
- the fail-closed allow-list for repo-contained work only under D169
- the explicit no-source accounting and command-scan reporting
- the human opt-in hook and its safety text

Split out to follow-up cards (not filed here):
- README snippet and Decider log rendering for command_scan
- scan-transcript dedupe and false-positive tune-ups (rmtree in read-only command text, Python string-literal parsing, PowerShell `-Exclude`/`-Filter`/comment cases, `git clean -f -- -n` and option spellings)
- Agent-tool / subagent transcript automation and legacy non-card loop / probe coverage
- symlinks and junction logic, git reset/stash/HEAD state checks, and follow-up wrappers such as `robocopy /MIR`, `getattr(shutil, "rmtree")`, `trap`/`EXIT` cases, and other interpreter heredocs

This split keeps the card inside D136 and D168 while preserving the real corrective in the main card.

## Applicable rules
- D165: intent line before first red; this card has a specific intent and no hidden scope.
- D166: assumption line with a probe if the detector, allow-list, or ranking changes; required for any command-form rule change.
- D167: hostile-input list in the slice for any list/map rule (verbs, wrappers, path names, exclusions); answer each entry in red.
- D168: slice size cap of 15 KB; the card must remain within or be split before the fix. The proposal itself is intentionally short and the work is scoped.
- D169: applies directly; recursive deletes may not target anything outside the working tree, and detection after the fact is acceptable only if the work is committed and pushed regularly.
- D171: all vote windows must cite a point from the proposal; unread votes do not count.
- D174: if this card is voted on as a window, launch the whole rotation once, write the seat-use lines in one window commit, and start all voters at once.

## Open points
- What counts as a valid command source for each backend when the transcript is absent or empty?
- Should a no-source backend fail closed in the gate or be recorded as a source gap that blocks the final ship?
- Should the pre-run hook be a repo-local opt-in only, or must it also refuse out-of-tree delete targets even when detection is quiet?
- Which false positives are worth fixing in this card vs. follow-up tuning after the main guard is stable?
- How should the card treat legacy/probe/seat-run files that are not under the current card loop? They need an explicit rule, not an assumption.
