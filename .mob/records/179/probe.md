# Card 179 baseline probe (facilitator, 2026-11-08): the guide's commands run once, PowerShell 7.6.9, Windows, SDK 10.0.403 only (no 8.0.x SDK installed; the net8.0 projects built and ran)

Script: the facilitator's probe179.ps1 (steps 1 to 10 and a rebuild with a leftover file), in a fresh single-branch clone of the local main (so the unpushed-HEAD limit applies: the clone sees only committed work). Output, verbatim:

```
PSVersion 7.6.9
SDKS: 10.0.403 [C:\Program Files\dotnet\sdk]
== 1 clone (local path, unpushed HEAD limit)
Updating files: 100% (1756/1756)
Updating files: 100% (1756/1756), done.
== 2 pack
  Avowry.Cli -> C:\Users\ringe\AppData\Local\Temp\avowry-probe179\avowry\src\Avowry.Cli\bin\Release\net8.0\publish\
  The package Avowry.0.1.0 is missing a readme. Go to https://aka.ms/nuget/authoring-best-practices/readme to learn why package readmes are important.
  Successfully created package 'C:\Users\ringe\AppData\Local\Temp\avowry-probe179\feed\Avowry.0.1.0.nupkg'.
== 3 tool install
You can invoke the tool using the following command: avowry
Tool 'avowry' (version '0.1.0') was successfully installed.
== 4 version
Avowry 0.1.0
== 5 build
Tasks.Api
Tasks.Specs
Tasks.avowrymap.json
== 6 test
  Tasks.Api -> C:\Users\ringe\AppData\Local\Temp\avowry-probe179\out\Tasks.Api\bin\Debug\net8.0\Tasks.Api.dll
  Tasks.Specs -> C:\Users\ringe\AppData\Local\Temp\avowry-probe179\out\Tasks.Specs\bin\Debug\net8.0\Tasks.Specs.dll
Test run for C:\Users\ringe\AppData\Local\Temp\avowry-probe179\out\Tasks.Specs\bin\Debug\net8.0\Tasks.Specs.dll (.NETCoreApp,Version=v8.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    11, Skipped:     0, Total:    11, Duration: 683 ms - Tasks.Specs.dll (net8.0)
== 7 run (background)
API UP True
== 8 create
{"task":{"id":"task-1","title":"Write the demo","done":false}}
== 9 read
{"task":{"id":"task-1","title":"Write the demo","done":false}}
== 10 failure (empty title)
FAIL STATUS 422 :: {"statusCode":422,"message":"One or more errors occurred!","errors":{"generalErrors":["EmptyTitle"]}}
== rebuild with a leftover file
LEFTOVER STILL THERE: True
PROBE179 DONE
```

Findings the guide must state or respect:
- pack, tool install from the local feed, `avowry --version` (Avowry 0.1.0), `avowry build demo/tasks/Tasks.avowry -o OUT` (writes Tasks.Api, Tasks.Specs, Tasks.avowrymap.json), `dotnet test OUT/Tasks.Specs` (Passed: 11) and `dotnet run --project OUT/Tasks.Api --urls http://localhost:5099` all work as the slice says.
- create returns {"task":{"id":"task-1","title":"Write the demo","done":false}}; read returns the same; an empty title returns 422 with errors.generalErrors ["EmptyTitle"] (read from ErrorDetails.Message in PowerShell).
- A rebuild into the same OUT leaves a leftover file in place: the guide says rebuild overwrites the same relative paths and does not delete leftover files (confirmed).
- `dotnet pack` warns that the package has no readme (not an error); the guide need not mention it.
- Not probed: Windows PowerShell 5.1, bash, an 8.0.x SDK, a clone from the pushed remote (the local-path clone sees committed work only).
