"""Original-protocol confirmation step for one Decider, in one run (stagger + retry so the seat-use history is not raced).
Usage: confirm_generic.py DID PROPFILE_REL PRIMARY_TAG_PREFIX OUTLIER_ROLES(comma) OTHER_ROLES(comma) [driver]
Outliers: 'is YOUR condition now met, yes or no'. Others: 'does this amendment change your vote'. Both answered Yes/No by decider.py's parse."""
import json, glob, subprocess, time, sys, concurrent.futures as cf
S = 'C:/Users/<user>/AppData/Local/Temp/claude/1ff08f82160bcdeb/2ca12683-042f-40b7-b61a-623ae180176b/scratchpad/'
R = 'C:/Users/<user>/projects/phrase-lang/'
RF = 'C:/Users/<user>/AppData/Local/Temp/claude/1ff08f82160bcdeb/f5302246-709f-4d35-b4d7-d556237d7061/scratchpad/'
import sys as _s; _s.path.insert(0, 'C:/Users/<user>/AppData/Local/Temp/claude/1ff08f82160bcdeb/2ca12683-042f-40b7-b61a-623ae180176b/scratchpad'); import mkbackends; B = mkbackends.build()
did, prop, prefix, outl, oth = sys.argv[1:6]
driver = sys.argv[6] if len(sys.argv) > 6 else 'grok'
amended = open(R + prop, encoding='utf-8').read()
KEY = {'qa': ('role-qa.txt', 'qa'), 'auditlens': ('role-auditlens.txt', 'auditlens'), 'architect': ('role-architect.txt', 'architect'), 'dev': ('role-dev.txt', 'dev'),
       'decider': ('role-decider.txt', 'decider'), 'coach': ('role-coach.txt', 'coach')}


SEATS = {}


def prompt(role, outlier):
    rf, short = KEY[role]
    seat = None
    for cand in ('grok', 'claude', 'codex', 'muse'):
        f = glob.glob(f'{S}{prefix}-{short}-{cand}-out/**/decider-result.json', recursive=True)
        if f and list(json.load(open(f[0], encoding='utf-8'))['votes'].values())[0].get('counted'):
            seat = cand
            break
    v = json.load(open(f[0], encoding='utf-8'))['votes'][seat]
    SEATS[role] = seat
    r = v['reply']
    j = r.rfind('\nNo\n') if outlier else r.rfind('\n' + v['vote'] + '\n')
    reply = r[j + 1:].strip() if j >= 0 else r[-900:]
    if outlier:
        q = ('You, in this same role, voted No in the primary window of Decider ' + did + ' and stated one condition. The proposer adopted your condition and amended the text (the section '
             '"AMENDMENT AFTER THE PRIMARY WINDOW" states the adoption). You are asked ONE question only: is YOUR condition now met by the amended text below, yes or no? First line exactly "Yes" '
             '(my condition is met and I now support the proposal; once given what I asked for I drop all resistance) or "No" (my condition is not met: say in one sentence which part of it the text does not meet). '
             'A new demand, objection or condition is NOT adopted at this step and breaks the outlier\'s promise; raise none.')
    else:
        q = ('You, in this same role, voted ' + v['vote'] + ' in the primary window of Decider ' + did + '. Two outliers\' conditions were adopted as an amendment (the section "AMENDMENT AFTER THE PRIMARY WINDOW" '
             'of the text below). You are asked ONE narrow question about the AMENDMENT ONLY: does this amendment change your vote? Not a fresh review; a new gap or demand outside the amendment is not accepted. '
             'First line exactly "Yes" (the amendment does NOT change my vote: I still support the proposal) or "No" (the amendment changes my vote to No: one sentence about what in the amendment does it).')
    return (open(RF + rf, encoding='utf-8').read() + '\n\nDECIDER, ' + ('CONFIRMATION STEP' if outlier else 'AMENDMENT CHECK') + ' (the original protocol). ' + q +
            ' Then at most 60 words. Last line "CITE: <at least 6 consecutive words copied exactly from the amended proposal below, with their capitals>". Be quick; read only the text below.\n\n'
            'YOUR REPLY IN THE PRIMARY WINDOW (quoted):\n' + reply + '\n\nTHE AMENDED PROPOSAL:\n\n' + amended)


def job(item):
    role, outlier = item
    fn = S + f'conf-{did}-{role}.txt'
    open(fn, 'w', encoding='utf-8', newline='\n').write(prompt(role, outlier))
    for attempt in (1, 2, 3):
        out = S + f'conf-{did}-{role}-out{attempt}'
        subprocess.run(['python', 'C:/Users/<user>/mob-crosscli/decider.py', '--repo', R, '--proposal-file', fn, '--seats', SEATS.get(role, driver) if attempt == 1 else driver, '--backends', B, '--out', out, '--timeout', '420'],
                       stdout=open(S + f'conf-{did}-{role}.log{attempt}', 'w'), stderr=subprocess.STDOUT, timeout=900)
        g = glob.glob(out + '/**/decider-result.json', recursive=True)
        if g:
            vv = list(json.load(open(g[0], encoding='utf-8'))['votes'].values())[0]
            if vv.get('vote') and vv.get('counted') and vv.get('cite_ok'):
                print(role, '|', 'outlier' if outlier else 'other', '|', vv['vote'], '| attempt', attempt, flush=True)
                return {'role': role, 'kind': 'outlier' if outlier else 'other', 'vote': vv['vote'], 'attempt': attempt, 'reply': vv.get('reply', '')[-800:]}
        time.sleep(8)
    print(role, '| NONE', flush=True)
    return {'role': role, 'vote': None}


items = [(r, True) for r in outl.split(',') if r] + [(r, False) for r in oth.split(',') if r]
with cf.ThreadPoolExecutor(max_workers=len(items)) as ex:
    futs = []
    for it in items:
        futs.append(ex.submit(job, it))
        time.sleep(5)
    res = [f.result() for f in futs]
json.dump(res, open(S + f'conf-{did}.json', 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
ok = all(r.get('vote') == 'Yes' for r in res)
print('CONFIRM DONE', 'ALL YES' if ok else 'NOT ALL YES', [(r['role'], r.get('vote')) for r in res], flush=True)
