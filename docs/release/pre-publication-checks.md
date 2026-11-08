# Pre-publication checks (card 186)

Findings for the sponsor persona before the first publish, recorded 2026-11-08. Every figure was produced by the command at the top of its section, run on the tree at commit `c4692e074c8970081687d332adb64f200728426c` (the commit before this page). Where the repository cannot say, the page says `not reviewed: needs <source>`. Nothing here is legal advice. Nothing is published.

## Package licences

Command: `git grep -n PackageReference -- '*.csproj' '*.props' '*.targets'` plus the same search in `src/Avowry.Adapters.CSharp/CSharpProvider.cs`; licences read from the `<license>` element of each nuspec file under `~/.nuget/packages`. Scan tip: `c4692e074c8970081687d332adb64f200728426c`.

| id | version | licence | source | where used |
| --- | --- | --- | --- | --- |
| FastEndpoints | 8.3.0 | MIT | nuspec licence expression | generated project (`CSharpProvider.cs:193`) and `reference/csharp/Orders.Api` |
| Microsoft.NET.Test.Sdk | 17.11.1 | MIT | nuspec licence expression | test-only: three tracked test projects, `reference/csharp/Orders.Specs`, generated Specs project (`CSharpProvider.cs:207`) |
| xunit | 2.9.2 | Apache-2.0 | nuspec licence expression | test-only: same projects as above (`CSharpProvider.cs:208`) |
| xunit.runner.visualstudio | 2.8.2 | Apache-2.0 | nuspec licence expression | test-only: same projects as above (`CSharpProvider.cs:209`) |

Shipped with the compiler and CLI: none. `Avowry.Cli.csproj` and every other project under `src/` have no PackageReference; the first package is added only to a project the compiler generates.

Transitive packages: not reviewed: needs a restore (`obj/project.assets.json` is untracked, so the repository cannot list them).

Recommendation: the four direct packages are permissive (MIT, Apache-2.0); accept them, and treat the transitive licences as an open check before the first publish.

## Vendor terms

Command: `git ls-files 'bench/results/*.json'` (four files), then the `model` value and the `reply_head` field of each file. Scan tip: `c4692e074c8970081687d332adb64f200728426c`.

| model | vendor (inferred from the file name only) | reply_head published | terms status |
| --- | --- | --- | --- |
| grok-4.7 | xAI (inferred) | no reply_head in this file; present in history rows | not reviewed: needs the vendor terms page |
| codex-gpt-5.6-sol | OpenAI (inferred) | no reply_head in this file; present in history rows | not reviewed: needs the vendor terms page |
| agy-gemini-4-argon-medium | Google (inferred) | reply_head present, 139 characters | not reviewed: needs the vendor terms page |
| copilot-default | GitHub (inferred; the underlying model is not named) | no reply_head in this file; present in history rows | not reviewed: needs the vendor terms page |

Correction to the earlier draft, which said bench rows hold scores and no output text: `reply_head` is a prefix of the model's reply, up to 200 characters. `bench/results/history.jsonl` has 336 rows with the field (331 non-empty) and `bench/csharp-baseline/results/history.jsonl` has 120 more (all non-empty). Published rows therefore hold vendor model output.

Recommendation: hold publishing vendor output (the `reply_head` prefixes) until the vendor terms are read.

## Secrets scan

Command: `python scripts/scan-history-for-secrets.py c4692e074c8970081687d332adb64f200728426c`, run from the repository root. Scan tip: `c4692e074c8970081687d332adb64f200728426c`, the red commit, the last commit before the one that added this page. The range is every commit reachable from that hash (3,124 commits); commits only on other refs are not in the range. The script reads only added lines (`git log -p -U0`), loads its patterns from `scripts/check-no-secrets.py` and drops the two noisy ones (long hex string, home path), and prints the kind, path and commit of each hit, never the matched text.

Result: count 65. All 65 are of one kind, credential assignment. The other kinds (private key, GitHub token, API key prefix, AWS key, Slack token, Google key, auth header) have 0 hits. Binary files in range: 1 (an archived bundle under `.mob/records/196`); a diff scan cannot look inside it, and the slice record says its commits were unbundled in a scratch clone and scanned with 0 hits (not rerun here). Merge commits show no diff, so a merge's content is its parents' content.

Dispositions, grouped by kind and path area (65 in total; none is a live credential; judged by the shape of the line):
- 45 hits, credential assignment: an identifier or prose using the word token or secret, in seat-session logs and slice records under `.mob/records/298`, `299` and `313`.
- 5 hits, credential assignment: source code with an identifier named token (`scripts/delete_guard.py`, the parser, a lexer test).
- 4 hits, credential assignment: prose in a decision document, a board file and a reviewer record using the word secret or token.
- 8 hits, credential assignment: a deliberate redaction fixture with a dummy lower-case value in `tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs`.
- 3 hits, credential assignment: a fixture or placeholder in two seat records (one builds a fake GitHub-style prefix by concatenation, one holds a placeholder), under `.mob/records/229` and `.mob/records/230`.

Limits: dummy values were judged by shape, not by content; token shapes the patterns do not know (JWTs, passwords inside URLs) are not covered. The new script sits in the next scan range and its own lines may add false positives of the same kind.

Recommendation: no secret was found by these patterns; skim the 11 fixture and placeholder hits once by eye, then accept.

## Author metadata

Command: `git log c4692e074c8970081687d332adb64f200728426c --format='%an|%ae|%cn|%ce' | sort -u | wc -l` (and the pairs `%an|%ae` and `%cn|%ce` alone). Scan tip: `c4692e074c8970081687d332adb64f200728426c`, 3,124 commits.

- distinct author identities 1
- distinct committer identities 1
- combined 1

Described by role: the repository-local identity; every commit carries it. This page holds neither the name nor the address, by design.

Recommendation: the owner decides whether that identity is the one to publish under, and what identity later commits use.

## Personal-name check

Command: `python scripts/check-no-personal-names.py` at scan tip `c4692e074c8970081687d332adb64f200728426c`. Exit code 0, no findings in tracked public files.

Recommendation: keep the check as a commit gate; it passes.

## Name collisions (not a professional search)

Source: the screen recorded in D125 (`docs/decisions/process-and-staffing.md:128`), not queried again on 2026-11-08. Scan tip: `c4692e074c8970081687d332adb64f200728426c`. Free means free on the date given.

- NuGet package id Avowry: free on 2026-10-29, source D125.
- npm: free on 2026-10-29, source D125.
- PyPI: free on 2026-10-29, source D125.
- crates.io: free on 2026-10-29, source D125.
- GitHub handle: free on 2026-10-29, source D125.
- avowry.dev: free on 2026-10-29, source D125.
- avowry.io: free on 2026-10-29, source D125.
- Trademark databases and the earlier brand called Phrase: not reviewed: needs a professional search.
- Registry state after 2026-10-29: not reviewed: needs a fresh lookup on the day of publishing.

Recommendation: repeat the registry lookups on the publish day and commission a professional trademark search before the first publish.

## Owner items

Command: read from `NOTICE`, `TRADEMARKS.md`, D121 and D125. Scan tip: `c4692e074c8970081687d332adb64f200728426c`. Every item is open; none can be answered from the repository.

- Public name: OPEN, asked 2026-11-08. Confirm Avowry (D125).
- Holder line at `NOTICE:2` and `TRADEMARKS.md:5`: OPEN, asked 2026-11-08. Both still hold the marked placeholder.
- Professional trademark search: OPEN, asked 2026-11-08.
- Four questions for a lawyer (D121): OPEN, asked 2026-11-08.
  1. Do the generated runtime helper files need a notice, given that generated code belongs to the user?
  2. Is a trademark policy on this name enforceable?
  3. Does the rename rule conflict with Apache-2.0 notice retention?
  4. Who is the holder?
- Author identity going forward: OPEN, asked 2026-11-08.
- Unread vendor terms (four vendors): OPEN, asked 2026-11-08.
- The `reply_head` publication choice: OPEN, asked 2026-11-08. Publish, trim or drop the reply prefixes in the bench history files (up to 200 characters each).
- Transitive package licences: OPEN, asked 2026-11-08. Needs a restore.

Recommendation: answer the eight items before approving the first publish; the placeholders in `NOTICE` and `TRADEMARKS.md` stay until the holder is named.
