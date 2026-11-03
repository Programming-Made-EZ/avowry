# Card 276 retro inputs

## Grok single-turn runs stop after reading, before writing (Dispatch note, 2026-11-03)

- Seen twice on card 276. The slice seat (`grok -p ... --permission-mode plan`, about 01:35 UTC) exited 0 after reading, with only narration on stdout. So did the scope seat (04:31 UTC). Each time the facilitator continued the same session (`grok -p "...write it now..." -c`), which worked. The scope continuation added "Do not run more tools".
- The question, per coach-don't-bench, is about the prompt or the invocation. Does the brief need an explicit "write your answer in this same turn, after reading", or does the CLI need a flag? Candidates from `grok --help`: --output-format json or streaming-json, to see whether the run ends on a turn or a tool limit; and whether plan mode ends the turn when it would present a plan.
- Tracking: the next Grok seat's brief carries the explicit same-turn instruction. Record whether the first run writes the answer (fix works) or needs a continuation (fix does not). If it still stops, try one flag at a time and record each result.
- Related: both Grok outputs also had wrong Size lines (10911 for 9866, 5672 for 5664), each corrected by the seat on a re-ask. The facilitator measures every Size line.
- Tracking, trial 1 (scope-2 seat, 06:14 UTC): the brief carried "IMPORTANT: write your complete answer in this same turn ... Do not stop after reading." The run still stopped after reading (316 bytes of narration ending "I'll count the proposal bytes ... then reply"). Result: the prompt-only fix does not work. Next trial: a CLI flag, one at a time; first --output-format json, to see what ends the turn. The Size line was 8 bytes high for the third time (3872 for 3864), corrected on a re-ask.
