# Card 435 third review (on the rework e374ab7, branch card-435-rework; told to try to break it)

VERDICT: FLAG. Seat: qa (agy, gemini-4-argon-high, pinned with --model), 21:24:59Z to 21:55:44Z, exit 0. Raw: `third-review-raw.md`. The facilitator's replay: `third-review-verify.md`.

Every BLOCKING input of the second review is fixed on pass 1 and pass 2, and all 27 mutants of the re-slice are killed. But the rework's new pieces bring new BLOCKING defects:

| # | Finding | Replayed by the facilitator | Class |
|---|---|---|---|
| 1 | Part D refuses `<` after `\\`: `\\<host>\Users\<user>` and `\\<host>\Users\<user>\x.cs` are left whole (the pre-card copy scrubbed them) | yes, new with this round | BLOCKING |
| 2 | Part D's prefix folders are `Plain+`: `\\host(1)\Users\<user>` and `\\host's\Users\<user>` leak the name (pre-card: `<path>`); the escaped-letter host rows leak before too | yes for the parentheses and apostrophe rows (new); the escaped-letter rows are older | BLOCKING (new rows) |
| 3 | `Glued` after `\uXXXX` without the `esc` capture: `\u0027-IC:\a\x.cs\u0027/home/<user>` leaks; `gcc \u0027-IC:\Users\<user>\include\u0027` loses its closing quote; `see a\u00f6-b/tmp/x.cs` is cut | yes, new with this round | BLOCKING |
| 4 | Idempotence: `C:\Users\<user>D:\Users\<user>\x.cs` now changes on pass 2 (it leaked before the card too); the `foo-~\nx.cs` row is refuted (same as before) | partly | BLOCKING (the pass-2 change) |
| 5 | The new tail drops a `(1)` before `-D:` (no leak) | yes | per the reviewer BLOCKING (a regression on a correct line) |
| 6 | A new quadratic shape: 20,000 characters of `\\C:/a\n` take 2,001 ms (5 ms before the card); 10,000 take 639 ms | yes, new with this round | BLOCKING |
| 7 | 8 mutants of the rework's changed lines survive (killing rows in the raw report) | not replayed | BLOCKING |
| 8 | The extended sweep's 160 new-against-before leaks and 586 second-pass changes: the older bridging and bare-`\\` classes | reviewer's judgement | MINOR |

## The facilitator's reading

Two rounds in a row, fixing the BLOCKING list of one review produced a comparable number of new BLOCKING defects in the next (round 2: 11; round 3: 7, all from the rework's own pieces). The single-regex approach is not converging under the current BLOCKING definition. Measured against main (which carries the first green 547538f): the rework is better on every comparison made (extended sweep leaks 7,807 against 61,514; the worst blow-up 2 s against 7 s; every round-2 BLOCKING input fixed). How to proceed is the team's call: Decider D259 (proposal `decider-259-proposal.md`).
