"""Facilitator check (window 229-scope's adopted condition): run decider.main at mob-crosscli HEAD with a fake failing
copilot seat whose stderr holds a home path and a token, then show that decider-result.json holds neither."""
import io
import json
import os
import sys
from contextlib import redirect_stderr, redirect_stdout
from pathlib import Path
from unittest import mock

MC = Path("C:/Users/ringe/mob-crosscli")
sys.path.insert(0, str(MC))
import decider  # noqa: E402
import facilitator as fac  # noqa: E402
import mob_summary  # noqa: E402

OUT, WORK = Path(sys.argv[1]), Path(sys.argv[2])
FAKE = MC / "tests" / "fakes" / "fake_launch_seat.py"
BS = chr(92)
home = BS.join(["C:", "Users", "ringe", "secret", "creds.txt"])
token = "ghp_" + "A1b2C3d4E5f6G7h8I9j0K1l2M3n4O5p6Q7r8"
stderr = f"error: could not open {home}; token {token}; The command line is too long."
inv = f'"{sys.executable}" "{FAKE}" copilot {{PROMPT}}'
cfg = {"backends": {"copilot": {"enabled": True, "models": {"default": "fake"}, "invocation": inv,
                                "readonly_invocation": inv}}}
(WORK / "backends.json").write_text(json.dumps(cfg), encoding="utf-8")
(WORK / "proposal.md").write_text("Fake proposal for the no-home-path check.\n", encoding="utf-8")
state = WORK / "state"
state.mkdir(exist_ok=True)
plans = {"copilot": [{"rc": 1, "stderr": stderr}]}


def probe(name, cfg, repo, timeout, stall_seconds):
    return {"ok": True, "taken_at": "2026-11-04T00:00:00Z", "evidence": f"{name} probe OK"}


argv = ["--repo", str(WORK / "repo"), "--proposal-file", str(WORK / "proposal.md"), "--seats", "copilot",
        "--backends", str(WORK / "backends.json"), "--out", str(OUT), "--scratch-root", str(WORK / "scratch"),
        "--timeout", "60", "--ref", "D229-check"]
env = {"FAKE_PLAN": json.dumps(plans), "FAKE_STATE": str(state), "PYTHONIOENCODING": "utf-8"}
with mock.patch.dict(os.environ, env), mock.patch.object(fac, "probe_backend", probe), \
        redirect_stderr(io.StringIO()), redirect_stdout(io.StringIO()):
    code = decider.main(argv)
res = sorted(OUT.rglob("decider-result.json"))[0]
text = res.read_text(encoding="utf-8")
vote = json.loads(text)["votes"]["copilot"]
checks = {
    "home path absent": "ringe" not in text.lower() and "secret" + BS * 2 not in text and "secret" + BS not in text,
    "token absent": token not in text,
    "contains_secret false": (not mob_summary.contains_secret(text)) if hasattr(mob_summary, "contains_secret") else "n/a",
    "attempt_log present": bool(vote.get("attempt_log")),
    "vote not counted": vote.get("counted") is False,
}
print(json.dumps({"decider_exit": code, "mob_crosscli_head": os.popen(f'git -C "{MC}" rev-parse --short HEAD').read().strip(),
                  "stderr_input_shape": "error: could not open C:<sep>Users<sep><user><sep>secret<sep>creds.txt; token ghp_<36 chars>; The command line is too long.",
                  "stored_reason": vote.get("reason"), "attempt_log": vote.get("attempt_log"),
                  "result_file": str(res), "checks": checks}, indent=1))
