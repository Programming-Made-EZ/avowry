# Card 226 slice: architect (ctx-2, Claude seat)

Sources read: board/226-*.md; mob-defaults decider.py (whole), facilitator.py CardRun (__init__, probe_drivers, reprobe_if_stale, read_seat_use, append_seat_use, gate_seat, probe_backend, Seat), mob_process.py (probe_is_stale, quota_reason, seat_use_line, last_seating, claude_wait_problem, rotation_reason), tests/test_decider_runner.py (the harness), core/backends.json, the 2026-10-30 probe file, card 219's slice for the format. Seat started 2026-10-31 01:36 UTC (in-flight line 21a9f79; last probe age 16 min; cap 02:01 UTC).

Assumes: decider.py voters share card seats' seat-use history (one append-only .mob/records/seat-use.jsonl under --repo, read through CardRun.read_seat_use) and the probe a voter starts on is taken by decider.py itself with the card seats' probe_drivers, not parsed from the hand-written probe file; probe: the current decider.py call path (run -> vote_seat -> facilitator.run_backend: no probe taken, no history read, only `enabled` read from backends.json) and .mob/records/seat-use.jsonl does not exist in C:\Users\ringe\projects\phrase-lang (checked 2026-10-31 01:3x UTC; no commit ever touched it), while .mob/records/probes/2026-10-30.jsonl has the shape {driver, probed_at, available, exit, command, evidence} that no code reads (the code's own probe is {taken_at, results{name:{ok,taken_at,evidence}}} in .mob/<id>/probes.json).

## Where decider.py reads probes and backends today

- Backends: run() reads `--backends` (default core/backends.json) once, `json.loads(...)["backends"]`. vote_seat reads only `cfg["enabled"]` and `readonly_invocation`/`invocation`, then calls `facilitator.run_backend(seat, seat_cfg, "default", prompt, cwd=clone, timeout)`. It never reads `probe_model`, `models` pins, any probe or any history.
- Probes: none. decider.py takes no probe, reads no probe file, and has no clock. A seat with no quota simply returns no vote ("no response") and the run fails after two attempts. The model a voter ran on is kept in `votes[seat]["model"]`, which goes only to the temp decider-result.json (not git).
- The Decider log (`append_log`, written to --log, in git) records seat names, votes and the result: no model, no probe time, no reason.
- Callers: people and the facilitator's Decider and objection-window runs invoke decider.py; facilitator.py only parses the finished record (`decider_result`). Voters and window seats share the one entry `decider.run`; there is no second path to gate.

## How the shared rules are imported without a second copy

decider.py already does `import facilitator`. It adds no copy of any rule. The rules reach it through the one object that already composes them in the right order, `facilitator.CardRun`:

- decider.run builds `facilitator.CardRun(card=<stub with id=ref or "decider", text="">, args=<namespace: repo, dry_run=False, probe_max_age_minutes=STALE_PROBE_MINUTES (imported from mob_process), talk_timeout, stall_seconds, card=None>, backends_cfg={"backends": backends}, clock=<seam>, probe_fn=<seam>)`.
- It calls `run.probe_drivers("decider start")` once (a fresh probe on every run: always under 60 minutes, no new file format to parse) and, per voter, the gate (below). Staleness, the history read and append-only check, `claude_wait_problem`, `rotation_reason`, `seat_use_line`, `last_seating`, `SeatRefused` and its wording all stay in card 208's code and are called, not copied.
- Two attributes are redirected so the real tree is not written by the probe: `run.run_dir` and `run.mob_path` point into the scratch run dir (plain attributes; probes.json and the probe MOB.md block go there). `seat_use_path` is NOT redirected: it stays `<repo>/.mob/records/seat-use.jsonl`, the shared history (the point of the card).
- One small structural change in facilitator.py (commit 3 below, behavior-preserving): `gate_seat` is split into `check_seat(seat, role, pending=())` (reprobe if stale, read history, Claude wait check against history plus `pending` lines; raises SeatRefused; writes nothing) and `commit_seat(seat, role, history)` (append the line, set quota_reasons). `gate_seat` is check then commit, unchanged. Reason: a Decider seats N voters at once; if the first voter's line were appended and a later voter refused, the history would say a seat started that never did and card 208's rotation would be skewed. Checking all voters first and appending only once all pass keeps "refused = no voter launched, no line written".

Seat order inside a run (my reading of D163/D164 for simultaneous voters, named so the reviewer can attack it): non-Claude voters are checked first, in --seats order, and count as just seated (as `pending` lines built with `seat_use_line`) when a Claude voter of the same run is judged. So a Claude voter is refused only while an available model that is NOT voting in this run has waited longer, or the probe has no readable result for an enabled model. A model voting in this run is not "waiting longer". Voters launch only after every check passes; lines are then appended in that order.

Voters whose own probe says no quota are not newly refused (no strictness beyond D163): they behave as today (two attempts, "no response", the run fails). Open point for the PM, not a blocker: if the card wants those refused, that is a fourth check and a split.

Context string for a decision seat: `<ref or "decider">-<seat>` (for example `D169-claude`); role "decider voter"; the seat-use line's card field is the ref, so an auditor can group lines per decision.

## Numbered Done checks (D157; one red-green cycle each)

1. Fresh probe before every decision seat: a probe is taken at the start of the run and re-taken, before any backend call, when the one in force is 60 minutes old or more by the clock at the voter's launch; if the re-probe or keeping its evidence fails, the run is refused naming the seat and the probe age, exit 2, no voter launched, no result file.
2. Least-recently-used rotation: the shared seat-use history decides; a Claude voter is refused (exit 2, no voter launched, no line written) only while another available model not voting in this run has waited longer, or an enabled model's probe result is unreadable; each started voter gets one seat-use line; an unreadable or non-append-only history refuses.
3. The record: each voter's model, probe taken_at and seat-use line, and for each Claude voter the reason built by `rotation_reason`, appear in decider-result.json and in the Decider log entry (the part in git), so an auditor recomputes rotation from the log plus seat-use.jsonl in git.

Recording rule for Done check 3 (how I read the card's "reason" for each voter): the reason is typed by nobody, built from the probe and history, and recorded for Claude voters (card 208's `quota_reason` scope); every voter records model, probe time and its seat-use line index. If the PM wants a reason string for non-Claude voters too, it is one line of text, no new rule.

Each red commit body carries "Criterion: n". Green n before any red for n+1. Greens pushed at once by the facilitator. The "Intent:" line quoting D163 and D164 is committed by QA before red 1 (D165).

## Criterion-by-layer table (outermost first; F-055 doubles named)

Doubles, named once: CLOCK = scripted callable returning datetimes (injected as `clock=`); PROBE = recorded probe double `probe_fn(name, cfg, repo, timeout, stall)` returning `{ok, taken_at, evidence}` per backend and counting calls (injected as `probe_fn=`); BACKEND = a mock of `facilitator.run_backend` that records calls and is never launched (in-process layers) or the existing fake_voter.py subprocess (process layer); HISTORY = a real seat-use.jsonl in a real temporary git repo (the existing RunnerCase repo), seeded by the test; no gh.

| Crit | Layer | Test (new tests/test_decider_gate.py; process-layer cases extend tests/test_decider_runner.py) | Doubles consumed | Red-able or born-green |
|---|---|---|---|---|
| 1 | Acceptance, in process: `decider.run(args, clock=, probe_fn=)` on a RunnerCase repo | `test_a_stale_probe_is_repeated_before_any_backend_call` (CLOCK advances 61 min between the start probe and the voter launch; PROBE called twice; BACKEND called only after the second probe); `test_refused_when_the_reprobe_fails_naming_seat_and_age` (second PROBE call raises; exit 2; BACKEND never called; no decider-result.json; stderr names the seat and the age) | CLOCK, PROBE, BACKEND, real repo | Red-able. decider.run has no clock/probe seam and takes no probe. QA adds the two keyword seams as unused parameters in the red commit so the red is an assertion failure (BACKEND was called, no refusal), not a TypeError. |
| 1 | Process: `python decider.py ...` subprocess (existing `run_runner`) | `test_a_run_takes_a_fresh_probe_and_records_it`: fake backends get `probe_model`; afterwards `<scratch>/probes.json` exists with taken_at inside the run, and nothing under the real repo's `.mob/<id>/` exists beyond seat-use.jsonl | fake_voter subprocess | Red-able (no probe today). |
| 1 | Unit: boundary through the decider path | `test_exactly_60_minutes_is_stale_and_59m59s_is_fresh` (CLOCK at 59:59 gives one probe, at 60:00 two) | CLOCK, PROBE | Born-green guard on card 208's `probe_is_stale` reached through the decider path; kills M1c; mutation run before its commit. |
| 2 | Acceptance, in process | `test_claude_voter_is_refused_while_codex_has_waited_longer_and_nothing_launches` (HISTORY: codex seated, then claude; PROBE codex ok; --seats claude; exit 2; message names the claude seat and codex; BACKEND not called; seat-use.jsonl byte-identical) | CLOCK, PROBE, BACKEND, HISTORY | Red-able after green 1 (green 1 stages only the staleness check, so the Claude voter launches). |
| 2 | Acceptance, in process | `test_claude_voter_runs_when_the_other_available_models_vote_in_the_same_run` (--seats codex,claude, both ok, empty history: both launch; two lines appended, codex first) | same | Red-able (no lines appended before green 2). Pins the seat-order reading. |
| 2 | Acceptance, in process | `test_an_unreadable_probe_result_is_not_read_as_no_quota` (PROBE has no result for an enabled model, Claude voter refused, asked to probe again); `test_a_corrupt_or_rewritten_history_refuses` (bad JSON line; and a voter that removes a line is caught by a second read_seat_use after the run) | same | First: red-able. Second: red-able for the corrupt line; the post-run rewrite half is a born-green guard with M2e. |
| 2 | Unit: `CardRun.check_seat/commit_seat` | `test_check_seat_writes_nothing_and_gate_seat_is_check_then_commit` | probe double and scripted clock from card 208's test_seat_gate fixtures | Born-green structural guard (commit 3). Card 208's test_seat_gate, test_seat_rotation and test_seat_rotation_rules stay green unchanged: they prove the gate order did not move. |
| 3 | Acceptance, in process | `test_the_result_and_the_log_name_model_probe_time_and_reason_per_voter` (decider-result.json votes[seat] and the --log entry both carry model, probe_taken_at and history line index; the Claude voter's reason is `rotation_reason` text naming the probe time and each other model's evidence and last seating; a non-Claude voter carries no reason) | CLOCK, PROBE, BACKEND returning a model, HISTORY, real --log file | Red-able after green 2 (the log has none of these fields). |
| 3 | Process | `test_the_log_entry_alone_lets_an_auditor_recompute_rotation`: read only the log entry plus seat-use.jsonl and rebuild each voter's pass or refusal with `claude_wait_problem` | fake_voter subprocess, real log and history | Red-able. |
| 3 | Unit | `append_log` renders the new fields; an old-shape vote without them still renders (no KeyError) | none | First red-able; second a born-green guard for the existing Logging callers (existing Logging test stays green). |

Layers not consumed, declared so a reviewer does not look for them: no gh, no board, no hook, no facilitator loop change, no UI. test_decider_runner's existing cases stay and pass unchanged; their fake backends have no `probe_model`, so the probe for them is `ok: False` ("no usable probe_model") and non-Claude voters still launch as today.

## Named mutations (each born-green guard cites one; run and recorded in its commit body)

- M1 do not call the probe or gate on the decider path (voters launch as today): the cycle 1 and 2 tests fail.
- M1b gate after submit (voters launched, then the gate refuses): "BACKEND never called on refusal" fails.
- M1c stale test `>` instead of `>=`, or a literal instead of the shared limit: the 60:00 boundary case fails.
- M1d swallow the re-probe exception and carry on: the refused-when-reprobe-fails test fails.
- M2 exempt Claude from the wait check: the codex-waited-longer refusal test fails.
- M2b judge Claude before the same run's non-Claude voters are pending: the same-run test fails (Claude wrongly refused).
- M2c append each line as it passes instead of after all pass (no check/commit split): the refused run's history-unchanged assertion fails.
- M2d read a missing probe result as no quota: the unreadable-result test fails.
- M2e skip the post-run append-only read: the voter-rewrites-history test fails.
- M3 take the reason from the voter's reply text instead of `rotation_reason`: the reason test fails (must contain the probe taken_at and each model's evidence).
- M3b write the new fields to decider-result.json only, not append_log: the log-only auditor test fails.
- M3c copy `claude_wait_problem` (or another shared rule) into decider.py: a source guard test (decider.py has no `def claude_wait_problem`, `rotation_reason`, `probe_is_stale`, `seat_use_line` or `last_seating`) fails. Born-green; kills the second-copy loophole the card names.

## Commit plan (mob-defaults, D136: one concern per commit; every subject cites MD-002, D163, D164 and D157 Criterion where it applies)

0. QA: the Intent line (D165) quoting D163 and D164, before any red.
1. QA red 1: `Card 226 red 1 (MD-002, D163; D157 Criterion 1): decider.run takes a fresh probe and repeats a stale one before any backend call` (body "Criterion: 1"; seams as unused parameters; tests fail by assertion).
2. Backend: unit layers (boundary), then green 1: build CardRun in run(), `probe_drivers`, per voter a staleness-only check before submit; refusal exit 2. Stage: staleness only (no history yet).
3. Backend, structural: `Card 226 refactor (MD-002): CardRun.gate_seat is check_seat then commit_seat, behavior unchanged` (card 208 suites green; no Criterion line).
4. QA red 2 (Criterion 2), then backend green 2: gate order above, pending lines, lines appended after all checks, post-run append-only read.
5. QA red 3 (Criterion 3), then backend green 3: model, probe time and line index per voter, `rotation_reason` for Claude voters, append_log fields.
6. Delivery lead at ship: stage the Decider log entries and `.mob/records/seat-use.jsonl` explicitly (it first exists after the first gated run); one full mob-defaults suite (about 1260 s last measured) with its own in-flight line; no per-commit full runs, only the targeted modules (test_decider_gate, test_decider_runner, test_seat_gate, test_seat_rotation, test_seat_rotation_rules).

## Slice verdict

SHIP AS IS. Three criteria, three cycles, one extra behavior-preserving structural commit, one new test module, edits confined to decider.py and facilitator.py (the split). Named alternative if the reviewer rejects the split or the non-Claude-first order: split the same-run ordering and the check/commit atomicity into a follow-up card (Done check 2 would then hold for single-voter runs only). Not recommended: without it a refused run still writes lines for seats that never started.

## Risks handed to the reviewer (loophole lens)

- A stale probe at voter start: the clock is read at each voter's launch, not once per run.
- A refusal that still launches a voter or still writes a seat-use line (M1b, M2c).
- A typed reason (M3), or any reason a seat can supply.
- An unreadable probe read as no quota (M2d).
- The integrity snapshot: `snapshot(repo)` must be taken after the gate writes seat-use.jsonl, or the run reports its own history write as an escaped voter (exit 5). Expected writes before the snapshot: seat-use.jsonl only; probe files go to scratch. A voter that edits seat-use.jsonl is caught by the post-run read_seat_use, not by the git snapshot (an untracked file's content is not in it).
- Two probe formats exist (the hand file and probes.json): decider.py reads no hand file; do not add a parser for it in this card.

## Domain-expert question (T2)

Should a member with different domain knowledge or context join now? No. What this slice needs is card 208's gate and rotation rules plus decider.py's run path, which the architect and backend developer hold; the risks are the loopholes above, which the independent reviewer (audit lens) is seated for. The one thing that would justify a mid-card expert through a Decider (with its id) is a Windows process problem in the parallel voters or the scratch cleanup, but voters, sandboxes and cleanup are unchanged and the gate runs in the main thread before any worker starts, so no new concurrency enters. If the PM decides voters on a no-quota backend must also be refused, that widens the card past D163 and is a split, not a joiner.
