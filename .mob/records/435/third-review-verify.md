# Card 435: the facilitator's replay of the third review's inputs (a probe outside the repository linking PathScrub.cs at e374ab7 and PathScrubBefore435.cs)

```
# R1 placeholder UNC host
IN  \\<host>\Users\<user>
  before <path>
  now    \\<host>\Users\<user>
IN  \\<host>\Users\<user>\x.cs
  before x.cs
  now    \\<host>\Users\<user>\x.cs
# R2 UNC host with parens, apostrophe, escaped letter
IN  \\host(1)\Users\<user>
  before <path>
  now    alice
IN  \\host's\Users\<user>
  before <path>
  now    alice
IN  \\\\h\u00f6st\\Users\\<user>
  before alice
  now    alice
# R3 escaped quote then glued flag on a drive path
IN  \u0027-IC:\a\x.cs\u0027/home/<user>
  before \u0027-IC:\a\x.cs\u0027<path>
  now    \u0027-Ialice
IN  gcc \u0027-IC:\Users\<user>\include\u0027
  before gcc \u0027-IC:\Users\<user>\include\u0027
  now    gcc \u0027-Iu0027
IN  see a\u00f6-b/tmp/x.cs
  before see a\u00f6-b/tmp/x.cs
  now    see a\u00f6-bx.cs
# R4 tilde and drive guard idempotence
IN  foo-~\nx.cs
  before foo-nx.cs
  now    foo-nx.cs
IN  C:\Users\<user>:\Users\<user>\x.cs
  before <path>:\Users\<user>\x.cs
  now    <path>D:\Users\<user>\x.cs
  pass2  <path>x.cs
# R5 tail
IN  C:\Users\<user>(1)-D:\Users\<user>\x.cs
  before <path>(1)-x.cs
  now    <path>-x.cs
TIME bs-bs-C:/a-n x1250 len=10000 before=3ms now=619ms
TIME bs-bs-C:/a-n x2500 len=20000 before=5ms now=1751ms
TIME dbl-bs ; x6666 len=19998 before=10ms now=7ms
TIME dbl-bs , x6666 len=19998 before=5ms now=6ms
TIME dbl-bs a; x5000 len=20000 before=5ms now=8ms
TIME unc-x.cs-n x870 len=20010 before=1ms now=5ms
TIME \; x6666 len=13332 before=4ms now=5ms
TIME \, x6666 len=13332 before=4ms now=5ms
TIME \a; x5000 len=15000 before=6ms now=8ms
```
