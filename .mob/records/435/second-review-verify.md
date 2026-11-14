# Card 435: the facilitator's replay of the second review's inputs (a console probe outside the repository linking PathScrub.cs at 547538f and PathScrubBefore435.cs; `before` is the pre-card scrub, `now` the green, `pass2` a second Apply when it differs)

```
# 3 \b \f after a doubled-backslash path
IN  C:\\a\\x.cs\b/home/alice/y.cs
  before y.cs
  now    x.cs\b/home/alice/y.cs
IN  C:\\a\\x.cs\f/home/alice/y.cs
  before y.cs
  now    x.cs\f/home/alice/y.cs
# 4c tab then double backslash drive
IN  \t\\C:\Users\<user>
  before \t\\<path>
  now    \t<path>
# 7 UNC prefix bridging
IN  ["\\host\share\x.cs","C:\Users\<user>\y.cs"]
  before ["x.cs","y.cs"]
  now    ["y.cs"]
IN  \u0027\\\\host\\share\\x.cs\u0027,\u0027/home/alice/y.cs\u0027
  before \u0027x.cs\u0027,\u0027y.cs\u0027
  now    \u0027y.cs\u0027
IN  \\host\share\x.cs;C:\Users\<user>\y.cs
  before x.cs;y.cs
  now    y.cs
# 9b
IN  \\/home/<user>\nx.cs
  before \\nx.cs
  now    \\<path>\nx.cs
  pass2  nx.cs
IN  \\/opt/data/home/alice
  before \\/opt/data/home/alice
  now    \\<path>
  pass2  <path>
IN  C:\\a\\~\nx.cs
  before nx.cs
  now    ~\nx.cs
  pass2  nx.cs
# 6 older doubled
IN  C:\\a\\x.cs\n/home/alice
  before alice
  now    x.cs\n<path>
TIME dbl-bs ; x6666 len=19998 before=19ms now=7139ms
TIME dbl-bs , x6666 len=19998 before=12ms now=6873ms
TIME dbl-bs a; x5000 len=20000 before=15ms now=4347ms
TIME unc-x.cs-n x870 len=20010 before=2ms now=717ms
TIME \; x6666 len=13332 before=9ms now=12ms
TIME \, x6666 len=13332 before=9ms now=12ms
TIME \a; x5000 len=15000 before=13ms now=18ms
```

Single-backslash rows (second run):

```
# 1 glued parens then a second path
IN  C:\Users\<user>(1)D:\Users\<user>\x.cs
  before <path>(1)x.cs
  now    <path>:\Users\<user>\x.cs
IN  C:\Users\<user>(bb)file:///C:/Users/<user>/x.cs
  before <path>(bb)x.cs
  now    <path>:///x.cs
# 2 escape then glued flag
IN  gcc /tmp/a.c\n-I/home/alice/include
  before gcc include
  now    gcc a.c\n-I/home/alice/include
IN  gcc /tmp/a.c\u0027-I/home/alice/include\u0027
  before gcc u0027
  now    gcc a.c\u0027-I/home/alice/include\u0027
# 4 drive letter consumed
IN  \u00f6/tmp/a\u00f6C:\\Users\\<user>
  before \u00f6a\u00f6<path>
  now    \u00f6a\u00f6C:\\Users\\<user>
  pass2  \u00f6a\u00f6<path>
IN  \t/tmp/a.C:\\Users\\<user>
  before \t/tmp/a.<path>
  now    \ta.C:\\Users\\<user>
  pass2  \ta.<path>
IN  \t/tmp/a.C:\Users\<user>
  before \t/tmp/a.<path>
  now    \ta.C:\Users\<user>
  pass2  \ta.<path>
# 8 sentence dot
IN  see C:\Users\<user>(bb).
  before see <path>(bb).
  now    see <path>
# 9 idempotence
IN  /tmp/a/~\nx.cs
  before nx.cs
  now    ~\nx.cs
  pass2  nx.cs
# 6 older (single backslash, no escape stop by design)
IN  gcc /tmp/a.c -I/home/alice
  before gcc alice
  now    gcc alice
IN  C:\a\x.cs\n/home/alice
  before alice
  now    alice
# mutant 2 killer row (passes on the green; fails under mutant 2 per two seats)
IN  /d/users/<user>
  before <path>
  now    <path>
```
