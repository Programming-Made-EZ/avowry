SIGN-OFF

# Card 172 cross-family review (qwen, local model, reply-only with the green diff inlined, ctx-172x2, under Avowry's heavy lock; saved by the facilitator)

Note: reviewed green e0780f2 from the inlined diff only; it could not see GeneratedApiSourceScan.cs, which held review.md's BLOCKING finding.


Findings:
- `src/Avowry.Cli/Program.cs` (CheckHash & CsFileLeavesOutputFolder hunks) MINOR: Missing generated file correctly sets a warning string and returns true (hash mismatch), which combined with `strict` exits 1 per R172-1(3). `CsFileLeavesOutputFolder` correctly rejects absolute, drive-letter, and `..` paths with exit 2 via `Validate` per R172-1(4). Edge case: `root is null` returns false (assumes match), but `Path.GetFullPath` makes this practically unreachable for valid map paths.
- `tests/Avowry.Acceptance.Tests/Support/NestedDotnet.cs` (RunAsync & KillEntireTreeIfRunning hunks) MINOR: Shared helper correctly catches timeout, calls `process.Kill(entireProcessTree: true)`, and `await process.WaitForExitAsync()` before returning. `ProcessRunner.cs` delegates to it, and `GeneratedApi.cs` uses the shared kill method, fully satisfying the process-tree kill and wait requirement.
- `tests/Avowry.Acceptance.Tests/Support/DotnetCli.cs` & `PhraseCli.cs` (ApplyHostEnvironment removal & NestedDotnet.Apply addition hunks) MINOR: Both helpers now call `NestedDotnet.Apply(psi)`, replacing their previous 4-variable scrub with the unified 12-variable list. No needed environment variables are lost; the expansion is strictly safer and aligns with R172-1(1). `GeneratedApi.cs` correctly retains its additional ASPNETCORE scrubs after calling `Apply`.
- Diff-wide (test files) MINOR: No test files are removed or renamed in the provided diff, satisfying criterion 3.

Could not check:
- Full gate/CI suite status (only slice tests 172/167/168/142/176 were reported as passed).
- Generated C# output hash (`409bf3c6` / `19fe6d71`) verification (no generated files appear in the diff).
- The specific test proving a timed-out child process tree is fully gone (assumed present in test files not included in the diff).
- Static scan for residual `Kill(` calls in helpers (diff shows removals, but full repository scan is outside scope).
