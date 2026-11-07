# Card 196 proposal (product manager seat, ready; read-only)

The seat's reply (a fresh context, read-only on the repository), committed with the facilitator's rerun note at the end. Facts were run at HEAD deb4334; counts that needed a build ran in a `git archive HEAD` copy, so nothing in the repository was written.

## Premise facts
- **Projects:** `ls -d tests/Phrase.* tests/Avowry.*` gives exactly `tests/Phrase.Acceptance.Tests` and `tests/Phrase.Compiler.Tests`, and no `Avowry.*`. 100 tracked files in Acceptance, 27 in Compiler; no tracked bin/obj.
- **References outside the two projects:** `git grep -l -E "Phrase\.(Compiler|Acceptance)\.Tests" -- . ':!board' ':!.mob'` outside the two projects: 20 files (about 49 lines).
  - Build and config: `Phrase.sln` (2 hits), `.gitattributes:1` (`tests/Phrase.Acceptance.Tests/Fixtures/corpus/*.avowry -text`), `tools/Phrase.Bench/Program.cs:448,473`.
  - Scripts: `scripts/check-readme-reference-status.py`, `check-run-pairs-217.py`, `ci-log-outcomes-217.py`, `repeat-run-264.py`, `usable-v1-report.py:147`.
  - Python tests: `tests/test_bench_launcher_scan_264.py`, `test_outcome_extractors_217.py`, `test_repeat_evidence.py`, `test_run_pairs_217.py`.
  - Docs: `README.md` (7 lines), `HANDOFF.md`, live ADRs `docs/adr/ADR-extension-map.md` and `ADR-library-binding-layering.md`. `docs/decisions/*` and `docs/retros/seeds.md` are history and stay.
- **Inside the two projects:** 96 files, 203 lines: 69 Acceptance files declare `namespace Phrase.Acceptance.Tests` (55 `using ...Tests.Support` lines), string paths such as `RepoPaths.Find("tests","Phrase.Acceptance.Tests",...)` and `tests/Phrase.Acceptance.Tests/bin/<cfg>/net8.0` (`BenchLauncherContract.cs:196`); 26 Compiler files declare the namespace and the csproj links `../Phrase.Acceptance.Tests/Fixtures/**/*` (line 18). No `AssemblyName` or `RootNamespace` override: the csproj file name sets the assembly name.
- **CI:** `ci.yml` names only `Phrase.sln`, never a test path; all four `runs-on` lines in `.github/workflows/*.yml` are `ubuntu-latest`, no matrix. The old criterion "CI green on Windows and Linux" is a false premise: CI is Linux only, and the Windows evidence is the facilitator's local gate. No workflow runs the python tests.
- **Old criterion 3 premise:** `tests/test_demo_readme.py` has 0 hits for the old names; it is not path-bound. The path-bound python modules are the four above plus `test_readme_reference_status` through its script.
- **Namespace decision:** the card renames namespaces too, with one mechanical anchored rewrite `s/Phrase.Compiler.Tests/Avowry.Compiler.Tests/` and `s/Phrase.Acceptance.Tests/Avowry.Acceptance.Tests/` (the 5 `Phrase.GetProperty` hits and the `Phrase.Bench*` hits stay). Reasons: card 195 did the same, and the namespace is the test's `FullyQualifiedName`, which the 217 scripts match by prefix. Estimate about 252 lines across about 127 renamed files; the commit carries `Slice-exception: rewrite s/Phrase.Compiler.Tests/Avowry.Compiler.Tests/ (and Acceptance)`.
- **Counts before:** `dotnet test <proj> --list-tests`: Compiler 284 listed (284 distinct), Acceptance 317 listed (304 distinct display names; 13 theory-row duplicates), so compare the sorted list name for name, not the distinct count. The `Status!=Pending` run total was not measured; the red seat records it.
- **Python baseline** (scratch copy, all OK): `test_bench_launcher_scan_264` 6, `test_outcome_extractors_217` 17, `test_repeat_evidence` 21, `test_run_pairs_217` 36, `test_readme_reference_status` 4. `test_demo_readme` not baselined cleanly; the red seat records it in the real checkout.
- **Card 197 boundary:** 196 owns the two project directories, csproj files and namespaces, the two `Phrase.sln` entries, and every file that names those two projects (`.gitattributes`, the scripts, the python tests, `tools/Phrase.Bench/Program.cs`, README and the live ADRs). 197 owns the file name `Phrase.sln`, the CI job and command text, `Directory.Build.props`, script file names and the whole-repository `Phrase` inventory. **Gap:** `tools/Phrase.Bench` and `Phrase.BenchCs` (53 lines under tests/) are named by no card (card 195 said they "keep their names for cards 196 and 197"); they need an owner at 197's Ready.

## Card text (replaces the issue body)
```card
<!-- phrase-card:196 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** tests (S; Avowry rename 7 of 9)
- **Risk to shipping:** none (a naming card). A silent loss of tests or of a CRLF guard is the only harm, and each is a scenario below.
- **Source:** the product owner's naming decision (public name Avowry) and Decider D125; premise facts rerun at HEAD deb4334 (20 outside files, 96 inside files, 284 + 317 tests listed).
- **Depends on:** 195 (Done). Card 197 depends on this one.
- **Story:** As Morgan, I want the two test projects, their namespaces and every path that names them to say Avowry, so that I can find a test by the product's name and a search for the old project names finds nothing in tracked files under tests/, with no test lost.
- **Slice:** one concern, one atomic rename commit, `Slice-exception: rewrite s/Phrase.Compiler.Tests/Avowry.Compiler.Tests/ (and Acceptance)`.

## Scenarios (letters in the title)

Scenario: [Z] no old name remains
  Given the tracked files at HEAD
  When tracked paths under tests/ and tracked live files (not board/, .mob/, docs/decisions, docs/retros) are searched for Phrase.Compiler.Tests and Phrase.Acceptance.Tests
  Then zero paths and zero lines match, and tests/Avowry.Compiler.Tests and tests/Avowry.Acceptance.Tests are tracked

Scenario: [O][M] every test still runs, none added or removed
  Given the before-lists of 284 (Compiler) and 317 (Acceptance) listed tests
  When each project is listed under its new name
  Then each list equals the before-list after the prefix rewrite, name for name, and the gate run total is unchanged

Scenario: [B] the strings only a reader of the code would find
  Given the probe that decides whether the acceptance assembly is loaded, and the corpus fixtures marked -text
  When the rename is done
  Then the probe names Avowry.Acceptance, `git check-attr text` on a corpus fixture reports unset, and `git diff -M` shows the corpus files as 100% renames

Scenario: [I] what names the projects still works
  Given the scripts, python tests, solution and csproj fixture link that name the two projects
  When the four path-bound python modules and `dotnet test Phrase.sln` run
  Then they pass with the before counts (6, 17, 21, 36) and the 217 extractor returns the Avowry.Acceptance.Tests outcomes, not an empty list

Scenario: [E] partial work is refused
  Given a copy of the finished tree with one old name put back (one script, one namespace, or one gitattributes line)
  When the Slice=196 check runs
  Then it fails and names the file and line

Scenario: [S] the plain route
  Given the finished commit pushed
  When `dotnet test Phrase.sln --filter "Status!=Pending"` runs in a clean copy and CI runs
  Then it is green on Windows locally and in CI on Linux, with the same totals as before

## Done checks (3, one per cycle)
1. [Z][E] the Slice=196 check (python, tests/test_rename_196.py) passes and fails for each put-back.
2. [O][M][B] lists equal, probe and gitattributes proofs hold.
3. [I][S] path-bound modules, the gate in a clean copy, CI Linux green.

## Not in this card
Phrase.sln file name, CI job text and Directory.Build.props (197); tools/Phrase.Bench and Phrase.BenchCs (no owner yet, raised at 197); board/ archive, .mob/records and docs/decisions history; the repository name (198); a Windows CI job.

## Done when
python -m unittest tests.test_rename_196
dotnet test Phrase.sln --filter "Status!=Pending"
python -m unittest tests.test_bench_launcher_scan_264 tests.test_outcome_extractors_217 tests.test_repeat_evidence tests.test_run_pairs_217 tests.test_readme_reference_status
```

Letter notes the reviewer should agree with: [O] and [M] share one scenario because the single-project and whole-suite cases are one comparison. Morgan's value is real but small, since it rests on the product owner's naming decision.

## Task force proposal
- **Seats:** ready (this seat); slice by the architect, whose job is to confirm SHIP AS IS; red by QA; green by a build seat; ship gate by a non-author; fresh sign-off.
- **Author:** the rotation is Codex, Copilot, Grok, agy, with Claude a normal member. The last two greens (299, 303) were Codex, so next is Copilot (Rung 1, 0/3 clean: the facilitator re-runs every claim, which suits a bounded mechanical rename), subject to a fresh probe. `DRIVERS.md` is stale (last updated 2026-10-30), so the facilitator checks the current probes. If Copilot fails the probe: Grok, then agy, then Claude, reason logged.
- **Red:** QA writes `tests/test_rename_196.py` (python, so it is not collected by the dotnet gate or CI) plus the before-lists committed under `.mob/records/196/`. The checks point at the new paths, so all three criteria are red today. The put-back mutation checks (scenario [E]) run in a scratch copy.
- **Cycles:** cap of 3, one criterion each, in the order of the done checks. The rename is atomic, so criteria 2 and 3 go green with criterion 1's commit; each must be shown red once by a named put-back or file-drop (in place of the D193 mutation pair). A fourth cycle needs a D175 window.
- **Green:** one commit using `git mv` plus the anchored rewrite, staging explicit paths; it deletes nothing recursively and leaves the ignored old `bin/obj` directories alone.
- **Ship gate:** a non-author re-runs the gate in a clean `git archive` copy, not the author's checkout, and lists the Criterion order from git log.
- **Sign-off:** a fresh Claude that did not write the green.
- **Repository and D193:** phrase-lang, not mob-crosscli, so the D193 trial does not apply; the gate is kept for the clean-copy run, because the local old `bin/obj` can mask a missed path.
- **Left out:** UX, security, DevOps (`ci.yml` needs no edit), technical writer (README lines are mechanical), domain expert (the architect covers dotnet and gitattributes); agile coach observes.
- **Reversal:** `git revert` of the one rename commit; the red commit can stay. Nothing is published and there is no data.

## Risks
1. **Silent test loss.** Compare name-for-name lists, not totals (13 duplicate display names hide a drop).
2. **Vacuous pass.** `tests/Fixtures/StarvationProbe/Program.cs:31` matches `StartsWith("Phrase.Acceptance")` and `StarvationProbe.cs:121` asserts `"false"`; after the rename the old prefix never matches, so the test passes while proving nothing. QA must show it red once.
3. **CRLF guard.** `.gitattributes:1` is `-text` for corpus fixtures; if missed, line endings normalise and the fixtures change. Check: `check-attr` plus `diff -M` at 100%.
4. **Name-bound scripts.** `ci-log-outcomes-217.py:36` filters on the old prefix and would return an empty list without an error; the 217 and 264 scripts and tests hold old names as fixture data, so the green seat separates "update" from "frozen sample".
5. **Stale artifacts.** The old local `bin/obj` satisfies `BenchLauncherContract.cs:196`'s path. Only the clean-copy gate is trusted.
6. **CI.** No path filters; CI is Linux only, so the card claims no Windows CI.
7. **The solution file.** A project path or GUID error in `Phrase.sln` would make the gate run fewer projects: the gate shows both projects in the run.
8. **Doc links.** README tree and table, `HANDOFF.md`, the two ADRs (`test_readme_reference_status` covers one).
9. **Overreach.** An unanchored rewrite would touch `Phrase.Bench*` and `Phrase.GetProperty`; the diff reviewer checks the sed is anchored.

Not run by the seat: the real-checkout baseline for `test_demo_readme` and the `Status!=Pending` run total.

## Facilitator's rerun note
Re-run at the boundary: 20 outside files and 96 inside files match the old names (`git grep -l`); `StarvationProbe/Program.cs:31` has the `StartsWith("Phrase.Acceptance"` prefix; the four `runs-on` lines are ubuntu-latest.
