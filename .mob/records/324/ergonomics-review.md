# Card 324: LLM-ergonomics review (read-only Claude reviewer, once)

Verdict: nothing to change. No grammar slips; "avow" reads as a noun in all 19 messages and matches the vocabulary of docs/GRAMMAR.md and docs/CHEATSHEET.md (avow, composite avow, avow template, avow call).

## Messages read (code, new text)

| Code | New text |
|---|---|
| PH010 | Avow 'ping {m: Int}' has the same wording as 'ping {n: Int}'. |
| PH321 | Avow 'count things' returns Int; use 'let' to keep it. |
| PH320 | Avow 'do nothing' returns no value and cannot be used with 'let'. |
| PH327 | Avow 'ping {n: Int}' participates in a call cycle. |
| PH331 (input) | Avow specs cannot bind input; use a hole binding. |
| PH331 (hole) | Avow spec for 'check {n: Int}' does not bind hole 'q'. Valid holes: n. |
| PH334 | Avow 'check {n: Int}' cannot fail with 'Underflow'. Valid options: Overflow. |
| PH203 | Avow has no implementation; add `native <target>` or `steps`. |
| PH325 | Composite avow 'count {n: Int}' cannot return a value. |
| PH328 | Composite avow 'store {n: Int}' must declare effect 'Db'. |
| PH330 | Spec 'wrong trigger' on avow 'add {a: Int} and {b: Int}' must use 'when called'. |
| PH333 | Spec 'succeeds with value' expects success for an avow that returns a value. |
| PH001 | No avow matches this step. Closest: "do nothing". |
| PH208 | An avow is implemented by native blocks or by steps, not both. |
| PH200 | Expected a quoted avow template, found '42'. |
| PH200 | Expected an avow clause (effects, fails with, spec, native or steps), found 'returns'. |
| PH200 | Expected an avow call, found end of line. |
| PH200 | Expected `called` (avow spec) or `handled` (endpoint spec), found 'frobnicated'. |
| PH330 (second) | Spec 'bad trigger' on avow 'check {n: Int}' must use 'when called'. |

Also read: PH201 (Parser.cs:204), the filler error (Parser.cs:371), and the NoMatchMessage assembly (Resolver.cs:727-737); they already use the new vocabulary.

## Notes (none blocking)

- Seven messages open with a bare "Avow '...' ...". The quoted template right after it and the docs settle the noun reading; the old messages had the same shape.
- PH203 could read "An avow has no implementation; ..." and PH208 could end "; remove one". Optional, later.
- Internal variable names `phrases` and `phraseDeclarations` in Resolver.cs and Parser.cs still say phrase. Internal, out of scope for diagnostics.
