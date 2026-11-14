# Card 435: model allocation per seat (declared against actual), from the resume on

The owner's ruling of 2026-11-14 (model size per role, declared before any work): every seat declares its service, its exact model or size and a reason before it runs; the seat log records the model the CLI reported; a mismatch is a coach finding.

| Seat | Role | Service | Declared before the run | Reason | Actual (as the CLI reported) | Note |
|---|---|---|---|---|---|---|
| green (first) | dev, green | cursor | none (the ruling arrived after launch) | | not recorded | undeclared; finding for the retro |
| second review, try 1 | qa | grok | grok-4.6 | adversarial review, the only Grok model | grok-4.6-build | ended on 402 (balance exhausted), no findings |
| second review, try 2 | qa | agy | gemini-4-argon-high | adversarial regex reasoning, strong tier | gemini-4-argon-high (the launch flag; agy prints no model line) | FLAG, 37 min |
| rework architect | architect | claude (subagent) | "opus" (an alias, not an exact model) | regex redesign, strong tier | claude-opus-5-5 (the alias's mapping in this harness) | CORRECTION below |
| rework red | dev, red | claude (subagent) | "sonnet" (an alias, not an exact model) | given rows, mechanical test writing, mid tier | claude-sonnet-5-5 (the alias's mapping in this harness) | CORRECTION below |
| rework green | dev, green | cursor (cursor-agent -p --force) | "default model" (NOT pinned) | paste-ready design, mechanical | not reported at launch: `--list-models` shows `auto - Auto (default)` and `gpt-5.3-codex - Codex 5.3 (current)`; the run's own report is added when it ends | CORRECTION below |
| third review (planned) | qa | agy | gemini-4-argon-high, pinned with `--model` | adversarial review, strong tier; wrote neither the tests nor the code | | |

## Correction (takeover-style, 2026-11-14, raised by Dispatch)

Three seats of the rework round ran without an exact model declared before the run: the architect and the red were declared by the harness alias ("opus", "sonnet"), and the rework green was launched on Cursor's unpinned default. The ruling asks for the exact model or size before any work. Recorded after the fact above with what each actually resolved to; the green's actual model is added from its run. From the next seat on, every launch pins the model explicitly (`--model` for cursor-agent and agy, `-m` for grok; a Claude subagent seat takes only a family alias, so its declaration names the alias and the exact model it maps to, for example `opus` = claude-opus-5-5) and the declaration is written here before the launch.
