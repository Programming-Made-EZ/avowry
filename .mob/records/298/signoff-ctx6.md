# Sign-off: card 298 (fresh Claude ctx-6, read-only; summary committed by the facilitator)

Result: SIGNED OFF with conditions. All three Done checks pass on the seat's own rerun of `python -m unittest tests.test_decider_runner.LiveVoteOverlap` (Ran 7, OK, 19.9 s); the equal full head_start and head_end assertion is really in the cycle 1 test; cycle 2 rests on the mutation evidence (record-only). No direct reader of mob-decider.lock exists in either repository (git grep, rerun).

What the gate missed (the seat's rerun probes against mob-crosscli 1590679), filed as bug cards:
- Card 313: a run that starts as another ends can be left without a lock: after its mkdir, the other run's release rmdirs the empty directory, the temporary-file open fails, the OSError branch returns a no-op, and decider_lock_live is false while the run votes (simulated).
- Card 314: a crashed run's .tmp file in the holders directory is skipped by the sweep forever, so the directory never goes away.
- Card 315: a run that cannot create the holders directory starts unprotected with no warning (card 230 did the same; not a regression).
- Recorded, no card: a reused pid keeps a dead holder looking live (fail-safe, as in card 230); a Windows file held open can make unlink or rmdir fail (swallowed, low risk).

Condition: the three bugs are filed (done, 313 to 315) before the card closes.
