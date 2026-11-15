# Card 439 review round 3, on 4b9966b (the port of the second fixed reference)

VERDICT: FLAG; new BLOCKING: 3; new MINOR: 3 (rounds so far: 8 and 2, 2 and 4, 3 and 3). Seat: QA (claude opus, ctx-6, D262). Raw: `review3-raw.md`. The facilitator's replay: `review3-verify.md` (the reference and the port agree: the defects are in the design). Round 2's findings 1 and 2 are fixed on every colon-list input; every round-2 killing row behaves as ruled; every one of 21 re-timed shapes is linear (at most 2.1 ms at 20,000 characters).

| # | Finding | Replay | Class |
|---|---|---|---|
| 1 | Docker volume specs with a Windows drive or UNC host leak: `docker run -v C:\Users\<user>\proj:/home/<user>` gives `proalice`-style output (regex and pre-435: `proj:<path>`); the adjacency rule still holds for any `X:/` after a drive or UNC path | confirmed | BLOCKING (Docker Desktop and Podman on Windows, their errors) |
| 2 | The same mechanism mangles them without a leak: `-v C:\Users\<user>\proj:/app` gives `proapp` (regex: `proj:/app`) | confirmed | BLOCKING |
| 3 | An account folder followed by `:/` changes on a second pass: `docker run -v /home/<user>:/data`, `/home/<user>:/bin/bash` (an `/etc/passwd` line); pass 1 keeps the account's last letter, as the regex does, pass 2 drops it | confirmed | BLOCKING (new in fix round 2) |
| 4 | The one-letter rule changes on a second pass when no root follows (`-v ~/x:/data`) | reviewer | MINOR (contrived; a corpus row) |
| 5 | Three mutants of the new rules survive, with killing rows | reviewer | MINOR; the killing rows join the fix round's red |
| 6 | Older contrived shapes (`/tmp/x://server/...`, `/tmp/x'y:/...`) | reviewer | MINOR |

The reviewer measured two candidate fixes as mutants that pass all four slices: finding 1 and 2, adjacency after any path only for `file:///` or a backslash drive; finding 3, the one-letter rule only at the start of the final name or after a word letter, before `:/`.

## Fix round 3 (test-first)

The architect evaluates both candidates against everything (the data rows, the review inputs of all three rounds, the mutants, the three sweeps, the timings) and either adopts them or replaces them with a measured alternative; then the red, the port, the gate, and review round 4.
