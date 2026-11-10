# Card 162 gate review, pass A (independent gate reviewer, fresh context)

Reviewed at main a81e784fde05185fcd38a2381009b5f2a01fd6c1 (evidence commit e2e5a4adbc202ebcf1ebcf27d5e6d3690d2d057e, re-freeze record 46179b4, green 1 1bcae823941d5e09b0f01b829b314f10eee29252). Nothing tracked was edited or committed; this file is the only file created.

## Verdict: PASS A

No blocking finding. Two low findings and two notes below. The change is gate-ready as "green 1 plus re-freeze plus Grok evidence"; green 2 and the Codex run are correctly outstanding.

## Step 0: ancestor check (D219/D226)

```
git merge-base --is-ancestor 70511e654244c0567177af84f589047192d4782e 1bcae823941d5e09b0f01b829b314f10eee29252   -> yes (exit 0), also ancestor of main
git merge-base --is-ancestor bf723c252de281477656dd77f6b9f023ecc5652d 1bcae823941d5e09b0f01b829b314f10eee29252   -> yes (exit 0), also ancestor of main
```
PASS. Seat difference: red-record.md states the red was the QA RED seat, a fresh Claude context; the declaration (D237) assigns green 1 to the Cursor writing seat. All commits carry one git identity, so git itself cannot show the seat difference (finding L1).

## 1. The ten needles: PASS
`python -m pytest tests/test_pattern_docs.py -q` -> `10 passed in 0.04s`.

## 2. Pending traits as designed: PASS
`dotnet test --filter` (R1_the_refreeze, Evidence_rule, Sheet_is_at_most_700, PatternDocs, EnsureStepCompiles, RecordUpdateFormCompiles): 15 of 15 passed (R1 passed; all nine Evidence_rule rows passed; Sheet_is_at_most_700 passed; PatternDocs passed when selected by name; compile tests passed).
By source read: PatternDocs (162-1) `Status=Pending` (PatternDocs.cs:14); Bar6 (162-2) `Status=Pending` (UsableV1.cs:102); Z1 and Z2 `Status=Pending` (UsableV1Closure.cs:20, 38); R1 carries no Pending trait and passes. As designed.

## 3. Hashes and tokens: PASS
All nine D238 machine lines (marker refreeze-162), new-hash column, recomputed with sha256sum: MATCH for GRAMMAR.md 92029d22b0aaf52d266ef2710238a75cb5908afeacf1146e44343a808dc9d222, CHEATSHEET.md 3611d24bac1de57a2b0de0225a6125916c9a610a899697cb37660b65107cc189, prompt.md 61c0e7b02ad5caffa1a319dceb268c56b5f6528974dfb1b533ae9869daed4019, outcomes.json 57d330cc5fad87974aa23dab133fa86bfe0b5422644a36b7c37e09aabe5380d1, suite.json 5ad91957db8fc1703287d4bd98f3fad91af35e4122e718cc1b9ec82eb3fe7962, Tasks.avowry eee88242de7ffba4b5860420231b32545c7f229c61cd8600bcab3cf81f200306, orders.avowry 4d7a24b9877a79b43b308d5052cb3a807af0dd88465399b9fc639958a4f637f8, Program.cs 0e9e91c2a7f2a6c5c579ede6d87161f9160d59be55723769842e7fae42b20f8c, run-llm-check.py 88e17459c8c2d226821948745baf43d722c2510cb48ff390461d0b4c0d31e465.
Tokens ceil(chars/4): cheat sheet 2771 chars -> 693 (record says 693, cap 700, 7 spare); GRAMMAR 21874 chars -> 5469 (proposal says 5469). Files are LF only (0 CR bytes), bytes equal chars.

## 4. Evidence JSON pins: PASS
- llm-check-codex.json (untouched since ff3b3b9): grammar ad0f6e51fe1d61968fecb7ed3609bb00b204f3aa482defcf38f6fd6f6728dc9f, cheatsheet fe3d8a8b37f4b5bfe2c95e9a38bd9b59de5c57fa163c2c4526ef55d5c1300ae3 = the recorded old hashes. So bar 6 still fails for Codex until its rerun, which is why Bar6 stays Pending.
- llm-check-grok.json (e2e5a4a): grammar 92029d22...dc9d222, cheatsheet 3611d24b...cc189 = recorded new hashes; passed true, specs 13, specs_passed 13, build_warnings 0, proof_level specs-passed, attempts 1.
- `cmp` against .mob/records/162/llm-check-grok.json: byte-identical. sha256 8cdc563f18a616c159d0d42d582329d576a52f592d504e20ec745fb95946491e equals the value in run-grok.md. The date 2026-11-10 is UTC (run taken 2026-11-10T00:37Z, 16:37 local on the 9th), not an anomaly.

## 5. Freeze window: PASS
`git diff --quiet 46179b4..HEAD` over GRAMMAR.md, CHEATSHEET.md, bench/, demo/, orders.avowry, Program.cs, run-llm-check.py: clean. `git diff --name-only 46179b4..HEAD` outside .mob and MOB.md lists only docs/usable-v1/llm-check-grok.json (not a bound file).

## 6. Sponsor-persona read against the code (CLI built from this tree, scratch files outside the repo)
Verified by `Avowry.Cli build`:
- ensure code not in `fails with`: `AV326 (11:5) Ensure failure 'Other' is not declared in 'fails with'.` Matches the grammar and sheet.
- repeated `fails with` code: `AV329 (8:1) Failure 'Blank' is repeated in 'fails with'.` Matches.
- record update with a repeated field: `AV303 (10:37) Field 'done' is repeated in the update.` Matches.
- unknown field `AV311`, wrong field type `AV312`, existing name `AV313`, empty field list `AV200`: all as documented.
- Int path on the right (`ensure n < m`): compiles, as documented. Out-of-range length (-1, 99999999999): AV200; out-of-range Int literal: AV312. Documented as "non-negative 32-bit" and "signed 32-bit"; consistent.
- endpoint ensure with no `fails with`: builds (code becomes an effective failure), as documented. `ensure` line without `else`: falls to avow resolution (AV001), as documented. A well-formed avow using all three conditions and three codes builds clean.
- AV convention sentence ("AV followed by three digits, a code keeps its number"): every AV code in src matches that shape; the table range AV300-AV335 plus AV337/340/341 matches the codes in src (AV327/AV328/AV336 gap: 336 is unused in src, consistent).

Contradictions found: none that the code refutes. Two wording risks, see L2 and L3.

## 7. Promised but not yet delivered, and correctly still pending
- Green 2 (not started). Correct: it waits for Codex.
- Codex run: llm-check-codex.json still pins the old hashes. Correct; scheduled at its reset (2026-11-11 per the ruling).
- Un-pending: Bar6 (162-2), Z1, Z2 and PatternDocs (162-1) still Pending. Correct, per design (green 2 does it; un-pending Bar6 earlier would turn the Status!=Pending gate red).
- Deletion of suspended.6 in board/usable-v1-ratchet.json: still present (lines 11-13). Correct; Z1 pins it.

## 8. Value rule
The sponsor persona reading only GRAMMAR and the cheat sheet now has both new steps, the code convention and the diagnostic rows, and every claim I ran held. Grok, a model that never saw the app, passed 13 of 13 specs from these two pages alone on attempt 1; the cold reader (Muse) wrote both step forms from the pages. This is consumer value. The cheat sheet stayed within the 700-token cap (693). PASS.

## Findings, ranked
1. (None blocking.)
2. L1 (low, process): green 1 has no per-seat record file and all commits share one git identity, so the red and green seat difference rests on the declaration and red-record.md, not on anything git verifies. Suggest the green-2 or retro record names the green 1 seat explicitly.
3. L2 (low, doc): the cheat sheet writes `ensure <Text path> is [not] blank|length at most N else Code`. A reader can take it as `is length at most N`; the CLI rejects that with AV200 (`Expected a comparison operator ... found 'length'`) because the grammar form is `<path> length at most N` with no `is`. Grok passed anyway, and the sheet's own example uses `is not blank`. The pages are frozen until green 2, so any fix is a new re-freeze; defer, record as a candidate.
4. L3 (low, doc): the grammar says the `else` code is an Ident but does not say it must be Capitalized; `else bad` is AV200 (`Expected a failure code (Capitalized)`). The sheet's examples all use capitalized codes. Same freeze consideration as L2.
5. N1 (note, stale comment): PatternDocs.cs:15 says "Pending until green 1 folds the pattern steps"; green 1 has landed, so the comment is out of date. The Pending trait itself is correct (green 2 removes it). Fix the comment at green 2, not now (the file is not in the bound set but the card plans one edit pass).
6. N2 (note): the working tree has untracked files under .mob/records/176, 228, 234, 237 (not card 162); none touch the bound files.
