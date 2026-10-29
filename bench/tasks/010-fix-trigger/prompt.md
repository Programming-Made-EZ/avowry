Fix the failing spec in this Avowry module. Do not change the intended behavior or remove the spec. Return only valid Avowry source.

```avowry
module Greeting

avow to "greeting" -> Text
  spec "greets"
    when handled
    then returns "hello"
  native csharp
    return "hello";
```
