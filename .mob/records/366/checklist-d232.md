# D158 pre-vote checklist: Decider D232, the card 366 retro pick (facilitator)
- Boundary: card 366 signed off (SIGNED OFF WITH CONDITIONS, none blocking); the seven retro voices complete. Inbox read at this boundary before the vote.
- Proposal: .mob/records/366/retro-pick-proposal.md (patch agreement-amendment.diff; voices retro-voices.json; retro input retro-input.md), committed before the vote. One concern: a No that restates a rule in force costs one question to the objector.
- Category: a change to TEAM_WORKING_AGREEMENT.md (a tuning of Decider protocol step 6). Full Decider run; step 6 (D224) in its current text: the confirmation step for any No with an amendment; trigger (c) if the primary window passes with no No and no amendment.
- PREFLIGHT (D228): committed template .mob/records/templates/vote-prompt.md; no other run active; no stray prompt-*.txt; fallback chains Claude, Cursor, Qwen, Muse only; seats one at a time (bugs 365 and 374 open).
- NO IDLE (D230): every wait has a failure-aware monitor; the next step is launched at each boundary.
- Seat spread: Qwen, Cursor, Muse, Claude by rotation (Grok, Codex, Copilot, agy out of quota).
- No commit and no tracked-file change on main during a vote window.
