# Getting started

This guide takes you from a clone to a running spec-first API, using the C# adapter preview. It should take about 15 minutes. Every command has a bash form and a PowerShell form, so Windows needs no bash.

Before you start, read [docs/PREREQUISITES.md](PREREQUISITES.md). You need the .NET 8 SDK; a newer SDK or a newer ASP.NET Core runtime does not replace it. The package is not published, so you install it from a local folder you pack yourself. In the commands below the folders `avowry-feed`, `avowry-tools` and `avowry-tasks` are scratch folders: bash uses `/tmp/`, PowerShell uses `$env:TEMP\`.

## Check the SDK

Run from: any directory
Needs: the `dotnet` command

```bash
dotnet --list-sdks
```

Same in PowerShell.

The list must hold an 8.0 SDK.

## 1. Clone the repository

Run from: any directory
Needs: git

```bash
git clone https://github.com/Programming-Made-EZ/avowry.git
cd avowry
```

Same in PowerShell.

## 2. Pack the compiler into a local feed

Run from: the repository root, the `avowry` folder you just cloned
Needs: the .NET 8 SDK

```bash
dotnet pack src/Avowry.Cli/Avowry.Cli.csproj -c Release -o /tmp/avowry-feed
```

```powershell
dotnet pack src/Avowry.Cli/Avowry.Cli.csproj -c Release -o "$env:TEMP\avowry-feed"
```

A warning that the package has no readme is harmless.

## 3. Install the avowry command

Run from: the repository root
Needs: the .NET 8 SDK and the packed local feed

```bash
dotnet tool install Avowry --tool-path /tmp/avowry-tools --add-source /tmp/avowry-feed
/tmp/avowry-tools/avowry --version
```

```powershell
dotnet tool install Avowry --tool-path "$env:TEMP\avowry-tools" --add-source "$env:TEMP\avowry-feed"
& "$env:TEMP\avowry-tools\avowry.exe" --version
```

The last command prints:

```text
Avowry 0.1.0
```

## 4. A small spec-first API

The sample is `demo/tasks/Tasks.avowry`. An avow is one line of intent. An endpoint says what it takes and returns, and its specs say what must hold. This excerpt shows creating a task, reading it back, and a declared failure (`fails with EmptyTitle`); the file also lists, completes and deletes tasks.

```avowry
avow to "validate task title {title: Text}"
  fails with EmptyTitle, TitleTooLong
  steps
    ensure title is not blank else EmptyTitle
    ensure title length at most 200 else TitleTooLong

endpoint CreateTask : POST /tasks
  input
    id: Id
    title: Text
  output
    task: Task
  steps
    validate task title input.title
    let task = create task input.id titled input.title
    respond with { task: task }
  spec "a valid title creates an open task"
    given input { id: "task-1", title: "Write the demo" }
    when handled
    then responds { task: { id: "task-1", title: "Write the demo", done: false } }
  spec "an empty title is rejected"
    given input { id: "task-1", title: "" }
    when handled
    then fails with EmptyTitle

endpoint GetTask : GET /tasks/{id}
  input
    id: Id
  output
    task: Task
  steps
    let task = find task input.id
    respond with { task: task }
  spec "a known id returns its task"
    given input { id: "task-1" }
    when handled
    then responds { task: { id: "task-1", title: "Write the demo", done: false } }
```

## 5. Build it

Run from: the repository root
Needs: the installed avowry command

```bash
/tmp/avowry-tools/avowry build demo/tasks/Tasks.avowry -o /tmp/avowry-tasks
```

```powershell
& "$env:TEMP\avowry-tools\avowry.exe" build demo/tasks/Tasks.avowry -o "$env:TEMP\avowry-tasks"
```

It writes `Tasks.Api` (the API), `Tasks.Specs` (a test project with one test per spec) and `Tasks.avowrymap.json`.

## 6. Run the specs

Run from: the repository root
Needs: the .NET 8 SDK and the built output

```bash
dotnet test /tmp/avowry-tasks/Tasks.Specs
```

```powershell
dotnet test "$env:TEMP\avowry-tasks\Tasks.Specs"
```

The last line reports `Passed: 11` of 11, one test for each of the 11 specs in the file:

```text
Passed!  - Failed:     0, Passed:    11, Skipped:     0, Total:    11
```

## 7. Start the API

Run from: the repository root, in a second window (the API keeps running)
Needs: the .NET 8 SDK and the built output

```bash
dotnet run --project /tmp/avowry-tasks/Tasks.Api --urls http://localhost:5099
```

```powershell
dotnet run --project "$env:TEMP\avowry-tasks\Tasks.Api" --urls http://localhost:5099
```

## 8. Call it

Create a task, a read and a declared failure. Use the first window.

### Create a task

Run from: any directory, in the first window
Needs: the API running on port 5099

```bash
curl -s -X POST http://localhost:5099/tasks -H "Content-Type: application/json" -d '{"id":"task-1","title":"Write the demo"}'
```

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5099/tasks -ContentType application/json -Body '{"id":"task-1","title":"Write the demo"}'
```

It returns the task:

```json
{"task":{"id":"task-1","title":"Write the demo","done":false}}
```

### Read it back

Run from: any directory, in the first window
Needs: the API running on port 5099

```bash
curl -s http://localhost:5099/tasks/task-1
```

```powershell
Invoke-RestMethod http://localhost:5099/tasks/task-1
```

It returns the same task.

### A declared failure

A task with an empty title is rejected:

Run from: any directory, in the first window
Needs: the API running on port 5099

```bash
curl -s -i -X POST http://localhost:5099/tasks -H "Content-Type: application/json" -d '{"id":"task-2","title":""}'
```

```powershell
try { Invoke-RestMethod -Method Post -Uri http://localhost:5099/tasks -ContentType application/json -Body '{"id":"task-2","title":""}' } catch { $_.Exception.Response.StatusCode.value__; $_.ErrorDetails.Message }
```

The API answers with status 422 and names the failure the spec declared:

```json
{"statusCode":422,"message":"One or more errors occurred!","errors":{"generalErrors":["EmptyTitle"]}}
```

Stop the API with Ctrl+C in the second window.

## Afterwards

The `.avowry` file is the source: edit it, not the generated code. The generated projects are output. A rebuild overwrites the same relative paths and does not delete leftover files, so build into a fresh folder, or remove the old output yourself, when you rename or drop something.

The known limitations page is not yet written (card 187). For what you need installed, see [docs/PREREQUISITES.md](PREREQUISITES.md).
