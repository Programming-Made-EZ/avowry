Read this Avowry source and add comments explaining when `MissingName` can occur and what the spec proves. Preserve behavior and return only valid Avowry source.

```avowry
module Names

avow to "ensure {name: Text} has a value"
  fails with MissingName
  spec "accepts a name"
    given name = "Ada"
    when called
    then succeeds
  native csharp
    if (string.IsNullOrEmpty(name)) throw new AvowryFailure("MissingName");
```
