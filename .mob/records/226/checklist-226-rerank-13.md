# D158 pre-window checklist: window 226-rerank-13 (facilitator)

- Seats: all six non-proposer roles polled (architect, QA engineer, backend developer, independent reviewer (audit lens), delivery lead, product manager (signoff)); F-054.
- Order: one seat at a time through decider.py, because its append-only seat-use check refuses a history another run changed meanwhile (card 226).
- Caps: about 2 minutes per seat, 15 minutes for the window.
- Working directory: decider.py clones C:/Users/ringe/projects/phrase-lang; the proposal travels in the prompt, and the trail .mob/records/decisions/226-rerank-13.json is written once every reply is in, so no incomplete window is ever pushed (push on every commit, product owner ruling 2026-10-31).
- Line endings: no seat edits in a window; the trail is written with LF and checked with tr before the commit.
- Probes: 08:39 UTC (this push); decider.py also takes its own fresh probe per run.
- Backends: scratch backends-pinned-sonnet.json (claude probe_model sonnet).
