SIGN-OFF

# md63 cross-family review (agy, gemini-3.8-flash-medium, fresh)

Range `git diff f281d53..220b674` (card-63) reviewed in clone. No repository files or tests modified.

## Scenarios
- [Z] Met. In `facilitator.py:563-565`, a zero-exit reply under `RATE_LIMIT_REPLY_MAX_CHARS` (600) is returned ok when `expect_vote` is true and `reply_first_line_is_vote` matches the vote pattern on the first non-empty line (`facilitator.py:226-238`). `decider.vote_seat` passes `expect_vote=True` (`decider.py:220`).
- [O] Met. A genuine usage-limit notice without a vote first line (such as "You've hit your usage limit ... try again at ...") fails `reply_first_line_is_vote` and is returned not-ok. "No quota left" fails because a vote word followed by whitespace and another word is not treated as a vote line.
- [M] Met. Short replies that cite or quote proposal text containing limit keywords (e.g. "Yes. muse's 429 ... resets at 05:05") return ok, and the raw combined text is preserved intact so citation lines (`CITE:`) survive.
- [B] Met. `RATE_LIMIT_REPLY_MAX_CHARS` remains 600 characters. Replies longer than 600 characters and invocations without `expect_vote=True` behave identically to pre-card-63 behavior.
- [I] Met. In `decider.py:174-175`, `attempt_reason` returns `read as a usage-limit notice: <excerpt>` when `not ok` and `rc == 0`.
- [E] Met. Rate-limit sign strings in `RATE_LIMIT_SIGNS` and detection logic in `looks_rate_limited` are unchanged; all characterisation tests pass.
- [S] Met. Backend probes (`facilitator.py:844`, `facilitator.py:866`) do not pass `expect_vote`, defaulting to False; probe quota detection remains unaffected.

## Check 1: Real CLI Reply Shapes vs parse_vote
Evaluated with test text followed by " quota ran out ...":

| Reply Shape | reply_first_line_is_vote | parse_vote | Consistent |
|---|---|---|---|
| `Yes.` | True | Yes | Yes |
| `   Yes.` (leading spaces) | True | Yes | Yes |
| `Yes` (bare on first line) | True | Yes | Yes |
| codex output cleaned by `clean_output` | True | Yes | Yes |
| `**Yes.**` / `**Yes**.` (markdown bold) | False | none | Yes |
| `- Yes.` (list bullet) | False | none | Yes |
| `\ufeffYes.` (Byte Order Mark) | False | none | Yes |
| `"Yes."` (quoted) | False | none | Yes |

Both `reply_first_line_is_vote` and `parse_vote` reject markdown, bullet, and BOM shapes, meaning no regression is introduced relative to existing parsing behaviour.

## Findings
1. MINOR: `_VOTE_FIRST_LINE` punctuation set is narrower than `parse_vote`'s boundary rule (`\b`).
   - Evidence: `facilitator.py:226-229` vs `decider.py:45`.
   - Reproduced cases:
     - `Yes — quota discussion` (em dash) -> `reply_first_line_is_vote` False, `parse_vote` 'Yes'
     - `Yes – quota discussion` (en dash) -> `reply_first_line_is_vote` False, `parse_vote` 'Yes'
     - `Yes (quota discussion)` (parentheses) -> `reply_first_line_is_vote` False, `parse_vote` 'Yes'
     - `Vote: Yes.` (preamble on line 1) -> `reply_first_line_is_vote` False, `parse_vote` 'Yes'
   - Context: The binding slice in `~/projects/phrase-lang/.mob/records/md63/declaration.md` specifically specified punctuation `[.,!:;\-]|$`, which the green implemented accurately. The prompt template instructs voters to state their vote at the beginning. This is non-blocking, but em-dash and en-dash punctuation may be candidate additions for future refinements.

2. MINOR: Potential false negative for notices coincidentally beginning with a vote word and punctuation.
   - Evidence: `facilitator.py:227` (`_VOTE_FIRST_LINE`).
   - Reproduced cases:
     - `No. You have reached your quota.` -> `reply_first_line_is_vote` True
     - `No-one has quota left.` -> `reply_first_line_is_vote` True (due to `-` punctuation match)
   - Likelihood & Consequence: Extremely low in production because provider API and CLI notices use phrases like "Rate limit reached", "Quota exceeded", or "Too many requests". If encountered, `parse_vote` extracts a vote but lacks a valid 6-word `CITE:`, so `counted` evaluates to False (`decider.py:603-604`). An uncounted non-empty vote is omitted from the tally (`decider.py:622-623`), safely leaving the seat uncounted rather than recording a false No vote.

3. Check 3 verification (no finding): `rc == 0` rule in `attempt_reason` is safe.
   - Evidence: `decider.py:174-175` and `facilitator.py:468-565`.
   - In `_launch_backend`, all error exits before process completion (template validation, unresolvable executable, command line length limit, stall, timeouts, and missing binaries) return prior to `exit_status.append`, yielding `rc = None`. Non-zero exit codes return with `proc.returncode != 0`. The usage-limit check (`facilitator.py:563-565`) is the sole exit point where `ok is False` and `rc == 0`.

4. Check 4 verification (no finding): Callers of `run_backend` and probe invariant.
   - Evidence: `decider.py:220` is the only caller passing `expect_vote=True`. `run_turn` (`facilitator.py:598`), `command_capability_probe` (`facilitator.py:844`), and `probe_backend` (`facilitator.py:866`) all use default `expect_vote=False`. Test fakes accept `**kwargs` or explicit signature. Probes and card turns remain untouched.

## Run
`python -m pytest tests/test_vote_reply_not_limit.py tests/test_decider_record_units.py -q --capture=sys`: 56 passed, 14 subtests passed in 6.83s
