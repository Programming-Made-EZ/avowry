# Card 441 review round 3 (told to try to break the change), card-441 at 8be01b7

VERDICT: SIGN-OFF; new BLOCKING: 0; new MINOR: 3. Seat: QA (claude opus = claude-opus-5-5, pinned with the Agent model flag, a fresh context for ctx-4, round 3), read-only in a throwaway clone; the heavy lock checked before every build and test (free each time); the clone's PathScrub.cs restored byte for byte after every mutant (`git diff --stat` empty); timings taken only after the facilitator's gate had finished. This record is written from the reviewer's report as relayed in the facilitator session.

## Earlier BLOCKING findings, replayed (the scanner before this card / the pre-435 copy / the port)

- Round 1, 1 to 3: fixed (Java and URI forms with a drive after `/`; escaped JSON colon lists; forward-slash volume specs, stable).
- Round 2, 1: fixed. `/C://Users//<user>//x` gives `/x`; `file:/C://Users/<user>/x` gives `file:/x`; the `jar:file:` forms with `//` and `///`, `Documents and Settings`, and `x/c:/\users\<user>\x` all equal both earlier scanners; 43 more separator-run forms (file, jar, vscode, stack frames, classpath lists, quoted and escaped forms) equal the scanner before this card, 0 leaks, 0 second-pass changes.
- Round 2, 2: fixed. `{"Binds":["\/home\/<user>\/d:\/data"]}` gives `<path>:\/data`, stable; also `:rw`, `:ro`, upper-case drives, other targets, deeper chains, PATH and PYTHONPATH lists, `env` arrays.

| # | Finding | Class |
|---|---|---|
| 1 | The MINOR 3 glue reads an escaped `X:\/` after a word glued to an account tail as a drive, so an escaped account root behind it stays: `C:\Users\<user>(1)xd:\/home\/<user>` gives `<path><user>` (389d790 gave `<path>d:<path>`); stable; both earlier scanners leak it too; random generation found 9 in 1.1 million inputs, all this shape | MINOR (contrived; fix round 2 made it worse than 389d790, not than the scanner before this card) |
| 2 | Left whole by all three scanners: `file://server/share/Users/<user>/x`, `file:///c%3a/users/<user>/x`, `vscode-remote://wsl%2Bubuntu/home/<user>/x` | MINOR (older; the percent forms under ruling 1's no-general-decoding limit) |
| 3 | Three mutants of the escaped root exception survive: `users-exc-off`, `root-ci`, `root-no-trailing`; killing rows `{"volumes":["\/home\/<user>\/d:\/Users\/<user>"]}`, `{"Binds":["\/home\/<user>\/d:\/users\/x"]}`, `{"Binds":["\/home\/<user>\/d:\/home"]}`; `glued-any-pos` named equivalent (0 differences in 900,000 inputs) | MINOR (test gap; the rows go to the follow-up card) |

Held: compiler and linker output (gcc, ld, rustc, MSVC, CMake, go, javac), .NET, Java, Python, Node and Rust stack traces, JSON logs (Docker Mounts and Binds), git dubious-ownership and safe.directory lines, WSL and Git Bash, UNC forms, prose; the corpus 151 rows, 0 misses in both forms, all stable; about 1.5 million random inputs, 0 new second-pass changes against 389d790. Linearity: 61 shapes at 20k, 200k and 1M characters, all linear; the worst `file:/C:` + a 1M `/` run + `Users` at 179.6 ms (linear, about 50 times the scanner before this card on that shape).

## Disposition

SIGN-OFF stands under D259 clause 4 (0 BLOCKING). The three MINORs, with round 1's MINORs 4 to 7, round 2's MINOR 5 and the named limit (the forward-slash sibling of MINOR 3), go to a follow-up card as corpus rows and killing rows. The sign-off seat (claude sonnet = claude-sonnet-5-5, ctx-5) runs next.
