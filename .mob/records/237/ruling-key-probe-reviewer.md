# Card 237 ruling: key-name probe deviation (independent reviewer, audit lens, ctx-4)

Intent (D165): rule, in git before the one-commit cap raise and before run 2, whether the delivery lead's membership probe of the checker (turn-pilot-delivery.md:9) compromised the miss key, as the first part of the condition adopted in .mob/records/decisions/237-cap-fired.json (adopted_conditions).

Assumes (D166): the checker's unknown-key message names only the ledger run_key and nothing from the key entry; probe: I read scripts/check-wiki-trial.py:96-107, miss-key.json (as a non-runner) and the one run's tool log. FALSE would be a message that echoed rule, test or route text, or a run log with a read of the key.

## Ruling

(i) The key is not compromised. Only run-key names leaked, and they carry no rule text. The miss tests stay valid and the trial proceeds under option (d). Option (b) (proposal-cap-fired-architect.md:48) is not triggered.

## Evidence

- What an unknown-key error discloses: check_rows builds `known` from the run_key fields only (scripts/check-wiki-trial.py:96) and the finding is `<ledger path>: unknown row key <rk>` (:107). It echoes the candidate the prober typed, which the prober already knew. A name that is absent from the findings is a name in the key. The probe learns membership of strings, nothing else: no rule, no grader_test, no wiki route, no agreement line (those fields sit in miss-key.json:9-18 and the other entries and are never read by the message).
- Do the names hint at the rules? No. The 12 names are the full cross product of arm {baseline, wiki} and the six roles in freeze.json role_order (freeze.json:22-57) plus a stage suffix -1. miss-key.json:5 says the same rule is seeded for a role in both arms, so the names do not differ by rule. The arm and role set was already public to the runner through the frozen tasks and freeze. The only new fact is that all 12 cells hold a key and that the suffix is -1, which follows from "one stage A run per cell". That confirms a rule exists per role, but not which rule; the ready-seat rule space is the whole agreement.
- Could the one completed run have used the names? No. (1) The run read only TEAM_WORKING_AGREEMENT.md and board/227-... (turn-pilot-delivery.md:9; runs/baseline-ready-1.tools.jsonl seq 1-5: two Reads of the agreement, two Globs of board/227*, one Read of the board card); the rest are writes and size checks of its own output file. No call touches .mob/records/237 or miss-key.json. (2) The clone was sparse, without .mob/records/237/ (turn-pilot-delivery.md:8). (3) The run key appears in the report only because the wrapper supplied it as the output name (report line 1). (4) Even a run that knew the names would learn no rule text.
- Residual weakness, stated: the probe was a breach of the secrecy intent (turn-key-qa.md Secrecy finding), done by the one seat that may not see the key. I found no sign it changed what was written; the output was authored before the ledger was keyed, and the report scores nothing yet (misses unscored, turn-pilot-delivery.md:30).

## Guard for the remaining runs

1. Names are no longer a secret: the format is `<arm>-<role>-1` with arm in {baseline, wiki} and role in {ready, slice, red, green, ship, signoff} (stage B would be -2). The runner uses this and needs no further probing.
2. No further checker runs by the runner on any ledger with invented or candidate names, and no run of the checker in a scratch clone with a key file present, during the remaining runs. The checker is run only on the real ledger after the last run, in the main checkout.
3. Runs keep the sparse clone without .mob/records/237/, identical for both arms.
4. I (reviewer) check, per run, runs/<run_key>.tools.jsonl and the raw session log for any Read, Glob, Grep, Bash or PowerShell call naming .mob/records/237, miss-key.json or trial/, and I check the runner's shell history and the probe clone for further checker invocations. Any hit on rule text voids that run's miss test and I escalate to option (b) for the remaining runs.

No change was made to thresholds.json, miss-key.json, the freeze or the checker.
