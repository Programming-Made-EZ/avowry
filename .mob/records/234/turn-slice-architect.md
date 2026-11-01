# Card 234 slice: architect (ctx-2, Copilot seat)

Sources read: board/234-an-indexed-process-wiki-with-just-in-time-reading.md; .mob/records/234/record-in-progress.json; .mob/records/decisions/234-taskforce.json; docs/decisions/process-and-staffing.md (D136, D157, D165, D166, D167, D168, D170); TEAM_WORKING_AGREEMENT.md; docs/decisions/INDEX.md; HANDOFF.md; .mob/records/226/turn-slice-architect.md.

## Scope and T2 domain-expert answer

This slice covers all three Done checks as numbered criteria (D157), with explicit measurement methods and git-checkable evidence paths, plus page/index structure, anchor guarantees, seat-brief routing, facilitator handoff use, and the delivery plan under D136.

Domain-expert question (T2): should a member with different domain knowledge join now? **Yes**: documentation information architecture. The technical writer seat already present in the declared task force (ctx-3) is the domain expert for page taxonomy, summaries, and seat-routing clarity.

## Criteria (Done checks 1 to 3; D157 numbering)

1. **Indexed process wiki with anchor-preserving rule pages.**  
   The process corpus is split into indexed pages, each page starts with a one-line summary and every rule reference still resolves to its decision anchor.
2. **Seat briefs route index + needed pages only, with token metrics.**  
   Each seat brief names the index and only the pages needed for that role/step, and per-seat token use is recorded against baselines 50K (setup) and 125K (median end).
3. **Facilitator restart from wiki + handoff notes, with context and miss metrics.**  
   The facilitator restarts from a boundary using only the wiki and handoff notes; facilitator context size is recorded against baseline 965K; unread-page rule misses are counted.

## Wiki location, page shape, and index

- **Location (now): both repositories, one canonical source.**  
  Canonical editable wiki pages live in **avowry** so card execution and evidence stay local to the active mob.  
  **mob-defaults** carries the template mirror only (seed pages for new mobs).  
  If the experiment gets a keep verdict (Done check 3), avowry pages are promoted into mob-defaults through the normal template-change path and template review (D170).
- **Index page:** `docs/process-wiki/INDEX.md` in avowry; mirrored as the same path in mob-defaults template.
- **Page format:** first line is a one-line summary, then body; every rule mention keeps a decision anchor link in `area-file.md#dNNN` form (for process rules, `docs/decisions/process-and-staffing.md#dNNN`).

## Anchor integrity and git-checkable failure test

Each rule keeps its decision anchor by storing both:
1) visible D-number text (`D157`, `D166`, etc), and  
2) explicit link target (`...process-and-staffing.md#d157`, etc).

Git-checkable test (red first) fails on any of:
- missing required metric numbers (`50K`, `125K`, `965K`) or missing measurement-method text in card 234 board file or card 234 record file;
- missing `DNNN` token next to a rule statement on a wiki page;
- anchor link target that no longer resolves (link target missing from decision file anchors);
- unresolved page listed by a seat brief.

Outside-in check shape:
- parse wiki pages and seat briefs;
- extract required D-numbers and anchor URLs;
- verify anchors exist in docs/decisions area files;
- verify measurement numbers and method sentences exist in board/234 and .mob/records/234 record file;
- fail closed on unknown page name or broken link.

## Measurement method (adopted condition made concrete)

- **Tokens per seat (Criterion 2):**  
  For each seat context (`ctx-1` to `ctx-8` in `.mob/records/234/record-in-progress.json`), record provider-reported token totals (`input + output`) at two checkpoints: setup complete and seat done. Store these numbers in the card record under the seat entry. Compare setup values to **50,000** baseline and end values to **125,000** median baseline.
- **Facilitator context size (Criterion 3):**  
  At each restart boundary, record facilitator context token count from the facilitator run record (same card record family under `.mob/records/234/`). Compare against baseline **965,000**.
- **Rule-miss counter-metric (Criterion 3):**  
  Every case where a seat misses a rule due to an unread required page is logged as one structured case in card 234 record (`rule_miss_cases`), with seat context and missed page.

These methods are valid only when committed in the card record; if numbers exist without method text, the criterion is not run (adopted condition).

## Seat briefs, handoff notes, and non-wiki boundary

- **Seat brief page naming:** each brief has a `Pages:` line with canonical page IDs, not free text paths:  
  `core/index`, `rules/<slug>`, `role/<role>-<step>`, `howto/<topic>`.  
  Resolver map in the index maps IDs to file paths. Unknown ID fails the brief check.
- **Facilitator handoff notes:** handoff notes stay as operational state and boundary checkpoints (what changed, next boundary, open risks, exact resume point). They are consumed with the wiki at restart; they are not replaced by wiki prose.
- **Stays outside the wiki:** transient runtime evidence, raw command transcripts, probe logs, in-flight operational artifacts, and audit-only scratch notes. Wiki holds stable process guidance, routing, and anchor-linked rules only.

## D166 assumption lines (one per invariant, with probes)

Assumes: process decision anchors are stable HTML ids in decision files and index links target them directly; probe: `docs/decisions/process-and-staffing.md` contains `<a id="d157">` through `<a id="d170">` and `docs/decisions/INDEX.md` links include `process-and-staffing.md#d157`..`#d170`.

Assumes: card 234 already carries numeric baselines and adopted-condition wording that can be checked from git; probe: board/234 file includes Done checks with 50K, 125K, 965K and the adopted-condition line requiring numbers+method and anchor-resolve failure.

Assumes: facilitator restart context is anchored by committed handoff documentation plus per-card records, not by private memory; probe: `HANDOFF.md` defines takeover/restart behavior and `.mob/records/234/record-in-progress.json` declares contexts and metrics surface for this card.

## D167 hostile-input lines by criterion

Criterion 1 Inputs: disabled: a rule page marked disabled in index must not be assigned by any brief and must not count as required; unknown: a rule id not in index fails routing check; repeated: duplicate rule id in a page map is rejected; reordered: page order differences do not change anchor-resolve result.

Criterion 2 Inputs: disabled: disabled page id in a seat brief fails that brief; unknown: unknown page id in `Pages:` fails that brief; repeated: repeated page id is de-duplicated for reading but flagged in check output; reordered: reordered `Pages:` line remains valid if set equality with required pages holds.

Criterion 3 Inputs: disabled: disabled handoff section id is n/a: handoff sections are not disable-able, only superseded with explicit replacement; unknown: unknown boundary marker in handoff notes fails restart check; repeated: repeated boundary marker uses last-write-wins and records warning; reordered: boundary entries out of order fail if monotonic boundary sequence is broken.

## Criterion-by-layer table (outside-in)

| Criterion | Outside layer (acceptance) | Middle layer | Inner layer |
|---|---|---|---|
| 1 | Wiki index and rule pages compile to a complete route map; every rule link resolves to a live D-anchor. | Anchor/link checker over page IDs and D-links. | Anchor source in decision files (`<a id="dNNN">`) and D-number preservation in page text. |
| 2 | Seat brief for each role-step lists only index + needed pages and records token metrics. | Brief parser validates `Pages:` IDs and required-page set per seat-step. | Record writer persists per-seat setup/end token counts with committed provider-usage source refs and baseline comparisons; Copilot rows may be `not_measured` only with reason and available quota evidence. |
| 3 | Facilitator can restart at boundary from wiki + handoff notes; context and rule-miss metrics recorded. | Restart checker validates boundary marker, required pages, and miss counter entry shape. | Record writer persists facilitator context size from a committed provider-usage source, plus explicit `rule_miss_cases` and its source (including the zero-case source). |

## D136 commit plan (one concern per commit; criterion order enforced by D157)

1. **Red 1 (Criterion: 1):** failing checks for index/page split plus anchor-resolve integrity and missing-number-or-method guard.
2. **Green 1 (Criterion: 1):** implement index, page summaries, and anchor-preserving link format.
3. **Red 2 (Criterion: 2):** failing checks for seat brief `Pages:` routing plus missing committed provider-usage source refs, missing per-seat setup/end token values, or missing Copilot `not_measured` reason/evidence.
4. **Green 2 (Criterion: 2):** implement seat brief page-ID routing + record fields for per-seat token values, provider-usage source refs, and Copilot `not_measured` handling.
5. **Red 3 (Criterion: 3):** failing checks for restart-from-wiki+handoff plus missing facilitator token value/source and missing explicit `rule_miss_cases` (including empty-without-source).
6. **Green 3 (Criterion: 3):** implement restart flow and record writers for facilitator token value/source and explicit `rule_miss_cases` + source.
7. **Review-fix commits (if needed):** separate, scoped fixes only; no criterion mixing.

Slice verdict: **SHIP AS IS** (three criteria, one concern: indexed process wiki with just-in-time reading and measurable restart/routing behavior).

## Amendment for window 234-slice (ctx-2)

- **Committed provider-usage source for token values:** Claude seats use committed Claude transcript usage fields (`usage.input_tokens`, `usage.output_tokens`) as the provider source; Copilot seat token usage is **not measured** because this repo records no Copilot command source/transcript for seat runs (only quota/account probe evidence and explicit `no command source`/unscanned status).
- **Where values are committed (file + fields):** Card values are committed in `.mob/records/234/record-in-progress.json` under explicit seat/facilitator metric fields keyed by context. For each seat (`ctx-1`..`ctx-8`): `setup_tokens`, `end_tokens`, `provider_usage_source` (path/ref to committed provider transcript/record), and when applicable `not_measured_reason` + `available_evidence_source` (Copilot quota/account probe path/ref). For facilitator restart runs: `facilitator_context_tokens` and `facilitator_provider_usage_source` in the same card record family.
- **`rule_miss_cases` shape and source:** The card record must contain explicit `rule_miss_cases` as an array (possibly empty) of objects with at least seat/facilitator context, boundary or restart point, missed required page id, and source ref. If empty, the record must also carry an explicit zero-case source field (for example a checked restart/run reference proving none were observed).
- **How the git check fails on missing pieces:** fail when (1) any required seat/facilitator token value is missing, (2) any token value lacks a committed provider-usage source ref, (3) a Copilot `not_measured` entry lacks reason or committed evidence source, (4) `rule_miss_cases` field is absent, or (5) `rule_miss_cases` is empty without its stated zero-case source.

Source anchors for this amendment: `.mob/records/decisions/234-slice.json` (`adopted_conditions`), `.mob/records/234/record-in-progress.json` (`task_force` contexts and `command_scans` showing Copilot `no command source`), `.mob/records/probes/2026-10-30.jsonl` (available Copilot quota evidence), and `TEAM_WORKING_AGREEMENT.md` (non-Claude command-source limit statement).

Size: 11876 bytes
