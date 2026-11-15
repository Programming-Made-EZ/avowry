# Card 430 slice: the wall-clock guard recognises the missed time-limit spellings

Architect (claude opus = claude-opus-5-5, D266, ctx-1), 2026-11-15, main read-only. Probe: `<scratchpad>/probe430` (`GuardScan.cs` = the REFERENCE with `M("...")` switches; `GuardScanV0.cs` = the guard's scan code copied unchanged from WallClockTestsRunIsolated.cs lines 12-41, 344-383, 399-777; `Committed.cs` = the 417 rows; `gen430.py` writes the corpus). Corpus: `<scratchpad>/corpus430.txt` (160 rows, 38 KB; columns id, family, encoding, expected, from, text with `\\ \n \t` escapes and `|||` between files).

## 1. Design changes (all in WallClockTestsRunIsolated.cs; the scan structure, blanking, partial merge, nested judgement and reporting are unchanged)

Elapsed member `MEM` = `Elapsed`, `ElapsedMilliseconds`, `ElapsedTicks`, `TotalSeconds`, `TotalMilliseconds`, `TotalMinutes` (V0: Elapsed, TotalSeconds, TotalMilliseconds; Ela + psed stays split). All rules run on the Code text (comments, strings, chars blanked; interpolation holes are code), as in 417.

| Rule | Reference (regex on Code unless said) | Card |
|---|---|---|
| R1 forward compare | `\bMEM\b(\s*\.\s*MEM\b)*\s*(<=` or `<(?![<=])(?!\s*[A-Za-z_][\w.]*\s*>))`: `<` may now touch a letter, but not open a generic `<T>` | [O] [M] no space, variable limit |
| R2 reversed | `(>=` or `(?<!=)>)\s*([\w.]+\s*\.\s*)?MEM\b` (not a lambda `=>`) | [M] `limit > sw.Elapsed` |
| R3 InRange | `\bInRange\s*(<[^<>;]{0,80}>\s*)?\(\s*[^,;]{0,200}?\bMEM\b` (first argument holds MEM) | [M] |
| R4 CTS | any of: `new CTS(TimeSpan.From(Seconds or Milliseconds or Minutes or Hours)(`; `new CTS(` + a non-TimeSpan argument; `CTS? name = new(` + an argument; `.CancelAfter(` + an argument; AND, as in 417, the Text body holds `did not finish within` (ruling R-1) | [M] |
| R5 WaitAsync | `\.\s*WaitAsync\s*\(\s*` + an argument that is not `)`, not a token (`([\w.]*\.)?(\w*[Tt]oken or ct)\s*[,)]`), not `CancellationToken.None` | [M] [E] |
| R6 WhenAny | `\bTask\s*\.\s*WhenAny\s*\(` AND `\bTask\s*\.\s*Delay\s*\(` in the same type | [M] |
| R7 HttpClient | `(new\s+HttpClient\b` or `\bHttpClient\s*\??\s+\w+\s*=\s*new\s*\()[^;]{0,400}?\bTimeout\s*=(?!=)` + not `(System.Threading.)Timeout.InfiniteTimeSpan`; or `name.Timeout =` (same infinite exclusion) where `name` is declared in the type by `HttpClient name = new HttpClient`, `var name = new HttpClient` or `HttpClient name = new(` (names in a set, one pass each) | [M] [E] |
| R8 test attribute | `(\[ or ,)\s*(Xunit\s*\.\s*)?(Skippable)?(Fact or Theory)(Attribute)?\s*(?=[\](,])` (V0: only `[Fact` / `[Theory` right after `[`) | [I] |
| R9 test type | ClassDecl `(?<![:,]\s*)\b(record\s+(class\s+)?` or `class\s+)(\w+)`; `record struct` is not a test type | [I] |

Linear time: the bounded scans (`{0,80}`, `{0,200}`, `{0,400}`) are load-bearing. With unbounded lazy scans the pass is quadratic (20,000 / 80,000-character body with no `,` or `;`: InRange 415 / 6,917 ms, HttpClient 404 / 2,804 ms; bounded 9 / 41 ms and 37 / 53 ms). The `name.Timeout` check must use a name set and one regex pass, not one regex per declaration.

## 2. Corpus (D263 five-plus families from real test code, D265 one row per encoding)

160 rows, 65 expected named, 95 expected not named. Each row cites its source (file:line, `card [M]`, `committed B row` or `encoding`).

| Family | Rows | Real sources (all under tests/Avowry.Acceptance.Tests) |
|---|---|---|
| elapsed (Stopwatch, Elapsed, InRange) | 34 | BenchTimeoutIsolation 46, PathScan439 134, CSharpMutationCheck 85, SourceMapDeployment 92, TasksDemoApp 54 |
| cts (CancellationTokenSource, CancelAfter) | 23 | BenchTimeoutIsolation 125/133, GettingStartedDocs 31, DotnetCli 45, GeneratedApi 285-286, HarnessProbes 24 |
| waitasync | 24 | TasksOverHttp 110, GeneratedApiHazards 172, GeneratedApi 123/157/179, PhraseCli 92, NoBlockingProcessWaits 76, GeneratedApiBoundAddress 146 |
| whenany (Task.WhenAny with Task.Delay) | 17 | GeneratedApiBoundAddress 95, GeneratedApiHazards 134, GeneratedApi 153/161 |
| httpclient (Timeout) | 24 | EndpointFailureMapsToHttpStatus 28, TasksOverHttp 53, HarnessProbes 150, GeneratedApiSeams 27, PhraseCli 17 |
| process (near misses: --timeout-ms, WaitForExitAsync, ProcessRunner, QuietChild, WaitAll, EnsureFresh) | 6 | committed B rows, ExtensionMap 31, GeneratedApiBoundAddress 17, Fixtures StarvationProbe 114, PhraseCliFreshness 29 |
| collection (the 5 O3 and 5 named B rows, one new spelling isolated) | 11 | committed rows, BenchmarkHarnessAcceptance 9 |
| testtype ([I]) | 21 | card [I], BenchRowsHoldNoFolderPath 32, SourceMapTraceability 170, Support/GeneratedApi shape |

Encodings (rows): code 69, line comment (incl. `///`) 12, block comment 5, regular string 7, verbatim 6, interpolated text 5, interpolation hole (regular and raw `$$`) 12, raw and raw interpolated text 10, char literal 5, attribute 10, attribute list 3, nested 4, partial (across files) 3, record 5, record class 1, SkippableFact 2, SkippableTheory 1. Every new spelling family (elapsed, cts, waitasync, whenany, httpclient) has the 12-row encoding set `X-<family>-{code, line-comment, block-comment, doc-comment, string, verbatim, interp-text, interp-hole, raw, raw-interp-text, raw-interp-hole, char}` (holes are named, the rest not).

Every [M] spelling is a row: `M-ms-var`, `M-total-var`, `M-inrange-ms`, `M-inrange-span`, `M-reversed`, `M-nospace`, `M-cts-minutes`, `M-cts-variable`, `M-cts-target`, `M-cancelafter`, `W-real-span` (WaitAsync TimeSpan), `W-real-var`, `A-real`, `M-whenany-var`, `H-real`, `H-real-var`; neighbours `M-inrange-generic`, `M-reversed-ge`, `M-nospace-span`, `M-le`, `M-ticks`, `M-total-minutes`, `M-cts-target-using`, `W-real-exit`, `W-span-and-token`, `H-real-only`, `M-http-handler`, `M-http-target`, `M-http-assign`. Every 417 near miss stays a row with its current outcome (`B-timeout-ms-argument`, `B-wait-for-exit-limit`, `B-wrong-case-collection`, `B-bench-processes`, `B-collection-attribute`, `B-constant`, `B-commented-out`, the comment and string forms in `X-*`, the five `O3-*`).

## 3. Measured outcomes

| Set | V0 | Reference |
|---|---|---|
| corpus, 160 rows | 103 as expected, 57 missed | 160 as expected |
| rows where V0 and reference differ | 57: each a V0 miss the reference fixes | |
| committed 417 rows (Z1, O1, O2, 5 O3, 9 B, I-strings, I-several, 2 partial, bin/obj, guard file) | 23 / 23 | 23 / 23 (unchanged) |
| L2 (live set holds BenchTimeoutIsolation) | yes | yes |
| L1 (live, named) | 0 | 7 (the [E] list below, until the green) |

No existing row changes its outcome. Family pass counts V0 / reference: elapsed 19/34, cts 15/23, waitasync 17/24, whenany 12/17, httpclient 15/24, testtype 8/21, process 6/6, collection 11/11.

## 4. The real test project (the [E] input)

The guard reads only tests/Avowry.Acceptance.Tests (105 files); the probe also ran all of tests/ (138 files): same result, nothing outside the acceptance project is named.

| Class (file = class name .cs) | V0 | Reference | Rule | Limit | Green |
|---|---|---|---|---|---|
| BenchTimeoutIsolation | isolated | isolated | R1, CTS | 20 s | already carries it |
| TasksOverHttp | - | named | R5, R7 | WaitAsync 15 s; HttpClient 30 s and 15 s | add `[Collection("Isolated timing")]` |
| GeneratedApiHazards | - | named | R5 | WaitAsync 10 s | add |
| EndpointFailureMapsToHttpStatus | - | named | R7 | HttpClient 30 s | add |
| GetEndpointsCompile | - | named | R7 | HttpClient 30 s | add |
| PutAndDeleteEndpointsCompile | - | named | R7 | HttpClient 30 s | add |
| GeneratedApiBoundAddress | - | named | R6 | WhenAny with Delay 1.5 s (an inverted check: it asserts start did NOT finish); its QuietChild startup limits (5 to 30 s) are not seen by the guard | add (ruling R-2) |
| SourceMapDeployment | - | named | R5 | `WaitAsync(PublishTimeout)`, 5 minutes around dotnet publish | add (ruling R-2) |

None of the seven carries another collection today, so the attribute can be added without a conflict. No class must NOT get it under the reference. Under the alternative of ruling R-1 (no message needed for a CTS) 7 more are named: GettingStartedDocs, LlmCustomerCheck (already in its own `Collection("LlmCustomerCheck")`, so it could not join), MatchingCodebases, PrePublicationChecks, PrerequisitesPage, SourceMapTraceCommand, TasksDemoReadme (2 to 8 minute process limits).

## 5. Mutants (28; 27 killed by named rows, 1 timing-only)

| Mutant (switch) | Killed by |
|---|---|
| record-off | I-record, I-record-positional, I-partial-record |
| record-class-off | I-record-class |
| attrlist-off | I-attr-list-trait-first, I-fact-attribute |
| skippable-off | I-skippable-fact, I-skippable-theory, I-nested-under-plain, I-partial-plain |
| elapsed-ms-off | O1, M-ms-var, M-inrange-ms, M-le, M-ticks, X-elapsed-code (+4) |
| minutes-member-off | M-total-minutes |
| lt-letter-off (V0 lookahead) | M-nospace, M-nospace-span |
| lt-generic-off | N-e-generic (contrived, D259 clause 4 MINOR shape) |
| reversed-off | M-reversed, M-reversed-ge |
| reversed-lambda | N-e-lambda |
| inrange-off | M-inrange-ms, M-inrange-span, M-inrange-generic |
| inrange-any | N-e-inrange-count (also changes the live tree) |
| cts-minutes-off | M-cts-minutes |
| cts-value-off | M-cts-variable, X-cts-interp-hole, X-cts-raw-interp-hole |
| cts-target-off | M-cts-target, M-cts-target-using, X-cts-code |
| cancelafter-off | M-cancelafter |
| cts-message-off | N-c-no-message, N-c-seconds-no-message (live tree: 14 named) |
| waitasync-off | W-real-var, W-real-span, W-real-exit, W-span-and-token, X-waitasync-code (+14) |
| waitasync-token | N-w-token, N-w-cancellation-token, N-w-ct, N-w-none, N-w-context-token |
| whenany-off | A-real, M-whenany-var, X-whenany-code, X-whenany-interp-hole, X-whenany-raw-interp-hole |
| whenany-optional (Delay alone) | N-a-delay-alone, N-a-poll |
| whenany-delay-optional (WhenAny alone) | N-a-whenany-alone |
| http-off | H-real, H-real-var, H-real-only, M-http-handler, M-http-target (+4) |
| http-infinite | N-h-infinite, N-h-infinite-assign |
| http-any-timeout (`Timeout =` anywhere) | N-h-field |
| http-target-off | M-http-target |
| http-assign-off | M-http-assign |
| unbounded (lazy scans unbounded) | no row: only time shows it (section 1). D254 forbids a wall-clock assertion, so the red cannot hold it; the green keeps the bounds and QA checks them by reading |

## 6. Timing (Release probe, 50 passes, median / max)

tests/Avowry.Acceptance.Tests (105 files, 915,641 chars): V0 15.2 / 30.3 ms, reference 18.0 / 33.8 ms. All of tests/ (138 files, 1,077,763 chars): V0 9.9 / 13.5 ms, reference 22.5 / 31.3 ms. Pathological 80,000-character bodies (no `,`/`;`, open `InRange<`, many `new HttpClient`, `Elapsed<a.b`, `a > b.`, many `WaitAsync(` or `new CTS(`): reference at most 53 ms each, growing linearly from 20,000 to 80,000.

## 7. Rows the red must embed

All 160 corpus rows, by id, as one Pending theory per family with the expected named classes from column 4 (copy `corpus430.txt` to `.mob/records/430/` and generate the rows from it, or inline them; write backslash-holding rows with a file-writing tool). The minimum, if the facilitator cuts: Z1, O1; every `M-*`, `W-*`, `A-*`, `H-*`, `C-real`, `E-real` row; every `N-*` row; every `I-*` row; the 60 `X-*` encoding rows; the 11 collection rows and the 2 process B rows. Every mutant killer in section 5 is in that minimum. The committed 417 rows stay as they are (all pass on the reference). [E]: the red keeps L1 as the live check (it turns red with the 7 names once the green ports the reference, and green again when the attribute is added); [S] stays the sign-off's heavy run.

## 8. Rulings needed from the facilitator

- R-1 (CTS message). The reference keeps 417's rule that a CancellationTokenSource limit counts only with `did not finish within` in the type, for every new CTS spelling ([M] fixtures carry the message, as O2 does). Without it, 14 real classes are named instead of 7, including the 2 to 8 minute process tests and LlmCustomerCheck, which already has its own collection, and [S]'s serial tail grows. Recommend: keep the message rule.
- R-2 (two classes the card's [E] does not list). GeneratedApiBoundAddress (WhenAny with a 1.5 s Delay, an inverted check) and SourceMapDeployment (a 5 minute WaitAsync around dotnet publish) are named by the card's own [M] spellings. Recommend: give both the collection and let [S] measure the cost. The alternative is a minimum-duration rule (resolving `TimeSpan` constants by name), which is a new design and a new card.
- R-3 (nested types). xUnit does not apply an outer type's `[Collection]` to a nested test class, so 417's judgement (a nested class counts with its parent) can pass a nested limit class that runs in parallel. The card keeps 417's judgement (rows I-nested-*, including I-nested-attribute-on-child, which names the parent even though the child carries the attribute). The real tree has no nested test class (the nested `[Xunit.Fact]` types sit in raw strings). Recommend: keep it for this card and file a card to judge nested types on their own.
- R-4 (out of scope, likely QA finds). These are not recognised and have no row: xUnit's `[Fact(Timeout = ...)]`; a local alias (`var elapsed = sw.Elapsed; elapsed < limit`); `Assert.False(sw.Elapsed > limit)`; `DateTime.UtcNow - start < limit` without a Total member; `client.Timeout =` on a client the type did not declare; an HttpClient initializer longer than 400 characters; an InRange first argument longer than 200 characters. Recommend: `[Fact(Timeout = ...)]` joins this card (two rows: named, and `Timeout = 0` or absent), and the rest are named as limits in the card's close.
