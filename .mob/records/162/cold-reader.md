# Card 162 cold reader (Muse, reply-only, Probation; GRAMMAR.md and CHEATSHEET.md inlined; read as the sponsor persona)

Tested tip: green 1 1bcae82 (main). The first launch went through the vote wrapper and came back as the one word "Yes" (the wrapper reads any reply as a vote: a harness defect, the reply-only form is a direct `muse exec` with the prompt file); the reply below is from the direct run with the same prompt (.mob/records/162/cold-reader-prompt.txt). Verbatim:

# WRITE IT

```avowry
avow to "reject blank title {title: Text}"
  fails with Blank
  steps
    ensure title is not blank else Blank

avow to "guarded copy of {item: Item}"
  fails with Blank
  steps
    let copied = item with { qty: 2 }
    ensure copied.name is not blank else Blank
```

# RULES YOU FOUND

- An ensure step is `ensure <condition> else <Failure>`. It fails with that code when the condition does not hold. Grammar taught this.
- To refuse blank Text, use `<path> is not blank`. Blank means empty or only whitespace. Grammar and Cheat Sheet taught this.
- The failure after `else` must be listed in the avow's `fails with`; otherwise `AV326`. Grammar and Cheat Sheet taught this.
- An `ensure` line without `else` is not this step; it is an ordinary avow call. Grammar and Cheat Sheet taught this.
- `let y = x with { f: v }` binds a copy of a record with named fields replaced; the source is unchanged and must already be in scope. Grammar and Cheat Sheet taught this.
- The update's new name must not already be defined (`AV313`), the field must exist (`AV311`), not be repeated (`AV303`), and the value must match its type (`AV312`). Grammar taught this.
- The update is only a `let` step, never a call argument or inside a record. Grammar taught this.

# CONFUSING OR MISSING

- Success after the last ensure is implied, not stated. The examples end after `ensure ... else ...`, yet a spec says `then succeeds`. I had to guess the avow simply succeeds.
- A complete file needs `module`, but the requested fragments reuse `Item` from the Cheat Sheet example without saying whether to redefine it: `"file = module_decl , { top_decl }"`.
- Grammar says `"with {` after a name already in scope starts a record update"`, while also saying after any other word it is ordinary words. Deciding which applies needs scope knowledge the Cheat Sheet does not explain.
- `AV303` is listed both as `"Duplicate field (type, input or output)"` and, for updates, a field named twice. Whether these are the same check is unclear.

# VERDICT

CLEAR — the two Cheat Sheet examples map almost directly onto both requested avows, and Grammar supplies the exact failure-declaration and copy constraints.

## Facilitator reading

Verdict CLEAR: the reader wrote both avows (the blank-title ensure and the record update followed by an ensure) from the two pages alone and listed the rules it relied on with the page that taught each. Four notes; two acted on in GRAMMAR.md only (the sheet is at 693 of 700 tokens, so it takes no words): the AV303 table row now also names a field named twice in a record update; the Ensure step section now says the avow succeeds when every step passes. The other two (a module line and the Item type are needed for a whole file; scope of `with {`) are fragment-only effects of the request and are already stated (the sheet's Shop fence defines Item; GRAMMAR says with { after a name in scope).
