# Card 441 review round 1: the facilitator's replay (pre435, the replaced regex 7ee5411, the architect's updated reference, the green at ede15fd; green2 = a second pass when it differs)

```
# 441 review finding 1: a drive right after /
IN  jar:file:/c:/users/<user>/.m2/a.jar!/x.class
  pre435 jar:file:/x.class
  regex  jar:file:/x.class
  ref    jar:file:/c:/users/<user>/.m2/a.jar!/x.class
  green  jar:file:/c:/users/<user>/.m2/a.jar!/x.class
IN  java.io.FileNotFoundException: /c:/users/<user>/x.txt
  pre435 java.io.FileNotFoundException: /x.txt
  regex  java.io.FileNotFoundException: /x.txt
  ref    java.io.FileNotFoundException: /c:/users/<user>/x.txt
  green  java.io.FileNotFoundException: /c:/users/<user>/x.txt
IN  file:/C:/Documents and Settings/<user>/.m2/repository/a.jar
  pre435 file:/a.jar
  regex  file:/a.jar
  ref    file:/C:/Documents and Settings/<user>/.m2/repository/a.jar
  green  file:/C:/Documents and Settings/<user>/.m2/repository/a.jar
# finding 3: Windows forward-slash volume spec with a one-letter folder
IN  docker run -v D:/a:/a x
  pre435 docker run -v a:/a x
  regex  docker run -v <path>a x
  ref    docker run -v <path>a:/a x
  green  docker run -v <path>a:/a x
  green2 docker run -v <path>a x
IN  docker run -v C:/Users/<user>/d:/data x
  pre435 docker run -v d:/data x
  regex  docker run -v <path>data x
  ref    docker run -v <path>d:/data x
  green  docker run -v <path>d:/data x
  green2 docker run -v <path>data x
# finding 2: JSON escaped colon list
IN  {"env":"PYTHONPATH=\/home\/<user>\/lib:\/home\/<user>"}
  pre435 {"env":"PYTHONPATH=\/home\/<user>\/lib:\/home\/<user>"}
  regex  {"env":"PYTHONPATH=\/home\/<user>\/lib:\/home\/<user>"}
  ref    {"env":"PYTHONPATH=lialice"}
  green  {"env":"PYTHONPATH=lialice"}
```
