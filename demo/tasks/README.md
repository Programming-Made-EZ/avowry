# Tasks demo

The Tasks API is the Usable v1 prototype: a five-endpoint CRUD service written in Phrase, compiled to C#, and verified by eleven Given/When/Then specs.

Principle: a language is usable when a reader can compile a real app, run its specs, start it and call an endpoint from one page of commands, because "usable" is a procedure, not a claim.

All commands below run from the repository root. Expected output is the text captured from a hand run of these same commands.

## Prerequisites

- The .NET 8 SDK
- A bash-style shell with `curl` (Git Bash, WSL, macOS or Linux)

Rationale: the compiler and the generated host target .NET 8; `curl` is the HTTP client that matches the live checks.

## Output folder

Generated C# is an artifact, not the source of truth. Write it outside the repository (or to a git-ignored path). These steps use `/tmp/phrase-tasks`.

```bash
dotnet run --project src/Phrase.Cli -- build demo/tasks/Tasks.avowry -o /tmp/phrase-tasks
```

Expected: the Phrase compiler prints nothing and exits 0. It writes `/tmp/phrase-tasks/Tasks.Api` and `/tmp/phrase-tasks/Tasks.Specs`.

```bash
dotnet build /tmp/phrase-tasks/Tasks.Api
```

Expected:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

```bash
dotnet test /tmp/phrase-tasks/Tasks.Specs
```

Expected: `Passed!` `Failed: 0, Passed: 11, Total: 11`

```bash
dotnet run --project /tmp/phrase-tasks/Tasks.Api --urls http://localhost:5099
```

Expected: `Registered 5 endpoints` and `Now listening on: http://localhost:5099`

Keep that process running. The store is in memory and disappears when the API stops (Ctrl+C).

## Call the endpoints

Create an open task:

```bash
curl -sS -X POST http://localhost:5099/tasks \
  -H "Content-Type: application/json" \
  -d '{"id":"task-1","title":"Write the demo"}'
```

Expected HTTP 200:

```
{"task":{"id":"task-1","title":"Write the demo","done":false}}
```

List tasks. The `available` flag exists only so a spec can reach the storage-failure path; without it the answer is HTTP 422 StoreUnavailable.

```bash
curl -sS "http://localhost:5099/tasks?available=true"
```

Expected HTTP 200:

```
{"tasks":[{"id":"task-1","title":"Write the demo","done":false}]}
```

Complete the task:

```bash
curl -sS -X PUT http://localhost:5099/tasks/task-1/complete
```

Expected HTTP 200 with `"done":true`.

Look up a missing id:

```bash
curl -sS -X GET http://localhost:5099/tasks/nope
```

Expected HTTP 422 NotFound.

Reject an empty title:

```bash
curl -sS -X POST http://localhost:5099/tasks \
  -H "Content-Type: application/json" \
  -d '{"id":"task-2","title":""}'
```

Expected HTTP 422 EmptyTitle.

Delete the task:

```bash
curl -sS -X DELETE http://localhost:5099/tasks/task-1
```

Expected HTTP 200 with an empty body.

## Failures

Failures answer HTTP 422 with the declared error name in `generalErrors`. The names come from the avow (`EmptyTitle`, `NotFound`, `StoreUnavailable`, `TitleTooLong`), not from the HTTP stack.

## Native C# remaining

Five `native csharp` blocks remain. Completing a task is now a Phrase update (`task with { done: true }`) plus the existing load and save steps. The remaining blocks are the escape hatch for store operations and record construction that Phrase does not yet express. The list is in [docs/PATTERNS.md](../../docs/PATTERNS.md).

Library patterns belong in Phrase so a reader can see the rule without reading C#:

- Card 159, [a validation step](../../board/159-a-validation-step-in-phrase-ensure-a-condition-or-fail-with-a-declared-name.md): ensure a condition or fail with a declared name
- Card 160, [a record update form](../../board/160-a-record-update-form-a-copy-of-a-record-with-one-field-changed.md): a copy of a record with one field changed

The layering and the order of logic sources (library pattern, adapter idiom, generated body, native escape hatch) are in [docs/adr/ADR-library-binding-layering.md](../../docs/adr/ADR-library-binding-layering.md).
