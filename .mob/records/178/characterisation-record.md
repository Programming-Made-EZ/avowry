# Card 178 characterisation 3 (D157 Criterion 3), QA (red) seat

Declared CHARACTERISATION, not a red. The tests pass on the pre-change tree (HEAD b9e642348fdac7f80aac4fc33bdc2f74dcddaff2, no card 178 page yet). Reason: criterion 3 says README.md and demo/tasks/README.md stay byte-identical to the pre-change commit; that is already true, and the tests pin it before the green so a later edit of either README at the page-adding commit fails.

BASE = `b9e642348fdac7f80aac4fc33bdc2f74dcddaff2` (`git rev-parse HEAD` at this commit). The same hash is the constant in `tests/test_readmes_untouched_178.py`.

## What was written

`tests/test_readmes_untouched_178.py` (module `tests.test_readmes_untouched_178`, no C# wrapper). Class `ReadmesUntouched178`, 4 tests:

1. BASE resolves and is an ancestor of HEAD.
2. the two README blobs at BASE equal the blobs at the comparison commit (`git show REV:path`; empty blobs fail). Comparison commit: `git log --diff-filter=A --format=%H -1 -- docs/PREREQUISITES.md`; HEAD if none.
3. look-alike-commit and uncommitted-edit reds stay green: a `git clone --shared --no-checkout` scratch clone adds the page, then a later commit with subject `Card 178 green 1 (scratch)` edits README.md, and demo/tasks/README.md gets an uncommitted append; the helper still reports equal.
4. a scratch commit that adds the page and edits a README reports a difference.

## Run

`python -m unittest tests.test_readmes_untouched_178 -v`

```
test_adding_the_page_and_editing_a_readme_reports_a_difference ... ok
test_base_resolves_and_is_an_ancestor_of_head ... ok
test_look_alike_commit_and_uncommitted_edit_still_report_equal ... ok
test_the_two_readmes_are_byte_identical_to_the_base ... ok

----------------------------------------------------------------------
Ran 4 tests in 3.335s

OK
```

## Mutation patches (run and shown failing before this file was committed)

Forward patches under `.mob/records/178/mutations/`. Each was applied, the module run, then the file restored.

### M1-select-by-subject-prefix.patch

Replaces `comparison_commit()` with a subject-prefix selector (`Card 178 green`). `test_look_alike_commit_and_uncommitted_edit_still_report_equal` fails:

```
AssertionError: Lists differ: [] != ['README.md']
look-alike commit or uncommitted edit changed the helper: ['README.md']
Ran 4 tests in 3.505s
FAILED (failures=1)
```

### M2-read-working-tree.patch

Replaces `blob()` with `(REPO / path).read_bytes()`. Two failures:

```
test_adding_the_page_and_editing_a_readme_reports_a_difference
AssertionError: Lists differ: ['README.md'] != []
expected README.md to differ at 9548728

test_look_alike_commit_and_uncommitted_edit_still_report_equal
AssertionError: Lists differ: [] != ['README.md', 'demo/tasks/README.md']
look-alike commit or uncommitted edit changed the helper: ['README.md', 'demo/tasks/README.md']
Ran 4 tests in 2.847s
FAILED (failures=2)
```

## Neighbouring tests

`python -m unittest tests.test_demo_readme tests.test_readme_reference_status tests.test_license_files`: Ran 18, OK.

`python -m unittest tests.test_live_docs_266` (also listed): Ran 36, FAILED (failures=1) on this pre-change tree, before the characterisation file existed. `GuardScan266.test_t11_putting_phrase_back_in_the_agreement_line_156_fails_the_scan`: `TEAM_WORKING_AGREEMENT.md:156 holds no Avowry to put Phrase back over`. That pin is the agreement, not a README. README bytes were not changed.

## Line count

`tests/test_readmes_untouched_178.py`: 168 lines (136 nonempty).

## Revision 1

Reason: the scratch self-tests cloned the real repo (`git clone --shared`). Once the green commit adds `docs/PREREQUISITES.md`, `git log --diff-filter=A -1 -- docs/PREREQUISITES.md` in that clone finds the real page-adding commit (READMEs equal BASE) instead of the scratch commit. `test_adding_the_page_and_editing_a_readme_reports_a_difference` then fails with `Lists differ: ['README.md'] != []`. The slice required the characterisation to pass on the pre-change tree and after the green.

Fix: `_synthetic_repo` builds a `git init` repository in `tempfile.TemporaryDirectory` (base commit holds README.md and demo/tasks/README.md). The two scratch tests patch the module's `REPO` and `BASE` to that repo. The difference test's second commit adds the page and edits README.md. The look-alike test adds the page with READMEs unchanged, then a later commit subject `Card 178 green 1 (scratch)` edits README.md, plus an uncommitted append to demo/tasks/README.md. Real-repo tests (BASE ancestor; READMEs byte-identical at the comparison commit) are unchanged.

### Two-tree proof

Pre-change tree (`card-178-red2`, no page): `python -m unittest tests.test_readmes_untouched_178 -v`

```
test_adding_the_page_and_editing_a_readme_reports_a_difference ... ok
test_base_resolves_and_is_an_ancestor_of_head ... ok
test_look_alike_commit_and_uncommitted_edit_still_report_equal ... ok
test_the_two_readmes_are_byte_identical_to_the_base ... ok

----------------------------------------------------------------------
Ran 4 tests in 2.511s

OK
```

Post-green: `git checkout -b card-178-tmp`, `git cherry-pick 4141290`, `python -m unittest tests.test_readmes_untouched_178 tests.test_prerequisites -v`, then `git checkout card-178-red2` and `git branch -D card-178-tmp`.

```
test_adding_the_page_and_editing_a_readme_reports_a_difference ... ok
test_base_resolves_and_is_an_ancestor_of_head ... ok
test_look_alike_commit_and_uncommitted_edit_still_report_equal ... ok
test_the_two_readmes_are_byte_identical_to_the_base ... ok
test_prerequisites_page_matches_what_CI_uses ... ok
test_labels_only_page_fails_every_prose_check ... ok
test_minimal_compliant_page_passes ... ok
test_parser_reads_a_synthetic_matrix_workflow ... ok

----------------------------------------------------------------------
Ran 8 tests in 1.643s

OK
```

### Mutation lines (applied with `git apply`, reversed with `git apply -R`)

### M1-select-by-subject-prefix.patch

`test_look_alike_commit_and_uncommitted_edit_still_report_equal` fails:

```
AssertionError: Lists differ: [] != ['README.md']
look-alike commit or uncommitted edit changed the helper: ['README.md']
Ran 4 tests in 1.952s
FAILED (failures=1)
```

### M2-read-working-tree.patch

Two failures:

```
test_adding_the_page_and_editing_a_readme_reports_a_difference
AssertionError: Lists differ: ['README.md'] != []
expected README.md to differ at b0e7b85

test_look_alike_commit_and_uncommitted_edit_still_report_equal
AssertionError: Lists differ: [] != ['README.md', 'demo/tasks/README.md']
look-alike commit or uncommitted edit changed the helper: ['README.md', 'demo/tasks/README.md']
Ran 4 tests in 1.914s
FAILED (failures=2)
```

### Neighbouring tests

`python -m unittest tests.test_demo_readme tests.test_readme_reference_status tests.test_license_files`: Ran 18 tests in 0.069s, OK.

`tests/test_readmes_untouched_178.py`: 175 lines (143 nonempty).
