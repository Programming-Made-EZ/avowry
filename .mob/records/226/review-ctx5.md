# Card 226 review (ctx-5, independent reviewer, audit lens, runner lens)

Seat: Claude (the 04:46 UTC probes in .mob/records/probes/2026-10-30.jsonl show Codex, Copilot, Grok and agy without usable quota). Reviewed mob-defaults 9cda13d..165fde8 (18 "Card 226" commits, pushed). I wrote none of it. Scratch copies only (git archive of 165fde8 under the session scratchpad); the repository was not touched.

## Verdict: CHANGES REQUESTED

The three Done checks hold and fail on partial work (mutations below). The invariants hold. One real defect (a seat-use line is written for a voter that never starts) and one guard gap (the Claude voter's model is tied to the probe's model only by key order) need closing; one hygiene finding. All three are small and test-first.

## Findings

F1 (medium, rotation integrity). decider.py:146-158 (check_voters) and 161-166 (commit_voters), with decider.py:98-103 (vote_seat) and 243 (the seat list). A seat-use line is written for every name in --seats, but D164 C1 and the slice say one line per seat started. Three cases write a line for a voter that never launches: a disabled backend, an unknown name, and a duplicate name. Evidence (scratch test through tests.test_decider_gate.GateCase.drive, in process, backend and probe doubles):
- `seats=("codex","grok")` with grok `enabled: false`: exit 1, backend called for codex only, vote grok "seat unavailable" with attempts 0, yet seat-use.jsonl gets a grok line and the result records `line_index: 1` and `probe_ok: True` for it.
- `seats=("codex","nonesuch")`: exit 1, only codex launched, a "nonesuch" line is written.
- `seats=("codex","codex")`: exit 0, one backend call (the second clone fails on the existing directory), two codex lines, one vote record.
Effect: the history is what D164's LRU is computed from, so a seat that did not run now looks recently used (a disabled or misspelled seat skews the next run's rotation, and the next run's Claude voter may be refused or admitted on a line nobody ran). The slice's own sentence ("refused = no voter launched, no line written") is only half kept for these runs. No test covers them (the 140 tests pass on all three). Fix: in run(), before start_voters, take the seats in order without duplicates, and give lines only to seats whose backend is in backends.json and enabled (the others keep the vote "seat unavailable" and get no line, no record, and are excluded from `pending`); add three tests (disabled, unknown, duplicate: history unchanged for that seat, one line per launched voter). A mutation that writes the line for every name (as now) must fail them.

F2 (medium to low, guard gap; the card 208 F3 precedent, D164 C5). decider.py:115 launches every voter with the key "default" (`facilitator.run_backend(seat, seat_cfg, "default", ...)`), while the probe the record cites runs the backend's `probe_model` pin. claude's `models` has no "default" key, so a Claude voter runs the first entry; core/backends.json lists haiku first and pins haiku, so they agree today by key order only. Card 208's seat_turn already launches the pin (facilitator.py:1328). Mutation in a scratch copy, voter launches `"sonnet" if seat == "claude" else "default"` (the model a reorder of claude's models would give): the eight decider and seat-gate modules, 140 tests, OK (survived). Consequence for this card's purpose: the record would name probe_evidence for haiku against a voter that ran sonnet, and an auditor would trust the wrong probe. Fix: either `cfg.get("probe_model") or "default"` in vote_seat, or at least a test that the model key passed to run_backend for a Claude voter equals the backend's probe_model, with claude's models reordered in the test backends. Which Claude family seats use stays a product-owner question (D164 C5).

F3 (low, hygiene). decider.py:187-205 (voter_record, probe_record). probe_evidence is copied raw into decider-result.json and into the git-tracked Decider log ("Rotation record" and "Probe in force" lines); vote replies and reasons are scrubbed (mob_summary.scrub) but probe evidence is not. The evidence is the first 200 characters of the CLI's own output (facilitator.py probe_backend), which on a failed probe can carry a path or account text. Card 208 already puts the same evidence into MOB.md, so this is not new exposure, but this card adds a second place in git. Fix: `scrub(...)` the evidence when building the record (the ok flag and taken_at are what the recompute needs); one test with a home-directory path in the evidence.

F4 (info). decider.py:158 builds the pending line by hand (`{"seat": seat}`) instead of through seat_use_line. It is not a copy of a rule; it relies on the "seat" key that last_seating reads (mob_process.py:181). Mutation `pass` instead of the pending append fails 3 tests, so a rename would break loudly. Accepted.

F5 (info, cost). Every Decider run now probes every enabled backend (card 208's probe_drivers: one CLI call each, Claude included) before the first voter. That is the slice's choice (a fresh probe on every run, no new probe format), noted so the retro reads it as a cost, not an accident.

## The item left to me: the top-level probe object and the "Probe in force" log line

ACCEPT, no change to the field choice. Reasons: (1) Done check 3 says an auditor recomputes rotation from git; claude_wait_problem judges a Claude voter on the OTHER models' ok flags, and those models are usually not voters, so no per-voter field carries them; without the probe object the recompute needs the probe file, which is outside git. QA's reading is the minimum that closes the check. (2) It is narrow: taken_at plus {ok, evidence} per backend, nothing else (units pin it; a backend with no result is None, never False). (3) It is guarded: dropping it from decider-result.json fails 5 tests; dropping the log line fails 4. Conditions: scrub the evidence (F3), and name the "probe" field in the Done check 3 text when the card is closed, so it is not an undocumented field.

## Done checks and invariants

- Check 1 (fresh probe): start probe through CardRun.probe_drivers (decider.py:139); repeat at 60 minutes or more by the clock at each voter's turn; failure exits 2 with no voter and no result file. Mutations: `>=` to `>` in probe_is_stale: 5 failures; start-probe exception no longer turned into a refusal: 2 errors.
- Check 2 (rotation): non-Claude voters judged first, Claude last, same-run voters as pending; all checks before any line; append-only re-read after the run. Mutations: Claude judged first: 13 failures, 1 error; pending not appended: 2 failures, 1 error; verify_history removed: 1 failure (the rewrite test). A voter appending a line is caught too (read_seat_use compares the whole file with what the run last saw).
- Check 3 (record): model, probe_taken_at, probe_ok, probe_evidence, line_index, and rotation_reason for Claude only, in the result and in the log. Mutations: reason typed: 8 failures; line_index off by one: 10; "Rotation record" log line dropped: 7; "Probe in force" line dropped: 4; probe object dropped from the result: 5.
- Invariants: parse_vote, vote_seat, build_prompt, the sandboxed clones and snapshot/diff_snapshots are untouched by the diff; card-mode seats keep card 208's gate: check_seat then commit_seat is gate_seat, and tests.test_seat_gate, test_seat_rotation, test_seat_rotation_rules, test_quota_first, test_probe_model (91 tests) pass unchanged. No second copy: rotation_reason is imported, check_seat and commit_seat are called; adding a `def claude_wait_problem` to decider.py fails both source guards. No strictness beyond D163: a voter on a failed probe is recorded, not refused; Claude is refused only for an older available model outside the run or an unread result (D164 C1, C2).
- Line endings: decider.py and facilitator.py are CRLF in base and now (every line); the six new or changed test files are LF; no stray CR in LF files.

## D157, D136, bodies

- D157 order: red 8357f14 -> units 372d4b0, ddbf6ff -> green 2189ca2 (Criterion 1); structural d3826ea (no Criterion line, behaviour-preserving, as the slice planned); red a54c9d9 -> units df9d9d7 to 10a6f9d -> green ed5b0e1 (Criterion 2); red 9c8bb71 -> units 2ebedd0, c466110, e6d24bb -> green 165fde8 (Criterion 3). Every commit has its "Criterion: n" (except the structural one); no red for n+1 before the green for n; no trailers (0 of 18); the Intent line 52d4676 is cited in the Criterion 2 and 3 commits.
- D136: units and greens are small. The reds are large single-concern files (166, 207 and 333 lines), the same shape card 208's review accepted. Deviation: green 165fde8 carries a test-file fix (the guard regex) beside the production change, two concerns in one commit; disclosed in the body. Low.
- Recorded deviations, weighed: (a) red a54c9d9 "stayed green" overstates what was measured (three modules); not amended, covered by the green's single full run (497 OK); this is the claim-without-command pattern stand-down 2 targets. Low. (b) e6d24bb says "Ran 21" where the run was 18, and its copied-rule guard held a 0x08 byte so it could not fail. The count is low. The broken guard is medium as process: a layer commit shipped a guard that could not fail while its body presented it as a mutation guard; the green body discloses both and fixes the regex. I re-ran the fixed guard: a copy of claude_wait_problem in decider.py now fails two guards (tests.test_decider_record_units.VoterRecord and tests.test_decider_gate.Guards). Accepted as recorded; no amend.
- Commit bodies' claims: the 165fde8 body gives command, count and the recompute output for each claim (23 and 18 tests, 160 total, 538 full run, mutation list). I rely on the recorded full run and did not re-run it.

## What I ran

- `python -m unittest tests.test_decider_probe tests.test_decider_gate tests.test_decider_rotation tests.test_decider_record tests.test_decider_record_units tests.test_decider_runner` (scratch copy of 165fde8): Ran 109, OK, 75 s.
- `python -m unittest tests.test_seat_gate tests.test_seat_rotation tests.test_seat_rotation_rules tests.test_quota_first tests.test_probe_model`: Ran 91, OK.
- Mutations (one change each, in separate scratch copies, the eight decider and seat-gate modules, 140 tests): listed above. The only survivor is the voter-model mutation (F2). The full suite (538, about 22 minutes) was not re-run.

## To approve

Close F1 (code and three tests), F2 (test, or the one-line fix) and F3 (scrub and a test). Then re-run the decider and seat modules; I will re-read the diff. F4 and F5 need nothing.

## Re-check

Seat: Claude (ctx-5, fresh context; the 06:10 UTC probes show Codex, Copilot, Grok and agy without usable quota). Reviewed mob-defaults 165fde8..43912b7 (5 commits, pushed; origin/main equals 43912b7). Scratch copies only (git archive of 43912b7 and of 165fde8 under the session scratchpad); the repository was not touched.

### Verdict: CHANGES REQUESTED (one small item: the guard for M4 is not deterministic)

F1, F2 and F3 are closed in the production code and by guards that fail. One new finding (R1) concerns the guard c65ec21 added for the surviving mutation M4: it can pass with the mutant present, so M4 is not reliably killed. Everything else holds.

### R1 (low to medium, guard that cannot be relied on to fail)

tests/test_decider_launched.py `test_a_repeated_name_starts_one_voter_exits_0_and_keeps_that_seats_vote` (c65ec21) depends on a race. With the dedupe in run() (`seats = list(dict.fromkeys(seats))`) deleted, `futures` is a dict comprehension over a list holding "codex" twice, so two `vote_seat` calls run in the pool against the same clone directory; whether the second clone fails ("File exists") and overwrites the vote with "none" depends on thread timing.
- Evidence: M4 mutant in a scratch copy of 43912b7 (decider.py CRLF kept, the one line removed). That test alone, 8 runs: 6 FAILED, 2 OK (the mutant survived twice). The four-module-plus-runner set (154 tests: launched, pin_scrub, gate, record, record_units, probe, rotation, runner): Ran 154, OK (survived) once. The commit body measured one run and reported a kill; with a race, one kill proves nothing.
- Why it matters: Criterion 2's repeated-name rule is the part of F1 that has no deterministic guard; the other tests on repeats (`launched`, history, vote names) also read a timing-dependent outcome once the mutant is in.
- Fix (test only, born green): make the second start observable without timing, e.g. wrap or replace `decider.vote_seat` with a recording double and assert it is called once for `["codex","codex"]` (or assert the backend mock and the clone directory are touched once), then show the mutant red on repeated runs (say 10) and give the count. No production change.

### F1 closed (Criterion 2: red 4810ed8, green fec8489)

- decider.py check_voters now keeps seats in order, without duplicates, whose backend is in backends.json and enabled; the rest keep the vote "seat unavailable" with no line, no record and no pending entry. Five inserted lines, CRLF kept, no second copy of card 208's rule (check_seat and commit_seat remain the judge and writer).
- Red against the old code: 4810ed8's tests with the 165fde8 decider (scratch copy of 165fde8, tests of 43912b7): tests.test_decider_launched Ran 28, FAILED (failures=26) (25 reds plus the M4 guard, which also fails without any dedupe). Green: the same module at 43912b7, Ran 28, OK.
- Mutations of the green, one change each, 154 tests (same eight modules), scratch copy: enabled check dropped: FAILED (failures=18); duplicate check in check_voters dropped: FAILED (failures=7); unknown name treated as enabled: FAILED (failures=11). All killed. The run() dedupe (M4) is the R1 case.
- No new defect found: the filter runs before the Claude ordering, so a disabled or unknown sibling is neither judged nor a pending line (tests cover a disabled codex not making Claude wait); a refusal still leaves the history as found; the vote of a filtered name stays "seat unavailable" from vote_seat; the voter record updates only for names with a record.

### F2 closed (Criterion 3: red 3a0819f, green 43912b7)

- vote_seat launches `cfg.get("probe_model") or "default"`, the key card 208's seat_turn uses; a backend with no pin keeps "default". Mutations: key always "default": FAILED (failures=5); launch key without the fallback: FAILED (failures=2). Both killed.
- Red against the old code: tests.test_decider_pin_scrub on the 165fde8 decider: Ran 17, FAILED (failures=14), as the red body states; at 43912b7: Ran 17, OK.

### F3 closed (Criterion 3)

- `_scrubbed` reuses mob_summary.scrub (imported already), keeps None as None; applied to the voter record's probe_evidence, to each backend's evidence in the probe object, and (beyond my review, accepted) to a Claude voter's rotation_reason, which quotes other backends' evidence. The Decider log lines render those records, so they are scrubbed with them. Mutations: scrub identity: FAILED (failures=9); rotation_reason unscrubbed: FAILED (failures=4); probe_record unscrubbed: FAILED (failures=5); voter_record unscrubbed: FAILED (failures=5); scrubbing None to "": FAILED (failures=3). All killed.
- The probe-object condition set in my first review (scrub, F3) is met. The condition to name the "probe" field in the Done check 3 text at card close stays open for the closer.

### Hygiene

- D157 order: red 4810ed8 -> green fec8489 -> guard c65ec21 (Criterion 2) -> red 3a0819f -> green 43912b7 (Criterion 3). Each commit has its Criterion line and the Intent 52d4676; no Criterion 3 red before the Criterion 2 green. The guard c65ec21 sits between green and the next red as a born-green step, which is the recorded way.
- D136: production changes are 5 and 9+4 lines; the two new test files (284 and 202 lines) are single-concern reds, as accepted before.
- Line endings: decider.py CRLF in index and worktree, the two test files LF (git ls-files --eol); the cached stats equal the ignore-CR stats as the bodies say. Trailers: 0 of 5 commits carry Co-Authored-By or Signed-off-by.
- Bodies' claims carry command and count: red and green bodies give the module command, counts and mutation lists; the 583-run and the 565-run full suites are recorded and I did not repeat them. I found no claim without a command. The c65ec21 body's "M4 killed" is the claim R1 qualifies (one run).
- Invariants: parse_vote, build_prompt, sandbox clones and snapshot code untouched by the diff; no new function beyond `_scrubbed`.

### What I ran

- `python -m unittest` over tests.test_decider_launched, test_decider_pin_scrub, test_decider_probe, test_decider_gate, test_decider_rotation, test_decider_record, test_decider_record_units, test_decider_runner, test_seat_gate, test_seat_rotation, test_seat_rotation_rules, test_quota_first, test_probe_model on 43912b7: Ran 245, OK, 121 s. The full suite was not run.
- Eleven one-change mutations (list above) over the eight decider modules (154 tests each) in a scratch copy; the repeated runs of the M4 guard in a second copy.

### To approve

Make the M4 guard deterministic (R1) and show the mutant red repeatedly. I will then re-read that one test; nothing else is open.

## Second re-check

Seat: Claude (ctx-5, fresh context; the 06:49 UTC probes show Codex, Copilot, Grok and agy without usable quota). Reviewed mob-defaults 43912b7..56b39bc (1 commit, pushed; origin/main equals 56b39bc). Scratch copies only (git archive of 56b39bc, one good and one mutant, under the session scratchpad); neither repository was touched by tests.

### Verdict: APPROVE

R1 is closed: the M4 guard no longer depends on timing and fails on every run with the dedupe removed.

### What changed since 43912b7

- `git diff --name-only 43912b7 56b39bc`: one file, tests/test_decider_launched.py (14 insertions, 4 deletions). No production file, no other test, changed.

### R1 closed

- The guard replaces `decider.vote_seat` with a double that counts calls per seat name under a lock and returns a fixed Yes without a clone, then asserts `calls == {"codex": 1}` plus exit 0 and the kept vote. The count is 2 whenever run() submits the repeated name twice, whatever the thread timing.
- M4 (line `seats = list(dict.fromkeys(seats))` deleted from decider.py in a scratch copy of 56b39bc): that test alone, 12 runs: 12 FAILED, 0 OK. Good tree, same test, 12 runs: 12 OK, 0 FAILED.
- Inside the decider modules (launched, pin_scrub, probe, gate, rotation, record, record_units, runner; 154 tests), M4 mutant, 3 runs: each Ran 154, FAILED (failures=1) (the one guard). Good tree: Ran 154, OK.
- The double does not hide a real defect: it replaces only vote_seat, whose return keys (vote, reason, reply, attempts, wrote_files, model) it mirrors; the vote_seat tests themselves (launch key, pin, scrub, unavailable) are untouched and still run against the real function in the other tests; the check that matters here, how many times run() starts a seat, is the part run() owns. The removed `self.launched == ["codex"]` assertion is replaced by the stronger call count.

### What I ran

- `python -m unittest` over the eight decider modules on the good and the mutant tree (counts above), and the guard alone 12 times on each. The full suite was not run.
