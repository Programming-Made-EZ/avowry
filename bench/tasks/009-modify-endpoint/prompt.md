Modify this endpoint to return an output record containing `message: Text`, respond with `"pong"`, and include a spec proving it. Return only valid Avowry source.

```avowry
module Ping

endpoint Ping : GET /ping
  input
    requestId: Id
  steps
    fail with NotImplemented
```
