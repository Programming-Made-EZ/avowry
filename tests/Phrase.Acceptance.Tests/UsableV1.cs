using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Milestone: Usable v1 (board/MILESTONE-usable-v1.md)
//   As Steve
//   I want one automated test that says whether Phrase can build an application
//   So that "usable" is detected, not guessed
//
// Pending until card 012 lands the full flow; CI runs it in a non-blocking `usable-v1` job so the first
// bar that fails is always visible. When it first passes on main, the mob reports USABLE v1 REACHED.
public class UsableV1
{
    [Fact]
    [Trait("Status", "Pending")]
    [Trait("Milestone", "UsableV1")]
    public void A_real_app_written_in_phrase_is_usable_end_to_end()
    {
        // Given a real app written entirely in .phrase (bar 1)
        var app = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        Assert.True(File.Exists(app), "Usable v1 bar 1 unmet: demo/tasks/Tasks.phrase does not exist (card 009).");

        // When one CLI command compiles it (bar 2), Then dotnet build has zero warnings (bar 3),
        // And the generated xUnit specs all pass (bar 4), And live HTTP behaves as the specs say (bar 5),
        // And an LLM customer extended it from the docs alone, fresh against this GRAMMAR.md (bar 6).
        // These steps are added by cards 007, 010, 011 and 012; until then the test stops at the first unmet bar.
        Assert.Fail("Usable v1 bars 2-6 are not wired yet (cards 007, 010, 011, 012).");
    }
}
