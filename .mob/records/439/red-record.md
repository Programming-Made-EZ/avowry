# Card 439 red record (RED seat, claude sonnet = claude-sonnet-5-5, ctx-2)

Worktree wt-439, branch card-439, HEAD 6c16aab. Changes UNCOMMITTED. No production file touched (`git diff --stat -- tools` is empty; `git diff --stat` is empty because both files are new).

## Files (untracked, new)
- `tests/Avowry.Acceptance.Tests/PathScan439.cs`: review-input and mutant-killing data sets, the blow-up facts, `ReviewInputs` (107 inputs in the architect's order, used by sweep C).
- `tests/Avowry.Acceptance.Tests/PathScan439Generated.cs`: sweeps B (seed 439, extended lists) and C (seed 4391, review inputs as templates), ported from the architect's `Gen.cs`; the card 435 generator is unchanged.

## Data sets (row counts; each row runs in both account forms, `<user>` and the plain stand-in; expected = the reference design's output)
| Data set | Criterion | Pending (fail today) | Characterisation (pass today) |
|---|---|---|---|
| ThirdReviewBlocking (ids 3.x class B) | 439-1 | 23 | 8 |
| OtherReviewRows (2.x, 3.2d-f, 3.4d, 3.8, hdr) | 439-2 | 37 | 39 |
| MutantKillingRows (45 mutants, the slice's section 3) | 439-4 | 19 | 26 |
| BlowUpShapes at 20,000 characters (9 shapes) | 439-3 | 0 | 9 |
| Sweep B, sweep C (facts) | 439-2 | 2 | 0 |

Totals: 81 Pending, 82 characterisation. Pending rows carry `[Trait("Status", "Pending")]` and the comment `// Pending until the card 439 green that changes tools/Avowry.Bench/PathScrub.cs` (form of commit 2a5be77). Every test carries `Slice`=439 and its `Criterion`. 439-5 (the review prompt) has no test.

## Commands and summary lines
- `dotnet build Avowry.sln -c Debug`: 0 Warning(s), 0 Error(s).
- `--filter "Slice=439&Status=Pending"`: Failed! Failed: 81, Passed: 0, Total: 81 (79 `Assert.Equal() Failure: Strings differ`, 2 sweep facts by `Assert.True`; no crash, no exception other than assertion).
- `--filter "Slice=439&Status!=Pending"`: Passed! Failed: 0, Passed: 82, Total: 82.
- `--filter "Slice=435|Slice=426|Slice=418"`: Passed! Failed: 0, Passed: 186, Total: 186.

## Sweep counts printed today (match the architect's table for "today")
- B: leaks_base=45965 leaks_new=19720 regressions=2424 second_pass=1523 exclusions=0
- C: leaks_base=50216 leaks_new=28735 regressions=6592 second_pass=6867 exclusions=0
The green must reach regressions=0 and second_pass=0 (the architect measured leaks_new 4559 in B and 2904 in C; printed, not asserted).

## Characterisation rows (pass today, no Pending)
- ThirdReviewBlocking: 3.5e, 3.7a, 3.7b, 3.7c, 3.7d, 3.7e, 3.7f, 3.7g
- OtherReviewRows: 2.1a, 2.1b, 2.1c, 2.1d, 2.2a, 2.2b, 2.2c, 2.2d, 2.2e, 2.2f, 2.2g, 2.2h, 2.3a, 2.3b, 2.3c, 2.4a, 2.4b, 2.4c, 2.4d, 2.4e, 2.4f, 2.5e, 2.5n, 2.5o, 2.7a, 2.7b, 2.7c, 2.8, 2.9a, 2.9b, 2.9c, 2.9d, 2.9e, 2.9f, 2.9g, 2.9h, 2.9i, 2.9j, 2.11
- MutantKillingRows: e1-unescaped, esc-no-b, letter-any, flag-letter-I, unc-pathmark, unc-always-sgl, unc-no-drive-host, tilde-no-paren, walk-any-end, fixed-no-vartmp, unix-no-Users, unix-drive-c, final-paren-ok, names-semicolon-ok, guard-n-only, esc-off, parent-no-home, parent-no-das, parent-cs, no-parent-rule, parent-drive-any-depth, parent-unix-ci, dir-space-ok, no-tail, no-tilde-name, word-no-esc
- BlowUpShapes: `\;`, `\,`, `\a;`, repeated doubled UNC paths, `\C:/a
`, `\C:\a
`, `\\a$
`, `/a`, `/-` at 20,000 characters: output stable on a second pass (the time of the first pass is printed, never asserted, D254).

## Failing (Pending) test ids with the first assertion lines (account name shown as `<user>`)
- `PathScan439.Id_439_2_other_review_rows(id: "2.5g", input: "C:\\Users\\<user>(bb'cc)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>(bb'cc)"
- `PathScan439.Id_439_2_other_review_rows(id: "2.13", input: "\\\\host\\c$\\Documents and Settings\\<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5a", input: "C:\\Users\\<user>(\\u00f6bb)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>(\\u00f6bb)"
- `PathScan439.Id_439_2_other_review_rows(id: "2.6a", input: "gcc /tmp/a.c -I/home/<user>", expected: "gcc a.c -I<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "gcc a.c -I<path>"
    Actual:   "gcc <user>"
- `PathScan439.Id_439_2_other_review_rows(id: "3.8b", input: "\\t/tmp/a.\\\\host\\Users\\<user>", expected: "\\t<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\t<path>"
    Actual:   "\\t<user>"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5d", input: "/home/<user>(\\u00f6bb)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>(\\u00f6bb)"
- `PathScan439.Id_439_2_other_review_rows(id: "3.2e", input: "\\\\\\\\host\\\\sh\\u00f6re\\\\Users\\\\<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_2_other_review_rows(id: "2.6b", input: "gcc /tmp/a.c -I/Users/<user>", expected: "gcc a.c -I<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "gcc a.c -I<path>"
    Actual:   "gcc <user>"
- `PathScan439.Id_439_2_other_review_rows(id: "3.8f", input: "\\\\/cygdrive/c/Users/<user>/x.cs\\tnext", expected: "\\\\<path>\\tnext")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\\\<path>\\tnext"
    Actual:   "\\\\x.cs\\tnext"
- `PathScan439.Id_439_2_other_review_rows(id: "2.6d", input: "gcc /tmp/a.c -I/home/<user>/include", expected: "gcc a.c -Iinclude")`
    Assert.Equal() Failure: Strings differ
    Expected: "gcc a.c -Iinclude"
    Actual:   "gcc include"
- `PathScan439.Id_439_2_other_review_rows(id: "3.4d", input: "foo-~\\nx.cs", expected: "foo-~\\nx.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "foo-~\\nx.cs"
    Actual:   "foo-nx.cs"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5i", input: "C:\\Users\\<user> bb.(cc)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>.(cc)"
- `PathScan439.Id_439_2_other_review_rows(id: "2.12c", input: "-I~/include", expected: "-Iinclude")`
    Assert.Equal() Failure: Strings differ
    Expected: "-Iinclude"
    Actual:   "-I~/include"
- `PathScan439.Id_439_2_other_review_rows(id: "3.2f", input: "\\u0027\\\\h\\u00f6st\\Users\\<user>\\u0027", expected: "\\u0027<path>\\u0027")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\u0027<path>\\u0027"
    Actual:   "\\u0027<user>\\u0027"
- `PathScan439.Id_439_2_other_review_rows(id: "2.12b", input: "\\u0027~/x.cs\\u0027", expected: "\\u0027x.cs\\u0027")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\u0027x.cs\\u0027"
    Actual:   "\\u0027~/x.cs\\u0027"
- `PathScan439.Id_439_2_other_review_rows(id: "2.6e", input: "see /tmp/x.cs (/home/<user>)", expected: "see <path>)")`
    Assert.Equal() Failure: Strings differ
    Expected: "see <path>)"
    Actual:   "see <user>)"
- `PathScan439.Id_439_2_other_review_rows(id: "3.8c", input: "\\u00f6/tmp/a\\u00f6\\\\wsl$\\Ubuntu\\home\\<user>"···, expected: "\\u00f6<path>\\tnext")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\u00f6<path>\\tnext"
    Actual:   "\\u00f6<user>\\tnext"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5m", input: "C:\\Users\\<user>ö'bb", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>'bb"
- `PathScan439.Id_439_2_other_review_rows(id: "2.12a", input: "C:\\\\a\\\\x.cs\\n~/y.cs", expected: "x.cs\\ny.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "x.cs\\ny.cs"
    Actual:   "x.cs\\n~/y.cs"
- `PathScan439.Id_439_2_other_review_rows(id: "3.8a", input: "-I/tmp/a.\\\\host\\c$\\Users\\<user>\\nnext", expected: "-I<path>\\nnext")`
    Assert.Equal() Failure: Strings differ
    Expected: "-I<path>\\nnext"
    Actual:   "-I<user>\\nnext"
- `PathScan439.Id_439_2_other_review_rows(id: "2.2j", input: "(-I/home/<user>/include)", expected: "(-Iinclude)")`
    Assert.Equal() Failure: Strings differ
    Expected: "(-Iinclude)"
    Actual:   "(-I/home/<user>/include)"
- `PathScan439.Id_439_2_other_review_rows(id: "2.6c", input: "gcc /tmp/a.c -I/cygdrive/c/Users/<user>", expected: "gcc a.c -I<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "gcc a.c -I<path>"
    Actual:   "gcc <user>"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5j", input: "C:\\Users\\<user>'bb\\x.cs", expected: "x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "x.cs"
    Actual:   "<path>'bb\\x.cs"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5b", input: "C:\\\\Users\\\\<user>(\\u00f6bb)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>(\\u00f6bb)"
- `PathScan439.Id_439_2_other_review_rows(id: "3.8d", input: "\\u00f6/tmp/a\\u00f6/home/<user>", expected: "\\u00f6<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\u00f6<path>"
    Actual:   "\\u00f6<user>"
- `PathScan439.Id_439_2_other_review_rows(id: "2.2k", input: "-Wall,-I/home/<user>/include", expected: "-Wall,-Iinclude")`
    Assert.Equal() Failure: Strings differ
    Expected: "-Wall,-Iinclude"
    Actual:   "-Wall,-I/home/<user>/include"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5l", input: "C:\\Users\\<user>\\u00f6'bb", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>'bb"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5k", input: "C:\\Users\\<user>'bb", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>'bb"
- `PathScan439.Id_439_2_other_review_rows(id: "3.8e", input: "\\\\/opt/data/home/<user>/src/x.cs", expected: "\\\\<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\\\<path>"
    Actual:   "\\\\x.cs"
- `PathScan439.Id_439_2_other_review_rows(id: "2.6g", input: "C:\\Users\\<user>\\n/home/<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_2_other_review_rows(id: "hdr", input: "match \\\\d+ here", expected: "match \\\\d+ here")`
    Assert.Equal() Failure: Strings differ
    Expected: "match \\\\d+ here"
    Actual:   "match d+ here"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5h", input: "C:\\Users\\<user> bb.\\u00f6cc", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>.\\u00f6cc"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5c", input: "\\u0027C:\\\\Users\\\\<user>(\\u00f6bb)\\u0027", expected: "\\u0027<path>\\u0027")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\u0027<path>\\u0027"
    Actual:   "\\u0027<path>(\\u00f6bb)\\u0027"
- `PathScan439.Id_439_2_other_review_rows(id: "2.2i", input: "[-I/home/<user>/include]", expected: "[-Iinclude]")`
    Assert.Equal() Failure: Strings differ
    Expected: "[-Iinclude]"
    Actual:   "[-I/home/<user>/include]"
- `PathScan439.Id_439_2_other_review_rows(id: "2.6f", input: "C:\\a\\x.cs\\n/home/<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_2_other_review_rows(id: "2.5f", input: "C:\\Users\\<user>(bb cc)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>(bb cc)"
- `PathScan439.Id_439_2_other_review_rows(id: "3.2d", input: "\\\\\\\\h\\u00f6st\\\\Users\\\\<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.5b", input: "see (C:\\Users\\<user>(bb)) here", expected: "see (<path>) here")`
    Assert.Equal() Failure: Strings differ
    Expected: "see (<path>) here"
    Actual:   "see (<path>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.5a", input: "see (C:\\Users\\<user>(bb))", expected: "see (<path>)")`
    Assert.Equal() Failure: Strings differ
    Expected: "see (<path>)"
    Actual:   "see (<path>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.4f", input: "C:\\Users\\<user>_D:\\Users\\<user>\\x.cs", expected: "<path>x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>x.cs"
    Actual:   "<path>D:\\Users\\<user>\\x.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.3c", input: "gcc \\u0027-IC:\\Users\\<user>\\include\\u0027", expected: "gcc \\u0027-Iinclude\\u0027")`
    Assert.Equal() Failure: Strings differ
    Expected: "gcc \\u0027-Iinclude\\u0027"
    Actual:   "gcc \\u0027-Iu0027"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.3b", input: "\\u0027-I\\\\host\\share\\x.cs\\u0027/home/<user>/"···, expected: "\\u0027-Ix.cs\\u0027y.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\u0027-Ix.cs\\u0027y.cs"
    Actual:   "\\u0027-Iy.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.2c", input: "\\\\a<b>\\Users\\<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.3d", input: "gcc \\u0027-IC:\\Users\\<user>\\u0027", expected: "gcc \\u0027-I<path>\\u0027")`
    Assert.Equal() Failure: Strings differ
    Expected: "gcc \\u0027-I<path>\\u0027"
    Actual:   "gcc \\u0027-Iu0027"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.4a", input: "/tmp/a/foo-~\\nx.cs", expected: "foo-~\\nx.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "foo-~\\nx.cs"
    Actual:   "foo-nx.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.1b", input: "\\\\<host>\\Users\\<user>\\x.cs", expected: "x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "x.cs"
    Actual:   "\\\\<host>\\Users\\<user>\\x.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.1a", input: "\\\\<host>\\Users\\<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "\\\\<host>\\Users\\<user>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.4h", input: "\\/tmp/a/nC:\\Users\\<user>", expected: "\\n<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\n<path>"
    Actual:   "\\nC:\\Users\\<user>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.3e", input: "see a\\u00f6-b/tmp/x.cs", expected: "see a\\u00f6-b/tmp/x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "see a\\u00f6-b/tmp/x.cs"
    Actual:   "see a\\u00f6-bx.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.4e", input: "C:\\Users\\<user>D:\\Users\\<user>\\x.cs", expected: "<path>x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>x.cs"
    Actual:   "<path>D:\\Users\\<user>\\x.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.2b", input: "\\\\host's\\Users\\<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.1c", input: "\\\\<host>\\share\\x.cs", expected: "x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "x.cs"
    Actual:   "\\\\<host>\\share\\x.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.4c", input: "C:\\\\a\\\\foo-~\\nx.cs", expected: "foo-~\\nx.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "foo-~\\nx.cs"
    Actual:   "foo-nx.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.3a", input: "\\u0027-IC:\\a\\x.cs\\u0027/home/<user>", expected: "\\u0027-Ix.cs\\u0027<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\u0027-Ix.cs\\u0027<path>"
    Actual:   "\\u0027-I<user>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.5c", input: "C:\\Users\\<user>(bb)ccfile:///C:/Users/<user>/x.c"···, expected: "<path>x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>x.cs"
    Actual:   "<path>ccfile:///x.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.5d", input: "C:\\Users\\<user>(bb)ccD:\\Users\\<user>\\x.cs", expected: "<path>x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>x.cs"
    Actual:   "<path>ccD:\\Users\\<user>\\x.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.4b", input: "/tmp/a/~~\\nx.cs", expected: "~~\\nx.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "~~\\nx.cs"
    Actual:   "~nx.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.4g", input: "C:\\Users\\<user>.file://localhost/home/<user>/x.c"···, expected: "<path>x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>x.cs"
    Actual:   "<path>file://localhost/home/<user>/x.cs"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.2a", input: "\\\\host(1)\\Users\\<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_1_third_review_blocking_rows(id: "3.4i", input: "\\t/tmp/a.file://localhost/home/<user>/x.cs", expected: "\\ta.x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\ta.x.cs"
    Actual:   "\\ta.file://localhost/home/<user>/x.cs"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "dot-no-paren", input: "C:\\Users\\<user> bb.(cc)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>.(cc)"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "glued-parens", input: "see (C:\\Users\\<user>(bb))", expected: "see (<path>)")`
    Assert.Equal() Failure: Strings differ
    Expected: "see (<path>)"
    Actual:   "see (<path>"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "flag-no-esc", input: "gcc \\u0027-IC:\\Users\\<user>\\include\\u0027", expected: "gcc \\u0027-Iinclude\\u0027")`
    Assert.Equal() Failure: Strings differ
    Expected: "gcc \\u0027-Iinclude\\u0027"
    Actual:   "gcc \\u0027-Iu0027"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "flag-any-u", input: "see a\\u00f6-b/tmp/x.cs", expected: "see a\\u00f6-b/tmp/x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "see a\\u00f6-b/tmp/x.cs"
    Actual:   "see a\\u00f6-bx.cs"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "no-bare-unc", input: "\\\\/cygdrive/c/Users/<user>/x.cs\\tnext", expected: "\\\\<path>\\tnext")`
    Assert.Equal() Failure: Strings differ
    Expected: "\\\\<path>\\tnext"
    Actual:   "\\\\x.cs\\tnext"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "glued-no-root", input: "C:\\Users\\<user>(bb)ccD:\\Users\\<user>\\x.cs", expected: "<path>x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>x.cs"
    Actual:   "<path>ccD:\\Users\\<user>\\x.cs"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "no-flag-split", input: "gcc /tmp/a.c -I/home/<user>/include", expected: "gcc a.c -Iinclude")`
    Assert.Equal() Failure: Strings differ
    Expected: "gcc a.c -Iinclude"
    Actual:   "gcc include"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "group-no-space", input: "C:\\Users\\<user>(bb cc)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>(bb cc)"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "unc-host-alone", input: "match \\\\d+ here", expected: "match \\\\d+ here")`
    Assert.Equal() Failure: Strings differ
    Expected: "match \\\\d+ here"
    Actual:   "match d+ here"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "no-adjacent", input: "C:\\Users\\<user>_D:\\Users\\<user>\\x.cs", expected: "<path>x.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>x.cs"
    Actual:   "<path>D:\\Users\\<user>\\x.cs"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "unix-no-letter-users", input: "see /tmp/x.cs (/c/users/<user>)", expected: "see <path>)")`
    Assert.Equal() Failure: Strings differ
    Expected: "see <path>)"
    Actual:   "see <user>)"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "apos-no-esc", input: "C:\\Users\\<user>\\u00f6'bb", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>'bb"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "tilde-today", input: "/tmp/a/foo-~\\nx.cs", expected: "foo-~\\nx.cs")`
    Assert.Equal() Failure: Strings differ
    Expected: "foo-~\\nx.cs"
    Actual:   "foo-nx.cs"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "parent-no-bridge", input: "C:\\a\\x.cs\\n/home/<user>", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<user>"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "apos-no-mark", input: "C:\\Users\\<user>'bb", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>'bb"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "dot-no-esc", input: "C:\\Users\\<user> bb.\\u00f6cc", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>.\\u00f6cc"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "group-no-esc", input: "C:\\Users\\<user>(\\u00f6bb)", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>(\\u00f6bb)"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "apos-today", input: "C:\\Users\\<user>'bb", expected: "<path>")`
    Assert.Equal() Failure: Strings differ
    Expected: "<path>"
    Actual:   "<path>'bb"
- `PathScan439.Id_439_4_mutant_killing_rows(id: "flag-left-quotes", input: "[-I/home/<user>/include]", expected: "[-Iinclude]")`
    Assert.Equal() Failure: Strings differ
    Expected: "[-Iinclude]"
    Actual:   "[-I/home/<user>/include]"
- `PathScan439Generated.Id_439_2_sweep_B_extended_lists_add_no_leak_and_settle_in_one_pass`
    sweep=B extended lists seed 439 inputs=100000 leaks_base=45965 leaks_new=19720 regressions=2424 second_pass=1523 exclusions=0
- `PathScan439Generated.Id_439_2_sweep_C_review_shapes_add_no_leak_and_settle_in_one_pass`
    sweep=C review shapes seed 4391 inputs=100000 leaks_base=50216 leaks_new=28735 regressions=6592 second_pass=6867 exclusions=0

## Deviations from the slice, and notes
- The slice says 100 review inputs (200 in both forms); Review.cs holds 107 ids (107 ids give 200 rows in both forms, since 14 ids hold no account name). The tests use all 107 ids.
- Ruling 2 says the absolute leak counts are "pinned and printed"; the brief says print them. They are printed and not pinned (pinning would fix the green to the reference scanner's 4559 and 2904). Ask the facilitator if a pin is wanted.
- Mutant rows: a Pending row proves its mutant's killing row fails on today's code; a characterisation row passes today and kills its mutant only when the green is mutated. The red did not run the mutants against a scanner (none exists yet); the architect's 45 of 45 stands.
- Criterion mapping (the slice does not state one): 439-1 third review BLOCKING ids; 439-2 all other review ids and sweeps B and C; 439-3 blow-up shapes; 439-4 mutant rows.
- Blow-up facts assert only second-pass stability; today's quadratic shapes (`\C:/a
`) take about 2 s each at 20,000 characters, which makes that file's characterisation run about 10 s.
- The expected outputs and the Pending split were computed with a throwaway harness in the scratchpad (design and today's code side by side) and then confirmed by the real runs above (81 and 82 as predicted).

## Facilitator ruling on the leak counts (after the first red)
- Ruling: pin the sweep leak counts as upper bounds at the reference design's measured values, in addition to 0 new leaks and 0 second-pass changes. Sweep B (seed 439) asserts leaks_new <= 4559; sweep C (seed 4391) asserts leaks_new <= 2904; counts still printed; both stay Pending (they fail today). This replaces the "printed, not pinned" note above.

## Fix round red (review round 1; worktree at 7903a6a, the round 1 green)

Changes, uncommitted, in `PathScan439.cs` and `PathScan439Generated.cs` only (no production file; earlier rows not edited or reordered, with the one correction below).

New rows (all run with the placeholder, the plain stand-in, and two stand-ins that start with an escape letter, n and r, built at run time):
- `Id_439_2_fix_round_findings_4_to_8` (Pending, criterion 439-2, 11 rows): 4a `C:/Users\<user>\x.cs` and 4b the `file:///` form (expected `x.cs`); 5a `/mnt/wsl/c/users/<user>/x.cs`, 5b `/opt/cygdrive/c/users/<user>/x.cs` (`x.cs`), 5c `/a/c/users/<user>` (`<path>`); 7 the JSON row (`{"pattern":"\\d+","path":"x.cs"}`, the reference's output); 8a to 8c (`<path> [Verification.csproj]`, `<path> "is denied"`, `<path> .\nnext`); 6a `see C:/temp/x.cs (/home/<user>)` and 6b `see ~/a.cs (/home/<user>)` (`see <path>)` both).
- `Id_439_4_fix_round_mutant_killing_rows` (Pending, 439-4, 2 rows): `drive-parent-guard`, `no-opener-bridge`.
- `Id_439_4_fix_round_mutant_killing_rows_that_pass_on_the_green` (characterisation, 2 rows): `glued-apos-first` (`C:\Users\<user>(bb)'\u00f6cc` gives `<path>`) and the new killing row of `unix-no-letter-users` (`/tmp/a./c/users/<user>` gives `<path>`); both already pass on the green.
- `Id_439_3_glued_flag_blow_up_shapes_stay_unchanged_and_settle` (characterisation, 439-3, 2 facts): `/=-I` and `/(-I` repeated to 20,000 characters. The reference returns the text unchanged and stable, so the fact asserts equality with the input and a stable second pass; the first-pass time is printed, never asserted (D254). They pass on the green (only its time is bad, 2 s; a wall-clock bound is not allowed).
- Sweep C upper bound 2904 to 2679 (slice-fix.md); sweep B stays 4559. Both sweeps are no longer Pending (the green removed that trait); they still pass on the current green with the new bound.

Findings 4 to 8: none of their rows passes on the green (all 11 fail by assertion). The mutant `glued-apos-first` row and the `unix-no-letter-users` row pass already. Finding 9 and 1's time are not rows.

Runs (after `dotnet build Avowry.sln -c Debug`: 0 warnings, 0 errors):
- `Slice=439&Status=Pending`: `Failed!  - Failed:    13, Passed:     0, Skipped:     0, Total:    13` (13 of 13 `Assert.Equal() Failure: Strings differ`).
- `Slice=439&Status!=Pending`: `Passed!  - Failed:     0, Passed:   167, Skipped:     0, Total:   167`.
- `Slice=435|Slice=426|Slice=418`: `Passed!  - Failed:     0, Passed:   186, Skipped:     0, Total:   186`.

First assertion lines of the 13 Pending tests (account name as `<user>`): 4a, 4b and drive-parent-guard Actual `<path>` (Expected `x.cs`); 5a, 5b Actual the input unchanged (a leak); 5c Actual `/a/c/users/<user>`; 6a, 6b and no-opener-bridge Actual `see <user>)` (a leak); 7 Actual `{"pattern":"x.cs"}`; 8a Actual `<path>[Verification.csproj]`; 8b Actual `<path>"is denied"`; 8c Actual `<path>\nnext`.

Correction to the first red: the nine blow-up facts of `Id_439_3_blow_up_shapes_settle_in_one_pass` were written with single backslashes by a shell heredoc that collapsed doubled backslashes (so `\;` instead of `\\;`, and so on). They are now the shapes the slice names (`\\;`, `\\,`, `\\a;`, `\\\\host\\share\\x.cs\n`, `\\C:/a\n`, `\\C:\\a\n`, `\\\\a$\n`, `/a`, `/-`). The same collapse garbled the shape list in the BlowUpShapes line of the section "Characterisation rows" above. All other rows came from a dump file and were not affected, and sweeps B and C reproduced the architect's counts.
