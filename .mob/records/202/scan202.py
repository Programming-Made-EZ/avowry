import re, subprocess
files = subprocess.run(['git', 'ls-files', '*.avowry'], capture_output=True, text=True, cwd='C:/Users/ringe/projects/phrase-lang').stdout.split()
bad = []
total = 0
for f in files:
    if f.startswith('.mob/'):
        continue
    text = open('C:/Users/ringe/projects/phrase-lang/' + f, encoding='utf-8').read().replace('\r\n', '\n')
    blocks = re.split(r'(?m)^(?=avow to |endpoint |type |store )', text)
    for b in blocks:
        if not b.startswith('avow to '):
            continue
        declared = set()
        for m in re.finditer(r'(?m)^\s*fails with\s+(.+)$', b):
            declared |= {x.strip() for x in m.group(1).split(',')}
        for m in re.finditer(r'Fail\("([^"]+)"\)', b):
            total += 1
            if m.group(1) not in declared:
                bad.append((f, m.group(1), sorted(declared), b.splitlines()[0][:50]))
print(len(files), 'files', total, 'Fail literals')
for x in bad:
    print('UNDECLARED', x)
