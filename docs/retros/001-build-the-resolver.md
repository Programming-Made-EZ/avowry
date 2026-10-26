# Retro: card 001, build the resolver (2026-10-25)

## Flow metrics (from git timestamps)

- Card filed to first increment: 2026-10-20 13:03 to 2026-10-25 18:13 (about 5 days waiting for the toolchain: only .NET 10 was installed; the .NET 8 SDK was installed user-scope on 10-25).
- Mob time on the card: 18:13 to 19:13, five increments (9f313b2, 59d8a9c, 84b5737, a47ed6a, 45813e1), about 12 minutes per increment, CI green on each.
- Minutes lost to rework: about 20. The card was marked done after a weak Done-when passed while 5 of 7 review findings were open; it was reopened, then split into card 002.

## Individual

- Codex: cycle 1 clean; cycles 2-3 had defects Grok found (throws, missed PH312, option coercion), so no ladder promotion (1/3). Its PM turn also claimed "no pending tests remaining" while two classes were still Pending (a misreport; the facilitator caught it by re-running).
- Copilot: authored the specs and fixture cycle (a47ed6a); independent review of it still owed. Reviewed cycle 1 well.
- Grok: review was excellent (red check on a scratch clone, six concrete defects); authored 45813e1 (2 of 7 findings).
- agy: in progress on card 002.
- Facilitator: wrote a Done-when that passed trivially (`PH335` already in GRAMMAR.md). Adjustment: every Done-when check must be red before the work starts.

## Team (mob)

- Rotation worked: Codex, Codex, Copilot, Grok. QA by Codex returned "DEFECTS" twice for environment reasons (missing .NET 8 runtime in its shell), so QA sign-off was weak evidence. Adjustment: the facilitator exports DOTNET_ROOT and PATH for every turn.

## Organization

- The shared-machine limits were lifted when the play-tracker mob paused; driver allotments untouched so far.
- Claude CLI is not signed in (last resort only).
- mob-crosscli is not a git repository, so its template and config changes are not versioned.

## Product (model feedback, the LLMs-are-customers check)

Codex, Copilot, Grok and agy each read GRAMMAR.md and orders.phrase and wrote a small phrase (Claude skipped: not signed in). Results:

| Finding | Models | Outcome |
|---|---|---|
| EBNF `argument` omits the bare-word form that Section 4 allows; `and` line may be an outcome but a spec may have one outcome | Codex, agy, Grok | **Card 013**, rank 2 |
| `native` block environment undocumented: `Fail("X")` (fixture) vs `PhraseFailure` (line 207); no passing-guard example | Grok, agy, Codex, Copilot | **Card 014**, rank 4 |
| `when called` / `when handled` carries no information; drop it | agy | **Card 015**, rank 10, Decider vote first |
| Comma-separated `given` bindings | agy | **Rejected**: a second way to bind violates the small-fixed-core and deterministic-resolution rules, and saves few tokens next to `and` lines. Revisit only if measured token waste says otherwise |
| Failure codes as typed identifiers, not strings (`Fail(Failure.EmptyOrder)`) | agy | **Deferred into 014/004**: the emitter can generate constants with no grammar change |
| Add `average quantity of {items}` style value-phrase example | Copilot | **Folded into 014** |

Decider runs this retro: none yet (no two models disagreed). Card 015 will open with one.

## One process improvement

Done-when checks must be shown red before work starts, and must not be satisfiable by existing text. Area: Team.
