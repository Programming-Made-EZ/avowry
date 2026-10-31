# Card 219 review (ctx-5, independent reviewer, audit lens, Claude seat)

Verdict: APPROVE

Seat started 2026-10-31 00:43 UTC (in-flight line 8f262a8; last probe age 9 min; cap 01:13 UTC). Reviewed 79e0892..ae7f982 (scripts/mob_process.py, tests/test_mob_process.py, the commit bodies) against D159 C1 to C6, D157, D136, D162.

## Findings

1. Low (advisory, no change required) - M3 forged-date guard: not needed. scripts/mob_process.py:126-133 orders by `git rev-list --topo-order --reverse HEAD` position, never by committer date, so a forged record date cannot reorder records by construction; a test would only pin the design. Evidence dates (`%cI`) were already forgeable and are unchanged (D159 "Not in this card"). Fix: none; optionally say so in the signoff note.
2. Low - tests/test_mob_process.py `test_guard_fixture_commit_dates_are_the_fixed_ones_in_close_order` tests the fixture, not the checker. Harmless (it keeps the strict `<` from being a false red); could be dropped later.
3. Medium (for the facilitator, not the card) - landing the held records needs a MOB.md regeneration in the same step (D159 C6). In the rehearsal the only two errors were "MOB.md done list does not equal record and legacy cards" and "MOB.md is not byte-identical to write-mob-md output". Fix: run the MOB.md writer after adding 209.json and 208.json, then re-run check.
4. Note (out of scope, unchanged by this card) - first-add position lets a record be added, removed and re-added to claim an earlier slot (scripts/mob_process.py:152-153, `splitlines()[-1]`). That only makes the previous record older, so it does not widen anti-backdating. The architect already named it.

No Blocker or High findings.

## What I verified

- C1: `previous_record` (scripts/mob_process.py:126-134) takes, among records whose commit is strictly earlier in history than the card's own record commit, the one nearest to it; none gives None (first record, same-commit records, commit not in history, card without a record; table rows in the unit test).
- C2 kept: own-commit, after-closure, reuse, "Card NNN in message" and distinct-evidence lines are untouched (diff touches only the helper, the `commit_order` line and the `previous =` line). Anti-backdating holds: evidence must still postdate the record committed before this card's record (`when < record_dates[previous]`).
- C3: the message is unchanged text and names the commit-order record; `test_backdated_evidence_is_refused_naming_the_record_committed_immediately_before` asserts exactly `Card 209 evidence <sha> predates Card 214's record` with 216, 214, 209 (a fixture chosen so the staged rule and the final rule differ; the deviation from the slice's 205/214/209 is explained and right).
- Three Done checks: 1 = `test_record_added_out_of_number_order_is_accepted` (red 451f4ea by assertion); 2 = the back-dated test (red 55b5794 by assertion); 3 = `test_held_record_sequence_passes` with add, hold back, 219, re-add (born-green, mutation-proved, declared in the commit body as a card check inside cycle 2 under the slice amendment).
- D157 order from git: red 451f4ea, unit ab94601, green b374ef7 (criterion 1), then red 55b5794, unit 66904ab, green 1a08bd6 (criterion 2). Each carries "Criterion: n"; green 1 precedes red 2. Outside-in: acceptance test first, unit layer under it, no doubles beyond the temp git repo.
- D136: two countable files; commits of 1 to 53 added lines; one concern per commit.
- Full test file on HEAD: `python -m unittest tests.test_mob_process`, 77 tests OK.
- Mutations re-run in a scratch clone (now removed): M1 (revert to `max(n < card)`): out_of_number_order, backdated and held_record each FAIL (previous_record unit table passes, as it tests the helper directly). M5 (`splitlines()[0]` for the record commit): held_record FAILS, the other two pass, so Done check 3 does real work. My attempt at M3 (order by committer date) errored in the test run because my patch ran `git show` from the wrong directory, so I rely on the structural argument in finding 1.
- Record checker real use: in a throwaway `git clone --local` (origin pointed back at the real GitHub URL) I copied 209.json.held and 208.json.held to .mob/records/, committed them there, and ran `python scripts/mob_process.py check` once. No "predates", reuse, own-commit or closure error; the only two errors were the MOB.md ones in finding 3. The clone directory was removed by its literal path.

## Domain-expert answer (T2)

Does a member with different domain knowledge need to join? No. The change is a pure ordering helper over git history plus one rev-list call; the risks (out-of-order close, back-dated evidence, first record, same-commit ties, hold-and-re-add of the real 209/208 history) are each covered by a named test and a mutation. No security boundary, runner or language change is involved.
