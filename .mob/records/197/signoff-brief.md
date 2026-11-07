# Card 197 sign-off brief (a fresh Claude context that wrote no part of the card; READ-ONLY on C:\Users\ringe\projects\phrase-lang)

You are the final sign-off. Do not edit, create or delete anything under that repository; do all runs in scratch copies in your own temp directory. Reply in text (under 800 words); the facilitator commits it as signoff-ctx6.md. Label each line rerun, record-only or blocked, and give a Load line for each full dotnet run (D202).

Read the card (`gh issue view 197`), .mob/records/197/preship-gate.md (the gate: SHIP), green-record-1.md, local-windows-run.md, red-record-1/2/3.md, redfirst-finding.md and redfirst-run-output.txt, and docs/decisions/process-and-staffing.md sections D202 and D203. The commit under sign-off is dc1193f (code and tests); later commits add records only.

Verify the card's Done-when and its seven scenarios by your own commands (a `git clone --no-hardlinks --single-branch -b main` for history checks, `git archive` copies for the rest):
- [Z] no old build-graph name remains (the scan; exact-case tracked paths);
- [O] the Bench tool builds and its self-check exits 0 (`dotnet run --project tools/Avowry.Bench -- --self-check`, about 6 s; it writes under bench/results/self-check, so run it in a copy);
- [M] the solution names the same seven projects and three solution folders with their original GUIDs (compare with baseline/project-lines.txt after the rules) and each test list equals its baseline name for name;
- [B] byte exactness: recompute all 33 changed files from the old bytes at 47ddda1 with the four rules and compare with 2d58c6c (python, bytes), including the CRLF files; the parity test in the clone;
- [I] ci.yml, README, HANDOFF, the agreement's gate commands, run-llm-check.py and RepoPaths name tracked files; the path-bound python modules and the ADR/script/test triple agree;
- [E] the old command `dotnet test Phrase.sln` is refused; apply 3 mutation patches of your choice from .mob/records/197/mutations (use `git apply --index` in a clone, a patch adding a new file is only seen by tracked-file tests when staged) and show the named test fails;
- [S] CI on the pushed commits is green (use `gh run list`) for `test`, `usable-v1` and `mob-process`, and the Windows gate totals equal the baseline (local-windows-run.md vs baseline/gate-run-before.txt).
Run `python -m unittest tests.test_rename_197` (19), `tests.test_rename_196` (15) and the path-bound modules (100) in the clone.
Also run, ONCE, a full gate: wait first for the file C:\Users\ringe\AppData\Local\Temp\claude\1ff08f82160bcdeb\2ca12683-042f-40b7-b61a-623ae180176b\scratchpad\push-quiet197.done (the facilitator's push is using the machine; poll with sleep 30, up to 25 minutes; the facilitator will have nothing else running after it), then run `dotnet test Avowry.sln --filter "Status!=Pending"` in a clean archive copy in the background (about 10 minutes) and report totals and failing names against the baseline (284; 313 passed with 3 known failures) and whether the card 304 timing guard failed.

RED-FIRST RULE (product owner ruling of 2026-11-07, now permanent): form your own judgment of redfirst-finding.md: are its facts right (re-run its command against 21a2caa in a scratch clone if you can: copy tests/test_rename_197.py from dc1193f into a clone at 21a2caa and run the ListsAndPaths and OldCommandRefused classes), is the classification (8 new behaviour, 4 characterisation) correct, and does criterion 1 itself satisfy the rule (red 1 watched failing before the green)? Say whether this finding blocks the sign-off or is a retro finding only (the ruling says: a finding raised at the retro, never accepted silently, reported to the product owner), and why.

Verdict: SIGNED OFF, SIGNED OFF WITH CONDITIONS or NOT SIGNED OFF, one paragraph. State any promise of the card that the evidence does not show, and anything for the retro.
