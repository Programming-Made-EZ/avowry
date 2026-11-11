# Card 426 second read-only review (a fresh Claude context told to try to break the change), main at 488554d

VERDICT: ACCEPT WITH NOTES. Saved by the facilitator, condensed; inputs use only the placeholder `<user>` or a plain stand-in folder; the reviewer ran a throwaway console project outside the repository against PathScrub.cs and the old version from dcd96f4.

Every card row behaves as claimed; the regex is linear (20,000-character inputs 0 to 12 ms, a 1.04 MB single line 204 ms against 243 ms before); URL, XML and `\d+` rows hold.

1. LEAK, new with this card (a line that was clean before now leaks, because the old over-scrub hid a later unmatched path): `copy /tmp/a.cs to \u0027/home/<user>/b.cs\u0027` became `copy u0027` before and keeps the second path now; also `gcc /tmp/a.c -I /usr/include -I/home/<user>/include`, `Restoring /tmp/x.csproj for /cygdrive/c/Users/<user>/x.csproj`, `in /tmp/x.cs at /opt/data/home/<user>/x.cs` (the last three are older gaps: a root glued to a letter or an unlisted root).
2. LEAK, older gap inside the card's value: a JSON escape before a non-drive root (`\u0027/home/<user>/x.cs\u0027`, `\u0027/home/<user>\u0027`, `\u0027/mnt/c/Users/<user>/x.cs\u0027`, `\u0027\host\Users\<user>\x.cs\u0027`): the escape lookbehind sat only on the drive branch.
3. LEAK: the account-folder rule takes a one-letter drive word (`moved C:\Users\<user> to D:\Users\<user>\x.cs` became `moved <path>:\Users\<user>\x.cs`).
4. LEAK, older: a UNC path whose Users folder is not the first (`\host\c$\Users\<user>`, `\wsl$\Ubuntu\home\<user>`).
5. Partial LEAK: a JSON-escaped non-ASCII letter in an account name.
6. Partial LEAK, older: the trailing-words rule takes only plain ASCII letters (accents, parentheses, a dot).
7. Over-scrub only, new: a one-letter URL host (`https://a/<Users>/x`) read as a Git Bash drive (the architect's risk 2); `\\d+` loses its backslashes (ruling 5 did not claim it).
8. Over-scrub only, older: JSON escapes glued after a path (`\n`, `\t`, `\u0022`).
9. Changes with no leak, as designed (`C:\a b \c\x.cs` becomes `a b \c\x.cs`).
10. Idempotence: fuzzing 300,000 contrived strings found 177 where a second pass changes the output (173 before the card).

Mutations the card's tests would not catch: lower-case `users` on the Unix root, `[a-z]` replaced by `c`, the case of `documents and settings`, the escape lookbehind without its backslash, the UNC home check widened to any folder, the trailing-words handling of a following `X:`, a one-letter URL host, a JSON-escaped non-drive root, an idempotence loop limited to the listed rows. The label tests and the strengthened 418-3b look sound.

Facilitator's disposition: findings 1 (first row), 2, 3 and 7 are fixed in this card with a failing test first (seat-changes.md); the rest are filed as one follow-up bug before the publish.
