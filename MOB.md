# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Live mob dashboard: see every seat's status from a phone (036)

## Who did what

- **Codex:** 19 turns (advance x1, green x4, ready x4, refactor x5, ship x4, slice x1)
- **Copilot:** 2 turns (red x1, signoff x1)
- **Grok:** 1 turns (red x1)

## Recent turns

- Turn 5: Codex (backend-dev, refactor): changed facilitator.py
- Turn 6: Codex (devops-engineer, ship): Shipping stopped: the required dashboard acceptance suite is not green.
- Turn 7: Codex (backend-dev, green): changed facilitator.py
- Turn 8: Codex (backend-dev, refactor): changed facilitator.py
- Turn 9: Codex (devops-engineer, ship): Shipment stopped: the required gate is not green.
- Turn 10: Codex (backend-dev, green): changed facilitator.py, status.py, tests/test_status.py
- Turn 11: Codex (backend-dev, refactor): changed facilitator.py, status.py, tests/test_status.py
- Turn 12: Codex (devops-engineer, ship): Shipment stopped: the required acceptance gate is red.
- Turn 13: Copilot (qa-engineer, signoff): SIGNED OFF — The red-first proof is in the mob log: python -m unittest tests.acceptancedashboard failed before any production code change, with the…
- Turn 14: Codex (product-manager, advance): Still In Progress.
- Turn 15: Codex (product-manager, ready): READY — The card has a sound Connextra story, comprehensive Given/When/Then acceptance criteria, and all referenced project guidelines, tests, and…
- Turn 16: Codex (architect, slice): CONFIRMED — This is a thin vertical slice: an operator can open one local, phone-friendly dashboard and observe mob status end to end.
- Turn 17: Grok (qa-engineer, red): changed facilitator.py, dashboard/index.html, dashboard/serve.py, status.py, tests/test_status.py
- Turn 18: Codex (backend-dev, green): changed facilitator.py, dashboard/index.html, dashboard/serve.py, status.py, tests/test_dashboard_server.py, tests/test_status.py
- Turn 19: Codex (backend-dev, refactor): changed facilitator.py, dashboard/index.html, dashboard/serve.py, status.py, tests/test_dashboard_server.py, tests/test_status.py
- Turn 20: Codex (devops-engineer, ship): Shipment stopped because the required acceptance gate is red.
- Turn 21: Codex (backend-dev, green): changed facilitator.py, dashboard/index.html, dashboard/serve.py, status.py, tests/test_dashboard_server.py, tests/test_status.py
- Turn 22: Codex (backend-dev, refactor): changed facilitator.py, dashboard/index.html, dashboard/serve.py, status.py, tests/test_dashboard_server.py, tests/test_status.py

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 2 of 3 checks pass
- **After cycle 1: not done**: 2 of 3 checks pass
- **After cycle 2: not done**: 2 of 3 checks pass

## Decider outcomes (latest)

- Decider D8: pause experiment E1 (true mobbing) on card 032 after 8 runs; finish 032 in the standard driver-plus-review mode with a whole-team sign-off before the… (passed)
- Decider D11: declare card 032 Done as a zero-shot baseline repair; milestone M2 'Measured' is NOT declared reached (aligned with the bootstrap exit: reached when… (passed)
- Decider D12: experiment E1 (true mobbing) stays OPEN and opt-in under the bootstrap-phase rule: not the default practice, no TWA rule, tooling kept behind… (passed)

## Board

- **In progress:** Live mob dashboard (tooling, experiment E2; **in progress**: finish the local-only version, then stop dashboard work) (036)
- **Next in Ready:** next up: Adapter contract and C# adapter: types and runtime (003); 2nd in Ready: C# adapter: phrases, effects, failures (004); 3rd in Ready: C# adapter: FastEndpoints endpoints and Program (005)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016); Benchmark baseline is invalid: fix the harness, store a trustworthy baseline (032)

## Latest commits

- ffbbbaa Balance, not removal (Steve's clarification): agreement wording revised; Steve's diagnosis and P6 logged; defect
- 7e43d31 Steve's course correction: product first, ~20% process budget, lighter Decider, headline number; board re-ranked
- d77b0f9 Seed: the approved process retro after the dashboard card, its data plan and interim flow numbers

## What's next

- Running. Latest: turn 22 by Codex (refactor). Next: the next stage of the current cycle.

_Updated 2026-10-26 16:59 UTC. Summary only: no prompts, replies, code or secrets are published._
