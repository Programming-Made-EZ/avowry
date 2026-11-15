# Card 430: the facilitator's rulings on the architect's slice (2026-11-15)

The slice: slice-architect.md (architect: claude opus = claude-opus-5-5, ctx-1, D266; measured in its own probe outside the repository; 13,702 bytes). The D263 and D265 corpus: corpus.txt, 160 rows in 8 families (elapsed, cts, waitasync, whenany, httpclient, process near misses, collection attribute, test-type shapes), each row citing the real file and line it came from, with 12 encoding rows for each of the five new spelling families; the old guard misses 57 rows and the reference fixes all 57 with no other outcome change; the 23 committed guard rows pass on both copies; 27 of 28 mutants killed by named rows (the 28th, the scan caps, shows only in timing, which D254 does not let a test assert); the real acceptance project scanned in 18.0 ms median. Every probe build ran after a lock check that printed `free`.

1. A CancellationTokenSource spelling counts only with "did not finish within" in the class, as card 417 ruled: ACCEPTED (without it 14 classes are named, including process tests of 2 to 8 minutes and a class that already has its own collection).
2. GeneratedApiBoundAddress (`Task.WhenAny` with a 1.5 s `Task.Delay`) and SourceMapDeployment (a 5 minute `WaitAsync` around `dotnet publish`) take the isolated collection with the card's five named classes, and the [S] ceiling check measures the cost: ACCEPTED (an exemption would need a minimum-duration rule, a new design). Scenario [E]'s list therefore holds seven classes.
3. Nested test classes keep card 417's judgement by the parent's collection on this card (the real tree has no nested test class); xUnit does not apply an outer class's collection to a nested class, so the gap goes to a new card (Avowry #443): ACCEPTED.
4. `[Fact(Timeout = ...)]`, a local elapsed alias (`var elapsed = sw.Elapsed`), `Assert.False(sw.Elapsed > limit)`, a bare `DateTime` difference, a client the type did not declare, and very long initializers or `InRange` arguments are NAMED LIMITS of this card, not added after D266 fixed the scope; they go to the same new card as ruling 3 (Avowry #443).
5. The scan caps (the only change no row can check) stay; their timing is reported in the slice and re-measured by the review.

## Process note (facilitator, 2026-11-15 17:12Z)

The red seat (Claude Sonnet, ctx-2) kept a lock watcher running after the facilitator ran its build and filters under the 16:51Z watchdog ruling. When the watcher woke, the seat built in wt-430 while the Cursor green seat (ctx-3) was editing the same worktree, so two drivers were in one tree. Nothing was committed by the red seat, the red had been reproduced in a clean clone of 9daa8f6, and the facilitator reran the green filters after Cursor finished, so no record is affected. Lesson for the retro: when the facilitator takes over a waiting seat's runs, stop that seat first.

## Review round 1 rulings (facilitator, 2026-11-15)

Review round 1 is `review.md`: FLAG, 2 BLOCKING, 8 MINOR, by the QA seat (claude opus = claude-opus-5-5, ctx-4). The facilitator replayed both BLOCKING findings in the source. GeneratedApiBoundAddressEdges starts QuietChild with 2, 3, 10 and 20 s startup limits through `Probes.QuietChild`. StarvationProbe asserts a 3,000 ms readiness window enforced inside a child process (`--window-ms`). Neither class carries a collection. Both are older tests that card 417's guard never saw.

R1-1 (BLOCKING 1 and 2): fixed in this card's fix round. GeneratedApiBoundAddressEdges and StarvationProbe get `Collection("Isolated timing")`, and the [E] fact's list grows from seven classes to nine. The guard does not learn to see a limit enforced in a helper or a child process. That is a named limit (no rows), added to #443 with both examples, because recognising it needs call-graph or cross-file reading that the text guard does not do. The fix slice confirms that neither class carries another collection.

R1-2 (MINOR 6, new super-linear time in `\s*\??\s+` of HttpTimeoutInit, HttpDeclared and CtsTargetTyped): fixed in this card's fix round, because the time is new with this card and the fix is a pattern change. Any timing check stays in the probe, not in a test (D254: no wall-clock assertion). The fix slice measures the old and new pattern at 10k, 20k, 40k and 80k and commits the D265 neighbour rows (each spelling the changed patterns match, with a long whitespace or blanked-comment run before the name) before the red.

R1-3 (MINOR 4, the WaitAsync rule names token waits: `_ct`, `cancel`, `linkedCt`, `cancellationToken: ct`, `Timeout.InfiniteTimeSpan`, `Timeout.Infinite`): fixed in this card's fix round, because it is new with this card and `_ct` and the named argument are common in real code. The fix slice decides the token test (a named `cancellationToken:` argument, an identifier ending in `ct`, `Ct`, `token`, `Token` or `cancel` with or without a leading underscore, and the two infinite forms are not limits) and commits the neighbour rows before the red.

R1-4 (MINOR 3, 5, 7, 8, 9 and 10): named limits of this card. They are contrived or absent from this repository's tests, and a false name fails the gate loudly. They are filed on #443 with the review's minimal inputs. No rows in this card.

The fix round runs the same seats as D266: the architect's fix slice (claude opus), the red (claude sonnet), the green (cursor gpt-5.3-codex), the gate under the heavy lock, then review round 2 (claude opus). Lesson from the red, applied now: when the facilitator takes over a waiting seat's runs, the seat is stopped first.

## Review round 2 rulings (facilitator, 2026-11-15)

Review round 2 is `review-2.md`, run on card-430 at a5ca65f by the QA seat (claude opus = claude-opus-5-5, ctx-4b). Verdict: SIGN-OFF, 0 BLOCKING, 5 MINOR. It confirmed:
- no old row changed;
- the live tree names no class;
- the time is linear on 40 shapes up to 320k characters;
- `Slice=430|Slice=417` passes 383 of 383 in a clone.

R2-1, all five MINOR findings: named limits of this card, added to #443 with the review's minimal inputs.
- MINOR 1: whitespace or a comment between `WaitAsync(` and a token argument still names the class, because `\(\s*` backtracks into the lookahead. The reviewer's fix is `\.\s*WaitAsync\s*\((?!\s*(?:[)]|...))`. It is a false name, so the gate fails loudly; it never leaks. All 12 WaitAsync calls in the repository's tests have their argument touching `(`, which the facilitator replayed with grep. It is marked first on #443.
- MINOR 2: a reversed named-argument order is missed.
- MINOR 3: the `[Cc]ancel` suffix also matches a limit name that ends in Cancel.
- MINOR 4: older token-rule shapes.
- MINOR 5: `#if` and a using alias in a declaration gap.

None occurs in this repository, and none is a leak on real code. No further fix round runs.

Rule breach (recorded): the review seat's first probe build ran a recursive delete of `bin obj` inside its own new probe folder. Those folders did not exist yet, so nothing was deleted, and the seat reported it itself. Future review briefs will forbid any delete in the probe folder by name, as well as by rule.
