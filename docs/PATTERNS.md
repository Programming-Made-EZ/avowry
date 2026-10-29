# Avowry patterns

Patterns keep portable intent in Avowry while allowing each target adapter to emit idiomatic code. They are preferred over generated bodies and native blocks when the base library has a matching universal operation.

## Ensure

An ensure step states a condition that must hold and the declared failure raised when it does not:

```phrase
phrase "validate task title {title: Text}"
  fails with EmptyTitle, TitleTooLong
  steps
    ensure title is not blank else EmptyTitle
    ensure title length at most 200 else TitleTooLong
```

The failure after `else` must appear in the containing avow's `fails with` list. An endpoint may also use an ensure step; its failure becomes one of the endpoint's effective failures.
Failure names in a `fails with` list must be unique.

The condition vocabulary is deliberately small:

- `<text> is blank` and `<text> is not blank`
- `<text> length at most <integer>`
- `<int> <operator> <integer-or-int-input>`, where the operator is `<`, `<=`, `>`, `>=`, `==`, or `!=`

The value being checked may be a variable or a field path such as `task.title`. Paths are resolved against declared record fields and must have the type required by the condition. Length maxima are non-negative 32-bit integers, and Int literals in comparisons must fit in a signed 32-bit Int.

The core syntax and base-library pattern define the condition and declared-failure semantics. The C# adapter maps them to early-return-style guard clauses that raise the declared `PhraseFailure`; for example, `is not blank` becomes a `string.IsNullOrWhiteSpace` guard and `length at most 200` becomes a length guard. A project profile may select a different idiomatic representation later without changing the Avowry source.

## Record update

A record update is a copy of a record with named fields replaced. The original value is not mutated:

```phrase
let completed = task with { done: true }
```

The source must be a record already in scope (a hole, a prior `let`, or `input`, optionally followed by a field path). The parser reserves `with { ... }` for an update only when the name before `with` is already in scope and a brace follows; the update itself then requires a non-empty `name: value` field list, and an empty or malformed list is a parse diagnostic (PH200), not a phrase call. This narrow reservation leaves ordinary phrase calls such as `let sent = send with { urgent: true }` available when `send` is wording rather than an in-scope value.

Each named field must exist on that record and match the field's type (Text, Int, Bool, Id, or a user type). Duplicate names in one update are an error; an unknown field is an error even when the record declares no fields. A trailing comma is not allowed. Nested update expressions are deliberately rejected with a syntax diagnostic; bind the inner update to a `let` first, then use that name as the outer field value.

The form is a core pattern, usable in the steps of an avow and of an endpoint. The C# adapter emits a `with`-expression on the generated record type (positional records already support it). A project profile may change surrounding layout later without changing the Avowry source.

Example, completing a stored task with the existing load and save steps:

```phrase
endpoint CompleteTask : PUT /tasks/{id}/complete
  steps
    let task = find task input.id
    let completed = task with { done: true }
    save completed
    respond with { task: completed }
```

## Remaining native blocks in the Tasks demo

Five `native csharp` blocks remain after the update form. Each is an escape hatch for a construct Avowry does not yet express:

| Avowry source | Construct still lacking |
|---|---|
| `create task {id} titled {title}` | constructing a new record and a save store step (card 145) |
| `find task {id}` | a load store step that fails when missing (card 145) |
| `list tasks {available}` | a list store step, and a branch on a flag |
| `save {task}` | a save store step (card 145) |
| `delete task {id}` | a remove store step (card 145) |
