import sys, os

# usage: python -I edits.py <repo-root> <stage: 1 | 3 | all | rev>  (rev undoes all 19 edits: the byte-parity check)
# Each edit: (relative file, 1-based line, cycle, old fragment, new fragment). The old fragment must occur
# exactly once on that line; the file is read and written as bytes (LF and ASCII preserved).
R = "src/Avowry.Compiler/Semantics/Resolver.cs"
P = "src/Avowry.Compiler/Syntax/Parser.cs"
EDITS = [
    (R, 105, 1, "Phrase '", "Avow '"),
    (R, 187, 1, "Phrase '", "Avow '"),
    (R, 200, 1, "Phrase '", "Avow '"),
    (R, 331, 1, "Phrase '", "Avow '"),
    (R, 400, 1, "Phrase specs cannot", "Avow specs cannot"),
    (R, 408, 1, "Phrase spec for '", "Avow spec for '"),
    (R, 556, 1, "Phrase '", "Avow '"),
    (P, 364, 1, "Phrase has no implementation", "Avow has no implementation"),
    (R, 119, 3, "Composite phrase '", "Composite avow '"),
    (R, 351, 3, "Composite phrase '", "Composite avow '"),
    (R, 389, 3, "on phrase '", "on avow '"),
    (R, 566, 3, "for a phrase that returns", "for an avow that returns"),
    (R, 729, 3, "No phrase matches", "No avow matches"),
    (P, 291, 3, "a quoted phrase template", "a quoted avow template"),
    (P, 341, 3, "A phrase is implemented", "An avow is implemented"),
    (P, 353, 3, "A phrase is implemented", "An avow is implemented"),
    (P, 358, 3, "a phrase clause", "an avow clause"),
    (P, 699, 3, "a phrase call", "an avow call"),
    (P, 873, 3, "(phrase spec)", "(avow spec)"),
]

def main():
    root, stage = sys.argv[1], sys.argv[2]
    rev = stage == "rev"
    want = {"1": {1}, "3": {3}, "all": {1, 3}, "rev": {1, 3}}[stage]
    data = {}
    for f in (R, P):
        with open(os.path.join(root, f), "rb") as fh:
            data[f] = fh.read()
    before = {f: len(b) for f, b in data.items()}
    for f, line, cycle, old, new in EDITS:
        if cycle not in want:
            continue
        lines = data[f].split(b"\n")
        text = lines[line - 1]
        o, n = old.encode("ascii"), new.encode("ascii")
        if rev:
            o, n = n, o
        if text.count(o) != 1:
            sys.exit("FAIL %s:%d old fragment %r occurs %d times" % (f, line, old, text.count(o)))
        lines[line - 1] = text.replace(o, n)
        data[f] = b"\n".join(lines)
    for f, b in data.items():
        if b"\r" in b or any(c > 127 for c in b):
            sys.exit("FAIL %s has CR or non-ASCII" % f)
        with open(os.path.join(root, f), "wb") as fh:
            fh.write(b)
        print(f, before[f], "->", len(b), "delta", len(b) - before[f])

main()
