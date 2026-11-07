# Card 324: pre-ship gate review (independent Claude reviewer, read-only)

Independence: a Claude seat reviewing code written by Codex and Grok seats and a Claude architect/PM. Ran only read-only git and grep; no `dotnet`. Did not rerun tests, did not check the author of the cycle 3 green commit, did not recompute the seat-use ledger.

Verdict: SHIP. No blockers.

1. NOTE: production diff is exactly the intended edits: 19 hunks (Resolver.cs 12, Parser.cs 7), each changing only the unit word plus the article where the architect's table says so. Blobs 42139 and 39272 bytes, 0 CR, 0 non-ASCII. sources.txt and baseline.txt unchanged since the characterisation commit.
2. NOTE: tests pin the criteria. Criterion 1 and 3 tests assert new text literally or via ToNew, so a regression to "Phrase" fails; characterisation maps new back to old by design; the drift Fact covers spacing, article and case drift.
3. NOTE: scanner limits. Each literal is tested separately, so a split such as "Phr" + "ase" is missed; only .cs under src; comments ignored on purpose; format strings, interpolation text, constants and raw strings covered. `TrackedSourceFiles` reads the working tree, not git-tracked files, so the name is misleading (the architect ruled this on purpose).
4. NOTE: pins are right: ExtensionKeywords.cs has 6 "phrase" literals (phrase-steps at line 66, phrase at lines 88-91 and 149); a seventh, the same literal elsewhere, or a lower-count pin yields a violation; ExceptionsMatched == 6 on the real tree.
5. NOTE: red-first holds for new-state work (cycle 1: 8 failed; cycle 3: 11 failed Theory, CLI test failed, scan listed the 11 sites; scanner tests watched failing against a stub first per the record). Cycle 2 is a declared characterisation. Scanner tests 1 and 2 were never red against the real tree; acceptable for a guard tool.
6. NOTE: cycle 3 red is exactly 400 C# lines (31 + 240 + 129); the cap is inclusive; no readability cramming seen.
7. NOTE: CliKeepsDiagnosticShape accepts (phrase|avow) by design; CompilerMessagesNameTheAvow pins the avow wording and covers PH001.
8. NOTE: seat plan: the reviewer did not check the cycle 3 green commit's author (Codex wrote it, recorded in seat324-c3g.out).

Follow-up candidates, none in scope: `TrackedSourceFiles` rename or `git ls-files`; ExtensionKeywords.cs:138 ArgumentException message and the internal registry arm name "phrase" (pinned, not a user diagnostic).
