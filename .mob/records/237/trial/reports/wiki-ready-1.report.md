# Ready record: block variable-path recursive deletes and push every commit (card 227)

Seat: product manager, ready step. Card treated as open. Built from the board card text only.

## Pages and rules relied on
- docs/process-wiki/INDEX.md (route row product-manager-ready: core/index, role/product-manager-ready, rules/seat-rotation, rules/decisions)
- docs/process-wiki/roles/product-manager-ready.md: task force declared through an objection window with every declared role polled (D153); proposal lists every required seat, slice and ship included, each present or removed with a reason (D158); anything for the product owner describes a card or decision by what it does, ID only in parentheses (D161).
- docs/process-wiki/rules/decisions.md: a routine per-card call takes an objection window, not a full Decider; a full Decider only on an objection, a one-way door or a changed standing rule.
- docs/process-wiki/rules/seat-rotation.md: seats go to the available model whose last seating is oldest (D163, D164); facilitator prints working directory and repo top level before the window (D158).

## Task force proposal (for the objection window, not yet adopted)
Proposed through an objection window (D153). Each declared role seat must be polled in its own fresh context and record no objection, or an objection with ONE fixed condition. The facilitator runs the pre-window checklist first.

Present:
| Seat | Step | Reason |
|---|---|---|
| Product manager | ready | Declares the task force and the value metrics. |
| Architect | slice | Required seat. Numbers the three Done checks, writes the assumption line and the hostile-input list. |
| Security reviewer (adversarial lens) [reviewer] | adversarial review | Domain expert, see below. |
| QA engineer | red | Writes the failing tests and the ZOMBIES lists the card assigns to QA at ready (Done checks 1 and 3). |
| Backend developer [author] | green, refactor | Changes the scan and adds the detector. |
| Independent reviewer (audit lens) [reviewer] | audit | Required review seat, separate from the author. |
| Delivery lead | ship | Required seat. Push of every green. |
| Product manager | sign-off | Separate fresh context from the ready seat. |

Left out, a reason each:
- UX designer, frontend developer: no user interface.
- Database developer: no database.
- Technical writer: only two short rule texts (working agreement and template). The backend developer writes them and the independent reviewer checks them.
- Language designer, grammar designer: no language change.
- Performance engineer: no hot path.
- DevOps or runner expert: the detector uses git only, and the card changes no hook or workflow. The command scan reads tool calls or a transcript. If the slice finds a hook change is needed, add this seat then by a Decider.
- Agile coach: no process-change experiment.

Seating: each seat in its own fresh context. Models go by least-recently-used among enabled models with a probe ok (D163, D164). The facilitator must show the probe evidence. I do not assert which models are available.

## Domain-expert question and answer
Question: should a member with different domain knowledge or context join now?
Answer: yes, a security reviewer with an adversarial lens. The card is a safety item (product owner safety ruling, audit 62). Its failure mode is a mis-quoted delete wiping a repository including .git, as in the 2026-10-08 home-folder wipe. The people writing the scan patterns cannot be the only ones trying to defeat them. Any member can add a further expert later by a Decider (mid-card addition, with the Decider id).

## Open points to resolve at the slice
1. The exact slip is not recorded verbatim. QA reconstructs it from evidence (card 226's recorded deviation) before the first red.
2. The scan reads replies and may never see an executed command. The slice must say how the scan sees tool calls (hook or transcript). The card requires this for Done check 1, including a failing test on a destructive command absent from the reply text and an empty or missing command-scan source failing.
3. Where the trust-ladder record lives (template's trust-ladder record). Needed for Done check 2.
4. The audit 62 failure identification (F-071): the ledger text is not in the repository, so the auditor confirms or corrects.
5. Repository paths for avowry and mob-defaults for the push detector. The detector runs git only, no gh calls.
6. "More than about 5 minutes": the slice fixes one exact threshold and its test boundary.
7. The three Done checks are numbered 1 to 3 before red. Each red commit carries "Criterion: n", and the green for n comes before any red for n+1.
8. The "Intent:" line quoting the product owner's ruling comes before the first red.
9. Hostile-input list for the scan's command forms, per howto/hostile-inputs. That page is outside my route, so the architect applies it.
10. Scope guard: the commit-message claim check and process-kill rules are out of this card.

## Product value and metrics
Value: no seat can delete a repository by a variable-path recursive delete unseen, and unpushed work is never more than about 5 minutes old.

Metrics:
- Unflagged recursive deletes on variable or computed paths: target 0.
- Commits on avowry or mob-defaults unpushed for more than about 5 minutes: target 0.

Counter-metric (Goodhart guard):
- False positives on literal paths inside a known scratch root through safe_delete, which must stay allowed.
- Replies reworded to dodge the scan: count and review them, because a falling flag count must not mean the slips are hidden.
- Read both together with the targets above. A zero on the targets with a rising counter-metric is not a success.

Note: this record is for the product owner, so it describes the card by what it does ("block variable-path recursive deletes, push every commit"), with the ID in parentheses (D161).

Size: 5899 bytes
