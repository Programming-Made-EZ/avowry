# Card 178 proposal (product manager seat, ready; read-only; a Grok seat)

The seat's reply (a fresh context, read-only on the repository), committed verbatim, with the facilitator's reruns and the declaration voted at the end. The proposal the Decider votes is the card text in section 2, the scope and slice in section 4, and the task-force declaration in the facilitator's section "THE DECLARATION VOTED", which replaces section 5. The standing-rules preflight (the facilitator's own discipline since the card 176 retro) is section 0 of the seat's reply and is checked again in the facilitator's section.

# Card 178 Ready (product manager, read-only)

## 0. STANDING-RULES PREFLIGHT

- **D137.** One Pending criterion test at a time. Criteria 1 and 2 share one Pending test `Prerequisites_page_matches_what_CI_uses`; the green removes only `Status=Pending`. Criterion 3 is characterisation and is never Pending.
- **D154 / D157.** The red record stores the pre-change commit, the command, the failing test, and the expected failure. The gate replays that red on the commit before the first production change and lists Criterion order from `git log`.
- **D204.** Characterisation (criterion 3) is committed first and passes on the pre-change tree. The new-state red is watched failing, then one atomic green adds the page and removes Pending with `Criterion: 1-2`.
- **D206.** Cold-reader seat at the gate reads only `docs/PREREQUISITES.md` as Dana; report under `.mob/records/178/`. Story-promise check is section 3. The drift test is a live-state guard and reads the working tree. The characterisation reads committed blobs (`git show <sha>:path`).
- **Card format rule.** Connextra with Dana; Gherkin scenarios titled with ZOMBIES letters; three criteria, each NEW-STATE or CHARACTERISATION.
- **Value rule.** Dana can tell before installing whether Avowry will work on her machine.

## 1. PREMISE FACTS

- **CI, no matrix.** `.github/workflows/ci.yml`: jobs `test` (52–70) and `usable-v1` (73–87) use `runs-on: ubuntu-latest` and `dotnet-version: 8.0.x`. `mob-process.yml:16` is `ubuntu-latest` with Python only. There is no `strategy:` / `matrix:` and no `windows-latest`. Architecture is unpinned.
- **SDK / TFM.** `Directory.Build.props:3` `net8.0`. Emitter `src/Avowry.Adapters.CSharp/CSharpProvider.cs:157,170` writes `net8.0`. No `global.json`. TWA 533–534: .NET 8 SDK. CLI inherits TFM; `Avowry.Cli.csproj:5–8` is `PackAsTool`, command `avowry`, package `Avowry`, version `0.1.0` (card 176 closed).
- **Install command (tests only).** `PackedToolIsVersionedInstallsAndBuildsTheDemo.cs:46–51`: `dotnet tool install Avowry --tool-path … --add-source … --configfile …` with sources cleared. No user-facing doc says `dotnet tool install`.
- **Guide / README commands today.** Root `README.md:30–32`: `dotnet test Avowry.sln` and `--filter "Status!=Pending"`. Demo `demo/tasks/README.md:9–12,21–45`: .NET 8 SDK, bash+curl, `dotnet run --project src/Avowry.Cli -- build … -o /tmp/avowry-tasks`, `dotnet build` / `test` / `run`. Root `README.md:39` says a newer ASP.NET Core runtime plus `DOTNET_ROLL_FORWARD=Major` can run the reference tests.
- **No prerequisites page.** `docs/PREREQUISITES.md` is absent. Docs guards: `tests/test_demo_readme.py` + `TasksDemoReadme.cs:46–55`; `tests/test_live_docs_266.py` (working tree for live scans, blobs for history); `tests/test_readme_reference_status.py`.
- **Stale on 178.** Done-when `Phrase.sln`; “CI workflow matrix”; “Windows and Linux versions CI uses” (CI is Linux only); criterion 3 talks about the **guide** (card 179). Card 177 (Windows/Linux install jobs) is still Ready.
- **326 → 178.** Unclear prerequisites (SDK vs newer runtime: cold-reader.md, README:39 vs demo README:11); each command says where to run it and what it needs. Layout, stale status, `trace` / PH → 179. D211: merge 326 into 178 and 179, then close 326 as duplicate.
- **Audience rec.** One line on the page: v0.1 is .NET-first; generated output is C# for net8.0 and needs the .NET 8 SDK. Reasoning lives on card 187.
- **Unverified without CI/dotnet.** Exact Ubuntu image and CPU arch behind `ubuntu-latest`. Facilitator: `gh run list --workflow=ci.yml --branch main --limit 1` then `gh run view <id> --log` (look for runner OS / arch); `dotnet --list-sdks`.

## 2. THE CARD TEXT

```markdown
<!-- phrase-card:178 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** docs (S; v0.1 bar 1)
- **Repository:** avowry (`docs/PREREQUISITES.md`, `tests/test_prerequisites.py`, `tests/Avowry.Acceptance.Tests/PrerequisitesPage.cs`)
- **Source:** v0.1 bar 1; D121; D211 (326 findings); `.mob/records/176/pm-audience-recommendation.md`
- **Depends on:** card 176 (Done)
- **Story:** As Dana, I want the supported .NET SDK version, operating systems and architecture stated in one place, so that I know before installing whether it will work for me.
- **Observes:** one page names the SDK pin and OS labels CI uses, says architecture is unpinned, says what to do on untested systems, states that v0.1 generated output needs the .NET 8 SDK, and gives a PowerShell form of each command on that page.
- **Advances:** v0.1 bar 1 (prerequisites page matches CI).

- **Criterion 1 (NEW-STATE):** `docs/PREREQUISITES.md` states the CI `dotnet-version` and every `runs-on` of jobs that set up dotnet, that architecture is unpinned, what to do on systems CI does not test, the .NET-first line (emitter emits C# for net8.0, so build and run need the .NET 8 SDK; a newer ASP.NET Core runtime alone is not enough), that the CLI is installed from a local feed and is not on a public registry, and each command names the working directory and what it needs, with a PowerShell equivalent (or “same in PowerShell”). Named test (shared with criterion 2): `Prerequisites_page_matches_what_CI_uses`. Pre-change failure: the page is missing.
- **Criterion 2 (NEW-STATE):** that test parses `.github/workflows/*.yml` (job-level `runs-on` / `dotnet-version` and any future `strategy.matrix`) and fails unless every such value appears on the page and every SDK/OS claim on the page appears in those workflows. Live-state guard: reads the working tree. Same named test; same pre-change failure. Style: `tests/test_demo_readme.py` plus a C# wrapper with `Slice=178`, `Criterion=178-1`, `Status=Pending` (`TasksDemoReadme.cs`). Aggregate independent checks in one assertion (card 176).
- **Criterion 3 (CHARACTERISATION):** `README.md` and `demo/tasks/README.md` stay byte-identical to the pre-change commit. Passes on the pre-change tree. Reads committed blobs.

Scenario: [Z] a machine CI does not test
  Given Dana’s OS or architecture is absent from the CI workflows
  When she reads the prerequisites page
  Then it says those systems are untested and that she needs the stated SDK if she tries anyway

Scenario: [O,S] one tested environment
  Given CI uses ubuntu-latest and 8.0.x
  When Dana reads the page
  Then it names that SDK pin and that OS label, and the .NET-first sentence

Scenario: [M] every CI job
  Given one or more jobs (or a matrix) set `runs-on` and `dotnet-version`
  When the drift test runs
  Then every such value is on the page and every SDK/OS claim on the page is in the workflows

Scenario: [B] SDK form and unpinned architecture
  Given CI pins 8.0.x, projects target net8.0, and the workflow names no architecture
  When Dana reads the page
  Then it states 8.0.x, net8.0 for generated projects, and that architecture is unpinned

Scenario: [I] commands say where, what they need, and PowerShell
  Given the commands on the page (check the SDK; the local-feed install shape)
  When Dana is on Windows without bash
  Then each command names its working directory and prerequisite, and a PowerShell form is given

Scenario: [E] missing SDK or runtime-only
  Given a machine with no .NET 8 SDK, or only a newer ASP.NET Core runtime
  When Dana reads the page
  Then it says the CLI and generated output need the .NET 8 SDK

ZOMBIES: all letters apply.

- **Not in this card:** publishing; README and getting-started guide (179), including Layout, stale status, `trace` / PH, linking this page, and PowerShell for **guide** commands (179 criterion 3); known-limitations page (187) beyond the one .NET-first line; package-id ownership (186); adding Windows/Linux CI jobs (177 — that card updates this page in the same change so criterion 2 stays green); supporting other operating systems.
- **326:** prerequisites findings are here; remaining findings go to 179; then close 326 as duplicate (D211).

## Done when

```
dotnet test Avowry.sln --filter "Status!=Pending"
```
```

## 3. THE STORY-PROMISE CHECK (D206)

Promise: “I know before installing whether it will work for me.”

| Dana must find | Cover |
|---|---|
| Required SDK version | C1, C2 |
| Which OS CI tests | C1, C2 |
| Architecture | C1 (unpinned) |
| Untested systems | C1 [Z][E] |
| SDK vs runtime-only | C1 [E] |
| Local-feed, not published | C1 |
| Where each page command runs and what it needs; PowerShell | C1 [I] |

Install walkthrough, README link, guide PowerShell: 179. Windows as a CI-tested OS: 177. Why .NET-first: 187.

## 4. SCOPE AND SLICE

SHIP AS IS: one page, three criteria, three increments.

1. Characterisation (C3) on the pre-change tree.
2. One Pending red for C1+C2, watched failing because the page is missing; red record has hash, command, test, expected failure.
3. One atomic green: the page plus the python checks; remove Pending; `Criterion: 1-2`.

Caps: C3 zero green cycles; C1+C2 one atomic green. A later `runs-on` / `dotnet-version` change (177) updates this page in the same commit.

## 5. TASK-FORCE DECLARATION

- **Ready (PM):** Grok (this proposal).
- **Slice (architect):** Claude, fresh, read-only. Reason: Grok holds PM; Codex out until about 2026-11-11; Copilot and agy out of quota.
- **Red (QA):** Grok, fresh; characterisation first, then the one Pending red; red record complete.
- **Green:** Claude, fresh; never the red’s seat.
- **Cold-reader:** trial driver (Cursor or Muse; local Qwen reply-only). Reads only `docs/PREREQUISITES.md` as Dana; writes `.mob/records/178/cold-reader.md`. Claude fallback if the harness fails.
- **Independent gate reviewer:** Claude, fresh. Gate is not a trial-driver seat.
- **Sign-off:** a separate fresh Claude context that does not read the gate report before its own reruns.

**Gate, in order:** `dotnet build src/Avowry.Cli`; full suite in a single-branch clone of main (not an archive; C3 needs blobs); replay the Pending red on the commit before the first production change and HOLD unless it is an assertion failure matching the red record; `git log` Criterion order. Report: Gate evidence line (hash, CLI build, how the copy was made, totals) and a Load line. Trial drivers stay off counted writing seats.

## 6. RISKS AND WHAT YOU COULD NOT VERIFY

- Naming a Windows version as CI-tested would be false until 177.
- Pinning an Ubuntu release or x64 instead of the workflow labels would drift when GitHub moves `ubuntu-latest`.
- `dotnet tool install Avowry` from nuget.org is untested and must not be implied.
- Did not run dotnet or a CI log. Facilitator: `gh run list --workflow=ci.yml --branch main --limit 1`; `gh run view <id>` for image/arch; `dotnet --list-sdks`. DRIVERS.md probes are dated 2026-10-30; staffing above follows the brief and the card 176 retro.

## FACILITATOR'S RERUNS AND CORRECTIONS (2026-11-08 UTC)
- Reruns against the repository: .github/workflows/ci.yml has `runs-on: ubuntu-latest` at lines 12, 52 and 74 and `dotnet-version: 8.0.x` at lines 59 and 81, no `strategy` or `matrix`, no Windows job; docs/PREREQUISITES.md does not exist; README.md lines 30 to 32 hold the `dotnet test Avowry.sln` commands; the C# wrapper pattern for a python docs guard exists in tests/Avowry.Acceptance.Tests/TasksDemoReadme.cs (it runs `python -m unittest tests.test_demo_readme`); card 326 is open (merged into 178 and 179 at their Ready steps, then closed as a duplicate); card 176 is closed (the CLI is `Avowry` 0.1.0, installed from a local feed, not published). The seat's unverifiable items (the exact Ubuntu image and architecture behind `ubuntu-latest`; `dotnet --list-sdks`) are confirmed by the facilitator at the gate and do not change the page, which states the workflow labels, not a pinned image.
- Correction 1 (Done-when): the issue's Done-when changes from `dotnet test Phrase.sln` to `dotnet test Avowry.sln --filter "Status!=Pending"`; the issue's old "Task force" section is replaced by the declaration below.
- Correction 2 (the pending red): the single Pending test is a python test wrapped by a C# test with `Status=Pending` and the Criterion trait, in the style of TasksDemoReadme.cs; the green removes only the Pending trait line and the python test file stays byte-identical between red and green (D137); the python test is the live-state guard and reads the working tree on purpose (D206).
- Correction 3 (the cold-reader seat): it is a read-only seat on whichever driver the seat plan names (Muse or Cursor on trial where the harness allows, else Grok or Claude); its report is committed under .mob/records/178/ with the lines it read. It is not a task-force role.

## THE DECLARATION VOTED (replaces section 5)
- Ready (product manager): Grok (this proposal).
- Slice (architect): a fresh Claude context, read-only (the reason: the PM is Grok, Codex is out of quota until about 2026-11-11, Copilot and agy are out of quota).
- Red (QA): Grok, a fresh context in its own git worktree; the characterisation first (README.md and demo/tasks/README.md byte-identical to the base commit, reading committed blobs), then the single Pending red for criteria 1 and 2 with the red record (the pre-change commit, the command, the failing test, the expected failure); it runs the existing tests that assert on the same files before committing (the card 176 retro's neighbouring-test rule, a line of the QA brief).
- Green (build): a fresh Claude context in its own git worktree (never the red's seat; Codex out of quota); one atomic green `Criterion: 1-2`: the page and nothing else, removing only the Pending trait line.
- Cold-reader seat for this docs card (D206): see correction 3.
- Independent pre-ship gate reviewer: a fresh Claude context (read-only; the reasons as for card 176); it also replays the Pending red on the commit before the first production change (it must fail by assertion matching the red record) and lists the Criterion order from git log.
- Fresh sign-off: a separate fresh Claude context that does not read the gate report before its own reruns.
- The gate runs, in order: `dotnet build src/Avowry.Cli`, the full suite in a single-branch clone of main (not an archive), the python docs guards (`python -m unittest tests.test_prerequisites tests.test_demo_readme`), the replay; the report carries a Gate evidence line (the commit hash, the CLI build, how the copy was made, the totals) and a Load line.

## AMENDMENT AFTER THE PRIMARY WINDOW OF D216 (the outliers' conditions, adopted)
Primary window: architect (Claude), developer and decider Yes; QA, coach and audit lens No (Grok). The three conditions, each checked against the standing rules (D137 one Pending test; D154 and D157 the gate replay; D204 the red record fields; the card 176 aggregated-checks practice) and adopted:
1. QA's and the audit lens's condition (the same substance): the single Pending python test records an independent check for every Criterion 1 bullet (the CI `runs-on` and `dotnet-version` values, the unpinned architecture, what to do on untested systems, the .NET-first sentence with the net8.0 and the SDK-versus-runtime-only statement, the local-feed install and that the CLI is not published, each command's working directory and prerequisite, and the PowerShell form) and reports every failing check in one aggregated assertion (as the card 176 test did), so a page that only names the workflow labels stays red like a missing page; Criterion 2's reverse match covers only the values parsed from the workflows (`runs-on`, `dotnet-version` and any matrix pins), not the other Criterion 1 sentences.
2. The coach's condition: THE DECLARATION VOTED gains three fields, with no change of assignee. (a) Each role's domain and why this card needs it: Ready, Grok, Dana's prerequisites and the story promise; slice, a fresh Claude, the CI-to-page drift guard (a python test wrapped by one Pending C# test) and the one-Pending-test rule; red, Grok, the red-first order, the aggregated checks and the characterisation by committed blobs; green, a fresh Claude, the page and nothing else; gate reviewer and sign-off, fresh Claude contexts, independence from the authors and the replay of the red; the cold-reader seat, the reader's view of a docs card (D206). (b) The default card-mode seats left out and why: no tech-writer seat (the cold-reader seat and the story-promise check cover the reader), no security seat (the page changes no code path or data), no extra backend developer (the green is the build seat), no delivery lead (one page, three increments). (c) The domain-expert question: none is needed, since every SDK and OS claim is checked mechanically against .github/workflows.
Each of the three outliers is asked only whether its own condition is now met; the other three are asked only whether the amendment changes their vote.
