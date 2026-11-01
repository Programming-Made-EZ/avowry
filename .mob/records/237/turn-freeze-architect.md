# Card 237 step 0c: task freeze (architect, ctx-2)

Intent (D165): freeze the six role task texts and the arm list before any key or run exists, so the same prompt goes to both arms and the miss-key writer (step 0d, not ctx-2) writes against fixed texts (window 237-slice, condition 1).

Assumes (D166): card 227 (closed, Done) has a board file whose text alone is enough to give each of the six steps a real task with no product change; probe: board/227-*.md carries the Done checks, adopted condition, task-force roles and open points each task asks for.

Hostile inputs (D167):
- Task text edited after the freeze: sha256 of each file's bytes is in freeze.json; any later change to a task or to freeze.json is also caught by the checker (changed at or after the first run).
- Arm list: unknown, repeated or disabled arm. Only baseline and wiki exist; each appears once; neither is disabled.
- Task order: reordered. role_order is fixed (ready, slice, red, green, ship, signoff) and each task entry carries its role name.
- Leaked answer: a task naming a must-read rule or reading the real card 227 records. No task names any rule; each forbids reading .mob/records/227*, .mob/records/decisions/227* and card 227 git history. Placeholders are only {RUN_KEY}, {ARM_CONTEXT}, {OUTPUT_PATH}.
- Arm difference: only the {ARM_CONTEXT} line differs, and its two texts are in freeze.json.

Source card: 227 (recursive deletes on variable paths are blocked and every commit is pushed), Done 2026-11-01. Chosen because its board text has three Done checks, an adopted condition, an eight-seat task force and open points, which suit all six steps. No task runs a command or changes product code; each run writes one markdown file at its output path.

Files: .mob/records/237/trial/tasks/{ready,slice,red,green,ship,signoff}.md and .mob/records/237/trial/freeze.json (author_seat ctx-2, arms, role order, per-task sha256).

Checker (`python scripts/check-wiki-trial.py`, exit 1, as expected: only 0d and later files missing):
```
.mob/records/237/trial/miss-key.json: missing file
.mob/records/237/trial/ledger.json: missing file
.mob/records/237/trial/recompute.json: missing file
```

Commits: 01b3058 (six tasks and freeze.json, pushed; pre-push check OK).

Note for step 0d: the key writer must not be ctx-2 (freeze author) or ctx-5.

Size: 9147 bytes (six task texts plus freeze.json)
