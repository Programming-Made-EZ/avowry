# Card 422: the facilitator's rulings on the architect's slice (2026-11-15)

The slice is `slice-architect.md` and the corpus is `corpus.txt`: 176 rows in 6 families and 25 encodings. The seat was the architect (claude opus = claude-opus-5-5, ctx-1, Decider D268). Main was read at 7f4f805. The D268 proposal named e5bf308, the head before the kickoff commits; there is no code difference.

Ruling 1 (R-1): accepted. The reviewed licence-URL table lives in the C# test. A package that only gives a licence URL that is not in the table fails until a reviewer adds the row. Failing closed is the point: a new URL needs a human review before the page can claim its licence.

Ruling 2 (R-2): accepted as the reference has it. A tool package under a false Condition is still reported. The tool must not gain a package declaration in any form, whether or not this build evaluates it.

Ruling 3 (R-3): a named limit of this card, with no rows. A nearer `Directory.Build.props`, a `.targets` file or the project file itself can still override the two lock properties after the root props set them. It is filed as a follow-up card.

Ruling 4 (R-4): the Python test skips the commit-blob check with its reason on a shallow checkout. The C# wrapper writes the Python output to the test log, so a passing run shows the skip.

Ruling 5 (R-5): out of scope. `MOB.md` in the guard's allow-list with no History entry is filed with ruling 3's follow-up. This card removes only H6 and H7.

Ruling 6 (R-6): accepted. The page is checked as it is written. Rewording a figure sentence fails until the check's pattern changes in the same commit, and the failure names the sentence.

The 8 deliberate outcome changes in the slice (I-comment, I-cdata-text, O-warm-warning, O-warm-nopkg, O-partial, O-planted-offline, O-planted-nu10041, G-other-lock) are accepted with the reasons the slice gives. The red embeds every corpus row. The networked parts of [E] and [O] run only under the heavy-run lock at the gate and the sign-off; their offline parts are red and green on fixtures.
