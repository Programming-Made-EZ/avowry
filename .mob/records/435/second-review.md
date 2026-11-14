# Card 435 second review (told to try to break the change), main at ca84f2d (the green 547538f)

VERDICT: FLAG. Raw report: `second-review-raw.md` (home paths scrubbed). The facilitator replayed every new-with-this-card input in a console probe outside the repository (`second-review-verify.md`: `before` is PathScrubBefore435, `now` the green, `pass2` a second Apply).

## Seats

- Try 1: qa (grok, grok-4.6 declared; the CLI reported grok-4.6-build), 19:40:21Z to 19:55:58Z, exit 1: `402 Payment Required: Grok Build usage balance exhausted` after 17 model calls (1.24M input tokens, 1.06M of them cached). No findings. It left mutant 2 applied in its throwaway clone; the facilitator restored the file.
- Try 2: qa (agy, gemini-4-argon-high declared: adversarial regex reasoning needs the strong tier; an Argon probe answered OK at launch), 19:57:28Z to 20:34:40Z, exit 0. Mutant edits restored by the seat (`git diff --stat` empty, confirmed by the facilitator).

## Disposition (stop rule: BLOCKING is fixed in this card, failing test first; MINOR goes to one follow-up card)

| # | Finding | Replayed | Class |
|---|---|---|---|
| 1 | Glued parentheses (part G's trailing `Plain*`) swallow the next path's drive letter or scheme: `C:\Users\<user>(1)D:\Users\<user>\x.cs` leaks the second account folder | yes, new | BLOCKING |
| 2 | An escape stop (part F) exposes a glued flag that `Glued` refuses after a non-space: `gcc /tmp/a.c\n-I/home/<user>/include` and the `\u0027` form leak | yes, new | BLOCKING |
| 3 | Part F ends a doubled path before `\b` and `\f`, but `EscBefore` lacks `b f`: `C:\\a\\x.cs\b/home/<user>/y.cs` leaks | yes, new | BLOCKING |
| 4 | A preceding path started by `EscLetter`, `EscBefore` or `Glued` eats the drive letter: `\t/tmp/a.C:\Users\<user>` leaks on pass 1 and changes on pass 2 (rows a and b). Row c (`\t\\C:\Users\<user>`) does not leak (`\t<path>`): refuted | a, b yes; c no | BLOCKING (a, b) |
| 5 | Account-folder forms with escaped non-ASCII letters, apostrophes next to non-ASCII letters, spaces inside glued parentheses, `bb:` | older or partial | MINOR |
| 6 | A single-backslash path bridges `\n` into a bare account folder; a bare `-I/home/<user>` after an unspaced path | older (single backslash has no escape stop by design) | MINOR |
| 7 | Part D's UNC prefix loop crosses quotes, commas and semicolons: `["\\host\share\x.cs","C:\Users\<user>\y.cs"]` loses `"x.cs",` | yes, new (over-scrub, no leak) | BLOCKING (a regression) |
| 8 | A sentence-ending dot after `aa(bb)` is swallowed | yes, new (over-scrub) | BLOCKING (a regression; criterion 2 keeps the dot) |
| 9 | A second pass changes `/tmp/a/~\nx.cs`, `C:\\a\\~\nx.cs`, `\\/home/<user>\nx.cs`; the `file://` and `b.C:` rows are older | new rows yes; older MINOR | BLOCKING (new rows) |
| 10 | Quadratic backtracking in part D's UNC prefix loop: 20,000 characters of `\\;` take 7,107 ms (20 ms before), `\\,` 6,647 ms, `\\a;` 4,310 ms, repeated doubled UNC paths 692 ms (3 ms) | yes, new | BLOCKING |
| 11 | Mutant 2 survives (two seats agree; killing row `/d/users/<user>` expecting `<path>`); 11 mutants of the new constants and of parts D and G survive every Slice 435 and 426 test (killing inputs named in the raw report) | mutant 2 by two seats; the 11 by the reviewer | BLOCKING (surviving mutants on changed lines) |
| 12 | The `~` branch lacks `EscBefore`, `Glued` and the escape lookbehind | new for `C:\\a\\x.cs\n~/y.cs` (no account leak), else older | MINOR |
| 13 | UNC `Documents and Settings` is not a home folder | older | MINOR |

## Next

A rework round in this card: the architect re-slices parts D, F and G and the start lookbehinds against findings 1 to 4 and 7 to 11 (bounded UNC prefix with no delimiter crossing, the escape set aligned between `EscBefore` and part F, a stop after the glued-parentheses tail, a drive letter never joined to a previous name); the red adds a failing row for each BLOCKING input and a killing row for each surviving mutant; then a green and a new second review. The MINOR findings (5, 6, 12, 13, and the older rows of 9) become one follow-up card.
