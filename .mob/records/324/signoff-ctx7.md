# Card 324 sign-off (fresh Claude context 7, read-only; scratch copies under the facilitator scratchpad)

Verdict: SIGNED OFF. No promise of the card lacks evidence; nothing blocks.

## Production diff (rerun)
- `git diff dfbf469 HEAD -- src`: 19 hunks at -U0 (17 at default context): Resolver.cs 12, Parser.cs 7. Word-diff shows only phrase to avow (Phrase to Avow, "a phrase" to "an avow").
- HEAD blobs: Resolver.cs 42139 bytes, Parser.cs 39272 bytes, 0 CR, 0 non-ASCII. Only two paths under src/.

## Mutations (rerun, clone, named filters)
- PH010 back to "Phrase": LeadingMessagesSayAvow M01 failed; live scan named Resolver.cs:105.
- PH325/PH328 back to "Composite phrase": OtherTen failed M09 and M10 (2 of 11); live scan named Resolver.cs 105, 119, 351.
- PH203 changed to PH204, and PH203 anchored on another position: characterisation failed on M08-PH203 both times. Two other position edits did not compile (not counted).
- `_ = "a phrase";` appended at Parser.cs:364: scan failed and printed `Parser.cs:364 a phrase`.

## Tests (rerun)
- Compiler.Tests: 324 passed, 0 failed. Load: CPU 21%, 1 dotnet process, ~27 GB free.
- Acceptance filters CompilerMessagesNameTheAvow|CliKeepsDiagnosticShape: 7 passed.

## Full gate (rerun, clean `git archive` copy after building Avowry.Cli)
- Load: CPU 12%, 1 dotnet process, ~27 GB free, nothing else running.
- Compiler 324 passed, 0 failed. Acceptance 319 passed, 4 failed of 323, 9 m 33 s.
- Failures: the three known baseline failures (PrivateHoldoutResults x2, HoldoutTaskConversion; the two git-based ones probably because an archive copy has no .git: inference, untested) and BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms (20 s timing; passed alone in 9 s at load 0%: a flake). None touch diagnostics.
- The facilitator's own run on the working tree was 323 of 323 Acceptance passed.

## CI (record-only)
ci green on 5646c8a, 142e02d and 1031b8e. Series 4c764fc..5646c8a pushed together; no red pushed alone. mob-process: latest schedule run green; two issue-triggered failures belong to cards 266 and 197.

## Red-first (own judgment)
- Criterion 1: at f28cb05 LeadingMessagesSayAvow fails 8 of 8, characterisation passes.
- Criterion 3: at dcb4812 OtherTen fails 11 of 11, the CLI test fails, the live scan lists exactly the 11 sites (Resolver 119, 351, 389, 566, 729; Parser 291, 341, 353, 358, 699, 873).
- Scanner tests: record-only (dcb4812 ships the real scanner; stub-first failure declared in the commit subject and red-record-3.md).
- Criterion 2: honest characterisation (passes before, after, fails on drift). No test born green undeclared. Weakness: CliKeepsDiagnosticShape accepts either wording by design.

## Promises
[Z] CliKeepsDiagnosticShape("clean"); [O] LeadingMessagesSayAvow M01; [M] LeadingMessagesSayAvow (8) + OtherTen (11 rows); [B] Diagnostic_codes_positions_and_tails_are_unchanged (19 rows); [I] Build_prints_the_avow_wording_for_an_unmatched_step; [E] the planted-name test, the exceptions test and the live scan; [S] LeadingMessagesSayAvow M08. All pass.

## For the retro (none blocks)
- Scanner reads the working tree: `TrackedSourceFiles` misleads; a split literal "Phr"+"ase" evades it.
- ExtensionKeywords.cs:138 and the registry arm name "phrase" remain, pinned.
- Scanner tests were red only against a stub (record-only evidence).
- The 20 s timeout test flakes under load; the holdout tests fail in a git-less copy.
- The brief's "19 hunks" is true only at -U0.
