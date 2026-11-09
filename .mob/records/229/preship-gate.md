# PRE-SHIP GATE — Card 229 (ctx-5, independent reviewer)

## 1) Done checks vs committed evidence (as adopted)
1. **Done check 1 — met.**
   - Adopted requirement (BOTH parts) is explicit in taskforce result: committed over-limit test **and** live Copilot+Grok parsed cited votes with committed evidence (`.mob/records/decisions/229-taskforce.json:12,18,788`; also carried in record: `.mob/records/229/record-in-progress.json:7`).
   - **(a) Committed over-limit test:** red/green pair `ab8f4a5` -> `4674d19` adds and passes long-prompt delivery checks; test fixture and limits are explicit (`C:\Users\<user>\mob-crosscli\tests\test_launch_long_prompt.py:20,49-52,247-256,278-289`).
   - **(b) Live window evidence:** both seat records show parsed votes, `cite_ok: true`, `counted: true`, and one-attempt `attempt_log` (`.mob/records/229/live/copilot/decider-result.json:35,41-45,53-55`; `.mob/records/229/live/grok/decider-result.json:35,41-45,53-55`). Seat-use lines named in evidence are present (`.mob/records/229/live/evidence.md:8`; `.mob/records/seat-use.jsonl:928-929`).
   - Prompt-over-limit requirement is met: committed evidence states 9774 chars built (`.mob/records/229/live/evidence.md:5`), and direct check here matched that (`proposal_chars=9249`, `built_chars=9774`, `built_lines=61`, sha256 `1efdfc...34bcc`).

2. **Done check 2 — met.**
   - Failure honesty + empty guard landed across `858649b` -> `a2bb1d4` -> `d13148e` -> `cda02f2`, with red-before-green order per D182 (`.mob/records/229/record-in-progress.json:12,15,23-24`).
   - Current implementation/evidence includes `failure_excerpt`, `attempt_log`, explicit failure reasons, `cite_ok`, `counted`, and `counted_seats` (`C:\Users\<user>\mob-crosscli\decider.py:119-129,154,165,168,338-340,352`; `.mob/records/229/live/copilot/decider-result.json:41-45,53-54,61-63`; `.mob/records/229/live/grok/decider-result.json:41-45,53-54,61-63`).
   - Scope-2 moved the cited-only overall result rule to bug card 290 by adopted decision; that move is recorded and in-scope compliant (`.mob/records/decisions/229-scope-2.json:12,201`; `.mob/records/229/record-in-progress.json:22`).

3. **Done check 3 — met.**
   - Red/green pair `cef48a7` -> `2d4d24d` completes placeholder-literal handling (`.mob/records/229/record-in-progress.json:27,29`).
   - Placeholder literals and refusal behavior are covered in committed tests (`C:\Users\<user>\mob-crosscli\tests\test_prompt_placeholders.py:9-12,21-23,127-128,137,142-143`).

## 2) Criterion order in git and cap/rule checks
- Observed commit order in `mob-crosscli` is: `ab8f4a5` (red1) -> `858649b` (red2) -> `4674d19` (green1) -> `a2bb1d4` (green2) -> `7c26629` (red2 cycle2) -> `d13148e` (red2 cycle3) -> `cda02f2` (green2 cycle3) -> `cef48a7` (red3) -> `2d4d24d` (green3).
- Each red precedes its corresponding green, and commit bodies include `Criterion:`/`Cycle` lines (verified from git log bodies for all listed hashes).
- Caps were raised as adopted to **1+3+1 = 5** (`.mob/records/decisions/229-scope.json:436`; `.mob/records/decisions/229-scope-2.json:201`; `.mob/records/229/record-in-progress.json:18,22`).
- D181 lettered-red rule is respected: no post-pass same-cycle lettered-red reuse; reds after passed greens are explicit new cycles (`docs/decisions/process-and-staffing.md:844,859-861`; `.mob/records/decisions/229-scope.json:26`; `.mob/records/decisions/229-scope-2.json:4,23,44`).
- D182 QA condition is respected: red 2 committed before green 1 (`.mob/records/229/record-in-progress.json:12`; `858649b` precedes `4674d19`; D182 condition at `docs/decisions/process-and-staffing.md:888`).

## 3) Required runs (D176 Environment lines)
Environment (D176): shell=PowerShell; launch form=`python -m unittest tests.test_launch_long_prompt tests.test_launch_failure_honesty tests.test_prompt_placeholders tests.test_decider_launched tests.test_decider_pin_scrub tests.test_decider_record tests.test_repo_path_lengths tests.test_muse_command_source`; working directory=`C:\Users\<user>\mob-crosscli`; commit=`2d4d24d42484d0c187587b320d59d0467da0fdda`.
Result: `Ran 151 tests in 69.049s` — `OK`.

Environment (D176): shell=PowerShell; launch form=`python scripts/mob_process.py check`; working directory=`C:\Users\<user>\projects\phrase-lang`; commit=`3353c7cde539e784d32eb53d91959187bfe36707`.
Result: `mob-process: OK (105 closed cards, 14 records, 91 legacy)`.

Attestation only (not re-run full suite): facilitator full run on `2d4d24d` is recorded as `Ran 1713 tests ... OK (skipped=1)` (`.mob/records/229/record-in-progress.json:32`).

## 4) No-home-path check (`.mob/records/229/nohome-check/`)
- `decider-result.json` does **not** store a home path or token in failure text; both are masked (`.mob/records/229/nohome-check/decider-result.json:16,24-30`).
- Vote is correctly uncounted on failed launch (`.mob/records/229/nohome-check/decider-result.json:15,38-39,46`).
- `check.json` agrees: `home path absent=true`, `token absent=true`; `contains_secret false=false` is solely from `prompt_sha` pattern matching (`.mob/records/229/nohome-check/check.json:20-24`; explanation is explicit and honest in `.mob/records/229/nohome-check/README.md:10`).

## 5) Deviations: honesty and blockers
Recorded honestly, including the requested set:
- D171 re-asks/round handling are explicitly recorded in taskforce result (`.mob/records/decisions/229-taskforce.json:788`).
- Copilot votes counted in window 229-scope despite declaration exclusion are recorded (`.mob/records/229/record-in-progress.json:63`).
- Round-2 launch without in-flight line is recorded (`.mob/records/229/record-in-progress.json:62`).
- Green 2 cycle 2 could not pass; no commit; failing evidence retained (`.mob/records/229/record-in-progress.json:21`).
- Live step used a different prompt than slice named, recorded as deviation (`.mob/records/229/record-in-progress.json:64`; `.mob/records/229/live/evidence.md:6`).
- Grok narration before vote word is recorded (`.mob/records/229/live/evidence.md:19`; raw reply in `.mob/records/229/live/grok/decider-result.json:37`).

**Ship blocker check:** none of these deviations blocks card 229’s adopted Done checks as currently written.

## 6) Safety checks
- Seat scan entries are present in record artifacts (declared seat scans and per-window `command_scan` fields): `.mob/records/229/record-in-progress.json:38,44,50,56`; `.mob/records/decisions/229-taskforce.json:29,49,69,89,109,129`; `.mob/records/decisions/229-slice.json:24,44,64,84,104,124`.
- Listed subject commits are pushed (verified against `origin/main`): `ab8f4a5`, `858649b`, `4674d19`, `a2bb1d4`, `7c26629`, `d13148e`, `cda02f2`, `cef48a7`, `2d4d24d` => all `pushed=yes`.
- No team writes under `.mob/audits/` found from git path log check in this gate run (no output for `git log -- .mob\audits`).
- No delete/stash evidence in the card record set; one explicit note says a failed attempt was reverted **without delete/reset** (`.mob/records/229/record-in-progress.json:21`).

## 7) Follow-ups
- **Card 290:** should stay open (it is the tracked bug for cited-only result counting, created from this card’s scope decision) (`board/290-the-decider-s-result-counts-only-votes-with-a-valid-cite.md:5,7,9,19-21`; `.mob/records/229/record-in-progress.json:22-24`).
- **Card 270 folded-in closure:** yes, close with 229 shipment per adopted fold decision (`.mob/records/229/record-in-progress.json:8`; `.mob/records/decisions/229-taskforce.json:12,788`).
- **Additional bug-card candidate (D183-consistent):** parser/rule mismatch where Grok can prepend narration and still parse in `decider.py`, while first-line window readers would miss it (`.mob/records/229/live/evidence.md:19`).

## Verdict
**SHIP** — no blocking findings for card 229 pre-ship gate.
