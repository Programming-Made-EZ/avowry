# Card 371 characterisation (D204), QA (red) seat

Declared CHARACTERISATION, not a red. These must pass on the pre-change tree: no production file changes in this worktree (`docs/release/pre-publication-checks.md`, NOTICE, TRADEMARKS.md, docs/LICENSING.md and LICENSE are untouched). Base: `1a82c2f` (main, the architect's slice commit). Branch `wt-371-red`.

## Declared up front

| Id | Test | Why it passes today and after the green |
|---|---|---|
| A10 | `PrePublicationOwnerItemsTests.test_the_page_has_nine_owner_items` (exactly nine top-level items before any `###` in Owner items) | The page already has nine items (lines 102 to 105 and 110 to 114, the lawyer sub-list is indented and not counted); the green rewrites the nine in place and adds the brief under a `###`, which the count stops at. A boundary check, not a red. |
| X1 | `PrePublicationOwnerItemsTests.test_the_stale_check_ignores_the_answered_page` (subTests `X1`, `X1-owner-items`, `X1-brief`: the three checkers report nothing on the answered fixture page) | It reads the module's own `ANSWERED` fixture, not the live page, so it passes before and after; it guards a checker that is over-eager. It also passes against a stub that returns nothing, so the E and F rows (watched failing against the stub) are what prove the checkers can fail. |
| Card 186 | `tests.test_pre_publication_checks` (six tests, gated by `PrePublicationChecks.Pre_publication_findings_match_the_repo`) | The first commit removed the six-OPEN clause and its fixture rows; the module passes on the unrevised page and will pass on the answered page. |

The wrapper `PrePublicationOwnerItems.The_owner_items_stay_nine_and_the_stale_check_ignores_the_answered_page` (`Slice=371`, no `Criterion`, not Pending) runs A10 and X1, so the gate runs them.

Results and commands are in `red-record.md`.
