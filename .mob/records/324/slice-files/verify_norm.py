import re, os
S = os.path.dirname(os.path.abspath(__file__))
# (old fragment, new fragment): the whole normalisation, 13 exact pairs
PAIRS = [
    ("Phrase '", "Avow '"),
    ("Phrase specs cannot", "Avow specs cannot"),
    ("Phrase spec for '", "Avow spec for '"),
    ("Phrase has no implementation", "Avow has no implementation"),
    ("Composite phrase '", "Composite avow '"),
    (" on phrase '", " on avow '"),
    ("for a phrase that returns", "for an avow that returns"),
    ("No phrase matches", "No avow matches"),
    ("A phrase is implemented", "An avow is implemented"),
    ("a quoted phrase template", "a quoted avow template"),
    ("a phrase clause", "an avow clause"),
    ("a phrase call", "an avow call"),
    ("(phrase spec)", "(avow spec)"),
]
TO_OLD = {n: o for o, n in PAIRS}
RX = re.compile("|".join(re.escape(n) for n in sorted(TO_OLD, key=len, reverse=True)))

def to_old(text):
    return RX.sub(lambda m: TO_OLD[m.group(0)], text)

def load(path):
    d, cur = {}, None
    for l in open(path, encoding="utf-8").read().replace("\r\n", "\n").split("\n"):
        m = re.match(r"## (\S+)  ", l) or re.match(r"### (\S+)$", l)
        if m:
            cur = m.group(1); d[cur] = []
        elif l.strip().startswith("PH"):
            d[cur].append(l.strip())
    return d

base = load(os.path.join(S, "baseline.txt"))
for name in ("out_pre.txt", "out_s1.txt", "out_sall.txt"):
    act = load(os.path.join(S, name))
    bad = [k for k in base if [to_old(x) for x in act[k]] != base[k]]
    print(name, "normalised == baseline for", len(base) - len(bad), "of", len(base), "bad:", bad)

drifts = ["Avow  'x' has the same wording as 'y'.", "An Avow '", "Avow' x", "an avow '", "Avow Composite '",
          "Composite Avow '", "No avows match", "Expected a avow clause", "Expected an  avow call"]
for d in drifts:
    print(repr(d), "-> unchanged-by-normaliser (kept different from any old text):", to_old(d) == d or "mapped to " + repr(to_old(d)))
# round trip: forward on old text gives the new text exactly
FW = re.compile("|".join(re.escape(o) for o, _ in sorted(PAIRS, key=lambda p: -len(p[0]))))
FMAP = dict(PAIRS)
for k, rows in base.items():
    for r in rows:
        t = r.split("|", 3)[3]
        new = FW.sub(lambda m: FMAP[m.group(0)], t)
        assert to_old(new) == t, (k, t)
print("round trip ok; messages with unit word:", sum(1 for rows in base.values() for r in rows if FW.search(r)))
