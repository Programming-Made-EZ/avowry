# Card 179 cold-reader report (D206): Muse, a trial driver's first cold-reader seat, reading only README.md, docs/GETTING_STARTED.md and docs/PREREQUISITES.md as Dana (the decider's adopted condition names the third file)

Reader: muse exec --prompt-file, in a scratch directory holding only the three files from commit ced00e3 and the task (scratchpad coldreader179/TASK.md); read-only; about 1 minute.

## Reply, verbatim

(1) Yes. Avowry is a language for describing software as intent: each avow is one line, names are descriptions, specs are the truth. It is for Dana, who directs an agent and must judge the result without reading C#. Aims: every spec becomes a running test; each avow preserves its intent so the reason survives code; changes are checked against the spec instead of line by line. By C# adapter preview I mean an early release with only a C# FastEndpoints adapter, turning a .avowry file into a running API and tests.

(2) Get it by cloning https://github.com/Programming-Made-EZ/avowry.git. First check prerequisites for the .NET 8 SDK, then follow GETTING_STARTED from clone to a running API.

(3) On Windows it is mostly followable: steps state where to run, what they need, and PowerShell forms for pack, install, build, test, run and calls. Order is otherwise sound. I would stop if dotnet or git is missing, as no install source is given; guess when the API is ready, as no startup output is shown; and puzzle over first versus second windows, since only the second is introduced. Rerunning into same TEMP folders is unexplained.

(4) WRONG: README points to docs/PREREQUISITES.md and docs/GETTING_STARTED.md (README.md:24), while GETTING_STARTED links to sibling PREREQUISITES.md (GETTING_STARTED.md:5); one path is wrong. It also claims every command has both forms (GETTING_STARTED.md:3), but two just say Same in PowerShell (GETTING_STARTED.md:16). CONFUSING: Dana stays free of any target (README.md:13) yet only C# ships (README.md:5); Windows is presented as covered (GETTING_STARTED.md:3) but any non-ubuntu-latest OS is untested (PREREQUISITES.md:9). Link text says docs/PREREQUISITES.md but targets PREREQUISITES.md (GETTING_STARTED.md:224). MISSING: where to get .NET 8 SDK or git, a passing SDK check in PREREQUISITES.md:23, API-ready output, port and rerun help. FINE TO LEAVE: harmless pack warning (GETTING_STARTED.md:45), old Phrase name (README.md:9), unwritten limitations note (README.md:27).

(5) The newer-SDK message is clear but action is not: newer SDK or runtime does not replace .NET 8 (PREREQUISITES.md:11, PREREQUISITES.md:13), yet it never says install 8 side by side, where to get it, or what passes its check.

## Disposition (facilitator)

Real, fixed in the page-only revision (green seat, Claude): (1) no source for the .NET 8 SDK or git; (2) no sign that the API is ready; (3) first versus second window; (4) the newer-SDK sentence says what it does not do but not what to do; (5) link text docs/PREREQUISITES.md against target PREREQUISITES.md in the guide; (6) 'every command has both forms' against 'Same in PowerShell.'.
Misread, no change: 'README points to docs/PREREQUISITES.md while the guide links PREREQUISITES.md; one path is wrong' (both are correct relative paths from their own folders; the test resolves each).
Kept as is: 'Dana stays free of any target yet only C# ships' (the product owner's wording of 2026-11-08: Avowry is its own language, this preview ships one adapter); 'Windows is untested' (true and stated; the guide gives a Windows path, not a Windows test claim); the harmless pack warning, the old Phrase name, the unwritten limitations page (card 187).
Not fixed here: help for rerunning into the same TEMP folders beyond the one sentence the revision adds.
