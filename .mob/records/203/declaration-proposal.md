# Proposal for Decider D227: the task-force declaration and card text for card 203 (names reserved by generated code are rejected)

Facilitator, 2026-11-08 UTC, main at c3c7042. VOTE on this declaration: adopt the card text below (the product manager seat's Ready draft with the facilitator's corrections) as card 203's Ready text, and the task force named in the corrections block: red = a fresh Claude QA context in its own worktree (Grok, Codex, Copilot and agy are out of quota; product owner direction 2026-11-08), green = a different fresh Claude context (never the red's seat), architect slice first, independent gate reviewer and sign-off two further fresh Claude contexts, second read-only review by Cursor or Muse, no cold reader (not a docs card).

STANDING-RULES PREFLIGHT: TDD red-first hard rule, D137, D154, D157, D204 (characterisation declared up front), D222 (the MULTI id lists R1 to R10 and N1 to N6), the Card format rule, the Value rule (Dana reads the diagnostic), CODING_GUIDELINES, the independence rule (red and green never the same seat; gate reviewer and sign-off are not the green): each is answered in section 0 of the draft and rechecked against the declaration. Decider protocol step 6 (D224) applies; this proposal changes no agreement text. The 30 percent rule and the strict reading of a No are unchanged.

The draft (verbatim, with the facilitator's notes first):

# Card 203 proposal (product manager seat, ready; reply-only; a Cursor seat on Probation rung 1, its second real seat)

The seat's reply (Cursor in ask mode, through cursor_reply.py, a fresh context; read-only, it ran nothing), committed verbatim below the facilitator's notes. A first attempt on a Muse seat returned no reply (the runner's cap; the brief reads the emitter and several documents), and a second attempt through decider.py was read as a vote and answered "Yes" (not a draft); the draft below is the third, through cursor_reply.py.

## Facilitator reruns and corrections (2026-11-08 UTC, main at c3c7042)

- Rerun and confirmed: PH338, PH339 and PH341 are used nowhere in src or docs/GRAMMAR.md; `AvowryBuild` is only a file name and the metadata key `AvowryBuildId` (CSharpProvider.cs:42); `AvowAttribute` (:229), `AvowryFailure` (:235), `Avows` (:422) and `AvowSpecs` (:827) are emitted at the lines the draft cites.
- Correction to section 6 (the declaration): Grok, Codex, Copilot and agy are out of quota, so the red seat is not Grok. Declared instead (product owner direction of 2026-11-08): the red is a fresh Claude QA context in its own git worktree; the green is a different fresh Claude context (never the red's seat); the slice is the architect (Claude, its own context); the second read-only review is Cursor or Muse by rotation; the Decider voters are by rotation among Claude, Muse, Cursor and Qwen (Qwen needs the records inlined). The independent gate reviewer and the sign-off are two further fresh Claude contexts (they must run the gate, which a Cursor or Qwen seat cannot).
- The draft's `PH341` stays a recommendation for the architect; card 361 renames it with the rest. The follow-up cards the draft names (derived names, framework names, generated locals, the `.cs.cs` extension, the missing PH329 row) are filed after the card closes, each as its own card.

---

# Card 203 Ready draft (product manager seat, read-only, nothing run)

**Summary:** the card can go Ready with one new code, recommended `PH341`, reported by the C# adapter before any file is written. Reading the emitter corrects the card's reserved list in three ways:
- `AvowryRuntime` and `AvowryBuild` are file names only. No C# symbol has either name, so both become near misses that must keep building.
- The real collisions are three types (`AvowAttribute`, `AvowryFailure`, `Avows`), `AvowSpecs` in the specs namespace, a module named `Program`, three avow method names, and a field named like its record.
- Today, `avowry build` exits 0 on every collision. The C# error only shows up later, when `dotnet build` or `dotnet test` runs on the output.

The predicted C# error codes come from C# naming rules. The red confirms each one by building the emitted project once on the current tree.

## 0. Standing-rules preflight

| Rule | Card line that honours it |
|---|---|
| Card format (`TEAM_WORKING_AGREEMENT.md:120`) | A Connextra story for Dana and 3 Gherkin scenarios. Together they cover Z, O, M, B, I, E and S, and each title carries its letters. |
| TDD red-first, hard rule (`:88`) | The test `Names_reserved_by_generated_csharp_report_a_diagnostic` is committed `Status=Pending` and watched failing before any production change. Characterisation tests are declared up front, with reasons, in section 4. |
| D137, outside-in TDD (`:81-83`) | One Pending test, trait `Criterion=203-1`. The green removes `Pending` in the commit that makes it pass. |
| D157, one criterion per green (`:511`) | Only criterion 1 is new behaviour, so there is one green. |
| D204, characterisation (`:519`) | The near-miss fixtures and the golden test are named characterisation and must pass on the current tree. Every reject id must fail there. |
| D222, two constructs become two ids (`:531`) | Criterion 1 is MULTI (ids R1 to R10) and criterion 2 is MULTI (ids N1 to N6). The red record lists every id. |
| CODING_GUIDELINES | The compiler never throws on user input: we report a diagnostic, not a C# error (`:24`). Dependencies point one way, so the check lives in the C# adapter, not the resolver (`:10`). One code per distinct problem (`:19`). Exact assertions (`:34`). No hidden coupling through strings: the guard and the emitter share one definition of each reserved name (`:41`). Review item 10: generated output is unchanged (`:65`). |
| Card 361 (AV prefix rename) | The new code is written `PHnnn` like the others and renamed with them. |

**Stale text in the card:**
- `Phrase.sln` (`board/203…:30`) becomes `dotnet test Avowry.sln --filter "Status!=Pending"`.
- Acceptance criterion 1 (`:13`) lists `AvowryRuntime` and `AvowryBuild`. They are not C# symbols (fact F2 below).
- The Source line's "for example CS0542" (`:9`) fits only the method and field rows. Type collisions give CS0101.

## 1. Premise facts

**F1. Where the names come from.** `CSharpProvider.Emit` validates first (`CSharpProvider.cs:23-25`) and then builds every file (`:29-50`). When a `CompilationFailedException` is thrown, the CLI writes nothing and exits 1 (`Program.cs:40-50`). On success it writes the C#, writes the source map and exits 0. `avowry build` never compiles the C# itself.

**F2. Every name the generated code reserves, read from the emitter.**

*Fixed names:*

| Name | Kind and namespace | Where |
|---|---|---|
| `AvowAttribute` | class in `{M}`, used as `[Avow(...)]` | `:229`, `:445` |
| `AvowryFailure` | class in `{M}` with `Raise` | `:235-240` |
| `Avows` | static class in `{M}` | `:422` |
| `Fail(string)` | private member of `Avows`, emitted when any avow has `fails with` | `:430-433` |
| `AvowSpecs` | class in `{M}.Specs` | `:826-827` |
| `Program` | top-level program plus `public partial class Program;`, global namespace | `:254-266` |
| `AvowryRuntime`, `Types`, `AvowryBuild` | **file names only**. `AvowryBuild.cs` holds only the metadata key `"AvowryBuildId"` | `:33`, `:34`, `:42` |

*Derived names:*
- Endpoint names produce `{E}Request` (`:535`), `{E}Response` (`:540`), `{E}Handler` with `Handle` (`:547`), `{E}Endpoint` with `Configure` and `HandleAsync` (`:567-592`), and `{E}Specs` in `{M}.Specs` (`:656`, `:719`).
- Effect names produce `I{Effect}` (`:418`, `:325`), `InMemory{Entity}Store` (`:317`) and the nested `{Effect}Double` (`:843`).
- Avow method names are the Pascal-cased template parts joined together, holes included (`:910-911`).
- Record properties are Pascal-cased field names (`:396`, `:927-928`).
- Spec-class members `AssertSpecEqual` and `Format` (`:888`, `:899`) cannot collide, because every generated spec method name contains `__` (`:800-801`, `:913-918`).

*Names the generated code consumes without qualification:* `Attribute`, `Exception`, `List`, `Assert`, `Endpoint`, `Send` and others. `Task` is already escaped (`:791-794`), which is the only escape precedent besides `@` for keywords (`:932-947`).

**F3. What happens today for each collision.** In every row below, `avowry build` exits 0 and the C# error appears only when the output is compiled.

| Id | Source | Expected C# error |
|---|---|---|
| R1 | `type AvowAttribute` | CS0101: duplicate type in `{M}` (with `:229`) |
| R2 | `type AvowryFailure` | CS0101 (with `:235`) |
| R3 | `type Avows` | CS0101 (with `:422`) |
| R4 | `type AvowSpecs`, used in a spec value | Inside `{M}.Specs` the name binds to the specs class (`:827`), so a type error at the use. It builds if no spec uses the type. |
| R5 | `endpoint Avow` | `AvowSpecs` is generated twice in `{M}.Specs` (`:656` and `:827`): CS0101 |
| R6 | `module Program` | The namespace `Program` and the global class `Program` (`:266`) clash: CS0101 |
| R7 | avow `"avows …"` (method `Avows`) | CS0542: a member named like its enclosing type `Avows` |
| R8 | avow `"avowry failure"` in a module with failures or an `ensure` | `AvowryFailure.Raise` (`:433`, `:484`) binds to the method group: CS0119 |
| R9 | avow `"fail {x: Text}"` in a module with failures | A second `Fail(string)` (`:433`): CS0111. `Parser.cs:403-407` reserves only `fail with`. |
| R10 | `type Email` with field `email` | `record Email(string Email)`: CS0542 |

The parser accepts all of these names. Module, type and endpoint names are TypeName tokens (`Parser.cs:227`, `:237`, `:474`) and template words are Name tokens (`:436`).

**F4. Existing diagnostics and the next free code.**
- In use: `PH001`, `002`, `010`, `020`, `100-109`, `200-213`, `300-304`, `310-314`, `320-329`, `330-335`, `337` and `340` (`GRAMMAR.md:166-240`; `Resolver.cs`; `CSharpProvider.cs`).
- `PH329` is emitted (`Resolver.cs:71`) but has no row in `GRAMMAR.md`. That is a separate gap.
- `PH336` is reserved (`docs/decisions/language-and-grammar.md:24`).
- Free: `PH305-309`, `PH338-339`, `PH341`.
- **Recommendation: `PH341`.** It is a C# target diagnostic like `PH340`, and Dana's fix is "rename". The architect assigns the final code. The green adds one row after `GRAMMAR.md:196` and edits the sentence at `:162`. The frozen cheat sheet (D72) is not touched; the card's "at the next re-freeze" refers to that sheet.

**F5. Layering cost.** `TypeModel` (`Model.cs:29`), `FieldModel` (`:27`) and `ModuleModel` (`:13`) carry no line. A check in the adapter therefore needs a `Line` init property on each, as `AvowModel` and `EndpointModel` already have. That adds no emitted bytes.

## 2. Decision

**Reject.** A diagnostic is one guard in the adapter, and the generated C# of every valid source stays byte-identical, which the golden test proves. Escaping would change emitted names, the source map and `trace` output.

## 3. Scenarios

**Story:** As Dana, I want a clear language diagnostic when a name I chose collides with a name the generated C# needs, so that I do not meet an unexplained C# error.

```gherkin
Scenario: [O,S,E] one type named like a generated class is rejected before any C# is written
  Given Dana's module declares `type Avows` on line 3
  When `avowry build m.avowry -o out` runs
  Then it exits 1 with `PH341 (3:1)` and a message naming 'Avows' as a name the generated C# reserves
  And `out` holds no .cs file and no source map

Scenario: [M,I] every reserved-name family reports its own diagnostic through the CLI
  Given one source per reject id R1 to R10
  When each is built with the real CLI
  Then each exits 1 with PH341 at its marker line, naming its reserved name and construct (Type, Endpoint, Module, Avow, Field)
  And none exits 0, crashes, or leaves the error to the C# compiler

Scenario: [Z,B] near misses and valid sources build exactly as before
  Given the Tasks demo, the existing corpus, and near misses N1 to N6
  When each is built and its generated specs run
  Then each exits 0, its specs pass, and the Tasks demo C# matches its golden file byte for byte
```

**How each scenario fails on partial work:**
- Scenario 1 fails if the check runs after files are written, reports the wrong line, or leaves the name out of the message.
- Scenario 2 fails if any one id is missing.
- Scenario 3 fails if the guard matches by prefix or ignoring case, rejects file-name-only names, or changes emission.

**Ids (D222):**
- **Reject, NEW-STATE:** R1 `type-avowattribute`, R2 `type-avowryfailure`, R3 `type-avows`, R4 `type-avowspecs`, R5 `endpoint-avow`, R6 `module-program`, R7 `avow-method-avows`, R8 `avow-method-avowryfailure`, R9 `avow-method-fail-text`, R10 `field-named-like-its-type`.
- **Near miss, CHARACTERISATION:**
  - N1 `type-avow`: attribute lookup ignores types that are not attributes, so `[Avow]` still binds `AvowAttribute`.
  - N2 `type-avowryruntime`: a file name only.
  - N3 `type-avowrybuild`: a file name only.
  - N4 `type-program`: `{M}.Program` is not `global::Program`.
  - N5 `type-AVOWS`: an ordinal-case variant.
  - N6 `avow-fail-int`: `Fail(int)` is a legal overload.

If N1 fails on the current tree, it becomes reject id R11 and the red record says so.

## 4. Named tests and what each shows on the current tree

| Test | Kind | Current tree |
|---|---|---|
| `ReservedGeneratedNames.Names_reserved_by_generated_csharp_report_a_diagnostic` (new; `Slice=203`, `Criterion=203-1`, `Status=Pending` in the red; rows R1 to R10, same shape as `UnsupportedCsharpConstructs.cs`) | NEW-STATE, MULTI | Every row fails with "exit 0; no PHnnn (line:col); .cs files present in out". |
| `EdgeCaseFixtureCorpus.Every_corpus_fixture_matches_its_expected_outcome`, one reject fixture per family: R3, R5, R6, R7, R10 (`outcome: diagnostic`, PH341, line) | NEW-STATE pins | `Assert.NotEqual(0, build.ExitCode)` fails (Actual: 0), per `EdgeCaseFixtureCorpus.cs:35`. These are added with the green, because a red-committed corpus row would break the gate. Their red is the matching row of the Pending test, and the red record quotes a local run on the current tree. |
| The same corpus theory, near-miss fixtures N1 to N6 (`outcome: builds`, **`runSpecs: true`**, at least one spec each) | CHARACTERISATION | Pass. They are committed in the red commit. Without `runSpecs` a "builds" fixture proves nothing, because collisions also "build" today. |
| `TasksDemoGeneratedCsharp.Tasks_demo_generated_csharp_matches_the_golden_file` (existing) | CHARACTERISATION | Passes. It guards review item 10. |
| The existing `builds` corpus fixtures and `The_corpus_has_a_header_comment_and_an_expected_file_per_fixture` | CHARACTERISATION | Pass. New fixtures need a `# name:` header. |

The red record also quotes one `dotnet build` of each emitted reject project, which confirms the C# error codes in F3.

## 5. Size and what is not in the card

**Size:** S. The work is one adapter guard of about 30 lines, a `Line` property on three model records, one `GRAMMAR.md` row, one acceptance test, and 11 fixtures. The counter-metric is six extra `dotnet test` runs for the near misses. The architect may merge them into one fixture if the record gives a reason (one rule: an exact ordinal match).

**Not in this card:**
- Escaping in any form.
- Derived-name collisions: `{E}Request`, `{E}Response`, `{E}Handler`, `{E}Endpoint` and `{E}Specs` against user types; `I{Effect}`; `InMemory{E}Store`.
- Framework names shadowed by a user type in `{M}`, such as `Attribute`, `Exception`, `List`, `Assert` and `Endpoint`.
- Generated locals and parameters (`input`, `result`, `failure`, `response`, `body`, lowercased effect names) against hole and `let` names.
- Other targets.
- The missing `PH329` row.
- The frozen cheat sheet.
- The AV rename (card 361).
- The `.cs.cs` double extension of endpoint files (`:46`, `:49`, `:790`, `:795`), which the golden file pins.

Each of the derived, framework, local and `.cs.cs` items should be filed as its own follow-up card.

## 6. Task-force declaration

- **Product manager (Ready):** Cursor, read-only. It reads code and runs nothing.
- **Architect (slice):** a fresh Claude context. It fixes the code number, the model `Line` change, whether the near-miss fixtures merge, and the shared constant for each reserved name.
- **Red (QA):** Grok in a worktree. It writes the Pending test and the near-miss fixtures, and quotes the `dotnet build` C# errors.
- **Green (developer):** a fresh Claude context, never the red's seat. It writes the guard, the model lines, the reject fixtures and the `GRAMMAR.md` row.
- **Independent gate reviewer:** a fresh Claude context. Grok wrote the reds, so it is not independent. The gate replays the red on the current tree, checks the D222 id list against section 3, and runs the gate in a clone.
- **Second read-only review:** Muse. Its focus is C# name-binding claims and near-miss coverage.
- **Sign-off:** a separate fresh Claude context that runs its own full gate.
- **Cold reader:** none. This is not a docs card (one table row), so D206 does not apply.
- **Decider voters:** Grok, Muse, Cursor, Qwen and Claude, by rotation.
- **Domain expert:** none needed. The red's `dotnet build` evidence settles the C# rules.

**Done when:**

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

CI on main must also be green.
