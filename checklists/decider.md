# Any Decider

When: whenever a Decider runs: at refinement, at a cycle boundary or at a retro. The loops that run one point here.

- The voters are the declared roles of the card's task force, each filled by a probed model in its own fresh context (`decider.py --roles ROLE=MODEL,...`); the facilitator votes only when it fills a declared role. Gate: `mob_process.py record`. Evidence: the Decider record.
- Each ballot reads `<role> (<model>): vote (reason)`. Gate: `mob_process.py record`. Evidence: the ballots in the Decider record.
- No role is absent without probe evidence. Gate: `mob_process.py record`. Evidence: the probe taken for the Decider.
- The votes are counted, and the tally groups by role. Gate: `mob_process.py record`. Evidence: the tally in the Decider record. Card: #23
