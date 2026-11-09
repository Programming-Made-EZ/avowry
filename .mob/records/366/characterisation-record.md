# Card 366 characterisation (D204), QA (red) seat

Declared CHARACTERISATION, not a red. These must pass on the pre-change tree: no production file changes in this worktree (NOTICE, TRADEMARKS.md, docs/LICENSING.md and LICENSE are untouched). Base: `4476596` (main, the architect's slice commit). Branch `wt-366-red`.

## Declared up front

| Id | Test | Why it passes today and after the green |
|---|---|---|
| C1 | `LicenseFilesTests.test_license_is_the_standard_apache_2_0_text_byte_for_byte` | LICENSE is hash-pinned and the green does not touch it (its `[yyyy]` boilerplate is not a placeholder to fill). |
| C2 | the four standing-terms tests: `test_reader_facing_legal_files_use_the_avowry_name`, `test_the_readme_and_the_licensing_page_say_generated_code_belongs_to_the_user`, `test_the_trademark_policy_covers_redistribution_references_forks_and_the_rationale`, `test_the_licensing_page_states_the_apache_licence_and_links_the_trademark_policy` | None reads the holder line; the green changes only the holder text, the year and the status sentence. |
| N3 | exactly one `Copyright ` line in NOTICE (a subTest inside `test_notice_and_trademarks_name_the_holder_and_the_year`, carried over from the deleted placeholder test) | The green replaces line 2 and adds no second copyright line. It is a boundary check, not a red. |
| X1, X2 | the scanner's negative rows (a markdown link, the filled NOTICE line) | They pass against the stub and against the real scanner; they guard against an over-eager scanner. |

The wrapper `LicenseFiles.The_license_and_standing_terms_are_unchanged` (`Slice=366`, no `Criterion`, not Pending) runs C1 and C2, so the gate now runs those Python tests (nothing ran `test_license_files.py` before).

Results and commands are in `red-record.md`.
