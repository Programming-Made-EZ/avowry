# Slice for card 227 (slice 1 of 1), architect seat

Card: board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md

Pages relied on, by path (route row architect-slice in docs/process-wiki/INDEX.md):
- docs/process-wiki/roles/architect-slice.md (what this seat writes)
- docs/process-wiki/rules/thin-slices.md (D136, D168: three criteria, one concern, slice under 15360 bytes, whole criteria)
- docs/process-wiki/rules/one-criterion.md (D157: one criterion per red commit)
- docs/process-wiki/howto/hostile-inputs.md (read; it only says the index row ids and statuses are the source of truth for criterion checks, so the list below is built from the card's own text)

## Criteria (D157)

The card has 3 Done checks, one concern, so it fits D136 and is not split (D168).

1. The scan blocks recursive deletes on variable or computed paths in the commands a seat executes (tool calls, hook or transcript), not only reply text. First test reproduces card 226's recorded slip, then Remove-Item -Recurse -Force, shutil.rmtree and equivalents. Includes the adopted D145 condition: a failing test on a destructive command absent from the reply text, and the Zero case (empty or missing command-scan source fails).
2. A trust-ladder entry is recorded for the slip (seat, rule, step down or a stated reason none applies) in the template's trust-ladder record.
3. Push on every commit: the working agreement and the mob-defaults template say so, and a detector reports FAIL for any commit on avowry or mob-defaults unpushed for more than about 5 minutes (git only, no gh).

## Intent line (D165)

Intent: product owner ruling 2026-10-31 (audit 62): one corrective card covering the scan fix and push-on-every-commit, "because a mis-quoted delete like the 2026-10-08 home-folder wipe could take out a repository including .git, losing every unpushed commit". Must be in a commit before the first red. (Quote taken from the card's Source field; D165 is cited by the card's Context, its page is not on my route.)

## Assumption line (D166, this card changes a rule and the checker)

Assumption: a seat's executed commands are readable from a hook or transcript that the process check can reach, and the unpushed age of a commit can be read from git alone. Probe: before red 1, run the scan source against one real transcript and confirm it contains tool-call command text; before red 3, run git log origin/main..HEAD with commit dates on avowry and mob-defaults and confirm age is derivable. If the first probe fails, stop and return to the product manager (this is the card's open point "the scan reads replies and may never see a command").

## Hostile-input list (D167, the scan takes a list of command forms)

Command forms the scan must handle, each an entry in the criterion 1 test table:
- Variable path: rm -rf "$DIR", rm -rf $DIR/, rm -rf "${A}/${B}", rm -r with an empty variable.
- Computed path: rm -rf $(dirname ...), backticks, string concatenation, PowerShell "$root\$name".
- Equivalents: Remove-Item -Recurse with and without -Force, ri, rd /s, rmdir /s, shutil.rmtree, os.removedirs, find ... -delete, git clean -fdx.
- Disguised: flags reordered or split (-fr, -r -f, --recursive), mixed case, extra whitespace, quoted command names, the command inside bash -c or powershell -Command, chained after && or ;.
- Allowed (must not flag): a literal path in a known scratch root through safe_delete.
- Zero/missing: empty command list, missing or unreadable command-scan source (must FAIL, per D145).
- Repeated: the same command twice, reported twice or once consistently, not crashing.
- Unknown: a command form not in the list is not silently passed when it carries a variable and a recursive flag.
- Disabled: a seat that is listed but whose commands are not scanned (unscanned seat) must fail the record check.

## Commit plan (D157, D136)

Three increments, one per criterion, each a red then a green; green for n before any red for n+1; greens pushed at once. "Criterion: n" in each red commit body. Exact D136 file and line limits are not stated in the pages on my route, so I set no number here: each commit stays small and the checker applies D136.
0. Commit: this slice file plus the Intent line (before the first red), body repeats the Size number (D168).
1. Red 1 (Criterion: 1): failing scan tests for the hostile list, including the command absent from reply text and the Zero case.
2. Green 1: the scan reads executed commands and flags the forms; push.
3. Red 2 (Criterion: 2): failing check that the trust-ladder entry exists.
4. Green 2: add the trust-ladder entry; push.
5. Red 3 (Criterion: 3): failing detector test for a commit unpushed over 5 minutes on each of the two repositories.
6. Green 3: detector plus the two rule texts (working agreement and mob-defaults template); push.

Not in this slice: commit-message claim check, process-kill rules (card Not in this card).

Open points returned: exact slip text (QA reconstructs from evidence), where the trust-ladder record lives (to settle in the criterion 2 red).
Size: 5075 bytes
