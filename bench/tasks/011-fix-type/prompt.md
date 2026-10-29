Fix the type error in this Avowry module while preserving the avow name and the spec's expected value. Return only valid Avowry source.

```avowry
module Count

avow to "answer" -> Text
  spec "is forty two"
    when called
    then returns 42
  native csharp
    return 42;
```
