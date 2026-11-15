# Card 439 review round 3: the facilitator's replay (pre435, the replaced regex 7ee5411, the architect's second fixed reference, the port at 4b9966b; green2 = a second pass when it differs)

```
# round 3 finding 1 and 2: Windows-host volume specs
IN  docker run -v C:\Users\<user>\proj:/home/<user> ubuntu
  pre435 docker run -v proj:<path>
  regex  docker run -v proj:<path>
  ref    docker run -v proalice ubuntu
  green  docker run -v proalice ubuntu
IN  docker run -v C:/Users/<user>/proj:/home/<user> ubuntu
  pre435 docker run -v proj:<path>
  regex  docker run -v proj:<path>
  ref    docker run -v proalice ubuntu
  green  docker run -v proalice ubuntu
IN  D:\data\proj:/home/<user>
  pre435 proj:<path>
  regex  proj:<path>
  ref    proalice
  green  proalice
IN  docker run -v C:\Users\<user>\proj:/app node
  pre435 docker run -v proj:/app node
  regex  docker run -v proj:/app node
  ref    docker run -v proapp node
  green  docker run -v proapp node
# finding 3: an account folder followed by :/
IN  docker run -v /home/<user>:/data ubuntu
  pre435 docker run -v <path>:/data ubuntu
  regex  docker run -v <path>e:/data ubuntu
  ref    docker run -v <path>e:/data ubuntu
  green  docker run -v <path>e:/data ubuntu
  green2 docker run -v <path>data ubuntu
IN  /home/<user>:/bin/bash
  pre435 <path>:/bin/bash
  regex  <path>e:/bin/bash
  ref    <path>e:/bin/bash
  green  <path>e:/bin/bash
  green2 <path>bash
```
