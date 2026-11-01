# Slice for card 227 (slice 1 of 1): recursive deletes on variable paths are blocked and every commit is pushed

Run key: baseline-slice-1. Seat: architect (slice). Source: `board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md`.

## Rules relied on (all in `TEAM_WORKING_AGREEMENT.md`, the only process context)

- `TEAM_WORKING_AGREEMENT.md`, Practices > Ceremonies, "One acceptance criterion per red-green cycle" (D157): numbered criteria, `Criterion: n` in each red body, green for n before red for n+1.
- `TEAM_WORKING_AGREEMENT.md`, Practices > Board and cards, "Thin slices" (D136): SHIP AS IS or SPLIT; one concern, at most 3 increments and 3 criteria; at most 15 countable files and 400 changed lines per card commit.
- `TEAM_WORKING_AGREEMENT.md`, "Slice size cap" (D168): at most 15360 bytes, ends with `Size: <N> bytes`, N measured with `head -n -1 <slice> | wc -c`.
- `TEAM_WORKING_AGREEMENT.md`, "Intent line before the first red" (D165), "Assumption line in the slice" (D166), "Hostile-input list in the slice" (D167).
- `TEAM_WORKING_AGREEMENT.md`, Engineering discipline: "Outside-in TDD with ZOMBIES" (D137: one acceptance test per criterion, committed `Status=Pending` with a `Criterion=NNN-K` trait, one pending at a time), "Push every commit" (F-071, card 227), "No Co-Authored-By trailers", "Stage explicit paths only".
- `TEAM_WORKING_AGREEMENT.md`, "Deletes: detect after the fact inside the working tree, never outside it" (D169) and Safety (variable-path recursive delete ban; the command scan's limits).
- `TEAM_WORKING_AGREEMENT.md`, Driver trust ladder, "Trust-ladder record (checked, from card 226)"; "Decisions stay reversible"; "Task force before every card" (one context never holds two roles).
- Card text: Context bullets (window 227-taskforce D153, adopted condition D145, ruling D169).

## Decision: SHIP AS IS (D136)

One concern (a variable-path recursive delete is seen, and an old unpushed commit is seen), 3 criteria (limit 3), 3 increments, one per criterion (limit 3). Each commit is planned well under 15 countable files and 400 lines. If the red for criterion 1 shows the command-source reader alone passes 400 lines, the architect re-splits criterion 1 into 1a (reader, Zero case) and 1b (equivalent forms) as slice 2, through the D153 window.

Reversal: each green is a plain addition or a rule text change; `git revert` of a criterion's green removes only that criterion. No one-way door (no publish, deletion or migration).

## Done checks, numbered (D157)

1. **Criterion 1: the scan sees recursive deletes on variable or computed paths in the commands a seat executes** (tool calls, through a hook or transcript), not only the reply text. The failing test comes first and reproduces the card 226 slip (`rm -rf` on a variable scratch path, reconstructed by QA from the recorded deviation, because it is not recorded verbatim), then `Remove-Item -Recurse -Force`, `shutil.rmtree` and equivalents. Adopted condition (D145): a destructive command absent from the reply text is still reported, and an empty or missing command-scan source fails (Zero).
2. **Criterion 2: a trust-ladder entry exists for the card 226 slip.** One line in `.mob/records/trust-ladder.jsonl` with seat, rule and `step` (down to rung N, or `none: <reason>`); `mob_process.py ladder <repo>` exits 0 with it and 1 without it.
3. **Criterion 3: push on every commit, with a detector.** The working agreement and the mob-defaults template carry the rule, and `mob_process.py unpushed <repo>...` exits 1 for a commit unpushed more than 300 seconds, naming repository, short sha and age; git only, no `gh`, origin never contacted.

Order 1, 2, 3. Criterion 1 is the open stop-the-line failure (audit 62); 2 and 3 are independent of it. No red for n+1 is committed ahead of the green for n, so no "ahead" exception is claimed. Each green is pushed at once.

## Intent line (D165: criteria implement a finding and a product owner direction)

Intent: the product owner wants no seat able to delete a repository through an unseen recursive delete on a variable path, and no work more than about 5 minutes unpushed, because a mis-quoted delete like the 2026-10-08 home-folder wipe could take out a repository including `.git` and every unpushed commit (product owner safety ruling 2026-10-31, "one corrective card covering the scan fix and push-on-every-commit"; finding F-071 / audit 62; D169, "detection after the fact is acceptable ... commit and push regularly"). Traced to: card 227 Source and Context bullets, D169, F-071.

## Assumption line (D166: this card changes a rule and a checker)

Assumes: every executed command of every Claude seat is readable from a transcript or hook source the record check can name, and a seat with none is a FAIL (`no command source`), never a pass; a commit's age is its committer time, or the earliest reflog entry when older, and 300 seconds passes while 301 fails; probe: `scripts/delete_guard.py` `find_unsafe` (line 145) and `scripts/check-unsafe-deletes.py` as they stand, and `.mob/records/trust-ladder.jsonl` (2 lines at slice time, one for card 226 whose `step` begins "none:"), read at slice time but not yet run on the reconstructed command. Until the red seat runs `find_unsafe` on the reconstructed card 226 command and puts the real result in the C1-red body, this line is unprobed against the real result, and review checks it first.

## Hostile-input list (D167: criteria 1 and 3 take a list of names)

Criterion 1 (list = command forms and scan sources):
Inputs: disabled: a seat whose command source is off or missing, expect FAIL `no command source`, not silence; unknown: a delete verb or wrapper the scan does not know (`cmd /c rd /s`, `bash -c "rm -rf $X"`, `pwsh -Command`, `python -c "shutil.rmtree(p)"`, `find -delete`, `del /s`), expect reported, or named under "Not covered" with the D169 limit that makes it invisible; repeated: the same delete twice in one transcript, expect one finding per occurrence with the seat named, no crash and no suppression of the second; reordered: flags around the path (`rm -fr`, `rm -r -f`, `rm $P -rf`, `Remove-Item -Force -Recurse $P`, `-Recurse` after the path) and a literal `safe_delete` call before a variable one, expect the same verdict in every order.

Criterion 2: no list input.

Criterion 3 (list = repositories given to `unpushed`):
Inputs: disabled: a repository outside `MOB_WATCHED_REPOS` when it is set is not checked, and with it unset the seat's repository and mob-defaults are; unknown: a missing path, a plain folder, and a repository with commits and no `origin` are FAIL, not skipped; repeated: the same repository named twice, or reached through a worktree, is reported once per repository and commit; reordered: swapping the repositories changes no verdict and no exit code.

## Commit plan (D136 limits; D137 pending and green mechanics; D157 order)

Countable files exclude `board/`, `docs/decisions/`, `MOB.md`, `.mob/records/`, `docs/retros/` and generated fixtures. Likely touch points (from the repo as it stands, may shift): `scripts/delete_guard.py`, `scripts/mob_process.py`, `scripts/check-unsafe-deletes.py`, `tests/test_mob_process.py`, one new test file for the delete scan.

Cycle 1, Criterion 1:
- C1-red: one Pending acceptance test, `Criterion=227-1`: a transcript holding the card 226 command, absent from the reply text, makes the record check fail naming command and seat. Body: `Criterion: 1`, the intent line, one answer per entry of the criterion 1 Inputs line (test name or `n/a: <reason>`). About 1 file, under 120 lines.
- C1-u1: unit, reader returning a seat's executed commands; Zero case (empty or missing source fails); file system as test double. About 2 files, under 120 lines.
- C1-u2: unit, `find_unsafe` over executed commands: the exact card 226 form first, then Remove-Item -Recurse -Force, shutil.rmtree, reordered flags, wrapper forms. Invariant test: a literal path inside a scratch root through `safe_delete` is NOT reported. About 2 files, under 160 lines.
- C1-green: wire the reader into the record check and remove `Pending` in the same commit; push at once.

Cycle 2, Criterion 2:
- C2-red: Pending test, `Criterion=227-2`: `mob_process.py ladder` on a fixture with a card 226 safety deviation and no line exits 1, with the line exits 0. Body: `Criterion: 2`, intent, `no list input`. Under 60 lines.
- C2-green: the trust-ladder line for the slip (record data, not countable) plus any check fix the red exposes; remove `Pending`; push at once. The `step` is decided by the reviewer from the ladder rules (the ledger says Claude is already at Rung 1, so `none: <reason>` is expected), not fixed by this slice.

Cycle 3, Criterion 3:
- C3-red: Pending test, `Criterion=227-3`: a repository with a 301-second-old commit and no upstream makes `unpushed` exit 1 naming repository, short sha and age; 300 seconds passes. Body: `Criterion: 3`, intent, answers to the criterion 3 Inputs line. Under 120 lines.
- C3-u1, C3-u2: units, age from committer time or earliest reflog entry; the FAIL cases (future-dated commit, missing path, no origin, non-zero git exit); `MOB_WATCHED_REPOS` selection; git as test double. About 2 files, under 200 lines together.
- C3-green: the `unpushed` subcommand; remove `Pending`; push at once.
- C3-docs: rule text in the mob-defaults template (card 216 consumes it). This repo's agreement already carries "Push every commit", so that part is checked, not rewritten. Text only, no flag (atomic document change).

Total: about 10 code commits, each under both limits. No `Co-Authored-By` in any body; explicit paths only; nothing under `.mob/` staged except the records the process names.

## Open points the card handed to the slice

- Exact slip not recorded verbatim: QA reconstructs it from the card 226 deviation (recorded in 9f42b70 per the ledger) and shows the reconstruction in the C1-red body.
- Trust-ladder record location: `.mob/records/trust-ladder.jsonl`, append-only, in this repository.
- Agent-tool sub-agent seats: scanned by hand until card 228; they report `no command source`, and the Zero case makes that a FAIL (the adopted condition).
- D169 condition 2 (no recursive delete outside the working tree) belongs to card 228.

## Not in this slice

The reviewer's commit-message claim check, process-kill rules, blocking before execution (card 228), `gh` calls in the push detector, any product feature or flag.

## Review order

1. Size line re-measured with `head -n -1 | wc -c` (D168).
2. Assumption probe (D166), unprobed against the real result until the C1-red run.
3. Inputs lines against the red bodies (D167); an unanswered entry or bare "n/a" counts as not run.
4. Intent line traced to its decisions (D165).
5. `Criterion:` order from `git log` against 1, 2, 3 (D157).

Size: 10847 bytes
