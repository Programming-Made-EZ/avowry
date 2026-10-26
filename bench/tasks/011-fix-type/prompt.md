Fix the type error in this Phrase module while preserving the phrase name and the spec's expected value. Return only valid Phrase source.

```phrase
module Count

phrase "answer" -> Text
  spec "is forty two"
    when called
    then returns 42
  native csharp
    return 42;
```
