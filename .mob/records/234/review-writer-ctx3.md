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
