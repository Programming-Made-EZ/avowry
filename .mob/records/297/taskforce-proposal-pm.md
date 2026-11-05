# Card 297 task-force declaration proposal (PM)

Card: a Codex seat on Windows runs shell commands in its sandbox, and the probe proves it. Shape: Contract, bug. The probe only asks for the word OK, so it reports Codex available while the sandboxed shell cannot start (Store alias pwsh, error 1920).

## Task force
- Product manager (ready): Copilot, ctx-1. Domain: queue, scope, ready gate. Why: keeps the card to the probe/placement behaviour and records the ready check.
- Architect (slice): Codex (read-only, cmd.exe path), ctx-2. Domain: Codex CLI sandbox modes, Windows process launch, driver probe design. Why: chooses where the command probe lives and proves the premise (blocked shell fails, cmd path works) before any window.
- QA engineer (red): Claude, ctx-3. Domain: deterministic test design, fake-shell fixtures. Why: writes one red per done check, with no dependency on this machine's real Store alias.
- Backend developer (green): Claude, ctx-4. Domain: probe and seat-placement code in mob-crosscli. Why: Codex cannot run shell commands on this card, so product-owner priority puts Claude in every coding seat.
- Independent reviewer (ship): Copilot, ctx-5. Domain: git-level review, sandbox-not-bypassed invariant. Why: did not author; checks that no sandbox is bypassed and that voting/read-only still place.
- Signoff / Decider: Claude, ctx-6 (a context that did not write the green). Why: final decision and integration under the single-writer lock.

Placement facts: Claude fills every coding seat (red, green) while Codex cannot run shell commands. Copilot and Codex (read-only/cmd.exe path) can take ready, ship and voting. Grok stays out of counted windows until card 291. Whoever authors never reviews.

Left out: UX, frontend, database (no user interface or data); security (no sandbox change, the invariant is checked by the reviewer); technical writer (setup note is one line inside the green); DevOps (the machine fix is a product-owner decision, not work in this card); domain expert (see below); agile coach (observer only, not counted).

## Domain-expert question (refinement)
Should an agent with different domain knowledge or context join? No. The work is probe and placement logic with a fake shell; Windows sandbox knowledge is already in the card's evidence (reproduced, error 1920) and the architect's premise run covers it. Revisit at each cycle boundary: if the real Codex sandbox behaves differently from the fake, propose a Windows/Codex sandbox specialist through the Decider.

## Ready check
SHIP AS IS. One concern (a command-running Codex seat is placed only when a shell probe passes). Three increments: probe runs a command under the seat's sandbox mode; placement gates on it; voting/read-only skip the gate. Three criteria. Scope note: the card says the machine fix (non-Store PowerShell, or Codex set to use cmd) needs product-owner approval, so only probe and placement behaviour is in scope. Done check 2 is proven with a fake/fixture shell; applying the fix on this machine is not done here. If check 2 turns out to need the real machine change, that part waits on the product owner and the card does not stall on it.

## ZOMBIES, one criterion per cycle
Cycle 1, check 1 (blocked shell fails the probe with details):
- Z: no shell path resolvable. Probe fails naming "no shell" (reason kept as the empty case).
- O: one blocked shell, error 1920. Probe fails, message has shell path, sandbox mode, 1920.
- M: several sandbox modes (workspace-write, read-only) each name their own mode.
- B: error code other than 1920 still reports the code; empty output; timeout (as observed in the repro) reports a timeout, not a pass.
- I: probe is called by the same entry point the placement uses.
- E: probe process itself crashes or hangs: fails closed with a reason.
- S: simple blocked-shell run, happy failure path.

Cycle 2, check 2 (fix applied, probe passes, green commits):
- Z: nothing to commit. Green runs a command, no commit, no lock taken wrongly.
- O: one command, one commit on main under the single-writer lock.
- M: two successive commands/commits keep the single lock.
- B: probe passes via cmd.exe path as well as a non-Store pwsh.
- I: probe result recorded where placement reads it.
- E: lock held by another writer: green waits or refuses, no second writer.
- S: probe passes, command echo returns its output.

Cycle 3, check 3 (voting/read-only still placed):
- Z: no command-probe result at all: voting seat still placed.
- O: one voting seat, probe failed, placed.
- M: voting plus read-only seats both placed; a build seat in the same run is refused.
- B: probe passes: all three placed; probe fails: only the build seat is refused.
- I: placement output names why the build seat was refused and why the others were not.
- E: probe errors out: build seat refused, others placed.
- S: simple one-seat voting placement.

## Value, metric, counter-metric
Value: Codex can return to build seats on Windows when it truly can run commands, and a seat that cannot is refused up front instead of wasting a window (card 230 green 1 attempt 1 cost a timeout).
Metric: 0 build-seat windows that start and cannot run a command; probe failure messages carry all three facts (path, mode, 1920) in 100% of blocked cases.
Counter-metric: Codex voting or read-only seats wrongly refused (must stay 0), and probe time added to every placement (should stay a single short command).
