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

## Without the thread-pool floor (commit A, after cycle 3a removed `ThreadPoolFloor.cs`)

Head `d6e8f262f5f2a226d1569422e42d29f64c490efa` (commit A, clean tracked tree; unrelated untracked files only). Windows 11 Pro
10.0.26300, dotnet 10.0.403, `dotnet build tests/Phrase.Acceptance.Tests` once, then
`dotnet test tests/Phrase.Acceptance.Tests --no-build --results-directory <TEMP>/trx --logger "trx;LogFileName=nofloorN.trx"`
twice, consecutively, cwd `C:/Users/ringe/projects/phrase-lang`. `TEMP` and `TMP` both
`C:/Users/ringe/AppData/Local/Temp/claude/green217c`; the TRX files are in `<TEMP>/trx` (outside the repo; they stay there).

| file | TRX | sha256 of the TRX | run (local time, UTC-8) | result |
| --- | --- | --- | --- | --- |
| `local-nofloor-1.outcomes.json` | `nofloor1.trx` | `6d39cfc1c1735c41a0cdd0f25aa997a6740d0b80cdf3feab92322df30304c3d9` | 2026-11-05 17:10:13 to 17:21:20 | 316 tests, 311 passed, 5 failed, 11 m 6 s |
| `local-nofloor-2.outcomes.json` | `nofloor2.trx` | `f2d5c85211263874eb7975c6bd9b4449b81f98870b93903c823359357af846c8` | 2026-11-05 17:21:21 to 17:32:10 | 316 tests, 313 passed, 3 failed, 10 m 48 s |

Run 1 failed the three F-065 tests and also `GeneratedApiBoundAddress.I_readiness_comes_from_the_childs_own_output_before_any_connection_and_not_before_the_gate`
(20 s) and `GeneratedApiHazards.Guard_two_parallel_starts_get_distinct_base_urls` (32 s). Run 2 failed only the three F-065 tests.
The two no-floor lists therefore differ (the two extra tests); nothing was fixed or retried.

## With the thread-pool floor restored (commit C, cycle 3b)

Head `2bb2f306c686692097fe33a41a867f244ea4a24f` (commit C restores `ThreadPoolFloor.cs` byte-identical to 3c5712e; clean tracked tree).
Local: `dotnet build` once, then two consecutive full runs as above, `TEMP` and `TMP` both `C:/Users/ringe/AppData/Local/Temp/claude/green217d`.

| file | source | sha256 of the TRX / log | result |
| --- | --- | --- | --- |
| `local-floorC-1.outcomes.json` | `floorC1.trx` | `bd2c676b36315b2d06d8435594549d2622b20988e0263bf17b8d38809e5e16bf` | 316 tests, 313 passed, 3 failed (the F-065 tests), 11 m 12 s |
| `local-floorC-2.outcomes.json` | `floorC2.trx` | `e4ab5a2d812ca0ecce126fd9ba013ee03d1a3c14dbcd61c12952799f164dec67` | 316 tests, 313 passed, 3 failed (the F-065 tests), 11 m 9 s |
| `ci-floorC-1.outcomes.json` | CI run 42146757413, attempt 1, job test 127659984503 | in the file | 315 of 315 passed |
| `ci-floorC-2.outcomes.json` | CI run 42146757413, attempt 2, job test 127663195417 | in the file | 315 of 315 passed |

CI no-floor evidence on commit A (`d6e8f262f5f2a226d1569422e42d29f64c490efa`), run 42142783699: `ci-nofloor-1.outcomes.json` (attempt 1, job 127646923033)
and `ci-nofloor-2.outcomes.json` (attempt 2, job 127652566364) both 315 of 315 passed; CI did not show the starvation that local no-floor run 1 showed.
The decision is in `run-pairs.json`: floor kept.
