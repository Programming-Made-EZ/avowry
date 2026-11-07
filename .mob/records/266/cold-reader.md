# Card 266 cold-reader seat (a fresh Claude context named Ari, no prior context; read only README.md and demo/tasks/README.md; ran nothing, edited nothing)

Isolation and Load: the seat ran during the facilitator's push and gate window; it used only the Read tool on two files (no command, no dotnet), so it added no measurable machine load. The seat verified nothing against the repository (by design). The facilitator's classification of each finding follows the report.

**Name Ari would search for:** Avowry. Root line 7 says it was named Phrase until 2026-10-29, so Avowry is current; the only other name is Phrase, the old name. The demo README never says which is current and still carries old-name traces (see WRONG 2).

## WRONG (a clear error)
1. Root lines 40 and 41 conflict with demo/tasks/README.md lines 21 to 42. The root README says the compile test stops with `NotImplementedException` at the resolver and marks the resolver pending and in progress; the demo README tells the reader to run `build` on `Tasks.avowry` and get 11 passing specs. Both cannot be current. The root README never mentions `demo/`.
2. demo/tasks/README.md lines 124 and 125 link to files named `...-in-phrase-ensure-...`: the old product name inside card file names, in a document that otherwise says Avowry. The reader cannot tell whether the files were renamed.

## CONFUSING (could mislead)
- Root line 5: the "avowry command" and "still uses" the `.avowry` extension: "still" hints the extension may change; nothing explains how to get an `avowry` command on PATH (root line 21 and demo line 21 use `dotnet run --project src/Avowry.Cli -- ...`).
- Root line 21 lists `trace` as a CLI command; it is never documented in either file.
- Root line 7: "older records keep the old name" does not say where those records are. Root lines 41 and 42 use `PH100` to `PH211` diagnostic codes: `PH` looks like the old name and nothing explains the prefix or whether it will change.
- The root Layout (lines 17 to 26) omits things other lines use: `demo/`, `board/`, `docs/PATTERNS.md`, `docs/adr/`, `scripts/`, `Avowry.sln`, `VISION.md`, `HANDOFF.md`, `LICENSE`, `NOTICE`, `TRADEMARKS.md`.
- Root line 39 runs `python scripts/check-readme-reference-status.py` without saying where to run it or that Python is required; it also says a newer ASP.NET Core runtime alone is enough with roll-forward while demo line 11 requires the .NET 8 SDK.
- Root line 18 calls the grammar "v0.1"; demo line 3 calls the demo "Usable v1 prototype": language versions or milestones?
- Demo heading "Output folder" (line 16) covers the whole build, test and run procedure; line 18 says "a git-ignored path" but names none; `/tmp/avowry-tasks` may not resolve for native Windows `dotnet` under Git Bash.
- Demo line 24 names `Tasks.Api` and `Tasks.Specs` without saying where the names come from.
- "Card 159" and "Card 160" (demo lines 124 and 125) and "the board" are never explained; line 120 says completing a task "is now" an Avowry update while lines 122 to 125 list the same area as still to do.
- Demo line 68 is ambiguous ("without it" for the `available` parameter); line 116 lists `TitleTooLong`, which no example triggers; line 120 says five `native csharp` blocks remain and the list is not inline.
- "Mob" in root line 13 is unexplained; root line 58 says to remove the "Pending trait" while line 32 filters on `Status!=Pending`.

## FINE-TO-LEAVE
Test counts (lexer 51, parser 79, formatter 8, resolver 83) and the spec counts may go stale; the layout row repeats counts that the table has; the `curl` examples, port 5099 and "Registered 5 endpoints" are consistent.

## Facilitator's classification
- **WRONG 1 (stale status and missing demo pointer in the root README):** outside this card's three criteria, inside its story ("a README that describes the repository as it is"): a new Draft card (README status, prerequisites and demo pointer describe what runs) is filed for it; not a finding against this card's evidence.
- **WRONG 2 (`in-phrase-ensure` file names):** a pinned exception (a real tracked history path the doc test requires to exist); renaming the history file is out of scope (decision 1: history stays verbatim); the README note already says older records keep the old name.
- **CONFUSING items about names:** the `PH` prefix (decision 3, a later owner ruling), "still uses" (stale wording left by the README's own earlier sentence, now only the shorter sentence remains: the extension is `.avowry`), the note not saying where records are (candidate: one sentence naming `board/` and `.mob/`; left for the new README card); the other items (prerequisites, versions, layout completeness, ambiguity in the demo steps) are about the README's accuracy and belong to the new README card and to card 266's retro as evidence that the Layout block is still incomplete.
- The seat found no remaining wrong product name, path or command in the two files beyond the pinned history links, which is what criteria 1 and 2 promise.
