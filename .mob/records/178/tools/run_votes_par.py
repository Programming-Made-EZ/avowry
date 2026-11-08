"""Decider window through decider.py's gate, ALL SIX ROLES LAUNCHED AT ONCE (D174, the agreement's parallel voter dispatch).
Usage: run_votes_par.py TAG role=driver ...   env: PROPFILE, CTX (optional), RERUN=1.
Each role is its own decider.py invocation (each needs its own role brief), run in its own thread; a seat that gives no counted vote
falls back to the other non-Claude driver, then Claude, as a second batch for that role only (D171 re-asks run in parallel).
Prints one line per role as it finishes, then the mechanical tally: the 30 percent rule (No plus Support-it more than about 30 percent
of the seats drops the proposal) and the strict rule (any No fails it)."""
import sys, os, json, glob, subprocess, datetime, concurrent.futures as cf
OUT = os.path.dirname(os.path.abspath(__file__))
ROLES = "C:/Users/ringe/AppData/Local/Temp/claude/1ff08f82160bcdeb/f5302246-709f-4d35-b4d7-d556237d7061/scratchpad"
import sys as _s; _s.path.insert(0, "C:/Users/ringe/AppData/Local/Temp/claude/1ff08f82160bcdeb/2ca12683-042f-40b7-b61a-623ae180176b/scratchpad"); import mkbackends; BACK = mkbackends.build()
tag = sys.argv[1]
PROP = open(os.environ['PROPFILE'], encoding='utf-8').read()
RR = os.environ.get('RERUN') == '1'
BRIEF = ('HOW TO VOTE. Yes means "I endorse this": reservations or suggestions go in your reply as notes and do not downgrade the vote. '
         'Support-it means "this is not what I would choose, but I will not block it": a real signal of weak buy-in, and it counts toward dropping the proposal '
         '(No plus Support-it votes of more than about 30 percent of the seats drop it). No means one precise condition that would bring you to Yes, with the '
         "outlier's promise: once it is given you drop all resistance. Do not use Support-it as a default for Yes-with-notes.")


def run(t, text, seat):
    open(f'{OUT}/{t}.txt', 'w', encoding='utf-8', newline='\n').write(text)
    open(f'{OUT}/{t}.start', 'w').write(datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'))
    try:
        subprocess.run(['python', 'C:/Users/ringe/mob-crosscli/decider.py', '--repo', 'C:/Users/ringe/projects/phrase-lang', '--proposal-file',
                        f'{OUT}/{t}.txt', '--seats', seat, '--backends', BACK, '--out', f'{OUT}/{t}-out', '--timeout', '420'],
                       stdout=open(f'{OUT}/{t}.log', 'w'), stderr=subprocess.STDOUT, timeout=900)
    except subprocess.TimeoutExpired:
        open(f'{OUT}/{t}.log', 'a').write('[runner] exceeded 900 s; treated as no reply\n')
    f = glob.glob(f'{OUT}/{t}-out/**/decider-result.json', recursive=True)
    if not f:
        return None
    v = json.load(open(f[0], encoding='utf-8'))['votes'].get(seat, {})
    return v['vote'] if v.get('vote') in ('Yes', 'Support-it', 'No') and v.get('counted') and v.get('cite_ok') else None


def role_job(pair):
    role, driver = pair.split('=')
    head = 'DECIDER (full vote, independent re-run in a fresh context). ' if RR else 'DECIDER (full vote). '
    ctx = os.environ.get('CTX', '')
    text = open(f'{ROLES}/role-{role}.txt', encoding='utf-8').read() + '\n\n' + head + BRIEF + '\n\n' + ctx + '\n\n' + PROP
    order = {'grok': ['grok', 'claude'], 'codex': ['codex', 'grok', 'claude'], 'claude': ['claude', 'grok'], 'muse': ['muse', 'grok', 'claude'], 'cursor': ['grok', 'claude']}[driver]
    for seat in order:
        vote = run(f'{tag}-{role}-{seat}', text, seat)
        if vote:
            print(role, '|', seat, '|', vote, flush=True)
            return role, seat, vote
    print(role, '| NONE | -', flush=True)
    return role, None, None


pairs = sys.argv[2:]
with cf.ThreadPoolExecutor(max_workers=len(pairs)) as ex:
    results = list(ex.map(role_job, pairs))
n = len(results)
no = sum(1 for r in results if r[2] == 'No')
sup = sum(1 for r in results if r[2] == 'Support-it')
yes = sum(1 for r in results if r[2] == 'Yes')
none = sum(1 for r in results if r[2] is None)
share = (no + sup) / n
print(f'TALLY yes={yes} support_it={sup} no={no} no_vote={none} seats={n} no_plus_support_share={share:.2f}', flush=True)
verdict = 'FAILS (a No)' if no else ('DROPPED by the 30 percent rule' if share > 0.30 else ('INCOMPLETE (a seat gave no vote)' if none else 'PASSES'))
print('RESULT', verdict, flush=True)
print('VOTES DONE', flush=True)
