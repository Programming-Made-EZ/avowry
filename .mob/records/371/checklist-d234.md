# D158 pre-vote checklist: Decider D234, the card 371 retro pick (facilitator)
- Boundary: card 371 signed off (SIGNED OFF WITH CONDITIONS, none blocking); the seven retro voices complete. Inbox read at this boundary before the vote.
- Proposal: .mob/records/371/retro-pick-proposal.md (patch agreement-amendment.diff; block .mob/records/templates/declaration-evidence.md; voices retro-voices.json; retro input retro-input.md), committed before the vote. One concern: the standing evidence block in every declaration.
- Category: a change to TEAM_WORKING_AGREEMENT.md (a new Experimental entry). Full Decider run; step 6 (D224) with the D232 paragraph: a No whose condition a rule in force meets is answered by quoting the line; trigger (c) if the primary window passes with no No and no amendment.
- PREFLIGHT (D228): committed template .mob/records/templates/vote-prompt.md; no other run active; no stray prompt-*.txt; fallback chains Claude, Cursor, Qwen, Muse only; seats one at a time (bugs 365 and 374 open).
- NO IDLE (D230): every wait has a failure-aware monitor; the next step is launched at each boundary.
- Seat spread: Qwen, Cursor, Muse, Claude by rotation (Grok, Codex, Copilot, agy out of quota).
- No commit and no tracked-file change on main during a vote window.
