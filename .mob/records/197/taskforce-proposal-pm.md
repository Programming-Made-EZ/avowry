# Card 197 proposal (product manager seat, ready; read-only)

The seat's reply (a fresh context, read-only on the repository), committed with the facilitator's rerun and correction note at the end. Facts were run at HEAD 2f0e48a. "Live" excludes `board/`, `.mob/`, `docs/decisions`, `docs/retros`. The seat reported creating one scratch file in the repository root by mistake and removing it; `git status` at the facilitator's check shows no tracked or new file from it.

## Premise facts
Commands: `git grep -I -n -i phrase -- . ':!board' ':!.mob' ':!docs/decisions' ':!docs/retros'`, then `git grep -l/-c` per token, `git ls-files | grep -i phrase`, `gh api` for branch protection and rulesets, and `gh issue view` for 195, 197, 198, 204, 266 and 304.

Premises in the card text as it stood that are false:
- **Directory.Build.props:** it has 0 hits.
- **CI job names:** none carry the old name (jobs `test`, `usable-v1`, `mob-process`; workflows `ci`, `mob-process`). Exactly 4 command lines in `ci.yml` name `Phrase.sln` (lines 62, 65, 70, 84).
- **Required checks:** `gh api .../branches/main/protection` returns 404 "Branch not protected" and the rulesets list is `[]`. A job rename could not break a required check.
- **Phrase.BenchCs location:** it is `tools/Phrase.BenchCs`, not under `tests/`.
- **Card 198:** it is Done (2026-10-29). `origin` is already `Programming-Made-EZ/avowry`. The `phrase-lang` hits are the local-folder placeholder `<phrase-lang>` and a path, which stay.
- **Gap table:** `docs/release/v0.1-bar.md` has no rename row, so the old criterion 3 (gap table marks the rename cards done) is false and is dropped.
- **Frozen files (D72):** `docs/CHEATSHEET.md` has 0 hits. `docs/GRAMMAR.md` has 1, the English "noun phrase". `Fixtures/orders.avowry` has 0. This card touches none of them.
- **Script file names:** there is one, `scripts/check-phrase-executed-complete.py`, with its test `tests/test_phrase_executed_complete.py`. Only that test refers to it by name.

Counts of the 1994 live lines matching `phrase` (case-insensitive), by owner:
- **Solution name `Phrase.sln`:** 21 lines in 9 files. In scope. This includes the 4 CI lines and `RepoPaths.cs`, which finds the repo root by walking up to `Phrase.sln` at run time. `BenchLauncherContract.cs:199` writes a fake `Phrase.sln` into a temp layout.
- **`Phrase.Bench` and `Phrase.BenchCs`:** 86 lines in 21 files, plus both directory names, both csproj names, and 2 lines in the solution file. Both projects are tracked, 4 files in all, and both are in the solution that CI builds. In scope.
- **Benchmark history data:** 489 lines in 2 `history.jsonl` files. Data, stays.
- **`<phrase-lang>` placeholder and `phrase-card` issue marker:** 15 and 56 lines. Both stay. The marker is the board tooling's interface.
- **Identifiers and test file names (`PhraseCli`, `PhraseCheatSheet.cs` and similar):** about 255 lines. Owned by Draft card 266 (code identifiers, test names).
- **Capital word "Phrase" in prose and the benchmark arm label:** about 212 lines in 71 files. Owned by 266. The personas are owned by 204.
- **Lowercase English word and variables:** about 856 lines. Not a name.

So about 107 lines in about 30 files are in scope, about 5% of the hits.

Ownership of the Bench names: they are IN this card. Both projects are listed in `Phrase.sln`, which this card renames. 18 of the 21 files that name a Bench project also name the solution or share a test with it. A split would touch the same files twice and leave an `Avowry.sln` that lists `Phrase.Bench`. Card 266 owns identifiers and prose, not paths.

Forms a literal scan or an anchored sed misses (card 196's lesson):
- **Escaped dots:** `tests/test_bench_launcher_scan_264.py:22` holds `Phrase\.Bench\.csproj|RunArm\("Phrase\.Bench"|Project: "Phrase\.Bench"`. The file is CRLF. If the pattern is missed, the scan test passes vacuously.
- **Backslash paths:** `Phrase.sln` has `tools\Phrase.Bench\Phrase.Bench.csproj`.
- **Derived constants:** `BenchLauncher.cs` holds `BenchProject = "Phrase.Bench"` and builds `BenchProject + ".csproj"` and `+ ".dll"`. `RunArm("Phrase.Bench")` and `Project: "Phrase.Bench"` feed the launcher. The error text "Phrase.BenchCs did not finish within 20 seconds" in the card 304 test is derived from the same string.
- **Doc, script and test triple:** the ADR table, `check-library-binding-layering.py:71-72` and `test_library_binding_layering.py:18-19` must agree text for text.
- **Existing guard test:** `tests/test_rename_196.py` reads `Phrase.sln` directly, so it breaks unless updated here.
- **Kebab-case names:** `phrase-bench-cs` and `phrase-acceptance` temp names are out of reach of a `Phrase.` pattern. They go to 266.

CRLF files in the touched set: `BenchChild.cs`, `BenchLauncher.cs` and `test_bench_launcher_scan_264.py`. The solution file is LF.

## Split decision
One card. About 30 files and about 107 lines with 3 criteria is a fraction of card 196 (144 files). It is one concern: renaming the .NET build graph's remaining old names. If the architect's slice exceeds 15,360 bytes, the cut is (a) solution, CI, docs and script name versus (b) the Bench projects; the seat advises against it (same files twice, an `Avowry.sln` that lists `Phrase.Bench`). No new card is filed. The leftover identifier and prose hits are carded under 266 and 204; the facilitator tells 266 about the roughly 1,880 lines it keeps.

## Card text (replaces the issue body)
```card
<!-- phrase-card:197 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** tooling (S; Avowry rename 8 of 9)
- **Risk to shipping:** real, bounded. The commit edits `.github/workflows/ci.yml`, which runs on every push to main. A wrong solution path makes `test` and `usable-v1` fail at restore and build, so main CI goes red, and CI on main is part of the line. No required check names depend on this (branch unprotected, rulesets empty; job names unchanged). Rollback is `git revert` of the one rename commit.
- **Source:** the product owner's naming decision (Avowry) and Decider D125; premise facts at HEAD 2f0e48a.
- **Depends on:** 196 (Done).
- **Story:** As Morgan, I want the solution file and the benchmark tool projects to say Avowry, and every command, script and workflow that names them to keep working, so that I can build, test and trace the repository by the product's name without a stale path or a broken gate.
- **Slice:** one atomic rename commit; rewrite rules: Phrase.sln to Avowry.sln, Phrase.Bench to Avowry.Bench (covers BenchCs), and the escaped-dot form; named exceptions listed in the slice.

Scenario: [Z] no old build-graph name remains
  Given the tracked files at HEAD
  When tracked paths and live lines (not board/, .mob/, docs/decisions, docs/retros, history.jsonl) are searched for Phrase.sln, Phrase.Bench and Phrase.BenchCs in plain, escaped-dot and backslash-path forms
  Then none match, Avowry.sln, tools/Avowry.Bench and tools/Avowry.BenchCs are tracked with exact case, and scripts/check-avowry-executed-complete.py replaces the old script name

Scenario: [O] one project: the Bench tool
  Given a build of Avowry.sln
  When the launcher looks for tools/Avowry.Bench/bin/<cfg>/<tfm>/Avowry.Bench.dll and bench/README.md's self-check command runs against tools/Avowry.Bench
  Then the dll is found fresh and the self-check exits 0

Scenario: [M] many: every project and test still there
  Given the before-lists (Compiler 284, Acceptance 317 listed; 600 non-pending)
  When Avowry.sln is listed and tested
  Then it names the same seven projects and three solution folders with their original GUIDs, and each test list equals its before-list name for name

Scenario: [B] bytes and splits survive
  Given the three CRLF files, the solution GUIDs and the escaped regex in test_bench_launcher_scan_264.py
  When the byte-parity checker undoes the rewrite on every changed file
  Then it reproduces the old bytes (line endings, GUIDs, sizes) for all files except the named exceptions, and the scan test's pattern still matches at least one file

Scenario: [I] what names the build graph still works
  Given ci.yml (4 lines), README and HANDOFF gate commands, run-llm-check.py's Bench project path, RepoPaths' root probe and the ADR, script and test triple
  When python -m unittest tests.test_rename_197 and the path-bound python modules run
  Then they pass at their before counts and the ADR, script and test agree

Scenario: [E] partial work and the old command are refused
  Given a copy of the finished tree with one old name put back (a ci.yml line, the RepoPaths probe, the escaped regex, one Bench path)
  When the Slice=197 check runs, or `dotnet test Phrase.sln` is run
  Then the check names the file and line; the old command fails with "does not exist" (no compatibility file is kept)

Scenario: [S] the plain route
  Given the finished commit pushed after an isolated gate
  When `dotnet test Avowry.sln --filter "Status!=Pending"` runs in a clean copy and CI runs on main
  Then totals equal the baseline (Compiler 284; Acceptance 313 passed and the same 3 known failures) and CI is green on test, usable-v1 and mob-process

## Done checks (3, one per cycle)
1. [Z][E][B] tests/test_rename_197.py scan plus byte-parity checker; fails today (about 107 lines).
2. [O][M][I] lists, solution, path-bound modules and put-backs.
3. [S][E] clean-copy gate with Load line, old command refused, CI green after push.

## Not in this card
Identifiers, test file names and prose (266); personas (204); the README line naming the nonexistent src/Phrase.Providers.CSharp (179); history.jsonl data; the `<phrase-lang>` placeholder and the phrase-card marker; board/, .mob/, docs/decisions, docs/retros; frozen D72 files; any CI matrix or Windows CI job.

## Done when
python -m unittest tests.test_rename_197
dotnet test Avowry.sln --filter "Status!=Pending"
```

## Task force proposal
- **Seats:** PM (this seat); architect (slice; SHIP AS IS unless over 15,360 bytes); QA (red); build seat (green); non-author ship gate, with one checklist item: Linux CI is case-sensitive and Windows is not, so each new name's case is checked exactly; fresh Claude sign-off. Left out: UX, security, technical writer, domain expert (the gate checklist covers CI).
- **Author:** the rotation after Copilot (196) is Grok. The seat could not probe it: `DRIVERS.md` is stale (last entries 2026-10-30, Grok out on a 402), so a fresh probe is required. Fallbacks: agy, then Codex, then Copilot. Copilot is not benched, because the ladder coaches rather than bans; it simply is not next, and its Rung 1 misreport on 196 stays at 0/3. Everyone is at Rung 1, so the facilitator re-runs every claim, whoever builds. Claude is not the author.
- **Red:** QA writes `tests/test_rename_197.py` (python, not collected by the dotnet gate). Red 1 holds the scan test (Z, E) and the byte-parity test (B) before the green exists (the D202 change from 196, where the parity test came after the green). The parity test computes the old bytes from the parent of the commit that adds `Avowry.sln`, reads them with `git cat-file`, applies the rules (literal, escaped, and the script-name rename), and lists the named exceptions (`tests/test_phrase_executed_complete.py` line 9, the `test_rename_196.py` references, the two script paths). It is red today ("no commit adds Avowry.sln"). The green seat runs it on its own commit and attaches the output before saying "clean"; a count of renames or greps is not a clean claim; the facilitator recomputes from the old bytes as on 196. Cycles 2 and 3 are born green after the atomic rename and each is shown red by a named put-back: a ci.yml line, the RepoPaths probe, the escaped regex in the 264 scan (red on the non-vacuity assertion), a Bench path in `run-llm-check.py`, an ADR versus script mismatch, a CR strip in a CRLF file. A fourth cycle needs a D175 window.
- **Green:** a fresh worktree, `git mv` of the solution, the two Bench directories (with their csproj files) and the script pair, then GNU sed with the anchored rules. Staged by explicit path, nothing deleted, old `bin/obj` left alone.
- **Baseline** (red 1, `.mob/records/197/baseline/`): the sorted test lists; the `Status!=Pending` summary (Compiler 284, Acceptance 313 passed, 3 known failures, with the failure names); the five python module counts; the solution GUIDs and `git ls-files --eol`; the card 304 caveat: `BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms` failed 3 of 6 full runs under load and passed 8 of 8 alone (9 to 11 s); it names `Phrase.BenchCs` in its message, so this card touches it. A failure of that test alone is rerun in isolation and recorded under 304; it is not waved through and not an automatic HOLD.
- **Gate and sign-off:** isolated per D202. Both run in a clean `git archive` or single-branch clone. No push, other gate or seat build runs at the same time, and the push waits. Each report carries a Load line for every full run. The gate checks that both Bench dlls and both test dlls appear, plus the commit order and the parity output.
- **CI path:** one push after the isolated gate; then `python scripts/check-ci-main.py --wait` until `test`, `usable-v1` and `mob-process` are green on the head SHA; record the run URL and the 217 extractor outcome count against the baseline. CI is Linux-only; Windows evidence is the local gate in `local-windows-run.md`.
- **Rollback:** `git revert` of the one rename commit; the workflow change is in the same commit, so main stays coherent.
- **Board follow-up:** 70 of 161 open issues carry `dotnet test Phrase.sln` in their Done-when. The facilitator syncs those bodies after the green push, or at each card's Ready rewrite. Otherwise a pulled card runs a command that no longer exists.

## Risks
1. **CI red on main.** A wrong `ci.yml` path or a case typo that Windows hides. Mitigation: the exact-case tracked-path check and the CI wait.
2. **Scripts that open the solution or Bench.** `RepoPaths.cs`, whose failure breaks every acceptance test at once; `run-llm-check.py:499`; the fake layout in `BenchLauncherContract.cs`, which must match what the probe now looks for.
3. **Vacuous passes.** The escaped regex in the 264 scan, and the `FLOOR` filters.
4. **Docs links.** README, HANDOFF, the ADR table and `bench/README.md`. The ADR, script and test triple must agree.
5. **.gitattributes.** It holds no `tools` or solution entries (a single `-text` line for the corpus fixtures), so it needs no edit; the parity checker covers the CRLF files.
6. **GUIDs.** Swapped GUIDs happened on 196. The `Phrase.sln` GUID-pairing test is carried over to `Avowry.sln`.
7. **Frozen files.** None are touched; the card says so.
8. **Stale local artifacts.** Old `bin/obj` under `tools/Phrase.*` stay untracked and can mask a missed path. Only the clean-copy gate is trusted.
9. **Load.** The full gate takes about 11 minutes.

## Facilitator's rerun and correction note
Re-run at the boundary: `Phrase.sln` is named by 9 live files and `ci.yml` has 4 such lines; branch protection returns 404 "Branch not protected"; 70 open issues carry `dotnet test Phrase.sln`; `git status` shows no tracked change from the seat's scratch file. **Correction:** the seat wrote "the same six projects" in scenario [M]; `grep '^Project(' Phrase.sln` gives 10 entries: seven projects (Avowry.Compiler, Avowry.Adapters.CSharp, Avowry.Acceptance.Tests, Avowry.Compiler.Tests, Phrase.Bench, Phrase.BenchCs, Avowry.Cli) and three solution folders (src, tests, tools). The card text above says seven projects and three solution folders. Untracked, ignored `tests/Phrase.Acceptance.Tests` and `tests/Phrase.Compiler.Tests` directories (old bin/obj) still exist locally and are not tracked.
