Card 230 — criterion 3, cycle 2 scope (caps 2+1+2)

Premises on `bf3e4ab80ec53d4a9290a785d8a86e6d2dd0787b`:

- Command:
  `findstr /n ".*" tests\test_decider_counted_result.py`
  
  Quoted output:
  `340:RESULT_KEYS = {"result", "prompt_sha", "probe", "votes", "counted_seats", "violations", "integrity"}`
  
  Thus card 290’s exact shape pin omits only the newly adopted `head_start` and `head_end`.

- Command:
  `findstr /n ".*" mob_summary.py`
  
  Quoted output:
  `24: re.compile(r"\bgh[pousr]_[A-Za-z0-9]{20,}\b"),`
  `26: re.compile(r"\bsk-[A-Za-z0-9_\-]{16,}\b"),`
  `27: re.compile(r"\bAKIA[0-9A-Z]{16}\b"),`
  `28: re.compile(r"\bxox[abprs]-[A-Za-z0-9\-]{10,}\b"),`
  `29: re.compile(r"\bAIza[0-9A-Za-z_\-]{30,}\b"),`
  `32: re.compile(r"\b[A-Fa-f0-9]{40,}\b"),`
  
  No enumerated real credential form is a bare 40-character lowercase hexadecimal string; only the generic catch-all has that shape, so it also matches Git object IDs.

- In `CycleTwoInterfaces`, `strings_in()` recursively yields every string key and value. The test removes only `prompt_sha`, then runs `mob_summary.contains_secret()` and the home-path regex over every remaining yielded string; it separately checks the raw JSON for the actual home directory and fixture leaks.

- Command:
  `git cat-file -t bf3e4ab80ec53d4a9290a785d8a86e6d2dd0787b`
  
  Quoted output:
  `commit`

- The supplied full-suite result is:
  `Ran 1836 tests`
  `FAILED (failures=2)`
  
  The two failures are the exact-shape pin reporting `head_start`/`head_end` as extra and the generic 40-hex detector reporting the stored Git SHA as a secret. A direct rerun in this read-only seat could not create `TemporaryDirectory`, so no contrary runtime result was produced.

Cause: criterion 3 deliberately added full commit IDs, but two older contract tests still interpret the new fields as schema drift and generic secret-shaped text.

Fix/window:

1. Red may touch only:
   - `tests/test_decider_counted_result.py`
   - `tests/test_launch_failure_honesty.py`

   Update the exact result-key pin by adding exactly `head_start` and `head_end`.

   Make the privacy test traverse named fields, exempting only `head_start` and `head_end` when the value matches exactly `^[0-9a-f]{40}$` and `git cat-file -t <value>` in the test repository succeeds with exactly `commit`. Keep `prompt_sha`’s existing exact 64-hex assertion. Every non-exempt key/value still passes through the unchanged secret and home-path checks; the identical 40-hex string stored under any other field must still fail.

2. Green may touch only:
   - `decider.py`
   - `facilitator.py`

   No production edit is expected if the adopted output already satisfies the corrected tests. If the red exposes a contract defect, make only the smallest change needed to emit the two full, repository-resolvable commit IDs.

ZOMBIES: Z equal start/end on a stable vote; O only one approved head field; M changed start/end commits; B 39/41 hex, uppercase hex, missing object, or non-commit object; I the same valid SHA under another field still trips secret detection; E real token and home-path fixtures remain rejected everywhere.

Stop rule: stop at the first full-suite pass with exactly the adopted two fields and unchanged privacy coverage. Stop and return to scope if passing requires another schema change, an exemption by value alone, skipping unresolved/non-commit SHAs, weakening `contains_secret` globally, touching files outside the stated red/green sets, or exceeding caps 2+1+2.

## Round 2: condition from round 1's audit lens (met, stated as fact)
The reviewed commit is reachable: green 3, bf3e4ab80ec53d4a9290a785d8a86e6d2dd0787b, is on mob-crosscli's origin main (https://github.com/Programming-Made-EZ/mob-defaults.git; git ls-remote origin refs/heads/main returns it, and git branch -r --contains bf3e4ab lists origin/main). Every premise above can be rerun from a clean clone at that commit. The red and green of this cycle are pushed the same way, each before its record. Caps 2+1+2; files and stop rule unchanged.
