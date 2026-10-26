# Retro seeds

Items raised mid-card, to be taken at the next retro. Newest first.

## 2026-10-26: a Decider voter ran with full write access and edited the repo (area: Organization, safety)

- **What happened:** my PowerShell launcher flattened a one-item list, so the Copilot voter in one round received the bare prompt
  'd11' instead of the vote text. Copilot was started with `--allow-all-tools` in the repo directory, treated 'd11' as a task, and
  for about 15 minutes edited source files (an unreviewed start of the C# adapter, +580 lines in `CSharpProvider.cs`, plus a
  `DotnetCli.cs` roll-forward tweak). It made no commit and no push. I found it from file timestamps after a status check showed
  edits I did not make, traced the process chain (my launcher, the job runspace, node, copilot.exe), and stopped exactly those
  PIDs. The edits are in a labeled stash, not shipped.
- **Root causes:** (1) 'read-only' for voters was a promise in the prompt, not a mechanism (the same gap as the delete ban: no
  detector); (2) voters ran in the real working tree with permission-bypass flags; (3) my scratch launcher had no check that each
  seat got the intended prompt.
- **Fix to build next (test first, mob-defaults):** a Decider runner that clones the repo into a scratch directory and runs every
  voter there (so nothing a voter does can touch the real tree), uses each CLI's read-only or restricted mode where one exists,
  checks the prompt each seat received, parses votes in one place (narration, conditional 'No ... yes if', and echoed transcripts
  are the three parsing traps seen so far), and verifies the real tree and HEAD are unchanged afterwards, failing loudly if not.
- **Counter-metrics:** none to game; this is a safety fix. Any voter-caused change to the real tree is a bug in the runner.

## 2026-10-26: the safety bans had no detector (area: Organization, safety)

- **What happened:** while clearing a scratch folder the facilitator ran `rm -rf` on a variable path. The folder did not exist and
  nothing was deleted, but the standing ban (never `rm -rf` a variable or computed path) relied on memory.
- **Steve's rule applied:** "constraints must self-report": a ban with no detector isn't in force.
- **Shipped (mob-defaults `delete_guard.py`, 80 tests, and phrase-lang pre-commit):** a detector for recursive deletes of variable or
  unresolved paths and for kills by process name; `safe_delete(path, roots)` as the single safe way to delete; the safety rule
  appended to every prompt; a `Safety flag` raised in MOB.md (and on the published page) for any model reply that shows such a
  command; a pre-commit check on added script lines.
- **Hypothesis for the retro to check (a safety fix, not a trial):** every seeded violation in the tests is detected (25 unsafe
  patterns), and ordinary literal scratch deletes and exact-PID kills are not flagged (10 safe patterns). Counter-metrics: false
  flags per card (target 0; any false flag is a bug in the detector) and the tokens the extra prompt sentence adds (about 60 per turn).
- **Known gaps:** the guard cannot stop a command a driver runs in its own shell; it flags the evidence in its reply and blocks the
  commit of such a script. A symlink case is covered in code but its test is skipped on Windows (no symlink privilege).
- **Ask of the retro:** is a runtime wrapper around the drivers' shells worth the cost, or is detect-and-flag enough?

