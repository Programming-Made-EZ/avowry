# mob-defaults #63: kickoff declaration (2026-11-16, about 13:20Z)

Probe at 13:16:39Z (eight services): agy, claude, cursor, muse, qwen available (qwen probe-only, gate-bound); codex and copilot quota-reached; grok unavailable (402).

Worktree wt-md63, branch card-63 at f281d53 (main after #58).

| Role | Service | Model | Reason |
|---|---|---|---|
| red | claude (Agent subagent) | sonnet = claude-sonnet-5-5 | a small, precise detection change; tests around run_backend's fakes; mid tier |
| green | cursor (cursor-agent -p --force) | composer-2.5 | first-choice cursor writer (#61 and #58 retros) |
| QA | claude (Agent subagent) | opus = claude-opus-5-5 | false-positive and false-negative edges of a detection rule, strong tier |
| cross-family review | agy (plan mode, clone) | gemini-3.8-flash-medium | a non-author family |
| sign-off | muse (read-only) | default, effort medium | a service other than the green's and the reviewers' |

R-ROT: claude 2 seats, cursor 1, agy 1, muse 1. No Decider roles planned.

## Slice (binding on red and green)

1. `facilitator.run_backend(..., expect_vote=False)`: a new keyword. Only when `expect_vote` is true, a zero-exit reply of 600 characters or fewer whose first non-empty line starts with a vote word (`Yes`, `Support-it`, `No`, `ABSOLUTELY NO`, any case) followed by punctuation (`.`, `,`, `!`, `:`, `;`, `-`) or the end of the line is returned ok, whatever words follow. Without `expect_vote`, behaviour is unchanged ([B], [S]). A first line such as "No quota left" (vote word, then a space and a word) is still a usage-limit notice.
2. `decider.vote_seat` passes `expect_vote=True`.
3. [I] `decider.attempt_reason`: when `ok` is false and `rc == 0` (the only path that discards a zero-exit reply is the usage-limit check), the reason is `read as a usage-limit notice: <excerpt>`.
4. [E] and [S]: existing rate-limit tests and the probe's quota detection are unchanged.

## R63-1 (facilitator ruling, 2026-11-16 about 13:44Z): the red's end-to-end [I] test expects the wrong vote spelling

`test_63_i_vote_seat_counts_a_vote_that_mentions_a_quota_end_to_end` asserts the vote is `'yes'`; `decider.parse_vote` returns the canonical `'Yes'` (`VOTES`), as every existing Decider test expects. The green's result `{'vote': 'Yes'}` is right. The red author (ctx-63r) changes that expected value only; no other edit.

## R63-2 (facilitator ruling, 2026-11-16 about 13:52Z): the vote line accepts an em dash and an en dash

Both reviewers (Opus review.md MINOR 1, agy review-agy.md MINOR 1) reproduced `Yes — ...` and `Yes – ...` as votes `parse_vote` counts but `reply_first_line_is_vote` rejects; the em dash is a common model style. Fix round 1, test-first: the vote word may also be followed by U+2014 or U+2013. Parentheses, "because" and a preamble line stay out (the vote prompt asks for the vote as the first words). Opus MINOR 2 (a notice starting "No." now passes the check but is dropped from the tally by the CITE rule, not counted as No) is a named limit on the card.
