# Card 441: the facilitator's rulings on the architect's slice (2026-11-15)

The slice: slice-architect.md (architect: claude opus = claude-opus-5-5, ctx-1, D264; measured in its own probe outside the repositories; 13,582 bytes). The D263 corpus: corpus.txt, 8 families and 104 rows (compiler and linker, environment lists, container volumes, passwd lines, JSON logs, git/ssh/scp/rsync remotes, stack traces, and card 439's MINOR shapes); the seven real families show 0 leaks and 0 second-pass changes; the only leaks are in pinned MINOR rows. Card 439's 405 committed rows are unchanged; 239 of 239 data rows equal today; 79 of 79 mutants killed; sweeps A, B and C 0 new leaks and 0 second-pass changes.

1. `ssh://host/home/` + name gives `ssh://host/repo.git` (the path's leading `/` kept): ACCEPTED.
2. A one-letter name before `:/` with no root after gives `<path>:/data` (stable; the regex gave `<path>data`): ACCEPTED; no committed row changes.
3. Slash flags (MSVC `/I`, `/Fo`) are one or two letters and attach only before `X:\` or a UNC path: ACCEPTED (keeps `/proj:/app` as plain text).
4. The two corpus rows that hold a literal name (`u1a`, `https://a/` + `Users/x`) go into test code as split strings, not into corpus.txt: ACCEPTED.

A breach to record: the slice's three sweeps and its timing runs ran after 08:05:09Z while play-tracker's train 56 held the heavy-run lock. The facilitator's interim helper read the lock wrong (it parsed only a lower-case `pid=` and only Windows pids; play-tracker writes `PID=` with an MSYS pid), so it printed `free`. The helper was fixed at about 08:25Z; the architect then held its re-runs. The timings in the slice were taken on a loaded machine (the product's `/=-I` measured 5.7 ms instead of 2.3 ms at 20,000 characters) and are re-taken after the lock clears; the sweep counts are deterministic and stand.
