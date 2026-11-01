# Pre-ship checklist, card 227 (ship, delivery lead seat)

Run key: wiki-ship-1. Card 227 is treated as open at the ship step. Nothing was pushed.

## Sources relied on
- docs/process-wiki/INDEX.md (route row delivery-lead-ship: core/index, role/delivery-lead-ship, rules/stand-downs, rules/safety-and-push, rules/decisions)
- docs/process-wiki/roles/delivery-lead-ship.md (D154, D136, D159, D169)
- docs/process-wiki/rules/stand-downs.md (D151, D155)
- docs/process-wiki/rules/safety-and-push.md (D169)
- docs/process-wiki/rules/decisions.md (D153, D170)
- board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md (Done checks, D145 condition, D169 ruling)

Not read, by instruction: TEAM_WORKING_AGREEMENT.md, .mob/records/227*, decisions/227*, card 227 git history. The wiki pages do not name the closure record check script or the detector script, so the commands below say "the closure record check" and "the unpushed-commit detector" and the seat must use the repository's own entry points.

## Checklist

| # | Item | Proof (command or file) | Rule |
|---|---|---|---|
| 1 | Closure record check passes in dry-run mode, and every commit hash and record path in the closure record resolves on origin | Run the closure record check with its dry-run flag; for each cited hash, `git fetch origin` then `git branch -r --contains <hash>` (or `git cat-file -e <hash>`) against origin; each record path exists at origin's tip | D154 (delivery-lead-ship) |
| 2 | Evidence is compared with the record committed immediately before card 227's own record, not an older one | The check's comparison base is the record added in git history just before card 227's record | D159 (delivery-lead-ship) |
| 3 | Done check 1: the scan blocks variable or computed path recursive deletes (rm -rf, rm -r, Remove-Item -Recurse with or without -Force, shutil.rmtree) in executed commands, not only reply text; failing test written first, reproducing the card 226 slip; Zero case (empty or missing command-scan source fails) | Test file and green commit for criterion 1; test run output; D145 condition met per board card | Board card; D145 |
| 4 | Done check 2: trust-ladder entry recorded for the slip (seat, rule, step down or reason none applies) | The template's trust-ladder record contains the entry | Board card |
| 5 | Done check 3: push on every commit is stated in the working agreement and the mob-defaults template, and the detector reports FAIL for a commit on avowry or mob-defaults unpushed for more than about 5 minutes (git only, no gh calls) | Rule text in both files; detector test; detector run output | Board card; D169 |
| 6 | The unpushed-commit detector is actually called at ship, on both repositories (avowry at C:/Users/ringe/projects/phrase-lang, mob-defaults at C:\Users\ringe\mob-crosscli), and reports no FAIL | Run the detector; also `git status -sb` and `git log origin/<branch>..HEAD` in each repo must be empty | D169 (safety-and-push, delivery-lead-ship) |
| 7 | Recursive deletes only target paths inside the repository working tree (condition 2 of D169, guaranteed by card 228); I run no recursive delete outside the tree during ship | Review of my own commands; card 228 status | D169 (safety-and-push) |
| 8 | Invariants hold: a literal path in a known scratch root through safe_delete is still allowed; process-kill rules unchanged | Tests for the allowed case; diff shows no process-kill change | Board card |
| 9 | Retro for card 227 is written and pushed before issue 227 is closed and before any later commit naming a different card | Retro file in the records; `git branch -r --contains <retro hash>` shows origin | D136 (delivery-lead-ship) |
| 10 | Stand-down at the boundary records what worked, what did not, and one adjustment; it opens by reviewing each adjustment from the previous stand-down or retro (including the card 226 stand-down that named the slip and adjusted nothing) before any new adjustment | Stand-down record for the ship boundary | D151, D155 (stand-downs) |
| 11 | Decisions: routine calls (task force, ship timing) went through an objection window, not a full Decider; any rule change (the push-per-commit rule text, the scan rule) has a full Decider; any product owner ruling is recorded as a decision | Window and Decider records cited by the closure record | D153, D170 (decisions) |
| 12 | Board status line and issue 227 state are consistent (card shows Done with its summary; issue closed only after items 1, 6 and 9 pass) | Board card header vs issue state | Board card; D136 |

## Push steps (to be run only after the checklist is green and the seat is told to push; not run in this pilot)
1. `git status -sb` in each repo: tree clean, nothing staged by mistake.
2. `git fetch origin`, then confirm the branch is fast-forwardable (no force, no hook skipping).
3. Push the card's greens and records at once: `git push origin <branch>` in mob-defaults, then in avowry if it carries commits.
4. Push the retro for card 227 (item 9) before closing the issue.
5. Re-run the closure record check dry-run and the unpushed-commit detector; both must pass against origin.
6. Close issue 227 only then.

## Stand-down steps
1. Open by reviewing every adjustment from the previous stand-down or retro (D155), stating kept, changed or dropped.
2. Record what worked, what did not, and exactly one new adjustment (D151).
3. Push the stand-down record at once (every commit is pushed, D169).

## What blocks shipping
- Closure record check fails, or any cited hash or record path does not resolve on origin (D154).
- The record check's comparison base is not the record immediately before card 227's (D159).
- Any commit on avowry or mob-defaults is unpushed, or the detector was not called, or it reports FAIL (D169).
- Any Done check (1, 2, 3) lacks proof, or the scan does not see executed commands or fails the Zero case (D145 condition).
- Retro not pushed yet (D136), or a later commit naming another card exists ahead of it.
- Stand-down missing, or the previous adjustment not reviewed first (D151, D155).
- A rule change without a full Decider, or an objection in a window that has not gone to a Decider (D153).
- Any recursive delete targeting outside the working tree (D169; card 228).
- Unresolved open points that I cannot settle from the wiki: the exact script names of the closure check and detector, and the real state of origin, which I did not query here.
Size: 6469 bytes
