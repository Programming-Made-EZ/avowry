# RE-GATE — Card 276 (ctx-5, independent reviewer)

## 1) Own scan (required re-scan)
Environment (D176): shell=PowerShell (Copilot CLI tool runner); launch form=`python -c "from tests.muse_real_logs import copy_muse_real_fixture; from command_source import scan_entry; ..."`; working directory=`C:\Users\ringe\mob-crosscli`; commit=`839380be9b260a3735ce6807c1937580af0abc1c`.

Using `tests/muse_real_logs.py` (`copy_muse_real_fixture`) and `command_source.scan_entry(..., root=<fixture data root>)` at `839380b`:

| Session | status | commands | other_tools | sha256 |
|---|---:|---:|---:|---|
| e338c0ee-ba55-4c61-9106-2b2546c9f686 (vote) | clean | 0 | 3 | b066f64aad497dc6a74fe0dee6df168222c15dc49b2ead27bbfa1b5af19d0f61 |
| a7505b9d-8f2b-4cad-9bfa-aeb467f9da2b (capture) | clean | 3 | 0 | 29240089c474cbbf6f756189ea93f16cb1110c590728ef1b80c19e827086a0f0 |

Done check 3’s vote-seat condition (“returns a cited vote and scans clean”) is met by this gate’s own scan of e338c0ee (`board/276-muse-code-joins-the-mob-as-a-trial-driver.md:21`).

Byte identity against the original evidence source: the committed fixture is **not byte-identical** to the current live top-level session file for e338 or a750 (fixture/live SHA mismatch), while both e338 subagent files are byte-identical. I can verify the evidence session id/path and prior no-source state (`.mob/records/276/muse-trial-seat/evidence.json:10,13,22`) and that fixtures were intentionally scrubbed (`.mob/records/276/record-in-progress.json:31`), but I cannot prove historical byte identity to the exact bytes read during the original vote run.

## 2) Done checks vs committed evidence
1. **Done check 1 — met.** Install and sign-in are recorded (`.mob/records/276/record-in-progress.json:8-9,13`; board criteria at `board/276-muse-code-joins-the-mob-as-a-trial-driver.md:19`).
2. **Done check 2 — met.** Probe criterion and shipped evidence are recorded (`.mob/records/276/record-in-progress.json:14`; board criterion at `board/276-muse-code-joins-the-mob-as-a-trial-driver.md:20`).
3. **Done check 3 — met (re-gate evidence).** Prior gate HOLD was “partly met” (`.mob/records/276/preship-gate.md:3,6-7,19-21`), but this re-gate’s own scan now returns clean for the vote log and command extraction for capture log, matching scope-4 re-gate expectations (`.mob/records/276/turn-scope4-architect.md:27,32`).

## 3) Criterion order in git since first gate, with cap accounting
First gate HOLD reference: `.mob/records/276/record-in-progress.json:23`.

Observed commit order after that gate (by git timestamp):
- `5b3292e` red → `2e56d9f` green
- `25d50b8` red (lettered 4b) → `7c04e2e` green
- `58b5c26` red (lettered 4c) → `0000715` green
- then refactor `fdfa821`, fix `839380b`

Each commit body carries `Criterion: 3` and cycle lines (messages for these hashes). Scope ruling is explicit that lettered reds 4b/4c are actually cycles 5/6 past prior cap 4, then ratified in scope-4 (`.mob/records/276/record-in-progress.json:35,37`; `.mob/records/decisions/276-scope-4.json:4,12,321`). Caps were raised and ratified to `1+1+6 = 8` (`.mob/records/decisions/276-scope-4.json:12,321`; `.mob/records/276/turn-scope4-architect.md:17`).

## 4) Required runs (each with Environment line)
Environment (D176): shell=PowerShell; launch form=`python -m unittest tests.test_muse_install tests.test_muse_probe_state tests.test_muse_command_source tests.test_muse_cycle2_scope tests.test_muse_cycle3_scope tests.test_muse_cycle4_scope tests.test_muse_cycle4b_real_logs tests.test_muse_cycle4c_condition tests.test_repo_path_lengths tests.test_command_scan_wiring tests.test_command_source tests.test_command_scan_files_2b tests.test_probe_model tests.test_quota_first`; working directory=`C:\Users\ringe\mob-crosscli`; commit=`839380be9b260a3735ce6807c1937580af0abc1c`.
Result: `Ran 169 tests in 30.553s` — `OK (skipped=1)`.

Environment (D176): shell=PowerShell; launch form=`python scripts/mob_process.py check`; working directory=`C:\Users\ringe\projects\phrase-lang`; commit=`c622f39682aeb1c97f1a7e565447759d5086abb7`.
Result: `mob-process: OK (104 closed cards, 13 records, 91 legacy)`.

Attestation of facilitator full run on 839380b from record (not re-run here): `Ran 1643 tests ... OK (skipped=1)` (`.mob/records/276/record-in-progress.json:36`).

## 5) Deviations since first gate: honesty and blockers
Recorded deviations are explicit and match the asked set: argument-refusal on `muse exec` (`...record-in-progress.json:166`), trimmed fixtures (`:167`), resumed context compaction/lost brief (`:168`), Windows path-length failure (`:169`), two reds past cap (`:170`), guard false positive on read-only command (`:185`), sandboxed shell timeout/non-completion (`:29`).

These are honestly documented; none is currently shipping-blocking for card 276 after scope-4 ratification (`.mob/records/decisions/276-scope-4.json:321`) and this gate’s clean vote-log scan.

## 6) Safety checks
- Scan entries present for seat records and decision windows (`.mob/records/276/preship-gate.md:56`; `.mob/records/276/record-in-progress.json:50-155`; `.mob/records/decisions/276-scope-3.json:29-597`; `.mob/records/decisions/276-scope-4.json:24-307`).
- Card 276 subject commits are pushed (verified for `5ef4b8f`..`839380b` as ancestors of `origin/main`).
- No team writes under `.mob/audits/` in records repo since 2026-11-03 (git path log check returned none).
- No seat-run delete/stash command evidence; the flagged case at 17:04:32 UTC is recorded as read-only false positive (`.mob/records/276/record-in-progress.json:185`; `board/283-the-delete-guard-does-not-flag-a-read-only-python-command.md:9`).
- Muse beyond voting seats: backend exists/enabled (`core/backends.json:67-75`) but role fallback chains do not include muse (`core/backends.json:82-151`), consistent with D179 voting-seats-first (`docs/decisions/process-and-staffing.md:827`).

## 7) Follow-ups 279–283
Each is a card (not a ship blocker for 276):
- #279 reader-real-shape card (`board/279-the-muse-reader-reads-muse-s-real-tool-records.md:9,18`)
- #280 sign-in/usage classification (`board/280-muse-usage-text-reads-as-signed-in-or-signed-in-with-unknown-quota.md:9,18`)
- #281 workspace-trust posture (`board/281-muse-seats-run-with-a-known-workspace-trust.md:9,18`)
- #282 placeholder rewriting bug (`board/282-a-seat-s-prompt-reaches-it-without-placeholder-rewriting.md:9,18`)
- #283 delete-guard false positive (`board/283-the-delete-guard-does-not-flag-a-read-only-python-command.md:9,18`)

Overlap check: card 279 now overlaps what shipped in 276 (scope-4 itself says 279’s three checks already hold on HEAD) (`.mob/records/276/turn-scope4-architect.md:13`; `.mob/records/decisions/276-scope-4.json:12,321`). This is cleanup/rerank work, not a blocker.

## Verdict
**SHIP** — no blocking findings remain for card 276 in this re-gate.
