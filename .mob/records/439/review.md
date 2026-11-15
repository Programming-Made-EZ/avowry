# Card 439 review round 1 (told to try to break the linear scanner), card-439 at 1ae9d03

VERDICT: FLAG; new BLOCKING: 8; new MINOR: 2 (the reviewer's counts, D259's guard). Seat: QA (agy, gemini-4-argon-high, pinned by D261, ctx-4), 02:35:47Z to 03:23:54Z, exit 0. Raw: `review-raw.md`. The facilitator's replay, four ways (the pre-435 copy, the replaced regex at 7ee5411, the architect's reference scanner, the green): `review-verify.md`.

## Disposition (stop rule; D259 clause 4)

| # | Finding | Replay | Where the defect is | Class |
|---|---|---|---|---|
| 1 | Quadratic time on glued flags after a failed Unix walk: `/=-I` and `/(-I` repeated (compiler and Makefile flag shapes) | confirmed: at 20,000 characters the reference takes 1,435 ms and 1,328 ms, the green 2,384 ms and 1,947 ms (5,000: 240 and 74 ms reference) | the DESIGN (the reference has it) | BLOCKING |
| 2 | Quadratic time on whitespace runs after a path root | NOT reproduced with the reviewer's input `"a"` plus spaces (3 ms at 20,000); to re-check with a root before the run | open | to re-check at the fix round's review |
| 3 | 543 regressions against the replaced regex in sweep A, and other corpus regressions | not replayed by the facilitator | open | to re-check at the fix round's review |
| 4 | A leak on mixed-slash drive paths: `C:/Users\<user>\x.cs`, `file:///C:/Users\<user>\x.cs` keep the account folder (the pre-435 copy and the replaced regex scrubbed them) | confirmed | the DESIGN (the reference has it) | BLOCKING |
| 5 | A leak on prefix folders before a drive-style users root: `/mnt/wsl/c/users/<user>/x.cs`, `/opt/cygdrive/c/users/<user>/x.cs`, `/a/c/users/<user>` | confirmed in the green only (the reference and the replaced regex scrub them) | the GREEN departs from the design | BLOCKING |
| 6 | A bridged second path in parentheses after a slash drive or `~` path leaks: `see C:/temp/x.cs (/home/<user>)` | confirmed in all four, the pre-435 copy too | older | MINOR (card 435's MINOR finding 6; a corpus row in the follow-up) |
| 7 | JSON log with an escaped regex before a path field: `{"pattern":"\\d+","path":"C:\\Users\\<user>\\x.cs"}` loses its path field | confirmed in the green only (the reference keeps it) | the GREEN | BLOCKING |
| 8 | The green swallows the space or punctuation after an account folder: `<path>[Verification.csproj]`, `<path>"is denied"`, `<path>\nnext` | confirmed in the green only (the reference keeps them) | the GREEN | BLOCKING |
| 9 | A second-pass change when three paths chain through UNC or `~` after an account tail | not replayed | | MINOR (the reviewer's class) |
| 10 | Code size: 939 lines against the reference's about 330; dead code, duplicated rules, branches no test reaches | the green is 938 lines | the GREEN | MINOR, and the fix round removes it (below) |

## The fix round (test-first)

1. The architect (claude opus, ctx-1) fixes findings 1 and 4 in the reference scanner and measures every reviewer shape, findings 2 and 3 included, before any test.
2. The red (claude sonnet, ctx-2) adds a failing row for each confirmed finding (1 as a measured-evidence fact with a second-pass assertion, no wall-clock assertion per D254; 4, 5, 7 and 8 as rows), Pending until the green.
3. The green (cursor gpt-5.3-codex, ctx-3) PORTS the fixed reference faithfully instead of reimplementing it: findings 5, 7, 8 and 10 come from the green departing from the design.
4. A second review round (agy argon, ctx-4) with D259 clause 4.
