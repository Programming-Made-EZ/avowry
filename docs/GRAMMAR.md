# Avowry Language: Grammar v0.1 (draft)

Avowry is a source language designed for LLMs to read, write, and navigate with minimal context. Its core idea: **the name of every unit of behavior is a plain-language description of that behavior**, bound to executable specs. Code at any level reads as its own summary, and the list of all avow names is the project index.

Programs compile through a language-neutral IR to target **providers** (C# first, then TypeScript, Python). Every provider emits the same specs as native tests, so all targets are verified against one behavior suite.

## 1. Design rules

1. **Names are descriptions.** An avow name is a sentence with typed holes: `"total quantity of {items: List<LineItem>}"`.
2. **Specs are the truth, names are the summary.** Every avow may carry Given/When/Then specs. Specs compile to tests in every target. A body that drifts from its description fails CI.
3. **Small fixed core, open avow library.** The core has a handful of keywords. Everything domain-specific is an avow defined in the project.
4. **Leaves are target-specific, composition is universal.** Only leaf avows contain `native` code. Composite units (endpoints, composite avows) are pure avow composition and transpile identically to every target.
5. **Deterministic resolution.** Every step resolves to exactly one avow or the compile fails. No guessing.
6. **Specs state consumer-observable outcomes (2026-10-26).** Specs state consumer-observable outcomes so a `then` says what the consumer gets: a returned value, a stored or emitted effect, a response, or a declared failure. A bare `then succeeds` proves only that nothing threw; prefer an outcome with a value, and give every declared `fails with` a spec that shows it. (Guidance today; whether the compiler warns or refuses is under review on the board.)

## 2. Lexical structure

- **Indentation-sensitive.** Exactly two spaces per level; an indent must open exactly one level. The lexer emits `INDENT` and `DEDENT` tokens (Python style). Tabs in indentation are an error.
- **Blank and comment-only lines** are skipped and never change layout.
- **Implicit line joining.** Inside `{ }` or `[ ]`, newlines and indentation are ignored, so long records and lists can span lines.
- **Comments** start with `#` (outside strings) and run to end of line. `native` blocks are passed through raw, so `#` there is target code.
- **Names** (lexer token `Name`): a lowercase letter, then letters or digits; `-` and `'` join the word only when a letter or digit follows (`first-class`, `don't`, but `x->` is `x` then `->`). The parser treats a `Name` as an `ident` (fields, holes, variables) or a `word` (templates, steps) by position. Words match case-insensitively.
- **Type names** (lexer token `TypeName`, grammar `Ident`): `[A-Z][A-Za-z0-9]*`. Types, modules, endpoints, failure codes, HTTP verbs.
- **Strings:** double-quoted, single line, `\"` and `\\` escapes only. An avow template is lexed as one string; the parser splits its words and holes.
- **Numbers:** `-?[0-9]+(\.[0-9]+)?`. A `-` followed by a digit starts a number; there is no arithmetic.
- **Symbols:** `: , . = { } [ ] < > / ->` and the comparisons `<=`, `>=`, `==`, `!=`.
- **Native blocks.** A logical line that is exactly `native <target>` captures every following line indented deeper than it (blank lines included) as one `RAW` token, with the block's base indentation (header + 2) removed and trailing blank lines dropped. The block ends at the first non-blank line at or left of the header's indentation.

### Core keywords

`module` `type` `avow` `endpoint` `input` `output` `steps` `let` `respond with` `fail with` `fails with` `effects` `spec` `given` `when` `then` `and` `native`

Core keywords are reserved only at the **start of a line**. Inside avow templates and steps, words like `with` or `input` are ordinary words, except that `with {` after a name already in scope starts a record update, and a line that starts with `ensure` and contains `else` is an ensure step.

An avow accepts at most one filler from the fixed set `to`, `that`, `it`, directly after `avow`. The filler is ignored, is never part of the declared name, and is not allowed after any other keyword. The formatter always writes the canonical form `avow to "..."`; for example, `avow to "to do list"` declares the name `to do list`.

## 3. Grammar (EBNF)

```ebnf
file            = module_decl , { top_decl } ;
module_decl     = "module" , Ident , NEWLINE ;
top_decl        = type_decl | avow_decl | endpoint_decl ;

(* ---------- types ---------- *)
type_decl       = "type" , Ident , NEWLINE , INDENT , field_line , { field_line } , DEDENT ;
field_line      = field , NEWLINE ;
field           = ident , ":" , type_ref ;
type_ref        = Ident , [ "<" , type_ref , { "," , type_ref } , ">" ] ;
                  (* core: Text Int Decimal Bool Id List<T> Option<T> *)

(* ---------- avows ---------- *)
avow_decl       = "avow" , [ filler ] , template , [ "->" , type_ref ] , NEWLINE ,
                  INDENT , avow_clause , { avow_clause } , DEDENT ;
filler          = "to" | "that" | "it" ;
avow_clause     = effects_clause | fails_clause | spec | native_block | steps_block ;
                  (* any order; at most one effects, one fails with, one native per target;
                     native blocks OR steps, not both; at least one of them *)
template        = '"' , template_part , { template_part } , '"' ;   (* >= 1 word *)
template_part   = word | hole ;
hole            = "{" , ident , ":" , type_ref , "}" ;              (* names unique *)

effects_clause  = "effects" , ( "none" | Ident , { "," , Ident } ) , NEWLINE ;
fails_clause    = "fails with" , Ident , { "," , Ident } , NEWLINE ;

impl            = native_block , { native_block }      (* one per target *)
                | steps_block ;                         (* composite avow *)
native_block    = "native" , target , NEWLINE , RAW , NEWLINE ;   (* RAW: see section 2 *)
target          = "csharp" | "typescript" | "python" ;

(* ---------- endpoints ---------- *)
endpoint_decl   = "endpoint" , Ident , ":" , http_verb , route , NEWLINE ,
                  INDENT , endpoint_section , { endpoint_section } , DEDENT ;
endpoint_section = "input" , NEWLINE , field_block      (* required, once *)
                 | "output" , NEWLINE , field_block     (* optional, once *)
                 | steps_block                          (* required, once *)
                 | spec ;                               (* any order *)
field_block     = INDENT , field_line , { field_line } , DEDENT ;
http_verb       = "GET" | "POST" | "PUT" | "PATCH" | "DELETE" ;
route           = "/" , [ segment , { "/" , segment } , [ "/" ] ] ;
segment         = name | number | "{" , ident , "}" ;    (* /orders/{orderId}/lines *)

(* ---------- steps ---------- *)
steps_block     = "steps" , NEWLINE , INDENT , step_line , { step_line } , DEDENT ;
step_line       = step , NEWLINE ;
step            = let_step | update_step | ensure_step | respond_step | fail_step | avow_call ;
let_step        = "let" , ident , "=" , avow_call ;
update_step     = "let" , ident , "=" , path , "with" , record ;   (* root of path in scope *)
ensure_step     = "ensure" , condition , "else" , Ident ;
condition       = path , "is" , [ "not" ] , "blank"
                | path , "length" , "at" , "most" , number
                | path , compare_op , expr ;
compare_op      = "<" | "<=" | ">" | ">=" | "==" | "!=" ;
respond_step    = "respond with" , record ;
fail_step       = "fail with" , Ident ;
avow_call     = call_part , { call_part } ;           (* resolved, see section 4 *)
call_part       = word | argument ;
word            = name ;                                (* a bare lowercase name, no '.' after it *)
argument        = path_with_dot | literal | record | list ;
path_with_dot   = ident , "." , ident , { "." , ident } ;

(* ---------- expressions ---------- *)
expr            = path | literal | record | list ;
path            = ident , { "." , ident } ;
literal         = string | number | "true" | "false" | "none" ;
record          = "{" , [ ident , ":" , expr , { "," , ident , ":" , expr } , [ "," ] ] , "}" ;
list            = "[" , [ expr , { "," , expr } , [ "," ] ] , "]" ;   (* trailing commas allowed *)

(* ---------- specs ---------- *)
spec            = "spec" , string , NEWLINE , INDENT ,
                  [ given_line , { and_line } ] ,        (* no givens: avow with no holes *)
                  when_line ,
                  then_line , { and_line } ,
                  DEDENT ;
given_line      = "given" , ( binding | "input" , record ) , NEWLINE ;
binding         = ident , "=" , expr ;                  (* one per hole, avow specs *)
when_line       = "when" , ( "called" | "handled" ) , NEWLINE ;
then_line       = "then" , outcome , NEWLINE ;
outcome         = "returns" , expr
                | "responds" , record
                | "fails with" , Ident
                | "succeeds" ;
and_line        = "and" , ( binding | outcome ) , NEWLINE ;
```

## 4. Avow resolution (the important part)

A step that is not `let`, `respond with`, `fail with`, an ensure step or a record update is a **avow call**. The compiler resolves it against every avow template in scope:

1. Split the template into **words** and **holes**. The parser has already split the step into **words** (bare names) and **arguments** (dotted paths, literals, records, lists).
2. A hole matches exactly **one argument**, or **one bare word that names a variable in scope** (a `let`, or a hole of the enclosing composite avow). So `respond with { totalQuantity: total }` and `ensure items is not empty` inside a composite both work (an `ensure` line without `else` is an ordinary avow call). Arguments never contain spaces outside brackets, so matching is a linear scan with no backtracking.
3. Words match case-insensitively; whitespace is normalized.
4. **Exactly one** template must match. Zero matches is error `AV001 unknown avow` (with the closest templates suggested). Two or more is error `AV002 ambiguous avow`.
5. The matched expressions are type-checked against the hole types.

Example:

```
template:  total quantity of {items: List<LineItem>}
step:      total quantity of input.items
result:    call total-quantity-of(items = input.items) : Int
```

### Ensure step

An ensure step has the form `ensure <condition> else <Failure>`: it checks a condition and fails with the named code when the condition does not hold. The condition is one of:

- `<path> is blank`: a Text value that is empty or only whitespace.
- `<path> is not blank`: a Text value with at least one non-space character.
- `<path> length at most N`: a Text value of at most `N` characters; `N` is a non-negative 32-bit integer.
- `<path> <op> <value>`: an Int comparison with `<`, `<=`, `>`, `>=`, `==` or `!=`; the right side is a signed 32-bit Int literal or an Int path.

The path is a variable in scope or a field path such as `task.title`. In an avow, the code after `else` must be in the avow's `fails with` list (`AV326` otherwise). In an endpoint, the code becomes one of the endpoint's effective failures. A code repeated in `fails with` is `AV329`. A path of the wrong type for its condition is `AV312`; a malformed condition is `AV200`. An `ensure` line without `else` is an ordinary avow call, resolved against your own templates.

### Record update step

`let <name> = <source> with { field: value, ... }` binds a copy of a record with the named fields replaced; the source is never changed. The source must already be in scope: a hole, a `let` or `input`, optionally followed by a field path (`input.task`). After any other word, `with {` is ordinary words in an avow call.

- An empty field list, a trailing comma or a nested update is `AV200`; bind the inner update to a `let` first and use that name as the field value.
- An unknown field, or a source with no fields, is `AV311`.
- A field named twice is `AV303`.
- A value of the wrong type for its field is `AV312`.
- A name that is already defined is `AV313`.

The update is a `let` step only; it cannot appear as a call argument or inside a record.

### Library lints

- `AV010 duplicate skeleton`: two templates with the same word sequence (holes ignored) are an error.
- `AV011 near-duplicate`: templates that differ only by a synonym from the controlled vocabulary (`check` vs `ensure`) warn.

### Controlled vocabulary (starter set)

| Verb | Meaning |
|---|---|
| `ensure` | Guard. Returns nothing; fails with a declared code. Built in as the ensure step; without `else` it is wording for your own avows. |
| `validate` | Structural check of input shape. Fails with a declared code. |
| `find` | Read that may return `none`. |
| `load` | Read that must succeed; fails if missing. |
| `compute` / noun phrase | Pure calculation returning a value. |
| `save` / `publish` | Effects. Must be declared in `effects`. |

### Template rules (enforced by the parser)

- At least one word. A template made only of holes would match every call.
- Hole names are lowercase and unique within the template.
- No `#` (it would read as a comment when the template is re-lexed).
- Must not start with a step keyword (`let`, `native`, `respond with`, `fail with`), because no step could ever call it.

### Resolver diagnostics

Every diagnostic code is `AV` followed by three digits (for example `AV001`), and a code keeps its number.

The full resolver diagnostic table (`AV001`, `AV002`, `AV010`, `AV020`, and `AV300` to `AV335`, plus `AV337`, `AV340` and `AV341` from the C# target), which previously lived in [HANDOFF.md](../HANDOFF.md), now lives here:

| Code | Meaning |
|---|---|
| `AV001` | Unknown avow (quotes the closest template; suggests `variable.field` when a bare word matches a record field in scope) |
| `AV002` | Ambiguous avow (lists the matching templates) |
| `AV010` | Duplicate wording (two templates with the same word sequence, holes ignored) |
| `AV020` | Leaf avow with no `native` block for the selected target (the C# target reports it as "has no implementation for target csharp; add a `native csharp` block") |
| `AV300` | Unknown type |
| `AV301` | Wrong number of type arguments |
| `AV302` | Type name already taken (duplicate, or a core type) |
| `AV303` | Duplicate field (type, input or output) |
| `AV304` | Duplicate endpoint name, or same verb and route |
| `AV310` | Unknown variable (message lists what is in scope) |
| `AV311` | Unknown field, or a field read on a non-record (message lists the fields) |
| `AV312` | Type mismatch (message names both types) |
| `AV313` | Variable already defined |
| `AV314` | Record fields do not match the expected type |
| `AV320` | `let` of an avow that returns nothing |
| `AV321` | Returned value discarded; use `let` |
| `AV322` | `respond with` outside an endpoint, or in an endpoint with no output |
| `AV323` | Endpoint with output never responds |
| `AV324` | Step after `respond with` or `fail with` is unreachable |
| `AV325` | Composite avow with a return type (not supported in v0.1) |
| `AV326` | Composite fails with an undeclared code (`fail with` or `ensure ... else`) |
| `AV327` | Avows call each other in a cycle |
| `AV328` | Composite calls an avow whose effect it does not declare |
| `AV329` | A failure code repeated in `fails with` |
| `AV330` | Wrong trigger (`called` for avows, `handled` for endpoints) |
| `AV331` | Givens wrong: unknown, missing or duplicate hole; `input` in an avow spec; missing input in an endpoint spec |
| `AV332` | Spec value is a path, not a literal |
| `AV333` | Outcome the unit cannot have (`returns` with no return type, `responds` in an avow spec, `returns` in an endpoint spec, `responds` with no output) |
| `AV334` | Expected failure the unit can never produce (message lists the ones it can) |
| `AV335` | More than one outcome in a spec |
| `AV337` | A native csharp block raises `Fail("X")` where `X` is not in the avow's `fails with` |
| `AV340` | The C# target does not support a construct yet (a plain-call step or `fail with` in a composite, more than one effect, a call argument that is not a variable, a spec `given` or `then returns` of `none`, an effectful avow in a composite `let`, a `then succeeds` seed of a stored field that is not Text, Id or Bool) |
| `AV341` | The C# target reserves a name the generated code declares itself: a type named `AvowAttribute`, `AvowryFailure`, `Avows` or `AvowSpecs`, a module named `Program`, an avow whose method name is `Avows` or `AvowryFailure`, a field named like its own type, or an endpoint named `Avow` (message names the reserved name and the line to rename) |

Rules fixed by the resolver tests:

- **Duplicate wording** (`AV010`) compares the word sequence with holes ignored entirely.
- **Types check against an expected type.** `"text"` fits `Text` or `Id`; whole numbers fit `Int` or `Decimal`; `2.5` fits only `Decimal`; `none` fits only `Option<T>`; a `T` also fits `Option<T>` (with literal widening for `Option<Decimal>` and `Option<Id>`); records and lists check against record and list types (`AV312` otherwise), and records need their exact field set (`AV314`).
- **Failed bindings do not cascade:** a `let` whose avow call fails to resolve binds its variable as `PType.Error` (`?`), which is compatible with every type so later uses do not emit a second diagnostic.
- **Results are never silently discarded:** calling a value-returning avow as a statement is `AV321`; use `let`.
- **Flow is linear:** an endpoint with an output must end with `respond with` or `fail with`; anything after either is unreachable (`AV324`).
- **Specs use literal data only** (`AV332`) and have exactly one outcome (`AV335`).
- **Endpoint request and response shapes** are record types named `<Endpoint>.Input` and `<Endpoint>.Output`.

### Lexer diagnostics

| Code | Meaning |
|---|---|
| `AV100` | Tab used in indentation |
| `AV101` | Dedent to a level that matches no open block |
| `AV102` | Indent that opens more (or less) than one 2-space level |
| `AV103` | String not closed before end of line |
| `AV104` | Character Avowry does not use (e.g. `+`) |
| `AV105` | Native block line under-indented (deeper than header, shallower than base) |
| `AV106` | Native block has no body |
| `AV107` | Unknown string escape |
| `AV108` | `{` or `[` never closed |
| `AV109` | `}` or `]` with no matching opener |

### Parser diagnostics

| Code | Meaning |
|---|---|
| `AV200` | Syntax error: expected X, found Y |
| `AV201` | Top-level line is not `module`, `type`, `avow` or `endpoint` |
| `AV202` | Module declaration missing, not first, or repeated |
| `AV203` | Required part missing: type fields, avow implementation, endpoint `input`/`steps`, spec `when`/`then` |
| `AV204` | Clause or section repeated (`fails with`, `input`, `when`, `native <same target>`, ...) |
| `AV205` | Invalid template (see template rules) |
| `AV206` | Duplicate hole name in a template |
| `AV207` | Unknown native target |
| `AV208` | Avow has both native blocks and steps |
| `AV209` | Unknown HTTP verb |
| `AV210` | Spec clauses out of order, or `and` with nothing to continue |
| `AV211` | Unexpected indented block |
| `AV212` | Unknown word before an avow template; allowed fillers are `to`, `that`, `it` |
| `AV213` | More than one filler before an avow template |

The parser skips a bad line together with its indented children and keeps going, so one error never hides the declarations after it.

The lexer always returns a complete token stream with balanced `INDENT`/`DEDENT`, even with errors, so later stages can keep reporting.

## 5. Semantics in brief

- **Failures** are declared codes, not exceptions. An avow may only fail with codes in its `fails with` clause, plus codes from avows it calls. Providers map failures to their idiom (C#: an `AvowryFailure` carrying the code, mapped to HTTP 422 at endpoints). In a `native csharp` block, a `Fail("X")` call with `X` not in `fails with` is `AV337`; the scan reads only unqualified `Fail("literal")` calls outside comments and string or char literals, so computed, interpolated and concatenated codes are not checked, verbatim and nested-quote interpolated strings are not tokenised, `#if` branches are all scanned, other targets are not scanned, and a differently named helper is not seen.
- **Effects** default to `none`. An avow that calls an effectful avow must declare the union. Providers use effects to choose sync vs async and what to inject.
- **Naming in targets.** Providers convert names to target conventions: field `quantity` becomes `Quantity` in C#, `quantity` in TypeScript. `native` blocks use target conventions.
- **Missing native block.** If a leaf avow has no `native` block for the selected target, compilation fails with `AV020 no implementation for target`. (Later: the LLM-assisted generator proposes one, gated by the avow's specs.)

## 6. Compilation pipeline

```
.avowry source
  -> lexer (INDENT/DEDENT)
  -> parser (AST)
  -> resolver (avow calls bound, types checked, lints)
  -> IR (avow graph: types, templates, contracts, effects, specs, impls)
  -> provider (C#: FastEndpoints + xUnit | TS: Hono + Vitest | Python: FastAPI + pytest)
```

Each generated target symbol carries its source avow (C#: `[Avow("...")]` attribute), so an LLM reading generated code can always trace back to the avow.

## 7. Out of scope for v0.1

- Composite avows with `steps` (`let`, ensure and record update steps) are emitted by the C# target; constructs it does not emit yet are `AV340`.
- Effects other than `none`
- Content-hash pinning and the generation cache (`avowry.lock`)
- The `avowry index` command (one line per avow: template, return type, effects, failures)
- Modules importing other modules
