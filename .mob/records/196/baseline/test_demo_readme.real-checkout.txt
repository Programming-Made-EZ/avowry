......
----------------------------------------------------------------------
Ran 6 tests in 0.084s

OK
## Usable v1: 6 of 6 criteria met

- MET: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry)
- MET: bar 2, One CLI command compiles it to C#/FastEndpoints
- MET: bar 3, dotnet build of the generated projects has zero warnings
- MET: bar 4, The generated xUnit specs all pass, one test per spec
- MET: bar 5, The app runs and live HTTP matches the specs
- MET: bar 6, A model that never saw the app adds an endpoint from the docs alone
USABLE v1 REACHED: demo app at demo/tasks (see demo/tasks/README.md for how to run it)
::notice::USABLE v1 REACHED
## Usable v1: 0 of 6 criteria met

- not met: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry) (x)
- not met: bar 2, One CLI command compiles it to C#/FastEndpoints (x)
- not met: bar 3, dotnet build of the generated projects has zero warnings (x)
- not met: bar 4, The generated xUnit specs all pass, one test per spec (x)
- not met: bar 5, The app runs and live HTTP matches the specs (x)
- not met: bar 6, A model that never saw the app adds an endpoint from the docs alone (x)
::error::Usable v1 REGRESSION: bar(s) [1, 2, 3, 4, 5, 6] previously passed (board/usable-v1-ratchet.json) and now fail
## Usable v1: 5 of 6 criteria met

- MET: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry)
- MET: bar 2, One CLI command compiles it to C#/FastEndpoints
- MET: bar 3, dotnet build of the generated projects has zero warnings
- MET: bar 4, The generated xUnit specs all pass, one test per spec
- MET: bar 5, The app runs and live HTTP matches the specs
- not met: bar 6, A model that never saw the app adds an endpoint from the docs alone (stale)
::error::Usable v1 REGRESSION: bar(s) [6] previously passed (board/usable-v1-ratchet.json) and now fail
## Usable v1: 5 of 6 criteria met

- MET: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry)
- MET: bar 2, One CLI command compiles it to C#/FastEndpoints
- MET: bar 3, dotnet build of the generated projects has zero warnings
- MET: bar 4, The generated xUnit specs all pass, one test per spec
- MET: bar 5, The app runs and live HTTP matches the specs
- SUSPENDED: bar 6, A model that never saw the app adds an endpoint from the docs alone (stale)
::warning::Usable v1 bar 6 is SUSPENDED, its evidence is stale: reason
USABLE v1 REACHED: demo app at demo/tasks (see demo/tasks/README.md for how to run it)
::notice::USABLE v1 REACHED (with suspended bars)
## Usable v1: 4 of 6 criteria met

- MET: bar 1, A real app written entirely in .avowry (demo/tasks/Tasks.avowry)
- MET: bar 2, One CLI command compiles it to C#/FastEndpoints
- MET: bar 3, dotnet build of the generated projects has zero warnings
- MET: bar 4, The generated xUnit specs all pass, one test per spec
- not met: bar 5, The app runs and live HTTP matches the specs (x)
- SUSPENDED: bar 6, A model that never saw the app adds an endpoint from the docs alone (stale)
::error::Usable v1 REGRESSION: bar(s) [5] previously passed (board/usable-v1-ratchet.json) and now fail
self-test ok
exit 0
