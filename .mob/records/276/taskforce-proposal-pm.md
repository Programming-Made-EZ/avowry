# Card 276 task-force declaration proposal

Intent line (D165): Deliver the ruled first slice for Muse Code safely on Windows: official install method resolved by install test, browser/device-friendly sign-in path for product-owner phone completion, availability/quota probe wiring, and event-log reader for voting seats first.

Assumption line (D166): The current probe (23:48 UTC) is authoritative for seating in this window (Copilot and Grok available; agy out of quota; Codex without quota), and a voting-seat-first trial can proceed without build seats.

## Team roles, models, context numbers, and why

- Ready / product manager — **Copilot**, **ctx-1**: current seated role; owns scope, safety constraints, and declaration quality for card 276.
- Slice / architect — **Grok**, **ctx-2**: defines the minimal first-slice contract and keeps to D168/D175 limits.
- Red / QA engineer — **Copilot**, **ctx-3**: writes acceptance checks for install test evidence, sign-in evidence, probe evidence, and reader evidence.
- Green / backend developer — **Grok**, **ctx-4**: implements backend probe and command-source reader integration for Muse.
- Review / independent reviewer — **Copilot**, **ctx-5**: non-author check on evidence, citations, and safety invariants.
- Gate / sign-off lens — **Grok**, **ctx-6**: pre-ship evidence review with D176 environment reporting once build seats are in scope.

Who is left out and why:
- **agy** is left out now: currently out of quota per probe.
- **Codex** is left out now: currently without quota per probe.
- Build seats are intentionally deferred by ruling D179 (voting seats first).

## Domain-expert question (T2)

For `mob-crosscli` (backends, `facilitator.py run_backend`, `probe_drivers`, and `command_source.py`): what is the minimal, auditable event schema needed from Muse session logs so a vote reader can extract command executions and final vote text without false "clean" states when a command source is missing?

## Value, metric, counter-metric

Value: restore a second usable decision-seat path by adding Muse as a trial driver under explicit trust controls.

Metric: one Muse voting seat can be probed, scanned, and return a cited vote in a decision window.

Counter-metric: added false positives/no-source gaps in command scanning and additional window minutes.

## Done checks with D175 caps (sum <= 3)

1. **Install + sign-in evidence** (Cap: criterion 1, at most 1 cycle)
   - Official Windows install command source recorded.
   - Sign-in flow documented as browser/URL-capable and product-owner handoff-compatible.
2. **Availability/quota probe evidence** (Cap: criterion 2, at most 1 cycle)
   - Muse probe path defined alongside existing drivers and evidence format fixed.
3. **Event-log reader for voting seats** (Cap: criterion 3, at most 1 cycle)
   - Reader contract specified for voting seats first; build-seat reader work explicitly deferred.

Cap sum: **1 + 1 + 1 = 3** (meets D175).

## Trust-ladder placement

- **Lowest rung (Rung 1 / Probation)** for Muse trial seats.
- **Voting seats first**, build seats later, per D179 first-slice ruling.

## Which decisions apply

- **D165** applies: explicit intent line required before red.
- **D166** applies: explicit assumption line and probe-backed assumption.
- **D167** applies: hostile-input list required for list/map-like command-reader criteria.
- **D168** applies: keep slice within 15 KB and split if needed.
- **D171** applies: cite-a-point voting; unread votes do not count.
- **D175** applies: written cycle caps per criterion, total <= 3.
- **D176** applies at gate time: environment lines and untouched-failure handling in gate report.
- **D178** applies: decision-seat rotation breaker behavior if a model returns empty/voteless or quota-reached outcomes.

## Findings (a) to (d) with sources

### (a) Official Windows install method

- **Command:** `irm https://dev.meta.ai/install.ps1 | iex`
- **Source:** https://dev.meta.ai/docs/muse-code (also mirrored at https://dev.meta.ai/docs/muse-code.md and quickstart at https://dev.meta.ai/docs/quickstart)
- **Officiality verification:** source is Meta’s official docs domain (`dev.meta.ai`) under "Meta Model API" and the Muse Code product page; this satisfies the "official site" requirement. No separate official package-registry install entry was found in these official docs for the CLI itself.

### (b) Sign-in flow (device code or URL)

- **Documented:** browser sign-in (URL/browser approval flow) and API-key flow.
- **Not documented in official pages reviewed:** explicit OAuth device-code flow wording.
- **Source:** https://dev.meta.ai/docs/muse-code/auth and https://dev.meta.ai/docs/muse-code (first-run section).

### (c) Event-log location and format

- **Documented location:** `${XDG_DATA_HOME:-$HOME/.local/share}/muse/sessions/YYYY/MM/DD/<session-id>/session.jsonl`
- **Documented format:** append-only JSONL; each line envelope includes `{sequence, recorded_at, record_type, durability, payload_type, payload}`.
- **Windows note:** official docs reviewed do not publish a Windows-specific path mapping for this session log.
- **Source:** https://dev.meta.ai/docs/cookbook/deterministic-replay (Find Recorded Sessions section); supporting model statement in https://dev.meta.ai/docs/muse-code/interactive (event-sourced append-only session log).

### (d) Pre-execution hook

- **Yes, documented:** hook lifecycle supports `PreToolUse`, and hooks can block actions before execution.
- **Hook sources/placement:** project hooks at `<project-root>/.muse/hooks.json`, user hooks in settings hooks block, managed hooks via `managed_hooks_path`.
- **Source:** https://dev.meta.ai/docs/muse-code/extending (Hooks and Lifecycle events) and https://dev.meta.ai/docs/muse-code/configuration (settings hooks block).

## Open points

1. Does Muse publish a Windows-native session-log path equivalent to the documented XDG path, or should we treat Windows path discovery as a local probe outcome in criterion 3?
2. Is an explicit device-code flow available but undocumented, or is browser URL approval the only supported interactive method?
3. Which event fields are stable enough for `command_source.py` parsing contract (and what fallback behavior is required when missing)?
4. Should D178 breaker thresholds be pre-configured for Muse decision seats from day one, or inherit default decision-seat breaker behavior first and tune after first window evidence?