# PRE-SHIP GATE — Card 276

**Verdict: HOLD**

Blocking findings:
- **Done check 3 is only partly met**: the Muse seat was probed and cast a cited vote, but the scan result is **"no command source"**, not clean (`board/276-muse-code-joins-the-mob-as-a-trial-driver.md:21`; `.mob/records/276/record-in-progress.json:22`; `.mob/records/276/muse-trial-seat/evidence.json:3-4,12-13,18,22`; `.mob/records/276/muse-trial-seat/decider-result-2.json:41-42,47`).
- The required subject-repo test command could not be re-run in this context (tooling returned **"Permission denied because no interactive user response was available"**), so this gate cannot freshly attest that run.

## 1) Done checks vs committed evidence

1. **Done check 1 (Install + sign-in): met.**
   - Card requirement (`board/276-muse-code-joins-the-mob-as-a-trial-driver.md:19`).
   - Committed evidence records install facts and sign-in outcome (`.mob/records/276/record-in-progress.json:9,13`; green/refactor commits `2196c58`, `41ecf78`).

2. **Done check 2 (Probe): met.**
   - Card requirement (`board/276-muse-code-joins-the-mob-as-a-trial-driver.md:20`).
   - Committed evidence records distinct probe states and wiring (`.mob/records/276/record-in-progress.json:14`; commits `dbe2e1c`, `d3a9787`, `c71c05f`).

3. **Done check 3 (Reader + first seat): partly met (not fully met).**
   - Card requires cited vote **and scans clean** (`board/276-muse-code-joins-the-mob-as-a-trial-driver.md:21`).
   - Evidence shows **probed=yes, cited vote=yes, scan=no command source** (`.mob/records/276/record-in-progress.json:22`; `.mob/records/276/muse-trial-seat/evidence.json:3-4,9,12-13,18,22`).
   - Meaning for shipping: criterion 3 is not fully satisfied against the board wording; shipping needs either a policy waiver for non-clean scan in this trial or a fix for the real Muse log shape.

## 2) Criterion order in git and cap progression

- Commit bodies carry explicit criterion/cycle markers in red→green order:
  - C1: red `5ef4b8f`, red `970118c`, green `2196c58` (then refactor `41ecf78`) with `Criterion: 1`, `Cycle 1 of 1` (tool capture file `C:\Users\<user>\AppData\Local\Temp\1793689940682-copilot-tool-output-17732-d44270f7-63bc-4818-83f1-2666b9443ebf.txt:1-43`).
  - C2: red `dbe2e1c`, green `d3a9787` (then refactor `c71c05f`) with `Criterion: 2`, `Cycle 1 of 1` (same file:66-87).
  - C3: red `ac5a5a2`, red `fd09aaf`, green `40d0d08`, refactor `5157e3f`; then cycle 2 red `244678b`, amendment `c0354cb`, green `cefaf4e`; then cycle 3 red `228d2df`, green `2f34856` with `Criterion: 3`, `Cycle 1/1`, `2/2`, `3/3` (same file:99-130,152-183,195-217).
- Cap raises are explicitly recorded and adopted:
  - Slice cap baseline `1+1+1=3` (`.mob/records/276/turn-slice-architect.md:46`).
  - Scope raise to `1+1+2=4` (`.mob/records/276/turn-scope-architect.md:35`; adoption `.mob/records/decisions/276-scope.json:611`).
  - Scope-2 raise to `1+1+3=5` (`.mob/records/276/turn-scope2-architect.md:15`; adoption `.mob/records/decisions/276-scope-2.json:381`).

## 3) Required runs (with Environment lines)

1. **mob-crosscli targeted unittest command**
   - Environment: shell=PowerShell tool runner; launch form=`python -m unittest tests.test_muse_install tests.test_muse_probe_state tests.test_muse_command_source tests.test_muse_cycle2_scope tests.test_muse_cycle3_scope tests.test_command_scan_wiring tests.test_probe_model tests.test_quota_first`; working directory=`C:\Users\<user>\mob-crosscli`; commit=`2f348568633f1928bd101ee4b1501bbec1c660bd`.
   - Result: **not executed in this context** due permission gate: `Permission denied because no interactive user response was available.`

2. **records repo process check**
   - Environment: shell=PowerShell tool runner; launch form=`python scripts/mob_process.py check`; working directory=`C:\Users\<user>\projects\phrase-lang`; commit=`656e8413664e0d950dcab424cfdeb0660772159d`.
   - Result: `mob-process: OK (104 closed cards, 13 records, 91 legacy)`.

## 4) Deviations and shipping impact

Deviations are recorded candidly (`.mob/records/276/record-in-progress.json:116-131`).
- Invisible pytest red / reference mismatch is explicitly recorded and corrected (`:121,123`) — not a blocker now.
- Late full-suite regression is explicitly recorded and fixed by `e3608ae` (`:16,128`) — not a blocker now.
- Co-author trailer on green 1 is disclosed (`:125`) — governance note, not a blocker.
- Seat timeout is disclosed (`:130`) — not a blocker.
- Model-id-not-validated is disclosed and traced to refusal; cycle 3 removed `--model` (`:19-21`) — not itself blocking after fix.

## 5) Safety checks

- **Scan entries present:** seat records include command-scan status for committed seats (`.mob/records/276/record-in-progress.json:39,45,52,58,64,70,76,82,88,94,100,106,112`) and decision windows carry `command_scan` records (counts: taskforce 29, slice 18, scope 30, scope-2 18).
- **Commits pushed:** every listed subject commit is contained in `origin/main` (`5ef4b8f` through `2f34856`).
- **No `.mob/audits/` writes by this team window:** no recent log entries under `.mob/audits/` for this timeframe.
- **No delete/stash commands found in card 276 record tree:** no matches for `rm`, `del`, `Remove-Item`, `rmdir`, `git clean`, `git reset --hard`, `git stash`, `shutil.rmtree` under `.mob/records/276`.
- **Muse placement beyond voting seats:** backend is enabled (`core/backends.json@2f34856:67-75`), but role fallback chains shown do not include muse (`core/backends.json@2f34856:79-155`), so no evidence it was expanded into general build-seat routing.

## 6) Follow-ups named in record: candidate vs blocker

- **Reader real shape**: currently a **ship blocker for Done check 3 clean-scan claim** (record itself says metric is scanned-no and reader is blind to real shape: `.mob/records/276/record-in-progress.json:22`; evidence note `.mob/records/276/muse-trial-seat/evidence.json:22`).
- **Sign-in text classification**: valid follow-up card candidate (`.mob/records/276/record-in-progress.json:17,140`).
- **Untrusted-workspace message handling**: valid follow-up card candidate (`.mob/records/276/record-in-progress.json:17,21,140`).
- **Prompt placeholder substitution bug**: valid follow-up card candidate (`.mob/records/276/record-in-progress.json:143`).

---

**Final gate decision: HOLD** until either (a) criterion 3 is satisfied with a clean scan for the Muse vote seat, or (b) an explicit, documented waiver accepts "no command source" for this trial metric and redefines Done check 3 accordingly; plus re-run/attest the required mob-crosscli targeted unittest command in an execution context that permits it.