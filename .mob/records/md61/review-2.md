# mob-defaults #61 review round 2, card-61 at b64725f

VERDICT: SIGN-OFF; new BLOCKING: 0; new MINOR: 5

QA seat: claude opus = claude-opus-5-5, ctx-4b, a fresh throwaway clone (`reviewmd61r2-clone`, HEAD b64725f). Diff reviewed: `145c5d1..HEAD`. Probes:
- `reviewmd61r2probe/test_probe_r2.py`: seat turns T1-T6, Decider D7-D9, timing.
- `reviewmd61r2probe/probe_real_history.py`: compares 4c1addd and HEAD on two real 2,633/2,671-row histories, read-only.
- The round 1 probes in `reviewmd61probe`, re-run against this clone. The seat_log work dir is `reviewmd61r2probe/work1`.

The required test set passed: 263 passed, 48 subtests.

## Round 1 findings, re-checked

| R1 # | status at b64725f | replay |
|---|---|---|
| B1 one failed clone dropped every outcome | fixed: 4 launches, 4 outcomes (codex, copilot and agy `success`; grok `timeout`) | `test_probe_decider.py::test_d1`, `test_d5` |
| B2 a `turn` launch counted whatever its outcome | fixed: P6 and P6b give -1. P6c now refuses Claude and names grok. A seat turn writes its own outcome (T1, T5) | `probe_rotation.py` P6/P6b/P6c, `test_probe_r2.py` T1 |
| B3 two roles on one model shared one outcome | fixed: each role has its own context (`D169-PM`, `D169-QA`), launch and outcome. Result: `(codex, quota_exhausted)`, `(codex, success)`, `(grok, success)` | `test_probe_decider.py::test_d3`, D8 |
| B4 quadratic `last_seating` | fixed: n=10,000 launch-only rows take 0.01 s; 26,667 mixed rows take 0.16 s for wait plus reason (linear) | `probe_rotation.py` T, `test_probe_r2.py::test_timing_real_size` |
| M5 outcome write crash lost the record | fixed for a refused append (exit 5, record written, problem in `integrity.changes`). Not fixed for a refused lock: see new finding 1 | `test_d4`, D7 |
| M6 a voter exception gave 0 outcomes | fixed: 4 outcomes, grok `no_vote` | `test_d2` |
| M7 two outcomes for one launch | fixed: P12b gives -1 | `probe_rotation.py` P12b |
| M8 seat_log outcome ignored `--card` and allowed orphans | fixed: S5 and S6 exit 2 and write nothing | `probe_seat_log.py` S5, S6 |
| M9 non-finite or negative duration | fixed: nan, inf, -5 and 1e309 each exit 2 | S1 |
| M10 empty identifying arguments | fixed: exit 2, nothing written | S2, S2b |
| M11 | ruled a named limit (to #60); not re-reported | n/a |

## New findings

| # | finding with a minimal input | replay | class with reason |
|---|---|---|---|
| 1 | **The Decider's outcome phase lets a seat-use lock refusal escape.** The `with gate.seat_use_lock(...)` that wraps the per-voter outcome writes (decider.py, after the vote) sits outside the `try/except SeatRefused` that collects `outcome_errors`. If the lock is held by a live process for more than `SEAT_USE_LOCK_WAIT` (30 s) at that moment, `SeatRefused` propagates out of `run()`. The result: no outcome lines and no `decider-result.json`, so the decision record is lost. That is the case ruling F1-2 (MINOR 5) says must never happen. Input: the second `seat_use_lock` call raises `SeatRefused`. | `test_probe_r2.py::test_d7`: `exit=raised SeatRefused outcomes=0 result_file=False` | MINOR, new: needs a live lock holder for 30 s or more in that window, which is unusual. The history stays correct (a crash means "not served", per [B]), and no line is lost. Fix: catch it into `changes`, as `verify_history` already does. |
| 2 | **A seat turn can crash after its backend ran.** `seat_turn` now takes the seat-use lock and calls `append_seat_outcome` at turn end, with no handling. A lock refusal, or a history that stopped being append-only, raises `SeatRefused` after the turn's work and its MOB.md entry are done. The card run stops, and the served seat has no outcome. | T4: `raised SeatRefused`, history `[launch]` only | MINOR, new: the same unusual trigger as #1. Rotation fails safe (the seat counts as not served). Before this change, a turn could not fail after its backend returned. |
| 3 | **`seat_turn` now swallows every `Exception` from `run_backend`** (`call_backend`) and turns it into a failed turn with outcome `no_vote`/`timeout`/`quota_exhausted`. Before, the exception stopped the card run. Now a programming error (TypeError and the like) in the launch path is masked. The run goes on: a failed navigator or voter gives a "none" or FLAG verdict, and a failed driver stops the stage. | T2: `(False, 'RuntimeError: cli vanished')`, outcome `no_vote` | MINOR, new: rotation is correct (not served). It is a control-flow change outside the card's scope. Ruling F1-1 asked for it only on the Decider's voter path. |
| 4 | **"Every seat turn writes its outcome" does not hold on the unavailable-seat path.** `gate_seat` writes the launch line, then `seat_turn` returns early with `[facilitator] seat 'X' is not available` when the backend is missing or disabled, before the outcome code. The result is a launch line with no outcome. The dry-run return is the same shape when probes are present. | T3: history `[(claude, ctx-1, turn, launched)]`, no outcome | MINOR, new: rotation is correct, because a launch with no outcome counts as not served, which is right for a seat that never ran. It is only a data gap: the row reads as a crash, not as `refused`. |
| 5 | **The reader pairs outcomes without the role; the writers pair with it.** `last_seating` keys pending launches on (seat, card, context). `append_seat_outcome` (CardRun, Decider, seat_log) also filters on `role`. With two roles running concurrently in one context on one service, QA's success is paired to UX's later launch. Input: `[L(grok,x,QA), L(grok,x,UX), O(grok,x,QA,success)]` gives `last_seating = 1` (the still-running UX launch), where the writer meant line 0. If a Claude seating falls between the two launches, the rule lets Claude in although grok has waited longer. | inline probe R1/R2 (`last_seating 1`) | MINOR, new: needs two roles sharing one context at the same time, which R-ROT's fresh-context-per-seat rule excludes, so it is contrived. It matches ruling F1-2's letter ("same card, context and seat"). Fix if wanted: add `role` to the key. |

## What held

- **Seat turns.**
  - A task-force `Seat` turn writes its launch and its outcome with the Seat's own context (`ctx-1`), and the launch is a seating only after `success` (T1).
  - A failed turn is not a seating, and rotation stays correct after it (T5).
  - String-seat turns pair in order (T6).
  - Turns run one after another (`checkpoint` and `mobbed_stage` loop), so the per-run, non-reentrant lock is never contended across threads.
- **Per-voter pairing.**
  - One outcome per launched voter on every path: counted vote, no vote, failed clone, voter exception, plain tuple, and roles mode with two roles on one model (D1-D6, D8).
  - Each outcome is written to its own voter's context and service, including a role named like a model (`codex=grok,grok=codex`, D8).
  - Unlaunched voters get no outcome (the card's test).
- **The single-pass `last_seating`.**
  - The LIFO pending stack per (seat, card, context) gives every round 1 rotation shape the same result as round 1's "held" list (P1-P10), and P12b is now -1.
  - Non-dict rows are skipped.
  - The read count test (at most 2 reads per row) passes. `claude_wait_problem` now calls `last_seating` once per backend. Time is linear.
- **Old rows.**
  - On two real histories (2,633 and 2,671 old-shape rows), 4c1addd and HEAD give identical `last_seating` for nine backends, an identical `rotation_reason` text and an identical `claude_wait_problem`. That takes 0.009 s per pair.
  - `card_seat_log` and `cycles_from_evidence` read only `card` and `cycle`, so outcome lines do not affect `mob_process.py record`.
- **Append-only.**
  - No code path rewrites `seat-use.jsonl`. The fold is gone, and every history write in source is `open("a")`.
  - A Decider run on an old history keeps the old bytes as an exact prefix and adds 8 lines (D9).
  - 12 concurrent `seat_log.py launch` and then 12 `outcome` processes all exit 0, with 24 parsing rows (S10, S10b).
- **`seat_log.py` refusals.**
  - Empty or whitespace-only identifiers are refused, and so are non-finite or negative durations, an orphan outcome and an outcome for another card.
  - A non-UTF-8 or unparsable history is refused and left byte-identical (S4, S8).
  - Blank lines are tolerated (S7). A newline inside an argument stays one escaped row (S3).
- **Test selectors.**
  - `tests/test_seat_rotation.py` changes only selectors: `used()` and the new `launches()` skip outcome lines, `len(self.history())` became `len(self.launches())`, and `history()[0]` became `launches()[0]`. Every asserted value is unchanged.
  - `tests/test_decider_launched.py` adds only a `lines()` override that skips outcome lines. The `stored()` byte comparisons are untouched. No decision assertion changed, per FX1-3.
