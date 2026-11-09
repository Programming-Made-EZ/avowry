# Card 361 fresh sign-off (fresh Claude context, separate from the gate reviewer; own full gate in its own clone)

**SIGNED OFF WITH CONDITIONS.** The first two conditions are for card 162, the third is for the retro. None blocks card 361 itself.

## Gate evidence
Tested commit `817befcce317` (main), which contains the green `8ed59b1` (`merge-base --is-ancestor` exit 0). Copy: a fresh `git clone --no-hardlinks --single-branch -b main` into `signoff361-clone`; the tree was clean apart from the seat's own `tmp-signoff/`. Dotnet processes at start: 1 (already running, not the seat's).

## Commands and results
1. `dotnet build src/Avowry.Cli`: 0 warnings, 0 errors.
2. First `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`: Compiler 344/344; Acceptance 375 of 376, with `BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms` failing. This run was contaminated by the seat's own mistake: the tool backgrounded the long run and the Python suite was started beside it.
3. That test rerun alone: Passed, 9 s.
4. The whole gate rerun alone: Compiler 344/344; Acceptance 376/376 in 9 min 29 s. `WebExtensionBoundary` (orders hash `19fe6d71…`) passed.
5. `python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map tests.test_getting_started tests.board.test_story_persona` alone: Ran 78 tests, 8 failures, exactly the expected set (`test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`, `test_lists_equal_baseline` x2, T11).
Load line: the first run overlapped with the Python run (the seat's error; the flake appeared in it); the second full gate and the Python suite each ran alone, one dotnet process before and no other work beside them.

## CLI checks (CLI built from the clone; tiny sources in `tmp-signoff/`)
Duplicate avow: exit 1, stderr `avowry: Compilation failed:` then `AV010 (7:1) Avow 'ping {m: Int}' has the same wording as 'ping {m: Int}'.` Tab-indented avow: exit 1, `AV100 (4:1) Indentation must use spaces, not tabs.` Valid source: exit 0, empty stdout and stderr; output has `M.Api`, `M.Specs`, `M.avowrymap.json`; a grep for the old prefix over the whole temp folder returns 0 hits; the map and the trace command print no diagnostic codes.

## Judgement against the done checks
1. Criterion 1 holds (CLI output, the nine C-rows, `CodesKeepTheirNumbers` 20 of 20).
2. Criterion 2 holds: outside history, `git grep` over tracked files returns only `.mob`, `docs/decisions`, `docs/retros`, `bench/results`, `board` and the generated `MOB.md`; README, GRAMMAR, CODING_GUIDELINES, the cheat sheet, patterns and traceability have no old-prefix code or wording. GRAMMAR line 162 reads "`AV300` to `AV335`, plus `AV337`, `AV340` and `AV341` from the C# target"; CODING_GUIDELINES line 19 reads "Diagnostics keep the existing `AVnnn` code style".
3. Criterion 3 holds: the live-tree guard (L1 to L7) passes; mutations d1 and d2 were killed in the gate review; the orders hash is unchanged and pinned.
4. History untouched: retros 001 and 002 still say `PH312`, `PH335`, `PH334`; D229, the card 203 retro and the INDEX state the rename.
5. Dana and Ari: every published code (CLI stderr, the 56-row grammar table) carries the AV prefix, no alias; no consumer-value gap under the Value rule.
6. Scope: numbers and messages unchanged (the gate review's prefix-only normalisation of `src/` found no other change).

## Conditions
1. Card 162, block re-freeze: `docs/usable-v1/llm-check-codex.json` and `llm-check-grok.json` pin `grammar_sha256` `ad0f6e51…`; GRAMMAR.md at the green's parent hashes to `88746d6b…` and at the tip to `8e4f91bd…`, so those pins were already stale before this card; the re-freeze must regenerate them and bar 6 (Pending) needs that. GRAMMAR.md has no single sentence that says the codes use the `AV` prefix (the table header is just "Code"): card 162 should add one line stating the convention.
2. Card 162 or a doc card, history wording: old retros, decision records and the generated `MOB.md` (lines 18 and 19) still say `PH` with no "formerly PH" pointer except in D229; a one-line note in the retros or decisions index would remove the confusion. `TEAM_WORKING_AGREEMENT.md` line 434 (a ladder row about card 001) now says `AV312` while retro 001 for the same event says `PH312` (the facilitator's ruling chose this; it rewrites a historical fact in a living file).
3. Retro risks: the regex narrowing in `tests/test_getting_started.py` is defended by no committed test (mutation f survived); the adapter code numbers (AV020, AV337, AV340, AV341) rest on the CLI rows alone; the scanner reads disk rather than committed blobs; the benchmark-harness timeout test is sensitive to load (gate runs must stay alone); the tool backgrounds any run past 180 s, which makes it easy to start a second job by accident (a Monitor-style wait is the safer pattern).

Leftovers: the clone `signoff361-clone` (with untracked `tmp-signoff/`) and two logs next to it (no recursive delete).
