# Card 439 review round 2: the facilitator's replay of finding 1 and 2 (pre435, the replaced regex 7ee5411, the architect's fixed reference, the port at 4e4893d)

```
# colon lists
IN  docker run -v /home/<user>/work:/home/<user> -it ubuntu
  pre435 docker run -v work:<path>
  regex  docker run -v work:<path>
  ref    docker run -v woralice -it ubuntu
  green  docker run -v woralice -it ubuntu
IN  PYTHONPATH=/home/<user>/lib:/home/<user>
  pre435 PYTHONPATH=lib:<path>
  regex  PYTHONPATH=lib:<path>
  ref    PYTHONPATH=lialice
  green  PYTHONPATH=lialice
IN  PATH=/home/<user>/bin:/home/<user>/.local/bin:/usr/bin
  pre435 PATH=bin:bin:/usr/bin
  regex  PATH=bin:bin:/usr/bin
  ref    PATH=bibibin
  green  PATH=bibibin
IN  LD_LIBRARY_PATH=/home/<user>/lib:/home/<user>/.local/lib
  pre435 LD_LIBRARY_PATH=lib:lib
  regex  LD_LIBRARY_PATH=lib:lib
  ref    LD_LIBRARY_PATH=lilib
  green  LD_LIBRARY_PATH=lilib
```
