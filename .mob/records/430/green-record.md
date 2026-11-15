# Card 430 green record

## git diff --stat

```
 .../EndpointFailureMapsToHttpStatus.cs             |  1 +
 .../GeneratedApiBoundAddress.cs                    |  1 +
 .../Avowry.Acceptance.Tests/GeneratedApiHazards.cs |  1 +
 .../Avowry.Acceptance.Tests/GetEndpointsCompile.cs |  1 +
 .../PutAndDeleteEndpointsCompile.cs                |  1 +
 .../Avowry.Acceptance.Tests/SourceMapDeployment.cs |  1 +
 tests/Avowry.Acceptance.Tests/TasksOverHttp.cs     |  1 +
 .../WallClockSpellings430.cs                       |  8 --
 .../WallClockTestsRunIsolated.cs                   | 92 +++++++++++++++++++---
 9 files changed, 90 insertions(+), 17 deletions(-)
```

## Port notes

- Ported the reference guard scan into `WallClockTestsRunIsolated.cs` for the constants/regexes and the detection, parsing, attribute, and blanking pipeline, while preserving the class shape and required private signatures (`Find(IEnumerable<Source>)`, `Src`, `Describe`, `TestClass`).
- Removed probe-only machinery by omission in the port: no `mutant` field/property, no `M(string)`, no regex `Build()` rebuild, no `M("...")` branches, no probe `Run(...)` adapter; all regexes are now static readonly non-mutant patterns built once.
- Kept wall-clock token construction from pieces (`Ela + "psed"`, `"Cancellation" + "TokenSource"`), matching the reference behavior.
- Updated the guard header comment to state card 430 extension scope (spellings, test types, ruling 4 named limits).
- No behavioral differences were introduced from the reference scan logic; only probe-only switch paths were removed as directed.

## Class collection attributes

- Added `[Collection("Isolated timing")]` directly above class declarations in:
  - `TasksOverHttp.cs`
  - `GeneratedApiHazards.cs`
  - `EndpointFailureMapsToHttpStatus.cs`
  - `GetEndpointsCompile.cs`
  - `PutAndDeleteEndpointsCompile.cs`
  - `GeneratedApiBoundAddress.cs`
  - `SourceMapDeployment.cs`
- Class names matched file names in all seven files.

## Pending trait removal

- Removed all eight `[Trait("Status", "Pending")]` lines from `WallClockSpellings430.cs` and changed nothing else in that file.

## Command summary lines

- `dotnet build Avowry.sln -c Debug`: Build succeeded. 0 Warning(s), 0 Error(s).
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=430"`: Passed. Failed: 0, Passed: 162, Skipped: 0, Total: 162.
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=417"`: Passed. Failed: 0, Passed: 25, Skipped: 0, Total: 25.

## WallClockTestsRunIsolated.cs line count

- 851 lines.

## Model report

- No CLI model line was shown in the command outputs in this run.

## Fix round 1 green

### git diff --stat

```
 .../GeneratedApiBoundAddressEdges.cs               |  1 +
 tests/Avowry.Acceptance.Tests/StarvationProbe.cs   |  1 +
 tests/Avowry.Acceptance.Tests/WallClockFix430.cs   |  2 --
 .../WallClockSpellings430.cs                       |  1 -
 .../WallClockTestsRunIsolated.cs                   | 22 ++++++++++++++--------
 5 files changed, 16 insertions(+), 11 deletions(-)
```

### Pattern deltas (exact old -> new)

- `HttpTimeoutInit`
  - old: `@"(?:new\s+HttpClient\b|\bHttpClient\s*\??\s+\w+\s*=\s*new\s*\()[^;]{0,400}?\bTimeout\s*=(?!=)" + NotInfinite`
  - new: `@"(?:new\s+HttpClient\b|\bHttpClient(?:\s*\?\s*|\s+)\w+\s*=\s*new\s*\()[^;]{0,400}?\bTimeout\s*=(?!=)" + NotInfinite`
- `HttpDeclared`
  - old: `@"\b(?:HttpClient\s*\??|var)\s+(\w+)\s*=\s*new\s+HttpClient\b|\bHttpClient\s*\??\s+(\w+)\s*=\s*new\s*\("`
  - new: `@"\b(?:HttpClient(?:\s*\?\s*|\s+)|var\s+)(\w+)\s*=\s*new\s+HttpClient\b|\bHttpClient(?:\s*\?\s*|\s+)(\w+)\s*=\s*new\s*\("`
- `CtsTargetTyped`
  - old: `@"\b" + CtsType + @"\s*\??\s+\w+\s*=\s*new\s*\(\s*[^)\s]"`
  - new: `@"\b" + CtsType + @"(?:\s*\?\s*|\s+)\w+\s*=\s*new\s*\(\s*[^)\s]"`
- `WaitAsyncLimit` helper set
  - old exclusion forms: `(?:[\w.]*\.)?(?:\w*[Tt]oken|ct)\s*[,)]|CancellationToken\s*\.\s*None\b`
  - new exclusion forms: `cancellationToken\s*:(?!:)|[\w.]*(?:[Cc]t|[Tt]oken|[Cc]ancel)\s*[,)]|(?:System\s*\.\s*Threading\s*\.\s*)?Timeout\s*\.\s*Infinite(?:TimeSpan)?\s*[,)]|CancellationToken\s*\.\s*None\b`
- `WaitAsyncLimit`
  - old: `@"\.\s*WaitAsync\s*\(\s*(?![)]|(?:[\w.]*\.)?(?:\w*[Tt]oken|ct)\s*[,)]|CancellationToken\s*\.\s*None\b)"`
  - new: `@"\.\s*WaitAsync\s*\(\s*(?![)]" + WaitAsyncTokenForms + ")"`

### Command summary lines

- `dotnet build Avowry.sln -c Debug`: Build succeeded. 0 Warning(s), 0 Error(s).
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=430"`: Passed. Failed: 0, Passed: 358, Skipped: 0, Total: 358.
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=417"`: Passed. Failed: 0, Passed: 25, Skipped: 0, Total: 25.

### WallClockTestsRunIsolated.cs line count

- `857`

### Model report

- No CLI model line was shown in command output.
