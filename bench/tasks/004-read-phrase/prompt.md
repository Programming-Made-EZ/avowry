Read this Phrase source and return it unchanged except for comments explaining the phrase and its spec. Comments must begin with `#`. Return only valid Phrase source.

```phrase
module Identity

phrase "identity of {value: Text}" -> Text
  spec "returns its input"
    given value = "sample"
    when called
    then returns "sample"
  native csharp
    return value;
```
