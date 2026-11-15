# Proposal for Decider D269: the task-force declaration and card text for mob-defaults #61 (every new seat-use row carries its service, model, work type and outcome, and a failed seat does not count as a rotation turn)

Facilitator, 2026-11-15 UTC. VOTE on this declaration: adopt the card text of .mob/records/md61/ready-body.md as mob-defaults #61's Ready text, with the task force below. That text is the issue body, written from the card template; card_ready.py with mob-defaults' PERSONAS.md prints ok. Each role's service and exact model is pinned before any seat runs.

The card comes from the owner's dispatch audit of 2026-11-15 and the 23:20Z watchdog: "add the model, work-type and outcome fields to seat-use.jsonl". The code is in mob-defaults (`facilitator.py` `CardRun.commit_seat` and `check_seat`, `decider.py` `start_voters`). The seat-use history is each mob's `.mob/records/seat-use.jsonl`. The Avowry mob runs it while card 422's sign-off waits on play-tracker's heavy lock (#1069, since 22:51Z). This is light work only: unit tests of mob-defaults, with no acceptance project and no heavy run. It changes no agreement text, so no confirmation step runs.

The seat plan cites the live probe .mob/records/md61/probe-all-2026-11-15.jsonl, taken at about 23:22 UTC:
- available: agy, claude and cursor;
- probes ok but stays unseated: qwen (a heavy local model under the heavy-run rule);
- out: codex, copilot and muse (quota) and grok (unavailable, 402).

STANDING RULES:
- the card template and its Ready check;
- the kickoff checklist (the model item);
- red first;
- D259 clause 4;
- D260 (pushes read unpiped, the remote head equal to the local commit);
- D267 (stop a waiting seat before its runs are taken over);
- the account-name ruling in every brief.

D263 and D265 do not apply. The change records data; it does not recognise text.

## Task force (each role in its own context; models pinned at launch)

- Dev, red: claude sonnet = claude-sonnet-5-5 | why: the scenarios name the rows and the fields; mechanical test writing at the mid tier (0 expectation errors across cards 430 and 422) | context: ctx-2
- Dev, green: cursor, `--model gpt-5.3-codex` | why: a small code change to the writer and the rotation reader, code-writing tier, pinned | context: ctx-3
- QA, adversarial review: claude opus = claude-opus-5-5 | why: rotation and append-only history are easy to break subtly (old rows, concurrent runs, a missing outcome line); strong tier | context: ctx-4
- Sign-off: claude sonnet = claude-sonnet-5-5 | why: runs the mob-defaults unit suite in a fresh clone and judges, mid tier; a different seat from the green | context: ctx-5

There is no architect seat. The facilitator writes the slice: which functions change and the row format, with play-tracker's field names. The red brief carries it.

Domain-expert question at refinement: should an agent with different domain knowledge join? No. Play-tracker's seat log is the reference format and is read directly.

## Clauses

1. Adopt the Ready text of .mob/records/md61/ready-body.md as mob-defaults #61's text.
2. Adopt the task force above, with each role's service, exact model and reason, as declared before any seat runs.
3. Work only in a linked worktree of mob-defaults, with light runs only. The facilitator's slice is part of the red brief and is committed with the red.
