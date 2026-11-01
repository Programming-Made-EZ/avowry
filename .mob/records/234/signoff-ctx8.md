# Card 234 sign-off (product manager, ctx-8, fresh context)

Seat: Claude. In-flight line committed first (16f5bd8). Read: the card (board/234) with its adopted conditions, D170 in docs/decisions/process-and-staffing.md, .mob/records/234/record-in-progress.json (restarts, seat_ab, seat_metrics, token_usage, deviations), the six files in .mob/records/234/usage/, review-ctx6.md and review-writer-ctx3.md with their re-checks (both APPROVE), preship-gate.md (371c2ed) and the four turn records (skimmed, not re-audited line by line). Ran three read-only demo commands. Changed no code. No delete command, no git stash.

## Verdict: SIGN OFF the card. Experiment verdict: REVISE (do not promote to the template yet)

The card did what it set out to do: it built the wiki, the routing, the seat-brief consumer and a checker that refuses invented numbers. It did not yet produce evidence strong enough for a keep. Those are two different questions, and the product owner's ruling (D170) asks for both.

## Demo (read-only, real output)

- `python scripts/check-process-wiki.py` -> `process-wiki: OK (18 pages)`.
- `python scripts/check-restart-record.py` -> `restart-record: OK (2 restarts)`; seat_ab pair full 69701, brief 49032 (brief/full 0.70); one unverified row (the 949200 Copilot red 2 fix), one unverified end figure (528400, left out of the median); verified median end of 109588 over 2 contexts against 125000 (-15411.5).
- `python scripts/mob_process.py seat-prompt qa-red | head -3` -> `bytes: 6291`, then "Index of the process wiki: one line per page, each page read just in time." The seat brief is a real, consumable thing.

## Against the three Done checks

1. Indexed pages with one-line summaries, every rule resolving to its decision: MET. 18 pages, the checker passes, and the writer re-check ran a script over every Quote line against the decisions file with nothing missing. Residual page-quality gaps (below) are real but do not fail the check as written.
2. Seat briefs give the index and the needed pages only; tokens measured against baseline: MET AS A MEASUREMENT, WEAK AS EVIDENCE. See the metrics section.
3. Facilitator restarts from the wiki and handoff notes at a boundary; context measured against about 965K; rule misses counted: MET AS A MEASUREMENT, WEAK AS EVIDENCE. The promotion half of this check is conditional on a keep verdict, so a revise does not trigger it.

The adopted condition (numbers and methods stated; a git-checkable test fails if any is missing or an anchor stops resolving) holds: the checker rejected ten of ten mutations in the re-check, including invented token counts and a zero-miss claim pointed at a non-usage file.

## What the evidence shows for the experiment's metrics

Plainly: three numbers, one run each, none able to carry a keep on its own.

1. The measured pair, 69701 (full agreement) against 49032 (qa-red brief). The brief used about 30 percent fewer tokens, 20669 fewer. One run each, no variance. Two cautions: both runs include the CLI's own fixed system prompt, so the floor is not zero; and the brief run read 30341 tokens from cache while the full run created 69003 fresh, so a caching difference between two single runs is part of what is compared. Against the card's baselines the brief lands at -968 from the 50K setup figure, which is "at the baseline", not "far below it". It is also one question ("which three rules must a red commit body satisfy"), and the two answers differ in content (the full run named the no-trailer rule; the brief run did not). Nobody graded which answer was right, so token cost was measured and answer quality was not.
2. The restart, 83666 tokens against 965000, 0 computed misses. It shows a fresh context can reach the right next card, step, boundary and rule pages from HANDOFF and the index in about 84K tokens and 18 tool calls (the seat matched the key the facilitator wrote beforehand). It does not show that the facilitator's context stays low: 83666 is a fresh restart seat's reading, not the facilitator working through a card, and 965K is a long accumulated session. A fresh context is always small; what matters is how fast it grows back and how often a restart is needed. The other facilitator reading (782199 at the criterion 3 boundary, before any restart) is one point in the same long session, not a result. Zero misses is computed (the checker diffs the report's rule pages against the key's; a mutation dropping one page produced "record 0, computed 1"), which beats asserted, but it measures "did the seat read the pages the key said to apply", and the key was written by the facilitator who already knew the answer. That is a weak test of whether an unread page would cause a miss.
3. The Copilot seats, 0.9 to 6.7 million tokens (949200 for the red 2 fix, 6.7m for green 3, others between). These are inputs including cached tokens, from the Copilot CLI's own usage lines, and they are not comparable to the 50K and 125K baselines, which are Claude-seat figures; the Copilot count includes every agentic step's re-read of context, and most of it is cache reads (for example 881.8k of 937.9k). So there is no like-for-like number for the seats that did most of the work. Several Copilot seats ran with -s and are not measured at all (ctx-1, ctx-2, ctx-3, ctx-6, ctx-8 have no setup or end figure). The only verified Claude seat end figures are ctx-4 (142065) and ctx-5 (77112): median 109588 against 125000, 12 percent under, over two contexts. ctx-5's end figure is below its own setup figure (151676), so the figures are cumulative per step and noisy, not a clean setup-versus-end comparison; the record says as much.

Counter-metric (rule misses from an unread page): 0 computed in the restart; no other miss cases recorded. The more honest signal is in the reviews: the hostile-inputs page is a single line that explains nothing; rules/one-criterion carries text about the product owner setting priority rather than the one-criterion rule; two pages hold only amendment bookkeeping; the role-fit requirement is quoted nowhere. A seat routed to those pages today would miss something. They were rated non-blocking for the Done checks, correctly, but they are the counter-metric in waiting.

## Deviations and gate items, and whether any blocks

- Recorded, not blocking: two oversize reds (603 and 969 lines) and one 16-file unit commit against D136; a restart unit commit unpushed about 7 minutes; a checker test failing on main for about 4 minutes; the intent confirmation committed after red 1. None hidden, none rewritten.
- Pre-ship gate (371c2ed): item 1 FAIL only for the untracked scratch folder named as a record path (working scratch, not on origin); items 2 and 3 PARTIAL or NOT MEASURED (full suite last recorded 503 OK, ten commits earlier; CI not read without gh); item 8 PARTIAL (no command scan yet for ctx-8, this seat; the facilitator scans on return). Carried, not blocking.
- Commit-body proof for the green commits was moved by the reviewers to card 235 rather than fixed here. An honest split, not a hidden gap.
- Not verified by this seat: a full test run, avowry CI, page content quality beyond what the two reviewers wrote, and whether the briefs change how a real Copilot seat behaves (no seat other than the two measurement runs was observed doing work from a brief).

## Recommendation for the experiment's review: REVISE

Not keep: the numbers are one run each, the only like-for-like seat comparison is a single question, the restart figure compares a fresh seat with an accumulated session, and the Copilot seats (most of the real spend) have no comparable measurement. Not drop: every measured direction is the right one (about 30 percent fewer tokens on the one pair, a successful restart at about 84K, a verified median 12 percent under baseline, a working checker, an index the product owner can read) and nothing measured went the wrong way.

Revise means: keep the wiki in this repository as built (it stays in force under D170's waiver); do not promote to the mob-defaults template; fix the page-content gaps the writer named (hostile-inputs page, one-criterion page, the two bookkeeping pages, HANDOFF naming next_step, the restart seat's token line labelled facilitator-filled); and run the experiment properly across the next cards.

## What would make a later keep verdict trustworthy

1. More runs with variance stated: at least five brief-versus-full pairs per role (not one question for one role), several different questions, repeats so cache effects show, and a spread reported instead of one ratio.
2. An answer-quality check on the same pairs: fixed questions with known right answers, graded, so a cheaper answer that omits a rule (as the brief run did on the no-trailer rule) counts as a miss, not a saving.
3. Like-for-like seat numbers: setup and end checkpoints for every seat on the next three cards, Copilot seats run without -s, and one counting rule (cache included or excluded) for every seat and the baselines, so the 0.9 to 6.7 million figures can be set against something.
4. A real miss test: rules seeded in wiki pages that a seat must apply to pass, the restart key written by someone who does not know what is on the pages, repeated at several boundaries, so "0 misses" means 0 of a stated number of chances.
5. Facilitator context measured over a whole card with restarts: peak, average, and restarts per card, against the same kind of card run the old way if possible.
6. The cost of keeping the wiki true: time and tokens spent maintaining 18 pages and the checker, counted as part of the experiment.
7. One seat outside this mob family running a card from the wiki alone.

If the next three cards meet points 1 to 4 and the savings and zero misses hold, promote through the normal template-change path, including the template review (D170).

## Carried, not failed

- ctx-8 command scan (this seat) to be run by the facilitator on return.
- Full suite last recorded at 503 OK; ten commits since have not recorded a full run.
- Card 235 (commit-body proof blocks) owns the moved review item.
