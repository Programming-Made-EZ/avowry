# Card 266 characterisation 3 (D157 Criterion 3), QA engineer seat

Declared CHARACTERISATION, not a red. The tests pass on the pre-change tree (HEAD f7937a9, no card 266 green yet). Reason: criterion 3 says nothing outside the nine scoped files changes; that is already true, and the tests pin it before the greens so any later change outside the scope fails.

## What was written
`tests/test_live_docs_266.py` (new): constants `SCOPED` (the nine files, literals), `BASE = "0fa5b228c69665349aa625f0238716b7057d9b22"` (verified with `git rev-parse 0fa5b22`, same hash), and class `UntouchedBytes266` with 3 tests:
1. every path outside the exclusions has the same (mode, blob sha) at BASE and at the comparison commit (non-vacuity: more than 100 paths compared);
2. the path sets outside the exclusions are equal (nothing added, nothing deleted);
3. `docs/CODING_GUIDELINES.md` and `docs/GRAMMAR.md` are identical between BASE and the comparison commit (scenario Z).

The comparison commit is the newest commit whose subject starts `Card 266 green 1` or `Card 266 green 2`; HEAD if there is none. Comparison uses `git ls-tree -r -z`, so line endings and bytes are compared as stored.

## Exclusions and why
- Exact files: the nine scoped files, `tests/test_live_docs_266.py`, `tests/test_demo_readme.py`.
- Prefixes: `.mob/` (mob records, baselines, logs), `docs/decisions/` (decision records are history that grows), `board/266-` (this card's own board file).
- Probe: `git diff --name-status 0fa5b22 HEAD` outside `.mob/` shows NO difference at all today. Cards 324 and 325 and the board archive files were part of the D205 commit, which IS the base, so no board/ allowlist is needed: the set of board/ paths that differ today is empty. Any other board/ change (another card's file, `board/MILESTONES.md`, `board/MILESTONE-usable-v1.md`) fails the guard on purpose. If the board tooling regenerates those during the card, the facilitator must decide whether to add them to `EXCLUDED_PREFIXES` with a reason; I did not pre-allow them.
- `docs/decisions/` is allowed by the brief although nothing differs there today.

## Run
`python -m unittest tests.test_live_docs_266.UntouchedBytes266`: Ran 3 tests, OK (at HEAD f7937a9).

## Baseline files (`.mob/records/266/baseline/`)
- `guard-hits.txt`: `git grep -n -E "<P>" 0fa5b22 -- <nine files>`, 39 hit lines (matches the slice).
- `eol.txt`: `git ls-files --eol` for the nine files and `tests/test_demo_readme.py`.
- `python-counts.txt`: run in a `git archive` copy of HEAD in a temporary directory. test_demo_readme 6, test_readme_reference_status 4, test_process_wiki 69, tests.board.test_board_archive 17, test_license_files 6, test_no_personal_names 4, all OK; `scripts/check-process-wiki.py` OK (18 pages). No difference from the expected numbers.
- `gate-run-before.txt` is the facilitator's and is not written here.
