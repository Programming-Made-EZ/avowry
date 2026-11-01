# Card 234 writer review (ctx-3)

Verdict: CHANGES REQUESTED

1. `docs/process-wiki/INDEX.md:5-21` and `docs/process-wiki/briefs/qa-red.md:1-3` / `docs/process-wiki/briefs/backend-developer-green.md:1-3`
   - The wiki says the index is the starting point and that each seat reads the index plus the pages it needs, but the index does not list the brief routes and the briefs themselves only point at the role page. A seat that follows the brief cannot discover the rule pages it actually needs from the index; the route is therefore incomplete.
   - Fix: Add the `briefs/*` entries to `INDEX.md` (or move the route lists into the index so the route is discoverable from the index alone), and ensure each brief lists every rule page needed by that role and step.

2. `docs/process-wiki/briefs/qa-red.md:1-3` and `docs/process-wiki/briefs/backend-developer-green.md:1-3`
   - The red and green briefs omit the concrete rule pages that define the work at those steps. In the agreement, a red step must honor one acceptance criterion per cycle and outside-in TDD (`TEAM_WORKING_AGREEMENT.md:84-95`), while a green step must also honor commit and push discipline (`TEAM_WORKING_AGREEMENT.md:93-110`). The briefs route to only the role page and therefore miss the process-critical rules from the agreement.
   - Fix: Extend both briefs to include the exact rule pages for the step: `rules/one-criterion`, `rules/outside-in-tdd`, and (for green) `rules/thin-slices` and `rules/safety-and-push` as needed.

3. `docs/process-wiki/briefs/architect-slice.md:1-3`
   - The slice brief does not route the architect to the thin-slice page, even though the actual policy is in the agreement and is the only place that defines the size and split limits (`TEAM_WORKING_AGREEMENT.md:126-128`). A reader following only the brief can miss the one-concern, three-criterion, and 15-file / 400-line limits, which is exactly the rule a slice seat must apply.
   - Fix: Add `rules/thin-slices` and `rules/one-criterion` to the slice brief, and keep the route restricted to the pages the slice step actually needs.

4. `docs/process-wiki/briefs/technical-writer.md:1-3` and `docs/process-wiki/INDEX.md:5-21`
   - The writer brief is a route to a role page, but the index does not include the writer brief itself. That makes the "index alone" promise false for the writing role: the page that anchors the writer's responsibilities is not discoverable from the index, and the route does not connect to the writing rules that govern page summaries and card text.
   - Fix: Add the writer brief to the index and route it to the relevant rule pages and role page so the writer can find the rule without hunting through the directory tree.

Per-role notes: if a seat read only its brief, it would miss the following.

- product-manager-ready: it would miss the task-force declaration and objection-window rule in `TEAM_WORKING_AGREEMENT.md:17-25` and the role-fit requirement in `TEAM_WORKING_AGREEMENT.md:305-334`; it also would not see the rule that product work describes what it does, not a role name or design goal (`TEAM_WORKING_AGREEMENT.md:119-125`).
- qa-red: it would miss the one-criterion and outside-in TDD contract (`TEAM_WORKING_AGREEMENT.md:84-95`), the hostile-input/assumption requirement (`HANDOFF.md:1-9`, `TEAM_WORKING_AGREEMENT.md:119-128`), and the commitment rules for a failing red test.
- architect-slice: it would miss the thin-slice limits, the split decision, and the size line requirements (`TEAM_WORKING_AGREEMENT.md:126-128`, `docs/process-wiki/rules/thin-slices.md:1-7`).
- backend-developer-green: it would miss the green step's mandatory commit and push discipline (`TEAM_WORKING_AGREEMENT.md:93-110`), plus the size cap and safety rules that are enforced during the green cycle.
- independent-reviewer-audit: it would miss the review-order and prior-adjustment requirements (`TEAM_WORKING_AGREEMENT.md:203-223` and `docs/process-wiki/roles/independent-reviewer-audit.md:1-9`) and therefore could sign off without checking the actual evidence chain.
- delivery-lead-ship: it would miss the closure record and evidence-before-ship rules (`TEAM_WORKING_AGREEMENT.md:126-136`, `docs/process-wiki/roles/delivery-lead-ship.md:1-8`), so the card could ship without the required proof.
- product-manager-signoff: it would miss the done-check-to-evidence rule and the experiment keep/revise/drop rule (`TEAM_WORKING_AGREEMENT.md:119-125`, `docs/process-wiki/roles/product-manager-signoff.md:1-8`).
- technical-writer: it would miss the rule that reports and pages must describe the thing by what it does, not by a role label or a raw ID (`TEAM_WORKING_AGREEMENT.md:119-125`, `docs/process-wiki/roles/technical-writer.md:1-8`).

Overall: the wiki is close in style, but the seat-routing implementation is under-specified. The briefs are narrower than the actual job steps, and the index is not a complete route map. That is enough to request changes before approving the wiki as a working process tool.

## Re-check (ctx-3)

Verdict: APPROVE

Re-checked the wiki as it stands: `docs/process-wiki/INDEX.md` with its Routes table, all nine briefs (including `briefs/facilitator-restart.md`), the role and rule pages with their Quote lines, `HANDOFF.md` "## Restart here" (lines 8-26), and `.mob/records/234/usage/restart-report-20261101T093906Z.txt`. I checked every Quote line against `docs/decisions/process-and-staffing.md` with a script: all quotes appear verbatim (no MISSING output).

### Earlier findings

- W1 (index does not list the brief routes): CLOSED. `INDEX.md:23-34` is a Routes table with one row per brief, and `check-process-wiki.py` enforces that each brief's Pages line equals its row (`briefs/*.md:3`).
- W2 (red and green briefs omit the rule pages): CLOSED. `INDEX.md:26` routes qa-red to `rules/one-criterion`, `rules/outside-in-tdd` and `howto/hostile-inputs`. `INDEX.md:28` routes backend-developer-green to `rules/one-criterion`, `rules/outside-in-tdd`, `rules/thin-slices` and `rules/safety-and-push`. The briefs match (`briefs/qa-red.md:3`, `briefs/backend-developer-green.md:3`).
- W3 (slice brief omits thin-slices): CLOSED. `briefs/architect-slice.md:3` now has `rules/thin-slices`, `rules/one-criterion` and `howto/hostile-inputs`. The limits are in `rules/thin-slices.md:3` and `:5`, and in `roles/architect-slice.md:4-5`.
- W4 (writer brief not discoverable from the index): CLOSED. The writer row is at `INDEX.md:31` and the brief is `briefs/technical-writer.md:3`. The writer page carries the describe-not-ID rule (`roles/technical-writer.md:3`).

### Per-role notes

- product-manager-ready: CLOSED. Objection window and polled roles at `roles/product-manager-ready.md:3`, required seats at `:4`, describe-not-ID at `:5`, and `rules/decisions.md:3-4` is routed. Residual: the role-fit requirement (`TEAM_WORKING_AGREEMENT.md:305-334`) is not quoted anywhere.
- qa-red: CLOSED. Intent line, hostile-input and assumption answers at `roles/qa-red.md:3-4`. The one-criterion and outside-in rules are routed (`rules/outside-in-tdd.md:3-4`).
- architect-slice: CLOSED (`roles/architect-slice.md:3-7`, `rules/thin-slices.md:3,5`).
- backend-developer-green: CLOSED. Commit and push discipline at `roles/backend-developer-green.md:6` and `rules/safety-and-push.md:3-4`, the size cap at `:5`.
- independent-reviewer-audit: CLOSED (`roles/independent-reviewer-audit.md:3-5`, plus `rules/stand-downs.md:3-4`).
- delivery-lead-ship: CLOSED (`roles/delivery-lead-ship.md:3-6`).
- product-manager-signoff: CLOSED (`roles/product-manager-signoff.md:3-5`, `rules/decisions.md:5`).
- technical-writer: CLOSED (`roles/technical-writer.md:3-5`).

### Routing test: did a fresh context find what it needed?

Yes, mostly. The restart seat's report (`restart-report-20261101T093906Z.txt`) matches the committed key (`restart-key-20261101T093721Z.json`) on next_card 234, next_step re-review and boundary_hash 3186c15510b1, and its rules_read equals the facilitator-restart row at `INDEX.md:34`. It read ten files, all named by the index, HANDOFF, the brief or the re-rank note, and it never read TEAM_WORKING_AGREEMENT.md. The seat-prompt output for `qa-red` is 6291 bytes and for `technical-writer` 6518 bytes, and each ends with its role page.

### New gaps (none blocking)

- N1: `HANDOFF.md:23` says next step is the `plan` field of the last cycle. In `record-in-progress.json` that field is a long arrow chain, not a step. The seat said so in its notes and used the `next_step` field instead, which is what matched the key. Fix the HANDOFF line to name `next_step`.
- N2: `howto/hostile-inputs.md:3` is a single line that explains nothing. It is routed to qa-red and architect-slice (`INDEX.md:26-27`). The actual hostile-input rule lives only in `roles/qa-red.md:4` and `roles/architect-slice.md:7`. Either give the page the four cases (disabled, unknown, repeated, reordered) with an example, or drop it from the routes.
- N3: `rules/one-criterion.md:4` is about the product owner setting priority, a duplicate of `rules/decisions.md:5`, and has nothing to do with one criterion. It is also why the writer brief (`briefs/technical-writer.md:3`) routes to that page for no clear reason. Replace line 4 with the red-names-one-criterion detail and have the writer route to the describe-not-ID rule instead.
- N4: `rules/outside-in-tdd.md:5` and `rules/thin-slices.md:4` are bookkeeping ("was amended") and tell a seat nothing about what to do. State the amended rule text or fold them into the lines above.
- N5: the restart report's order was HANDOFF first, then the index, while `HANDOFF.md:12-14` and `roles/facilitator-restart.md:3` say index first. That is natural, because the seat starts from HANDOFF. State in one sentence that HANDOFF is entered first and the index second.
- N6: `roles/facilitator-restart.md` has no Rule and Quote lines, unlike every other role page. That is fine for a procedure page, but the convention is not uniform.
- N7: the seat could not report subagent_tokens (`restart-report`: "not available to me"), so the facilitator appended it. The field cannot be self-reported by the seat; drop it from the seat's required fields or label it facilitator-filled in `HANDOFF.md:26`.

### Command output

`python scripts/check-process-wiki.py`:
```
process-wiki: OK (18 pages)
```
`python scripts/mob_process.py seat-prompt docs/process-wiki/briefs/qa-red.md` and the same for technical-writer.md: `::error::mob-process: missing brief: ...` with exit 1. The command takes the brief name, not a path. With names:
- `seat-prompt qa-red`: `bytes: 6291`, then the index, the Routes table, and the qa-red role page (first rule line: "Rule: The red seat writes the intent line first. ... See D165").
- `seat-prompt technical-writer`: `bytes: 6518`, then the same index and Routes table, and the writer role page (first rule line: "Rule: Reports and pages describe what a card, decision or finding does; an ID may follow in parentheses. ... See D161").
