SIGN-OFF

# md63 QA review (claude-opus-5-5, ctx-63q, fresh)

Range `git diff f281d53..220b674` (card-63), read in a read-only clone. No repo file was changed. The shape probes were run from a scratch script with `python -I`, with no CLI and no network.

## Scenarios
- [Z] Met. `facilitator.py:226-238` (`_VOTE_FIRST_LINE`, `reply_first_line_is_vote`) and `facilitator.py:563-565`: when `expect_vote` is set, a short zero-exit reply whose first non-empty line is a vote followed by punctuation or the end of the line is returned ok. `decider.py:220` passes `expect_vote=True`.
- [O] Met. "You've hit your usage limit ..." has no vote line and is still not-ok. "No quota left" is still a notice because a vote word followed by a space and a word does not count.
- [M] Met. "Yes. muse's 429 ... resets at 05:05" is ok. The function returns `combined` unchanged, so the CITE line is kept.
- [B] Met. `RATE_LIMIT_REPLY_MAX_CHARS` and `looks_rate_limited` are unchanged. With `expect_vote` false, the code path is the same as before.
- [I] Met. `decider.py:174-175` gives `read as a usage-limit notice: <excerpt>` when ok is false and rc is 0.
- [E] Met. `RATE_LIMIT_SIGNS` is unchanged, and the characterisation tests pass.
- [S] Met. The probe calls at `facilitator.py:844` and `facilitator.py:866` do not pass `expect_vote`, so it defaults to False. The other callers (`run_turn` at `facilitator.py:598` and the CardRun `call_backend` at `facilitator.py:2280`) are also unchanged. Every test fake of `run_backend` in the decider tests takes `**kw`, so the new keyword breaks none of them.

## Check 1: which reply shapes count, compared with parse_vote (reproduced)
Each case below is the shape followed by " muse's quota ran out ...".

| Shape | helper | parse_vote |
|---|---|---|
| `Yes.` | yes | Yes |
| `   Yes.` (leading spaces) | yes | Yes |
| `Yes` on its own line | yes | Yes |
| codex: raw echo cleaned by `clean_output` to `Yes. ...` | yes | Yes |
| `**Yes.**` / `**Yes**.` | no | none |
| `- Yes.` | no | none |
| BOM + `Yes.` | no | none |
| `"Yes."` | no | none |

The helper and parse_vote agree on all of these. The markdown, list and BOM shapes were already lost before this change, and both functions still reject them, so the change introduces no regression there.

## Findings
1. MINOR: the helper is narrower than parse_vote, so some votes are still discarded when they contain a limit word. The following shapes are counted by `parse_vote` (`decider.py:45`, which ends in `\b`) but rejected by `_VOTE_FIRST_LINE`. Each was reproduced:
   - `Yes — ...` (em dash) and `Yes – ...` (en dash)
   - `Yes because ...`
   - `Yes (...)`
   - `Vote: Yes.`
   - narration on line 1 with `Yes.` on line 2

   Any of these that also contains "quota", "429", "resets at" and so on is still read as a usage-limit notice. The em dash form is a realistic model style. This follows the binding slice (its punctuation set is `.,!:;-`), and `build_prompt` (`decider.py:133`) asks for the vote as the first words. So this is not a defect against the card. It is worth a follow-up: add `–` and `—` to the set, or let the helper use parse_vote's own rule.
2. MINOR: there is a narrow false-negative path. "No. You have reached your quota.", "Yes, you've hit your usage limit." and "No-one has quota left" (the `-` after `No` is accepted) now pass as votes; all three were reproduced.
   - Likelihood: low. The real notices observed in this project start with "You've hit ...", "Quota exceeded ..." or "Rate limit ...", never with a vote word.
   - What follows: `parse_vote` gives No (or Yes), and `vote_seat` stops retrying because the vote is not none (`decider.py:236-238`). A notice has no 6-word CITE, so `counted` is false (`decider.py:603-604`). `voter_outcome` still records `quota_exhausted` because the reason text contains "quota" (`decider.py:332`).
   - The effect on the tally: before this change, the discarded notice gave an empty reply with vote none, which was included in the tally and failed the decision. Now the reply is non-empty and uncounted, so it is left out of the tally (`decider.py:622-623`). The decision can therefore pass with one fewer voter instead of failing.

   So in this case a misread notice is not counted as a No. Instead the seat quietly drops out of the tally. This is acceptable given how unlikely it is, but it should be noted.
3. Check 3, no finding: the `rc == 0` rule is safe. Every other not-ok return in `_launch_backend` leaves the rc as None or non-zero:
   - The launch refusals (`facilitator.py:466-467` and `488-491`), the stall and timeout paths (`527-537`) and FileNotFoundError and TimeoutExpired (`538-547`) all return before `exit_status` is appended, so `LaunchResult` gets rc None.
   - A non-zero exit returns `failure_excerpt` with that rc (`facilitator.py:557-558`).

   The only path that is not ok yet has `exit_status == [0]` is the usage-limit check at `facilitator.py:563-565`.

## Run
`python -m pytest tests/test_vote_reply_not_limit.py tests/test_decider_record_units.py -q --capture=sys`: 56 passed, 14 subtests passed in 5.46s
