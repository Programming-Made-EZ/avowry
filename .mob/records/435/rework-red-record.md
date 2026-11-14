# Card 435 rework red record (RED writing seat, Claude Sonnet, linked worktree, HEAD e888e13)

Changes left uncommitted. `git diff --stat`:

    tests/Avowry.Acceptance.Tests/PathScrubForms.cs | 135 +++
    1 file changed, 135 insertions(+)

`git diff --stat -- tools` is empty. No existing test or data set changed.

## Added (test file only)
| Data set | Rows | Pending theory (F) | Characterisation theory (C) |
|---|---|---|---|
| `GluedAndUncRootsRework` (435-1) | 11 | `Id_435_1_rework_rows` Take(6) | `Id_435_1_rework_rows_that_pass_today` Skip(6), 5 rows |
| `NonAsciiAccountFoldersRework` (435-2) | 8 | `Id_435_2_rework_rows` Take(3) | `Id_435_2_rework_rows_that_pass_today` Skip(3), 5 rows |
| `EscapesAfterAPathRework` (435-3) | 16 | `Id_435_3_rework_rows` Take(12) | `Id_435_3_rework_rows_that_pass_today` Skip(12), 4 rows |

All ten theories go through `AssertMissedRoot` (both forms, no `<user>`/stand-in left, stable second pass). Inputs are verbatim strings, so `\n`, `\u0027`, `\\host` are literal text; `<user>` is the placeholder, replaced by the stand-in inside the assertion. Root words (`home/`, `Users\`) are split across concatenations as in the first red.

## Commands and results
- `dotnet build Avowry.sln -c Debug`: 0 Warning(s), 0 Error(s).
- `--filter "Slice=435"`: Failed 21, Passed 91, Total 112 (77 + 35, as expected).
- `Criterion=435-1`: Failed 6, Passed 24, Total 30.
- `Criterion=435-2`: Failed 3, Passed 54, Total 57.
- `Criterion=435-3`: Failed 12, Passed 13, Total 25.

All failures are `Assert.Equal` mismatches (no crash, no compile error). Every C row and every pre-existing test passes.

## Failing cases (first line of message: Expected / Actual)
435-1 (`Id_435_1_rework_rows`)
- `gcc /tmp/a.c\n-I/home/<user>/include`: expected `gcc a.c\n-Iinclude`, actual `gcc a.c\n-I/home/<user>/include`
- `gcc /tmp/a.c\u0027-I/home/<user>/include\u0027`: actual leaves `-I/home/<user>/include`
- `gcc '-I/home/<user>/include'`: actual unchanged
- `["\host\share\x.cs","C:\Users\<user>\y.cs"]`: expected `["x.cs","y.cs"]`, actual `["y.cs"]`
- `\u0027\\host\share\x.cs\u0027,\u0027/home/<user>/y.cs\u0027`: actual `\u0027y.cs\u0027`
- `\host\share\x.cs;C:\Users\<user>\y.cs`: expected `x.cs;y.cs`, actual `y.cs`

435-2 (`Id_435_2_rework_rows`)
- `C:\Users\<user>(1)D:\Users\<user>\x.cs`: expected `<path>x.cs`, actual `<path>:\Users\<user>\x.cs`
- `C:\Users\<user>(1)file:///C:/Users/<user>/x.cs`: actual `<path>:///x.cs`
- `see C:\Users\<user>(bb).`: expected `see <path>.`, actual `see <path>`

435-3 (`Id_435_3_rework_rows`)
- `C:\a\x.cs\b/home/<user>/y.cs`: actual `x.cs\b/home/<user>/y.cs`
- `C:\a\x.cs\f/home/<user>/y.cs`: actual `x.cs\f/home/<user>/y.cs`
- `\u00f6/tmp/a\u00f6C:\Users\<user>`: actual leaves `C:\Users\<user>`
- `\t/tmp/a.C:\Users\<user>`: actual leaves the `C:\Users\<user>` tail
- `-I/tmp/a.C:\Users\<user>`: actual `-Ia.C:\Users\<user>`
- `\u00f6/tmp/a\u00f6file://localhost/home/<user>/x.cs`: actual leaves the file URL
- `/tmp/a/~\nx.cs`: expected `<path>\nx.cs`, actual `~\nx.cs`
- `C:\a\~\nx.cs`: actual `~\nx.cs`
- `\/home/<user>\nx.cs`: expected `\<path>\nx.cs`, actual `nx.cs`
- `\/opt/data/home/<user>`: expected `\<path>`, actual `<path>`
- `\\host\share\x.cs\n/home/<user>/y.cs`: expected `x.cs\ny.cs`, actual `y.cs`
- `\u0027\host\share\x.cs\u0027/home/<user>/y.cs`: expected `\u0027x.cs\u0027y.cs`, actual `\u0027y.cs`

## Deviations from the spec
1. Counts: 435-2 has 5 C rows (M6, M7, M8, M9, M11), not 6; totals still 35 (11 + 8 + 16), 21 F.
2. Status trait: the existing 435 Pending theories carry NO `[Trait("Status","Pending")]` and no pending comment (only `Slice` and `Criterion`; "Pending" there is a naming convention in the record). I mirrored them, so the new Pending theories also have no Status trait. If the facilitator wants the trait added to the six new F theories, it is a one-line edit each.
3. The rows with `\u0027` after `<user>` (e.g. row 2) use concatenated verbatim strings; the string values equal the table text.
