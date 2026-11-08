# Card 179 characterisation (D157): the Tasks demo README stays byte-identical

Declared CHARACTERISATION, a slice constraint, not a criterion. The tests pass on the pre-change tree (HEAD `b30803420f30f4ff749f77d39be7640461d0506f`, no `docs/GETTING_STARTED.md` yet). Reason: the green must not edit `demo/tasks/README.md`; that is already true, and the tests pin it before the green so a later edit of that file at the guide-adding commit fails.

BASE = `b30803420f30f4ff749f77d39be7640461d0506f` (`git rev-parse HEAD` at this commit). The same hash is the constant in `tests/test_demo_readme_untouched_179.py`.

## What was written

`tests/test_demo_readme_untouched_179.py` (module `tests.test_demo_readme_untouched_179`, no C# wrapper). Class `DemoReadmeUntouched179`, 4 tests:

1. BASE resolves and is an ancestor of HEAD.
2. the `demo/tasks/README.md` blob at BASE equals the blob at the comparison commit (`git show REV:demo/tasks/README.md`; empty blob fails). Comparison commit: `git log --diff-filter=A --format=%H -1 -- docs/GETTING_STARTED.md`; HEAD if none.
3. look-alike-commit and uncommitted-edit stay equal: a `git init` synthetic repo adds the guide, then a later commit with subject `Card 179 green 1 (scratch)` edits README.md, and demo/tasks/README.md gets an uncommitted append; the helper still reports equal.
4. a scratch commit that adds the guide and edits the demo README reports a difference.

Self-tests use a `git init` synthetic repo, not a clone (card 178 revision 1).

## Command

`python -m unittest tests.test_demo_readme_untouched_179 -v`

## Result on the pre-change tree

Worktree `C:\Users\ringe\projects\wt-179-red`, HEAD `b30803420f30f4ff749f77d39be7640461d0506f`, no `docs/GETTING_STARTED.md`.

```
test_adding_the_guide_and_editing_the_demo_readme_reports_a_difference ... ok
test_base_resolves_and_is_an_ancestor_of_head ... ok
test_look_alike_commit_and_uncommitted_edit_still_report_equal ... ok
test_the_demo_readme_is_byte_identical_to_the_base ... ok

----------------------------------------------------------------------
Ran 4 tests in 2.500s

OK
```

## Result on the post-change scratch tree

Clone left at `C:\Users\ringe\AppData\Local\Temp\avowry-179-char-pass`. That clone added `docs/GETTING_STARTED.md` (any text) and rewrote `README.md` (any text) in one commit; `demo/tasks/README.md` was not touched. The characterisation test file was copied into the clone's working tree (uncommitted) so the module could run.

```
test_adding_the_guide_and_editing_the_demo_readme_reports_a_difference ... ok
test_base_resolves_and_is_an_ancestor_of_head ... ok
test_look_alike_commit_and_uncommitted_edit_still_report_equal ... ok
test_the_demo_readme_is_byte_identical_to_the_base ... ok

----------------------------------------------------------------------
Ran 4 tests in 1.997s

OK
```

## Mutation (edit of demo/tasks/README.md)

Clone left at `C:\Users\ringe\AppData\Local\Temp\avowry-179-char-fail`. Same new state plus `demo/tasks/README.md` edited in the guide-adding commit.

```
FAIL: test_the_demo_readme_is_byte_identical_to_the_base
AssertionError: Lists differ: [] != ['demo/tasks/README.md']
demo README differs between b308034 and f4dd729: ['demo/tasks/README.md']
Ran 4 tests in 1.862s
FAILED (failures=1)
```

The three synthetic self-tests still passed on that tree.

## Neighbouring tests (pre-change tree, before this commit)

`python -m unittest tests.test_demo_readme tests.test_license_files tests.test_prerequisites tests.test_readme_reference_status tests.test_live_docs_266`

Ran 40 tests in 11.118s, FAILED (failures=1): `tests.test_live_docs_266` T11 (`TEAM_WORKING_AGREEMENT.md:156 holds no Avowry to put Phrase back over`). Bug 338, already red, not this card.

`tests.test_rename_196` ran in this worktree (no extra clone): Ran 15 tests in 47.543s, FAILED (failures=6). List counts, eol pins and directory counts have drifted since card 196 (extra diagnostics files and later acceptance tests). Already red on this tree; not this card.

`tests.test_rename_197` was not run for this characterisation commit (it lists acceptance tests via `dotnet test --list-tests`; this commit adds no C# test).

## Line count

`tests/test_demo_readme_untouched_179.py`: 174 lines.
