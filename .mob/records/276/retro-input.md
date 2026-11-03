# Card 276 retro input (facilitator; facts only)

Card: Muse Code joins the mob as a trial driver (D179; voting seats first). Code in mob-defaults, 25 commits 5ef4b8f..839380b; signed off with conditions (.mob/records/276/signoff-ctx6.md).

## Flow metrics (UTC, from commits)
- Ruling D179 00:24, task-force proposal 00:29, first red 02:18, sign-off 18:22 on 2026-11-03: about 18 hours. That includes about 7 hours blocked (08:50 to 16:05) on the product owner's ruling D180 and the elevated Muse sandbox setup by Dispatch.
- Caps: slice 1+1+1; criterion 3 raised by four scope windows to 2, 3, 4 and 6 (final 1+1+6 = 8). Cycles 5 and 6 ran past cap 4 without a window and were ratified after the fact (audit 195; window 276-scope-4).
- Windows: 276-taskforce (two rounds, first run withdrawn), 276-slice, 276-scope, 276-scope-2, 276-scope-3 (two rounds), 276-scope-4. Every Claude seat in each window first paid an empty Grok and an empty Copilot attempt (the cmd.exe line limit; cards 229, 270, 277).
- Gates: first gate HOLD (Done check 3 partly met; the gate seat had no --add-dir); re-gate SHIP with its own scan.
- Full suites (PowerShell worktree): 1581, 1592, 1601 with 1 failure (regression, fixed by e3608ae), 1613, 1623, 1626, 1633, 1643; all OK otherwise.

## Adjustments to review first (D155)
1. D175 written cycle caps and scope windows: four raises on one criterion, then two cycles past the cap under lettered names.
2. D176 gate environment lines: the first gate could not run its subject tests (no --add-dir); the re-gate ran them and its own scan.
3. D178 decision-seat breaker (card 211 retro pick, experiment for the next 3 decision windows): no detector in decider.py, so it never fired; reshaped by the product owner's coach-don't-bench principle into card 277.
4. Green commit rule and the refactor line per green: greens 7c04e2e and 0000715 had no full suite of their own; refactors after each green except where noted.

## Observations
- Muse: installed and signed in by Dispatch; the probe, the reader and one cited trial vote landed; the vote log scans clean only after the reader learned Muse's real tool records (three red-green pairs after the first real capture). Muse's sandboxed shell still does not complete a command (card 285). A model id was never validated (`--model default` refused).
- Fixtures: a trimmed red fixture let a green pass while real logs failed; whole real logs then exceeded the Windows path limit in deep checkouts (fixed flat, guard test added).
- Seats: a resumed Copilot QA context compacted at 128k and lost its brief (fresh ctx-7 stood in); --model auto picked mai-code-1.1-flash; Grok plan-mode early stops were traced to a cancelled byte-counting tool call and fixed in the brief.
- A delete-guard false positive on a read-only command (card 283).
- Cards filed on this card: 277 (reshaped), 279 to 283, 285 to 287. Card 279 is delivered by this card.

## Queued inputs to weigh
- Proposal issues 278 (the cmd.exe line-limit fix ranked next) and 284 (drift resistance as a benefit, with a measure) go to re-rank 20, not this retro; .mob/records/notes/2026-11-03-empty-decision-votes-root-cause.md.

## Standing section: the feedback-loop lens (D165)
Each voice names, for the 7 loops, who or what gives the feedback, its latency, what the team does with it, and MISSING where none exists.

## Retro notes recorded during the card
## Grok single-turn runs stop after reading, before writing (Dispatch note, 2026-11-03)

- Seen twice on card 276. The slice seat (`grok -p ... --permission-mode plan`, about 01:35 UTC) exited 0 after reading, with only narration on stdout. So did the scope seat (04:31 UTC). Each time the facilitator continued the same session (`grok -p "...write it now..." -c`), which worked. The scope continuation added "Do not run more tools".
- The question, per coach-don't-bench, is about the prompt or the invocation. Does the brief need an explicit "write your answer in this same turn, after reading", or does the CLI need a flag? Candidates from `grok --help`: --output-format json or streaming-json, to see whether the run ends on a turn or a tool limit; and whether plan mode ends the turn when it would present a plan.
- Tracking: the next Grok seat's brief carries the explicit same-turn instruction. Record whether the first run writes the answer (fix works) or needs a continuation (fix does not). If it still stops, try one flag at a time and record each result.
- Related: both Grok outputs also had wrong Size lines (10911 for 9866, 5672 for 5664), each corrected by the seat on a re-ask. The facilitator measures every Size line.
- Tracking, trial 1 (scope-2 seat, 06:14 UTC): the brief carried "IMPORTANT: write your complete answer in this same turn ... Do not stop after reading." The run still stopped after reading (316 bytes of narration ending "I'll count the proposal bytes ... then reply"). Result: the prompt-only fix does not work. Next trial: a CLI flag, one at a time; first --output-format json, to see what ends the turn. The Size line was 8 bytes high for the third time (3872 for 3864), corrected on a re-ask.
- Tracking, trial 2 (scope-3 seat, 07:19 UTC, `--output-format json`): the JSON showed stopReason "cancelled" after 9 turns. Grok's thinking showed it calling a terminal command to count bytes for the Size line; in plan mode a shell call needs approval, and with no interactive user the turn is cancelled. Root cause: the brief asked for a measured Size line, and in plan mode that measuring is cancelled. It is not a model defect.
- Trial 3 (the continuation): the brief said "do NOT run any command or tool to count bytes ... the facilitator will measure and correct it". Result: stopReason end_turn, 1 turn, full proposal. The fix works. Its Size line was an estimate (3920 for 5480); the facilitator corrected it as the brief promised.
- Proposal for the retro: Grok plan-mode briefs say "do not run tools to measure; the facilitator measures the Size line", and decision and seat runs use --output-format json so the stop reason is recorded. The earlier wrong Size lines (8 bytes high) were estimates from turns that could not measure.

- Red 4b attempt 1 (2026-11-03 16:41 UTC): the resumed QA context (Copilot, --model auto picked mai-code-1.1-flash) compacted at its 128k threshold during the run, dropped the brief and committed nothing. Question for the retro: cap resumed-seat context size, or seat fresh contexts by default for reds; pin a model instead of auto.

- Lettered sub-cycles (audit 195, 2026-11-03): red 4b and 4c followed greens that had passed, so they were new cycles (5 and 6) past the cap, run without a scope window. Proposal for the retro: a lettered red is allowed only before any green has passed the red it corrects; once a green passes, the next red is a new cycle and counts against the cap.
