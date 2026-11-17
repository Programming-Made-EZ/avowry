# Card 171 slice (facilitator; binding on red and green; adopted by D281)

## Today
`src/Avowry.Cli/Program.cs` hardcodes the C# adapter's naming: `$"Avows.{MethodName(phrase)}"` (line 84) with its own `MethodName` (line 288), and `$"{endpoint.Name}Handler.Handle"` (line 106, in `Record`). The adapter (`src/Avowry.Adapters.CSharp/CSharpProvider.cs`) already returns `EndpointFiles` and `AvowFiles` on `CompilationOutput` (`src/Avowry.Compiler/CompilationOutput.cs`); `ReservedNames` holds the adapter's names.

## Change (green)
- `CompilationOutput` gains two maps filled by the adapter, beside the file maps: the map method name per endpoint (today `"<Endpoint>Handler.Handle"`) and per avow key (today `"Avows.<Method>"`). The C# adapter fills them with its own naming (one definition of the method-name rule, in the adapter).
- The CLI's map records read `csMethod` from those maps; `Program.cs` holds no `Handler`, `Handle`, `Avows` or `Phrases` literal and no copy of the method-name rule.
- Program.cs style (criterion 2): one statement per line; no nested loop headers stacked without braces (lines 86 to 87 today); no line over 120 characters; no statement chaining more than four `.Name(` calls; no null-forgiving `!`.
- The generated C# for orders and the demo is byte-identical (the recorded orders hash `409bf3c6...` and the demo output unchanged); the map bytes are unchanged too.

## Tests (red; Slice 171, one per criterion; C# in tests/Avowry.Acceptance.Tests/CliReadsAdapterNames.cs)
- 171-1: a source scan of Program.cs finds none of the adapter literals or a method-name rule (planted rows prove it), AND a build of a fixture with two endpoints and an avow shows each map record's `csMethod` equal to the value the adapter's new maps return (a true red today: the maps do not exist).
- 171-2: a source scan of Program.cs for the four style rules, with a planted row for each (a true red today: lines over 120 characters, stacked loops).
- 171-3: the orders hash and the demo generated output are unchanged (characterisation; the existing pins may already cover it: name them and add only what is missing).
- Card 169's `MapCultureInvariance` 169-2 requires a `MethodName` helper in Program.cs; the red changes that helper list so it does not require helpers this card moves, keeps every other check, and adds a planted proof for any helper that stays. Nothing else in card 169's tests changes.
- dotnet runs are the facilitator's, under Avowry's heavy lock.

## R171-1 (facilitator ruling, 2026-11-17 about 02:15Z): csMethod names the real generated method; the map may change, the generated C# may not

The red found two method-name rules: the adapter's `MethodName` (CSharpProvider.cs:954) joins every part, holes included, and is the name of the generated method in Avows.cs; the CLI's copy (Program.cs:288) skips holes. So today the map's csMethod for an avow with a hole names a method that does not exist. Criterion 1 (csMethod comes from the adapter's output) fixes that, and the card's criterion 3 pins only the generated C#. My slice's "the map bytes are unchanged too" is withdrawn.
- 171-3 becomes: the generated C# for orders and the demo is byte-identical (the Slice 142 pin `19fe6d71...`, the current value, and the Slice 176 demo golden; the slice's `409bf3c6...` was stale), and every map csMethod names a method or handler that exists in the generated C# (a true red today for an avow with a hole). Today's hole-skipping csMethod values are not pinned.
- The green does not add a second, hole-skipping rule; the adapter's one rule is used. The map change is recorded in the retro as a fix.
