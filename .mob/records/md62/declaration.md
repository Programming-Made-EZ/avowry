# mob-defaults #62: kickoff declaration (2026-11-16, about 14:45Z)

Probe at 14:43:10Z (all eight): agy, claude, cursor, muse, qwen ok (qwen probe-only); codex, copilot quota-reached; grok unavailable (402).

Worktree wt-md62, branch card-62 at 1ff374f (main after #63).

| Role | Service | Model | Reason |
|---|---|---|---|
| red | claude (Agent subagent) | sonnet = claude-sonnet-5-5 | several small units with fakes (probe rows, a check, a report), mid tier |
| green | cursor (cursor-agent -p --force) | composer-2.5 | first-choice cursor writer (#61, #58, #63 retros) |
| QA | claude (Agent subagent) | opus = claude-opus-5-5 | a refusal rule over history rows; strong tier |
| cross-family review | agy (plan mode, clone) | gemini-3.8-flash-medium | a non-author family |
| sign-off | muse (read-only) | default, effort medium | a service other than the green's and reviewers' |

No-fit reasons for services that probed ok and hold no seat: qwen: probe-only (the model is not loaded by the probe) and gate-bound; this card's gate is a 45-minute full suite, and the open qwen driver finding is unresolved, so it fits no writing or review role here.

R-ROT: claude 2 seats, cursor 1, agy 1, muse 1. No Decider roles planned.

## Slice (binding on red and green)

Pool: `POOL = ("agy", "claude", "codex", "copilot", "cursor", "grok", "muse", "qwen")` in `mob_process.py` (cursor-agent is the `cursor` backend).

1. [O][E] `probe_all.py` probes every POOL service whose backend exists, whatever its earlier rows say (no skip from history), and besides its existing probes jsonl appends one row per service to `<repo>/.mob/records/seat-use.jsonl`: `{"event": "probe", "service", "result", "error", "taken_at"}`. `result` is `ok` for an ok probe, `quota` for state `quota-reached`, else `error`; `error` is the evidence text (empty when ok). Write through one function `mob_process.probe_row(service, result_dict)` (pure) plus an append under the seat-use history's existing lock.
2. [I] Before probing a `gate_bound` backend, `probe_all.py` asks a seam `gate_reason(repo)` (default: `local_driver.heavy_run_reason(repo, locks_dir=~/.mob-locks)`), which returns a reason string when any `heavy-*.lock` there names a live PID, or `local_driver.gate_busy(repo)` is true; else None. When it returns a reason, that service is not launched; its row is `result: error`, `error: "gate busy: <reason>"`.
3. [Z][B][M][I] New `kickoff_check.py`: `python kickoff_check.py --repo R --declaration FILE [--now ISO]`. The declaration is JSON: `{"card": "...", "seats": [{"role": "...", "service": "..."}], "no_fit": {"<service>": "<reason>"}}`. Pure core `mob_process.kickoff_problems(history, declaration, now) -> list[str]`; empty means pass (exit 0); else print each problem and exit 1. Rules:
   - The fresh row of a service is its latest `event: probe` row with `now - taken_at < 60 min`.
   - [Z][B] Every POOL service without a fresh row is a problem naming it ("no probe row in the last 60 minutes"). Never a warning.
   - [M] Every service whose fresh row is `ok` must appear in `seats` or have a non-empty `no_fit` reason; else a problem naming it.
   - [I] A service whose fresh row is `error` with `gate busy` needs nothing more (it is not ok); a `no_fit` reason citing gate busy is also accepted for an ok row.
4. [S] `mob_process.idle_services(history, now) -> list[(service, reason)]`: every POOL service with no `event: outcome`, `outcome: success` row (joined to its launch row's `service`, or the outcome row's own `seat`) in the 24 h before `now`, with the `result: error` text of its latest probe row (or "no probe row"). `seat_log.py idle-report --repo R` prints it for the retro's Model allocation review.
5. No change to `facilitator.probe_backend`, seat launch/outcome rows, or the rotation rule.

## R62-1 (facilitator ruling, 2026-11-16 about 14:55Z): the heavy lock's real format

The red assumed a heavy lock holds a bare PID. Real locks hold `KEY=value` tokens, on one line (play-tracker: `PID=392343 WINPID=36084 UTC=2026-11-16T09:59:13Z OWNER=play-tracker ship train`) or one per line (Avowry's: `PID=…`, `WINPID=…`, `UTC=…`, `OWNER=avowry`). `heavy_run_reason` reads tokens split on whitespace; liveness is checked on `WINPID` when present (Windows process id, what `local_driver.pid_alive` checks), else `PID`; a file whose whole content is one integer is also accepted. The reason names the lock file and its OWNER text. An unreadable or unparseable lock is busy (refuse rather than run over another mob). Tests use both real shapes.

## R62-2 (facilitator ruling, 2026-11-16 about 15:10Z): fix round 1 from review round 1

Opus (review.md) CHANGES: B1 BLOCKING, M1 to M6 MINOR. agy (review-agy.md) lists the same seven findings with the same labels; review.md was in the folder it read, so it is recorded as not independent evidence.

Fix round 1, test-first, all seven:
- B1: probe_all writes its probes jsonl first, then appends the seat-use rows waiting for the lock as other writers do (the default wait); a failure to append is reported on stderr with exit 1, after the jsonl is safe.
- M1: probe_all probes every POOL service present in backends.json, enabled or not (probe_backend already returns a `disabled` result, which maps to `error`); a POOL service absent from backends.json gets an `error` row "not configured".
- M2: idle_services joins an outcome to its launch on `(seat, card, context, role)`, as #61's seating rule does.
- M3: a null or unparseable `taken_at` is not a fresh row; a `taken_at` more than 5 minutes in the future is not fresh; neither crashes. The same for an outcome `ts` in idle_services (ignored).
- M4: kickoff_check takes `--card` (optional); when given, a declaration whose `card` differs is a problem. A seat with an empty role or service does not count as seated.
- M5: `--now` is optional; default the real UTC clock.
- M6: the reason names only the OWNER value up to the next KEY= token; the gate path reads `gate busy: <reason>` once; an idle service whose latest probe was ok reads "probe ok, not seated in 24 h".

## R62-3 (facilitator ruling, 2026-11-16 about 15:22Z): the M3 null case never reaches production

The fix round 1 green stopped on `test_62_f1_m3_a_null_or_garbage_taken_at_is_not_a_fresh_row_and_does_not_crash`: its row helper writes `taken_at: at or ago(10)`, so `None` (and `""`) become a fresh row before production sees them. The green is right and edited no test. The red author changes that helper so only an omitted argument defaults (`ago(10) if at is None` with a sentinel for "omitted"), so `None` and `""` are written as given. No other edit.
