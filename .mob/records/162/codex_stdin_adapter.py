"""Adapter for card 162's Codex LLM-customer check on this machine (session scratch, not committed).

The bench appends ` exec --skip-git-repo-check <cold flags>` to the model command and writes the prompt to the command's stdin.
The machine-local launcher (bin/codex_nostore.py) removes the Store alias from PATH and gives Codex a small TEMP, but it closes
stdin, so Codex would see no prompt. This adapter uses the launcher's own constants and environment fix and passes the prompt on
stdin with the `-` argument. It adds the pinned model and a read-only sandbox (the check only needs the reply text).
"""
import importlib.util
import os
import subprocess
import sys

launcher = os.path.join(os.path.expanduser("~"), "bin", "codex_nostore.py")
spec = importlib.util.spec_from_file_location("codex_nostore", launcher)
cn = importlib.util.module_from_spec(spec)
spec.loader.exec_module(cn)

os.makedirs(cn.SEAT_TEMP, exist_ok=True)
env = dict(os.environ)
env["PATH"] = cn.without_store_alias(env.get("PATH", ""))
env["TEMP"] = env["TMP"] = cn.SEAT_TEMP
prompt = sys.stdin.buffer.read()
args = list(sys.argv[1:])
if args and args[0] == "exec":
    extra = []
    if "-m" not in args and "--model" not in args:
        extra += ["-m", "gpt-5.6-sol"]
    if "-s" not in args and "--sandbox" not in args:
        extra += ["-s", "read-only"]
    args = ["exec"] + extra + args[1:]
args.append("-")
proc = subprocess.run([cn.NODE, cn.ENTRY, *args], env=env, input=prompt, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
sys.stdout.buffer.write(proc.stdout or b"")
sys.stderr.buffer.write(proc.stderr or b"")
sys.exit(proc.returncode)
