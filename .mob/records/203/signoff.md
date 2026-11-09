# Card 203 fresh sign-off (fresh Claude context, separate from the gate reviewer; own full gate in its own clone)

**SIGNED OFF WITH CONDITIONS.** No condition blocks the card. All three criteria are met and the gate is green; the conditions are follow-ups.

## Commands and results (own clone of main at 0c3988a; 5a82e7d is an ancestor, check exit 0)
- `dotnet build src/Avowry.Cli`: PASS, 0 warnings, 0 errors.
- `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`: PASS, no flake, no rerun. Compiler 324 of 324; Acceptance 350 of 350 in 9 m 30 s.
- `python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map`: Ran 56 tests, FAILED (failures=8): the expected eight (five rename_196, two rename_197, T11 live_docs_266).

## Real CLI checks (built from the clone, tiny sources in a temp folder inside it)
1. `type Avows`: exit 1, no `.cs` files written. `PH341 (3:1) The C# target reserves the name 'Avows' for the generated class that holds every avow, so type 'Avows' cannot use it; rename the type declared on line 3.`
2. A valid source (types `Avow` and `Plain`) builds, exit 0, 6 `.cs` files.
3. Near misses still build: type `Avow` exit 0; an endpoint named `Types` exit 0 (`Types.cs` beside `Types.cs.cs`). (The seat's first endpoint source used wrong syntax, PH200/PH203; its own error, corrected from the corpus fixture.)
4. Other rejections, exit 1 with PH341: `module Program` (1:1, correct); avow `"avows"` (3:1, correct); `type Email` with field `email`: "...field 'email' of type 'Email' cannot use it; rename the field declared on line 3": the wrong line (the field is on line 4; line 3 is the type): gate finding F1 shows up as a user-visible message.

## Dana's view
The message names the reserved name, says why, names which declaration collides and gives the line; no C# error is seen and nothing is written. Understandable. Two wording nits: "The C# target reserves" is slightly internal; no replacement name is suggested.

## Story and criteria
Story satisfied (a language diagnostic, exit 1, zero C# files). Criterion 1 met (name and line before any C# is generated; type, module, avow-method and field collisions covered; `AvowryRuntime` and `AvowryBuild` are file names only and stay buildable per the adopted Ready draft; the field line is wrong, F1). Criterion 2 met (nine reject rows, five reject fixtures, near-miss fixtures `type-lookalikes`, `avow-fail-no-holes`, `endpoint-file-stems`). Criterion 3 met (gate green; generated-C# golden test passes; CI not checked by this seat). Value rule: no consumer-value gap.

**Gate evidence:** tested commit 5a82e7d30e4cc26b1d53cd2498f05b07cff7fee3 (main tip 0c3988a); `--no-hardlinks` single-branch clone of local main; dotnet processes at start: 1 (a residual build server). **Load:** the gate ran alone; only file reads during the Acceptance run.

## Conditions (none blocks)
1. File F1 as a bug (the field-collision message cites the type's line); F2 (dead `FieldModel.Line` and `FieldModel.Column`) rides with it.
2. Surviving mutants are known test limits to carry: a case-differing method name has no near miss; "all hits, sorted by line" is untested; the kind-word assertions are weak; the unconditional rejection of the `AvowSpecs` and `AvowryFailure` names is not pinned by an unused-name source.

## Retro risks
Rejecting unused `AvowSpecs`/`avowry failure` names is stricter than C# requires (a deliberate cost; check no real source hits it); the follow-up cards the proposal listed (derived names, the `.cs.cs` extension, case-insensitive file systems, the `Specs` namespace) still need filing; the card file still says `Phrase.sln` and lists `AvowryRuntime`/`AvowryBuild` as reserved (the Decider's corrections cover it); the 9.5-minute Acceptance run is heavy and the timing test load-sensitive.
