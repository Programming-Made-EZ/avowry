# Green plan, card 227, Done check 1 (green seat, run wiki-green-1)

Plan only; nothing is changed.

## Rules relied on (all by path)
- docs/process-wiki/INDEX.md: route row `backend-developer-green` lists the pages read: core/index, role/backend-developer-green, rules/one-criterion, rules/outside-in-tdd, rules/thin-slices, rules/safety-and-push.
- docs/process-wiki/roles/backend-developer-green.md (D157, D154, D136, D169):
  - the green for criterion n is pushed at once, before any red for n+1;
  - no done/PASS/evidence without a pushed commit hash that contains it;
  - at most 15 countable files and 400 changed lines per commit;
  - commit and push regularly, and no recursive delete outside the working tree.
- docs/process-wiki/rules/one-criterion.md (D157): one criterion per cycle. This cycle is criterion 1 only; checks 2 and 3 are not touched.
- docs/process-wiki/rules/outside-in-tdd.md (D137): the green commit removes `Status=Pending` from the acceptance test in the same commit that makes it pass; the test body stays byte-identical.
- docs/process-wiki/rules/thin-slices.md (D136, D168): size limits, and a criterion is never cut in two. Check 1 goes in whole, in one commit.
- docs/process-wiki/rules/safety-and-push.md (D169): no recursive deletes outside the tree, and the unpushed detector must actually be called.
- board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md: Done check 1 text and the adopted condition (window 227-taskforce, D145).

## What the red must already be (precondition)
The repo has no QA red for check 1 that I could find. tests/ has no delete-guard test, and Pending marks are not visible in the files I read. This plan therefore assumes the red commit exists with a pending test naming "Criterion: 1". If it does not, the green seat stops and asks for the red. It does not write the test.

## Gap in the code as it stands
- scripts/delete_guard.py `find_unsafe(text)` already flags `rm -r`, `Remove-Item -Recurse` and `shutil.rmtree` on variable paths, as well as rd/rmdir/del /s and `git clean -f`.
- It only scans text handed to it. scripts/check-unsafe-deletes.py feeds it only added lines of staged files. Nothing scans the commands a seat executes (tool calls or a transcript), and nothing fails on an empty or missing command source. That is the card's adopted condition and the Zero case.

## Smallest change that turns the red green
1. In scripts/delete_guard.py add one function, `scan_commands(source)`:
   - read the executed-command source (a JSONL of tool-call commands, the form the red fixture uses);
   - run `find_unsafe` on each command and return findings tagged with the seat;
   - an empty or missing source returns a failing finding (the Zero case).
2. Call it from the existing record check path in scripts/mob_process.py, so an unscanned seat or a variable-path recursive delete fails the record check.
3. Widen a `find_unsafe` pattern only if the red names a form it misses (tonight's exact slip, or an equivalent). Do not widen it otherwise.
4. Remove `Status=Pending` from the red test in the same commit. Leave the test body byte-identical.
5. Keep scripts/delete_guard.py in sync with its mob-defaults copy, as the docstring in check-unsafe-deletes.py requires.

Out of scope: check 2 (trust-ladder entry) and check 3 (push detector); anything under the board or records; no refactor.

## Files touched (expected 3 to 4, well under the 15-file / 400-line cap)
- scripts/delete_guard.py
- scripts/mob_process.py
- the red test file, only to drop `Status=Pending`
- the mob-defaults copy of delete_guard.py, in its own repo and commit, if the red or the pre-commit sync check requires it

If the diff nears 400 lines, split by concern. Never split the criterion.

## Proof block for the green commit body
The wiki pages I read do not give the exact Proof block layout. I take the fields from D154, D137 and D157; the block should be conformed to the commit-msg hook (.githooks/commit-msg) before commit.

```
Criterion: 1
Proof:
- Red commit: <hash of the pushed red commit for criterion 1>
- Test: <test id>, was Pending in <red hash>, Pending removed here, body byte-identical
- Run before: failing at <red hash> (output line quoted)
- Run after: passing at this commit (command and pass count quoted)
- Zero case: empty or missing command-scan source fails (test id)
- Size: <n> files, <n> changed lines (limit 15 / 400)
```

D154 says evidence must cite a pushed hash that contains it. A commit cannot contain its own hash, so the push confirmation (the green hash on origin) goes in the next record or message, not in the commit body.

## Push step
1. Run the criterion 1 test and the existing suite. Run the scripts/check-unsafe-deletes.py pre-commit hook. Commit once with the Proof block.
2. `git push` at once (D157, D169). Do not start the red for criterion 2 first.
3. Confirm with `git status -sb` and `git log origin/<branch> -1` that the green hash is on origin. Report that hash as the evidence.
4. If the push fails, fix it and retry. Do not report done until the hash is on origin.
5. All of this uses git only. No recursive delete is run at any point.

Size: 5201 bytes
