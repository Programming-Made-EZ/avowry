# Raw keyword candidates (from LLM-as-customer feedback, retro 016)

Input to card 024 (pattern discovery) and card 025 (base library v0). Raw means unmerged and unmeasured: these are what
four models said they repeatedly write, not yet evidence from real codebases. Ranked by how many models named them.

| Candidate | Models | Sketch | Needs |
|---|---|---|---|
| Retry with exponential backoff | Codex, Copilot, Grok (3) | `retry {action} up to {attempts} times with backoff`; spec: fails twice then succeeds => returns, calls 3 times | 033 generics, 034 behavioral holes |
| Pagination (cursor, or default size capped) | Codex, Grok (2) | `page {items} after {cursor} with limit {limit}`; `page size {requested} defaulting to {d} and capped at {cap}` | 033 |
| Validation guards: not blank, between, has a value | Copilot, agy (2) | `ensure {value} is not blank`, `ensure {value} is between {min} and {max}`, `ensure {value} has a value` | 033 for Option<T>; 035 for caller codes |
| Idempotent execute | Codex (1) | `execute {operation} once for {idempotencyKey}` | 034, an idempotency-store effect |
| Normalize an email address | Grok (1) | `normalize {address} as an email address`; fails with InvalidEmail | none |

Estimated savings quoted by the models: guards 5-9 lines; pagination 15-40; retry 25-60; idempotent execute 40-100.
These are model guesses, to be replaced by measurements (card 027).
