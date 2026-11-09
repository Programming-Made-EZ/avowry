import json, shutil, subprocess, sys, tempfile
from pathlib import Path
REPO = Path(r"C:\Users\<user>\projects\phrase-lang")
RECORD = ".mob/records/234/record-in-progress.json"
SEATS = ".mob/records/234/usage/seats-2026-11-01-0626.json"
FULL = ".mob/records/234/usage/measure-qa-red-full.json"
INCL = [RECORD, ".mob/records/234/usage", ".mob/records/234/turn-slice-architect.md", "docs/process-wiki/INDEX.md", "HANDOFF.md"]


def fresh(tag):
    root = Path(tempfile.mkdtemp(prefix=f"g2c-{tag}-"))
    for rel in INCL:
        src, dst = REPO / rel, root / rel
        dst.parent.mkdir(parents=True, exist_ok=True)
        if src.is_dir():
            shutil.copytree(src, dst, dirs_exist_ok=True)
        else:
            shutil.copy2(src, dst)
    return root


def run(root):
    p = subprocess.run([sys.executable, str(REPO / "scripts/check-restart-record.py"), "--root", str(root)],
                       capture_output=True, text=True, encoding="utf-8")
    return p.returncode, (p.stderr.strip().splitlines() or p.stdout.strip().splitlines() or [""])[0]


def rec_mut(fn):
    def go(root):
        p = root / RECORD
        r = json.loads(p.read_text(encoding="utf-8"))
        fn(r, root)
        p.write_text(json.dumps(r, indent=1) + "\n", encoding="utf-8")
    return go


def m_seat_one(r, root):
    r["seat_metrics"]["ctx-4"].update(setup_tokens=1, end_tokens=1)
def m_fac5(r, root):
    r["restarts"][0].update(facilitator_context_tokens=5, delta_tokens=5 - 965000)
def m_zero(r, root):
    r["restarts"][0]["rule_miss_cases_zero_source"] = "HANDOFF.md"
def m_allnm(r, root):
    for c in list(r["seat_metrics"]):
        r["seat_metrics"][c] = {"not_measured_reason": "x", "available_evidence_source": SEATS}
def m_row(r, root):
    p = root / SEATS
    d = json.loads(p.read_text(encoding="utf-8"))
    d["seats"][2]["tokens"] += 1
    p.write_text(json.dumps(d, indent=1) + "\n", encoding="utf-8")
def m_raw(r, root):
    p = root / FULL
    d = json.loads(p.read_text(encoding="utf-8"))
    d["usage"]["output_tokens"] += 1
    p.write_text(json.dumps(d), encoding="utf-8")
def m_nokey(r, root):
    del r["seat_ab"][0]["brief_source_key"]
def m_base(r, root):
    r["seat_ab"][0]["setup_baseline_tokens"] = 1
def m_ab_typed(r, root):
    r["seat_ab"][0]["brief_tokens"] = 100
def m_ab_missing(r, root):
    r["seat_ab"][0]["full_source"] = ".mob/records/234/usage/nope.json"

CASES = [("reviewer: seat ctx-4 setup=end=1", m_seat_one), ("reviewer: facilitator tokens 5 + delta", m_fac5),
         ("reviewer: zero source HANDOFF.md", m_zero), ("reviewer: all seats not_measured", m_allnm),
         ("extra: seats row changed by one", m_row), ("extra: raw measure file edited", m_raw),
         ("extra: seat_ab key removed", m_nokey), ("extra: seat_ab baseline 1", m_base),
         ("extra: seat_ab brief tokens typed 100", m_ab_typed), ("extra: seat_ab raw source missing", m_ab_missing)]
base = fresh("clean")
print("clean copy:", run(base))
caught = 0
for i, (name, fn) in enumerate(CASES):
    root = fresh(f"m{i}")
    (fn if fn in (m_row, m_raw) and False else rec_mut(fn))(root)
    code, line = run(root)
    caught += code == 1
    print(f"{name}: exit {code} | {line}")
print(f"caught {caught}/{len(CASES)}")
