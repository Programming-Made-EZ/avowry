# Retro input: card 371 (the pre-publication page shows the owner's answers and the trademark search brief), facts from git and run logs (2026-11-09 local times)

Names, addresses, home paths and secret values are not printed here. Read the records named.

## What shipped
docs/release/pre-publication-checks.md now shows the nine owner items as answered 2026-11-08 (the product owner's answers, recorded on the page), the search item reading "commissioned by the owner; pending result", the lawyer questions "skipped by owner decision", the stale lines corrected, a "What blocks what" paragraph and a `### Trademark search brief` (the mark, classes 9 and 42 with a goods and services description, jurisdictions proposed by the mob, the registry results of 2026-10-29 and what was not checked). Card 186's 6-OPEN clause was moved out in its own commit; a new module of tests (32 ids) with three Pending wrappers and a characterisation wrapper guards the answered page. Green 479f2db, page revision 1 f61e99c; gate review SHIP; sign-off SIGNED OFF WITH CONDITIONS (HANDOFF.md corrected in the close-out; the generated board files regenerate after close).

## Flow metrics and process against build time (local times)
- Ready draft: Cursor through cursor_reply.py with the facts supplied, about 10 minutes (committed 04:45). Decider D233: primary window 04:46 to 05:00 (four Yes, one audit-lens No, the developer seat refused for Claude by the rotation rule), amendment 05:15, confirmation window 05:15 to 05:30 (six Yes), recorded 05:30: about 45 minutes from the proposal to the record, about 30 of them the No and its confirmation.
- Build: slice 05:36, loosening 05:38, red 05:55, green 06:13: about 43 minutes from the record to the green. Cold reader and second review 06:15 to 06:19 (UNCLEAR and REVISE), page revision 06:21, baseline gate and push 06:26 to 06:36, independent gate review 06:55, sign-off 07:10.
- Process against build before the gates: about 1.0 to 1 (45 against 43; the declaration with its Ready draft about 55). After the green the two page reviews and the revision took about 8 minutes of reading plus the revision, then the gates about 50 minutes.
- Red-first held: characterisation declared first; the stub-first runs watched the planted rows fail by assertion; 32 ids failed on the pre-change page (rerun by the facilitator; replayed by the gate reviewer); the green changed the wrapper by the Pending traits and the page only. No born-green finding.

## Seat mix
Card seats beyond votes: Ready draft Cursor; cold reader Qwen (reply-only, inlined); second read-only review Cursor; slice, red (own worktree), green (a different context, which also made the page revision), gate reviewer and sign-off all Claude. D233 votes: Muse 2, Cursor 3, Qwen 2 (the No and its confirmation), Claude 2. Reasons: Grok, Codex, Copilot and agy out of quota; red and green are separate contexts and worktrees; the gate review and sign-off must run the gate.

## Experimental entries this card exercised
D228 preflight (fourth window): no harness-lost window. D230 no idle between steps (second card): every wait had a failure-aware monitor. D219 as tuned by D226 (second docs card): the cold reader and the second review both finished and were committed before the gate review started; the page revision was committed after both reports; the gate review opened with the ancestor line against the revision commit; gate reviews started on a pre-revision tip: 0. D232 (a No whose condition a rule in force meets): considered for D233's audit-lens No and not applied (no line in force proves isolation): 0 of 1 Deciders with a No used it; the confirmation window cost about 15 minutes again. D222: used; no dropped half. D206: the cold reader returned UNCLEAR for a real reason and the page was revised.

## Findings
- The two page reviews found different things: the cold reader the unclear release gate (publishing the name versus the package metadata), the second review (Cursor, by reading) six wrong or unsupported cited lines, an unsupported clause about the earlier brand and shorthand answers. Neither alone would have found both.
- The page revision was made by the green's context (not a separate seat); the cold reader was not re-run on the revised page.
- The audit lens (Qwen) voted No on the D233 declaration (a context-freshness hash): a fourth No in five Deciders; its condition was adopted as a statement of the git evidence the agreement already requires.
- The pre-commit personal-name check prints a list of generated board lines naming the product owner by first name on every commit (bug 375 filed).
- A load flake did not occur in the five full-gate runs of this card.
- Idle minutes on this card: about 0; Dispatch nudges 0.

## Carried
The look-back at D208, D209, D210; D222 and the D228 preflight for adoption; the D232 trial (0 of 1); card 361 test-limit cards; card 203 follow-ups; bugs 365, 372, 373, 374, 375.

## Questions for the voices
The adjustment review of D228 (fourth window), D230 (second card), D219 as tuned by D226 (second docs card), D232 (0 of 1 applied), D222, D206; process against build; the audit lens's Nos (4 of the last 5 Deciders); idle minutes; drivers left; one process improvement candidate.
