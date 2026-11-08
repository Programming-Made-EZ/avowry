# Card 176 characterisation 3 (D157 Criterion 3), QA (red) seat

Declared CHARACTERISATION, not a red. The tests pass on the pre-change tree (HEAD dc2db36afd9d259e238a98811afdf565500c4c8a, no card 176 packaging change yet). Reason: criterion 3 says packaging changes neither the compiler's output nor the existing tests, and the demo's generated C# equals its pre-change result.

## Existing tests checked first

- `PhraseBuildOneFile.Build_output_matches_the_compiler_bytes` compares CLI output to `AvowryCompiler.Compile` plus `WriteTo` for `tests/Avowry.Acceptance.Tests/Fixtures/orders.avowry`, not `demo/tasks`.
- `TasksDemoApp.Tasks_api_builds_and_every_endpoint_has_passing_success_and_failure_specs` builds `demo/tasks/Tasks.avowry` through the CLI, then asserts the API builds with zero warnings, the generated specs pass, and `Avows.cs` has four `lock (gate)` sites. It does not compare the demo's generated C# against the compiler's bytes or a golden file.

Nothing already compared the demo's generated C# against the compiler's own bytes, so a compact golden-file test was added.

## What was written

- `tests/Avowry.Acceptance.Tests/TasksDemoGeneratedCsharp.cs`: builds `demo/tasks/Tasks.avowry` with `PhraseCli` into a `TempWorkspace` and compares every generated `.cs` file (paths relative, sorted, LF-normalised, `bin`/`obj` skipped) to the golden file. Traits: `Slice` = `176`, `Criterion` = `3`.
- `tests/Avowry.Acceptance.Tests/TasksDemoGeneratedCsharp.golden`: concatenated snapshot of the 16 generated C# files from `avowry build demo/tasks/Tasks.avowry` on this tree. 28532 bytes, sha256 `17991fcd84dae73f794f5208c07d1161d28ab075f8800fc66e96a90e86d7e563`.

## Run

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter FullyQualifiedName~Tasks_demo_generated_csharp_matches_the_golden_file`

```
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 428 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

The new test passed on the unchanged tree.
