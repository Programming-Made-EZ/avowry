Fix the failing endpoint spec so it matches the endpoint contract. Preserve the endpoint behavior and return only valid Avowry source.

```avowry
module Lookup

endpoint Find : GET /find
  input
    id: Id
  output
    found: Bool
  steps
    respond with { found: true }
  spec "finds an item"
    given input { id: "item-1" }
    when called
    then returns true
```
