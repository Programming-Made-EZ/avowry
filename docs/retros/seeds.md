# Retro seeds

Items raised mid-card, to be taken at the next retro. Newest first.

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

