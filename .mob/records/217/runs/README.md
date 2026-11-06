# Card 217, criterion 3: run evidence

Each `*.outcomes.json` is `{"source": {...identity...}, "outcomes": {<fully qualified test>: <outcome>}}`, derived
with `scripts/trx-outcomes-217.py` (local runs, from a TRX file) or `scripts/ci-log-outcomes-217.py` (CI runs, from the
console log of the Test step). `scripts/check-run-pairs-217.py` compares the two lists of an environment against the
record `.mob/records/217/run-pairs.json` (written in cycle 3b). The TRX files and CI logs themselves are not
committed; the sha256 of each is in the `source` of its list, so the list can be checked against the file.

A long theory argument is cut by the xUnit adapter, so the 14 `EdgeCaseFixtureCorpus.Every_corpus_fixture_matches_its_expected_outcome`
cases share one display name. The extractor merges them into one entry and records the merge in
`source.collapsed_truncated_names` (14 results behind one name; `count` is the number of results, 316, the list has 303
names). If their outcomes differ the entry reads `Mixed(...)`, which the checker refuses.

## With the thread-pool floor (baseline, before commit A)

Head `3c5712e620c1c90dca53897ecf4ce9c8699f1b16` (no C# or project file changed between it and the head the lists were
derived at). Two consecutive full local runs of `tests/Phrase.Acceptance.Tests`, `TEMP` and `TMP` set to a scratch folder
(`C:/Users/ringe/AppData/Local/Temp/claude/full217a`), TRX in `C:/Users/ringe/AppData/Local/Temp/claude/full217a/trx`
(outside the repo; they stay there).

| file | TRX | sha256 of the TRX | finished (local time, UTC-8) | result |
| --- | --- | --- | --- | --- |
| `local-floor-1.outcomes.json` | `run1.trx` | `cd216b315b3f331c18d26a6363febff729c664c510f032d47363418045df0f79` | 2026-11-05 15:59 | 316 tests, 313 passed, 3 failed, 11 m 1 s |
| `local-floor-2.outcomes.json` | `run2.trx` | `2b30e3f5af7a25369af3d6ff1fe1a7112c1adbb6bd8c32a3d2a817bff51be9ab` | 2026-11-05 16:58 | 316 tests, 313 passed, 3 failed, 11 m 3 s |

The three failures in both runs are the F-065 tests: `UsableV1.Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone`
(owner card 162), `CSharpBaselineAcceptance.endpoint_reply_fails_when_response_record_or_status_are_wrong` and
`CSharpBaselineAcceptance.endpoint_reply_passes_when_response_record_and_status_match` (owner card 303). The two lists are
identical.
