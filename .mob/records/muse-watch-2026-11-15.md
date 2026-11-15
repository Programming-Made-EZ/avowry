# Muse: probe record and re-probe plan (the owner's question of 2026-11-15)

Muse is enabled in mob-defaults' `core/backends.json`. Its invocation is the medium-effort call: `muse exec ... --reasoning-effort medium --max-model-steps 60 --max-tool-output-bytes 20000 --disable-sandbox`. `probe_all.py` uses that invocation. Avowry seated Muse on 90 seat-use rows, the last on 2026-11-10.

Probes on 2026-11-15, each at a kickoff, all with the medium-effort invocation:

| Taken (UTC) | Kickoff | State | Evidence |
|---|---|---|---|
| 15:24:48 | card 430 | quota-reached | API error 429: "Subscription quota exhausted"; the usage window resets 2026-11-16 00:00 UTC |
| 20:08:53 | card 422 | quota-reached | the same 429, reset 2026-11-16 00:00 UTC |
| 23:21:26 | mob-defaults #61 | quota-reached | the same 429, reset 2026-11-16 00:00 UTC |

Muse was not skipped. Each declaration (D266, D268, D269) lists it as out until 00:00 UTC, citing its probe.

Re-probe: scheduled for 2026-11-16 00:05 UTC, after the reset. The result is written to `.mob/records/probes/2026-11-16.jsonl` and appended below.
- If Muse answers, it is a candidate seat for the next declared roles, first to relieve agy as a voter and retro voice. It is seated only through a declaration with its model and reason, and its outcome is logged per seat.
- If it fails, the error is recorded here and the probe repeats at every kickoff and at its next stated reset.
