# Card 179 proposal (product manager seat, ready; read-only; a Grok seat)

The seat's reply (a fresh context, read-only on the repository), committed verbatim, with the facilitator's reruns and corrections and the declaration voted at the end. The proposal the Decider votes is the card text in section 2 (as corrected below), the scope and slice in section 4 (as corrected below), and the facilitator's section "THE DECLARATION VOTED", which replaces section 5. The standing-rules preflight is section 0 of the seat's reply and is checked again in the facilitator's section.

# Card 179 Ready (product manager, read-only)

## 0. STANDING-RULES PREFLIGHT

- **D137.** One Pending criterion test: `Getting_started_docs_match_danas_story` (python `tests.test_getting_started`, C# wrapper `GettingStartedDocs`). The green removes only `[Trait("Status", "Pending")]`. Python body identical between red and green.
- **D154 / D157.** Red record stores pre-change commit, command, failing test, expected assertion. Gate replays that red on the commit before the first production change and lists Criterion order from `git log`.
- **D204.** Characterisation of `demo/tasks/README.md` is a slice constraint (blob identity), not a fourth criterion. Criteria 1–3 are NEW-STATE. One Pending red, then one atomic green `Criterion: 1-3`.
- **D206.** Cold-reader seat reads only `README.md` and `docs/GETTING_STARTED.md` as Dana. Story-promise check is section 3. The new test is a live-state guard (working tree). Any byte-identity check uses `git show <sha>:path` and an exact key, never a subject prefix.
- **Process-start callers.** A wrapper is added. Slice names `tests/Avowry.Acceptance.Tests/GettingStartedDocs.cs` and `tests/Avowry.Acceptance.Tests/GeneratedApiSourceScan.cs` (`ProcessStartCallers`; add `GettingStartedDocs.cs` in the **red**, or the full gate fails as on 178).
- **Card format.** Connextra with Dana; Gherkin titled with ZOMBIES letters; three criteria, each NEW-STATE, each with a named test.
- **Value rule.** Dana can judge Avowry and reach a running spec-first API in about 15 minutes.
- **350-line / no parser.** New python test: fenced-block regex and substring needles in the style of `tests/test_demo_readme.py`. No YAML or Avowry parser. Aim ≤350 lines.
- **D217.** This proposal does not change the working agreement. If the primary window has no No and no amendment, trigger (d): no separate re-run. An amendment uses the confirmation step.

## 1. PREMISE FACTS

**Root README today** (`README.md`): H1 Avowry (line 1); “names are descriptions” / “specs are the truth” (3); “compiles through a language-neutral IR to target providers; C# (FastEndpoints + xUnit) is first” (3); “Avowry is the language; each avow is one line of intent in it” (3); command and `.avowry` extension (5); rename note (7); Layout (17–26) omits `demo/`, `board/`, `scripts/`, `Avowry.sln`; Running is `dotnet test Avowry.sln` (30–32); Test status table (37–44) still says `CompileEndpointToCSharp` is Red (Pending) with `NotImplementedException` at the resolver (40) and resolver tests Red (Pending), 83 (44); Next: inner loop still lists Resolver “in progress” (54); contributing is `HANDOFF.md` (13). No clone URL. No link to `docs/PREREQUISITES.md`. No getting-started guide. No `demo/`.

**Stale (326 / 266 cold-reader, verified in repo):** `CompileEndpointToCSharp.Generated_post_endpoint_binds_its_request_and_returns_its_success_response` has no Pending trait (`CompileEndpointToCSharp.cs:18–19`). `OrdersPhraseCompiles` asserts that class is not Pending (`OrdersPhraseCompiles.cs:50–58`). No `Status=Pending` under `tests/Avowry.Compiler.Tests`. The only remaining Pending acceptance test is Usable v1 bar 6 (`UsableV1.cs:101–103`), not the resolver. `HANDOFF.md:1` says the resolver is long done. Demo README builds Tasks with 11 specs (`demo/tasks/README.md:21–42`). Those two READMEs contradict. Layout is incomplete. `trace` and PH codes are unexplained. Line 39 claims a newer ASP.NET Core runtime plus `DOTNET_ROLL_FORWARD=Major` can run the reference tests; `docs/PREREQUISITES.md:11` says a newer ASP.NET Core runtime alone is not enough.

**Demo README** (`demo/tasks/README.md:9–45`): .NET 8 SDK; bash + curl; `dotnet run --project src/Avowry.Cli -- build demo/tasks/Tasks.avowry -o /tmp/avowry-tasks`; then `dotnet build` / `test` / `run` on generated `Tasks.Api` / `Tasks.Specs`. No PowerShell. `/tmp/` is the Unix path. Guarded by `tests/test_demo_readme.py` and `TasksDemoReadme.cs`. **Leave it byte-identical.**

**Prerequisites** (`docs/PREREQUISITES.md`): CI `ubuntu-latest` + `8.0.x`; architecture unpinned; untested systems; .NET-first / net8.0; local feed, not published; pack `src/Avowry.Cli/Avowry.Cli.csproj`; `dotnet tool install Avowry --tool-path <dir> --add-source <feed>`; three blocks with Run from / Needs / Same in PowerShell. **No clone URL** (178 cold-reader). Lines 7–9: “A newer SDK than 8.0.x is also untested. If you try anyway, you need the .NET 8 SDK” is awkward (178 gate NOTE 3). Keyword checks in `tests/test_prerequisites.py` must keep `untested`/`try`/`.NET 8 SDK`/`other operating system` and `net8.0`/`ASP.NET Core runtime`/`not enough`.

**CLI that the guide may call, as the repo states (dotnet not run):**
- Install: `Avowry.Cli.csproj:4–8` PackAsTool, command `avowry`, package `Avowry`, version `0.1.0`. Shape proven in `PackedToolIsVersionedInstallsAndBuildsTheDemo.cs:46–65`: pack, `dotnet tool install Avowry --tool-path … --add-source …`, `avowry --version` prints `Avowry <version>`, `avowry build demo/tasks/Tasks.avowry -o <out>` writes `*.Api.csproj` and `*.Specs.csproj`.
- Build: `Program.cs:11` `avowry build FILE.avowry -o OUT_DIR [--target csharp]`. Only csharp.
- Specs / start: demo README (`demo/tasks/README.md:37–48`) `dotnet test …Tasks.Specs` (Passed: 11) and `dotnet run --project …Tasks.Api --urls http://localhost:5099`. Packed-tool test does **not** run specs or start the API.
- Regeneration: `CompilationOutput.cs:20` `File.WriteAllText` overwrites the same relative paths; it does not delete leftovers.
- Guide file: **absent** (no `docs/GETTING_STARTED.md`).
- Clone: `git remote` is `https://github.com/Programming-Made-EZ/avowry.git`. Stated in `HANDOFF.md:3`, not on the newcomer path.

**Neighbour tests a README rewrite must keep green:** `tests/test_license_files.py:34–42,66–70` (H1 `# Avowry`, command/extension sentence, avow sentence, generated-code sentence); `tests/test_live_docs_266.py:257–261` (exact rename line); `tests/test_readme_reference_status.py` requires a status-table row for `ReferenceTarget` (`scripts/check-readme-reference-status.py:10–13`) — that table is the stale inner-loop page; retire the script and its tests in the same green and fold “no resolver-pending / no NotImplementedException claim” into the new live-state test. `tests/test_readmes_untouched_178.py` compares README blobs at BASE vs the commit that **added** `docs/PREREQUISITES.md`, not HEAD, so a later README edit does not fail it.

**Unverified without dotnet (facilitator):** `dotnet build src/Avowry.Cli`; pack + `dotnet tool install Avowry --tool-path <dir> --add-source <feed>`; `<dir>/avowry build demo/tasks/Tasks.avowry -o <out>`; `dotnet test <out>/Tasks.Specs`; `dotnet run --project <out>/Tasks.Api`. A **new** tiny listing is unproven; until a slice probe compiles one, the guide’s compile target is `demo/tasks/Tasks.avowry`.

## 2. THE CARD TEXT

```markdown
<!-- phrase-card:179 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** docs (M; v0.1 bar 2)
- **Repository:** avowry (`README.md`, `docs/GETTING_STARTED.md`, `docs/PREREQUISITES.md`, `tests/test_getting_started.py`, `tests/Avowry.Acceptance.Tests/GettingStartedDocs.cs`, `tests/Avowry.Acceptance.Tests/GeneratedApiSourceScan.cs`)
- **Source:** v0.1 bar 2; D121; D125; D211 (326 findings); product-owner 2026-11-08 (C# adapter preview); 178 cold-reader (clone URL; awkward newer-SDK paragraph)
- **Depends on:** 178 (Done), 176 (Done)
- **Story:** As Dana, I want a README that tells me in one screen what Avowry is and who it is for, and a guide that takes me from clone and install to a running spec-first API, so that I can judge Avowry and use it in about 15 minutes.
- **Observes:** one-screen README: Avowry is its own language, this preview is the C# adapter preview, who it is for, the three aims (verifiability, preserved intent, reviewability), clone URL, links to the guide and prerequisites, and a name of the known-limitations page; stale resolver/red-test claims gone. Guide: clone, install from a local feed, a small spec-first API (create, read, a failure path), `avowry build`, `dotnet test` of generated specs, start, call an endpoint, afterwards (`.avowry` is source, generated project is output, rebuild overwrites the same relative paths and does not delete leftovers). Every command has a no-bash path.
- **Advances:** v0.1 bar 2.

- **Criterion 1 (NEW-STATE):** `README.md` in one screen says Avowry is its own language and v0.1 is the C# adapter preview (C# / FastEndpoints is the first shipped adapter; no other target and no date); who it is for (Dana directing an agent; Dana is target-agnostic, this preview’s adapter is C#); the three aims in plain words (verifiability: specs become running tests; preserved intent: the Avowry source states what each avow must do; reviewability: a change can be checked against the spec); the clone URL `https://github.com/Programming-Made-EZ/avowry.git`; links `docs/GETTING_STARTED.md` and `docs/PREREQUISITES.md`; names the known-limitations page as not yet written (card 187) with **no** relative link to a missing file; keeps the D125 avow sentence, the command/`.avowry` sentence, the generated-code sentence, H1 `# Avowry`, and the rename line `Avowry was named Phrase until 2026-10-29; older records keep the old name.`; drops Layout, the Test status table, Running `dotnet test` as the newcomer path, resolver-in-progress, `NotImplementedException`, unexplained `trace`/PH, `DOTNET_ROLL_FORWARD`, and any promise of Wasm or a second target. Same named test as 2 and 3. Pre-change failure: README still has the resolver Pending / `NotImplementedException` row; no clone URL; no guide or prerequisites link.

- **Criterion 2 (NEW-STATE):** `docs/GETTING_STARTED.md` covers clone of that URL; install by packing and `dotnet tool install Avowry --tool-path <dir> --add-source <feed>` from a local feed, run by path under `<dir>` (placeholders without angle brackets); writing a small spec-first API (create, read, a failure path) with a fenced `avowry` listing, compiling a **proven** target (`demo/tasks/Tasks.avowry` unless a slice probe has compiled the listing); `avowry build FILE.avowry -o OUT_DIR`; `dotnet test` of the generated Specs project; start the Api project and call an endpoint; afterwards: `.avowry` is the source, generated projects are output, a rebuild overwrites the same relative paths and does not delete leftover files. Install commands keep Run from / Needs / Same in PowerShell. Reword `docs/PREREQUISITES.md` lines 7–9 so “newer SDK untested” cannot be read as “skip the .NET 8 SDK”, without dropping the 178 keyword needles. Same named test. Pre-change failure: `docs/GETTING_STARTED.md` is missing.

- **Criterion 3 (NEW-STATE):** every **command** fence in the guide has a path that needs no bash: `dotnet` and `git` and `avowry` are “Same in PowerShell.”; any HTTP client that is `curl` has a real `powershell`/`pwsh` block (`Invoke-RestMethod` or `curl.exe`); output folder is a Windows-usable path (`$env:TEMP\…` or an absolute path), not only `/tmp/`. Same named test: `Getting_started_docs_match_danas_story` / `tests.test_getting_started`. Pre-change failure: the guide is missing, so the PowerShell and Windows-path checks are blocked by `page missing` (assertion, not FileNotFound).

Named test (C1–C3): python `tests.test_getting_started.GettingStartedTests.test_readme_and_guide_match_the_story`; wrapper `Avowry.Acceptance.Tests.GettingStartedDocs.Getting_started_docs_match_danas_story` with `[Trait("Slice", "179")]`, `[Trait("Criterion", "179-1")]`, `[Trait("Status", "Pending")]`. Live-state, working tree. Aggregate independent checks in one assertion (card 176/178). Style: `TasksDemoReadme.cs` (`ProcessStartInfo("python")`, `-m unittest tests.test_getting_started`, 2 minute timeout).

Scenario: [Z] no guide yet
  Given the tree before this card’s green
  When the named test runs
  Then it fails by assertion that `docs/GETTING_STARTED.md` is missing, and every guide check is blocked

Scenario: [O,S] one screen and one path
  Given Dana has only the README
  When she reads one screen and follows the linked guide
  Then she sees what Avowry is, who it is for, the three aims, the C# adapter preview, the clone URL, and can clone, install, build, run specs, start, and call create and read

Scenario: [M] every command
  Given the fenced commands in the guide
  When Dana is on Windows without bash
  Then each command has a PowerShell form or “Same in PowerShell.”, and HTTP is not bash-only `curl`

Scenario: [B] untested SDK and Windows output
  Given a newer SDK than 8.0.x, or Windows
  When Dana reads prerequisites (from the README link) and the guide
  Then a newer SDK is untested and is not a substitute for having the .NET 8 SDK; a newer ASP.NET Core runtime alone is not enough; the output folder is not `/tmp/`-only

Scenario: [I] links and the installed command
  Given the README and the guide
  When Dana looks for the next page or the compiler
  Then the README links the guide and prerequisites, names known limitations as card 187, and the guide’s compile step is `avowry build` from the tool-path install, not `dotnet run --project src/Avowry.Cli` as the primary path

Scenario: [E] a failure path and regeneration
  Given the sample API and a second build
  When Dana follows the guide
  Then the sample includes a declared failure; afterwards text says `.avowry` is source, generated code is output, and rebuild overwrites the same relative paths without deleting leftovers

ZOMBIES: all letters apply.

- **Not in this card:** publishing; writing the known-limitations page (card 187 / issue 186) or linking a missing file; package-id ownership (card 186 / issue 188); running the guide’s commands (180); a cold model run (181); other targets or a Wasm date; editing `demo/tasks/README.md`; Windows/Linux CI jobs (177); PERSONAS.md still saying Phrase (card 204); explaining `trace` or PH on the one-screen README (drop them). Close 326 as duplicate after this Ready.

## Done when

```
dotnet test Avowry.sln --filter "Status!=Pending"
```
```

## 3. THE STORY-PROMISE CHECK (D206)

Promise: Dana judges Avowry from one screen and uses it (clone → running spec-first API) in about 15 minutes.

| Dana must find | Cover |
|---|---|
| What Avowry is (own language; C# adapter preview) | C1 |
| Who it is for (Dana, target-agnostic; this preview’s adapter is C#) | C1 |
| Three aims | C1 |
| Clone URL/host | C1 |
| Links: guide, prerequisites; known limitations named | C1 (187 writes the page) |
| Stale resolver/red-test/Layout/`trace`/PH/roll-forward gone | C1 |
| Awkward newer-SDK paragraph readable | C2 (PREREQUISITES reword) [B] |
| Install from local feed, run by path | C2 [I] |
| Write create, read, a failure | C2 [E] |
| `avowry build`, specs, start, call | C2 [O] |
| Afterwards: source vs output vs regeneration | C2 [E] |
| No bash required | C3 [M][B] |
| ~15 minutes | value; timed run is 181, not here |
| Wasm / other targets as shipped | Not-in-this-card (do not promise) |

## 4. SCOPE AND SLICE

SHIP AS IS: one README, one guide, three criteria, three increments.

1. Characterisation (slice constraint, not a criterion): `demo/tasks/README.md` byte-identical to the pre-change commit; `git show`; passes on the pre-change tree.
2. One Pending red for C1–C3, watched failing because the guide is missing and the README still has the resolver Pending / `NotImplementedException` claims; wrapper + `ProcessStartCallers` entry in the **same** red; red record complete.
3. One atomic green `Criterion: 1-3`: README, guide, PREREQUISITES wording, retire `scripts/check-readme-reference-status.py` and `tests/test_readme_reference_status.py`; remove only the Pending trait. Keep the license-test strings and the 266 rename line so those tests stay green without edits.

Caps: characterisation zero green cycles; C1–C3 one atomic green. Keep the README to one screen so the countable diff stays revertable. Hostile inputs for the test: missing guide; README that only restyles the old table; curl without a PowerShell block.

## 5. TASK-FORCE DECLARATION

- **Ready (PM):** Grok (this proposal). Domain: Dana’s one-screen promise and the C# adapter preview wording.
- **Slice (architect):** fresh Claude, read-only. Reason: PM is Grok; Codex out until about 2026-11-11; Copilot and agy out. Domain: live-state docs guard, callers list, proven compile target.
- **Red (QA):** Grok, fresh. Characterisation first, then the one Pending red; neighbouring tests (`test_license_files`, `test_live_docs_266`, `test_prerequisites`, `test_demo_readme`) run before commit.
- **Green:** fresh Claude; never the red’s seat.
- **Cold-reader (D206, not a task-force role):** Muse (product owner: Muse takes Decider and cold-reader). Fallback Cursor. Reads only `README.md` and `docs/GETTING_STARTED.md` as Dana; writes `.mob/records/179/cold-reader.md`. Qwen reply-only with inlined text if used.
- **Independent gate reviewer:** fresh Claude. Replay the Pending red on the commit before the first production change; Criterion order from `git log`.
- **Fresh sign-off:** a separate fresh Claude context that does not read the gate report before its own reruns.

Left out: tech-writer (cold-reader + promise check); security (no code path); extra backend (green is the build seat); delivery lead (two docs, three increments). Domain-expert: none; the preview claim is the 2026-11-08 product-owner decision.

**Gate, in order:** `dotnet build src/Avowry.Cli`; full suite in a `git clone --no-hardlinks --single-branch -b main` of main (not an archive); `python -m unittest tests.test_getting_started tests.test_prerequisites tests.test_demo_readme tests.test_license_files tests.test_live_docs_266`; replay the Pending red; `git log` Criterion order. Report: Gate evidence line (hash, CLI build, how the copy was made, totals) and a Load line. Known unrelated red: `tests.test_live_docs_266` T11 / bug 338 — do not HOLD on it.

## 6. RISKS AND WHAT YOU COULD NOT VERIFY

- A new tiny `.avowry` listing is unproven; the packed-tool test only builds `demo/tasks/Tasks.avowry`. Slice probe before the green, or the guide compiles that demo file.
- `WriteTo` does not delete leftover generated files (`CompilationOutput.cs:20`); the afterwards section must say so.
- PowerShell 5 aliases `curl` to `Invoke-WebRequest`; the guide must use `curl.exe` or `Invoke-RestMethod`.
- `dotnet tool install` without `--configfile` may consult nuget.org; the package is not published (178 gate NOTE 5). Say local feed only.
- A relative link to a missing known-limitations file fails `ScopedLinks`. Name card 187; do not link the file.
- Did not run dotnet. Facilitator: the pack/install/build/test/run commands in section 1. `tests.test_live_docs_266` T11 is bug 338. Close 326 as duplicate after this Ready.

---

# Facilitator's reruns, corrections and the declaration

## Reruns (facts checked by the facilitator on HEAD 3b3a074)

- README.md Test-status table: the `CompileEndpointToCSharp.Generated_csharp_builds_and_every_spec_passes` row says "Red (Pending)" and the resolver row says "Red (Pending), 83 tests" (README lines 36 to 44): confirmed stale (card 326's finding).
- `docs/GETTING_STARTED.md` does not exist; `docs/PREREQUISITES.md` exists; `ProcessStartCallers` in `tests/Avowry.Acceptance.Tests/GeneratedApiSourceScan.cs` (lines 22 to 29) does not list `GettingStartedDocs.cs`: so the red must add it, as on card 178 (otherwise the red commit alone fails the full gate).
- `avowry build FILE.avowry -o OUT_DIR [--target csharp]` is the usage string of `src/Avowry.Cli/Program.cs:11`; no bash is needed for it.
- Card numbers (board): 186 is the pre-publication checks (name, licences, history, vendor terms), 187 the known-limitations page, 188 publishing the first version.

## Corrections to the seat's draft (binding on the card text)

1. **Card numbers.** The draft's "package-id ownership (card 186 / issue 188)" is replaced by "publishing (card 188) and the pre-publication checks (card 186)". The known-limitations page is card 187 (the draft says "card 187 / issue 186" once: use 187 only).
2. **Retiring `scripts/check-readme-reference-status.py` and `tests/test_readme_reference_status.py` is not free.** `tests/test_rename_196.py:296` and `tests/test_rename_197.py:326` list the module `tests.test_readme_reference_status` (4 tests) in their module tables, and `README.md:39` is the row the script requires. The architect's slice decides between (a) the green removes the table, the script and its test and edits the two table lines in test_rename_196.py and test_rename_197.py in the same green, with the new live-state test absorbing the "no resolver-pending / no NotImplementedException claim" check, or (b) the README keeps one honest `ReferenceTarget` status row. Either way the slice names every neighbouring test the green touches, and the full gate must pass.
3. **Wording of the preview (product-owner decision 2026-11-08).** The README and the guide say that Avowry is its own language and that this preview ships one adapter, the C# one. They name no other target and no date. They do not say "Wasm". Dana stays target-agnostic.
4. **Cold reader (D206).** The cold-reader seat reads only `README.md` and `docs/GETTING_STARTED.md` and answers as Dana: what is Avowry, who is it for, what do I do first, what would I not know before installing; the clone URL and the reworded newer-SDK sentence are the two points the card 178 reader raised. Its report is committed under `.mob/records/179/` before the sign-off.
5. **Test size.** The new python test is aimed at 350 lines or fewer, written with fenced-block regexes and substring needles (the style of `tests/test_demo_readme.py`): no hand-rolled parser (the card 178 lesson).
6. **Characterisation of `demo/tasks/README.md`** is a slice constraint on the red's commit, declared and passing on the pre-change tree; it is not a fourth criterion. It is proven on a tree that already has the new state before the red is filed (the card 178 lesson; a line of the QA brief).

## THE DECLARATION VOTED

The task force of section 5 of the draft with these changes: slice (architect) Claude, read-only, writes the slice and decides correction 2; red (QA) Grok, a worktree; green a fresh Claude context that is never the red's seat; the cold reader Muse (fallback Cursor; fallback Qwen reply-only); gate reviewer and sign-off two separate fresh Claude contexts (no non-author non-trial driver is in quota); the baseline gate by the facilitator, isolated, the CLI built first. Decider seats: six fresh seats at once through decider.py, Muse in two of them. Per D217 this proposal does not change the agreement, so if the primary window passes with no No and no amendment no separate re-run is run (trigger line (d)); an amendment goes through the confirmation step.
