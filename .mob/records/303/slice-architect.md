# Card 303 slice (architect seat, cycle 1)

Seat: Codex ctx-2, read-only, through the machine-local launcher (codex_nostore.py exec -s read-only), brief at HEAD 4dc6adf. Written by the seat; committed by the facilitator unchanged below the line. Seat order on this slice: Muse (trial, no reply), Grok (narration only), then Codex as declared. Output files: none written by the seat (tracked tree unchanged after the run).

## Facilitator notes before red (not the seat's text)
- The red as cut says "the run fails on the current machine". That is machine-dependent: on Linux CI the endpoint test passes, so a test that requires the failure would be red in CI. QA must make the red machine-independent (for example, drive the zero-harness-tests branch through a fake `dotnet test` output or a runner function that takes the captured output), so CI Linux outcomes stay unchanged (Done check 3).
- The seat's numbers are all "record-only" or its own listed commands (D189); the facilitator's line numbers at HEAD 4dc6adf may differ by a few lines from the seat's; QA and green re-read the file.
- Probe: the seat's PowerShell polling snippet is a suggestion; the green's command and output go in the committed records, whatever the exact capture method.

---

SLICE — cycle 1 / Done check 1

Observable behaviour

When a generated verification project writes a trx containing zero harness tests, Phrase.BenchCs reports `tests_did_not_run: <captured dotnet output>` rather than the bare `tests_did_not_run`.

Check 1 is evidence-only; this is the smallest code behaviour that exposes the evidence needed to name the cause. It does not claim that the missing ASP.NET Core runtime is proven.

Smallest RED

Location:
tests/Phrase.Acceptance.Tests/CSharpBaselineAcceptance.cs

Add one xUnit fact beside the endpoint tests. Run the existing correct endpoint case and assert:

- the run fails on the current machine;
- the first diagnostic starts with `tests_did_not_run:`;
- text after that prefix is non-empty;
- the failure message includes the complete runner result via `FailureDetail`.

RED now: the diagnostic equals bare `tests_did_not_run`.
GREEN: the zero-harness-tests branch appends the captured stdout/stderr.

It cannot pass from merely renaming the diagnostic or changing the endpoint assertion: the required prefix and non-empty suffix must both be present. Do not assert `Microsoft.AspNetCore.App` yet; that would promote the lead to a finding before the probe.

This remains inside the card: it changes diagnostic visibility only. It neither weakens endpoint assertions, skips Windows, nor touches UsableV1.Bar6.

Probe — green runs this first

The runner deletes its scratch tree in `finally`, so capture a verification directory while the failing acceptance test is running:

$probeRoot = Join-Path $env:TEMP 'phrase-bench-cs'
$job = Start-Job { dotnet test tests/Phrase.Acceptance.Tests --filter "FullyQualifiedName~CSharpBaselineAcceptance.endpoint_reply_passes_when_response_record_and_status_match" }
do {
  $project = Get-ChildItem $probeRoot -Directory -Recurse -Filter 'verification-*' -ErrorAction SilentlyContinue |
    Where-Object { Test-Path (Join-Path $_.FullName 'Verification.csproj') } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
} until ($project)
$copy = Join-Path $env:TEMP 'card303-verification-probe'
Copy-Item -LiteralPath $project.FullName -Destination $copy -Recurse
dotnet test (Join-Path $copy 'Verification.csproj') --nologo --logger 'trx;LogFileName=probe.trx'
Receive-Job $job -Wait

The required probe is the final `dotnet test` command: it deliberately omits `--verbosity quiet`.

Commit under .mob/records/303/:

- cause-probe.txt — generated directory, exact command, complete output, and captured error lines.
- runtimes.txt — command and output of `dotnet --list-runtimes`.
- ci-comparison.txt — CI run 42146757413, attempts 1 and 2, and what Linux outcomes are being compared.

Only after those records exist may the card name the cause.

Green may touch

- tools/Phrase.BenchCs/Program.cs
- .mob/records/303/cause-probe.txt
- .mob/records/303/runtimes.txt
- .mob/records/303/ci-comparison.txt

The QA/red seat may touch:

- tests/Phrase.Acceptance.Tests/CSharpBaselineAcceptance.cs

Must not touch

- Existing endpoint expectations or generated outcome assertions
- Any other acceptance test
- EndpointVerificationProject target framework or FrameworkReference
- UsableV1.Bar6 files
- CI configuration
- Package versions
- Skip/platform conditions

Risks

- The scratch project may disappear before copying; keep the polling loop running while the acceptance test executes.
- Copying during restore may capture an incomplete obj tree, but rerunning `dotnet test` in the copy regenerates it.
- Captured output may be empty. Record that result and do not infer a cause; it satisfies the cycle’s E case but not a root-cause claim.
- Installing a runtime or adding roll-forward is cycle 2 work, not this slice.

Unanswered question

What exact host error does the generated project print without quiet verbosity? The files do not contain it. The probe must answer this before choosing an environment or harness remedy.

Number provenance

Number quoted | Provenance
303 | Record-only: card identifier supplied by facilitator and filenames
1 (cycle/check) | Record-only: card Done check and requested slice
0 harness tests | Record-only: taskforce-proposal-pm-r2.md; facilitator rerun
42146757413 | Record-only: taskforce-proposal-pm-r2.md and card
1 and 2 (CI attempts) | Record-only: taskforce-proposal-pm-r2.md and card
Program.cs 230, 247, 312, 326 | `Get-Content` with indexed line output; 230 invokes quiet test, 247 emits the bare diagnostic, 312 declares EndpointVerificationProject, 326 adds FrameworkReference
Acceptance lines 85 and 107 | `rg -n "endpoint_reply_" tests/Phrase.Acceptance.Tests/CSharpBaselineAcceptance.cs`
Scratch lines 122, 197, 212 | `rg -n "GetTempPath|Directory\.Delete|project =" tools/Phrase.BenchCs/Program.cs`
