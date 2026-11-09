"""Build a D-votes.json from the parallel runner's result dirs. Usage: mkvotes_generic.py DNNN SUBJECT RESULT PROPCOMMIT_OR_FILE=label:tagprefix [...]
Each block spec: label=tagprefix@proposalref  (proposalref: a git commit hash for .mob/records/176/<file>, or 'file' for the working file); env PROPPATH is the repo path."""
import json, glob, re, subprocess, sys, os
S = 'C:/Users/<user>/AppData/Local/Temp/claude/1ff08f82160bcdeb/2ca12683-042f-40b7-b61a-623ae180176b/scratchpad/'
G = 'C:/Users/<user>/projects/phrase-lang'
PROPPATH = os.environ['PROPPATH']
d, subject, result = sys.argv[1:4]
R = {'architect': 'architect', 'qa': 'qa', 'developer': 'dev', 'audit lens': 'auditlens', 'decider': 'decider', 'coach': 'coach'}


def prop(ref):
    if ref == 'file':
        return open(G + '/' + PROPPATH, encoding='utf-8').read()
    return subprocess.run(['git', '-C', G, 'show', ref + ':' + PROPPATH], capture_output=True, text=True, encoding='utf-8').stdout


def n(x):
    return x.lower().replace('*', '').strip().strip('"').rstrip('.;:,')


def entries(prefix, role, p):
    out = []
    for seat in ('grok', 'codex', 'claude'):
        f = glob.glob(f'{S}{prefix}-{R[role]}-{seat}-out/**/decider-result.json', recursive=True)
        if not f:
            continue
        v = json.load(open(f[0], encoding='utf-8'))['votes'].get(seat, {})
        r = v.get('reply', '')
        i = r.find('Reading additional input')
        r = r[:i].rstrip() if i > 0 else r
        m = re.search(r'CITE:\s*(.+)', r)
        c = m.group(1).strip().strip('"') if m else ''
        j = r.rfind('\n' + str(v.get('vote')) + '\n')
        head = (r[j + 1:] if j >= 0 else r).split('CITE:')[0].strip()
        e = {'tag': f'{prefix}-{R[role]}-{seat}', 'seat': seat, 'model': v.get('model'), 'vote': v.get('vote'), 'counted_by_decider': v.get('counted'), 'cite': c,
             'cite_verbatim': c in p, 'decider_cite_ok': v.get('cite_ok'), 'reply_head': head[:650]}
        if not v.get('counted'):
            e['note'] = 'not counted by the decider (no vote, or the cite was not exact); the role was re-asked on the next driver'
        out.append(e)
    return out


res = {'decider': d, 'subject': subject, 'result': result}
for spec in sys.argv[4:]:
    label, rest = spec.split('=', 1)
    prefix, ref = rest.split('@')
    p = prop(ref)
    res[label] = {role: entries(prefix, role, p) for role in R}
open(f'{G}/.mob/records/deciders/{d}-votes.json', 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=1) + '\n')
for k, v in res.items():
    if isinstance(v, dict):
        print(k, [(r, [(e.get('seat'), e.get('vote'), e.get('counted_by_decider')) for e in es]) for r, es in v.items()])
