# Card 172 slice (facilitator; binding on red and green; adopted by D282)

## Criterion 1: trace guards (src/Avowry.Cli/Program.cs)
- `CheckHash` (line 266) returns `File.Exists(path) && Hash(...) != record.CsHash`: a missing generated file reads as "hash matches". After: a missing file is reported as a warning naming the record's csFile; with `--strict` trace exits nonzero (follow how `--strict` already treats a hash mismatch in `TraceCore`, line 159).
- A record whose `csFile` is absolute (any rooted path, a drive letter, a leading `/` or `\`) or contains a `..` segment is refused with exit 2 and a message naming the record, before any file is read (the validation near line 258 is the natural place).
- Tests (true reds): one per behaviour, through the CLI as `SourceMapTraceCommand.cs` runs it, with a map planted in a temp folder: a deleted generated file (warning; nonzero with `--strict`); `csFile` = an absolute path; `csFile` with `..` (both exit 2).

## Criterion 2: one shared nested-dotnet helper (tests/Avowry.Acceptance.Tests/Support/)
- Today the environment scrub, the MSBuild node-reuse settings, the timeout and the process kill are repeated in `DotnetCli.cs`, `GeneratedApi.cs`, `PhraseCli.cs`, `ProcessRunner.cs`, `NoMsbuildNodeReuse.cs`, and in the card 167 and 168 tests (`SourceMapDeployment.cs`, `SourceMapTraceCommand.cs`), with `Kill()` without the tree in some places (GeneratedApi.cs:112 and :243, LlmCustomerCheck.cs:242, BenchTimeoutIsolation.cs:65).
- One helper owns the scrub list, node-reuse settings, the timeout, the process-tree kill and the wait for exit; DotnetCli, GeneratedApi, PhraseCli and the card 167 and 168 helpers call it; their copies are removed. Out of scope: the bench and LLM-check tests' own process handling (not nested dotnet test helpers), unless trivially the same.
- Tests: a test that starts a child that starts a grandchild, lets the helper time it out, and proves both processes are gone (by process id, with a bounded wait); a source scan that the scrub list and the node-reuse settings are defined once and that no listed helper calls `Kill(` itself.

## Criterion 3
No test removed or renamed (the red lists the test names before and after); the generated C# unchanged (the Slice 142 orders pin `19fe6d71`, tests/Avowry.Acceptance.Tests/WebExtensionBoundary.cs:18, and the Slice 176 demo golden); the full gate green.

## Decided by D282
The avow record keeps today's endpoint rule (docs/TRACEABILITY.md); no change in this card.

dotnet runs are the facilitator's, under Avowry's heavy lock.

## R172-1 (facilitator ruling, 2026-11-17 about 04:06Z): the red's four ambiguities
All four are accepted as the red wrote them: (1) one scrub list, the union of the 12 names (DotnetCli and PhraseCli scrubbed 4); (2) ProcessRunner is in the scanned set and may not keep its own Kill; (3) `--strict` on a missing generated file exits 1, like a hash mismatch; (4) any csFile in the map that leaves the output folder refuses the trace, whichever record is traced.

## R172-2 (facilitator ruling, 2026-11-17 about 04:33Z): fix round 1 from review.md (Opus CHANGES)
- B1 (red): update `ProcessStartCallers` in GeneratedApiSourceScan.cs to the files that now start processes (NestedDotnet.cs in, the five moved files out); nothing removed or renamed. Every later proof of this card runs the full `Status!=Pending` set, not a slice filter (a filtered proof missed this).
- M2 (green): after the tree kill, the exit wait and both drains are bounded (a short fixed bound), then the helper reports the timeout as before.
- M3 (green): ProcessRunner keeps "no timeout" meaning no limit when called with null, and surfaces TimedOut to its callers as a clear failure.
- M4 (red): the timeout test reads the pid file inside its finally; KillIfRunning checks the process start time against the recorded one before killing (no reused pid).
- M6 (green): docs/TRACEABILITY.md line 11 describes the missing-file warning (exit 1 with --strict) and the exit 2 refusal.
- M5 and M7: named limits.
