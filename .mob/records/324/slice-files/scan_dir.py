import sys, os, re
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from scan import literals
root = sys.argv[1]; pat = re.compile(sys.argv[2])
tot = 0
for d, ds, fs in os.walk(root):
    ds[:] = [x for x in ds if x not in ('obj', 'bin', '.git')]
    for f in sorted(fs):
        if f.endswith('.cs'):
            p = os.path.join(d, f)
            for ln, kind, interp, body in literals(open(p, encoding='utf-8', newline='').read()):
                b = body.replace('\x00HOLE\x00', '')
                if pat.search(b):
                    tot += 1
                    print(os.path.relpath(p, root).replace(os.sep, '/'), ln, repr(b))
print('total', tot)
