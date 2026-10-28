# Phrase patterns

Patterns keep portable intent in Phrase while allowing each target adapter to emit idiomatic code. They are preferred over generated bodies and native blocks when the base library has a matching universal operation.

## Ensure

An ensure step states a condition that must hold and the declared failure raised when it does not:

```phrase
phrase "validate task title {title: Text}"
  fails with EmptyTitle, TitleTooLong
  steps
    ensure title is not blank else EmptyTitle
    ensure title length at most 200 else TitleTooLong
```

The failure after `else` must appear in the containing phrase's `fails with` list. An endpoint may also use an ensure step; its failure becomes one of the endpoint's effective failures.

The condition vocabulary is deliberately small:

- `<text> is blank` and `<text> is not blank`
- `<text> length at most <integer>`
- `<int> <operator> <integer-or-int-input>`, where the operator is `<`, `<=`, `>`, `>=`, `==`, or `!=`

The core syntax and base-library pattern define the condition and declared-failure semantics. The C# adapter maps them to early-return-style guard clauses that raise the declared `PhraseFailure`; for example, `is not blank` becomes a `string.IsNullOrWhiteSpace` guard and `length at most 200` becomes a length guard. A project profile may select a different idiomatic representation later without changing the Phrase source.
