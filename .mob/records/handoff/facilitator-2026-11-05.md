# Facilitator handoff, 2026-11-05 (after card 230)

For a fresh facilitator session. Card 230 is Done; no card is in flight; the next card is 297. Start with a probe, then card 297's ready seat.

## Repos and state

- Records: C:/Users/ringe/projects/phrase-lang (GitHub Programming-Made-EZ/avowry), main in sync with origin at the commit that adds this file.
- Code: C:/Users/ringe/mob-crosscli (mob-defaults, GitHub Programming-Made-EZ/mob-defaults), main at 1732a82, in sync. Untracked `.mob/` there is old and expected.
- Board: GitHub project 1 (the Phrase Board). Issues are cards; board/ is a generated archive (scripts/board/generate_archive.py --apply). Check: `python scripts/mob_process.py check` (OK at 108 closed cards).

## Board head (ranks)

91 card 297 (Codex shell in the Windows sandbox; bug; product owner priority: Claude fills every coding seat while Codex cannot run shell commands), 92 card 264 (read-and-modify acceptance test not flaky), 93 card 217 (acceptance tests do not starve the thread pool), 94 to 96 cards 196, 197, 266 (Avowry rename), 97 card 232, 98 card 162, 99 card 201, 100 card 258. New unranked bugs: 298 (a second decider run votes unprotected after the first releases the vote lock) and 299 (a second writer refused across processes; the hand-run case card 230 did not cover); rank them at the next re-rank. Cards 295 and 296 (legacy characterization, then portability) sit after card 030 (ranks 197, 198).

## Open audit items

- Latest audit in the records: 221 (.mob/audits/last.json): open FAILs F-072, F-074, F-078; 51 open FLAGs. The FAIL texts are in the external auditor's ledger (read-only to the team); their texts are still owed in our records (pending since card 228). The team never writes or edits tick files or anything under .mob/audits/.
- Findings from audits 219 and 220 handled on card 230: F-085 (seats started on a probe over 50 minutes old) and the push-loop MOB.md commits.

## Rules in force (read TEAM_WORKING_AGREEMENT.md and docs/decisions/process-and-staffing.md)

- Seats in fresh contexts by role; one criterion per cycle (D157); objection windows (D153, D145) with verbatim D171 cites (at least 6 words, or "line N: w1 w2 w3 w4"); one re-ask for an unread vote; D175 caps with a scope window before any red past a cap; D181 lettered reds only before a green has passed; D184 scope premises run in code on HEAD; D176 environment line in every red and green.
- D187 (new, experiment, next three cards' windows): when round 1 has no No and exactly one seat's condition, fold it in and re-ask only that seat and any seat whose cited point it changes; full re-poll for any real No or multiple or conflicting conditions; name who was re-asked in each window record. Card 230 is the baseline.
- Trunk-based (working agreement, 2026-11-04): all writing serial on main in the one checkout under the single-writer lock; seats commit directly on main; the facilitator writes nothing in that checkout during a seat's turn; no per-seat worktrees, branches or copies. Parallelism only for voting: voters are read-only and may share the one checkout.
- D183: the board holds only user stories and bugs; process ideas go to docs/retros/experiments.md. Justify decisions on their merits; record product-owner directions neutrally, with no quotes or attribution; they/them for the product owner.
- Safety: kill only your own PIDs, never by name; never rm -rf variable paths; never AskUserQuestion; plain text; never git add -A; no amend, history rewrite, --no-verify or Co-Authored-By trailers (a hook refuses them); seats never run delete commands or git stash; no commit to avowry while a decider voter runs; read every scan result before recording it.
- Probes under 60 minutes and re-probed at 50 before every seat (F-085). Push promptly (5-minute rule). A refactor commit or a reasoned "none needed" after each green. Flaky tests are bugs. End turns only at true boundaries.

## In-flight and push conventions

- An in-flight line in .mob/records/blockers/in-flight.md before every seat or window, written by the scratchpad's inflight.sh: `bash inflight.sh "<what, seat and model>" <cap minutes> "<cap source>"` (run from the scratchpad; it commits only that file and pushes).
- Pushes: scratchpad push_retry.sh (from the repo): it regenerates MOB.md only when the pre-push check says MOB.md is stale or not byte-identical, and otherwise prints PUSH FAILED with the reason. Do not loop regenerations on other refusals (audit 219).
- A closed issue that is not a card needs the not-card label (created 2026-11-04).
- GraphQL budget: edit_card.py costs about 90 points per edit; check `gh api graphql -f query='{rateLimit{remaining}}'`. rerank.py placements are cheap per item.

## Seats and drivers

- Grok: disabled in counted windows (backends-nogrok.json, the WBACKENDS variable) until card 291.
- Codex: voting and read-only seats work. Under its Windows sandbox, PowerShell (a Store app alias) is refused with CreateProcessAsUserW error 1920; commands through cmd.exe work, so brief read-only Codex seats to use cmd. Codex build seats are blocked on card 297; its one-time machine fix (a non-Store PowerShell, or Codex set to use cmd) needs the product owner's approval. Codex replies carry a CLI echo after "Reading additional input from stdin"; trim it from records (card 293).
- Copilot: counted seats; long test runs inside a Copilot seat can hit the timeout (card 230 gate attempt 1); give gate seats a narrow run list.
- Claude: subagents for reds, greens and sign-off.

## Scratchpad tooling (C:/Users/ringe/AppData/Local/Temp/claude/C--Users-ringe/73298da3-f8ff-4a30-af55-b3e95ffa16f4/scratchpad)

inflight.sh, push_retry.sh, probe_drivers.py (`--only copilot codex --out FILE`, then append to .mob/records/probes/2026-10-30.jsonl and commit), run_window.py (`WBACKENDS=backends-nogrok.json WNOTE=w276-note.txt python run_window.py TAG PROP "CTX" roles...`; re-ask note w229-reask-note.txt), rec_window3.py (records a window from a spec JSON; spec-230sc3.json is the latest model; category must be one of the closed list, for example "slice decision", "card-text or declaration fix", "task-force declaration"), run_voices.py (retro voices), run_votes_pick230.py (Decider on a retro pick; RERUN=1 for the re-run with coach in place of the product manager), rr21_apply_lean.py (cheap board edits). A fresh session may get a new scratchpad: copy these first.

## Retro experiments to watch (docs/retros/experiments.md)

D187's targeted re-ask (trial); each slice S-case gets its own red before the first green (architect); a counted quorum; a CITE asked for in build_prompt; a seat time cap the runner enforces; the stored Codex echo; the measure-only premise probe before the slice (D186, not adopted, with its objections); rx-265 assumes parallel voters on the one checkout; board scripts fetch once.

## Pending

- Owed cleanup, held for the product owner's confirmation: in mob-crosscli, 14 card-230 worktrees under the scratchpad (wt-230-*) and 10 local card230-* branches, all clean and merged into main, none pushed; plus older scratch worktrees from cards 229 to 290 (`git worktree list`). Removal deletes directories; do not run it without approval.
- .mob/records/228/body.tmp (untracked), and .mob/records/234/scratch/ (untracked, holds nested copies of itself that slow repo-wide searches).
- The F-072, F-074 and F-078 texts in our records.
- core/roles/devops-engineer.md:7 still tells the ship seat to pull --rebase --autostash and push (only a prompt sentence overrides it); a candidate for card 299 or 216.
