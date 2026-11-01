# Card 234 pre-ship gate (D154, D157), delivery lead ship, ctx-7

Checked at `2026-11-01T10:06Z` to `2026-11-01T10:15Z` from `main...origin/main [ahead 1]` after `git fetch --all --prune --quiet`.  
Sources used: board/234 card, `.mob/records/234/record-in-progress.json`, both slice files, both review/re-check files, and `.mob/records/234/usage/*`.

| # | Check | Verdict | Command and count |
|---|---|---|---|
| 1 | Every cited hash and record path resolves on origin | **FAIL** (hashes pass, one record path missing on origin) | `python` gate script using `git cat-file -e <hash>^{commit}` + `git branch -r --contains` + `git cat-file -e origin/main:<path>`: hashes `39/39` valid commits on `origin/main` (from `60` hex tokens; `21` non-commit tokens), record paths `17/18` on `origin/main`; missing path: `.mob/records/234/scratch/`. |
| 2 | Last full suite result: 503 OK after restart evidence; anything unrun since | **PARTIAL** | `git log --grep "503 OK"` found `153927b`; `git rev-list --count 153927b..HEAD` = `10` later commits; no later commit message records another full-suite run. |
| 3 | avowry CI | **NOT MEASURED** | **not measured without gh** |
| 4 | D157 criterion order across all card 234 commits | **PASS** | `git log --reverse --grep "Card 234"` + `git show -s --format=%B`: `60` card commits, `19` commits with one `Criterion:` line each, `0` commits with multiple criterion lines; sequence: `1 1 2 2 2 2 2 3 3 3 1 1 1 2 2 2 2 3 3`. |
| 5 | D136 per commit (known deviations c12f7f2, eb68d95, f8ef376) | **PASS** (with recorded deviations) | `git show --numstat` across `60` card commits: over-limit `3` commits exactly `c12f7f2` (`603` lines), `eb68d95` (`969` lines), `f8ef376` (`16` files). |
| 6 | Trailers and line endings | **PASS** | `git show -s --format=%B` scan across `60` card commits: trailer lines `0`, CR in messages `0`; `git show --format=` patch scan: CR count `0`. |
| 7 | Push-on-every-commit checks run now, with outputs | **PASS** | Ran `python scripts/mob_process.py check`, `python scripts/check-restart-record.py`, `python scripts/check-process-wiki.py` once each (`3/3` exit 0). Output pasted below. |
| 8 | command_scans coverage ctx-1..ctx-8 | **PARTIAL** | `python` read of `record-in-progress.json` command_scans: ctx-1 `1`, ctx-2 `1`, ctx-3 `2`, ctx-4 `8`, ctx-5 `7`, ctx-6 `2`, ctx-7 `1`, ctx-8 `0`. Coverage exists for ctx-1..ctx-7; none for ctx-8 yet. |
| 9 | Done checks 1..3 against evidence (restart and measured pair included) | **PASS** | `python scripts/check-process-wiki.py`: `process-wiki: OK (18 pages)`; `python scripts/check-restart-record.py`: `restart-record: OK (2 restarts)` and seat_ab pair `full 69701`, `brief 49032`, ratio `0.70`; re-check approvals present in record (`9f8ed23`, `039e468`, `2/2` APPROVE). |
| 10 | Domain-expert question at ship (T2) | **PASS** | `turn-slice-architect.md` states T2 answer: technical-writer domain expert required and already in task force; `review-writer-ctx3.md` re-check verdict APPROVE confirms the role was seated. Count: `1` domain expert needed, `1` present. |

## Item 7 command outputs

```text
mob-process: OK (99 closed cards, 8 records, 91 legacy)
restart-record: OK (2 restarts)
restart-record: seat_ab 1 qa-red: full 69701, brief 49032, brief/full 0.70; vs 50000: full +19701, brief -968; vs 125000: full -55299, brief -75968
restart-record: unverified row ctx-4 seats[7].tokens (949200)
restart-record: unverified end_tokens ctx-7 (528400): left out of the median
restart-record: median verified end_tokens 109588 over 2 contexts vs 125000: -15411.5
process-wiki: OK (18 pages)
```
