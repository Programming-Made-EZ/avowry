Intent (D165): Make card 211 criterion 3 red under D177: a committed record test that fails until mob-defaults holds a final-state review by a fresh-context non-author seat, with full seat attribution, cited from the Decider MD-010 entry.
Assumes (D166): D177 makes context, not committing identity, the test, so "not an author" means the reviewer's seat_context is not in the record's own author_contexts (the test cannot check that list against git); probe: no records/211/final-review.json is tracked, and DECIDERS.md's MD-010 entry cites no review file.

Seat: QA engineer, RED 3, criterion 3 only, cap 1 cycle. Context ctx-3 (fresh, red).

Record path: records/211/final-review.json in mob-defaults, next to votes.json and rerun.json.
Fields: finding (MD-010), ruling (names D177), range (687410e..e3736d1), head_reviewed (a commit that contains e3736d1), model, author_models, same_model_reason (needed only if the reviewer model is an author model), seat_context, session_path, author_contexts, authored_none_of_the_27 (true) with an attestation, findings[] of {stance confirms|adds|disputes, text, defect 1 to 8 for confirms and disputes}, verdict (accepted | accepted-with-findings | rejected). DECIDERS.md's Decider MD-010 entry must name records/211/final-review.json.

Session path decision: the session file is outside the repo. The test always checks its form (non-blank, absolute) and, in the real-record test, that it exists on disk. Form alone accepts any invented string, so the attribution could not be audited; existence is the least that shows a real file. Cost: that test passes only on the machine holding the session file, and it proves the file is there, not what it says. The fixture tests inject the existence check, so they run anywhere.

Test: tests/test_card_211_final_review.py, committed fe4c092, pushed to origin main. No other unpushed commit in either repo. git status in mob-crosscli showed only untracked .mob/ (no modified source file) when I ran it.
Asserts: no record; each of the model, seat_context, session_path, head_reviewed, attestation, verdict, author_contexts, author_models, findings, ruling and finding fields missing fails; complete passes; confirm, add and dispute are all recordable; reviewer in author_contexts fails; a false authorship attestation fails; same model as the authors needs a reason; Decider MD-010 entry not citing the file (or citing it only under a later entry) fails; relative or absent session path fails; wrong range or unresolvable head fails; bad verdict or empty finding fails.
ZOMBIES: Z no review; O one field missing; M complete; B reviewer is an author, review uncited; E session path absent, wrong range.

Run on committed code (HEAD fe4c092, Git Bash on Windows, cwd C:\Users\ringe\mob-crosscli): python -m unittest tests.test_card_211_final_review. One module, not the full suite. Ran 11 tests, FAILED (failures=1): the real-record test, "no final-state review records/211/final-review.json". The 10 fixture tests pass.

Open for green: a fresh-context seat that wrote none of the 27 commits, on a model different from the authors where available, reviews the final state, writes records/211/final-review.json with the attribution fields, and adds the citation to the DECIDERS.md MD-010 entry. Scratch folder left in place: C:\Users\ringe\AppData\Local\Temp\claude\qa211-3-13478 (empty).
Size: 3373 bytes
