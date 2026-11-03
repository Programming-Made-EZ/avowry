# Card 276: Dispatch install and sign-in note (2026-11-03, about 00:40 UTC)

Source: a Dispatch message to the facilitator in chat. The facilitator checked the install read-only and changed nothing.

Facts from Dispatch:
- Dispatch installed Muse Code at the product owner's request, after reviewing Meta's install.ps1. It runs natively on Windows.
- Version 1.4.14 (R7309.1), at %LOCALAPPDATA%\Programs\muse\muse.cmd, on the user PATH (new shells).
- Not signed in. `muse login` prints a device code URL (https://auth.meta.com/oauth/device/?code=XXXX-XXXX). Codes expire after about 10 minutes, and two have already expired waiting for the product owner.
- Sign-in is Dispatch-owned. Dispatch runs `muse login` when the product owner says "ready". The team does not block on it and does not run login.
- Useful commands: `muse exec` (headless, one prompt), `muse usage` (subscription allowance), `muse --version`.

Facilitator read-only check, made from Git Bash:
- `muse.cmd --version` printed "Muse Code 1.4.14 (1.4.14-R7309.1)".
- The install folder holds muse.cmd, .muse-launcher.ps1 and muse-bin-1.4.14-R7309.1.exe.

Effect on the slice, which the slice seat must settle:
- Criterion 1 changes. Re-running install.ps1 would duplicate Dispatch's work. Instead, criterion 1 asserts the install on Windows: an install test that checks the version, the PATH resolution and the launcher. No seat runs install.ps1 or `muse login`.
- The sign-in step is out of the team's hands. The product owner's "report the code or URL to Dispatch" is satisfied by Dispatch owning the step. The probe must report "not signed in" as a distinct state, never as a failure or quota.
- Criterion 2's probe can use `muse usage` and `muse --version`.
