# Card 441: the facilitator's rulings on the architect's slice (2026-11-15)

The slice: slice-architect.md (architect: claude opus = claude-opus-5-5, ctx-1, D264; measured in its own probe outside the repositories; 13,582 bytes). The D263 corpus: corpus.txt, 8 families and 104 rows (compiler and linker, environment lists, container volumes, passwd lines, JSON logs, git/ssh/scp/rsync remotes, stack traces, and card 439's MINOR shapes); the seven real families show 0 leaks and 0 second-pass changes; the only leaks are in pinned MINOR rows. Card 439's 405 committed rows are unchanged; 239 of 239 data rows equal today; 79 of 79 mutants killed; sweeps A, B and C 0 new leaks and 0 second-pass changes.

1. `ssh://host/home/` + name gives `ssh://host/repo.git` (the path's leading `/` kept): ACCEPTED.
2. A one-letter name before `:/` with no root after gives `<path>:/data` (stable; the regex gave `<path>data`): ACCEPTED; no committed row changes.
3. Slash flags (MSVC `/I`, `/Fo`) are one or two letters and attach only before `X:\` or a UNC path: ACCEPTED (keeps `/proj:/app` as plain text).
4. The two corpus rows that hold a literal name (`u1a`, `https://a/` + `Users/x`) go into test code as split strings, not into corpus.txt: ACCEPTED.

A breach to record: the slice's three sweeps and its timing runs ran after 08:05:09Z while play-tracker's train 56 held the heavy-run lock. The facilitator's interim helper read the lock wrong (it parsed only a lower-case `pid=` and only Windows pids; play-tracker writes `PID=` with an MSYS pid), so it printed `free`. The helper was fixed at about 08:25Z; the architect then held its re-runs. The timings in the slice were taken on a loaded machine (the product's `/=-I` measured 5.7 ms instead of 2.3 ms at 20,000 characters) and are re-taken after the lock clears; the sweep counts are deterministic and stand.

## Rulings on the fix slice (review round 1, 2026-11-15)

The fix slice: slice-fix.md (the architect, ctx-1; built and measured only after play-tracker's heavy lock cleared). Measured: all 11 Java and URI forms equal the pre-435 copy and the replaced regex; escaped JSON lists read as lists; `-v D:/a:/a` and the other forward-slash volume specs stable; the corpus now 11 families and 130 rows (26 added under `# fix round 1`) with 0 new leaks and 0 second-pass changes; card 439's 405 and card 441's 207 committed rows unchanged; 86 of 86 mutants killed (the reviewer's three survivors and four new ones included); sweeps A, B and C 0 new leaks and 0 second-pass changes; every shape linear.

1. Percent-decoding only `documents%20and%20settings`, nothing else: ACCEPTED; general URI decoding would be a new card.
2. Any `X:/` may start where the previous path ended (the exception that keeps the volume specs stable, matching the scanner before this card): ACCEPTED.

## Rulings on the fix slice (review round 2, 2026-11-15)

The fix slice: slice-fix2.md (the architect, claude opus = claude-opus-5-5, a fresh context for ctx-1). Measured: the pre-fix reference (PathScanV6) equals the green at 389d790 on 1,918 rows in both forms and the 11 blow-up units; both BLOCKING findings fixed behind new mutant switches; the only changed outputs are the finding and MINOR 3 rows; no committed row, sweep (A 0, B 4,559, C 2,679 leaks, unchanged) or blow-up second pass changed; the corpus now 151 rows in 11 families (21 added under `# fix round 2`); 93 of 95 mutants killed, m4 and m11 named equivalent; every shape linear (the worst 81 ms at 1,000,000 characters).

1. Finding 2's root exception: the lone letter is kept before an escaped account root (`d:\/home\/` gives `d:<path>`), like the unescaped rule, rather than the reviewer's `<path>:<path>`: ACCEPTED (stable, no leak, consistent with the unescaped form).
2. MINOR 5 (an escaped `X:\/` right after `\/` not refused like rule 3e): stays a PINNED row with its current output; the measured narrow fix (`cand-minor5`) changes the m10 killing row, so it fails clause 3 of D264; it goes to the follow-up card with the other older MINORs.
3. The forward-slash sibling of MINOR 3 (`C:\Users\` + name + `(1)xC:/a/y` changes on pass 2, no leak; unstable in V6 too): a NAMED LIMIT of this card, not a row (an unstable output cannot be pinned) and not fixed here (the fix changes two of card 439's committed rows); it goes to the follow-up card.
4. MINOR 3 fixed by a narrow change (an escaped `X:\/` starting where a word glued to an account tail ended is a drive): ACCEPTED; no other row changes.

A breach to record: one chained probe command in the architect's session ran a few-second pass with the already-built binary right after the lock check printed `busy` (play-tracker's #1017 E2E); no build ran; the pass was re-run after the lock cleared and its numbers come from the re-run.
