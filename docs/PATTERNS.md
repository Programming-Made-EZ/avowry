# Avowry patterns

Patterns keep portable intent in Avowry while allowing each target adapter to emit idiomatic code. They are preferred over generated bodies and native blocks when the base library has a matching universal operation.

## Ensure

An ensure step states a condition that must hold and the declared failure raised when it does not:

```avowry
avow to "validate task title {title: Text}"
  fails with EmptyTitle, TitleTooLong
  steps
    ensure title is not blank else EmptyTitle
    ensure title length at most 200 else TitleTooLong
```

The conditions, the failure rules and their diagnostics are defined in [the grammar's ensure step section](GRAMMAR.md#ensure-step).

The core syntax and base-library pattern define the condition and declared-failure semantics. The C# adapter maps them to early-return-style guard clauses that raise the declared `AvowryFailure`; for example, `is not blank` becomes a `string.IsNullOrWhiteSpace` guard and `length at most 200` becomes a length guard. A project profile may select a different idiomatic representation later without changing the Avowry source.

## Record update

A record update is a copy of a record with named fields replaced. The original value is not mutated:

```avowry
let completed = task with { done: true }
```

The source rules, the field rules and their diagnostics are defined in [the grammar's record update step section](GRAMMAR.md#record-update-step).

The form is a core pattern, usable in the steps of an avow and of an endpoint. The C# adapter emits a `with`-expression on the generated record type (positional records already support it). A project profile may change surrounding layout later without changing the Avowry source.

Example, completing a stored task with the existing load and save steps:

```avowry
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
