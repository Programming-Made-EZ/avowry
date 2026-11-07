# Re-rank note 2026-11-07 (27), revision "product first" PROPOSAL (supersedes the withdrawn first draft, 2026-11-07-27.md)

The part up to "Facilitator's additions" is the product manager seat's reply (fresh context, read-only), committed unchanged. The first draft of re-rank 27 (2026-11-07-27.md) put three process stories first; its round 2 vote passed but it was never recorded, and it is withdrawn because the product owner ruled "product first" (inbox, 2026-11-07) before it was recorded.

Status: PROPOSAL (not yet through the Decider). It supersedes the withdrawn first draft of re-rank 27, which put process stories first. Only the read-only inputs were used.

- **Not a one-way door:** a rank edit is one `rerank.py` call. The same call with the Before order reverses it. No issue is closed, edited or deleted.
- **Not a standing-rule change:** it applies the working agreement's "Product first" paragraph as written. The head-size guard (at most 4 bugs above the first product story) holds.

## Intent

Order the open board so product stories come first. Only bugs that put user data, safety or the team's ability to ship at risk may sit above them, and each must state that risk on its card. Process and tooling cards rank below the product work, and each one that fits is folded into the product story it would serve.

## Inputs

- The open-issues snapshot (`rerank-27-open-issues.json`), the dry-run rank list, and the board card texts (`board/NNN-*.md`) for every bug and process card.
- The working agreement's Value rule, Card format rule, D183 and Product-first paragraph.
- The rank list holds 180 cards. 68 of them are closed, withdrawn, superseded or not in the open snapshot. They are not classified and only shift down. That leaves 112 live cards.
- The inbox was read at this boundary. It holds no entry that changes this order.
- The classification below judges who gains, not the Type field.

## Author

Product manager seat, fresh context, read-only.

## Classification of the 112 live cards

**Bug at risk (2).**
- **Second writer on one checkout is refused across processes (299).** It qualifies because the harm has already happened twice. A seat overwrote another card's committed proposal, and two facilitator processes raced a push. Committed work is lost and delivery stops.
  - The card text names the recurrences but states no risk line.
  - Line to add: "Risk: committed work can be overwritten or a push lost, seen twice in card 230 (the card 290 proposal overwritten; a failed push on 2026-11-04), which stops delivery."
- **A Codex build seat's commands go through the delete scan (305).** It qualifies because Codex build seats now run in full-access mode, and a recursive or hidden delete by one goes unscanned. Files on the machine could be destroyed. This is a safety gap in a guard the decision said "still applies".
  - The card is typed tooling and its value line only implies the risk.
  - Two edits are needed before it can sit in the head. Retype the card to bug, and add: "Risk: a Codex build seat with full machine access can delete files with no scan and no flag."

**Bugs that do not qualify. They go below the product work, with no change to their text.**
- **Run that cannot register its vote lock starts unprotected (315).** No occurrence is cited since card 313 closed the usual path. It threatens the integrity of a vote, not user data or shipping.
- **Overlapping decision runs fail each other (307).** Overlap is already safe since 298. The failure is a false integrity verdict, and a run can avoid it by not overlapping.
- **Two readiness tests pass without the thread-pool floor (304).** The floor is still in place, so nothing is red today.
- **Benchmark children build concurrently (302).** Flaky acceptance runs are noise, not data or shipping risk.
- **Legacy turn on a Codex machine whose shell cannot start (300) and probe nonce copied from the prompt (301).** The machine fixes of 2026-11-06 removed the live cause.
- **Browser opened on the laptop (232).** It disrupted the desktop once on 2026-10-29. It is not data loss and not a shipping risk.
- **NO OBJECTION stored as a No vote (286).** The facilitator reads the first line, so only the stored field misleads.

**Product (94).** Order kept as it is today.
- Renames to Avowry: test projects (196), solution, CI jobs and scripts (197), docs and code naming (266).
- Docs and guide: grammar and cheat sheet (162), prerequisites page (178), README and getting-started guide (179), known-limitations page (186), changelog and 0.1.0 (184), issues, contributing and security notes (185), and the docs, ADR and grammar cards (53, 22, 13, 19, 14, 46, 143, 204, 57-class ones that remain live).
- Packaging and release: dotnet tool (176), CI installing the packed tool (177), publish the first version (187), pre-publication checks (188), the guide test and cold-model run (180, 181), actionable messages (182), `avowry fmt` (200).
- Language, compiler and adapter work: 189, 202, 203, 169, 171, 172, 165, 166, 163, 164, 145-148, 134, 89, 56, 58, 20, 52, 23, 33, 34, 25, 15, 21, 35, 26, 107-109, 28-31, 295, 296.
- Benchmarks and measurements: 201, 153, 155, 149-151, 289, 140, 115-118, 120, 123-129, 17, 24, 27, 60, 81. Dana and Ari's value is the benchmark's outcome, and the product owner reads these to decide about Avowry.
- Spikes and decisions: 59, 82, 61, 83.
- Dogfood app (38).

**Process, tooling and mob-internal (16).**
- Lowest-risk fold-ins: wait never leaves the mob idle (321), gate runs narrow suites (316), Ready check (317).
- Persona-template card (156). It is mob-internal: the template and its checker serve story writing.
- The bugs that did not qualify: 315, 307, 304, 302, 300, 301, 286, 232.
- Decider and loop cards: record result must follow votes (312), vote scored as written (311), rulings recorded (308), fourth driver Muse (310).

## Before (current order, dry run)

97 315, 98 299, 99 307, 100 316, 101 317, 102 305, 103 196, 104 197, 105 312, 106 304, 107 266, 108 311, 109 310, 110 308, 111 302, 112 300, 113 301, 116 232, 117 162, then the tail. Card 321 is unranked. Card 313 is at 96.

To reverse the proposal, run `rerank.py --after 313` with the live cards in that order. The closed cards 309 and 306 are left out.

## After (proposed)

| Ranks | Cards |
|---|---|
| 97-98 | 299, 305 |
| 99-192 | the 94 product cards in current relative order, starting 196, 197, 266, 162, 201, 189, 202, 203, 176, 178, 179, 186, ... |
| 193-208 | 321, 316, 315, 307, 317, 156, 304, 302, 312, 311, 308, 310, 300, 301, 286, 232 |

## Reasons

- **Why the head is only 299 and 305.** The ruling lets a bug stay above product only if it meets the bug rule. Only these two have a stated, evidenced risk to data, safety or shipping. The other bugs are real defects, but their harm is a false verdict, flakiness, or a risk not yet seen.
- **Process order, by what each would unblock for a product story.**
  - **321:** it removes the hour-long stalls that delay every story.
  - **316:** it speeds every story's gate.
  - **315 then 307:** they make the Decider safe when a product story needs a decision.
  - **317 and 156:** they improve card quality at Ready.
  - **304 and 302:** they steady the test runs.
  - **312, 311, 308:** they tighten decision records.
  - **310:** it adds a fourth driver's capacity.
  - **300, 301, 286, 232:** they are cosmetic or already mitigated.
- **Fold-ins.** Where a card fits, it is done inside the product story it serves and proven by that story's outcome.
  - 321 and 316 fold into the rename of the test projects (196), which changes the gate's suites.
  - 304 folds into CI installing the packed tool (177), which already needs repeated CI runs.
  - 302 folds into migrating the holdouts and recording a baseline (201), which runs the benchmark children.
  - 156 folds into reconciling the personas with the v0.1 bar (204).
  - 315 and 307 fold into the first product story that needs a decision.
  - The rest stay standalone at the bottom and are pulled only when a story needs them.

## Which single card is pulled first: 299 (second writer on one checkout is refused)

This is the only card whose harm is already seen, twice, and loses committed work. 305 follows. Its risk is serious but not yet seen, so it is second. The first product story, the test-project rename (196), follows.

## Head-size guard

There are 2 bugs above the first product story (196), against a limit of 4. Two places remain.

## Reversals stated

- 315, 307, 300 or 301 returns above the product work if one is seen once in real work with a named loss of data, safety or shipping.
- 305 returns to the process group if the facilitator does not retype it to bug and add the risk line.
- Any other placement reverts with the Before order in one run.

## Dry run

One run, anchor 313, no `--apply`, 110 cards:

`PYTHONPATH=. python scripts/board/rerank.py --after 313 --cards 299,305,196,197,266,162,201,189,202,203,176,178,179,186,169,171,172,165,166,177,200,180,181,187,163,182,164,184,153,185,188,155,204,145,146,147,148,134,149,150,151,289,140,143,144,107,108,109,115,116,117,118,120,123,124,125,126,127,128,129,89,53,54,56,22,58,13,19,14,46,17,24,27,60,81,20,52,23,33,34,59,82,25,15,21,35,38,26,61,83,28,29,30,295,296,31,321,316,315,307,317,156,304,302,312,311,308,310,300,301,286,232`

Quoted lines, labelled rerun:
- rerun: would: (299) rank 98 -> 97
- rerun: would: (305) rank 102 -> 98
- rerun: would: (196) rank 103 -> 99
- rerun: would: (266) rank 107 -> 101
- rerun: would: (189) rank 124 -> 104
- rerun: would: (321) rank None -> 193
- rerun: would: (316) rank 100 -> 194
- rerun: would: (315) rank 97 -> 195
- rerun: would: (307) rank 99 -> 196
- rerun: would: (317) rank 101 -> 197
- rerun: would: (156) rank 155 -> 198
- rerun: would: (232) rank 116 -> 208

The run lists 180 ranked cards. All 180 change rank: the 112 live cards, plus the 68 dead cards that only shift down. Card 321 is newly ranked. The 94 product cards keep their relative order.

## Not verified

- Whether the Decider and the product owner accept 305 as a bug. It needs the retype and the risk line.
- The assignments of 20 to 25 blank-Type old cards to product (rows 13, 22 and similar). I judged them from titles and actor lines, not full texts.
- The fold-in targets. They are suggestions, and the architect checks the fit at Ready.
- That the 68 dead cards stay at their shifted tail positions. The dry run shows this, but I did not run `--apply`.

## Facilitator's additions (not the seat's text)

- The command the apply script runs is the seat's list verbatim, anchor 313 (see `rerank-27-verdicts.json`, the order key); cards 318, 319, 320 and 322 (process stories) are not named and stay unranked, so they are not above the line and are pulled only when a product story needs them (the seat's "pulled only when a story needs them" for the bottom group). Closed cards 306, 309 and 314 are not named.
- The two risk lines the seat says must be on the cards before they may sit above product work are written on cards 299 and 305 by the apply step, and card 305 is retyped to a bug in its Type line; the seat's wording is used verbatim. These are two `gh issue edit` steps made by the facilitator after the vote passes and recorded in the apply log's notes (the apply script has no "risk line" operation; adding one is not part of this note).
- The ten Riley re-points (cards 17, 24, 27, 60, 81, 83, 117, 123, 128, 144; card 38 stays Riley as dogfooding) are carried over unchanged from the withdrawn draft in the same artifact, applied by `scripts/board/apply_value_verdicts.py`. Dry run: 10 edits and one rerank.
- The working agreement now carries the Product first rule (TEAM_WORKING_AGREEMENT.md). The mob-defaults copy is story 318.
- The old draft's round results (round 1 two No votes whose condition was that the files be committed; round 2 five Yes and one Support-it; the independent re-run stopped before it completed) are kept as history in `checklist-rerank-27.md`; none of them is a vote on this note.

## Revision 2 of the product-first note, after the product owner's ruling that process improvements are not issues (facilitator's additions; the seat's text above is unchanged)

- The product owner ruled (inbox, 2026-11-07) that the board holds only product stories and product bugs; a process improvement is a proposed change to a document, voted for adoption through Decider after the retro; the current process issues are converted: closed as not planned with the label not-card, their substance moved to docs/retros/experiments.md (section dated 2026-11-07) or applied directly where the product owner already ruled. Re-rank 27 ranks product stories and product bugs only.
- Closed by this re-rank (twelve, through scripts/board/apply_value_verdicts.py, label not-card added by the facilitator): 305, 308, 310, 311, 312, 316, 317, 318, 319, 320, 321, 322 (309 was closed earlier). Where they went: 316 and 322 are adopted Deciders (D193, D196), applied as trial paragraphs in the working agreement; 320 is the product owner's own ruling, applied to the Decider protocol; 305, 308, 310, 311, 312, 317, 318, 319 and 321 are PROPOSED document changes for the next retro's vote, each with its evidence and acceptance checks in experiments.md.
- The seat's order, minus the converted cards: 299 first (the one at-risk bug: committed work lost twice; its risk line is added to the card by the facilitator after the vote), then the 94 product cards in their current order, then the bugs 315, 307, 304, 302, 300, 301, 286, 232 and card 156 below them. Card 305 is no longer retyped to a bug; it is a proposed document change. The head has one bug above the first product story (guard: at most 4). Artifact: rerank-27-verdicts.json (12 closures, 10 Riley re-points, an order of 104 cards); the apply script's dry run validates it (12 closures, 10 edits, one rerank).
- Open question for the product owner (not blocking): bugs 315, 307, 300, 301, 286 and 232 are bugs in the mob's own tooling, not product bugs. They stay on the board below the product work as bugs (the product-first ruling says to re-check 315, 299 and 307 and rank the rest below product); if the product owner reads "product bugs only" as excluding them, they move to experiments.md as proposed changes too.
