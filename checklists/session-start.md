# Session start or restart

When: at every session start, restart, reboot or handoff, before any card work.

- The "Restart here" block is read, and it is fresh. Evidence: the handoff's Restart here block and its As-of commit. Card: #9
- Drivers are probed before the first card; a probe older than 60 minutes is repeated or the card refused. Gate: `facilitator.py` (card mode probes at every card start)
- Stale locks are checked by owner PID, never by process name. Evidence: the lock files and their owner PIDs. Card: #15
- The inbox is read, and its result is written down. Evidence: inbox_open.py's result line in MOB.md (the script exits 1 while any entry is open, so it is not a gate here).
- No audit is overdue. Gate: `facilitator.py` (card mode refuses a card start on an overdue audit, exit 6)
