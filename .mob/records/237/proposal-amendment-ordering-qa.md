# Card 237: proposal on how the wiki-trial checker treats the recorded cap amendment (QA, ctx-3)

Intent (D165): give the objection window a costed choice on how scripts/check-wiki-trial.py should treat the cap raise the team adopted, so the checker and the team's own ruling stop contradicting each other. This proposal decides nothing and changes no checker file.

Assumes (D166): the adopted window (.mob/records/decisions/237-cap-fired.json), the ruling 327d635, the cap raise c30b932 and the decider record (77f54a8) are what they say; probe: 2026-11-01 14:43 UTC, I read the checker, the decision record, the decider record and `git show c30b932`, and checked ancestry with git. I did not read the miss key.

## 1. The conflict, with lines

The check, in check_order (scripts/check-wiki-trial.py:143):
- :152-153 first_run = the earliest commit that adds ledger.json or anything under trial/runs/ (git log --diff-filter=A, falling back to the ledger's first commit).
- :154-156 for thresholds, freeze and miss-key: if the LAST commit touching the file is not an ancestor of first_run, report "committed or changed at or after the first run".
- :157-158 if the last thresholds commit is not an ancestor of the ledger's first commit, report "thresholds not committed before the ledger".

Facts from git: run 1 artifacts are 1281b05, the ledger is 5de395a, and c30b932 (the cap raise) comes after both. 327d635 is an ancestor of c30b932, and the decision file was added in 4435877, also before c30b932. So the last thresholds commit is c30b932 and both findings fire. The rule treats every post-run edit as forged order, with no way to say "this one was ruled on first".

The masking, a defect in its own right:
- :149-150 `if any(not log[n] for n in FILES): return` ends check_order after reporting "not committed", so no ordering check runs for the files that do exist.
- :198 `if not findings:` in run() goes further: load() (:53-67) adds a finding for any missing or invalid file, and that one finding skips check_thresholds, check_rows, check_recompute and check_order together.
So today the only printed finding is recompute.json missing, and the ordering failure stays invisible until the last file lands. An ordering failure must not wait on, or hide behind, a missing file: the files that exist (thresholds, freeze, key, ledger) are enough to judge their own order. This fix is needed whichever option is chosen.

## 2. Options

(a) Accept a bounded post-first-run thresholds change, rules in section 3. Cost: about 40 lines of checker logic using a git-show comparison, about 22 tests, one new trust point (the amendment text). Keeps run 1, the key and the 11 pending runs. Residual risk: the checker proves the change was shape-limited, raise-only and ruled on first; it cannot prove the ruling was wise. That stays with the Decider.

(b) Restart stage A from a new run commit, discarding run 1. Does it satisfy the checker? No, not alone. first_run comes from `--diff-filter=A` over ledger.json and runs/; history keeps run 1's add (1281b05) even if the files are removed and re-added, and the last thresholds commit is still c30b932, after it. Only a checker change would fix it (a new run-set anchor). So (b) costs everything in (a), plus a rewritten anchor, 12 runs (about 10M counted tokens), a new freeze, a new key by a non-runner, and the loss of run 1. It also contradicts the adopted reasoning that the rule needs no change, only the cap. Highest cost, no gain.

(c) Better: do (a) as a content comparison, not a commit-order rule. Compare thresholds.json at first_run (git show first_run:path) with HEAD, and allow only the section 3 differences. "Changed after the first run" then means "its content differs", so a whitespace-only touch no longer fails and a real change is judged by what it changed. Freeze and key keep the strict ancestor rule. I fold (c) into (a) below.

(d) Hard-code the commit hash of c30b932 in the checker as an exemption. Smallest, but the checker's trust becomes a typed hash with no check on what that commit changed, so any extra edit in it would pass. Rejected.

## 3. Option (a) rules, as the checker would apply them

Let T0 = thresholds.json at first_run, T1 = at HEAD, C = the last commit touching it.
1. If T0 equals T1: pass.
2. Otherwise every one of these must hold:
   - the keys that differ are only cap_stage_a, cap_stage_ab and an added cap_amendment (added, not edited);
   - both caps in T1 are ints, are >= T0's, and cap_stage_ab >= cap_stage_a still holds;
   - cap_amendment names a decision file .mob/records/decisions/<window>.json that exists in git, added in a strict ancestor of C;
   - cap_amendment names a ruling commit, resolved by rev-parse to exactly one commit, that is a strict ancestor of C;
   - C is the only commit touching thresholds.json after first_run (one amendment, "in one commit");
   - C is an ancestor of the first commit of every run row after the first-run row, so the raise postdates no outcome data other than run 1's cost.
3. The :154-156 and :157-158 findings are replaced for thresholds only; freeze and key keep them.

Text format: c30b932's cap_amendment is prose, not a record. To avoid a second post-run edit, the checker parses the string: `window <name>` maps to decisions/<name>.json and `ruling commit <hex 7-40>` gives the ruling. Both are in c30b932 today. A structured object would need a second amendment that itself must pass these rules, so I do not propose it.

## 4. Hostile inputs (D167) for option (a)

Each is a red test, one per cycle:
1. Lowered cap (either one below T0): reject.
2. Caps equal to T0 with cap_amendment added: pass (no-op raise, record present).
3. Another field changed along with the caps (a delta threshold, blocked_seconds, cap_reached text): reject, naming the key.
4. A removed key, a renamed key, or an added key other than cap_amendment: reject.
5. cap_amendment edited or replaced in a second change: reject.
6. Caps raised, cap_amendment absent: reject.
7. cap_amendment names a decision file that does not exist: reject.
8. Decision file only in the working tree, or added in a commit that is not an ancestor of C: reject.
9. Decision file added in C itself or later (forged to fit): reject.
10. Decision path with `..`, absolute, or outside .mob/records/decisions/: reject.
11. Ruling commit missing, ambiguous as an abbreviation, or a tree or blob hash: reject.
12. Ruling commit exists but is not an ancestor of C (other branch): reject.
13. Ruling commit equals C: reject.
14. Amendment lands after run 2 (a later run row's source commit does not descend from C): reject. This is what the decider's "before run 2" condition protects.
15. Two thresholds commits after the first run, each fine alone: reject.
16. Non-integer, bool, string or negative cap values in T1: reject (the bad-threshold rule must fire on T1).
17. Whitespace-only rewrite after the first run: pass (the old rule failed it; pin it).
18. cap_amendment lacks the window text or the ruling text: reject, no crash.
19. Total counted tokens above the new cap: the existing "cap exceeded" finding still uses T1.
20. Any one file missing: ordering findings still print next to "missing file" (section 5, cycle 1).
21. First_run falling back to the ledger commit (no add under runs/): the amendment is still judged.

## 5. Recommendation

Choose (a), implemented as the content comparison in (c). It is the only option that keeps the adopted window, run 1 and the key, costs no extra runs, and still lets the checker catch a forged or widened edit. (b) fails the checker by itself and costs most; (d) is too weak.

Work, in my own commits under outside-in TDD, one criterion per cycle:
1. Red then green for the masking (hostile cases 20 and 21): check_order and the other checks run on the files that exist, and an ordering finding prints beside "missing file". Fix the :198 gate and the :149-150 early return. This lands first because it is a defect today and makes the real failure visible.
2. One red test per remaining hostile case in section 4, each followed by the smallest change to pass it; first the accept case on a fixture shaped like c30b932, then the rejects.
3. A final run of the full test file and the real-folder check, with recompute.json missing and then present.
Condition: the window records this as an adopted change before I edit scripts/check-wiki-trial.py, since it alters a rule I wrote and the team agreed.

Open for the window: string-parse versus structured field; whether "one amendment commit" is the right limit; whether freeze and key stay strict.

Size: 8623 bytes
