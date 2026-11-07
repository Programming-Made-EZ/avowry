"""Facilitator G3 probes for card 313: failure paths of hold_vote_lock that no unit test drives. Run from C:/Users/ringe/mob-crosscli."""
import contextlib
import io
import os
import subprocess
import sys
import tempfile
from pathlib import Path
from unittest import mock

sys.path.insert(0, "C:/Users/ringe/mob-crosscli")
import decider  # noqa: E402
import facilitator as fac  # noqa: E402


def repo():
    d = tempfile.mkdtemp()
    subprocess.run(["git", "init", "-q", d], check=True)
    return Path(d)


def run(label, repo_path, patcher=None):
    err = io.StringIO()
    release = None
    exc = None
    with contextlib.redirect_stderr(err):
        try:
            if patcher:
                with patcher:
                    release = decider.hold_vote_lock(repo_path)
            else:
                release = decider.hold_vote_lock(repo_path)
        except BaseException as e:  # noqa: BLE001
            exc = e
    live = fac.decider_lock_live(repo_path) if exc is None else None
    holders = fac.decider_holders(repo_path) if exc is None else None
    twice = ""
    if release:
        with contextlib.redirect_stderr(io.StringIO()) as e2:
            try:
                release()
                release()
            except BaseException as e:  # noqa: BLE001
                twice = f" release raised {e!r}"
        twice = twice or " release twice ok"
    print(f"{label}: exception={exc!r} live={live} holders={len(holders) if holders is not None else None} stderr={err.getvalue().strip()!r}{twice}")


# 1. outside a git checkout
d = tempfile.mkdtemp()
run("outside git", Path(d))

# 2. mkdir raises OSError (not FileNotFoundError)
r = repo()
run("mkdir OSError", r, mock.patch.object(Path, "mkdir", side_effect=PermissionError("no")))

# 3. other os.open OSError
r = repo()
run("open PermissionError", r, mock.patch.object(decider.os, "open", side_effect=PermissionError("no")))

# 4. write raises OSError after open
r = repo()
real_fdopen = os.fdopen
run("write OSError", r, mock.patch.object(decider.os, "fdopen", side_effect=OSError("disk full")))
print("   leftover .tmp after write failure:", [p.name for p in (r / ".git" / "mob-decider.lock.d").glob("*")] if (r / ".git" / "mob-decider.lock.d").exists() else "no directory")

# 5. replace raises OSError other than FileNotFoundError
r = repo()
run("replace PermissionError", r, mock.patch.object(decider.os, "replace", side_effect=PermissionError("no")))
print("   leftover after replace failure:", [p.name for p in (r / ".git" / "mob-decider.lock.d").glob("*")] if (r / ".git" / "mob-decider.lock.d").exists() else "no directory")

# 6. release unlink OSError
r = repo()
rel = decider.hold_vote_lock(r)
err = io.StringIO()
with contextlib.redirect_stderr(err):
    with mock.patch.object(Path, "unlink", side_effect=PermissionError("no")):
        try:
            rel()
            raised = None
        except BaseException as e:  # noqa: BLE001
            raised = e
print(f"release unlink OSError: exception={raised!r} stderr={err.getvalue().strip()[:160]!r}")
