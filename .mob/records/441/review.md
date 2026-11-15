# Card 441 review round 1 (told to try to break the change), card-441 at ede15fd

VERDICT: FLAG; new BLOCKING: 3; new MINOR: 7. Seat: QA (claude opus = claude-opus-5-5, ctx-4, D264), read-only in a throwaway clone; the heavy lock checked before every build and test (free each time). The facilitator's replay: `review-verify.md` (the architect's updated reference and the green agree: the defects are in the design). The reviewer's transcript file was empty; this record is written from its report as relayed in the facilitator session.

| # | Finding | Replay | Class |
|---|---|---|---|
| 1 | Rule 3e (no drive right after `/`) leaves Java and URI forms whole that the previous scanner and the pre-435 copy scrubbed: `jar:file:/c:/users/<user>/...`, `java.io.FileNotFoundException: /c:/users/<user>/x.txt`, `file:/C:/Documents and Settings/<user>/...`, `file:/C:/Documents%20and%20Settings/<user>/...`, `///c:/users/<user>/x`, `vscode-remote://wsl+Ubuntu/c:/users/<user>/x` (exact-case `/C:/Users/` stays clean through the Unix marker) | confirmed | BLOCKING (a regression with a leak; Java `File.toURI()`, classpath lines, `URL.getPath()`) |
| 2 | The JSON escaped-slash rule reads an escaped colon list's last letter plus `:\/` as a backslash drive: `{"env":"PYTHONPATH=\/home\/<user>\/lib:\/home\/<user>"}` gives `li` + the name; escaped volume specs and PATH values are mangled (`bibin`) | confirmed | BLOCKING (inside this card's new rule and corpus families; both earlier scanners leaked the whole value) |
| 3 | A new second-pass change: a one-letter folder in a forward-slash drive chain before `:/` (`docker run -v D:/a:/a`, `-v C:/Users/<user>/d:/data`, compose `- C:/work/d:/data`) gives `<path>a:/a` then `<path>a` (the previous scanner was stable) | confirmed | BLOCKING (Docker Desktop on Windows) |
| 4 | MSVC slash options beyond one or two letters (`/doc`, `/external:I`) glued to a path: older, all three scanners | reviewer | MINOR |
| 5 | `X=~name/` (`HOME=~` + name + `/x`): older | reviewer | MINOR |
| 6 | Roots no rule covers (lower-case `\\?\unc\`, JSON-escaped `/var/mail/`, `/mnt/c/Users/`, `ssh:\/\/`, `ssh+git://`, `scp://`, `rsync://`, `-isystem~` + name): older | reviewer | MINOR |
| 7 | `/I\\C:\n` + a stand-in (the `\\C:` blow-up family): contrived | reviewer | MINOR |
| 8 | Rule 3e changes output without a leak (`ssh://host/C:/Users/...` gives `ssh://host/C:r.git`) | reviewer | MINOR (the fix round revisits rule 3e) |
| 9 | Placeholder-form rows (`ssh://<user>@host/...`, `~<user>.old/x`) do not test the real-name behaviour | reviewer | MINOR (test design; the fix round's rows use the stand-in form there) |
| 10 | Three mutants survive (`tilde-name-no-dot`, `ssh-host-no-dot`, `unc-slash-flag-off`), with killing rows | reviewer | MINOR; the killing rows join the fix round's red |

Held: MSVC `/W4 /O2 /Fe /Fd /Fp /FI /Yu /link /LIBPATH: /OUT: /PDB:`, compile_commands.json, gcc `-include -imacros -iprefix -isysroot -MF -MT -o -Wl,-rpath=`, `~name/` in URLs, scp, rsync and quotes, `MAIL`, postfix and procmail lines, `ssh://` and `sftp://` with a user, a port or a dotted host, `\\?\UNC\`; every one of 49 timed shapes linear (the worst 0.94 ms at 20,000 characters).

## Fix round 1 (test-first)

The architect revisits rule 3e so a drive after `/` is read again where it leads to an account parent in any case (including `Documents and Settings` and `%20`-encoded), keeps the escaped colon list a list (no glued drive after `:\/`), and makes a one-letter folder before `:/` in a forward-slash drive chain stable; adds the corpus rows (Java and URI forms, escaped JSON lists, Windows forward-slash volume specs) and the three killing rows; then the red, the port, the gate, and review round 2.
