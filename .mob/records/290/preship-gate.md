# PRE-SHIP GATE — Card 290 (ctx-5, independent reviewer)

## 1) Done checks vs committed evidence (adopted condition)
Adopted condition requires both a recognised vote line and a D171-valid CITE; anything else is unread/left out (`.mob/records/290/record-in-progress.json:7`, `.mob/records/decisions/290-taskforce.json:18`, `docs/decisions/process-and-staffing.md:667-670`). In code, counted is `vote != "none" and cite_ok`; result input includes counted votes plus never-answered empty replies only (`C:\Users\<user>\mob-crosscli\decider.py:42-53`, `C:\Users\<user>\mob-crosscli\decider.py:135-149`, `C:\Users\<user>\mob-crosscli\decider.py:352-354`). `result_of({})` fails with `failed: no counted votes` (`C:\Users\<user>\mob-crosscli\decider.py:79-81`).

1. **Done check 1 — met.** Uncited Yes is uncounted and cannot pass, including beside cited votes (`board/290-the-decider-s-result-counts-only-votes-with-a-valid-cite.md:19`; evidence commits `38b8eb5`, `7e0f200`, `93c4f62`, `8f88c93`, `640c41e`; tests at `C:\Users\<user>\mob-crosscli\tests\test_decider_counted_result.py:90`, `:116`, `:120`, `:473`; run result below: module passed).
2. **Done check 2 — met.** Cited Yes passes and cited No fails as before (`board/290-the-decider-s-result-counts-only-votes-with-a-valid-cite.md:20`; evidence commit `78ab63b`; tests at `C:\Users\<user>\mob-crosscli\tests\test_decider_counted_result.py:356`, `:364`, `:375`, `:386`; run result below: module passed).
3. **Done check 3 — met.** Doubles now emit valid cites and decider modules pass (`board/290-the-decider-s-result-counts-only-votes-with-a-valid-cite.md:21`; evidence commits `3d503c8`, `73eea8a`; helper at `C:\Users\<user>\mob-crosscli\tests\fakes\fake_voter.py:22-30`; cite tests at `C:\Users\<user>\mob-crosscli\tests\test_fake_voter_cites.py:58-107`; record says all 12 modules passed on green 3: `.mob/records/290/record-in-progress.json:26`).

## 2) Criterion order in git, caps, and D181
Commit order is exactly: `38b8eb5` (red 1) -> `7e0f200` (red 1b) -> `93c4f62` (green 1) -> `78ab63b` (red 2) -> `3d503c8` (red 3) -> `8f88c93` (red 1c) -> `640c41e` (green 1b) -> `73eea8a` (green 3), matching the card record (`.mob/records/290/record-in-progress.json:12-14`, `:16`, `:18-19`, `:22`, `:24`, `:26`). Each listed commit body carries `Criterion:` and `Cycle` lines (verified directly in commit messages for these hashes).

Red-before-green ordering is satisfied for the implemented criteria (1 and 3), with criterion 2 intentionally a pinned-red behavior check (`.mob/records/290/record-in-progress.json:18`). Caps were 1+1+1 after scope and raised to 2+1+1 by scope-2 (`.mob/records/290/record-in-progress.json:13`, `:21`; `.mob/records/decisions/290-scope-2.json:181`). D181 is respected: red 1b stayed before any green, and red 1c is cycle 2 after green 1 with a window first (`docs/decisions/process-and-staffing.md:844`, `:859-860`; `.mob/records/290/record-in-progress.json:21-22`).

## 3) Judgment on the three facilitator readings
1. **Declaration-fix “as none” vs green 1 left-out:** honestly recorded as a drafting contradiction and deviation, with green 1 aligned to adopted task-force/slice condition (`.mob/records/290/record-in-progress.json:61`, `:16`; `.mob/records/decisions/290-declfix.json:12`, `:140`). **Acceptable.**
2. **Green 1b implemented scope-2 Intent instead of literal wording:** explicitly recorded in `green_1b_brief` with rationale (literal wording would break red 1 E cases) and implemented at call-site only (`.mob/records/290/record-in-progress.json:23-24`; `.mob/records/decisions/290-scope-2.json:12`, `:181`). **Honest and acceptable.**
3. **Green 3 rewrote one test exit assertion to new rule (slice C3 ZOMBIE E):** recorded in decision text and record, and visible in test assertion (`.mob/records/290/record-in-progress.json:26`; `C:\Users\<user>\mob-crosscli\tests\test_decider_record.py:142`, `:144`). **Honest and acceptable.**

## 4) Required runs (D176 Environment lines)
**Environment:** shell=PowerShell; launch form=`python -m unittest tests.test_decider_counted_result tests.test_fake_voter_cites tests.test_decider_launched tests.test_decider_gate tests.test_decider_pin_scrub tests.test_decider_record tests.test_decider_record_units tests.test_decider_runner tests.test_launch_failure_honesty tests.test_repo_path_lengths`; working directory=`C:\Users\<user>\mob-crosscli`; commit=`73eea8a16707bc924fb34c29466877e52bea8a6e`.
Result: `Ran 236 tests in 132.517s` — `OK` (with ResourceWarning noise only).

**Environment:** shell=PowerShell; launch form=`python scripts/mob_process.py check`; working directory=`C:\Users\<user>\projects\phrase-lang`; commit=`ade67fdcf79fa6775c8cb3043a3b320047bc7881`.
Result: `mob-process: OK (106 closed cards, 15 records, 91 legacy)`.

Full-suite attestation only (not re-run): facilitator reference suite on `73eea8a` is recorded as `Ran 1788 tests ... OK (skipped=1)` (`.mob/records/290/record-in-progress.json:28`).

## 5) Deviations and blockers
Deviations are explicitly recorded, including declaration-fix wording error and green-1 regression discovery path (`.mob/records/290/record-in-progress.json:56-63`). I find the deviation record honest and sufficiently specific. **No ship blocker found.**

## 6) Safety checks
- **Seat scan entry present:** seat records include scan status entries (`.mob/records/290/record-in-progress.json:34`, `:40`, `:46`, `:52`).
- **Every listed commit pushed:** verified all `38b8eb5`, `7e0f200`, `93c4f62`, `78ab63b`, `3d503c8`, `8f88c93`, `640c41e`, `73eea8a` are contained in `origin/main`.
- **No team writes under `.mob/audits/`:** `git log -- .mob\\audits` returns no entries in this records repo check.
- **No delete/stash commands by seats in card-290 records:** no matches in `.mob/records/290` for `rm`, `del`, `Remove-Item`, `rmdir`, `git clean`, `git reset --hard`, `git stash`, `shutil.rmtree`.

## Verdict
**SHIP** — no blocking findings.