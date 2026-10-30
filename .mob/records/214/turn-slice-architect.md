# Card 214 slice (architect, ctx-6)

Decision: SHIP AS IS as one card (no split cards) under D136 and D156 C3, cut into 6 commits with red before fix (D156 C4). One concern: the harness owns its port (GeneratedApi.StartAsync). The largest commit is about 5 files and 250 lines; none goes near 15 files or 400 lines. If red commit 3 passes 400 lines, split it at the Z/O/M/I versus B/E/S cases (already the planned seam).

## 1. Seams (smallest set)
| Seam | Verdict | Why, and what moves if dropped |
|---|---|---|
| S1 `BeforeChildStart` hook | NEEDED | Only way to read the chosen URL, hold the port and count starts without sleeps. Default null, fires once. |
| S2 `StartupTimeout` | NEEDED | Z/B/E timeout cases would take 2 minutes each otherwise. Default stays 2 min, pinned by a read-back test. |
| S3 `PrebuiltDll` | NEEDED, reason changed | Not only speed: xUnit runs classes in parallel, and concurrent `dotnet build` of the same QuietChild project collides on obj/ locks (a new flake). QuietChild tests skip the build; real-API tests (hazard a, O) keep the build, and the second one is an up-to-date no-op. |
| S4 QuietChild fixture project | NEEDED, kept tiny | No portable in-test process exists: python is not guaranteed on Windows dev, shell scripts differ on ubuntu, and StartAsync launches `dotnet <dll>`, so a child needs a dll. Cost: Program.cs (about 35 lines), QuietChild.csproj (net8.0, Exe), one ProjectReference line (ReferenceOutputAssembly=false, built with the test project). Commands only: `--print LINE` (stdout), `--eprint LINE` (stderr), `--exit N`, `--wait-file PATH` (print only after the file exists, the gate); default is stay alive and silent. No listening logic in it (it must not re-implement Kestrel). |

ZOMBIES cases that move off S4 or S1:
- B port parsing (1 and 65535 accepted; 0, 65536, empty, non-numeric rejected; [::1], localhost, https rejected) moves to pure unit tests of an internal `TryParseListeningAddress(string line, out Uri address)`: no process, no S4.
- E "BeforeChildStart throws, no child left": needs only S1 plus S3 (pointing at the QuietChild dll, never started).
- "StartupTimeout 2 min and stop wait 15 s unchanged": one test reading `internal const` defaults, no seam.
- Hazard a stays on a real generated API (Kestrel really failing to bind is the F-056 evidence). Hazard b needs no real API: a silent QuietChild plus a TcpListener the test owns.

## 2. Criterion-by-layer table (outermost first)
Layers: L1 callers (the 5 test classes, `StartAsync(path)` unchanged); L2 `StartAsync` orchestration (options, URL env, hook, start, dispose); L3 readiness (`WaitUntilListeningAsync` awaiting a TaskCompletionSource, timeout, exit detection); L4 pump and pure parser (`TryParseListeningAddress`); L5 process boundary (real Kestrel or QuietChild); L6 repo source scan.

| Done check | Layers needed, outermost first | Red-able today? | Doubles consumed |
|---|---|---|---|
| 1a port taken before bind | L2 -> L5 (real API) | red-able: old harness throws "address already in use" | S1 hook plus test-owned TcpListener on the chosen port; real API, no double |
| 1b foreign listener reported as up | L2 -> L3 -> L5 (QuietChild silent) | red-able: old returns an api on the foreign port, accepted count above 0 | S1, S2 (1 s), S3, S4 silent child, test-owned TcpListener (3 holder states, one case each) |
| 1 Z/O guard (hook null by default, fires once, no listener = one start succeeds) | L2 | born-green on old and new | S1; guarded by asserting hook count equals 1, so it cannot pass by the hook not firing |
| 1 M distinct BaseUrl under WhenAll | L2 (two parallel) | born-green on old (ports almost always differ); real value after fix | S1, S3, S4 |
| 1 E (child exit 3, hook throws) | L2 -> L3 -> L5 | born-green (exit check and PID-dead already exist) | S1, S3, S4 `--exit 3` |
| 2 own the port: ASPNETCORE_URLS exactly `http://127.0.0.1:0`, BaseUrl equals child's own line, GET answered | L2 -> L3 -> L4 -> L5 (real API) | red-able (old URL has a nonzero port) | S1 (read env); real API |
| 2 Z silent child, 1 s, TimeoutException names PID, URL `...:0`, log | L2 -> L3 -> L5 | red-able only by the URL text, so assert that text | S2, S3, S4 default |
| 2 M earlier unrelated line then reported line; second listener never contacted | L2 -> L3 -> L4 -> L5 | red-able (old never reads output) | S3, S4 `--print` twice, test-owned TcpListener |
| 2 B parser boundaries | L4 only | red-able (stub returns false from commit 1) | none, plain strings |
| 2 B line on stderr; line after 5000 earlier log lines | L3 -> L4 -> L5 | red-able | S3, S4 `--eprint`, repeat flag or args file |
| 2 B wrong host or https fails naming PID, line, log | L2 -> L3 -> L4 | red-able | S3, S4 |
| 2 E exits after reporting; reports an address nothing listens on | L2 -> L3 -> L5 | exit case born-green (old exit check); not-listening case red-able via URL text | S2, S3, S4, closed TcpListener |
| 2 E squatter on the reported address: out of scope, test name says so, asserts the Pid that printed the line is the one held | L2 | born-green by design (documents the limit) | S3, S4 |
| 2 I gate: 0 connections before the gate, no return before it | L2 -> L3 -> L5 | red-able (old polls the port at once) | S3, S4 `--wait-file`, counting TcpListener |
| 2 I defaults 2 min and 15 s, message form | L2/L3 | born-green | pin test on `internal const`; guard is a named mutation in the test comment |
| 2 S one process start on bind failure (no retry) | L2 | born-green (old has no retry) | S1 counter, S4 `--exit 1`; guard: a retry loop would turn it red |
| 3 scan Z: no `FreeLoopbackPort` in .cs/.py/.yml | L6 | red-able | RepoPaths.Find, real files, no network |
| 3 O: `ASPNETCORE_URLS` set in exactly one place, literal value | L6 | red-able (old value is interpolated) | same |
| 3 M/B patterns (`new TcpListener(` outside allowlist, Start-then-Stop to read a port, int helpers named Port/Free/Unused/Available, Process.Start checked set) | L6 | red-able while FreeLoopbackPort exists (it holds `new TcpListener(`); the Process.Start checked-set part is born-green | same; allowlist is only the card's own test files |
| 3 E closure evidence (git log -S, gh run head sha) | not a test: delivery lead commands, CI | not applicable | git, gh; head sha must equal the last commit |

Overlaps: check 2 re-runs the two check 1 tests unchanged (no new tests; they are the green proof). Born-green items stay as named guard tests inside their check and are labelled as characterisation in the commit message so the reviewer can mutate them.

Self-reference trap for check 3: the scan test and the red tests contain the strings `FreeLoopbackPort` and `new TcpListener(`. The scan must build its needles by concatenation and name its own file as excluded, and the allowlist must name the card's test files by path, or the scan fails forever on itself.

## 3. Commit plan (each within D136)
1. Seams (green, no behaviour change): `GeneratedApiStartOptions` plus S1/S2/S3 in GeneratedApi.cs (still using FreeLoopbackPort), stub `TryParseListeningAddress` returning false, QuietChild Program.cs and csproj, ProjectReference. About 5 files, 120 lines. Guard tests: hook null by default; QuietChild dll exists.
2. Red check 1 (a, b, B holder states, E, I): 1 test file, about 200 lines. Only failing tests, Trait("Slice","214").
3. Red check 2 part 1 (Z, O, M, I, defaults): 1 test file, about 220 lines.
4. Red check 2 part 2 (B, E, S, parser unit tests) plus the check 3 scan: 2 files, about 250 lines. All reds are in before the fix.
5. Green: GeneratedApi.cs only. URL literal `http://127.0.0.1:0`; the pump completes a `TaskCompletionSource<Uri>` (RunContinuationsAsynchronously, TrySetResult) on the first matching line from stdout or stderr; real `TryParseListeningAddress`; `WaitUntilListeningAsync` awaits it with the unchanged timeout and exit checks; remove `FreeLoopbackPort` and `PortIsOpenAsync`. About 90 changed lines. No retry on bind failure.
6. Board note and closure evidence, no code.

## 4. Linux CI (ubuntu runner)
- Line format: the console logger prints two lines: `info: Microsoft.Hosting.Lifetime[14]`, then an indented `      Now listening on: http://127.0.0.1:PORT`. The address is never on the first line, so parse per line with an unanchored `Now listening on:\s*(\S+)`, then `Uri.TryCreate`. Same text on Windows and Linux. With port 0 Kestrel reports the resolved port. Take the first matching line only. Reject a scheme other than http, a host other than 127.0.0.1, a port outside 1 to 65535.
- Buffering: the child's stdout is redirected, but Console.Out autoflushes and the console logger writes each message from its own queue thread, so there is no block-buffering stall. The harness reads with `ReadLineAsync`, which handles `\n` and `\r\n`. The one real risk is the generated app silencing `Microsoft.Hosting.Lifetime`: harness-only mitigation is env `Logging__LogLevel__Microsoft.Hosting.Lifetime=Information` in Prepare (no product change). The backend seat confirms the generated Program and appsettings do not raise that level (grep of this repo found no UseUrls or logging config).
- ASPNETCORE_URLS port 0: the kernel assigns the port at bind and Kestrel keeps the socket, so there is no choose-then-release window on Linux. Prepare should also remove `ASPNETCORE_HTTP_PORTS`, `ASPNETCORE_HTTPS_PORTS` and `DOTNET_URLS` (a runner or image could set them; .NET 8 warns or overrides when both are present), next to the existing removal of `ASPNETCORE_HTTPS_PORT`.
- Paths and kill: the dll name is case-sensitive on ubuntu (`QuietChild.dll`), net8.0 like the generated APIs, started through the same Prepare with `DOTNET_ROLL_FORWARD=Major`. Dispose still kills only the recorded PID of `dotnet <dll>` (no apphost, no wrapper), one PID per child on both systems.
- Check in commit 1: the ProjectReference must put QuietChild.dll and QuietChild.runtimeconfig.json next to the test assembly on a clean ubuntu build. If not, the fallback is RepoPaths.Find to `tests/Fixtures/QuietChild/bin/Debug/net8.0/QuietChild.dll`. The fixture-exists guard test catches this before any red test depends on it.
- Domain expert: no change. D156 C1 stands (a hosting or CI expert joins only by Decider). The Linux points are documented behaviour covered by the backend and reviewer seats; the first ubuntu CI run after commit 5 is the evidence, and a mismatch there is the trigger for a Decider.

Slice field: SHIP AS IS | about 11 files over 6 commits, none above 250 lines or 5 code files | 6 increments | one concern: the harness owns its port and reads the bound address from the child

- Seat: architect (slice), context ctx-6, Claude Sonnet subagent; reason for a Claude seat: probes at 08:21 UTC (commit a5be583) show Codex, Copilot, Grok and agy without usable quota.
