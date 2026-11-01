# Pre-ship checklist, card 227 (ship seat, delivery lead)

Run key: baseline-ship-1. Nothing was pushed. This is a plan and a list of proofs; no item is marked passed.

## Sources relied on
- `TEAM_WORKING_AGREEMENT.md` (the only process context). Rules used: Practices, "Push every commit (checked, card 227, F-071)"; "Claim equals commit, and a pre-ship evidence gate (D154)"; "One acceptance criterion per red-green cycle (D157)"; "Stand-down at each cycle boundary (D151)"; "Every stand-down and retro opens by reviewing each previous adjustment (D155)"; "Deletes: detect after the fact inside the working tree (D169)"; "Facilitator pre-window checklist (D158)"; "Retro after every Done card (D136)"; "Thin slices (D136)"; "Slice size cap (D168)"; "Intent line (D165)", "Assumption line (D166)", "Hostile-input list (D167)"; "No Co-Authored-By trailers"; "Stage explicit paths only"; "Trunk-based, direct commits to main (D135)"; "CI on main is part of the line"; "Driver trust ladder" (trust-ladder record, from card 226); Safety (the command scan's limits); "Task force before every card"; "Staffing: roles over models" (fresh probes under 60 minutes); Independence of review; Escalation (safety carve-out).
- `board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md`: Done checks 1 to 3, Invariants, Context, D169 ruling.
- Repository files read: `scripts/mob_process.py` (subcommands here: check, write-mob-md, seat-prompt), `scripts/delete_guard.py`, `.githooks/pre-push`, `DRIVERS.md`.
- Not read, as instructed: `.mob/records/227*`, `.mob/records/decisions/227*`, card 227 git history, `docs/process-wiki/`.

## Limit of this run
This clone is the avowry repository. The card's code and tests live in mob-defaults (C:\Users\ringe\mob-crosscli), which is not in this clone. Here `scripts/mob_process.py` has no `unpushed` or `ladder` subcommand, so those proofs could not be run or verified from this tree. The board file's header says Done, but this run treats the card as open at pre-ship. Every proof below is "to be run by the real ship seat".

## Checklist (item, proof)

| # | Item | Proof (command or file) |
|---|---|---|
| 1 | Done check 1: the scan flags a variable-path recursive delete in executed commands, not only reply text; red reproduces card 226's slip, then Remove-Item -Recurse -Force, shutil.rmtree; Zero case (empty or missing command source) fails | In mob-defaults: the scan's test file run with its runner; `python delete_guard.py scan-transcript <transcript>` on a fixture; red and green commit hashes for Criterion 1 |
| 2 | Invariant: a literal path in a scratch root through `safe_delete` stays allowed; kill rules unchanged | Test: `safe_delete` on a literal scratch child passes; home, drive root, `..`, symlink, the root itself refuse. `git diff` shows `_find_kills` untouched |
| 3 | Done check 2: a trust-ladder entry for the slip (seat, rule, step or "none: reason", `brief_checked`) | `.mob/records/trust-ladder.jsonl` line for card 226 criterion 1; `python <mob-defaults>/mob_process.py ladder <repo>` exits 0. The agreement's ledger already cites it (ctx-4, stays Rung 1, coached) |
| 4 | Done check 3: rule text in the working agreement and the mob-defaults template; the detector FAILs a commit unpushed more than 300 s | Agreement bullet "Push every commit" (present in TEAM_WORKING_AGREEMENT.md); template file in mob-defaults; `python mob_process.py unpushed <repo>...` boundary tests: 300 s passes, 301 s fails; future-dated commit, missing path, no `origin`, non-zero git exit all FAIL |
| 5 | Invariant: the push detector is git only, no `gh` | grep the detector source for `gh`; no match; origin never contacted |
| 6 | Criterion order (D157, D154 gate line): "Criterion: n" order from `git log`, greens before the next red, breaches explained | `git log --format='%h %s%n%b'` filtered for `Criterion:`; output committed in the pre-ship gate file |
| 7 | Pre-ship evidence gate (D154): closure record check dry-run; every cited commit and record path resolves on origin | Run the closure record check (`python scripts/mob_process.py check` here, plus mob-defaults' check); after `git fetch`, `git cat-file -e <sha>` and `git ls-tree origin/main -- <path>` for each cited item; commit the output under `.mob/records/` before the closing commit |
| 8 | Claim equals commit: every PASS or "recorded" cites a pushed hash | `git branch -r --contains <sha>` lists `origin/main` for each cited hash |
| 9 | Slice rules: each card commit at most 15 countable files and 400 changed lines; slice file Size line matches `head -n -1 <slice> \| wc -c` (15360 cap); Intent, Assumes (with probe) and hostile-input lines present | `git show --stat <commit>`; the slice file; red commit bodies |
| 10 | Stand-downs: N boundaries give N-1 entries, each with an adjustment, a non-facilitator seat context, and the D155 review of the prior adjustment | `stand_downs` in the card's `.mob/records/NNN.json` (off limits to this run; the real seat reads it) |
| 11 | Task force: eight declared seats, none both `[author]` and `[reviewer]`, domain-expert answer at each boundary, a recorded reason for every Claude seat; probes under 60 minutes old at the seat | Card `## Task force`; `.mob/records/probes/`; `DRIVERS.md` (Codex, Grok, Copilot, agy shown unavailable; probes there are old, so re-probe) |
| 12 | Independent review by non-author in a fresh context, with real output (security reviewer, independent reviewer; the backend developer author is neither) | The review records |
| 13 | Gate green: `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings; no Pending criterion test survives the card | The command and its output; grep for `Status` Pending in tests |
| 14 | Hygiene: no `Co-Authored-By` trailer on card commits; explicit paths staged; `.mob/` scratch not staged except `.mob/records/`; board file untouched (generated archive) | `git log --format=%B <range>` grep -i co-authored-by is empty; `git status --short`; `git config core.hooksPath` is `.githooks` |
| 15 | No unpushed commits before ship | `python mob_process.py unpushed <avowry> <mob-defaults>` exits 0 |
| 16 | Honest residuals recorded: the scan is a detector, not a block; encoded, hidden-verb, written-to-file and non-Claude-seat forms stay uncovered; D169 condition 2 (nothing outside the working tree) belongs to card 228 | Wording in the card record and the retro |

## Push and stand-down steps (not executed in this run)
1. In one command print the working directory and `git rev-parse --show-toplevel`, for avowry and for mob-defaults (D158).
2. If the newest Codex, Copilot, Grok or agy probe is over 60 minutes old, re-probe and commit the evidence under `.mob/records/probes/`.
3. Commit the pre-ship gate output (items 6 to 8) under `.mob/records/`; explicit paths only; no Co-Authored-By trailer.
4. Push straight after each commit, mob-defaults first (the code), then avowry: `git push origin main`. The pre-push hook needs `gh auth status` and runs `mob_process.py check`; if it blocks, fix the cause, never bypass it.
5. After each push: `git fetch`, `git status -sb` shows nothing ahead, and `unpushed <repo>...` exits 0.
6. CI: `python scripts/check-ci-main.py --wait 1500`. Green or pending: continue. Red: stop the line.
7. Stand-down: ship is the last boundary before Done, so it has none (N boundaries, N-1 stand-downs). The full per-card retro follows, opens with flow metrics and the D155 review of earlier adjustments, includes the feedback-loop lens, and is pushed before issue 227 closes (D136).
8. The facilitator closes the issue; the board archive is regenerated, never hand-edited.

## What blocks shipping
- Any item above with no output, or a cited hash that does not resolve on origin.
- The scan missing a destructive command absent from reply text, or an empty or missing command source passing.
- The unpushed detector passing a commit older than 300 s, or failing the 300 s boundary case.
- A missing trust-ladder entry (`ladder` exits 1).
- Any commit unpushed more than 5 minutes (a FAIL by itself).
- Red CI on main, a failing gate, a surviving Pending criterion test, or a Co-Authored-By trailer.
- A seat that is both author and reviewer, or no non-author review.
- An oversize slice, a missing or wrong Size line, or a commit over 15 files or 400 lines without a verified exception kind.
- A recursive delete on a computed path during ship itself, or one aimed outside the working tree (D169).
- Escalation: none to the product owner for this card; anything new that is destructive or irreversible needs a one-word product owner OK first.

Size: 8652 bytes
