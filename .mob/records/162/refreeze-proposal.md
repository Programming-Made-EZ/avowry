# Proposal for Decider D238: the card 162 re-freeze (D72, D127) of the documents the LLM-customer check reads

Facilitator, 2026-11-09 UTC, main at 8c3aaee (green 1 1bcae82, cold reader and two clarifications 85e5b2c). VOTE on this record: adopt the re-freeze below as the one re-freeze of D127, so that Grok's and Codex's check runs are made against exactly these bytes, and the freeze window (nothing edits these files until green 2) starts when it lands.

STANDING-RULES PREFLIGHT: D72 and D127 (one re-freeze in one Decider, with old and new hashes and a token report), D131 (the bar 6 suspension stays until both runs), D237 (card 162 declaration and the freeze window of its ruling 3), D126 (the `to` case), TDD red-first (the red is on main: tests/test_pattern_docs.py, the Bar6 rule with planted rows, the closure rows; R1 is un-pended by this record's commit), D234 and D236 (evidence and live probe are those of D237; no seat plan changes here). Decider protocol step 6 (D224) applies; this proposal changes no agreement text.

THE FACTS (all recomputed by command on main at 8c3aaee, raw bytes):

| File | Old sha256 (prefix) | New sha256 |
|---|---|---|
| docs/GRAMMAR.md | ad0f6e51 (the pin in both llm-check JSONs) | 92029d22b0aaf52d266ef2710238a75cb5908afeacf1146e44343a808dc9d222 |
| docs/CHEATSHEET.md | fe3d8a8b (the pin) | 3611d24bac1de57a2b0de0225a6125916c9a610a899697cb37660b65107cc189 |
| bench/usable-v1-llm-check/tasks/U1-add-endpoint/prompt.md | 0a577149 | 61c0e7b02ad5caffa1a319dceb268c56b5f6528974dfb1b533ae9869daed4019 |
| .../U1-add-endpoint/outcomes.json | unchanged | 57d330cc5fad87974aa23dab133fa86bfe0b5422644a36b7c37e09aabe5380d1 |
| bench/usable-v1-llm-check/suite.json | unchanged | 5ad91957db8fc1703287d4bd98f3fad91af35e4122e718cc1b9ec82eb3fe7962 |
| demo/tasks/Tasks.avowry | unchanged | eee88242de7ffba4b5860420231b32545c7f229c61cd8600bcab3cf81f200306 |
| tests/Avowry.Acceptance.Tests/Fixtures/orders.avowry | 194003df (frozen by D72 as orders.phrase; changed by the rename of card 199 and by card 196) | 4d7a24b9877a79b43b308d5052cb3a807af0dd88465399b9fc639958a4f637f8 |
| tools/Avowry.Bench/Program.cs (informational) | n/a | 0e9e91c2a7f2a6c5c579ede6d87161f9160d59be55723769842e7fae42b20f8c |
| scripts/run-llm-check.py (informational) | n/a | 88e17459c8c2d226821948745baf43d722c2510cb48ff390461d0b4c0d31e465 |

Token report (estimate ceil(chars/4), the rule of PhraseCheatSheet.cs): GRAMMAR 4736 before, 5469 after (21874 chars); cheat sheet 518 before, 693 after (2771 chars; cap 700); documentation block (grammar, blank line, sheet, blank line) 5254 before, about 6163 after. The extra grammar tokens come from the two new sections (ensure step, record update step), the EBNF rules and the table rows; the PATTERNS page shrank by 1465 bytes because its prose now links to them.

Cold reader (Muse, verdict CLEAR; .mob/records/162/cold-reader.md): from the two pages alone it wrote an ensure avow and a record-update avow and named the rules it relied on; two small clarifications were applied to GRAMMAR.md (85e5b2c).

Baseline answer (D72): earlier history rows labelled with the document hashes 6b9d6c04 and 233c3fc6 are not compared with rows measured against the new block; the scored arm's frozen set moves to the new GRAMMAR and cheat sheet plus the current orders file. The two existing llm-check JSONs are stale evidence of the old pages and are replaced by the Grok and Codex reruns.

D126 `to` case: not forced by the fixed task; it is met by GRAMMAR line 35, the cheat sheet's "never part of the name" and the parser tests.

THE FREEZE WINDOW (from this record's commit to green 2): nothing edits GRAMMAR.md, CHEATSHEET.md, the check prompt, outcomes.json, suite.json, Tasks.avowry, orders.avowry, tools/Avowry.Bench/Program.cs or scripts/run-llm-check.py. R1 (un-pended by the record commit) enforces the seven bound content files in the gate; the launcher llm_check_when_back.py (guard, exit 76) enforces them and the two informational files at run time. A card that needs a GRAMMAR row waits (card 166 among them). Grok's run follows this record immediately; Codex's at its reset by the scheduled launcher; green 2 only after both.

DECISION ASKED: adopt this re-freeze as the one re-freeze of D127.
