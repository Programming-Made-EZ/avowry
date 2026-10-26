Modify this endpoint to return an output record containing `message: Text`, respond with `"pong"`, and include a spec proving it. Return only valid Phrase source.

```phrase
module Ping

endpoint Ping : GET /ping
  input
    requestId: Id
  steps
    fail with NotImplemented
```
