Read this Phrase source and add comments explaining when `MissingName` can occur and what the spec proves. Preserve behavior and return only valid Phrase source.

```phrase
module Names

phrase "ensure {name: Text} has a value"
  fails with MissingName
  spec "accepts a name"
    given name = "Ada"
    when called
    then succeeds
  native csharp
    if (string.IsNullOrEmpty(name)) throw new PhraseFailure("MissingName");
```
