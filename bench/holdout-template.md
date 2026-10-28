<!-- nonce: REPLACE-WITH-32-RANDOM-HEX-CHARACTERS-GENERATED-WHEN-YOU-WRITE-THE-TASK -->
# Holdout task H<n>: <short neutral title>

This is the TEMPLATE for a sealed holdout task (Decider D32; the product owner's priority ruling 2026-10-26). One task is ONE file, UTF-8, LF line endings, no BOM,
named `H<n>-<kind>.md` (for example `H1-write.md`, `H2-modify.md`, `H3-write.md`). Written OUTSIDE the mob by someone who will not drive or review cards 086 to 088.
The mob never reads it before card 088 unseals it. The file is neutral: it names no language. The harness derives the Phrase prompt and the plain-C# prompt from it with the SAME wrapper,
so nothing here may favour either language.

## Header (exactly these keys, one per line)

- **id:** H<n>
- **kind:** write | modify            (write = build it from nothing; modify = change existing code; "read" and "fix" tasks are not used for the holdout)
- **fits_no_pattern:** true | false   (exactly ONE of the three holdouts is true: a task that fits no pattern in docs/pattern-candidates-raw.md or the seed catalogue)
- **why_it_fits_no_pattern:** <one sentence if true, otherwise: n/a>
- **libraries:** the libraries both arms may use (for example: ASP.NET Core minimal API or FastEndpoints; the C# arm and the Phrase-generated arm use the SAME ones)

## The task, in plain words (neutral)

What the program must do, for a person who has never seen Phrase. Inputs, outputs, failure cases, in prose. Keep it small enough for one model run (a few paragraphs).

## Observable outcomes (the cases both arms are judged by)

Every case is observed by a caller, a client, a store or an event capture, never "it succeeds". Use this JSON, one fenced block, an array of cases:

```json
[
  { "name": "accepts a valid name", "given": { "name": "Ada" }, "when": "called", "then": { "returns": "Ada" } },
  { "name": "rejects an empty name", "given": { "name": "" }, "when": "called", "then": { "fails_with": "MissingName" } }
]
```

`then` is exactly one of: `{ "returns": <json> }`, `{ "responds": <json record>, "status": <int> }`, `{ "fails_with": "<Code>" }`, `{ "stored": <json> }`, `{ "published": "<EventName>" }`.
At least one failure-path case for every failure code named in the prose.

## Starting code (modify tasks only; delete this section for write tasks)

Two equivalent starting points that behave the same, so the change is the same size in both arms:

### start.phrase
```phrase
<the existing Phrase source>
```

### start.cs
```csharp
<the equivalent existing C# source>
```

### The change requested
<one paragraph: what to change, in neutral words, plus the extra outcome cases above that the changed program must satisfy>

---

# Sealing rules (not part of the file; the file ends above this line)

1. The hash is SHA-256 of the WHOLE FILE, exact bytes as saved (including the first-line nonce comment), lower-case hex, 64 characters. Compute it AFTER the file is final and never edit the file afterwards:
   `python -c "import hashlib,sys;print(hashlib.sha256(open(sys.argv[1],'rb').read()).hexdigest())" H1-write.md`
2. Give the facilitator ONLY the three lines `H1 <kind> <sha256>`, `H2 <kind> <sha256>`, `H3 <kind> <sha256>` and the sentence confirming who wrote them and that they will not drive or review cards 086 to 088. NOT the files.
3. The facilitator records the hashes in `bench/holdout-hashes.txt` and in a Decider record BEFORE card 086 starts. After that a hash can only change by a new Decider that voids rows.
4. The sealed files stay with the author (outside this repository and off the mob's machine). No path to them is written anywhere in the repo. The repo is PUBLIC: never commit them before unsealing.
5. Unsealing happens when card 088 starts: the author hands over the three files; the facilitator verifies each SHA-256 equals the recorded hash (a mismatch voids the holdout), then the harness runs them under the frozen bar.
