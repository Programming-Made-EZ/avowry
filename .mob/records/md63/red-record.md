# md63 red record

Seat: red (dev, red), claude-sonnet-5-5, ctx-63r. Branch card-63 at f281d53, worktree ~/projects/wt-md63. Not committed.
File: tests/test_vote_reply_not_limit.py (untracked). All faked: FakePopen for run_backend, mocked run_backend for vote_seat. No CLI, no network.

## Counts
- Test63Pending: 22 tests, all fail today (20 FAILED plus 2 that fail only in subtests: 3 + 4 SUBFAILED).
- Test63Characterisation: 16 tests, all pass today.

## Pending tests by scenario (failure reason today)
Every `run_reply(..., expect_vote=True)` test fails with `TypeError: run_backend() got an unexpected keyword argument 'expect_vote'` unless noted.
- [Z] test_63_z_yes_vote_that_mentions_the_quota_is_ok; _support_it_with_a_comma_and_quota_is_ok; _no_vote_that_mentions_a_rate_limit_is_ok; _absolutely_no_with_a_colon_is_ok; _vote_word_in_any_case_is_ok (4 subtests); _a_vote_word_alone_on_the_first_line_is_ok; _a_leading_blank_line_does_not_hide_the_vote_line; _the_returned_text_is_the_reply; _a_vote_reply_of_exactly_600_characters_is_ok: all the TypeError.
- [M] test_63_m_a_vote_that_quotes_the_429_and_resets_at_is_ok; _the_quoting_vote_keeps_its_cite_line: TypeError.
- [O] test_63_o_no_quota_left_with_expect_vote_is_still_not_ok; _a_usage_limit_notice_with_expect_vote_is_not_ok; _a_vote_word_then_a_space_and_a_word_is_a_notice (3 subtests); _the_vote_must_be_on_the_first_line; _a_non_zero_exit_is_not_ok_even_for_a_vote_reply: TypeError (they pin behaviour that must survive the change).
- [B] test_63_b_a_601_character_quota_reply_is_ok_with_expect_vote_too: TypeError.
- [I] test_63_i_attempt_reason_names_the_usage_limit_notice and _is_not_a_launch_failure_at_rc_0: attempt_reason returns the bare excerpt. test_63_i_vote_seat_passes_expect_vote_true: kwarg absent (None is not True). test_63_i_vote_seat_records_the_discard_as_a_usage_limit_notice: reason is the bare notice text. test_63_i_vote_seat_counts_a_vote_that_mentions_a_quota_end_to_end: vote is 'none' (reply discarded as a notice).

## Characterisation tests (pass today)
- [O] 3: notice without expect_vote not ok; reset-time notice not ok; non-zero exit not ok with its rc.
- [B] 6: vote reply plus quota without expect_vote not ok (MUSE_VOTE and Support-it); 601 chars ok; 600 chars not ok; no-sign reply ok; RATE_LIMIT_REPLY_MAX_CHARS == 600.
- [E] 2: looks_rate_limited signs; RATE_LIMIT_SIGNS list unchanged.
- [I] 3: other attempt_reason paths (rc 1, no rc, ok-without-vote/empty).
- [S] 2: probe_backend short quota reply still not ok; probe_backend never passes expect_vote.

## Runs
- `python -m pytest tests/test_vote_reply_not_limit.py -q --capture=sys`: 27 failed, 18 passed, 7 subtests passed in 2.93s
  (27 = 20 failed tests + 7 SUBFAILED entries; "18 passed" = 16 characterisation + the 2 subtest-only pending tests, which pytest counts as passed at test level though their subtests fail.)
- `python -m pytest tests/test_decider_record_units.py tests/test_decider_launched.py tests/test_card_mode.py -q --capture=sys`: 183 passed, 34 subtests passed in 1016.90s (0:16:56)

## Ambiguities
- Slice says a first line "No quota left" is a notice, and "Yes quota ..." (vote word, space, word) is one too; I pinned both. A vote word followed by a bare newline counts as end of line (ok) in [Z] tests.
- "First non-empty line": tested a leading blank line is skipped, and a vote on line 2 after a notice line is not ok.
- Exactly 600-char vote reply is ok with expect_vote (boundary is <= 600).
- A non-zero exit with expect_vote stays not-ok (not in the slice explicitly; follows "zero-exit reply").
- Slice item 3 says reason is `read as a usage-limit notice: <excerpt>`; tests use the full short notice as the excerpt (it is under the excerpt limit).
- test_card_mode.py ran ~17 minutes, over the shell timeout, so it ran in the background to completion (no process left behind).

## Red fix (R63-1)
Line 191 of tests/test_vote_reply_not_limit.py: expected vote changed from "yes" to "Yes" (parse_vote returns the canonical spelling). No other edit.
Run, with the green's production changes in the worktree: `python -m pytest tests/test_vote_reply_not_limit.py -q --capture=sys`: 38 passed, 14 subtests passed in 2.36s

## Fix round 1 red (R63-2)
Worktree at 220b674. Added class Test63Fix1 to tests/test_vote_reply_not_limit.py (no existing test edited), all with expect_vote=True:
- Pending (fail today, the vote line rejects the dash): test_63_f1_em_dash_after_the_vote_word_is_ok ("Yes — the quota ran out."), test_63_f1_en_dash_after_the_vote_word_is_ok ("No – muse's 429 resets at 05:05.").
- Characterisation (pass today): test_63_f1_a_parenthesis_after_the_vote_word_is_still_not_ok ("Yes (quota ran out)"), test_63_f1_a_preamble_before_the_vote_word_is_still_not_ok ("Vote: Yes. quota").
Run: `python -m pytest tests/test_vote_reply_not_limit.py -q --capture=sys`: 2 failed, 40 passed, 14 subtests passed in 3.02s (the 2 failures are the two dash tests).
