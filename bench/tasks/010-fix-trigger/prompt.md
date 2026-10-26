Fix the failing spec in this Phrase module. Do not change the intended behavior or remove the spec. Return only valid Phrase source.

```phrase
module Greeting

phrase "greeting" -> Text
  spec "greets"
    when handled
    then returns "hello"
  native csharp
    return "hello";
```
