**Status: from the owner's dispatch audit of 2026-11-15 ("are we still rotating and assigning models correctly?").** It narrows #53's [M][S] scenario to the seat-use writer, and it touches #58.

**Findings, measured on Avowry's `.mob/records/seat-use.jsonl` (2,633 rows):**
- **No model data.** Every row has only `seat`, `role`, `context`, `card`, `ts` and `probe_taken_at`. Models live only in the per-card `model-allocation.md` files, so the model-selection guide cannot be checked from data.
- **Lines are written before the seat runs.** `decider.py` `start_voters` calls `check_voters` and then `commit_voters`, so every voter's line is in the log before any voter launches. Nothing records the outcome afterwards.
- **Rotation reads those lines.** The D164 rule in `CardRun.check_seat` reads the history, so a seat that then fails (402, quota, timeout, a fallback) still counts as having served.
- **Avowry's 2026-11-14 grok rows show this:**
  - Of the 7 grok rows, 5 match counted D258 ballots (all Yes, reasons cited).
  - 2 (18:06:48Z and 18:06:55Z) come from a run that left no counted ballot.
  - Cursor's rows at 18:29Z were written although its clone timed out and Claude was seated instead.
  - Avowry has no grok row since then (grok probes `unavailable`, 402, on 2026-11-15).
- **Play-tracker's own log already has the fields:** `docs/model-allocation/seat-log.jsonl` carries `service`, `declared_model`, `declared_reason`, `actual_model_by_wrapper_config`, `model_reported_by_cli`, `work_type`, `outcome`, `duration_s` and `signals`. Its 17 grok rows are 13 `success`, 3 `quota_exhausted` and 1 `timeout`.

## Persona

Owen

## Story

As Owen, I want every new seat-use row to carry its service, its declared model or size, its work type and its outcome, and rotation to count only seats that served, so that the model-selection guide can be checked from data and a failed seat never counts as a turn (goal: trust a mob's done without re-checking it).

## Acceptance criteria

- Scenario: [Z] Given a history written before this card (no model fields), when rotation or a review reads it, then the old rows still parse, their model shows as unknown, nothing is backfilled, and they count exactly as they do today.
- Scenario: [O] Given one seat launched through `CardRun.commit_seat`, when its line is written, then it carries `service`, `declared_model` (or the size tier), `declared_reason` and `work_type` beside today's fields, using play-tracker's seat-log names, and `outcome` is `launched`.
- Scenario: [M] Given a Decider run of six voters, when each voter finishes, then an outcome line for that seat is appended: `success`, `quota_exhausted`, `timeout`, `refused`, `no_vote` or `fallback`, with `duration_s` and the actual model the wrapper used (and the model the CLI reported, when it reports one).
- Scenario: [B] Given a seat whose outcome is anything but `success`, when the D164 rotation rule decides who has waited longer, then that seat's launch does not count as a turn, and a seat with only a `launched` line from a run that never wrote an outcome counts as not served.
- Scenario: [I] Given the extended rows, when `mob_process.py check` and the existing seat-use readers run, then they accept both the old and the new row shapes.
- Scenario: [E] Given a seat with no declared model (an unpinned launcher), when its line is written, then `declared_model` is `undeclared`, and the line is still written: the gap is visible, not hidden.
- Scenario: [S] Given the change, when it ships, then every test is written red first, and the new fields need no change to any mob's records except new rows.

## ZOMBIES coverage

- Z (Zero): covered
- O (One): covered
- M (Many): covered
- B (Boundaries): covered
- I (Interface): covered
- E (Exceptions): covered
- S (Simple): covered

## Dimensions of Difficulty

- Environment: no
- Scope: no
- Unknown: no
- Communication: no

## Links

- Parent epic: none
- Spikes: none
- Related: #53 (the shared per-seat log format), #58 (rotation counts only seats that can be seated now), #60 (the CLI-reported model), play-tracker docs/model-allocation/seat-log.jsonl (the field names)
