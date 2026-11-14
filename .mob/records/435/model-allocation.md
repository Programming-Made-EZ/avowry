# Card 435: model allocation per seat (declared against actual), from the resume on

The owner's ruling of 2026-11-14 (model size per role, declared before any work): every seat declares its service, its exact model or size and a reason before it runs; the seat log records the model the CLI reported; a mismatch is a coach finding.

| Seat | Role | Service | Declared before the run | Reason | Actual (as the CLI reported) | Note |
|---|---|---|---|---|---|---|
| green (first) | dev, green | cursor | none (the ruling arrived after launch) | | not recorded | undeclared; finding for the retro |
| second review, try 1 | qa | grok | grok-4.6 | adversarial review, the only Grok model | grok-4.6-build | ended on 402 (balance exhausted), no findings |
| second review, try 2 | qa | agy | gemini-4-argon-high | adversarial regex reasoning, strong tier | gemini-4-argon-high (the launch flag; agy prints no model line) | FLAG, 37 min |
| rework architect | architect | claude (subagent) | "opus" (an alias, not an exact model) | regex redesign, strong tier | claude-opus-5-5 (the alias's mapping in this harness) | CORRECTION below |
| rework red | dev, red | claude (subagent) | "sonnet" (an alias, not an exact model) | given rows, mechanical test writing, mid tier | claude-sonnet-5-5 (the alias's mapping in this harness) | CORRECTION below |
| rework green | dev, green | cursor (cursor-agent -p --force) | "default model" (NOT pinned) | paste-ready design, mechanical | not reported at launch: `--list-models` shows `auto - Auto (default)` and `gpt-5.3-codex - Codex 5.3 (current)`; the run reported no model (its record: "No explicit model identifier was shown"), so the actual model is unknown | CORRECTION below; 3 min, Slice=435 112 of 112 |

## Correction (takeover-style, 2026-11-14, raised by Dispatch)

Three seats of the rework round ran without an exact model declared before the run: the architect and the red were declared by the harness alias ("opus", "sonnet"), and the rework green was launched on Cursor's unpinned default. The ruling asks for the exact model or size before any work. Recorded after the fact above with what each actually resolved to; the green's actual model is added from its run. From the next seat on, every launch pins the model explicitly (`--model` for cursor-agent and agy, `-m` for grok; a Claude subagent seat takes only a family alias, so its declaration names the alias and the exact model it maps to, for example `opus` = claude-opus-5-5) and the declaration is written here before the launch.

## Declared before launch (the rule in force from here on)

| Seat | Role | Service | Model, pinned | Reason |
|---|---|---|---|---|
| third review | qa | agy | gemini-4-argon-high (`--model`) | adversarial review, strong tier; wrote neither tests nor code. Actual: the flag (agy prints no model line). FLAG, 31 min |
| D259 vote | architect, coach, qa | claude (decider.py, fresh context per role) | sonnet = claude-sonnet-5-5 (the derived backends file's default) | a judgement vote on given evidence, mid tier |
| D259 vote | auditlens, decider, dev | agy (decider.py, fresh context per role) | gemini-3.8-flash-medium (pinned as the derived backends file's agy default) | the same, mid tier; cursor is not seated because its vote launcher takes no model flag |
| D259 vote, reseat | architect | codex (decider.py) | gpt-5.6-sol (the backends default, pinned in the derived file) | the three Claude seats were refused by the seat rotation rule before starting (qwen, codex and cursor had waited longer); qwen is not seated while a play-tracker train is alive; one short vote fits Codex's ~3% quota |
| D259 vote, reseat | coach, qa | agy (decider.py, fresh context per role) | gemini-3.8-flash-medium | cursor is not seated: its vote launcher takes no model flag (a card asks for one); one model in several roles is allowed with a fresh context each, recorded as thin diversity |
| D259 vote, second reseat | architect | agy (decider.py, fresh context) | gemini-3.8-flash-medium | Codex refused at launch: usage limit, retry after 2026-11-18 00:12; claude still refused by rotation, qwen blocked by the train, cursor unpinnable |
| retro voices | product manager, audit lens, coach | agy (CLI, a fresh context each) | gemini-3.8-flash-medium (`--model`) | reflective judgement on committed records, mid tier; two services so the voices are not all one model |
| retro voices | architect, qa, dev, decider | claude (subagent, a fresh context each) | sonnet = claude-sonnet-5-5 | the same, mid tier |
| D260 vote (the retro pick) | all six roles | agy (decider.py, a fresh context per role) | gemini-3.8-flash-medium | a judgement vote, mid tier; the only usable service: claude refused by rotation, codex and grok out of quota, qwen blocked by the train, cursor unpinnable (the two-service floor the retro proposes cannot be met yet; recorded) |
| sign-off | signoff | claude (subagent, a fresh context) | sonnet = claude-sonnet-5-5 | run the given gate filters in a fresh clone and judge the result, mid tier; differs from the green seat (Cursor) |
