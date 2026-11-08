"""Post one GitHub Discussion per Decider (General category) from the committed decision record and votes file.
Usage: post_discussions.py D208 D209 ...  Prints the URL of each thread; appends id, url to the scratchpad log (the facilitator commits the log)."""
import sys, re, json, subprocess, os, glob
G = 'C:/Users/ringe/projects/phrase-lang/'
SP = os.path.dirname(os.path.abspath(__file__)) + '/'
REPO_ID = 'R_kgDOVSbx0g'
CAT = 'DIC_kwDOVSbx0s4DKM2e'  # General
text = open(G + 'docs/decisions/process-and-staffing.md', encoding='utf-8').read()


def section(d):
    m = re.search(r'(?m)^#### Decider %s: .*$' % d, text)
    assert m, d
    e = re.search(r'(?m)^#### ', text[m.end():])
    return text[m.start(): m.end() + (e.start() if e else len(text) - m.end())].strip()


def votes_md(d):
    f = G + f'.mob/records/deciders/{d}-votes.json'
    if not os.path.exists(f):
        return ''
    j = json.load(open(f, encoding='utf-8'))
    out = ['## Votes (from .mob/records/deciders/%s-votes.json)' % d]
    for k, v in j.items():
        if not isinstance(v, dict) or not all(isinstance(x, list) for x in v.values()):
            continue
        out.append(f'\n**{k.replace("_", " ")}**\n')
        out.append('| Role | Seat | Vote | Counted | Reasoning (start) |')
        out.append('|---|---|---|---|---|')
        for role, ents in v.items():
            for e in ents:
                if not e.get('vote'):
                    out.append(f'| {role} | {e.get("seat")} | - | no | {e.get("note", "")[:160]} |')
                    continue
                rh = (e.get('reply_head') or '').replace('\n', ' ').replace('|', '/')[:240]
                note = (' [' + e['note'][:80] + ']') if e.get('note') else ''
                out.append(f'| {role} | {e.get("seat")} | {e["vote"]} | {"yes" if e.get("counted_by_decider") else "no"} | {rh}{note} |')
    return '\n'.join(out)


for d in sys.argv[1:]:
    sec = section(d)
    title = re.match(r'#### (Decider %s: .*)' % d, sec).group(1)[:240]
    body = ('This thread records one Decider of the Avowry mob. The git records stay the evidence the mob\'s checks read (docs/decisions/process-and-staffing.md, '
            '.mob/records/deciders/%s-votes.json); this thread is the place to read and discuss it. Any product-owner comment here is input to the Decider.\n\n'
            '## The decision record\n\n%s\n\n%s\n') % (d, sec.replace('#### ', '### ', 1), votes_md(d))
    bf = SP + f'disc-{d}.md'
    open(bf, 'w', encoding='utf-8', newline='\n').write(body)
    q = 'mutation($r:ID!,$c:ID!,$t:String!,$b:String!){ createDiscussion(input:{repositoryId:$r,categoryId:$c,title:$t,body:$b}){ discussion{ number url } } }'
    r = subprocess.run(['gh', 'api', 'graphql', '-f', 'query=' + q, '-f', 'r=' + REPO_ID, '-f', 'c=' + CAT, '-f', 't=' + title, '-F', 'b=@' + bf],
                       capture_output=True, text=True, encoding='utf-8')
    if r.returncode:
        print(d, 'FAILED', r.stderr[:300]); continue
    dd = json.loads(r.stdout)['data']['createDiscussion']['discussion']
    print(d, dd['number'], dd['url'], len(body), flush=True)
    open(SP + 'discussions.log', 'a', encoding='utf-8').write(f'{d}\t{dd["number"]}\t{dd["url"]}\n')
