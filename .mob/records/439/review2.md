# Card 439 review round 2, on 4e4893d (the port of the fixed reference)

VERDICT: FLAG; new BLOCKING: 2; new MINOR: 4 (D259's guard: round 1 had 8 and 2). Seat: QA (claude opus = claude-opus-5-5, ctx-6, reseated by D262). Raw: `review2-raw.md`. The facilitator's replay of findings 1 and 2: `review2-verify.md` (the reference and the port give the same output: the defect is in the design).

Round 1's BLOCKING findings 1, 4, 5, 7 and 8 are fixed; 2 and 3 do not reproduce; every one of 71 timed shapes is linear (at most 1.9 ms at 20,000 characters and 101 ms at 1,000,000).

| # | Finding | Replay | Class |
|---|---|---|---|
| 1 | Unix colon lists leak the account folder: `docker run -v /home/<user>/work:/home/<user>` gives `woralice`-style output, `PYTHONPATH=/home/<user>/lib:/home/<user>` gives `lialice`-style output (the adjacency rule reads the last letter before `:/` as a drive); the regex and the pre-435 copy give `work:<path>` and `lib:<path>` | confirmed (reference and port) | BLOCKING (real: docker volume specs, PATH-like variables, CLASSPATH, rsync errors) |
| 2 | Every Unix colon list is mangled without a leak: `PATH=/home/<user>/bin:/home/<user>/.local/bin:/usr/bin` gives `PATH=bibibin` (regex: `PATH=bin:bin:/usr/bin`) | confirmed | BLOCKING (a regression on a real shape) |
| 3 | Older leaks the regex and the pre-435 copy share (MSVC glued `/I`, `-isystem/home/...`, `~name/`, JSON-escaped slashes, `\?\UNC\...`, URLs, `a/b:/home/...`) | reviewer | MINOR (older; corpus rows for the follow-up) |
| 4 | Device-path prefixes `\?\` and `\.\` differ from the regex, no row pins them, no leak | reviewer | MINOR |
| 5 | Seven mutants survive the four slices, with killing rows (one, `wordends-no-dash`, leaks on a spaced hyphenated account name) | reviewer | MINOR; the killing rows join the fix round's red |
| 6 | Dead code: the `/mnt/` candidate in `AccountMarkerAt` | reviewer | MINOR; removed in the fix round if the architect agrees |

## Fix round 2 (test-first)

The architect narrows the adjacency rule so a letter glued after a Unix path name never starts a drive (rows 3.4f and 3.4h must still pass), measures colon lists of every kind the reviewer named, and checks finding 5's killing rows; the red adds the colon-list rows and the seven killing rows; the green ports the fixed reference; the gate; review round 3 on the same QA seat.
