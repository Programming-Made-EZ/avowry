# Retro pick proposal for Decider D270: a green brief names the caller of every new helper, and the first gate runs in the CI checkout shape

This is the card 422 retro, 2026-11-16 UTC. The proposer is the facilitator, from the seven retro voices (.mob/records/422/retro-voices.json; the input .mob/records/422/retro-input.md). Seven of seven voices picked caller naming. Four (architect, qa, audit lens, product manager) also asked for the CI-shape gate from round 0. The proposal adds a new Experimental entry to TEAM_WORKING_AGREEMENT.md (patch .mob/records/422/agreement-amendment-d270.diff). It covers one concern: the seams between seats that review found on card 422.

STANDING RULES: the Decider protocol steps 2 to 4 are unchanged. Voters are the task force's roles, each in a fresh context, with the model pinned before launch. This changes the agreement, so the confirmation step (D224 trigger c) runs if the primary window passes with no No and no amendment. The account-name ruling holds.

EVIDENCE:
- Review round 1 found the [E] and [O] helpers ported but never called on the networked path. The facilitator's green brief had named no caller, and the red's rows tested the helpers alone. That cost one fix round.
- Review round 2 found a new test row that failed on a shallow checkout, which is the shape CI's test job uses. The laptop gates ran in full clones and could not see it.
- Three of the card's three BLOCKING findings were of these two kinds. None was a gap of the D263 corpus.

THE ENTRY: A green brief names the caller of every new helper, and the first gate runs in the CI checkout shape (experiment; card 422 retro, Decider D270). While this entry is on trial:
- every slice and green brief that introduces a helper names each production caller by symbol;
- the red commits a source-scan wiring row for each named call before the green;
- the first gate of the card runs every test that reads git history (the Python modules first) in a depth-1 clone, the shape CI checks out, as well as in the full clone.

Measures: round-1 review BLOCKING findings that are wiring or clone-shape gaps (card 422: 3 over two rounds), target 0. Counter-metric: minutes added to the slice or brief (more than 10 on a card is a miss). Sample: three cards. Expiry: the third card's retro.

NOT IN THIS PICK:
- D263 and D265's adoption, which is Decider D271;
- D267, which continues its trial;
- the qwen driver finding (no answer written; a repair item);
- the surviving deep-path mutants of card 369's L5 (#444).
