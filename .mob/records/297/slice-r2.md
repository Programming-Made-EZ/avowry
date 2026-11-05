## Intent

Ensure a Codex seat that will run commands is placed only after its availability probe proves a shell command can start inside the seat’s declared sandbox. Preserve placement of read-only and voting seats.

The GitHub card could not be fetched because network access is blocked; the local task-force proposal supplied the card statement and three done checks.

## Premises (D184)

1. HEAD is `1732a8205b29b880b2d78482c59cbbbc587937a1`. Command: `cmd /c git rev-parse HEAD`. Output: `1732a8205b29b880b2d78482c59cbbbc587937a1`.

2. The card request could not reach GitHub. Command: `cmd /c gh issue view 297 --repo Programming-Made-EZ/avowry`. Output: `Post "https://api.github.com/graphql": dial tcp 20.29.134.17:443: connectex: An attempt was made to access a socket in a way forbidden by its access permissions.`

3. The present probe proves only that a backend can answer a prompt. Command: `cmd /c rg -n -C 40 ^def.probe facilitator.py`. Output: `773-PROBE_PROMPT = "Availability probe: reply with exactly OK and nothing else."` and `804-    ok, model, output = run_backend(name, cfg, pin, PROBE_PROMPT, repo, timeout, None, stall_seconds)`.

4. The command-running probe belongs in `probe_backend`, not `local_driver.py` or placement: this is the existing per-backend seam used by `CardRun.probe_drivers`. Command: `cmd /c rg -n -C 25 probe_fn facilitator.py`. Output: `1184:        self.probe_fn = probe_fn or probe_backend` and `1440:        results = {name: self.probe_fn(name, cfg, self.repo, self.args.talk_timeout, self.args.stall_seconds)`.

5. Placement currently checks freshness and Claude rotation, but not whether the selected seat can run commands. Command: `cmd /c rg -n -C 45 def.check_seat facilitator.py`. Output: `1300-        self.reprobe_if_stale(...)`, `1305-        if backend == "claude":`, and `1310-        return self.probes, history, backend, context`.

6. The call site knows whether a mob seat is command-running. Command: `cmd /c rg -n -C 18 driver facilitator.py`. Output: `1683:    def seat_turn(self, seat, role, play, ask, driver=False, charter=None):` and `1711:        release = self.seat_claim(cwd, role) if driver else (lambda: None)`.

7. Codex configuration already distinguishes command-capable and read-only invocations, but command-capable Codex currently bypasses the sandbox. Command: `cmd /c type core\backends.json`. Output: `"invocation": "codex exec {PROMPT} -m {MODEL} --dangerously-bypass-approvals-and-sandbox --skip-git-repo-check"` and `"readonly_invocation": "codex exec {PROMPT} -m {MODEL} -s read-only --skip-git-repo-check"`.

8. The working tree had pre-existing untracked mob state; no inference is drawn from cleanliness. Command: `cmd /c git status --short`. Output: `?? .mob/`.

## Decision: SHIP AS IS

Concern: distinguish seat capability from backend availability without turning the probe into a sandbox bypass.

Increments:

1. A blocked shell makes the command-capability probe fail with shell path, sandbox mode, and exit/timeout detail.
2. A working shell makes that probe pass and permits a command-running seat.
3. Voting/read-only seats remain placeable when the command-capability probe fails.

Criteria: one deterministic red/green cycle per increment; no real Codex dependency; no change to quota/rotation semantics.

## Increments

1. Blocked shell fails.

   Red may touch `tests/test_driver_command_probe.py` and a fixture under `tests/fakes/`. Green may touch `facilitator.py` and `core/backends.json`. Use a fake Codex executable that receives the normal invocation and emits fixture evidence for a shell launch returning Windows error 1920; no real Codex or Store alias runs. First red: `test_codex_command_probe_reports_blocked_shell_path_mode_and_1920`; assert `ok` is false and evidence contains the resolved shell path, `workspace-write`, and `1920`.

2. Working shell passes and placement admits a driver.

   Red may touch `tests/test_driver_command_probe.py` and `tests/test_seat_gate.py`. Green may touch `facilitator.py` and `core/backends.json`. The fake Codex launches a fixture `.cmd`/Python shim that prints a nonce such as `MOB_SHELL_OK`; inject the runner and configuration through `probe_fn`, never invoke Codex. First red: `test_command_capable_codex_seat_is_placed_after_shell_probe_passes`; assert `gate_seat(..., command_required=True)` returns the recorded probe and the backend call is allowed.

3. Read-only/voting placement survives failure.

   Red may touch `tests/test_seat_gate.py` and directly affected Decider probe tests. Green may touch `facilitator.py`; `decider.py` only if it must pass the non-command capability explicitly. Reuse the failing fake result from increment 1. First red: `test_failed_command_probe_refuses_driver_but_places_read_only_and_voting_seats`; assert the driver is refused before backend launch while read-only and voting seats are recorded and launched.

The sandbox mode should come from explicit backend configuration used to build the invocation, not be guessed from prose. The shell path should likewise be explicit configuration or injected probe input, resolved before launch and reported verbatim. Tests substitute both values and the process runner; therefore they obtain mode/path without running real Codex. `probe_backend` owns execution and evidence; `check_seat` only consumes the recorded capability according to `command_required`.

## Risks

- Seats whose probe is read-only/voting must keep being placed; command capability is an additional requirement only for driver/build/green seats.
- No sandbox bypass: do not satisfy the card by retaining or adding `--dangerously-bypass-approvals-and-sandbox`. The probe must exercise the same declared sandbox mode as the command-running seat.
- Do not infer capability from an `OK` model response, executable presence, or read-only probe.
- Fail closed on missing shell, launch exception, nonzero exit, malformed evidence, stall, or timeout, while retaining actionable path/mode/error evidence.
- Preserve freshness, quota rotation, probe persistence, and seat-use recording.

## Conditions adopted after round 1 (facilitator, folding in two OBJECTION conditions; no other change)

Round 1 had two objections (QA engineer, audit lens) and four NO OBJECTION; the product manager's reply was not in window form. The two conditions overlap on one point (the probe and the driver invocation use the same sandbox mode) and add nothing outside the card's second and third checks, so both are adopted into the slice:

- Condition A (QA engineer): a red test proves the actual command-running Codex seat invocation uses the same configured sandbox mode as the probe that admitted it, and that the invocation excludes `--dangerously-bypass-approvals-and-sandbox`. It belongs to increment 2 (red may touch `tests/test_driver_command_probe.py`).
- Condition B (audit lens): the command-capability probe result is persisted in the probe state with shell path, sandbox mode and exit evidence; a test in increment 2 or 3 fails if `check_seat` admits a driver on a stale record, an absent record, or a record made under a different sandbox mode than the invocation. This needs no new file beyond the existing probe state.

The increments, criteria and file limits are otherwise as written above; still one concern, three increments, three criteria.
