Index of the process wiki: one line per page, each page read just in time.

| id | path | status | summary |
|---|---|---|---|
| core/index | docs/process-wiki/INDEX.md | enabled | Index of the process wiki: one line per page, each page read just in time. |
| rules/one-criterion | docs/process-wiki/rules/one-criterion.md | enabled | One acceptance criterion per red-green cycle, checked from git. |
| role/qa-red | docs/process-wiki/roles/qa-red.md | enabled | What the QA seat reads and writes at the red step. |
| howto/hostile-inputs | docs/process-wiki/howto/hostile-inputs.md | enabled | How to answer the disabled, unknown, repeated and reordered entries. |
| role/product-manager-ready | docs/process-wiki/roles/product-manager-ready.md | enabled | What the product manager reads and writes when a card is made ready and its task force declared. |
| role/architect-slice | docs/process-wiki/roles/architect-slice.md | enabled | What the architect reads and writes at the slice step. |
| role/technical-writer | docs/process-wiki/roles/technical-writer.md | enabled | What the technical writer reads and writes for wiki pages, briefs and card text. |
| role/backend-developer-green | docs/process-wiki/roles/backend-developer-green.md | enabled | What the backend developer reads and writes at the green step. |
| role/independent-reviewer-audit | docs/process-wiki/roles/independent-reviewer-audit.md | enabled | What the independent reviewer reads and writes at the audit step. |
| role/delivery-lead-ship | docs/process-wiki/roles/delivery-lead-ship.md | enabled | What the delivery lead reads and writes at the ship step. |
| role/product-manager-signoff | docs/process-wiki/roles/product-manager-signoff.md | enabled | What the product manager reads and writes at sign-off. |
| role/facilitator-restart | docs/process-wiki/roles/facilitator-restart.md | enabled | What a fresh facilitator reads and writes at a restart; its seat brief is briefs/facilitator-restart.md. |
| rules/outside-in-tdd | docs/process-wiki/rules/outside-in-tdd.md | enabled | Outside-in TDD: one failing acceptance test per criterion, committed pending, made green in one commit. |
| rules/thin-slices | docs/process-wiki/rules/thin-slices.md | enabled | Thin slices: numeric limits per card and commit, and the slice size cap. |
| rules/decisions | docs/process-wiki/rules/decisions.md | enabled | Decisions: routine per-card calls take an objection window, rule changes take a full Decider. |
| rules/stand-downs | docs/process-wiki/rules/stand-downs.md | enabled | Stand-downs: a micro-retro at each cycle boundary with one adjustment, reviewed at the next. |
| rules/seat-rotation | docs/process-wiki/rules/seat-rotation.md | enabled | Seat rotation: least-recently-used seating and the facilitator checklist before each window. |
| rules/safety-and-push | docs/process-wiki/rules/safety-and-push.md | enabled | Safety and push: detect after the fact, commit and push often, delete nothing outside the tree. |

Routes: one row per brief; a brief's Pages line must equal its row, in the same order.

| brief | pages |
|---|---|
| product-manager-ready | core/index, role/product-manager-ready, rules/seat-rotation, rules/decisions |
| qa-red | core/index, role/qa-red, rules/one-criterion, rules/outside-in-tdd, howto/hostile-inputs |
| architect-slice | core/index, role/architect-slice, rules/thin-slices, rules/one-criterion, howto/hostile-inputs |
| backend-developer-green | core/index, role/backend-developer-green, rules/one-criterion, rules/outside-in-tdd, rules/thin-slices, rules/safety-and-push |
| independent-reviewer-audit | core/index, role/independent-reviewer-audit, rules/stand-downs, rules/decisions, rules/safety-and-push |
| delivery-lead-ship | core/index, role/delivery-lead-ship, rules/stand-downs, rules/safety-and-push, rules/decisions |
| product-manager-signoff | core/index, role/product-manager-signoff, rules/decisions, rules/stand-downs |
| technical-writer | core/index, role/technical-writer, rules/decisions, rules/one-criterion |
| facilitator-restart | core/index, role/facilitator-restart, rules/seat-rotation, rules/stand-downs, rules/safety-and-push, rules/thin-slices |
