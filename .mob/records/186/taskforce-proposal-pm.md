# Card 186 proposal (product manager seat, ready; reply-only; a Cursor seat on Probation rung 1, its first real seat)

The seat's reply (Cursor in ask mode, through cursor_reply.py, a fresh context; it could read files but its shell calls, `git log` and `gh`, were refused), committed verbatim, with the facilitator's reruns and corrections and the declaration voted at the end. The proposal the Decider votes is the card text in section 2 (as corrected below), the scope and slice in section 4 (as corrected below), and the facilitator's section "THE DECLARATION VOTED", which replaces section 5. The standing-rules preflight is section 0 of the seat's reply and is checked again in the facilitator's section.

**Summary:** card 186 can be closed by the mob without waiting on the product owner. The mob commits one findings page, guarded by named tests across three criteria, and leaves the owner's decisions listed as open items. My shell calls were refused (`git log` and `gh` both), so the author facts below come from `.git/config` and `.git/logs/HEAD`, and nothing was run.

## 0. Standing-rules preflight

- **D137:** each criterion gets one C# test with `Status=Pending` and `Criterion=186-n`. Only one is pending at a time, and the green removes only that trait.
- **D154:** the page cites the HEAD each scan ran on. No "done" without a pushed commit.
- **D157:** three red-then-green cycles, one per criterion. The gate checks the order from `git log`.
- **D204:** every NEW-STATE test is watched failing on the pre-change tree. `personal-names` is declared CHARACTERISATION up front.
- **D222:** all three criteria are MULTI. Each id is listed in the red record.
- **Card format:** a Connextra story for Steve, 3 criteria, scenarios titled with ZOMBIES letters, and named tests with their pre-change failures.
- **Value rule:** Steve approves the first publish on recorded evidence and answers in one word. Dana gets terms and a name she can trust (`TRADEMARKS.md:3`).
- **CODING_GUIDELINES:** tests assert exact rows (lines 30–35). Documents use the role "the product owner" (line 49). No personal name or email address is copied into the page.

## 1. Premise facts

**Stale text in the card**
- The card says Phrase and `Phrase.sln` (`board/186…:13,21-22,33`). The gate today is `dotnet test Avowry.sln` (`TEAM_WORKING_AGREEMENT.md:540`, `ci.yml:62-70`).
- The Context's risk about the Phrase localization brand is superseded by D125 (`process-and-staffing.md:128`).
- The Amendment's "card 188" means the publish card (`v0.1-bar.md:34`). Card 186's own issue is also #188, so the text should say "the publish card" to avoid confusion.

**Placeholders**
- The real placeholders are `NOTICE:2` and `TRADEMARKS.md:5`.
- `tests/test_license_files.py:54-64` pins them, so filling in the holder must change that test.
- `LICENSE:190` is the standard Apache appendix boilerplate and is hash-pinned (`test_license_files.py:44-52`). It is not a placeholder and must not be edited.

**Dependencies (direct references in tracked csproj files)**

| Package | Version | Licence | Where it is used |
|---|---|---|---|
| Microsoft.NET.Test.Sdk | 17.11.1 | MIT | test projects |
| xunit | 2.9.2 | Apache-2.0 | test projects |
| xunit.runner.visualstudio | 2.8.2 | Apache-2.0 | test projects |
| FastEndpoints | 8.3.0 | MIT | `reference/csharp/Orders.Api` and the generated-project template (`CSharpProvider.cs:193`) |

- The licences come from the nuspec files in the local NuGet cache (`%USERPROFILE%\.nuget\packages`), not from the csproj files.
- `Avowry.Cli.csproj` has no PackageReference, so the packed tool ships no third-party package.
- The csproj sets no licence expression, authors or copyright. That is a finding for the publish card.
- Transitive licences (for example FluentValidation 12.1.1) are not knowable offline.
- The repo has no `package.json`, requirements file or `pyproject.toml`.

**Secrets scan tooling**
- No gitleaks, trufflehog or detect-secrets appears in `scripts/` or `docs/`, and none is under WinGet. I could not check PATH.
- `scripts/check-no-secrets.py` only checks staged `MOB.md` (lines 28–33).
- Two of its patterns would flood a history scan: long hex strings (line 22, which matches every sha256 `prompt_hash` in `bench/results`) and home paths (lines 23–24).

**Author metadata**
- `.git/logs/HEAD` has 3,123 entries, all one identity: the repo's local config (`.git/config:8-9`), which is the owner's full personal name and a personal gmail address.
- D177 (`process-and-staffing.md:789`) records commits under a handle-style name with the same address.
- So there are at least two names and one address. The reflog shows local identity, not authorship, so the true count is unverified.

**The history is probably already public**
- `v0.1-bar.md:34,51` and `check-no-secrets.py:2` say the repository is public.
- If so, any secret found is already exposed and the remedy is rotating it. Rewriting history is banned (`TEAM_WORKING_AGREEMENT.md:103-106`).

**Personal-name check**
- The gate runs only the checker's unit tests (`DocumentationPrinciples.cs:27`). The scan of tracked files runs only in the pre-commit hook (`.githooks/pre-commit:8`), not in CI.
- Untracked scratch copies with Phrase-named projects exist under `.mob/records/234/scratch/`, so every scan must use `git ls-files`.
- The check matches only the first name. The surname appears as a law's name (`TEAM_WORKING_AGREEMENT.md:49`).

**What is knowable offline, and what is not**
- **Name:** only D125's preliminary screen from 2026-10-29 (NuGet, npm, PyPI, crates.io, the GitHub handle, avowry.dev and avowry.io were free). Checking again today needs the web.
- **Vendor terms:** the repo has no terms text. The inventory is knowable: the models in `bench/results` are grok-4.7, copilot-default, codex-gpt-5.6-sol and agy-gemini-4-argon-medium, and their rows hold scores, not output text (`grok-4.7.json:1`). A summary of the terms needs the published terms.

## 2. The card text

````markdown
<!-- phrase-card:186 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** release (S; v0.1 bar 7): a findings page plus tests
- **Repository:** avowry (`docs/release/pre-publication-checks.md`, `tests/Avowry.Acceptance.Tests/PrePublicationChecks.cs`, `tests/Avowry.Acceptance.Tests/DocumentationPrinciples.cs`)
- **Source:** docs/release/v0.1-bar.md bar 7; D121; D125
- **Depends on:** none
- **Story:** As Steve, I want the pre-publication findings (licences, vendor terms, history, name) committed with a recommendation, and each decision only I can make asked in one word, so that I can approve the first publish knowing nothing public will need to be taken back.
- **Consumer value:** Steve decides on evidence; Dana finds terms and a name she can trust are the official toolchain's.
- **Observes:** one page; each section gives findings, the command and HEAD hash, and a Recommendation line; an Owner items list, each open with the date asked.

- **Criterion 1 (NEW-STATE, MULTI):** ids `package-licences` (every PackageReference id+version in tracked `*.csproj` and the generated-project template: licence, nuspec source, shipped / generated / test-only) and `vendor-terms` (every model in `bench/results/*.json`: vendor, what is published, terms status). Tests: `Every_package_reference_has_a_licence_row`, `Every_benchmarked_model_has_a_terms_row`. Pre-change: page absent.
- **Criterion 2 (MULTI):** `secrets-scan` (NEW-STATE: command, range root..HEAD, patterns, count, a disposition per finding, never a value), `author-metadata` (NEW-STATE: distinct author/committer identities counted against `git log`, described without names or addresses), `personal-names` (CHARACTERISATION: `scripts/check-no-personal-names.py` exits 0; if it fails pre-change it becomes NEW-STATE). Tests: `History_scan_records_range_command_and_dispositions`, `Author_identity_count_matches_git_log`, `DocumentationPrinciples.The_personal_name_check_passes_on_tracked_files`. Pre-change: sections absent.
- **Criterion 3 (NEW-STATE, MULTI):** `name-collisions` (each registry and domain with date and source, headed "not a professional search"), `owner-items` (public name; holder line at NOTICE:2 and TRADEMARKS.md:5; professional trademark search; the four lawyer questions; author identity going forward; unread vendor terms). Tests: `Name_section_is_marked_not_a_professional_search`, `Every_owner_item_is_open_with_the_date_asked`. Pre-change: sections absent.

Scenario: [Z] zero findings still recorded
  Given the history scan finds nothing
  Then the page shows 0, the command and the HEAD hash

Scenario: [O,S] one generated-project dependency
  Given FastEndpoints 8.3.0 in the template
  Then one row: MIT, generated project, not in the Avowry package

Scenario: [M] many references, one row each
  Given three test packages in three projects
  Then each id+version appears once and none is missing

Scenario: [B] tracked files only
  Given untracked Phrase-named copies under .mob/
  Then only `git ls-files` paths count

Scenario: [I] one-word owner ask
  Given an owner item
  Then it is open with its ask date, and NOTICE and TRADEMARKS.md still hold the marked placeholder

Scenario: [E] the mob cannot verify
  Given terms or a trademark search need the web
  Then the row says "not reviewed: needs <source>" and recommends holding the publish

Scenario: [E] a live secret is found
  Then the page names file and commit, not the value; stop the line; rotation goes to the owner

- **Not in this card:** filling the holder; editing NOTICE, TRADEMARKS.md, LICENSE; the professional search; reading terms online; rewriting history or git identity; CLI package metadata (finding for the publish card); publishing.

## Done when

```
dotnet test Avowry.sln --filter "Status!=Pending"
python scripts/check-no-personal-names.py
```
````

## 3. Story-promise check

| What Steve must be able to see | Covered by |
|---|---|
| The dependency licences are safe | Criterion 1, `package-licences` |
| What publishing benchmark results exposes | Criterion 1, `vendor-terms` |
| The history holds no secret, and what the identity metadata reveals | Criterion 2 |
| What the name check did and did not cover | Criterion 3, `name-collisions` |
| Each decision he has to make, asked in one word | Criterion 3, `owner-items` |
| A recommendation for each topic | the Recommendation line each named test asserts |

## 4. Scope and slice

Ship as is, in three increments in this order: Criterion 1, then 3, then 2. History goes last because a finding there stops the line. Each increment is a pending C# red, then a green that adds that section of the page, which is a committed document.

The only code is the tests, plus whatever history-scan command the architect fixes. That command is recorded verbatim on the page and is not added as a new script.

**How the card closes without the owner:**
- The card closes when the gate and CI are green, with the Owner items still open.
- The facilitator asks each owner item in the next report, in the one-word escalation format (`TEAM_WORKING_AGREEMENT.md:185`).
- The publish card's Ready check reads the Owner items. It cannot skip the holder line silently, because `test_license_files.py:54-64` pins the placeholder and would have to change.

## 5. Task-force declaration

**Seats**
- **Product manager (Ready):** Cursor, a reply-only seat, which rung 1 allows.
- **Architect (slice):** a fresh Claude context. It decides the history-scan patterns (keep or allow-list long hex and home paths, with reasons), and whether the tests call git (which adds a process-start caller).
- **Red (QA):** Grok.
- **Green (developer and technical writer):** a fresh Claude context, never the red's seat.
- **Security reviewer for Criterion 2:** Muse.
- **Cold reader reading as Steve (D206), before the gate review (D219):** Qwen, reply-only.
- **Gate reviewer:** a fresh Claude context. Grok wrote the reds, so it is not independent.
- **Sign-off:** Muse in a fresh context, or Claude with a recorded reason if Muse fails its probe.
- **Decider voters:** Grok, Muse twice, Cursor, Qwen and Claude.

**Domain-expert question:** yes, a licensing or trademark lawyer is needed. No seat can fill that role, so it goes to the owner as an owner item. Codex, Copilot and agy are out of quota.

**What the pre-ship gate must run**
1. Build the CLI.
2. Replay each red on its pre-change commit: NEW-STATE tests fail by assertion, the characterisation test passes.
3. Check the criterion order from `git log`.
4. Check the red record's MULTI list against the card.
5. Run the full suite in a single-branch clone.
6. Run `python scripts/check-no-personal-names.py`.
7. Rerun the page's history-scan command and compare the counts.
8. Rerun `git log --format="%an|%ae|%cn|%ce"`, take the distinct lines, and compare with the page's count.
9. Confirm `git diff <pre>..HEAD -- NOTICE TRADEMARKS.md LICENSE` is empty.
10. Include the Gate evidence and Load lines, and check CI on main.

## 6. Risks and what I could not verify

- **Nothing was run.** Shell calls were refused: `git log`, `gh issue view 188` and the personal-name scan. The issue body may differ from the archive copy, and the author count is unverified.
- **History scan noise.** The scan will be noisy. A real secret means stop the line, plus the owner's OK under the safety carve-out (security exposure).
- **The name list may be out of date.** It only restates the 2026-10-29 screen.
- **Vendor terms can't be read offline.** So the card closes with them open, and that open item is a risk the owner holds.
- **Transitive licences are unknown** without the restore output (`obj/project.assets.json`, which is untracked).
- **Weak reds.** A red that fails only because the page is missing is weak. The tests should assert specific rows so that a partial page also fails.
- **Split: no.** It is one concern, with three criteria and three increments, inside the thin-slice rule (D136). Owner items are not board cards. Split only if the scan finds a secret, which becomes its own bug card.

---

# Facilitator's reruns, corrections and the declaration

## Reruns (facts checked by the facilitator on HEAD 0412ea2; the seat could not run commands, so every shell fact is rerun here)

- Dependencies: confirmed. The tracked `*.csproj` and `*.props` files hold exactly four PackageReference ids: `xunit` 2.9.2 (3 projects), `xunit.runner.visualstudio` 2.8.2 (3), `Microsoft.NET.Test.Sdk` 17.11.1 (3) and `FastEndpoints` 8.3.0 (1, in `reference/csharp/Orders.Api`); the generated-project template in `CSharpProvider.cs:193` also writes `FastEndpoints` 8.3.0. The NuGet cache holds `fastendpoints`, `microsoft.net.test.sdk` and `xunit` (the licence text is in the nuspec there).
- **Author metadata (a correction).** The seat inferred "at least two names and one address" from D177 and the reflog. `git log --format='%an|%ae' | sort -u` gives ONE distinct author identity, `%cn|%ce` ONE distinct committer identity, and the combined four-field count is ONE, over 3,119 commits: every commit carries the repository-local identity (the owner's name and a personal address, in `.git/config`). The finding for the owner is therefore that the whole public history carries that one identity, not that several appear; whether to keep it is an owner item. The page records the counts and describes the identity without printing a name or an address.
- Placeholders: confirmed at `NOTICE:2` (`Copyright [YEAR] [COPYRIGHT HOLDER: to be confirmed by the product owner at the pre-publication checks (card 186)]`) and `TRADEMARKS.md:5` (`Trademark holder: [COPYRIGHT HOLDER: ...]`); `tests/test_license_files.py` pins them.
- The personal-name check runs only in `.githooks/pre-commit:8` and, through `DocumentationPrinciples.cs`, as its unit tests (`python -m unittest tests.test_no_personal_names`) in the gate; no CI workflow runs the scan of tracked files. `scripts/check-no-secrets.py` checks staged `MOB.md` only; its pattern list includes two noisy ones (a long hex string, home paths).
- Secrets-scan tooling: none of `gitleaks`, `trufflehog`, `detect-secrets` is on PATH. The scan is therefore a recorded `git` command over the history with the credential-shaped patterns of check-no-secrets.py minus its two noisy ones, and the page records the command verbatim; the architect fixes it.
- Vendor models: `bench/results/` holds four model result files (`agy-gemini-4-argon-medium.json`, `codex-gpt-5.6-sol.json`, `copilot-default.json`, `grok-4.7.json`), `history.jsonl`, `invalid` and `self-check`.
- The card is issue 188 (`<!-- phrase-card:186 -->`).

## Corrections to the seat's draft (binding on the card text)

1. **One test with ids, not three Pending cycles.** D137 allows one Pending test at a time; cards 189 and 202 used one aggregated test with one id per row and one atomic green. This card does the same unless the architect shows a reason for three increments: one Pending test `Pre_publication_findings_match_the_repo` with the ids of the seat's three criteria (`package-licences`, `vendor-terms`, `secrets-scan`, `author-metadata`, `personal-names` (a characterisation), `name-collisions`, `owner-items`), each id a check against the committed page and the repo (D222: the red record lists every MULTI row of the Ready draft with its ids first; the seat marked all three criteria MULTI).
2. **Author metadata** reads `git log` (not the reflog) and compares the page's counts with the repo; the page prints counts and roles, never a name or address (the personal-name rule).
3. **The history scan.** The seat's "stop the line on a live secret" stays; the repository is public (`docs/release/v0.1-bar.md`, `check-no-secrets.py:2`), so a found secret is already exposed and the remedy is rotation by the owner; history is not rewritten (TEAM_WORKING_AGREEMENT.md); a finding becomes its own bug card and an owner item.
4. **Owner items are not board cards**, and the card closes with them open; each carries the date asked and is asked in the next report to the product owner (this report). The publish card's Ready check reads them. The placeholders in NOTICE and TRADEMARKS.md stay as they are (`test_license_files.py` pins them).
5. **Docs card rules.** This is a docs card with a cold reader (D206): the cold reader reads only the findings page as Steve; the gate review starts only after the cold-reader report and any page revision are on the tip (D219, trial 1 of 3).
6. **Process-start callers.** A test that calls `git` spawns a process: the slice names the file and the red adds it to `ProcessStartCallers` (`GeneratedApiSourceScan.cs`).
7. **Seats (the product owner's direction of 2026-11-08: Cursor and Qwen on Probation rung 1, with a real review-type seat each).** The seat's table is changed as follows: sign-off stays a fresh Claude context that runs its own full gate (a Cursor ask-mode seat cannot run commands); Cursor takes a SECOND read-only review of the final tip (page, diff and records), after the independent gate reviewer, every claim of which the facilitator reruns; Qwen (reply-only) takes the cold-reader seat as Steve if the gate is idle (fallback Muse); Muse reads the secrets-scan section with a security lens (read-only). Decider voters: Grok, Muse, Cursor, Qwen and Claude by the runner's rotation (a seat that does not give a counted vote falls back and the fallback is recorded).

## THE DECLARATION VOTED

The task force of section 5 of the draft with the changes of correction 7: slice (architect) a fresh Claude context, read-only; red (QA) Grok, a worktree; green a fresh Claude context, never the red's seat; cold reader Qwen (fallback Muse); independent gate reviewer a fresh Claude context; second read-only review Cursor; separate fresh sign-off Claude. The baseline gate by the facilitator, isolated, in a clone, the CLI built first. Decider seats: six fresh seats at once through decider.py, Muse in two. Per the Decider protocol step 6 (the confirmation step; adopted by D224) this proposal does not change the agreement, so if the primary window passes with no No and no amendment no separate re-run is run (trigger line (d)); an amendment goes through the confirmation step.
