# D158 pre-vote checklist: Decider rerank-20 (facilitator)

- Proposal: .mob/records/reranks/2026-11-03-20.md (product manager seat, fresh Claude context; transcript scan clean, 21 commands). It carries positions on product-owner proposals 278, 284 and 288 and the full order.
- Decider (Dispatch: "the Decider on proposals 278, 284 and 288, plus placing the listed cards"): seats architect, QA engineer, backend developer, independent reviewer (audit lens), decider and agile coach; the proposer (product manager) does not vote. Each reply: first line the overall vote (Yes, Support-it or No) on the whole proposal; second line one vote per proposal ("278: Yes|No; 284: Yes|No; 288: Yes|No"); reasons; a D171 CITE. An unread or uncited vote is re-asked once.
- Serial launch through decider.py's decision-seat gate; Grok and Copilot seats are expected to return empty first (the cmd.exe line limit this re-rank ranks first).
- No commit to avowry while a voter runs.
- FAIL head: audit 200 (.mob/audits/last.json) lists open FAILs F-072, F-074, F-078, F-086. No Ready card names any of them (facilitator grep of board/ and the records), and their texts are not in the repository (F-072 and F-074 were asked of the product owner earlier). Card 264 names audit 123, which is not in audit 200's open list; the proposal states the reversal if it is still open.
- Line 36 of TEAM_WORKING_AGREEMENT.md is product-owner direction (D161). If 288 is adopted, the facilitator asks the product owner, through Dispatch, to confirm the replacement wording before editing line 36. The order can be applied meanwhile: the next pull (229) is above the cut under both the old and the new rule.
- Probe: re-probed before the vote, plus decider.py's own probe per run.
