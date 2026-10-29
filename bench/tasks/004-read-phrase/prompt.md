Read this Avowry source and return it unchanged except for comments explaining the avow and its spec. Comments must begin with `#`. Return only valid Avowry source.

```avowry
module Identity

avow to "identity of {value: Text}" -> Text
  spec "returns its input"
    given value = "sample"
    when called
    then returns "sample"
  native csharp
    return value;
```
