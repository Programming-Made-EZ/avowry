# Card 430 fix slice, review round 1 (R1-1, R1-2, R1-3)

Architect (claude opus = claude-opus-5-5, D266, ctx-1b, a fresh context), 2026-11-15. Read-only for main and wt-430 (card-430 at 065456f). Probe: `<scratchpad>` + `/probe430fix`, a copy of `/probe430`. `GuardScan.cs` is the NEW reference with `M("...")` switches. `GuardScanR0.cs` is the round 0 reference, unchanged except for its class name. `GuardScanV0.cs` is the 417 guard. `Fix.cs` holds the timing shapes and the gap check. `genfix430.py` writes the neighbour rows to `<scratchpad>` + `/corpusfix430.txt`. Outputs are in `*.out`. Lock: the heavy lock printed `busy` (play-tracker #1065, since 17:58Z). Under the facilitator's light-work ruling (owner watchdog, 16:51Z) I built and ran only the probe. I did not build or test the repository and ran no git.

## 1. Pattern changes (R1-2)

The fix changes one piece in three patterns: the gap between the type name and the declared name.

| Pattern | Round 0 gap | New gap |
|---|---|---|
| HttpTimeoutInit (target-typed branch) | `\bHttpClient\s*\??\s+\w+\s*=\s*new\s*\(` | `\bHttpClient(?:\s*\?\s*\|\s+)\w+\s*=\s*new\s*\(` |
| HttpDeclared (both branches) | `\b(?:HttpClient\s*\??\|var)\s+(\w+)...` and `\bHttpClient\s*\??\s+(\w+)...` | `\b(?:HttpClient(?:\s*\?\s*\|\s+)\|var\s+)(\w+)\s*=\s*new\s+HttpClient\b` and `\bHttpClient(?:\s*\?\s*\|\s+)(\w+)\s*=\s*new\s*\(` |
| CtsTargetTyped | `\bCTS\s*\??\s+\w+\s*=\s*new\s*\(\s*[^)\s]` | `\bCTS(?:\s*\?\s*\|\s+)\w+\s*=\s*new\s*\(\s*[^)\s]` |

(`\|` stands for `|` inside this table. CTS is `"Cancellation" + "TokenSource"`, as now.)

Why it is linear: each branch scans the run once. The `\s*\?` branch fails at the first non-`?` character, and `\s+` then has nothing left to split. In round 0, the two whitespace quantifiers split the run every possible way.

Language check (`gapcheck`): over all 87,381 strings of length 0 to 8 drawn from {space, newline, `?`, `a`}, every gap that round 0 accepts, the new gap also accepts (only_old = 0). The new gap accepts exactly 255 more strings, all of the shape `\s*\?`: a `?` touching the name, as in `HttpClient?http` or `HttpClient   ?http`. That is valid C# and a real limit. This is the only widening, and it is what lets a row kill the switch by outcome (section 5).

Switches: `fix-time-off` restores all three round 0 gaps. `fix-time-init-old`, `fix-time-declared-old` and `fix-time-cts-old` restore one each. `fix-off` restores the whole round 0 reference (`fix-time-off` plus `fix-waitasync-off`).

## 2. Timings (`patho2.out`; old = round 0, new = the fix; ms; one old pass, best of three new)

| Shape (the reviewer's first) | 10k | 20k | 40k | 80k |
|---|---|---|---|---|
| `HttpClient` + n spaces + `)` | 592 / 0.5 | 3,762 / 0.8 | 12,114 / 1.9 | 37,542 / 3.0 |
| `static HttpClient` + commented-out lines + `Client;` | 538 / 0.7 | 2,278 / 0.8 | 9,143 / 1.2 | 34,791 / 2.9 |
| `static HttpClient /*` x...x `*/ )` | 559 / 0.5 | 2,159 / 0.6 | 8,683 / 1.8 | 36,430 / 1.9 |
| `HttpClient` + n/2 spaces + `?` + n/2 spaces + `)` | 119 / 0.3 | 536 / 0.5 | 2,435 / 1.5 | 8,690 / 1.6 |
| CTS + n spaces + `)` | 155 / 0.1 | 642 / 0.4 | 3,100 / 0.5 | 11,765 / 1.2 |
| CTS + commented-out lines + `)` | 175 / 0.1 | 730 / 0.3 | 3,015 / 0.7 | 11,841 / 1.4 |
| `HttpClient?` + n spaces + `)` (already linear) | 0.5 / 0.4 | 0.9 / 0.8 | 0.9 / 0.8 | 1.5 / 1.4 |
| `HttpClient ` / `HttpClient? ` / CTS repeated | at most 6.7 / 5.8 at 80k | | | |

The new token rule stays linear: `.WaitAsync(` followed by a run of `a.`, one long identifier, `.WaitAsync(a.b.c ` repeated, `.WaitAsync(a.ct.ct ` repeated, `cancellationToken` + spaces, and `Timeout` + spaces each take at most 7.4 ms new and 6.7 ms old at 80k.

The reviewer's other shapes are unchanged, old against new: InRange repeated 52.8 / 50.1; the reversed chain, `Elapsed<a...`, the HttpClient assign and declare repeats, big strings and WhenAny repeats each at most 7.8 ms at 80k. The `.Elapsed` chain is still super-linear, at 10,317 / 10,325 ms at 80k. That is MINOR 7: older, and a named limit under R1-4. This slice does not change it.

Real tree, 50 passes, median / max (the play-tracker heavy run was live on the laptop, so this run is noisier than round 0's):

| Scope | V0 | Round 0 | New |
|---|---|---|---|
| tests/Avowry.Acceptance.Tests (106 files, 985,901 chars) | 21.3 / 33.2 ms | 19.5 / 34.0 ms | 24.4 / 41.5 ms |
| tests/ (139 files, 1,148,023 chars) | 14.6 / 18.2 ms | 28.8 / 37.2 ms | 23.9 / 34.7 ms |

The round 0 and new medians cross between the two scopes, so the difference is noise. The real tree has no long run after a type name.

## 3. The token rule (R1-3)

`WaitAsyncLimit` = `\.\s*WaitAsync\s*\(\s*(?!` + one of the alternatives below + `)`. The first argument is a limit unless it is:

| Not a limit | Pattern | New? |
|---|---|---|
| no argument | `[)]` | as before |
| a named `cancellationToken:` argument | `cancellationToken\s*:(?!:)` | new |
| an identifier ending in ct, Ct, token, Token, cancel or Cancel, with or without a leading `_` or a member path (`ct`, `_ct`, `linkedCt`, `this._ct`, `token`, `linked.Token`, `cancel`, `owner.Cancel`) | `[\w.]*(?:[Cc]t\|[Tt]oken\|[Cc]ancel)\s*[,)]` | widened (round 0: `(?:[\w.]*\.)?(?:\w*[Tt]oken\|ct)\s*[,)]`) |
| `Timeout.InfiniteTimeSpan`, `Timeout.Infinite`, with or without `System.Threading.` | `(?:System\s*\.\s*Threading\s*\.\s*)?Timeout\s*\.\s*Infinite(?:TimeSpan)?\s*[,)]` | new |
| `CancellationToken.None` | `CancellationToken\s*\.\s*None\b` | as before |

These stay limits: a TimeSpan expression (including `TimeSpan.FromSeconds(5), ct`), `limit`, `_timeout`, `ParallelTimeout`, `PublishTimeout`, `options.StartupTimeout`, `connectTimeout` (the `ct` inside "connect" is not at the end), `timeout: ...`, and `timeout: limit, cancellationToken: ct`. Only the first argument is read, as in round 0.

The switch `fix-waitasync-off` restores the round 0 rule. Section 5 lists five finer switches.

## 4. New rows (`corpusfix430.txt`, 196 rows, same columns as corpus.txt, ids FT-* and FW-*)

**fix-time (46 rows: 32 named, 14 not).** Most come in two versions, `-sp` (a 1,000-space run between the type and the name) and `-cm` (a blanked-comment run of about 1,100 characters: twelve `//` lines that hold an HttpClient Timeout spelling, then a block comment):
- HttpTimeoutInit: `FT-init-{space,q,q-after-run,static,readonly,private}-{sp,cm}`, where `q-after-run` is `HttpClient<run>?http`.
- HttpDeclared plus `name.Timeout =`: `FT-decl-{space,target,q,static}-{sp,cm}`.
- CtsTargetTyped plus the 417 message: `FT-cts-{space,q,new-space,field}-{sp,cm}`. `new-space` is `= new (ParallelTimeout)`, and `field` is a readonly field.
- Near misses: `FT-n-{http-paren,http-field,http-no-timeout,cts-paren,cts-no-arg,cts-no-message}-{sp,cm}`. `http-paren` and `cts-paren` are the reviewer's `<type><run>)` shapes. `http-field` is `static HttpClient<run>Client;`.
- Without a run, the switch killers: `FT-init-q-tight`, `FT-decl-q-tight`, `FT-decl-q-tight-target` and `FT-cts-q-tight` (a `?` touching the name, named). Two neighbours stay unnamed: `FT-n-init-q-tight-infinite` (InfiniteTimeSpan) and `FT-n-cts-q-tight-no-message`.

**fix-waitasync (150 rows: 24 named, 126 not).** The ids are `FW-token-<form>-<enc>` and `FW-limit-<form>-<enc>`. `<enc>` is one of code, comment (`//`), string, verbatim, interp-hole (`$"{...}"`) and raw-hole (`$$"""{{...}}"""`).
- The 17 token forms: named, ct, underscore-ct, linked-ct, underscore-linked-ct, member-ct, token, underscore-token, linked-token, member-token, cancel, underscore-cancel, member-cancel, infinite-span, infinite-span-qualified, infinite, none. Not named in any encoding.
- The 8 limit forms: limit, span-and-ct, underscore-timeout, named-timeout, named-timeout-and-token, member-timeout, connect-timeout, publish-timeout. Named in code and both holes, and not named in comment, string or verbatim.

Each row's expected outcome is the one measured on the new reference (196 of 196 as expected).

## 5. Measured outcomes and mutants

| Set | V0 | Round 0 | New |
|---|---|---|---|
| corpus.txt, 160 rows | 103 ok | 160 ok | 160 ok (0 change from round 0) |
| corpusfix430.txt, 196 rows | 140 ok | 157 ok | 196 ok |
| committed 417 rows | 23 / 23 | 23 / 23 | 23 / 23 |
| live tree: L2 (BenchTimeoutIsolation detected) / L1 (named) | yes / 0 | yes / 0 | yes / 0 |
| live tree plus the green's two attributes (overlay) | 0 named | 0 named | 0 named |

**No existing row changes its outcome.** The 39 round 0 misses are all new rows: 6 fix-time rows where a `?` touches the name, and 33 FW-token rows (11 forms × code and both holes) that round 0 named. On the real tree, round 0 and the new reference detect the same 8 classes (BenchTimeoutIsolation and the 7 [E] classes), all isolated. They name 0 classes in both scopes, before and after the overlay. Neither GeneratedApiBoundAddressEdges nor StarvationProbe is detected, so adding their attribute changes no guard outcome.

Mutants (`mutants.out`, run over the 160 + 196 + 23 rows): 41 in all, 40 killed by named rows, and 1 (`unbounded`) that only timing shows, as in round 0.

| Mutant | Killed by (a selection; the counts are the full kill sets) |
|---|---|
| The 27 round 0 kills (record-off ... http-assign-off) | the same killers as slice-architect.md section 5. Several also gained FT-* killers: cts-target-off 12, http-target-off 14, http-assign-off 11, http-off 22, cts-message-off 5, http-infinite 3, waitasync-token 56 |
| unbounded | none (timing only, D254), unchanged |
| fix-off | 39: the 6 fix-time and 33 fix-waitasync rows below |
| fix-time-off | FT-init-q-after-run-sp/cm, FT-init-q-tight, FT-decl-q-tight, FT-decl-q-tight-target, FT-cts-q-tight (6, by outcome) |
| fix-time-init-old | FT-init-q-after-run-sp, FT-init-q-after-run-cm, FT-init-q-tight |
| fix-time-declared-old | FT-decl-q-tight, FT-decl-q-tight-target |
| fix-time-cts-old | FT-cts-q-tight |
| fix-waitasync-off | 33: FW-token-{named, underscore-ct, linked-ct, underscore-linked-ct, member-ct, cancel, underscore-cancel, member-cancel, infinite-span, infinite-span-qualified, infinite}-{code, interp-hole, raw-hole} |
| fix-waitasync-named-off | FW-token-named-{code, interp-hole, raw-hole} |
| fix-waitasync-any-named (every `name:` is a token) | FW-limit-named-timeout-*, FW-limit-named-timeout-and-token-* (6) |
| fix-waitasync-any-ident (every identifier is a token) | 32, including W-real-var, X-waitasync-code, FW-limit-limit-code, FW-limit-underscore-timeout-code, FW-limit-publish-timeout-code |
| fix-waitasync-suffix-old | 21 (the underscore, Ct and cancel forms) |
| fix-waitasync-suffix-unanchored (no `\s*[,)]`) | FW-limit-connect-timeout-{code, interp-hole, raw-hole} |
| fix-waitasync-suffix-lower (only ct, token, cancel) | 18, including N-w-token, N-w-cancellation-token, FW-token-linked-ct-code, FW-token-member-token-code |
| fix-waitasync-infinite-off | FW-token-infinite*-{code, interp-hole, raw-hole} (9) |

## 6. R1-1: the [E] list grows to nine

I checked both classes in wt-430:
- GeneratedApiBoundAddressEdges (`tests/Avowry.Acceptance.Tests/` + `GeneratedApiBoundAddressEdges.cs`, line 7) carries no attribute. It uses the stateless static helpers `Probes` (Support/HarnessProbes.cs) and `GeneratedApi.StartAsync` with a prebuilt `QuietChild.dll`. Each `Holder` is a per-test instance.
- StarvationProbe (`StarvationProbe.cs`, line 24) carries no attribute. It holds only `const` and `static readonly` values, and it uses the stateless `ProcessRunner` and the read-only `StarvationProbe.dll`.

Neither shares mutable state with a class in another collection. The other collections are "Bench processes" (BenchmarkHarnessAcceptance, BenchRowsHoldNoFolderPath, NewBenchRowsCarryNoReply, RestoredPackageLicences, RestoredPackageLicencesLockedRestore) and "LlmCustomerCheck". Those classes share only `ProcessRunner`, which has no static field. Neither of the two classes uses PhraseCli's `BuildGate` or `FreshnessChecked`, the only static mutable state in Support.

The green adds `[Collection("Isolated timing")]` above both classes. `IsolatedRealClasses430` becomes:

TasksOverHttp, GeneratedApiHazards, EndpointFailureMapsToHttpStatus, GetEndpointsCompile, PutAndDeleteEndpointsCompile, GeneratedApiBoundAddress, SourceMapDeployment, **GeneratedApiBoundAddressEdges, StarvationProbe**.

The [E] fact's name and comment change from "seven" to "nine". Its existing regex matches both declarations (`public class <Name>`). The red turns [E] red by adding the two names before the green.

## 7. Named limit for #443 (no rows)

**Limit enforced in a helper or a child process.** The guard reads only the text of the test class. It does not see a limit that a test passes to a helper, which enforces it in another file or another process.
- Example 1: GeneratedApiBoundAddressEdges calls `Probes.QuietChild([...], TimeSpan.FromSeconds(2))` (also 3, 10 and 20 s). `GeneratedApi.WaitUntilListeningAsync` enforces the limit through `new CancellationTokenSource(_startupTimeout)`.
- Example 2: StarvationProbe passes `--window-ms 3000` to `tests/Fixtures/StarvationProbe`. The child enforces it with `allMet.Wait(options.WindowMs)`.

Recognising either needs call-graph or cross-file reading, which the text guard does not do. Until then, such classes are isolated by hand, as these two are.

## 8. Rulings needed

- **RF-1 (the gap widening).** The new gap names `HttpClient?x = new() { Timeout = ... }` and `CancellationTokenSource?x = new(...)` with no space after `?`. Round 0 did not name them. Both are valid C# and real limits, and they are the only outcome difference that the R1-2 change makes. They are also the only way a row can kill `fix-time-*` by outcome. The fully equivalent `(?:\s*\?\s+|\s+)` is also linear, but no row could kill it. Recommend: accept the widening.
- **RF-2 (the ct suffix reaches past tokens).** Any identifier ending in `ct` is read as a token, so a limit named, for example, `strict` or `exact` would be missed. There is no such argument in the repository (its arguments are `ParallelTimeout`, `PublishTimeout`, `timeout`, `timeout.Token`, TimeSpan expressions, `ExitSettle, timeout.Token` and `()`). The rule is the one R1-3 fixed. Recommend: a named limit on #443, no rows.
- **RF-3 (row size).** corpusfix430.txt is 94,880 bytes, because 46 rows carry 1,000-character runs. Recommend: the red embeds all 196 rows as it embedded corpus.txt (generated, with the completeness fact extended to both files), and holds no timing check (D254).
