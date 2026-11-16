# mob-defaults #58: the facilitator's slice (light work, drafted while #61 waits; 2026-11-16)

The card: `gh issue view 58 -R Programming-Made-EZ/mob-defaults`. Its scenarios [Z][O], [M][B], [I], [E] and [S] are the contract. The Mob Board ranks it next after #61 (Dispatch, 2026-11-16 about 03:20 UTC), because the rule blocked flow on #61's Decider D273. This slice becomes binding once the card's declaration Decider adopts it, and it builds on #61's code: outcome lines, and the probe in force read at each voter's turn.

## The evidence from 2026-11-16 (Avowry, `.mob/records/md61/`)

The D164 rule in `mob_process.claude_wait_problem` refused a Claude Decider seat five times in about 40 minutes:
- for qwen ("has waited longer") while qwen had never been seated;
- for muse while muse's quota was exhausted, because the probe in force still read ok from 00:05 UTC;
- for cursor and agy, which already held the other roles in that same window.

Each refusal left a role without a counted vote, and the vote waited two hours for muse's quota reset.

## Design

1. **Usable now.** `claude_wait_problem` counts a non-Claude model as "waited longer" only when it is usable now. Usable now means all of these hold:
   - (a) its probe in force is ok and less than 60 minutes old at the voter's turn. Otherwise it is re-probed first, and a not-ok re-probe means not usable;
   - (b) it is not gate-blocked. For qwen that means no cross-mob heavy lock held by another mob and no test process alive, the same test `local_driver.py` uses;
   - (c) it is pinnable: its launcher accepts the declared model, so cursor counts only with a named `--model` that the launcher can pass (#57), and a launcher with no model pin is not usable;
   - (d) it is not already seated in a role of this same run (the pending voters in `check_voters`), so a model voting in this window has not "waited longer".
2. **The refusal names the alternative [I].** A refusal names the first usable model to seat instead, and, for every model it skipped, why: stale or not-ok probe, gate busy, not pinnable, or seated in this run.
3. **Nobody else usable [E].** When no other model is usable, the Claude seat starts, and its rotation reason says so: "no other model is usable now", with each model's skip reason.
4. **The agreement template [S].** The rotation rule's text in `TEAM_WORKING_AGREEMENT.md` says it counts only models that can be seated now, and lists the four conditions.

## Tests (red first; Python, `--capture=sys`; a fixed clock, no wall-clock asserts)

There is one file, `tests/test_rotation_usable_now.py`, with at least one test per scenario. It reuses the fixtures of `tests/test_seat_rotation.py`. It covers:
- a stale ok probe re-probed to quota (the muse case);
- a never-seated model that is gate-blocked (the qwen case);
- a pinless cursor;
- a model already voting in the same window;
- the refusal text naming the alternative and the skip reasons;
- the nobody-else-usable start.

It also proves old behaviour is unchanged: with every model usable, today's rotation tests pass as they are.

## Seats (proposed for the declaration; ruling R-ROT applies, no service with more than two seats)

- red: claude sonnet;
- green: a healthy pinned writer (cursor composer-2.5 or muse medium);
- QA: claude opus, plus a cross-family reviewer;
- sign-off: a service other than the green's.
