# Card 439 review round 1: the facilitator's replay (a probe outside the repository: pre435 = PathScrubBefore435, regex = the replaced PathScrub.cs at 7ee5411, ref = the architect's reference scanner, green = PathScrub.cs at 1ae9d03; times are one run each)

```
# 4 mixed slash, account starting with an escape letter
IN  C:/Users\<user>\x.cs
  pre435 x.cs
  regex  x.cs
  ref    Users\riley\x.cs
  green  Users\riley\x.cs
IN  C:/Users\<user>\x.cs
  pre435 x.cs
  regex  x.cs
  ref    Users\nate\x.cs
  green  Users\nate\x.cs
IN  file:///C:/Users\<user>\x.cs
  pre435 x.cs
  regex  x.cs
  ref    Users\riley\x.cs
  green  Users\riley\x.cs
IN  /home/<user>\notes\run-426
  pre435 run-426
  regex  <path>\notes\run-426
  ref    <path>\notes\run-426
  green  <path>\notes\run-426
# 5 prefix folders before a drive-style users root
IN  /mnt/wsl/c/users/<user>/x.cs
  pre435 /mnt/wsl/c/users/<user>/x.cs
  regex  x.cs
  ref    x.cs
  green  /mnt/wsl/c/users/<user>/x.cs
IN  /opt/cygdrive/c/users/<user>/x.cs
  pre435 /opt/cygdrive/c/users/<user>/x.cs
  regex  x.cs
  ref    x.cs
  green  /opt/cygdrive/c/users/<user>/x.cs
IN  /a/c/users/<user>
  pre435 /a/c/users/<user>
  regex  <path>
  ref    <path>
  green  /a/c/users/<user>
# 6 bridged second path in parentheses
IN  see C:/temp/x.cs (/home/<user>)
  pre435 see alice)
  regex  see alice)
  ref    see alice)
  green  see alice)
IN  see ~/a.cs (/home/<user>)
  pre435 see alice)
  regex  see alice)
  ref    see alice)
  green  see alice)
# 7 JSON escaped regex before a path field
IN  {"pattern":"\\d+","path":"C:\\Users\\<user>\\x.cs"}
  pre435 {"pattern":"x.cs"}
  regex  {"pattern":"d+","path":"x.cs"}
  ref    {"pattern":"\\d+","path":"x.cs"}
  green  {"pattern":"x.cs"}
# 8 MSBuild diagnostic after an account folder
IN  C:\Users\<user> bb [Verification.csproj]
  pre435 <path> [Verification.csproj]
  regex  <path> [Verification.csproj]
  ref    <path> [Verification.csproj]
  green  <path>[Verification.csproj]
IN  C:\Users\<user> bb "is denied"
  pre435 <path> "is denied"
  regex  <path> "is denied"
  ref    <path> "is denied"
  green  <path>"is denied"
IN  C:\\Users\\<user> .\nnext
  pre435 nnext
  regex  <path> .\nnext
  ref    <path> .\nnext
  green  <path>\nnext
TIME /=-I n=5000 ref=214ms green=251ms
TIME /=-I n=10000 ref=458ms green=544ms
TIME /=-I n=20000 ref=1226ms green=1981ms
TIME /(-I n=5000 ref=78ms green=112ms
TIME /(-I n=10000 ref=259ms green=488ms
TIME /(-I n=20000 ref=1293ms green=2383ms
TIME a+spaces n=5000 ref=0ms green=0ms
TIME a+spaces n=10000 ref=0ms green=1ms
TIME a+spaces n=20000 ref=0ms green=1ms
```
