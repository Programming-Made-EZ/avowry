"""Card 264 diagnostic (green 2): two concurrent `dotnet run` of Phrase.Bench against separate temp suites, 3 rounds.
Before each round the mtime of tools/Phrase.Bench/Program.cs is touched (content unchanged) so both children rebuild at once.
Prints one line per child: round, exit code, whether results/history.jsonl exists, seconds."""
import os, shutil, subprocess, sys, tempfile, time, concurrent.futures as cf
REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
PROJECT = os.path.join(REPO, "tools", "Phrase.Bench", "Phrase.Bench.csproj")
TASK = "004-read-phrase"

def make_suite(root):
    bench = os.path.join(root, "bench"); dest = os.path.join(bench, "tasks", TASK)
    os.makedirs(dest)
    src = os.path.join(REPO, "bench", "tasks", TASK)
    for f in os.listdir(src):
        shutil.copy(os.path.join(src, f), dest)
    if not os.path.exists(os.path.join(dest, "outcomes.json")):
        shutil.copy(os.path.join(REPO, "bench", "csharp-baseline", "tasks", TASK, "outcomes.json"), dest)
    suite = os.path.join(bench, "suite.json")
    open(suite, "w").write('{"version":1,"tasks":[{"id":"%s","kind":"read","prompt":"tasks/%s/prompt.md","holdout":false}]}' % (TASK, TASK))
    reply = os.path.join(root, "reply.txt"); open(reply, "w").write("not a module\n")
    return suite, reply

def one(rnd, idx):
    root = tempfile.mkdtemp(prefix="diag264-")
    suite, reply = make_suite(root)
    cmd = 'more > nul && type "%s"' % reply if os.name == "nt" else 'cat > /dev/null && cat "%s"' % reply
    env = dict(os.environ, MSBUILDDISABLENODEREUSE="1", DOTNET_ROLL_FORWARD="Major")
    history = os.path.join(root, "results", "history.jsonl")
    t = time.time()
    p = subprocess.run(["dotnet", "run", "--disable-build-servers", "--project", PROJECT, "--", "run", "--suite", suite,
                        "--model", "diag-%d-%d" % (rnd, idx), "--model-command", cmd, "--private-results", history],
                       capture_output=True, text=True, env=env, stdin=subprocess.DEVNULL)
    return rnd, idx, p.returncode, os.path.isfile(history), round(time.time() - t, 1), (p.stdout + p.stderr)[-300:]

bad = 0
for rnd in range(1, 4):
    if os.environ.get("DIAG_NO_TOUCH") != "1": os.utime(os.path.join(REPO, "tools", "Phrase.Bench", "Program.cs"))
    with cf.ThreadPoolExecutor(2) as ex:
        for r in [f.result() for f in [ex.submit(one, rnd, i) for i in (1, 2)]]:
            print("round=%d child=%d exit=%d history=%s seconds=%s" % r[:5])
            if r[2] not in (0, 3) or not r[3]:
                bad += 1; print(r[5])
print("anomalies=%d" % bad)
sys.exit(1 if bad else 0)
