# Card 365 second read-only review (Grok, reply-only, plan mode; the code diff of the green inlined, scrubbed of the account name; hand count on the sent prompt 0)

Tested green: mob-defaults 5d5da21e484fe0bb08b4d1878b649f62fa58b373. Reply verbatim:

**FINDINGS** (ranked)

1. **Live holder swept at 120s, then release steals the successor’s lock** (`facilitator.py` `_seat_use_lock_stale` MAX_AGE test; `_release_seat_use_lock` unlink after a matching token read). Interleaving: A is inside `hold_seat_use_lock` (history IO hung). At t>120s B sees a live pid, still treats the lock as stale, double-reads, unlinks, `O_EXCL`s, enters the CS. A finishes, `finally` reads its own token (or the read loses the race), unlinks **B’s** file. A and B both run `read_seat_use` / `append_seat_use`. Two appends can tear a JSONL line; prefix check then fires or a bad line is kept. Same steal if `_read_seat_use_lock` returns `None` while the path still exists: token is not checked and unlink proceeds.

2. **Unix dual-hold on create-then-write vs GRACE** (`os.open` `O_EXCL`, then `fdopen` write; `_seat_use_lock_stale` empty/unreadable `age > 2`). Interleaving: A `O_EXCL`s (empty inode, mtime T). A stalls >2s before `write`. B reads empty, age>2, raw text matches, `unlink`s A’s inode, `O_EXCL`s a new inode, writes, yields. A writes to the unlinked fd, yields. Two CSes; the lock file only names B. Windows unlink of an open file usually fails, so this is Linux/macOS.

3. **Windows `PermissionError` = “busy” forever** (`hold_seat_use_lock` treats `PermissionError` like `FileExistsError`). Delete-pending or AV handle: every later run polls 30s then `SeatRefused` (“held by pid …”). A non-pending ACL error takes the same path instead of “cannot be created”. Release retry (2s) then `{"released": true}` still cannot remove a delete-pending name.

4. **Zombie / reused pid** (`pid_alive` in `_seat_use_lock_stale`). Zombie: pid still alive, waiters refuse at 30s, file blocks until 120s. Reuse within 120s: unrelated pid looks like the holder; same refusals. Designed, but it is a multi-minute outage, not a sweep.

5. **Forged append** (prefix rule `lines[:len(seen)] != seen`). A rewrite/delete/empty of a line this run saw is refused. An extra line is accepted and becomes `seat_use_seen`. The lock is cooperative; an unlocked writer can mint a seat-use line. That is outside append-only tamper-evidence; do not stretch this card to authenticate writers.

**NOT A PROBLEM**

- Happy-path exclusion: `O_CREAT|O_EXCL` plus token check on a successful read; two live cooperative takers do not both create.
- Double-read-before-unlink avoids deleting a lock whose bytes already changed.
- Prefix rule: shorter file, changed earlier line, emptied file all fail `lines[:len(seen)] == seen`.
- Lock is not held across a probe (`reprobe_if_stale` before `with`; `_seat_use_held` refuses a probe in CS) or across a vote (`start_voters` releases before voters run; `verify_history` only rereads). Non-reentrant `RuntimeError` blocks nesting this lock. Diff never takes the vote lock inside the seat-use CS.
- `line_index`: `base` is checked with `lines[base] != text` after reread; mismatch refuses. `written[:index]` is lines before that seat (same as old `first+n`). `errors="replace"` can disagree with strict `read_text`; that refuses, it does not bind the wrong line.
- Fallback is in-process only, as documented.

**VERDICT**

ACCEPT WITH NOTES: cooperative concurrent seats get the prefix rule and a short exclusive CS, but stale-sweep/release can still produce two writers (120s live sweep + unlink TOCTOU; Unix empty-file GRACE) and Windows `PermissionError` can refuse every later run.

## Facilitator ruling

ACCEPT WITH NOTES. Findings 1 and 2 are the residual races the slice already named (a live lock swept by the 120 s age rule only if a critical section of milliseconds hangs for two minutes; a create-then-write gap of more than 2 s on Linux); they are narrow, backstopped by the prefix rule and the position check, and the card's three done checks hold. Finding 3 (a Windows delete-pending name refusing later runs) is handled by the pid-0 overwrite fallback and the named refusal but is not proven on a real antivirus handle. Finding 4 is a bounded outage by design. Finding 5 is outside the card (the design makes appends tamper-evident for a prefix, not authenticated). The hardening candidates are filed as one bug after the card: do not sweep a lock whose pid is alive by age alone unless the file has not changed for the max age and the holder is not in a critical section, and refuse to unlink when the token cannot be read.
