"""Retro voices for card 176, ALL SEVEN AT ONCE through decider.py's gate (parallel dispatch). Usage: run_voices_par.py role=driver ..."""
import sys, os, json, glob, subprocess, datetime, concurrent.futures as cf
OUT = os.path.dirname(os.path.abspath(__file__))
ROLES = "C:/Users/ringe/AppData/Local/Temp/claude/1ff08f82160bcdeb/f5302246-709f-4d35-b4d7-d556237d7061/scratchpad"
import sys as _s; _s.path.insert(0, "C:/Users/ringe/AppData/Local/Temp/claude/1ff08f82160bcdeb/2ca12683-042f-40b7-b61a-623ae180176b/scratchpad"); import mkbackends; BACK = mkbackends.build()
card = "176"
ASK = (f"\n\nRETRO VOICE for card {card} (each role gives feedback in its own fresh context). Read .mob/records/{card}/retro-input.md first, then as useful "
       f".mob/records/{card}/ (red-record.md, gate-review.md, signoff.md, seat-plan.md), docs/decisions/process-and-staffing.md D211 to D214 and docs/retros/experiments.md (the section on what turns a Yes into a No). "
       "Be quick: reply in under 300 words with these headings: ADJUSTMENT REVIEW (the Experimental entries the card exercised: yes/no/partly/unknown with evidence, verdict keep/tune/revert/adopt); "
       "FEEDBACK LOOPS (who or what gives the feedback, its latency, MISSING if none); DRIFT; INDIVIDUAL; TEAM; ORGANISATION; PRODUCT (each: one improvement with evidence and benefit to the user, or 'kept, no change'); "
       "KEEP OR REMOVE (task-force members and seats); PROCESS TIME AGAINST BUILD TIME (the product owner's finding: about 100 minutes of Deciders against about 25 of building; what one change would cut the decision time without losing the safeguard?); "
       "ONE PROCESS IMPROVEMENT CANDIDATE (hypothesis, metric, counter-metric, trial scope; one concern only). Also say whether the red-first rule held on this card.\n")


def run(tag, text, seat):
    open(f'{OUT}/{tag}.txt', 'w', encoding='utf-8', newline='\n').write(text)
    open(f'{OUT}/{tag}.start', 'w').write(datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'))
    try:
        subprocess.run(['python', 'C:/Users/ringe/mob-crosscli/decider.py', '--repo', 'C:/Users/ringe/projects/phrase-lang', '--proposal-file',
                        f'{OUT}/{tag}.txt', '--seats', seat, '--backends', BACK, '--out', f'{OUT}/{tag}-out', '--timeout', '420'],
                       stdout=open(f'{OUT}/{tag}.log', 'w'), stderr=subprocess.STDOUT, timeout=900)
    except subprocess.TimeoutExpired:
        open(f'{OUT}/{tag}.log', 'a').write('[runner] exceeded 900 s; treated as no reply\n')
    f = glob.glob(f'{OUT}/{tag}-out/**/decider-result.json', recursive=True)
    if not f:
        return ''
    return json.load(open(f[0], encoding='utf-8'))['votes'].get(seat, {}).get('reply', '')


def job(pair):
    role, driver = pair.split('=')
    text = open(f'{ROLES}/role-{role}.txt', encoding='utf-8').read() + ASK
    order = {'grok': ['grok', 'claude'], 'claude': ['claude']}[driver]
    for seat in order:
        rep = run(f'r176-{role}-{seat}', text, seat)
        if 'ADJUSTMENT REVIEW' in rep.replace('**', '').upper():
            i = rep.find('Reading additional input')
            open(f'{OUT}/r176-{role}.final.json', 'w', encoding='utf-8', newline='\n').write(json.dumps({"seat": seat, "reply": rep[:i].rstrip() if i > 0 else rep}, ensure_ascii=False))
            print(role, '|', seat, flush=True)
            return role, seat
    print(role, '| NONE', flush=True)
    return role, None


pairs = sys.argv[1:]
with cf.ThreadPoolExecutor(max_workers=len(pairs)) as ex:
    res = list(ex.map(job, pairs))
print('VOICES DONE', [(r, s) for r, s in res], flush=True)
