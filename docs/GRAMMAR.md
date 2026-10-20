# Phrase Language: Grammar v0.1 (draft)

Phrase is a source language designed for LLMs to read, write, and navigate with minimal context. Its core idea: **the name of every unit of behavior is a plain-language description of that behavior**, bound to executable specs. Code at any level reads as its own summary, and the list of all phrase names is the project index.

Programs compile through a language-neutral IR to target **providers** (C# first, then TypeScript, Python). Every provider emits the same specs as native tests, so all targets are verified against one behavior suite.

## 1. Design rules

1. **Names are descriptions.** A phrase name is a sentence with typed holes: `"total quantity of {items: List<LineItem>}"`.
2. **Specs are the truth, names are the summary.** Every phrase may carry Given/When/Then specs. Specs compile to tests in every target. A body that drifts from its description fails CI.
3. **Small fixed core, open phrase library.** The core has a handful of keywords. Everything domain-specific is a phrase defined in the project.
4. **Leaves are target-specific, composition is universal.** Only leaf phrases contain `native` code. Composite units (endpoints, composite phrases) are pure phrase composition and transpile identically to every target.
5. **Deterministic resolution.** Every step resolves to exactly one phrase or the compile fails. No guessing.

## 2. Lexical structure

- **Indentation-sensitive.** Exactly two spaces per level; an indent must open exactly one level. The lexer emits `INDENT` and `DEDENT` tokens (Python style). Tabs in indentation are an error.
- **Blank and comment-only lines** are skipped and never change layout.
- **Implicit line joining.** Inside `{ }` or `[ ]`, newlines and indentation are ignored, so long records and lists can span lines.
- **Comments** start with `#` (outside strings) and run to end of line. `native` blocks are passed through raw, so `#` there is target code.
- **Names** (lexer token `Name`): a lowercase letter, then letters or digits; `-` and `'` join the word only when a letter or digit follows (`first-class`, `don't`, but `x->` is `x` then `->`). The parser treats a `Name` as an `ident` (fields, holes, variables) or a `word` (templates, steps) by position. Words match case-insensitively.
- **Type names** (lexer token `TypeName`, grammar `Ident`): `[A-Z][A-Za-z0-9]*`. Types, modules, endpoints, failure codes, HTTP verbs.
- **Strings:** double-quoted, single line, `\"` and `\\` escapes only. A phrase template is lexed as one string; the parser splits its words and holes.
- **Numbers:** `-?[0-9]+(\.[0-9]+)?`. A `-` followed by a digit starts a number; there is no arithmetic.
- **Symbols:** `: , . = { } [ ] < > / ->`.
- **Native blocks.** A logical line that is exactly `native <target>` captures every following line indented deeper than it (blank lines included) as one `RAW` token, with the block's base indentation (header + 2) removed and trailing blank lines dropped. The block ends at the first non-blank line at or left of the header's indentation.

### Core keywords

`module` `type` `phrase` `endpoint` `input` `output` `steps` `let` `respond with` `fail with` `fails with` `effects` `spec` `given` `when` `then` `and` `native`

Core keywords are reserved only at the **start of a line**. Inside phrase templates and steps, words like `with` or `input` are ordinary words.

## 3. Grammar (EBNF)

```ebnf
file            = module_decl , { top_decl } ;
module_decl     = "module" , Ident , NEWLINE ;
top_decl        = type_decl | phrase_decl | endpoint_decl ;

(* ---------- types ---------- *)
type_decl       = "type" , Ident , NEWLINE , INDENT , field_line , { field_line } , DEDENT ;
field_line      = field , NEWLINE ;
field           = ident , ":" , type_ref ;
type_ref        = Ident , [ "<" , type_ref , { "," , type_ref } , ">" ] ;
                  (* core: Text Int Decimal Bool Id List<T> Option<T> *)

(* ---------- phrases ---------- *)
phrase_decl     = "phrase" , template , [ "->" , type_ref ] , NEWLINE ,
                  INDENT , phrase_clause , { phrase_clause } , DEDENT ;
phrase_clause   = effects_clause | fails_clause | spec | native_block | steps_block ;
                  (* any order; at most one effects, one fails with, one native per target;
                     native blocks OR steps, not both; at least one of them *)
template        = '"' , template_part , { template_part } , '"' ;   (* >= 1 word *)
template_part   = word | hole ;
hole            = "{" , ident , ":" , type_ref , "}" ;              (* names unique *)

effects_clause  = "effects" , ( "none" | Ident , { "," , Ident } ) , NEWLINE ;
fails_clause    = "fails with" , Ident , { "," , Ident } , NEWLINE ;

impl            = native_block , { native_block }      (* one per target *)
                | steps_block ;                         (* composite phrase *)
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
step            = let_step | respond_step | fail_step | phrase_call ;
let_step        = "let" , ident , "=" , phrase_call ;
respond_step    = "respond with" , record ;
fail_step       = "fail with" , Ident ;
phrase_call     = call_part , { call_part } ;           (* resolved, see section 4 *)
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
                  [ given_line , { and_line } ] ,        (* no givens: phrase with no holes *)
                  when_line ,
                  then_line , { and_line } ,
                  DEDENT ;
given_line      = "given" , ( binding | "input" , record ) , NEWLINE ;
binding         = ident , "=" , expr ;                  (* one per hole, phrase specs *)
when_line       = "when" , ( "called" | "handled" ) , NEWLINE ;
then_line       = "then" , outcome , NEWLINE ;
outcome         = "returns" , expr
                | "responds" , record
                | "fails with" , Ident
                | "succeeds" ;
and_line        = "and" , ( binding | outcome ) , NEWLINE ;
```

## 4. Phrase resolution (the important part)

A step that is not `let`, `respond with`, or `fail with` is a **phrase call**. The compiler resolves it against every phrase template in scope:

1. Split the template into **words** and **holes**. The parser has already split the step into **words** (bare names) and **arguments** (dotted paths, literals, records, lists).
2. A hole matches exactly **one argument**, or **one bare word that names a variable in scope** (a `let`, or a hole of the enclosing composite phrase). So `respond with { totalQuantity: total }` and `ensure items is not empty` inside a composite both work. Arguments never contain spaces outside brackets, so matching is a linear scan with no backtracking.
3. Words match case-insensitively; whitespace is normalized.
4. **Exactly one** template must match. Zero matches is error `PH001 unknown phrase` (with the closest templates suggested). Two or more is error `PH002 ambiguous phrase`.
5. The matched expressions are type-checked against the hole types.

Example:

```
template:  total quantity of {items: List<LineItem>}
step:      total quantity of input.items
result:    call total-quantity-of(items = input.items) : Int
```

### Library lints

- `PH010 duplicate skeleton`: two templates with the same word sequence (holes ignored) are an error.
- `PH011 near-duplicate`: templates that differ only by a synonym from the controlled vocabulary (`check` vs `ensure`) warn.

### Controlled vocabulary (starter set)

| Verb | Meaning |
|---|---|
| `ensure` | Guard. Returns nothing; fails with a declared code. |
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

### Lexer diagnostics

| Code | Meaning |
|---|---|
| `PH100` | Tab used in indentation |
| `PH101` | Dedent to a level that matches no open block |
| `PH102` | Indent that opens more (or less) than one 2-space level |
| `PH103` | String not closed before end of line |
| `PH104` | Character Phrase does not use (e.g. `+`) |
| `PH105` | Native block line under-indented (deeper than header, shallower than base) |
| `PH106` | Native block has no body |
| `PH107` | Unknown string escape |
| `PH108` | `{` or `[` never closed |
| `PH109` | `}` or `]` with no matching opener |

### Parser diagnostics

| Code | Meaning |
|---|---|
| `PH200` | Syntax error: expected X, found Y |
| `PH201` | Top-level line is not `module`, `type`, `phrase` or `endpoint` |
| `PH202` | Module declaration missing, not first, or repeated |
| `PH203` | Required part missing: type fields, phrase implementation, endpoint `input`/`steps`, spec `when`/`then` |
| `PH204` | Clause or section repeated (`fails with`, `input`, `when`, `native <same target>`, ...) |
| `PH205` | Invalid template (see template rules) |
| `PH206` | Duplicate hole name in a template |
| `PH207` | Unknown native target |
| `PH208` | Phrase has both native blocks and steps |
| `PH209` | Unknown HTTP verb |
| `PH210` | Spec clauses out of order, or `and` with nothing to continue |
| `PH211` | Unexpected indented block |

The parser skips a bad line together with its indented children and keeps going, so one error never hides the declarations after it.

The lexer always returns a complete token stream with balanced `INDENT`/`DEDENT`, even with errors, so later stages can keep reporting.

## 5. Semantics in brief

- **Failures** are declared codes, not exceptions. A phrase may only fail with codes in its `fails with` clause, plus codes from phrases it calls. Providers map failures to their idiom (C#: a `PhraseFailure` carrying the code, mapped to HTTP 422 at endpoints).
- **Effects** default to `none`. A phrase that calls an effectful phrase must declare the union. Providers use effects to choose sync vs async and what to inject.
- **Naming in targets.** Providers convert names to target conventions: field `quantity` becomes `Quantity` in C#, `quantity` in TypeScript. `native` blocks use target conventions.
- **Missing native block.** If a leaf phrase has no `native` block for the selected target, compilation fails with `PH020 no implementation for target`. (Later: the LLM-assisted generator proposes one, gated by the phrase's specs.)

## 6. Compilation pipeline

```
.phrase source
  -> lexer (INDENT/DEDENT)
  -> parser (AST)
  -> resolver (phrase calls bound, types checked, lints)
  -> IR (phrase graph: types, templates, contracts, effects, specs, impls)
  -> provider (C#: FastEndpoints + xUnit | TS: Hono + Vitest | Python: FastAPI + pytest)
```

Each generated target symbol carries its source phrase (C#: `[Phrase("...")]` attribute), so an LLM reading generated code can always trace back to the phrase.

## 7. Out of scope for v0.1

- Emitting composite phrases with `steps` (the parser supports them; the first provider will not)
- Effects other than `none`
- Content-hash pinning and the generation cache (`phrase.lock`)
- The `phrase index` command (one line per phrase: template, return type, effects, failures)
- Modules importing other modules
