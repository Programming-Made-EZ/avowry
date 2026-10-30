# Observation for the next stand-down: the GitHub REST quota ran out (2026-10-30, 08:22 to 08:33 UTC)

Raised by the facilitator at the product owner's request, as an observation for the team to decide an experiment on. Not a decision.

## What happened
- The shared token's REST core quota (5,000 per hour) reached 0 at about 08:22 UTC and reset at 08:33:03 UTC. GraphQL still had 4,058 left.
- Pushes were blocked as well, but not by `git push` itself, which spends no REST quota. The D134 pre-push hook (.githooks/pre-push) runs `scripts/mob_process.py check`, and that check calls `gh`, so it failed with "gh failed after 3 attempts: API rate limit exceeded".

## Measured cost per call (09:0x UTC, same token)
- One `python scripts/mob_process.py check` costs 92 REST calls. `read_issues` fetches the timeline of every closed issue, one `gh api repos/.../issues/N/timeline --paginate` call per issue, 93 closed today. The count grows with every closed card.
- One `python scripts/check-ci-main.py` poll costs about 1 REST call. `--wait` polls every 30 s, so a 25-minute wait costs about 50.

## Top consumers in the 07:33 to 08:33 window (counted from the session transcripts)
1. `mob_process.py check`. The facilitator ran it by hand 18 times, the pre-push hook ran it on 14 push attempts, and seat agents ran it, or test runs that include it, about 11 times. That is about 43 runs at about 92 calls each, roughly 3,900 calls, about 80% of the quota. The facilitator's manual runs right before a push were pure duplicates of the hook.
2. CI polling. One capped `check-ci-main.py --wait 1500` at 30 s intervals, plus 9 direct `gh run list`/`view` calls: about 60 calls.
3. The facilitator's reset poller (Copilot quota every 5 minutes) and probes: about 15 calls. Stopped at 08:23.
4. Board scripts (`rerank.py` renumbering 123 items, `generate_archive.py`, `new_card.py`) mostly use the GraphQL project API, not REST.
5. The external auditor's `gh` reads share the same token. Their volume is not visible from here and is not counted above.

## Applied now by the facilitator (practice, reversible, no code change)
- No manual `mob_process.py check` before a push; the pre-push hook runs it once.
- Pushes are batched: one push per boundary, not one per commit group.
- CI is read with one call per check, at intervals of at least 120 s, never in a tight wait loop. The background reset poller stays off until a reset is due.
- Only the gh-dependent steps wait on quota (issues, board, CI reads). Everything else (windows, seats, commits) continues.

## For the team to decide (experiment candidates, with hypothesis, metric and counter-metric)
- A. Cache closed-issue timelines in `read_issues`, keyed by issue number and `closedAt`, so a check re-reads only issues closed or changed since the last run. Metric: REST calls per check (92 today). Counter-metric: a missed un-labelling event (the reason the timelines are read at all).
- B. Raise `check-ci-main.py`'s poll interval and give it a call budget. Metric: calls per wait. Counter-metric: minutes from a red CI to stop-the-line. The fb13e3d red was missed for about 40 minutes (F-056).
- C. A quota guard: `mob_process.py` prints the remaining REST quota and refuses to start a check that would exhaust it. Counter-metric: blocked pushes.
