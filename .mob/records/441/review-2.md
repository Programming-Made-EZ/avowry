# Card 441 review round 2 (told to try to break the change), card-441 at 389d790

VERDICT: FLAG; new BLOCKING: 2; new MINOR: 3. Seat: QA (claude opus = claude-opus-5-5, pinned with the Agent model flag, a fresh context for ctx-4, D264), read-only in a throwaway clone; the heavy lock checked before every build and test (free each time); the clone's PathScrub.cs restored byte for byte after every mutant. This record is written from the reviewer's report as relayed in the facilitator session.

## Round-1 BLOCKING findings, replayed (the scanner before this card / the pre-435 copy / the port)

- 1 fixed: `jar:file:/c:/users/<user>/.m2/a.jar!/x.class` gives `jar:file:/x.class` in all three; also `java.io.FileNotFoundException: /c:/users/<user>/x.txt`, both `Documents and Settings` spellings in every case, `///c:/users/`, `vscode-remote://wsl+Ubuntu/c:/users/`, `file://localhost/c:/users/`, `ssh://host/C:/Users/<user>/r.git` (now `ssh://host/r.git`).
- 2 fixed: `{"env":"PYTHONPATH=\/home\/<user>\/lib:\/home\/<user>"}` gives `PYTHONPATH=lib:<path>`, stable (both earlier scanners leak it whole).
- 3 fixed: `docker run -v D:/a:/a x` gives `<path>a`, stable, equal to before; also `-v C:/Users/<user>/d:/data`, compose short and long syntax, `--mount source=`, quoted and `-v=` forms.

| # | Finding | Class |
|---|---|---|
| 1 | A drive after `/` followed by a doubled or mixed separator leaks: `/C://Users//<user>//x`, `file:/C://Users/<user>/x`, `jar:file:/c://users/<user>/x.class`, `jar:file:/c:///users/<user>/x`, `jar:file:/C://Documents and Settings/<user>/x`, `x/c:/\users\<user>\x`; both earlier scanners scrub them; the green before the fix round (ede15fd) leaked them too (missed, not new). Real source: `URL.getPath()` of a `file:///C://Users/...` URL; a root-plus-part join put into a `file:` or `jar:` URI. Reviewer's checked fix: skip the separator run at the top of `DriveParentAhead`. | BLOCKING (a regression with a leak, the Java and URI family) |
| 2 | A new second-pass change on escaped JSON lists and volumes whose element ends in a one-letter folder before `:\/`: `{"Binds":["\/home\/<user>\/d:\/data"]}` gives `d:\/data` then `data`; also `PATH=\/home\/<user>\/x:\/usr\/bin`, PYTHONPATH lists, `mount`, `volumes`, `:ro`. The fix round's `ColonSlash` keeps the letter; the chain rule that turns a lone letter before `:/` into `<path>` tests only `/`. Stable at ede15fd. Reviewer's checked fix: the chain rule also accepts `:\/`. | BLOCKING (a second-pass change in the corpus json-escaped family with round 1's finding-3 shape) |
| 3 | `C:\Users\<user>(1)xC:\/a\/y` changes on pass 2 (the glue change); kills mutant m4 | MINOR (contrived) |
| 4 | `file:/C:/Documents%20and Settings/<user>/x` (mixed encoded and plain space) leaks | MINOR (contrived; ruling 1) |
| 5 | An escaped `X:\/` right after `\/` is not refused like rule 3e's unescaped form: `{"PATH":"\/opt\/b:\/home\/<user>"}` gives `\/opt\/<user>` | MINOR (older: all three scanners leak) |

Held: 24 timed shapes linear (the worst `C:/a/b:/c/d:/e/` 58.9 ms at 1M characters against 19.9 ms before); .NET, Python, Node and Java stack traces; rustc, MSVC, ld, cmake, gcc output; git dubious-ownership lines; Git Bash, WSL, UNC, `\\?\UNC\` (now scrubbed); prose.

Mutants (Slice 441 / 439 / 435 failures): killed m1, m2, m3, m5, m6, m8, m9, m12. Survivors with killing rows: m4 (glue reverted; row of finding 3), m7 (no separator check after the parent name; `PATH=/opt/b:/userspace/x`), m10 (rule 3e on any separator; `x/c:\tools\<user>\a.cs` and `{"PATH":"\/opt\/x:\/opt\/y"}`); m11 (`p > prevEnd`) equivalent.

## Fix round 2 (test-first)

The architect (ctx-1) takes the two BLOCKING fixes into the reference (the separator run after a drive after `/`; the chain rule's lone letter before `:\/`), checks the reviewer's fixes against every corpus row and both blow-up rows' second pass, adds the rows of findings 1 and 2 and the m4, m7 and m10 killing rows to the corpus; then the red, the port, the gate and review round 3.
