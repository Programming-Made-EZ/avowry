Intent (D165): Make card 211 criterion 1 red: a committed record test that fails until mob-defaults holds a Decider record and a votes file ruling on 687410e..e3736d1 with one valid, attributable, D171-cited vote per declared seat.
Assumes (D166): a D171-valid CITE is a line of the proposal kept in the votes file (mob-defaults has no cite check of its own, so the test defines the minimum); the declared seats are the six adopted in 211-taskforce.json (ctx-1 to ctx-6), copied into the test as a fixture; probe: decider.py has --log (appends a "#### Decider <ref>: <title>" entry to a DECIDERS.md) and --out (writes only decider-result.json, no seat context or CITE), and no DECIDERS.md or votes file is tracked.

Seat: QA engineer, RED 1, criterion 1 only, cap 1 cycle (D175).

Chosen paths in mob-defaults, and why:
- DECIDERS.md at the root: the file name decider.py --log already documents, so the Decider record can be produced by the tool.
- records/211/votes.json: decider-result.json cannot hold seat context or CITE, so the votes file is its own, keyed by card.

Test: tests/test_card_211_decider_record.py, committed 6fccd4a, pushed to origin main (no other unpushed commit of another seat; git status clean except the untracked .mob/).
Asserts: the record has a "#### Decider MD-010" entry, the range, and all 27 commits; the votes file has the range, the proposal text, and exactly one vote per declared seat with a valid vote, a reason, a context equal to the seat's ctx (not missing, wrong or shared) and a CITE found in the proposal.
ZOMBIES: Z no files; O one seat; M all seats pass; B each single missing seat fails; E empty reason, empty CITE, unread CITE, missing or borrowed context, duplicate seat; I a missing commit name fails.

Run on committed code (git status showed only untracked .mob/), from C:\Users\ringe\mob-crosscli, Git Bash, HEAD 6fccd4a: python -m unittest tests.test_card_211_decider_record. Modules run: 1 (not the full suite). Ran 11 tests, FAILED (failures=1): the real-record test, "no Decider record DECIDERS.md; no votes file records/211/votes.json". The 10 fixture tests pass.

Open for green: the facilitator, not the QA seat, writes DECIDERS.md and the votes file. The range is counted as 687410e^..e3736d1 (27 commits).
Size: 2269 bytes
