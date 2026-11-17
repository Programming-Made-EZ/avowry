SIGN-OFF (two MINOR findings, neither blocks the card)

# Card 166 QA review, round 2 (claude opus, claude-opus-5-5, context ctx-166q; R166-3)

Range read: `git diff 9d7e9e2..HEAD` in ~/projects/wt-166 (red round 2 a71d189, green round 2 ee5416d). The range has 5 files: CSharpProvider.cs, Program.cs, LinkInTheMap166.cs, LinkInTheMap166Comments.cs and the Tasks golden. I read R166-3 at the end of slice.md. I did not run dotnet, a build or a test.

## Each fix against the ruling

- **M1, as ruled.**
  - The fix: CSharpProvider.cs:837 (`// When handled` in effectful specs) is removed.
  - The golden diff is exactly 11 deleted `        // When handled` lines and nothing else. No `// When` is left in the golden or in src/.
  - The orders hash is unchanged, as expected: orders has only an effect-free spec, which lost the line in round 1.
  - The card 165 BornGreen hash is unaffected, because it sets comment lines aside.
  - The new test, LinkInTheMap166Comments.cs:129-151 (`No_generated_comment_restates_a_step_of_a_spec_including_When_handled`), reads trimmed comment lines (CommentLines, :258-261). On 9d7e9e2 its tasks case fails, for the right reason: the 11 lines. The orders case passes on 9d7e9e2 and is a guard. That matches the "3 failed" count.
- **M2, as ruled.**
  - The fix: the two discards are gone from Program.cs:46-47.
  - The test (LinkInTheMap166.cs:399-420) now requires that Program.cs does not mention `EndpointTemplates` or `AvowTemplates` at all. On 9d7e9e2 it fails at that check, for the right reason: the discards.
  - It requires SourceMapWriter.cs to read `.EndpointTemplates` and `.AvowTemplates`, which it does at SourceMapWriter.cs:44 and :60.
  - It runs the template-knowledge scan on the writer too, which closes the gap I named in round 1. The writer's `"endpoint"` kind literal has no trailing space, so the scanner's own planted negative already accepts it.
  - The two `Assert.Contains` checks on the map names pin the property names, so a rename cannot make the loop empty.
- **M4, as ruled.**
  - The fix: Program.cs:233 now uses `!string.IsNullOrEmpty(record.Template)`. Print is the only reader of `Template` in the CLI.
  - The test (LinkInTheMap166.cs:351-370) plants `"template": null` and asserts the JSON really holds it. It requires exit 0, the `build:` line, no template line and no exception text.
  - On 9d7e9e2 it fails for the right reason. System.Text.Json fills the non-nullable string parameter with null, `.Length` throws, and the catch-all returns exit 2 with "Could not trace: Object reference...".
- **grok 5, as ruled.** The comment at CSharpProvider.cs:608 now reads "One statement per step".
- **Follow-ups (M3, M5, the stale ADR citations) and M6 in part 2:** recorded in R166-3 as named limits. Nothing in this range touches them.

Nothing new was introduced. The product changes are exactly these three lines plus one comment. The test helper `Plant` gains an optional `jsonNull` flag (LinkInTheMap166.cs:549, 568-571), and the existing callers are unchanged.

## Findings

- **R2-M1 MINOR.** The file header of LinkInTheMap166Comments.cs:18 still says "`// When handled` is not pinned either way", which is no longer true. Fix it to name R166-3 the next time the file is touched.
- **R2-M2 MINOR.** R166-3 does not mention the lighter half of my M1: the endpoint-spec header "Emitted by the Avowry C# provider from the spec blocks." still names the source construct, whereas verdict 2 said "likewise". I take R166-3's silence as keeping that header, which is acceptable as plain text. Say so in the record, or add it to part 2, so it is not lost.

## Could not check
- I ran no test (no dotnet). I relied on the facilitator's proofs: red round 2 with 3 predicted failures, and green proof 5 with compiler 344 of 344 and acceptance 1614 of 1614.
- Golden re-recorded from a run: I checked only that its diff is the 11 deleted lines.
- The red's three failures on 9d7e9e2 are read from the code, not observed.
