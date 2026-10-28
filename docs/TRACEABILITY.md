# Generated C# traceability

The C# adapter emits stable `// phrase:` comments containing the portable Phrase filename,
declaration line, and declaration name. Generated files point to the module declaration;
generated endpoint and phrase methods point to their own declarations. Paths supplied by the
CLI are reduced to their filename so generated output contains no machine-specific absolute path.

Declared failures returned by generated HTTP endpoints carry the same information in the
`X-Phrase-Source` response header:

```text
X-Phrase-Source: Tasks.phrase:52 endpoint CreateTask failure EmptyTitle
```

The header was chosen instead of changing the error document. The existing FastEndpoints 422
body remains the public failure body: `statusCode`, `message`, and `errors.generalErrors` are
unchanged.
