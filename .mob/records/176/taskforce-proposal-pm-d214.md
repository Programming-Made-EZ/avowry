# Card 176 proposal (product manager seat, ready; read-only; a Codex seat)

The seat's reply (a fresh context, read-only on the repository; GitHub was unreachable from its sandbox, so it read the tracked board mirror board/176-*.md), committed verbatim, with the facilitator's reruns and corrections at the end. The proposal the Decider votes is the card text in section 2 as corrected, the scope and slice in section 3, and the task-force declaration in the facilitator's section "THE DECLARATION VOTED", which replaces section 4. Facilitator's difficulty questions are answered informally, as the Dimensions of Difficulty matrix is not adopted.

## 1. PREMISE FACTS

- GitHub was unreachable: `gh issue view 176` failed with a network-permission error. I used `board/176-package-the-cli-as-a-dotnet-tool-and-version-it.md`.
- The card is stale: it names `src/Phrase.Cli`, `Phrase.sln`, `phrase`, and Phrase at lines 11–14 and 30. The current files are `src/Avowry.Cli`, `Avowry.sln`, and command `avowry` (`Program.cs:8`; amendment at card lines 35–37).
- `Avowry.Cli.csproj:1-10` contains only `OutputType`, `AssemblyName`, and project references. `git grep` found no `PackAsTool`, `ToolCommandName`, `PackageId`, or `<Version>` in it or `Directory.Build.props`. Therefore it is not configured as a tool and has no explicit version.
- `Program.cs:18-22` implements `--version` using the executing assembly version. Its actual unset value is unverified without running: `dotnet run --project src/Avowry.Cli -- --version`.
- The solution includes the CLI (`Avowry.sln`, project entry `Avowry.Cli`).
- CI restores, builds, and tests `Avowry.sln` on Ubuntu/.NET 8 (`.github/workflows/ci.yml:51-70`). It does **not** pack or locally install a tool.
- Release bar 1 requires installation from a package feed on stated Windows/Linux systems and `avowry --version` (`v0.1-bar.md:15`). Card 176 serves only the package/version portion; card 177 owns cross-platform CI installation and card 178 prerequisites (`:28`). Bars 2 and 8 concern the guide and limitations (`:16,22`), not this implementation.
- “Placeholder package id” is stale/ambiguous. D125 selected `Avowry` as the package ID (`process-and-staffing.md:127-131`), while collision/ownership confirmation remains card 186 and nothing may be published (D121, `:109-114`; release bar `:49-54`).
- D205 explicitly leaves the CLI command unchanged (`avowry`) and PH codes unchanged (`process-and-staffing.md:1112-1115`).
- Pack/install behavior is unverifiable without dotnet. Facilitator commands: `dotnet pack src/Avowry.Cli/Avowry.Cli.csproj -c Release -o <feed>`; install from that feed into a scratch tool path; run `avowry --version`; run the demo build.

## 2. THE CARD TEXT

```markdown
<!-- phrase-card:176 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** tooling (M; v0.1 bar 1)
- **Repository:** avowry (`src/Avowry.Cli`, packaging and acceptance tests)
- **Source:** v0.1 release bar 1; Deciders D121, D125 and D205
- **Depends on:** card 191 (Done: command renamed to `avowry`)
- **Story:** As Dana, I want to install a versioned Avowry CLI as a .NET tool from a package feed, so that I can try Avowry without building its source.
- **Observes:** `dotnet pack` produces an installable tool package; its installed command is `avowry`; `avowry --version` reports the package version; the installed command builds the demo.

- **Criterion 1 (NEW-STATE):** the CLI packs as a .NET tool with command `avowry`, package ID `Avowry`, and one explicit version source. Named red (shared with criterion 2): `Packed_tool_is_versioned_installs_and_builds_the_demo`, watched failing on the pre-change tree because no tool package is configured.
- **Criterion 2 (NEW-STATE):** the packed tool installs from a local folder feed into a scratch tool path; `avowry --version` reports the package version and `avowry build` builds the demo. Named red (the same test as criterion 1, committed once with Status=Pending): `Packed_tool_is_versioned_installs_and_builds_the_demo`, watched failing on the pre-change tree because no installable tool package exists.
- **Criterion 3 (CHARACTERISATION):** packaging does not change compiler output or remove existing tests. Existing Acceptance coverage is run and the demo’s generated C# is compared with its pre-change result; it passes before production changes.

Scenario: [Z] no global installation is required
  Given no Avowry tool is installed globally
  When Dana installs the packed tool into a scratch tool path
  Then installation and invocation succeed without changing the global tool list

Scenario: [O,S] one package installs one command
  Given one locally packed version
  When Dana installs it
  Then the available command is `avowry`

Scenario: [M,B] version has one source
  Given pack, package metadata and `--version`
  When their versions are inspected
  Then they agree exactly, including the chosen prerelease boundary if present

Scenario: [I] the installed tool builds the demo
  Given the packed tool and Tasks demo
  When Dana invokes `avowry build`
  Then the demo builds with unchanged generated C#

Scenario: [E] the package is absent
  Given an empty local feed
  When installation is attempted
  Then installation fails without falling back to a public feed

ZOMBIES: all letters apply; none are omitted.

- **Not in this card:** publishing to any registry; package-ID ownership or collision decisions; changing the `PH` diagnostic prefix; renaming the `avowry` command.
```

## 3. SCOPE AND SLICE

SHIP AS IS: one concern, three criteria, and at most three increments. Order (amended after rounds 1 and 2 of D212): criterion 3 characterisation first (passes on the pre-change tree); then ONE new-state test that carries criteria 1 and 2 together, `Packed_tool_is_versioned_installs_and_builds_the_demo`, asserting that the CLI packs as a .NET tool with command `avowry` and package ID `Avowry`, that pack, package metadata and `--version` agree on one version, that the package installs from a local feed with the public sources off into a scratch tool path, and that the installed tool builds the demo; it collects its independent checks (pack, command, package ID, version agreement, local-feed source isolation, scratch tool path, unchanged global tool list, installed command, demo build) and reports every failing one in a single assertion failure, so an early pack failure cannot mask the others, and the red record lists each expected failure and distinguishes the checks that failed on their own assertion from the checks blocked by the missing package, and a blocked check is not counted as evidence that it catches its violation (amended after rounds 1 and 2 of D213); it is committed with Status=Pending (D137: at most one criterion test is pending at a time and the pending-red job requires it to fail by assertion) and watched failing on the pre-change tree because no tool package is configured; then ONE atomic packaging green removes Pending in the same commit and carries `Criterion: 1-2` (the Red before the atomic green trial entry allows it). The card's criteria 1 and 2 stay separate observable behaviours in the card text; they share this red. Caps: criterion 3 zero green cycles; criteria 1 and 2 one atomic green.

## 4. TASK-FORCE DECLARATION

- PM/Ready: Codex.
- Architect/slice and packaging design: Grok.
- QA/red author: Grok.
- Build/green: Codex; never authors the red it makes green.
- Independent pre-ship gate reviewer: Muse, read/run-only.
- Final sign-off: fresh Claude seat, isolated and non-author.

The gate runs restore/build/test; builds the CLI **before** Acceptance tests; `dotnet pack`; installs from a local feed into a new scratch tool path with public-source fallback disabled; runs `--version`; and builds the demo. On Windows, write multi-line commands/scripts to files rather than passing long or layered commands through tool shims.

## 5. DIFFICULTY QUESTIONS

- Environment: .NET 8 is documented by CI; exact supported Windows/Linux versions and architecture belong to card 178. Local-feed installation should require no sign-in.
- Scope: package and prove locally; do not add registry publication or card 177’s OS matrix.
- Unknowns: package layout and assembly/package version agreement need the named red, but do not justify a spike unless packing fails in a way that blocks this top-ranked card.
- Communication: the product owner need not decide the command. `Avowry` is the recorded provisional ID; ownership/collision confirmation remains card 186.

## 6. RISKS AND WHAT I COULD NOT VERIFY

NuGet may reject or transform metadata; `--version` may expose an assembly-form version rather than the package version; local installation can accidentally fall through to public feeds; Windows command shims and locked files can make reinstall tests flaky. I could not run dotnet, inspect a produced `.nupkg`, install the tool, verify its runtime version, or run the demo. GitHub’s live issue body was also unavailable, so the tracked board mirror was the issue source.

## ROUND 2 OF D213 AND THIS PROPOSAL (D214)
D213 failed on a strict reading: round 2 had one No (QA, Codex), the five other seats Yes. QA's condition had a free alternative, adopted above: the red record explicitly distinguishes dependency-blocked checks from actual assertion failures and does not count a blocked check as evidence that it catches its violation. This is the corrected text voted as D214 (the facilitator's one corrected re-proposal after the failure of D213; a further failure returns the card's declaration to the next retro).

## ROUND 1 OF D213 (amended above)
Round 1 (all six seats launched at once; window 7.1 minutes): architect, developer, decider and coach Yes (the coach on Grok after the Codex seat gave no counted vote); QA (Codex) and audit lens (Grok) No. QA: the single pending red must aggregate its independent checks and record each expected failure, so an early pack failure cannot mask version, source-isolation, scratch-path, global-list, command or demo-build violations. Audit lens: the gate must HOLD unless the replayed pending-red output matches the committed expected-failure assertion and the runner result is an assertion failure (pending-red CI is still open, D137). Both conditions are small and reasonable and are adopted; the proposal is voted once more.

## ROUND 2 OF D212 AND THE NEW PROPOSAL (D213)
D212 (the text with both reds committed first) failed on a strict reading: round 2 had architect (Grok) and audit lens (Codex) No, QA, developer, decider and coach Yes. The architect's condition rests on a verified rule: D137 allows at most one pending criterion test at a time, so two reds cannot both be committed first; fold criteria 1 and 2 into one Pending new-state test closed by the atomic packaging green (adopted above). The audit lens's condition: each red is backed by committed evidence, recording the pre-change commit hash, the command, the failing test and the expected failure, before any production commit (adopted: the red record carries those four fields; the gate reviewer reruns it). The re-run's audit lens (Grok) added one more condition, adopted: the gate also replays the pending red on the commit before the first production change and checks the Criterion lines in git log (see the declaration). This is a new Decider (D213) on the corrected text.

## ROUND 1 OF D212 (amended above)
Round 1: architect (Grok), developer (Grok), audit lens (Codex) and coach (Codex) voted No; QA (Codex) Yes; decider (Codex, after the Muse seat on trial gave no vote: seat unavailable to decider.py) Yes. The four conditions were each adopted: (1) architect and developer: both new-state reds watched failing on the pre-change tree before any production change (criterion 2's named red would otherwise fail for a false reason after criterion 1's green); (2) audit lens: the sign-off is a separate fresh context from the gate reviewer; (3) coach: the trial drivers leave the task-force declaration and go to the seat plan. Raw round 1 in .mob/records/176/d212-round1-raw.json; the independent re-run of the unamended text was stopped.

## FACILITATOR'S RERUNS AND CORRECTIONS (2026-11-07)
- Reruns against the repository and GitHub: the live issue body is the stale text the seat found (it says Phrase, `src/Phrase.Cli`, `phrase`; Done when `dotnet test Phrase.sln`) with the D125 amendment at the end (command avowry, package id Avowry as a placeholder until card 186; follows card 191); card 191 is closed ("The command is avowry; tool output and the map file say Avowry"); D125 is in docs/decisions/process-and-staffing.md; src/Avowry.Cli/Avowry.Cli.csproj holds only OutputType, AssemblyName and two project references, so no PackAsTool, ToolCommandName, PackageId or Version exists; `Directory.Build.props` exists and does not set them (the seat's git grep). The seat's unverifiable claims are listed as the facilitator's confirmation commands in section 1; the facilitator runs them at the gate.
- Correction 1 (card text): the Done-when of the issue changes from `dotnet test Phrase.sln --filter "Status!=Pending"` to `dotnet test Avowry.sln --filter "Status!=Pending"` (the solution is Avowry.sln); the criteria say `avowry` and `Avowry.Cli`, not `phrase` and `Phrase.Cli`; the issue's old "Task force" section is replaced by the declaration below.
- Correction 2 (criterion 2): the install test uses a scratch tool path (`--tool-path`) and a local folder feed with the public sources disabled (`--ignore-failed-sources` is not enough; use `--add-source <feed>` with a nuget.config that clears sources, or the equivalent), and asserts the global tool list is unchanged.
- Correction 3 (the seat's declaration): the seat put a Muse seat on the independent gate reviewer. The product owner's direction is that trial drivers (Muse, Cursor, local Qwen) get low-stakes seats first, and the gate reviewer runs the pack, the install and the full gate: not low stakes. Corrected below.

## THE DECLARATION VOTED (replaces section 4)
- Ready (product manager): Codex (this proposal).
- Slice (architect): Grok, read-only, in a fresh context.
- Red (QA): Grok, the characterisation first, then the single pending red for criteria 1 and 2, whose committed red record gives the pre-change commit hash, the command, the failing test and the expected failure; each in its own git worktree, the facilitator reruns every claim and cherry-picks.
- Green (build): Codex, in its own git worktree; a red and its green are never the same seat; the facilitator recomputes the bytes of any mechanical change.
- Independent pre-ship gate reviewer: a fresh Claude context, read-only on the repository, running its own pack, install and gate in scratch copies. Reason recorded for this Claude seat: the authors of the card's commits are Grok and Codex, Copilot and agy are out of quota, and the remaining non-author drivers (Muse, Cursor, local Qwen) are trial drivers that start on low-stakes seats; the gate is not one.
- Trial drivers (Muse, Cursor, local Qwen) are not part of this task force (amended after round 1 of D212): the product owner's direction to trial them on low-stakes seats is carried out through the seat plan (.mob/records/176/seat-plan.md) on Decider and retro-voice seats, not as task-force roles, and each result is reported in the card's retro with the captured error of any failure.
- Fresh sign-off: a separate fresh Claude context (amended after round 1 of D212) that did not run the gate reviewer's checks and does not read the gate reviewer's report before its own reruns, isolated from the authors; the same reason as the gate reviewer for the Claude seat.
- The independent gate reviewer also replays the single pending red on the commit before the first production change, records that it fails because no tool package is configured (the failing test, the command and the output), and HOLDs unless the replayed output matches the committed expected-failure assertion and the runner result is an assertion failure, not a compile or helper failure (amended after round 1 of D213), and lists from `git log` that each Criterion line precedes the production change (D154, D157 and the Red before the atomic green trial entry; added after round 2 of D212 and its re-run).
- The gate runs, in order: build the CLI project (`dotnet build src/Avowry.Cli`), the full suite in a single-branch clone of main (not a git archive copy), `dotnet pack`, the install from a local feed into a scratch tool path with the public sources off, `avowry --version`, a demo build; the report carries a Gate evidence line (the commit hash tested, the CLI build result, how the copy was made, the totals) and a Load line.
- Informal difficulty answers: environment: .NET 8; a local folder feed needs no sign-in; scope: package and prove locally only; unknowns: package layout and version agreement are covered by the named reds, no spike unless packing fails in a way that blocks the card at the top of the board; communication: the product owner decides nothing here (Avowry is the recorded provisional id; ownership and collision stay card 186; nothing is published).
