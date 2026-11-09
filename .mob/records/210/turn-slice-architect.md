# Card 210 slice: the card record check rejects an empty task force and empty probes

Seat: architect (slice), Claude ctx-2, fresh context. Change repo: mob-defaults (C:\Users\<user>\mob-crosscli), mob_process.py and its tests. First slice under D175. Decision: SHIP AS IS under D136 (one concern, three increments, three criteria; a red that would pass 400 lines is split by test file, never by criterion).

Intent (D165): the owner wants the record check to stop trusting a value just because it is present. Card 210 Observes: "it does not trust a truthy value alone". Source: audit finding MD-003 (audit 17, mob-crosscli 687410e..586f15e) and F-043; D147 lets the card start under an open FAIL because its issue names MD-003.

Assumes (D166):
- A1. check_record has two callers, mob_process.main and facilitator.record_check, and both hold the card's MOB.md; probe: grep of mob-crosscli, and check_record run over the 11 closed records of avowry (.mob/records 195 to 237): every one raises AttributeError today (schema 1: seats, retro as a string), so none is fed to it.
- A2. The facilitator writes probes as {taken_at, results{name:{ok,evidence}}} for EVERY backend in core/backends.json (codex, copilot, grok, agy, claude), a disabled or out-of-quota one included, with one record-level taken_at and no per-model time; probe: facilitator.probe_drivers, and the real line in .mob/records/probes/2026-10-30.jsonl (grok, 2026-11-02T12:43:59Z: available false, exit 1, evidence kept).
- A3. A task_force entry from write_record has role, assignee, context, why (optional joined, removed, decider, quota_reason) and NO domain key; the domain is named in the role or the why (templates/TASK_FORCE.md); probe: facilitator.write_record.

## This card's domain-expert question (T2)

The PM proposal's T2 (the real command source per backend) was copied from card 228 and is not asked here. This card's T2:

Question: which fields of a record does the facilitator itself write from what it observed, so which new refusals would reject a record it wrote honestly?

Answer, from the code (no new member needed): write_record writes `cycles` as max(old, cycles_run) with no log behind it, `task_force` entries with no `domain` key, and `probes` as in A2 (claude and disabled backends included, one taken_at). So: (1) "each probe with a timestamp" means the record-level taken_at, or a per-model `probed_at` when present; claude needs no result but its presence is no error; (2) "a domain" means the role or why names it: non-blank role and why, and a `domain` key only if present; (3) a cycle count must come from MOB.md, which the facilitator writes: `#### Cycle N roles` in mob mode (run_mobbed) and, new in criterion 2, `#### Cycle N` in the sequential loop. Anything stricter would refuse the facilitator's own honest record.

## Invariant and the records that must keep passing

A record that passes today (full task force, four probes, MOB.md or seat log, complete retro) still passes. Findings gate, commit rules, trust ladder: untouched.
- Must keep passing: good_record() in tests/test_mob_process.py (27 uses), via tests/test_command_scans_record.py and test_command_scans_record_strict.py, and the records the facilitator writes in tests/test_card_mode.py (the quota_reason cases near lines 799, 894, 933). No complete record is committed in mob-crosscli itself (.mob there holds 036 and 063 run folders only).
- One fixture change, named: good_record has ONE probed model (codex), so it is not a "four probes" record. Red 1 upgrades it to codex, copilot, grok, agy plus claude: a fixture change, never a weakened check. A test_card_mode fake backend list that lacks one of the four is fixed the same way; a failing green is never answered by loosening a rule.
- Back-compat door: check_record gains `seat_log=None`. When mob_text and seat_log are both None, no evidence was supplied and `cycles` is read as today, so the 27 unit callers pass. main and record_check always pass evidence, so in production the self-reported field is never the answer. Named gap: a direct caller can still skip evidence (follow-up F1).
- Avowry's closed records (195, 205, 208, 209, 214, 219, 226, 227, 228, 234, 237) and its checker (scripts/mob_process.py check_records: schema, card, title, process, seats, retro, notes): this card edits only mob-crosscli, imports nothing from avowry, and avowry's checker never calls check_record. Probe run now: `python scripts/mob_process.py check` in avowry prints OK (102 closed cards, 11 records, 91 legacy). The slice avoids a break by touching neither side. Also seen: card 210's own record-in-progress.json holds "cycles" as a LIST, and today int() raises TypeError; criterion 2 turns that raise into a refusal.
- Red guard (criterion 1): check_record never raises on a task_force or probes of the wrong type (string, list, number); it refuses.

## Criterion 1: non-empty task force and probes (Done check 1)

Cap: criterion 1, at most 1 cycles

Behaviour: with the key present, `task_force` must be a non-empty list of objects (`task_force: empty, a card needs a declared task force`; `task_force: must be a list`). `probes` must be an object with a parseable `taken_at` and a `results` object, and each of codex, copilot, grok, agy needs an entry with a non-blank string `evidence` and a bool `ok`; a `probed_at`, if present, must parse. A failed probe (ok false) still covers the model: it was tried and its evidence kept. Messages name field and model: `probes: no result for grok (codex, copilot, grok and agy each need a probe)`. A missing key stays the existing "no '<section>' section". The stale rule is unchanged.

ZOMBIES (acceptance, through `python mob_process.py record <repo> <card>`: exit 1 and the message):
- Z: task_force [] and probes {}: both refused, each by name.
- O: one model probed: the other three named, one message each.
- M: all four present (claude present or absent): no probe problem.
- B: three of four: the fourth named; an entry with an unparseable time: refused; evidence "" or "  ": refused; ok "yes" (not a bool): refused.
- I: probes a list or a string, task_force a string: refused as the wrong type, no exception.
- E: the stale rule still fires at 61 minutes, not at 59 (existing test).
- S: N/A, covered by test_a_complete_record_has_no_problems.

Outside-in plan: acceptance test = CLI on a tmp repo record (subprocess, real files). Layer 1: main() reads and prints (no double needed). Layer 2: `_task_force_problems(task_force)` and `_probe_problems(probes)` with the clock as an argument (double: fixed `now`). Real layer last: the existing stale check. Unit ZOMBIES per helper: empty, one, many, boundary, wrong type.

Inputs (D167), the probed names:
- disabled: SEEN (probes/2026-10-30.jsonl, grok 2026-11-02T12:43:59Z, available false): an ok:false entry with evidence covers the model.
- unknown: SEEN for the claude key (A2; core/backends.json lists it): allowed, not required. INSURANCE for any other name (gemini): ignored, no error.
- repeated: INSURANCE (a JSON object cannot repeat a key; "Codex" beside "codex" has no real occurrence): the capital key does not cover codex.
- reordered: SEEN (card 226 review F2, recorded in D167: a model map tied to the probe by key order): reversed key order, same verdict.

## Criterion 2: cycles are derived from evidence (Done check 2)

Cap: criterion 2, at most 1 cycles

Behaviour: `cycles_from_evidence(mob_text, seat_log)` returns None when both are None, else the largest N in MOB.md lines `#### Cycle N...` and the largest integer `cycle` on a seat_log line (lines of another card ignored; main filters .mob/records/seat-use.jsonl by the record's card), else 0. With evidence, the domain-expert boundaries `cycle 1` to `cycle N` use the DERIVED count, never the field. A field present and different from the derived count is refused: `cycles: record says 0, MOB.md shows 2`. Derived 0 with a field of 1 or more: `cycles: 2 claimed, none found in MOB.md or the seat log (unverifiable)`. A field that is not an integer (list, string, bool) is refused naming its type, never a raise. Facilitator, one line (the only non-checker change): the sequential loop writes the status heading `Cycle N` at the start of each cycle, since it writes none today.

ZOMBIES:
- Z: no cycles field, MOB.md shows cycle 1, answers only at refinement: cycle 1 named.
- O: one cycle in MOB.md, field 1, answers at refinement and cycle 1: passes.
- M: three cycles, answers at cycle 1 and 2 only: cycle 3 named.
- B: field 0, MOB.md shows 2: refused with both numbers; field 3, MOB.md shows 2: refused.
- I: seat_log lines with `cycle` 1 and 2 and no MOB.md: derived 2; a line of another card is ignored.
- E: evidence supplied but empty and field 1: unverifiable; field a list (card 210's own record): refused, no exception.
- S: N/A, the single-cycle case is O.

Outside-in plan: acceptance = CLI on a tmp repo with .mob/NNN/MOB.md and a record (subprocess). Layer 2: `cycles_from_evidence` and the boundary list in check_record (doubles: strings and lists of dicts, no files). Layer 3, real: the facilitator heading, driven by an existing test_card_mode run (fake CLI) asserting `#### Cycle 1` in MOB.md.

Inputs (D167), the seat-log lines:
- disabled: n/a: a log line has no enabled flag; backend state is criterion 1's.
- unknown: SEEN (.mob/records/seat-use.jsonl: 476 lines, all card "decider", none this card's): lines of another card are ignored.
- repeated: INSURANCE (no real line carries a cycle field yet): cycle 1 on four lines counts as 1, not 4.
- reordered: INSURANCE: cycle 2 before cycle 1 in the log gives 2.
Also SEEN: cycles as a list (card 210 record-in-progress.json, probed above).

## Criterion 3: per-member checks and the retro structure (Done check 3)

Cap: criterion 3, at most 1 cycles

Behaviour: each member needs a non-blank string role (the domain, per T2), why and context; whitespace-only, "true", "false" and the other TASK_FORCE_PLACEHOLDERS count as blank; a `domain` key, if present, gets the same rule. Message: `task_force: ctx-3 (QA engineer) has no why`. The generic roster is refused: when every member's (role, why) is one of the template's example lines (templates/TASK_FORCE.md, held in a constant, not read from the repo at call time): `task_force: the roster is the template example, not a declared task force`. From card 208 on (forward-only, through _card_number; an unparseable card id is left to the command_scans check), a member whose assignee names Claude needs a non-blank `quota_reason` (the field 208 writes): `task_force: ctx-2 (architect) is a Claude seat with no quota_reason`. The retro keeps `flow_metrics` and `practice_metrics`, each a non-empty object: True, 1, "x", [] and {} are refused naming key and type.

ZOMBIES:
- Z: a member with no fields: role, why and context each named (context keeps its message).
- O: one member with a blank role: that member named, others clean.
- M: three members, one without a why: only that one named.
- B: why "" refused, why "x" accepted; the template roster refused, the same roster with one why changed accepted; a Claude seat on card 207 needs no quota_reason, on 208 it does.
- I: flow_metrics True and practice_metrics "ok": both named with the type; a member that is not an object: refused, no exception.
- E: a Claude seat with no quota_reason; a why of "true"; a whitespace-only domain.
- S: N/A, covered by the existing record tests.

Outside-in plan: acceptance = CLI on a tmp repo record (subprocess). Layer 2: `_member_problems(task_force, card_number)` and `_retro_problems(retro)` (double: dict fixtures). Real layer last: the template constant, with one test that parses templates/TASK_FORCE.md through parse_task_force and equals the constant.

Inputs (D167), the members:
- disabled: n/a: a removed member (removed: retro) is checked like any other; the removal rule is card 209's.
- unknown: INSURANCE: an assignee naming no backend (a typo) needs no quota_reason.
- repeated: INSURANCE: one context on two roles stays refused once (existing test), and one role in two contexts is allowed; the test pins both.
- reordered: INSURANCE: the template roster in reverse order is still refused.
Retro forms: SEEN for truthy flow_metrics (audit 17, MD-003: "only as truthy values"). Whitespace-only and "true" strings: INSURANCE (no real record holds them; the card lists them).

## Order, caps, limits

D157: one criterion per red-green cycle, in order 1, 2, 3 (3 reads the quota_reason that 208 wrote). Each red commit body carries "Criterion: n", "Cycle 1 of 1 for criterion n" (D175 C5) and the D167 answers (a test name or n/a with a reason per entry). Total 3 cycles. A second cycle for any criterion needs a scope window (Decider) committed before its red; a fix cycle after the gate counts in the 3. Each commit subject cites MD-003 (corrective commit, D145). Seats: red Claude ctx-3, green Claude ctx-5, ship Copilot ctx-4, sign-off Claude ctx-6 (window 210-taskforce).

## Moved to follow-up (named)

- F1: commits as cycle evidence (git subjects) and evidence mandatory for every caller; needs git access in check_record.
- F2: a Claude seat planned but never started has no quota_reason and fails criterion 3 by design; whether it needs a softer message is for the retro.

Size: 13332 bytes
