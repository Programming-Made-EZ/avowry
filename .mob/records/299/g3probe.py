"""Facilitator G3 probes for card 299 (the gate's record-only lines). Run from C:/Users/<user>/mob-crosscli: python <this file>."""
import contextlib
import io
import os
import subprocess
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, "C:/Users/<user>/mob-crosscli")
import facilitator as fac  # noqa: E402


def repo():
    d = tempfile.mkdtemp()
    subprocess.run(["git", "init", "-q", d], check=True)
    subprocess.run(["git", "-C", d, "-c", "user.email=a@b", "-c", "user.name=n", "commit", "-q", "--allow-empty", "-m", "base"], check=True)
    return Path(d)


# 1. the gap between a stale rename and the restore link: a third writer can claim while a live holder's file is moved away (the architect's risk 2)
r = repo()
w2 = fac.Writers()
w2.claim(r, "seat-a", "seat turn")
lock = fac.writer_lock_path(r)
moved = lock.with_name("mob-writer.lock.stale-probe")
os.replace(lock, moved)          # what a taker's rename does to a live lock
w3 = fac.Writers()
try:
    w3.claim(r, "seat-b", "seat turn")
    third = "third writer CLAIMED while the live holder's file was moved away"
except fac.WriterRefused as exc:
    third = f"third writer refused: {exc}"
try:
    os.link(moved, lock)         # the restore
    restore = "restore linked"
except FileExistsError:
    restore = "restore link hit FileExistsError (the third writer's file is in place): two live holders in this gap"
print("1. restore gap:", third, "|", restore)

# 2. the gap between read and unlink in release: the lock is released only if its token is mine; a replaced lock is left alone
r = repo()
w1 = fac.Writers()
w1.claim(r, "facilitator", "commit")
lock = fac.writer_lock_path(r)
key = next(iter(w1.tokens))
token = w1.tokens[key]
# another process's holder replaces the file (as a takeover would after this process died); a late release must leave it
foreign = lock.with_name("foreign.tmp")
foreign.write_text('{"pid": %d, "started": "2026-01-01T00:00:00+00:00", "token": "other", "writer": "x", "operation": "y"}' % os.getpid())
os.replace(foreign, lock)
fac.release_writer_lock(r, token)
print("2. late release with a foreign token: lock still there =", lock.exists(), "| holder token =", __import__("json").loads(lock.read_text())["token"])

# 3. a writer lock cannot be created whole-or-not-at-all is covered by the link test; here: no stray temp files after a normal claim and release
r = repo()
w = fac.Writers()
w.claim(r, "facilitator", "commit")
w.release(r, "facilitator")
print("3. files left after a normal claim and release:", sorted(p.name for p in fac.writer_lock_path(r).parent.glob("mob-writer*")))
