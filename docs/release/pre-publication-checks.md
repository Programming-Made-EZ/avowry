# Pre-publication checks (card 186)

Findings for the sponsor persona before the first package publish, recorded 2026-11-08. Each figure comes from the command named in its section, run on the tree at commit `c4692e074c8970081687d332adb64f200728426c` (the commit before this page), except where a section says otherwise: the Owner items section reads `NOTICE` and `TRADEMARKS.md` as filled by card 366 and the product owner's answers of 2026-11-08 (its scan tip is kept for the other sections), the licences come from the local NuGet cache, the home-path counts were taken at commit `dde36dc`, and a few bench counts are labelled as counted by a short script. Where the repository cannot say, the page says `not reviewed: needs <source>`. Nothing here is legal advice.

The repository is public now (`docs/release/v0.1-bar.md:34` and `:51`). So the history, the author identity, the records and the bench rows below are already exposed, and a problem found in history cannot be undone by holding the publish: pushed history is not rewritten (`TEAM_WORKING_AGREEMENT.md`). The hold recommendations below apply to what is published next: the package, new bench output and new releases.

## Package licences

Command: `git grep -n PackageReference -- '*.csproj' '*.props' '*.targets'` plus the same search in `src/Avowry.Adapters.CSharp/CSharpProvider.cs`; licences read from the `<license>` element of each nuspec file in the local NuGet cache (`~/.nuget/packages`), not from the scan tip. Scan tip: `c4692e074c8970081687d332adb64f200728426c`.

| id | version | licence | source | where used |
| --- | --- | --- | --- | --- |
| FastEndpoints | 8.3.0 | MIT | nuspec licence expression | generated project (`CSharpProvider.cs:237`) and `reference/csharp/Orders.Api` |
| Microsoft.NET.Test.Sdk | 17.11.1 | MIT | nuspec licence expression | test-only: three tracked test projects, `reference/csharp/Orders.Specs`, generated Specs project (`CSharpProvider.cs:251`) |
| xunit | 2.9.2 | Apache-2.0 | nuspec licence expression | test-only: same projects as above (`CSharpProvider.cs:252`) |
| xunit.runner.visualstudio | 2.8.2 | Apache-2.0 | nuspec licence expression | test-only: same projects as above (`CSharpProvider.cs:253`) |

Shipped with the compiler and CLI: none. `Avowry.Cli.csproj` and every other project under `src/` have no PackageReference; the first package is added only to a project the compiler generates.

Compatibility (bar 7 asks for it, this page only lists): the four direct packages are permissive (MIT, Apache-2.0), and the mob sees no incompatibility with Apache-2.0. That is the mob's reading, not legal advice; the lawyer questions were skipped by owner decision (Owner items).

Package metadata, for the publish card: `src/Avowry.Cli/Avowry.Cli.csproj` sets the package id, version, tool command and a description, but no licence expression, no authors, no copyright and no packed `NOTICE` (Apache-2.0 section 4 asks that the notice travel with redistributions).

Transitive packages: not reviewed: needs a restore (`obj/project.assets.json` is untracked, so the repository cannot list them).

Recommendation: the four direct packages are permissive; accept them, set the package metadata on the publish card, and treat the transitive licences as a review (card 369).

## Vendor terms

Command: `git ls-files 'bench/results/*.json' ':!bench/results/self-check/*'` (four vendor model files; the unfiltered pathspec also lists one self-check fixture), then the `model` value and the `reply_head` field of each file. Scan tip: `c4692e074c8970081687d332adb64f200728426c`.

| model | vendor (inferred from the file name only) | reply_head published | terms status |
| --- | --- | --- | --- |
| grok-4.7 | xAI (inferred) | no reply_head in this file; present in history rows | not reviewed: needs the vendor terms page |
| codex-gpt-5.6-sol | OpenAI (inferred) | no reply_head in this file; present in history rows | not reviewed: needs the vendor terms page |
| agy-gemini-4-argon-medium | Google (inferred) | reply_head present, 139 characters | not reviewed: needs the vendor terms page |
| copilot-default | GitHub (inferred; the underlying model is not named) | no reply_head in this file; present in history rows | not reviewed: needs the vendor terms page |

Correction to the earlier draft, which said bench rows hold scores and no output text: `reply_head` is a prefix of the model's reply, up to 200 characters. Commands: `grep -c reply_head bench/results/history.jsonl` gives 336 and `grep -c reply_head bench/csharp-baseline/results/history.jsonl` gives 120. The 139 characters, the 331 non-empty values in the first file, all 120 non-empty in the second and the 200-character maximum were counted by a short script that parses each row as JSON. Published rows therefore hold vendor model output.

Vendor output also sits outside the bench: the mob's seat replies are committed verbatim under `.mob/records` (for example cold-reader reports and vote files). Their terms are not reviewed either.

Recommendation: hold NEW vendor output (new bench rows, new releases) until the terms are read; rows already pushed stay public.

## Secrets scan

Command: `python scripts/scan-history-for-secrets.py c4692e074c8970081687d332adb64f200728426c`, run from the repository root. Scan tip: `c4692e074c8970081687d332adb64f200728426c`, the red commit, the last commit before the one that added this page. The range is every commit reachable from that hash (3,124 commits); commits only on other refs are not in the range. The script reads the added lines (`git log -p -U0`) of non-merge commits, loads its patterns from `scripts/check-no-secrets.py` and drops the two noisy ones (long hex string, home path), and prints the kind, path and commit of each hit, never the matched text.

Result: count 65. All 65 are of one kind, credential assignment. The other kinds (private key, GitHub token, API key prefix, AWS key, Slack token, Google key, auth header) have 0 hits. Binary files in range: 1 (an archived bundle under `.mob/records/196`); a diff scan cannot look inside it, and the slice record says its commits were unbundled in a scratch clone and scanned with 0 hits (not rerun here).

Dispositions, grouped by kind and path area (65 in total; none looks like a credential by the shape of the line, and whether a value is live cannot be shown by shape):
- 45 hits, credential assignment: an identifier or prose using the word token or secret, in seat-session logs and slice records under `.mob/records/298`, `299` and `313`.
- 5 hits, credential assignment: source code with an identifier named token (`scripts/delete_guard.py`, the parser, a lexer test).
- 4 hits, credential assignment: prose in a decision document, a board file and a reviewer record using the word secret or token.
- 8 hits, credential assignment: a deliberate redaction fixture with a dummy lower-case value in the benchmark-harness acceptance test file (a tracked file whose old project folder name is deliberately not written here).
- 3 hits, credential assignment: a fixture or placeholder in two seat records (one builds a fake GitHub-style prefix by concatenation, one holds a placeholder), under `.mob/records/229` and `.mob/records/230`.

Limits of the scan: the content of a merge commit (its conflict-resolution lines) is not scanned; commit messages, author fields and tags are not scanned; UTF-16 files are not read; dummy values were judged by shape, not content. The patterns miss names with a keyword after an underscore (a database password variable, for example) because no word boundary sits there, miss JSON keys where a quote stands between the keyword and the colon, and do not know other token shapes (JWTs, passwords inside URLs). The new script sits in the next scan range and its own lines may add false positives of the same kind.

Home paths: the scan drops the home-path pattern as noisy, so they were counted separately at commit `dde36dc`, counts only: `git grep -l -E 'C:\\Users\\[A-Za-z0-9._-]+|C:/Users/[A-Za-z0-9._-]+' | wc -l` gives 146 files, and the same search with `-c` summed gives 277 lines (381 matches by `git grep -o`). Nearly all are under `.mob/records` (137 of the 146 files). A local account name is therefore in public history.

Recommendation: no secret was found by these patterns, and none can be pulled back from history anyway; skim the 11 fixture and placeholder hits once by eye, rotate any credential found, and scrub the home paths from tracked records (card 370; history stays public).

## Author metadata

Command: `git log c4692e074c8970081687d332adb64f200728426c --format='%an|%ae|%cn|%ce' | sort -u | wc -l` (and the pairs `%an|%ae` and `%cn|%ce` alone). Scan tip: `c4692e074c8970081687d332adb64f200728426c`, 3,124 commits.

- distinct author identities 1
- distinct committer identities 1
- combined 1

Described by role: the repository-local identity; every commit carries it. This page holds neither the name nor the address, by design. Because the repository is public, that identity is already visible in history.

Recommendation: the owner kept the repository-local identity for the package and for later commits (answered 2026-11-08).

## Personal-name check

Command: `python scripts/check-no-personal-names.py` at scan tip `c4692e074c8970081687d332adb64f200728426c`. Exit code 0. It checks attribution patterns around one first name (possessive, verb, colon, dated, by or from, escalation, a quoted passage beside the name) and a gendered pronoun beside a product role. It skips the bench jsonl files and files it cannot decode; findings under `board/` are printed as archive but do not change the exit code. It does not check emails or other names.

Recommendation: keep the check as a commit gate; it passes.

## Name collisions (not a professional search)

Source: the screen recorded in D125 (`docs/decisions/process-and-staffing.md:129`), not queried again on 2026-11-08. Scan tip: `c4692e074c8970081687d332adb64f200728426c`. Free means free on the date given.

- NuGet package id Avowry: free on 2026-10-29, source D125.
- npm: free on 2026-10-29, source D125.
- PyPI: free on 2026-10-29, source D125.
- crates.io: free on 2026-10-29, source D125.
- GitHub handle: free on 2026-10-29, source D125.
- avowry.dev: free on 2026-10-29, source D125.
- avowry.io: free on 2026-10-29, source D125.
- Trademark databases and the earlier brand called Phrase (the earlier name, no longer the product name): not reviewed by a professional search: declined by owner decision of 2026-11-10; a free first-pass screen of public databases is run by the product owner's assistant and its result (name screen: CLEAR or CONFLICT) is relayed to the mob.
- Registry state after 2026-10-29: not reviewed: needs a fresh lookup on the day of publishing.

Recommendation: repeat the registry lookups on the publish day; the owner declined a professional trademark search (2026-11-10); publishing waits for the free first-pass screen to be reported CLEAR and for the owner's written approval, which is given on the condition that no existing conflicting trademark is found.

## Owner items

What blocks what: nothing is published as a release until the free first-pass name screen is reported CLEAR and the owner approves in writing (the professional trademark search is no longer a blocker: declined by owner decision, 2026-11-10; the owner's conditional approval of 2026-11-10 is recorded in the inbox and needs the CLEAR report). Preparation (cards 366 to 370 and the publish card's package metadata) can proceed meanwhile. The registry state after 2026-10-29 is re-checked on the day of publishing (the brief says so).

Command: read from `NOTICE`, `TRADEMARKS.md`, D121, D125 and the owner's answers of 2026-11-08 (`NOTICE` and `TRADEMARKS.md` filled by card 366). These items are the product owner's answers of 2026-11-08, recorded here; the mob's inbox copy is outside the repository, so this page is the record. Scan tip: `c4692e074c8970081687d332adb64f200728426c`. Every item is answered; the search was declined by the owner on 2026-11-10 and four items are carried by cards 367 to 370.

- Public name: answered 2026-11-08. Avowry confirmed (D125); the owner does not intend to register a trademark before the preview (2026-11-10).
- Holder at `NOTICE:2` and `TRADEMARKS.md:5`: answered 2026-11-08. Programming Made EZ; 2026 is an assumption the owner can correct (docs/LICENSING.md); same holder for the trademark line: yes (card 366).
- Professional trademark search: answered 2026-11-08. Declined by the owner on 2026-11-10 (no professional search, no trademark registration before the preview); the Apache-2.0 licence and the TRADEMARKS.md policy stand. Publishing waits only for the free first-pass screen's CLEAR report and the owner's written approval.
- The four lawyer questions (D121): answered 2026-11-08. The lawyer questions were skipped by owner decision.
  1. Do the generated runtime helper files need a notice, given that generated code belongs to the user?
  2. Is a trademark policy on this name enforceable?
  3. Does the rename rule conflict with Apache-2.0 notice retention?
  4. Who is the holder? (answered by the holder item above)
- Author identity going forward: answered 2026-11-08. Decision: keep. The current author identity stays for the package and for later commits.
- Unread vendor terms (four vendors, plus the seat replies under `.mob/records`): answered 2026-11-08. Decision: read-first (card 367). Read the four vendors' terms before anything more is published.
- The `reply_head` publication choice (prefixes of up to 200 characters in the bench history files): answered 2026-11-08. Decision: trim for new rows only; pushed rows stay public (card 368).
- Transitive package licences: answered 2026-11-08. Decision: restore-and-review (card 369). Restore the packages, then review their licences.
- Home paths in tracked records: answered 2026-11-08. Decision: scrub-tracked; history stays public (card 370). Remove home paths from tracked files.

### Trademark search brief (not used: the owner declined the search on 2026-11-10; kept as the record of what was prepared)

- Applicant: Programming Made EZ.
- Applicant's country and legal form: supplied privately by the owner to the provider; not written on this page.
- Mark: Avowry.
- Mark type: word mark (the name Avowry in plain characters); the logo is not part of this search.
- Classes: 9 (downloadable software) and 42 (software development services).
- Goods and services for both classes: programming-language compiler and command-line tool; software development services.
- Jurisdictions (proposed by the mob; the owner confirms with the provider): United States, European Union, United Kingdom.
- NuGet package id: free on 2026-10-29 (D125); not checked since.
- npm: free on 2026-10-29 (D125); not checked since.
- PyPI: free on 2026-10-29 (D125); not checked since.
- crates.io: free on 2026-10-29 (D125); not checked since.
- GitHub handle: free on 2026-10-29 (D125); not checked since.
- avowry.dev: free on 2026-10-29 (D125); not checked since.
- avowry.io: free on 2026-10-29 (D125); not checked since.
- Trademark databases and the earlier brand called Phrase: not checked (the earlier name is no longer the product name; the search is for Avowry).
- Registry state after 2026-10-29: not checked. Re-checked on the day of publishing.
- Registry screen: [Name collisions](#name-collisions-not-a-professional-search).

Recommendation: name Avowry confirmed, publishing waits on the free first-pass screen being CLEAR and the owner's written approval; holder Programming Made EZ (card 366); professional search declined by the owner; lawyer questions skipped by owner decision; author identity keep; vendor terms read-first (card 367); reply_head trim for new rows only, pushed rows stay public (card 368); transitive licences restore-and-review (card 369); home paths scrub-tracked, history stays public (card 370).
