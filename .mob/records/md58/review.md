# mob-defaults #58: QA review

Verdict: CHANGES

Seat: QA review, claude-opus-5-5, context ctx-58q (fresh, not the author). Reviewed `git diff 56d8f41..HEAD` on branch card-58 in `~/projects/wt-md58` (a101237 red, bb28fce red fix, 3139262 green) against the card body, ruling R58-1, the slice and the green record. No file changed except this one.

Runs (`--capture=sys`): `tests/test_rotation_usable_now.py tests/test_seat_rotation.py tests/test_seat_rotation_rules.py tests/test_decider_rotation.py`: 80 passed, 19 subtests passed. The #61 failures at the base were not counted. Each finding below was reproduced with a scratch script that drives the branch code (`check_seat`, `gate_seat`, `decider.start_voters`, `claude_wait_problem`, `rotation_reason`) over the test file's own doubles and over the real `core/backends.json`.

What holds: [Z][O], the gate half of [M][B], [I] (with R58-1: codex stays out of the refusal, and qwen and cursor are named with their reasons), [E] in `quota_reasons`, and [S]. The template wording is accurate. `gate_bound: true` on qwen is correct. The refusal has no special case keyed on row shape or on the shape of a probe double.

## Findings

### 1. BLOCKING: [M][B] "cursor without a model pin" is met only by the tests, not by production
- Where: `core/backends.json` (cursor, lines 83-93); `mob_process.py:251`.
- `not_usable_now` reports "no model pin" only when the config holds `"pinnable": false`. No backend in `core/backends.json` sets that key; the green record confirms "No `pinnable: false` anywhere". The real cursor entry has no `{MODEL}` in its invocation, and its notes say "No {MODEL}: the model is the account's Cursor default". That is exactly the card's pinless cursor.
- Failure scenario (reproduced with the real backends.json, gate busy, every other model seated after Claude, cursor never seated): the Claude seat is refused with "cursor has waited longer ... Seat it on cursor instead." This is the D259 refusal the card exists to stop. The pin rule is a key that only the tests' backend doubles carry. That is close to what R58-1 forbids: behaviour that depends on a key only a double has.
- Fix direction: derive "pinnable" from the launcher (for example, the invocation the seat will use takes `{MODEL}`, as the slice's design point (c) and #57 have it), or add `"pinnable": false` to cursor in backends.json and say why in its notes. Either way, the real config must drive the decision.
- Suggested test: load the real `core/backends.json`, give every enabled backend an ok probe, seat every non-Claude model except cursor after Claude, and assert that `claude_wait_problem(..., backends=real, gate_is_busy=False)` is `''` or at least does not name cursor as the seat to use instead. A second test: a backend config whose invocation has no `{MODEL}` and no `pinnable` key is skipped as "no model pin".

### 2. BLOCKING: the Decider record's reason disagrees with the decision; voter_record still uses the old rule
- Where: `decider.py:385` (`voter_record` calls `rotation_reason(probes, before)` without `backends` or the gate state).
- Failure scenario (reproduced: the [M] setup, gate busy, `decider.start_voters(run, ["claude"])`): `run.quota_reasons` carries "no other model can be seated now; skipped: ...", but the vote record's `rotation_reason` does not. The record says only "Claude was the least recently used available model", while qwen and cursor show `ok`, "last seated never". The record documents itself as "What an auditor recomputes a voter's turn from". An auditor who recomputes `claude_wait_problem(results, expected, before)` from it gets a refusal (reproduced). The record also stores no gate state and no per-model skip reason, so the start cannot be recomputed from it at all. This is the "path that still uses the old rule" the brief asks about. The card names the Decider explicitly ("a Decider keeps two services").
- The same disagreement shows outside [E]. When some usable model exists but has waited less, Claude starts because qwen or cursor was skipped. Neither `quota_reasons` nor the record says so (the note is appended only when `usable == 0`, `mob_process.py:322`), so the reason lists a never-seated, probe-ok model and still calls Claude "the least recently used available model".
- Fix direction: pass the same `backends` and the same gate answer to `voter_record`'s `rotation_reason` (see finding 3: one gate answer per seat). Have the reason name every skipped model whenever a skip changed the outcome, not only when nothing is usable.
- Suggested tests: (a) [M] setup, gate busy, `start_voters(["claude"])`: assert `records["claude"]["rotation_reason"] == run.quota_reasons[<claude context>]` (both scrubbed), or that both carry the skip note. (b) The [Z] setup (codex usable and seated after Claude, qwen gate-busy and never seated): assert the recorded reason names "qwen (gate busy)".

### 3. MINOR (close to blocking): the gate is asked twice per Claude seat, and the answer can change between the check and the commit
- Where: `facilitator.py:1787` (check_seat) and `facilitator.py:1834` (commit_seat), both calling `gate_is_busy_now()`.
- Failure scenario (reproduced: [M] setup, gate seam returns busy then idle): `check_seat` lets Claude start because qwen is gate-busy. By commit time the gate is idle, so the recorded reason has no skip note and does not mention qwen, and qwen looks like a model that "waited longer" and was passed over. The opposite flip (idle, then busy) gives a reason saying "no other model can be seated now" for a seat that was judged with qwen usable. Real trains start and stop, so the window is real. In a Decider both calls happen under `seat_use_lock`.
- Cost: on Windows, `local_driver.gate_busy` runs a PowerShell `Get-CimInstance` listing with a 30 s timeout. It is skipped when no backend is `gate_bound` (good), but it runs twice for every Claude seat, and in `start_voters` it runs while the seat-use lock is held (2 calls per Claude voter, measured).
- Fix direction: take the gate answer once in `check_seat` and carry it in the outcome that `check_seat` already returns to `commit_seat` (and on to `voter_record`), so the decision and every reason use the same answer.
- Suggested test: a gate seam scripted `[True, False]`. Assert it is called exactly once per Claude seat, and that `quota_reasons` names "qwen (gate busy)".

### 4. MINOR: an exception from `gate_busy` escapes as a raw error, not as a refusal
- Where: `facilitator.py:1797`.
- Failure scenario (reproduced: the seam raises `OSError`): `check_seat` raises `OSError` out of `check_voters`. A Decider aborts with a traceback instead of `SeatRefused`, or instead of a defined fallback. The real `gate_busy` swallows listing errors, but `gate_lock_active`'s `stat()` can still raise (for example, on a permission error or a race with the lock's removal).
- Fix direction: decide the fallback and test it. Treating the gate as busy (so qwen is skipped, with the reason "gate state unknown") matches qwen's own launcher, which refuses to load while it is unsure.
- Suggested test: a seam that raises. Assert a defined outcome (SeatRefused with a readable reason, or a start whose reason names "gate state unknown"), never a raw exception.

### 5. MINOR: rotation_reason and claude_wait_problem disagree on which models count
- Where: `mob_process.py:311` (`cfg.get("enabled", True)`) against `facilitator.py:1785` (`expected` uses `cfg.get("enabled", False)`).
- Failure scenario (reproduced): a backend that has a probe result but no entry in `backends` counts as "usable" in `rotation_reason`, so the [E] note is suppressed. The decision (`claude_wait_problem`) never counted that model.
- Fix: give `rotation_reason` the same `expected` list, or the same `enabled` default, as the decision.
- Suggested test: probe results that include a backend absent from `backends`, all other models skipped. Assert the [E] note is present.

### 6. MINOR: the gate check reads only the run's own repo lock
- Where: `facilitator.py:1797` (`self.gate_busy(self.args.repo)`).
- The card's case is "a play-tracker train alive". `gate_busy` sees the run repo's `.mob/gate.lock` and dotnet test processes anywhere on the machine. It does not see another repo's `gate.lock`, nor the cross-mob heavy lock from design point (b) of the slice. If the play-tracker train is a dotnet test run, it is caught. Otherwise qwen is counted as usable while its launcher would exit 75. Confirm which case applies, or read the gate repo qwen's launcher itself uses (`MOB_GATE_REPO`).
- Suggested test: a run repo with no lock and a second gate repo with a fresh lock. Assert qwen is skipped as "gate busy".

## Test adequacy
- The red tests would catch a wrong refusal, but not findings 1 to 3. Every test builds its backends with `backend_cfg(pinnable=False)`, so none touches the real config. None asserts the Decider record's reason. None asserts a single gate call per seat, or that the gate is not called when no backend is gate_bound.
- `test_58_e` checks only `quota_reasons`. The [E] wording "the log says so" should also hold for the vote record and for the record's `task_force[].quota_reason` (which `quota_reasons` does feed).
