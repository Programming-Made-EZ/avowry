# SCOPE PROPOSAL: window 276-scope-4

Intent: Finish D179's voting-seat metric so a Muse decision seat, already probed and cited, also scans clean. Source: board 276 Done check 3 "returns a cited vote and scans clean"; D179 "voting seats first"; D180 (the clean scan is not waived); gate HOLD 07:16 UTC.

Assumes: `_read_muse_envelope` on HEAD returns clean on vote log e338c0ee (commands 0, other_tools 3) and capture log a7505b9d (commands 3), and no-source for an unknown tool in a subagent log, a shell call with no command, and an orphan shell started row. Probe: cycle_4_green_4b scan_entry (vote clean, capture commands 3); cycle_4_red_4c (3 FAIL on 7c04e2e); command_source.py `_read_muse_envelope`. The re-gate runs its own scan_entry; the facilitator's re-scan is a pointer only.

## Choice: (A) ratify cycles 5 and 6

Cycle 4 of 4 closed at green 2e56d9f. Red 4b 25d50b8 / green 7c04e2e and red 4c 58b5c26 / green 0000715 are cycles 5 and 6 (cycle_count_ruling, audit 195), past scope-3's cap of 4 with no prior window (D175 C4). This window rules after the fact.

(A) raises C3 from 4 to 6, keeps HEAD, next step the re-gate. (B) reverts to 2e56d9f by revert commits (no history rewrite); Done check 3 stays partly met; the real-log reader moves to card 279.

Adopt (A). Cycle 4's red used trimmed rows, so 2e56d9f passed while both real logs stayed no-source (same class as red 3b). Red 4b committed the whole logs; both then scanned clean. Green 4b skipped unknown tools in subagent logs and orphan shell rows; red 4c applied the adopted allowlist on those paths. D180 holds the card open until the scan is clean; (B) leaves that unmet here and sends a finished reader to 279, whose three Done checks already hold on HEAD. The D175 metric still counts two reds past cap without a prior window.

Muse's sandbox ACL lock timeout is outside Done check 3: a voting seat only reads. Fixture-path fix 839380b is not a cycle (flat names, path-length guard, no behaviour change; precedent e3608ae). Suite 1633 OK on 2e56d9f; a suite is running on 839380b.

Raise C3 from Cap 4 to Cap 6. Ratify cycles 5 and 6 of 6. Caps 1+1+6 = 8. No further cycle on this card.

Why the total may exceed 3 (and the prior 6): sign-in and the live log shape arrived after the slice caps; 5 and 6 closed the trimmed-fixture gap inside scope-3. Cut: (B); a seventh cycle; sandbox completion; catalog-id; build seats.

## Cycles and hours used

C1: 1 of 1. C2: 1 of 1. C3: 6 spent against cap 4 (4: 5b3292e / 2e56d9f; 5: 25d50b8 / 7c04e2e; 6: 58b5c26 / 0000715 / fdfa821). Hours: 5ef4b8f 02:18 UTC to the ruling 17:22 UTC, about 15 h of committed work.

## Re-gate (next step)

Gate seat: `--add-dir` for mob-crosscli (gate_1 missed this). The seat itself calls `scan_entry` on fixtures e338c0ee and a7505b9d (tests/fixtures/muse_real), writing Environment, status, commands, other_tools, sha256; that run is the evidence. It runs gate_1's named modules plus the cycle-4, 4b, 4c and path-length tests, and attests the full suite on 839380b in PowerShell. It judges Done check 3 from the board wording and lists reds 25d50b8 and 58b5c26 with this window (after those reds: D175 metric hit).

## Smallest scope

No further red.
Acceptance: e338c0ee clean, commands 0, other_tools 3; a7505b9d clean, commands 3; unknown subagent tool, shell without a command, and orphan shell started row are no-source; missing, empty, unparsable, unresolved-root stay no-source.
Cap: criterion 3, at most 6 cycles (this window ratifies cycles 5 and 6 of 6; total 1+1+6 = 8).
Stop: no further red on card 276. Re-gate scan_entry on the vote log other than clean => HOLD, remainder to card 279. A new CLI or log shape, sandbox completion, and a further unknown-tool class go to a follow-up. Quota still excludes (card 277).

Ask: NO OBJECTION, or OBJECTION with one fixed condition.
Size: 3848 bytes (facilitator-measured; the seat estimated 3886)
